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

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, string name) =>
        client.PostAsJsonAsync("/projects", new CreateProjectRequest(name, new DateOnly(2026, 11, 1)));

    [Fact]
    public async Task AnOwnerCreatesAProjectAndAddsItsUnitsInOrder()
    {
        var owner = await OwnerAsync();

        var project = await ProjectOf(await CreateAsync(owner, "  غصون 7  "));
        await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest("الفلة الغربية"));
        var withUnits = await ProjectOf(await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest("الفلة الجنوبية")));

        Assert.Equal("غصون 7", project.Name);
        Assert.Equal(ProjectStatus.Running, project.Status);
        Assert.Equal(["الفلة الغربية", "الفلة الجنوبية"], withUnits.Units.Select(unit => unit.Name));
        var listed = (await owner.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single(listed => listed.Id == project.Id);
        Assert.Equal((withUnits.Name, withUnits.StartDate, withUnits.Status), (listed.Name, listed.StartDate, listed.Status));
        Assert.Equal(withUnits.Units, listed.Units);
    }

    [Fact]
    public async Task AClosedProjectIsReadOnlyAndStaysListed()
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 6"));

        var closed = await ProjectOf(await owner.PostAsync($"/projects/{project.Id}/close", null));
        var newUnit = await owner.PostAsJsonAsync($"/projects/{project.Id}/units", new AddUnitRequest("الفلة الشرقية"));
        var closedAgain = await owner.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal(ProjectStatus.Closed, closed.Status);
        Assert.Equal(HttpStatusCode.Conflict, newUnit.StatusCode);
        Assert.Equal("projectClosed", await CodeOf(newUnit));
        Assert.Equal("projectClosed", await CodeOf(closedAgain));
        var listed = (await owner.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!.Single(listed => listed.Id == project.Id);
        Assert.Equal(ProjectStatus.Closed, listed.Status);
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task AProjectOrAUnitNeedsANameOfAtMost100Characters(string name)
    {
        var owner = await OwnerAsync();
        var project = await ProjectOf(await CreateAsync(owner, "غصون 8"));

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
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateAsync(user, "غصون 9")).StatusCode);
    }
}
