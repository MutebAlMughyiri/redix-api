using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Projects;

namespace Redix.Api.Auth;

/// <summary>
/// The logins (UC-007, FR-02): corrects, disables and resets them. A login is shared across projects, so one project's manager may change it only if they manage every
/// project it is in, and never one holding a system permission; otherwise a reset could let them sign in to another project or as the admin (BR-33). Every change is saved
/// with its audit record.
/// </summary>
public static class UsersEndpoints
{
    /// <summary>Maps, under <c>/users/{userId}</c>, the edit, <c>reset-password</c>, <c>disable</c> and <c>enable</c>.</summary>
    public static void MapUsers(this WebApplication app)
    {
        var users = app.MapGroup("/users/{userId:guid}").RequireAuthorization(SessionPolicies.Working);
        users.MapPost("", Edit);
        users.MapPost("/reset-password", ResetPassword);
        users.MapPost("/disable", Disable);
        users.MapPost("/enable", Enable);
    }

    /// <summary>The projects and permissions in <paramref name="held"/>, by user: one line per project, by its name, with what the user holds there in the permissions' order and what they are called there.</summary>
    public static async Task<ILookup<Guid, UserProjectView>> ProjectsByUserAsync(IQueryable<ProjectPermission> held, RedixDbContext db)
    {
        var rows = await held
            .Join(db.Projects, row => row.ProjectId, project => project.Id, (row, project) => new { row.UserId, row.ProjectId, project.Name, row.Permission })
            .ToListAsync();
        var roleNames = await db.ProjectRoleNames.Where(named => held.Any(row => row.UserId == named.UserId && row.ProjectId == named.ProjectId))
            .ToDictionaryAsync(named => (named.UserId, named.ProjectId), named => named.RoleName);
        return rows.GroupBy(row => (row.UserId, row.ProjectId, row.Name)).OrderBy(line => line.Key.Name)
            .ToLookup(line => line.Key.UserId, line => new UserProjectView(line.Key.ProjectId, line.Key.Name, line.Select(row => row.Permission).Order().ToList(), roleNames.GetValueOrDefault((line.Key.UserId, line.Key.ProjectId), "")));
    }

    /// <summary>Changes a user's name and mobile. A name outside its limit answers <c>invalidName</c>; a mobile that is not digits with a leading plus at most, or too long, <c>invalidMobile</c>.</summary>
    /// <param name="userId">The user to change.</param>
    /// <param name="request">The new name and mobile.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Edit(Guid userId, EditUserRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var displayName = request.DisplayName.Trim();
        var mobile = request.Mobile.Trim();
        var refusal = User.RefusalOfDetails(displayName, mobile);
        if (refusal.Length > 0)
        {
            return Task.FromResult(Refusal.Of(StatusCodes.Status400BadRequest, refusal));
        }
        return ChangeAsync(userId, principal, db, time, AuditAction.Update, user =>
        {
            user.DisplayName = displayName;
            user.Mobile = mobile;
            return Task.FromResult("");
        });
    }

    /// <summary>
    /// Sets a new temporary password for a forgotten one: the user must change it, a lock from wrong passwords ends, and every device is signed out.
    /// One shorter than <see cref="User.MinimumPasswordLength"/> answers <c>weakPassword</c>.
    /// </summary>
    /// <param name="userId">The user to reset.</param>
    /// <param name="request">The new temporary password.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user and devices live in.</param>
    /// <param name="hasher">Hashes the temporary password.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> ResetPassword(Guid userId, ResetPasswordRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        if (!User.IsLongEnough(request.TemporaryPassword))
        {
            return Task.FromResult(Refusal.Of(StatusCodes.Status400BadRequest, "WeakPassword"));
        }
        return ChangeAsync(userId, principal, db, time, AuditAction.ResetPassword, async user =>
        {
            await db.Devices.Where(device => device.UserId == userId).ExecuteUpdateAsync(device => device.SetProperty(d => d.IsRevoked, true));
            user.ResetPassword(request.TemporaryPassword, hasher);
            return "";
        });
    }

    /// <summary>Disables a user, who is signed out at the next request and cannot sign in (AC-01.3). A project's last active manager answers <c>lastManager</c> (BR-29).</summary>
    /// <param name="userId">The user to disable.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Disable(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(userId, principal, db, time, AuditAction.Disable, async user =>
        {
            if (await ProjectManagers.IsTheLastOfAProjectAsync(userId, db))
            {
                return "LastManager";
            }
            user.IsActive = false;
            return "";
        });

    /// <summary>Lets a disabled user sign in again.</summary>
    /// <param name="userId">The user to enable.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Enable(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(userId, principal, db, time, AuditAction.Enable, user =>
        {
            user.IsActive = true;
            return Task.FromResult("");
        });

    private static async Task<bool> MayChangeAsync(Guid userId, ClaimsPrincipal principal, RedixDbContext db)
    {
        if (await db.SystemPermissions.AnyAsync(row => row.UserId == userId))
        {
            return false;
        }
        var access = await UserAccess.OfAsync(AccessTokens.UserIdOf(principal), db);
        var theirProjects = await db.ProjectPermissions.Where(held => held.UserId == userId).Select(held => held.ProjectId).Distinct().ToListAsync();
        return theirProjects.Count > 0 && theirProjects.All(project => access.In(project).Contains(Permission.ManageUsers));
    }

    private static async Task<IResult> ChangeAsync(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, AuditAction action, Func<User, Task<string>> change)
    {
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return Results.NotFound();
        }
        if (!await MayChangeAsync(userId, principal, db))
        {
            return Refusal.Of(StatusCodes.Status403Forbidden, "OtherProject");
        }

        var refusal = await change(user);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, refusal);
        }
        db.Audit(user, action, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        var projects = await ProjectsByUserAsync(db.ProjectPermissions.Where(held => held.UserId == userId), db);
        return Results.Ok(UserView.Of(user, projects[userId].ToList()));
    }
}

/// <summary>What the app sends to change a user.</summary>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; may be empty.</param>
public record EditUserRequest(string DisplayName, string Mobile);

/// <summary>What the app sends to reset a password.</summary>
/// <param name="TemporaryPassword">The password the user changes at the next sign-in.</param>
public record ResetPasswordRequest(string TemporaryPassword);

/// <summary>What a user holds in one project, as their answer shows it.</summary>
/// <param name="ProjectId">The project's id.</param>
/// <param name="Name">The project's name.</param>
/// <param name="Permissions">What the user may do there.</param>
/// <param name="RoleName">What they are called there; empty when nothing was typed.</param>
public record UserProjectView(Guid ProjectId, string Name, List<Permission> Permissions, string RoleName);

/// <summary>A user as the app shows them. It names its fields, so the password's hash and any new column stay out.</summary>
/// <param name="Id">The user's id.</param>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; empty when none.</param>
/// <param name="IsActive">False once disabled.</param>
/// <param name="MustChangePassword">True while the password is temporary.</param>
/// <param name="Projects">What they hold, one line per project the caller may see.</param>
public record UserView(Guid Id, string Username, string DisplayName, string Mobile, bool IsActive, bool MustChangePassword, List<UserProjectView> Projects)
{
    /// <summary>The view of <paramref name="user"/>, with these <paramref name="projects"/>.</summary>
    public static UserView Of(User user, List<UserProjectView> projects) =>
        new(user.Id, user.Username, user.DisplayName, user.Mobile, user.IsActive, user.MustChangePassword, projects);
}
