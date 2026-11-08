using Microsoft.EntityFrameworkCore;
using Redix.Api.Partners;
using Redix.Api.Revenue;

namespace Redix.Api.Entries;

/// <summary>Reversing what is approved (UC-035, FR-50): an opposite entry of the same amount, tied to the original and approved at once, so both show in the reports and the net effect is zero.</summary>
public static class EntryReversal
{
    /// <summary>Adds the reversal of <paramref name="entry"/>, with a copy of what it holds; saved with the change.</summary>
    public static async Task<Entry> AddAsync(Entry entry, string reason, Guid userId, DateTime at, DateOnly today, RedixDbContext db)
    {
        var reversal = DirectEntries.Approved(entry.ProjectId, entry.Kind, today, -entry.Amount, entry.AccountId, reason, userId, at);
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
        }
        var revenue = await db.Set<RevenueDetails>().SingleOrDefaultAsync(row => row.EntryId == entry.Id);
        if (revenue is not null)
        {
            details.Add(new RevenueDetails { EntryId = reversal.Id, ItemId = revenue.ItemId, SubgroupId = revenue.SubgroupId });
        }
        await DirectEntries.AddAsync(reversal, details, userId, at, db);
        return reversal;
    }
}
