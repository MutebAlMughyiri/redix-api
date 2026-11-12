using Redix.Api.Structure;

namespace Redix.Api.Projects;

/// <summary>
/// A real-estate development project (FR-04): its name, its start date and its units. It owns the rules that it waits for the admin's decision (BR-34), and that
/// a finished project is read-only, while its reports stay available (AC-04.1).
/// </summary>
public class Project
{
    /// <summary>The longest name a project or a unit may have.</summary>
    public const int NameMaxLength = 100;

    /// <summary>The longest reason a rejection may give.</summary>
    public const int ReasonMaxLength = 500;

    /// <summary>The longest description a project may have (Muteb, 8 Nov 2026, redix-api 93).</summary>
    public const int DescriptionMaxLength = 2000;

    /// <summary>The project's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project's name, as it was typed; it changes only through <see cref="Rename"/>.</summary>
    public required string Name { get; set; }

    /// <summary>The day the project started.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Where it stands; a new project waits for the admin.</summary>
    public ProjectStatus Status { get; private set; } = ProjectStatus.PendingApproval;

    /// <summary>Why the admin refused it; empty otherwise.</summary>
    public string RejectionReason { get; private set; } = "";

    /// <summary>What the project is, as its manager wrote it, for everyone in it and for the admin who approves it; empty for none. It changes only through <see cref="Describe"/>.</summary>
    public string Description { get; private set; } = "";

    /// <summary>True for the default template: a project nobody is a member of, whose structure a new project copies (FR-05).</summary>
    public bool IsTemplate { get; init; }

    /// <summary>The earlier project its registration chose to copy the structure from when it is approved; <see cref="Guid.Empty"/> for the default template.</summary>
    public Guid StructureSourceId { get; set; }

    /// <summary>The project its structure is copied from when it is approved.</summary>
    public Guid StructureSource => StructureSourceId == Guid.Empty ? StructureSeed.TemplateId : StructureSourceId;

    /// <summary>The project's units, in the order they were added.</summary>
    public List<Unit> Units { get; } = [];

    /// <summary>True once the project is finished: nothing in it changes any more (AC-04.1).</summary>
    public bool IsReadOnly => Status == ProjectStatus.Finished;

    /// <summary>Where a project stands once the admin has let it in, so permissions count in it (BR-34).</summary>
    public static readonly IReadOnlyList<ProjectStatus> ApprovedStatuses = [ProjectStatus.Active, ProjectStatus.Finished];

    /// <summary>True once the admin has let it in, so permissions count in it (BR-34).</summary>
    public bool IsApproved => ApprovedStatuses.Contains(Status);

    /// <summary>A new project waiting for approval, with its units in order and its <paramref name="description"/>; the outcome says whether its name, every unit's and the description are acceptable.</summary>
    public static (Project Project, ProjectChangeOutcome Outcome) New(string name, DateOnly startDate, IEnumerable<string> units, string description)
    {
        var project = new Project { Name = name.Trim(), StartDate = startDate };
        var outcome = IsAcceptableName(project.Name) ? project.Describe(description.Trim()) : ProjectChangeOutcome.InvalidName;
        foreach (var unit in units)
        {
            var added = project.AddUnit(unit.Trim());
            outcome = added == ProjectChangeOutcome.Done ? outcome : added;
        }
        return (project, outcome);
    }

