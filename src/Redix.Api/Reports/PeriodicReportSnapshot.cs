using Redix.Api.Accounts;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>Everything a periodic report shows, as it was when issued (BRS section 6.1).</summary>
/// <param name="ProjectName">The project.</param>
/// <param name="Number">The report's number.</param>
/// <param name="IssuedAt">When it was issued.</param>
/// <param name="PreviousExpenses">The previous report's expenses.</param>
/// <param name="Expenses">Its expenses.</param>
/// <param name="Capital">Partners' capital.</param>
/// <param name="VatRefunds">VAT refunded.</param>
/// <param name="Sales">Sales.</param>
/// <param name="OtherRevenue">Anything else that came in.</param>
/// <param name="Surplus">Revenue less expenses.</param>
/// <param name="SurplusWithoutSales">The same without sales.</param>
/// <param name="Categories">Each category of expense items, its total with its items'.</param>
/// <param name="Accounts">Each account's real balance at the latest reconciliation.</param>
/// <param name="AccountsTotal">Their total.</param>
/// <param name="ReconciliationDifference">That total less the surplus (BR-08).</param>
/// <param name="PendingCount">How many expenses waited for approval, a line of their own (BR-26). Until 8 Nov 2026 only expenses could wait, so earlier reports read the same.</param>
/// <param name="PendingAmount">What they came to.</param>
/// <param name="CapitalTotal">All partners' capital.</param>
/// <param name="Partners">Each partner's capital, share and movements.</param>
/// <param name="Items">Each item's report, expense and revenue, new rows marked.</param>
/// <param name="RevenueCategories">Each category of revenue items, its total with its items' (FR-38): the summary's revenue side.</param>
/// <param name="PendingRevenueCount">How many revenue entries and partners' movements waited for approval (findings row 147); none in a report issued before 8 Nov 2026.</param>
/// <param name="PendingRevenueAmount">What they would have added to revenue.</param>
public record PeriodicReportSnapshot(
    string ProjectName,
    int Number,
    DateTime IssuedAt,
    decimal PreviousExpenses,
    decimal Expenses,
    decimal Capital,
    decimal VatRefunds,
    decimal Sales,
    decimal OtherRevenue,
    decimal Surplus,
    decimal SurplusWithoutSales,
    List<CategoryTotalView> Categories,
    List<ReportAccountView> Accounts,
    decimal AccountsTotal,
    decimal ReconciliationDifference,
    int PendingCount,
    decimal PendingAmount,
    decimal CapitalTotal,
    List<ReportPartnerView> Partners,
    List<ItemReportView> Items,
    List<CategoryTotalView> RevenueCategories,
    int PendingRevenueCount,
    decimal PendingRevenueAmount)
{
    /// <summary>Expenses less the previous report's (BR-09).</summary>
    public decimal ExpensesDifference => Expenses - PreviousExpenses;

    /// <summary>All revenue (BR-06).</summary>
    public decimal Revenue => Capital + VatRefunds + Sales + OtherRevenue;

    /// <summary>All revenue but sales.</summary>
    public decimal RevenueWithoutSales => Revenue - Sales;

    /// <summary>
    /// The same report for someone who does not see all capital (BR-31): the total, and only the partners <paramref name="sight"/> sees. A report issued before redix-api #106 kept no partner's id,
    /// so it shows them none. The item reports, رأس المال's holding every partner's money (redix-api #109), stay for the files, which <see cref="ForFilesOf"/> narrows to the partners each reader sees.
    /// </summary>
    public PeriodicReportSnapshot SeenBy(PartnerSight sight) => this with { Partners = Partners.Where(partner => sight.Sees(partner.Id)).ToList(), Items = [] };

    /// <summary>
    /// The same report as the files of someone <paramref name="sight"/> describes give it (redix-api #131 D1, BR-31): رأس المال's tables and the partners' lines only of the partners
    /// they see, as the capital screen shows them, its total still the whole capital, which everyone sees. Whoever sees all capital gets every partner.
    /// </summary>
    public PeriodicReportSnapshot ForFilesOf(PartnerSight sight) => sight.SeesAll ? this : this with
    {
        Partners = [.. Partners.Where(partner => sight.Sees(partner.Id))],
        Items = [.. Items.Select(item => item.Type == ItemType.Capital ? item.WithTablesOf(sight.Sees) : item)],
    };
}

/// <summary>An account's balance in a report.</summary>
/// <param name="Name">Its name.</param>
/// <param name="Type">What kind it is.</param>
/// <param name="Balance">What it really held.</param>
public record ReportAccountView(string Name, AccountType Type, decimal Balance);

/// <summary>A partner in a report.</summary>
/// <param name="Name">Their name.</param>
/// <param name="Id">The partner; empty in a report issued before redix-api #106.</param>
/// <param name="Balance">Their capital.</param>
/// <param name="SharePercent">Their share, with ten decimals.</param>
public record ReportPartnerView(string Name, Guid Id, decimal Balance, decimal SharePercent);
