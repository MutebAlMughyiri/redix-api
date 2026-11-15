namespace Redix.Api.Auth;

/// <summary>What an <see cref="EmailCode"/> proves the person may do once it is entered (redix-api #127, the rule).</summary>
public enum EmailCodePurpose : byte
{
    /// <summary>Reset a forgotten password from the sign-in screen: the code goes to the account's email (AC-02.6).</summary>
    ResetPassword,

    /// <summary>Make a new address the person's email, in Settings: the code goes to the new address (AC-10.4).</summary>
    ConfirmEmail,
}
