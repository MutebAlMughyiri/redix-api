using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>
/// A partner, a table of the project's رأس المال (redix-api #109): money they put in, or take out as a withdrawal (BR-05, BR-06), as the choice of «نوع الحركة» says by its sign
/// (redix-api #129); its file is the bank receipt.
/// </summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The capital item.</param>
/// <param name="Partner">The partner.</param>
public sealed record PartnerTable(Category Category, Item Item, Partner Partner) : EntryTable(Category, Item, Partner.Id, Partner.Name, ItemTableKind.Partner, Partner.SortOrder)
{
    /// <summary>Capital out when the choice of its signed list is «−», as «سحب مبلغ»; capital in otherwise.</summary>
    public override EntryKind EntryKindOf(EntryRequest request) => SignOf(request) == ChoiceSign.Minus ? EntryKind.CapitalOut : EntryKind.CapitalIn;

    /// <summary>The total as typed: what a movement does to the capital is its kind's to say (<see cref="EntryKinds.Signed"/>).</summary>
    public override decimal AmountOf(EntryRequest request) => TotalOf(request);

    /// <inheritdoc />
    public override List<object> DetailsOf(EntryRequest request) =>
        [new CapitalMovementDetails { EntryId = request.Id, PartnerId = Partner.Id }];
}
