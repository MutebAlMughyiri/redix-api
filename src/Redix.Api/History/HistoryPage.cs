using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.History;

/// <summary>
/// What every history screen shares (redix-app #101): a page of lines, newest first, after the last line already shown and, when asked, only what the caller did and only the
/// lines holding what was typed (redix-app #104); and reading a value a row kept. The accounts' history and the partners' history both read through it.
/// </summary>
public static class HistoryPage
{
    /// <summary>How many lines a page holds.</summary>
    public const int Size = 50;

    /// <summary>
    /// The page of lines <paramref name="linesOf"/> makes of <paramref name="rows"/> after the line <paramref name="before"/> (0 for the newest), only <paramref name="userId"/>'s own
    /// when <paramref name="mine"/>, and, once something was typed in <paramref name="text"/>, only those that hold it. A line's names are read after its row, so a search makes
    /// the lines of every row after <paramref name="before"/> and keeps a page of them.
    /// </summary>
    public static async Task<List<TLine>> LinesAsync<TLine>(IQueryable<HistoryRecord> rows, Guid userId, bool mine, long before, string text, Func<List<HistoryRecord>, Task<List<TLine>>> linesOf)
        where TLine : IHistoryLine
    {
        var newest = rows.Where(row => (!mine || row.UserId == userId) && (before <= 0 || row.Id < before)).OrderByDescending(row => row.Id);
        if (text.Trim().Length == 0)
        {
            return await linesOf(await newest.Take(Size).ToListAsync());
        }
        return [.. (await linesOf(await newest.ToListAsync())).Where(line => line.Holds(text)).Take(Size)];
    }

    /// <summary>The text a row kept under <paramref name="name"/>; empty when it kept none.</summary>
    public static string TextIn(string values, string name) =>
        JsonDocument.Parse(values).RootElement.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";

    /// <summary>The id a row kept under <paramref name="name"/>; <see cref="Guid.Empty"/> when it kept none.</summary>
    public static Guid IdIn(string values, string name) => Guid.TryParse(TextIn(values, name), out var id) ? id : Guid.Empty;
}

/// <summary>A line of a history, which knows what a search finds it by (redix-app #104).</summary>
public interface IHistoryLine
{
    /// <summary>Whether <paramref name="text"/> is in what the line names or in its amount, as <see cref="ArabicSpelling.Holds"/> reads it.</summary>
    /// <param name="text">What was typed.</param>
    bool Holds(string text);
}
