namespace Redix.Api.Translations;

public class TranslationKey
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Key { get; init; }
    public string Description { get; init; } = "";
    public List<Translation> Translations { get; init; } = [];
}
