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

    /// <summary>The side of a transfer that leaves an account.</summary>
    TransferOut,

    /// <summary>The side of a transfer that reaches an account.</summary>
    TransferIn,
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

/// <summary>What ties two entries.</summary>
public enum EntryLinkType : byte
{
    /// <summary>The two sides of a transfer between accounts.</summary>
    TransferPair,

    /// <summary>The two sides of a partner's balance carried between projects.</summary>
    CarryPair,

    /// <summary>The entry that reverses an approved one (FR-50).</summary>
    Reversal,

    /// <summary>The credit note of an invoice (BR-18).</summary>
    CreditNoteOf,
}

/// <summary>What becomes of an expense paid from a partner's money (BR-25); nothing until UC-045 "Expense paid from a partner's money".</summary>
public enum PartnerTreatment : byte
{
    /// <summary>The expense was not paid from a partner's money.</summary>
    Empty,

    /// <summary>It adds to the partner's capital.</summary>
    Capital,

    /// <summary>It is owed back to the partner.</summary>
    AmountDue,
}
