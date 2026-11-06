namespace Redix.Api.Entries;

/// <summary>What the app sends to record a purchase expense. It sends what was typed; every figure is computed here, never sent. It owns which amounts and texts are acceptable, and which of an item's columns it answers.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public record ExpenseRequest(Guid Id) : EntryRequest(Id)
{
    /// <summary>The item.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The group inside the item; left out for its general group.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The unit.</summary>
    public Guid UnitId { get; init; }

    /// <summary>The supplier, from the project's register.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>What was bought; left out when not said.</summary>
    public string Product { get; init; } = "";

    /// <summary>The price of one, including VAT (BR-01); left out when not used.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>How many; left out when not used.</summary>
    public decimal Quantity { get; init; }

    /// <summary>True for a credit note (BR-18): its own number and date, and <see cref="Amount"/> typed as a plain amount, which is kept negative.</summary>
    public bool IsCreditNote { get; init; }

    /// <summary>The entry of the invoice a credit note returns; left out when it is linked to none, which BR-18 allows.</summary>
    public Guid OriginalEntryId { get; init; }

    /// <summary>The amount; left out, it is the unit price times the quantity. Negative for a deduction (FR-14), which has no invoice and says why in <see cref="EntryRequest.WithoutInvoiceReason"/>.</summary>
    public decimal Amount { get; init; }

    /// <summary>Whether <see cref="Amount"/> includes VAT, the default and what a QR code gives (BR-01).</summary>
    public bool AmountIncludesVat { get; init; } = true;

    /// <summary>The discount, before VAT; left out when none.</summary>
    public decimal Discount { get; init; }

    /// <summary>True for an invoice not taxable, whose VAT is zero (BR-14).</summary>
    public bool IsNotTaxable { get; init; }

    /// <summary>What was paid; left out when it is the invoice's total (BR-24).</summary>
    public decimal PaymentAmount { get; init; }

    /// <summary>Each unit's part, for an expense recorded against the shared unit (FR-15); left out when it is not split.</summary>
    public List<UnitShareRequest> UnitShares { get; init; } = [];

    /// <summary>Whether this is a deduction: a negative amount, which FR-14 allows and which comes off the item.</summary>
    public bool IsDeduction() => Amount < 0;

    /// <summary>The amount as typed, or else what the unit price and quantity make: which BR-01 says includes VAT.</summary>
    public decimal EnteredAmount() => Amount != 0 ? Amount : UnitPrice * Quantity;

    /// <summary>Whether <see cref="EnteredAmount"/> includes VAT: a unit price always does (BR-01).</summary>
    public bool EnteredAmountIncludesVat() => Amount == 0 || AmountIncludesVat;

    /// <summary>What was paid, given the <paramref name="kept"/> figures: the invoice's total unless the payment differs (BR-24). A credit note's payment is money coming back, so it is negative as its figures are.</summary>
    public decimal PaidOf(InvoiceFigures kept)
    {
        if (PaymentAmount == 0)
        {
            return kept.Total;
        }
        return IsCreditNote ? -PaymentAmount : PaymentAmount;
    }

    /// <summary>Why the texts and typed amounts cannot be kept: each amount is not negative, has at most two decimals and stays within <see cref="Money.Largest"/>. Empty when they can.</summary>
    public string RefusalOfTyped()
    {
        var common = RefusalOfCommon();
        if (common.Length > 0)
        {
            return common;
        }
        if (IsDeduction() && !IsWithoutInvoice())
        {
            return "DeductionNeedsReason";
        }
        if (IsCreditNote && IsWithoutInvoice())
        {
            return "CreditNoteNeedsInvoice";
        }
        if (!IsCreditNote && OriginalEntryId != Guid.Empty)
        {
            return "UnknownOriginalInvoice";
        }
        if (InvoiceNumber.Trim().Length > Invoice.NumberMaxLength)
        {
            return "InvalidInvoiceNumber";
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
        if (!Money.IsAcceptable(Amount) || (Amount == 0 && !HasPriceAndQuantityWithinLimit))
        {
            return "InvalidAmount";
        }
        if (Discount < 0 || !Money.IsAcceptable(Discount) || (IsDeduction() && Discount > 0))
        {
            return "InvalidDiscount";
        }
        return PaymentAmount < 0 || !Money.IsAcceptable(PaymentAmount) || (IsDeduction() && PaymentAmount > 0) ? "InvalidPaymentAmount" : "";
    }

    /// <summary>Why <paramref name="figures"/> cannot be kept: a discount above the amount, or a total beyond <see cref="Money.Largest"/> either way. Empty when they can.</summary>
    public string RefusalOfFigures(InvoiceFigures figures)
    {
        if (figures.AmountAfterDiscount < 0 && !IsDeduction())
        {
            return "InvalidDiscount";
        }
        return Math.Abs(figures.Total) > Money.Largest ? "InvalidAmount" : "";
    }

    /// <inheritdoc />
    public override EntryAnswers Answers() => new(EntryKind.Purchase, Product.Trim(), UnitPrice, Quantity, Notes.Trim(), CustomValuesKept());

    private bool HasPriceAndQuantityWithinLimit => UnitPrice > 0 && Quantity > 0 && UnitPrice <= Money.Largest / Quantity;
}
