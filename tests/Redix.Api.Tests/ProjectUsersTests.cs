using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(FourthDatabaseCollection.Name)]
public class ProjectUsersTests(MigratedApiFactory factory)
{
    private const string Password = "Project-users-1";
    private const string Temporary = "Temporary-1";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private static string NewUsername() => $"user-{Guid.CreateVersion7():N}";

    /// <summary>Someone who registered a project, now approved, and so manages it.</summary>
    private async Task<(User User, HttpClient Client, Guid ProjectId)> ManagerAsync()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        return (user, client, await StartProjectAsync(client));
    }

    private async Task<Guid> StartProjectAsync(HttpClient client) => (await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}")).Id;

    private static async Task<UserView> ViewOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<UserView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, Guid projectId, string username, string displayName = "نواف", string mobile = "0500000000", string password = Temporary, List<Permission>? ticked = null) =>
        client.PostAsJsonAsync($"/projects/{projectId}/users", new CreateUserRequest(displayName, username, mobile, password) { Permissions = ticked ?? [] }, Json);

    private static Task<HttpResponseMessage> AddExistingAsync(HttpClient client, Guid projectId, string username) =>
        client.PostAsJsonAsync($"/projects/{projectId}/users/existing", new AddExistingUserRequest(username));

    /// <summary>Ticks <paramref name="permission"/>, named in the route as the app names it.</summary>
    private static Task<HttpResponseMessage> GiveAsync(HttpClient client, Guid projectId, Guid userId, Permission permission) =>
        client.PostAsync($"/projects/{projectId}/users/{userId}/permissions/{JsonNamingPolicy.CamelCase.ConvertName(permission.ToString())}", null);

    private static Task<HttpResponseMessage> TakeAsync(HttpClient client, Guid projectId, Guid userId, Permission permission) =>
        client.PostAsync($"/projects/{projectId}/users/{userId}/permissions/{JsonNamingPolicy.CamelCase.ConvertName(permission.ToString())}/remove", null);

    private static List<Permission> HeldIn(UserView user) => Assert.Single(user.Projects).Permissions;

    private static Task<HttpResponseMessage> NameRoleAsync(HttpClient client, Guid projectId, Guid userId, string roleName) =>
        client.PostAsJsonAsync($"/projects/{projectId}/users/{userId}/role-name", new RoleNameRequest(roleName));

    private static Task<HttpResponseMessage> ResetAsync(HttpClient client, Guid userId) =>
        client.PostAsJsonAsync($"/users/{userId}/reset-password", new ResetPasswordRequest(Temporary));

    private Task<HttpResponseMessage> SignInAsync(string username, string password) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password));

    private static Task<List<UserView>?> ListAsync(HttpClient client, Guid projectId) =>
        client.GetFromJsonAsync<List<UserView>>($"/projects/{projectId}/users", Json);

    [Fact]
    public async Task AManagerCreatesALoginHoldingWhatWasTickedAndSeeingTheProjectWhoMustChangeThePassword()
    {
        var (_, client, projectId) = await ManagerAsync();
        var username = NewUsername();

        var created = await ViewOf(await CreateAsync(client, projectId, $"  {username}  ", "  نواف  ", " 0500000000 ", ticked: [Permission.RecordEntries, Permission.AttachFiles]));

        Assert.Equal((username, "نواف", "0500000000", true, true), (created.Username, created.DisplayName, created.Mobile, created.IsActive, created.MustChangePassword));
        Assert.Equal([Permission.ReadProject, Permission.RecordEntries, Permission.AttachFiles], HeldIn(created));
        var session = (await (await SignInAsync(username, Temporary)).Content.ReadFromJsonAsync<Session>(Json))!;
        Assert.True(session.MustChangePassword);
        Assert.Empty(session.Permissions);
    }

    [Fact]
    public async Task ANewLoginCannotBeGivenTheAdminsPermission()
    {
        var (_, client, projectId) = await ManagerAsync();

        var refused = await CreateAsync(client, projectId, NewUsername(), ticked: [Permission.ApproveProjects]);

        Assert.Equal("invalidPermission", await CodeOf(refused));
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

    [Theory]
    [InlineData("٠٥٠٠٠٠٠٠٠٠")]
    [InlineData("050 000 0000")]
    [InlineData("05+00000000")]
    [InlineData("++966500000000")]
    [InlineData("+")]
    [InlineData("mobile")]
    public async Task AMobileIsAsciiDigitsWithALeadingPlusAtMost(string mobile)
    {
        var (_, client, projectId) = await ManagerAsync();

        var refused = await CreateAsync(client, projectId, NewUsername(), mobile: mobile);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidMobile", await CodeOf(refused));
    }

    [Fact]
    public async Task AMobileMayStartWithAPlus()
    {
        var (_, client, projectId) = await ManagerAsync();

        Assert.Equal("+966500000000", (await ViewOf(await CreateAsync(client, projectId, NewUsername(), mobile: "+966500000000"))).Mobile);
    }

    [Fact]
    public async Task AnExistingUserIsAddedByUsernameSeeingTheProject()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);

        var added = await ViewOf(await AddExistingAsync(client, projectId, $" {user.Username} "));
        var again = await ViewOf(await AddExistingAsync(client, projectId, user.Username));

        Assert.Equal([Permission.ReadProject], HeldIn(added));
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
        await factory.GiveAsync(shared.Id, projectId, Holds.Field);
        await factory.GiveAsync(shared.Id, otherProjectId, Holds.Reader);
        await factory.GiveAsync(stranger.Id, otherProjectId, Holds.Reader);

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
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateAsync(client, otherProjectId, NewUsername())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AddExistingAsync(client, otherProjectId, user.Username)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await GiveAsync(client, otherProjectId, user.Id, Permission.ApproveEntries)).StatusCode);
    }

    [Theory]
    [InlineData(nameof(Holds.Finance))]
    [InlineData(nameof(Holds.Field))]
    [InlineData(nameof(Holds.Reader))]
    public async Task OnlyWhoeverManagesUsersManagesAProjectsPeople(string holds)
    {
        var (_, _, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Of(holds));
        var client = await factory.ClientSignedInAsAsync(user, Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{projectId}/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await GiveAsync(client, projectId, user.Id, Permission.ManageUsers)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await NameRoleAsync(client, projectId, user.Id, "مدير المشروع")).StatusCode);
    }

    [Fact]
    public async Task AManagerTypesWhatSomeoneIsCalledInTheProjectAndThatPersonReadsItWithTheProject()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var created = await ViewOf(await CreateAsync(client, projectId, NewUsername()));

        var named = await ViewOf(await NameRoleAsync(client, projectId, created.Id, "  مشرف مالي  "));
        await ViewOf(await NameRoleAsync(client, projectId, manager.Id, "مدير المشروع"));

        Assert.Equal("مشرف مالي", Assert.Single(named.Projects).RoleName);
        Assert.Equal("مشرف مالي", Assert.Single((await ListAsync(client, projectId))!, user => user.Id == created.Id).Projects.Single().RoleName);
        var theirs = Assert.Single((await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!, project => project.Id == projectId);
        Assert.Equal("مدير المشروع", theirs.RoleName);
        Assert.Equal("", Assert.Single((await ViewOf(await NameRoleAsync(client, projectId, created.Id, ""))).Projects).RoleName);
    }

    [Fact]
    public async Task TakingSomeoneOutOfTheProjectTakesTheirRoleNameSoComingBackStartsWithNone()
    {
        var (_, client, projectId) = await ManagerAsync();
        var created = await ViewOf(await CreateAsync(client, projectId, NewUsername()));
        await ViewOf(await NameRoleAsync(client, projectId, created.Id, "مدير المشروع"));

        await TakeAsync(client, projectId, created.Id, Permission.ReadProject);
        var back = await ViewOf(await AddExistingAsync(client, projectId, created.Username));

        Assert.Equal("", Assert.Single(back.Projects).RoleName);
        await factory.WithDatabaseAsync(async db => Assert.False(await db.ProjectRoleNames.AnyAsync(row => row.ProjectId == projectId && row.UserId == created.Id)));
    }

    [Fact]
    public async Task ARoleNameMayBeTypedWhenCreatingALoginAndIsEmptyWhenNoneWas()
    {
        var (_, client, projectId) = await ManagerAsync();

        var named = await ViewOf(await client.PostAsJsonAsync($"/projects/{projectId}/users", new CreateUserRequest("نواف", NewUsername(), "", Temporary) { RoleName = " مشرف ميداني " }, Json));
        var unnamed = await ViewOf(await CreateAsync(client, projectId, NewUsername()));

        Assert.Equal("مشرف ميداني", Assert.Single(named.Projects).RoleName);
        Assert.Equal("", Assert.Single(unnamed.Projects).RoleName);
    }

    [Fact]
    public async Task ARoleNameOver50IsRefusedAndOnlySomeoneInTheProjectIsNamed()
    {
        var (_, client, projectId) = await ManagerAsync();
        var created = await ViewOf(await CreateAsync(client, projectId, NewUsername()));
        var stranger = await factory.AddUserAsync(Password);

        var tooLong = await NameRoleAsync(client, projectId, created.Id, new string('م', ProjectRoleName.MaxLength + 1));
        var creating = await client.PostAsJsonAsync($"/projects/{projectId}/users", new CreateUserRequest("نواف", NewUsername(), "", Temporary) { RoleName = new string('م', ProjectRoleName.MaxLength + 1) }, Json);

        Assert.Equal((HttpStatusCode.BadRequest, "invalidRoleName"), (tooLong.StatusCode, await CodeOf(tooLong)));
        Assert.Equal((HttpStatusCode.BadRequest, "invalidRoleName"), (creating.StatusCode, await CodeOf(creating)));
        Assert.Equal(HttpStatusCode.OK, (await NameRoleAsync(client, projectId, created.Id, new string('م', ProjectRoleName.MaxLength))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await NameRoleAsync(client, projectId, stranger.Id, "مشرف")).StatusCode);
    }

    [Fact]
    public async Task WhoeverRegistersAProjectHoldsEveryPermissionOfAProjectInItSeeingAllCapitalRatherThanChosenPartners()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var listed = (await ListAsync(client, projectId))!.Single(user => user.Id == manager.Id);

        Assert.Equal(ProjectPermission.OfWhoeverStarts.Order(), HeldIn(listed));
        Assert.Equal(15, ProjectPermission.InAProject.Count);
    }

    [Fact]
    public async Task TickingGivesOnePermissionAndUntickingTakesOne()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Reader);

        var given = await ViewOf(await GiveAsync(client, projectId, user.Id, Permission.ApproveEntries));
        var again = await ViewOf(await GiveAsync(client, projectId, user.Id, Permission.ApproveEntries));
        var taken = await ViewOf(await TakeAsync(client, projectId, user.Id, Permission.ApproveEntries));

        Assert.Equal([Permission.ReadProject, Permission.ApproveEntries], HeldIn(given));
        Assert.Equal(HeldIn(given), HeldIn(again));
        Assert.Equal([Permission.ReadProject], HeldIn(taken));
    }

    [Fact]
    public async Task UntickingSeeTheProjectTakesThemOutOfTheProject()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Field);
        var theirs = await factory.ClientSignedInAsAsync(user, Password);

        var answer = await ViewOf(await TakeAsync(client, projectId, user.Id, Permission.ReadProject));

        Assert.Empty(answer.Projects);
        Assert.DoesNotContain((await ListAsync(client, projectId))!, listed => listed.Id == user.Id);
        Assert.Empty((await theirs.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!);
        Assert.Equal(HttpStatusCode.NotFound, (await GiveAsync(client, projectId, user.Id, Permission.RecordEntries)).StatusCode);
    }

    [Fact]
    public async Task AUserNotInTheProjectIsNotFound()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);

        Assert.Equal(HttpStatusCode.NotFound, (await GiveAsync(client, projectId, user.Id, Permission.ApproveEntries)).StatusCode);
    }

    [Fact]
    public async Task TheAdminsPermissionOrAnUnknownNameIsRefused()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var system = await GiveAsync(client, projectId, manager.Id, Permission.ApproveProjects);
        var unknown = await client.PostAsync($"/projects/{projectId}/users/{manager.Id}/permissions/notAPermission", null);

        Assert.Equal(HttpStatusCode.BadRequest, system.StatusCode);
        Assert.Equal("invalidPermission", await CodeOf(system));
        Assert.Equal("invalidPermission", await CodeOf(unknown));
    }

    [Theory]
    [InlineData(Permission.ManageUsers)]
    [InlineData(Permission.ReadProject)]
    public async Task TheLastWhoManagesUsersCannotLoseIt(Permission unticked)
    {
        var (manager, client, projectId) = await ManagerAsync();

        var refused = await TakeAsync(client, projectId, manager.Id, unticked);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("lastManager", await CodeOf(refused));
    }

    [Fact]
    public async Task WithASecondManagerTheFirstCanLeave()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var second = await factory.AddUserAsync(Password);
        await factory.GiveAsync(second.Id, projectId, Holds.Manager);

        var answer = await ViewOf(await TakeAsync(client, projectId, manager.Id, Permission.ReadProject));

        Assert.Empty(answer.Projects);
        Assert.DoesNotContain((await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!, project => project.Id == projectId);
    }

    [Fact]
    public async Task ADisabledManagerDoesNotCountAsTheOneWhoStays()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var second = await factory.AddUserAsync(Password, isActive: false);
        await factory.GiveAsync(second.Id, projectId, Holds.Manager);

        Assert.Equal("lastManager", await CodeOf(await TakeAsync(client, projectId, manager.Id, Permission.ManageUsers)));
    }

    [Fact]
    public async Task TheLastManagerCannotBeDisabled()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var refused = await client.PostAsync($"/users/{manager.Id}/disable", null);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("lastManager", await CodeOf(refused));
        Assert.True((await ListAsync(client, projectId))!.Single(user => user.Id == manager.Id).IsActive);
    }

    [Fact]
    public async Task AManagerResetsAndDisablesSomeoneOnlyInTheirProject()
    {
        var (_, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Field);

        var reset = await ViewOf(await ResetAsync(client, user.Id));
        var disabled = await ViewOf(await client.PostAsync($"/users/{user.Id}/disable", null));

        Assert.True(reset.MustChangePassword);
        Assert.False(disabled.IsActive);
    }

    [Fact]
    public async Task AManagerCannotResetSomeoneInAnotherProject()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var shared = await factory.AddUserAsync(Password);
        await factory.GiveAsync(shared.Id, projectId, Holds.Field);
        await factory.GiveAsync(shared.Id, otherProjectId, Holds.Field);
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
    public async Task AManagerCannotTakeOverTheAdmin()
    {
        var (_, client, projectId) = await ManagerAsync();
        var admin = await factory.AddUserAsync(Password, isAdmin: true);

        var added = await AddExistingAsync(client, projectId, admin.Username);
        Assert.Equal(HttpStatusCode.NotFound, added.StatusCode);
        Assert.Equal("unknownUser", await CodeOf(added));

        await factory.GiveAsync(admin.Id, projectId, Holds.Reader);
        var reset = await ResetAsync(client, admin.Id);
        Assert.Equal(HttpStatusCode.Forbidden, reset.StatusCode);
        Assert.Equal("otherProject", await CodeOf(reset));
        Assert.Equal("otherProject", await CodeOf(await client.PostAsync($"/users/{admin.Id}/disable", null)));
        Assert.Equal("otherProject", await CodeOf(await client.PostAsJsonAsync($"/users/{admin.Id}", new EditUserRequest("x", ""))));
        Assert.Equal(HttpStatusCode.OK, (await SignInAsync(admin.Username, Password)).StatusCode);
    }

    [Fact]
    public async Task ThePeopleOfAProjectAreListedActiveFirstByName()
    {
        var (_, client, projectId) = await ManagerAsync();
        var disabled = await factory.AddUserAsync(Password, isActive: false);
        var active = await factory.AddUserAsync(Password);
        await factory.GiveAsync(disabled.Id, projectId, Holds.Reader);
        await factory.GiveAsync(active.Id, projectId, Holds.Reader);

        var listed = (await ListAsync(client, projectId))!;

        Assert.True(listed.FindIndex(user => user.Id == active.Id) < listed.FindIndex(user => user.Id == disabled.Id));
    }

    [Fact]
    public async Task SomeoneWhoManagesBothProjectsIsNotHeldBack()
    {
        var (_, client, projectId) = await ManagerAsync();
        var otherProjectId = await StartProjectAsync(client);
        var shared = await factory.AddUserAsync(Password);
        await factory.GiveAsync(shared.Id, projectId, Holds.Field);
        await factory.GiveAsync(shared.Id, otherProjectId, Holds.Field);
        Assert.True((await ViewOf(await ResetAsync(client, shared.Id))).MustChangePassword);
    }

    [Fact]
    public async Task EveryPermissionChangeIsInTheHistory()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Reader);
        await GiveAsync(client, projectId, user.Id, Permission.ApproveEntries);
        var heldId = Guid.Empty;
        await factory.WithDatabaseAsync(async db => heldId = await db.ProjectPermissions.Where(row => row.UserId == user.Id && row.Permission == Permission.ApproveEntries).Select(row => row.Id).SingleAsync());
        await TakeAsync(client, projectId, user.Id, Permission.ApproveEntries);

        var trail = await factory.HistoryOfAsync(heldId);

        Assert.Equal([HistoryAction.AssignProject, HistoryAction.RemoveProject], trail.Select(record => record.Action));
        Assert.All(trail, record => Assert.Equal(manager.Id, record.UserId));
        Assert.Equal(Permission.ApproveEntries, (Permission)JsonNode.Parse(trail[0].After)!["Permission"]!.GetValue<int>());
        Assert.Equal("{}", trail[1].After.Replace(" ", ""));
    }

    [Fact]
    public async Task ANewLoginIsInTheHistoryWithNoPasswordHash()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var username = NewUsername();

        var created = await ViewOf(await CreateAsync(client, projectId, username));

        var trail = await factory.HistoryOfAsync(created.Id);
        var record = Assert.Single(trail);
        Assert.Equal((HistoryAction.Create, manager.Id), (record.Action, record.UserId));
        Assert.Equal(username, JsonNode.Parse(record.After)!["Username"]!.GetValue<string>());
        Assert.DoesNotContain("PasswordHash", record.Before + record.After);
    }
}
