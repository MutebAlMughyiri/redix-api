namespace Redix.Api.Translations;

public class Translation
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TranslationKeyId { get; init; }
    public required string Language { get; init; }
    public required string Value { get; set; }
}
