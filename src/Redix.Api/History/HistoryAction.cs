namespace Redix.Api.History;

/// <summary>What a record of the History table says was done (FR-21). Each use case adds the actions it records; stored by name.</summary>
public enum HistoryAction
{
    /// <summary>A user signed in on a new device (UC-002).</summary>
    SignIn,

    /// <summary>A user signed out of a device (UC-062).</summary>
    SignOut,

    /// <summary>A wrong password was typed for a user, or any password while they are locked; what was typed never enters the History table (UC-005).</summary>
    SignInFailed,

    /// <summary>A wrong password locked a user's account (AC-01.2, UC-005).</summary>
    AccountLocked,

    /// <summary>A user replaced their password (UC-003); the password's hash never enters the History table.</summary>
    ChangePassword,

    /// <summary>A project manager created a user (UC-007, UC-009), Finance added a unit (UC-006), a contract (UC-020) or an account, someone saved an entry as a draft, or the parts of a new entry (UC-022), or moved money between two accounts (redix-app #89).</summary>
    Create,

    /// <summary>A user's name or mobile was changed (UC-007), an account (UC-015), or a contract amended (UC-020).</summary>
    Update,

    /// <summary>A user was given a new temporary password (UC-007).</summary>
    ResetPassword,

    /// <summary>A person was made inactive in a project (redix-api #126; a whole login before it, UC-007), or a contract or an account was disabled (UC-020, redix-app #89).</summary>
    Disable,

    /// <summary>A person inactive in a project was made active there (redix-api #126; a whole login before it, UC-007), or a disabled contract or account was enabled (UC-020, redix-app #89).</summary>
    Enable,

    /// <summary>A user was given a permission in a project (UC-008, UC-009).</summary>
    AssignProject,

    /// <summary>A user's permission in a project was taken away (UC-008, UC-009), and with the last of them his being inactive there (redix-api #126).</summary>
    RemoveProject,

    /// <summary>A project was registered, and waits for the admin (UC-066).</summary>
    RegisterProject,

    /// <summary>The admin let a registered project in (UC-066).</summary>
    ApproveProject,

    /// <summary>Finance approved an entry, so it counts (UC-032).</summary>
    Approve,

    /// <summary>Finance rejected an entry, saying why (UC-032).</summary>
    Reject,

    /// <summary>The admin refused a registered project, saying why (UC-066).</summary>
    RejectProject,

    /// <summary>A user turned on fingerprint or face on a phone, so its session no longer times out (UC-011).</summary>
    TurnOnBiometrics,

    /// <summary>A user turned off fingerprint or face on a phone, so its session times out again (UC-011).</summary>
    TurnOffBiometrics,

    /// <summary>An entry was sent for approval (UC-022).</summary>
    Submit,

    /// <summary>A file was kept, or linked to a contract (UC-021).</summary>
    Attach,

    /// <summary>A project was closed, which cannot be undone (UC-006).</summary>
    CloseProject,

    /// <summary>A unit, a document, an account nothing named or an entry was deleted (redix-api #96, redix-app #89), or an approved entry removed softly (redix-api #119).</summary>
    Delete,

    /// <summary>An approved entry removed softly was brought back, the undo of its removal (redix-api #119).</summary>
    Restore,

    /// <summary>
    /// An entry approved at once as the opposite of another, written by reversing before it went (redix-api #119, D5). Nothing writes it now; it stays because History rows never
    /// change (AC-21.1), and the histories read it as the approval of an ordinary entry.
    /// </summary>
    Reverse,
}
