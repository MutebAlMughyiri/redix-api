using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What an entry holds that an item's columns can require (FR-41), wherever it is read from: a request being recorded, or the rows of a draft being sent. It owns which column each of these answers.</summary>
/// <param name="Kind">What the entry is: what only a purchase holds is never asked of anything else.</param>
/// <param name="Product">What was bought; empty when not said.</param>
/// <param name="UnitPrice">The price of one; 0 when not used.</param>
/// <param name="Quantity">How many; 0 when not used.</param>
/// <param name="Notes">The notes; empty when none.</param>
/// <param name="CustomValues">The values of the item's custom columns, by each column's key.</param>
public record EntryAnswers(EntryKind Kind, string Product, decimal UnitPrice, decimal Quantity, string Notes, Dictionary<string, string> CustomValues)
{
    /// <summary>The keys of the required ones among <paramref name="columns"/> this holds no value for, in their order.</summary>
    public List<string> MissingOf(IEnumerable<ItemColumn> columns) => columns.Where(column => column.IsRequired && !Gives(column)).Select(column => column.Key).ToList();

    /// <summary>Whether this gives <paramref name="column"/> a value: what is checked elsewhere counts as given, and only what a person may leave empty is asked here (FR-41).</summary>
    private bool Gives(ItemColumn column) => column.Key switch
    {
        "product" => Kind != EntryKind.Purchase || Product.Length > 0,
        "unitPrice" => Kind != EntryKind.Purchase || UnitPrice > 0,
        "quantity" => Kind != EntryKind.Purchase || Quantity > 0,
        "notes" => Notes.Length > 0,
        _ => !column.IsCustom || CustomValues.ContainsKey(column.Key),
    };
}
