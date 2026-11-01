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

    public async Task<User> AddUserAsync(string password, bool isActive = true)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var user = new User { Username = $"user-{Guid.NewGuid():N}", IsActive = isActive, CreatedAt = Time.GetUtcNow().UtcDateTime };
        user.SetPassword(password, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>());
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task DisableAsync(User user)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        (await db.Users.SingleAsync(stored => stored.Id == user.Id)).IsActive = false;
        await db.SaveChangesAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<MigratedApiFactory>
{
    public const string Name = "Database";
}
