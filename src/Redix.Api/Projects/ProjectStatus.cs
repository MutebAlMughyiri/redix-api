namespace Redix.Api.Projects;

/// <summary>Where a project stands (FR-04).</summary>
public enum ProjectStatus : byte
{
    /// <summary>Entries are recorded and approved.</summary>
    Running,

    /// <summary>Finished: read-only, with its reports still available (AC-04.1).</summary>
    Closed,
}
