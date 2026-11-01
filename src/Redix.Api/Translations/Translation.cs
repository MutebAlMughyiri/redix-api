namespace Redix.Api.Translations;

/// <summary>The value of one <see cref="TranslationKey"/> in one language.</summary>
public class Translation
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The key this value belongs to.</summary>
    public Guid TranslationKeyId { get; init; }

    /// <summary>The two-letter language code, <c>ar</c> or <c>en</c>.</summary>
    public required string Language { get; init; }

    /// <summary>The word as the user reads it.</summary>
    public required string Value { get; set; }
}
