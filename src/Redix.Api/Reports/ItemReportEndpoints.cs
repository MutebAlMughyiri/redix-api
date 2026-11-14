using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Revenue;
using Redix.Api.Structure;
using System.Security.Claims;
using Redix.Api.Partners;

namespace Redix.Api.Reports;

/// <summary>
/// Everything spent on one item (UC-050, FR-30): its purchases by subgroup, then each contract's payments; for a revenue item, its revenue by subgroup; for a capital item, each partner's
/// money in and out, a table each (redix-api #109).
/// </summary>
public static class ItemReportEndpoints
{
    /// <summary>Maps the routes of an item's report and of a contract's, for whoever holds <see cref="Permission.SeeReports"/> (redix-api #131 D2).</summary>
    /// <param name="app">The app.</param>
    public static void MapItemReport(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/reports/items/{itemId:guid}", Report).RequireAuthorization(nameof(Permission.SeeReports));
        app.MapGet("/projects/{projectId:guid}/reports/contracts/{contractId:guid}", ContractReport).RequireAuthorization(nameof(Permission.SeeReports));
    }

    /// <summary>
    /// The report of one contract, as the Contracts screen opens it (Muteb, 12 Nov 2026): its item's report holding that contract's table alone, its payments, value, paid and
    /// remaining. A contract of another project is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> ContractReport(Guid projectId, Guid contractId, RedixDbContext db)
    {
        var report = await OfContractAsync(projectId, contractId, db);
        return report == ItemReportView.None ? Results.NotFound() : Results.Ok(report);
    }

    /// <summary>The report of the contract <paramref name="contractId"/>, for its screen and its files: its item's report holding that contract's table alone; none for a contract of another project.</summary>
    public static async Task<ItemReportView> OfContractAsync(Guid projectId, Guid contractId, RedixDbContext db)
    {
        var itemId = await db.Contracts.Where(contract => contract.Id == contractId && contract.ProjectId == projectId).Select(contract => contract.ItemId).SingleOrDefaultAsync();
        var item = await ProjectStructure.ItemWithTablesAsync(projectId, itemId, db);
        if (item == Item.None)
        {
            return ItemReportView.None;
        }
        var report = await OfAsync(projectId, item, (await PeriodicReport.LatestOfAsync(projectId, db)).IssuedAt, db, _ => false);
        return report with { Subgroups = [], Contracts = [.. report.Contracts.Where(contract => contract.Id == contractId)] };
    }

