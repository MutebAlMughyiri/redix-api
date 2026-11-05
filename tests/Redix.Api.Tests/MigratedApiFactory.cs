using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

public class MigratedApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Admin-test-password";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var testDatabase = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__Redix") ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True")
        {
            InitialCatalog = "RedixTests",
        };
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Redix"] = testDatabase.ConnectionString,
            ["Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ["Admin:Username"] = AdminUsername,
            ["Admin:Password"] = AdminPassword,
            ["Registration:PermitsPerHour"] = "100000",
            ["Attachments:Folder"] = Path.Combine(Path.GetTempPath(), "RedixTestsAttachments"),
        }));
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Time));
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<RedixDbContext>().Database;
        await database.EnsureDeletedAsync();
        await database.MigrateAsync();
    }

    public async Task<User> AddUserAsync(string password, bool isActive = true, bool mustChangePassword = false, bool isAdmin = false)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var user = new User
        {
            Username = $"user-{Guid.NewGuid():N}", IsActive = isActive, MustChangePassword = mustChangePassword, CreatedAt = Time.GetUtcNow().UtcDateTime,
        };
        user.SetPassword(password, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>());
        db.Users.Add(user);
        if (isAdmin)
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = RoleSeed.AdminId });
        }
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<HttpClient> ClientSignedInAsAsync(User user, string password)
    {
        var client = CreateClient();
        var signIn = await client.PostAsJsonAsync("/auth/sign-in", new SignInRequest(user.Username, password));
        var session = await signIn.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        return client;
    }

    public async Task DisableAsync(User user)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        (await db.Users.SingleAsync(stored => stored.Id == user.Id)).IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditRecord>> AuditTrailOfAsync(Guid recordId)
    {
        using var scope = Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<RedixDbContext>().AuditTrail.Where(record => record.RecordId == recordId).OrderBy(record => record.Id).ToListAsync();
    }

    public async Task<Guid> ProjectMemberIdAsync(Guid userId, Guid projectId, Guid roleId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        return await db.ProjectMembers.Where(member => member.UserId == userId && member.ProjectId == projectId && member.RoleId == roleId).Select(member => member.Id).SingleAsync();
    }

    /// <summary>Gives a role straight in the database, for a test whose subject is something else.</summary>
    public Task GiveRoleAsync(Guid userId, Guid projectId, string wordKey) =>
        WithDatabaseAsync(db =>
        {
            db.ProjectMembers.Add(new ProjectMember { UserId = userId, ProjectId = projectId, RoleId = RoleSeed.IdOf(wordKey) });
            return db.SaveChangesAsync();
        });

    /// <summary>Registers a project for <paramref name="client"/> and approves it straight in the database, for a test whose subject is something else.</summary>
    public async Task<ProjectView> ApprovedProjectAsync(HttpClient client, string name, params List<string> units)
    {
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(name, new DateOnly(2026, 11, 1), units));
        var project = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;
        await WithDatabaseAsync(async db =>
        {
            var managerId = await db.ProjectMembers.Where(member => member.ProjectId == project.Id).Select(member => member.UserId).FirstAsync();
            await ProjectStructure.ApproveAsync(await db.Projects.SingleAsync(stored => stored.Id == project.Id), managerId, db);
            await db.SaveChangesAsync();
        });
        return (await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single(listed => listed.Id == project.Id);
    }

    /// <summary>Works on the database directly, as someone configuring roles does (BR-32).</summary>
    public async Task WithDatabaseAsync(Func<RedixDbContext, Task> work)
    {
        using var scope = Services.CreateScope();
        await work(scope.ServiceProvider.GetRequiredService<RedixDbContext>());
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database";
}
