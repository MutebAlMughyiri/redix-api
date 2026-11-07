namespace Redix.Api.Config;

/// <summary>
/// One configurable value, kept as a row so it changes with no release: a timeout today, any value later. Every value here is read by the app before sign-in, so none
/// is ever a secret; secrets stay in environment variables.
/// </summary>
public class ConfigValue
{
    /// <summary>The longest a key is.</summary>
    public const int KeyMaxLength = 100;

    /// <summary>The longest a value is.</summary>
    public const int ValueMaxLength = 500;

    /// <summary>The values every database starts with; after the first migration they are data.</summary>
    public static IReadOnlyList<ConfigValue> Seeded { get; } =
    [
        Of("requestTimeoutSeconds", "10"),
        Of("fileTimeoutSeconds", "120"),
    ];

    /// <summary>The row's id, the same in every database for a seeded value.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The value's name, as the app reads it.</summary>
    public required string Key { get; init; }

    /// <summary>The value, as text.</summary>
    public required string Value { get; set; }

    private static ConfigValue Of(string key, string value) => new() { Id = StableIds.Of($"config:{key}"), Key = key, Value = value };
}
