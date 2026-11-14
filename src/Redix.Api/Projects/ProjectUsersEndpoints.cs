using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Partners;

namespace Redix.Api.Projects;

/// <summary>A project's people and what each may do in it (UC-009, FR-02, FR-36), for whoever manages its users. Every project keeps its own list (BR-33), and every change is saved with its history row.</summary>
public static class ProjectUsersEndpoints
{
    /// <summary>
    /// Maps, under <c>/projects/{projectId}</c>, <c>GET /members</c> for anyone who reads the project; and for whoever manages its users <c>GET /users</c>, <c>POST /users</c>,
    /// <c>POST /users/existing</c>, giving and taking a permission, a person's role name, and making a person inactive there and active again.
    /// </summary>
    public static void MapProjectUsers(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/members", Members).RequireAuthorization(nameof(Permission.ReadProject));
        var project = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ManageUsers));
        project.MapGet("/users", List);
        project.MapPost("/users", Create);
        project.MapPost("/users/existing", AddExisting);
        project.MapPost("/users/{userId:guid}/permissions/{permission}", Give);
        project.MapDelete("/users/{userId:guid}/permissions/{permission}", Take);
        project.MapPost("/users/{userId:guid}/partners/{partnerId:guid}", Choose);
        project.MapDelete("/users/{userId:guid}/partners/{partnerId:guid}", Unchoose);
        project.MapPut("/users/{userId:guid}/role-name", SetRoleName);
        project.MapPost("/users/{userId:guid}/disable", Disable);
        project.MapPost("/users/{userId:guid}/enable", Enable);
    }

    /// <summary>The people of this project active there, by name, once each, for choosing who holds something in it (redix-api #126). Only their names show (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<MemberView>> Members(Guid projectId, RedixDbContext db) =>
        db.Users.Where(user => db.ProjectPermissions.Any(held => held.ProjectId == projectId && held.UserId == user.Id)
                && !db.InactiveMembers.Any(inactive => inactive.ProjectId == projectId && inactive.UserId == user.Id))
            .OrderBy(user => user.DisplayName).Select(user => new MemberView(user.Id, user.DisplayName)).ToListAsync();

    /// <summary>Whether <paramref name="userId"/> is in <paramref name="projectId"/>: holds anything there.</summary>
    public static Task<bool> IsMemberAsync(Guid projectId, Guid userId, RedixDbContext db) => db.ProjectPermissions.AnyAsync(held => held.ProjectId == projectId && held.UserId == userId);

    /// <summary>The people of this project and what each may do in it, those active there first (redix-api #126), by name. Their other projects never show (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<UserView>> List(Guid projectId, RedixDbContext db)
    {
        var held = db.ProjectPermissions.Where(row => row.ProjectId == projectId);
        var users = await db.Users.Where(user => held.Any(row => row.UserId == user.Id))
            .OrderBy(user => db.InactiveMembers.Any(inactive => inactive.ProjectId == projectId && inactive.UserId == user.Id)).ThenBy(user => user.DisplayName).ToListAsync();
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
    /// <param name="time">The current time, for the user and the history rows.</param>
    public static async Task<IResult> Create(Guid projectId, CreateUserRequest request, ClaimsPrincipal principal, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var roleName = request.RoleName.Trim();
        var roleRefusal = ProjectRoleName.RefusalOf(roleName);
        if (roleRefusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, roleRefusal);
        }
        if (!request.Permissions.All(ProjectPermission.InAProject.Contains) || CapitalSight.HoldsBoth(request.Permissions))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPermission");
        }
        var partnerIds = request.Permissions.Contains(Permission.SeeChosenCapital) ? request.PartnerIds.Distinct().ToList() : [];
        if (await db.Partners.Where(Partner.Kept).CountAsync(partner => partner.ProjectId == projectId && partnerIds.Contains(partner.Id)) != partnerIds.Count)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownPartner");
        }

        var now = time.GetUtcNow().UtcDateTime;
        var (user, status, refusal) = await User.AddNewAsync(request.DisplayName, request.Username, request.Mobile, request.TemporaryPassword, mustChangePassword: true, db, hasher, now);
        if (refusal.Length > 0)
        {
            return Refusal.Of(status, refusal);
        }
        db.AddHistory(user, HistoryAction.Create, AccessTokens.UserIdOf(principal), now);
        foreach (var permission in request.Permissions.Prepend(Permission.ReadProject).Distinct())
        {
            Add(projectId, user.Id, permission, principal, db, now);
        }
        foreach (var partnerId in partnerIds)
        {
            CapitalSight.Choose(projectId, user.Id, partnerId, AccessTokens.UserIdOf(principal), now, db);
        }
        await ProjectRoleName.NameAsync(projectId, user.Id, roleName, AccessTokens.UserIdOf(principal), now, db);
        await db.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Adds someone who already has a login, named by their exact username, to this project holding <see cref="Permission.ReadProject"/>; nothing else about them shows (BR-33). An unknown username answers <c>unknownUser</c>, and so does the admin's, so a project can never reach that login.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The exact username.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> AddExisting(Guid projectId, AddExistingUserRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var username = request.Username.Trim();
        var user = await db.Users.SingleOrDefaultAsync(user => user.Username == username && !db.SystemPermissions.Any(row => row.UserId == user.Id)) ?? User.None;
        if (user == User.None)
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

    /// <summary>
    /// Gives someone in this project one permission; holding it already changes nothing. Giving one of <see cref="Permission.SeeAllCapital"/> and <see cref="Permission.SeeChosenCapital"/> takes
    /// the other (redix-api #106). A name that is not a permission of a project answers <c>invalidPermission</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="permission">The permission's name.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Give(Guid projectId, Guid userId, string permission, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
        {
            return Results.NotFound();
        }
        if (!ProjectPermission.TryOf(permission, out var given))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPermission");
        }

        await GiveAsync(projectId, userId, given, principal, db, time.GetUtcNow().UtcDateTime);
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>
    /// Takes one permission from someone in this project; taking <see cref="Permission.ReadProject"/> takes every one of theirs, which takes them out of it, and ends their being inactive
    /// there, so someone added again arrives active (redix-api #126). Taken twice at the same moment, the second answers as the first (findings row 190). What would leave the project
    /// with nobody active managing its users answers <c>lastManager</c> (BR-29); a name that is not a permission of a project, <c>invalidPermission</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="permission">The permission's name.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Take(Guid projectId, Guid userId, string permission, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
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
            db.AddHistory(held, HistoryAction.RemoveProject, AccessTokens.UserIdOf(principal), now);
        }
        if (taken.Any(held => held.Permission == Permission.SeeChosenCapital))
        {
            await CapitalSight.ClearAsync(projectId, userId, AccessTokens.UserIdOf(principal), now, db);
        }
        if (taking == Permission.ReadProject)
        {
            await ProjectRoleName.NameAsync(projectId, userId, "", AccessTokens.UserIdOf(principal), now, db);
            await InactiveMember.EndAsync(projectId, userId, HistoryAction.RemoveProject, AccessTokens.UserIdOf(principal), now, db);
        }
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Names what someone in this project is called there (Muteb, 9 Nov 2026), replacing what was typed before; empty clears it. Longer than <see cref="ProjectRoleName.MaxLength"/> answers <c>invalidRoleName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="request">The role name.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> SetRoleName(Guid projectId, Guid userId, RoleNameRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
        {
            return Results.NotFound();
        }
        var roleName = request.RoleName.Trim();
        var refusal = ProjectRoleName.RefusalOf(roleName);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        await ProjectRoleName.NameAsync(projectId, userId, roleName, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db);
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>
    /// Chooses a partner of this project whose capital someone in it sees (redix-api #106), giving them <see cref="Permission.SeeChosenCapital"/>, which takes <see cref="Permission.SeeAllCapital"/>.
    /// Chosen already changes nothing; a person or a partner not of this project is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="partnerId">The partner, who must be of this project.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history rows.</param>
    public static async Task<IResult> Choose(Guid projectId, Guid userId, Guid partnerId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None || !await db.Partners.Where(Partner.Kept).AnyAsync(partner => partner.Id == partnerId && partner.ProjectId == projectId))
        {
            return Results.NotFound();
        }

        var now = time.GetUtcNow().UtcDateTime;
        await GiveAsync(projectId, userId, Permission.SeeChosenCapital, principal, db, now);
        if (!await db.ChosenPartners.AnyAsync(row => row.ProjectId == projectId && row.UserId == userId && row.PartnerId == partnerId))
        {
            CapitalSight.Choose(projectId, userId, partnerId, AccessTokens.UserIdOf(principal), now, db);
        }
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Takes one chosen partner from someone in this project (redix-api #106); not chosen changes nothing, at the same moment too (findings row 190). A person not of this project is not found.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="partnerId">The partner.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Unchoose(Guid projectId, Guid userId, Guid partnerId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
        {
            return Results.NotFound();
        }

        await CapitalSight.ClearAsync(projectId, userId, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db, partnerId);
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>
    /// Makes someone in this project inactive there (redix-api #126, D1), with its History line: he keeps his permissions and entries, and every route of the project refuses him as for a
    /// project he is not in (<see cref="UserAccess"/>), while his project list shows it gray. His default project is skipped, not cleared (AC-37.2). Inactive already changes nothing. What
    /// would leave the project with nobody active managing its users answers <c>lastManager</c> (BR-29); a person not of this project is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Disable(Guid projectId, Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
        {
            return Results.NotFound();
        }
        if (await ProjectManagers.IsTheLastAsync(projectId, userId, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "LastManager");
        }

        if (!await db.InactiveMembers.AnyAsync(row => row.ProjectId == projectId && row.UserId == userId))
        {
            var inactive = new InactiveMember { ProjectId = projectId, UserId = userId };
            db.InactiveMembers.Add(inactive);
            db.AddHistory(inactive, HistoryAction.Disable, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
            await db.SaveOnceAsync();
        }
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>
    /// Makes someone inactive in this project active there again (redix-api #126), with its History line: he holds what he held before. Active already changes nothing, at the
    /// same moment too (findings row 190); a person not of this project is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="userId">The person, who must be in this project.</param>
    /// <param name="principal">The session of whoever manages the project's users.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Enable(Guid projectId, Guid userId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var user = await InProjectAsync(projectId, userId, db);
        if (user == User.None)
        {
            return Results.NotFound();
        }

        await InactiveMember.EndAsync(projectId, userId, HistoryAction.Enable, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db);
        await db.SaveOnceAsync();
        return Results.Ok(await ViewInProjectAsync(user, projectId, db));
    }

    /// <summary>Gives <paramref name="given"/> unless it is held, and takes the other capital tick when it is one of them (redix-api #106); the caller saves.</summary>
    private static async Task GiveAsync(Guid projectId, Guid userId, Permission given, ClaimsPrincipal principal, RedixDbContext db, DateTime now)
    {
        var held = await db.ProjectPermissions.Where(row => row.ProjectId == projectId && row.UserId == userId).ToListAsync();
        if (held.All(row => row.Permission != given))
        {
            Add(projectId, userId, given, principal, db, now);
        }
        if (!CapitalSight.Ticks.Contains(given))
        {
            return;
        }
        foreach (var other in held.Where(row => CapitalSight.Ticks.Contains(row.Permission) && row.Permission != given))
        {
            db.ProjectPermissions.Remove(other);
            db.AddHistory(other, HistoryAction.RemoveProject, AccessTokens.UserIdOf(principal), now);
        }
        if (given == Permission.SeeAllCapital)
        {
            await CapitalSight.ClearAsync(projectId, userId, AccessTokens.UserIdOf(principal), now, db);
        }
    }

    private static void Add(Guid projectId, Guid userId, Permission permission, ClaimsPrincipal principal, RedixDbContext db, DateTime now)
    {
        var held = new ProjectPermission { ProjectId = projectId, UserId = userId, Permission = permission };
        db.ProjectPermissions.Add(held);
        db.AddHistory(held, HistoryAction.AssignProject, AccessTokens.UserIdOf(principal), now);
    }

    private static async Task<User> InProjectAsync(Guid projectId, Guid userId, RedixDbContext db) =>
        await db.Users.SingleOrDefaultAsync(user => user.Id == userId && db.ProjectPermissions.Any(held => held.ProjectId == projectId && held.UserId == userId)) ?? User.None;

    private static async Task<UserView> ViewInProjectAsync(User user, Guid projectId, RedixDbContext db)
    {
        var held = db.ProjectPermissions.Where(row => row.ProjectId == projectId && row.UserId == user.Id);
        return UserView.Of(user, (await UsersEndpoints.ProjectsByUserAsync(held, db))[user.Id].ToList());
    }
}

/// <summary>What the app sends to name what someone is called in a project.</summary>
/// <param name="RoleName">The role name; empty clears it.</param>
public record RoleNameRequest(string RoleName);

/// <summary>What the app sends to add someone who already has a login.</summary>
/// <param name="Username">Their exact username.</param>
public record AddExistingUserRequest(string Username);

/// <summary>Someone in a project, as a person choosing among them sees them.</summary>
/// <param name="Id">The user's id.</param>
/// <param name="DisplayName">The user's name.</param>
public record MemberView(Guid Id, string DisplayName);

/// <summary>What the app sends to create a user.</summary>
/// <param name="DisplayName">The name as it shows on entries.</param>
/// <param name="Username">The name the user signs in with.</param>
/// <param name="Mobile">The user's mobile; may be empty.</param>
/// <param name="TemporaryPassword">The password the user changes at first sign-in.</param>
public record CreateUserRequest(string DisplayName, string Username, string Mobile, string TemporaryPassword)
{
    /// <summary>What the user may do in the project, as ticked; <see cref="Permission.ReadProject"/> is held whatever is ticked.</summary>
    public List<Permission> Permissions { get; init; } = [];

    /// <summary>What they are called in the project; may be empty (Muteb, 9 Nov 2026).</summary>
    public string RoleName { get; init; } = "";

    /// <summary>The partners whose capital they see, kept only when <see cref="Permission.SeeChosenCapital"/> is ticked (redix-api #106); a partner not of the project answers <c>unknownPartner</c>.</summary>
    public List<Guid> PartnerIds { get; init; } = [];
}
