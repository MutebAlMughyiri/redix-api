using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Translations;

public static class TranslationsEndpoint
{
    public static void MapTranslations(this WebApplication app) =>
        app.MapGet("/translations/{language}", async (string language, HttpContext http, RedixDbContext db) =>
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
        });
}
