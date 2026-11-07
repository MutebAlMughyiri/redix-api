namespace Redix.Api.Entries;

/// <summary>What the app sends for any expense, a purchase or a contractor payment: its total, its date, how it was paid, its file when there is one, and what its item's columns ask (redix-api #96).</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public abstract record EntryRequest(Guid Id)
{
    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>The account it was paid from.</summary>
    public Guid AccountId { get; init; }

    /// <summary>The expense's one date: the day it was paid (redix-api #96, D10).</summary>
    public DateOnly PaymentDate { get; init; }

    /// <summary>Notes on it; left out when none. A deduction says why here (D14).</summary>
    public string Notes { get; init; } = "";

    /// <summary>The values of the item's custom columns, by each column's key (FR-41).</summary>
    public Dictionary<string, string> CustomValues { get; init; } = [];

    /// <summary>The file, already kept; left out when there is none, which every expense may be (D3, D16).</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>True to send it for approval; otherwise it is saved as a draft.</summary>
    public bool Submit { get; init; }

    /// <summary>What it comes to: negative for a deduction (FR-14).</summary>
    public abstract decimal Total();

    /// <summary>Whether this is a deduction: a negative total, which comes off the item.</summary>
    public bool IsDeduction() => Total() < 0;

    /// <summary>The custom values to keep: each trimmed, and the empty ones gone.</summary>
    public Dictionary<string, string> CustomValuesKept() => CustomValues.Where(value => value.Value.Trim().Length > 0).ToDictionary(value => value.Key, value => value.Value.Trim());

    /// <summary>What this holds that an item's columns can require.</summary>
    public abstract EntryAnswers Answers();

    /// <summary>Why what every expense has cannot be kept: notes too long, or a deduction that does not say why in them (D14). Empty when it can.</summary>
    protected string RefusalOfCommon()
    {
        var notes = Notes.Trim();
        if (notes.Length > Entry.NotesMaxLength)
        {
            return "InvalidNotes";
        }
        return IsDeduction() && notes.Length == 0 ? "DeductionNeedsNotes" : "";
    }
}
