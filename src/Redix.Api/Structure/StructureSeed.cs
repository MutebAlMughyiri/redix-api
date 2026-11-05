using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>The default template every database starts with (BRS section 3): a project nobody is a member of, whose categories, items and columns a new project copies. After the first migration it is data.</summary>
public static class StructureSeed
{
    private static readonly (string Category, (string Name, ItemType Type)[] Items)[] Seeded =
    [
        ("تكلفة الأراضي", []),
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
    ];

    private static readonly (string Key, string Name, ColumnDataType DataType, bool IsCore, bool IsRequired, bool ShowInReports)[] PurchaseColumns =
    [
        ("date", "التاريخ", ColumnDataType.Date, true, true, true),
        ("supplier", "المورد", ColumnDataType.List, true, true, false),
        ("invoiceNumber", "رقم الفاتورة", ColumnDataType.Text, true, true, true),
        ("unit", "الوحدة", ColumnDataType.List, false, false, false),
        ("product", "الصنف", ColumnDataType.Text, false, false, true),
        ("unitPrice", "سعر الوحدة", ColumnDataType.Amount, false, false, true),
        ("quantity", "العدد", ColumnDataType.Number, false, false, true),
        ("amountBeforeDiscount", "المبلغ قبل الخصم", ColumnDataType.Amount, true, true, false),
        ("discount", "الخصم", ColumnDataType.Amount, true, false, false),
        ("vat", "الضريبة", ColumnDataType.Amount, true, false, false),
        ("total", "الإجمالي", ColumnDataType.Amount, true, false, true),
        ("paymentMethod", "طريقة الدفع", ColumnDataType.List, false, false, false),
        ("account", "الحساب", ColumnDataType.List, true, true, false),
        ("attachment", "المرفق", ColumnDataType.File, true, true, false),
        ("notes", "ملاحظات", ColumnDataType.Text, false, false, false),
    ];

    private static readonly (string Key, string Name, ColumnDataType DataType, bool IsCore, bool IsRequired, bool ShowInReports)[] ContractorPaymentColumns =
    [
        ("date", "التاريخ", ColumnDataType.Date, true, true, true),
        ("contract", "العقد", ColumnDataType.List, true, true, false),
        ("amount", "المبلغ", ColumnDataType.Amount, true, true, true),
        ("paymentMethod", "طريقة الدفع", ColumnDataType.List, false, false, false),
        ("account", "الحساب", ColumnDataType.List, true, true, false),
        ("outsideContract", "داخل أو خارج العقد", ColumnDataType.List, true, false, false),
        ("notes", "ملاحظات", ColumnDataType.Text, false, false, true),
        ("attachment", "المرفق", ColumnDataType.File, true, true, false),
        ("supplier", "المورد", ColumnDataType.List, true, false, false),
        ("invoiceNumber", "رقم الفاتورة", ColumnDataType.Text, true, false, false),
        ("amountBeforeDiscount", "المبلغ قبل الخصم", ColumnDataType.Amount, true, false, false),
        ("vat", "الضريبة", ColumnDataType.Amount, true, false, false),
    ];

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
        new Item { Id = ItemId(category.Category, item.Name), CategoryId = CategoryId(category.Category), Name = item.Name, Type = item.Type, SortOrder = index }));

    /// <summary>Each item's general group.</summary>
    public static IEnumerable<Subgroup> Subgroups => Items.Select(item => new Subgroup { Id = StableIds.Of($"subgroup:{item.Id}"), ItemId = item.Id, Name = Subgroup.GeneralName });

    /// <summary>What an item of each type starts with (FR-38). An item of both types holds the purchase columns, then what only a contractor payment has, which no purchase can require.</summary>
    public static IEnumerable<DefaultColumn> DefaultColumns =>
    [
        .. ColumnsOf(ItemType.Purchases, PurchaseColumns),
        .. ColumnsOf(ItemType.ContractorPayments, ContractorPaymentColumns),
        .. ColumnsOf(ItemType.Both, [.. PurchaseColumns, .. ContractorPaymentColumns.Where(column => PurchaseColumns.All(purchase => purchase.Key != column.Key)).Select(column => column with { IsRequired = false })]),
    ];

    private static IEnumerable<DefaultColumn> ColumnsOf(ItemType type, (string Key, string Name, ColumnDataType DataType, bool IsCore, bool IsRequired, bool ShowInReports)[] columns) =>
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
        });

    private static Guid CategoryId(string category) => StableIds.Of($"category:{category}");

    private static Guid ItemId(string category, string item) => StableIds.Of($"item:{category}:{item}");
}
