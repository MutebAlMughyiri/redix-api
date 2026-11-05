namespace Redix.Api.Audit;

/// <summary>What a record of the audit trail says was done (FR-21). Each use case adds the actions it records; stored by name.</summary>
public enum AuditAction
{
    /// <summary>A user signed in on a new device (UC-002).</summary>
    SignIn,

    /// <summary>A user signed out of a device (UC-062).</summary>
    SignOut,

    /// <summary>A user replaced their password (UC-003); the password's hash never enters the trail.</summary>
    ChangePassword,

    /// <summary>A project manager created a user (UC-007, UC-009), or Finance added a contract (UC-020).</summary>
    Create,

    /// <summary>A user's name or mobile was changed (UC-007), or a contract amended (UC-020).</summary>
    Update,

    /// <summary>A user was given a new temporary password (UC-007).</summary>
    ResetPassword,

    /// <summary>A user or a contract was disabled (UC-007, UC-020).</summary>
    Disable,

    /// <summary>A disabled user or contract was enabled (UC-007, UC-020).</summary>
    Enable,

    /// <summary>A user was given a role in a project (UC-008, UC-009).</summary>
    AssignProject,

    /// <summary>A user's role in a project was taken away (UC-008, UC-009).</summary>
    RemoveProject,

    /// <summary>A project was registered, and waits for the admin (UC-066).</summary>
    RegisterProject,

    /// <summary>The admin let a registered project in (UC-066).</summary>
    ApproveProject,

    /// <summary>The admin refused a registered project, saying why (UC-066).</summary>
    RejectProject,

    /// <summary>A user turned on fingerprint or face on a phone, so its session no longer times out (UC-011).</summary>
    TurnOnBiometrics,

    /// <summary>A user turned off fingerprint or face on a phone, so its session times out again (UC-011).</summary>
    TurnOffBiometrics,

    /// <summary>A file was kept, or linked to a contract (UC-021).</summary>
    Attach,
}
