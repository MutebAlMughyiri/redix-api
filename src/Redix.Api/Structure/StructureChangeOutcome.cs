namespace Redix.Api.Structure;

/// <summary>What became of a change asked of a project's structure; every outcome but <see cref="Done"/> is a refusal whose name is the answer's code.</summary>
public enum StructureChangeOutcome
{
    /// <summary>The change is made.</summary>
    Done,

    /// <summary>The name is empty or too long.</summary>
    InvalidName,

    /// <summary>The new order does not name every row of the list exactly once.</summary>
    InvalidOrder,

    /// <summary>The item's general group is never renamed or removed, so an entry always has a subgroup (UC-012).</summary>
    GeneralSubgroup,

    /// <summary>An entry uses the subgroup, so it stays (FR-05).</summary>
    SubgroupInUse,

    /// <summary>An entry names the item, so its type stays (FR-38).</summary>
    ItemHasEntries,

    /// <summary>A contract names the item, so its type stays, or that contract's payments would be refused (FR-38).</summary>
    ItemHasContract,

    /// <summary>Partners are the item's tables, so it stays a capital item (redix-api #109).</summary>
    ItemHasPartners,

    /// <summary>
    /// A project has one capital item, so its total is the whole capital that every reader sees (BR-31) and no second item shows one partner's balance; until a table can be hidden from
    /// someone (redix-api #106).
    /// </summary>
    CapitalItemExists,

    /// <summary>A person cannot choose that type for a column (FR-41).</summary>
    InvalidDataType,

    /// <summary>A list has no choice, or a choice is too long.</summary>
    InvalidOptions,

    /// <summary>A core column is renamed and reordered, and nothing else (BR-20).</summary>
    CoreColumn,

    /// <summary>A table has at most one list whose choices give the amount a sign, so the amount's direction has one answer (redix-api #129).</summary>
    SignedListExists,

    /// <summary>A person adds no column to a revenue item: it asks only what its template gives it (FR-38, redix-api #129).</summary>
    RevenueTakesNoColumns,

    /// <summary>A sub-table holds purchases, so an item that takes none has no sub-table (Muteb, 7 Nov 2026).</summary>
    ItemTakesNoPurchases,
}
