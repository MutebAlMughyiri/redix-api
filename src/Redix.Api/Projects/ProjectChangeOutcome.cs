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
}
