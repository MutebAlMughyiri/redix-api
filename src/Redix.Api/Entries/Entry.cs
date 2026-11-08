namespace Redix.Api.Entries;

/// <summary>One movement of money in the ledger, of any kind. Approval, audit, reversal, attachments and account balances are built once, on this. What only one kind has lives in a detail table keyed by the entry.</summary>
public class Entry
{
    /// <summary>The longest notes an entry may have.</summary>
    public const int NotesMaxLength = 1000;

    /// <summary>The <see cref="ApprovedAt"/> of an entry not approved yet.</summary>
    public static readonly DateTime NotApproved = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Saudi Arabia's time, three hours ahead of UTC all year: the one owner of the project's day and of the time a report prints.</summary>
    public static readonly TimeSpan SaudiOffset = TimeSpan.FromHours(3);

    /// <summary>The entry's id, time-ordered and made on the phone, so an entry has it before it reaches the server (AC-16.3).</summary>
    public Guid Id { get; init; }

    /// <summary>The project it belongs to (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>What it is.</summary>
    public EntryKind Kind { get; set; }

    /// <summary>Where it stands.</summary>
    public EntryStatus Status { get; set; }

    /// <summary>The entry's one date (redix-api #96, D10).</summary>
    public DateOnly EntryDate { get; set; }

    /// <summary>Its total; negative for a credit note or a deduction (FR-14).</summary>
    public decimal Amount { get; set; }

    /// <summary>The account it was paid from or into.</summary>
    public Guid AccountId { get; set; }

    /// <summary>Notes on it; empty when none.</summary>
    public string Notes { get; set; } = "";

    /// <summary>The user who entered it.</summary>
    public Guid CreatedByUserId { get; init; }

    /// <summary>When it was entered.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>When it was approved, which marks it new since the last report; <see cref="NotApproved"/> until then.</summary>
    public DateTime ApprovedAt { get; set; } = NotApproved;

    /// <summary>Whether it can be sent for approval: only a draft, or what was rejected and fixed (UC-028).</summary>
    public bool CanBeSubmitted() => Status is EntryStatus.Draft or EntryStatus.Rejected;

    /// <summary>Today's date in Saudi Arabia, where every project is, at <paramref name="time"/>: a date a person typed is theirs, not the server's.</summary>
    public static DateOnly Today(TimeProvider time) => DateOnly.FromDateTime(time.GetUtcNow().ToOffset(SaudiOffset).DateTime);

    /// <summary>Whether <paramref name="date"/> is inside the project's period and not in the future (BR-12). A project has no end date, so its period runs from <paramref name="projectStart"/> to today.</summary>
    public static bool IsAcceptableDate(DateOnly date, DateOnly projectStart, DateOnly today) => date >= projectStart && date <= today;
}
