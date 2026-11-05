namespace Redix.Api.Entries;

/// <summary>What only a purchase has: who it was bought from (BR-19) and what was bought.</summary>
public class PurchaseDetails
{
    /// <summary>The longest product name a purchase may have.</summary>
    public const int ProductMaxLength = 200;

    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The supplier, from the project's register; never free text (BR-19).</summary>
    public Guid SupplierId { get; init; }

    /// <summary>What was bought (الصنف); empty when not said.</summary>
    public string Product { get; init; } = "";

    /// <summary>The price of one, including VAT as in the accountant's statement (BR-01); 0 when not used.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>How many; 0 when not used.</summary>
    public decimal Quantity { get; init; }
}
