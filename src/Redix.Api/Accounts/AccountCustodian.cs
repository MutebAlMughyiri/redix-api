namespace Redix.Api.Accounts;

/// <summary>Who holds an account (صاحب العهدة), and so may spend from it without being offered every account. An account has one at most, and having no row is having none.</summary>
public class AccountCustodian
{
    /// <summary>The account.</summary>
    public Guid AccountId { get; init; }

    /// <summary>The user who holds it.</summary>
    public Guid UserId { get; set; }
}
