using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Redix.Api.Entries;
using Redix.Api.Structure;
using Redix.Api.Sync;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-030: what a phone keeps to work without a network.</summary>
[Collection(DatabaseCollection.Name)]
public class SyncTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<SyncView> SyncAsync(HttpClient client, Guid projectId, string since = "") =>
        (await client.GetFromJsonAsync<SyncView>($"/projects/{projectId}/sync?since={since}", Json))!;

    [Fact]
    public async Task AFirstSyncAnswersEveryListAtTheAddressTheAppReadsItAt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var sync = await SyncAsync(ledger.Client, ledger.ProjectId);

        var project = $"/projects/{ledger.ProjectId}";
        foreach (var address in new[] { "categories/offered?itemType=purchases", "categories/offered?itemType=contractorPayments", "suppliers", "suppliers/search?text=", "contracts", "accounts", "accounts/offered", "payment-methods", "payment-methods/offered", "entries/mine", $"items/{ledger.ItemId}/columns" })
        {
            Assert.Contains($"{project}/{address}", sync.Lists.Keys);
        }
    }

    [Fact]
    public async Task EachListHoldsWhatItsOwnReadAnswers()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var address = $"/projects/{ledger.ProjectId}/suppliers";

        var sync = await SyncAsync(ledger.Client, ledger.ProjectId);

        var read = await ledger.Client.GetStringAsync(address);
        Assert.Equal(read, ((JsonElement)sync.Lists[address]).GetRawText());
    }

    [Fact]
    public async Task NothingChangedAnswersNoLists()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await SyncAsync(ledger.Client, ledger.ProjectId);

        var second = await SyncAsync(ledger.Client, ledger.ProjectId, first.Stamp);

        Assert.Equal(first.Stamp, second.Stamp);
        Assert.Empty(second.Lists);
    }

    [Fact]
    public async Task AC_05_1_AnItemFinanceAddedReachesTheFieldAtTheNextSync()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("users.roleField");
        var before = await SyncAsync(field, ledger.ProjectId);
        var category = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories", Json))![0];

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/categories/{category.Id}/items", new AddItemRequest("بند الدرج", ItemType.Purchases), Json)).EnsureSuccessStatusCode();

        var after = await SyncAsync(field, ledger.ProjectId, before.Stamp);
        Assert.NotEqual(before.Stamp, after.Stamp);
        var offered = ((JsonElement)after.Lists[$"/projects/{ledger.ProjectId}/categories/offered?itemType=purchases"]).Deserialize<List<CategoryView>>(Json)!;
        Assert.Contains(offered.SelectMany(row => row.Items), item => item.Name == "بند الدرج");
    }

    [Fact]
    public async Task ARowThatLeftAListChangesTheStampToo()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var before = await SyncAsync(ledger.Client, ledger.ProjectId);
        var method = (await ledger.Client.GetFromJsonAsync<List<JsonElement>>($"/projects/{ledger.ProjectId}/payment-methods", Json))![0].GetProperty("id").GetGuid();

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/payment-methods/{method}/disable", null)).EnsureSuccessStatusCode();

        Assert.NotEqual(before.Stamp, (await SyncAsync(ledger.Client, ledger.ProjectId, before.Stamp)).Stamp);
    }

    [Fact]
    public async Task TheFieldKeepsOnlyTheAccountsTheyMayCharge()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("users.roleField");

        var sync = await SyncAsync(field, ledger.ProjectId);

        Assert.Equal(0, ((JsonElement)sync.Lists[$"/projects/{ledger.ProjectId}/accounts/offered"]).GetArrayLength());
    }

    [Fact]
    public async Task EachPersonKeepsOnlyTheStatusOfTheirOwnEntries()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var recorded = await ledger.RecordAsync(ledger.Purchase());
        var (other, _) = await ledger.MemberAsync("users.roleFinance");

        var mine = await ledger.Client.GetFromJsonAsync<List<EntryStatusView>>($"/projects/{ledger.ProjectId}/entries/mine", Json);
        var theirs = await other.GetFromJsonAsync<List<EntryStatusView>>($"/projects/{ledger.ProjectId}/entries/mine", Json);

        Assert.Equal(recorded.Id, Assert.Single(mine!).Id);
        Assert.Empty(theirs!);
    }

    [Fact]
    public async Task SomeoneOutsideTheProjectGetsNothing()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var stranger = await factory.AddUserAsync("Sync-pass-1");
        var client = await factory.ClientSignedInAsAsync(stranger, "Sync-pass-1");

        var refused = await client.GetAsync($"/projects/{ledger.ProjectId}/sync");

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
    }
}
