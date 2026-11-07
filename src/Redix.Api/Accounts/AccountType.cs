namespace Redix.Api.Accounts;

/// <summary>What kind of account holds a project's money (FR-22).</summary>
public enum AccountType : byte
{
    /// <summary>A bank account (حساب بنكي).</summary>
    Bank,

    /// <summary>Cash held by one person (عهدة نقدية).</summary>
    CashCustody,
}
