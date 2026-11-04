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

    private async Task<HttpClient> SystemAdminAsync() => await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isSystemAdmin: true), Password);

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
        var systemAdmin = await SystemAdminAsync();
        var user = await factory.AddUserAsync(Password);

        var edited = await ViewOf(await systemAdmin.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" نواف الأول ", "0511111111")));

        Assert.Equal((user.Username, "نواف الأول", "0511111111"), (edited.Username, edited.DisplayName, edited.Mobile));
        var listed = (await systemAdmin.GetFromJsonAsync<List<UserView>>("/users", Json))!.Single(listed => listed.Id == user.Id);
        Assert.Equal("نواف الأول", listed.DisplayName);
        Assert.Equal("invalidName", await CodeOf(await systemAdmin.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest(" ", ""))));
    }

    [Fact]
    public async Task AResetForcesAChangeEndsTheLockAndSignsOutEveryDevice()
    {
        var systemAdmin = await SystemAdminAsync();
        var user = await factory.AddUserAsync(Password);
        var session = (await (await SignInAsync(user.Username, Password)).Content.ReadFromJsonAsync<Session>(Json))!;
        for (var attempt = 0; attempt < User.WrongPasswordsBeforeLock; attempt++)
        {
            await SignInAsync(user.Username, "wrong");
        }
        Assert.Equal("accountLocked", await CodeOf(await SignInAsync(user.Username, Password)));

        var reset = await ViewOf(await ResetAsync(systemAdmin, user.Id));

        Assert.True(reset.MustChangePassword);
        var renewal = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewal.StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
        var signedIn = (await (await SignInAsync(user.Username, Temporary)).Content.ReadFromJsonAsync<Session>(Json))!;
        Assert.True(signedIn.MustChangePassword);
        Assert.Equal("weakPassword", await CodeOf(await ResetAsync(systemAdmin, user.Id, "Short-1")));
    }

    [Fact]
    public async Task DisablingAUserEndsTheOpenSessionAtTheNextRequest()
    {
        var systemAdmin = await SystemAdminAsync();
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users")).StatusCode);

        var disabled = await ViewOf(await systemAdmin.PostAsync($"/users/{user.Id}/disable", null));

        Assert.False(disabled.IsActive);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/users")).StatusCode);
        Assert.Equal("invalidCredentials", await CodeOf(await SignInAsync(user.Username, Password)));
    }

    [Fact]
    public async Task AnEnabledUserCanSignInAgain()
    {
        var systemAdmin = await SystemAdminAsync();
        var user = await factory.AddUserAsync(Password);
        await systemAdmin.PostAsync($"/users/{user.Id}/disable", null);

        var enabled = await ViewOf(await systemAdmin.PostAsync($"/users/{user.Id}/enable", null));

        Assert.True(enabled.IsActive);
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(user.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task TheSystemAdminResetsAnyLogin()
    {
        var starter = await SystemAdminAsync();
        var created = await starter.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 6", new DateOnly(2026, 11, 1), []));
        var project = (await created.Content.ReadFromJsonAsync<ProjectView>(Json))!;
        var manager = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(manager.Id, project.Id, "users.roleManager");

        var reset = await ViewOf(await ResetAsync(await SystemAdminAsync(), manager.Id));

        Assert.True(reset.MustChangePassword);
    }

    [Fact]
    public async Task EveryLoginIsListedActiveUsersFirstByNameWithTheirRoles()
    {
        var starter = await SystemAdminAsync();
        var created = await starter.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 6", new DateOnly(2026, 11, 1), []));
        var project = (await created.Content.ReadFromJsonAsync<ProjectView>(Json))!;
        var systemAdmin = await SystemAdminAsync();
        var disabled = await factory.AddUserAsync(Password, isActive: false);
        var active = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(active.Id, project.Id, "users.roleField");

        var users = (await systemAdmin.GetFromJsonAsync<List<UserView>>("/users", Json))!;

        Assert.True(users.FindIndex(user => user.Id == active.Id) < users.FindIndex(user => user.Id == disabled.Id));
        var line = Assert.Single(users.Single(user => user.Id == active.Id).Projects);
        Assert.Equal((project.Id, "غصون 6", "users.roleField"), (line.ProjectId, line.Name, line.Role.WordKey));
    }

    [Fact]
    public async Task AnUnknownUserIsNotFound()
    {
        var systemAdmin = await SystemAdminAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await systemAdmin.PostAsync($"/users/{Guid.CreateVersion7()}/enable", null)).StatusCode);
    }

    [Fact]
    public async Task OnlyTheSystemAdminListsEveryLogin()
    {
        var user = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync("/users")).StatusCode);
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
        var systemAdmin = await factory.AddUserAsync(Password, isSystemAdmin: true);
        var client = await factory.ClientSignedInAsAsync(systemAdmin, Password);
        var user = await factory.AddUserAsync(Password);

        await client.PostAsJsonAsync($"/users/{user.Id}", new EditUserRequest("نواف الأول", "0500000000"));
        await ResetAsync(client, user.Id);
        await client.PostAsync($"/users/{user.Id}/disable", null);
        await client.PostAsync($"/users/{user.Id}/enable", null);

        var trail = await factory.AuditTrailOfAsync(user.Id);
        Assert.Equal([AuditAction.Update, AuditAction.ResetPassword, AuditAction.Disable, AuditAction.Enable], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(systemAdmin.Id, record.UserId));
        Assert.Equal("نواف الأول", JsonNode.Parse(trail[0].After)!["DisplayName"]!.GetValue<string>());
        Assert.All(trail, record => Assert.DoesNotContain("PasswordHash", record.Before + record.After));
    }

    [Fact]
    public async Task AnAnswerNeverCarriesThePasswordHash()
    {
        var systemAdmin = await SystemAdminAsync();

        var listed = await systemAdmin.GetAsync("/users");

        Assert.DoesNotContain("passwordHash", await listed.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }
}
