namespace Redix.Api.Settings;

/// <summary>A way a project pays, such as شبكة (FR-40). Each project keeps its own (BR-33).</summary>
public class PaymentMethod : IProjectRow
{
    /// <summary>The method's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project it belongs to.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The method's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>Its place in the project's list, in the order they were added.</summary>
    public int SortOrder { get; init; }

    /// <summary>False once disabled: it disappears from entry and stays on the entries that used it (AC-40.1).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The same method, for a new project.</summary>
    public PaymentMethod CopyFor(Guid projectId) => new() { ProjectId = projectId, Name = Name, SortOrder = SortOrder };
}
