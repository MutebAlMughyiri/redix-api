using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ColumnsTests(MigratedApiFactory factory)
{
    private const string Password = "Columns-pass-1";
    private const string Finance = "Finance";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private static readonly List<string> CoreColumns = ["date", "supplier", "invoiceNumber", "total", "account", "attachment"];

    private static AddColumnRequest AreaColumn => new("المساحة م²", ColumnDataType.Number, true, true, []);

    private async Task<(User User, HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (user, client, project);
    }

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static async Task<List<CategoryView>> CategoriesOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories", Json))!;

    /// <summary>The first item of the project with <paramref name="type"/>, skipping <paramref name="skip"/>.</summary>
    private static async Task<ItemView> ItemOfType(HttpClient client, Guid projectId, ItemType type, int skip = 0) =>
        (await CategoriesOf(client, projectId)).SelectMany(category => category.Items).Where(item => item.Type == type).Skip(skip).First();

    private static async Task<List<ColumnView>> ColumnsOf(HttpClient client, Guid projectId, Guid itemId) =>
        (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{projectId}/items/{itemId}/columns", Json))!;

    private static async Task<List<ColumnView>> TemplateOf(HttpClient client, Guid projectId, string itemType = "both") =>
        (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{projectId}/default-columns/{itemType}", Json))!;

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Task<HttpResponseMessage> AddToTemplateAsync(HttpClient client, Guid projectId, bool applyToAllItems, string name = "رقم أمر الشراء", string itemType = "both") =>
        client.PostAsJsonAsync($"/projects/{projectId}/default-columns/{itemType}", new AddDefaultColumnRequest(name, ColumnDataType.Text, false, true, [], applyToAllItems));

    private static EditDefaultColumnRequest Renamed(ColumnView column, string name, bool applyToAllItems) => new(name, column.IsRequired, column.ShowInReports, applyToAllItems);

    [Fact]
    public async Task TheCoreColumnsOfAPurchaseAreTheSixLeftOfBr20()
    {
        var (_, client, project) = await FinanceAsync();

        var template = await TemplateOf(client, project.Id, "purchases");

        Assert.Equal(CoreColumns.Order(), template.Where(column => column.IsCore).Select(column => column.Key).Order());
        Assert.Equal(["product", "unitPrice", "quantity", "paymentMethod", "notes"], template.Where(column => !column.IsCore).Select(column => column.Key));
    }

    [Fact]
    public async Task ATemplateColumnIsNotCustom()
    {
        var (_, client, project) = await FinanceAsync();

        Assert.All(await TemplateOf(client, project.Id), column => Assert.False(column.IsCustom));
    }

    [Fact]
    public async Task AnItemOfBothTypesHoldsThePurchaseColumnsThenWhatOnlyAContractorPaymentHas()
    {
        var (_, client, project) = await FinanceAsync();
        var purchases = await TemplateOf(client, project.Id, "purchases");

        var both = await TemplateOf(client, project.Id);

        Assert.Equal(purchases.Select(column => column.Key), both.Take(purchases.Count).Select(column => column.Key));
        Assert.Equal(["contract", "amount", "outsideContract"], both.Skip(purchases.Count).Select(column => column.Key));
        Assert.All(both.Skip(purchases.Count), column => Assert.False(column.IsRequired));
    }

    [Theory]
    [InlineData(ItemType.Purchases, "purchases")]
    [InlineData(ItemType.ContractorPayments, "contractorPayments")]
    [InlineData(ItemType.Both, "both")]
    public async Task ANewItemInheritsTheDefaultColumnsOfItsType(ItemType type, string typeInUrl)
    {
        var (_, client, project) = await FinanceAsync();
        var category = (await CategoriesOf(client, project.Id))[3];

        var added = await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items", new AddItemRequest("بند الدرج", type));
        var item = (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ItemView>(Json))!;

        var expected = await TemplateOf(client, project.Id, typeInUrl);
        var columns = await ColumnsOf(client, project.Id, item.Id);
        Assert.Equal(expected.Select(column => (column.Key, column.Name, column.DataType, column.IsCore, column.IsRequired, column.ShowInReports)),
            columns.Select(column => (column.Key, column.Name, column.DataType, column.IsCore, column.IsRequired, column.ShowInReports)));
    }

    [Fact]
    public async Task ATemplateItemIsCopiedWithTheColumnsOfItsType()
    {
        var (_, client, project) = await FinanceAsync();

        var steel = await ItemOfType(client, project.Id, ItemType.Purchases);
        var payments = await ItemOfType(client, project.Id, ItemType.ContractorPayments);

        Assert.Equal((await TemplateOf(client, project.Id, "purchases")).Select(column => column.Key), (await ColumnsOf(client, project.Id, steel.Id)).Select(column => column.Key));
        Assert.Equal((await TemplateOf(client, project.Id, "contractorPayments")).Select(column => column.Key), (await ColumnsOf(client, project.Id, payments.Id)).Select(column => column.Key));
    }

    [Fact]
    public async Task AColumnAddedToOneItemIsAskedInThatItemOnly()
    {
        var (_, client, project) = await FinanceAsync();
        var tiles = await ItemOfType(client, project.Id, ItemType.Both);
        var other = await ItemOfType(client, project.Id, ItemType.Both, skip: 1);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{tiles.Id}/columns", AreaColumn)).EnsureSuccessStatusCode();

        var area = (await ColumnsOf(client, project.Id, tiles.Id)).Last();
        Assert.Equal(("المساحة م²", ColumnDataType.Number, true, true, true, false), (area.Name, area.DataType, area.IsRequired, area.ShowInReports, area.IsCustom, area.IsCore));
        Assert.DoesNotContain(await ColumnsOf(client, project.Id, other.Id), column => column.Name == "المساحة م²");
        Assert.DoesNotContain(await TemplateOf(client, project.Id), column => column.Name == "المساحة م²");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("x")]
    public async Task AColumnNeedsANameOfAtMost100Characters(string name)
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var unacceptable = name == "x" ? new string('x', Project.NameMaxLength + 1) : name;
        var column = (await ColumnsOf(client, project.Id, item.Id))[0];

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn with { Name = unacceptable }),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{column.Id}", new EditColumnRequest(unacceptable, column.IsRequired, column.ShowInReports)),
            await AddToTemplateAsync(client, project.Id, false, unacceptable),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidName", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task APickListNeedsAChoice()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);

        foreach (var options in new List<string>[] { [], ["  "], [new string('x', Project.NameMaxLength + 1)] })
        {
            var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn with { DataType = ColumnDataType.List, Options = options });
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidOptions", await CodeOf(refused));
        }
    }

    [Fact]
    public async Task APickListKeepsItsChoicesAndAnotherTypeNone()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn with { Name = "اللون", DataType = ColumnDataType.List, Options = [" أبيض ", "", "رمادي", "أبيض"] })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn with { Options = ["لا يخصه"] })).EnsureSuccessStatusCode();

        var columns = await ColumnsOf(client, project.Id, item.Id);
        Assert.Equal(["أبيض", "رمادي"], columns.Single(column => column.Name == "اللون").Options);
        Assert.Empty(columns.Single(column => column.Name == "المساحة م²").Options);
    }

    [Fact]
    public async Task APersonCannotAddAFileColumn()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn with { DataType = ColumnDataType.File });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidDataType", await CodeOf(refused));
    }

    [Fact]
    public async Task ACoreColumnIsRenamedAndReordered()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var columns = await ColumnsOf(client, project.Id, item.Id);
        var date = columns.Single(column => column.Key == "date");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{date.Id}", new EditColumnRequest(" تاريخ الفاتورة ", date.IsRequired, date.ShowInReports))).EnsureSuccessStatusCode();
        List<Guid> reversed = [.. columns.Select(column => column.Id).Reverse()];
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/reorder", new ReorderRequest(reversed))).EnsureSuccessStatusCode();

        var after = await ColumnsOf(client, project.Id, item.Id);
        Assert.Equal(reversed, after.Select(column => column.Id));
        Assert.Equal("تاريخ الفاتورة", after.Last().Name);
    }

    [Fact]
    public async Task ACoreColumnChangesNothingButItsName()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var date = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "date");

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{date.Id}", new EditColumnRequest(date.Name, !date.IsRequired, date.ShowInReports));

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("coreColumn", await CodeOf(refused));
    }

    [Fact]
    public async Task AColumnThatIsNotCoreChangesWhatEntryAsksAndWhatPrints()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var method = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "paymentMethod");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{method.Id}", new EditColumnRequest("وسيلة الدفع", true, true))).EnsureSuccessStatusCode();

        var changed = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "paymentMethod");
        Assert.Equal(("وسيلة الدفع", true, true), (changed.Name, changed.IsRequired, changed.ShowInReports));
    }

    [Fact]
    public async Task ATypeNeverChanges()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var quantity = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "quantity");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{quantity.Id}", new { name = quantity.Name, isRequired = false, showInReports = true, dataType = "text" })).EnsureSuccessStatusCode();

        Assert.Equal(ColumnDataType.Number, (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "quantity").DataType);
    }

    [Theory]
    [InlineData("date")]
    [InlineData("supplier")]
    [InlineData("invoiceNumber")]
    [InlineData("total")]
    [InlineData("account")]
    [InlineData("attachment")]
    public async Task ACoreColumnIsNeverDeleted(string key)
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var column = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == key);
        var inTemplate = (await TemplateOf(client, project.Id)).Single(column => column.Key == key);

        var refused = await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{column.Id}/delete", null);
        var refusedInTemplate = await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{inTemplate.Id}/delete", new ApplyToAllItemsRequest(true));

        Assert.Equal((HttpStatusCode.Conflict, "coreColumn"), (refused.StatusCode, await CodeOf(refused)));
        Assert.Equal((HttpStatusCode.Conflict, "coreColumn"), (refusedInTemplate.StatusCode, await CodeOf(refusedInTemplate)));
        Assert.Contains(await ColumnsOf(client, project.Id, item.Id), kept => kept.Key == key);
    }

    [Fact]
    public async Task ADeletedColumnLeavesEntryAndItsRowStays()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn)).EnsureSuccessStatusCode();
        var area = (await ColumnsOf(client, project.Id, item.Id)).Last();

        (await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{area.Id}/delete", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await ColumnsOf(client, project.Id, item.Id), column => column.Id == area.Id);
        await factory.WithDatabaseAsync(async db => Assert.True((await db.ItemColumns.SingleAsync(column => column.Id == area.Id)).IsHidden));
    }

    [Fact]
    public async Task ANonCoreColumnOfTheTemplateIsDeletedOnAnItem()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var method = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "paymentMethod");

        (await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{method.Id}/delete", null)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await ColumnsOf(client, project.Id, item.Id), column => column.Key == "paymentMethod");
    }

    [Fact]
    public async Task ADeletedColumnCanBeDeletedOrChangedNoMore()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var method = (await ColumnsOf(client, project.Id, item.Id)).Single(column => column.Key == "paymentMethod");
        await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{method.Id}/delete", null);

        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{method.Id}/delete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{method.Id}", new EditColumnRequest("الوحدة", false, false))).StatusCode);
    }

    [Fact]
    public async Task AnOrderMustNameEveryVisibleColumnExactlyOnce()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var ids = (await ColumnsOf(client, project.Id, item.Id)).Select(column => column.Id).ToList();
        var templateIds = (await TemplateOf(client, project.Id)).Select(column => column.Id).ToList();

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/reorder", new ReorderRequest([.. ids.Skip(1)]));
        var refusedInTemplate = await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/reorder", new ReorderDefaultColumnsRequest([.. templateIds.Skip(1)], true));

        Assert.Equal((HttpStatusCode.BadRequest, "invalidOrder"), (refused.StatusCode, await CodeOf(refused)));
        Assert.Equal((HttpStatusCode.BadRequest, "invalidOrder"), (refusedInTemplate.StatusCode, await CodeOf(refusedInTemplate)));
    }

    [Fact]
    public async Task ATemplateChangeLeavesExistingItemsAloneUnlessApplied()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var before = (await ColumnsOf(client, project.Id, item.Id)).Select(column => (column.Key, column.Name)).ToList();

        (await AddToTemplateAsync(client, project.Id, applyToAllItems: false)).EnsureSuccessStatusCode();
        var template = await TemplateOf(client, project.Id);
        var method = template.Single(column => column.Key == "paymentMethod");
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{method.Id}", Renamed(method, "وسيلة الدفع", false))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/reorder", new ReorderDefaultColumnsRequest([.. template.Select(column => column.Id).Reverse()], false))).EnsureSuccessStatusCode();
        var notes = template.Single(column => column.Key == "notes");
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{notes.Id}/delete", new ApplyToAllItemsRequest(false))).EnsureSuccessStatusCode();

        Assert.Equal(before, (await ColumnsOf(client, project.Id, item.Id)).Select(column => (column.Key, column.Name)));
    }

    [Fact]
    public async Task ANewItemStartsFromTheTemplateAsItIsNow()
    {
        var (_, client, project) = await FinanceAsync();
        (await AddToTemplateAsync(client, project.Id, applyToAllItems: false)).EnsureSuccessStatusCode();
        var notes = (await TemplateOf(client, project.Id)).Single(column => column.Key == "notes");
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{notes.Id}/delete", new ApplyToAllItemsRequest(false))).EnsureSuccessStatusCode();
        var category = (await CategoriesOf(client, project.Id))[3];

        var added = await client.PostAsJsonAsync($"/projects/{project.Id}/categories/{category.Id}/items", new AddItemRequest("بند الدرج", ItemType.Both));
        var item = (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ItemView>(Json))!;

        var columns = await ColumnsOf(client, project.Id, item.Id);
        Assert.Contains(columns, column => column.Name == "رقم أمر الشراء" && column.IsCustom);
        Assert.DoesNotContain(columns, column => column.Key == "notes");
    }

    [Fact]
    public async Task AColumnAppliedToAllReachesEveryItemOfThatTypeAndNoOther()
    {
        var (_, client, project) = await FinanceAsync();
        var both = await ItemOfType(client, project.Id, ItemType.Both, skip: 3);
        var steel = await ItemOfType(client, project.Id, ItemType.Purchases);
        await client.PostAsync($"/projects/{project.Id}/items/{both.Id}/disable", null);

        (await AddToTemplateAsync(client, project.Id, applyToAllItems: true)).EnsureSuccessStatusCode();

        var key = (await TemplateOf(client, project.Id)).Last().Key;
        var last = (await ColumnsOf(client, project.Id, both.Id)).Last();
        Assert.Equal((key, "رقم أمر الشراء"), (last.Key, last.Name));
        Assert.DoesNotContain(await ColumnsOf(client, project.Id, steel.Id), column => column.Key == key);
    }

    [Fact]
    public async Task ARenameAppliedToAllReachesEveryItemOfThatType()
    {
        var (_, client, project) = await FinanceAsync();
        var first = await ItemOfType(client, project.Id, ItemType.Both);
        var second = await ItemOfType(client, project.Id, ItemType.Both, skip: 1);
        var steel = await ItemOfType(client, project.Id, ItemType.Purchases);
        var date = (await TemplateOf(client, project.Id)).Single(column => column.Key == "date");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{date.Id}", Renamed(date, "تاريخ الدفع", true))).EnsureSuccessStatusCode();

        Assert.Equal("تاريخ الدفع", (await ColumnsOf(client, project.Id, first.Id)).Single(column => column.Key == "date").Name);
        Assert.Equal("تاريخ الدفع", (await ColumnsOf(client, project.Id, second.Id)).Single(column => column.Key == "date").Name);
        Assert.Equal("التاريخ", (await ColumnsOf(client, project.Id, steel.Id)).Single(column => column.Key == "date").Name);
    }

    [Fact]
    public async Task ADeleteAppliedToAllHidesItOnEveryItemOfThatType()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var notes = (await TemplateOf(client, project.Id)).Single(column => column.Key == "notes");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{notes.Id}/delete", new ApplyToAllItemsRequest(true))).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await TemplateOf(client, project.Id), column => column.Key == "notes");
        Assert.DoesNotContain(await ColumnsOf(client, project.Id, item.Id), column => column.Key == "notes");
    }

    [Fact]
    public async Task AnOrderAppliedToAllKeepsWhatOnlyThatItemHasAfterTheRest()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn)).EnsureSuccessStatusCode();
        var template = await TemplateOf(client, project.Id);
        List<ColumnView> reversed = [.. template.AsEnumerable().Reverse()];

        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/reorder", new ReorderDefaultColumnsRequest([.. reversed.Select(column => column.Id)], true))).EnsureSuccessStatusCode();

        var columns = await ColumnsOf(client, project.Id, item.Id);
        Assert.Equal(reversed.Select(column => column.Key), columns.Take(reversed.Count).Select(column => column.Key));
        Assert.Equal("المساحة م²", columns.Last().Name);
    }

    [Fact]
    public async Task OneProjectsTemplateChangesNoOtherProject()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, otherClient, other) = await FinanceAsync("غصون 5");

        (await AddToTemplateAsync(client, project.Id, applyToAllItems: true)).EnsureSuccessStatusCode();

        Assert.DoesNotContain(await TemplateOf(otherClient, other.Id), column => column.IsCustom);
        var item = await ItemOfType(otherClient, other.Id, ItemType.Both);
        Assert.DoesNotContain(await ColumnsOf(otherClient, other.Id, item.Id), column => column.IsCustom);
        await factory.WithDatabaseAsync(async db => Assert.False(await db.DefaultColumns.AnyAsync(column => column.ProjectId == StructureSeed.TemplateId && column.Name == "رقم أمر الشراء")));
    }

    [Fact]
    public async Task AnEarlierProjectsColumnsAreCopiedWithoutWhatItDeleted()
    {
        var (user, client, earlier) = await FinanceAsync();
        var item = await ItemOfType(client, earlier.Id, ItemType.Both);
        (await client.PostAsJsonAsync($"/projects/{earlier.Id}/items/{item.Id}/columns", AreaColumn)).EnsureSuccessStatusCode();
        var method = (await ColumnsOf(client, earlier.Id, item.Id)).Single(column => column.Key == "paymentMethod");
        await client.PostAsync($"/projects/{earlier.Id}/items/{item.Id}/columns/{method.Id}/delete", null);
        (await AddToTemplateAsync(client, earlier.Id, applyToAllItems: false)).EnsureSuccessStatusCode();

        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });
        var project = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;
        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{project.Id}/approve", null)).EnsureSuccessStatusCode();
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));

        var copy = await ItemOfType(client, project.Id, ItemType.Both);
        var columns = await ColumnsOf(client, project.Id, copy.Id);
        Assert.Contains(columns, column => column.Name == "المساحة م²");
        Assert.DoesNotContain(columns, column => column.Key == "paymentMethod");
        Assert.Contains(await TemplateOf(client, project.Id), column => column.Name == "رقم أمر الشراء");
    }

    [Fact]
    public async Task EveryoneWithAccessReadsColumnsAndOnlyFinanceChangesThem()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);

        foreach (var role in new[] { "Field", "Reader", "Manager" })
        {
            var member = await MemberAsync(project, role);
            Assert.NotEmpty(await ColumnsOf(member, project.Id, item.Id));
            Assert.NotEmpty(await TemplateOf(member, project.Id));
            Assert.Equal(HttpStatusCode.Forbidden, (await member.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await AddToTemplateAsync(member, project.Id, true)).StatusCode);
        }
    }

    [Fact]
    public async Task ARouteOfOneProjectNeverReachesAnothersColumns()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, otherClient, other) = await FinanceAsync("غصون 5");
        var item = await ItemOfType(otherClient, other.Id, ItemType.Both);
        var column = (await ColumnsOf(otherClient, other.Id, item.Id)).Single(row => row.Key == "paymentMethod");
        var inTemplate = (await TemplateOf(otherClient, other.Id)).Single(row => row.Key == "paymentMethod");

        foreach (var missed in new[]
        {
            await client.GetAsync($"/projects/{project.Id}/items/{item.Id}/columns"),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/reorder", new ReorderRequest([column.Id])),
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns/{column.Id}", new EditColumnRequest("سرقة", false, false)),
            await client.PostAsync($"/projects/{project.Id}/items/{item.Id}/columns/{column.Id}/delete", null),
            await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{inTemplate.Id}", Renamed(inTemplate, "سرقة", true)),
            await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{inTemplate.Id}/delete", new ApplyToAllItemsRequest(true)),
        })
        {
            Assert.Equal(HttpStatusCode.NotFound, missed.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/default-columns/both")).StatusCode);
    }

    [Fact]
    public async Task AFinishedProjectRefusesColumnChangesAndStaysReadable()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn),
            await AddToTemplateAsync(client, project.Id, true),
        })
        {
            Assert.Equal((HttpStatusCode.Conflict, "projectFinished"), (refused.StatusCode, await CodeOf(refused)));
        }
        Assert.NotEmpty(await ColumnsOf(client, project.Id, item.Id));
    }

    [Fact]
    public async Task AnUnknownItemTypeInTheAddressIsABadRequest()
    {
        var (_, client, project) = await FinanceAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/projects/{project.Id}/default-columns/rent")).StatusCode);
    }

    private static async Task<(HttpClient Client, Guid ProjectId, Guid ItemId, ColumnView Column)> PickListAsync(HttpClient client, Guid projectId)
    {
        var item = await ItemOfType(client, projectId, ItemType.Both);
        (await client.PostAsJsonAsync($"/projects/{projectId}/items/{item.Id}/columns", new AddColumnRequest("الجهة", ColumnDataType.List, false, false, ["شرق", "غرب"]), Json)).EnsureSuccessStatusCode();
        return (client, projectId, item.Id, (await ColumnsOf(client, projectId, item.Id)).Last());
    }

    private static Task<HttpResponseMessage> EditItemColumnAsync(HttpClient client, Guid projectId, Guid itemId, ColumnView column, params List<string> newOptions) =>
        client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/columns/{column.Id}", new { name = column.Name, isRequired = column.IsRequired, showInReports = column.ShowInReports, newOptions }, Json);

    [Fact]
    public async Task APickListGainsChoicesAndKeepsItsOwnInTheirOrder()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, projectId, itemId, column) = await PickListAsync(client, project.Id);

        (await EditItemColumnAsync(client, projectId, itemId, column, " شمال ", "", "شرق", "جنوب", "شمال")).EnsureSuccessStatusCode();

        Assert.Equal(["شرق", "غرب", "شمال", "جنوب"], (await ColumnsOf(client, projectId, itemId)).Last().Options);
    }

    [Fact]
    public async Task AnEditThatGivesNoNewChoiceChangesNoChoice()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, projectId, itemId, column) = await PickListAsync(client, project.Id);

        (await client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/columns/{column.Id}", new EditColumnRequest("الجهة الأصلية", false, false), Json)).EnsureSuccessStatusCode();

        Assert.Equal(["شرق", "غرب"], (await ColumnsOf(client, projectId, itemId)).Last().Options);
    }

    [Fact]
    public async Task ANewChoiceIsWithinTheLimitOfAName()
    {
        var (_, client, project) = await FinanceAsync();
        var (_, projectId, itemId, column) = await PickListAsync(client, project.Id);

        var refused = await EditItemColumnAsync(client, projectId, itemId, column, new string('x', Project.NameMaxLength + 1));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidOptions", await CodeOf(refused));
    }

    [Fact]
    public async Task OnlyAPickListAPersonAddedGainsChoices()
    {
        var (_, client, project) = await FinanceAsync();
        var item = await ItemOfType(client, project.Id, ItemType.Both);
        var columns = await ColumnsOf(client, project.Id, item.Id);
        (await client.PostAsJsonAsync($"/projects/{project.Id}/items/{item.Id}/columns", AreaColumn, Json)).EnsureSuccessStatusCode();
        var area = (await ColumnsOf(client, project.Id, item.Id)).Last();

        Assert.Equal("invalidOptions", await CodeOf(await EditItemColumnAsync(client, project.Id, item.Id, area, "شمال")));
        Assert.Equal("invalidOptions", await CodeOf(await EditItemColumnAsync(client, project.Id, item.Id, columns.Single(column => column.Key == "paymentMethod"), "شمال")));
    }

    [Fact]
    public async Task AnEntryCanHoldTheChoiceAListGained()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/columns", new AddColumnRequest("الجهة", ColumnDataType.List, false, false, ["شرق"]), Json)).EnsureSuccessStatusCode();
        var column = (await ColumnsOf(ledger.Client, ledger.ProjectId, ledger.ItemId)).Last();
        await ledger.AssertRefusedAsync(ledger.Purchase() with { CustomValues = new() { [column.Key] = "شمال" } }, "invalidColumnValue");

        (await EditItemColumnAsync(ledger.Client, ledger.ProjectId, ledger.ItemId, column, "شمال")).EnsureSuccessStatusCode();

        await ledger.RecordAsync(ledger.Purchase() with { CustomValues = new() { [column.Key] = "شمال" } });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AChoiceTheTemplateGainsReachesItemsOnlyWhenApplied(bool applyToAllItems)
    {
        var (_, client, project) = await FinanceAsync();
        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both", new AddDefaultColumnRequest("الجهة", ColumnDataType.List, false, false, ["شرق"], true), Json)).EnsureSuccessStatusCode();
        var column = (await TemplateOf(client, project.Id)).Last();
        var item = await ItemOfType(client, project.Id, ItemType.Both);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/default-columns/both/{column.Id}", new { name = column.Name, isRequired = false, showInReports = false, applyToAllItems, newOptions = new[] { "شمال" } }, Json)).EnsureSuccessStatusCode();

        Assert.Equal(["شرق", "شمال"], (await TemplateOf(client, project.Id)).Last().Options);
        Assert.Equal(applyToAllItems ? ["شرق", "شمال"] : ["شرق"], (await ColumnsOf(client, project.Id, item.Id)).Single(row => row.Key == column.Key).Options);
    }
}
