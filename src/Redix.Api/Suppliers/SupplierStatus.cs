namespace Redix.Api.Suppliers;

/// <summary>Where a supplier stands (FR-44).</summary>
public enum SupplierStatus : byte
{
    /// <summary>In the register, for anyone to use.</summary>
    Approved,

    /// <summary>Added from the field: it waits for the first expense on it to be approved (BR-30).</summary>
    PendingApproval,
}
