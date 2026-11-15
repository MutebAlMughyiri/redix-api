using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Redix.Api.History;
using Redix.Api.Auth;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class UsersTests(MigratedApiFactory factory)
{
    private const string Password = "Users-pass-1";
    private const string Temporary = "Temporary-1";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    /// <summary>A project's manager, with <paramref name="user"/> in that project.</summary>
    private async Task<HttpClient> ManagerOfAsync(User user)
    {
        var manager = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);
        var project = await factory.ApprovedProjectAsync(manager, $"project-{Guid.CreateVersion7()}");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of("Field"));
        return manager;
    }

    private static async Task<UserView> ViewOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<UserView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private static Task<HttpResponseMessage> ResetAsync(HttpClient client, Guid userId, string password = Temporary) =>
        client.PostAsJsonAsync($"/users/{userId}/reset-password", new ResetPasswordRequest(password));

    /// <summary>The code «نسيت كلمة المرور؟» sends to <paramref name="email"/>, the email of the account <paramref name="login"/> names (redix-api #127).</summary>
    private async Task<string> ResetCodeAsync(string login, string email)
    {
        (await factory.CreateClient().PostAsJsonAsync("/auth/forgot-password", new ForgotPasswordRequest(login))).EnsureSuccessStatusCode();
        return await factory.Emails.LastCodeToAsync(email);
    }

    private Task<HttpResponseMessage> ResetWithCodeAsync(string login, string code) =>
        factory.CreateClient().PostAsJsonAsync("/auth/reset-with-code", new ResetWithCodeRequest(login, code, "Remembered-2"));

    [Fact]
    public async Task EditingChangesTheNameAndMobile()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);

        var edited = await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" نواف الأول ", "0511111111", MigratedApiFactory.NewEmail())));

        Assert.Equal((user.Username, "نواف الأول", "0511111111"), (edited.Username, edited.DisplayName, edited.Mobile));
        Assert.Equal("invalidName", await CodeOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" ", "", MigratedApiFactory.NewEmail()))));
    }

    [Fact]
    public async Task AC_07_8_TheManagerChangesAPersonsEmailOrKeepsIt()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);
        var email = MigratedApiFactory.NewEmail();

        var changed = await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", $" {email} ")));
        var kept = await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "", email)));

        Assert.Equal((email, email), (changed.Email, kept.Email));
    }

    [Fact]
    public async Task R127_TheManagerChangingAPersonsEmailEndsTheResetCodeSentToTheOldOne()
    {
        var old = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: old);
        var manager = await ManagerOfAsync(user);
        var code = await ResetCodeAsync(user.Username, old);

        await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", MigratedApiFactory.NewEmail())));

        Assert.Equal("codeWrong", await CodeOf(await ResetWithCodeAsync(user.Username, code)));
    }

    [Fact]
    public async Task R127_TheManagerChangingAPersonsEmailEndsTheirCodeToAnAddressNotYetConfirmed()
    {
        var user = await factory.AddUserAsync(Password, email: MigratedApiFactory.NewEmail());
        var manager = await ManagerOfAsync(user);
        var person = await factory.ClientSignedInAsAsync(user, Password);
        var wanted = MigratedApiFactory.NewEmail();
        (await person.PostAsJsonAsync("/me/email", new MyEmailRequest(wanted))).EnsureSuccessStatusCode();
        var code = await factory.Emails.LastCodeToAsync(wanted);

        await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", MigratedApiFactory.NewEmail())));

        Assert.Equal("codeWrong", await CodeOf(await person.PostAsJsonAsync("/me/email/confirm", new ConfirmMyEmailRequest(code))));
    }

    [Fact]
    public async Task R127_AnEditThatKeepsTheEmailLeavesItsResetCodeWorking()
    {
        var email = MigratedApiFactory.NewEmail();
        var user = await factory.AddUserAsync(Password, email: email);
        var manager = await ManagerOfAsync(user);
        var code = await ResetCodeAsync(user.Username, email);

        await ViewOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "0511111111", email)));

        Assert.Equal(HttpStatusCode.NoContent, (await ResetWithCodeAsync(user.Username, code)).StatusCode);
    }

    [Fact]
    public async Task AC_07_8_R9_TheManagerCannotGiveAnEmailAnotherAccountHoldsNorOneNotWellFormed()
    {
        var other = MigratedApiFactory.NewEmail();
        await factory.AddUserAsync(Password, email: other);
        var user = await factory.AddUserAsync(Password, email: MigratedApiFactory.NewEmail());
        var manager = await ManagerOfAsync(user);

        var taken = await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", other.ToUpperInvariant()));

        Assert.Equal((HttpStatusCode.Conflict, "emailTaken"), (taken.StatusCode, await CodeOf(taken)));
        Assert.Equal("emailInvalid", await CodeOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", ""))));
        Assert.Equal("emailInvalid", await CodeOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "", "nawaf"))));
    }

    [Fact]
    public async Task AnEditCannotGiveAMobileThatIsNotDigits()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);

        Assert.Equal("invalidMobile", await CodeOf(await manager.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "٠٥١١١١١١١١", MigratedApiFactory.NewEmail()))));
    }

    [Fact]
    public async Task AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);
        var session = (await (await SignInAsync(user.Username, Password)).Content.ReadFromJsonAsync<Session>(Json))!;
        for (var attempt = 0; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }
        Assert.Equal("accountLocked", await CodeOf(await SignInAsync(user.Username, Password)));

        var reset = await ViewOf(await ResetAsync(manager, user.Id));

        Assert.True(reset.MustChangePassword);
        var renewal = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewal.StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
        var signedIn = (await (await SignInAsync(user.Username, Temporary)).Content.ReadFromJsonAsync<Session>(Json))!;
        Assert.True(signedIn.MustChangePassword);
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(manager, user.Id, "Short-1")));
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(manager, user.Id, "only-letters")));
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(manager, user.Id, user.Username)));
    }

    [Fact]
    public async Task R126_D3_ALoginIsNoLongerDisabledOrEnabledAsAWhole()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);

        Assert.Equal(HttpStatusCode.NotFound, (await manager.PostAsync($"/users/{user.Id}/disable", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await manager.PostAsync($"/users/{user.Id}/enable", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task AnUnknownUserIsNotFound()
    {
        var user = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.NotFound, (await user.PutAsJsonAsync($"/users/{Guid.CreateVersion7()}", new EditUserRequest("نواف", "", MigratedApiFactory.NewEmail()))).StatusCode);
    }

    [Fact]
    public async Task SomeoneWithNoRoleOverALoginCannotChangeIt()
    {
        var target = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        var refused = await ResetAsync(client, target.Id);

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.Equal("otherProject", await CodeOf(refused));
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(target.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task EveryChangeIsInTheHistoryWithNoPasswordHash()
    {
        var managerUser = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(managerUser, Password);
        var project = await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of("Field"));

        await client.PutAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "0500000000", MigratedApiFactory.NewEmail()));
        await ResetAsync(client, user.Id);

        var trail = await factory.HistoryOfAsync(user.Id);
        Assert.Equal([HistoryAction.Update, HistoryAction.ResetPassword], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(managerUser.Id, record.UserId));
        Assert.Equal("نواف الأول", JsonNode.Parse(trail[0].After)!["DisplayName"]!.GetValue<string>());
        Assert.All(trail, record => Assert.DoesNotContain("PasswordHash", record.Before + record.After));
    }

    [Fact]
    public async Task AnAnswerNeverCarriesThePasswordHash()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(manager, $"project-{Guid.CreateVersion7()}");

        var listed = await manager.GetAsync($"/projects/{project.Id}/users");

        Assert.DoesNotContain("passwordHash", await listed.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
