using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.History;

/// <summary>
/// One record of the History table (FR-21, AC-21.1): who did what to which record, when, and the values it changed, before and after.
/// Written once with the change it records; the database refuses to change or delete it.
/// </summary>
public class HistoryRecord
{
    /// <summary>The record's number, in the order records were written.</summary>
    public long Id { get; private set; }

    /// <summary>When it was done.</summary>
    public DateTime At { get; private init; }

    /// <summary>The project the record belongs to, which a project's history is read by (redix-app #89, D11); <see cref="Guid.Empty"/> for one outside any project, such as a user.</summary>
    public Guid ProjectId { get; private init; }

    /// <summary>The user who did it.</summary>
    public Guid UserId { get; private init; }

    /// <summary>What was done.</summary>
    public HistoryAction Action { get; private init; }

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
    /// the coming save changes: every value of a new or a removed record, the modified ones otherwise. Neither the record's id (the first part of its key), the rowversion, nor a value marked
    /// <see cref="NotInHistoryAttribute"/> enters it.
    /// </summary>
    public static HistoryRecord Of(EntityEntry entry, HistoryAction action, Guid userId, DateTime at)
    {
        var isNew = entry.State == EntityState.Added;
        var isRemoved = entry.State == EntityState.Deleted;
        var recordId = entry.Metadata.FindPrimaryKey()!.Properties[0];
        var changed = entry.Properties.Where(property => (isNew || isRemoved || property.IsModified) && property.Metadata != recordId && IsInHistory(property)).ToList();
        return new HistoryRecord
        {
            At = at,
            ProjectId = ProjectOf(entry),
            UserId = userId,
            Action = action,
            TableName = entry.Metadata.GetTableName() ?? "",
            RecordId = (Guid)entry.Property(recordId.Name).CurrentValue!,
            Before = isNew ? "{}" : JsonSerializer.Serialize(changed.ToDictionary(property => property.Metadata.Name, property => property.OriginalValue)),
            After = isRemoved ? "{}" : JsonSerializer.Serialize(changed.ToDictionary(property => property.Metadata.Name, property => property.CurrentValue)),
        };
    }

    /// <summary>
    /// The project <paramref name="entry"/>'s record belongs to: a project's own id, the project a record names, or the project of the entry, the contract, the category or the
    /// account it hangs off; <see cref="Guid.Empty"/> for a record outside any project.
    /// </summary>
    private static Guid ProjectOf(EntityEntry entry)
    {
        var db = (RedixDbContext)entry.Context;
        return entry.Entity switch
        {
            Project project => project.Id,
            _ when IdNamed(entry, nameof(Entry.ProjectId)) is var projectId && projectId != Guid.Empty => projectId,
            _ when IdNamed(entry, "EntryId") is var entryId && entryId != Guid.Empty => db.Entries.Find(entryId)!.ProjectId,
            _ when IdNamed(entry, "ContractId") is var contractId && contractId != Guid.Empty => db.Contracts.Find(contractId)!.ProjectId,
            _ when IdNamed(entry, "CategoryId") is var categoryId && categoryId != Guid.Empty => db.Categories.Find(categoryId)!.ProjectId,
            _ when IdNamed(entry, "AccountId") is var accountId && accountId != Guid.Empty => db.Accounts.Find(accountId)!.ProjectId,
            _ => Guid.Empty,
        };
    }

    private static Guid IdNamed(EntityEntry entry, string name) => entry.Metadata.FindProperty(name) is null ? Guid.Empty : (Guid)entry.Property(name).CurrentValue!;

    private static bool IsInHistory(PropertyEntry property) =>
        !property.Metadata.IsShadowProperty() && property.Metadata.PropertyInfo?.GetCustomAttribute<NotInHistoryAttribute>() is null;
}
