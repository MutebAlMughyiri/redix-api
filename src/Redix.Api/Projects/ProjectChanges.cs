using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>What every route that changes something inside a project shares: the permission it needs, the refusal of a finished project, and the rule of a name.</summary>
public static class ProjectChanges
{
    /// <summary>The routes under a project that need <paramref name="permission"/>; a finished project refuses them all with <c>projectFinished</c> (AC-04.1), so no route can forget it.</summary>
    public static RouteGroupBuilder MapProjectChanges(this WebApplication app, Permission permission) =>
        app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(permission.ToString()).AddEndpointFilter(RefuseAFinishedProjectAsync);

    /// <summary>Makes <paramref name="change"/> with <paramref name="typed"/> trimmed; an unacceptable name answers <c>invalidName</c>.</summary>
    public static async Task<IResult> WithNameAsync(string typed, Func<string, Task<IResult>> change)
    {
        var name = typed.Trim();
        return Project.IsAcceptableName(name) ? await change(name) : Refusal.Of(StatusCodes.Status400BadRequest, nameof(ProjectChangeOutcome.InvalidName));
    }

    private static async ValueTask<object?> RefuseAFinishedProjectAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<RedixDbContext>();
        var projectId = Guid.Parse((string)context.HttpContext.GetRouteValue("projectId")!);
        var project = await db.Projects.SingleAsync(project => project.Id == projectId);
        return project.IsReadOnly ? Refusal.Of(StatusCodes.Status409Conflict, nameof(ProjectChangeOutcome.ProjectFinished)) : await next(context);
    }
}
