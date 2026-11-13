namespace Redix.Api.Auth;

/// <summary>What a sign-in attempt led to. A refusal's name, in camel case, is the <c>code</c> the app translates.</summary>
public enum SignInOutcome
{
    /// <summary>The password was right; a session opens.</summary>
    Success,

    /// <summary>A wrong username or a wrong password, never told apart (AC-01.2).</summary>
    InvalidCredentials,

    /// <summary>Too many wrong passwords in a row; the account is locked for a while (AC-01.2).</summary>
    AccountLocked,
}
