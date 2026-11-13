using System.Globalization;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>A field an item asks for at entry and may print in its report (FR-41). It owns what both the default template and one item allow (BR-20, BR-21).</summary>
public abstract class Column : IOrdered
{
    /// <summary>The longest key a column may have.</summary>
    public const int KeyMaxLength = 50;

    /// <summary>The longest value an entry may give a column.</summary>
    public const int ValueMaxLength = 1000;

    private const string CustomKeyPrefix = "custom.";

    /// <summary>The column's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>What names this column in code and in an entry's values; the same on the default column and on every item that took it.</summary>
    public required string Key { get; init; }

    /// <summary>The column's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>What it holds; set when it is added, and never changed (BR-21).</summary>
    public ColumnDataType DataType { get; init; }

    /// <summary>True when the calculations and the tax statement depend on it: it is renamed and reordered, and never deleted (BR-20).</summary>
    public bool IsCore { get; init; }

    /// <summary>Whether entry asks for it.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Whether it prints in the item's table in the reports.</summary>
    public bool ShowInReports { get; set; }

    /// <summary>Its place in its list.</summary>
    public int SortOrder { get; set; }

    /// <summary>The choices of a list kept in its definition, each with its id and its sign (redix-api #129); empty otherwise, a register's list included.</summary>
    public List<ColumnChoice> Choices { get; init; } = [];

    /// <summary>True for a list whose choices give the amount a sign: an item has at most one, so the amount's direction has one answer (redix-api #129).</summary>
    public bool IsSigned => Choices.Any(choice => choice.Sign != ChoiceSign.None);

    /// <summary>True for a column a person added, rather than one the template starts with.</summary>
    public bool IsCustom => Key.StartsWith(CustomKeyPrefix);

    /// <summary>A key for a column a person adds.</summary>
    public static string NewCustomKey() => CustomKeyPrefix + Guid.CreateVersion7().ToString("N");

    /// <summary>Whether an entry can give this column <paramref name="value"/>: a number, an amount with at most two decimals, a date as <c>yyyy-MM-dd</c>, the id of one of a list's choices (redix-api #129), or a text within its limit.</summary>
    public bool Accepts(string value) => DataType switch
    {
        ColumnDataType.Number => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
        ColumnDataType.Amount => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && Money.IsAcceptable(amount),
        ColumnDataType.Date => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
        ColumnDataType.List => ChoiceOf(value) != ColumnChoice.None,
        _ => value.Length <= ValueMaxLength,
    };

    /// <summary>The choice whose id <paramref name="value"/> holds; <see cref="ColumnChoice.None"/> when it names none of this column's.</summary>
    public ColumnChoice ChoiceOf(string value) => Guid.TryParse(value, out var id) ? Choices.FirstOrDefault(choice => choice.Id == id, ColumnChoice.None) : ColumnChoice.None;

    /// <summary>Why a column with these values cannot be added, or <see cref="StructureChangeOutcome.Done"/>. A person chooses among five types, and a list needs a choice (FR-41).</summary>
    public static StructureChangeOutcome CheckNew(string name, ColumnDataType dataType, List<ChoiceRequest> choices)
    {
        if (!Project.IsAcceptableName(name.Trim()))
        {
            return StructureChangeOutcome.InvalidName;
        }
        if (dataType == ColumnDataType.File)
        {
            return StructureChangeOutcome.InvalidDataType;
        }
        var typed = ChoicesOf(choices);
        return dataType == ColumnDataType.List && (typed.Count == 0 || !typed.All(choice => Project.IsAcceptableName(choice.Name))) ? StructureChangeOutcome.InvalidOptions : StructureChangeOutcome.Done;
    }

    /// <summary>What was typed as the choices of a list, each a new choice with its sign: each name trimmed, the empty ones and the repeated ones gone.</summary>
    public static List<ColumnChoice> ChoicesOf(List<ChoiceRequest> choices) =>
        choices.Select(choice => choice with { Name = choice.Name.Trim() }).Where(choice => choice.Name.Length > 0).DistinctBy(choice => choice.Name).Select(choice => new ColumnChoice { Name = choice.Name, Sign = choice.Sign }).ToList();

    /// <summary>
    /// Changes the name, and on a column that is not core whether entry asks for it and whether it prints (BR-20). The type never changes (BR-21). A list a person added gains <paramref name="newChoices"/>,
    /// each within the limit of a name, or it answers <see cref="StructureChangeOutcome.InvalidOptions"/>, as any other column given one does. Its own choices stay, because entries hold their ids.
    /// </summary>
    public StructureChangeOutcome Edit(string name, bool isRequired, bool showInReports, List<ChoiceRequest> newChoices)
    {
        if (!Project.IsAcceptableName(name.Trim()))
        {
            return StructureChangeOutcome.InvalidName;
        }
        if (IsCore && (isRequired != IsRequired || showInReports != ShowInReports))
        {
            return StructureChangeOutcome.CoreColumn;
        }
        var choices = ChoicesOf(newChoices);
        if (choices.Count > 0 && (DataType != ColumnDataType.List || !IsCustom || !choices.All(choice => Project.IsAcceptableName(choice.Name))))
        {
            return StructureChangeOutcome.InvalidOptions;
        }
        Name = name.Trim();
        IsRequired = isRequired;
        ShowInReports = showInReports;
        Choices.AddRange(choices.Where(choice => Choices.All(kept => kept.Name != choice.Name)));
        return StructureChangeOutcome.Done;
    }
}
