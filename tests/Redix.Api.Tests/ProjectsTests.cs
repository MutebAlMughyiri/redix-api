using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ProjectsTests(MigratedApiFactory factory)
{
    private const string Password = "Projects-pass-1";
    private const string Manager = "users.roleManager";
    private const string Finance = "users.roleFinance";
    private const string Field = "users.roleField";
    private const string Reader = "users.roleReader";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    private Task<User> StarterAsync() => factory.AddUserAsync(Password);

    private Task<HttpClient> ClientOfAsync(User user) => factory.ClientSignedInAsAsync(user, Password);

    /// <summary>Someone who registered <paramref name="name"/>, now approved, and also does its finance.</summary>
    private async Task<(HttpClient Client, ProjectView Project)> FinanceOfNewProjectAsync(string name, params List<string> units)
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var project = await factory.ApprovedProjectAsync(client, name, units);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);
        return (client, project);
    }

    private static async Task<ProjectView> ProjectOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<ProjectView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Task<List<ProjectView>?> ListAsync(HttpClient client) => client.GetFromJsonAsync<List<ProjectView>>("/projects", Json);

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, string name, params List<string> units) =>
        client.PostAsJsonAsync("/projects", new CreateProjectRequest(name, new DateOnly(2026, 11, 1), units));

    private static Task<HttpResponseMessage> AddUnitAsync(HttpClient client, Guid projectId, string name = "الفلة الشرقية") =>
        client.PostAsJsonAsync($"/projects/{projectId}/units", new AddUnitRequest(name));

    [Fact]
    public async Task ANewUserHasNoProjects()
    {
        var user = await ClientOfAsync(await factory.AddUserAsync(Password));

        Assert.Empty((await ListAsync(user))!);
    }

    [Fact]
    public async Task AProjectKeepsItsUnitsInOrder()
    {
        var client = await ClientOfAsync(await StarterAsync());

        var project = await factory.ApprovedProjectAsync(client, "  غصون 7  ", " الفلة الغربية ", "الفلة الجنوبية");

        Assert.Equal("غصون 7", project.Name);
        Assert.Equal(ProjectStatus.Running, project.Status);
        Assert.Equal(["الفلة الغربية", "الفلة الجنوبية"], project.Units.Select(unit => unit.Name));
        var listed = (await ListAsync(client))!.Single(listed => listed.Id == project.Id);
        Assert.Equal((project.Name, project.StartDate, project.Status), (listed.Name, listed.StartDate, listed.Status));
        Assert.Equal(project.Units, listed.Units);
    }

    [Fact]
    public async Task WhoeverStartsAProjectManagesIt()
    {
        var client = await ClientOfAsync(await StarterAsync());

        var project = await factory.ApprovedProjectAsync(client, "غصون 7");

        Assert.Equal([Manager], project.Roles);
        Assert.Contains(Permission.ManageUsers, project.Permissions);
        Assert.Contains(Permission.ReadProject, project.Permissions);
    }

    [Fact]
    public async Task ABadUnitNameCreatesNothing()
    {
        var client = await ClientOfAsync(await StarterAsync());
        var name = $"project-{Guid.CreateVersion7()}";

        var response = await CreateAsync(client, name, "الفلة الغربية", "   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalidName", await CodeOf(response));
        Assert.DoesNotContain((await ListAsync(client))!, listed => listed.Name == name);
    }

    [Theory]
    [InlineData("""{ "startDate": "2026-11-01", "units": [] }""")]
    [InlineData("""{ "name": null, "startDate": "2026-11-01", "units": [] }""")]
    [InlineData("""{ "name": "غصون 8", "units": [] }""")]
    [InlineData("""{ "name": "غصون 8", "startDate": "2026-11-01" }""")]
    public async Task ARequestMissingAFieldIsABadRequest(string body)
    {
        var client = await ClientOfAsync(await StarterAsync());

        var response = await client.PostAsync("/projects", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AUnitIsAddedAtTheEnd()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 9", "الفلة الغربية");

        var withUnit = await ProjectOf(await AddUnitAsync(client, project.Id, "الفلة الجنوبية"));

        Assert.Equal(["الفلة الغربية", "الفلة الجنوبية"], withUnit.Units.Select(unit => unit.Name));
    }

    [Fact]
    public async Task AFinishedProjectIsReadOnlyAndStaysListed()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");

        var finished = await ProjectOf(await client.PostAsync($"/projects/{project.Id}/close", null));
        var newUnit = await AddUnitAsync(client, project.Id);
        var closedAgain = await client.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal(ProjectStatus.Finished, finished.Status);
        Assert.Equal(HttpStatusCode.Conflict, newUnit.StatusCode);
        Assert.Equal("projectFinished", await CodeOf(newUnit));
        Assert.Equal("projectFinished", await CodeOf(closedAgain));
        Assert.Equal(ProjectStatus.Finished, (await ListAsync(client))!.Single(listed => listed.Id == project.Id).Status);
    }

    [Fact]
    public async Task TheStatusTravelsAsItsGlossaryName()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 4");

        var closed = await client.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal("finished", (await closed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task AProjectOrAUnitNeedsANameOfAtMost100Characters(string name)
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 3");

        var refusedProject = await CreateAsync(client, name);
        var refusedUnit = await AddUnitAsync(client, project.Id, name);

        Assert.Equal(HttpStatusCode.BadRequest, refusedProject.StatusCode);
        Assert.Equal("invalidName", await CodeOf(refusedProject));
        Assert.Equal(HttpStatusCode.BadRequest, refusedUnit.StatusCode);
        Assert.Equal("invalidName", await CodeOf(refusedUnit));
    }

    [Fact]
    public async Task AUserSeesOnlyTheirProjects()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var theirs = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var other = await factory.ApprovedProjectAsync(starter, "غصون 5");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, theirs.Id, Field);

        var listed = await ListAsync(await ClientOfAsync(user));

        Assert.Equal([theirs.Id], listed!.Select(project => project.Id));
        Assert.DoesNotContain(listed!, project => project.Id == other.Id);
    }

    [Fact]
    public async Task AnotherProjectIsForbidden()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var theirs = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var other = await factory.ApprovedProjectAsync(starter, "غصون 5");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, theirs.Id, Finance);
        var client = await ClientOfAsync(user);

        Assert.Equal(HttpStatusCode.OK, (await AddUnitAsync(client, theirs.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, other.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{other.Id}/close", null)).StatusCode);
    }

    [Fact]
    public async Task AProjectThatDoesNotExistIsForbiddenSoTheAnswerNeverSaysWhichExist()
    {
        var (client, _) = await FinanceOfNewProjectAsync("غصون 6");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{Guid.CreateVersion7()}/close", null)).StatusCode);
    }

    [Fact]
    public async Task PermissionsFollowTheRoleInEachProject()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var financeProject = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var fieldProject = await factory.ApprovedProjectAsync(starter, "غصون 7");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, financeProject.Id, Finance);
        await factory.GiveRoleAsync(user.Id, fieldProject.Id, Field);
        var client = await ClientOfAsync(user);

        var listed = (await ListAsync(client))!;

        Assert.Equal([Finance], listed.Single(project => project.Id == financeProject.Id).Roles);
        Assert.Contains(Permission.ManageProject, listed.Single(project => project.Id == financeProject.Id).Permissions);
        Assert.Equal([Field], listed.Single(project => project.Id == fieldProject.Id).Roles);
        Assert.Equal([Permission.ReadProject], listed.Single(project => project.Id == fieldProject.Id).Permissions);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/projects/{financeProject.Id}/close", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{fieldProject.Id}/close", null)).StatusCode);
    }

    [Theory]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task FieldAndReaderCannotChangeAProject(string role)
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var project = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, project.Id, role);
        var client = await ClientOfAsync(user);

        Assert.Equal([project.Id], (await ListAsync(client))!.Select(listed => listed.Id));
        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, project.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{project.Id}/close", null)).StatusCode);
    }

    [Fact]
    public async Task AManagerChangesAProjectOnlyWithFinanceToo()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6");
        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, project.Id)).StatusCode);

        await factory.GiveRoleAsync(user.Id, project.Id, Finance);

        Assert.Equal(HttpStatusCode.OK, (await AddUnitAsync(client, project.Id)).StatusCode);
        var listed = (await ListAsync(client))!.Single(listed => listed.Id == project.Id);
        Assert.Equal([Manager, Finance], listed.Roles);
    }

    [Fact]
    public async Task ASessionlessRequestIsUnauthorized()
    {
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"/projects/{Guid.CreateVersion7()}/close", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/projects")).StatusCode);
    }

    [Fact]
    public async Task ATemporaryPasswordSessionIsForbidden()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var project = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var user = await factory.AddUserAsync(Password, mustChangePassword: true);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);
        var client = await ClientOfAsync(user);

        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, project.Id)).StatusCode);
    }
}
