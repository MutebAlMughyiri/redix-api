namespace Redix.Api.Entries;

/// <summary>
/// What the app sends for any entry, whatever its table (redix-api #110): the table it goes into, its date, its amount, its account, its notes, its file when there is one, the
/// values of its item's columns, and whether it is sent for approval or kept as a draft. What only one kind of table asks is here too, and every other kind leaves it out: a
/// purchase's supplier, and a contractor payment's place inside or outside its contract (FR-13). Whether a purchase is a credit note and whether a partner's money is a withdrawal
/// are the choices of their table's lists, «نوع القيد» and «نوع الحركة», among the column values (redix-api #129).
/// </summary>
/// <param name="Id">The entry's id, made on the phone, so a second post of it answers the first (AC-16.3).</param>
public record EntryRequest(Guid Id)
{
    /// <summary>The item whose table it goes into.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The table: one of the item's groups, a contract or a partner.</summary>
    public Guid TableId { get; init; }

    /// <summary>The entry's one date: the day the money moved (redix-api #96, D10).</summary>
    public DateOnly Date { get; init; }

    /// <summary>What it comes to. Negative for a deduction on an expense (FR-14); a purchase may leave it out for its unit price times its quantity, when its table has both columns.</summary>
    public decimal Amount { get; init; }

    /// <summary>The account the money moved through.</summary>
    public Guid AccountId { get; init; }

    /// <summary>How an expense was paid; revenue and a partner's money take none.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>Notes on it; left out when none. A deduction says why here (D14).</summary>
    public string Notes { get; init; } = "";

    /// <summary>The values of the item's columns the entry holds in no field of its own, by each column's key (FR-41): a person's own, the unit price, the quantity, the product (redix-api #129, R5).</summary>
    public IReadOnlyDictionary<string, string> ColumnValues { get; init; } = new Dictionary<string, string>();

    /// <summary>The file, already kept; left out when there is none, which every entry may be (D3, D16).</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>True to send it for approval; otherwise it is saved as a draft.</summary>
    public bool Submit { get; init; }

    /// <summary>A purchase's supplier, from the project's register; left out when it names none, which a purchase may (Muteb, 13 Nov 2026, redix-api #129).</summary>
    public Guid SupplierId { get; init; }

    /// <summary>True for a contractor payment outside the contract, which counts in the item's total and leaves the remaining alone (FR-13).</summary>
    public bool IsOutsideContract { get; init; }

    /// <summary>The column values that hold something, trimmed.</summary>
    public Dictionary<string, string> ColumnValuesKept() => ColumnValues.Where(value => value.Value.Trim().Length > 0).ToDictionary(value => value.Key, value => value.Value.Trim());

    /// <summary>What it answers of an item's columns (FR-41).</summary>
    public EntryAnswers Answers() => new(Notes.Trim(), ColumnValuesKept());
}
