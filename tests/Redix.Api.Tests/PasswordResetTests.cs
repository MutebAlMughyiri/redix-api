using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Redix.Api.Auth;
using Redix.Api.History;
using Redix.Api.Translations;

namespace Redix.Api.Tests;

/// <summary>«نسيت كلمة المرور؟» on the sign-in screen: a code by email, then the code and a new password (redix-api #127).</summary>
[Collection(SecondDatabaseCollection.Name)]
public class PasswordResetTests(MigratedApiFactory factory)
{
    private const string Password = "Forgot-pass-1";
    private const string NewPassword = "Remembered-2";

    private static string WrongCodeFor(string code) => code == "000000" ? "111111" : "000000";

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private Task<HttpResponseMessage> AskAsync(string login) => AskAsync(login, factory.CreateClient());

    private static Task<HttpResponseMessage> AskAsync(string login, HttpClient client) => client.PostAsJsonAsync("/auth/forgot-password", new ForgotPasswordRequest(login));

    private Task<HttpResponseMessage> ResetAsync(string login, string code, string newPassword = NewPassword) =>
        factory.CreateClient().PostAsJsonAsync("/auth/reset-with-code", new ResetWithCodeRequest(login, code, newPassword));

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private async Task<Session> SessionOfAsync(string username, string password) =>
        (await (await SignInAsync(username, password)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json))!;

