using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>Shapes a project's expense structure (UC-012): its categories, their items, and the items' subgroups. What is no longer wanted is disabled (BR-15); only a subgroup no entry uses can be removed.</summary>
public static class CategoriesEndpoints
{
    /// <summary>Maps the routes that read a project's categories, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageStructure"/>.</summary>
    public static void MapCategories(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/categories").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/offered", Offered);
        app.MapGet("/projects/{projectId:guid}/items/{itemId:guid}/tables", Tables).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/entry-tables", EntryTables).RequireAuthorization(nameof(Permission.ReadProject));

        var changes = app.MapProjectChanges(Permission.ManageStructure);
        changes.MapPost("/categories", AddCategory);
        changes.MapPost("/categories/reorder", ReorderCategories);
        changes.MapPost("/categories/{categoryId:guid}", RenameCategory);
        changes.MapPost("/categories/{categoryId:guid}/disable", DisableCategory);
        changes.MapPost("/categories/{categoryId:guid}/enable", EnableCategory);
        changes.MapPost("/categories/{categoryId:guid}/items", AddItem);
        changes.MapPost("/categories/{categoryId:guid}/items/reorder", ReorderItems);
        changes.MapPost("/items/{itemId:guid}", RenameItem);
        changes.MapPost("/items/{itemId:guid}/disable", DisableItem);
        changes.MapPost("/items/{itemId:guid}/enable", EnableItem);
        changes.MapPost("/items/{itemId:guid}/subgroups", AddSubgroup);
        changes.MapPost("/items/{itemId:guid}/type", ChangeItemType);
        changes.MapPost("/items/{itemId:guid}/table-percentages", SetTablePercentages);
        changes.MapPost("/items/{itemId:guid}/tables/reorder", ReorderTables);
        changes.MapPost("/items/{itemId:guid}/subgroups/{subgroupId:guid}", RenameSubgroup);
        changes.MapPost("/items/{itemId:guid}/subgroups/{subgroupId:guid}/delete", DeleteSubgroup);
    }

    /// <summary>Every category and item of the project, disabled ones included, in the order of the periodic report (AC-39.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<List<CategoryView>> List(Guid projectId, RedixDbContext db) =>
        (await ReadAsync(projectId, db)).Select(category => CategoryView.Of(category, category.Items)).ToList();

    /// <summary>
    /// What an entry of <paramref name="itemType"/> may choose from: active items of that type in active categories (AC-05.1, AC-05.3, AC-38.1). A category with nothing to offer is left out.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c> or <c>contractorPayments</c>.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<List<CategoryView>> Offered(Guid projectId, ItemTypeInUrl itemType, RedixDbContext db) =>
        (await ReadAsync(projectId, db)).Select(category => CategoryView.Of(category, category.ItemsOfferedFor(itemType.Value))).Where(view => view.Items.Count > 0).ToList();

    /// <summary>Adds a category at the end of the project's list (FR-39). An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The category's name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> AddCategory(Guid projectId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var category = new Category { ProjectId = projectId, Name = name, SortOrder = await db.Categories.CountAsync(row => row.ProjectId == projectId) };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/projects/{projectId}/categories/{category.Id}", CategoryView.Of(category, []));
        });

    /// <summary>Puts the project's categories in the order given, which is their order in the periodic report (AC-39.1). A list that is not exactly the project's categories answers <c>invalidOrder</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">Every category's id, in the new order.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ReorderCategories(Guid projectId, ReorderRequest request, RedixDbContext db) =>
        await ReorderAsync(await db.Categories.Where(category => category.ProjectId == projectId).ToListAsync(), request, db);

    /// <summary>Renames a category. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> RenameCategory(Guid projectId, Guid categoryId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, name => ChangeCategoryAsync(projectId, categoryId, db, category => category.Name = name));

    /// <summary>Disables a category: it and its items disappear from entry and stay in the reports (BR-15).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> DisableCategory(Guid projectId, Guid categoryId, RedixDbContext db) =>
        ChangeCategoryAsync(projectId, categoryId, db, category => category.IsActive = false);

    /// <summary>Brings a disabled category back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> EnableCategory(Guid projectId, Guid categoryId, RedixDbContext db) =>
        ChangeCategoryAsync(projectId, categoryId, db, category => category.IsActive = true);

    /// <summary>Adds an item with its type at the end of a category, with its general group and the default columns of its type (FR-05, FR-38). An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category it joins.</param>
    /// <param name="request">The item's name and type.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> AddItem(Guid projectId, Guid categoryId, AddItemRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            if (!await db.Categories.AnyAsync(category => category.Id == categoryId && category.ProjectId == projectId))
            {
                return Results.NotFound();
            }

            var defaults = await db.DefaultColumns.Where(column => column.ProjectId == projectId && column.ItemType == request.Type).ToListAsync();
            var item = Item.New(categoryId, name, request.Type, await db.Items.CountAsync(row => row.CategoryId == categoryId), defaults);
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/projects/{projectId}/items/{item.Id}", ItemView.Of(item));
        });

    /// <summary>Puts a category's items in the order given, which is their order in the periodic report. A list that is not exactly the category's items answers <c>invalidOrder</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category.</param>
    /// <param name="request">Every item's id, in the new order.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ReorderItems(Guid projectId, Guid categoryId, ReorderRequest request, RedixDbContext db)
    {
        if (!await db.Categories.AnyAsync(category => category.Id == categoryId && category.ProjectId == projectId))
        {
            return Results.NotFound();
        }
        return await ReorderAsync(await db.Items.Where(item => item.CategoryId == categoryId).ToListAsync(), request, db);
    }

    /// <summary>Renames an item. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> RenameItem(Guid projectId, Guid itemId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, name => ChangeItemAsync(projectId, itemId, db, item => item.Name = name));

    /// <summary>Disables an item: it disappears from entry and stays in the reports (BR-15).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> DisableItem(Guid projectId, Guid itemId, RedixDbContext db) => ChangeItemAsync(projectId, itemId, db, item => item.IsActive = false);

    /// <summary>Brings a disabled item back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> EnableItem(Guid projectId, Guid itemId, RedixDbContext db) => ChangeItemAsync(projectId, itemId, db, item => item.IsActive = true);

    /// <summary>
    /// Changes what an item is for while no entry names it, or <c>itemHasEntries</c>, and no contract does, or <c>itemHasContract</c>; it is in the audit trail. Its built-in columns become the project's default columns for the new type, because what an entry is asked follows them (FR-38),
    /// and nothing has a value in them yet. The columns a person added stay, after them.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">What it becomes.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the structure lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> ChangeItemType(Guid projectId, Guid itemId, ChangeItemTypeRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var item = await ProjectStructure.ItemOfAsync(projectId, itemId, db);
        if (item is null)
        {
            return Results.NotFound();
        }
        if (await db.ExpenseDetails.AnyAsync(details => details.ItemId == itemId) || await db.Set<Revenue.RevenueDetails>().AnyAsync(details => details.ItemId == itemId))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.ItemHasEntries);
        }
        if (await db.Contracts.AnyAsync(contract => contract.ItemId == itemId))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.ItemHasContract);
        }

        item.Type = request.Type;
        var defaults = await db.DefaultColumns.Where(column => column.ProjectId == projectId && column.ItemType == request.Type).OrderBy(column => column.SortOrder).ToListAsync();
        var columns = await db.ItemColumns.Where(column => column.ItemId == itemId).ToListAsync();
        var added = columns.Where(column => column.IsCustom && defaults.All(row => row.Key != column.Key)).OrderBy(column => column.SortOrder).ToList();
        db.ItemColumns.RemoveRange(columns.Except(added));
        db.ItemColumns.AddRange(defaults.Select((column, place) => ItemColumn.Of(column, itemId, place)));
        foreach (var (column, place) in added.Select((column, place) => (column, place)))
        {
            column.SortOrder = defaults.Count + place;
        }
        db.Audit(item, AuditAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Sets whether the item's sheet calculates each table's percentage of اجمالي الكل (AC-46.9), in the audit trail.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">Whether it does.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the structure lives in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> SetTablePercentages(Guid projectId, Guid itemId, TablePercentagesRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var item = await ProjectStructure.ItemOfAsync(projectId, itemId, db);
        if (item is null)
        {
            return Results.NotFound();
        }
        item.ShowsTablePercentages = request.IsOn;
        db.Audit(item, AuditAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Adds a named subgroup at the end of an item's list. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">The subgroup's name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> AddSubgroup(Guid projectId, Guid itemId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            if (await ProjectStructure.ItemOfAsync(projectId, itemId, db) is null)
            {
                return Results.NotFound();
            }
            db.Subgroups.Add(new Subgroup { ItemId = itemId, Name = name, SortOrder = await ProjectStructure.NextTablePlaceAsync(itemId, db) });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>
    /// An item's tables in the order its sheet prints them (AC-46.8): its general table, named after the item, each named subgroup, and each contract's payments, named after its
    /// contractor, with how many approved entries each holds.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> Tables(Guid projectId, Guid itemId, RedixDbContext db)
    {
        var item = await ProjectStructure.ItemOfAsync(projectId, itemId, db);
        if (item is null)
        {
            return Results.NotFound();
        }
        var approved = db.Entries.Where(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.Approved);
        var purchases = await (from details in db.ExpenseDetails join entry in approved on details.EntryId equals entry.Id where details.ItemId == itemId && entry.Kind == EntryKind.Purchase select details.SubgroupId)
            .Concat(from details in db.Set<Revenue.RevenueDetails>() join entry in approved on details.EntryId equals entry.Id where details.ItemId == itemId select details.SubgroupId)
            .GroupBy(subgroupId => subgroupId).Select(group => new { group.Key, Count = group.Count() }).ToDictionaryAsync(row => row.Key, row => row.Count);
        var payments = await (from payment in db.ContractorPaymentDetails join entry in approved on payment.EntryId equals entry.Id select payment.ContractId)
            .GroupBy(contractId => contractId).Select(group => new { group.Key, Count = group.Count() }).ToDictionaryAsync(row => row.Key, row => row.Count);
        var subgroups = await db.Subgroups.Where(subgroup => subgroup.ItemId == itemId).ToListAsync();
        var contracts = await ContractTablesAsync(db.Contracts.Where(contract => contract.ItemId == itemId), db);
        return Results.Ok(TablesOf(item, subgroups, contracts)
            .Select(table => new ItemTableView(table.Id, table.Kind, table.Name, (table.Kind == ItemTableKind.Contract ? payments : purchases).GetValueOrDefault(table.Id)))
            .ToList());
    }

    /// <summary>
    /// Every table an expense may go into (redix-app #84): each active category's items that take purchases or contractor payments, each with its tables in the order its sheet
    /// prints them. An item's general and named tables show when it takes purchases, and its active contracts when it takes contractor payments. An item or a category with nothing
    /// to offer is left out, and so is every revenue item.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<List<TableCategoryView>> EntryTables(Guid projectId, RedixDbContext db)
    {
        var contracts = await ContractTablesAsync(db.Contracts.Where(contract => contract.ProjectId == projectId && contract.IsActive), db);
        return (await ReadAsync(projectId, db))
            .Select(category => new TableCategoryView(category.Id, category.Name, category.ItemsOfferedFor(ItemType.Purchases).Concat(category.ItemsOfferedFor(ItemType.ContractorPayments)).Distinct()
                .OrderBy(item => item.SortOrder)
                .Select(item => new TableItemView(item.Id, item.Name, TablesOf(
                    item,
                    item.IsOfferedFor(ItemType.Purchases) ? item.Subgroups : [],
                    item.IsOfferedFor(ItemType.ContractorPayments) ? contracts.Where(contract => contract.ItemId == item.Id) : []).ToList()))
                .Where(view => view.Tables.Count > 0)
                .ToList()))
            .Where(view => view.Items.Count > 0)
            .ToList();
    }

    /// <summary>The contracts of <paramref name="contracts"/> as tables, each named after its contractor.</summary>
    private static Task<List<ContractTable>> ContractTablesAsync(IQueryable<Contract> contracts, RedixDbContext db) =>
        (from contract in contracts join supplier in db.Suppliers on contract.SupplierId equals supplier.Id select new ContractTable(contract.Id, contract.ItemId, contract.SortOrder, supplier.Name)).ToListAsync();

    /// <summary>The tables of <paramref name="item"/> among <paramref name="subgroups"/> and <paramref name="contracts"/>, in the order its sheet prints them (AC-46.8): its general table named after the item, each named subgroup, and each contract's payments named after its contractor.</summary>
    private static IEnumerable<EntryTableView> TablesOf(Item item, IEnumerable<Subgroup> subgroups, IEnumerable<ContractTable> contracts) =>
        subgroups.Select(subgroup => (subgroup.SortOrder, Table: new EntryTableView(subgroup.Id, subgroup.Name == Subgroup.GeneralName ? ItemTableKind.General : ItemTableKind.Subgroup, subgroup.Name == Subgroup.GeneralName ? item.Name : subgroup.Name)))
            .Concat(contracts.Select(contract => (contract.SortOrder, Table: new EntryTableView(contract.Id, ItemTableKind.Contract, contract.Name))))
            .OrderBy(table => table.SortOrder)
            .Select(table => table.Table);

    /// <summary>A contract as a table: its id, its item, its place in the item's sheet and its contractor's name.</summary>
    private sealed record ContractTable(Guid Id, Guid ItemId, int SortOrder, string Name);

    /// <summary>Puts all an item's tables, its subgroups and its contracts alike, in the order given, which is the order of its sheet (AC-46.8). A list that is not exactly them answers <c>invalidOrder</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">Every table's id, in the new order.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ReorderTables(Guid projectId, Guid itemId, ReorderRequest request, RedixDbContext db)
    {
        if (await ProjectStructure.ItemOfAsync(projectId, itemId, db) is null)
        {
            return Results.NotFound();
        }
        List<IOrdered> tables = [.. await db.Subgroups.Where(subgroup => subgroup.ItemId == itemId).ToListAsync(), .. await db.Contracts.Where(contract => contract.ItemId == itemId).ToListAsync()];
        return await ReorderAsync(tables, request, db);
    }

    /// <summary>Removes a subgroup no entry uses. One an entry uses answers <c>subgroupInUse</c>; the item's general group, <c>generalSubgroup</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="subgroupId">The subgroup.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> DeleteSubgroup(Guid projectId, Guid itemId, Guid subgroupId, RedixDbContext db)
    {
        var subgroup = await db.Subgroups.SingleOrDefaultAsync(row => row.Id == subgroupId && row.ItemId == itemId);
        if (subgroup is null || await ProjectStructure.ItemOfAsync(projectId, itemId, db) is null)
        {
            return Results.NotFound();
        }
        if (subgroup.Name == Subgroup.GeneralName)
        {
            return ProjectStructure.Refused(StructureChangeOutcome.GeneralSubgroup);
        }
        if (await db.ExpenseDetails.AnyAsync(details => details.SubgroupId == subgroupId))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.SubgroupInUse);
        }
        db.Subgroups.Remove(subgroup);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Renames a subgroup. An unacceptable name answers <c>invalidName</c>; the item's general group has no name to change, <c>generalSubgroup</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="subgroupId">The subgroup.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> RenameSubgroup(Guid projectId, Guid itemId, Guid subgroupId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            var subgroup = await db.Subgroups.SingleOrDefaultAsync(row => row.Id == subgroupId && row.ItemId == itemId);
            if (subgroup is null || await ProjectStructure.ItemOfAsync(projectId, itemId, db) is null)
            {
                return Results.NotFound();
            }
            if (subgroup.Name == Subgroup.GeneralName)
            {
                return ProjectStructure.Refused(StructureChangeOutcome.GeneralSubgroup);
            }
            subgroup.Name = name;
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    private static async Task<List<Category>> ReadAsync(Guid projectId, RedixDbContext db) =>
        await db.Categories.Where(category => category.ProjectId == projectId).OrderBy(category => category.SortOrder)
            .Include(category => category.Items.OrderBy(item => item.SortOrder)).ThenInclude(item => item.Subgroups.OrderBy(subgroup => subgroup.SortOrder))
            .ToListAsync();

    private static async Task<IResult> ReorderAsync<T>(List<T> rows, ReorderRequest request, RedixDbContext db) where T : IOrdered
    {
        if (!ProjectStructure.PutInOrder(rows, request.Ids))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.InvalidOrder);
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ChangeCategoryAsync(Guid projectId, Guid categoryId, RedixDbContext db, Action<Category> change)
    {
        var category = await db.Categories.SingleOrDefaultAsync(category => category.Id == categoryId && category.ProjectId == projectId);
        if (category is null)
        {
            return Results.NotFound();
        }
        change(category);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ChangeItemAsync(Guid projectId, Guid itemId, RedixDbContext db, Action<Item> change)
    {
        var item = await ProjectStructure.ItemOfAsync(projectId, itemId, db);
        if (item is null)
        {
            return Results.NotFound();
        }
        change(item);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to name something in a project.</summary>
/// <param name="Name">The name.</param>
public record NameRequest(string Name);

/// <summary>What the app sends to add an item.</summary>
/// <param name="Name">The item's name.</param>
/// <param name="Type">What it is for.</param>
public record AddItemRequest(string Name, ItemType Type);

/// <summary>What the app sends to change what an item is for.</summary>
/// <param name="Type">What it becomes.</param>
public record ChangeItemTypeRequest(ItemType Type);

/// <summary>What the app sends to choose whether an item's sheet calculates each table's percentage.</summary>
/// <param name="IsOn">Whether it does.</param>
public record TablePercentagesRequest(bool IsOn);

/// <summary>What the app sends to put a list in a new order.</summary>
/// <param name="Ids">Every row's id, in the new order.</param>
public record ReorderRequest(List<Guid> Ids);

/// <summary>A category as the app shows it.</summary>
/// <param name="Id">The category's id.</param>
/// <param name="Name">The category's name.</param>
/// <param name="IsActive">False once disabled.</param>
/// <param name="Items">Its items, in order.</param>
public record CategoryView(Guid Id, string Name, bool IsActive, List<ItemView> Items)
{
    /// <summary>The view of <paramref name="category"/> holding <paramref name="items"/>.</summary>
    public static CategoryView Of(Category category, List<Item> items) => new(category.Id, category.Name, category.IsActive, items.Select(ItemView.Of).ToList());
}

/// <summary>An item as the app shows it.</summary>
/// <param name="Id">The item's id.</param>
/// <param name="Name">The item's name.</param>
/// <param name="Type">What it is for.</param>
/// <param name="IsActive">False once disabled.</param>
/// <param name="Subgroups">Its subgroups in order, the general one first, whose name is empty.</param>
/// <param name="ShowsTablePercentages">True when its sheet calculates each table's percentage (AC-46.9).</param>
public record ItemView(Guid Id, string Name, ItemType Type, bool IsActive, List<SubgroupView> Subgroups, bool ShowsTablePercentages)
{
    /// <summary>The view of <paramref name="item"/>.</summary>
    public static ItemView Of(Item item) => new(item.Id, item.Name, item.Type, item.IsActive, item.Subgroups.Select(subgroup => new SubgroupView(subgroup.Id, subgroup.Name)).ToList(), item.ShowsTablePercentages);
}

/// <summary>One table of an item's sheet, as the app lists them.</summary>
/// <param name="Id">The subgroup or contract it prints.</param>
/// <param name="Kind">What it prints.</param>
/// <param name="Name">The item's name for its general table, the subgroup's, or the contractor's.</param>
/// <param name="Count">How many approved entries it holds.</param>
public record ItemTableView(Guid Id, ItemTableKind Kind, string Name, int Count);

/// <summary>A category of the tables an expense may go into (redix-app #84).</summary>
/// <param name="Id">The category's id.</param>
/// <param name="Name">The category's name.</param>
/// <param name="Items">Its items that take expenses, in order.</param>
public record TableCategoryView(Guid Id, string Name, List<TableItemView> Items);

/// <summary>An item and the tables of it an expense may go into.</summary>
/// <param name="Id">The item's id.</param>
/// <param name="Name">The item's name.</param>
/// <param name="Tables">Its tables, in the order its sheet prints them.</param>
public record TableItemView(Guid Id, string Name, List<EntryTableView> Tables);

/// <summary>One table of an item's sheet: a purchase goes into its general or a named one, a contractor payment into its contract's.</summary>
/// <param name="Id">The subgroup or contract it prints.</param>
/// <param name="Kind">What it prints.</param>
/// <param name="Name">The item's name for its general table, the subgroup's, or the contractor's.</param>
public record EntryTableView(Guid Id, ItemTableKind Kind, string Name);

/// <summary>What a table of an item's sheet prints.</summary>
public enum ItemTableKind : byte
{
    /// <summary>The item's general group.</summary>
    General,

    /// <summary>A named subgroup.</summary>
    Subgroup,

    /// <summary>A contract's payments.</summary>
    Contract,
}

/// <summary>A subgroup as the app shows it.</summary>
/// <param name="Id">The subgroup's id.</param>
/// <param name="Name">The subgroup's name; empty for the item's general group.</param>
public record SubgroupView(Guid Id, string Name);
