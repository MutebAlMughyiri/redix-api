using Redix.Api.Contracts;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>
/// A contract's payments (FR-12): the table of an active contract on an item that takes contractor payments, named after its contractor (redix-app #84). Its entry goes under the
/// item's general group, inside or outside the contract (FR-13); its file is the receipt, and recording it answers the contract's figures with it.
/// </summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The item.</param>
/// <param name="Contract">The contract.</param>
/// <param name="Name">The contractor's name, the table's name in the picker.</param>
public sealed record ContractTable(Category Category, Item Item, Contract Contract, string Name) : EntryTable(Category, Item, Contract.Id, Name, ItemTableKind.Contract, Contract.SortOrder)
{
    /// <inheritdoc />
    public override EntryKind EntryKindOf(EntryRequest request) => EntryKind.ContractorPayment;

    /// <inheritdoc />
    public override EntryAttachmentRole FileRole => EntryAttachmentRole.Receipt;

    /// <summary>Everything of its item but what only a purchase has, its supplier among it (FR-38).</summary>
    protected override bool Asks(ItemColumn column) => column.IsAskedOf(EntryKind.ContractorPayment);

    /// <inheritdoc />
    public override List<object> DetailsOf(EntryRequest request) =>
    [
        ExpenseDetailsOf(request, Item.Subgroups.Single(row => row.IsGeneral).Id),
        new ContractorPaymentDetails { EntryId = request.Id, ContractId = Contract.Id, IsOutsideContract = request.IsOutsideContract },
    ];

    /// <summary>The payment with what the contract has paid and has waiting besides it, what remains after it, and whether it goes beyond the contract's value (BR-07).</summary>
    public override async Task<object> AnswerOfAsync(Entry entry, EntryRequest request, RedixDbContext db)
    {
        var money = (await ContractMoney.OfProjectAsync(entry.ProjectId, db, entry.Id)).GetValueOrDefault(Contract.Id);
        var counted = money.Paid + money.Pending + (request.IsOutsideContract ? 0 : entry.Amount);
        return new ContractorPaymentView(entry.Id, entry.Status, entry.Amount, money.Paid, money.Pending, Contract.RemainingAfter(counted), Contract.IsOverrunBy(counted));
    }
}

/// <summary>A recorded contractor payment, and what its contract stands at with it.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="Paid">What was approved inside the contract before it.</param>
/// <param name="Pending">What waited for approval inside the contract before it.</param>
/// <param name="RemainingAfter">The value less everything paid or waiting, with this payment if inside the contract; 0 for an open-value contract.</param>
/// <param name="IsOverrun">Whether that goes beyond the value, which shows an alert.</param>
public record ContractorPaymentView(Guid Id, EntryStatus Status, decimal Amount, decimal Paid, decimal Pending, decimal RemainingAfter, bool IsOverrun);
