using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>A project's figures at a glance (UC-049, FR-34), with its completion and cost (UC-063).</summary>
public static class DashboardEndpoints
{
    /// <summary>Maps the route that reads the dashboard, for anyone who reads the project.</summary>
    /// <param name="app">The app.</param>
    public static void MapDashboard(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/dashboard", Dashboard).RequireAuthorization(nameof(Permission.ReadProject));
    }

    /// <summary>Revenue, expenses, the surplus, the reconciliation difference, what waits for approval, each category's total with its items', the capital with the partners' lines the caller may see (BR-31), and the completion. Read on every request, so an approval shows at once (AC-34.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session, which says whose capital they see.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<DashboardView> Dashboard(Guid projectId, ClaimsPrincipal principal, RedixDbContext db) =>
        await OfAsync(projectId, await CapitalEndpoints.Capital(projectId, principal, db), db);

    /// <summary>The dashboard of <paramref name="projectId"/>, with <paramref name="capital"/> as its capital.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="capital">The capital, with the partners' lines its reader may see.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<DashboardView> OfAsync(Guid projectId, CapitalView capital, RedixDbContext db)
    {
        var figures = await ProjectFigures.OfAsync(projectId, db);
        var reconciliation = await ReconciliationsEndpoints.Latest(projectId, db);
        var waiting = await WaitingFigures.OfAsync(projectId, db);
        var totals = await ItemTotals.OfProjectAsync(projectId, db);
        var categories = await db.Categories.Where(category => category.ProjectId == projectId).OrderBy(category => category.SortOrder).ToListAsync();
        var categoryIds = categories.Select(category => category.Id).ToList();
        var items = (await db.Items.Where(item => categoryIds.Contains(item.CategoryId)).OrderBy(item => item.SortOrder).ToListAsync()).ToLookup(item => item.CategoryId);
        List<CategoryTotalView> SideOf(bool isRevenue) => categories
            .Select(category => (Category: category, Items: items[category.Id].Where(item => item.Type.IsRevenue() == isRevenue).Select(item => new ItemTotalView(item.Id, item.Name, totals.GetValueOrDefault(item.Id), item.Type)).ToList()))
            .Where(line => line.Items.Count > 0)
            .Select(line => new CategoryTotalView(line.Category.Id, line.Category.Name, line.Items.Sum(item => item.Total), line.Items))
            .ToList();
        return new DashboardView(figures.Revenue, capital, figures.Expenses, figures.Surplus, reconciliation.At != default, reconciliation.Difference, waiting, figures.CompletionPercent, SideOf(false), SideOf(true));
    }
}

/// <summary>A project's figures at a glance.</summary>
/// <param name="Revenue">Capital, VAT refunds and sales (BR-06).</param>
/// <param name="Capital">What the partners put in less what they took out: the revenue side's first line, as on the periodic report (BR-06, AC-46.7), opening to the partners' lines the caller may see (BR-31).</param>
/// <param name="Expenses">Every approved expense: the project's cost.</param>
/// <param name="Surplus">Revenue less expenses (BR-07).</param>
/// <param name="WasReconciled">True once the accounts were checked against the books.</param>
/// <param name="ReconciliationDifference">The latest check's difference (BR-08).</param>
/// <param name="Waiting">What waits for approval: expenses, and revenue with partners' money.</param>
/// <param name="CompletionPercent">How far the work has come: expenses as a percentage of revenue.</param>
/// <param name="Categories">Each category holding expense items, its total with those items'.</param>
/// <param name="RevenueCategories">Each category holding revenue items, its total with those items' (FR-38); with <paramref name="Capital"/> they add up to <paramref name="Revenue"/>.</param>
public record DashboardView(decimal Revenue, CapitalView Capital, decimal Expenses, decimal Surplus, bool WasReconciled, decimal ReconciliationDifference, WaitingFigures Waiting, decimal CompletionPercent, List<CategoryTotalView> Categories, List<CategoryTotalView> RevenueCategories);

/// <summary>One category's total (BR-03).</summary>
/// <param name="Id">The category.</param>
/// <param name="Name">Its name.</param>
/// <param name="Total">The sum of its items.</param>
/// <param name="Items">Each item's total.</param>
public record CategoryTotalView(Guid Id, string Name, decimal Total, List<ItemTotalView> Items);

/// <summary>One item's total (BR-02).</summary>
/// <param name="Id">The item.</param>
/// <param name="Name">Its name.</param>
/// <param name="Total">Its purchases and contractor payments, or its revenue.</param>
/// <param name="Type">What it is for, which says its side.</param>
public record ItemTotalView(Guid Id, string Name, decimal Total, ItemType Type);
