using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Suppliers;

namespace Redix.Api.Revenue;

/// <summary>The VAT statement the accountant files (UC-056, FR-32): the one owner of which invoices it holds and of the net VAT due, so the statement, its files and a refund's difference cannot disagree.</summary>
public static class VatStatement
{
    /// <summary>The first day of the quarter after the one starting <paramref name="start"/>.</summary>
    public static DateOnly NextQuarter(DateOnly start) => start.AddMonths(3);

    /// <summary>The statement of <paramref name="first"/> to <paramref name="last"/>: a section per quarter it touches, each invoice under the quarter it is declared in (BR-17), by invoice date, and what was left out, saying why.</summary>
    public static async Task<VatStatementView> OfAsync(Guid projectId, DateOnly first, DateOnly last, RedixDbContext db)
    {
        var invoices = await (
            from invoice in db.Invoices
            join entry in db.Entries on invoice.EntryId equals entry.Id
            where entry.ProjectId == projectId && (entry.Status == EntryStatus.Approved || entry.Status == EntryStatus.PendingApproval)
            select new { invoice, entry }).ToListAsync();
        var withoutInvoice = await (
            from expense in db.ExpenseDetails
            join entry in db.Entries on expense.EntryId equals entry.Id
            where entry.ProjectId == projectId && expense.WithoutInvoiceReason != "" && (entry.Status == EntryStatus.Approved || entry.Status == EntryStatus.PendingApproval) && entry.EntryDate >= first && entry.EntryDate <= last
                && !db.EntryLinks.Any(link => link.Type == EntryLinkType.Reversal && link.EntryId == entry.Id)
            select entry).ToListAsync();
        var sales = await (
            from sale in db.Set<SalesInvoice>()
            join entry in db.Entries on sale.EntryId equals entry.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved
            select sale).ToListAsync();
        var facts = await EntryFacts.OfAsync(projectId, [.. invoices.Select(row => row.entry), .. withoutInvoice], db);

        var quarters = new List<VatQuarterView>();
        var exceptions = new List<VatExceptionView>();
        foreach (var entry in withoutInvoice)
        {
            exceptions.Add(new VatExceptionView("withoutInvoice", entry.Id, entry.EntryDate, facts.SupplierNameOf(entry), "", entry.Amount));
        }
        for (var start = Invoice.QuarterStartOf(first); start <= last; start = NextQuarter(start))
        {
            var end = NextQuarter(start).AddDays(-1);
            var quarterStart = start;
            var inQuarter = invoices.Where(row => row.invoice.VatPeriodStart == quarterStart && (row.invoice.InvoiceDate < quarterStart || (row.invoice.InvoiceDate >= first && row.invoice.InvoiceDate <= last))).OrderBy(row => row.invoice.InvoiceDate).ThenBy(row => row.entry.Id).ToList();
            var purchases = new List<VatPurchaseRow>();
            foreach (var row in inQuarter)
            {
                var supplier = facts.SupplierOf(row.entry);
                var exception = RefusalOf(row.entry, row.invoice, supplier);
                if (exception.Length > 0)
                {
                    exceptions.Add(new VatExceptionView(exception, row.entry.Id, row.invoice.InvoiceDate, supplier.Name, row.invoice.Number, row.invoice.Total));
                    continue;
                }
                if (row.invoice.InvoiceDate < quarterStart)
                {
                    exceptions.Add(new VatExceptionView("declaredLater", row.entry.Id, row.invoice.InvoiceDate, supplier.Name, row.invoice.Number, row.invoice.Total));
                }
                var attachmentId = facts.AttachmentIdOf(row.entry);
                if (attachmentId.Length == 0)
                {
                    exceptions.Add(new VatExceptionView("noFile", row.entry.Id, row.invoice.InvoiceDate, supplier.Name, row.invoice.Number, row.invoice.Total));
                }
                var product = facts.ProductOf(row.entry);
                var what = product.Length > 0 ? product : facts.ItemNameOf(row.entry);
                purchases.Add(new VatPurchaseRow(purchases.Count + 1, row.entry.Id, row.invoice.InvoiceDate, supplier.Name, $"{what} / {supplier.Name} / ف{row.invoice.Number}", supplier.VatNumber, row.invoice.AmountBeforeDiscount, row.invoice.Discount, row.invoice.AmountAfterDiscount, row.invoice.Vat, row.invoice.Total, row.invoice.Number, row.invoice.IsCreditNote, InvoiceFigures.RateOf(row.invoice.AmountAfterDiscount, row.invoice.Vat), attachmentId));
            }
            var saleRows = sales.Where(sale => sale.VatPeriodStart == quarterStart && sale.InvoiceDate >= first && sale.InvoiceDate <= last).OrderBy(sale => sale.InvoiceDate).ThenBy(sale => sale.EntryId).Select((sale, index) => new VatSaleRow(index + 1, sale.EntryId, sale.InvoiceDate, sale.BuyerName, sale.BuyerVatNumber, sale.AmountBeforeDiscount, sale.Discount, sale.AmountAfterDiscount, sale.Tax, sale.Total, sale.Number, sale.TaxType)).ToList();
            quarters.Add(new VatQuarterView(quarterStart, end, purchases, saleRows));
        }
        return new VatStatementView(first, last, quarters, exceptions.OrderBy(row => row.Date).ThenBy(row => row.EntryId).ToList());
    }

    /// <summary>The net VAT due of the quarter starting <paramref name="start"/>; negative when it is to be refunded.</summary>
    public static async Task<decimal> NetDueOfAsync(Guid projectId, DateOnly start, RedixDbContext db)
    {
        var statement = await OfAsync(projectId, start, NextQuarter(start).AddDays(-1), db);
        return statement.Quarters.Sum(quarter => quarter.NetDue);
    }

