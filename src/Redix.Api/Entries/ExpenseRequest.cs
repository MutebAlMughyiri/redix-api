namespace Redix.Api.Entries;

/// <summary>What the app sends to record a purchase expense: a total the project spent, on an item, from a supplier, on a day (redix-api #96). It owns which amounts and texts are acceptable, and which of an item's columns it answers.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public record ExpenseRequest(Guid Id) : EntryRequest(Id)
{
    /// <summary>The item.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The group inside the item; left out for its general group.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The supplier, from the project's register.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>What was bought; left out when not said.</summary>
    public string Product { get; init; } = "";

    /// <summary>The price of one; left out when not used.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>How many; left out when not used.</summary>
    public decimal Quantity { get; init; }

    /// <summary>True for a credit note: a total coming back to the bill <see cref="OriginalEntryId"/> names, typed as a plain amount and kept negative (D13).</summary>
    public bool IsCreditNote { get; init; }

    /// <summary>The bill a credit note returns; required for a credit note, and only for one (D13).</summary>
    public Guid OriginalEntryId { get; init; }

    /// <summary>The total; left out, it is the unit price times the quantity. Negative for a deduction (FR-14), which says why in its notes (D14).</summary>
    public decimal Amount { get; init; }

    /// <inheritdoc />
    public override decimal Total() => Amount != 0 ? Amount : HasPriceAndQuantityWithinLimit ? UnitPrice * Quantity : 0;

    /// <summary>The total as the ledger keeps it: a credit note's is negative, so that wherever totals are added up it takes away what its bill counted.</summary>
    public decimal KeptTotal() => IsCreditNote ? -Total() : Total();

    /// <summary>Why the texts and typed amounts cannot be kept: each amount is not negative, has at most two decimals and stays within <see cref="Money.Largest"/>. Empty when they can.</summary>
    public string RefusalOfTyped()
    {
        var common = RefusalOfCommon();
        if (common.Length > 0)
        {
            return common;
        }
        if (IsCreditNote != (OriginalEntryId != Guid.Empty))
        {
            return IsCreditNote ? "CreditNoteNeedsOriginal" : "UnknownOriginalInvoice";
        }
        if (Product.Trim().Length > PurchaseDetails.ProductMaxLength)
        {
            return "InvalidProduct";
        }
        if (UnitPrice < 0 || !Money.IsAcceptable(UnitPrice))
        {
            return "InvalidUnitPrice";
        }
        if (Quantity < 0 || !Money.IsAcceptable(Quantity))
        {
            return "InvalidQuantity";
        }
        return !Money.IsAcceptable(Amount) || (Amount == 0 && !HasPriceAndQuantityWithinLimit) || (IsCreditNote && IsDeduction()) ? "InvalidAmount" : "";
    }

    /// <inheritdoc />
    public override EntryAnswers Answers() => new(EntryKind.Purchase, Product.Trim(), UnitPrice, Quantity, Notes.Trim(), CustomValuesKept());

    private bool HasPriceAndQuantityWithinLimit => UnitPrice > 0 && Quantity > 0 && UnitPrice <= Money.Largest / Quantity;
}
