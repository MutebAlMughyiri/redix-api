using Redix.Api.Auth;

namespace Redix.Api.Tests;

/// <summary>redix-api #113 (Muteb, 12 Nov 2026): at least 8 characters, with a letter and a digit, and not the user name.</summary>
public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Ghosoon-6", "muteb")]
    [InlineData("كلمة-سر-2026", "muteb")]
    [InlineData("a1234567", "muteb")]
    public void R113_APasswordOfEightWithALetterAndADigitThatIsNotTheUserNameHolds(string password, string username) =>
        Assert.True(PasswordPolicy.Holds(password, username));

    [Theory]
    [InlineData("Short-1", "muteb")]
    [InlineData("only-letters", "muteb")]
    [InlineData("1234567890", "muteb")]
    [InlineData("Muteb-2026", "muteb-2026")]
    [InlineData("muteb-2026", " MUTEB-2026 ")]
    public void R113_AShortPasswordOneWithoutALetterOrADigitOrTheUserNameInAnyCaseIsRefused(string password, string username) =>
        Assert.False(PasswordPolicy.Holds(password, username));

    [Fact]
    public void R113_AChangeMustAlsoDifferFromTheCurrentPassword()
    {
        Assert.False(PasswordPolicy.MayReplace("Ghosoon-6", "Ghosoon-6", "muteb"));
        Assert.True(PasswordPolicy.MayReplace("Ghosoon-7", "Ghosoon-6", "muteb"));
    }
}
