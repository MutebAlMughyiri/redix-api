using System.Security.Claims;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Partners;
using Redix.Api.Config;
using Redix.Api.Contracts;
using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>
/// The report files besides the periodic report's (FR-33, UC-057, UC-058): for whoever holds <see cref="Permission.GenerateReports"/>, and a file's رأس المال holds only the partners its
/// reader may see (redix-api #131, D1, D2, BR-31).
/// </summary>
public static class ReportFilesEndpoints
{
    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>Maps the routes.</summary>
    /// <param name="app">The app.</param>
    public static void MapReportFiles(this WebApplication app)
    {
        var files = app.MapGroup("/projects/{projectId:guid}").RequireAuthorization(nameof(Permission.GenerateReports));
        files.MapGet("/vat-statement/xlsx", VatExcel);
        files.MapGet("/reports/items/{itemId:guid}/pdf", (Guid projectId, Guid itemId, ClaimsPrincipal principal, RedixDbContext db) => ItemFileAsync(projectId, itemId, principal, db, true));
        files.MapGet("/reports/items/{itemId:guid}/xlsx", (Guid projectId, Guid itemId, ClaimsPrincipal principal, RedixDbContext db) => ItemFileAsync(projectId, itemId, principal, db, false));
        files.MapGet("/reports/contracts/pdf", ContractsPdf);
        files.MapGet("/reports/contracts/xlsx", ContractsExcel);
        files.MapGet("/reports/contracts/{contractId:guid}/pdf", (Guid projectId, Guid contractId, RedixDbContext db) => ContractFileAsync(projectId, contractId, db, true));
        files.MapGet("/reports/contracts/{contractId:guid}/xlsx", (Guid projectId, Guid contractId, RedixDbContext db) => ContractFileAsync(projectId, contractId, db, false));
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
    public static async Task<IResult> ContractsPdf(Guid projectId, RedixDbContext db) =>
        Results.File(ItemReportFiles.ContractsPdf(await NameOfAsync(projectId, db), await ContractsEndpoints.List(projectId, db)), "application/pdf", "contracts.pdf");

    /// <summary>The contracts report as a workbook.</summary>
    public static async Task<IResult> ContractsExcel(Guid projectId, RedixDbContext db) =>
        Results.File(ItemReportFiles.ContractsExcel(await NameOfAsync(projectId, db), await ContractsEndpoints.List(projectId, db)), ExcelType, "contracts.xlsx");

    /// <summary>One contract's report as a PDF or a workbook, from the item report's own writers (Muteb, 12 Nov 2026); a contract of another project is not found.</summary>
    private static async Task<IResult> ContractFileAsync(Guid projectId, Guid contractId, RedixDbContext db, bool isPdf)
    {
        var report = await ItemReportEndpoints.OfContractAsync(projectId, contractId, db);
        if (report == ItemReportView.None)
        {
            return Results.NotFound();
        }
        return isPdf ? Results.File(ItemReportFiles.Pdf(report), "application/pdf", "contract.pdf") : Results.File(ItemReportFiles.Excel(await NameOfAsync(projectId, db), report), ExcelType, "contract.xlsx");
    }

    /// <summary>An item's report as a PDF or a workbook; رأس المال's holds only the partners the caller may see, its total the whole capital (redix-api #131 D1, BR-31).</summary>
    private static async Task<IResult> ItemFileAsync(Guid projectId, Guid itemId, ClaimsPrincipal principal, RedixDbContext db, bool isPdf)
    {
        var item = await ProjectStructure.ItemWithTablesAsync(projectId, itemId, db);
        if (item == Item.None)
        {
            return Results.NotFound();
        }
        var sight = await PartnerSight.OfAsync(principal, projectId, db);
        var report = await ItemReportEndpoints.OfAsync(projectId, item, (await PeriodicReport.LatestOfAsync(projectId, db)).IssuedAt, db, sight.Sees);
        return isPdf ? Results.File(ItemReportFiles.Pdf(report), "application/pdf", "item.pdf") : Results.File(ItemReportFiles.Excel(await NameOfAsync(projectId, db), report), ExcelType, "item.xlsx");
    }

    /// <summary>The project's name, which its files print.</summary>
    private static Task<string> NameOfAsync(Guid projectId, RedixDbContext db) => db.Projects.Where(project => project.Id == projectId).Select(project => project.Name).SingleAsync();
}
