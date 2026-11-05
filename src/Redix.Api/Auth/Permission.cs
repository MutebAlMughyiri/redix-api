namespace Redix.Api.Auth;

/// <summary>
/// What a route can ask of its caller (BRS section 2). Which role holds each one is data, in <see cref="RolePermission"/>; a new one is code, because a route has
/// to name it. Each use case adds the ones its routes need.
/// </summary>
public enum Permission
{
    /// <summary>Sees the registered projects, and approves or rejects them (BR-29, BR-34).</summary>
    ApproveProjects,

    /// <summary>Adds people to one project and gives their roles in it (FR-02, BR-29).</summary>
    ManageUsers,

    /// <summary>Sees one project and everything in it (BR-31).</summary>
    ReadProject,

    /// <summary>Sees every partner in one project and their capital, not only their own (BR-31).</summary>
    SeeAllCapital,

    /// <summary>Adds units to one project and closes it (FR-04).</summary>
    ManageProject,

    /// <summary>Shapes one project's expense structure: its categories, items, subgroups and columns (FR-05, FR-38, FR-39, FR-41).</summary>
    ManageStructure,

    /// <summary>Keeps one project's payment methods, VAT rate, accounts, partners, suppliers and contracts (FR-40, BR-14, FR-22, UC-016, FR-06, FR-07).</summary>
    ManageSettings,

    /// <summary>Adds a supplier to one project's register; without <see cref="ManageSettings"/> it waits for approval (FR-44, BR-30).</summary>
    AddSupplier,

    /// <summary>Spends from any active account of one project; without it, only from those the user holds (BRS open question 4). The entry use cases refuse the rest.</summary>
    ChargeAnyAccount,

    /// <summary>Records a purchase in one project's ledger (FR-08).</summary>
    RecordEntries,

    /// <summary>Keeps a picture or a PDF in one project, for an entry or a contract (FR-09).</summary>
    AttachFiles,
}
