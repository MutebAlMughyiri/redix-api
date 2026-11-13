using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(FourthDatabaseCollection.Name)]
public class ProjectsTests(MigratedApiFactory factory)
{
    private const string Password = "Projects-pass-1";
    private const string Manager = "Manager";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";

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
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
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
        Assert.Equal(ProjectStatus.Active, project.Status);
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

        Assert.Equal(ProjectPermission.OfWhoeverStarts.Order(), project.Permissions.Order());
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

        var response = await client.PostAsync("/projects", new StringContent(body, Encoding.UTF8, "application/json"));

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
        await factory.GiveAsync(user.Id, theirs.Id, Holds.Of(Field));

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
        await factory.GiveAsync(user.Id, theirs.Id, Holds.Of(Finance));
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
    public async Task PermissionsFollowWhatIsTickedInEachProject()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var financeProject = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var fieldProject = await factory.ApprovedProjectAsync(starter, "غصون 7");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, financeProject.Id, Holds.Of(Finance));
        await factory.GiveAsync(user.Id, fieldProject.Id, Holds.Of(Field));
        var client = await ClientOfAsync(user);

        var listed = (await ListAsync(client))!;

        Assert.Contains(Permission.ManageProject, listed.Single(project => project.Id == financeProject.Id).Permissions);
        Assert.DoesNotContain(Permission.ManageProject, listed.Single(project => project.Id == fieldProject.Id).Permissions);
        Assert.Contains(Permission.ReadProject, listed.Single(project => project.Id == fieldProject.Id).Permissions);
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
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        var client = await ClientOfAsync(user);

        Assert.Equal([project.Id], (await ListAsync(client))!.Select(listed => listed.Id));
        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, project.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/projects/{project.Id}/close", null)).StatusCode);
    }

    [Fact]
    public async Task WhoeverStartsAProjectChangesItWithNothingMoreTicked()
    {
        var client = await ClientOfAsync(await StarterAsync());
        var project = await factory.ApprovedProjectAsync(client, "غصون 6");

        Assert.Equal(HttpStatusCode.OK, (await AddUnitAsync(client, project.Id)).StatusCode);
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
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        var client = await ClientOfAsync(user);

        Assert.Equal(HttpStatusCode.Forbidden, (await AddUnitAsync(client, project.Id)).StatusCode);
    }

    private Task<HttpResponseMessage> SignInAsync(User user) =>
        factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(user.Username, Password));

    private async Task<Session> SessionOfAsync(User user) => (await (await SignInAsync(user)).Content.ReadFromJsonAsync<Session>(Json))!;

    private static Task<HttpResponseMessage> SetDefaultAsync(HttpClient client, Guid projectId) => client.PostAsync($"/projects/{projectId}/default", null);

    [Fact]
    public async Task NoDefaultProjectAtFirst()
    {
        var session = await SessionOfAsync(await factory.AddUserAsync(Password));

        Assert.Equal("", session.DefaultProjectId);
    }

    [Fact]
    public async Task AC_37_1_ADefaultProjectIsKeptForTheNextSessionWhichOpensIt()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6");

        var answer = await SetDefaultAsync(client, project.Id);

        Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
        Assert.Equal(project.Id.ToString(), (await SessionOfAsync(user)).DefaultProjectId);
    }

    [Fact]
    public async Task ANewDefaultReplacesTheOld()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var first = await factory.ApprovedProjectAsync(client, "غصون 6");
        var second = await factory.ApprovedProjectAsync(client, "غصون 7");
        await SetDefaultAsync(client, first.Id);

        await SetDefaultAsync(client, second.Id);

        Assert.Equal(second.Id.ToString(), (await SessionOfAsync(user)).DefaultProjectId);
    }

    [Fact]
    public async Task RemovingTheDefaultLeavesNoneSoSignInShowsTheList()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6");
        await SetDefaultAsync(client, project.Id);

        var answer = await client.DeleteAsync("/projects/default");

        Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
        Assert.Equal("", (await SessionOfAsync(user)).DefaultProjectId);
    }

    [Fact]
    public async Task ADefaultWithoutAccessIsNotOffered()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var project = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Reader));
        var client = await ClientOfAsync(user);
        await SetDefaultAsync(client, project.Id);
        Assert.Equal(project.Id.ToString(), (await SessionOfAsync(user)).DefaultProjectId);

        await starter.DeleteAsync($"/projects/{project.Id}/users/{user.Id}/permissions/readProject");

        Assert.Equal("", (await SessionOfAsync(user)).DefaultProjectId);
    }

    [Fact]
    public async Task AProjectWithoutAccessCannotBeTheDefault()
    {
        var starter = await ClientOfAsync(await StarterAsync());
        var project = await factory.ApprovedProjectAsync(starter, "غصون 6");
        var user = await factory.AddUserAsync(Password);
        var client = await ClientOfAsync(user);

        Assert.Equal(HttpStatusCode.Forbidden, (await SetDefaultAsync(client, project.Id)).StatusCode);

        Assert.Equal("", (await SessionOfAsync(user)).DefaultProjectId);
    }

    [Fact]
    public async Task AWaitingProjectCannotBeTheDefault()
    {
        var client = await ClientOfAsync(await StarterAsync());
        var waiting = await ProjectOf(await CreateAsync(client, "غصون 8"));

        Assert.Equal(HttpStatusCode.Forbidden, (await SetDefaultAsync(client, waiting.Id)).StatusCode);
    }

    [Fact]
    public async Task ADefaultIsOneUsersAlone()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6");
        var other = await factory.AddUserAsync(Password);
        await factory.GiveAsync(other.Id, project.Id, Holds.Of(Reader));

        await SetDefaultAsync(client, project.Id);

        Assert.Equal("", (await SessionOfAsync(other)).DefaultProjectId);
    }

    [Fact]
    public async Task R216_ADefaultProjectIsARowKeyedByItsUserSoANewDefaultChangesThatRow()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var first = await factory.ApprovedProjectAsync(client, "غصون 6");
        var second = await factory.ApprovedProjectAsync(client, "غصون 7");

        await SetDefaultAsync(client, first.Id);
        await SetDefaultAsync(client, second.Id);

        await factory.WithDatabaseAsync(async db => Assert.Equal(second.Id, (await db.UserDefaultProjects.SingleAsync(row => row.UserId == user.Id)).ProjectId));
    }

    [Fact]
    public async Task R216_TheDatabaseRefusesASecondDefaultProjectForOneUserByItsKey()
    {
        var user = await StarterAsync();
        var client = await ClientOfAsync(user);
        var first = await factory.ApprovedProjectAsync(client, "غصون 6");
        var second = await factory.ApprovedProjectAsync(client, "غصون 7");
        await SetDefaultAsync(client, first.Id);

        await factory.WithDatabaseAsync(async db =>
        {
            db.UserDefaultProjects.Add(new UserDefaultProject { UserId = user.Id, ProjectId = second.Id });

            var refused = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

            Assert.Contains("PK_UserDefaultProjects", refused.InnerException!.Message);
        });
    }

    [Fact]
    public async Task AFinishedProjectCanBeTheDefault()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 5");
        await client.PostAsync($"/projects/{project.Id}/close", null);

        Assert.Equal(HttpStatusCode.OK, (await SetDefaultAsync(client, project.Id)).StatusCode);
    }

    private static JsonElement ValuesOf(string values) => JsonDocument.Parse(values).RootElement;

    [Fact]
    public async Task CreatingAProjectIsInTheHistoryWithItsName()
    {
        var user = await StarterAsync();

        var project = await ProjectOf(await CreateAsync(await ClientOfAsync(user), "غصون 6"));

        var record = Assert.Single(await factory.HistoryOfAsync(project.Id));
        Assert.Equal((HistoryAction.RegisterProject, user.Id, "Projects"), (record.Action, record.UserId, record.TableName));
        Assert.Equal("غصون 6", ValuesOf(record.After).GetProperty("Name").GetString());
    }

    private static Task<HttpResponseMessage> DescribeAsync(HttpClient client, Guid projectId, string description) =>
        client.PutAsJsonAsync($"/projects/{projectId}/description", new DescriptionRequest(description));

    [Fact]
    public async Task More_info_ADescriptionIsKeptForEveryoneInTheProjectAndInTheHistory()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");
        var reader = await factory.AddUserAsync(Password);
        await factory.GiveAsync(reader.Id, project.Id, Holds.Of(Reader));

        var answer = await DescribeAsync(client, project.Id, "  فيلتان سكنيتان في حي النرجس.\nالتسليم ديسمبر 2027.  ");

        Assert.Equal("فيلتان سكنيتان في حي النرجس.\nالتسليم ديسمبر 2027.", (await ProjectOf(answer)).Description);
        Assert.Equal("فيلتان سكنيتان في حي النرجس.\nالتسليم ديسمبر 2027.", (await ListAsync(await ClientOfAsync(reader)))!.Single(each => each.Id == project.Id).Description);
        var record = (await factory.HistoryOfAsync(project.Id)).Last();
        Assert.Equal((HistoryAction.Update, "فيلتان سكنيتان في حي النرجس.\nالتسليم ديسمبر 2027."), (record.Action, ValuesOf(record.After).GetProperty("Description").GetString()));
    }

    [Fact]
    public async Task More_info_ADescriptionOf2000CharactersIsKeptAndOneLongerIsRefused()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");

        (await DescribeAsync(client, project.Id, new string('و', Project.DescriptionMaxLength))).EnsureSuccessStatusCode();
        var refused = await DescribeAsync(client, project.Id, new string('و', Project.DescriptionMaxLength + 1));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidDescription", await CodeOf(refused));
        Assert.Equal(Project.DescriptionMaxLength, (await ListAsync(client))!.Single(each => each.Id == project.Id).Description.Length);
    }

    [Fact]
    public async Task More_info_AnEmptyDescriptionClearsIt()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");
        await DescribeAsync(client, project.Id, "فيلتان");

        (await DescribeAsync(client, project.Id, "   ")).EnsureSuccessStatusCode();

        Assert.Equal("", (await ListAsync(client))!.Single(each => each.Id == project.Id).Description);
    }

    [Fact]
    public async Task More_info_OnlyWhoeverManagesTheProjectDescribesIt()
    {
        var (_, project) = await FinanceOfNewProjectAsync("غصون 6");
        var reader = await factory.AddUserAsync(Password);
        await factory.GiveAsync(reader.Id, project.Id, Holds.Of(Reader));

        var refused = await DescribeAsync(await ClientOfAsync(reader), project.Id, "فيلتان");

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
    }

    [Fact]
    public async Task More_info_AFinishedProjectKeepsItsDescription()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");
        await DescribeAsync(client, project.Id, "فيلتان");
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        var refused = await DescribeAsync(client, project.Id, "ثلاث فلل");

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("projectFinished", await CodeOf(refused));
        Assert.Equal("فيلتان", (await ListAsync(client))!.Single(each => each.Id == project.Id).Description);
    }

    [Fact]
    public async Task More_info_ADescriptionWrittenWhenTheProjectIsMadeIsKept()
    {
        var client = await ClientOfAsync(await StarterAsync());

        var project = await ProjectOf(await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { Description = "ثلاث فلل في حي الياسمين." }));

        Assert.Equal("ثلاث فلل في حي الياسمين.", project.Description);
    }

    [Fact]
    public async Task More_info_AProjectMadeWithADescriptionTooLongIsNotMade()
    {
        var client = await ClientOfAsync(await StarterAsync());
        var name = $"project-{Guid.CreateVersion7()}";

        var refused = await client.PostAsJsonAsync("/projects", new CreateProjectRequest(name, new DateOnly(2026, 11, 1), []) { Description = new string('و', Project.DescriptionMaxLength + 1) });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidDescription", await CodeOf(refused));
        Assert.DoesNotContain((await ListAsync(client))!, each => each.Name == name);
    }

    [Fact]
    public async Task AddingAUnitIsInTheHistoryWithItsName()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");

        var unit = (await ProjectOf(await AddUnitAsync(client, project.Id))).Units.Single();

        var record = Assert.Single(await factory.HistoryOfAsync(unit.Id));
        Assert.Equal((HistoryAction.Create, "Units"), (record.Action, record.TableName));
        Assert.Equal("الفلة الشرقية", ValuesOf(record.After).GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ClosingAProjectIsInTheHistoryWithItsStatusOldAndNew()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");

        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        var record = (await factory.HistoryOfAsync(project.Id)).Last();
        Assert.Equal(HistoryAction.CloseProject, record.Action);
        Assert.Equal((int)ProjectStatus.Active, ValuesOf(record.Before).GetProperty("Status").GetInt32());
        Assert.Equal((int)ProjectStatus.Finished, ValuesOf(record.After).GetProperty("Status").GetInt32());
    }

    [Fact]
    public async Task BR_35_AProjectWithEntriesWaitingForApprovalCannotBeClosed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase());
        await ledger.SetStatusAsync(entry.Id, EntryStatus.PendingApproval);

        var refused = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("pendingEntries", await CodeOf(refused));
        Assert.Equal(ProjectStatus.Active, (await ListAsync(ledger.Client))!.Single(project => project.Id == ledger.ProjectId).Status);
    }

    [Theory]
    [InlineData(EntryStatus.Draft)]
    [InlineData(EntryStatus.Approved)]
    [InlineData(EntryStatus.Rejected)]
    public async Task BR_35_OnlyEntriesWaitingForApprovalHoldAProjectOpen(EntryStatus status)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase());
        await ledger.SetStatusAsync(entry.Id, status);

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task BR_35_AnEntryWaitingInAnotherProjectHoldsNothingOpen()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase());
        await ledger.SetStatusAsync(entry.Id, EntryStatus.PendingApproval);
        var (client, project) = await FinanceOfNewProjectAsync("غصون 7");

        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("الفلة الغربية")]
    [InlineData("  الفلة الغربية  ")]
    public async Task BR_37_AUnitsNameCannotRepeatInItsProjectComparedTrimmed(string name)
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية");

        var refused = await AddUnitAsync(client, project.Id, name);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("unitNameTaken", await CodeOf(refused));
    }

    [Fact]
    public async Task BR_37_ANewProjectCannotRepeatAUnitsNameAndSavesNothing()
    {
        var client = await ClientOfAsync(await StarterAsync());

        var refused = await CreateAsync(client, "غصون 6", "الفلة الغربية", " الفلة الغربية ");

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("unitNameTaken", await CodeOf(refused));
        Assert.Empty((await ListAsync(client))!);
    }

    [Fact]
    public async Task BR_37_ProjectsMayRepeatANameAndEachTheirUnitsNames()
    {
        var client = await ClientOfAsync(await StarterAsync());

        await ProjectOf(await CreateAsync(client, "غصون 6", "الفلة الغربية"));
        await ProjectOf(await CreateAsync(client, "غصون 6", "الفلة الغربية"));

        Assert.Equal(2, (await ListAsync(client))!.Count);
    }

    private static Task<HttpResponseMessage> RenameAsync(HttpClient client, Guid projectId, string name) =>
        client.PutAsJsonAsync($"/projects/{projectId}", new { name });

    private static Task<HttpResponseMessage> RenameUnitAsync(HttpClient client, Guid projectId, Guid unitId, string name) =>
        client.PutAsJsonAsync($"/projects/{projectId}/units/{unitId}", new { name });

    private static Task<HttpResponseMessage> DeleteUnitAsync(HttpClient client, Guid projectId, Guid unitId) =>
        client.DeleteAsync($"/projects/{projectId}/units/{unitId}");

    [Fact]
    public async Task BR_38_ARunningProjectIsRenamedAndTheHistoryShowsBothNames()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");

        var renamed = await ProjectOf(await RenameAsync(client, project.Id, "  غصون السادس  "));

        Assert.Equal("غصون السادس", renamed.Name);
        var record = (await factory.HistoryOfAsync(project.Id)).Last();
        Assert.Equal(HistoryAction.Update, record.Action);
        Assert.Equal("غصون 6", ValuesOf(record.Before).GetProperty("Name").GetString());
        Assert.Equal("غصون السادس", ValuesOf(record.After).GetProperty("Name").GetString());
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task BR_38_AProjectOrAUnitIsRenamedOnlyToAnAcceptableName(string name)
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية");

        Assert.Equal("invalidName", await CodeOf(await RenameAsync(client, project.Id, name)));
        Assert.Equal("invalidName", await CodeOf(await RenameUnitAsync(client, project.Id, project.Units[0].Id, name)));
    }

    [Fact]
    public async Task BR_38_AUnitIsRenamedInPlaceAndTheHistoryShowsBothNames()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية", "الفلة الشرقية");

        var renamed = await ProjectOf(await RenameUnitAsync(client, project.Id, project.Units[0].Id, " الفلة الجنوبية "));

        Assert.Equal(["الفلة الجنوبية", "الفلة الشرقية"], renamed.Units.Select(unit => unit.Name));
        var record = (await factory.HistoryOfAsync(project.Units[0].Id)).Last();
        Assert.Equal((HistoryAction.Update, "Units"), (record.Action, record.TableName));
        Assert.Equal("الفلة الغربية", ValuesOf(record.Before).GetProperty("Name").GetString());
        Assert.Equal("الفلة الجنوبية", ValuesOf(record.After).GetProperty("Name").GetString());
    }

    [Fact]
    public async Task BR_37_ARenameCannotTakeAnotherUnitsNameAndMayKeepItsOwn()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية", "الفلة الشرقية");

        var refused = await RenameUnitAsync(client, project.Id, project.Units[0].Id, " الفلة الشرقية ");

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("unitNameTaken", await CodeOf(refused));
        await ProjectOf(await RenameUnitAsync(client, project.Id, project.Units[0].Id, "الفلة الغربية"));
    }

    [Fact]
    public async Task BR_38_AUnitNothingUsesIsDeletedAndTheHistoryKeepsWhatItWas()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية", "الفلة الشرقية");

        var left = await ProjectOf(await DeleteUnitAsync(client, project.Id, project.Units[0].Id));

        Assert.Equal(["الفلة الشرقية"], left.Units.Select(unit => unit.Name));
        var record = (await factory.HistoryOfAsync(project.Units[0].Id)).Last();
        Assert.Equal((HistoryAction.Delete, "Units"), (record.Action, record.TableName));
        Assert.Equal("الفلة الغربية", ValuesOf(record.Before).GetProperty("Name").GetString());
        Assert.Equal("{}", record.After);
    }

    [Fact]
    public async Task AUnitAddedAfterADeletionStillGoesLast()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية", "الفلة الشرقية", "الفلة الشمالية");
        await ProjectOf(await DeleteUnitAsync(client, project.Id, project.Units[0].Id));

        var withUnit = await ProjectOf(await AddUnitAsync(client, project.Id, "الفلة الجنوبية"));

        Assert.Equal(["الفلة الشرقية", "الفلة الشمالية", "الفلة الجنوبية"], withUnit.Units.Select(unit => unit.Name));
        Assert.Equal(["الفلة الشرقية", "الفلة الشمالية", "الفلة الجنوبية"], (await ListAsync(client))!.Single(row => row.Id == project.Id).Units.Select(unit => unit.Name));
    }

    [Fact]
    public async Task BR_38_ExpensesAndContractsBelongToTheProjectSoAUnitIsDeletedWhateverTheyHold()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase());
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, ledger.ItemId, 1000, false), Json)).EnsureSuccessStatusCode();

        (await DeleteUnitAsync(ledger.Client, ledger.ProjectId, ledger.Project.Units[0].Id)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task BR_38_AFinishedProjectRefusesRenamesAndDeletions()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية");
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        Assert.Equal("projectFinished", await CodeOf(await RenameAsync(client, project.Id, "غصون 7")));
        Assert.Equal("projectFinished", await CodeOf(await RenameUnitAsync(client, project.Id, project.Units[0].Id, "الوحدة")));
        Assert.Equal("projectFinished", await CodeOf(await DeleteUnitAsync(client, project.Id, project.Units[0].Id)));
    }

    [Fact]
    public async Task BR_33_AUnitOfAnotherProjectIsNotFound()
    {
        var (client, project) = await FinanceOfNewProjectAsync("غصون 6");
        var (_, other) = await FinanceOfNewProjectAsync("غصون 7", "الفلة الغربية");

        Assert.Equal(HttpStatusCode.NotFound, (await RenameUnitAsync(client, project.Id, other.Units[0].Id, "الوحدة")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await DeleteUnitAsync(client, project.Id, other.Units[0].Id)).StatusCode);
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task BR_38_OnlyWhoeverManagesTheProjectRenamesAndDeletes(string role)
    {
        var (_, project) = await FinanceOfNewProjectAsync("غصون 6", "الفلة الغربية");
        var member = await StarterAsync();
        await factory.GiveAsync(member.Id, project.Id, Holds.Of(role));
        var client = await ClientOfAsync(member);

        Assert.Equal(HttpStatusCode.Forbidden, (await RenameAsync(client, project.Id, "غصون 7")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await RenameUnitAsync(client, project.Id, project.Units[0].Id, "الوحدة")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteUnitAsync(client, project.Id, project.Units[0].Id)).StatusCode);
    }
}
