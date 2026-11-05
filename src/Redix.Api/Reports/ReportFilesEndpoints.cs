using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Redix.Api.Attachments;
using Redix.Api.Auth;
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
        files.MapGet("/vat-statement/pdf", VatPdf);
        files.MapGet("/vat-statement/zip", VatZip);
        files.MapGet("/reports/items/{itemId:guid}/pdf", (Guid projectId, Guid itemId, RedixDbContext db) => ItemFileAsync(projectId, itemId, db, true));
        files.MapGet("/reports/items/{itemId:guid}/xlsx", (Guid projectId, Guid itemId, RedixDbContext db) => ItemFileAsync(projectId, itemId, db, false));
        files.MapGet("/reports/contracts/pdf", ContractsPdf);
        files.MapGet("/reports/contracts/xlsx", ContractsExcel);
    }

    /// <summary>The VAT statement from <paramref name="from"/> to <paramref name="to"/> as the accountant's workbook.</summary>
    public static async Task<IResult> VatExcel(Guid projectId, DateOnly from, DateOnly to, RedixDbContext db) =>
        Results.File(VatStatementFiles.Excel(await VatStatement.OfAsync(projectId, from, to, db)), ExcelType, $"vat-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.xlsx");

    /// <summary>The VAT statement from <paramref name="from"/> to <paramref name="to"/> as a PDF.</summary>
    public static async Task<IResult> VatPdf(Guid projectId, DateOnly from, DateOnly to, RedixDbContext db) =>
        Results.File(VatStatementFiles.Pdf(await VatStatement.OfAsync(projectId, from, to, db)), "application/pdf", $"vat-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.pdf");

    /// <summary>The invoice files of the VAT statement from <paramref name="from"/> to <paramref name="to"/>, one ZIP.</summary>
    public static async Task<IResult> VatZip(Guid projectId, DateOnly from, DateOnly to, RedixDbContext db, IOptions<AttachmentOptions> options)
    {
        var statement = await VatStatement.OfAsync(projectId, from, to, db);
        var attachments = await db.Attachments.Where(attachment => attachment.ProjectId == projectId).ToDictionaryAsync(attachment => attachment.Id.ToString());
        return Results.File(VatStatementFiles.Zip(statement, id => (options.Value.PathOf(attachments[id]), attachments[id].FileName)), "application/zip", $"vat-invoices-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.zip");
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
