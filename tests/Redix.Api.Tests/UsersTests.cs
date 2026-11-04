using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class UsersTests(MigratedApiFactory factory)
{
    private const string Password = "Users-pass-1";
    private const string Temporary = "Temporary-1";

    private async Task<HttpClient> OwnerAsync() => await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isOwner: true), Password);

    private static string NewUsername() => $"user-{Guid.CreateVersion7():N}";

    private static async Task<UserView> ViewOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<UserView>())!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, string username, string displayName = "نواف", string mobile = "0500000000", string password = Temporary) =>
        client.PostAsJsonAsync("/users", new CreateUserRequest(displayName, username, mobile, password));

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private Task<HttpResponseMessage> ResetAsync(HttpClient owner, Guid userId, string password = Temporary) =>
        owner.PostAsJsonAsync($"/users/{userId}/reset-password", new ResetPasswordRequest(password));

    [Fact]
    public async Task AnOwnerCreatesAUserWhoMustChangeThePassword()
    {
        var owner = await OwnerAsync();
        var username = NewUsername();

        var created = await ViewOf(await CreateAsync(owner, $"  {username}  ", "  نواف  ", " 0500000000 "));

        Assert.Equal((username, "نواف", "0500000000", false, true, true), (created.Username, created.DisplayName, created.Mobile, created.IsOwner, created.IsActive, created.MustChangePassword));
        var session = (await (await SignInAsync(username, Temporary)).Content.ReadFromJsonAsync<Session>())!;
        Assert.True(session.MustChangePassword);
        Assert.False(session.IsOwner);
    }

    [Fact]
    public async Task AMobileMayBeEmpty()
    {
        var owner = await OwnerAsync();

        Assert.Equal("", (await ViewOf(await CreateAsync(owner, NewUsername(), mobile: ""))).Mobile);
    }

    [Fact]
    public async Task ARepeatedUsernameIsRefusedWhateverItsCase()
    {
        var owner = await OwnerAsync();
        var username = NewUsername();
        await ViewOf(await CreateAsync(owner, username));

        var repeated = await CreateAsync(owner, username.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        Assert.Equal("usernameTaken", await CodeOf(repeated));
    }

    [Theory]
    [InlineData("   ", "u", "", Temporary, "invalidName")]
    [InlineData("n", "   ", "", Temporary, "invalidUsername")]
    [InlineData("n", "u", "012345678901234567890", Temporary, "invalidMobile")]
    [InlineData("n", "u", "", "Short-1", "weakPassword")]
    public async Task AFieldOutsideItsLimitIsRefused(string displayName, string username, string mobile, string password, string code)
    {
        var owner = await OwnerAsync();

        var refused = await CreateAsync(owner, username, displayName, mobile, password);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal(code, await CodeOf(refused));
    }

    [Fact]
    public async Task EditingChangesTheNameAndMobile()
    {
        var owner = await OwnerAsync();
        var username = NewUsername();
        var user = await ViewOf(await CreateAsync(owner, username));

        var edited = await ViewOf(await owner.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" نواف الأول ", "0511111111")));

        Assert.Equal((username, "نواف الأول", "0511111111"), (edited.Username, edited.DisplayName, edited.Mobile));
        var listed = (await owner.GetFromJsonAsync<List<UserView>>("/users"))!.Single(listed => listed.Id == user.Id);
        Assert.Equal("نواف الأول", listed.DisplayName);
        Assert.Equal("invalidName", await CodeOf(await owner.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" ", ""))));
    }

    [Fact]
    public async Task AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice()
    {
        var owner = await OwnerAsync();
        var user = await factory.AddUserAsync(Password);
        var session = (await (await SignInAsync(user.Username, Password)).Content.ReadFromJsonAsync<Session>())!;
        for (var attempt = 0; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }
        Assert.Equal("accountLocked", await CodeOf(await SignInAsync(user.Username, Password)));

        var reset = await ViewOf(await ResetAsync(owner, user.Id));

        Assert.True(reset.MustChangePassword);
        var renewal = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewal.StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
        var signedIn = (await (await SignInAsync(user.Username, Temporary)).Content.ReadFromJsonAsync<Session>())!;
        Assert.True(signedIn.MustChangePassword);
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(owner, user.Id, "Short-1")));
    }

    [Fact]
    public async Task DisablingAUserEndsTheOpenSessionAtTheNextRequest()
    {
        var owner = await OwnerAsync();
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/projects")).StatusCode);

        var disabled = await ViewOf(await owner.PostAsync($"/users/{user.Id}/disable", null));

        Assert.False(disabled.IsActive);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/projects")).StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
    }

    [Fact]
    public async Task AnEnabledUserCanSignInAgain()
    {
        var owner = await OwnerAsync();
        var user = await factory.AddUserAsync(Password);
        await owner.PostAsync($"/users/{user.Id}/disable", null);

        var enabled = await ViewOf(await owner.PostAsync($"/users/{user.Id}/enable", null));

        Assert.True(enabled.IsActive);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task AnOwnerCannotBeDisabled()
    {
        var owner = await factory.AddUserAsync(Password, isOwner: true);
        var client = await factory.ClientSignedInAsAsync(owner, Password);

        var refused = await client.PostAsync($"/users/{owner.Id}/disable", null);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("ownerCannotBeDisabled", await CodeOf(refused));
        Assert.True((await client.GetFromJsonAsync<List<UserView>>("/users"))!.Single(listed => listed.Id == owner.Id).IsActive);
    }

    [Fact]
    public async Task OwnersComeFirstThenActiveUsersByName()
    {
        var owner = await OwnerAsync();
        var first = await ViewOf(await CreateAsync(owner, NewUsername(), "ا"));
        var second = await ViewOf(await CreateAsync(owner, NewUsername(), "ب"));
        await owner.PostAsync($"/users/{first.Id}/disable", null);

        var users = (await owner.GetFromJsonAsync<List<UserView>>("/users"))!;

        Assert.True(users.First().IsOwner);
        Assert.True(users.FindIndex(user => user.Id == second.Id) < users.FindIndex(user => user.Id == first.Id));
    }

    [Fact]
    public async Task AnUnknownUserIsNotFound()
    {
        var owner = await OwnerAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await owner.PostAsync($"/users/{Guid.CreateVersion7()}/enable", null)).StatusCode);
    }

    [Fact]
    public async Task OnlyAnOwnerManagesUsers()
    {
        var user = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync("/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateAsync(user, NewUsername())).StatusCode);
    }

    [Fact]
    public async Task EveryChangeIsInTheAuditTrailWithNoPasswordHash()
    {
        var owner = await factory.AddUserAsync(Password, isOwner: true);
        var client = await factory.ClientSignedInAsAsync(owner, Password);
        var username = NewUsername();
        var user = await ViewOf(await CreateAsync(client, username));

        await client.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "0500000000"));
        await ResetAsync(client, user.Id);
        await client.PostAsync($"/users/{user.Id}/disable", null);
        await client.PostAsync($"/users/{user.Id}/enable", null);

        var trail = await factory.AuditTrailOfAsync(user.Id);
        Assert.Equal([AuditAction.Create, AuditAction.Update, AuditAction.ResetPassword, AuditAction.Disable, AuditAction.Enable], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(owner.Id, record.UserId));
        Assert.Equal("{}", trail[0].Before.Replace(" ", ""));
        Assert.Equal(username, JsonNode.Parse(trail[0].After)!["Username"]!.GetValue<string>());
        Assert.Equal("نواف الأول", JsonNode.Parse(trail[1].After)!["DisplayName"]!.GetValue<string>());
        Assert.All(trail, record => Assert.DoesNotContain("PasswordHash", record.Before + record.After));
    }

    [Fact]
    public async Task AnAnswerNeverCarriesThePasswordHash()
    {
        var owner = await OwnerAsync();

        var created = await CreateAsync(owner, NewUsername());
        var listed = await owner.GetAsync("/users");

        Assert.DoesNotContain("passwordHash", await created.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", await listed.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
