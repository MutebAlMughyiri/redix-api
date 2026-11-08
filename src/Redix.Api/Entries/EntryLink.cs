namespace Redix.Api.Entries;

/// <summary>One entry's tie to another, of one type: a credit note to the bill it returns, or a reversal to the entry it reverses.</summary>
public class EntryLink
{
    /// <summary>The entry the link is from.</summary>
    public Guid EntryId { get; init; }

    /// <summary>What the link says.</summary>
    public EntryLinkType Type { get; init; }

    /// <summary>The entry it is tied to.</summary>
    public Guid LinkedEntryId { get; init; }

    /// <summary>Why, where the type asks for it; empty otherwise.</summary>
    public string Reason { get; init; } = "";
}