    private static string RefusalOf(Entry entry, Invoice invoice, Supplier supplier)
    {
        if (entry.Status != EntryStatus.Approved)
        {
            return "pending";
        }
        if (supplier.VatNumber.Length == 0)
        {
            return "noVatNumber";
        }
        if (!Supplier.IsValidVatNumber(supplier.VatNumber))
        {
            return "invalidVatNumber";
        }
        return invoice.Vat == 0 ? "notTaxable" : "";
    }
}

/// <summary>A VAT statement.</summary>
/// <param name="From">The first day it covers.</param>
/// <param name="To">The last day it covers.</param>
/// <param name="Quarters">A section per quarter it touches.</param>
/// <param name="Exceptions">What was left out, or needs a look, and why.</param>
public record VatStatementView(DateOnly From, DateOnly To, List<VatQuarterView> Quarters, List<VatExceptionView> Exceptions);

/// <summary>One quarter of a VAT statement. Each total adds the kept four decimals (BR-13), and rounding belongs to whoever shows it.</summary>
/// <param name="Start">Its first day.</param>
/// <param name="End">Its last day.</param>
/// <param name="Purchases">Its purchase invoices, credit notes negative.</param>
/// <param name="Sales">Its sales invoices.</param>
public record VatQuarterView(DateOnly Start, DateOnly End, List<VatPurchaseRow> Purchases, List<VatSaleRow> Sales)
{
    /// <summary>The purchases before VAT, after the discount.</summary>
    public decimal PurchasesAfterDiscount => Purchases.Sum(row => row.AfterDiscount);

    /// <summary>The purchases' VAT.</summary>
    public decimal PurchasesVat => Purchases.Sum(row => row.Vat);

    /// <summary>The purchases with VAT.</summary>
    public decimal PurchasesTotal => Purchases.Sum(row => row.Total);

    /// <summary>The sales' VAT.</summary>
    public decimal SalesVat => Sales.Where(row => row.TaxType == SaleTaxType.Vat).Sum(row => row.Tax);

    /// <summary>The sales' real estate transaction tax, which is not VAT.</summary>
    public decimal RealEstateTax => Sales.Where(row => row.TaxType == SaleTaxType.RealEstateTransaction).Sum(row => row.Tax);

    /// <summary>Sales VAT less purchases VAT; negative when it is to be refunded.</summary>
    public decimal NetDue => SalesVat - PurchasesVat;
}

/// <summary>One purchase invoice of a VAT statement, in the columns of the accountant's file.</summary>
/// <param name="Row">Its line.</param>
/// <param name="EntryId">The entry.</param>
/// <param name="Date">The invoice's date.</param>
/// <param name="CompanyName">The supplier.</param>
/// <param name="Description">What it was, who from, and the invoice's number.</param>
/// <param name="VatNumber">The supplier's VAT number.</param>
/// <param name="BeforeDiscount">The amount before VAT and before the discount.</param>
/// <param name="Discount">The discount.</param>
/// <param name="AfterDiscount">The amount before VAT, after the discount.</param>
/// <param name="Vat">The VAT.</param>
/// <param name="Total">The invoice's total.</param>
/// <param name="InvoiceNumber">Its number.</param>
/// <param name="IsCreditNote">True for a credit note.</param>
/// <param name="Rate">The rate it was computed at.</param>
/// <param name="AttachmentId">Its file; empty when it has none.</param>
public record VatPurchaseRow(int Row, Guid EntryId, DateOnly Date, string CompanyName, string Description, string VatNumber, decimal BeforeDiscount, decimal Discount, decimal AfterDiscount, decimal Vat, decimal Total, string InvoiceNumber, bool IsCreditNote, decimal Rate, string AttachmentId);

/// <summary>One sales invoice of a VAT statement.</summary>
/// <param name="Row">Its line.</param>
/// <param name="EntryId">The entry.</param>
/// <param name="Date">The invoice's date.</param>
/// <param name="BuyerName">The buyer.</param>
/// <param name="VatNumber">The buyer's VAT number.</param>
/// <param name="BeforeDiscount">The amount before tax and before the discount.</param>
/// <param name="Discount">The discount.</param>
/// <param name="AfterDiscount">The amount before tax, after the discount.</param>
/// <param name="Tax">The tax.</param>
/// <param name="Total">The total.</param>
/// <param name="InvoiceNumber">Its number.</param>
/// <param name="TaxType">Which tax it carries.</param>
public record VatSaleRow(int Row, Guid EntryId, DateOnly Date, string BuyerName, string VatNumber, decimal BeforeDiscount, decimal Discount, decimal AfterDiscount, decimal Tax, decimal Total, string InvoiceNumber, SaleTaxType TaxType);

/// <summary>Something left out of a VAT statement, or that needs a look.</summary>
/// <param name="Reason">Why: <c>withoutInvoice</c>, <c>pending</c>, <c>noVatNumber</c>, <c>invalidVatNumber</c>, <c>notTaxable</c>, <c>declaredLater</c> or <c>noFile</c>.</param>
/// <param name="EntryId">The entry.</param>
/// <param name="Date">Its date.</param>
/// <param name="CompanyName">The supplier.</param>
/// <param name="InvoiceNumber">The invoice's number; empty when it has none.</param>
/// <param name="Amount">Its total.</param>
public record VatExceptionView(string Reason, Guid EntryId, DateOnly Date, string CompanyName, string InvoiceNumber, decimal Amount);
