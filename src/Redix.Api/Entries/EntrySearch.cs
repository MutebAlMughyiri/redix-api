using System.Globalization;
using System.Text.Json;

namespace Redix.Api.Entries;

/// <summary>
/// What a search of a project's entries keeps (redix-app #104): every filter given must hold, and one left empty keeps everything. The app's Entries screen asks it a page
/// at a time, so no screen reads every entry. What was typed is matched as <see cref="ArabicSpelling.Holds"/> reads it, over what the entry's row says: its item, partner,
/// supplier, who entered it, its invoice's number and its amount.
/// </summary>
/// <param name="Text">What was typed; empty for any.</param>
/// <param name="Mine">True to keep only the caller's own.</param>
/// <param name="Kinds">The kinds kept, as their names travel (<c>purchase</c>, <c>contractorPayment</c>, ...); none for every kind.</param>
/// <param name="Statuses">Where the kept entries stand (<c>pendingApproval</c>, <c>approved</c>, <c>rejected</c>, <c>reversed</c>, <c>draft</c>); none for any. A reversed entry stands reversed.</param>
/// <param name="From">The first day kept; <see cref="DateOnly.MinValue"/> for no first day.</param>
/// <param name="To">The last day kept; <see cref="DateOnly.MinValue"/> for no last day.</param>
public record EntrySearch(string Text, bool Mine, string[] Kinds, string[] Statuses, DateOnly From, DateOnly To)
{
    /// <summary>The search a request asks for, its days written as ISO dates (<c>2026-11-01</c>); an empty or unreadable day sets no limit.</summary>
    /// <param name="text">What was typed.</param>
    /// <param name="mine">True for the caller's own alone.</param>
    /// <param name="kinds">The kinds kept.</param>
    /// <param name="statuses">Where the kept entries stand.</param>
    /// <param name="from">The first day kept.</param>
    /// <param name="to">The last day kept.</param>
    public static EntrySearch Of(string text, bool mine, string[] kinds, string[] statuses, string from, string to) => new(text, mine, kinds, statuses, DayOf(from), DayOf(to));

    private static DateOnly DayOf(string text) => DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : DateOnly.MinValue;

    /// <summary>True when any filter narrows the entries.</summary>
    public bool Narrows => Text.Trim().Length > 0 || Mine || Kinds.Length > 0 || Statuses.Length > 0 || From != DateOnly.MinValue || To != DateOnly.MinValue;

    /// <summary>Whether <paramref name="row"/> holds to every filter.</summary>
    /// <param name="row">An entry as its list shows it.</param>
    public bool Keeps(EntryRowView row) =>
        (!Mine || row.IsMine)
        && (Kinds.Length == 0 || Kinds.Contains(NameOf(row.Kind)))
        && (Statuses.Length == 0 || Statuses.Contains(row.IsReversed ? "reversed" : NameOf(row.Status)))
        && row.EntryDate >= From
        && (To == DateOnly.MinValue || row.EntryDate <= To)
        && ArabicSpelling.Holds(Text, row.Amount, row.ItemName, row.PartnerName, row.SupplierName, row.AuthorName, row.InvoiceNumber);

    private static string NameOf<TEnum>(TEnum value) where TEnum : struct, Enum => JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
}
