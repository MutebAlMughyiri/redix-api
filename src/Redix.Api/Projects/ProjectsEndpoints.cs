using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Projects;

/// <summary>Lists each user's projects (UC-008, FR-36), registers one for someone signed in (UC-066), renames them, adds, renames and deletes units and closes them (UC-006, FR-04, BR-38), sets their VAT rate (UC-014, BR-14), and keeps each user's default (UC-010, FR-37).</summary>
public static class ProjectsEndpoints
{
    /// <summary>Maps <c>GET /projects</c>, <c>POST /projects</c>, and under <c>POST /projects/{projectId}</c> the rename, <c>/units</c>, <c>/units/{unitId}</c>, <c>/units/{unitId}/delete</c>, <c>/close</c> and <c>/vat-rate</c>.</summary>
    public static void MapProjects(this WebApplication app)
    {
        app.MapGet("/projects", List);
        app.MapPost("/projects", Create).RequireAuthorization(SessionPolicies.Working);
        app.MapPost("/projects/{projectId:guid}", Rename).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/units", AddUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/units/{unitId:guid}", RenameUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/units/{unitId:guid}/delete", DeleteUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/close", Close).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/vat-rate", SetVatRate).RequireAuthorization(nameof(Permission.ManageSettings));
        app.MapPost("/projects/{projectId:guid}/default", SetDefault).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapPost("/projects/default/remove", RemoveDefault).RequireAuthorization(SessionPolicies.Working);
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
    /// longer than <see cref="Project.NameMaxLength"/>, for the project or any unit, answers <c>invalidName</c> and saves nothing; a unit's name repeated, <c>unitNameTaken</c> (BR-37). An earlier project to copy the structure from must be
    /// one the caller reads (BR-33), or it answers <c>unknownSource</c>.
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
        if (request.StructureSourceId != Guid.Empty && !(await UserAccess.OfAsync(userId, db)).In(request.StructureSourceId).Contains(Permission.ReadProject))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSource");
        }

        project.StructureSourceId = request.StructureSourceId;
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

    /// <summary>Adds a unit at the end of the project's list. A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>; a name another unit of the project holds, compared trimmed, <c>unitNameTaken</c> (BR-37).</summary>
    /// <param name="projectId">The project the unit belongs to.</param>
    /// <param name="request">The unit's name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> AddUnit(Guid projectId, AddUnitRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, AuditAction.Create, project => project.AddUnit(request.Name.Trim()));

    /// <summary>Renames a project while it is open (BR-38). A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Rename(Guid projectId, NameRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, AuditAction.Update, project => project.Rename(request.Name.Trim()));

    /// <summary>Renames a unit while its project is open (BR-38). It answers as <see cref="AddUnit"/> does; the shared unit answers <c>sharedUnit</c>, and one outside the project 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="unitId">The unit.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> RenameUnit(Guid projectId, Guid unitId, NameRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, AuditAction.Update, project => project.RenameUnit(unitId, request.Name.Trim()));

    /// <summary>Deletes a unit while its project is open (BR-38). One an entry, a contract or a blueprint uses answers <c>unitInUse</c>; the shared unit, <c>sharedUnit</c>; a finished project, <c>projectFinished</c>; one outside the project 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="unitId">The unit.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> DeleteUnit(Guid projectId, Guid unitId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var isUsed = await db.ExpenseDetails.AnyAsync(details => details.UnitId == unitId) || await db.Contracts.AnyAsync(contract => contract.UnitId == unitId) || await db.Blueprints.AnyAsync(blueprint => blueprint.UnitId == unitId);
        return await ChangeAsync(projectId, principal, db, time, AuditAction.Delete, project => project.RemoveUnit(unitId, isUsed));
    }

    /// <summary>
    /// Closes a finished project: from then on it is read-only, and its reports stay available (AC-04.1). A finished project answers <c>projectFinished</c>; one with entries waiting for approval, <c>pendingEntries</c> (BR-35).
    /// A draft waits for nobody, so it holds nothing open. One whose surplus would still be shared among partners answers <c>liquidateFirst</c>: only its liquidation closes it (BR-27).
    /// </summary>
    /// <param name="projectId">The project to close.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Close(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        if ((await LiquidationEndpoints.ViewOfAsync(projectId, db)).Parts.Any(part => part.Amount != 0))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "LiquidateFirst");
        }
        var hasEntriesWaiting = await db.Entries.AnyAsync(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval);
        return await ChangeAsync(projectId, principal, db, time, AuditAction.CloseProject, project => project.Close(hasEntriesWaiting));
    }

    /// <summary>Sets the rate new entries' VAT is computed at (BR-14); saved entries keep theirs. A finished project answers <c>projectFinished</c>; a percentage outside 0 to 100, or with more than two decimals, <c>invalidVatRate</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The rate, as a percentage.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> SetVatRate(Guid projectId, SetVatRateRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, AuditAction.Update, project => project.SetVatPercent(request.VatPercent));

    /// <summary>Makes a project the caller's default, replacing any other, and says whether sign-in opens it directly (FR-37). It applies from the next sign-in (AC-37.1).</summary>
    /// <param name="projectId">The project, which the caller must be able to read.</param>
    /// <param name="request">Whether sign-in opens it directly.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the choice lives in.</param>
    public static async Task<DefaultProjectView> SetDefault(Guid projectId, SetDefaultProjectRequest request, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var chosen = await db.UserDefaultProjects.SingleOrDefaultAsync(row => row.UserId == userId);
        if (chosen is null)
        {
            db.UserDefaultProjects.Add(new UserDefaultProject { UserId = userId, ProjectId = projectId });
        }
        else
        {
            chosen.ProjectId = projectId;
        }
        (await db.Users.SingleAsync(user => user.Id == userId)).OpenDefaultProject = request.OpenDirectly;
        await db.SaveChangesAsync();
        return new DefaultProjectView(projectId.ToString(), request.OpenDirectly);
    }

    /// <summary>Clears the caller's default, which also turns "open directly" off.</summary>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the choice lives in.</param>
    public static async Task<DefaultProjectView> RemoveDefault(ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        await db.UserDefaultProjects.Where(row => row.UserId == userId).ExecuteDeleteAsync();
        (await db.Users.SingleAsync(user => user.Id == userId)).OpenDefaultProject = false;
        await db.SaveChangesAsync();
        return new DefaultProjectView("", false);
    }

    private static async Task<IResult> ChangeAsync(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, AuditAction action, Func<Project, ProjectChangeOutcome> change)
    {
        var project = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder)).SingleOrDefaultAsync(project => project.Id == projectId);
        if (project is null)
        {
            return Results.NotFound();
        }

        var outcome = change(project);
        if (outcome == ProjectChangeOutcome.UnknownUnit)
        {
            return Results.NotFound();
        }
        if (outcome != ProjectChangeOutcome.Done)
        {
            var status = outcome is ProjectChangeOutcome.InvalidName or ProjectChangeOutcome.InvalidVatRate ? StatusCodes.Status400BadRequest : StatusCodes.Status409Conflict;
            return Refusal.Of(status, outcome.ToString());
        }
        var userId = AccessTokens.UserIdOf(principal);
        db.AuditChanges(action, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(project, userId, db));
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
public record CreateProjectRequest(string Name, DateOnly StartDate, List<string> Units)
{
    /// <summary>The earlier project to copy the structure from when it is approved; left out for the default template.</summary>
    public Guid StructureSourceId { get; init; }
}

