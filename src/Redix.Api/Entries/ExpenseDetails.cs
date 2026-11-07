using System.Text.Json;

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

    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>Why there is no invoice; empty when there is one (FR-09).</summary>
    public string WithoutInvoiceReason { get; set; } = "";

    /// <summary>The values of the item's custom columns (FR-41), a JSON object of each column's key and its value.</summary>
    public string CustomValues { get; init; } = "{}";

    /// <summary>The values of the item's custom columns, by each column's key.</summary>
    public Dictionary<string, string> CustomValuesByKey() => JsonSerializer.Deserialize<Dictionary<string, string>>(CustomValues)!;
}
