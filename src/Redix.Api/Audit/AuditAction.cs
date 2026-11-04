namespace Redix.Api.Audit;

/// <summary>What a record of the audit trail says was done (FR-21). Each use case adds the actions it records; stored by name.</summary>
public enum AuditAction
{
    /// <summary>A user signed in on a new device (UC-002).</summary>
    SignIn,

    /// <summary>A user replaced their password (UC-003); the password's hash never enters the trail.</summary>
    ChangePassword,

    /// <summary>An owner created a user (UC-007).</summary>
    Create,

    /// <summary>An owner changed a user's name or mobile (UC-007).</summary>
    Update,

    /// <summary>An owner set a new temporary password for a user (UC-007).</summary>
    ResetPassword,

    /// <summary>An owner disabled a user (UC-007).</summary>
    Disable,

    /// <summary>An owner enabled a disabled user (UC-007).</summary>
    Enable,
}
