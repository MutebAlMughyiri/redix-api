using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>
/// Changes the fields an item asks for without a code change (UC-013, FR-41): on one item, or on the project's default template for one type of item. A change to the template reaches
/// existing items only when it says <c>applyToAllItems</c> (BR-21).
/// </summary>
public static class ColumnsEndpoints
{
    /// <summary>Maps the routes that read columns, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageStructure"/>.</summary>
    public static void MapColumns(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/items/{itemId:guid}/columns", ListOfItem);
        reads.MapGet("/default-columns/{itemType}", ListOfTemplate);

        var changes = app.MapProjectChanges(Permission.ManageStructure);
        changes.MapPost("/items/{itemId:guid}/columns", AddToItem);
        changes.MapPost("/items/{itemId:guid}/columns/reorder", ReorderOfItem);
        changes.MapPut("/items/{itemId:guid}/columns/{columnId:guid}", EditOfItem);
        changes.MapDelete("/items/{itemId:guid}/columns/{columnId:guid}", DeleteOfItem);
        changes.MapPost("/default-columns/{itemType}", AddToTemplate);
        changes.MapPost("/default-columns/{itemType}/reorder", ReorderOfTemplate);
        changes.MapPut("/default-columns/{itemType}/{columnId:guid}", EditOfTemplate);
        changes.MapDelete("/default-columns/{itemType}/{columnId:guid}", DeleteOfTemplate);
    }

    /// <summary>The columns an item asks for, in order; a deleted one is not among them.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ListOfItem(Guid projectId, Guid itemId, RedixDbContext db)
    {
        if (!await ProjectStructure.HasItemAsync(projectId, itemId, db))
        {
            return Results.NotFound();
        }
        return Results.Ok(await ViewsOfItemAsync(itemId, db));
    }

    /// <summary>The columns an entry on the item <paramref name="itemId"/> asks for, in order.</summary>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the columns live in.</param>
    public static async Task<List<ColumnView>> ViewsOfItemAsync(Guid itemId, RedixDbContext db)
    {
        var columns = await db.ItemColumns.Where(column => column.ItemId == itemId && !column.IsHidden).OrderBy(column => column.SortOrder).ToListAsync();
        return columns.Select(ColumnView.Of).ToList();
    }

    /// <summary>The project's default template for one type of item, in order.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c>, <c>contractorPayments</c> or <c>both</c>.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<List<ColumnView>> ListOfTemplate(Guid projectId, ItemTypeInUrl itemType, RedixDbContext db)
    {
        var columns = await TemplateOf(projectId, itemType, db).OrderBy(column => column.SortOrder).ToListAsync();
        return columns.Select(ColumnView.Of).ToList();
    }

