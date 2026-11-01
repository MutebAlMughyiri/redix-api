namespace Redix.Api.Projects;

/// <summary>
/// A real-estate development project (FR-04): its name, its start date and its units. It owns the rule that a closed project is read-only,
/// while its reports stay available (AC-04.1).
/// </summary>
public class Project
{
    /// <summary>The longest name a project or a unit may have.</summary>
    public const int NameMaxLength = 100;

    /// <summary>The project's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project's name, as the financial supervisor typed it.</summary>
    public required string Name { get; init; }

    /// <summary>The day the project started.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Running, or closed once finished.</summary>
    public ProjectStatus Status { get; private set; } = ProjectStatus.Running;

    /// <summary>The project's units, in the order they were added.</summary>
    public List<Unit> Units { get; } = [];

    /// <summary>True once the project is closed: nothing in it changes any more (AC-04.1).</summary>
    public bool IsReadOnly => Status == ProjectStatus.Closed;

    /// <summary>Whether <paramref name="name"/>, already trimmed, may name a project or a unit: not empty, and at most <see cref="NameMaxLength"/> characters.</summary>
    public static bool IsAcceptableName(string name) => name.Length is > 0 and <= NameMaxLength;

    /// <summary>Adds a unit named <paramref name="name"/> at the end of the list, unless the project is closed or the name is not acceptable.</summary>
    public ProjectChangeOutcome AddUnit(string name)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectClosed;
        }
        if (!IsAcceptableName(name))
        {
            return ProjectChangeOutcome.InvalidName;
        }
        Units.Add(new Unit { ProjectId = Id, Name = name, SortOrder = Units.Count });
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Closes the finished project, which is read-only from then on (AC-04.1); a closed project cannot be closed again.</summary>
    public ProjectChangeOutcome Close()
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectClosed;
        }
        Status = ProjectStatus.Closed;
        return ProjectChangeOutcome.Done;
    }
}
