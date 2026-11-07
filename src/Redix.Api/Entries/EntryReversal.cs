using Microsoft.EntityFrameworkCore;
using Redix.Api.Partners;
using Redix.Api.Revenue;

namespace Redix.Api.Entries;

/// <summary>Reversing what is approved (UC-035, FR-50): an opposite entry of the same amount, tied to the original and approved at once, so both show in the reports and the net effect is zero. What was entered as one reverses as one, in whichever project it stands.</summary>
public static class EntryReversal
{
    private static readonly EntryLinkType[] Ties = [EntryLinkType.TransferPair, EntryLinkType.CarryPair];

    /// <summary>What was entered together with <paramref name="entryId"/>, and reverses with it.</summary>
    public static Task<List<Entry>> TiedToAsync(Guid entryId, RedixDbContext db)
    {
        var tied = db.EntryLinks.Where(link => Ties.Contains(link.Type) && link.EntryId == entryId).Select(link => link.LinkedEntryId)
            .Concat(db.EntryLinks.Where(link => Ties.Contains(link.Type) && link.LinkedEntryId == entryId).Select(link => link.EntryId));
        return db.Entries.Where(entry => tied.Contains(entry.Id)).ToListAsync();
    }

    /// <summary>Adds the reversal of <paramref name="entry"/>, with a copy of what it holds; saved with the change.</summary>
    public static async Task<Entry> AddAsync(Entry entry, string reason, Guid userId, DateTime at, DateOnly today, RedixDbContext db)
    {
        var reversal = DirectEntries.New(entry.ProjectId, entry.Kind, today, -entry.Amount, entry.AccountId, reason, userId, at);
        List<object> details = [new EntryLink { EntryId = reversal.Id, Type = EntryLinkType.Reversal, LinkedEntryId = entry.Id, Reason = reason }];
        var expense = await db.ExpenseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (expense is not null)
        {
            details.Add(new ExpenseDetails { EntryId = reversal.Id, ItemId = expense.ItemId, SubgroupId = expense.SubgroupId, PaymentMethodId = expense.PaymentMethodId, CustomValues = expense.CustomValues });
        }
        var purchase = await db.PurchaseDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (purchase is not null)
        {
            details.Add(new PurchaseDetails { EntryId = reversal.Id, SupplierId = purchase.SupplierId, Product = purchase.Product, UnitPrice = purchase.UnitPrice, Quantity = purchase.Quantity });
        }
        var payment = await db.ContractorPaymentDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (payment is not null)
        {
            details.Add(new ContractorPaymentDetails { EntryId = reversal.Id, ContractId = payment.ContractId, IsOutsideContract = payment.IsOutsideContract });
        }
        var capital = await db.CapitalMovementDetails.SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (capital is not null)
        {
            details.Add(new CapitalMovementDetails { EntryId = reversal.Id, PartnerId = capital.PartnerId, Type = capital.Type, Description = reason });
            await BringBackIfSettledAsync(capital, db);
        }
        var revenue = await db.Set<RevenueDetails>().SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (revenue is not null)
        {
            details.Add(new RevenueDetails { EntryId = reversal.Id, ItemId = revenue.ItemId, SubgroupId = revenue.SubgroupId, PeriodStart = revenue.PeriodStart, PeriodEnd = revenue.PeriodEnd });
        }
        var sale = await db.Set<SalesInvoice>().SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (sale is not null)
        {
            details.Add(new SalesInvoice { EntryId = reversal.Id, BuyerName = sale.BuyerName, BuyerVatNumber = sale.BuyerVatNumber, Number = sale.Number, InvoiceDate = sale.InvoiceDate, Total = -sale.Total });
        }
        DirectEntries.Add(reversal, details, userId, at, db);
        return reversal;
    }

    private static async Task BringBackIfSettledAsync(CapitalMovementDetails capital, RedixDbContext db)
    {
        if (capital.Type == CapitalMovementType.SettlementAndExit)
        {
            await CapitalEndpoints.SetActiveAsync(capital.PartnerId, true, db);
        }
    }
}