/// <summary>What the app sends to set the VAT rate.</summary>
/// <param name="VatPercent">The rate, as a percentage: 15 for 15%.</param>
public record SetVatRateRequest(decimal VatPercent);

/// <summary>What the app sends to set the default project.</summary>
/// <param name="OpenDirectly">Whether sign-in skips the list and opens it.</param>
public record SetDefaultProjectRequest(bool OpenDirectly);

/// <summary>The caller's default project, as a session says it.</summary>
/// <param name="DefaultProjectId">The project; empty when none.</param>
/// <param name="OpenDefaultProject">Whether sign-in opens it directly.</param>
public record DefaultProjectView(string DefaultProjectId, bool OpenDefaultProject);

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
/// <param name="VatPercent">The VAT rate of its new entries, as a percentage.</param>
/// <param name="EntryUnits">What an entry or a contract may name: the project's units in order, and the shared one last.</param>
public record ProjectView(Guid Id, string Name, DateOnly StartDate, ProjectStatus Status, string RejectionReason, List<UnitView> Units, List<string> Roles, List<Permission> Permissions, decimal VatPercent, List<UnitView> EntryUnits)
{
    /// <summary>The view of <paramref name="project"/>, with its units in order and the caller's <paramref name="roles"/> and <paramref name="permissions"/> in it.</summary>
    public static ProjectView Of(Project project, List<string> roles, List<Permission> permissions) =>
        new(project.Id, project.Name, project.StartDate, project.Status, project.RejectionReason, project.Units.Where(unit => !unit.IsShared).Select(UnitView.Of).ToList(), roles, permissions, project.VatRate * 100, project.Units.OrderBy(unit => unit.IsShared).Select(UnitView.Of).ToList());
}

/// <summary>A unit as the app shows it.</summary>
/// <param name="Id">The unit's id.</param>
/// <param name="Name">The unit's name.</param>
public record UnitView(Guid Id, string Name)
{
    /// <summary>The view of <paramref name="unit"/>.</summary>
    public static UnitView Of(Unit unit) => new(unit.Id, unit.Name);
}
