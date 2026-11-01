using System.Security.Cryptography;
using System.Text;

namespace Redix.Api.Translations;

/// <summary>The words every database starts with; a migration carries each new word to the database.</summary>
public static class TranslationSeed
{
    private static readonly (string Key, string English, string Arabic)[] Words =
    [
        ("app.title", "REDIX", "REDIX"),
        ("app.subtitle", "Project expenses", "مصروفات المشاريع"),
    ];

    /// <summary>One key per seeded word, with an id derived from the key so every migration and database agree on it.</summary>
    public static IEnumerable<TranslationKey> Keys => Words.Select(word => new TranslationKey { Id = StableId(word.Key), Key = word.Key });

    /// <summary>Each seeded word in English and Arabic, with ids derived from the key and the language.</summary>
    public static IEnumerable<Translation> Values => Words.SelectMany(word => new[]
    {
        new Translation { Id = StableId($"{word.Key}:en"), TranslationKeyId = StableId(word.Key), Language = "en", Value = word.English },
        new Translation { Id = StableId($"{word.Key}:ar"), TranslationKeyId = StableId(word.Key), Language = "ar", Value = word.Arabic },
    });

    private static Guid StableId(string name) => new(SHA256.HashData(Encoding.UTF8.GetBytes(name)).AsSpan(0, 16));
}
