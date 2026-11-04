using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ProjectsTests(MigratedApiFactory factory)
{
    private const string Password = "Projects-pass-1";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    private async Task<HttpClient> OwnerAsync() => await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isOwner: true), Password);

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

    [Fact]
    public async Task AnOwnerCreatesAProjectWithItsUnitsInOrder()
    {
        var owner = await OwnerAsync();

        var project = await ProjectOf(await CreateAsync(owner, "  غصون 7  ", " الفلة الغربية ", "الفلة الجنوبية"));

        Assert.Equal("غصون 7", project.Name);
        Assert.Equal(ProjectStatus.Running, project.Status);
        Assert.Equal(["الفلة الغربية", "الفلة الجنوبية"], project.Units.Select(unit => unit.Name));
        var listed = (await ListAsync(owner))!.Single(listed => listed.Id == project.Id);
        Assert.Equal((project.Name, project.StartDate, project.Status), (listed.Name, listed.StartDate, listed.Status));
        Assert.Equal(project.Units, listed.Units);
    }

    [Fact]
    public async Task ABadUnitNameCreatesNothing()
    {
        var owner = await OwnerAsync();
        var name = $"project-{Guid.CreateVersion7()}";

        var response = await CreateAsync(owner, name, "الفلة الغربية", "   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalidName", await CodeOf(response));
        Assert.DoesNotContain((await ListAsync(owner))!, listed => listed.Name == name);
    }

    [Theory]
    [InlineData("""{ "startDate": "2026-11-01", "units": [] }""")]
    [InlineData("""{ "name": null, "startDate": "2026-11-01", "units": [] }""")]
    [InlineData("""{ "name": "غصون 8", "units": [] }""")]
    [InlineData("""{ "name": "غصون 8", "startDate": "2026-11-01" }""")]
    public async Task ARequestMissingAFieldIsABadRequest(string body)
    {
        var owner = await OwnerAsync();

        var response = await owner.PostAsync("/projects", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AUnitIsAddedAtTheEnd()
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 9", "الفلة الغربية"));

        var withUnit = await ProjectOf(await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest("الفلة الجنوبية")));

        Assert.Equal(["الفلة الغربية", "الفلة الجنوبية"], withUnit.Units.Select(unit => unit.Name));
    }

    [Fact]
    public async Task AFinishedProjectIsReadOnlyAndStaysListed()
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 6"));

        var finished = await ProjectOf(await owner.PostAsync($"/projects/{project.Id}/close", null));
        var newUnit = await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest("الفلة الشرقية"));
        var closedAgain = await owner.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal(ProjectStatus.Finished, finished.Status);
        Assert.Equal(HttpStatusCode.Conflict, newUnit.StatusCode);
        Assert.Equal("projectFinished", await CodeOf(newUnit));
        Assert.Equal("projectFinished", await CodeOf(closedAgain));
        Assert.Equal(ProjectStatus.Finished, (await ListAsync(owner))!.Single(listed => listed.Id == project.Id).Status);
    }

    [Fact]
    public async Task TheStatusTravelsAsItsGlossaryName()
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 4"));

        var closed = await owner.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal("finished", (await closed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task AProjectOrAUnitNeedsANameOfAtMost100Characters(string name)
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 3"));

        var refusedProject = await CreateAsync(owner, name);
        var refusedUnit = await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest(name));

        Assert.Equal(HttpStatusCode.BadRequest, refusedProject.StatusCode);
        Assert.Equal("invalidName", await CodeOf(refusedProject));
        Assert.Equal(HttpStatusCode.BadRequest, refusedUnit.StatusCode);
        Assert.Equal("invalidName", await CodeOf(refusedUnit));
    }

    [Fact]
    public async Task AnUnknownProjectIsNotFound()
    {
        var owner = await OwnerAsync();

        var response = await owner.PostAsync($"/projects/{Guid.CreateVersion7()}/close", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OnlyAnOwnerManagesProjects()
    {
        var user = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync("/projects")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateAsync(user, "غصون 2")).StatusCode);
    }
}
