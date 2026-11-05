using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class RegistrationTests(MigratedApiFactory factory)
{
    private const string Password = "Registration-1";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private static string NewUsername() => $"user-{Guid.CreateVersion7():N}";

    private static RegisterProjectRequest Request(string username, string name = "غصون 8", List<string>? units = null, string displayName = "ريم", string mobile = "0500000000", string password = Password) =>
        new(name, new DateOnly(2026, 11, 5), units ?? ["الفلة الغربية"], displayName, username, mobile, password);

    private Task<HttpResponseMessage> RegisterAsync(RegisterProjectRequest request) => factory.CreateClient().PostAsJsonAsync("/projects/register", request);

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static async Task<ProjectView> ProjectOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<ProjectView>(Json))!;
    }

    private async Task<(User User, HttpClient Client)> AdminAsync()
    {
        var user = await factory.AddUserAsync(Password, isAdmin: true);
        return (user, await factory.ClientSignedInAsAsync(user, Password));
    }

    /// <summary>A manager who registered from the sign-in page, signed in, and the project waiting for approval.</summary>
    private async Task<(string Username, HttpClient Client, Guid ProjectId)> RegisteredAsync()
    {
        var username = NewUsername();
        var project = await ProjectOf(await RegisterAsync(Request(username)));
        return (username, await SignedInAsync(username), project.Id);
    }

    private async Task<HttpClient> SignedInAsync(string username)
    {
        var client = factory.CreateClient();
        var signIn = await client.PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, Password));
        var session = await signIn.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(Json);
        client.DefaultRequestHeaders.Authorization = new("Bearer", session!.AccessToken);
        return client;
    }

    private static async Task<ProjectView> ListedAsync(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single(listed => listed.Id == projectId);

    private static Task<HttpResponseMessage> RejectAsync(HttpClient client, Guid projectId, string reason = "الاسم مكرر") =>
        client.PostAsJsonAsync($"/projects/{projectId}/reject", new RejectProjectRequest(reason));

    [Fact]
    public async Task ARegisteredProjectHasItsManager()
    {
        var username = NewUsername();

        var project = await ProjectOf(await RegisterAsync(Request($"  {username}  ", "  غصون 8  ", [" الفلة الغربية "], "  ريم  ", " 0500000000 ")));

        Assert.Equal(("غصون 8", ProjectStatus.PendingApproval), (project.Name, project.Status));
        Assert.Equal(["الفلة الغربية"], project.Units.Select(unit => unit.Name));
        Assert.Equal(["users.roleManager"], project.Roles);
        var client = factory.CreateClient();
        var session = (await (await client.PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, Password))).Content.ReadFromJsonAsync<Session>(Json))!;
        Assert.False(session.MustChangePassword);
    }

    [Theory]
    [InlineData("   ", "ريم", "0500000000", Password, "invalidName")]
    [InlineData("غصون 8", "   ", "0500000000", Password, "invalidName")]
    [InlineData("غصون 8", "ريم", "012345678901234567890", Password, "invalidMobile")]
    [InlineData("غصون 8", "ريم", "٠٥٠٠٠٠٠٠٠٠", Password, "invalidMobile")]
    [InlineData("غصون 8", "ريم", "0500000000", "Short-1", "weakPassword")]
    public async Task ABadFieldCreatesNothing(string name, string displayName, string mobile, string password, string code)
    {
        var username = NewUsername();

        var refused = await RegisterAsync(Request(username, name, null, displayName, mobile, password));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal(code, await CodeOf(refused));
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, password))).StatusCode);
    }

    [Fact]
    public async Task ABadUnitCreatesNothing()
    {
        var username = NewUsername();

        var refused = await RegisterAsync(Request(username, units: ["   "]));

        Assert.Equal("invalidName", await CodeOf(refused));
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, Password))).StatusCode);
    }

    [Fact]
    public async Task AUsernameAlreadyTakenCreatesNothing()
    {
        var existing = await factory.AddUserAsync(Password);
        var (_, admin) = await AdminAsync();
        var name = $"project-{Guid.CreateVersion7()}";

        var refused = await RegisterAsync(Request(existing.Username.ToUpperInvariant(), name));

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("usernameTaken", await CodeOf(refused));
        var waiting = (await admin.GetFromJsonAsync<List<RegisteredProjectView>>("/projects/registered", Json))!;
        Assert.DoesNotContain(waiting, project => project.Name == name);
    }

    [Fact]
    public async Task SomeoneSignedInRegistersWithTheirOwnLogin()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);

        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 9", new DateOnly(2026, 11, 5), []));

        var project = await ProjectOf(created);
        Assert.Equal(ProjectStatus.PendingApproval, project.Status);
        Assert.Equal(["users.roleManager"], project.Roles);
        Assert.Empty(project.Permissions);
    }

    [Fact]
    public async Task AWaitingProjectGivesNoPermission()
    {
        var (_, client, projectId) = await RegisteredAsync();

        var listed = await ListedAsync(client, projectId);

        Assert.Equal(ProjectStatus.PendingApproval, listed.Status);
        Assert.Empty(listed.Permissions);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{projectId}/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{projectId}/close", null)).StatusCode);
    }

    [Fact]
    public async Task AnAdminApprovesAProject()
    {
        var (_, client, projectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();

        var approved = await admin.PostAsync($"/projects/{projectId}/approve", null);

        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);
        var listed = await ListedAsync(client, projectId);
        Assert.Equal(ProjectStatus.Running, listed.Status);
        Assert.Contains(Permission.ManageUsers, listed.Permissions);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/projects/{projectId}/users")).StatusCode);
    }

    [Fact]
    public async Task OnlyTheAdminApproves()
    {
        var (_, client, projectId) = await RegisteredAsync();
        var (_, finance, otherProjectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();
        await admin.PostAsync($"/projects/{otherProjectId}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{projectId}/approve", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await finance.PostAsync($"/projects/{projectId}/approve", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await RejectAsync(finance, projectId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await finance.GetAsync("/projects/registered")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/projects/registered")).StatusCode);
    }

    [Fact]
    public async Task TheAdminSeesWhatIsNeededToDecide()
    {
        var username = NewUsername();
        var name = $"project-{Guid.CreateVersion7()}";
        await ProjectOf(await RegisterAsync(Request(username, name)));
        var (_, admin) = await AdminAsync();

        var waiting = (await admin.GetFromJsonAsync<List<RegisteredProjectView>>("/projects/registered", Json))!.Single(project => project.Name == name);

        Assert.Equal(new DateOnly(2026, 11, 5), waiting.StartDate);
        Assert.Equal(["الفلة الغربية"], waiting.Units);
        Assert.Equal(("ريم", username, "0500000000"), (waiting.ManagerName, waiting.ManagerUsername, waiting.ManagerMobile));
    }

    [Fact]
    public async Task ADecidedProjectLeavesTheAdminsList()
    {
        var (_, _, projectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();

        await admin.PostAsync($"/projects/{projectId}/approve", null);

        Assert.DoesNotContain((await admin.GetFromJsonAsync<List<RegisteredProjectView>>("/projects/registered", Json))!, project => project.Id == projectId);
    }

    [Fact]
    public async Task TheAdminSeesNothingInsideAProject()
    {
        var (_, client, projectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();
        await admin.PostAsync($"/projects/{projectId}/approve", null);

        Assert.DoesNotContain((await admin.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!, project => project.Id == projectId);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync($"/projects/{projectId}/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.PostAsync($"/projects/{projectId}/close", null)).StatusCode);
        var managerId = (await ListUsersAsync(client, projectId)).Single().Id;
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.PostAsJsonAsync($"/users/{managerId}/reset-password", new ResetPasswordRequest("Temporary-1"))).StatusCode);
    }

    private static async Task<List<UserView>> ListUsersAsync(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<UserView>>($"/projects/{projectId}/users", Json))!;

    [Fact]
    public async Task ARejectionSaysWhy()
    {
        var (_, client, projectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();

        var rejected = await RejectAsync(admin, projectId, "  الاسم مكرر  ");

        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        var listed = await ListedAsync(client, projectId);
        Assert.Equal((ProjectStatus.Rejected, "الاسم مكرر"), (listed.Status, listed.RejectionReason));
        Assert.Empty(listed.Permissions);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{projectId}/users")).StatusCode);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ARejectionNeedsAReasonOfAtMost500Characters(string? reason)
    {
        var (_, _, projectId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();

        var refused = await RejectAsync(admin, projectId, reason ?? new string('x', Project.ReasonMaxLength + 1));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidReason", await CodeOf(refused));
    }

    [Fact]
    public async Task AProjectIsDecidedOnce()
    {
        var (_, _, approvedId) = await RegisteredAsync();
        var (_, _, rejectedId) = await RegisteredAsync();
        var (_, admin) = await AdminAsync();
        await admin.PostAsync($"/projects/{approvedId}/approve", null);
        await RejectAsync(admin, rejectedId);

        Assert.Equal("notPending", await CodeOf(await admin.PostAsync($"/projects/{approvedId}/approve", null)));
        Assert.Equal("notPending", await CodeOf(await RejectAsync(admin, approvedId)));
        Assert.Equal("notPending", await CodeOf(await admin.PostAsync($"/projects/{rejectedId}/approve", null)));
        Assert.Equal(HttpStatusCode.Conflict, (await RejectAsync(admin, rejectedId)).StatusCode);
    }

    [Fact]
    public async Task AnUnknownProjectIsNotFound()
    {
        var (_, admin) = await AdminAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await admin.PostAsync($"/projects/{Guid.CreateVersion7()}/approve", null)).StatusCode);
    }

    [Fact]
    public async Task EveryDecisionIsInTheAuditTrail()
    {
        var (_, client, approvedId) = await RegisteredAsync();
        var (_, _, rejectedId) = await RegisteredAsync();
        var (adminUser, admin) = await AdminAsync();
        var manager = (await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!;
        Assert.Single(manager);

        await admin.PostAsync($"/projects/{approvedId}/approve", null);
        await RejectAsync(admin, rejectedId);

        var approved = await factory.AuditTrailOfAsync(approvedId);
        Assert.Equal([AuditAction.RegisterProject, AuditAction.ApproveProject], approved.Select(record => record.Action));
        Assert.Equal(adminUser.Id, approved[1].UserId);
        var rejected = await factory.AuditTrailOfAsync(rejectedId);
        Assert.Equal([AuditAction.RegisterProject, AuditAction.RejectProject], rejected.Select(record => record.Action));
        Assert.Equal("الاسم مكرر", JsonNode.Parse(rejected[1].After)!["RejectionReason"]!.GetValue<string>());
    }

    [Fact]
    public async Task ANewLoginIsInTheAuditTrailWithNoPassword()
    {
        var username = NewUsername();
        await ProjectOf(await RegisterAsync(Request(username)));
        var client = await SignedInAsync(username);
        var project = (await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single();
        var (_, admin) = await AdminAsync();
        await admin.PostAsync($"/projects/{project.Id}/approve", null);
        var userId = (await ListUsersAsync(client, project.Id)).Single().Id;

        var record = (await factory.AuditTrailOfAsync(userId)).First();

        Assert.Equal((AuditAction.Create, userId), (record.Action, record.UserId));
        Assert.DoesNotContain("PasswordHash", record.Before + record.After);
    }

    [Fact]
    public async Task TooManyTriesAreRefused()
    {
        using var limited = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Registration:PermitsPerHour"] = "2" })));
        var client = limited.CreateClient();

        var first = await client.PostAsJsonAsync("/projects/register", Request(NewUsername()));
        var second = await client.PostAsJsonAsync("/projects/register", Request(NewUsername()));
        var third = await client.PostAsJsonAsync("/projects/register", Request(NewUsername()));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }
}
