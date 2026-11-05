namespace Redix.Api.Contracts;

/// <summary>What a contractor was promised in one project, for an item and a unit (FR-07), to follow what is paid and what remains (BR-04). It is disabled rather than deleted, and owns the rule of its remaining.</summary>
public class Contract
{
    /// <summary>The longest notes a contract may have.</summary>
    public const int NotesMaxLength = 1000;

    /// <summary>The contract's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project it belongs to (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The item it is for; it changes only while no payment names the contract.</summary>
    public Guid ItemId { get; set; }

    /// <summary>The unit it is for, which may be the shared one; it changes only while no payment names the contract.</summary>
    public Guid UnitId { get; set; }

    /// <summary>The contractor, who is a supplier of the project; it changes only while no payment names the contract.</summary>
    public Guid SupplierId { get; set; }

    /// <summary>What was promised; 0 for an open-value contract.</summary>
    public decimal Value { get; private set; }

    /// <summary>True for a contract whose payments only add up, with no remaining and no alert (AC-07.2).</summary>
    public bool IsOpenValue { get; private set; }

    /// <summary>The price of one unit of work, such as 380 per metre; 0 when not used.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>How many units of work; 0 when not used.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Notes on the contract and its amendments; empty when none.</summary>
    public string Notes { get; private set; } = "";

    /// <summary>False once disabled: no new payment can name it, and it stays in the reports.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The value less <paramref name="paid"/>, below zero once overrun; 0 for an open-value contract, which has no remaining (BR-04).</summary>
    public decimal RemainingAfter(decimal paid) => IsOpenValue ? 0 : Value - paid;

    /// <summary>Whether <paramref name="paid"/> goes beyond the value, which shows an alert; never for an open-value contract (BR-04).</summary>
    public bool IsOverrunBy(decimal paid) => !IsOpenValue && paid > Value;

    /// <summary>Why a contract with these terms, the notes already trimmed, cannot be kept: a value not above zero unless <paramref name="isOpenValue"/>, a unit price or a quantity below zero, an amount beyond <see cref="Money.Largest"/> or with more than two decimals, or notes too long. Empty when it can.</summary>
    public static string RefusalOf(decimal value, bool isOpenValue, decimal unitPrice, decimal quantity, string notes)
    {
        if (!isOpenValue && (value <= 0 || !Money.IsAcceptable(value)))
        {
            return "InvalidValue";
        }
        if (unitPrice < 0 || !Money.IsAcceptable(unitPrice))
        {
            return "InvalidUnitPrice";
        }
        if (quantity < 0 || !Money.IsAcceptable(quantity))
        {
            return "InvalidQuantity";
        }
        return notes.Length > NotesMaxLength ? "InvalidNotes" : "";
    }

    /// <summary>Sets what can change on a contract, which is also how an amendment is made: an open-value contract keeps no value.</summary>
    public void SetTerms(decimal value, bool isOpenValue, decimal unitPrice, decimal quantity, string notes)
    {
        Value = isOpenValue ? 0 : value;
        IsOpenValue = isOpenValue;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Notes = notes;
    }
}
