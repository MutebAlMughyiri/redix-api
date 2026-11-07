using Redix.Api.Projects;

namespace Redix.Api.Entries;

/// <summary>What the app sends for any expense, a purchase or a contractor payment: how it was paid, its file, and what its item's columns ask. It sends what was typed; every figure is computed here, never sent.</summary>
/// <param name="Id">The entry's id, made on the phone.</param>
public abstract record EntryRequest(Guid Id)
{
    /// <summary>The invoice's number; left out when there is no invoice, or the invoice shows none.</summary>
    public string InvoiceNumber { get; init; } = "";

    /// <summary>The invoice's date; left out when there is no invoice.</summary>
    public DateOnly InvoiceDate { get; init; }

    /// <summary>Why there is neither an invoice nor a receipt; left out when there is one (FR-09).</summary>
    public string WithoutInvoiceReason { get; init; } = "";

    /// <summary>How it was paid.</summary>
    public Guid PaymentMethodId { get; init; }

    /// <summary>The account it was paid from.</summary>
    public Guid AccountId { get; init; }

    /// <summary>The day it was paid: a contractor payment's date, and a purchase's when it has no invoice. A purchase with an invoice is paid on the invoice's date (redix-api #96, D10).</summary>
    public DateOnly PaymentDate { get; init; }

    /// <summary>Notes on it; left out when none.</summary>
    public string Notes { get; init; } = "";

    /// <summary>The values of the item's custom columns, by each column's key (FR-41).</summary>
    public Dictionary<string, string> CustomValues { get; init; } = [];

    /// <summary>The file, already kept; left out when there is none yet.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>True to send it for approval; otherwise it is saved as a draft.</summary>
    public bool Submit { get; init; }

    /// <summary>True once the person has seen that this supplier's invoice number exists already, and saves it anyway (FR-17).</summary>
    public bool ConfirmDuplicate { get; init; }

    /// <summary>Whether this says why it has no invoice or receipt (FR-09).</summary>
    public bool IsWithoutInvoice() => WithoutInvoiceReason.Trim().Length > 0;

    /// <summary>The custom values to keep: each trimmed, and the empty ones gone.</summary>
    public Dictionary<string, string> CustomValuesKept() => CustomValues.Where(value => value.Value.Trim().Length > 0).ToDictionary(value => value.Key, value => value.Value.Trim());

    /// <summary>What this holds that an item's columns can require.</summary>
    public abstract EntryAnswers Answers();

    /// <summary>Why what every expense has cannot be kept: a reason too long or beside a file, or notes too long. Empty when it can.</summary>
    protected string RefusalOfCommon()
    {
        if (WithoutInvoiceReason.Trim().Length > Project.ReasonMaxLength)
        {
            return "InvalidWithoutInvoiceReason";
        }
        if (IsWithoutInvoice() && AttachmentId != Guid.Empty)
        {
            return "InvoiceAndReason";
        }
        return Notes.Trim().Length > Entry.NotesMaxLength ? "InvalidNotes" : "";
    }
}