    /// <summary>Lets the project in; its permissions count from then on (BR-34). A decided project answers <see cref="ProjectChangeOutcome.NotPending"/>.</summary>
    public ProjectChangeOutcome Approve()
    {
        if (Status != ProjectStatus.PendingApproval)
        {
            return ProjectChangeOutcome.NotPending;
        }
        Status = ProjectStatus.Active;
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Refuses the project, saying why (AC-52.2). A decided project answers <see cref="ProjectChangeOutcome.NotPending"/>; an unacceptable reason, <see cref="ProjectChangeOutcome.InvalidReason"/>.</summary>
    public ProjectChangeOutcome Reject(string reason)
    {
        if (Status != ProjectStatus.PendingApproval)
        {
            return ProjectChangeOutcome.NotPending;
        }
        if (reason.Length is 0 or > ReasonMaxLength)
        {
            return ProjectChangeOutcome.InvalidReason;
        }
        Status = ProjectStatus.Rejected;
        RejectionReason = reason;
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Whether <paramref name="name"/>, already trimmed, may name a project, a unit or anything in its structure: not empty, and at most <see cref="NameMaxLength"/> characters.</summary>
    public static bool IsAcceptableName(string name) => name.Length is > 0 and <= NameMaxLength;

    /// <summary>Adds a unit named <paramref name="name"/> at the end of the list, unless the project is finished, the name is not acceptable, or another unit holds it (BR-37).</summary>
    public ProjectChangeOutcome AddUnit(string name)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectFinished;
        }
        if (!IsAcceptableName(name))
        {
            return ProjectChangeOutcome.InvalidName;
        }
        if (IsUnitNameTaken(name))
        {
            return ProjectChangeOutcome.UnitNameTaken;
        }
        Units.Add(new Unit { ProjectId = Id, Name = name, SortOrder = Units.Count == 0 ? 0 : Units.Max(unit => unit.SortOrder) + 1 });
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Keeps the project's description while it is open (AC-04.1): at most <see cref="DescriptionMaxLength"/> characters, empty for none (redix-api 93).</summary>
    public ProjectChangeOutcome Describe(string description)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectFinished;
        }
        if (description.Length > DescriptionMaxLength)
        {
            return ProjectChangeOutcome.InvalidDescription;
        }
        Description = description;
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Renames the project while it is open (BR-38); another project may hold the same name (BR-37).</summary>
    public ProjectChangeOutcome Rename(string name)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectFinished;
        }
        if (!IsAcceptableName(name))
        {
            return ProjectChangeOutcome.InvalidName;
        }
        Name = name;
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Renames one of the project's units while the project is open (BR-38), unless the name is not acceptable or another unit holds it (BR-37).</summary>
    public ProjectChangeOutcome RenameUnit(Guid unitId, string name)
    {
        var refusal = RefusalOfChangingUnit(unitId);
        if (refusal != ProjectChangeOutcome.Done)
        {
            return refusal;
        }
        if (!IsAcceptableName(name))
        {
            return ProjectChangeOutcome.InvalidName;
        }
        if (IsUnitNameTaken(name, unitId))
        {
            return ProjectChangeOutcome.UnitNameTaken;
        }
        Units.Single(unit => unit.Id == unitId).Name = name;
        return ProjectChangeOutcome.Done;
    }

    /// <summary>Removes one of the project's units while the project is open (BR-38).</summary>
    public ProjectChangeOutcome RemoveUnit(Guid unitId)
    {
        var refusal = RefusalOfChangingUnit(unitId);
        if (refusal != ProjectChangeOutcome.Done)
        {
            return refusal;
        }
        Units.RemoveAll(unit => unit.Id == unitId);
        return ProjectChangeOutcome.Done;
    }

    private ProjectChangeOutcome RefusalOfChangingUnit(Guid unitId)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectFinished;
        }
        return Units.Any(unit => unit.Id == unitId) ? ProjectChangeOutcome.Done : ProjectChangeOutcome.UnknownUnit;
    }

    private bool IsUnitNameTaken(string name, Guid unitId = default) => Units.Any(unit => unit.Name == name && unit.Id != unitId);

    /// <summary>Closes the finished project, which is read-only from then on (AC-04.1); a finished project cannot be closed again, nor one with <paramref name="hasEntriesWaiting"/> for approval (BR-35).</summary>
    public ProjectChangeOutcome Close(bool hasEntriesWaiting)
    {
        if (IsReadOnly)
        {
            return ProjectChangeOutcome.ProjectFinished;
        }
        if (hasEntriesWaiting)
        {
            return ProjectChangeOutcome.PendingEntries;
        }
        Status = ProjectStatus.Finished;
        return ProjectChangeOutcome.Done;
    }
}
