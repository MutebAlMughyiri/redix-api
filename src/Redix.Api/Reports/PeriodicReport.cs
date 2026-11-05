namespace Redix.Api.Reports;

/// <summary>A numbered periodic report as it was issued (FR-29, BR-26): every figure kept as it was, so nothing approved later changes it.</summary>
public class PeriodicReport
{
    /// <summary>Its id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>Its number in the project, with no gaps.</summary>
    public int Number { get; init; }

    /// <summary>When it was issued; what was approved after it is new in the next one.</summary>
    public DateTime IssuedAt { get; init; }

    /// <summary>Who issued it.</summary>
    public Guid IssuedByUserId { get; init; }

    /// <summary>Its expenses, which the next report's difference starts from (BR-09).</summary>
    public decimal ExpensesTotal { get; init; }

    /// <summary>Every figure as issued, as JSON.</summary>
    public string Snapshot { get; init; } = "{}";
}
