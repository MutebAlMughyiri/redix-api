using System.Text.Json;

namespace Redix.Api.History;

/// <summary>
/// What every history screen shares (redix-app #101): a page of History rows, newest first, after the last line already shown and, when asked, only what the caller did; and reading a
/// value a row kept. The accounts' history and the partners' history both read through it.
/// </summary>
public static class HistoryPage
{
    /// <summary>How many lines a page holds.</summary>
    public const int Size = 50;

    /// <summary>The page of <paramref name="rows"/> after the line <paramref name="before"/> (0 for the newest), only <paramref name="userId"/>'s own when <paramref name="mine"/>.</summary>
    public static IQueryable<HistoryRecord> Of(IQueryable<HistoryRecord> rows, Guid userId, bool mine, long before) =>
        rows.Where(row => (!mine || row.UserId == userId) && (before <= 0 || row.Id < before)).OrderByDescending(row => row.Id).Take(Size);

    /// <summary>The text a row kept under <paramref name="name"/>; empty when it kept none.</summary>
    public static string TextIn(string values, string name) =>
        JsonDocument.Parse(values).RootElement.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";

    /// <summary>The id a row kept under <paramref name="name"/>; <see cref="Guid.Empty"/> when it kept none.</summary>
    public static Guid IdIn(string values, string name) => Guid.TryParse(TextIn(values, name), out var id) ? id : Guid.Empty;
}
