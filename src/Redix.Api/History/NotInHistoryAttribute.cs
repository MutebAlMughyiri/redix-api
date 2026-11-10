namespace Redix.Api.History;

/// <summary>Marks a secret, such as a password's hash, whose values never enter the History table, whatever the action.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotInHistoryAttribute : Attribute;
