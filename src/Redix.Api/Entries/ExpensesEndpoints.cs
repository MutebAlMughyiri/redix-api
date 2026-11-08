using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>What a credit note can return (D13): the bills of a supplier still holding something to return. A purchase is recorded as every entry is (redix-api #110).</summary>
public static class ExpensesEndpoints
{
    /// <summary>Maps the route that reads what a credit note can return, for anyone who reads the project.</summary>
    public static void MapExpenses(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/expenses/creditable", Creditable).RequireAuthorization(nameof(Permission.ReadProject));
    }

    /// <summary>The bills of <paramref name="supplierId"/> that a credit note can return (D13): sent or approved, and neither a credit note nor a deduction themselves, each with what is left of its total after the credit notes that already name it. One rejected no longer takes anything.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<CreditableInvoiceView>> Creditable(Guid projectId, Guid supplierId, RedixDbContext db) => CreditableExceptAsync(projectId, supplierId, db, Guid.Empty);

    /// <summary>The bills of <paramref name="supplierId"/> a credit note can return, as <see cref="Creditable"/> answers them, leaving out the credit note <paramref name="editing"/> being changed.</summary>
    public static async Task<List<CreditableInvoiceView>> CreditableExceptAsync(Guid projectId, Guid supplierId, RedixDbContext db, Guid editing)
    {
        var bills = await (
            from purchase in db.PurchaseDetails
            join entry in db.Entries on purchase.EntryId equals entry.Id
            where entry.ProjectId == projectId && purchase.SupplierId == supplierId && entry.Amount > 0 && (entry.Status == EntryStatus.PendingApproval || entry.Status == EntryStatus.Approved)
            select new { entry.Id, entry.EntryDate, entry.Amount, Number = db.Invoices.Where(invoice => invoice.EntryId == entry.Id).Select(invoice => invoice.Number).FirstOrDefault() ?? "" }).ToListAsync();
        var returned = await (
            from link in db.EntryLinks
            join note in db.Entries on link.EntryId equals note.Id
            where link.Type == EntryLinkType.CreditNoteOf && note.ProjectId == projectId && note.Status != EntryStatus.Rejected && note.Id != editing
            group note.Amount by link.LinkedEntryId into notes
            select new { EntryId = notes.Key, Total = notes.Sum() }).ToDictionaryAsync(row => row.EntryId, row => row.Total);
        return bills.OrderBy(bill => bill.Id).Select(bill => new CreditableInvoiceView(bill.Id, bill.Number, bill.EntryDate, bill.Amount + returned.GetValueOrDefault(bill.Id))).ToList();
    }
}

/// <summary>A bill a credit note can return (D13).</summary>
/// <param name="EntryId">The bill's entry.</param>
/// <param name="Number">The number of an older bill that had one; empty for the rest.</param>
/// <param name="Date">The bill's date.</param>
/// <param name="Returnable">What is left of its total after the credit notes that already name it.</param>
public record CreditableInvoiceView(Guid EntryId, string Number, DateOnly Date, decimal Returnable);
