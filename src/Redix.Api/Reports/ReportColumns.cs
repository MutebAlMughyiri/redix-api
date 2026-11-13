using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Structure;
using Redix.Api.Translations;

namespace Redix.Api.Reports;

/// <summary>
/// What an item's table shows of its item's columns in the reports (FR-46, redix-api #129, R1): the columns it prints, each with the list its cells choose from, and what an entry
/// shows in each. The one owner, so the screen, the workbook and the PDF print the same columns of the same table.
/// </summary>
/// <param name="Suppliers">The project's supplier register, by name.</param>
/// <param name="Accounts">The project's accounts, by name.</param>
/// <param name="PaymentMethods">The project's payment methods, in their order.</param>
public sealed record ReportColumns(List<string> Suppliers, List<string> Accounts, List<string> PaymentMethods)
{
    /// <summary>What a file's cell says when the entry carries one, as the approved workbook writes it.</summary>
    public const string AttachmentWord = "فتح المرفق";

    /// <summary>
    /// The columns whose values make «الوصف», the first column of every table, as they did before redix-api #129: the product, or else the notes. They print in it, not as columns of
    /// their own, until Muteb decides what «الوصف» is once «ملاحظات» goes (his words, 13 Nov 2026: "احذف عمود ملاحظات بالكامل").
    /// </summary>
    private static readonly HashSet<string> DescriptionKeys = ["product", "notes"];

    /// <summary>The registers of the project <paramref name="projectId"/> its list columns choose from.</summary>
    public static async Task<ReportColumns> OfProjectAsync(Guid projectId, RedixDbContext db) => new(
        await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).OrderBy(supplier => supplier.Name).Select(supplier => supplier.Name).ToListAsync(),
        await db.Accounts.Where(account => account.ProjectId == projectId).OrderBy(account => account.Name).Select(account => account.Name).ToListAsync(),
        await db.PaymentMethods.Where(method => method.ProjectId == projectId).OrderBy(method => method.SortOrder).Select(method => method.Name).ToListAsync());

    /// <summary>
    /// The columns of <paramref name="item"/> a table of entries of <paramref name="kind"/> prints, in their order (R1): those it asks that print in the reports, but its amount, which
    /// is the table's last column, and what «الوصف» is made of, which is its first.
    /// </summary>
    public static List<ItemColumn> PrintedOf(Item item, EntryKind kind) =>
        [.. item.Columns.Where(column => !column.IsHidden && column.ShowInReports && column.IsAskedOf(kind) && !column.IsAmount && !DescriptionKeys.Contains(column.Key)).OrderBy(column => column.SortOrder)];

    /// <summary>The name of the amount column of <paramref name="item"/>'s tables of <paramref name="kind"/>, its header (redix-api #129); empty when the item has none.</summary>
    public static string AmountNameOf(Item item, EntryKind kind) => item.Columns.Where(column => column.IsAmount && column.IsAskedOf(kind)).Select(column => column.Name).FirstOrDefault("");

    /// <summary>What «الوصف» says of <paramref name="entry"/>: its product, or else its notes, as before redix-api #129.</summary>
    public static string DescriptionOf(Entry entry)
    {
        var product = entry.ColumnValuesByKey().GetValueOrDefault("product", "");
        return product.Length > 0 ? product : entry.Notes;
    }

    /// <summary>
    /// <paramref name="columns"/> as a report keeps them: each with its name and type, and for a list the choices its cells take, with their signs (redix-api #129): its own, the
    /// project's register it reads, or a contract's two places.
    /// </summary>
    public List<ReportColumnView> ViewsOf(List<ItemColumn> columns) => [.. columns.Select(column => new ReportColumnView(column.Name, column.DataType, column.IsRequired, ChoicesOf(column)))];

    /// <summary>What <paramref name="entry"/> shows under each of <paramref name="columns"/>, as text: a date as <c>yyyy-MM-dd</c>, a number as it was kept, a list's choice by its name.</summary>
    public static List<string> ValuesOf(List<ItemColumn> columns, Entry entry, EntryFacts facts, bool isOutsideContract)
    {
        var values = entry.ColumnValuesByKey();
        return [.. columns.Select(column => column.Key switch
        {
            "date" => entry.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "supplier" or "contract" => facts.SupplierNameOf(entry),
            "account" => facts.AccountNameOf(entry),
            "paymentMethod" => facts.PaymentMethodNameOf(entry),
            "attachment" => facts.AttachmentIdOf(entry).Length > 0 ? AttachmentWord : "",
            "outsideContract" => PlaceInContract(isOutsideContract),
            _ when column.DataType == ColumnDataType.List => column.ChoiceOf(values.GetValueOrDefault(column.Key, "")).Name,
            _ => values.GetValueOrDefault(column.Key, ""),
        })];
    }

    private List<ReportChoiceView> ChoicesOf(ItemColumn column) => column.Key switch
    {
        "supplier" => NamesOnly(Suppliers),
        "account" => NamesOnly(Accounts),
        "paymentMethod" => NamesOnly(PaymentMethods),
        "outsideContract" => NamesOnly([PlaceInContract(false), PlaceInContract(true)]),
        _ => [.. column.Choices.Select(choice => new ReportChoiceView(choice.Name, choice.Sign))],
    };

    private static List<ReportChoiceView> NamesOnly(List<string> names) => [.. names.Select(name => new ReportChoiceView(name, ChoiceSign.None))];

    private static string PlaceInContract(bool isOutside) => TranslationSeed.Arabic(isOutside ? "contractorPayments.outside" : "contractorPayments.inside");
}

/// <summary>A column of a table in a report, as the report keeps it (redix-api #129).</summary>
/// <param name="Name">Its name, which is its header.</param>
/// <param name="DataType">What it holds, which says how its cells are written.</param>
/// <param name="IsRequired">Whether entry asks it, which says whether a cell may be left empty in the workbook.</param>
/// <param name="Choices">The choices of a list, with their signs; empty for any other column.</param>
public record ReportColumnView(string Name, ColumnDataType DataType, bool IsRequired, List<ReportChoiceView> Choices);

/// <summary>A choice of a list column in a report, with the sign it gives the amount.</summary>
/// <param name="Name">Its name.</param>
/// <param name="Sign">Its sign; none for a register's name.</param>
public record ReportChoiceView(string Name, ChoiceSign Sign);
