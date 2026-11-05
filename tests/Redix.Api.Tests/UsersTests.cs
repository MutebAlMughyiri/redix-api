using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

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
        await factory.GiveRoleAsync(user.Id, project.Id, "users.roleField");
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

    [Fact]
    public async Task EditingChangesTheNameAndMobile()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);

        var edited = await ViewOf(await manager.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" نواف الأول ", "0511111111")));

        Assert.Equal((user.Username, "نواف الأول", "0511111111"), (edited.Username, edited.DisplayName, edited.Mobile));
        Assert.Equal("invalidName", await CodeOf(await manager.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" ", ""))));
    }

    [Fact]
    public async Task AnEditCannotGiveAMobileThatIsNotDigits()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);

        Assert.Equal("invalidMobile", await CodeOf(await manager.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف", "٠٥١١١١١١١١"))));
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
    }

    [Fact]
    public async Task DisablingAUserEndsTheOpenSessionAtTheNextRequest()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/projects")).StatusCode);

        var disabled = await ViewOf(await manager.PostAsync($"/users/{user.Id}/disable", null));

        Assert.False(disabled.IsActive);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/projects")).StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
    }

    [Fact]
    public async Task AnEnabledUserCanSignInAgain()
    {
        var user = await factory.AddUserAsync(Password);
        var manager = await ManagerOfAsync(user);
        await manager.PostAsync($"/users/{user.Id}/disable", null);

        var enabled = await ViewOf(await manager.PostAsync($"/users/{user.Id}/enable", null));

        Assert.True(enabled.IsActive);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task AnUnknownUserIsNotFound()
    {
        var user = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.NotFound, (await user.PostAsync($"/users/{Guid.CreateVersion7()}/enable", null)).StatusCode);
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
    public async Task EveryChangeIsInTheAuditTrailWithNoPasswordHash()
    {
        var managerUser = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(managerUser, Password);
        var project = await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, project.Id, "users.roleField");

        await client.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "0500000000"));
        await ResetAsync(client, user.Id);
        await client.PostAsync($"/users/{user.Id}/disable", null);
        await client.PostAsync($"/users/{user.Id}/enable", null);

        var trail = await factory.AuditTrailOfAsync(user.Id);
        Assert.Equal([AuditAction.Update, AuditAction.ResetPassword, AuditAction.Disable, AuditAction.Enable], trail.Select(record => record.Action));
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
