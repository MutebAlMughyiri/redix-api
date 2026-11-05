using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>What a project's whole structure shares: its starting copy, made when the admin approves it, the guard on every route that changes it, and how those routes answer.</summary>
public static class ProjectStructure
{
    /// <summary>
    /// Approves <paramref name="project"/> and copies its structure from what its registration chose, so every approved project starts with one (FR-05, UC-006 step 3). The caller saves
    /// them together. Only the structure is copied, never entries, contracts or balances (AC-05.2).
    /// </summary>
    public static async Task<ProjectChangeOutcome> ApproveAsync(Project project, RedixDbContext db)
    {
        var outcome = project.Approve();
        if (outcome == ProjectChangeOutcome.Done)
        {
            var defaults = await db.DefaultColumns.AsNoTracking().Where(column => column.ProjectId == project.StructureSource).ToListAsync();
            var categories = await db.Categories.AsNoTracking().Where(category => category.ProjectId == project.StructureSource && category.IsActive)
                .Include(category => category.Items).ThenInclude(item => item.Subgroups)
                .Include(category => category.Items).ThenInclude(item => item.Columns)
                .AsSplitQuery().ToListAsync();
            db.DefaultColumns.AddRange(defaults.Select(column => column.CopyFor(project.Id)));
            db.Categories.AddRange(categories.Select(category => category.CopyFor(project.Id, defaults)));
        }
        return outcome;
    }

    /// <summary>The routes under a project that change its structure: they need <see cref="Permission.ManageStructure"/>, and a finished project refuses them all with <c>projectFinished</c> (AC-04.1).</summary>
    public static RouteGroupBuilder MapStructureChanges(this WebApplication app) =>
        app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ManageStructure)).AddEndpointFilter(RefuseAFinishedProjectAsync);

    /// <summary>The refusal of <paramref name="outcome"/>: a request that could never work is a bad request, anything else a conflict with what the row is.</summary>
    public static IResult Refused(StructureChangeOutcome outcome) =>
        Refusal.Of(outcome is StructureChangeOutcome.GeneralSubgroup or StructureChangeOutcome.CoreColumn ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, outcome.ToString());

    /// <summary>The item <paramref name="itemId"/> if it belongs to <paramref name="projectId"/>, so one project's route never reaches another's rows (BR-33).</summary>
    public static Task<Item?> ItemOfAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        db.Items.SingleOrDefaultAsync(item => item.Id == itemId && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId));

    /// <summary>Puts <paramref name="rows"/> in the order of <paramref name="ids"/>, the order of the report (AC-39.1). Anything but every row exactly once changes nothing, and answers false.</summary>
    public static bool PutInOrder<T>(List<T> rows, List<Guid> ids) where T : IOrdered
    {
        if (!ids.Order().SequenceEqual(rows.Select(row => row.Id).Order()))
        {
            return false;
        }
        foreach (var row in rows)
        {
            row.SortOrder = ids.IndexOf(row.Id);
        }
        return true;
    }

    private static async ValueTask<object?> RefuseAFinishedProjectAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<RedixDbContext>();
        var projectId = Guid.Parse((string)context.HttpContext.GetRouteValue("projectId")!);
        var project = await db.Projects.SingleAsync(project => project.Id == projectId);
        return project.IsReadOnly ? Refusal.Of(StatusCodes.Status409Conflict, nameof(ProjectChangeOutcome.ProjectFinished)) : await next(context);
    }
}
