namespace Redix.Api.Suppliers;

/// <summary>The item a supplier's expenses usually go to (FR-06). A supplier has one at most, and having no row is having none.</summary>
public class SupplierDefaultItem : IProjectRow
{
    /// <summary>The supplier.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>The item, one of the supplier's project.</summary>
    public Guid ItemId { get; set; }
}
