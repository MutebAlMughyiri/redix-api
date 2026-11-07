using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(FourthDatabaseCollection.Name)]
public class PaymentMethodsTests(MigratedApiFactory factory)
{
    private const string Password = "Payment-methods-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";

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

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static async Task<List<PaymentMethodView>> MethodsOf(HttpClient client, Guid projectId, string path = "") =>
        (await client.GetFromJsonAsync<List<PaymentMethodView>>($"/projects/{projectId}/payment-methods{path}", Json))!;

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    [Fact]
    public async Task AnApprovedProjectStartsWithTheThreeMethods()
    {
        var (client, project) = await FinanceAsync();

        Assert.Equal(["شبكة", "تحويل", "كاش"], (await MethodsOf(client, project.Id)).Select(method => method.Name));
    }

    [Fact]
    public async Task ProjectsTakeTheirOwnRowsOfTheTemplatesMethods()
    {
        var (client, project) = await FinanceAsync();

        var methods = await MethodsOf(client, project.Id);

        await factory.WithDatabaseAsync(async db =>
            Assert.DoesNotContain(await db.PaymentMethods.Where(method => method.ProjectId == StructureSeed.TemplateId).Select(method => method.Id).ToListAsync(), id => methods.Any(method => method.Id == id)));
    }

    [Fact]
    public async Task AProjectCopiedFromAnEarlierOneStillStartsFromTheSeed()
    {
        var (client, earlier) = await FinanceAsync();
        (await client.PostAsJsonAsync($"/projects/{earlier.Id}/payment-methods", new NameRequest("مدى"))).EnsureSuccessStatusCode();
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });
        var later = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;

        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{later.Id}/approve", null)).EnsureSuccessStatusCode();

        Assert.Equal(["شبكة", "تحويل", "كاش"], (await MethodsOf(client, later.Id)).Select(method => method.Name));
    }

    [Fact]
    public async Task FinanceAddsAMethodAtTheEndOfTheList()
    {
        var (client, project) = await FinanceAsync();

        (await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods", new NameRequest("  مدى  "))).EnsureSuccessStatusCode();

        Assert.Equal("مدى", (await MethodsOf(client, project.Id)).Last().Name);
    }

    [Fact]
    public async Task FinanceRenamesAMethod()
    {
        var (client, project) = await FinanceAsync();
        var card = (await MethodsOf(client, project.Id))[0];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods/{card.Id}", new NameRequest("بطاقة"))).EnsureSuccessStatusCode();

        Assert.Equal("بطاقة", (await MethodsOf(client, project.Id))[0].Name);
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task AMethodNeedsANameOfAtMost100Characters(string name)
    {
        var (client, project) = await FinanceAsync();
        var card = (await MethodsOf(client, project.Id))[0];

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods", new NameRequest(name)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods/{card.Id}", new NameRequest(name)),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidName", await CodeOf(refused));
        }
        Assert.Equal(3, (await MethodsOf(client, project.Id)).Count);
    }

    [Fact]
    public async Task ADisabledMethodLeavesEntryAndStaysOnTheList()
    {
        var (client, project) = await FinanceAsync();
        var cash = (await MethodsOf(client, project.Id))[2];

        (await client.PostAsync($"/projects/{project.Id}/payment-methods/{cash.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await MethodsOf(client, project.Id, "/offered"), method => method.Id == cash.Id);
        Assert.False((await MethodsOf(client, project.Id)).Single(method => method.Id == cash.Id).IsActive);
    }

    [Fact]
    public async Task EnablingBringsAMethodBackIntoEntry()
    {
        var (client, project) = await FinanceAsync();
        var cash = (await MethodsOf(client, project.Id))[2];
        (await client.PostAsync($"/projects/{project.Id}/payment-methods/{cash.Id}/disable", null)).EnsureSuccessStatusCode();

        (await client.PostAsync($"/projects/{project.Id}/payment-methods/{cash.Id}/enable", null)).EnsureSuccessStatusCode();

        Assert.Contains(await MethodsOf(client, project.Id, "/offered"), method => method.Id == cash.Id);
    }

    [Fact]
    public async Task FinanceDeletesAMethodNoExpenseWasPaidWith()
    {
        var (client, project) = await FinanceAsync();
        var cash = (await MethodsOf(client, project.Id))[2];

        (await client.PostAsync($"/projects/{project.Id}/payment-methods/{cash.Id}/delete", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await MethodsOf(client, project.Id), method => method.Id == cash.Id);
    }

    [Fact]
    public async Task AMethodAnExpenseWasPaidWithIsNotDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(100));

        var refused = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/payment-methods/{ledger.MethodId}/delete", null);

        Assert.Equal((HttpStatusCode.Conflict, "paymentMethodUsed"), (refused.StatusCode, await CodeOf(refused)));
        Assert.Contains(await MethodsOf(ledger.Client, ledger.ProjectId), method => method.Id == ledger.MethodId);
    }

    [Fact]
    public async Task AChangeInOneProjectLeavesTheTemplateAndOtherProjectsAlone()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods", new NameRequest("مدى"))).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await MethodsOf(otherClient, other.Id), method => method.Name == "مدى");
        await factory.WithDatabaseAsync(async db => Assert.False(await db.PaymentMethods.AnyAsync(method => method.ProjectId == StructureSeed.TemplateId && method.Name == "مدى")));
    }

    [Fact]
    public async Task ARouteOfOneProjectNeverReachesAnothersMethods()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var theirs = (await MethodsOf(otherClient, other.Id))[0];

        foreach (var missed in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods/{theirs.Id}", new NameRequest("سرقة")),
            await client.PostAsync($"/projects/{project.Id}/payment-methods/{theirs.Id}/disable", null),
            await client.PostAsync($"/projects/{project.Id}/payment-methods/{theirs.Id}/delete", null),
        })
        {
            Assert.Equal(HttpStatusCode.NotFound, missed.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/payment-methods")).StatusCode);
    }

    [Theory]
    [InlineData(Field)]
    [InlineData(Reader)]
    [InlineData("Manager")]
    public async Task EveryoneWithAccessReadsMethodsAndOnlyFinanceChangesThem(string role)
    {
        var (client, project) = await FinanceAsync();
        var card = (await MethodsOf(client, project.Id))[0];
        var member = await MemberAsync(project, role);

        Assert.Equal(3, (await MethodsOf(member, project.Id)).Count);
        foreach (var refused in new[]
        {
            await member.PostAsJsonAsync($"/projects/{project.Id}/payment-methods", new NameRequest("مدى")),
            await member.PostAsJsonAsync($"/projects/{project.Id}/payment-methods/{card.Id}", new NameRequest("مدى")),
            await member.PostAsync($"/projects/{project.Id}/payment-methods/{card.Id}/disable", null),
            await member.PostAsync($"/projects/{project.Id}/payment-methods/{card.Id}/delete", null),
        })
        {
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        }
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (client, project) = await FinanceAsync();
        var card = (await MethodsOf(client, project.Id))[0];
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods", new NameRequest("مدى")),
            await client.PostAsJsonAsync($"/projects/{project.Id}/payment-methods/{card.Id}", new NameRequest("مدى")),
            await client.PostAsync($"/projects/{project.Id}/payment-methods/{card.Id}/disable", null),
            await client.PostAsync($"/projects/{project.Id}/payment-methods/{card.Id}/delete", null),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Equal(3, (await MethodsOf(client, project.Id)).Count);
    }
}
