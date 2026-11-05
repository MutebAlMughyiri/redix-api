namespace Redix.Api;

/// <summary>
/// What a search treats alike in Arabic (FR-43): أ إ آ with ا, ة with ه, ى with ي, ؤ with و, ئ with ي, and no tashkeel. No SQL Server collation treats them alike, so a table keeps a normalized copy of what is searched
/// beside it, filled when it is saved, and a search normalizes what was typed the same way.
/// </summary>
public static class ArabicSpelling
{
    private static readonly Dictionary<char, char> Letters = new()
    {
        ['أ'] = 'ا',
        ['إ'] = 'ا',
        ['آ'] = 'ا',
        ['ة'] = 'ه',
        ['ى'] = 'ي',
        ['ؤ'] = 'و',
        ['ئ'] = 'ي',
    };

    private const char FirstTashkeel = 'ً';

    private const char LastTashkeel = 'ْ';

    /// <summary><paramref name="text"/> with one form of each letter and no tashkeel; anything else stays as it is.</summary>
    public static string Normalize(string text) =>
        string.Concat(text.Where(letter => letter is < FirstTashkeel or > LastTashkeel).Select(letter => Letters.GetValueOrDefault(letter, letter)));

    /// <summary>The same rule over the column <paramref name="column"/>, as a SQL expression, for the migration that fills rows saved before it.</summary>
    public static string SqlOf(string column)
    {
        var expression = $"{column} COLLATE Latin1_General_100_BIN2";
        foreach (var (variant, letter) in Letters)
        {
            expression = $"REPLACE({expression}, N'{variant}', N'{letter}')";
        }
        for (var tashkeel = FirstTashkeel; tashkeel <= LastTashkeel; tashkeel++)
        {
            expression = $"REPLACE({expression}, NCHAR({(int)tashkeel}), N'')";
        }
        return expression;
    }
}
