using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Config;
using Redix.Api.Contracts;
using Redix.Api.Revenue;

namespace Redix.Api.Reports;

/// <summary>The report files besides the periodic report's (FR-33, UC-057, UC-058): for whoever sees every partner's capital, since only they export (BR-31).</summary>
public static class ReportFilesEndpoints
{
    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>Maps the routes.</summary>
    /// <param name="app">The app.</param>
    public static void MapReportFiles(this WebApplication app)
    {
        var files = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.SeeAllCapital));
        files.MapGet("/vat-statement/xlsx", VatExcel);
        files.MapGet("/reports/items/{itemId:guid}/pdf", (Guid projectId, Guid itemId, RedixDbContext db) => ItemFileAsync(projectId, itemId, db, true));
        files.MapGet("/reports/items/{itemId:guid}/xlsx", (Guid projectId, Guid itemId, RedixDbContext db) => ItemFileAsync(projectId, itemId, db, false));
        files.MapGet("/reports/contracts/pdf", ContractsPdf);
        files.MapGet("/reports/contracts/xlsx", ContractsExcel);
    }

    /// <summary>The كشف الضريبة from <paramref name="from"/> to <paramref name="to"/> as the accountant's workbook, at the rate of the config value <c>vatRatePercent</c> (D18, D19). A period ending before it starts answers <c>invalidPeriod</c>.</summary>
    public static async Task<IResult> VatExcel(Guid projectId, DateOnly from, DateOnly to, RedixDbContext db)
    {
        if (to < from)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPeriod");
        }
        var rate = decimal.Parse(await db.ConfigValues.Where(row => row.Key == ConfigValue.VatRatePercent).Select(row => row.Value).SingleAsync(), CultureInfo.InvariantCulture);
        return Results.File(VatStatementFiles.Excel(await VatStatement.OfAsync(projectId, from, to, db), rate), ExcelType, $"vat-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.xlsx");
    }

    /// <summary>The contracts report as a PDF.</summary>
    public static async Task<IResult> ContractsPdf(Guid projectId, RedixDbContext db)
    {
        var name = await db.Projects.Where(project => project.Id == projectId).Select(project => project.Name).SingleAsync();
        return Results.File(ItemReportFiles.ContractsPdf(name, await ContractsEndpoints.List(projectId, db)), "application/pdf", "contracts.pdf");
    }

    /// <summary>The contracts report as a workbook.</summary>
    public static async Task<IResult> ContractsExcel(Guid projectId, RedixDbContext db) =>
        Results.File(ItemReportFiles.ContractsExcel(await ContractsEndpoints.List(projectId, db)), ExcelType, "contracts.xlsx");

    private static async Task<IResult> ItemFileAsync(Guid projectId, Guid itemId, RedixDbContext db, bool isPdf)
    {
        var item = await db.Items.Include(row => row.Subgroups).Include(row => row.Columns).SingleOrDefaultAsync(row => row.Id == itemId && db.Categories.Any(category => category.Id == row.CategoryId && category.ProjectId == projectId));
        if (item is null)
        {
            return Results.NotFound();
        }
        var report = await ItemReportEndpoints.OfAsync(projectId, item, await ItemReportEndpoints.NewSinceAsync(projectId, db), db);
        return isPdf ? Results.File(ItemReportFiles.Pdf(report), "application/pdf", "item.pdf") : Results.File(ItemReportFiles.Excel(report), ExcelType, "item.xlsx");
    }
}
