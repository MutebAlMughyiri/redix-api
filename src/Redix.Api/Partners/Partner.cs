using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>
/// A partner in one project (FR-25), whose capital is reckoned from their movements (BR-05). A partner is a table of a capital item, as a contract is of a contractor's item
/// (redix-api #109): their movements are entered by picking that table. Someone who withdrew stays, with a zero balance and their history.
/// </summary>
public class Partner : IOrdered
{
    /// <summary>No partner: what a new one is added over, whose id and item are empty.</summary>
    public static Partner None { get; } = new() { Id = Guid.Empty, Name = "" };

    /// <summary>The partner's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project they are a partner in (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The login whose capital this is, so that person sees it as their own (BR-31); <see cref="Guid.Empty"/> for a partner with no login.</summary>
    public Guid UserId { get; set; }

    /// <summary>The partner's name, as it was typed, which names their table.</summary>
    public required string Name { get; set; }

    /// <summary>The capital item whose table they are.</summary>
    public Guid ItemId { get; set; }

    /// <summary>Their table's place among the item's tables (AC-46.8).</summary>
    public int SortOrder { get; set; }
}
