namespace Redix.Api.Structure;

/// <summary>A group inside an item, such as مواد under البلاط, printed as its own table in the report (FR-05). Every item has a general one, so an entry always has a subgroup.</summary>
public class Subgroup : IOrdered
{
    /// <summary>The name of an item's general group.</summary>
    public const string GeneralName = "";

    /// <summary>The subgroup's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The item it belongs to.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The subgroup's name, as it was typed; <see cref="GeneralName"/> for the item's general group.</summary>
    public required string Name { get; set; }

    /// <summary>Its place in its item's list, the general group first.</summary>
    public int SortOrder { get; set; }

    /// <summary>The same subgroup for a new item.</summary>
    public Subgroup CopyFor(Guid itemId) => new() { ItemId = itemId, Name = Name, SortOrder = SortOrder };
}
