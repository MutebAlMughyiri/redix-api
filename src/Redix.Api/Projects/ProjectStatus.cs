namespace Redix.Api.Projects;

/// <summary>Where a project stands (FR-04).</summary>
public enum ProjectStatus : byte
{
    /// <summary>Entries are recorded and approved.</summary>
    Active,

    /// <summary>Finished: read-only, with its reports still available (AC-04.1).</summary>
    Finished,

    /// <summary>Registered, and waiting for the admin; no permission counts in it yet (BR-34).</summary>
    PendingApproval,

    /// <summary>The admin refused it, saying why; no permission counts in it (BR-34).</summary>
    Rejected,
}
