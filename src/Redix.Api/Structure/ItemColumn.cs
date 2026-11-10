namespace Redix.Api.Structure;

/// <summary>A column of one item (العمود): what entry asks for in this item, and what its table prints (FR-41).</summary>
public class ItemColumn : Column
{
    /// <summary>The item that asks for it.</summary>
    public Guid ItemId { get; init; }

    /// <summary>True once deleted: it leaves entry, while its values stay on their entries and in the History table (BR-21).</summary>
    public bool IsHidden { get; private set; }

    /// <summary>The column <paramref name="source"/> on the item <paramref name="itemId"/>, at <paramref name="sortOrder"/>.</summary>
    public static ItemColumn Of(Column source, Guid itemId, int sortOrder) => new()
    {
        ItemId = itemId,
        Key = source.Key,
        Name = source.Name,
        DataType = source.DataType,
        IsCore = source.IsCore,
        IsRequired = source.IsRequired,
        ShowInReports = source.ShowInReports,
        SortOrder = sortOrder,
        Options = [.. source.Options],
    };

    /// <summary>Deletes the column by hiding it, whether or not it holds values yet, so no value can be lost (BR-21). A core column is never deleted (BR-20).</summary>
    public StructureChangeOutcome Hide()
    {
        if (IsCore)
        {
            return StructureChangeOutcome.CoreColumn;
        }
        IsHidden = true;
        return StructureChangeOutcome.Done;
    }
}
