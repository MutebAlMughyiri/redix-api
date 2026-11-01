namespace Redix.Api.Translations;

/// <summary>One word the app shows, named by a key such as <c>app.title</c>; its value in each language is a <see cref="Translation"/>.</summary>
public class TranslationKey
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The key the app's code uses for this word; the same in every database.</summary>
    public required string Key { get; init; }

    /// <summary>Where the word appears, for whoever edits the words; empty when obvious.</summary>
    public string Description { get; init; } = "";

    /// <summary>The word's value in each language.</summary>
    public List<Translation> Translations { get; init; } = [];
}
