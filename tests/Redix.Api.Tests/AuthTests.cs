using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class AuthTests(MigratedApiFactory factory)
{
    private const string Password = "Correct-horse-battery-1";

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(refreshToken));

    private static async Task<Session> SessionOf(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json))!;
    }

    private static async Task<string> RefusalCodeOf(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;
    }

    [Fact]
    public async Task TheFirstUserIsTheSystemAdminFromConfigurationAndMustChangeThePassword()
    {
        var session = await SessionOf(await SignInAsync(MigratedApiFactory.SystemAdminUsername, MigratedApiFactory.SystemAdminPassword));

        Assert.True(session.MustChangePassword);
        Assert.True(new JsonWebTokenHandler().ReadJsonWebToken(session.AccessToken).GetPayloadValue<bool>("mustChangePassword"));
    }

    [Fact]
    public async Task TheRightPasswordOpensASessionWithA15MinuteAccessToken()
    {
        var user = await factory.AddUserAsync(Password);

        var session = await SessionOf(await SignInAsync(user.Username, Password));

        var accessToken = new JsonWebTokenHandler().ReadJsonWebToken(session.AccessToken);
        Assert.Equal(user.Id.ToString(), accessToken.Subject);
        Assert.Equal(AccessTokens.Lifetime, accessToken.ValidTo - accessToken.IssuedAt);
        Assert.NotEmpty(session.RefreshToken);
        Assert.False(session.MustChangePassword);
    }

    [Fact]
    public async Task ASessionSaysWhatItsUserHoldsEverywhere()
    {
        var systemAdmin = await factory.AddUserAsync(Password, isSystemAdmin: true);
        var user = await factory.AddUserAsync(Password);

        var systemAdminSession = await SessionOf(await SignInAsync(systemAdmin.Username, Password));
        var userSession = await SessionOf(await SignInAsync(user.Username, Password));
        var renewed = await SessionOf(await RefreshAsync(systemAdminSession.RefreshToken));

        Assert.Equal([Permission.StartProject, Permission.ManageLogins], systemAdminSession.Permissions.Order());
        Assert.Empty(userSession.Permissions);
        Assert.Equal([Permission.StartProject, Permission.ManageLogins], renewed.Permissions.Order());
    }

    private async Task<HttpResponseMessage> SignOutAsync(Session session)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return await client.PostAsync("/auth/sign-out", null);
    }

    [Fact]
    public async Task AnAccessTokenWorksFor15MinutesByTheServersClock()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SessionOf(await SignInAsync(user.Username, Password));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users")).StatusCode);
        factory.Time.Advance(AccessTokens.Lifetime);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/users")).StatusCode);
    }

    [Fact]
    public async Task ASignOutEndsThisDeviceOnly()
    {
        var user = await factory.AddUserAsync(Password);
        var thisPhone = await SessionOf(await SignInAsync(user.Username, Password));
        var otherPhone = await SessionOf(await SignInAsync(user.Username, Password));

        var signedOut = await SignOutAsync(thisPhone);

        Assert.Equal(HttpStatusCode.NoContent, signedOut.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await SignOutAsync(thisPhone)).StatusCode);
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(thisPhone.RefreshToken)));
        await SessionOf(await RefreshAsync(otherPhone.RefreshToken));
    }

    [Fact]
    public async Task ASignOutIsInTheAuditTrail()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SessionOf(await SignInAsync(user.Username, Password));

        await SignOutAsync(session);

        var record = (await factory.AuditTrailOfAsync(user.Id)).Last();
        Assert.Equal(AuditAction.SignOut, record.Action);
        Assert.Equal(user.Id, record.UserId);
    }

    [Fact]
    public async Task SigningOutNeedsASession()
    {
        var response = await factory.CreateClient().PostAsync("/auth/sign-out", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ASignInIsInTheAuditTrailWithTheUserAndTheTime()
    {
        var user = await factory.AddUserAsync(Password);

        await SessionOf(await SignInAsync(user.Username, Password));

        var record = Assert.Single(await factory.AuditTrailOfAsync(user.Id));
        Assert.Equal(AuditAction.SignIn, record.Action);
        Assert.Equal(user.Id, record.UserId);
        Assert.Equal("Users", record.TableName);
        Assert.Equal(factory.Time.GetUtcNow().UtcDateTime, record.At);
    }

    [Fact]
    public async Task AWrongPasswordAndAnUnknownUsernameGetTheSameAnswer()
    {
        var user = await factory.AddUserAsync(Password);

        Assert.Equal("invalidCredentials", await RefusalCodeOf(await SignInAsync(user.Username, "wrong")));
        Assert.Equal("invalidCredentials", await RefusalCodeOf(await SignInAsync("nobody-" + Guid.NewGuid(), Password)));
    }

    [Fact]
    public async Task FiveWrongPasswordsInARowLockTheAccountFor15Minutes()
    {
        var user = await factory.AddUserAsync(Password);
        for (var attempt = 0; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            Assert.Equal("invalidCredentials", await RefusalCodeOf(await SignInAsync(user.Username, "wrong")));
        }

        Assert.Equal("accountLocked", await RefusalCodeOf(await SignInAsync(user.Username, Password)));

        factory.Time.Advance(User.LockDuration);
        await SessionOf(await SignInAsync(user.Username, Password));
    }

    [Fact]
    public async Task TheRightPasswordClearsTheCountOfWrongOnes()
    {
        var user = await factory.AddUserAsync(Password);
        for (var attempt = 1; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }
        await SessionOf(await SignInAsync(user.Username, Password));

        for (var attempt = 1; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }

        await SessionOf(await SignInAsync(user.Username, Password));
    }

    [Fact]
    public async Task ADisabledUserCannotSignIn()
    {
        var user = await factory.AddUserAsync(Password, isActive: false);

        Assert.Equal("invalidCredentials", await RefusalCodeOf(await SignInAsync(user.Username, Password)));
    }

    [Fact]
    public async Task ADisabledUsersOpenSessionEndsAtItsNextRequest()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SessionOf(await SignInAsync(user.Username, Password));

        await factory.DisableAsync(user);

        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(session.RefreshToken)));
    }

    [Fact]
    public async Task ARefreshReplacesTheRefreshToken()
    {
        var user = await factory.AddUserAsync(Password);
        var first = await SessionOf(await SignInAsync(user.Username, Password));

        var renewed = await SessionOf(await RefreshAsync(first.RefreshToken));

        Assert.NotEqual(first.RefreshToken, renewed.RefreshToken);
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(first.RefreshToken)));
    }

    [Fact]
    public async Task ASessionUsedWithin12HoursStaysOpenAndOneUnusedFor12HoursEnds()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SessionOf(await SignInAsync(user.Username, Password));

        factory.Time.Advance(Device.IdleTimeout - TimeSpan.FromMinutes(1));
        session = await SessionOf(await RefreshAsync(session.RefreshToken));

        factory.Time.Advance(Device.IdleTimeout);
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(session.RefreshToken)));
    }
}
