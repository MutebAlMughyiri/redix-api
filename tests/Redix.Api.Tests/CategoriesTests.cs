using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;
using Redix.Api.Revenue;

namespace Redix.Api.Tests;

[Collection(ThirdDatabaseCollection.Name)]
public class CategoriesTests(MigratedApiFactory factory)
{
    private const string Password = "Categories-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    /// <summary>Someone who registered a project, now approved, and also does its finance.</summary>
    private async Task<(User User, HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (user, client, project);
    }

    /// <summary>Someone else, holding <paramref name="role"/> in <paramref name="project"/>.</summary>
    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
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
    public async Task ASubgroupRevenueIsKeptOnIsNotRemoved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var category = (await CategoriesOf(ledger.Client, ledger.ProjectId))[0];
        var itemId = (await AddItemAsync(ledger.Client, ledger.ProjectId, category.Id, "إيجار المعدات")).Id;
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{itemId}/subgroups", new NameRequest("إيجار"), Ledger.Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{itemId}/type", new ChangeItemTypeRequest(ItemType.OtherRevenue), Ledger.Json)).EnsureSuccessStatusCode();
        var subgroup = (await CategoriesOf(ledger.Client, ledger.ProjectId)).SelectMany(category => category.Items).Single(item => item.Id == itemId).Subgroups[1];
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", new Revenue.RevenueRequest(itemId, ledger.Today, 70, ledger.AccountId, "") { SubgroupId = subgroup.Id }, Ledger.Json)).EnsureSuccessStatusCode();

        var refused = await DeleteSubgroupAsync(ledger.Client, ledger.ProjectId, itemId, subgroup.Id);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("subgroupInUse", await CodeOf(refused));
    }

    [Fact]
    public async Task TheTemplateHoldsTheCategoriesAndItemsOfTheBrs()
    {
        var (_, client, project) = await FinanceAsync();

        var categories = await CategoriesOf(client, project.Id);

        Assert.Equal(["تكلفة الأراضي", "الرسوم والفواتير", "العظم", "التشطيب", "الإشراف", "رأس المال", "استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], categories.Select(category => category.Name));
        Assert.Equal([1, 6, 9, 14, 2, 1, 1, 1, 1], categories.Select(category => category.Items.Count));
        Assert.Equal([ItemType.Capital, ItemType.VatRefund, ItemType.OtherRevenue, ItemType.Sale], categories.Skip(5).Select(category => category.Items.Single().Type));
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

        await factory.WithDatabaseAsync(async db => Assert.Equal(9, await db.Categories.CountAsync(category => category.ProjectId == project.Id)));
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
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));

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
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));

        var copied = await CategoriesOf(client, project.Id);
        Assert.Equal(["تكلفة الأراضي", "الرسوم والفواتير", "العظم", "التشطيب", "رأس المال", "استرجاع ضرائب", "إيرادات أخرى", "فلوس البيع المودعة بالحساب"], copied.Select(category => category.Name));
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

        foreach (var role in new[] { Field, Reader, "Manager" })
        {
            var member = await MemberAsync(project, role);
            Assert.Equal(9, (await CategoriesOf(member, project.Id)).Count);
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
        Assert.Equal(9, (await CategoriesOf(client, project.Id)).Count);
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

    private async Task<(HttpClient Client, Guid ProjectId, ItemView Item)> ItemWithSubgroupsAsync(params string[] names)
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[3].Items[5];
        foreach (var name in names)
        {
            (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups", new NameRequest(name))).EnsureSuccessStatusCode();
        }
        return (client, project.Id, (await CategoriesOf(client, project.Id))[3].Items[5]);
    }

    private static Task<HttpResponseMessage> ReorderTablesAsync(HttpClient client, Guid projectId, Guid itemId, params IEnumerable<Guid> ids) =>
        client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/tables/reorder", new { ids });

    private static async Task<List<TableCategoryView>> EntryTablesOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<TableCategoryView>>($"/projects/{projectId}/entry-tables", Json))!;

    [Fact]
    public async Task R10_AnItemOffersAnExpenseItsGeneralTableEachNamedOneAndEachActiveContractInItsSheetsOrder()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var contractor = (await ledger.Client.GetFromJsonAsync<List<SupplierView>>($"/projects/{ledger.ProjectId}/suppliers", Json))!.Single(supplier => supplier.Name == Supplier.GeneralCashSupplierName);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(contractor.Id, ledger.ItemId, 1000m, false), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/subgroups", new NameRequest("مواد"), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, ledger.ItemId, 2000m, false), Json)).EnsureSuccessStatusCode();
        var disabled = await ledger.ReadAsync(db => db.Contracts.SingleAsync(contract => contract.SupplierId == ledger.SupplierId));
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/contracts/{disabled.Id}/disable", null)).EnsureSuccessStatusCode();

        var item = (await EntryTablesOf(ledger.Client, ledger.ProjectId)).SelectMany(category => category.Items).Single(row => row.Id == ledger.ItemId);

        Assert.Equal([ItemTableKind.General, ItemTableKind.Contract, ItemTableKind.Subgroup], item.Tables.Select(table => table.Kind));
        Assert.Equal([item.Name, Supplier.GeneralCashSupplierName, "مواد"], item.Tables.Select(table => table.Name));
    }

    [Fact]
    public async Task R7_NoDisabledItemOrCategoryAndNoItemWithoutATableIsOffered()
    {
        var (_, client, project) = await FinanceAsync();
        var categories = await CategoriesOf(client, project.Id);
        var contractsOnly = await AddItemAsync(client, project.Id, categories[2].Id, "أعمال بالعقد", ItemType.ContractorPayments);
        var purchasesOnly = await AddItemAsync(client, project.Id, categories[2].Id, "مشتريات فقط", ItemType.Purchases);
        (await client.PostAsync($"/projects/{project.Id}/items/{categories[3].Items[0].Id}/disable", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/projects/{project.Id}/categories/{categories[4].Id}/disable", null)).EnsureSuccessStatusCode();

        var offered = await EntryTablesOf(client, project.Id);
        var items = offered.SelectMany(category => category.Items).ToList();

        Assert.DoesNotContain(offered, category => category.Id == categories[4].Id);
        Assert.DoesNotContain(items, item => item.Id == contractsOnly.Id || item.Id == categories[3].Items[0].Id);
        Assert.Equal([ItemTableKind.General], items.Single(item => item.Id == purchasesOnly.Id).Tables.Select(table => table.Kind));
    }

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name, await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId)), Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    [Fact]
    public async Task R3_R5_R13_UC_109_WhoeverMovesMoneyIsOfferedTheRevenueItemsAndRasAlMalWhoseTablesAreItsPartnersInTheirOrder()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var refund = await ledger.RevenueItemAsync(ItemType.VatRefund);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        var nawaf = await PartnerAsync(ledger, "نواف");
        var reem = await PartnerAsync(ledger, "ريم");

        var offered = await EntryTablesOf(ledger.Client, ledger.ProjectId);

        var revenue = offered.SelectMany(category => category.Items).Single(item => item.Id == refund);
        Assert.True(revenue.IsRevenue);
        Assert.Equal([ItemTableKind.General], revenue.Tables.Select(table => table.Kind));
        var capitalCategory = offered.Single(category => category.Items.Any(item => item.Id == capital));
        Assert.Equal(("رأس المال", "رأس المال", true), (capitalCategory.Name, capitalCategory.Items.Single().Name, capitalCategory.Items.Single().IsRevenue));
        Assert.Equal([(ItemTableKind.Partner, nawaf, "نواف"), (ItemTableKind.Partner, reem, "ريم")], capitalCategory.Items.Single().Tables.Select(table => (table.Kind, table.Id, table.Name)));
        Assert.All(offered, category => Assert.NotEqual("", category.Name));
        Assert.True(offered.IndexOf(capitalCategory) < offered.FindIndex(category => category.Items.Any(item => item.Id == refund)), "رأس المال comes first among the revenue");
        Assert.False(offered.SelectMany(category => category.Items).Single(item => item.Id == ledger.ItemId).IsRevenue);
    }

    [Fact]
    public async Task UC_109_APartnerIsATableOfACapitalItemLastAmongItsTablesAndNoOtherItemTakesOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        var nawaf = await PartnerAsync(ledger, "نواف");
        var reem = await PartnerAsync(ledger, "ريم");

        var refused = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("سالم", ledger.ItemId), Json);
        var tables = (await ledger.Client.GetFromJsonAsync<List<ItemTableView>>($"/projects/{ledger.ProjectId}/items/{capital}/tables", Json))!;
        var reordered = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{capital}/tables/reorder", new ReorderRequest([reem, nawaf]), Json);

        Assert.Equal((HttpStatusCode.BadRequest, "itemNotOffered"), (refused.StatusCode, await Ledger.CodeOf(refused)));
        Assert.Equal([(nawaf, ItemTableKind.Partner, "نواف"), (reem, ItemTableKind.Partner, "ريم")], tables.Select(table => (table.Id, table.Kind, table.Name)));
        reordered.EnsureSuccessStatusCode();
        Assert.Equal([reem, nawaf], (await ledger.Client.GetFromJsonAsync<List<ItemTableView>>($"/projects/{ledger.ProjectId}/items/{capital}/tables", Json))!.Select(table => table.Id));
    }

    [Fact]
    public async Task UC_109_RevenueGoesIntoEveryRevenueItemButRasAlMalAndAPartnerIntoNothingElse()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);

        var revenue = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/revenue", new RevenueRequest(capital, ledger.Today, 100, ledger.AccountId, ""), Json);
        var offeredForRevenue = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories/offered?itemType=otherRevenue", Json))!.SelectMany(category => category.Items);
        var offeredForCapital = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories/offered?itemType=capital", Json))!.SelectMany(category => category.Items);

        Assert.Equal((HttpStatusCode.BadRequest, "unknownItem"), (revenue.StatusCode, await Ledger.CodeOf(revenue)));
        Assert.DoesNotContain(offeredForRevenue, item => item.Id == capital);
        Assert.Equal([capital], offeredForCapital.Select(item => item.Id));
    }

    [Fact]
    public async Task BR_31_UC_109_RasAlMalsTablesShowEveryPartnerToWhoeverSeesAllCapitalTheirOwnToAPartnerAndNoneToAnyoneElse()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        var (partnerReader, partnerReaderId) = await ledger.MemberAsync("Reader");
        var (otherReader, _) = await ledger.MemberAsync("Reader");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest("القارئ", capital) { UserId = partnerReaderId }, Json)).EnsureSuccessStatusCode();
        await PartnerAsync(ledger, "غيره");

        async Task<List<string>> TablesSeenBy(HttpClient client) =>
            (await client.GetFromJsonAsync<List<ItemTableView>>($"/projects/{ledger.ProjectId}/items/{capital}/tables", Json))!.Select(table => table.Name).ToList();

        Assert.Equal(["القارئ", "غيره"], await TablesSeenBy(ledger.Client));
        Assert.Equal(["القارئ"], await TablesSeenBy(partnerReader));
        Assert.Empty(await TablesSeenBy(otherReader));
    }

    [Fact]
    public async Task BR_31_UC_109_AProjectHasOneCapitalItemSoNoSecondOneShowsOnePartnersBalance()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var category = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories", Json))!.First(row => row.Items.Any(item => item.Type == ItemType.Capital));
        var refund = await ledger.RevenueItemAsync(ItemType.VatRefund);

        var added = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/categories/{category.Id}/items", new AddItemRequest("رأس مال أحمد", ItemType.Capital), Json);
        var retyped = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{refund}/type", new ChangeItemTypeRequest(ItemType.Capital), Json);

        Assert.Equal((HttpStatusCode.Conflict, "capitalItemExists"), (added.StatusCode, await Ledger.CodeOf(added)));
        Assert.Equal((HttpStatusCode.Conflict, "capitalItemExists"), (retyped.StatusCode, await Ledger.CodeOf(retyped)));
    }

    [Fact]
    public async Task UC_109_ACapitalItemStaysOneWhilePartnersAreItsTablesAndTakesNoSubTable()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var capital = await Ledger.CapitalItemOfAsync(ledger.Client, ledger.ProjectId);
        await PartnerAsync(ledger, "نواف");

        var retyped = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{capital}/type", new ChangeItemTypeRequest(ItemType.OtherRevenue), Json);
        var subTable = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{capital}/subgroups", new NameRequest("دفعة أولى"), Json);

        Assert.Equal((HttpStatusCode.Conflict, "itemHasPartners"), (retyped.StatusCode, await Ledger.CodeOf(retyped)));
        Assert.Equal((HttpStatusCode.BadRequest, "itemTakesNoPurchases"), (subTable.StatusCode, await Ledger.CodeOf(subTable)));
    }

    [Fact]
    public async Task UC_109_EveryProjectStartsWithRasAlMalACategoryOfItsOwnFirstAmongTheRevenueWhoseSheetGivesEachPartnersShare()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var categories = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories", Json))!;

        var capital = categories.Single(category => category.Items.Any(item => item.Type == ItemType.Capital));
        var item = capital.Items.Single();
        Assert.Equal(("رأس المال", "رأس المال", true), (capital.Name, item.Name, item.ShowsTablePercentages));
        Assert.Equal(categories.FindIndex(category => category.Items.Any(row => row.Type.IsRevenue())), categories.IndexOf(capital));
        Assert.True(categories.FindLastIndex(category => category.Items.Any(row => !row.Type.IsRevenue())) < categories.IndexOf(capital));
    }

    [Fact]
    public async Task R9_SomeoneWhoMovesNoMoneyIsOfferedNoRevenueItemAndNoPartner()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var refund = await ledger.RevenueItemAsync(ItemType.VatRefund);
        await PartnerAsync(ledger, "نواف");
        var (field, _) = await ledger.MemberAsync("Field");

        var offered = await EntryTablesOf(field, ledger.ProjectId);

        Assert.DoesNotContain(offered.SelectMany(category => category.Items).SelectMany(item => item.Tables), table => table.Kind == ItemTableKind.Partner);
        Assert.DoesNotContain(offered.SelectMany(category => category.Items), item => item.Id == refund || item.IsRevenue);
        Assert.Contains(offered.SelectMany(category => category.Items), item => item.Id == ledger.ItemId);
    }

    private static async Task<List<ItemTableView>> TablesOf(HttpClient client, Guid projectId, Guid itemId) =>
        (await client.GetFromJsonAsync<List<ItemTableView>>($"/projects/{projectId}/items/{itemId}/tables", Json))!;

    private static Task<HttpResponseMessage> DeleteSubgroupAsync(HttpClient client, Guid projectId, Guid itemId, Guid subgroupId) =>
        client.PostAsync($"/projects/{projectId}/items/{itemId}/subgroups/{subgroupId}/delete", null);

    [Fact]
    public async Task ASubgroupNoEntryUsesIsRemoved()
    {
        var (client, projectId, item) = await ItemWithSubgroupsAsync("مواد", "دفان");

        (await DeleteSubgroupAsync(client, projectId, item.Id, item.Subgroups[1].Id)).EnsureSuccessStatusCode();

        Assert.Equal(["", "دفان"], (await CategoriesOf(client, projectId))[3].Items[5].Subgroups.Select(subgroup => subgroup.Name));
    }

    [Fact]
    public async Task ASubgroupAddedAfterARemovalStillGoesLast()
    {
        var (client, projectId, item) = await ItemWithSubgroupsAsync("مواد", "دفان");
        (await DeleteSubgroupAsync(client, projectId, item.Id, item.Subgroups[1].Id)).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync($"/projects/{projectId}/items/{item.Id}/subgroups", new NameRequest("أسمنت"))).EnsureSuccessStatusCode();

        Assert.Equal(["", "دفان", "أسمنت"], (await CategoriesOf(client, projectId))[3].Items[5].Subgroups.Select(subgroup => subgroup.Name));
    }

    [Fact]
    public async Task ASubgroupAnEntryUsesIsNotRemoved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var added = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/subgroups", new NameRequest("مواد"), Json);
        added.EnsureSuccessStatusCode();
        var subgroup = (await CategoriesOf(ledger.Client, ledger.ProjectId)).SelectMany(category => category.Items).Single(item => item.Id == ledger.ItemId).Subgroups[1];
        await ledger.RecordAsync(ledger.Purchase() with { SubgroupId = subgroup.Id });

        var refused = await DeleteSubgroupAsync(ledger.Client, ledger.ProjectId, ledger.ItemId, subgroup.Id);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("subgroupInUse", await CodeOf(refused));
    }

    [Fact]
    public async Task TheGeneralSubgroupIsNeverRemoved()
    {
        var (client, projectId, item) = await ItemWithSubgroupsAsync();

        var refused = await DeleteSubgroupAsync(client, projectId, item.Id, item.Subgroups[0].Id);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("generalSubgroup", await CodeOf(refused));
    }

    [Fact]
    public async Task AC_46_8_AnItemsTablesAreReorderedAsOneListTheGeneralOneToo()
    {
        var (client, projectId, item) = await ItemWithSubgroupsAsync("مواد", "دفان", "أسمنت");

        (await ReorderTablesAsync(client, projectId, item.Id, item.Subgroups[3].Id, item.Subgroups[0].Id, item.Subgroups[1].Id, item.Subgroups[2].Id)).EnsureSuccessStatusCode();

        Assert.Equal(["أسمنت", "", "مواد", "دفان"], (await CategoriesOf(client, projectId))[3].Items[5].Subgroups.Select(subgroup => subgroup.Name));
    }

    [Fact]
    public async Task AC_46_8_AnItemListsItsGeneralTableEachSubgroupAndEachContractWithTheirEntriesAndANewTableGoesLast()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var contractor = (await ledger.Client.GetFromJsonAsync<List<SupplierView>>($"/projects/{ledger.ProjectId}/suppliers", Json))!.Single(supplier => supplier.Name == Supplier.GeneralCashSupplierName);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(contractor.Id, ledger.ItemId, 1000m, false), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/subgroups", new NameRequest("مواد"), Json)).EnsureSuccessStatusCode();
        var purchase = ledger.Purchase(70) with { Submit = true };
        await ledger.RecordAsync(purchase);
        await ledger.SetStatusAsync(purchase.Id, EntryStatus.Approved);

        var tables = await TablesOf(ledger.Client, ledger.ProjectId, ledger.ItemId);

        Assert.Equal([ItemTableKind.General, ItemTableKind.Contract, ItemTableKind.Subgroup], tables.Select(table => table.Kind));
        Assert.Equal((Supplier.GeneralCashSupplierName, "مواد"), (tables[1].Name, tables[2].Name));
        Assert.Equal([1, 0, 0], tables.Select(table => table.Count));
    }

    [Fact]
    public async Task AnOrderOfTablesNamesEveryTableExactlyOnce()
    {
        var (client, projectId, item) = await ItemWithSubgroupsAsync("مواد", "دفان");
        List<Guid> every = [.. item.Subgroups.Select(subgroup => subgroup.Id)];

        foreach (var ids in new List<Guid>[] { [.. every.Skip(1)], [.. every, every[1]], [.. every.Skip(1), Guid.NewGuid()] })
        {
            var refused = await ReorderTablesAsync(client, projectId, item.Id, ids);
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidOrder", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task OneProjectsRoutesNeverReachAnothersSubgroups()
    {
        var (client, projectId, _) = await ItemWithSubgroupsAsync();
        var (_, otherProjectId, other) = await ItemWithSubgroupsAsync("مواد");

        Assert.Equal(HttpStatusCode.NotFound, (await DeleteSubgroupAsync(client, projectId, other.Id, other.Subgroups[1].Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ReorderTablesAsync(client, projectId, other.Id, other.Subgroups.Select(subgroup => subgroup.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/projects/{projectId}/items/{other.Id}/tables")).StatusCode);
        Assert.Equal(["", "مواد"], (await CategoriesOf(await MemberOfAsync(otherProjectId), otherProjectId))[3].Items[5].Subgroups.Select(subgroup => subgroup.Name));
    }

    private async Task<HttpClient> MemberOfAsync(Guid projectId)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Of(Reader));
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static Task<HttpResponseMessage> ChangeTypeAsync(HttpClient client, Guid projectId, Guid itemId, ItemType type) =>
        client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/type", new { type }, Json);

    private static async Task<List<ColumnView>> ColumnsOfAsync(HttpClient client, Guid projectId, Guid itemId) =>
        (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{projectId}/items/{itemId}/columns", Json))!;

    [Fact]
    public async Task AnItemsTypeChangesWhileItHasNoEntriesAndIsOfferedAsItsNewTypeIs()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        Assert.Equal(ItemType.Purchases, item.Type);

        (await ChangeTypeAsync(client, project.Id, item.Id, ItemType.ContractorPayments)).EnsureSuccessStatusCode();

        Assert.Equal(ItemType.ContractorPayments, (await CategoriesOf(client, project.Id))[2].Items[1].Type);
        Assert.DoesNotContain((await OfferedFor(client, project.Id, "purchases")).SelectMany(category => category.Items), offered => offered.Id == item.Id);
        Assert.Contains((await OfferedFor(client, project.Id, "contractorPayments")).SelectMany(category => category.Items), offered => offered.Id == item.Id);
    }

    [Fact]
    public async Task AChangedTypeIsInTheAuditTrailOldAndNew()
    {
        var (user, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];

        (await ChangeTypeAsync(client, project.Id, item.Id, ItemType.Both)).EnsureSuccessStatusCode();

        var record = Assert.Single(await factory.AuditTrailOfAsync(item.Id));
        Assert.Equal((AuditAction.Update, user.Id, "Items"), (record.Action, record.UserId, record.TableName));
        Assert.Equal((int)ItemType.Purchases, JsonDocument.Parse(record.Before).RootElement.GetProperty("Type").GetInt32());
        Assert.Equal((int)ItemType.Both, JsonDocument.Parse(record.After).RootElement.GetProperty("Type").GetInt32());
    }

    [Fact]
    public async Task SubTable_AnItemOfContractorPaymentsWithAContractMayBecomeBoth()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var category = (await CategoriesOf(ledger.Client, ledger.ProjectId))[1];
        var item = await AddItemAsync(ledger.Client, ledger.ProjectId, category.Id, "دفعات العظم", ItemType.ContractorPayments);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, item.Id, 1000m, false), Json)).EnsureSuccessStatusCode();

        (await ChangeTypeAsync(ledger.Client, ledger.ProjectId, item.Id, ItemType.Both)).EnsureSuccessStatusCode();

        Assert.Equal(ItemType.Both, (await CategoriesOf(ledger.Client, ledger.ProjectId)).SelectMany(row => row.Items).Single(row => row.Id == item.Id).Type);
    }

    [Fact]
    public async Task SubTable_AnItemOfPurchasesWithAnEntryMayBecomeBoth()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var category = (await CategoriesOf(ledger.Client, ledger.ProjectId))[1];
        var item = await AddItemAsync(ledger.Client, ledger.ProjectId, category.Id, "الجلي", ItemType.Purchases);
        await ledger.RecordAsync(ledger.Purchase() with { ItemId = item.Id });

        (await ChangeTypeAsync(ledger.Client, ledger.ProjectId, item.Id, ItemType.Both)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SubTable_AnItemThatTakesNoPurchasesHasNoSubTable()
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[1];

        foreach (var type in new[] { ItemType.ContractorPayments, ItemType.OtherRevenue })
        {
            var item = await AddItemAsync(client, project.Id, category.Id, $"بند {type}", type);
            var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/subgroups", new NameRequest("الجلاي"));

            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("itemTakesNoPurchases", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task SubTable_ASubTableAddedIsOfferedAndTakesAPurchase()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/subgroups", new NameRequest("الجلاي"), Json)).EnsureSuccessStatusCode();

        var table = (await EntryTablesOf(ledger.Client, ledger.ProjectId)).SelectMany(category => category.Items).Single(item => item.Id == ledger.ItemId).Tables.Single(row => row.Name == "الجلاي");
        var purchase = ledger.Purchase() with { SubgroupId = table.Id };
        await ledger.RecordAsync(purchase);

        Assert.Equal(table.Id, await ledger.ReadAsync(db => db.ExpenseDetails.Where(row => row.EntryId == purchase.Id).Select(row => row.SubgroupId).SingleAsync()));
    }

    [Fact]
    public async Task AnItemWithAnEntryKeepsItsType()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase());

        var refused = await ChangeTypeAsync(ledger.Client, ledger.ProjectId, ledger.ItemId, ItemType.ContractorPayments);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("itemHasEntries", await CodeOf(refused));
    }

    [Fact]
    public async Task AnItemAContractNamesKeepsItsType()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var contractor = (await ledger.Client.GetFromJsonAsync<List<SupplierView>>($"/projects/{ledger.ProjectId}/suppliers", Json))!.Single(supplier => supplier.Name == Supplier.GeneralCashSupplierName);
        var item = (await OfferedFor(ledger.Client, ledger.ProjectId, "contractorPayments")).SelectMany(category => category.Items).First();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(contractor.Id, item.Id, 34000m, false), Json)).EnsureSuccessStatusCode();

        var refused = await ChangeTypeAsync(ledger.Client, ledger.ProjectId, item.Id, ItemType.Purchases);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("itemHasContract", await CodeOf(refused));
    }

    [Fact]
    public async Task AChangedTypeAsksTheColumnsOfTheNewTypeAndKeepsWhatAPersonAdded()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", new AddColumnRequest("المساحة", ColumnDataType.Number, false, false, []), Json)).EnsureSuccessStatusCode();
        var template = (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{project.Id}/default-columns/contractorPayments", Json))!;

        (await ChangeTypeAsync(client, project.Id, item.Id, ItemType.ContractorPayments)).EnsureSuccessStatusCode();

        var columns = await ColumnsOfAsync(client, project.Id, item.Id);
        Assert.Equal([.. template.Select(column => column.Key), columns[^1].Key], columns.Select(column => column.Key));
        Assert.Equal("المساحة", columns[^1].Name);
    }

    [Fact]
    public async Task ChangingATypeThereAndBackLeavesOneOfEachColumn()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/purchases", new AddDefaultColumnRequest("رقم أمر الشراء", ColumnDataType.Text, false, false, [], true), Json)).EnsureSuccessStatusCode();

        (await ChangeTypeAsync(client, project.Id, item.Id, ItemType.ContractorPayments)).EnsureSuccessStatusCode();
        (await ChangeTypeAsync(client, project.Id, item.Id, ItemType.Purchases)).EnsureSuccessStatusCode();

        var keys = (await ColumnsOfAsync(client, project.Id, item.Id)).Select(column => column.Key).ToList();
        Assert.Equal(keys.Distinct().Count(), keys.Count);
        Assert.Equal((await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{project.Id}/default-columns/purchases", Json))!.Select(column => column.Key), keys);
    }

    [Fact]
    public async Task OnlyFinanceChangesATypeAndNeverAnotherProjects()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        var (_, otherClient, other) = await FinanceAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await ChangeTypeAsync(await MemberAsync(project, Field), project.Id, item.Id, ItemType.Both)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ChangeTypeAsync(otherClient, other.Id, item.Id, ItemType.Both)).StatusCode);
    }

    private static Task<HttpResponseMessage> TablePercentagesAsync(HttpClient client, Guid projectId, Guid itemId, bool isOn) =>
        client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/table-percentages", new TablePercentagesRequest(isOn), Json);

    [Fact]
    public async Task AC_46_9_AnItemsChoiceToCalculateEachTablesPercentageIsKeptAndInTheAuditTrail()
    {
        var (user, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        Assert.False(item.ShowsTablePercentages);

        (await TablePercentagesAsync(client, project.Id, item.Id, true)).EnsureSuccessStatusCode();

        Assert.True((await CategoriesOf(client, project.Id))[2].Items[1].ShowsTablePercentages);
        var record = Assert.Single(await factory.AuditTrailOfAsync(item.Id));
        Assert.Equal((AuditAction.Update, user.Id), (record.Action, record.UserId));
        Assert.True(JsonDocument.Parse(record.After).RootElement.GetProperty("ShowsTablePercentages").GetBoolean());
    }

    [Fact]
    public async Task OnlyFinanceChoosesTablePercentagesAndNeverInAnotherProject()
    {
        var (_, client, project) = await FinanceAsync();
        var item = (await CategoriesOf(client, project.Id))[2].Items[1];
        var (_, otherClient, other) = await FinanceAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await TablePercentagesAsync(await MemberAsync(project, Field), project.Id, item.Id, true)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await TablePercentagesAsync(otherClient, other.Id, item.Id, true)).StatusCode);
    }

    [Fact]
    public async Task FR_38_AnItemIsAddedOnItsSideAndRevenueEntryOffersEveryRevenueItemAndNoExpenseOne()
    {
        var (_, client, project) = await FinanceAsync();
        var category = await AddCategoryAsync(client, project.Id, "إيجارات");

        var rent = await AddItemAsync(client, project.Id, category.Id, "إيجار المكتب", ItemType.OtherRevenue);

        Assert.Equal(ItemType.OtherRevenue, rent.Type);
        var revenue = (await OfferedFor(client, project.Id, "otherRevenue")).SelectMany(row => row.Items).Select(item => item.Type).ToList();
        Assert.Equal(4, revenue.Count);
        Assert.All(revenue, type => Assert.True(type.IsRevenue()));
        Assert.DoesNotContain((await OfferedFor(client, project.Id, "purchases")).SelectMany(row => row.Items), item => item.Type.IsRevenue());
    }

    [Fact]
    public async Task FR_38_ARevenueItemAsksNoColumns()
    {
        var (_, client, project) = await FinanceAsync();
        var sale = (await CategoriesOf(client, project.Id)).SelectMany(category => category.Items).Single(item => item.Type == ItemType.Sale);

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{sale.Id}/columns", new AddColumnRequest("المشتري", ColumnDataType.Text, false, false, []), Json);

        Assert.Equal((HttpStatusCode.BadRequest, "revenueTakesNoColumns"), (refused.StatusCode, await CodeOf(refused)));
    }
}
