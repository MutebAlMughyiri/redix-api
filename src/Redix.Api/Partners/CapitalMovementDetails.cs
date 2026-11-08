namespace Redix.Api.Partners;

/// <summary>What a capital movement is (FR-25): money a partner put in or took out. Settlement, carry, in kind and refundable advance were removed with حركة (Muteb, 8 Nov 2026, redix-api #101).</summary>
public enum CapitalMovementType : byte
{
    /// <summary>A partner put money in.</summary>
    Contribution,

    /// <summary>A partner took money out.</summary>
    Withdrawal,
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
    public static bool IsInflow(CapitalMovementType type) => type == CapitalMovementType.Contribution;
}
