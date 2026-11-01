using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Auth;

/// <summary>
/// Creates the first owner (BR-29) when the database has no user, from the <c>Owner</c> configuration section (the <c>REDIX_Owner__Username</c> and
/// <c>REDIX_Owner__Password</c> environment variables), with a password that must be changed at first sign-in. EF Core runs it after every migration.
/// </summary>
public static class OwnerSeed
{
    /// <summary>Seeds the owner during a synchronous migration, as <c>dotnet ef database update</c> runs one.</summary>
    public static void Seed(DbContext context, IServiceProvider services)
    {
        var users = context.Set<User>();
        var owner = OwnerSettings(services);
        if (owner.Username.Length == 0 || users.Any())
        {
            return;
        }
        users.Add(CreateOwner(owner, services));
        context.SaveChanges();
    }

    /// <summary>Seeds the owner during an asynchronous migration, as the tests run one.</summary>
    public static async Task SeedAsync(DbContext context, IServiceProvider services, CancellationToken cancellation)
    {
        var users = context.Set<User>();
        var owner = OwnerSettings(services);
        if (owner.Username.Length == 0 || await users.AnyAsync(cancellation))
        {
            return;
        }
        users.Add(CreateOwner(owner, services));
        await context.SaveChangesAsync(cancellation);
    }

    private static OwnerOptions OwnerSettings(IServiceProvider services) =>
        services.GetRequiredService<IConfiguration>().GetSection("Owner").Get<OwnerOptions>() ?? new OwnerOptions();

    private static User CreateOwner(OwnerOptions settings, IServiceProvider services)
    {
        var owner = new User
        {
            Username = settings.Username,
            IsOwner = true,
            MustChangePassword = true,
            CreatedAt = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime,
        };
        owner.SetPassword(settings.Password, services.GetRequiredService<IPasswordHasher<User>>());
        return owner;
    }
}

/// <summary>The first owner's credentials, from the <c>Owner</c> configuration section; empty when none is configured.</summary>
public class OwnerOptions
{
    /// <summary>The owner's username.</summary>
    public string Username { get; set; } = "";

    /// <summary>The owner's temporary password.</summary>
    public string Password { get; set; } = "";
}
