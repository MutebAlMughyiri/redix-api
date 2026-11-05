namespace Redix.Api.Entries;

/// <summary>One unit's part of an expense recorded against the shared unit (FR-15): half and half, or a percentage each, adding up to 100.</summary>
public class UnitShare
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The unit, one of the project's own.</summary>
    public Guid UnitId { get; init; }

    /// <summary>Its part, as a percentage with at most two decimals.</summary>
    public decimal Percent { get; init; }
}

/// <summary>A unit's part of a shared expense, as the app sends it.</summary>
/// <param name="UnitId">The unit.</param>
/// <param name="Percent">Its part, as a percentage.</param>
public record UnitShareRequest(Guid UnitId, decimal Percent);
