namespace Redix.Api.History;

/// <summary>What a record of the History table says was done (FR-21). Each use case adds the actions it records; stored by name.</summary>
public enum HistoryAction
{
    /// <summary>A user signed in on a new device (UC-002).</summary>
    SignIn,

    /// <summary>A user signed out of a device (UC-062).</summary>
    SignOut,

    /// <summary>A wrong password was typed for a user, or any password while they are locked or disabled; what was typed never enters the History table (UC-005).</summary>
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

    /// <summary>A user, a contract or an account was disabled (UC-007, UC-020, redix-app #89).</summary>
    Disable,

    /// <summary>A disabled user, contract or account was enabled (UC-007, UC-020, redix-app #89).</summary>
    Enable,

    /// <summary>A user was given a permission in a project (UC-008, UC-009).</summary>
    AssignProject,

    /// <summary>A user's permission in a project was taken away (UC-008, UC-009).</summary>
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

    /// <summary>A unit, a document or an account nothing named was deleted (redix-api #96, redix-app #89).</summary>
    Delete,

    /// <summary>An approved entry was reversed: the opposite entry, approved at once (UC-035, redix-app #89).</summary>
    Reverse,
}
