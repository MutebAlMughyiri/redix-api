using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Structure;

/// <summary>
/// The earlier project a registration chose to copy the structure from when the project is approved (UC-012, findings row 214). A project has one at most, and having no row is
/// copying the default template.
/// </summary>
public class ProjectStructureSource
{
    /// <summary>The project registered.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The earlier project its structure is copied from.</summary>
    public Guid StructureSourceId { get; init; }

    /// <summary>The project the structure of <paramref name="projectId"/> is copied from: the one its registration chose, or the default template when it chose none (FR-05).</summary>
    public static async Task<Guid> OfAsync(Guid projectId, RedixDbContext db)
    {
        var chosen = await db.ProjectStructureSources.Where(row => row.ProjectId == projectId).Select(row => row.StructureSourceId).SingleOrDefaultAsync();
        return chosen == Guid.Empty ? StructureSeed.TemplateId : chosen;
    }
}
