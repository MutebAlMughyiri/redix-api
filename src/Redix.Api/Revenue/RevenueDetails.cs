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

/// <summary>The invoice of a sale, which feeds the sales section of the كشف الضريبة (FR-42, D22). It belongs to the sale's revenue entry, so its total is counted once.</summary>
public class SalesInvoice
{
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

    /// <summary>What the buyer paid.</summary>
    public decimal Total { get; init; }
}
