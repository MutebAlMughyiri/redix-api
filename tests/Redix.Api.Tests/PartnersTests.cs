using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Partners;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
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
        await factory.WithDatabaseAsync(async db => Assert.Equal(Guid.Empty, (await db.Partners.SingleAsync(row => row.Id == partner.Id)).UserId));
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

        (await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف") { UserId = nawaf.Id }, Json)).EnsureSuccessStatusCode();

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

        (await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف"), Json)).EnsureSuccessStatusCode();

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
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest(name), Json),
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
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("متعب") { UserId = stranger.Id }, Json),
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
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{other.Id}", new PartnerRequest("فواز") { UserId = nawaf.Id }, Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("userTaken", await CodeOf(refused));
        }
        (await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{mine.Id}", new PartnerRequest("نواف بن محمد") { UserId = nawaf.Id }, Json)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TheDatabaseHoldsALoginToOnePartnerInAProjectAndManyWithNoLogin()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "نواف", nawaf.Id);
        await AddAsync(client, project.Id, "هيثم");
        await AddAsync(client, project.Id, "محمد");

        await factory.WithDatabaseAsync(async db =>
        {
            db.Partners.Add(new Partner { ProjectId = project.Id, UserId = nawaf.Id, Name = "نواف الثاني" });
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
    public async Task SomeoneWithoutSeeAllCapitalSeesOnlyTheirOwnPartner(string role)
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, nawafClient) = await MemberAsync(project, role);
        await AddAsync(client, project.Id, "نواف", nawaf.Id);
        await AddAsync(client, project.Id, "متعب");

        Assert.Equal(["نواف"], (await PartnersOf(nawafClient, project.Id)).Select(partner => partner.Name));
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
            await memberClient.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("متعب") { UserId = member.Id }, Json),
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
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{theirs.Id}", new PartnerRequest("سرقة"), Json)).StatusCode);
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
            await client.PostAsJsonAsync($"/projects/{project.Id}/partners/{partner.Id}", new PartnerRequest("نواف"), Json),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Single(await PartnersOf(client, project.Id));
    }
}
