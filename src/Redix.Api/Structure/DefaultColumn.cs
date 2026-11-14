namespace Redix.Api.Structure;

/// <summary>A column of a project's default template (القالب الافتراضي): what a new item of one type starts with (FR-38, FR-41). Each project has its own, so changing it touches no other (BR-33).</summary>
public class DefaultColumn : Column, IProjectRow
{
    /// <summary>The project whose template this is.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The type of item that starts with it.</summary>
    public ItemType ItemType { get; init; }

    /// <summary>A new column a person described, at <paramref name="sortOrder"/>.</summary>
    public static DefaultColumn New(Guid projectId, ItemType itemType, AddColumnRequest request, int sortOrder) => new()
    {
        ProjectId = projectId,
        ItemType = itemType,
        Key = NewCustomKey(),
        Name = request.Name.Trim(),
        DataType = request.DataType,
        IsRequired = request.IsRequired,
        ShowInReports = request.ShowInReports,
        SortOrder = sortOrder,
        Choices = request.DataType == ColumnDataType.List ? ChoicesOf(request.Choices) : [],
    };

    /// <summary>The same column on the template of another project.</summary>
    public DefaultColumn CopyFor(Guid projectId) => new()
    {
        ProjectId = projectId,
        ItemType = ItemType,
        Key = Key,
        Name = Name,
        DataType = DataType,
        IsCore = IsCore,
        IsRequired = IsRequired,
        ShowInReports = ShowInReports,
        SortOrder = SortOrder,
        Choices = [.. Choices.Select(choice => choice.Copy())],
    };
}
