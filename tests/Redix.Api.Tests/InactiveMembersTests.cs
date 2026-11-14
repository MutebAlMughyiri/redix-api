using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.History;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

/// <summary>redix-api #126: a person made inactive in one project by whoever manages its users, who keeps his permissions and entries there and sees it gray on his project list.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class InactiveMembersTests(MigratedApiFactory factory)
{
    private const string Password = "Inactive-members-1";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    /// <summary>Someone who registered a project, now approved, and so manages its users.</summary>
    private async Task<(User User, HttpClient Client, Guid ProjectId)> ManagerAsync()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        return (user, client, (await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}")).Id);
    }

    /// <summary>Someone in <paramref name="projectIds"/> holding what a field person holds, signed in.</summary>
    private async Task<(User User, HttpClient Client)> MemberAsync(params Guid[] projectIds)
    {
        var user = await factory.AddUserAsync(Password);
        foreach (var projectId in projectIds)
        {
            await factory.GiveAsync(user.Id, projectId, Holds.Field);
        }
        return (user, await factory.ClientSignedInAsAsync(user, Password));
    }

    private static Task<HttpResponseMessage> DisableAsync(HttpClient client, Guid projectId, Guid userId) => client.PostAsync($"/projects/{projectId}/users/{userId}/disable", null);

    private static Task<HttpResponseMessage> EnableAsync(HttpClient client, Guid projectId, Guid userId) => client.PostAsync($"/projects/{projectId}/users/{userId}/enable", null);

    private static async Task<UserView> ViewOf(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
        return (await response.Content.ReadFromJsonAsync<UserView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static async Task<List<ProjectView>> ProjectsOf(HttpClient client) => (await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!;

    private async Task<Session> SignInAsync(User user)
    {
        var signedIn = await factory.CreateClient().PostAsJsonAsync("/auth/sign-in", new SignInRequest(user.Username, Password));
        return (await signedIn.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(Json))!;
    }

    private async Task<int> InactiveRowsAsync(Guid projectId, Guid userId)
    {
        var count = 0;
        await factory.WithDatabaseAsync(async db => count = await db.InactiveMembers.CountAsync(row => row.ProjectId == projectId && row.UserId == userId));
        return count;
    }

    /// <summary>The History lines of <paramref name="userId"/>'s being inactive in <paramref name="projectId"/>, oldest first.</summary>
    private async Task<List<HistoryAction>> InactiveHistoryAsync(Guid projectId, Guid userId)
    {
        var rows = new List<HistoryRecord>();
        await factory.WithDatabaseAsync(async db => rows = await db.History.Where(row => row.ProjectId == projectId && row.TableName == "InactiveMembers").OrderBy(row => row.Id).ToListAsync());
        return rows.Where(row => (row.Before + row.After).Contains(userId.ToString())).Select(row => row.Action).ToList();
    }

    [Fact]
    public async Task R1_D1_R7_AManagerMakesSomeoneInactiveInHisProjectAndActiveAgainAndHeKeepsWhatHeHolds()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);

        var disabled = await ViewOf(await DisableAsync(client, projectId, member.Id));
        var enabled = await ViewOf(await EnableAsync(client, projectId, member.Id));

        Assert.True(Assert.Single(disabled.Projects).IsInactive);
        Assert.False(Assert.Single(enabled.Projects).IsInactive);
        Assert.Equal(Holds.Field.Order(), disabled.Projects.Single().Permissions);
        Assert.Equal(Holds.Field.Order(), enabled.Projects.Single().Permissions);
    }

    [Fact]
    public async Task R9_MakingSomeoneInactiveAndActiveAgainEachWriteAHistoryLineByTheManager()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);

        await DisableAsync(client, projectId, member.Id);
        await EnableAsync(client, projectId, member.Id);

        Assert.Equal([HistoryAction.Disable, HistoryAction.Enable], await InactiveHistoryAsync(projectId, member.Id));
        await factory.WithDatabaseAsync(async db =>
            Assert.All(await db.History.Where(row => row.ProjectId == projectId && row.TableName == "InactiveMembers").ToListAsync(), row => Assert.Equal(manager.Id, row.UserId)));
    }

    [Fact]
    public async Task R5_EveryRouteOfAProjectHeIsInactiveInRefusesHimAsForAProjectHeIsNotIn()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId);
        var (_, stranger) = await MemberAsync();
        string[] routes = [$"/projects/{projectId}/members", $"/projects/{projectId}/accounts", $"/projects/{projectId}/partners", $"/projects/{projectId}/entries"];
        Assert.All(await Task.WhenAll(routes.Select(route => memberClient.GetAsync(route))), answer => Assert.Equal(HttpStatusCode.OK, answer.StatusCode));

        await DisableAsync(client, projectId, member.Id);

        foreach (var route in routes)
        {
            var refused = await memberClient.GetAsync(route);
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
            Assert.Equal((await stranger.GetAsync(route)).StatusCode, refused.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await memberClient.PostAsync($"/projects/{projectId}/default", null)).StatusCode);
    }

    [Fact]
    public async Task R5_InactiveInOneProjectHeWorksInAnotherWithTheSameSession()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId, otherProjectId);

        await DisableAsync(client, projectId, member.Id);

        Assert.Equal(HttpStatusCode.OK, (await memberClient.GetAsync($"/projects/{otherProjectId}/members")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await memberClient.GetAsync("/projects")).StatusCode);
    }

    [Fact]
    public async Task R2_D2_HisProjectListShowsAProjectHeIsInactiveInSayingSoAndHoldingNothing()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId, otherProjectId);

        await DisableAsync(client, projectId, member.Id);

        var listed = await ProjectsOf(memberClient);
        var inactive = Assert.Single(listed, project => project.Id == projectId);
        var active = Assert.Single(listed, project => project.Id == otherProjectId);
        Assert.True(inactive.IsInactive);
        Assert.Empty(inactive.Permissions);
        Assert.False(active.IsInactive);
        Assert.Equal(Holds.Field.Order(), active.Permissions.Order());
    }

    [Fact]
    public async Task R2_SomeoneTakenOutOfAProjectHeWasInactiveInNoLongerSeesItOnHisList()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId);
        await DisableAsync(client, projectId, member.Id);

        (await client.DeleteAsync($"/projects/{projectId}/users/{member.Id}/permissions/readProject")).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await ProjectsOf(memberClient), project => project.Id == projectId);
    }

    [Fact]
    public async Task R126_TakingSomeoneOutOfTheProjectEndsHisBeingInactiveThereSoAddedAgainHeArrivesActive()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);
        await DisableAsync(client, projectId, member.Id);

        (await client.DeleteAsync($"/projects/{projectId}/users/{member.Id}/permissions/readProject")).EnsureSuccessStatusCode();
        var added = await ViewOf(await client.PostAsJsonAsync($"/projects/{projectId}/users/existing", new AddExistingUserRequest(member.Username)));

        Assert.False(Assert.Single(added.Projects).IsInactive);
        Assert.Equal(0, await InactiveRowsAsync(projectId, member.Id));
        Assert.Equal([HistoryAction.Disable, HistoryAction.RemoveProject], await InactiveHistoryAsync(projectId, member.Id));
    }

    [Fact]
    public async Task AC_37_2_ADefaultProjectHeIsInactiveInIsSkippedNotClearedAndComesBackWhenHeIsActiveAgain()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId);
        (await memberClient.PostAsync($"/projects/{projectId}/default", null)).EnsureSuccessStatusCode();

        await DisableAsync(client, projectId, member.Id);
        var whileInactive = await SignInAsync(member);
        await EnableAsync(client, projectId, member.Id);
        var activeAgain = await SignInAsync(member);

        Assert.Equal("", whileInactive.DefaultProjectId);
        Assert.Equal(projectId.ToString(), activeAgain.DefaultProjectId);
    }

    [Fact]
    public async Task BR_29_TheLastActiveManagerOfAProjectCannotBeMadeInactiveThere()
    {
        var (manager, client, projectId) = await ManagerAsync();

        var refused = await DisableAsync(client, projectId, manager.Id);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("lastManager", await CodeOf(refused));
        Assert.Equal(0, await InactiveRowsAsync(projectId, manager.Id));
        Assert.Empty(await InactiveHistoryAsync(projectId, manager.Id));
    }

    [Fact]
    public async Task BR_29_WithASecondActiveManagerTheFirstCanBeMadeInactiveAndThenTheSecondCannot()
    {
        var (manager, client, projectId) = await ManagerAsync();
        var second = await factory.AddUserAsync(Password);
        await factory.GiveAsync(second.Id, projectId, Holds.Manager);
        var secondClient = await factory.ClientSignedInAsAsync(second, Password);

        Assert.True(Assert.Single((await ViewOf(await DisableAsync(client, projectId, manager.Id))).Projects).IsInactive);
        Assert.Equal("lastManager", await CodeOf(await DisableAsync(secondClient, projectId, second.Id)));
    }

    [Fact]
    public async Task R126_DoingItTwiceChangesNothing()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);

        Assert.True(Assert.Single((await ViewOf(await DisableAsync(client, projectId, member.Id))).Projects).IsInactive);
        Assert.True(Assert.Single((await ViewOf(await DisableAsync(client, projectId, member.Id))).Projects).IsInactive);
        Assert.Equal(1, await InactiveRowsAsync(projectId, member.Id));
        Assert.False(Assert.Single((await ViewOf(await EnableAsync(client, projectId, member.Id))).Projects).IsInactive);
        Assert.False(Assert.Single((await ViewOf(await EnableAsync(client, projectId, member.Id))).Projects).IsInactive);

        Assert.Equal([HistoryAction.Disable, HistoryAction.Enable], await InactiveHistoryAsync(projectId, member.Id));
    }

    [Fact]
    public async Task Row190_ManyDisablesAtTheSameMomentAllAnswerAsSavedAndKeepOneRow()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);

        var answers = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => DisableAsync(client, projectId, member.Id)));

        Assert.All(answers, answer => Assert.Equal(HttpStatusCode.OK, answer.StatusCode));
        Assert.Equal(1, await InactiveRowsAsync(projectId, member.Id));
        Assert.Equal([HistoryAction.Disable], await InactiveHistoryAsync(projectId, member.Id));
    }

    [Fact]
    public async Task Row190_ManyEnablesAtTheSameMomentAllAnswerAsSavedActiveAndWriteOneHistoryLine()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);
        await DisableAsync(client, projectId, member.Id);

        var answers = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => EnableAsync(client, projectId, member.Id)));

        Assert.All(answers, answer => Assert.Equal(HttpStatusCode.OK, answer.StatusCode));
        foreach (var answer in answers)
        {
            Assert.False(Assert.Single((await ViewOf(answer)).Projects).IsInactive);
        }
        Assert.Equal(0, await InactiveRowsAsync(projectId, member.Id));
        Assert.Equal([HistoryAction.Disable, HistoryAction.Enable], await InactiveHistoryAsync(projectId, member.Id));
    }

    [Fact]
    public async Task R126_SomeoneNotInTheProjectIsNotFound()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (stranger, _) = await MemberAsync();

        foreach (var userId in new[] { stranger.Id, Guid.CreateVersion7() })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await DisableAsync(client, projectId, userId)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await EnableAsync(client, projectId, userId)).StatusCode);
        }
        Assert.Equal(0, await InactiveRowsAsync(projectId, stranger.Id));
    }

    [Fact]
    public async Task R126_OnlyWhoeverManagesItsUsersMakesSomeoneInactiveOrActive()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, memberClient) = await MemberAsync(projectId);
        var (other, _) = await MemberAsync(projectId);
        await DisableAsync(client, projectId, other.Id);

        Assert.Equal(HttpStatusCode.Forbidden, (await DisableAsync(memberClient, projectId, member.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await EnableAsync(memberClient, projectId, other.Id)).StatusCode);
        Assert.Equal(1, await InactiveRowsAsync(projectId, other.Id));
    }

    [Fact]
    public async Task R126_ThePickersLeaveOutSomeoneInactiveInTheProjectAndKeepSomeoneInactiveElsewhere()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (_, _, otherProjectId) = await ManagerAsync();
        var (inactiveHere, _) = await MemberAsync(projectId);
        var (inactiveElsewhere, _) = await MemberAsync(projectId, otherProjectId);
        await factory.MakeInactiveAsync(inactiveHere.Id, projectId);
        await factory.MakeInactiveAsync(inactiveElsewhere.Id, otherProjectId);

        var members = (await client.GetFromJsonAsync<List<MemberView>>($"/projects/{projectId}/members", Json))!;

        Assert.DoesNotContain(members, member => member.Id == inactiveHere.Id);
        Assert.Contains(members, member => member.Id == inactiveElsewhere.Id);
    }

    [Fact]
    public async Task R126_AUserAnswerSaysInactiveForEachProjectAndNothingForTheWholeLogin()
    {
        var (_, client, projectId) = await ManagerAsync();
        var (member, _) = await MemberAsync(projectId);

        var answer = await (await DisableAsync(client, projectId, member.Id)).Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(answer.TryGetProperty("isActive", out _));
        Assert.True(answer.GetProperty("projects")[0].GetProperty("isInactive").GetBoolean());
    }
}
