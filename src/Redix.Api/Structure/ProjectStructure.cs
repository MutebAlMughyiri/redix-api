using Microsoft.EntityFrameworkCore;
using Redix.Api.Projects;
using Redix.Api.Suppliers;

namespace Redix.Api.Structure;

/// <summary>What a project's whole structure shares: its starting copy, made when the admin approves it, and how the routes that change it answer.</summary>
public static class ProjectStructure
{
    /// <summary>
    /// Approves <paramref name="project"/> and copies its structure from what its registration chose, so every approved project starts with one (FR-05, UC-006 step 3). The caller saves
    /// them together. Only the structure is copied, never entries, contracts or balances (AC-05.2). Its payment methods always come from the default template: what registration chooses names categories and items.
    /// Its register starts with the general cash supplier, added in the name of <paramref name="approverId"/> (UC-017), and its units with the shared one (BRS open question 3).
    /// </summary>
    public static async Task<ProjectChangeOutcome> ApproveAsync(Project project, Guid approverId, RedixDbContext db)
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
            var paymentMethods = await db.PaymentMethods.AsNoTracking().Where(method => method.ProjectId == StructureSeed.TemplateId).ToListAsync();
            db.PaymentMethods.AddRange(paymentMethods.Select(method => method.CopyFor(project.Id)));
            db.Suppliers.Add(Supplier.GeneralCashSupplier(project.Id, approverId));
            project.AddSharedUnit();
        }
        return outcome;
    }

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
}
