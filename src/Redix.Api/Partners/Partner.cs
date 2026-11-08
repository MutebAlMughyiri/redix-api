namespace Redix.Api.Partners;

/// <summary>A partner in one project (FR-25), whose capital is reckoned from their movements (BR-05). Someone who withdrew stays, with a zero balance and their history.</summary>
public class Partner
{
    /// <summary>The partner's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project they are a partner in (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The login whose capital this is, so that person sees it as their own (BR-31); <see cref="Guid.Empty"/> for a partner with no login.</summary>
    public Guid UserId { get; set; }

    /// <summary>The partner's name, as it was typed.</summary>
    public required string Name { get; set; }
}
