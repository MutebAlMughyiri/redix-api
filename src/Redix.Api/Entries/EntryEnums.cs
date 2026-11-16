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

/// <summary>What an entry's kind means for money.</summary>
public static class EntryKinds
{
    /// <summary><paramref name="amount"/> as <paramref name="kind"/> moves money: as it is for capital coming in and revenue, negated for what goes out. A database query cannot call it.</summary>
    public static decimal Signed(this EntryKind kind, decimal amount) => kind is EntryKind.CapitalIn or EntryKind.Revenue ? amount : -amount;

    /// <summary>
    /// What a file is to an entry of <paramref name="kind"/>: a purchase's invoice, a contractor payment's receipt voucher, a partner's bank transfer receipt, and anything else for
    /// revenue. Asked when the entry is kept and when a file is put on it later, whether or not its table is still offered (redix-app #115 D5).
    /// </summary>
    public static EntryAttachmentRole FileRole(this EntryKind kind) => kind switch
    {
        EntryKind.Purchase => EntryAttachmentRole.Invoice,
        EntryKind.ContractorPayment => EntryAttachmentRole.Receipt,
        EntryKind.CapitalIn or EntryKind.CapitalOut => EntryAttachmentRole.BankReceipt,
        _ => EntryAttachmentRole.Other,
    };
}

/// <summary>Where an entry stands. Only approved entries count in any total (BR-10).</summary>
public enum EntryStatus : byte
{
    /// <summary>Saved by who entered it, and seen and handled by nobody else until it is sent (BR-42).</summary>
    Draft,

    /// <summary>Sent, and waiting for Finance.</summary>
    PendingApproval,

    /// <summary>Approved: it counts.</summary>
    Approved,

    /// <summary>Refused, saying why.</summary>
    Rejected,

    /// <summary>Removed after approval by whoever holds <see cref="Auth.Permission.RemoveApprovedEntries"/> (redix-api #119): kept in the database with its history, and read by no list and no figure.</summary>
    Removed,
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

