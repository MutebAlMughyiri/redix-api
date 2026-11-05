namespace Redix.Api.Projects;

/// <summary>A unit of a project, such as الفلة الغربية: what its expenses are recorded against (FR-04).</summary>
public class Unit
{
    /// <summary>The name of the unit shared between all of a project's units (مشترك), which every approved project has (BRS open question 3).</summary>
    public const string SharedName = "مشترك";

    /// <summary>The unit's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project the unit belongs to.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The unit's name, as the financial supervisor typed it.</summary>
    public required string Name { get; init; }

    /// <summary>True for the shared unit, a row so that reports group it like any unit; it is not one of the project's own units.</summary>
    public bool IsShared { get; init; }

    /// <summary>The unit's place in its project's list: the order it was added in.</summary>
    public int SortOrder { get; init; }
}
