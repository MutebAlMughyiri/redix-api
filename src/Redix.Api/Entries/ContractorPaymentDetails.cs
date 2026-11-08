namespace Redix.Api.Entries;

/// <summary>What only a contractor payment has: the contract it is against, and whether it counts on that contract's remaining (BR-04).</summary>
public class ContractorPaymentDetails
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The contract, which says the contractor and the item.</summary>
    public Guid ContractId { get; init; }

    /// <summary>True for a payment that counts in the item's total and leaves the contract's remaining alone (FR-13).</summary>
    public bool IsOutsideContract { get; init; }
}
