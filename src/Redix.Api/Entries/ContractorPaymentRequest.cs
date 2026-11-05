namespace Redix.Api.Entries;

/// <summary>What the app sends to record a payment against a contract (FR-12). The contract says the item, the unit and the contractor. It owns which amounts and texts are acceptable.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public record ContractorPaymentRequest(Guid Id) : EntryRequest(Id)
{
    /// <summary>The contract.</summary>
    public Guid ContractId { get; init; }

    /// <summary>The subgroup of the contract's item; empty means its general group.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>True for a payment outside the contract, which counts in the item's total and leaves the remaining alone (FR-13).</summary>
    public bool IsOutsideContract { get; init; }

    /// <summary>What was paid. Negative for a deduction (FR-14), which says why in <see cref="EntryRequest.WithoutInvoiceReason"/>.</summary>
    public decimal Amount { get; init; }

    /// <summary>The amount before VAT of its tax invoice, whose VAT is computed from it; used only with <see cref="EntryRequest.InvoiceNumber"/>.</summary>
    public decimal InvoiceAmountBeforeVat { get; init; }

    /// <summary>Whether this is a deduction: no money moved, so there is no receipt.</summary>
    public bool IsDeduction() => Amount < 0;

    /// <summary>Whether it carries a tax invoice, which enters the VAT statement.</summary>
    public bool HasInvoice() => InvoiceNumber.Trim().Length > 0;

    /// <inheritdoc />
    public override EntryAnswers Answers() => new(EntryKind.ContractorPayment, "", 0, 0, Notes.Trim(), CustomValuesKept());

    /// <summary>Why the texts and typed amounts cannot be kept. Empty when they can.</summary>
    public string RefusalOfTyped()
    {
        var common = RefusalOfCommon();
        if (common.Length > 0)
        {
            return common;
        }
        if (Amount == 0 || !Money.IsAcceptable(Amount))
        {
            return "InvalidAmount";
        }
        if (IsDeduction() != IsWithoutInvoice())
        {
            return IsDeduction() ? "DeductionNeedsReason" : "InvalidWithoutInvoiceReason";
        }
        if (InvoiceNumber.Trim().Length > Invoice.NumberMaxLength || (IsDeduction() && HasInvoice()))
        {
            return "InvalidInvoiceNumber";
        }
        return HasInvoice() && (InvoiceAmountBeforeVat <= 0 || !Money.IsAcceptable(InvoiceAmountBeforeVat)) ? "InvalidInvoiceAmount" : "";
    }

    /// <summary>Why its dates cannot be kept: outside the project's period or in the future (BR-12). Empty when they can.</summary>
    public string RefusalOfDates(DateOnly projectStart, DateOnly today)
    {
        if (!Entry.IsAcceptableDate(PaymentDate, projectStart, today))
        {
            return "InvalidPaymentDate";
        }
        return HasInvoice() && !Entry.IsAcceptableDate(InvoiceDate, projectStart, today) ? "InvalidInvoiceDate" : "";
    }
}
