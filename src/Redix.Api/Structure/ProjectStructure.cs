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
    /// Its register starts with the general cash supplier, added in the name of <paramref name="approverId"/> (UC-017). The partners declared with it become tables of its first
    /// capital item, رأس المال (redix-api #109).
    /// </summary>
    public static async Task<ProjectChangeOutcome> ApproveAsync(Project project, Guid approverId, RedixDbContext db)
    {
        var outcome = project.Approve();
        if (outcome == ProjectChangeOutcome.Done)
        {
            var source = await ProjectStructureSource.OfAsync(project.Id, db);
            var defaults = await db.DefaultColumns.AsNoTracking().Where(column => column.ProjectId == source).ToListAsync();
            var categories = await db.Categories.AsNoTracking().Where(category => category.ProjectId == source && category.IsActive)
                .Include(category => category.Items).ThenInclude(item => item.Subgroups)
                .Include(category => category.Items).ThenInclude(item => item.Columns)
                .AsSplitQuery().ToListAsync();
            db.DefaultColumns.AddRange(defaults.Select(column => column.CopyFor(project.Id)));
            var copies = categories.Select(category => category.CopyFor(project.Id, defaults)).ToList();
            db.Categories.AddRange(copies);
            var capital = copies.OrderBy(category => category.SortOrder).SelectMany(category => category.Items.OrderBy(item => item.SortOrder)).FirstOrDefault(item => item.Type == ItemType.Capital, Item.None);
            foreach (var partner in await db.Partners.Where(partner => partner.ProjectId == project.Id && partner.ItemId == Guid.Empty).ToListAsync())
            {
                partner.ItemId = capital.Id;
            }
            var paymentMethods = await db.PaymentMethods.AsNoTracking().Where(method => method.ProjectId == StructureSeed.TemplateId).ToListAsync();
            db.PaymentMethods.AddRange(paymentMethods.Select(method => method.CopyFor(project.Id)));
            db.Suppliers.Add(Supplier.GeneralCashSupplier(project.Id, approverId));
        }
        return outcome;
    }

    /// <summary>The refusal of <paramref name="outcome"/>: a request that could never work is a bad request, anything else a conflict with what the row is.</summary>
    public static IResult Refused(StructureChangeOutcome outcome) =>
        Refusal.Of(outcome is StructureChangeOutcome.GeneralSubgroup or StructureChangeOutcome.SubgroupInUse or StructureChangeOutcome.ItemHasEntries or StructureChangeOutcome.ItemHasContract or StructureChangeOutcome.ItemHasPartners or StructureChangeOutcome.CoreColumn ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, outcome.ToString());

    /// <summary>True when an entry, an expense or revenue, is kept on the item <paramref name="itemId"/>.</summary>
    public static async Task<bool> ItemHasEntriesAsync(Guid itemId, RedixDbContext db) =>
        await db.ExpenseDetails.AnyAsync(details => details.ItemId == itemId) || await db.Set<Revenue.RevenueDetails>().AnyAsync(details => details.ItemId == itemId);

    /// <summary>True when an entry, an expense or revenue, is kept on the subgroup <paramref name="subgroupId"/>.</summary>
    public static async Task<bool> SubgroupHasEntriesAsync(Guid subgroupId, RedixDbContext db) =>
        await db.ExpenseDetails.AnyAsync(details => details.SubgroupId == subgroupId) || await db.Set<Revenue.RevenueDetails>().AnyAsync(details => details.SubgroupId == subgroupId);

    /// <summary>The items of <paramref name="projectId"/>: the one answer to whether an item belongs to a project, so one project's route never reaches another's rows (BR-33).</summary>
    public static IQueryable<Item> ItemsOf(Guid projectId, RedixDbContext db) =>
        db.Items.Where(item => db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId));

    /// <summary>The item <paramref name="itemId"/> if it belongs to <paramref name="projectId"/>; <see cref="Item.None"/> otherwise.</summary>
    public static async Task<Item> ItemOfAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        await ItemsOf(projectId, db).SingleOrDefaultAsync(item => item.Id == itemId) ?? Item.None;

    /// <summary>The item <paramref name="itemId"/> of <paramref name="projectId"/> with its subgroups and columns, as its report reads it (BR-33); <see cref="Item.None"/> otherwise.</summary>
    public static async Task<Item> ItemWithTablesAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        await ItemsOf(projectId, db).Include(row => row.Subgroups).Include(row => row.Columns).SingleOrDefaultAsync(row => row.Id == itemId) ?? Item.None;

    /// <summary>
    /// Why the project <paramref name="projectId"/> cannot take an item of <paramref name="type"/>: <c>capitalItemExists</c> for a second capital item (BR-31, redix-api #109); empty
    /// when it can.
    /// </summary>
    public static async Task<string> RefusalOfTypeAsync(Guid projectId, ItemType type, RedixDbContext db) =>
        type == ItemType.Capital && await ItemsOf(projectId, db).AnyAsync(item => item.Type == ItemType.Capital)
            ? nameof(StructureChangeOutcome.CapitalItemExists)
            : "";

    /// <summary>True when the item <paramref name="itemId"/> belongs to <paramref name="projectId"/> (BR-33).</summary>
    public static Task<bool> HasItemAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        ItemsOf(projectId, db).AnyAsync(item => item.Id == itemId);

    /// <summary>The item <paramref name="itemId"/> of the project with its subgroups and columns, if entering <paramref name="kind"/> offers it (AC-05.3, AC-38.1); <see cref="Item.None"/> otherwise.</summary>
    public static async Task<Item> OfferedItemAsync(Guid projectId, Guid itemId, ItemType kind, RedixDbContext db)
    {
        var category = await db.Categories.Include(row => row.Items).ThenInclude(item => item.Subgroups).Include(row => row.Items).ThenInclude(item => item.Columns).AsSplitQuery()
            .SingleOrDefaultAsync(row => row.ProjectId == projectId && row.Items.Any(item => item.Id == itemId));
        return category?.ItemsOfferedFor(kind).SingleOrDefault(item => item.Id == itemId) ?? Item.None;
    }

    /// <summary>The place after every table of the item <paramref name="itemId"/>, its subgroups, contracts and partners alike, for a table added to it (AC-46.8).</summary>
    public static async Task<int> NextTablePlaceAsync(Guid itemId, RedixDbContext db)
    {
        var subgroups = await db.Subgroups.Where(subgroup => subgroup.ItemId == itemId).Select(subgroup => subgroup.SortOrder).ToListAsync();
        var contracts = await db.Contracts.Where(contract => contract.ItemId == itemId).Select(contract => contract.SortOrder).ToListAsync();
        var partners = await db.Partners.Where(partner => partner.ItemId == itemId).Select(partner => partner.SortOrder).ToListAsync();
        return subgroups.Concat(contracts).Concat(partners).DefaultIfEmpty(-1).Max() + 1;
    }

    /// <summary>Puts <paramref name="rows"/> in the order of <paramref name="ids"/>, the order of the report (AC-39.1). Anything but every row exactly once changes nothing, and answers false.</summary>
    public static bool PutInOrder<T>(List<T> rows, List<Guid> ids) where T : class, IOrdered
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
