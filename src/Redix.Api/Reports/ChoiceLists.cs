using ClosedXML.Excel;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>
/// The sheet «القوائم», hidden and last in a workbook (redix-api #129): every list a table's cells choose from, a block each, titled with the list's column, its choices under
/// «الاختيار» with their signs under «الإشارة». Each list cell's data validation reads its block, so the choices live in one place, as a column's definition keeps them.
/// </summary>
public sealed class ChoiceLists
{
    private const string Name = "القوائم";

    private readonly List<ChoiceBlock> _blocks = [];

    private int _nextRow = 1;

    /// <summary>The lists of a workbook whose sheets take <paramref name="taken"/> names; the sheet's own name is taken now, so every validation can name it before it is written.</summary>
    public ChoiceLists(HashSet<string> taken) => SheetName = ReportFormats.SheetName(Name, taken);

    /// <summary>The sheet's name in its workbook.</summary>
    public string SheetName { get; }

    /// <summary>The cells of the block holding <paramref name="choices"/>, the list of the column <paramref name="title"/>, as a validation names them; one list shares one block.</summary>
    public string RangeOf(string title, List<ReportChoiceView> choices)
    {
        var block = _blocks.FirstOrDefault(kept => kept.Title == title && kept.Choices.SequenceEqual(choices), ChoiceBlock.None);
        if (block == ChoiceBlock.None)
        {
            block = new ChoiceBlock(title, choices, _nextRow);
            _blocks.Add(block);
            _nextRow += choices.Count + 3;
        }
        return StatementStyle.Reference(SheetName, $"$A${block.Row + 2}:$A${block.Row + 1 + choices.Count}");
    }

    /// <summary>Adds the sheet to <paramref name="workbook"/>, hidden, after every other, when any table holds a list; nothing otherwise.</summary>
    public void AddTo(XLWorkbook workbook, string footer)
    {
        if (_blocks.Count == 0)
        {
            return;
        }
        var sheet = StatementStyle.AddSheet(workbook, SheetName, [], 18, 10);
        foreach (var (title, choices, row) in _blocks)
        {
            sheet.Range(row, 1, row, 2).Merge().AsTitle().FirstCell().Value = title;
            sheet.Range(row + 1, 1, row + 1, 2).AsHeader();
            sheet.Cell(row + 1, 1).Value = "الاختيار";
            sheet.Cell(row + 1, 2).Value = "الإشارة";
            foreach (var (choice, place) in choices.Select((choice, place) => (choice, place)))
            {
                sheet.Cell(row + 2 + place, 1).Value = choice.Name;
                sheet.Cell(row + 2 + place, 2).SetCentred(SignOf(choice.Sign));
            }
            sheet.Range(row + 2, 1, row + 1 + choices.Count, 2).AsBody();
        }
        StatementStyle.Print(sheet, footer);
        sheet.Hide();
    }

    /// <summary>A choice's sign as the workbook writes it: + or −, nothing for none.</summary>
    private static string SignOf(ChoiceSign sign) => sign switch
    {
        ChoiceSign.Plus => "+",
        ChoiceSign.Minus => "−",
        _ => "",
    };
}

/// <summary>One list's block on «القوائم»: the column it is the list of, its choices, and the row of its title.</summary>
/// <param name="Title">The column's name.</param>
/// <param name="Choices">Its choices, with their signs.</param>
/// <param name="Row">The row of its title.</param>
internal sealed record ChoiceBlock(string Title, List<ReportChoiceView> Choices, int Row)
{
    /// <summary>No block: what a list not yet written finds.</summary>
    public static readonly ChoiceBlock None = new("", [], 0);
}
