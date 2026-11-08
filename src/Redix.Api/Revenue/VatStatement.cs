using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Revenue;

/// <summary>
/// The كشف الضريبة (UC-056): the period's approved purchases, quarter by quarter (sales invoices were removed on 8 Nov 2026), each a total with its party's VAT number. REDIX keeps no VAT: the Excel works it out of each total at the configured rate (redix-api #96, D4, D19).
/// </summary>
public static class VatStatement
{
    /// <summary>The first day of the quarter <paramref name="date"/> falls in.</summary>
    public static DateOnly QuarterStartOf(DateOnly date) => new(date.Year, (date.Month - 1) / 3 * 3 + 1, 1);

    /// <summary>The first day of the quarter after the one starting on <paramref name="start"/>.</summary>
    public static DateOnly NextQuarter(DateOnly start) => start.AddMonths(3);

    /// <summary>The quarters from the one holding <paramref name="first"/> to the one holding <paramref name="last"/>, each with the approved purchases dated inside the period, oldest first.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="first">The period's first day.</param>
    /// <param name="last">The period's last day.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<VatQuarterView>> OfAsync(Guid projectId, DateOnly first, DateOnly last, RedixDbContext db)
    {
        var purchases = await db.Entries
            .Where(entry => entry.ProjectId == projectId && entry.Kind == EntryKind.Purchase && entry.Status == EntryStatus.Approved && entry.EntryDate >= first && entry.EntryDate <= last)
            .OrderBy(entry => entry.EntryDate).ThenBy(entry => entry.Id).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, purchases, db);
        var purchaseRows = purchases.Select(entry =>
        {
            var product = facts.ProductOf(entry);
            return new VatRow(entry.EntryDate, facts.SupplierNameOf(entry), facts.SupplierVatNumberOf(entry), product.Length > 0 ? product : facts.ItemNameOf(entry), entry.Amount);
        }).ToList();

        var quarters = new List<VatQuarterView>();
        for (var start = QuarterStartOf(first); start <= last; start = NextQuarter(start))
        {
            var end = NextQuarter(start).AddDays(-1);
            bool Inside(VatRow row) => row.Date >= start && row.Date <= end;
            quarters.Add(new VatQuarterView(start, end, purchaseRows.Where(Inside).ToList()));
        }
        return quarters;
    }
}

/// <summary>One quarter of the statement.</summary>
/// <param name="Start">Its first day.</param>
/// <param name="End">Its last day.</param>
/// <param name="Purchases">Its approved purchases.</param>
public record VatQuarterView(DateOnly Start, DateOnly End, List<VatRow> Purchases);

/// <summary>One purchase on the statement.</summary>
/// <param name="Date">Its date.</param>
/// <param name="Party">The supplier, or the buyer.</param>
/// <param name="VatNumber">The party's VAT number; empty when it has none, so no VAT is worked out of it.</param>
/// <param name="Description">What was bought, or the sale's invoice number.</param>
/// <param name="Total">Its total: negative for a credit note.</param>
public record VatRow(DateOnly Date, string Party, string VatNumber, string Description, decimal Total);
