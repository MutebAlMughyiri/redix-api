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

    /// <summary>The item's general group has no name to change (UC-012).</summary>
    GeneralSubgroup,

    /// <summary>A person cannot choose that type for a column (FR-41).</summary>
    InvalidDataType,

    /// <summary>A list has no choice, or a choice is too long.</summary>
    InvalidOptions,

    /// <summary>A core column is renamed and reordered, and nothing else (BR-20).</summary>
    CoreColumn,
}