    /// <summary>
    /// The report of the item, from what is approved, in date order. Its subgroups and its contracts' payments add up to its total (AC-30.1). A capital item shows the caller only their own
    /// partner's table unless they see all capital (BR-31), and its total stays the project's capital. An item of another project is not found.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="itemId">The item.</param>
    /// <param name="principal">The caller's session, which says whose capital they see.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Report(Guid projectId, Guid itemId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var item = await ProjectStructure.ItemWithTablesAsync(projectId, itemId, db);
        if (item == Item.None)
        {
            return Results.NotFound();
        }
        var sight = await PartnerSight.OfAsync(principal, projectId, db);
        return Results.Ok(await OfAsync(projectId, item, (await PeriodicReport.LatestOfAsync(projectId, db)).IssuedAt, db, sight.Sees));
    }

    /// <summary>
    /// The report of <paramref name="item"/>, for whatever shows it: the screen, or a file. What was approved after <paramref name="newSince"/> is marked new (BR-26). Of a capital item's
    /// partners, only those <paramref name="shows"/> keeps have their table.
    /// </summary>
    public static async Task<ItemReportView> OfAsync(Guid projectId, Item item, DateTime newSince, RedixDbContext db, Func<Partner, bool> shows)
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
        var movements = await (
            from movement in db.CapitalMovementDetails
            join entry in db.Entries on movement.EntryId equals entry.Id
            join partner in db.Partners on movement.PartnerId equals partner.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved && partner.ItemId == item.Id
            orderby entry.EntryDate, entry.Id
            select new { entry, movement }).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, [.. rows.Select(row => row.entry), .. revenue.Select(row => row.entry), .. movements.Select(row => row.entry)], db);
        var ids = rows.Select(row => row.entry.Id).ToList();
        var payments = await db.ContractorPaymentDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId);
        var registers = await ReportColumns.OfProjectAsync(projectId, db);
        var groupKind = item.Type.IsRevenue() ? EntryKind.Revenue : EntryKind.Purchase;
        var groupColumns = ReportColumns.PrintedOf(item, groupKind);
        var paymentColumns = ReportColumns.PrintedOf(item, EntryKind.ContractorPayment);
        var capitalColumns = ReportColumns.PrintedOf(item, EntryKind.CapitalIn);

        ItemEntryRow RowOf(Entry entry, List<ItemColumn> columns, decimal amount)
        {
            var isOutside = (payments.GetValueOrDefault(entry.Id) ?? new ContractorPaymentDetails()).IsOutsideContract;
            return new ItemEntryRow(entry.Id, entry.EntryDate, amount, isOutside, ReportColumns.ValuesOf(columns, entry, facts, isOutside), entry.ApprovedAt > newSince);
        }

        var subgroups = item.Subgroups.OrderBy(subgroup => subgroup.SortOrder).Select(subgroup =>
        {
            var lines = rows.Where(row => row.entry.Kind == EntryKind.Purchase && row.expense.SubgroupId == subgroup.Id).Select(row => row.entry)
                .Concat(revenue.Where(row => row.details.SubgroupId == subgroup.Id).Select(row => row.entry)).Select(entry => RowOf(entry, groupColumns, entry.Amount)).ToList();
            return new SubgroupReportView(subgroup.Id, subgroup.Name, lines.Sum(line => line.Amount), lines)
            {
                SortOrder = subgroup.SortOrder,
                Columns = registers.ViewsOf(groupColumns),
                AmountName = ReportColumns.AmountNameOf(item, groupKind),
            };
        }).Where(subgroup => subgroup.Rows.Count > 0).ToList();
        var money = await ContractMoney.OfProjectAsync(projectId, db);
        var contracts = await (from contract in db.Contracts join supplier in db.Suppliers on contract.SupplierId equals supplier.Id where contract.ProjectId == projectId && contract.ItemId == item.Id orderby contract.SortOrder, contract.Id select new { contract, supplier.Name }).ToListAsync();
        var paymentRows = rows.Where(row => row.entry.Kind == EntryKind.ContractorPayment).ToList();
        var contractViews = contracts.Select(row =>
        {
            var lines = paymentRows.Where(payment => (payments.GetValueOrDefault(payment.entry.Id) ?? new ContractorPaymentDetails()).ContractId == row.contract.Id).Select(payment => RowOf(payment.entry, paymentColumns, payment.entry.Amount)).ToList();
            var paid = money.GetValueOrDefault(row.contract.Id).Paid;
            return new ContractReportView(row.contract.Id, row.Name, row.contract.Value, row.contract.IsOpenValue, paid, row.contract.RemainingAfter(paid), row.contract.IsOverrunBy(paid), lines.Sum(line => line.Amount), lines)
            {
                SortOrder = row.contract.SortOrder,
                Columns = registers.ViewsOf(paymentColumns),
                AmountName = ReportColumns.AmountNameOf(item, EntryKind.ContractorPayment),
            };
        }).Where(contract => contract.Payments.Count > 0 || contract.Value > 0).ToList();
        var everyPartner = await db.Partners.Where(partner => partner.ItemId == item.Id).ToListAsync();
        var partners = everyPartner.Where(shows).OrderBy(partner => partner.SortOrder).ThenBy(partner => partner.Id).ToList();
        var partnerTables = partners.Select(partner =>
        {
            var lines = movements.Where(row => row.movement.PartnerId == partner.Id).Select(row => RowOf(row.entry, capitalColumns, row.entry.Kind.Signed(row.entry.Amount))).ToList();
            return new SubgroupReportView(partner.Id, partner.Name, lines.Sum(line => line.Amount), lines)
            {
                SortOrder = partner.SortOrder,
                Columns = registers.ViewsOf(capitalColumns),
                AmountName = ReportColumns.AmountNameOf(item, EntryKind.CapitalIn),
            };
        }).Where(table => table.Rows.Count > 0);
        var categoryName = await db.Categories.Where(category => category.Id == item.CategoryId).Select(category => category.Name).SingleAsync();
        var total = rows.Sum(row => row.entry.Amount) + revenue.Sum(row => row.entry.Amount) + movements.Sum(row => row.entry.Kind.Signed(row.entry.Amount));
        return new ItemReportView(item.Id, item.Name, categoryName, total, [.. subgroups, .. partnerTables], contractViews)
        {
            ShowsTablePercentages = item.ShowsTablePercentages,
            Type = item.Type,
            ShowsEveryTable = partners.Count == everyPartner.Count,
        };
    }
}

