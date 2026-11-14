namespace Redix.Api.Revenue;

/// <summary>What only revenue has: the revenue item it came in on, whose type says what it is (FR-28, FR-38).</summary>
public class RevenueDetails : IProjectRow
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The revenue item.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The item's subgroup, its general one unless another was chosen.</summary>
    public Guid SubgroupId { get; init; }
}
