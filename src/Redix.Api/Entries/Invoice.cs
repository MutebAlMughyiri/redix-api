namespace Redix.Api.Entries;

/// <summary>The number of a bill entered before REDIX stopped asking for one (redix-api #96, D2): the only part of it kept, since no VAT is stored (D4).</summary>
public class Invoice
{
    /// <summary>The longest invoice number.</summary>
    public const int NumberMaxLength = 50;

    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The invoice's number.</summary>
    public string Number { get; init; } = "";
}
