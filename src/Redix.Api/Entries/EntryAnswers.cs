using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>What an entry holds that an item's columns can require (FR-41), wherever it is read from: a request being recorded, or the entry of a draft being sent. It owns which column each of these answers.</summary>
/// <param name="Description">The description; empty when none.</param>
/// <param name="ColumnValues">The values of the item's columns the entry holds in no field of its own, by each column's key (redix-api #129, R5).</param>
public record EntryAnswers(string Description, Dictionary<string, string> ColumnValues)
{
    /// <summary>The keys of the required ones among <paramref name="columns"/> this holds no value for, in their order.</summary>
    public List<string> MissingOf(IEnumerable<ItemColumn> columns) => columns.Where(column => column.IsRequired && !Gives(column)).Select(column => column.Key).ToList();

    /// <summary>Whether this gives <paramref name="column"/> a value: a field of the entry's own is checked elsewhere and counts as given, the description but for what a person may leave empty (FR-41).</summary>
    private bool Gives(ItemColumn column) => column.Key == "description" ? Description.Length > 0 : column.IsEntryField || ColumnValues.ContainsKey(column.Key);
}
