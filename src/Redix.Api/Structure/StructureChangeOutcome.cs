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

    /// <summary>A person cannot choose that type for a column (FR-41).</summary>
    InvalidDataType,

    /// <summary>A list has no choice, or a choice is too long.</summary>
    InvalidOptions,

    /// <summary>A core column is renamed and reordered, and nothing else (BR-20).</summary>
    CoreColumn,

    /// <summary>A revenue item asks no columns (FR-38).</summary>
    RevenueTakesNoColumns,
}
