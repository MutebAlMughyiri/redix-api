namespace Redix.Api.Audit;

/// <summary>Marks a secret, such as a password's hash, whose values never enter the audit trail, whatever the action.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotAuditedAttribute : Attribute;
