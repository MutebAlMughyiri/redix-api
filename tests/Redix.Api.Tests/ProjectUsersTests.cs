using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ProjectUsersTests(MigratedApiFactory factory)
{
    private const string Password = "Project-users-1";
    private const string Temporary = "Temporary-1";
    private const string ManagerWord = "users.roleManager";
    private const string FinanceWord = "users.roleFinance";
    private const string FieldWord = "users.roleField";
    private const string ReaderWord = "users.roleReader";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private static string NewUsername() => $"user-{Guid.CreateVersion7():N}";

    private static Guid RoleOf(string word) => RoleSeed.IdOf(word);

    /// <summary>A system admin who started a new project, and so manages it.</summary>
    private async Task<(User User, HttpClient Client, Guid ProjectId)> ManagerAsync()
    {
        var user = await factory.AddUserAsync(Password, isSystemAdmin: true);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        return (user, client, await StartProjectAsync(client));
    }

    private static async Task<Guid> StartProjectAsync(HttpClient client)
    {
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest($"project-{Guid.CreateVersion7()}", new DateOnly(2026, 11, 1), []));
        return (await created.Content.ReadFromJsonAsync<ProjectView>(Json))!.Id;
    }

    /// <summary>A manager of a new project who holds no system-wide role.</summary>
    private async Task<(User User, HttpClient Client, Guid ProjectId)> PlainManagerAsync()
    {
        var (_, _, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, ManagerWord);
        return (user, await factory.ClientSignedInAsAsync(user, Password), projectId);
    }

    private static async Task<UserView> ViewOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<UserView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, Guid projectId, string username, string displayName = "نواف", string mobile = "0500000000", string password = Temporary) =>
        client.PostAsJsonAsync($"/projects/{projectId}/users", new CreateUserRequest(displayName, username, mobile, password));

    private static Task<HttpResponseMessage> AddExistingAsync(HttpClient client, Guid projectId, string username) =>
        client.PostAsJsonAsync($"/projects/{projectId}/users/existing", new AddExistingUserRequest(username));

    private static Task<HttpResponseMessage> GiveAsync(HttpClient client, Guid projectId, Guid userId, string word) =>
        client.PostAsync($"/projects/{projectId}/users/{userId}/roles/{RoleOf(word)}", null);

    private static Task<HttpResponseMessage> TakeAsync(HttpClient client, Guid projectId, Guid userId, string word) =>
        client.PostAsync($"/projects/{projectId}/users/{userId}/roles/{RoleOf(word)}/remove", null);

    private static Task<HttpResponseMessage> ResetAsync(HttpClient client, Guid userId) =>
        client.PostAsJsonAsync($"/users/{userId}/reset-password", new ResetPasswordRequest(Temporary));

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private static Task<List<UserView>?> ListAsync(HttpClient client, Guid projectId) =>
        client.GetFromJsonAsync<List<UserView>>($"/projects/{projectId}/users", Json);

    [Fact]
    public async Task AManagerCreatesALoginInTheProjectWhoStartsAsTheLastRoleAndMustChangeThePassword()
    {
        var (_, client, projectId) = await ManagerAsync();
        var username = NewUsername();

        var created = await ViewOf(await CreateAsync(client, projectId, $"  {username}  ", "  نواف  ", " 0500000000 "));

        Assert.Equal((username, "نواف", "0500000000", true, true), (created.Username, created.DisplayName, created.Mobile, created.IsActive, created.MustChangePassword));
        Assert.Equal(ReaderWord, Assert.Single(created.Projects).Role.WordKey);
        var session = (await (await SignInAsync(username, Temporary)).Content.ReadFromJsonAsync<Session>(Json))!;
        Assert.True(session.MustChangePassword);
        Assert.Empty(session.Permissions);
    }

    [Fact]
    public async Task AMobileMayBeEmpty()
    {
        var (_, client, projectId) = await ManagerAsync();

        Assert.Equal("", (await ViewOf(await CreateAsync(client, projectId, NewUsername(), mobile: ""))).Mobile);
    }

    [Fact]
    public async Task ARepeatedUsernameIsRefusedWhateverItsCase()
    {
        var (_, client, projectId) = await ManagerAsync();
        var username = NewUsername();
        await ViewOf(await CreateAsync(client, projectId, username));

        var repeated = await CreateAsync(client, projectId, username.ToUpperInvariant());

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
        var (_, client, projectId) = await ManagerAsync();

        var refused = await CreateAsync(client, projectId, username, displayName, mobile, password);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal(code, await CodeOf(refused));
    }

    [Fact]
    public async Task AnExistingUserIsAddedByUsernameAsTheLastRole()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);

        var added = await ViewOf(await AddExistingAsync(client, projectId, $" {user.Username} "));
        var again = await ViewOf(await AddExistingAsync(client, projectId, user.Username));

        Assert.Equal(ReaderWord, Assert.Single(added.Projects).Role.WordKey);
        Assert.Single(again.Projects);
    }

    [Fact]
    public async Task AnUnknownUsernameSaysSo()
    {
        var (_, client, projectId) = await ManagerAsync();

        var refused = await AddExistingAsync(client, projectId, "nobody-has-this");

        Assert.Equal(HttpStatusCode.NotFound, refused.StatusCode);
        Assert.Equal("unknownUser", await CodeOf(refused));
    }

    [Fact]
    public async Task AManagerSeesOnlyTheirProjectsPeopleAndNoneOfTheirOtherProjects()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var (_, otherClient, otherProjectId) = await ManagerAsync();
        var shared = await factory.AddUserAsync(Password);
        var stranger = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(shared.Id, projectId, FieldWord);
        await factory.GiveRoleAsync(shared.Id, otherProjectId, ReaderWord);
        await factory.GiveRoleAsync(stranger.Id, otherProjectId, ReaderWord);

        var listed = (await ListAsync(client, projectId))!;

        Assert.Equal(new[] { manager.Id, shared.Id }.Order(), listed.Select(user => user.Id).Order());
        var theirLine = Assert.Single(listed.Single(user => user.Id == shared.Id).Projects);
        Assert.Equal(projectId, theirLine.ProjectId);
    }

    [Fact]
    public async Task AManagerCannotReachAnotherProject()
    {
        var (_, client, _) = await ManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{otherProjectId}/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{otherProjectId}/roles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateAsync(client, otherProjectId, NewUsername())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AddExistingAsync(client, otherProjectId, user.Username)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await GiveAsync(client, otherProjectId, user.Id, FinanceWord)).StatusCode);
    }

    [Theory]
    [InlineData(FinanceWord)]
    [InlineData(FieldWord)]
    [InlineData(ReaderWord)]
    public async Task OnlyAManagerManagesAProjectsPeople(string word)
    {
        var (_, _, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, word);
        var client = await factory.ClientSignedInAsAsync(user, Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{projectId}/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await GiveAsync(client, projectId, user.Id, ManagerWord)).StatusCode);
    }

    [Fact]
    public async Task ThePeopleOfAProjectCanHoldSeveralRolesInIt()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var answer = await ViewOf(await GiveAsync(client, projectId, manager.Id, FinanceWord));
        var again = await ViewOf(await GiveAsync(client, projectId, manager.Id, FinanceWord));

        Assert.Equal([ManagerWord, FinanceWord], answer.Projects.Select(line => line.Role.WordKey));
        Assert.Equal(2, again.Projects.Count);
    }

    [Fact]
    public async Task TakingTheLastRoleTakesThemOutOfTheProject()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, ReaderWord);
        var theirs = await factory.ClientSignedInAsAsync(user, Password);

        var answer = await ViewOf(await TakeAsync(client, projectId, user.Id, ReaderWord));

        Assert.Empty(answer.Projects);
        Assert.DoesNotContain((await ListAsync(client, projectId))!, listed => listed.Id == user.Id);
        Assert.Empty((await theirs.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!);
        Assert.Equal(HttpStatusCode.NotFound, (await TakeAsync(client, projectId, user.Id, ReaderWord)).StatusCode);
    }

    [Fact]
    public async Task AUserNotInTheProjectIsNotFound()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);

        Assert.Equal(HttpStatusCode.NotFound, (await GiveAsync(client, projectId, user.Id, FinanceWord)).StatusCode);
    }

    [Fact]
    public async Task ARoleOfTheWrongScopeOrAnUnknownOneIsRefused()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var system = await GiveAsync(client, projectId, manager.Id, "users.roleSystem");
        var unknown = await client.PostAsync($"/projects/{projectId}/users/{manager.Id}/roles/{Guid.CreateVersion7()}", null);

        Assert.Equal(HttpStatusCode.BadRequest, system.StatusCode);
        Assert.Equal("invalidRole", await CodeOf(system));
        Assert.Equal("invalidRole", await CodeOf(unknown));
    }

    [Fact]
    public async Task TheLastManagerCannotLoseTheRole()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var refused = await TakeAsync(client, projectId, manager.Id, ManagerWord);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("lastManager", await CodeOf(refused));
    }

    [Fact]
    public async Task WithASecondManagerTheFirstCanLeave()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var second = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(second.Id, projectId, ManagerWord);

        var answer = await ViewOf(await TakeAsync(client, projectId, manager.Id, ManagerWord));

        Assert.Empty(answer.Projects);
        Assert.DoesNotContain((await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!, project => project.Id == projectId);
    }

    [Fact]
    public async Task ADisabledManagerDoesNotCountAsTheOneWhoStays()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var second = await factory.AddUserAsync(Password, isActive: false);
        await factory.GiveRoleAsync(second.Id, projectId, ManagerWord);

        Assert.Equal("lastManager", await CodeOf(await TakeAsync(client, projectId, manager.Id, ManagerWord)));
    }

    [Fact]
    public async Task TheLastManagerCannotBeDisabled()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var systemAdmin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isSystemAdmin: true), Password);

        var refused = await systemAdmin.PostAsync($"/users/{manager.Id}/disable", null);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("lastManager", await CodeOf(refused));
        Assert.True((await ListAsync(client, projectId))!.Single(user => user.Id == manager.Id).IsActive);
    }

    [Fact]
    public async Task AManagerResetsAndDisablesSomeoneOnlyInTheirProject()
    {
        var (_, client, projectId) = await PlainManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, FieldWord);

        var reset = await ViewOf(await ResetAsync(client, user.Id));
        var disabled = await ViewOf(await client.PostAsync($"/users/{user.Id}/disable", null));

        Assert.True(reset.MustChangePassword);
        Assert.False(disabled.IsActive);
    }

    [Fact]
    public async Task AManagerCannotResetSomeoneInAnotherProject()
    {
        var (_, client, projectId) = await PlainManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var shared = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(shared.Id, projectId, FieldWord);
        await factory.GiveRoleAsync(shared.Id, otherProjectId, FieldWord);
        var theirSession = (await (await SignInAsync(shared.Username, Password)).Content.ReadFromJsonAsync<Session>(Json))!;

        var refused = await ResetAsync(client, shared.Id);

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.Equal("otherProject", await CodeOf(refused));
        Assert.Equal("otherProject", await CodeOf(await client.PostAsync($"/users/{shared.Id}/disable", null)));
        Assert.Equal("otherProject", await CodeOf(await client.PostAsJsonAsync($"/users/{shared.Id}", new EditUserRequest("x", ""))));
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(shared.Username, Password)).StatusCode);
        var renewal = await factory.CreateClient().PostAsJsonAsync("/auth/refresh", new RefreshRequest(theirSession.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, renewal.StatusCode);
    }

    [Fact]
    public async Task SomeoneWhoManagesBothProjectsIsNotHeldBack()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var otherProjectId = await StartProjectAsync(client);
        var shared = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(shared.Id, projectId, FieldWord);
        await factory.GiveRoleAsync(shared.Id, otherProjectId, FieldWord);
        await factory.WithDatabaseAsync(async db =>
        {
            db.UserRoles.RemoveRange(db.UserRoles.Where(row => row.UserId == manager.Id));
            await db.SaveChangesAsync();
        });

        Assert.True((await ViewOf(await ResetAsync(client, shared.Id))).MustChangePassword);
    }

    [Fact]
    public async Task EveryRoleChangeIsInTheAuditTrail()
    {
        var (manager, client, projectId) = await ManagerAsync();
        await GiveAsync(client, projectId, manager.Id, FinanceWord);
        var memberId = await factory.ProjectMemberIdAsync(manager.Id, projectId, RoleOf(FinanceWord));
        await TakeAsync(client, projectId, manager.Id, FinanceWord);

        var trail = await factory.AuditTrailOfAsync(memberId);

        Assert.Equal([AuditAction.AssignProject, AuditAction.RemoveProject], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(manager.Id, record.UserId));
        Assert.Equal(RoleOf(FinanceWord), JsonNode.Parse(trail[0].After)!["RoleId"]!.GetValue<Guid>());
        Assert.Equal("{}", trail[1].After.Replace(" ", ""));
    }

    [Fact]
    public async Task ANewLoginIsInTheAuditTrailWithNoPasswordHash()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var username = NewUsername();

        var created = await ViewOf(await CreateAsync(client, projectId, username));

        var trail = await factory.AuditTrailOfAsync(created.Id);
        var record = Assert.Single(trail);
        Assert.Equal((AuditAction.Create, manager.Id), (record.Action, record.UserId));
        Assert.Equal(username, JsonNode.Parse(record.After)!["Username"]!.GetValue<string>());
        Assert.DoesNotContain("PasswordHash", record.Before + record.After);
    }
}
