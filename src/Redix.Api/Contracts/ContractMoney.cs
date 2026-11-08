using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Contracts;

/// <summary>What the ledger holds inside a contract (BR-04): what was paid, which counts only once approved (BR-10), and what waits for approval, which shows beside it. Approval only has to change an entry's status for a payment to move from one to the other.</summary>
/// <param name="Paid">The approved payments inside the contract, a deduction taking away.</param>
/// <param name="Pending">Those waiting for approval. A draft and a rejected payment are neither.</param>
public readonly record struct ContractMoney(decimal Paid, decimal Pending)
{
    /// <summary>What the ledger holds inside each of the project's contracts, leaving out the entry <paramref name="leftOut"/>, the one being changed; a contract with nothing is left out, and its default is nothing.</summary>
    public static async Task<Dictionary<Guid, ContractMoney>> OfProjectAsync(Guid projectId, RedixDbContext db, Guid leftOut = default)
    {
        var rows = await (
            from payment in db.ContractorPaymentDetails
            join entry in db.Entries on payment.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Id != leftOut && !payment.IsOutsideContract && (entry.Status == EntryStatus.Approved || entry.Status == EntryStatus.PendingApproval)
            group entry.Amount by new { payment.ContractId, entry.Status } into amounts
            select new { amounts.Key.ContractId, amounts.Key.Status, Total = amounts.Sum() }).ToListAsync();
        return rows.GroupBy(row => row.ContractId).ToDictionary(contract => contract.Key, contract => new ContractMoney(
            contract.Where(row => row.Status == EntryStatus.Approved).Sum(row => row.Total),
            contract.Where(row => row.Status == EntryStatus.PendingApproval).Sum(row => row.Total)));
    }
}
