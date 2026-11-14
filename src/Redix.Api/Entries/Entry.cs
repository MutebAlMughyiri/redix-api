using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Entries;

/// <summary>One movement of money in the ledger, of any kind. Approval, history, reversal, attachments and account balances are built once, on this. What only one kind has lives in a detail table keyed by the entry.</summary>
public class Entry : IProjectRow
{
    /// <summary>The longest description an entry may have: room for what a purchase's product and notes held together before they became one (redix-api #129).</summary>
    public const int DescriptionMaxLength = 2000;

    /// <summary>No entry: what a lookup answers when there is none, and what "keep this as a new entry" passes for the entry it replaces, so no caller holds a null.</summary>
    public static Entry None { get; } = new();

    /// <summary>The entry <paramref name="entryId"/> if it belongs to <paramref name="projectId"/> (BR-33) and was not removed (redix-api #119); <see cref="None"/> otherwise.</summary>
    public static async Task<Entry> OfProjectAsync(Guid projectId, Guid entryId, RedixDbContext db) =>
        await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId && row.Status != EntryStatus.Removed) ?? None;

    /// <summary>The <see cref="ApprovedAt"/> of an entry not approved yet.</summary>
    public static readonly DateTime NotApproved = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Saudi Arabia's time, three hours ahead of UTC all year: the one owner of the project's day and of the time a report prints.</summary>
    public static readonly TimeSpan SaudiOffset = TimeSpan.FromHours(3);

    /// <summary>The entry's id, time-ordered and made on the phone, so an entry has it before it reaches the server (AC-16.3).</summary>
    public Guid Id { get; init; }

    /// <summary>The project it belongs to (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// «رقم القيد», its number in its project (FR-54): given once, by <see cref="EntryRecording.AddAsync"/>, when the server first keeps it, one more than the last its project gave; never
    /// changed and never given again (redix-api #128, D3).
    /// </summary>
    public int Number { get; set; }

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

    /// <summary>«الوصف», its one description, the first column of every table but رأس المال's (redix-api #129, Muteb 13 Nov 2026); empty when none. A deduction says why here (D14).</summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The values of its item's columns that it holds in no field of its own, a JSON object of each column's key and its value (FR-41): the same store whatever the column, a
    /// person's own, the unit price, the quantity or the product alike, so a table without one needs no code (redix-api #129, R5).
    /// </summary>
    public string ColumnValues { get; set; } = "{}";

    /// <summary>Its column values, by each column's key.</summary>
    public Dictionary<string, string> ColumnValuesByKey() => JsonSerializer.Deserialize<Dictionary<string, string>>(ColumnValues)!;

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
