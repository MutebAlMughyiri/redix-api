using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Structure;

namespace Redix.Api.Projects;

/// <summary>Letting a new project in (UC-066, FR-52): anyone registers one with its project manager, and the admin approves or rejects it, which is all the admin can do (BR-29).</summary>
public static class RegistrationEndpoints
{
    /// <summary>The name of the limit on the public route, the one that writes with no session.</summary>
    public const string RegistrationLimit = nameof(RegistrationLimit);

    /// <summary>Maps <c>POST /projects/register</c>, open with no session, and for the admin <c>GET /projects/registered</c>, <c>POST /projects/{projectId}/approve</c> and <c>/reject</c>.</summary>
    public static void MapRegistration(this WebApplication app)
    {
        app.MapPost("/projects/register", Register).AllowAnonymous().RequireRateLimiting(RegistrationLimit);
        app.MapGet("/projects/registered", Waiting).RequireAuthorization(nameof(Permission.ApproveProjects));
        app.MapPost("/projects/{projectId:guid}/approve", Approve).RequireAuthorization(nameof(Permission.ApproveProjects));
        app.MapPost("/projects/{projectId:guid}/reject", Reject).RequireAuthorization(nameof(Permission.ApproveProjects));
    }

    /// <summary>
    /// Registers a project with a new login as its manager, all or nothing, waiting for the admin (AC-52.1). The password is one they typed, so it is not temporary. A field
    /// outside its limit answers its code: <c>invalidName</c>, <c>invalidUsername</c>, <c>invalidMobile</c> or <c>weakPassword</c>; a unit's name repeated, <c>unitNameTaken</c> (BR-37); a username already taken, <c>usernameTaken</c>.
    /// </summary>
    /// <param name="request">The project and its manager.</param>
    /// <param name="db">The database they join.</param>
    /// <param name="hasher">Hashes the password.</param>
    /// <param name="time">The current time, for the user and the audit records.</param>
    public static async Task<IResult> Register(RegisterProjectRequest request, RedixDbContext db, IPasswordHasher<User> hasher, TimeProvider time)
    {
        var (project, outcome) = Project.New(request.Name, request.StartDate, request.Units);
        if (outcome != ProjectChangeOutcome.Done)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, outcome.ToString());
        }

        var displayName = request.DisplayName.Trim();
        var username = request.Username.Trim();
        var mobile = request.Mobile.Trim();
        var refusal = User.RefusalOfNew(displayName, username, mobile, request.Password);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if (await db.Users.AnyAsync(user => user.Username == username))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "UsernameTaken");
        }

        var now = time.GetUtcNow().UtcDateTime;
        var manager = new User { Username = username, DisplayName = displayName, Mobile = mobile, CreatedAt = now };
        manager.SetPassword(request.Password, hasher);
        db.Users.Add(manager);
        db.Audit(manager, AuditAction.Create, manager.Id, now);
        await ProjectsEndpoints.AddWithManagerAsync(project, manager.Id, db, now);
        await db.SaveChangesAsync();
        return Results.Created($"/projects/{project.Id}", await ProjectsEndpoints.ViewOfAsync(project, manager.Id, db));
    }

    /// <summary>What is waiting for the admin, by start date, with what is needed to decide and nothing more.</summary>
    /// <param name="db">The database the projects live in.</param>
    public static async Task<List<RegisteredProjectView>> Waiting(RedixDbContext db)
    {
        var projects = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder))
            .Where(project => project.Status == ProjectStatus.PendingApproval)
            .OrderBy(project => project.StartDate).ThenBy(project => project.Name)
            .ToListAsync();
        var managers = await db.ProjectMembers
            .Where(member => db.RolePermissions.Any(held => held.RoleId == member.RoleId && held.Permission == Permission.ManageUsers)
                && db.Projects.Any(project => project.Id == member.ProjectId && project.Status == ProjectStatus.PendingApproval))
            .Join(db.Users, member => member.UserId, user => user.Id, (member, user) => new { member.ProjectId, user.DisplayName, user.Username, user.Mobile })
            .ToListAsync();
        var managerOf = managers.ToLookup(manager => manager.ProjectId);
        return projects.Select(project =>
        {
            var manager = managerOf[project.Id].First();
            return new RegisteredProjectView(project.Id, project.Name, project.StartDate, project.Units.Select(unit => unit.Name).ToList(), manager.DisplayName, manager.Username, manager.Mobile);
        }).ToList();
    }

    /// <summary>Lets a registered project in, with its structure copied, so its roles count from the next request (BR-34). A decided project answers <c>notPending</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The admin's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Approve(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        DecideAsync(projectId, principal, db, time, AuditAction.ApproveProject, project => ProjectStructure.ApproveAsync(project, AccessTokens.UserIdOf(principal), db));

    /// <summary>Refuses a registered project, saying why (AC-52.2). A decided project answers <c>notPending</c>; a reason empty or too long, <c>invalidReason</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">Why it is refused.</param>
    /// <param name="principal">The admin's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Reject(Guid projectId, RejectProjectRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        DecideAsync(projectId, principal, db, time, AuditAction.RejectProject, project => Task.FromResult(project.Reject(request.Reason.Trim())));

    private static async Task<IResult> DecideAsync(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, AuditAction action, Func<Project, Task<ProjectChangeOutcome>> decide)
    {
        var project = await db.Projects.SingleOrDefaultAsync(project => project.Id == projectId);
        if (project is null)
        {
            return Results.NotFound();
        }

        var outcome = await decide(project);
        if (outcome != ProjectChangeOutcome.Done)
        {
            var status = outcome == ProjectChangeOutcome.InvalidReason ? StatusCodes.Status400BadRequest : StatusCodes.Status409Conflict;
            return Refusal.Of(status, outcome.ToString());
        }
        db.Audit(project, action, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the sign-in page sends to register a project.</summary>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day the project started.</param>
/// <param name="Units">The names of its units, in order; may be empty.</param>
/// <param name="DisplayName">The manager's name, as it shows on entries.</param>
/// <param name="Username">The name the manager signs in with.</param>
/// <param name="Mobile">The manager's mobile; may be empty.</param>
/// <param name="Password">The password the manager chose.</param>
public record RegisterProjectRequest(string Name, DateOnly StartDate, List<string> Units, string DisplayName, string Username, string Mobile, string Password);

/// <summary>What the admin sends to refuse a project.</summary>
/// <param name="Reason">Why, which its manager sees.</param>
public record RejectProjectRequest(string Reason);

/// <summary>A registered project as the admin sees it: what is needed to decide, and nothing more.</summary>
/// <param name="Id">The project's id.</param>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day it started.</param>
/// <param name="Units">The names of its units, in order.</param>
/// <param name="ManagerName">The name of whoever registered it.</param>
/// <param name="ManagerUsername">Their username.</param>
/// <param name="ManagerMobile">Their mobile; empty when none.</param>
public record RegisteredProjectView(Guid Id, string Name, DateOnly StartDate, List<string> Units, string ManagerName, string ManagerUsername, string ManagerMobile);
