using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>Lists each user's projects (UC-008, FR-36), registers one for someone signed in (UC-066), adds units and closes them (UC-006, FR-04).</summary>
public static class ProjectsEndpoints
{
    /// <summary>Maps <c>GET /projects</c>, <c>POST /projects</c>, <c>POST /projects/{projectId}/units</c> and <c>POST /projects/{projectId}/close</c>.</summary>
    public static void MapProjects(this WebApplication app)
    {
        app.MapGet("/projects", List);
        app.MapPost("/projects", Create).RequireAuthorization(SessionPolicies.Working);
        app.MapPost("/projects/{projectId:guid}/units", AddUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/close", Close).RequireAuthorization(nameof(Permission.ManageProject));
    }

    /// <summary>
    /// The projects the caller may read, and those they registered that are not approved, running ones first and then the newest (AC-36.1), each with their roles and
    /// what they may do in it (AC-36.2). A finished project stays listed, with its reports (AC-04.1).
    /// </summary>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the projects live in.</param>
    public static async Task<List<ProjectView>> List(ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var access = await UserAccess.OfAsync(userId, db);
        var roles = await RolesByProjectAsync(userId, db);
        var projects = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder))
            .OrderBy(project => project.Status).ThenByDescending(project => project.StartDate)
            .ToListAsync();
        return projects.Where(project => access.In(project.Id).Contains(Permission.ReadProject) || (!project.IsApproved && roles[project.Id].Any()))
            .Select(project => ProjectView.Of(project, roles[project.Id].ToList(), access.In(project.Id))).ToList();
    }

    /// <summary>
    /// Registers a project with its units, all or nothing, with the caller as its first project manager, waiting for the admin (BR-29, BR-34). An empty name, or one
    /// longer than <see cref="Project.NameMaxLength"/>, for the project or any unit, answers <c>invalidName</c> and saves nothing.
    /// </summary>
    /// <param name="request">The project's name, start date and units.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project is added to.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Create(CreateProjectRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var (project, outcome) = Project.New(request.Name, request.StartDate, request.Units);
        if (outcome != ProjectChangeOutcome.Done)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, outcome.ToString());
        }

        var userId = AccessTokens.UserIdOf(principal);
        await AddWithManagerAsync(project, userId, db, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Created($"/projects/{project.Id}", await ViewOfAsync(project, userId, db));
    }

    /// <summary>Adds a registered <paramref name="project"/> with <paramref name="managerId"/> as its first manager, and their audit records; the caller saves them together.</summary>
    public static async Task AddWithManagerAsync(Project project, Guid managerId, RedixDbContext db, DateTime now)
    {
        var manager = new ProjectMember { ProjectId = project.Id, UserId = managerId, RoleId = await ProjectManagers.RoleIdAsync(db) };
        db.Projects.Add(project);
        db.ProjectMembers.Add(manager);
        db.Audit(project, AuditAction.RegisterProject, managerId, now);
        db.Audit(manager, AuditAction.AssignProject, managerId, now);
    }

    /// <summary>Adds a unit at the end of the project's list. A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>.</summary>
    /// <param name="projectId">The project the unit belongs to.</param>
    /// <param name="request">The unit's name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    public static Task<IResult> AddUnit(Guid projectId, AddUnitRequest request, ClaimsPrincipal principal, RedixDbContext db) =>
        ChangeAsync(projectId, principal, db, project => project.AddUnit(request.Name.Trim()));

    /// <summary>Closes a finished project: from then on it is read-only, and its reports stay available (AC-04.1). A finished project answers <c>projectFinished</c>.</summary>
    /// <param name="projectId">The project to close.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    public static Task<IResult> Close(Guid projectId, ClaimsPrincipal principal, RedixDbContext db) => ChangeAsync(projectId, principal, db, project => project.Close());

    private static async Task<IResult> ChangeAsync(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, Func<Project, ProjectChangeOutcome> change)
    {
        var project = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder)).SingleOrDefaultAsync(project => project.Id == projectId);
        if (project is null)
        {
            return Results.NotFound();
        }

        var outcome = change(project);
        if (outcome != ProjectChangeOutcome.Done)
        {
            var status = outcome == ProjectChangeOutcome.InvalidName ? StatusCodes.Status400BadRequest : StatusCodes.Status409Conflict;
            return Refusal.Of(status, outcome.ToString());
        }
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(project, AccessTokens.UserIdOf(principal), db));
    }

    private static async Task<ILookup<Guid, string>> RolesByProjectAsync(Guid userId, RedixDbContext db)
    {
        var rows = await db.ProjectMembers.Where(member => member.UserId == userId)
            .Join(db.Roles, member => member.RoleId, role => role.Id, (member, role) => new { member.ProjectId, role.WordKey, role.SortOrder })
            .OrderBy(row => row.SortOrder).ToListAsync();
        return rows.ToLookup(row => row.ProjectId, row => row.WordKey);
    }

    /// <summary>The view of <paramref name="project"/> for <paramref name="userId"/>.</summary>
    public static async Task<ProjectView> ViewOfAsync(Project project, Guid userId, RedixDbContext db)
    {
        var access = await UserAccess.OfAsync(userId, db);
        var roles = await RolesByProjectAsync(userId, db);
        return ProjectView.Of(project, roles[project.Id].ToList(), access.In(project.Id));
    }
}

/// <summary>What the app sends to create a project.</summary>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day the project started.</param>
/// <param name="Units">The names of the project's units, in order; may be empty.</param>
public record CreateProjectRequest(string Name, DateOnly StartDate, List<string> Units);

/// <summary>What the app sends to add a unit.</summary>
/// <param name="Name">The unit's name.</param>
public record AddUnitRequest(string Name);

/// <summary>A project as the app shows it.</summary>
/// <param name="Id">The project's id.</param>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day the project started.</param>
/// <param name="Status"><c>pendingApproval</c>, <c>rejected</c>, <c>running</c>, or <c>finished</c> once read-only.</param>
/// <param name="RejectionReason">Why the admin refused it; empty otherwise.</param>
/// <param name="Units">The project's units, in order.</param>
/// <param name="Roles">The keys of the words naming the caller's roles in this project.</param>
/// <param name="Permissions">What the caller may do in this project; the app shows what they allow.</param>
public record ProjectView(Guid Id, string Name, DateOnly StartDate, ProjectStatus Status, string RejectionReason, List<UnitView> Units, List<string> Roles, List<Permission> Permissions)
{
    /// <summary>The view of <paramref name="project"/>, with its units in order and the caller's <paramref name="roles"/> and <paramref name="permissions"/> in it.</summary>
    public static ProjectView Of(Project project, List<string> roles, List<Permission> permissions) =>
        new(project.Id, project.Name, project.StartDate, project.Status, project.RejectionReason, project.Units.Select(unit => new UnitView(unit.Id, unit.Name)).ToList(), roles, permissions);
}

/// <summary>A unit as the app shows it.</summary>
/// <param name="Id">The unit's id.</param>
/// <param name="Name">The unit's name.</param>
public record UnitView(Guid Id, string Name);
