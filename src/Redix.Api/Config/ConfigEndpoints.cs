using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Config;

/// <summary>The configurable values the app reads at start, before anyone signs in: how long it waits for an answer, and later anything else made configurable.</summary>
public static class ConfigEndpoints
{
    /// <summary>Maps <c>GET /config</c>, open to everyone.</summary>
    public static void MapConfig(this WebApplication app) => app.MapGet("/config", Read).AllowAnonymous();

    /// <summary>Every value by its key, as text.</summary>
    /// <param name="db">The database the values live in.</param>
    public static Task<Dictionary<string, string>> Read(RedixDbContext db) => db.ConfigValues.ToDictionaryAsync(row => row.Key, row => row.Value);
}
