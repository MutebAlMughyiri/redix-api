namespace Redix.Api.Entries;

/// <summary>
/// Who a purchase was bought from: a supplier of the project's register, never free text (BR-19). The supplier is optional (Muteb, 13 Nov 2026, redix-api #129), so a purchase
/// that names none has no row, as a link table says an optional id. What was bought, its unit price and its quantity are columns like any other, kept in <see cref="Entry.ColumnValues"/>.
/// </summary>
public class PurchaseDetails
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The supplier.</summary>
    public Guid SupplierId { get; init; }
}
