using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Redix.Api.Auth;
using Redix.Api.History;

namespace Redix.Api.Tests;

/// <summary>Settings › «بريدي»: the person changes his own email, and the new address holds once its code is entered (redix-api #127, AC-10.4).</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class MyEmailTests(MigratedApiFactory factory)
{
    private const string Password = "My-email-pass-1";

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static string WrongCodeFor(string code) => code == "000000" ? "111111" : "000000";

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private async Task<Session> SessionOfAsync(User user) =>
        (await (await SignInAsync(user.Username, Password)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json))!;

    private static Task<HttpResponseMessage> AskAsync(HttpClient client, string email) => client.PostAsJsonAsync("/me/email", new MyEmailRequest(email));

    private static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, string code) => client.PostAsJsonAsync("/me/email/confirm", new ConfirmMyEmailRequest(code));

    private Task<HttpResponseMessage> ForgotAsync(string login) => factory.CreateClient().PostAsJsonAsync("/auth/forgot-password", new ForgotPasswordRequest(login));

    /// <summary>Someone signed in whose email is <paramref name="email"/>.</summary>
    private async Task<(User User, HttpClient Client)> SignedInAsync(string email = "", bool isAdmin = false)
    {
        var user = await factory.AddUserAsync(Password, isAdmin: isAdmin, email: email);
        return (user, await factory.ClientSignedInAsAsync(user, Password));
    }

    [Fact]
    public async Task AC_10_4_ANewAddressHoldsOnlyOnceItsCodeIsEnteredAndTheOldOneUntilThen()
    {
        var (old, changed) = (MigratedApiFactory.NewEmail(), MigratedApiFactory.NewEmail());
        var (user, client) = await SignedInAsync(old);

        Assert.Equal(HttpStatusCode.Accepted, (await AskAsync(client, $" {changed} ")).StatusCode);

        Assert.Single(factory.Emails.To(changed));
        Assert.Equal(old, (await SessionOfAsync(user)).Email);
        await ForgotAsync(changed);
        await ForgotAsync(old);
        Assert.Single(await factory.Emails.ToAsync(old));
        Assert.Single(factory.Emails.To(changed));

        Assert.Equal(HttpStatusCode.NoContent, (await ConfirmAsync(client, await factory.Emails.LastCodeToAsync(changed))).StatusCode);

        Assert.Equal(changed, (await SessionOfAsync(user)).Email);
        await ForgotAsync(old);
        await ForgotAsync(changed);
        Assert.Equal(2, (await factory.Emails.ToAsync(changed, 2)).Count);
        Assert.Single(factory.Emails.To(old));
    }

    [Fact]
    public async Task AC_10_4_AWrongCodeLeavesTheOldAddress()
    {
        var old = MigratedApiFactory.NewEmail();
        var changed = MigratedApiFactory.NewEmail();
        var (user, client) = await SignedInAsync(old);
        await AskAsync(client, changed);

        Assert.Equal("codeWrong", await CodeOf(await ConfirmAsync(client, WrongCodeFor(await factory.Emails.LastCodeToAsync(changed)))));

        Assert.Equal(old, (await SessionOfAsync(user)).Email);
    }

    [Fact]
    public async Task AC_10_4_AResetCodeConfirmsNoAddressAndNoCodeConfirmsNothing()
    {
        var old = MigratedApiFactory.NewEmail();
        var (user, client) = await SignedInAsync(old);
        Assert.Equal("codeWrong", await CodeOf(await ConfirmAsync(client, "123456")));
        await ForgotAsync(user.Username);

        Assert.Equal("codeWrong", await CodeOf(await ConfirmAsync(client, await factory.Emails.LastCodeToAsync(old))));

        Assert.Equal(old, (await SessionOfAsync(user)).Email);
    }

    [Fact]
    public async Task R9_AnAddressAnotherAccountHoldsOrOneNotWellFormedIsRefusedAndNothingIsSent()
    {
        var other = MigratedApiFactory.NewEmail();
        await factory.AddUserAsync(Password, email: other);
        var (_, client) = await SignedInAsync();

        var taken = await AskAsync(client, other.ToUpperInvariant());

        Assert.Equal((HttpStatusCode.Conflict, "emailTaken"), (taken.StatusCode, await CodeOf(taken)));
        Assert.Equal("emailInvalid", await CodeOf(await AskAsync(client, "not an email")));
        Assert.Equal("emailInvalid", await CodeOf(await AskAsync(client, "")));
        Assert.Empty(factory.Emails.To(other.ToUpperInvariant()));
    }

    [Fact]
    public async Task R9_AnAddressAnotherAccountTookBeforeItsCodeWasEnteredIsRefused()
    {
        var wanted = MigratedApiFactory.NewEmail();
        var (user, client) = await SignedInAsync();
        await AskAsync(client, wanted);
        await factory.AddUserAsync(Password, email: wanted);

        var refused = await ConfirmAsync(client, await factory.Emails.LastCodeToAsync(wanted));

        Assert.Equal((HttpStatusCode.Conflict, "emailTaken"), (refused.StatusCode, await CodeOf(refused)));
        Assert.Equal("", (await SessionOfAsync(user)).Email);
    }

    [Fact]
    public async Task D6_R10_TheAdminSetsHisEmailInSettingsAndThenResetsWithIt()
    {
        var email = MigratedApiFactory.NewEmail();
        var (admin, client) = await SignedInAsync(isAdmin: true);
        await AskAsync(client, email);
        await ConfirmAsync(client, await factory.Emails.LastCodeToAsync(email));

        await ForgotAsync(email);

        var reset = await factory.CreateClient().PostAsJsonAsync("/auth/reset-with-code", new ResetWithCodeRequest(admin.Username, await factory.Emails.LastCodeToAsync(email, 2), "Admin-new-pass-2"));
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(admin.Username, "Admin-new-pass-2")).StatusCode);
    }

    [Fact]
    public async Task AC_02_10_TheConfirmationIsInTheLanguageThePhoneAskedIn()
    {
        var email = MigratedApiFactory.NewEmail();
        var (_, client) = await SignedInAsync();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");

        await AskAsync(client, email);

        Assert.Equal("Confirm your REDIX email", factory.Emails.To(email).Single().Subject);
    }

    [Fact]
    public async Task AConfirmedAddressIsInTheHistoryByThePersonWithTheOldAndTheNew()
    {
        var (old, changed) = (MigratedApiFactory.NewEmail(), MigratedApiFactory.NewEmail());
        var (user, client) = await SignedInAsync(old);
        await AskAsync(client, changed);

        await ConfirmAsync(client, await factory.Emails.LastCodeToAsync(changed));

        var record = (await factory.HistoryOfAsync(user.Id)).Last();
        Assert.Equal((HistoryAction.Update, user.Id), (record.Action, record.UserId));
        Assert.Contains(old, record.Before);
        Assert.Contains(changed, record.After);
    }
}
