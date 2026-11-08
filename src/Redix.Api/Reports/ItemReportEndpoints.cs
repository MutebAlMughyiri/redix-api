using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>Everything spent on one item (UC-050, FR-30): its purchases by subgroup, then each contract's payments; for a revenue item, its revenue by subgroup.</summary>
public static class ItemReportEndpoints
{
    /// <summary>Maps the route, for anyone who reads the project.</summary>
    /// <param name="app">The app.</param>
    public static void MapItemReport(this WebApplication app) =>
        app.MapGet("/projects/{projectId:guid}/reports/items/{itemId:guid}", Report).RequireAuthorization(nameof(Permission.ReadProject));

    /// <summary>The report of the item, from what is approved, in date order. Its subgroups and its contracts' payments add up to its total (AC-30.1). An item of another project is not found.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Report(Guid projectId, Guid itemId, RedixDbContext db)
    {
        var item = await ProjectStructure.ItemWithTablesAsync(projectId, itemId, db);
        return item == Item.None ? Results.NotFound() : Results.Ok(await OfAsync(projectId, item, (await PeriodicReport.LatestOfAsync(projectId, db)).IssuedAt, db));
    }

    /// <summary>The report of <paramref name="item"/>, for whatever shows it: the screen, or a file. What was approved after <paramref name="newSince"/> is marked new (BR-26).</summary>
    public static async Task<ItemReportView> OfAsync(Guid projectId, Item item, DateTime newSince, RedixDbContext db)
    {
        var rows = await (
            from expense in db.ExpenseDetails
            join entry in db.Entries on expense.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && expense.ItemId == item.Id
            orderby entry.EntryDate, entry.Id
            select new { entry, expense }).ToListAsync();
        var revenue = await (
            from details in db.Set<RevenueDetails>()
            join entry in db.Entries on details.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && details.ItemId == item.Id
            orderby entry.EntryDate, entry.Id
            select new { entry, details }).ToListAsync();
        var entries = rows.Select(row => row.entry).ToList();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var ids = entries.Select(entry => entry.Id).ToList();
        var purchases = await db.PurchaseDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId);
        var payments = await db.ContractorPaymentDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId);
        var columns = item.Columns.Where(column => column.IsCustom && column.ShowInReports && !column.IsHidden).OrderBy(column => column.SortOrder).ToList();

        ItemEntryRow RowOf(Entry entry, ExpenseDetails expense)
        {
            var purchase = purchases.GetValueOrDefault(entry.Id) ?? new PurchaseDetails();
            var values = expense.CustomValuesByKey();
            return new ItemEntryRow(entry.Id, entry.EntryDate, purchase.Product.Length > 0 ? purchase.Product : entry.Notes, facts.SupplierNameOf(entry), facts.InvoiceOf(entry).Number, purchase.UnitPrice, purchase.Quantity, entry.Amount, (payments.GetValueOrDefault(entry.Id) ?? new ContractorPaymentDetails()).IsOutsideContract, columns.Select(column => values.GetValueOrDefault(column.Key, "")).ToList(), entry.ApprovedAt > newSince);
        }

        ItemEntryRow RevenueRowOf(Entry entry) => new(entry.Id, entry.EntryDate, entry.Notes, "", "", 0, 0, entry.Amount, false, [], entry.ApprovedAt > newSince);

        var subgroups = item.Subgroups.OrderBy(subgroup => subgroup.SortOrder).Select(subgroup =>
        {
            var lines = rows.Where(row => row.entry.Kind == EntryKind.Purchase && row.expense.SubgroupId == subgroup.Id).Select(row => RowOf(row.entry, row.expense))
                .Concat(revenue.Where(row => row.details.SubgroupId == subgroup.Id).Select(row => RevenueRowOf(row.entry))).ToList();
            return new SubgroupReportView(subgroup.Id, subgroup.Name, lines.Sum(line => line.Amount), lines) { SortOrder = subgroup.SortOrder };
        }).Where(subgroup => subgroup.Rows.Count > 0).ToList();
        var money = await ContractMoney.OfProjectAsync(projectId, db);
        var contracts = await (from contract in db.Contracts join supplier in db.Suppliers on contract.SupplierId equals supplier.Id where contract.ProjectId == projectId && contract.ItemId == item.Id orderby contract.SortOrder, contract.Id select new { contract, supplier.Name }).ToListAsync();
        var paymentRows = rows.Where(row => row.entry.Kind == EntryKind.ContractorPayment).ToList();
        var contractViews = contracts.Select(row =>
        {
            var lines = paymentRows.Where(payment => (payments.GetValueOrDefault(payment.entry.Id) ?? new ContractorPaymentDetails()).ContractId == row.contract.Id).Select(payment => RowOf(payment.entry, payment.expense)).ToList();
            var paid = money.GetValueOrDefault(row.contract.Id).Paid;
            return new ContractReportView(row.contract.Id, row.Name, row.contract.Value, row.contract.IsOpenValue, paid, row.contract.RemainingAfter(paid), row.contract.IsOverrunBy(paid), lines.Sum(line => line.Amount), lines) { SortOrder = row.contract.SortOrder };
        }).Where(contract => contract.Payments.Count > 0 || contract.Value > 0).ToList();
        var categoryName = await db.Categories.Where(category => category.Id == item.CategoryId).Select(category => category.Name).SingleAsync();
        var total = rows.Sum(row => row.entry.Amount) + revenue.Sum(row => row.entry.Amount);
        return new ItemReportView(item.Id, item.Name, categoryName, total, columns.Select(column => column.Name).ToList(), subgroups, contractViews) { ShowsTablePercentages = item.ShowsTablePercentages, Type = item.Type };
    }
}

