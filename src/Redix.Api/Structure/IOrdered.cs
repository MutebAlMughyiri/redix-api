namespace Redix.Api.Structure;

/// <summary>A row with a place in a list its project orders, which is the order of the periodic report (AC-39.1).</summary>
public interface IOrdered
{
    /// <summary>The row's id.</summary>
    Guid Id { get; }

    /// <summary>The row's place in its list.</summary>
    int SortOrder { get; set; }
}
