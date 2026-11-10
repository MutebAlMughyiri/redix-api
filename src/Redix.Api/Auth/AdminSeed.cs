using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>
/// Creates the first user, the admin (BR-29), when the database has no user, from the <c>Admin</c> configuration section (the username in the
/// environment's <c>appsettings.{name}.json</c>, the password in its <c>REDIX_{name}_Admin__Password</c> variable, redix-api #118), with a password that must be changed at first sign-in.
/// EF Core runs it after every migration, a rollback too; while a migration is still to apply, the tables it writes may not be there, so it waits.
/// </summary>
public static class AdminSeed
{
    /// <summary>Seeds the admin during a synchronous migration, as <c>dotnet ef database update</c> runs one.</summary>
    public static void Seed(DbContext context, IServiceProvider services)
    {
        var users = context.Set<User>();
        var settings = Settings(services);
        if (settings.Username.Length == 0 || context.Database.GetPendingMigrations().Any() || users.Any())
        {
            return;
        }
        Add(context, settings, services);
        context.SaveChanges();
    }

    /// <summary>Seeds the admin during an asynchronous migration, as the tests run one.</summary>
    public static async Task SeedAsync(DbContext context, IServiceProvider services, CancellationToken cancellation)
    {
        var users = context.Set<User>();
        var settings = Settings(services);
        if (settings.Username.Length == 0 || (await context.Database.GetPendingMigrationsAsync(cancellation)).Any() || await users.AnyAsync(cancellation))
        {
            return;
        }
        Add(context, settings, services);
        await context.SaveChangesAsync(cancellation);
    }

    private static AdminOptions Settings(IServiceProvider services) =>
        services.GetRequiredService<IConfiguration>().GetSection("Admin").Get<AdminOptions>() ?? new AdminOptions();

    private static void Add(DbContext context, AdminOptions settings, IServiceProvider services)
    {
        var user = new User
        {
            Username = settings.Username,
            DisplayName = settings.Username,
            MustChangePassword = true,
            CreatedAt = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime,
        };
        user.SetPassword(settings.Password, services.GetRequiredService<IPasswordHasher<User>>());
        context.Set<User>().Add(user);
        context.Set<SystemPermission>().Add(new SystemPermission { UserId = user.Id, Permission = Permission.ApproveProjects });
    }
}

/// <summary>The first user's credentials, from the <c>Admin</c> configuration section; empty when none is configured.</summary>
public class AdminOptions
{
    /// <summary>The admin's username.</summary>
    public string Username { get; set; } = "";

    /// <summary>The admin's temporary password.</summary>
    public string Password { get; set; } = "";
}
