namespace Redix.Api.Entries;

/// <summary>What every expense has, a purchase or a contractor payment: what it was for and how it was paid.</summary>
public class ExpenseDetails : IProjectRow
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The item it is recorded against.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The group inside the item, the general one unless another was chosen.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }
}
