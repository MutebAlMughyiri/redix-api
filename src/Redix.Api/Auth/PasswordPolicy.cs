namespace Redix.Api.Auth;

/// <summary>
/// The one password policy (redix-api #113, Muteb, 12 Nov 2026): at least <see cref="MinimumLength"/> characters, with a letter and a digit, and not the user name. Every
/// password a person sets is checked here: at registration, for a new user, at a reset, and at a change, which must also differ from the current one. Its words, which the
/// app shows under each password field, are <c>common.passwordRules</c> and each screen's <c>weakPassword</c>. Nothing else is asked: no history, no expiry, the same in every
/// project, and a password set before keeps working until it is next changed.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>The shortest password accepted.</summary>
    public const int MinimumLength = 8;

    /// <summary>Whether <paramref name="password"/> may be set for the user <paramref name="username"/>: long enough, with a letter and a digit, and not the user name in any case.</summary>
    public static bool Holds(string password, string username) =>
        password.Length >= MinimumLength && password.Any(char.IsLetter) && password.Any(char.IsDigit) && !string.Equals(password, username.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether <paramref name="newPassword"/> may replace <paramref name="currentPassword"/> for <paramref name="username"/>: it holds, and it is different.</summary>
    public static bool MayReplace(string newPassword, string currentPassword, string username) => Holds(newPassword, username) && newPassword != currentPassword;
}
