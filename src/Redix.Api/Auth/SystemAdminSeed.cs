using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>
/// Creates the first user, the system admin (BR-29), when the database has no user, from the <c>SystemAdmin</c> configuration section (the
/// <c>REDIX_SystemAdmin__Username</c> and <c>REDIX_SystemAdmin__Password</c> environment variables), with a password that must be changed at first sign-in.
/// EF Core runs it after every migration.
/// </summary>
public static class SystemAdminSeed
{
    /// <summary>Seeds the system admin during a synchronous migration, as <c>dotnet ef database update</c> runs one.</summary>
    public static void Seed(DbContext context, IServiceProvider services)
    {
        var users = context.Set<User>();
        var settings = Settings(services);
        if (settings.Username.Length == 0 || users.Any())
        {
            return;
        }
        Add(context, settings, services);
        context.SaveChanges();
    }

    /// <summary>Seeds the system admin during an asynchronous migration, as the tests run one.</summary>
    public static async Task SeedAsync(DbContext context, IServiceProvider services, CancellationToken cancellation)
    {
        var users = context.Set<User>();
        var settings = Settings(services);
        if (settings.Username.Length == 0 || await users.AnyAsync(cancellation))
        {
            return;
        }
        Add(context, settings, services);
        await context.SaveChangesAsync(cancellation);
    }

    private static SystemAdminOptions Settings(IServiceProvider services) =>
        services.GetRequiredService<IConfiguration>().GetSection("SystemAdmin").Get<SystemAdminOptions>() ?? new SystemAdminOptions();

    private static void Add(DbContext context, SystemAdminOptions settings, IServiceProvider services)
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
        context.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = RoleSeed.SystemAdminId });
    }
}

/// <summary>The first user's credentials, from the <c>SystemAdmin</c> configuration section; empty when none is configured.</summary>
public class SystemAdminOptions
{
    /// <summary>The system admin's username.</summary>
    public string Username { get; set; } = "";

    /// <summary>The system admin's temporary password.</summary>
    public string Password { get; set; } = "";
}
