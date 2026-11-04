using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ChangePasswordTests(MigratedApiFactory factory)
{
    private const string Password = "Temporary-pass-1";
    private const string NewPassword = "Chosen-pass-2";

    private async Task<Session> SignInAsync(string username, string password)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json))!;
    }

    private Task<HttpResponseMessage> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.PostAsJsonAsync("/auth/change-password", new ChangePasswordRequest(currentPassword, newPassword));
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    [Fact]
    public async Task ChangingATemporaryPasswordEndsTheForcedChangeAndTheNewPasswordWorks()
    {
        var user = await factory.AddUserAsync(Password, mustChangePassword: true);
        var session = await SignInAsync(user.Username, Password);

        var response = await ChangePasswordAsync(session.AccessToken, Password, NewPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False((await response.Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json))!.MustChangePassword);
        Assert.False((await SignInAsync(user.Username, NewPassword)).MustChangePassword);
    }

    [Fact]
    public async Task AWrongCurrentPasswordIsRefusedAndCountsTowardTheLockout()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SignInAsync(user.Username, Password);

        for (var attempt = 1; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            var refused = await ChangePasswordAsync(session.AccessToken, "wrong", NewPassword);
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidCredentials", await CodeOf(refused));
        }
        await ChangePasswordAsync(session.AccessToken, "wrong", NewPassword);

        Assert.Equal("accountLocked", await CodeOf(await ChangePasswordAsync(session.AccessToken, Password, NewPassword)));
    }

    [Theory]
    [InlineData("Short-1")]
    [InlineData(Password)]
    public async Task ANewPasswordShorterThan8CharactersOrUnchangedIsRefused(string newPassword)
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SignInAsync(user.Username, Password);

        var response = await ChangePasswordAsync(session.AccessToken, Password, newPassword);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("weakPassword", await CodeOf(response));
    }

    [Fact]
    public async Task ChangingThePasswordSignsOutTheOtherDevicesAndKeepsThisOne()
    {
        var user = await factory.AddUserAsync(Password);
        var thisPhone = await SignInAsync(user.Username, Password);
        var otherPhone = await SignInAsync(user.Username, Password);

        var changed = await (await ChangePasswordAsync(thisPhone.AccessToken, Password, NewPassword)).Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json);

        var otherRefresh = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(otherPhone.RefreshToken));
        var thisRefresh = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(changed!.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, otherRefresh.StatusCode);
        Assert.Equal(HttpStatusCode.OK, thisRefresh.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await ChangePasswordAsync(otherPhone.AccessToken, NewPassword, "Another-pass-3")).StatusCode);
    }

    [Fact]
    public async Task APasswordChangeIsInTheAuditTrailWithTheValuesBeforeAndAfterButNeverThePasswordHash()
    {
        var user = await factory.AddUserAsync(Password, mustChangePassword: true);
        var session = await SignInAsync(user.Username, Password);

        await ChangePasswordAsync(session.AccessToken, Password, NewPassword);

        var record = (await factory.AuditTrailOfAsync(user.Id)).Last();
        Assert.Equal(AuditAction.ChangePassword, record.Action);
        Assert.Equal(user.Id, record.UserId);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"MustChangePassword": true}"""), JsonNode.Parse(record.Before)));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"MustChangePassword": false}"""), JsonNode.Parse(record.After)));
    }

    [Fact]
    public async Task ChangingThePasswordNeedsASession()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/auth/change-password", new ChangePasswordRequest(Password, NewPassword));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ADisabledUsersAccessTokenIsRefusedAtTheNextRequest()
    {
        var user = await factory.AddUserAsync(Password);
        var session = await SignInAsync(user.Username, Password);

        await factory.DisableAsync(user);

        Assert.Equal(HttpStatusCode.Unauthorized, (await ChangePasswordAsync(session.AccessToken, Password, NewPassword)).StatusCode);
    }

    [Fact]
    public async Task EveryEndpointNotOpenToAllRefusesASessionWithATemporaryPassword()
    {
        var fallback = (await factory.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync())!;

        var claim = Assert.Single(fallback.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.ClaimsAuthorizationRequirement>());
        Assert.Equal(AccessTokens.MustChangePasswordClaim, claim.ClaimType);
        Assert.Equal(["false"], claim.AllowedValues);
    }
}
