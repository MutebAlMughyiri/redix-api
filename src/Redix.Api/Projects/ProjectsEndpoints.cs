using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>Creates projects, adds their units and closes them (UC-006, FR-04). Owners only, until project roles arrive with UC-008.</summary>
public static class ProjectsEndpoints
{
    /// <summary>Maps <c>GET /projects</c>, <c>POST /projects</c>, <c>POST /projects/{projectId}/units</c> and <c>POST /projects/{projectId}/close</c>.</summary>
    public static void MapProjects(this WebApplication app)
    {
        var projects = app.MapGroup("/projects").RequireAuthorization(SessionPolicies.Owner);
        projects.MapGet("", List);
        projects.MapPost("", Create);
        projects.MapPost("/{projectId:guid}/units", AddUnit);
        projects.MapPost("/{projectId:guid}/close", Close);
    }

    /// <summary>Every project, running ones first and then the newest, each with its units; a finished project stays listed, with its reports (AC-04.1).</summary>
    /// <param name="db">The database the projects live in.</param>
    public static async Task<List<ProjectView>> List(RedixDbContext db)
    {
        var projects = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder))
            .OrderBy(project => project.Status).ThenByDescending(project => project.StartDate)
            .ToListAsync();
        return projects.Select(ProjectView.Of).ToList();
    }

    /// <summary>
    /// Creates a running project with its units, all or nothing. An empty name, or one longer than <see cref="Project.NameMaxLength"/>, for the project or any
    /// unit, answers <c>invalidName</c> and saves nothing.
    /// </summary>
    /// <param name="request">The project's name, start date and units.</param>
    /// <param name="db">The database the project is added to.</param>
    public static async Task<IResult> Create(CreateProjectRequest request, RedixDbContext db)
    {
        var name = request.Name.Trim();
        if (!Project.IsAcceptableName(name))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, nameof(ProjectChangeOutcome.InvalidName));
        }

        var project = new Project { Name = name, StartDate = request.StartDate };
        foreach (var unitName in request.Units)
        {
            if (project.AddUnit(unitName.Trim()) != ProjectChangeOutcome.Done)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, nameof(ProjectChangeOutcome.InvalidName));
            }
        }

        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return Results.Created($"/projects/{project.Id}", ProjectView.Of(project));
    }

    /// <summary>Adds a unit at the end of the project's list. A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>.</summary>
    /// <param name="projectId">The project the unit belongs to.</param>
    /// <param name="request">The unit's name.</param>
    /// <param name="db">The database the project lives in.</param>
    public static Task<IResult> AddUnit(Guid projectId, AddUnitRequest request, RedixDbContext db) =>
        ChangeAsync(projectId, db, project => project.AddUnit(request.Name.Trim()));

    /// <summary>Closes a finished project: from then on it is read-only, and its reports stay available (AC-04.1). A finished project answers <c>projectFinished</c>.</summary>
    /// <param name="projectId">The project to close.</param>
    /// <param name="db">The database the project lives in.</param>
    public static Task<IResult> Close(Guid projectId, RedixDbContext db) => ChangeAsync(projectId, db, project => project.Close());

    private static async Task<IResult> ChangeAsync(Guid projectId, RedixDbContext db, Func<Project, ProjectChangeOutcome> change)
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
        return Results.Ok(ProjectView.Of(project));
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
/// <param name="Status"><c>running</c>, or <c>finished</c> once read-only.</param>
/// <param name="Units">The project's units, in order.</param>
public record ProjectView(Guid Id, string Name, DateOnly StartDate, ProjectStatus Status, List<UnitView> Units)
{
    /// <summary>The view of <paramref name="project"/>, with its units in order.</summary>
    public static ProjectView Of(Project project) =>
        new(project.Id, project.Name, project.StartDate, project.Status, project.Units.Select(unit => new UnitView(unit.Id, unit.Name)).ToList());
}

/// <summary>A unit as the app shows it.</summary>
/// <param name="Id">The unit's id.</param>
/// <param name="Name">The unit's name.</param>
public record UnitView(Guid Id, string Name);
