using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>The default template every database starts with (BRS section 3): a project nobody is a member of, whose categories, items and columns a new project copies. After the first migration it is data.</summary>
public static class StructureSeed
{
    private static readonly (string Category, (string Name, ItemType Type)[] Items)[] Seeded =
    [
        ("تكلفة الأراضي", [("تكلفة الأرض", ItemType.Both)]),
        ("الرسوم والفواتير",
        [
            ("المكتب الهندسي", ItemType.Both), ("رسوم الأمانة", ItemType.Both), ("المكتب المشرف", ItemType.Both), ("التأمين", ItemType.Both), ("المحاسب", ItemType.Both),
            ("أخرى", ItemType.Both),
        ]),
        ("العظم",
        [
            ("الحفر", ItemType.Both), ("الحديد", ItemType.Purchases), ("الخرسانة", ItemType.Purchases), ("البناء", ItemType.Both), ("دفعات العظم", ItemType.ContractorPayments),
            ("الكهرباء", ItemType.Both), ("السباكة", ItemType.Both), ("ردود الماء", ItemType.Both), ("أغراض الموقع", ItemType.Both),
        ]),
        ("التشطيب",
        [
            ("السباكة", ItemType.Both), ("الكهرباء", ItemType.Both), ("أبواب خارجية", ItemType.Both), ("لياسة", ItemType.Both), ("جبس", ItemType.Both), ("بلاط", ItemType.Both),
            ("بويه", ItemType.Both), ("ألمنيوم", ItemType.Both), ("درابزين", ItemType.Both), ("حجر الواجهة", ItemType.Both), ("أبواب داخلية", ItemType.Both), ("العوازل", ItemType.Both),
            ("معدات تشطيب", ItemType.Both), ("رواتب الحارس", ItemType.Both),
        ]),
        ("الإشراف", [("إشراف مالي", ItemType.Both), ("إشراف ميداني", ItemType.Both)]),
        ("رأس المال", [("رأس المال", ItemType.Capital)]),
        ("استرجاع ضرائب", [("استرجاع الضرائب", ItemType.VatRefund)]),
        ("إيرادات أخرى", [("إيرادات أخرى", ItemType.OtherRevenue)]),
        ("فلوس البيع المودعة بالحساب", [("البيع", ItemType.Sale)]),
    ];

    /// <summary>The id of «شراء», the choice of «نوع القيد» that counts a purchase as typed (redix-api #129).</summary>
    public static Guid PurchaseChoiceId { get; } = StableIds.Of("choice:entryType:purchase");

    /// <summary>The id of «مرتجع», the choice of «نوع القيد» that makes a purchase a credit note, its total taken away (redix-api #119 D4, #129).</summary>
    public static Guid CreditNoteChoiceId { get; } = StableIds.Of("choice:entryType:creditNote");

    /// <summary>The id of «إضافة مبلغ», the choice of «نوع الحركة» for money a partner puts in (BR-05, redix-api #129).</summary>
    public static Guid ContributionChoiceId { get; } = StableIds.Of("choice:movementType:contribution");

    /// <summary>The id of «سحب مبلغ», the choice of «نوع الحركة» for money a partner takes out (BR-06, redix-api #129).</summary>
    public static Guid WithdrawalChoiceId { get; } = StableIds.Of("choice:movementType:withdrawal");

    /// <summary>
    /// A purchase's columns, in the order of the report workbook Muteb approved on 13 Nov 2026 (redix-api #129): المورد, التاريخ, the unit price and the quantity, نوع القيد, الحساب,
    /// طريقة الدفع, المرفق, and the total, which prints last. «نوع القيد» is the list whose sign decides the total's direction: «مرتجع» takes it away.
    /// </summary>
    private static readonly TemplateColumn[] PurchaseColumns =
    [
        new("supplier", "المورد", ColumnDataType.List, true, false, true),
        new("date", "التاريخ", ColumnDataType.Date, true, true, true),
        new("product", "الصنف", ColumnDataType.Text, false, false, true),
        new("unitPrice", "سعر الوحدة", ColumnDataType.Amount, false, false, true),
        new("quantity", "العدد", ColumnDataType.Number, false, false, true),
        new("entryType", "نوع القيد", ColumnDataType.List, true, false, true,
            [new ColumnChoice { Id = PurchaseChoiceId, Name = "شراء", Sign = ChoiceSign.Plus }, new ColumnChoice { Id = CreditNoteChoiceId, Name = "مرتجع", Sign = ChoiceSign.Minus }]),
        new("account", "الحساب", ColumnDataType.List, true, true, true),
        new("paymentMethod", "طريقة الدفع", ColumnDataType.List, false, false, true),
        new("attachment", "المرفق", ColumnDataType.File, true, false, true),
        new("notes", "ملاحظات", ColumnDataType.Text, false, false, false),
        new("total", "الإجمالي", ColumnDataType.Amount, true, true, true),
    ];

