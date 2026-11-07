namespace Redix.Api.Auth;

/// <summary>
/// What a route can ask of its caller (BRS section 2). Who holds each one is data, ticked per person: <see cref="Projects.ProjectPermission"/> in a project,
/// <see cref="SystemPermission"/> across the system. A new one is code, because a route has to name it. Each use case adds the ones its routes need.
/// </summary>
public enum Permission
{
    /// <summary>Sees the registered projects, and approves or rejects them (BR-29, BR-34).</summary>
    ApproveProjects,

    /// <summary>Adds people to one project and ticks their permissions in it (FR-02, BR-29).</summary>
    ManageUsers,

    /// <summary>Sees one project and everything in it (BR-31).</summary>
    ReadProject,

    /// <summary>Sees every partner in one project and their capital, not only their own (BR-31).</summary>
    SeeAllCapital,

    /// <summary>Renames one project, adds, renames and deletes its units, and closes it (FR-04, BR-38).</summary>
    ManageProject,

    /// <summary>Shapes one project's expense structure: its categories, items, subgroups and columns (FR-05, FR-38, FR-39, FR-41).</summary>
    ManageStructure,

    /// <summary>Keeps one project's payment methods, accounts, partners, suppliers and contracts (FR-40, FR-22, UC-016, FR-06, FR-07).</summary>
    ManageSettings,

    /// <summary>Adds a supplier to one project's register; without <see cref="ManageSettings"/> it waits for approval (FR-44, BR-30).</summary>
    AddSupplier,

    /// <summary>Spends from any active account of one project; without it, only from those the user holds (BRS open question 4). The entry use cases refuse the rest.</summary>
    ChargeAnyAccount,

    /// <summary>Records a purchase or a contractor payment in one project's ledger (FR-08, FR-12).</summary>
    RecordEntries,

    /// <summary>Sends for approval an entry someone else entered in one project; without it, only one's own (UC-028).</summary>
    ManageAnyEntry,

    /// <summary>Deletes an entry of one project that is not approved: one's own draft or rejected entry, or with <see cref="ManageAnyEntry"/> anyone's or one waiting (UC-033).</summary>
    DeleteEntries,

    /// <summary>Keeps a picture or a PDF in one project, for an entry or a contract (FR-09).</summary>
    AttachFiles,

    /// <summary>Sees the entries waiting in one project, and approves or rejects them (FR-18, FR-19).</summary>
    ApproveEntries,

    /// <summary>Enters what counts at once in one project: capital movements and revenue, and reconciles its accounts (BR-30, FR-24, FR-25).</summary>
    MoveMoney,
}
