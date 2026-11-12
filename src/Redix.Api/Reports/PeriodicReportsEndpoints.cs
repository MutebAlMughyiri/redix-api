using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Partners;
using Redix.Api.Projects;

namespace Redix.Api.Reports;

/// <summary>The numbered periodic report that replaces the manual statement (UC-053, FR-29), and its PDF (UC-054) and workbook (UC-055).</summary>
public static class PeriodicReportsEndpoints
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Maps the routes that read, for anyone who reads the project, the one that issues, for whoever holds <see cref="Permission.MoveMoney"/>, and the files, for whoever sees every partner's capital, since only they export (BR-31).</summary>
    /// <param name="app">The app.</param>
    public static void MapPeriodicReports(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/periodic-reports").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/next", Next);
        reads.MapGet("/{number:int}", Read);
        var files = app.MapGroup("/projects/{projectId:guid}/periodic-reports/{number:int}").RequireAuthorization(nameof(Permission.SeeAllCapital));
        files.MapGet("/pdf", Pdf);
        files.MapGet("/xlsx", Excel);
        app.MapProjectChanges(Permission.MoveMoney).MapPost("/periodic-reports", Issue);
    }

    /// <summary>The reports issued, newest first.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the reports live in.</param>
    public static Task<List<PeriodicReportRowView>> List(Guid projectId, RedixDbContext db) =>
        db.PeriodicReports.Where(report => report.ProjectId == projectId).OrderByDescending(report => report.Number).Select(report => new PeriodicReportRowView(report.Number, report.IssuedAt, report.ExpensesTotal)).ToListAsync();

    /// <summary>The number the next report takes, and what waits for approval, which it shows as two lines (UC-053 step 2).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the reports live in.</param>
    public static async Task<NextReportView> Next(Guid projectId, RedixDbContext db) => new((await PeriodicReport.LatestOfAsync(projectId, db)).Number + 1, await WaitingFigures.OfAsync(projectId, db));

    /// <summary>
    /// Issues the next report from what is approved now: a numbered snapshot that never changes (BR-26). What was approved since the previous one is marked new, whatever its date, and the
    /// expenses' difference starts from the previous report's (BR-09). Numbering has no gaps (AC-29.3).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Issue(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var previous = await PeriodicReport.LatestOfAsync(projectId, db);
        var at = time.GetUtcNow().UtcDateTime;
        var snapshot = await SnapshotAsync(projectId, previous.Number + 1, previous, at, db);
        var report = new PeriodicReport { ProjectId = projectId, Number = snapshot.Number, IssuedAt = at, IssuedByUserId = AccessTokens.UserIdOf(principal), ExpensesTotal = snapshot.Expenses, Snapshot = JsonSerializer.Serialize(snapshot, Json) };
        db.PeriodicReports.Add(report);
        await db.SaveChangesAsync();
        return Results.Ok(new PeriodicReportRowView(report.Number, report.IssuedAt, report.ExpensesTotal));
    }

    /// <summary>A report as it was issued: every partner for whoever sees all capital, otherwise the total and only the lines of the partners the caller sees (BR-31). A number not issued is not found.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="number">The report's number.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the reports live in.</param>
    public static async Task<IResult> Read(Guid projectId, int number, ClaimsPrincipal principal, RedixDbContext db)
    {
        var snapshot = await SnapshotOfAsync(projectId, number, db);
        if (snapshot is null)
        {
            return Results.NotFound();
        }
        var sight = await PartnerSight.OfAsync(principal, projectId, db);
        return Results.Ok(sight.SeesAll ? snapshot : snapshot.SeenBy(sight));
    }

    /// <summary>The report as a PDF laid out as report 23 (UC-054).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="number">The report's number.</param>
    /// <param name="db">The database the reports live in.</param>
    public static async Task<IResult> Pdf(Guid projectId, int number, RedixDbContext db)
    {
        var snapshot = await SnapshotOfAsync(projectId, number, db);
        return snapshot is null ? Results.NotFound() : Results.File(PeriodicReportFiles.Pdf(snapshot), "application/pdf", $"report-{number}.pdf");
    }

    /// <summary>The report as a workbook with live totals (UC-055).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="number">The report's number.</param>
    /// <param name="db">The database the reports live in.</param>
    public static async Task<IResult> Excel(Guid projectId, int number, RedixDbContext db)
    {
        var snapshot = await SnapshotOfAsync(projectId, number, db);
        return snapshot is null ? Results.NotFound() : Results.File(PeriodicReportWorkbook.Of(snapshot), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report-{number}.xlsx");
    }

    private static async Task<PeriodicReportSnapshot?> SnapshotOfAsync(Guid projectId, int number, RedixDbContext db)
    {
        var stored = await db.PeriodicReports.Where(report => report.ProjectId == projectId && report.Number == number).Select(report => report.Snapshot).SingleOrDefaultAsync();
        return stored is null ? null : JsonSerializer.Deserialize<PeriodicReportSnapshot>(stored, Json);
    }

    private static async Task<PeriodicReportSnapshot> SnapshotAsync(Guid projectId, int number, PeriodicReport previous, DateTime at, RedixDbContext db)
    {
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        var figures = await ProjectFigures.OfAsync(projectId, db);
        var capital = await CapitalEndpoints.ViewOfAsync(projectId, PartnerSight.All, db);
        var summary = await SummaryEndpoints.OfAsync(projectId, db);
        var reconciliation = await ReconciliationsEndpoints.Latest(projectId, db);
        var partnerViews = capital.Partners.Select(line => new ReportPartnerView(line.Name, line.Id, line.Balance, line.SharePercent)).ToList();
        var itemIds = summary.Categories.Concat(summary.RevenueCategories).SelectMany(category => category.Items).Where(item => item.Total != 0).Select(item => item.Id).ToList();
        var items = await db.Items.Include(row => row.Subgroups).Include(row => row.Columns).Where(row => itemIds.Contains(row.Id)).ToDictionaryAsync(row => row.Id);
        var itemReports = new List<ItemReportView>();
        foreach (var id in itemIds)
        {
            itemReports.Add(await ItemReportEndpoints.OfAsync(projectId, items[id], previous.IssuedAt, db, _ => true));
        }
        var accounts = reconciliation.Balances.Select(row => new ReportAccountView(row.AccountName, row.Type, row.ActualBalance)).ToList();
        return new PeriodicReportSnapshot(
            project.Name, number, at, previous.ExpensesTotal, figures.Expenses, figures.Capital, figures.VatRefunds, figures.Sales, figures.OtherRevenue, figures.Surplus, figures.SurplusWithoutSales,
            summary.Categories, accounts, reconciliation.ActualTotal, reconciliation.Difference, summary.Waiting.ExpensesCount, summary.Waiting.ExpensesAmount, capital.Total, partnerViews, itemReports,
            summary.RevenueCategories, summary.Waiting.RevenueCount, summary.Waiting.RevenueAmount);
    }
}

/// <summary>A report issued.</summary>
/// <param name="Number">Its number.</param>
/// <param name="IssuedAt">When it was issued.</param>
/// <param name="Expenses">Its expenses.</param>
public record PeriodicReportRowView(int Number, DateTime IssuedAt, decimal Expenses);

/// <summary>What the next report takes.</summary>
/// <param name="Number">Its number.</param>
/// <param name="Waiting">What waits for approval, which it shows as two lines.</param>
public record NextReportView(int Number, WaitingFigures Waiting);
