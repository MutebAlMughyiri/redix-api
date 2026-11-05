namespace Redix.Api.Partners;

/// <summary>What a capital movement is (FR-25).</summary>
public enum CapitalMovementType : byte
{
    /// <summary>A partner put money in.</summary>
    Contribution,

    /// <summary>A partner took money out.</summary>
    Withdrawal,

    /// <summary>A balance arrived from another project (FR-26).</summary>
    CarryIn,

    /// <summary>A balance left for another project (FR-26).</summary>
    CarryOut,

    /// <summary>A partner took all they had and withdrew.</summary>
    SettlementAndExit,

    /// <summary>A partner gave something other than money, saying what.</summary>
    InKind,

    /// <summary>A partner took money that comes back to the account (BRS open question 8), reported as its own line.</summary>
    RefundableAdvance,

    /// <summary>A partner's part of the final surplus when the project closed, paid out or carried, or of the deficit, paid in (BR-27): counted as the liquidation, not as capital.</summary>
    Liquidation,
}

/// <summary>What only a capital movement has (FR-25).</summary>
public class CapitalMovementDetails
{
    /// <summary>The longest cash reason.</summary>
    public const int ReasonMaxLength = 500;

    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The partner.</summary>
    public Guid PartnerId { get; init; }

    /// <summary>What it is.</summary>
    public CapitalMovementType Type { get; init; }

    /// <summary>What was said about it.</summary>
    public string Description { get; init; } = "";

    /// <summary>Why it has no bank receipt (BR-23); empty when it has one.</summary>
    public string CashReason { get; init; } = "";

    /// <summary>Whether it adds to the partner's capital.</summary>
    public static bool IsInflow(CapitalMovementType type) => type is CapitalMovementType.Contribution or CapitalMovementType.CarryIn or CapitalMovementType.InKind;
}

/// <summary>The account that holds what a partner paid from their own pocket (BR-25). It is created with the partner, so every list of accounts offers it unasked.</summary>
public class PartnerPocket
{
    /// <summary>The account.</summary>
    public Guid AccountId { get; init; }

    /// <summary>The partner.</summary>
    public Guid PartnerId { get; init; }
}
