using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Translations;

/// <summary>Serves the words the app shows, one language at a time.</summary>
public static class TranslationsEndpoint
{
    /// <summary>Maps <c>GET /translations/{language}</c>, open without sign-in because the sign-in screen needs its words.</summary>
    public static void MapTranslations(this WebApplication app) => app.MapGet("/translations/{language}", GetBundle);

    /// <summary>Returns every word of a language as one bundle, keyed by word key, with an ETag; answers 304 while the bundle is unchanged and 404 for a language with no words.</summary>
    /// <param name="language">The two-letter language code, <c>ar</c> or <c>en</c>.</param>
    /// <param name="http">The request, read for <c>If-None-Match</c>, and the response, given the ETag.</param>
    /// <param name="db">The database the words are read from.</param>
    public static async Task<IResult> GetBundle(string language, HttpContext http, RedixDbContext db)
    {
        var words = await db.TranslationKeys
            .SelectMany(key => key.Translations.Where(translation => translation.Language == language), (key, translation) => new { key.Key, translation.Value })
            .OrderBy(word => word.Key)
            .ToDictionaryAsync(word => word.Key, word => word.Value);
        if (words.Count == 0)
        {
            return Results.NotFound();
        }

        var body = JsonSerializer.SerializeToUtf8Bytes(words);
        var etag = $"\"{Convert.ToHexString(SHA256.HashData(body))}\"";
        http.Response.Headers.ETag = etag;
        return http.Request.Headers.IfNoneMatch == etag ? Results.StatusCode(StatusCodes.Status304NotModified) : Results.Bytes(body, "application/json");
    }
}
