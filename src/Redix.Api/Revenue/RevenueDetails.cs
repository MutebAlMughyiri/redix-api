namespace Redix.Api.Revenue;

/// <summary>What only revenue has: the revenue item it came in on, whose type says what it is (FR-28, FR-38).</summary>
public class RevenueDetails
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The revenue item.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The item's subgroup, its general one unless another was chosen.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The first day of the period it is for.</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>The last day of the period it is for.</summary>
    public DateOnly PeriodEnd { get; init; }
}

/// <summary>Which tax a sale carries. BRS open question 6 waits for the accountant, so both exist, the real estate transaction tax being the default.</summary>
public enum SaleTaxType : byte
{
    /// <summary>The real estate transaction tax, which is not VAT, so it stays out of the net VAT due.</summary>
    RealEstateTransaction,

    /// <summary>VAT, at the project's rate.</summary>
    Vat,
}

/// <summary>The invoice of a sale, which feeds the sales section of the VAT statement (FR-42). It belongs to the sale's revenue entry, so its amount is counted once.</summary>
public class SalesInvoice
{
    /// <summary>The real estate transaction tax's rate.</summary>
    public const decimal RealEstateTransactionRate = 0.05m;

    /// <summary>The longest buyer's name.</summary>
    public const int BuyerNameMaxLength = 200;

    /// <summary>The revenue entry of the sale.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The buyer.</summary>
    public string BuyerName { get; init; } = "";

    /// <summary>The buyer's VAT number; empty when they have none.</summary>
    public string BuyerVatNumber { get; init; } = "";

    /// <summary>Its number.</summary>
    public string Number { get; init; } = "";

    /// <summary>Its date.</summary>
    public DateOnly InvoiceDate { get; init; }

    /// <summary>The amount before tax and before the discount.</summary>
    public decimal AmountBeforeDiscount { get; init; }

    /// <summary>The discount.</summary>
    public decimal Discount { get; init; }

    /// <summary>The amount before tax, after the discount.</summary>
    public decimal AmountAfterDiscount { get; init; }

    /// <summary>The tax of this row, never recomputed.</summary>
    public decimal Tax { get; init; }

    /// <summary>The amount after the discount, with tax.</summary>
    public decimal Total { get; init; }

    /// <summary>Which tax it carries.</summary>
    public SaleTaxType TaxType { get; init; }

    /// <summary>The first day of the quarter it is declared in.</summary>
    public DateOnly VatPeriodStart { get; init; }
}
