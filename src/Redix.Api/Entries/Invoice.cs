namespace Redix.Api.Entries;

/// <summary>The invoice of an expense, which the VAT statement goes by (BR-24): its number, its date and its figures, each kept with four decimals (BR-13).</summary>
public class Invoice
{
    /// <summary>The longest invoice number.</summary>
    public const int NumberMaxLength = 50;

    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The invoice's number.</summary>
    public string Number { get; init; } = "";

    /// <summary>The invoice's date.</summary>
    public DateOnly InvoiceDate { get; init; }

    /// <summary>The amount before VAT and before the discount; this and every figure below it is negative on a credit note (BR-18).</summary>
    public decimal AmountBeforeDiscount { get; init; }

    /// <summary>The discount, before VAT.</summary>
    public decimal Discount { get; init; }

    /// <summary>The amount before VAT, after the discount.</summary>
    public decimal AmountAfterDiscount { get; init; }

    /// <summary>The VAT, computed on this row and never recomputed, so a later change of the rate leaves it alone.</summary>
    public decimal Vat { get; init; }

    /// <summary>The amount after the discount, with VAT.</summary>
    public decimal Total { get; init; }

    /// <summary>True for a credit note (BR-18). Its figures are negative, so a total needs no check of this.</summary>
    public bool IsCreditNote { get; init; }

    /// <summary>The seller's VAT number on its QR code, when attached later (UC-039): what Finance compares with the supplier's. Empty otherwise.</summary>
    public string SellerVatNumber { get; init; } = "";

    /// <summary>The first day of the quarter it is declared in, by default the quarter of its date (BR-17).</summary>
    public DateOnly VatPeriodStart { get; set; }

    /// <summary>The first day of the quarter <paramref name="date"/> falls in.</summary>
    public static DateOnly QuarterStartOf(DateOnly date) => new(date.Year, (date.Month - 1) / 3 * 3 + 1, 1);
}
