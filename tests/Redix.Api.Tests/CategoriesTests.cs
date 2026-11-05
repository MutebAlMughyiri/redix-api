using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class CategoriesTests(MigratedApiFactory factory)
{
    private const string Password = "Categories-pass-1";
    private const string Finance = "users.roleFinance";
    private const string Field = "users.roleField";
    private const string Reader = "users.roleReader";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    /// <summary>Someone who registered a project, now approved, and also does its finance.</summary>
    private async Task<(User User, HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);
        return (user, client, project);
    }

    /// <summary>Someone else, holding <paramref name="role"/> in <paramref name="project"/>.</summary>
    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, project.Id, role);
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static async Task<List<CategoryView>> CategoriesOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories", Json))!;

    private static async Task<List<CategoryView>> OfferedFor(HttpClient client, Guid projectId, string itemType) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories/offered?itemType={itemType}", Json))!;

    private static async Task<CategoryView> AddCategoryAsync(HttpClient client, Guid projectId, string name)
    {
        var added = await client.PostAsJsonAsync($"/projects/{projectId}/categories", new NameRequest(name));
        return (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CategoryView>(Json))!;
    }

    private static async Task<ItemView> AddItemAsync(HttpClient client, Guid projectId, Guid categoryId, string name, ItemType type = ItemType.Both)
    {
        var added = await client.PostAsJsonAsync($"/projects/{projectId}/categories/{categoryId}/items", new AddItemRequest(name, type));
        return (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ItemView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private async Task<ProjectView> RegisterAsync(HttpClient client, Guid structureSourceId)
    {
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = structureSourceId });
        return (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;
    }

    private async Task ApproveAsAdminAsync(Guid projectId)
    {
        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{projectId}/approve", null)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TheTemplateHoldsTheCategoriesAndItemsOfTheBrs()
    {
        var (_, client, project) = await FinanceAsync();

        var categories = await CategoriesOf(client, project.Id);

        Assert.Equal(["تكلفة الأراضي", "الرسوم والفواتير", "العظم", "التشطيب", "الإشراف"], categories.Select(category => category.Name));
        Assert.Equal([1, 6, 9, 14, 2], categories.Select(category => category.Items.Count));
        Assert.Equal(["الحفر", "الحديد", "الخرسانة", "البناء", "دفعات العظم", "الكهرباء", "السباكة", "ردود الماء", "أغراض الموقع"], categories[2].Items.Select(item => item.Name));
    }

    [Fact]
    public async Task LandCostCanBeRecordedBecauseItsCategoryHoldsOneItemOfBothTypes()
    {
        var (_, client, project) = await FinanceAsync();

        var land = (await CategoriesOf(client, project.Id)).Single(category => category.Name == "تكلفة الأراضي");

        var item = Assert.Single(land.Items);
        Assert.Equal(("تكلفة الأرض", ItemType.Both), (item.Name, item.Type));
        Assert.Contains((await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories/offered?itemType={ItemType.Purchases}", Json))!, category => category.Name == "تكلفة الأراضي");
        Assert.Contains((await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories/offered?itemType={ItemType.ContractorPayments}", Json))!, category => category.Name == "تكلفة الأراضي");
    }

    [Fact]
    public async Task EveryItemHasAGeneralSubgroup()
    {
        var (_, client, project) = await FinanceAsync();

        var items = (await CategoriesOf(client, project.Id)).SelectMany(category => category.Items).ToList();

        Assert.All(items, item => Assert.Equal([""], item.Subgroups.Select(subgroup => subgroup.Name)));
    }

    [Fact]
    public async Task ApprovingCopiesTheTemplateIntoRowsOfTheProjectsOwn()
    {
        var (_, client, project) = await FinanceAsync();

        var copy = await CategoriesOf(client, project.Id);

        await factory.WithDatabaseAsync(async db =>
        {
            Assert.DoesNotContain(await db.Categories.Where(category => category.ProjectId == StructureSeed.TemplateId).Select(category => category.Id).ToListAsync(), id => copy.Any(category => category.Id == id));
            Assert.True((await db.Projects.SingleAsync(row => row.Id == StructureSeed.TemplateId)).IsTemplate);
        });
    }

    [Fact]
    public async Task AChangeInOneProjectLeavesTheTemplateAndOtherProjectsAlone()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, otherClient, other) = await FinanceAsync("غصون 5");

        await AddCategoryAsync(client, project.Id, "الملاحق");

        Assert.DoesNotContain(await CategoriesOf(otherClient, other.Id), category => category.Name == "الملاحق");
        await factory.WithDatabaseAsync(async db => Assert.False(await db.Categories.AnyAsync(category => category.ProjectId == StructureSeed.TemplateId && category.Name == "الملاحق")));
    }

    [Fact]
    public async Task NobodySeesTheTemplate()
    {
        var (_, client, _) = await FinanceAsync();
        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);

        Assert.DoesNotContain((await client.GetFromJsonAsync<List<ProjectView>>("/projects", Json))!, listed => listed.Id == StructureSeed.TemplateId);
        Assert.DoesNotContain((await admin.GetFromJsonAsync<List<RegisteredProjectView>>("/projects/registered", Json))!, listed => listed.Id == StructureSeed.TemplateId);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync($"/projects/{StructureSeed.TemplateId}/categories")).StatusCode);
    }

    [Fact]
    public async Task AWaitingOrRejectedProjectHasNoStructure()
    {
        var (_, client, _) = await FinanceAsync();
        var waiting = await RegisterAsync(client, Guid.Empty);
        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);

        await factory.WithDatabaseAsync(async db => Assert.False(await db.Categories.AnyAsync(category => category.ProjectId == waiting.Id)));
        await admin.PostAsJsonAsync($"/projects/{waiting.Id}/reject", new RejectProjectRequest("مكرر"));

        await factory.WithDatabaseAsync(async db => Assert.False(await db.Categories.AnyAsync(category => category.ProjectId == waiting.Id)));
    }

    [Fact]
    public async Task ThePublicRegistrationStartsFromTheTemplateWhenApproved()
    {
        var username = $"user-{Guid.CreateVersion7():N}";
        var registered = await factory.CreateClient().PostAsJsonAsync("/projects/register", new RegisterProjectRequest("غصون 8", new DateOnly(2026, 11, 5), [], "ريم", username, "", Password));
        var project = (await registered.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;

        await ApproveAsAdminAsync(project.Id);

        await factory.WithDatabaseAsync(async db => Assert.Equal(5, await db.Categories.CountAsync(category => category.ProjectId == project.Id)));
    }

    [Fact]
    public async Task AProjectCopiedFromAnEarlierOneTakesItsStructureAtApproval()
    {
        var (user, client, earlier) = await FinanceAsync();
        var category = await AddCategoryAsync(client, earlier.Id, "الملاحق");
        var item = await AddItemAsync(client, earlier.Id, category.Id, "المظلة", ItemType.Purchases);
        (await client.PostAsJsonAsync($"/projects/{earlier.Id}/items/{item.Id}/subgroups", new NameRequest("مواد"))).EnsureSuccessStatusCode();

        var project = await RegisterAsync(client, earlier.Id);
        await ApproveAsAdminAsync(project.Id);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);

        var copied = (await CategoriesOf(client, project.Id)).Last();
        Assert.Equal("الملاحق", copied.Name);
        Assert.NotEqual(category.Id, copied.Id);
        var copiedItem = Assert.Single(copied.Items);
        Assert.Equal(("المظلة", ItemType.Purchases), (copiedItem.Name, copiedItem.Type));
        Assert.Equal(["", "مواد"], copiedItem.Subgroups.Select(subgroup => subgroup.Name));
    }

    [Fact]
    public async Task WhatWasDisabledInTheEarlierProjectIsNotCopied()
    {
        var (user, client, earlier) = await FinanceAsync();
        var categories = await CategoriesOf(client, earlier.Id);
        await client.PostAsync($"/projects/{earlier.Id}/categories/{categories[4].Id}/disable", null);
        await client.PostAsync($"/projects/{earlier.Id}/items/{categories[2].Items[0].Id}/disable", null);

        var project = await RegisterAsync(client, earlier.Id);
        await ApproveAsAdminAsync(project.Id);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);

        var copied = await CategoriesOf(client, project.Id);
        Assert.Equal(["تكلفة الأراضي", "الرسوم والفواتير", "العظم", "التشطيب"], copied.Select(category => category.Name));
        Assert.DoesNotContain(copied[2].Items, copiedItem => copiedItem.Name == "الحفر");
    }

    [Fact]
    public async Task AnEarlierProjectTheCallerCannotReadCannotBeCopied()
    {
        var (_, _, earlier) = await FinanceAsync();
        var (_, stranger, _) = await FinanceAsync("غصون 5");

        var created = await stranger.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });

        Assert.Equal(HttpStatusCode.BadRequest, created.StatusCode);
        Assert.Equal("unknownSource", await CodeOf(created));
    }

    [Fact]
    public async Task TheTemplateItselfCannotBeNamedAsAnEarlierProject()
    {
        var (_, client, _) = await FinanceAsync();

        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = StructureSeed.TemplateId });

        Assert.Equal("unknownSource", await CodeOf(created));
    }

    [Fact]
    public async Task ACategoryIsAddedAtTheEnd()
    {
        var (_, client, project) = await FinanceAsync();

        await AddCategoryAsync(client, project.Id, "  الملاحق  ");

        var last = (await CategoriesOf(client, project.Id)).Last();
        Assert.Equal(("الملاحق", true), (last.Name, last.IsActive));
    }

    [Fact]
    public async Task AnItemIsAddedWithItsTypeAtTheEndOfItsCategoryWithItsGeneralGroup()
    {
        var (_, client, project) = await FinanceAsync();
        var finishing = (await CategoriesOf(client, project.Id))[3];

        await AddItemAsync(client, project.Id, finishing.Id, "بند الدرج", ItemType.ContractorPayments);

        var last = (await CategoriesOf(client, project.Id))[3].Items.Last();
        Assert.Equal(("بند الدرج", ItemType.ContractorPayments), (last.Name, last.Type));
        Assert.Equal([""], last.Subgroups.Select(subgroup => subgroup.Name));
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task ACategoryAnItemOrASubgroupNeedsANameOfAtMost100Characters(string name)
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];
        var itemId = category.Items[0].Id;

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories", new NameRequest(name)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}", new NameRequest(name)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items", new AddItemRequest(name, ItemType.Both)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{itemId}", new NameRequest(name)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{itemId}/subgroups", new NameRequest(name)),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidName", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task ACategoryAndAnItemAreRenamed()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}", new NameRequest("الهيكل"))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{category.Items[0].Id}", new NameRequest("بند الحفر"))).EnsureSuccessStatusCode();

        var renamed = (await CategoriesOf(client, project.Id))[2];
        Assert.Equal(("الهيكل", "بند الحفر"), (renamed.Name, renamed.Items[0].Name));
    }

    [Fact]
    public async Task TheOrderOfCategoriesIsTheOrderTheReportReads()
    {
        var (_, client, project) = await FinanceAsync();
        var categories = await CategoriesOf(client, project.Id);
        List<Guid> ids = [.. categories.Select(category => category.Id).Reverse()];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/categories/reorder", new ReorderRequest(ids))).EnsureSuccessStatusCode();

        Assert.Equal(ids, (await CategoriesOf(client, project.Id)).Select(category => category.Id));
    }

    [Fact]
    public async Task TheOrderOfItemsIsTheOrderTheReportReads()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[4];
        List<Guid> ids = [.. category.Items.Select(item => item.Id).Reverse()];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items/reorder", new ReorderRequest(ids))).EnsureSuccessStatusCode();

        Assert.Equal(ids, (await CategoriesOf(client, project.Id))[4].Items.Select(item => item.Id));
    }

    [Fact]
    public async Task AnOrderMustNameEveryRowExactlyOnce()
    {
        var (_, client, project) = await FinanceAsync();
        var categories = await CategoriesOf(client, project.Id);
        var ids = categories.Select(category => category.Id).ToList();

        foreach (List<Guid> order in new List<Guid>[] { [.. ids.Skip(1)], [.. ids.Skip(1), ids[1]], [.. ids.Skip(1), Guid.CreateVersion7()] })
        {
            var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/categories/reorder", new ReorderRequest(order));
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidOrder", await CodeOf(refused));
        }
        Assert.Equal(ids, (await CategoriesOf(client, project.Id)).Select(category => category.Id));
    }

    [Fact]
    public async Task ADisabledItemDisappearsFromEntryAndStaysListed()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];
        var item = category.Items[0];

        (await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain((await OfferedFor(client, project.Id, "purchases"))[1].Items, offered => offered.Id == item.Id);
        Assert.False((await CategoriesOf(client, project.Id))[2].Items[0].IsActive);
    }

    [Fact]
    public async Task ADisabledItemComesBackWhenEnabled()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[0];
        await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/disable", null);

        (await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/enable", null)).EnsureSuccessStatusCode();

        Assert.Contains((await OfferedFor(client, project.Id, "purchases")).SelectMany(category => category.Items), offered => offered.Id == item.Id);
    }

    [Fact]
    public async Task ADisabledCategoryTakesItsItemsOutOfEntryAndStaysListed()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];

        (await client.PostAsync($"/projects/{project.Id}/categories/{category.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await OfferedFor(client, project.Id, "purchases"), offered => offered.Id == category.Id);
        var listed = (await CategoriesOf(client, project.Id))[2];
        Assert.False(listed.IsActive);
        Assert.All(listed.Items, item => Assert.True(item.IsActive));

        (await client.PostAsync($"/projects/{project.Id}/categories/{category.Id}/enable", null)).EnsureSuccessStatusCode();
        Assert.Contains(await OfferedFor(client, project.Id, "purchases"), offered => offered.Id == category.Id);
    }

    [Fact]
    public async Task NothingInTheStructureCanBeDeleted()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];

        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync($"/projects/{project.Id}/categories/{category.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync($"/projects/{project.Id}/items/{category.Items[0].Id}")).StatusCode);
    }

    [Fact]
    public async Task APurchasesItemIsNotOfferedForContractorPayments()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[3];
        var item = await AddItemAsync(client, project.Id, category.Id, "الزجاج", ItemType.Purchases);

        Assert.Contains((await OfferedFor(client, project.Id, "purchases")).Single(offered => offered.Id == category.Id).Items, offered => offered.Id == item.Id);
        Assert.DoesNotContain((await OfferedFor(client, project.Id, "contractorPayments")).Single(offered => offered.Id == category.Id).Items, offered => offered.Id == item.Id);
    }

    [Fact]
    public async Task AContractorPaymentsItemIsNotOfferedForPurchases()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[3];
        var item = await AddItemAsync(client, project.Id, category.Id, "الدهان", ItemType.ContractorPayments);

        Assert.DoesNotContain((await OfferedFor(client, project.Id, "purchases")).Single(offered => offered.Id == category.Id).Items, offered => offered.Id == item.Id);
        Assert.Contains((await OfferedFor(client, project.Id, "contractorPayments")).Single(offered => offered.Id == category.Id).Items, offered => offered.Id == item.Id);
    }

    [Fact]
    public async Task ANewItemIsOfferedToFieldAtOnce()
    {
        var (_, client, project) = await FinanceAsync();
        var field = await MemberAsync(project, Field);
        var category = (await CategoriesOf(client, project.Id))[3];

        var item = await AddItemAsync(client, project.Id, category.Id, "بند الدرج");

        Assert.Contains((await OfferedFor(field, project.Id, "purchases")).Single(offered => offered.Id == category.Id).Items, offered => offered.Id == item.Id);
    }

    [Fact]
    public async Task ACategoryWithNothingToOfferIsLeftOut()
    {
        var (_, client, project) = await FinanceAsync();
        var empty = await AddCategoryAsync(client, project.Id, "الملاحق");

        Assert.DoesNotContain(await OfferedFor(client, project.Id, "purchases"), offered => offered.Id == empty.Id);
    }

    [Fact]
    public async Task ASubgroupIsAddedAfterTheGeneralOneAndRenamed()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[3].Items[5];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups", new NameRequest(" مواد "))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups", new NameRequest("دفان"))).EnsureSuccessStatusCode();
        var subgroups = (await CategoriesOf(client, project.Id))[3].Items[5].Subgroups;
        Assert.Equal(["", "مواد", "دفان"], subgroups.Select(subgroup => subgroup.Name));

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups/{subgroups[1].Id}", new NameRequest("أسمنت"))).EnsureSuccessStatusCode();

        Assert.Equal(["", "أسمنت", "دفان"], (await CategoriesOf(client, project.Id))[3].Items[5].Subgroups.Select(subgroup => subgroup.Name));
    }

    [Fact]
    public async Task TheGeneralSubgroupHasNoNameToChange()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[3].Items[5];

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups/{item.Subgroups[0].Id}", new NameRequest("عام"));

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("generalSubgroup", await CodeOf(refused));
    }

    [Fact]
    public async Task EveryoneWithAccessReadsTheStructureAndOnlyFinanceChangesIt()
    {
        var (_, _, project) = await FinanceAsync();
        var category = (await CategoriesOf(await MemberAsync(project, Finance), project.Id))[2];

        foreach (var role in new[] { Field, Reader, "users.roleManager" })
        {
            var member = await MemberAsync(project, role);
            Assert.Equal(5, (await CategoriesOf(member, project.Id)).Count);
            Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsJsonAsync($"/projects/{project.Id}/categories", new NameRequest("الملاحق"))).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsync($"/projects/{project.Id}/categories/{category.Id}/disable", null)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsync($"/projects/{project.Id}/items/{category.Items[0].Id}/disable", null)).StatusCode);
        }
    }

    [Fact]
    public async Task ARouteOfOneProjectNeverReachesAnothersRows()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, otherClient, other) = await FinanceAsync("غصون 5");
        var category = (await CategoriesOf(otherClient, other.Id))[2];
        var item = category.Items[0];

        foreach (var missed in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}", new NameRequest("سرقة")),
            await client.PostAsync($"/projects/{project.Id}/categories/{category.Id}/disable", null),
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items", new AddItemRequest("سرقة", ItemType.Both)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items/reorder", new ReorderRequest([.. category.Items.Select(row => row.Id)])),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}", new NameRequest("سرقة")),
            await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/disable", null),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups", new NameRequest("سرقة")),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups/{item.Subgroups[0].Id}", new NameRequest("سرقة")),
        })
        {
            Assert.Equal(HttpStatusCode.NotFound, missed.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/categories")).StatusCode);
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[2];
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories", new NameRequest("الملاحق")),
            await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}", new NameRequest("الهيكل")),
            await client.PostAsync($"/projects/{project.Id}/items/{category.Items[0].Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Equal(5, (await CategoriesOf(client, project.Id)).Count);
    }

    [Fact]
    public async Task AWaitingProjectIsNotChangedByItsManager()
    {
        var (_, client, _) = await FinanceAsync();
        var waiting = await RegisterAsync(client, Guid.Empty);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync($"/projects/{waiting.Id}/categories", new NameRequest("الملاحق"))).StatusCode);
    }

    [Fact]
    public async Task AnUnknownItemTypeIsABadRequest()
    {
        var (_, client, project) = await FinanceAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/projects/{project.Id}/categories/offered?itemType=rent")).StatusCode);
    }
}
