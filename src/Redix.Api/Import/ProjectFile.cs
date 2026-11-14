using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Import;

/// <summary>
/// A project's file (FR-53, redix-api #130, #132): a project that lived in Excel, in REDIX's own model, as the generated workbook lays it out. Its report, what each account really
/// held (D11), every list its cells choose from, and its categories with their items and tables, each table with its columns and rows. What is worked out is not in it, and the land
/// details are not read (D10). An empty value is <c>""</c>, never null.
/// </summary>
public sealed record ProjectFile
{
    /// <summary>What a REDIX project file says it is.</summary>
    public const string FormatName = "redix.project";

    /// <summary>The newest version of the file this API reads.</summary>
    public const int LatestVersion = 1;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        RespectNullableAnnotations = true,
    };

    /// <summary>No file: what a text that is not one reads as, so no reader holds a null.</summary>
    public static ProjectFile None { get; } = new();

    /// <summary>The report the file was written from; numbered 0 when it holds none (D9).</summary>
    public ProjectFileReport Report { get; init; } = new();

    /// <summary>What each account really held at that report (D11); empty when the file holds none.</summary>
    public List<ProjectFileAccount> Accounts { get; init; } = [];

    /// <summary>Every list a table's cells choose from, once, as the hidden «القوائم» sheet holds them.</summary>
    public List<ProjectFileList> Lists { get; init; } = [];

    /// <summary>Its categories, in the summary's order.</summary>
    public List<ProjectFileCategory> Categories { get; init; } = [];

    /// <summary>
    /// Reads <paramref name="text"/> as a project file (AC-53.9): one that does not say it is a REDIX project file, or is not laid out as one, a report with no date and time among
    /// it, answers <c>notAProjectFile</c>, and one of a version newer than <see cref="LatestVersion"/> <c>newerVersion</c>, both before anything else is read.
    /// </summary>
    public static (ProjectFile File, string Refusal) Read(string text)
    {
        try
        {
            var header = JsonSerializer.Deserialize<ProjectFileHeader>(text, Json);
            if (header is null || header.Format != FormatName || header.Version < 1)
            {
                return (None, "NotAProjectFile");
            }
            if (header.Version > LatestVersion)
            {
                return (None, "NewerVersion");
            }
            var file = JsonSerializer.Deserialize<ProjectFile>(text, Json) ?? None;
            return file.Report.Number > 0 && file.Report.At == DateTime.MinValue ? (None, "NotAProjectFile") : (file, "");
        }
        catch (JsonException)
        {
            return (None, "NotAProjectFile");
        }
    }

    /// <summary>What a file says it is, read before the rest so a newer one is told apart from one that is not a project file at all.</summary>
    /// <param name="Format">What it says it is.</param>
    /// <param name="Version">Its version.</param>
    private sealed record ProjectFileHeader(string Format = "", int Version = 0);
}

/// <summary>The report a project file was written from (D9): its number, and its date and time in Saudi Arabia.</summary>
public sealed record ProjectFileReport
{
    /// <summary>Its number; 0 when the file holds no report.</summary>
    public int Number { get; init; }

    /// <summary>Its date, as <c>yyyy-MM-dd</c>.</summary>
    public string Date { get; init; } = "";

    /// <summary>Its time, as <c>HH:mm</c>.</summary>
    public string Time { get; init; } = "";

    /// <summary>When it was issued, in UTC: its date and time read in Saudi Arabia's time (<see cref="Entry.SaudiOffset"/>); <see cref="DateTime.MinValue"/> when they are not a date and a time.</summary>
    public DateTime At =>
        DateOnly.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && TimeOnly.TryParseExact(Time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? new DateTimeOffset(date.ToDateTime(time), Entry.SaudiOffset).UtcDateTime
            : DateTime.MinValue;
}

/// <summary>One account of a project file, by its name, and what it really held at the file's report (D11).</summary>
public sealed record ProjectFileAccount
{
    /// <summary>The account's name, which an account of the project must hold (D6).</summary>
    public string Name { get; init; } = "";

    /// <summary>What it really held.</summary>
    public decimal ActualBalance { get; init; }
}

/// <summary>One list of a project file: its key, which a list column names, and its options, each with the id a row keeps.</summary>
public sealed record ProjectFileList
{
    /// <summary>Its key, such as <c>account</c> or <c>entryType</c>.</summary>
    public string Key { get; init; } = "";

    /// <summary>Its options.</summary>
    public List<ProjectFileOption> Options { get; init; } = [];
}

/// <summary>One option of a project file's list: the id a row keeps, such as <c>account-1</c>, and its label, the name REDIX knows it by.</summary>
public sealed record ProjectFileOption
{
    /// <summary>The id a row keeps when it chose this option.</summary>
    public string Id { get; init; } = "";

    /// <summary>Its label: an account's, a supplier's or a payment method's name, or a list's choice.</summary>
    public string Label { get; init; } = "";
}

/// <summary>One category of a project file, with its items.</summary>
public sealed record ProjectFileCategory
{
    /// <summary>Its name.</summary>
    public string Name { get; init; } = "";

    /// <summary>Its items, in order.</summary>
    public List<ProjectFileItem> Items { get; init; } = [];
}

/// <summary>One item of a project file: its name, what it is for, and its tables.</summary>
public sealed record ProjectFileItem
{
    /// <summary>Its name.</summary>
    public string Name { get; init; } = "";

    /// <summary>What it is for (FR-38).</summary>
    public ItemType Type { get; init; }

    /// <summary>Its tables, in the order its sheet prints them.</summary>
    public List<ProjectFileTable> Tables { get; init; } = [];
}

/// <summary>
/// One table of a project file: its name, what its entries are (purchases, contractor payments, capital, or a revenue item's), its columns and its rows; a contract's payments
/// also carry what was agreed.
/// </summary>
public sealed record ProjectFileTable
{
    /// <summary>Its name: the item's own for its general table, a subgroup's, «دفعات» and the contractor's for a contract's payments, or a partner's.</summary>
    public string Name { get; init; } = "";

    /// <summary>What its entries are, named as an item's type is.</summary>
    public ItemType Kind { get; init; }

    /// <summary>Its columns, in order.</summary>
    public List<ProjectFileColumn> Columns { get; init; } = [];

    /// <summary>Its rows, in order, each holding a value under a column's key: text, a number, or a list option's id.</summary>
    public List<Dictionary<string, JsonElement>> Rows { get; init; } = [];

    /// <summary>What a contract's payments were agreed at; <c>""</c> for an open value, and absent on any other table.</summary>
    public JsonElement ContractAmount { get; init; }
}

/// <summary>One column of a project file's table: its key, what it holds, and for a list the key of the list it chooses from.</summary>
public sealed record ProjectFileColumn
{
    /// <summary>Its key, the same as REDIX's column's.</summary>
    public string Key { get; init; } = "";

    /// <summary>Its name, its header in the workbook.</summary>
    public string Name { get; init; } = "";

    /// <summary>What it holds.</summary>
    public ColumnDataType Type { get; init; }

    /// <summary>The key of the list its cells choose from; empty for any column but a list.</summary>
    public string List { get; init; } = "";
}
