using System.Globalization;
using System.Reflection;
using System.Text.Json;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>How every report file writes a number and a date (NFR-13), and the Arabic font it embeds: the one owner, so the PDF, the workbook and the screen agree.</summary>
public static class ReportFormats
{
    /// <summary>The embedded font's family.</summary>
    public const string FontFamily = "IBM Plex Sans Arabic";

    /// <summary>An amount in a workbook: separators, two decimals, and a negative in brackets.</summary>
    public const string ExcelMoney = "#,##0.00;(#,##0.00)";

    /// <summary>A date in the periodic report's workbook.</summary>
    public const string ExcelDate = "dd.mm.yyyy";

    /// <summary>A date in the VAT statement's workbook, as the accountant's file has it (FR-47).</summary>
    public const string ExcelVatDate = "yyyy-mm-dd";

    private static readonly Lazy<bool> Ready = new(() =>
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.EndsWith(".ttf", StringComparison.Ordinal)))
        {
            using var font = assembly.GetManifestResourceStream(name)!;
            FontManager.RegisterFontFromStream(font);
        }
        return true;
    });

    /// <summary>Makes the PDF library ready, once: its licence and the embedded font (NFR-13).</summary>
    public static void PreparePdf() => _ = Ready.Value;

    /// <summary>An amount as 1,234.56, a negative in brackets, rounded half up from the kept decimals (BR-13).</summary>
    public static string Money(decimal amount)
    {
        var text = Math.Abs(decimal.Round(amount, 2, MidpointRounding.AwayFromZero)).ToString("#,##0.00", CultureInfo.InvariantCulture);
        return amount < 0 ? $"({text})" : text;
    }

    /// <summary>What a partner's movement is, in the statement's Arabic, for a movement with no description of its own: both report files say the same.</summary>
    public static string Movement(CapitalMovementType type) => TranslationSeed.Arabic($"capital.{JsonNamingPolicy.CamelCase.ConvertName(type.ToString())}");

    /// <summary>A date as dd.MM.yyyy.</summary>
    public static string Date(DateOnly date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    /// <summary>A time as dd.MM.yyyy HH:mm, in Saudi Arabia.</summary>
    public static string Time(DateTime utc) => InSaudiArabia(utc).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    /// <summary>A moment in Saudi Arabia's time (<see cref="Entry.SaudiOffset"/>).</summary>
    public static DateTime InSaudiArabia(DateTime utc) => utc + Entry.SaudiOffset;

    /// <summary>A name Excel accepts for a sheet: no forbidden character, at most 31 letters, and not one already taken in <paramref name="taken"/>.</summary>
    public static string SheetName(string name, HashSet<string> taken)
    {
        var clean = new string(name.Select(letter => "[]:*?/\\".Contains(letter) ? ' ' : letter).ToArray()).Trim();
        clean = clean.Length == 0 ? "-" : clean[..Math.Min(clean.Length, 28)];
        var unique = clean;
        for (var copy = 2; !taken.Add(unique); copy++)
        {
            unique = $"{clean} {copy}";
        }
        return unique;
    }
}