    private Task<HttpResponseMessage> RenewAsync(Session session) => factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(session.RefreshToken));

    /// <summary>Someone whose email is <paramref name="email"/>, who has asked for a code by username; the code the email carried.</summary>
    private async Task<(User User, string Code)> AskedAsync(string email)
    {
        var user = await factory.AddUserAsync(Password, email: email);
        Assert.Equal(HttpStatusCode.Accepted, (await AskAsync(user.Username)).StatusCode);
        return (user, await factory.Emails.LastCodeToAsync(email));
    }

    [Fact]
    public async Task AC_02_6_ACodeToTheAccountsEmailAndANewPasswordResetItAndTheOldOneNoLongerSignsIn()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        Assert.Matches(@"^\d{6}$", code);
        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(user.Username, code)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, NewPassword)).StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
    }

    [Fact]
    public async Task AC_02_6_D5_TheEmailNamesTheAccountAsTheUsernameDoesWhateverItsCase()
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: email);

        await AskAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(email, await factory.Emails.LastCodeToAsync(email))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, NewPassword)).StatusCode);
    }

    [Fact]
    public async Task D3_TriesSentAtOnceCheckNoMoreGuessesThanACodeAllows()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        var answers = await Task.WhenAll(Enumerable.Range(0, EmailCode.TriesAllowed * 2).Select(async _ => await CodeOf(await ResetAsync(user.Username, code, "weak"))));

        Assert.InRange(answers.Count(answer => answer == "weakPassword"), 1, EmailCode.TriesAllowed);
        Assert.All(answers, answer => Assert.Contains(answer, new[] { "weakPassword", "codeWrong" }));
    }

    [Fact]
    public async Task AC_02_6_AWrongCodeResetsNothing()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, WrongCodeFor(code))));

        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task R10_D6_TheAdminResetsTheSameWay()
    {
        var email = MigratedApiFactory.NewEmail();
        var admin = await factory.AddUserAsync(Password, isAdmin: true, email: email);

        await AskAsync(admin.Username);

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(admin.Username, await factory.Emails.LastCodeToAsync(email))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(admin.Username, NewPassword)).StatusCode);
    }

    [Fact]
    public async Task AC_02_7_R5_AnUnknownAccountOrOneWithNoEmailGetsTheSameAnswerAsAKnownOneAndNothingIsSent()
    {
        var email = MigratedApiFactory.NewEmail();
        var known = await factory.AddUserAsync(Password, email: email);
        var withoutEmail = await factory.AddUserAsync(Password);
        var stranger = MigratedApiFactory.NewEmail();

        var answers = new[] { await AskAsync(known.Username), await AskAsync($"nobody-{Guid.CreateVersion7():N}"), await AskAsync(stranger), await AskAsync(withoutEmail.Username) };

        Assert.All(answers, answer => Assert.Equal(HttpStatusCode.Accepted, answer.StatusCode));
        Assert.Single((await Task.WhenAll(answers.Select(answer => answer.Content.ReadAsStringAsync()))).Distinct());
        Assert.Single(await factory.Emails.ToAsync(email));
        Assert.Empty(factory.Emails.To(stranger));
        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(withoutEmail.Username, "123456")));
        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(stranger, "123456")));
    }

    [Fact]
    public async Task AC_02_7_R5_TheAnswerLeavesBeforeTheCodeIsSentSoHowLongItTakesSaysNothing()
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: email);

        using (factory.Emails.Hold())
        {
            var answer = await AskAsync(user.Username).WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(HttpStatusCode.Accepted, answer.StatusCode);
            Assert.Empty(factory.Emails.To(email));
        }

        Assert.Single(await factory.Emails.ToAsync(email));
    }

    [Fact]
    public async Task AC_02_8_ACodeWorksOnce()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(user.Username, code)).StatusCode);

        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, code, "Another-pass-3")));
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, NewPassword)).StatusCode);
    }

    [Fact]
    public async Task AC_02_8_ACodeWorksWithin15MinutesAndNotAfter()
    {
        var (late, lateCode) = await AskedAsync(MigratedApiFactory.NewEmail());
        var (onTime, onTimeCode) = await AskedAsync(MigratedApiFactory.NewEmail());

        factory.Time.Advance(EmailCode.Lifetime - TimeSpan.FromSeconds(1));
        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(onTime.Username, onTimeCode)).StatusCode);
        factory.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(late.Username, lateCode)));
    }

    [Fact]
    public async Task AC_02_8_FiveWrongCodesEndItSoTheRightOneIsRefusedAfterThemButNotWithATryLeft()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());
        var (other, otherCode) = await AskedAsync(MigratedApiFactory.NewEmail());
        for (var attempt = 1; attempt < EmailCode.TriesAllowed; attempt++)
        {
            Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, WrongCodeFor(code))));
            Assert.Equal("codeWrong", await CodeOf(await ResetAsync(other.Username, WrongCodeFor(otherCode))));
        }

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(other.Username, otherCode)).StatusCode);
        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, WrongCodeFor(code))));
        Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, code)));
    }

    [Fact]
    public async Task AC_02_8_D3_ANewCodeEndsTheEarlierOne()
    {
        var email = MigratedApiFactory.NewEmail();
        var (user, earlier) = await AskedAsync(email);

        await AskAsync(user.Username);
        var later = await factory.Emails.LastCodeToAsync(email, 2);

        Assert.Equal(2, factory.Emails.To(email).Count);
        if (earlier != later)
        {
            Assert.Equal("codeWrong", await CodeOf(await ResetAsync(user.Username, earlier)));
        }
        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(user.Username, later)).StatusCode);
        await factory.WithDatabaseAsync(async db => Assert.Single(await db.EmailCodes.Where(code => code.UserId == user.Id).ToListAsync()));
    }

    [Fact]
    public async Task AC_02_8_D4_AskingTooOftenIsRefusedForAWhileAndAskingWithinTheLimitIsNot()
    {
        using var limited = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["EmailCodes:PermitsPerHour"] = "2" })));
        var client = limited.CreateClient();

        var first = await AskAsync(MigratedApiFactory.NewEmail(), client);
        var second = await client.PostAsJsonAsync("/auth/reset-with-code", new ResetWithCodeRequest(MigratedApiFactory.NewEmail(), "123456", NewPassword));
        var third = await AskAsync(MigratedApiFactory.NewEmail(), client);

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }

    [Fact]
    public async Task AC_02_8_D4_OneLoginTypedTooOftenIsRefusedWhateverItsCaseAndAnotherLoginIsNot()
    {
        var login = $"nobody-{Guid.CreateVersion7():N}";
        for (var ask = 0; ask < EmailCode.AsksPerLoginAnHour; ask++)
        {
            Assert.Equal(HttpStatusCode.Accepted, (await AskAsync(ask % 2 == 0 ? login : $" {login.ToUpperInvariant()} ")).StatusCode);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await AskAsync(login)).StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, (await AskAsync($"{login}-other")).StatusCode);
    }

    [Fact]
    public async Task AC_02_8_D4_R5_AnAccountsLoginIsLimitedAsAnUnknownOneIsAndTheRefusedAskSendsNothing()
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: email);
        for (var ask = 0; ask < EmailCode.AsksPerLoginAnHour; ask++)
        {
            await AskAsync(user.Username);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await AskAsync(user.Username)).StatusCode);
        Assert.Equal(EmailCode.AsksPerLoginAnHour, (await factory.Emails.ToAsync(email, EmailCode.AsksPerLoginAnHour)).Count);
    }

    [Fact]
    public async Task AC_02_9_R7_AResetEndsEverySessionUnlocksTheAccountAndAsksForNoOtherChange()
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, mustChangePassword: true, email: email);
        var session = await SessionOfAsync(user.Username, Password);
        for (var attempt = 0; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }
        Assert.Equal("accountLocked", await CodeOf(await SignInAsync(user.Username, Password)));
        await AskAsync(user.Username);

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(user.Username, await factory.Emails.LastCodeToAsync(email))).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await RenewAsync(session)).StatusCode);
        Assert.False((await SessionOfAsync(user.Username, NewPassword)).MustChangePassword);
    }

    [Fact]
    public async Task AC_02_9_AWrongCodeEndsNoSession()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());
        var session = await SessionOfAsync(user.Username, Password);

        await ResetAsync(user.Username, WrongCodeFor(code));

        Assert.Equal(HttpStatusCode.OK, (await RenewAsync(session)).StatusCode);
    }

    [Fact]
    public async Task AWeakNewPasswordIsRefusedAndTheCodeStaysUnspent()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(user.Username, code, "short")));
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(user.Username, code, user.Username)));

        Assert.Equal(HttpStatusCode.NoContent, (await ResetAsync(user.Username, code)).StatusCode);
    }

    [Theory]
    [InlineData("en", "Your password reset code")]
    [InlineData("ar", "")]
    [InlineData("fr", "")]
    public async Task AC_02_10_R8_TheEmailIsInTheLanguageThePhoneAskedInAndArabicForAnyOther(string language, string englishSubject)
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: email);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);

        await AskAsync(user.Username, client);

        Assert.Equal(englishSubject.Length > 0 ? englishSubject : TranslationSeed.Arabic("email.resetSubject"), (await factory.Emails.ToAsync(email)).Single().Subject);
    }

    [Fact]
    public async Task AResetIsInTheHistoryByThePersonWithNoPasswordHash()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        await ResetAsync(user.Username, code);

        var record = (await factory.HistoryOfAsync(user.Id)).Last();
        Assert.Equal((HistoryAction.ResetPassword, user.Id), (record.Action, record.UserId));
        Assert.DoesNotContain("PasswordHash", record.Before + record.After);
    }

    [Fact]
    public async Task R127_OnlyTheCodesHashIsKept()
    {
        var (user, code) = await AskedAsync(MigratedApiFactory.NewEmail());

        await factory.WithDatabaseAsync(async db =>
        {
            var kept = await db.EmailCodes.SingleAsync(row => row.UserId == user.Id);
            Assert.DoesNotContain(code, kept.CodeHash);
            Assert.Equal((EmailCodePurpose.ResetPassword, 0, EmailCode.NotUsed), (kept.Purpose, kept.Tries, kept.UsedAt));
        });
    }
}