    /// <summary>A contractor payment's columns, in the order of the approved workbook: التاريخ, whether it is inside the contract, الحساب, طريقة الدفع, المرفق, and the amount, last.</summary>
    private static readonly TemplateColumn[] ContractorPaymentColumns =
    [
        new("date", "التاريخ", ColumnDataType.Date, true, true, true),
        new("outsideContract", "داخل أو خارج العقد", ColumnDataType.List, true, false, true),
        new("account", "الحساب", ColumnDataType.List, true, true, true),
        new("paymentMethod", "طريقة الدفع", ColumnDataType.List, false, false, true),
        new("attachment", "المرفق", ColumnDataType.File, true, false, true),
        new("contract", "العقد", ColumnDataType.List, true, true, false),
        new("supplier", "المورد", ColumnDataType.List, true, false, false),
        new("notes", "ملاحظات", ColumnDataType.Text, false, false, true),
        new("amount", "المبلغ", ColumnDataType.Amount, true, true, true),
    ];

    /// <summary>
    /// A partner's money in or out, the columns of رأس المال (redix-api #129): «نوع الحركة», the list whose sign decides whether it is put in or taken out, التاريخ, الحساب, المرفق, and
    /// the amount, last.
    /// </summary>
    private static readonly TemplateColumn[] CapitalColumns =
    [
        new("movementType", "نوع الحركة", ColumnDataType.List, true, false, true,
            [new ColumnChoice { Id = ContributionChoiceId, Name = "إضافة مبلغ", Sign = ChoiceSign.Plus }, new ColumnChoice { Id = WithdrawalChoiceId, Name = "سحب مبلغ", Sign = ChoiceSign.Minus }]),
        new("date", "التاريخ", ColumnDataType.Date, true, true, true),
        new("account", "الحساب", ColumnDataType.List, true, true, true),
        new("attachment", "المرفق", ColumnDataType.File, true, false, true),
        new("amount", "المبلغ", ColumnDataType.Amount, true, true, true),
    ];

    /// <summary>Revenue's columns, the sale, the VAT refunded and anything else that comes in (redix-api #129): التاريخ and the amount print, the account and the file are asked.</summary>
    private static readonly TemplateColumn[] RevenueColumns =
    [
        new("date", "التاريخ", ColumnDataType.Date, true, true, true),
        new("account", "الحساب", ColumnDataType.List, true, true, false),
        new("attachment", "المرفق", ColumnDataType.File, true, false, false),
        new("amount", "المبلغ", ColumnDataType.Amount, true, true, true),
    ];

    /// <summary>
    /// Whether an entry of <paramref name="kind"/> is asked the column <paramref name="key"/> (FR-38): an item of both types holds both templates' columns, and a purchase is never asked
    /// what only a contractor payment's template has, nor a payment what only a purchase's has, its supplier among them, since its contract names the contractor. Every other column, a
    /// person's own too, is asked of both.
    /// </summary>
    public static bool IsAsked(string key, EntryKind kind) => kind switch
    {
        EntryKind.Purchase => !IsOnlyIn(ContractorPaymentColumns, PurchaseColumns, key),
        EntryKind.ContractorPayment => key != "supplier" && !IsOnlyIn(PurchaseColumns, ContractorPaymentColumns, key),
        _ => true,
    };

    private static bool IsOnlyIn(TemplateColumn[] columns, TemplateColumn[] others, string key) => columns.Any(column => column.Key == key) && others.All(column => column.Key != key);

