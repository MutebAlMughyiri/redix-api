namespace Redix.Api.Entries;

/// <summary>
/// What the app sends for any entry, whatever its table (redix-api #110): the table it goes into, its date, its amount, its account, its notes, its file when there is one, and
/// whether it is sent for approval or kept as a draft. What only one kind of table asks is here too, and every other kind leaves it out: a purchase's supplier, product, price,
/// quantity and the bill a credit note returns (D13), a contractor payment's place inside or outside its contract (FR-13), and a partner's withdrawal (BR-06).
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

    /// <summary>What it comes to. Negative for a deduction on an expense (FR-14); a purchase may leave it out for its unit price times its quantity.</summary>
    public decimal Amount { get; init; }

    /// <summary>The account the money moved through.</summary>
    public Guid AccountId { get; init; }

    /// <summary>How an expense was paid; revenue and a partner's money take none.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>Notes on it; left out when none. A deduction says why here (D14).</summary>
    public string Notes { get; init; } = "";

    /// <summary>The values of the item's custom columns, by each column's key (FR-41).</summary>
    public Dictionary<string, string> CustomValues { get; init; } = [];

    /// <summary>The file, already kept; left out when there is none, which every entry may be (D3, D16).</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>True to send it for approval; otherwise it is saved as a draft.</summary>
    public bool Submit { get; init; }

    /// <summary>A purchase's supplier, from the project's register.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>What a purchase bought; left out when not said.</summary>
    public string Product { get; init; } = "";

    /// <summary>The price of one; left out when not used.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>How many; left out when not used.</summary>
    public decimal Quantity { get; init; }

    /// <summary>True for a credit note, a مرتجع: a total typed as a plain amount and kept negative, its own row as in Muteb's statement, naming no bill (redix-api #119, D4).</summary>
    public bool IsCreditNote { get; init; }

    /// <summary>True for a contractor payment outside the contract, which counts in the item's total and leaves the remaining alone (FR-13).</summary>
    public bool IsOutsideContract { get; init; }

    /// <summary>True for a partner taking money out of their capital; otherwise they put it in (BR-06).</summary>
    public bool IsWithdrawal { get; init; }

    /// <summary>The custom values that hold something, trimmed.</summary>
    public Dictionary<string, string> CustomValuesKept() => CustomValues.Where(value => value.Value.Trim().Length > 0).ToDictionary(value => value.Key, value => value.Value.Trim());

    /// <summary>What it answers of an item's columns (FR-41), as an entry of <paramref name="kind"/>.</summary>
    public EntryAnswers AnswersAs(EntryKind kind) => new(kind, Product.Trim(), UnitPrice, Quantity, Notes.Trim(), CustomValuesKept());
}
