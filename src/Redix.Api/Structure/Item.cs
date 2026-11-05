namespace Redix.Api.Structure;

/// <summary>An item (البند) under a category, such as البلاط under التشطيب, that expenses are recorded against (FR-05, FR-38). It owns the rule of when it is offered at entry.</summary>
public class Item : IOrdered
{
    /// <summary>The item's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The category it belongs to.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>The item's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>What it is for; set when it is added, and never changed.</summary>
    public ItemType Type { get; init; }

    /// <summary>Its place in its category, which is the order in the report.</summary>
    public int SortOrder { get; set; }

    /// <summary>False once disabled: it disappears from entry and stays in the reports (BR-15).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Its subgroups in order, the general one first.</summary>
    public List<Subgroup> Subgroups { get; init; } = [];

    /// <summary>A new item at <paramref name="sortOrder"/> of its category, with its general group, so an entry always has a subgroup.</summary>
    public static Item New(Guid categoryId, string name, ItemType type, int sortOrder)
    {
        var item = new Item { CategoryId = categoryId, Name = name, Type = type, SortOrder = sortOrder };
        item.Subgroups.Add(new Subgroup { ItemId = item.Id, Name = Subgroup.GeneralName });
        return item;
    }

    /// <summary>Whether entering <paramref name="kind"/> offers this item (AC-05.3, AC-38.1).</summary>
    public bool IsOfferedFor(ItemType kind) => IsActive && (Type == ItemType.Both || Type == kind);

    /// <summary>The same item with its subgroups, for a new category.</summary>
    public Item CopyFor(Guid categoryId)
    {
        var copy = new Item { CategoryId = categoryId, Name = Name, Type = Type, SortOrder = SortOrder };
        copy.Subgroups.AddRange(Subgroups.Select(subgroup => subgroup.CopyFor(copy.Id)));
        return copy;
    }
}
