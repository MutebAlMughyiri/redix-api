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
    public async Task TheFirstUserIsTheAdminFromConfigurationAndMustChangeThePassword()
    {
        var session = await SessionOf(await SignInAsync(MigratedApiFactory.AdminUsername, MigratedApiFactory.AdminPassword));

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
        var admin = await factory.AddUserAsync(Password, isAdmin: true);
        var user = await factory.AddUserAsync(Password);

        var adminSession = await SessionOf(await SignInAsync(admin.Username, Password));
        var userSession = await SessionOf(await SignInAsync(user.Username, Password));
        var renewed = await SessionOf(await RefreshAsync(adminSession.RefreshToken));

        Assert.Equal([Permission.ApproveProjects], adminSession.Permissions);
        Assert.Empty(userSession.Permissions);
        Assert.Equal([Permission.ApproveProjects], renewed.Permissions);
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

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/projects/registered")).StatusCode);
        factory.Time.Advance(AccessTokens.Lifetime);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/projects/registered")).StatusCode);
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

    private async Task<HttpResponseMessage> PostBiometricsAsync(Session session, bool on)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return await client.PostAsJsonAsync("/auth/biometrics", new SetBiometricsRequest(on));
    }

    private async Task<Session> SetBiometricsAsync(Session session, bool on) => await SessionOf(await PostBiometricsAsync(session, on));

    [Fact]
    public async Task TurningBiometricsOnIsAnswered()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SessionOf(await SignInAsync(user.Username, Password));

        var turnedOn = await SetBiometricsAsync(session, true);

        Assert.True(turnedOn.UsesBiometrics);
        Assert.NotEqual(session.RefreshToken, turnedOn.RefreshToken);
        Assert.True((await SessionOf(await RefreshAsync(turnedOn.RefreshToken))).UsesBiometrics);
    }

    [Fact]
    public async Task ANewSignInStartsWithBiometricsOff()
    {
        var user = await factory.AddUserAsync(Password);
        await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);

        Assert.False((await SessionOf(await SignInAsync(user.Username, Password))).UsesBiometrics);
    }

    [Fact]
    public async Task AnUnusedPhoneWithBiometricsNeverTimesOut()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);

        factory.Time.Advance(TimeSpan.FromDays(365));

        await SessionOf(await RefreshAsync(session.RefreshToken));
    }

    [Fact]
    public async Task TurningBiometricsOffBringsBackThe12Hours()
    {
        var user = await factory.AddUserAsync(Password);
        var turnedOn = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);

        factory.Time.Advance(TimeSpan.FromDays(30));
        var renewed = await SessionOf(await RefreshAsync(turnedOn.RefreshToken));
        var turnedOff = await SetBiometricsAsync(renewed, false);

        Assert.False(turnedOff.UsesBiometrics);
        factory.Time.Advance(Device.IdleTimeout);
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(turnedOff.RefreshToken)));
    }

    [Fact]
    public async Task BiometricsDoNotOutliveARevokedPhoneOrADisabledUser()
    {
        var user = await factory.AddUserAsync(Password);
        var signedOut = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);
        Assert.Equal(HttpStatusCode.NoContent, (await SignOutAsync(signedOut)).StatusCode);
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(signedOut.RefreshToken)));

        var otherPhone = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);
        var thisPhone = await SessionOf(await SignInAsync(user.Username, Password));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", thisPhone.AccessToken);
        var changed = await SessionOf(await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordRequest(Password, Password + "-new")));
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(otherPhone.RefreshToken)));

        var disabledPhone = await SetBiometricsAsync(changed, true);
        await factory.WithDatabaseAsync(async db =>
        {
            (await db.Users.FindAsync(user.Id))!.IsActive = false;
            await db.SaveChangesAsync();
        });
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(disabledPhone.RefreshToken)));
    }

    [Fact]
    public async Task ChangingThePasswordKeepsBiometricsOnThisPhone()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var changed = await SessionOf(await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordRequest(Password, Password + "-new")));

        Assert.True(changed.UsesBiometrics);
    }

    [Fact]
    public async Task BiometricsBelongToOnePhone()
    {
        var user = await factory.AddUserAsync(Password);
        var withIt = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);
        var without = await SessionOf(await SignInAsync(user.Username, Password));

        factory.Time.Advance(Device.IdleTimeout);

        await SessionOf(await RefreshAsync(withIt.RefreshToken));
        Assert.Equal("sessionExpired", await RefusalCodeOf(await RefreshAsync(without.RefreshToken)));
    }

    [Fact]
    public async Task ATemporaryPasswordCannotTurnBiometricsOn()
    {
        var user = await factory.AddUserAsync(Password, mustChangePassword: true);

        var answer = await PostBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);

        Assert.Equal(HttpStatusCode.Forbidden, answer.StatusCode);
    }

    [Fact]
    public async Task TurningBiometricsOnAndOffIsAudited()
    {
        var user = await factory.AddUserAsync(Password);
        var turnedOn = await SetBiometricsAsync(await SessionOf(await SignInAsync(user.Username, Password)), true);

        await SetBiometricsAsync(turnedOn, false);

        var trail = await factory.AuditTrailOfAsync(user.Id);
        Assert.Equal([AuditAction.SignIn, AuditAction.TurnOnBiometrics, AuditAction.TurnOffBiometrics], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(user.Id, record.UserId));
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