/// <summary>Everything spent on one item.</summary>
/// <param name="ItemId">The item.</param>
/// <param name="ItemName">Its name.</param>
/// <param name="CategoryName">Its category.</param>
/// <param name="Total">Its purchases and contractor payments (BR-02), its revenue, or for a capital item the whole capital.</param>
/// <param name="Subgroups">Its purchases, a table per subgroup.</param>
/// <param name="Contracts">Its contracts, each with its payments.</param>
public record ItemReportView(Guid ItemId, string ItemName, string CategoryName, decimal Total, List<SubgroupReportView> Subgroups, List<ContractReportView> Contracts)
{
    /// <summary>No report: what a contract of another project answers.</summary>
    public static readonly ItemReportView None = new(Guid.Empty, "", "", 0, [], []);

    /// <summary>True when its sheet calculates each table's percentage (AC-46.9); false in a report issued before the choice existed.</summary>
    public bool ShowsTablePercentages { get; init; }

    /// <summary>What the item is for, which says its side (FR-38).</summary>
    public ItemType Type { get; init; }

    /// <summary>False for رأس المال shown to someone who does not see every partner: its tables are theirs alone, and its total stays the whole capital (BR-31, redix-api #131 D1).</summary>
    public bool ShowsEveryTable { get; init; } = true;

    /// <summary>The same report holding only the tables <paramref name="shows"/> keeps, the partners' of رأس المال a reader may see; its total stays the whole (BR-31).</summary>
    public ItemReportView WithTablesOf(Func<Guid, bool> shows) =>
        Subgroups.All(table => shows(table.Id)) ? this : this with { Subgroups = [.. Subgroups.Where(table => shows(table.Id))], ShowsEveryTable = false };
}

/// <summary>One table's rows: a subgroup's purchases or revenue, or a partner's money in and out.</summary>
/// <param name="Id">The subgroup, or the partner.</param>
/// <param name="Name">Its name; empty for the general one.</param>
/// <param name="Total">What its rows add up to.</param>
/// <param name="Rows">Each row.</param>
public record SubgroupReportView(Guid Id, string Name, decimal Total, List<ItemEntryRow> Rows)
{
    /// <summary>Its place among the item's tables (AC-46.8); zero in a report issued before tables had one.</summary>
    public int SortOrder { get; init; }

    /// <summary>The columns it prints before its amount, its item's own, «الوصف» among them, in their order (redix-api #129, R1); none in a report issued before.</summary>
    public List<ReportColumnView> Columns { get; init; } = [];

    /// <summary>Its amount column's own name, its header (redix-api #129); empty in a report issued before.</summary>
    public string AmountName { get; init; } = "";
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

    /// <summary>The columns its table prints before the payment, its item's own that a payment is asked, «الوصف» among them, in their order (redix-api #129, R1); none in a report issued before.</summary>
    public List<ReportColumnView> Columns { get; init; } = [];

    /// <summary>Its amount column's own name, its header (redix-api #129); empty in a report issued before.</summary>
    public string AmountName { get; init; } = "";
}

/// <summary>One entry in an item's report, in its table's columns (redix-api #129, R1).</summary>
/// <param name="EntryId">The entry.</param>
/// <param name="Date">The day it was paid, which orders the rows.</param>
/// <param name="Amount">What was paid, or what it did to the capital.</param>
/// <param name="IsOutsideContract">True for a payment outside its contract, which leaves what remains of it alone (BR-04).</param>
/// <param name="Values">What it shows under each of its table's columns, in their order, as <see cref="ReportColumns.ValuesOf"/> writes them.</param>
/// <param name="IsNew">True when it was approved after the latest periodic report, shaded green (BR-26).</param>
public record ItemEntryRow(Guid EntryId, DateOnly Date, decimal Amount, bool IsOutsideContract, List<string> Values, bool IsNew);
