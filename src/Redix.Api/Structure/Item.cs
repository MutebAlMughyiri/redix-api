namespace Redix.Api.Structure;

/// <summary>An item (البند) under a category, such as البلاط under التشطيب, that expenses are recorded against (FR-05, FR-38). It owns the rule of when it is offered at entry.</summary>
public class Item : IOrdered
{
    /// <summary>No item: what a lookup that finds none answers, so no reader needs a null. Its empty id matches nothing.</summary>
    public static Item None { get; } = new() { Id = Guid.Empty, Name = "", IsActive = false };

    /// <summary>The item's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The category it belongs to.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>The item's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>What it is for; it changes only while no entry names it (FR-38).</summary>
    public ItemType Type { get; set; }

    /// <summary>Its place in its category, which is the order in the report.</summary>
    public int SortOrder { get; set; }

    /// <summary>False once disabled: it disappears from entry and stays in the reports (BR-15).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>True when its sheet calculates each table's percentage of اجمالي الكل (AC-46.9); off until chosen.</summary>
    public bool ShowsTablePercentages { get; set; }

    /// <summary>Its subgroups in order, the general one first.</summary>
    public List<Subgroup> Subgroups { get; init; } = [];

    /// <summary>What it asks for at entry.</summary>
    public List<ItemColumn> Columns { get; init; } = [];

    /// <summary>A new item at <paramref name="sortOrder"/> of its category, with its general group, so an entry always has a subgroup, and the default columns of its type (FR-38).</summary>
    public static Item New(Guid categoryId, string name, ItemType type, int sortOrder, List<DefaultColumn> defaults)
    {
        var item = new Item { CategoryId = categoryId, Name = name, Type = type, SortOrder = sortOrder };
        item.Subgroups.Add(new Subgroup { ItemId = item.Id, Name = Subgroup.GeneralName });
        item.Inherit(defaults);
        return item;
    }

    /// <summary>True when its subgroups are among its tables: every item but a capital one, whose tables are its partners alone (redix-api #109).</summary>
    public bool HasSubgroupTables => Type != ItemType.Capital;

    /// <summary>
    /// Whether entering <paramref name="kind"/> offers this item (AC-05.3, AC-38.1): an expense, an item of its type or of both; revenue of any kind, every revenue item but a capital one,
    /// its own type then saying what the revenue is; capital, every capital item, whose tables are its partners (redix-api #109).
    /// </summary>
    public bool IsOfferedFor(ItemType kind) =>
        IsActive && (kind.IsRevenue() ? Type.IsRevenue() && (Type == ItemType.Capital) == (kind == ItemType.Capital) : Type == ItemType.Both || Type == kind);

    /// <summary>
    /// The same item with its subgroups and the columns it still asks for, for a new category. An item of the default template holds no columns of its own, so a copy of it takes the
    /// <paramref name="defaults"/> of its type.
    /// </summary>
    public Item CopyFor(Guid categoryId, List<DefaultColumn> defaults)
    {
        var copy = new Item { CategoryId = categoryId, Name = Name, Type = Type, SortOrder = SortOrder, ShowsTablePercentages = ShowsTablePercentages };
        copy.Subgroups.AddRange(Subgroups.Select(subgroup => subgroup.CopyFor(copy.Id)));
        copy.Columns.AddRange(Columns.Where(column => !column.IsHidden).Select(column => ItemColumn.Of(column, copy.Id, column.SortOrder)));
        if (copy.Columns.Count == 0)
        {
            copy.Inherit(defaults);
        }
        return copy;
    }

    private void Inherit(List<DefaultColumn> defaults) =>
        Columns.AddRange(defaults.Where(column => column.ItemType == Type).Select(column => ItemColumn.Of(column, Id, column.SortOrder)));
}
