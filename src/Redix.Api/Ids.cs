namespace Redix.Api;

/// <summary>How an id that may be absent travels in an answer (findings row 154, Muteb 8 Nov 2026): as text, empty when there is none, which is what the app takes to mean none.</summary>
public static class Ids
{
    /// <summary><paramref name="id"/> as the app reads it: empty for <see cref="Guid.Empty"/>.</summary>
    public static string Text(Guid id) => id == Guid.Empty ? "" : id.ToString();
}
