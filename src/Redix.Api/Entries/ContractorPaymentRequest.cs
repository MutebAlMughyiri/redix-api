namespace Redix.Api.Entries;

/// <summary>What the app sends to record a payment against a contract (FR-12): the contract says the item and the contractor, and the payment goes under the item's general group, its contract being its table (redix-app #84). A deduction says why in its notes (D14), and the receipt is optional (D16). It owns which amounts and texts are acceptable.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public record ContractorPaymentRequest(Guid Id) : EntryRequest(Id)
{
    /// <summary>The contract.</summary>
    public Guid ContractId { get; init; }

    /// <summary>True for a payment outside the contract, which counts in the item's total and leaves the remaining alone (FR-13).</summary>
    public bool IsOutsideContract { get; init; }

    /// <summary>What was paid. Negative for a deduction (FR-14).</summary>
    public decimal Amount { get; init; }

    /// <inheritdoc />
    public override decimal Total() => Amount;

    /// <inheritdoc />
    public override EntryAnswers Answers() => new(EntryKind.ContractorPayment, "", 0, 0, Notes.Trim(), CustomValuesKept());

    /// <summary>Why the texts and the amount cannot be kept. Empty when they can.</summary>
    public string RefusalOfTyped()
    {
        var common = RefusalOfCommon();
        if (common.Length > 0)
        {
            return common;
        }
        return Amount == 0 || !Money.IsAcceptable(Amount) ? "InvalidAmount" : "";
    }
}
