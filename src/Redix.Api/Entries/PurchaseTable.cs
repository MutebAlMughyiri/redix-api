using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>
/// A purchase table (UC-022, FR-08): a group of an item that takes purchases, its general one going by the item's name. Its entry may name a supplier, one the caller may use (BR-19),
/// and as a credit note is kept negative, naming no bill (redix-api #119, D4); its file is the invoice. What was bought, its unit price and its quantity are its item's columns.
/// </summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The item.</param>
/// <param name="Subgroup">The item's group it is.</param>
public sealed record PurchaseTable(Category Category, Item Item, Subgroup Subgroup) : EntryTable(Category, Item, Subgroup.Id, NameOf(Item, Subgroup), KindOf(Subgroup), Subgroup.SortOrder)
{
    /// <inheritdoc />
    public override EntryKind EntryKindOf(EntryRequest request) => EntryKind.Purchase;

    /// <inheritdoc />
    public override EntryAttachmentRole FileRole => EntryAttachmentRole.Invoice;

    /// <summary>The amount typed; left out, the unit price times the quantity, when the entry gives both (BR-01).</summary>
    public override decimal TotalOf(EntryRequest request)
    {
        if (request.Amount != 0)
        {
            return request.Amount;
        }
        var values = request.ColumnValuesKept();
        var unitPrice = NumberOf(values.GetValueOrDefault("unitPrice", ""));
        var quantity = NumberOf(values.GetValueOrDefault("quantity", ""));
        return unitPrice > 0 && quantity > 0 && unitPrice <= Money.Largest / quantity ? unitPrice * quantity : 0;
    }

    /// <summary>The total, kept negative for a credit note, so that wherever totals are added up it takes away what its bill counted.</summary>
    public override decimal AmountOf(EntryRequest request) => request.IsCreditNote ? -TotalOf(request) : TotalOf(request);

    /// <summary>A credit note is no deduction, <c>invalidAmount</c>.</summary>
    public override string RefusalOfTyped(EntryRequest request) => request.IsCreditNote && TotalOf(request) < 0 ? "InvalidAmount" : "";

    /// <summary>A supplier named that the caller may not use answers <c>unknownSupplier</c> (BR-19); none named is no refusal (redix-api #129).</summary>
    public override async Task<string> RefusalOfAsync(Guid projectId, EntryRequest request, Guid userId, List<Permission> held, Guid editing, RedixDbContext db) =>
        request.SupplierId == Guid.Empty || await db.Suppliers.Where(Supplier.OfferedTo(userId, held.Contains(Permission.ManageSettings))).AnyAsync(row => row.Id == request.SupplierId && row.ProjectId == projectId) ? "" : "UnknownSupplier";

    /// <summary>What every expense holds, and its supplier's row when it names one.</summary>
    public override List<object> DetailsOf(EntryRequest request) =>
    [
        ExpenseDetailsOf(request, Subgroup.Id),
        .. request.SupplierId == Guid.Empty ? [] : new List<object> { new PurchaseDetails { EntryId = request.Id, SupplierId = request.SupplierId } },
    ];

    private static decimal NumberOf(string value) => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) ? number : 0;
}
