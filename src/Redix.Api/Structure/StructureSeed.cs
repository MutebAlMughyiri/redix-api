using Redix.Api.Projects;

namespace Redix.Api.Structure;

/// <summary>The default template every database starts with (BRS section 3): a project nobody is a member of, whose categories and items a new project copies. After the first migration it is data.</summary>
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

    private static Guid CategoryId(string category) => StableIds.Of($"category:{category}");

    private static Guid ItemId(string category, string item) => StableIds.Of($"item:{category}:{item}");
}
