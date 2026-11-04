using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
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

namespace Redix.Api.Tests;

public class MigratedApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string OwnerUsername = "owner";
    public const string OwnerPassword = "Owner-test-password";

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
            ["Owner:Username"] = OwnerUsername,
            ["Owner:Password"] = OwnerPassword,
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

    public async Task<User> AddUserAsync(string password, bool isActive = true, bool mustChangePassword = false, bool isOwner = false)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var user = new User
        {
            Username = $"user-{Guid.NewGuid():N}", IsActive = isActive, MustChangePassword = mustChangePassword, IsOwner = isOwner, CreatedAt = Time.GetUtcNow().UtcDateTime,
        };
        user.SetPassword(password, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>());
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<HttpClient> ClientSignedInAsAsync(User user, string password)
    {
        var client = CreateClient();
        var signIn = await client.PostAsJsonAsync("/auth/sign-in", new SignInRequest(user.Username, password));
        var session = await signIn.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>();
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

    public async Task<Guid> ProjectMemberIdAsync(Guid userId, Guid projectId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        return await db.ProjectMembers.Where(member => member.UserId == userId && member.ProjectId == projectId).Select(member => member.Id).SingleAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database";
}
