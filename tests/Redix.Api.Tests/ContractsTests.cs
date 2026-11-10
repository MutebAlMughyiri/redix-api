using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(SecondDatabaseCollection.Name)]
public class ContractsTests(MigratedApiFactory factory)
{
    private const string Password = "Contracts-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<decimal> UnacceptableValues => [0m, -1m, 1.234m, Money.Largest + 1];

    public static TheoryData<decimal> UnacceptableParts => [-1m, 1.234m, Money.Largest + 1];

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name, "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (client, project);
    }

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static async Task<List<ContractView>> ContractsOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<ContractView>>($"/projects/{projectId}/contracts", Json))!;

    private static async Task<SupplierView> ContractorOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{projectId}/suppliers", Json))!.Single(supplier => supplier.Name == Supplier.GeneralCashSupplierName);

    private static async Task<ItemView> ItemOf(HttpClient client, Guid projectId, ItemType itemType = ItemType.ContractorPayments) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories/offered?itemType={itemType}", Json))!.SelectMany(category => category.Items).First();

    private static async Task<AddContractRequest> RequestFor(HttpClient client, ProjectView project, decimal value = 34000m, bool isOpenValue = false) =>
        new((await ContractorOf(client, project.Id)).Id, (await ItemOf(client, project.Id)).Id, value, isOpenValue);

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid projectId, AddContractRequest request) =>
        client.PostAsJsonAsync($"/projects/{projectId}/contracts", request, Json);

    private static async Task<ContractView> AddAsync(HttpClient client, ProjectView project, decimal value = 34000m)
    {
        (await PostAsync(client, project.Id, await RequestFor(client, project, value))).EnsureSuccessStatusCode();
        return (await ContractsOf(client, project.Id)).Last();
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static Contract ContractOf(decimal value, bool isOpenValue = false)
    {
        var contract = new Contract();
        contract.SetTerms(value, isOpenValue, 0, 0, "");
        return contract;
    }

    [Fact]
    public async Task FinanceAddsAContractWithItsContractorItemAndValue()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project) with { UnitPrice = 380m, Quantity = 89.47m, Notes = " دفعات شهرية " };

        (await PostAsync(client, project.Id, request)).EnsureSuccessStatusCode();

        var contract = Assert.Single(await ContractsOf(client, project.Id));
        Assert.Equal(Supplier.GeneralCashSupplierName, contract.SupplierName);
        Assert.Equal((await ItemOf(client, project.Id)).Name, contract.ItemName);
        Assert.Equal(34000m, contract.Value);
        Assert.Equal(380m, contract.UnitPrice);
        Assert.Equal(89.47m, contract.Quantity);
        Assert.Equal("دفعات شهرية", contract.Notes);
        Assert.True(contract.IsActive);
    }

    [Fact]
    public async Task ANewContractHasItsWholeValueRemaining()
    {
        var (client, project) = await FinanceAsync();

        var contract = await AddAsync(client, project);

        Assert.Equal(0m, contract.Paid);
        Assert.Equal(34000m, contract.Remaining);
        Assert.False(contract.IsOverrun);
    }

    [Fact]
    public void TheRemainingIsTheValueLessWhatWasPaidInsideTheContract()
    {
        var contract = ContractOf(34000m);

        Assert.Equal(4000m, contract.RemainingAfter(30000m));
        Assert.False(contract.IsOverrunBy(30000m));
        Assert.False(contract.IsOverrunBy(34000m));
    }

    [Fact]
    public void APaymentBeyondTheValueLeavesANegativeRemainingWithAnAlert()
    {
        var view = ContractView.Of(ContractOf(34000m), new ContractMoney(35000m, 0), "مبلط", "البلاط");

        Assert.Equal(-1000m, view.Remaining);
        Assert.True(view.IsOverrun);
    }

    [Fact]
    public void AnOpenValueContractAddsUpWithNoRemainingAndNoAlert()
    {
        var view = ContractView.Of(ContractOf(0m, isOpenValue: true), new ContractMoney(12000m, 0), "دهان", "بويه");

        Assert.Equal(12000m, view.Paid);
        Assert.Equal(0m, view.Remaining);
        Assert.False(view.IsOverrun);
    }

    [Fact]
    public async Task AnOpenValueContractKeepsNoValue()
    {
        var (client, project) = await FinanceAsync();

        (await PostAsync(client, project.Id, await RequestFor(client, project, 5000m, isOpenValue: true))).EnsureSuccessStatusCode();

        var contract = Assert.Single(await ContractsOf(client, project.Id));
        Assert.True(contract.IsOpenValue);
        Assert.Equal(0m, contract.Value);
        Assert.Equal(0m, contract.Remaining);
    }

    [Theory]
    [MemberData(nameof(UnacceptableValues))]
    public async Task AValueIsAboveZeroWithAtMostTwoDecimals(decimal value)
    {
        var (client, project) = await FinanceAsync();

        var refused = await PostAsync(client, project.Id, await RequestFor(client, project, value));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidValue", await CodeOf(refused));
    }

    [Theory]
    [MemberData(nameof(UnacceptableParts))]
    public async Task AUnitPriceAndAQuantityAreNotBelowZeroWithAtMostTwoDecimals(decimal amount)
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project);

        Assert.Equal("invalidUnitPrice", await CodeOf(await PostAsync(client, project.Id, request with { UnitPrice = amount })));
        Assert.Equal("invalidQuantity", await CodeOf(await PostAsync(client, project.Id, request with { Quantity = amount })));
    }

    [Fact]
    public async Task NotesHaveAtMost1000Characters()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project) with { Notes = new string('x', Contract.NotesMaxLength + 1) };

        Assert.Equal("invalidNotes", await CodeOf(await PostAsync(client, project.Id, request)));
    }

    [Fact]
    public async Task TheContractorIsAnActiveSupplierOfTheProject()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var request = await RequestFor(client, project);
        var disabled = await AnotherContractorAsync(client, project.Id);
        (await client.PostAsync($"/projects/{project.Id}/suppliers/{disabled.Id}/disable", null)).EnsureSuccessStatusCode();

        foreach (var supplierId in new[] { disabled.Id, Guid.Empty, (await ContractorOf(otherClient, other.Id)).Id })
        {
            Assert.Equal("unknownSupplier", await CodeOf(await PostAsync(client, project.Id, request with { SupplierId = supplierId })));
        }
    }

    [Fact]
    public async Task TheItemTakesContractorPayments()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var request = await RequestFor(client, project);
        var purchasesOnly = Guid.Empty;
        await factory.WithDatabaseAsync(async db => purchasesOnly = await db.Items.Where(item => item.Type == ItemType.Purchases && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == project.Id)).Select(item => item.Id).FirstAsync());

        foreach (var itemId in new[] { purchasesOnly, Guid.Empty, (await ItemOf(otherClient, other.Id)).Id })
        {
            Assert.Equal("itemNotOffered", await CodeOf(await PostAsync(client, project.Id, request with { ItemId = itemId })));
        }
    }

    [Fact]
    public async Task ADisabledItemTakesNoNewContract()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project);
        (await client.PostAsync($"/projects/{project.Id}/items/{request.ItemId}/disable", null)).EnsureSuccessStatusCode();

        Assert.Equal("itemNotOffered", await CodeOf(await PostAsync(client, project.Id, request)));
    }

    [Fact]
    public async Task AnApprovedProjectHasOnlyItsOwnUnits()
    {
        var (_, project) = await FinanceAsync();

        Assert.Equal(["الفلة الغربية"], project.Units.Select(unit => unit.Name));
    }

    [Fact]
    public async Task AnAmendmentChangesTheTermsAndKeepsTheContractor()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(36000m, false) { UnitPrice = 400m, Quantity = 90m, Notes = "زيادة بملحق" }, Json)).EnsureSuccessStatusCode();

        var amended = Assert.Single(await ContractsOf(client, project.Id));
        Assert.Equal(36000m, amended.Value);
        Assert.Equal(400m, amended.UnitPrice);
        Assert.Equal(90m, amended.Quantity);
        Assert.Equal("زيادة بملحق", amended.Notes);
        Assert.Equal(contract.SupplierName, amended.SupplierName);
    }

    [Fact]
    public async Task AnAmendmentCanMakeAContractOpenValue()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(0m, true), Json)).EnsureSuccessStatusCode();

        var amended = Assert.Single(await ContractsOf(client, project.Id));
        Assert.True(amended.IsOpenValue);
        Assert.Equal(0m, amended.Value);
    }

    [Fact]
    public async Task AnAmendmentAnswersAsAddingDoes()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);

        Assert.Equal("invalidValue", await CodeOf(await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(0m, false), Json)));
    }

    [Fact]
    public async Task EveryChangeOfAContractIsInTheHistory()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(36000m, false), Json)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/disable", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/enable", null)).EnsureSuccessStatusCode();

        var trail = await factory.HistoryOfAsync(contract.Id);
        Assert.Equal([HistoryAction.Create, HistoryAction.Update, HistoryAction.Disable, HistoryAction.Enable], trail.Select(record => record.Action));
        Assert.Contains("34000", trail[1].Before);
        Assert.Contains("36000", trail[1].After);
    }

    [Fact]
    public async Task ADisabledContractStaysOnTheListAndCanBeEnabled()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);

        (await client.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/disable", null)).EnsureSuccessStatusCode();
        Assert.False(Assert.Single(await ContractsOf(client, project.Id)).IsActive);

        (await client.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/enable", null)).EnsureSuccessStatusCode();
        Assert.True(Assert.Single(await ContractsOf(client, project.Id)).IsActive);
    }

    [Fact]
    public async Task ContractsShowInTheOrderTheyWereAdded()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project, 1000m);
        await AddAsync(client, project, 2000m);
        await AddAsync(client, project, 3000m);

        Assert.Equal([1000m, 2000m, 3000m], (await ContractsOf(client, project.Id)).Select(contract => contract.Value));
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task EveryoneWithAccessReadsContractsAndOnlyFinanceChangesThem(string role)
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);
        var memberClient = await MemberAsync(project, role);

        Assert.Single(await ContractsOf(memberClient, project.Id));
        foreach (var refused in new[]
        {
            await PostAsync(memberClient, project.Id, await RequestFor(client, project)),
            await memberClient.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(1m, false), Json),
            await memberClient.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/disable", null),
            await memberClient.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/enable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        }
    }

    [Fact]
    public async Task OneProjectsContractsAreNeverAnothers()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var theirs = await AddAsync(otherClient, other);

        Assert.Empty(await ContractsOf(client, project.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{theirs.Id}", new ContractTerms(1m, false), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{project.Id}/contracts/{theirs.Id}/disable", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/contracts")).StatusCode);
    }

    [Fact]
    public async Task ACopiedProjectStartsWithNoContracts()
    {
        var (client, earlier) = await FinanceAsync();
        await AddAsync(client, earlier);
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });
        var later = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;

        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{later.Id}/approve", null)).EnsureSuccessStatusCode();

        Assert.Empty(await ContractsOf(client, later.Id));
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (client, project) = await FinanceAsync();
        var contract = await AddAsync(client, project);
        var request = await RequestFor(client, project);
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await PostAsync(client, project.Id, request),
            await client.PostAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(1m, false), Json),
            await client.PostAsync($"/projects/{project.Id}/contracts/{contract.Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Single(await ContractsOf(client, project.Id));
    }

    private static Task<HttpResponseMessage> AmendAsync(HttpClient client, Guid projectId, Guid contractId, object body) =>
        client.PostAsJsonAsync($"/projects/{projectId}/contracts/{contractId}", body, Json);

    private static async Task<SupplierView> AnotherContractorAsync(HttpClient client, Guid projectId) =>
        (await (await client.PostAsJsonAsync($"/projects/{projectId}/suppliers", new SupplierRequest("مؤسسة الواحة"), Json)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!;

    private static async Task<ItemView> AnotherItemAsync(HttpClient client, Guid projectId, Guid notThis) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories/offered?itemType={ItemType.ContractorPayments}", Json))!.SelectMany(category => category.Items).First(item => item.Id != notThis);

    [Fact]
    public async Task AContractAnswersItsPartiesAndThatItHasNoPayments()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project);
        (await PostAsync(client, project.Id, request)).EnsureSuccessStatusCode();

        var contract = Assert.Single(await ContractsOf(client, project.Id));

        Assert.Equal((request.SupplierId, request.ItemId, false), (contract.SupplierId, contract.ItemId, contract.HasPayments));
    }

    [Fact]
    public async Task AContractsPartiesChangeWhileItHasNoPaymentsAndTheHistoryShowsThemOldAndNew()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project);
        (await PostAsync(client, project.Id, request)).EnsureSuccessStatusCode();
        var contract = Assert.Single(await ContractsOf(client, project.Id));
        var contractor = await AnotherContractorAsync(client, project.Id);
        var item = await AnotherItemAsync(client, project.Id, request.ItemId);

        (await AmendAsync(client, project.Id, contract.Id, new { value = 34000m, isOpenValue = false, supplierId = contractor.Id, itemId = item.Id })).EnsureSuccessStatusCode();

        var amended = Assert.Single(await ContractsOf(client, project.Id));
        Assert.Equal((contractor.Name, item.Name), (amended.SupplierName, amended.ItemName));
        var record = (await factory.HistoryOfAsync(contract.Id)).Last();
        Assert.Equal(HistoryAction.Update, record.Action);
        Assert.Equal(request.SupplierId, JsonDocument.Parse(record.Before).RootElement.GetProperty("SupplierId").GetGuid());
        Assert.Equal(contractor.Id, JsonDocument.Parse(record.After).RootElement.GetProperty("SupplierId").GetGuid());
        Assert.Equal(request.ItemId, JsonDocument.Parse(record.Before).RootElement.GetProperty("ItemId").GetGuid());
        Assert.Equal(item.Id, JsonDocument.Parse(record.After).RootElement.GetProperty("ItemId").GetGuid());
    }

    [Theory]
    [InlineData(EntryStatus.Draft)]
    [InlineData(EntryStatus.PendingApproval)]
    [InlineData(EntryStatus.Approved)]
    [InlineData(EntryStatus.Rejected)]
    public async Task AContractWithAnyPaymentKeepsItsPartiesAndChangesNothing(EntryStatus status)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var item = await ItemOf(ledger.Client, ledger.ProjectId);
        (await PostAsync(ledger.Client, ledger.ProjectId, new AddContractRequest(ledger.SupplierId, item.Id, 34000m, false))).EnsureSuccessStatusCode();
        var contract = Assert.Single(await ContractsOf(ledger.Client, ledger.ProjectId));
        var payment = new EntryRequest(Guid.CreateVersion7()) { ItemId = item.Id, TableId = contract.Id, Date = ledger.Today, Amount = 3000m, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId };
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", payment, Json)).EnsureSuccessStatusCode();
        await ledger.SetStatusAsync(payment.Id, status);
        Assert.True(Assert.Single(await ContractsOf(ledger.Client, ledger.ProjectId)).HasPayments);
        var contractor = await AnotherContractorAsync(ledger.Client, ledger.ProjectId);

        var refused = await AmendAsync(ledger.Client, ledger.ProjectId, contract.Id, new { value = 99000m, isOpenValue = false, supplierId = contractor.Id, itemId = item.Id });

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("contractHasPayments", await CodeOf(refused));
        var kept = Assert.Single(await ContractsOf(ledger.Client, ledger.ProjectId));
        Assert.Equal((contract.SupplierName, 34000m), (kept.SupplierName, kept.Value));
    }

    [Fact]
    public async Task AContractWithAPaymentStillChangesItsTerms()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var item = await ItemOf(ledger.Client, ledger.ProjectId);
        (await PostAsync(ledger.Client, ledger.ProjectId, new AddContractRequest(ledger.SupplierId, item.Id, 34000m, false))).EnsureSuccessStatusCode();
        var contract = Assert.Single(await ContractsOf(ledger.Client, ledger.ProjectId));
        var payment = new EntryRequest(Guid.CreateVersion7()) { ItemId = item.Id, TableId = contract.Id, Date = ledger.Today, Amount = 3000m, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId };
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", payment, Json)).EnsureSuccessStatusCode();

        (await AmendAsync(ledger.Client, ledger.ProjectId, contract.Id, new { value = 40000m, isOpenValue = false })).EnsureSuccessStatusCode();
        (await AmendAsync(ledger.Client, ledger.ProjectId, contract.Id, new { value = 41000m, isOpenValue = false, supplierId = contract.SupplierId, itemId = contract.ItemId })).EnsureSuccessStatusCode();

        Assert.Equal(41000m, Assert.Single(await ContractsOf(ledger.Client, ledger.ProjectId)).Value);
    }

    [Fact]
    public async Task NewPartiesAnswerAsAddingDoes()
    {
        var (client, project) = await FinanceAsync();
        var request = await RequestFor(client, project);
        (await PostAsync(client, project.Id, request)).EnsureSuccessStatusCode();
        var contract = Assert.Single(await ContractsOf(client, project.Id));
        var purchasesOnly = (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories", Json))!.SelectMany(category => category.Items).First(item => item.Type == ItemType.Purchases);

        Assert.Equal("unknownSupplier", await CodeOf(await AmendAsync(client, project.Id, contract.Id, new { value = 1m, isOpenValue = false, supplierId = Guid.NewGuid() })));
        Assert.Equal("itemNotOffered", await CodeOf(await AmendAsync(client, project.Id, contract.Id, new { value = 1m, isOpenValue = false, itemId = purchasesOnly.Id })));
        Assert.Equal(34000m, Assert.Single(await ContractsOf(client, project.Id)).Value);
    }
}
