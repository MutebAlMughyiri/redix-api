namespace Redix.Api.Projects;

/// <summary>What became of a change asked of a project; every outcome but <see cref="Done"/> is a refusal whose name is the answer's code.</summary>
public enum ProjectChangeOutcome
{
    /// <summary>The change is made.</summary>
    Done,

    /// <summary>The project is finished, so it is read-only (AC-04.1).</summary>
    ProjectFinished,

    /// <summary>The name is empty or longer than <see cref="Project.NameMaxLength"/> characters.</summary>
    InvalidName,

    /// <summary>The project was already approved or rejected.</summary>
    NotPending,

    /// <summary>The reason for a rejection is empty or longer than <see cref="Project.ReasonMaxLength"/> characters.</summary>
    InvalidReason,

    /// <summary>The VAT rate is outside 0 to 100 percent, or has more than two decimals.</summary>
    InvalidVatRate,

    /// <summary>Entries of the project wait for approval, and must be approved or rejected before it closes (BR-35).</summary>
    PendingEntries,

    /// <summary>Another unit of the project holds the name (BR-37).</summary>
    UnitNameTaken,

    /// <summary>No such unit in this project, which answers 404.</summary>
    UnknownUnit,

    /// <summary>The shared unit is never renamed or deleted (BR-38).</summary>
    SharedUnit,

    /// <summary>An entry or a contract uses the unit, so it stays (BR-38).</summary>
    UnitInUse,
}
