using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>What a project's whole structure shares: its starting copy, made when the admin approves it, and the guard on every route that changes it.</summary>
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
            var categories = await db.Categories.AsNoTracking().Where(category => category.ProjectId == project.StructureSource && category.IsActive)
                .Include(category => category.Items).ThenInclude(item => item.Subgroups).ToListAsync();
            db.Categories.AddRange(categories.Select(category => category.CopyFor(project.Id)));
        }
        return outcome;
    }

    /// <summary>The routes under a project that change its structure: they need <see cref="Permission.ManageStructure"/>, and a finished project refuses them all with <c>projectFinished</c> (AC-04.1).</summary>
    public static RouteGroupBuilder MapStructureChanges(this WebApplication app) =>
        app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ManageStructure)).AddEndpointFilter(RefuseAFinishedProjectAsync);

    private static async ValueTask<object?> RefuseAFinishedProjectAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<RedixDbContext>();
        var projectId = Guid.Parse((string)context.HttpContext.GetRouteValue("projectId")!);
        var project = await db.Projects.SingleAsync(project => project.Id == projectId);
        return project.IsReadOnly ? Refusal.Of(StatusCodes.Status409Conflict, nameof(ProjectChangeOutcome.ProjectFinished)) : await next(context);
    }
}
