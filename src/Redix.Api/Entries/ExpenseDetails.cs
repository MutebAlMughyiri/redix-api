namespace Redix.Api.Entries;

/// <summary>What every expense has, a purchase or a contractor payment: what it was for and how it was paid.</summary>
public class ExpenseDetails
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The item it is recorded against.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The group inside the item, the general one unless another was chosen.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The unit it is for, which may be the shared one (BRS open question 3).</summary>
    public Guid UnitId { get; init; }

    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>What the bank charged on top; 0 when nothing (BR-24).</summary>
    public decimal BankFees { get; init; }

    /// <summary>Why there is no invoice; empty when there is one (FR-09).</summary>
    public string WithoutInvoiceReason { get; init; } = "";

    /// <summary>What becomes of it when paid from a partner's money (BR-25).</summary>
    public PartnerTreatment PartnerTreatment { get; init; } = PartnerTreatment.Empty;

    /// <summary>The values of the item's custom columns (FR-41), a JSON object of each column's key and its value.</summary>
    public string CustomValues { get; init; } = "{}";
}
