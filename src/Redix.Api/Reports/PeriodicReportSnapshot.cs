using Redix.Api.Accounts;
using Redix.Api.Partners;
using Redix.Api.Revenue;

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
/// <param name="Liquidation">What was distributed when the project closed (BR-27); zero until then.</param>
/// <param name="Categories">Each category's total with its items'.</param>
/// <param name="Accounts">Each account's real balance at the latest reconciliation.</param>
/// <param name="AccountsTotal">Their total.</param>
/// <param name="ReconciliationDifference">That total less the surplus (BR-08).</param>
/// <param name="PendingCount">How many entries waited for approval, a line of their own (BR-26).</param>
/// <param name="PendingAmount">What they came to.</param>
/// <param name="CapitalTotal">All partners' capital.</param>
/// <param name="Partners">Each partner's capital, share and movements.</param>
/// <param name="Items">Each item's report, new rows marked.</param>
/// <param name="RevenueRows">Each sale, VAT refund and other revenue, oldest first, new ones marked.</param>
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
    decimal Liquidation,
    List<CategoryTotalView> Categories,
    List<ReportAccountView> Accounts,
    decimal AccountsTotal,
    decimal ReconciliationDifference,
    int PendingCount,
    decimal PendingAmount,
    decimal CapitalTotal,
    List<ReportPartnerView> Partners,
    List<ItemReportView> Items,
    List<ReportRevenueView> RevenueRows)
{
    /// <summary>Expenses less the previous report's (BR-09).</summary>
    public decimal ExpensesDifference => Expenses - PreviousExpenses;

    /// <summary>All revenue (BR-06).</summary>
    public decimal Revenue => Capital + VatRefunds + Sales + OtherRevenue;

    /// <summary>All revenue but sales.</summary>
    public decimal RevenueWithoutSales => Revenue - Sales;

    /// <summary>The same report for someone who sees only their own capital (BR-31): the total, and no partner but theirs.</summary>
    public PeriodicReportSnapshot ForPartnerOf(Guid userId) => this with { Partners = Partners.Where(partner => partner.UserId == userId).ToList() };
}

/// <summary>One revenue entry in a report.</summary>
/// <param name="Type">What it is.</param>
/// <param name="Date">The day it came in.</param>
/// <param name="Description">What was said about it.</param>
/// <param name="Amount">How much.</param>
/// <param name="IsNew">True when it was approved after the previous report, shaded green (BR-26).</param>
public record ReportRevenueView(RevenueType Type, DateOnly Date, string Description, decimal Amount, bool IsNew);

/// <summary>An account's balance in a report.</summary>
/// <param name="Name">Its name.</param>
/// <param name="Type">What kind it is.</param>
/// <param name="Balance">What it really held.</param>
public record ReportAccountView(string Name, AccountType Type, decimal Balance);

/// <summary>A partner in a report.</summary>
/// <param name="Name">Their name.</param>
/// <param name="UserId">Their login; empty when they have none.</param>
/// <param name="Balance">Their capital.</param>
/// <param name="SharePercent">Their share, with ten decimals.</param>
/// <param name="Movements">Their movements, newest first.</param>
public record ReportPartnerView(string Name, Guid UserId, decimal Balance, decimal SharePercent, List<CapitalMovementView> Movements);
