namespace Redix.Api.Accounts;

/// <summary>
/// Money moved from one of a project's accounts to another (FR-23, redix-app #89): it counts at once in both balances and never in the surplus, the revenue or the expenses,
/// because it only moves the project's own money. It is not an entry, so nothing approves it; a mistake is put right with a transfer back.
/// </summary>
public class Transfer
{
    /// <summary>The longest notes it keeps, as an entry's.</summary>
    public const int NotesMaxLength = Entries.Entry.NotesMaxLength;

    /// <summary>The transfer's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project both accounts belong to.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The account the money left.</summary>
    public Guid FromAccountId { get; init; }

    /// <summary>The account the money reached.</summary>
    public Guid ToAccountId { get; init; }

    /// <summary>How much moved; always above zero.</summary>
    public decimal Amount { get; init; }

    /// <summary>The day it moved, as typed.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Notes on it; empty when none.</summary>
    public string Notes { get; init; } = "";

    /// <summary>Who moved it.</summary>
    public Guid CreatedByUserId { get; init; }

    /// <summary>When it was kept.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Why a transfer of <paramref name="amount"/> from <paramref name="from"/> to <paramref name="to"/>, with <paramref name="notes"/>, cannot be kept while <paramref name="from"/>
    /// holds <paramref name="fromHolds"/>; empty when it can. Each refusal would leave a balance wrong (R14, R26).
    /// </summary>
    public static string RefusalOf(Account from, Account to, decimal amount, string notes, decimal fromHolds)
    {
        if (from.Id == to.Id)
        {
            return "SameAccount";
        }
        if (!from.IsActive || !to.IsActive)
        {
            return "AccountDisabled";
        }
        if (amount <= 0 || !Money.IsAcceptable(amount))
        {
            return "InvalidAmount";
        }
        if (notes.Length > NotesMaxLength)
        {
            return "InvalidNotes";
        }
        return amount > fromHolds ? "InsufficientBalance" : "";
    }
}
