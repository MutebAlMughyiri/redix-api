using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>A project's people and their roles (UC-009, FR-02, FR-36), for its project manager. Every project keeps its own list (BR-33), and every change is saved with its audit record.</summary>
public static class ProjectUsersEndpoints
{
    /// <summary>Maps, under <c>/projects/{projectId}</c>, <c>GET /roles</c>, <c>GET /users</c>, <c>POST /users</c>, <c>POST /users/existing</c>, and giving and taking a role.</summary>
    public static void MapProjectUsers(this WebApplication app)
    {
        var project = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ManageUsers));
        project.MapGet("/roles", Roles);
        project.MapGet("/users", List);
        project.MapPost("/users", Create);
        project.MapPost("/users/existing", AddExisting);
        project.MapPost("/users/{userId:guid}/roles/{roleId:guid}", GiveRole);
        project.MapPost("/users/{userId:guid}/roles/{roleId:guid}/remove", TakeRole);
    }

    /// <summary>The roles that can be given in a project, in their order (BR-32).</summary>
    /// <param name="db">The database the roles live in.</param>
    public static Task<List<RoleView>> Roles(RedixDbContext db) =>
        db.Roles.Where(role => role.Scope == RoleScope.Project).OrderBy(role => role.SortOrder).Select(role => new RoleView(role.Id, role.WordKey)).ToListAsync();

    /// <summary>The people of this project and their roles in it, active users first, by name. Their other projects never show (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<UserView>> List(Guid projectId, RedixDbContext db)
    {
        var members = db.ProjectMembers.Where(member => member.ProjectId == projectId);
        var users = await db.Users.Where(user => members.Any(member => member.UserId == user.Id)).OrderByDescending(user => user.IsActive).ThenBy(user => user.DisplayName).ToListAsync();
        var projects = await UsersEndpoints.ProjectsByUserAsync(members, db);
        return users.Select(user => UserView.Of(user, projects[user.Id].ToList())).ToList();
    }

    /// <summary>
    /// Creates a login in this project, as its last role, who must change the temporary password at first sign-in (AC-02.1). A field outside its limit answers its
    /// code: <c>invalidName</c>, <c>invalidUsername</c>, <c>invalidMobile</c> or <c>weakPassword</c>; a username already taken, whatever its case, answers <c>usernameTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The user's name, username, mobile and temporary password.</param>
    /// <param name="principal">The project manager's session.</param>
    /// <param name="db">The database the user joins.</param>
    /// <param name="hasher">Hashes the temporary password.</param>
    /// <param name="time">The current time, for the user and the audit records.</param>
    public static async Task<IResult> Create(Guid projectId, CreateUserRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
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
        await AddAsTheLastRoleAsync(projectId, user.Id, principal, db, now);
        await db.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Adds someone who already has a login, named by their exact username, as this project's last role; nothing else about them shows (BR-33). An unknown username answers <c>unknownUser</c>, and so does the admin's, so a manager can never reach that login.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The exact username.</param>
    /// <param name="principal">The project manager's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> AddExisting(Guid projectId, AddExistingUserRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var username = request.Username.Trim();
        var user = await db.Users.SingleOrDefaultAsync(user => user.Username == username && !db.UserRoles.Any(row => row.UserId == user.Id));
        if (user is null)
        {
            return Refusal.Of(StatusCodes.Status404NotFound, "UnknownUser");
        }

        if (!await db.ProjectMembers.AnyAsync(member => member.ProjectId == projectId && member.UserId == user.Id))
        {
            await AddAsTheLastRoleAsync(projectId, user.Id, principal, db, time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Gives someone in this project a role; holding it already changes nothing. A role that cannot be given in a project answers <c>invalidRole</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="roleId">The role.</param>
    /// <param name="principal">The project manager's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> GiveRole(Guid projectId, Guid userId, Guid roleId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user is null)
        {
            return Results.NotFound();
        }
        if (!await db.Roles.AnyAsync(role => role.Id == roleId && role.Scope == RoleScope.Project))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidRole");
        }

        if (!await db.ProjectMembers.AnyAsync(member => member.ProjectId == projectId && member.UserId == userId && member.RoleId == roleId))
        {
            var member = new ProjectMember { ProjectId = projectId, UserId = userId, RoleId = roleId };
            db.ProjectMembers.Add(member);
            db.Audit(member, AuditAction.AssignProject, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Takes a role from someone in this project; their last role takes them out of it. The role of a project's last active manager answers <c>lastManager</c> (BR-29).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="roleId">The role.</param>
    /// <param name="principal">The project manager's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> TakeRole(Guid projectId, Guid userId, Guid roleId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user is null)
        {
            return Results.NotFound();
        }

        var member = await db.ProjectMembers.SingleOrDefaultAsync(member => member.ProjectId == projectId && member.UserId == userId && member.RoleId == roleId);
        if (member is not null)
        {
            if (await ProjectManagers.WouldLoseTheLastAsync(member, db))
            {
                return Refusal.Of(StatusCodes.Status409Conflict, "LastManager");
            }
            db.ProjectMembers.Remove(member);
            db.Audit(member, AuditAction.RemoveProject, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    private static async Task AddAsTheLastRoleAsync(Guid projectId, Guid userId, ClaimsPrincipal principal, RedixDbContext db, DateTime now)
    {
        var lastRoleId = await db.Roles.Where(role => role.Scope == RoleScope.Project).OrderByDescending(role => role.SortOrder).Select(role => role.Id).FirstAsync();
        var member = new ProjectMember { ProjectId = projectId, UserId = userId, RoleId = lastRoleId };
        db.ProjectMembers.Add(member);
        db.Audit(member, AuditAction.AssignProject, AccessTokens.UserIdOf(principal), now);
    }

    private static Task<User?> InProjectAsync(Guid projectId, Guid userId, RedixDbContext db) =>
        db.Users.SingleOrDefaultAsync(user => user.Id == userId && db.ProjectMembers.Any(member => member.ProjectId == projectId && member.UserId == userId));

    private static async Task<UserView> ViewInProjectAsync(User user, Guid projectId, RedixDbContext db)
    {
        var members = db.ProjectMembers.Where(member => member.ProjectId == projectId && member.UserId == user.Id);
        return UserView.Of(user, (await UsersEndpoints.ProjectsByUserAsync(members, db))[user.Id].ToList());
    }
}

/// <summary>What the app sends to add someone who already has a login.</summary>
/// <param name="Username">Their exact username.</param>
public record AddExistingUserRequest(string Username);
