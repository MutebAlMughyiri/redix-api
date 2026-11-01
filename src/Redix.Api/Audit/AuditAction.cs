namespace Redix.Api.Audit;

/// <summary>What a record of the audit trail says was done (FR-21). Each use case adds the actions it records; stored by name.</summary>
public enum AuditAction
{
    /// <summary>A user signed in on a new device (UC-002).</summary>
    SignIn,

    /// <summary>A user replaced their password (UC-003); the password's hash never enters the trail.</summary>
    ChangePassword,
}