    /// <summary>
    /// Adds a column to one item, at the end (AC-41.1). A name out of its limits answers <c>invalidName</c>; a type a person cannot choose, <c>invalidDataType</c>; a list with no choice, <c>invalidOptions</c>;
    /// a list whose choices give a sign, on an item that has one already, <c>signedListExists</c> (redix-api #129).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">The column.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> AddToItem(Guid projectId, Guid itemId, AddColumnRequest request, RedixDbContext db)
    {
        var outcome = Column.CheckNew(request.Name, request.DataType, request.Choices);
        if (outcome != StructureChangeOutcome.Done)
        {
            return ProjectStructure.Refused(outcome);
        }
        var item = await ProjectStructure.ItemOfAsync(projectId, itemId, db);
        if (item == Item.None)
        {
            return Results.NotFound();
        }
        if (item.Type.IsRevenue())
        {
            return ProjectStructure.Refused(StructureChangeOutcome.RevenueTakesNoColumns);
        }

        var sortOrder = await db.ItemColumns.CountAsync(column => column.ItemId == itemId);
        var added = ItemColumn.Of(DefaultColumn.New(projectId, item.Type, request, sortOrder), itemId, sortOrder);
        if (HasTwoSignedLists([.. await VisibleColumnsOf(itemId, db).ToListAsync(), added]))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
        }
        db.ItemColumns.Add(added);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Puts an item's columns in the order given, a core one too (BR-20). A list that is not exactly its columns answers <c>invalidOrder</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">Every column's id, in the new order.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ReorderOfItem(Guid projectId, Guid itemId, ReorderRequest request, RedixDbContext db)
    {
        if (!await ProjectStructure.HasItemAsync(projectId, itemId, db))
        {
            return Results.NotFound();
        }
        var columns = await db.ItemColumns.Where(column => column.ItemId == itemId && !column.IsHidden).ToListAsync();
        if (!ProjectStructure.PutInOrder(columns, request.Ids))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.InvalidOrder);
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Changes a column of one item. A name out of its limits answers <c>invalidName</c>; more than the name of a core column, <c>coreColumn</c>; a new choice too long, or for anything but a list a person added, <c>invalidOptions</c>; a sign on a second list of the item, <c>signedListExists</c> (redix-api #129). Its type is not among what can change, and a list's choices are only added to (BR-21).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="columnId">The column.</param>
    /// <param name="request">What it becomes.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> EditOfItem(Guid projectId, Guid itemId, Guid columnId, EditColumnRequest request, RedixDbContext db) =>
        ChangeOfItemAsync(projectId, itemId, columnId, db, column => column.Edit(request.Name, request.IsRequired, request.ShowInReports, request.NewChoices));

    /// <summary>Deletes a column of one item: it leaves entry, and its values stay on their entries and in the History table (AC-41.2). A core column answers <c>coreColumn</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="columnId">The column.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> DeleteOfItem(Guid projectId, Guid itemId, Guid columnId, RedixDbContext db) =>
        ChangeOfItemAsync(projectId, itemId, columnId, db, column => column.Hide());

    /// <summary>Adds a column to the default template, so new items of this type start with it, and to every existing one only with <c>applyToAllItems</c> (AC-41.3). Refused as on an item.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c>, <c>contractorPayments</c> or <c>both</c>.</param>
    /// <param name="request">The column.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> AddToTemplate(Guid projectId, ItemTypeInUrl itemType, AddDefaultColumnRequest request, RedixDbContext db)
    {
        var outcome = itemType.Value.IsRevenue() ? StructureChangeOutcome.RevenueTakesNoColumns : Column.CheckNew(request.Name, request.DataType, request.Choices);
        if (outcome != StructureChangeOutcome.Done)
        {
            return ProjectStructure.Refused(outcome);
        }

        var template = await TemplateOf(projectId, itemType, db).ToListAsync();
        var column = DefaultColumn.New(projectId, itemType.Value, request, template.Count);
        if (HasTwoSignedLists([.. template, column]))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
        }
        db.DefaultColumns.Add(column);
        if (request.ApplyToAllItems)
        {
            var items = await ItemsOf(projectId, itemType, db).Select(item => new { item.Id, Count = db.ItemColumns.Count(row => row.ItemId == item.Id) }).ToListAsync();
            List<ItemColumn> added = [.. items.Select(item => ItemColumn.Of(column, item.Id, item.Count))];
            var columns = await ColumnsOfItemsOf(projectId, itemType, db).ToListAsync();
            if (added.Any(addedColumn => HasTwoSignedLists([.. columns.Where(row => row.ItemId == addedColumn.ItemId), addedColumn])))
            {
                return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
            }
            db.ItemColumns.AddRange(added);
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>
    /// Puts the default template in the order given. With <c>applyToAllItems</c>, every existing item of this type follows it, with what only that item has after the rest (AC-41.3).
    /// A list that is not exactly its columns answers <c>invalidOrder</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c>, <c>contractorPayments</c> or <c>both</c>.</param>
    /// <param name="request">Every column's id, in the new order.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> ReorderOfTemplate(Guid projectId, ItemTypeInUrl itemType, ReorderDefaultColumnsRequest request, RedixDbContext db)
    {
        var template = await TemplateOf(projectId, itemType, db).ToListAsync();
        if (!ProjectStructure.PutInOrder(template, request.Ids))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.InvalidOrder);
        }

        if (request.ApplyToAllItems)
        {
            var placeOf = template.ToDictionary(column => column.Key, column => column.SortOrder);
            var columns = await ColumnsOfItemsOf(projectId, itemType, db).ToListAsync();
            foreach (var ofOneItem in columns.GroupBy(column => column.ItemId))
            {
                var ordered = ofOneItem.OrderBy(column => placeOf.GetValueOrDefault(column.Key, int.MaxValue)).ThenBy(column => column.SortOrder);
                foreach (var (column, place) in ordered.Select((column, place) => (column, place)))
                {
                    column.SortOrder = place;
                }
            }
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Changes a column of the default template, and with <c>applyToAllItems</c> the same column on every existing item of this type (AC-41.3). Refused as on an item.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c>, <c>contractorPayments</c> or <c>both</c>.</param>
    /// <param name="columnId">The column.</param>
    /// <param name="request">What it becomes.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> EditOfTemplate(Guid projectId, ItemTypeInUrl itemType, Guid columnId, EditDefaultColumnRequest request, RedixDbContext db)
    {
        var column = await TemplateOf(projectId, itemType, db).SingleOrDefaultAsync(column => column.Id == columnId);
        if (column is null)
        {
            return Results.NotFound();
        }
        var outcome = column.Edit(request.Name, request.IsRequired, request.ShowInReports, request.NewChoices);
        if (outcome != StructureChangeOutcome.Done)
        {
            return ProjectStructure.Refused(outcome);
        }
        if (HasTwoSignedLists(await TemplateOf(projectId, itemType, db).ToListAsync()))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
        }

        if (request.ApplyToAllItems)
        {
            var columns = await ColumnsOfItemsOf(projectId, itemType, db).ToListAsync();
            foreach (var ofItem in columns.Where(row => row.Key == column.Key))
            {
                ofItem.Edit(request.Name, request.IsRequired, request.ShowInReports, request.NewChoices);
            }
            if (columns.GroupBy(row => row.ItemId).Any(HasTwoSignedLists))
            {
                return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
            }
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>
    /// Deletes a column of the default template, so new items no longer start with it, and with <c>applyToAllItems</c> hides it on every existing item of this type (AC-41.3).
    /// A core column answers <c>coreColumn</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemType"><c>purchases</c>, <c>contractorPayments</c> or <c>both</c>.</param>
    /// <param name="columnId">The column.</param>
    /// <param name="applyToAllItems">Whether existing items follow, sent in the query since a delete has no body (redix-api #98).</param>
    /// <param name="db">The database the structure lives in.</param>
    public static async Task<IResult> DeleteOfTemplate(Guid projectId, ItemTypeInUrl itemType, Guid columnId, bool applyToAllItems, RedixDbContext db)
    {
        var column = await TemplateOf(projectId, itemType, db).SingleOrDefaultAsync(column => column.Id == columnId);
        if (column is null)
        {
            return Results.NotFound();
        }
        if (column.IsCore)
        {
            return ProjectStructure.Refused(StructureChangeOutcome.CoreColumn);
        }

        db.DefaultColumns.Remove(column);
        if (applyToAllItems)
        {
            foreach (var ofItem in await ColumnsOfItemsOf(projectId, itemType, db).Where(row => row.Key == column.Key).ToListAsync())
            {
                ofItem.Hide();
            }
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static IQueryable<DefaultColumn> TemplateOf(Guid projectId, ItemTypeInUrl itemType, RedixDbContext db) =>
        db.DefaultColumns.Where(column => column.ProjectId == projectId && column.ItemType == itemType.Value);

    private static IQueryable<Item> ItemsOf(Guid projectId, ItemTypeInUrl itemType, RedixDbContext db) =>
        ProjectStructure.ItemsOf(projectId, db).Where(item => item.Type == itemType.Value);

    private static IQueryable<ItemColumn> ColumnsOfItemsOf(Guid projectId, ItemTypeInUrl itemType, RedixDbContext db) =>
        db.ItemColumns.Where(column => !column.IsHidden && ItemsOf(projectId, itemType, db).Any(item => item.Id == column.ItemId));

    private static async Task<IResult> ChangeOfItemAsync(Guid projectId, Guid itemId, Guid columnId, RedixDbContext db, Func<ItemColumn, StructureChangeOutcome> change)
    {
        var column = await db.ItemColumns.SingleOrDefaultAsync(column => column.Id == columnId && column.ItemId == itemId && !column.IsHidden);
        if (column is null || !await ProjectStructure.HasItemAsync(projectId, itemId, db))
        {
            return Results.NotFound();
        }
        var outcome = change(column);
        if (outcome != StructureChangeOutcome.Done)
        {
            return ProjectStructure.Refused(outcome);
        }
        if (HasTwoSignedLists(await VisibleColumnsOf(itemId, db).ToListAsync()))
        {
            return ProjectStructure.Refused(StructureChangeOutcome.SignedListExists);
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static IQueryable<ItemColumn> VisibleColumnsOf(Guid itemId, RedixDbContext db) => db.ItemColumns.Where(column => column.ItemId == itemId && !column.IsHidden);

    /// <summary>True when <paramref name="columns"/>, one table's, hold more than one list whose choices give a sign (redix-api #129).</summary>
    private static bool HasTwoSignedLists(IEnumerable<Column> columns) => columns.Count(column => column.IsSigned) > 1;
}

/// <summary>What the app sends to add a column to an item.</summary>
/// <param name="Name">The column's name.</param>
/// <param name="DataType">What it holds: <c>text</c>, <c>number</c>, <c>amount</c>, <c>date</c> or <c>list</c>.</param>
/// <param name="IsRequired">Whether entry asks for it.</param>
/// <param name="ShowInReports">Whether it prints in the reports.</param>
/// <param name="Choices">The choices of a list, each with its sign (redix-api #129); ignored for another type.</param>
public record AddColumnRequest(string Name, ColumnDataType DataType, bool IsRequired, bool ShowInReports, List<ChoiceRequest> Choices);

/// <summary>What the app sends to add a column to the default template.</summary>
/// <param name="Name">The column's name.</param>
/// <param name="DataType">What it holds: <c>text</c>, <c>number</c>, <c>amount</c>, <c>date</c> or <c>list</c>.</param>
/// <param name="IsRequired">Whether entry asks for it.</param>
/// <param name="ShowInReports">Whether it prints in the reports.</param>
/// <param name="Choices">The choices of a list, each with its sign (redix-api #129); ignored for another type.</param>
/// <param name="ApplyToAllItems">Whether every existing item of this type takes it too.</param>
public record AddDefaultColumnRequest(string Name, ColumnDataType DataType, bool IsRequired, bool ShowInReports, List<ChoiceRequest> Choices, bool ApplyToAllItems)
    : AddColumnRequest(Name, DataType, IsRequired, ShowInReports, Choices);

/// <summary>What the app sends to change a column of an item. The type is not here, because it never changes (BR-21).</summary>
/// <param name="Name">Its name.</param>
/// <param name="IsRequired">Whether entry asks for it.</param>
/// <param name="ShowInReports">Whether it prints in the reports.</param>
public record EditColumnRequest(string Name, bool IsRequired, bool ShowInReports)
{
    /// <summary>Choices a list a person added gains, each with its sign (redix-api #129); left out when none.</summary>
    public List<ChoiceRequest> NewChoices { get; init; } = [];
}

/// <summary>What the app sends to change a column of the default template.</summary>
/// <param name="Name">Its name.</param>
/// <param name="IsRequired">Whether entry asks for it.</param>
/// <param name="ShowInReports">Whether it prints in the reports.</param>
/// <param name="ApplyToAllItems">Whether every existing item of this type follows.</param>
public record EditDefaultColumnRequest(string Name, bool IsRequired, bool ShowInReports, bool ApplyToAllItems)
{
    /// <summary>Choices a list a person added gains, each with its sign (redix-api #129); left out when none.</summary>
    public List<ChoiceRequest> NewChoices { get; init; } = [];
}

/// <summary>What the app sends to put the default template in a new order.</summary>
/// <param name="Ids">Every column's id, in the new order.</param>
/// <param name="ApplyToAllItems">Whether every existing item of this type follows.</param>
public record ReorderDefaultColumnsRequest(List<Guid> Ids, bool ApplyToAllItems);

/// <summary>A column as the app shows it.</summary>
/// <param name="Id">The column's id.</param>
/// <param name="Key">What names it in an entry's values.</param>
/// <param name="Name">The column's name.</param>
/// <param name="DataType">What it holds.</param>
/// <param name="IsCore">True when it is renamed and reordered, and never deleted.</param>
/// <param name="IsCustom">True when a person added it.</param>
/// <param name="IsRequired">Whether entry asks for it.</param>
/// <param name="ShowInReports">Whether it prints in the reports.</param>
/// <param name="Choices">The choices of a list kept in its definition, each with its id and its sign (redix-api #129); empty for any other column, a register's list included.</param>
public record ColumnView(Guid Id, string Key, string Name, ColumnDataType DataType, bool IsCore, bool IsCustom, bool IsRequired, bool ShowInReports, List<ColumnChoice> Choices)
{
    /// <summary>The view of <paramref name="column"/>.</summary>
    public static ColumnView Of(Column column) =>
        new(column.Id, column.Key, column.Name, column.DataType, column.IsCore, column.IsCustom, column.IsRequired, column.ShowInReports, column.Choices);
}
