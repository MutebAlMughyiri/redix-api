namespace Redix.Api.Projects;

/// <summary>What a user is in one project (FR-36). What each may do is UC-009.</summary>
public enum ProjectRole : byte
{
    /// <summary>Approves entries and issues the reports. Every owner is one in every project (BR-29).</summary>
    FinanceSupervisor,

    /// <summary>Records expenses and invoices.</summary>
    FieldSupervisor,

    /// <summary>Added by Muteb on 5 Nov 2026, beside the two the BRS names. The least a user can be given.</summary>
    Reader,
}
