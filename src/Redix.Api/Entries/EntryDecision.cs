namespace Redix.Api.Entries;

/// <summary>One step of an entry through approval: who sent or decided it, when, and why it was rejected (UC-032).</summary>
public class EntryDecision
{
    /// <summary>The step's id, in the order they happened.</summary>
    public long Id { get; init; }

    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>What happened.</summary>
    public EntryDecisionAction Action { get; init; }

    /// <summary>Who did it.</summary>
    public Guid UserId { get; init; }

    /// <summary>When.</summary>
    public DateTime At { get; init; }

    /// <summary>Why it was rejected; empty otherwise.</summary>
    public string Reason { get; init; } = "";
}
