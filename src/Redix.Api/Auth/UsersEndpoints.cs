using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Projects;

namespace Redix.Api.Auth;

/// <summary>Creates, corrects, disables and resets the users (UC-007, FR-02), and gives them projects (UC-008, FR-36). Owners only (BR-29). Every change is saved with its audit record.</summary>
public static class UsersEndpoints
{
    /// <summary>Maps <c>GET /users</c>, <c>POST /users</c>, and under <c>/users/{userId}</c> the edit, <c>reset-password</c>, <c>disable</c>, <c>enable</c>, and giving and removing a project.</summary>
    public static void MapUsers(this WebApplication app)
    {
        var users = app.MapGroup("/users").RequireAuthorization(SessionPolicies.Owner);
        users.MapGet("", List);
        users.MapPost("", Create);
        users.MapPost("/{userId:guid}", Edit);
        users.MapPost("/{userId:guid}/reset-password", ResetPassword);
        users.MapPost("/{userId:guid}/disable", Disable);
        users.MapPost("/{userId:guid}/enable", Enable);
        users.MapPost("/{userId:guid}/projects/{projectId:guid}", AssignProject);
        users.MapPost("/{userId:guid}/projects/{projectId:guid}/remove", RemoveProject);
    }

    /// <summary>Every user with their projects: owners first, then active users, by name.</summary>
    /// <param name="db">The database the users live in.</param>
    public static async Task<List<UserView>> List(RedixDbContext db)
    {
        var users = await db.Users.OrderByDescending(user => user.IsOwner).ThenByDescending(user => user.IsActive).ThenBy(user => user.DisplayName).ToListAsync();
        var projects = await ProjectsByUserAsync(db);
        return users.Select(user => UserView.Of(user, projects[user.Id].ToList())).ToList();
    }

    /// <summary>
    /// Gives <paramref name="userId"/> a project with a role, or changes the role they have in it (FR-36). An owner answers <c>ownerIsInEveryProject</c>:
    /// an owner has no rows to change (BR-29).
    /// </summary>
    /// <param name="userId">The user.</param>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The role in that project.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> AssignProject(Guid userId, Guid projectId, AssignProjectRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == userId);
        if (user is null || !await db.Projects.AnyAsync(project => project.Id == projectId))
        {
            return Results.NotFound();
        }
        if (user.IsOwner)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "OwnerIsInEveryProject");
        }

        var member = await db.ProjectMembers.SingleOrDefaultAsync(member => member.UserId == userId && member.ProjectId == projectId);
        if (member is null)
        {
            member = new ProjectMember { UserId = userId, ProjectId = projectId };
            db.ProjectMembers.Add(member);
        }
        member.Role = request.Role;
        db.Audit(member, AuditAction.AssignProject, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(user, db));
    }

    /// <summary>Takes a project away from <paramref name="userId"/>; nothing changes if they do not have it.</summary>
    /// <param name="userId">The user.</param>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> RemoveProject(Guid userId, Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return Results.NotFound();
        }

        var member = await db.ProjectMembers.SingleOrDefaultAsync(member => member.UserId == userId && member.ProjectId == projectId);
        if (member is not null)
        {
            db.ProjectMembers.Remove(member);
            db.Audit(member, AuditAction.RemoveProject, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewOfAsync(user, db));
    }

    private static async Task<ILookup<Guid, UserProjectView>> ProjectsByUserAsync(RedixDbContext db)
    {
        var rows = await db.ProjectMembers.Join(db.Projects, member => member.ProjectId, project => project.Id, (member, project) => new { member.UserId, project.Id, project.Name, member.Role })
            .OrderBy(row => row.Name).ToListAsync();
        return rows.ToLookup(row => row.UserId, row => new UserProjectView(row.Id, row.Name, row.Role));
    }

    private static async Task<UserView> ViewOfAsync(User user, RedixDbContext db) => UserView.Of(user, (await ProjectsByUserAsync(db))[user.Id].ToList());

    /// <summary>
    /// Creates a user who must change the temporary password at first sign-in (AC-02.1). A field outside its limit answers its code: <c>invalidName</c>,
    /// <c>invalidUsername</c>, <c>invalidMobile</c> or <c>weakPassword</c>; a username already taken, whatever its case, answers <c>usernameTaken</c>.
    /// </summary>
    /// <param name="request">The user's name, username, mobile and temporary password.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database the user joins.</param>
    /// <param name="hasher">Hashes the temporary password.</param>
    /// <param name="time">The current time, for the user and the audit record.</param>
    public static async Task<IResult> Create(CreateUserRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var displayName = request.DisplayName.Trim();
        var username = request.Username.Trim();
        var mobile = request.Mobile.Trim();
        var refusal = User.RefusalOfNew(displayName, username, mobile, request.TemporaryPassword);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if (await db.Users.AnyAsync(user => user.Username == username))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "UsernameTaken");
        }

        var now = time.GetUtcNow().UtcDateTime;
        var user = new User { Username = username, DisplayName = displayName, Mobile = mobile, MustChangePassword = true, CreatedAt = now };
        user.SetPassword(request.TemporaryPassword, hasher);
        db.Users.Add(user);
        db.Audit(user, AuditAction.Create, AccessTokens.UserIdOf(principal), now);
        await db.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", UserView.Of(user, []));
    }

    /// <summary>Changes a user's name and mobile. A field outside its limit answers <c>invalidName</c> or <c>invalidMobile</c>.</summary>
    /// <param name="userId">The user to change.</param>
    /// <param name="request">The new name and mobile.</param>
    /// <param name="principal">The owner's session.</param>
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
            return "";
        });
    }

    /// <summary>
    /// Sets a new temporary password for a forgotten one: the user must change it, a lock from wrong passwords ends, and every device is signed out.
    /// One shorter than <see cref="User.MinimumPasswordLength"/> answers <c>weakPassword</c>.
    /// </summary>
    /// <param name="userId">The user to reset.</param>
    /// <param name="request">The new temporary password.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database the user and devices live in.</param>
    /// <param name="hasher">Hashes the temporary password.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> ResetPassword(Guid userId, ResetPasswordRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        if (!User.IsLongEnough(request.TemporaryPassword))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "WeakPassword");
        }
        await db.Devices.Where(device => device.UserId == userId).ExecuteUpdateAsync(device => device.SetProperty(d => d.IsRevoked, true));
        return await ChangeAsync(userId, principal, db, time, AuditAction.ResetPassword, user =>
        {
            user.ResetPassword(request.TemporaryPassword, hasher);
            return "";
        });
    }

    /// <summary>Disables a user, who is signed out at the next request and cannot sign in (AC-01.3). An owner answers <c>ownerCannotBeDisabled</c>.</summary>
    /// <param name="userId">The user to disable.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Disable(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(userId, principal, db, time, AuditAction.Disable, user => user.Disable());

    /// <summary>Lets a disabled user sign in again.</summary>
    /// <param name="userId">The user to enable.</param>
    /// <param name="principal">The owner's session.</param>
    /// <param name="db">The database the user lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Enable(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(userId, principal, db, time, AuditAction.Enable, user =>
        {
            user.IsActive = true;
            return "";
        });

    private static async Task<IResult> ChangeAsync(Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, AuditAction action, Func<User, string> change)
    {
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == userId);
        if (user is null)
        {
            return Results.NotFound();
        }

        var refusal = change(user);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, refusal);
        }
        db.Audit(user, action, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(user, db));
    }
}

