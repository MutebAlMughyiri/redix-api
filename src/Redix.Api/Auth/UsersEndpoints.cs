using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Projects;

namespace Redix.Api.Auth;

/// <summary>
/// The logins (UC-007, FR-02): corrects and resets them; making someone inactive is per project (<see cref="ProjectUsersEndpoints.Disable"/>, redix-api #126). A login is shared
/// across projects, so one project's manager may change it only if they manage every project it is in, and never one holding a system permission; otherwise a reset could let
/// them sign in to another project or as the admin (BR-33). Every change is saved with its history row.
/// </summary>
public static class UsersEndpoints
{
    /// <summary>Maps, under <c>/users/{userId}</c>, the edit and <c>reset-password</c>.</summary>
    public static void MapUsers(this WebApplication app)
    {
        var users = app.MapGroup("/users/{userId:guid}").RequireAuthorization(SessionPolicies.Working);
        users.MapPut("", Edit);
        users.MapPost("/reset-password", ResetPassword);
    }

    /// <summary>
    /// The projects and permissions in <paramref name="held"/>, by user: one line per project, by its name, with what the user holds there in the permissions' order, what they are called
    /// there, and whether they are inactive there (redix-api #126).
    /// </summary>
    public static async Task<ILookup<Guid, UserProjectView>> ProjectsByUserAsync(IQueryable<ProjectPermission> held, RedixDbContext db)
    {
        var rows = await held
            .Join(db.Projects, row => row.ProjectId, project => project.Id, (row, project) => new { row.UserId, row.ProjectId, project.Name, row.Permission })
            .ToListAsync();
        var roleNames = await db.ProjectRoleNames.Where(named => held.Any(row => row.UserId == named.UserId && row.ProjectId == named.ProjectId))
            .ToDictionaryAsync(named => (named.UserId, named.ProjectId), named => named.RoleName);
        var chosen = (await db.ChosenPartners.Where(row => held.Any(heldRow => heldRow.UserId == row.UserId && heldRow.ProjectId == row.ProjectId)).ToListAsync())
            .ToLookup(row => (row.UserId, row.ProjectId), row => row.PartnerId);
        var inactive = (await db.InactiveMembers.Where(row => held.Any(heldRow => heldRow.UserId == row.UserId && heldRow.ProjectId == row.ProjectId)).ToListAsync())
            .Select(row => (row.UserId, row.ProjectId)).ToHashSet();
        return rows.GroupBy(row => (row.UserId, row.ProjectId, row.Name)).OrderBy(line => line.Key.Name)
            .ToLookup(line => line.Key.UserId, line => new UserProjectView(line.Key.ProjectId, line.Select(row => row.Permission).Order().ToList(),
                roleNames.GetValueOrDefault((line.Key.UserId, line.Key.ProjectId), ""), [.. chosen[(line.Key.UserId, line.Key.ProjectId)]], inactive.Contains((line.Key.UserId, line.Key.ProjectId))));
    }

    /// <summary>
    /// Changes a user's name, mobile and email, which the manager may change as he typed it when adding them (redix-api #127, AC-07.8). A name outside its limit answers <c>invalidName</c>; a
    /// mobile that is not digits with a leading plus at most, or too long, <c>invalidMobile</c>; an email not well formed <c>emailInvalid</c>, and one another account holds <c>emailTaken</c> (R9).
    /// </summary>
    /// <param name="userId">The user to change.</param>
    /// <param name="request">The new name, mobile and email.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Edit(Guid userId, EditUserRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var displayName = request.DisplayName.Trim();
        var mobile = request.Mobile.Trim();
        var email = request.Email.Trim();
        var refusal = User.RefusalOfDetails(displayName, mobile);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var (emailStatus, emailRefusal) = await User.RefusalOfEmailAsync(email, userId, db);
        if (emailRefusal.Length > 0)
        {
            return Refusal.Of(emailStatus, emailRefusal);
        }
        return await ChangeAsync(userId, principal, db, time, HistoryAction.Update, user =>
        {
            user.DisplayName = displayName;
            user.Mobile = mobile;
            user.Email = email;
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Sets a new temporary password for a forgotten one: the user must change it, a lock from wrong passwords ends, and every device is signed out.
    /// One <see cref="PasswordPolicy"/> refuses for that user answers <c>weakPassword</c>.
    /// </summary>
    /// <param name="userId">The user to reset.</param>
    /// <param name="request">The new temporary password.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the user and devices live in.</param>
    /// <param name="hasher">Hashes the temporary password.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> ResetPassword(Guid userId, ResetPasswordRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var username = await db.Users.Where(user => user.Id == userId).Select(user => user.Username).SingleOrDefaultAsync() ?? "";
        if (!PasswordPolicy.Holds(request.TemporaryPassword, username))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "WeakPassword");
        }
        return await ChangeAsync(userId, principal, db, time, HistoryAction.ResetPassword, user => user.ResetPasswordAsync(request.TemporaryPassword, mustChangePassword: true, hasher, db));
    }

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

    private static async Task<IResult> ChangeAsync(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, HistoryAction action, Func<User, Task> change)
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

        await change(user);
        db.AddHistory(user, action, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        var projects = await ProjectsByUserAsync(db.ProjectPermissions.Where(held => held.UserId == userId), db);
        return Results.Ok(UserView.Of(user, projects[userId].ToList()));
    }
}

/// <summary>What the app sends to change a user.</summary>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; may be empty.</param>
/// <param name="Email">Where a code reaches the user (redix-api #127).</param>
public record EditUserRequest(string DisplayName, string Mobile, string Email);

/// <summary>What the app sends to reset a password.</summary>
/// <param name="TemporaryPassword">The password the user changes at the next sign-in.</param>
public record ResetPasswordRequest(string TemporaryPassword);

/// <summary>What a user holds in one project, as their answer shows it.</summary>
/// <param name="ProjectId">The project's id.</param>
/// <param name="Permissions">What the user may do there.</param>
/// <param name="RoleName">What they are called there; empty when nothing was typed.</param>
/// <param name="PartnerIds">The partners chosen for them there, seen with <see cref="Permission.SeeChosenCapital"/> (redix-api #106).</param>
/// <param name="IsInactive">True while they are inactive there, made so by whoever manages its users (redix-api #126).</param>
public record UserProjectView(Guid ProjectId, List<Permission> Permissions, string RoleName, List<Guid> PartnerIds, bool IsInactive);

/// <summary>A user as the app shows them. It names its fields, so the password's hash and any new column stay out.</summary>
/// <param name="Id">The user's id.</param>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; empty when none.</param>
/// <param name="Email">Where a code reaches the user, which their manager may change (redix-api #127); empty while none is set.</param>
/// <param name="MustChangePassword">True while the password is temporary.</param>
/// <param name="Projects">What they hold, one line per project the caller may see.</param>
public record UserView(Guid Id, string Username, string DisplayName, string Mobile, string Email, bool MustChangePassword, List<UserProjectView> Projects)
{
    /// <summary>The view of <paramref name="user"/>, with these <paramref name="projects"/>.</summary>
    public static UserView Of(User user, List<UserProjectView> projects) =>
        new(user.Id, user.Username, user.DisplayName, user.Mobile, user.Email, user.MustChangePassword, projects);
}
