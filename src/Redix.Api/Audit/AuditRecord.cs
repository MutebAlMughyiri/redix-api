using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Redix.Api.Audit;

/// <summary>
/// One record of the audit trail (FR-21, AC-21.1): who did what to which record, when, and the values it changed, before and after.
/// Written once with the change it records; the database refuses to change or delete it.
/// </summary>
public class AuditRecord
{
    /// <summary>The record's number, in the order records were written.</summary>
    public long Id { get; private set; }

    /// <summary>When it was done.</summary>
    public DateTime At { get; private init; }

    /// <summary>The user who did it.</summary>
    public Guid UserId { get; private init; }

    /// <summary>What was done.</summary>
    public AuditAction Action { get; private init; }

    /// <summary>The table of the record it was done to.</summary>
    public string TableName { get; private init; } = "";

    /// <summary>The id of the record it was done to.</summary>
    public Guid RecordId { get; private init; }

    /// <summary>The values it changed, as they were: a JSON object of column names and values; <c>{}</c> for a new record.</summary>
    public string Before { get; private init; } = "{}";

    /// <summary>The same values as they became: a JSON object of column names and values; <c>{}</c> for a removed record.</summary>
    public string After { get; private init; } = "{}";

    /// <summary>
    /// The record of <paramref name="action"/> done by <paramref name="userId"/> at <paramref name="at"/> to the tracked <paramref name="entry"/>, with the values
    /// the coming save changes: every value of a new or a removed record, the modified ones otherwise. Neither the key, the rowversion, nor a value marked
    /// <see cref="NotAuditedAttribute"/> enters it.
    /// </summary>
    public static AuditRecord Of(EntityEntry entry, AuditAction action, Guid userId, DateTime at)
    {
        var isNew = entry.State == EntityState.Added;
        var isRemoved = entry.State == EntityState.Deleted;
        var changed = entry.Properties.Where(property => (isNew || isRemoved || property.IsModified) && IsAudited(property)).ToList();
        return new AuditRecord
        {
            At = at,
            UserId = userId,
            Action = action,
            TableName = entry.Metadata.GetTableName() ?? "",
            RecordId = (Guid)entry.Property("Id").CurrentValue!,
            Before = isNew ? "{}" : JsonSerializer.Serialize(changed.ToDictionary(property => property.Metadata.Name, property => property.OriginalValue)),
            After = isRemoved ? "{}" : JsonSerializer.Serialize(changed.ToDictionary(property => property.Metadata.Name, property => property.CurrentValue)),
        };
    }

    private static bool IsAudited(PropertyEntry property) =>
        !property.Metadata.IsKey() && !property.Metadata.IsShadowProperty() && property.Metadata.PropertyInfo?.GetCustomAttribute<NotAuditedAttribute>() is null;
}