/// <summary>What the app sends to create a user.</summary>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="Mobile">The user's mobile; may be empty.</param>
/// <param name="TemporaryPassword">The password the user changes at first sign-in.</param>
public record CreateUserRequest(string DisplayName, string Username, string Mobile, string TemporaryPassword);

/// <summary>What the app sends to change a user.</summary>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; may be empty.</param>
public record EditUserRequest(string DisplayName, string Mobile);

/// <summary>What the app sends to reset a password.</summary>
/// <param name="TemporaryPassword">The password the user changes at the next sign-in.</param>
public record ResetPasswordRequest(string TemporaryPassword);

/// <summary>What the app sends to give a project.</summary>
/// <param name="Role">The role in that project.</param>
public record AssignProjectRequest(ProjectRole Role);

/// <summary>A project as a user's answer shows it.</summary>
/// <param name="ProjectId">The project's id.</param>
/// <param name="Name">The project's name.</param>
/// <param name="Role">The user's role in it.</param>
public record UserProjectView(Guid ProjectId, string Name, ProjectRole Role);

/// <summary>A user as the app shows it. It names its fields, so the password's hash and any new column stay out.</summary>
/// <param name="Id">The user's id.</param>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Mobile">The user's mobile; empty when none.</param>
/// <param name="IsOwner">True for an owner (BR-29).</param>
/// <param name="IsActive">False once disabled.</param>
/// <param name="MustChangePassword">True while the password is temporary.</param>
/// <param name="Projects">The user's projects and roles; empty for an owner, who is in every project.</param>
public record UserView(Guid Id, string Username, string DisplayName, string Mobile, bool IsOwner, bool IsActive, bool MustChangePassword, List<UserProjectView> Projects)
{
    /// <summary>The view of <paramref name="user"/>, with their <paramref name="projects"/>.</summary>
    public static UserView Of(User user, List<UserProjectView> projects) =>
        new(user.Id, user.Username, user.DisplayName, user.Mobile, user.IsOwner, user.IsActive, user.MustChangePassword, projects);
}
