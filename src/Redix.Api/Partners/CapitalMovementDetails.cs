namespace Redix.Api.Partners;

/// <summary>
/// What only a capital movement has (FR-25): its partner. Whether it put money in or took it out is its kind, which the choice of «نوع الحركة» decides (redix-api #129); the
/// movement type it once kept beside the kind went with that list.
/// </summary>
public class CapitalMovementDetails : IProjectRow
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The partner.</summary>
    public Guid PartnerId { get; init; }
}
