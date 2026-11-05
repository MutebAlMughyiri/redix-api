namespace Redix.Api.Structure;

/// <summary>What a column holds (FR-41). A person chooses among the first five; <see cref="File"/> is the attachment's alone.</summary>
public enum ColumnDataType : byte
{
    /// <summary>Free text.</summary>
    Text,

    /// <summary>A number, such as an area.</summary>
    Number,

    /// <summary>An amount of money.</summary>
    Amount,

    /// <summary>A date.</summary>
    Date,

    /// <summary>A choice from a list: its own, or a register such as the suppliers.</summary>
    List,

    /// <summary>A picture or a PDF.</summary>
    File,
}
