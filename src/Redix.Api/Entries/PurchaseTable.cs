using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>
/// A purchase table (UC-022, FR-08): a group of an item that takes purchases, its general one going by the item's name. Its entry names a supplier the caller may use (BR-19), may
/// say what was bought, its unit price and its quantity, and as a credit note is kept negative, naming no bill (redix-api #119, D4); its file is the invoice.
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

    /// <summary>The amount typed; left out, the unit price times the quantity.</summary>
    public override decimal TotalOf(EntryRequest request) => request.Amount != 0 ? request.Amount : HasPriceAndQuantityWithinLimit(request) ? request.UnitPrice * request.Quantity : 0;

    /// <summary>The total, kept negative for a credit note, so that wherever totals are added up it takes away what its bill counted.</summary>
    public override decimal AmountOf(EntryRequest request) => request.IsCreditNote ? -TotalOf(request) : TotalOf(request);

    /// <summary>
    /// The product fits, <c>invalidProduct</c>; the unit price
    /// and the quantity are not negative and have at most two decimals, <c>invalidUnitPrice</c> or <c>invalidQuantity</c>; and a credit note is no deduction, <c>invalidAmount</c>.
    /// </summary>
    public override string RefusalOfTyped(EntryRequest request)
    {
        if (request.Product.Trim().Length > PurchaseDetails.ProductMaxLength)
        {
            return "InvalidProduct";
        }
        if (request.UnitPrice < 0 || !Money.IsAcceptable(request.UnitPrice))
        {
            return "InvalidUnitPrice";
        }
        if (request.Quantity < 0 || !Money.IsAcceptable(request.Quantity))
        {
            return "InvalidQuantity";
        }
        return request.IsCreditNote && TotalOf(request) < 0 ? "InvalidAmount" : "";
    }

    /// <summary>A supplier the caller may not use answers <c>unknownSupplier</c> (BR-19).</summary>
    public override async Task<string> RefusalOfAsync(Guid projectId, EntryRequest request, Guid userId, List<Permission> held, Guid editing, RedixDbContext db) =>
        await db.Suppliers.Where(Supplier.OfferedTo(userId, held.Contains(Permission.ManageSettings))).AnyAsync(row => row.Id == request.SupplierId && row.ProjectId == projectId) ? "" : "UnknownSupplier";

    /// <inheritdoc />
    public override List<object> DetailsOf(EntryRequest request) =>
    [
        ExpenseDetailsOf(request, Subgroup.Id),
        new PurchaseDetails { EntryId = request.Id, SupplierId = request.SupplierId, Product = request.Product.Trim(), UnitPrice = request.UnitPrice, Quantity = request.Quantity },
    ];

    private static bool HasPriceAndQuantityWithinLimit(EntryRequest request) => request.UnitPrice > 0 && request.Quantity > 0 && request.UnitPrice <= Money.Largest / request.Quantity;
}
