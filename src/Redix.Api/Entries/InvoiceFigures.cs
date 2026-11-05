namespace Redix.Api.Entries;

/// <summary>
/// An invoice's figures, computed as the accountant computes them (BR-01, BR-13, BR-14). This is the one owner of that arithmetic. It runs on the unrounded amounts, and each figure is kept with the four decimals of its column; rounding to two decimals belongs to display and totals only.
/// </summary>
/// <param name="AmountBeforeDiscount">The amount before VAT and before the discount.</param>
/// <param name="Discount">The discount, before VAT.</param>
/// <param name="AmountAfterDiscount">The amount before VAT, after the discount.</param>
/// <param name="Vat">The VAT of this row.</param>
/// <param name="Total">The amount after the discount, with VAT.</param>
public readonly record struct InvoiceFigures(decimal AmountBeforeDiscount, decimal Discount, decimal AmountAfterDiscount, decimal Vat, decimal Total)
{
    private const int KeptDecimals = 4;

    /// <summary>The figures of <paramref name="amount"/>, which <paramref name="includesVat"/> or not, less <paramref name="discount"/>, at <paramref name="vatRate"/> as a fraction; 0 for a supplier not VAT-registered or an invoice not taxable.</summary>
    public static InvoiceFigures Of(decimal amount, bool includesVat, decimal discount, decimal vatRate)
    {
        var beforeDiscount = includesVat ? amount / (1 + vatRate) : amount;
        var afterDiscount = beforeDiscount - discount;
        var vat = afterDiscount * vatRate;
        return new InvoiceFigures(Kept(beforeDiscount), discount, Kept(afterDiscount), Kept(vat), Kept(afterDiscount + vat));
    }

    /// <summary>The rate an invoice with <paramref name="amountAfterDiscount"/> and <paramref name="vat"/> was computed at, as a fraction of the four decimals a rate is kept with; 0 for an invoice that has no VAT.</summary>
    public static decimal RateOf(decimal amountAfterDiscount, decimal vat) => amountAfterDiscount == 0 ? 0 : decimal.Round(vat / amountAfterDiscount, KeptDecimals, MidpointRounding.AwayFromZero);

    /// <summary>These figures as a credit note holds them (BR-18): every one negative, so that wherever figures are added up it takes away what the original counted, and no reader has to ask what it is.</summary>
    public InvoiceFigures Negated() => new(-AmountBeforeDiscount, -Discount, -AmountAfterDiscount, -Vat, -Total);

    private static decimal Kept(decimal figure) => decimal.Round(figure, KeptDecimals, MidpointRounding.AwayFromZero);
}
