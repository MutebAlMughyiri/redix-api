using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>Shapes a project's expense structure (UC-012): its categories, their items, and the items' subgroups. There is no delete: what is no longer wanted is disabled (BR-15).</summary>
public static class CategoriesEndpoints
{
    /// <summary>Maps the routes that read a project's categories, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageStructure"/>.</summary>
    public static void MapCategories(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/categories").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/offered", Offered);

        var changes = app.MapStructureChanges();
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
        changes.MapPost("/items/{itemId:guid}/subgroups/{subgroupId:guid}", RenameSubgroup);
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
        WithNameAsync(request.Name, async name =>
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
        WithNameAsync(request.Name, name => ChangeCategoryAsync(projectId, categoryId, db, category => category.Name = name));

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

    /// <summary>Adds an item with its type at the end of a category, with its general group (FR-05, FR-38). An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="categoryId">The category it joins.</param>
    /// <param name="request">The item's name and type.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> AddItem(Guid projectId, Guid categoryId, AddItemRequest request, RedixDbContext db) =>
        WithNameAsync(request.Name, async name =>
        {
            if (!await db.Categories.AnyAsync(category => category.Id == categoryId && category.ProjectId == projectId))
            {
                return Results.NotFound();
            }

            var item = Item.New(categoryId, name, request.Type, await db.Items.CountAsync(row => row.CategoryId == categoryId));
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
        WithNameAsync(request.Name, name => ChangeItemAsync(projectId, itemId, db, item => item.Name = name));

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

    /// <summary>Adds a named subgroup at the end of an item's list. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="request">The subgroup's name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> AddSubgroup(Guid projectId, Guid itemId, NameRequest request, RedixDbContext db) =>
        WithNameAsync(request.Name, async name =>
        {
            if (await ItemOfAsync(projectId, itemId, db) is null)
            {
                return Results.NotFound();
            }
            db.Subgroups.Add(new Subgroup { ItemId = itemId, Name = name, SortOrder = await db.Subgroups.CountAsync(subgroup => subgroup.ItemId == itemId) });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>Renames a subgroup. An unacceptable name answers <c>invalidName</c>; the item's general group has no name to change, <c>generalSubgroup</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="subgroupId">The subgroup.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="db">The database the structure lives in.</param>
    public static Task<IResult> RenameSubgroup(Guid projectId, Guid itemId, Guid subgroupId, NameRequest request, RedixDbContext db) =>
        WithNameAsync(request.Name, async name =>
        {
            var subgroup = await db.Subgroups.SingleOrDefaultAsync(row => row.Id == subgroupId && row.ItemId == itemId);
            if (subgroup is null || await ItemOfAsync(projectId, itemId, db) is null)
            {
                return Results.NotFound();
            }
            if (subgroup.Name == Subgroup.GeneralName)
            {
                return Refused(StructureChangeOutcome.GeneralSubgroup);
            }
            subgroup.Name = name;
            await db.SaveChangesAsync();
            return Results.Ok();
        });

    /// <summary>The refusal of <paramref name="outcome"/>: a request that could never work is a bad request, anything else a conflict with what the row is.</summary>
    public static IResult Refused(StructureChangeOutcome outcome) =>
        Refusal.Of(outcome == StructureChangeOutcome.GeneralSubgroup ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, outcome.ToString());

    /// <summary>The item <paramref name="itemId"/> if it belongs to <paramref name="projectId"/>, so one project's route never reaches another's rows (BR-33).</summary>
    public static Task<Item?> ItemOfAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        db.Items.SingleOrDefaultAsync(item => item.Id == itemId && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId));

    private static async Task<List<Category>> ReadAsync(Guid projectId, RedixDbContext db) =>
        await db.Categories.Where(category => category.ProjectId == projectId).OrderBy(category => category.SortOrder)
            .Include(category => category.Items.OrderBy(item => item.SortOrder)).ThenInclude(item => item.Subgroups.OrderBy(subgroup => subgroup.SortOrder))
            .ToListAsync();

    private static async Task<IResult> ReorderAsync<T>(List<T> rows, ReorderRequest request, RedixDbContext db) where T : IOrdered
    {
        if (!request.Ids.Order().SequenceEqual(rows.Select(row => row.Id).Order()))
        {
            return Refused(StructureChangeOutcome.InvalidOrder);
        }
        foreach (var row in rows)
        {
            row.SortOrder = request.Ids.IndexOf(row.Id);
        }
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> WithNameAsync(string typed, Func<string, Task<IResult>> change)
    {
        var name = typed.Trim();
        return Project.IsAcceptableName(name) ? await change(name) : Refused(StructureChangeOutcome.InvalidName);
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
        var item = await ItemOfAsync(projectId, itemId, db);
        if (item is null)
        {
            return Results.NotFound();
        }
        change(item);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to name a category, an item or a subgroup.</summary>
/// <param name="Name">The name.</param>
public record NameRequest(string Name);

/// <summary>What the app sends to add an item.</summary>
/// <param name="Name">The item's name.</param>
/// <param name="Type">What it is for.</param>
public record AddItemRequest(string Name, ItemType Type);

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
public record ItemView(Guid Id, string Name, ItemType Type, bool IsActive, List<SubgroupView> Subgroups)
{
    /// <summary>The view of <paramref name="item"/>.</summary>
    public static ItemView Of(Item item) => new(item.Id, item.Name, item.Type, item.IsActive, item.Subgroups.Select(subgroup => new SubgroupView(subgroup.Id, subgroup.Name)).ToList());
}

/// <summary>A subgroup as the app shows it.</summary>
/// <param name="Id">The subgroup's id.</param>
/// <param name="Name">The subgroup's name; empty for the item's general group.</param>
public record SubgroupView(Guid Id, string Name);
