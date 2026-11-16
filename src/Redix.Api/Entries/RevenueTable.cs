using Redix.Api.Revenue;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>A revenue item's group (FR-38, redix-api #101): money coming in, its general group going by the item's name. It adds nothing of its own to an entry; its file is any file.</summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The revenue item.</param>
/// <param name="Subgroup">The item's group it is.</param>
public sealed record RevenueTable(Category Category, Item Item, Subgroup Subgroup) : EntryTable(Category, Item, Subgroup.Id, NameOf(Item, Subgroup), KindOf(Subgroup), Subgroup.SortOrder)
{
    /// <inheritdoc />
    public override EntryKind EntryKindOf(EntryRequest request) => EntryKind.Revenue;

    /// <inheritdoc />
    public override List<object> DetailsOf(EntryRequest request) => [new RevenueDetails { EntryId = request.Id, ItemId = Item.Id, SubgroupId = Subgroup.Id }];
}
