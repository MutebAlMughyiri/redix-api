using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

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

    /// <summary>
    /// The latest report issued in the project <paramref name="projectId"/>; while none was, one numbered 0 and issued before anything was approved. The one owner of the next number (AC-29.3),
    /// the previous report's expenses (BR-09) and what is new since (BR-26).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the reports live in.</param>
    public static async Task<PeriodicReport> LatestOfAsync(Guid projectId, RedixDbContext db) =>
        await db.PeriodicReports.Where(report => report.ProjectId == projectId).OrderByDescending(report => report.Number).FirstOrDefaultAsync() ?? new PeriodicReport { IssuedAt = Entry.NotApproved };
}