    /// <summary>The id of the default template, the same in every database.</summary>
    public static Guid TemplateId { get; } = StableIds.Of("project:template");

    /// <summary>The default template.</summary>
    public static Project Template
    {
        get
        {
            var template = new Project { Id = TemplateId, Name = "القالب الافتراضي", StartDate = new DateOnly(1900, 1, 1), IsTemplate = true };
            template.Approve();
            return template;
        }
    }

    /// <summary>Its categories, in the order of the report.</summary>
    public static IEnumerable<Category> Categories => Seeded.Select((category, index) => new Category { Id = CategoryId(category.Category), ProjectId = TemplateId, Name = category.Category, SortOrder = index });

    /// <summary>Their items.</summary>
    public static IEnumerable<Item> Items => Seeded.SelectMany(category => category.Items.Select((item, index) =>
        new Item { Id = ItemId(category.Category, item.Name), CategoryId = CategoryId(category.Category), Name = item.Name, Type = item.Type, SortOrder = index, ShowsTablePercentages = item.Type == ItemType.Capital }));

    /// <summary>Each item's general group.</summary>
    public static IEnumerable<Subgroup> Subgroups => Items.Select(item => new Subgroup { Id = StableIds.Of($"subgroup:{item.Id}"), ItemId = item.Id, Name = Subgroup.GeneralName });

    /// <summary>
    /// What an item of each type starts with (FR-38, redix-api #129): a purchase's, a contractor payment's, both, رأس المال's and revenue's. An item of both types holds the purchase
    /// columns, with what only a contractor payment has, which no purchase can require, each after the column it follows in a payment's template.
    /// </summary>
    public static IEnumerable<DefaultColumn> DefaultColumns =>
    [
        .. ColumnsOf(ItemType.Purchases, PurchaseColumns),
        .. ColumnsOf(ItemType.ContractorPayments, ContractorPaymentColumns),
        .. ColumnsOf(ItemType.Both, BothColumns()),
        .. ColumnsOf(ItemType.Capital, CapitalColumns),
        .. new[] { ItemType.Sale, ItemType.VatRefund, ItemType.OtherRevenue }.SelectMany(type => ColumnsOf(type, RevenueColumns)),
    ];

    private static TemplateColumn[] BothColumns()
    {
        var both = PurchaseColumns.ToList();
        foreach (var (column, place) in ContractorPaymentColumns.Select((column, place) => (column, place)).Where(pair => PurchaseColumns.All(purchase => purchase.Key != pair.column.Key)))
        {
            var after = ContractorPaymentColumns.Take(place).LastOrDefault(earlier => both.Any(kept => kept.Key == earlier.Key));
            both.Insert(after is null ? 0 : both.FindIndex(kept => kept.Key == after.Key) + 1, column with { IsRequired = false });
        }
        return [.. both];
    }

    private static IEnumerable<DefaultColumn> ColumnsOf(ItemType type, TemplateColumn[] columns) =>
        columns.Select((column, index) => new DefaultColumn
        {
            Id = StableIds.Of($"column:{type}:{column.Key}"),
            ProjectId = TemplateId,
            ItemType = type,
            Key = column.Key,
            Name = column.Name,
            DataType = column.DataType,
            IsCore = column.IsCore,
            IsRequired = column.IsRequired,
            ShowInReports = column.ShowInReports,
            SortOrder = index,
            Choices = [.. column.Choices.Select(choice => choice.Copy())],
        });

    /// <summary>One column of a type's template, as the seed declares it.</summary>
    private sealed record TemplateColumn(string Key, string Name, ColumnDataType DataType, bool IsCore, bool IsRequired, bool ShowInReports, ColumnChoice[] Choices)
    {
        public TemplateColumn(string key, string name, ColumnDataType dataType, bool isCore, bool isRequired, bool showInReports) : this(key, name, dataType, isCore, isRequired, showInReports, []) { }
    }

    private static Guid CategoryId(string category) => StableIds.Of($"category:{category}");

    private static Guid ItemId(string category, string item) => StableIds.Of($"item:{category}:{item}");
}
