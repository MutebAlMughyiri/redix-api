using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>A partner, a table of the project's رأس المال (redix-api #109): money they put in, or take out as a withdrawal (BR-05, BR-06); its file is the bank receipt.</summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The capital item.</param>
/// <param name="Partner">The partner.</param>
public sealed record PartnerTable(Category Category, Item Item, Partner Partner) : EntryTable(Category, Item, Partner.Id, Partner.Name, ItemTableKind.Partner, Partner.SortOrder)
{
    /// <inheritdoc />
    public override EntryKind EntryKindOf(EntryRequest request) => request.IsWithdrawal ? EntryKind.CapitalOut : EntryKind.CapitalIn;

    /// <inheritdoc />
    public override EntryAttachmentRole FileRole => EntryAttachmentRole.BankReceipt;

    /// <inheritdoc />
    public override List<object> DetailsOf(EntryRequest request) =>
        [new CapitalMovementDetails { EntryId = request.Id, PartnerId = Partner.Id, Type = request.IsWithdrawal ? CapitalMovementType.Withdrawal : CapitalMovementType.Contribution }];
}
