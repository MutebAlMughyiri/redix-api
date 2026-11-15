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
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

public class MigratedApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Admin-test-password";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

    private static int _instances;

    /// <summary>This factory's own database: each collection of tests has one, so the collections run at once without seeing each other's rows.</summary>
    private readonly string _database = $"RedixTests{Interlocked.Increment(ref _instances)}";

    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    /// <summary>A VAT number no other test holds, because a duplicate invoice is looked for across every project (FR-17).</summary>
    public static string NewVatNumber() => $"3{Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999)}3";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Redix"] = ConnectionTo(_database),
            ["Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ["Admin:Username"] = AdminUsername,
            ["Admin:Password"] = AdminPassword,
            ["Registration:PermitsPerHour"] = "100000",
            ["Attachments:Folder"] = Path.Combine(Path.GetTempPath(), "RedixTestsAttachments"),
        }));
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Time));
    }

    private static string ConnectionTo(string database) =>
        new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__Redix") ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True") { InitialCatalog = database }.ConnectionString;

    /// <summary>The same API on an empty database of its own, for a test that moves through the migrations. It stands until <paramref name="work"/> ends.</summary>
    public async Task OnScratchDatabaseAsync(string name, Func<RedixDbContext, Task> work)
    {
        await using var scratch = WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Redix"] = ConnectionTo(name) })));
        using var scope = scratch.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        await db.Database.EnsureDeletedAsync();
        try
        {
            await work(db);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<RedixDbContext>().Database;
        await database.EnsureDeletedAsync();
        await database.MigrateAsync();
    }

    public async Task<User> AddUserAsync(string password, bool mustChangePassword = false, bool isAdmin = false)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var user = new User
        {
            Username = $"user-{Guid.NewGuid():N}", MustChangePassword = mustChangePassword, CreatedAt = Time.GetUtcNow().UtcDateTime,
        };
        user.SetPassword(password, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>());
        db.Users.Add(user);
        if (isAdmin)
        {
            db.SystemPermissions.Add(new SystemPermission { UserId = user.Id, Permission = Permission.ApproveProjects });
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

    /// <summary>Makes <paramref name="userId"/> inactive in <paramref name="projectId"/> straight in the database (redix-api #126), for a test whose subject is something else.</summary>
    public Task MakeInactiveAsync(Guid userId, Guid projectId) =>
        WithDatabaseAsync(async db =>
        {
            db.InactiveMembers.Add(new InactiveMember { UserId = userId, ProjectId = projectId });
            await db.SaveChangesAsync();
        });

    public async Task<List<HistoryRecord>> HistoryOfAsync(Guid recordId)
    {
        using var scope = Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<RedixDbContext>().History.Where(record => record.RecordId == recordId).OrderBy(record => record.Id).ToListAsync();
    }

    /// <summary>Gives <paramref name="permissions"/> in a project straight in the database, those not held already, for a test whose subject is something else.</summary>
    public Task GiveAsync(Guid userId, Guid projectId, IEnumerable<Permission> permissions) =>
        WithDatabaseAsync(async db =>
        {
            var held = await db.ProjectPermissions.Where(row => row.UserId == userId && row.ProjectId == projectId).Select(row => row.Permission).ToListAsync();
            db.ProjectPermissions.AddRange(permissions.Except(held).Select(permission => new ProjectPermission { UserId = userId, ProjectId = projectId, Permission = permission }));
            await db.SaveChangesAsync();
        });

    /// <summary>Registers a project for <paramref name="client"/> and approves it straight in the database, for a test whose subject is something else.</summary>
    public async Task<ProjectView> ApprovedProjectAsync(HttpClient client, string name, params List<string> units)
    {
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(name, new DateOnly(2026, 11, 1), units));
        var project = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;
        await WithDatabaseAsync(async db =>
        {
            var managerId = await db.ProjectPermissions.Where(held => held.ProjectId == project.Id).Select(held => held.UserId).FirstAsync();
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

    /// <summary>Removes the rows <paramref name="which"/> picks through a save, as REDIX itself takes a permission, so the person's kept access is forgotten (redix-api #120).</summary>
    public Task TakeAsync<TRow>(Func<RedixDbContext, IQueryable<TRow>> which) where TRow : class => WithDatabaseAsync(async db =>
    {
        db.RemoveRange(await which(db).ToListAsync());
        await db.SaveChangesAsync();
    });

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// The test classes run in four collections at once, each with a factory and a database of its own; the tests of one collection run one after another, as they always did.
/// A class goes in the collection with the least time in it.
/// </summary>
[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database";
}

/// <summary>The second collection, on its own database.</summary>
[CollectionDefinition(Name)]
public class SecondDatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database 2";
}

/// <summary>The third collection, on its own database.</summary>
[CollectionDefinition(Name)]
public class ThirdDatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database 3";
}

/// <summary>The fourth collection, on its own database.</summary>
[CollectionDefinition(Name)]
public class FourthDatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database 4";
}
