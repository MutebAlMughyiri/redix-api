namespace Redix.Api.Entries;

/// <summary>What a movement of money in the ledger is. Each kind's own fields live in a detail table keyed by the entry.</summary>
public enum EntryKind : byte
{
    /// <summary>A purchase from a supplier (FR-08).</summary>
    Purchase,

    /// <summary>A payment to a contractor (FR-12).</summary>
    ContractorPayment,

    /// <summary>Capital coming in from a partner.</summary>
    CapitalIn,

    /// <summary>Capital going out to a partner.</summary>
    CapitalOut,

    /// <summary>Revenue, such as a sale or a VAT refund.</summary>
    Revenue,
}

/// <summary>Where an entry stands. Only approved entries count in any total (BR-10).</summary>
public enum EntryStatus : byte
{
    /// <summary>Saved by who entered it, and seen by nobody's inbox.</summary>
    Draft,

    /// <summary>Sent, and waiting for Finance.</summary>
    PendingApproval,

    /// <summary>Approved: it counts.</summary>
    Approved,

    /// <summary>Refused, saying why.</summary>
    Rejected,
}

/// <summary>What a file is to the entry that carries it.</summary>
public enum EntryAttachmentRole : byte
{
    /// <summary>The invoice (FR-09).</summary>
    Invoice,

    /// <summary>A receipt voucher.</summary>
    Receipt,

    /// <summary>A bank transfer receipt.</summary>
    BankReceipt,

    /// <summary>Anything else.</summary>
    Other,
}

/// <summary>One step of an entry through approval.</summary>
public enum EntryDecisionAction : byte
{
    /// <summary>Sent for approval.</summary>
    Submit,

    /// <summary>Approved: it counts.</summary>
    Approve,

    /// <summary>Refused, saying why.</summary>
    Reject,
}

/// <summary>What ties two entries. Each keeps the number it is stored as, since the transfer's pair (0) and the carry's (1) were removed (Muteb, 8 Nov 2026).</summary>
public enum EntryLinkType : byte
{
    /// <summary>The entry that reverses an approved one (FR-50).</summary>
    Reversal = 2,

    /// <summary>The credit note of an invoice (BR-18).</summary>
    CreditNoteOf = 3,
}
