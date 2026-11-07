using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>A project's people and what each may do in it (UC-009, FR-02, FR-36), for whoever manages its users. Every project keeps its own list (BR-33), and every change is saved with its audit record.</summary>
public static class ProjectUsersEndpoints
{
    /// <summary>Maps, under <c>/projects/{projectId}</c>, <c>GET /members</c> for anyone who reads the project; and for whoever manages its users <c>GET /users</c>, <c>POST /users</c>, <c>POST /users/existing</c>, and giving and taking a permission.</summary>
    public static void MapProjectUsers(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/members", Members).RequireAuthorization(nameof(Permission.ReadProject));
        var project = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ManageUsers));
        project.MapGet("/users", List);
        project.MapPost("/users", Create);
        project.MapPost("/users/existing", AddExisting);
        project.MapPost("/users/{userId:guid}/permissions/{permission}", Give);
        project.MapPost("/users/{userId:guid}/permissions/{permission}/remove", Take);
    }

    /// <summary>The active people of this project by name, once each, for choosing who holds something in it. Only their names show (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<MemberView>> Members(Guid projectId, RedixDbContext db) =>
        db.Users.Where(user => user.IsActive && db.ProjectPermissions.Any(held => held.ProjectId == projectId && held.UserId == user.Id))
            .OrderBy(user => user.DisplayName).Select(user => new MemberView(user.Id, user.DisplayName)).ToListAsync();

    /// <summary>Whether <paramref name="userId"/> is in <paramref name="projectId"/>: holds anything there.</summary>
    public static Task<bool> IsMemberAsync(Guid projectId, Guid userId, RedixDbContext db) => db.ProjectPermissions.AnyAsync(held => held.ProjectId == projectId && held.UserId == userId);

    /// <summary>The people of this project and what each may do in it, active users first, by name. Their other projects never show (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<UserView>> List(Guid projectId, RedixDbContext db)
    {
        var held = db.ProjectPermissions.Where(row => row.ProjectId == projectId);
        var users = await db.Users.Where(user => held.Any(row => row.UserId == user.Id)).OrderByDescending(user => user.IsActive).ThenBy(user => user.DisplayName).ToListAsync();
        var projects = await UsersEndpoints.ProjectsByUserAsync(held, db);
        return users.Select(user => UserView.Of(user, projects[user.Id].ToList())).ToList();
    }

    /// <summary>
    /// Creates a login in this project holding what was ticked and <see cref="Permission.ReadProject"/>, who must change the temporary password at first sign-in (AC-02.1). A field outside
    /// its limit answers its code: <c>invalidName</c>, <c>invalidUsername</c>, <c>invalidMobile</c> or <c>weakPassword</c>; the system's own permission <c>invalidPermission</c>; a username
    /// already taken, whatever its case, <c>usernameTaken</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The user's name, username, mobile, temporary password and permissions.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
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
        if (!request.Permissions.All(ProjectPermission.InAProject.Contains))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPermission");
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
        foreach (var permission in request.Permissions.Prepend(Permission.ReadProject).Distinct())
        {
            Add(projectId, user.Id, permission, principal, db, now);
        }
        await db.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Adds someone who already has a login, named by their exact username, to this project holding <see cref="Permission.ReadProject"/>; nothing else about them shows (BR-33). An unknown username answers <c>unknownUser</c>, and so does the admin's, so a project can never reach that login.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The exact username.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> AddExisting(Guid projectId, AddExistingUserRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var username = request.Username.Trim();
        var user = await db.Users.SingleOrDefaultAsync(user => user.Username == username && !db.SystemPermissions.Any(row => row.UserId == user.Id));
        if (user is null)
        {
            return Refusal.Of(StatusCodes.Status404NotFound, "UnknownUser");
        }

        if (!await IsMemberAsync(projectId, user.Id, db))
        {
            Add(projectId, user.Id, Permission.ReadProject, principal, db, time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Gives someone in this project one permission; holding it already changes nothing. A name that is not a permission of a project answers <c>invalidPermission</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="permission">The permission's name.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Give(Guid projectId, Guid userId, string permission, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user is null)
        {
            return Results.NotFound();
        }
        if (!ProjectPermission.TryOf(permission, out var given))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPermission");
        }

        if (!await db.ProjectPermissions.AnyAsync(held => held.ProjectId == projectId && held.UserId == userId && held.Permission == given))
        {
            Add(projectId, userId, given, principal, db, time.GetUtcNow().UtcDateTime);
            await db.SaveChangesAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>
    /// Takes one permission from someone in this project; taking <see cref="Permission.ReadProject"/> takes every one of theirs, which takes them out of it. What would leave the project
    /// with nobody active managing its users answers <c>lastManager</c> (BR-29); a name that is not a permission of a project, <c>invalidPermission</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="permission">The permission's name.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Take(Guid projectId, Guid userId, string permission, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user is null)
        {
            return Results.NotFound();
        }
        if (!ProjectPermission.TryOf(permission, out var taking))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPermission");
        }

        var taken = await db.ProjectPermissions.Where(held => held.ProjectId == projectId && held.UserId == userId && (taking == Permission.ReadProject || held.Permission == taking)).ToListAsync();
        if (await ProjectManagers.WouldLoseTheLastAsync(taken, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "LastManager");
        }
        var now = time.GetUtcNow().UtcDateTime;
        foreach (var held in taken)
        {
            db.ProjectPermissions.Remove(held);
            db.Audit(held, AuditAction.RemoveProject, AccessTokens.UserIdOf(principal), now);
        }
        await db.SaveChangesAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    private static void Add(Guid projectId, Guid userId, Permission permission, ClaimsPrincipal principal, RedixDbContext db, DateTime now)
    {
        var held = new ProjectPermission { ProjectId = projectId, UserId = userId, Permission = permission };
        db.ProjectPermissions.Add(held);
        db.Audit(held, AuditAction.AssignProject, AccessTokens.UserIdOf(principal), now);
    }

    private static Task<User?> InProjectAsync(Guid projectId, Guid userId, RedixDbContext db) =>
        db.Users.SingleOrDefaultAsync(user => user.Id == userId && db.ProjectPermissions.Any(held => held.ProjectId == projectId && held.UserId == userId));

    private static async Task<UserView> ViewInProjectAsync(User user, Guid projectId, RedixDbContext db)
    {
        var held = db.ProjectPermissions.Where(row => row.ProjectId == projectId && row.UserId == user.Id);
        return UserView.Of(user, (await UsersEndpoints.ProjectsByUserAsync(held, db))[user.Id].ToList());
    }
}

/// <summary>What the app sends to add someone who already has a login.</summary>
/// <param name="Username">Their exact username.</param>
public record AddExistingUserRequest(string Username);

/// <summary>Someone in a project, as a person choosing among them sees them.</summary>
/// <param name="Id">The user's id.</param>
/// <param name="DisplayName">The user's name.</param>
public record MemberView(Guid Id, string DisplayName);
