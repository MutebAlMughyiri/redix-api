namespace Redix.Api.Partners;

/// <summary>
/// A partner's own login (findings row 214): whose account the partner is, shown beside their name. A partner has one at most and having no row is having none; a login is one kept
/// partner's in a project (UC-016), so a deleted partner keeps none. It gives no sight of capital (redix-api #106).
/// </summary>
public class PartnerUser : IProjectRow
{
    /// <summary>The partner.</summary>
    public Guid PartnerId { get; init; }

    /// <summary>The partner's project, in which the login is one partner's.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The partner's own login.</summary>
    public Guid UserId { get; set; }
}
