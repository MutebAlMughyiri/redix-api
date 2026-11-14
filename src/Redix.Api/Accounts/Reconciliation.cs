namespace Redix.Api.Accounts;

/// <summary>One check of the accounts' real money against the books (FR-24).</summary>
public class Reconciliation : IProjectRow
{
    /// <summary>Its id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>When it was done.</summary>
    public DateTime At { get; init; }

    /// <summary>Who did it.</summary>
    public Guid UserId { get; init; }

    /// <summary>What each account really held.</summary>
    public List<ReconciliationBalance> Balances { get; init; } = [];
}

/// <summary>What one account really held at a reconciliation.</summary>
public class ReconciliationBalance : IProjectRow
{
    /// <summary>The reconciliation.</summary>
    public Guid ReconciliationId { get; init; }

    /// <summary>The account.</summary>
    public Guid AccountId { get; init; }

    /// <summary>What it really held.</summary>
    public decimal ActualBalance { get; init; }
}
