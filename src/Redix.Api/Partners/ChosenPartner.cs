using Redix.Api.Auth;

namespace Redix.Api.Partners;

/// <summary>One partner whose capital one person sees in one project, ticked on their permissions while they hold <see cref="Auth.Permission.SeeChosenCapital"/> (BR-31, redix-api #106).</summary>
public class ChosenPartner : IProjectRow, IAccessRow
{
    /// <summary>The row's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The person who sees the partner.</summary>
    public Guid UserId { get; init; }

    /// <summary>The partner they see.</summary>
    public Guid PartnerId { get; init; }
}
