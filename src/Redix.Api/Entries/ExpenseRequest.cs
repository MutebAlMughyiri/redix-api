using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What the app sends to record a purchase expense. It sends what was typed; every figure is computed here, never sent. It owns which amounts and texts are acceptable, and which of an item's columns it answers.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public record ExpenseRequest(Guid Id)
{
    /// <summary>The item.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The group inside the item; left out for its general group.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The unit.</summary>
    public Guid UnitId { get; init; }

    /// <summary>The supplier, from the project's register.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>The invoice's number; left out for an expense without an invoice.</summary>
    public string InvoiceNumber { get; init; } = "";

    /// <summary>The invoice's date; left out for an expense without an invoice.</summary>
    public DateOnly InvoiceDate { get; init; }

    /// <summary>Why there is no invoice; left out when there is one (FR-09). An expense that says why has no invoice number, no file and no VAT, and its date is <see cref="PaymentDate"/>.</summary>
    public string WithoutInvoiceReason { get; init; } = "";

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

    /// <summary>The amount; left out, it is the unit price times the quantity. Negative for a deduction (FR-14), which has no invoice and says why in <see cref="WithoutInvoiceReason"/>.</summary>
    public decimal Amount { get; init; }

    /// <summary>Whether <see cref="Amount"/> includes VAT, the default and what a QR code gives (BR-01).</summary>
    public bool AmountIncludesVat { get; init; } = true;

    /// <summary>The discount, before VAT; left out when none.</summary>
    public decimal Discount { get; init; }

    /// <summary>True for an invoice not taxable, whose VAT is zero (BR-14).</summary>
    public bool IsNotTaxable { get; init; }

    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>The account it was paid from.</summary>
    public Guid AccountId { get; init; }

    /// <summary>The day it was paid; left out when it is the invoice's date (BR-24), and required without an invoice.</summary>
    public DateOnly PaymentDate { get; init; }

    /// <summary>What was paid; left out when it is the invoice's total (BR-24).</summary>
    public decimal PaymentAmount { get; init; }

    /// <summary>What the bank charged on top; left out when nothing (BR-24).</summary>
    public decimal BankFees { get; init; }

    /// <summary>Notes on it; left out when none.</summary>
    public string Notes { get; init; } = "";

    /// <summary>The values of the item's custom columns, by each column's key (FR-41).</summary>
    public Dictionary<string, string> CustomValues { get; init; } = [];

    /// <summary>The file of the invoice, already kept; left out when there is none yet.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>True to send it for approval; otherwise it is saved as a draft.</summary>
    public bool Submit { get; init; }

    /// <summary>True once the person has seen that this supplier's invoice number exists already, and saves it anyway (FR-17).</summary>
    public bool ConfirmDuplicate { get; init; }

    /// <summary>Whether this expense says why it has no invoice (FR-09).</summary>
    public bool IsWithoutInvoice() => WithoutInvoiceReason.Trim().Length > 0;

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

    /// <summary>The custom values to keep: each trimmed, and the empty ones gone.</summary>
    public Dictionary<string, string> CustomValuesKept() => CustomValues.Where(value => value.Value.Trim().Length > 0).ToDictionary(value => value.Key, value => value.Value.Trim());

    /// <summary>Why the texts and typed amounts cannot be kept: each amount is not negative, has at most two decimals and stays within <see cref="Money.Largest"/>. Empty when they can.</summary>
    public string RefusalOfTyped()
    {
        if (WithoutInvoiceReason.Trim().Length > Project.ReasonMaxLength)
        {
            return "InvalidWithoutInvoiceReason";
        }
        if (IsWithoutInvoice() && AttachmentId != Guid.Empty)
        {
            return "InvoiceAndReason";
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
        if (!IsWithoutInvoice() && InvoiceNumber.Trim().Length is 0 or > Invoice.NumberMaxLength)
        {
            return "InvalidInvoiceNumber";
        }
        if (Product.Trim().Length > PurchaseDetails.ProductMaxLength)
        {
            return "InvalidProduct";
        }
        if (Notes.Trim().Length > Entry.NotesMaxLength)
        {
            return "InvalidNotes";
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
        if (PaymentAmount < 0 || !Money.IsAcceptable(PaymentAmount) || (IsDeduction() && PaymentAmount > 0))
        {
            return "InvalidPaymentAmount";
        }
        return BankFees < 0 || !Money.IsAcceptable(BankFees) ? "InvalidBankFees" : "";
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

    /// <summary>Whether this request gives <paramref name="column"/> a value: what is checked elsewhere counts as given, and only what a person may leave empty is asked here (FR-41).</summary>
    public bool Answers(ItemColumn column) => column.Key switch
    {
        "product" => Product.Trim().Length > 0,
        "unitPrice" => UnitPrice > 0,
        "quantity" => Quantity > 0,
        "notes" => Notes.Trim().Length > 0,
        _ => !column.IsCustom || CustomValuesKept().ContainsKey(column.Key),
    };

    private bool HasPriceAndQuantityWithinLimit => UnitPrice > 0 && Quantity > 0 && UnitPrice <= Money.Largest / Quantity;
}
