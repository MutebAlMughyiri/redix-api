namespace Redix.Api.Structure;

/// <summary>A category (التصنيف الرئيسي) of a project's expenses, such as العظم, holding its items (FR-39). Its place in the list is its place in the periodic report (AC-39.1).</summary>
public class Category : IOrdered
{
    /// <summary>The category's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project it belongs to.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The category's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>Its place in the project, which is the order in the report.</summary>
    public int SortOrder { get; set; }

    /// <summary>False once disabled: it and its items disappear from entry and stay in the reports (BR-15).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Its items.</summary>
    public List<Item> Items { get; init; } = [];

    /// <summary>What entering <paramref name="kind"/> offers from this category (AC-05.3, AC-38.1).</summary>
    public List<Item> ItemsOfferedFor(ItemType kind) => IsActive ? Items.Where(item => item.IsOfferedFor(kind)).ToList() : [];

    /// <summary>The same category with its active items, for a new project: what was disabled here stays behind.</summary>
    public Category CopyFor(Guid projectId, List<DefaultColumn> defaults)
    {
        var copy = new Category { ProjectId = projectId, Name = Name, SortOrder = SortOrder };
        copy.Items.AddRange(Items.Where(item => item.IsActive).Select(item => item.CopyFor(copy.Id, defaults)));
        return copy;
    }
}