/// <summary>Everything spent on one item.</summary>
/// <param name="ItemId">The item.</param>
/// <param name="ItemName">Its name.</param>
/// <param name="CategoryName">Its category.</param>
/// <param name="Total">Its purchases and contractor payments (BR-02).</param>
/// <param name="Columns">The names of its own columns that print in its report (FR-41), in the order of each row's values.</param>
/// <param name="Subgroups">Its purchases, a table per subgroup.</param>
/// <param name="Contracts">Its contracts, each with its payments.</param>
public record ItemReportView(Guid ItemId, string ItemName, string CategoryName, decimal Total, List<string> Columns, List<SubgroupReportView> Subgroups, List<ContractReportView> Contracts)
{
    /// <summary>True when its sheet calculates each table's percentage (AC-46.9); false in a report issued before the choice existed.</summary>
    public bool ShowsTablePercentages { get; init; }

    /// <summary>What the item is for, which says its side (FR-38).</summary>
    public ItemType Type { get; init; }
}

/// <summary>One subgroup's purchases.</summary>
/// <param name="Id">The subgroup.</param>
/// <param name="Name">Its name; empty for the general one.</param>
/// <param name="Total">Its purchases.</param>
/// <param name="Rows">Each purchase.</param>
public record SubgroupReportView(Guid Id, string Name, decimal Total, List<ItemEntryRow> Rows)
{
    /// <summary>Its place among the item's tables (AC-46.8); zero in a report issued before tables had one.</summary>
    public int SortOrder { get; init; }
}

/// <summary>One contract with its payments.</summary>
/// <param name="Id">The contract.</param>
/// <param name="ContractorName">The contractor.</param>
/// <param name="Value">What it is worth.</param>
/// <param name="IsOpenValue">True when it has no fixed value.</param>
/// <param name="Paid">What was paid inside it (BR-04).</param>
/// <param name="Remaining">Its value less that; nothing for an open-value contract.</param>
/// <param name="IsOverrun">True when more was paid than it is worth (AC-31.1).</param>
/// <param name="PaymentsTotal">Every payment on it, outside the contract too: what counts in the item.</param>
/// <param name="Payments">Each payment.</param>
public record ContractReportView(Guid Id, string ContractorName, decimal Value, bool IsOpenValue, decimal Paid, decimal Remaining, bool IsOverrun, decimal PaymentsTotal, List<ItemEntryRow> Payments)
{
    /// <summary>Its place among the item's tables (AC-46.8); zero in a report issued before tables had one.</summary>
    public int SortOrder { get; init; }
}

/// <summary>One purchase or payment in an item's report, in the columns of the current statement.</summary>
/// <param name="EntryId">The entry.</param>
/// <param name="Date">The day it was paid.</param>
/// <param name="Description">What was bought, or what was said.</param>
/// <param name="SupplierName">The supplier, or the contractor.</param>
/// <param name="InvoiceNumber">The invoice's number; empty when none.</param>
/// <param name="UnitPrice">The price of one; zero when not used.</param>
/// <param name="Quantity">How many; zero when not used.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="IsOutsideContract">True for a payment outside its contract.</param>
/// <param name="Values">The values of the item's own columns, in the report's order.</param>
/// <param name="IsNew">True when it was approved after the latest periodic report, shaded green (BR-26).</param>
public record ItemEntryRow(Guid EntryId, DateOnly Date, string Description, string SupplierName, string InvoiceNumber, decimal UnitPrice, decimal Quantity, decimal Amount, bool IsOutsideContract, List<string> Values, bool IsNew);
