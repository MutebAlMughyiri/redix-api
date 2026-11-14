using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(SecondDatabaseCollection.Name)]
public class PartnersTests(MigratedApiFactory factory)
{
    private const string Password = "Partners-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (client, project);
    }

    private async Task<(User User, HttpClient Client)> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        return (user, await factory.ClientSignedInAsAsync(user, Password));
    }

    [Fact]
    public async Task UC_109_PartnersDeclaredWithANewProjectBecomeTablesOfItsRasAlMalInTheirOrderOnceApproved()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);

        var refused = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 8", new DateOnly(2026, 11, 1), []) { Partners = ["نواف", " "] }, Json);
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 9", new DateOnly(2026, 11, 1), []) { Partners = [" نواف ", "ريم"] }, Json);
        var project = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;
        await factory.WithDatabaseAsync(async db =>
        {
            await ProjectStructure.ApproveAsync(await db.Projects.SingleAsync(stored => stored.Id == project.Id), user.Id, db);
            await db.SaveChangesAsync();
        });
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));

        Assert.Equal((HttpStatusCode.BadRequest, "invalidName"), (refused.StatusCode, await Ledger.CodeOf(refused)));
        var capital = await Ledger.CapitalItemOfAsync(client, project.Id);
        Assert.Equal(["نواف", "ريم"], (await PartnersOf(client, project.Id)).Select(partner => partner.Name));
        await factory.WithDatabaseAsync(async db => Assert.All(await db.Partners.Where(row => row.ProjectId == project.Id).ToListAsync(), row => Assert.Equal(capital, row.ItemId)));
    }

    [Fact]
    public async Task UC_106_APartnerIsAddedByNameToTheProjectsOneCapitalItemAndNoneIsAddedWhileItIsDisabled()
    {
        var (client, project) = await FinanceAsync();
        var capital = await Ledger.CapitalItemOfAsync(client, project.Id);

        var partner = await AddAsync(client, project.Id, "نواف");
        await factory.WithDatabaseAsync(async db => Assert.Equal(capital, (await db.Partners.SingleAsync(row => row.Id == partner.Id)).ItemId));
        (await client.PostAsync($"/projects/{project.Id}/items/{capital}/disable", null)).EnsureSuccessStatusCode();
        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest("ريم"), Json);

        Assert.Equal((HttpStatusCode.BadRequest, "itemNotOffered"), (refused.StatusCode, await CodeOf(refused)));
    }

    private static async Task<List<PartnerView>> PartnersOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<PartnerView>>($"/projects/{projectId}/partners", Json))!;

    private static async Task<PartnerView> AddAsync(HttpClient client, Guid projectId, string name, Guid userId = default)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await PartnersOf(client, projectId)).Single(partner => partner.Name == name);
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    [Fact]
    public async Task FinanceAddsAPartnerByNameWithNoLogin()
    {
        var (client, project) = await FinanceAsync();

        var partner = await AddAsync(client, project.Id, "هيثم");

        Assert.Equal("", partner.UserId);
        Assert.Equal("", partner.UserName);
        await factory.WithDatabaseAsync(async db => Assert.False(await db.PartnerUsers.AnyAsync(row => row.PartnerId == partner.Id)));
    }

    [Fact]
    public async Task R214_APartnersLoginIsARowOfItsOwnThatTakingTheLoginAwayRemoves()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var partner = await AddAsync(client, project.Id, "نواف", nawaf.Id);
        await factory.WithDatabaseAsync(async db => Assert.Equal(nawaf.Id, (await db.PartnerUsers.SingleAsync(row => row.PartnerId == partner.Id)).UserId));

        (await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();

        await factory.WithDatabaseAsync(async db => Assert.False(await db.PartnerUsers.AnyAsync(row => row.PartnerId == partner.Id)));
    }

    [Fact]
    public async Task R214_TheDatabaseRefusesAPartnersLoginNamingNoUser()
    {
        var (client, project) = await FinanceAsync();
        var partner = await AddAsync(client, project.Id, "هيثم");

        await factory.WithDatabaseAsync(async db =>
        {
            db.PartnerUsers.Add(new PartnerUser { PartnerId = partner.Id, ProjectId = project.Id, UserId = Guid.CreateVersion7() });

            var refused = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

            Assert.Contains("FK_PartnerUsers_Users_UserId", refused.InnerException!.Message);
        });
    }

    [Fact]
    public async Task R214_ADeletedPartnersLoginMayBeAnotherPartnersInTheProject()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var deleted = await AddAsync(client, project.Id, "نواف", nawaf.Id);
        (await client.DeleteAsync($"/projects/{project.Id}/partners/{deleted.Id}")).EnsureSuccessStatusCode();

        var again = await AddAsync(client, project.Id, "نواف بن محمد", nawaf.Id);

        Assert.Equal(nawaf.Id.ToString(), again.UserId);
        await factory.WithDatabaseAsync(async db => Assert.False(await db.PartnerUsers.AnyAsync(row => row.PartnerId == deleted.Id)));
    }

    [Fact]
    public async Task APartnerIsLinkedToTheirLogin()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);

        var partner = await AddAsync(client, project.Id, "نواف", nawaf.Id);

        Assert.Equal(nawaf.Id.ToString(), partner.UserId);
        Assert.Equal(nawaf.DisplayName, partner.UserName);
    }

    [Fact]
    public async Task PartnersShowInTheOrderTheyWereAdded()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "متعب");
        await AddAsync(client, project.Id, "نواف");
        await AddAsync(client, project.Id, "فواز");

        Assert.Equal(["متعب", "نواف", "فواز"], (await PartnersOf(client, project.Id)).Select(partner => partner.Name));
    }

    [Fact]
    public async Task AnEditRenamesAPartnerAndSetsTheirLogin()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var partner = await AddAsync(client, project.Id, "نوف");

        (await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف") { UserId = nawaf.Id }, Json)).EnsureSuccessStatusCode();

        var edited = (await PartnersOf(client, project.Id)).Single();
        Assert.Equal("نواف", edited.Name);
        Assert.Equal(nawaf.Id.ToString(), edited.UserId);
    }

    [Fact]
    public async Task AnEditCanTakeALoginAway()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var partner = await AddAsync(client, project.Id, "نواف", nawaf.Id);

        (await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();

        Assert.Equal("", (await PartnersOf(client, project.Id)).Single().UserId);
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task APartnerNeedsANameOfAtMost100Characters(string name)
    {
        var (client, project) = await FinanceAsync();
        var partner = await AddAsync(client, project.Id, "متعب");

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest(name), Json),
            await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest(name), Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidName", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task ALoginMustBeInTheProject()
    {
        var (client, project) = await FinanceAsync();
        var stranger = await factory.AddUserAsync(Password);
        var partner = await AddAsync(client, project.Id, "متعب");

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest("نواف") { UserId = stranger.Id }, Json),
            await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("متعب") { UserId = stranger.Id }, Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("unknownUser", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task ALoginIsOnePartnerInAProject()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var mine = await AddAsync(client, project.Id, "نواف", nawaf.Id);
        var other = await AddAsync(client, project.Id, "فواز");

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest("نواف الثاني") { UserId = nawaf.Id }, Json),
            await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{other.Id}", new PartnerRequest("فواز") { UserId = nawaf.Id }, Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("userTaken", await CodeOf(refused));
        }
        (await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{mine.Id}", new PartnerRequest("نواف بن محمد") { UserId = nawaf.Id }, Json)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TheDatabaseHoldsALoginToOnePartnerInAProjectAndManyWithNoLogin()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "نواف", nawaf.Id);
        var haitham = await AddAsync(client, project.Id, "هيثم");
        await AddAsync(client, project.Id, "محمد");

        await factory.WithDatabaseAsync(async db =>
        {
            db.PartnerUsers.Add(new PartnerUser { PartnerId = haitham.Id, ProjectId = project.Id, UserId = nawaf.Id });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        });
    }

    [Fact]
    public async Task OneLoginMayBeAPartnerInTwoProjects()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        foreach (var name in new[] { "غصون 6", "غصون 5" })
        {
            var project = await factory.ApprovedProjectAsync(client, name);
            await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
            await AddAsync(client, project.Id, "متعب", user.Id);
        }
    }

    [Theory]
    [InlineData(Reader)]
    [InlineData(Field)]
    public async Task UC_106_WithoutSeeAllCapitalSomeoneSeesOnlyThePartnersChosenForThemAndTheirOwnLoginGivesNoSight(string role)
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, nawafClient) = await MemberAsync(project, role);
        await AddAsync(client, project.Id, "نواف", nawaf.Id);
        var chosen = await AddAsync(client, project.Id, "متعب");
        await AddAsync(client, project.Id, "هيثم");

        Assert.Empty(await PartnersOf(nawafClient, project.Id));
        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{chosen.Id}", null)).EnsureSuccessStatusCode();

        Assert.Equal(["متعب"], (await PartnersOf(nawafClient, project.Id)).Select(partner => partner.Name));
        Assert.Equal(HttpStatusCode.OK, (await nawafClient.GetAsync($"/projects/{project.Id}/partners/{chosen.Id}/movements")).StatusCode);
    }

    private static async Task<UserProjectView> HeldAsync(HttpClient client, Guid projectId, Guid userId) =>
        (await client.GetFromJsonAsync<List<UserView>>($"/projects/{projectId}/users", Json))!.Single(user => user.Id == userId).Projects.Single();

    [Fact]
    public async Task UC_106_ChoosingAPartnerGivesChosenCapitalAndTakesSeeAllAndTakingChosenCapitalForgetsThePartners()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Manager);
        var partner = await AddAsync(client, project.Id, "متعب");

        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{partner.Id}", null)).EnsureSuccessStatusCode();
        var chosen = await HeldAsync(client, project.Id, nawaf.Id);
        (await client.DeleteAsync($"/projects/{project.Id}/users/{nawaf.Id}/permissions/{Permission.SeeChosenCapital}")).EnsureSuccessStatusCode();
        var taken = await HeldAsync(client, project.Id, nawaf.Id);

        Assert.Equal([partner.Id], chosen.PartnerIds);
        Assert.Equal((true, false), (chosen.Permissions.Contains(Permission.SeeChosenCapital), chosen.Permissions.Contains(Permission.SeeAllCapital)));
        Assert.Equal((0, false), (taken.PartnerIds.Count, taken.Permissions.Contains(Permission.SeeChosenCapital)));
    }

    [Fact]
    public async Task Row190_ManyUnchoosesOfOnePartnerAtTheSameMomentAllAnswerAsDoneAndLeaveItUnchosen()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Manager);
        var partner = await AddAsync(client, project.Id, "متعب");
        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{partner.Id}", null)).EnsureSuccessStatusCode();

        var answers = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.DeleteAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{partner.Id}")));

        Assert.All(answers, answer => Assert.Equal(HttpStatusCode.OK, answer.StatusCode));
        Assert.Empty((await HeldAsync(client, project.Id, nawaf.Id)).PartnerIds);
    }

    [Fact]
    public async Task UC_106_SeeAllAndChosenCapitalExcludeEachOtherAndARegistrantSeesAll()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Reader);
        var partner = await AddAsync(client, project.Id, "متعب");
        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{partner.Id}", null)).EnsureSuccessStatusCode();

        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/permissions/{Permission.SeeAllCapital}", null)).EnsureSuccessStatusCode();
        var all = await HeldAsync(client, project.Id, nawaf.Id);
        (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/permissions/{Permission.SeeChosenCapital}", null)).EnsureSuccessStatusCode();
        var chosen = await HeldAsync(client, project.Id, nawaf.Id);
        var registrant = (await client.GetFromJsonAsync<List<UserView>>($"/projects/{project.Id}/users", Json))!.Single(user => user.Id != nawaf.Id).Projects.Single();

        Assert.Equal((true, false, 0), (all.Permissions.Contains(Permission.SeeAllCapital), all.Permissions.Contains(Permission.SeeChosenCapital), all.PartnerIds.Count));
        Assert.Equal((false, true), (chosen.Permissions.Contains(Permission.SeeAllCapital), chosen.Permissions.Contains(Permission.SeeChosenCapital)));
        Assert.Equal((true, false), (registrant.Permissions.Contains(Permission.SeeAllCapital), registrant.Permissions.Contains(Permission.SeeChosenCapital)));
    }

    [Fact]
    public async Task UC_106_APersonIsCreatedWithChosenPartnersButNotWithBothCapitalTicksNorAnotherProjectsPartner()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var partner = await AddAsync(client, project.Id, "متعب");
        var theirs = await AddAsync(otherClient, other.Id, "شريكهم");
        CreateUserRequest Request(string username, List<Permission> permissions, List<Guid> partnerIds) =>
            new("المحاسب", username, "", "Temporary-pass-1") { Permissions = permissions, PartnerIds = partnerIds };

        var made = await client.PostAsJsonAsync($"/projects/{project.Id}/users", Request($"accountant-{Guid.NewGuid():N}", [Permission.SeeChosenCapital], [partner.Id]), Json);
        var both = await client.PostAsJsonAsync($"/projects/{project.Id}/users", Request($"both-{Guid.NewGuid():N}", [Permission.SeeAllCapital, Permission.SeeChosenCapital], []), Json);
        var stranger = await client.PostAsJsonAsync($"/projects/{project.Id}/users", Request($"stranger-{Guid.NewGuid():N}", [Permission.SeeChosenCapital], [theirs.Id]), Json);

        Assert.Equal([partner.Id], (await made.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<UserView>(Json))!.Projects.Single().PartnerIds);
        Assert.Equal((HttpStatusCode.BadRequest, "invalidPermission"), (both.StatusCode, await CodeOf(both)));
        Assert.Equal((HttpStatusCode.BadRequest, "unknownPartner"), (stranger.StatusCode, await CodeOf(stranger)));
    }

    [Fact]
    public async Task UC_106_ChoosingAPartnerOfAnotherProjectOrForSomeoneOutsideIsNotFound()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var (nawaf, _) = await MemberAsync(project, Reader);
        var theirs = await AddAsync(otherClient, other.Id, "شريكهم");
        var ours = await AddAsync(client, project.Id, "متعب");
        var stranger = await factory.AddUserAsync(Password);

        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{project.Id}/users/{nawaf.Id}/partners/{theirs.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{project.Id}/users/{stranger.Id}/partners/{ours.Id}", null)).StatusCode);
    }

    [Fact]
    public async Task SomeoneWhoIsNoPartnerSeesNoPartners()
    {
        var (client, project) = await FinanceAsync();
        var (_, fieldClient) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "متعب");
        await AddAsync(client, project.Id, "هيثم");

        Assert.Empty(await PartnersOf(fieldClient, project.Id));
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Finance)]
    public async Task WhoeverHoldsSeeAllCapitalSeesEveryPartner(string role)
    {
        var (client, project) = await FinanceAsync();
        var (_, memberClient) = await MemberAsync(project, role);
        await AddAsync(client, project.Id, "متعب");
        await AddAsync(client, project.Id, "نواف");

        Assert.Equal(2, (await PartnersOf(memberClient, project.Id)).Count);
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task OnlyFinanceChangesPartners(string role)
    {
        var (client, project) = await FinanceAsync();
        var partner = await AddAsync(client, project.Id, "متعب");
        var (member, memberClient) = await MemberAsync(project, role);

        foreach (var refused in new[]
        {
            await memberClient.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest("نواف"), Json),
            await memberClient.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("متعب") { UserId = member.Id }, Json),
            await memberClient.DeleteAsync($"/projects/{project.Id}/partners/{partner.Id}"),
        })
        {
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        }
    }

    [Fact]
    public async Task OneProjectsPartnersAreNeverAnothers()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var theirs = await AddAsync(otherClient, other.Id, "شريكهم");

        Assert.Empty(await PartnersOf(client, project.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{theirs.Id}", new PartnerRequest("سرقة"), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/partners")).StatusCode);
    }

    [Fact]
    public async Task ACopiedProjectStartsWithNoPartners()
    {
        var (client, earlier) = await FinanceAsync();
        await AddAsync(client, earlier.Id, "متعب");
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });
        var later = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;

        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{later.Id}/approve", null)).EnsureSuccessStatusCode();

        Assert.Empty(await PartnersOf(client, later.Id));
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (client, project) = await FinanceAsync();
        var partner = await AddAsync(client, project.Id, "متعب");
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners", new PartnerRequest("نواف"), Json),
            await client.PutAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف"), Json),
            await client.DeleteAsync($"/projects/{project.Id}/partners/{partner.Id}"),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Single(await PartnersOf(client, project.Id));
    }
}
