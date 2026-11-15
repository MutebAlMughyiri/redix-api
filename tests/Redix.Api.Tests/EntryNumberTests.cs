using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>
/// redix-api #128, FR-54: every entry has a number in its project, «رقم القيد», given by the server when it first keeps the entry, one more than the last given in that project; it
/// never changes, is never given again, and a filter reaches the entry by it.
/// </summary>
[Collection(SecondDatabaseCollection.Name)]
public class EntryNumberTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<List<EntryRowView>> ListAsync(Ledger ledger, string query = "", HttpClient? client = null) =>
        (await (client ?? ledger.Client).GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries{query}", Json))!;

    private static async Task<List<PendingEntryView>> InboxAsync(Ledger ledger) =>
        (await ledger.Client.GetFromJsonAsync<List<PendingEntryView>>($"/projects/{ledger.ProjectId}/entries/pending", Json))!;

    private static Task<int> NumberOfAsync(Ledger ledger, Guid entryId) => ledger.ReadAsync(db => db.Entries.Where(entry => entry.Id == entryId).Select(entry => entry.Number).SingleAsync());

    [Fact]
    public async Task AC_54_1_AKeptEntryHasANumberAndTheNextOneKeptInTheProjectHasTheNext()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (first, second) = (ledger.Purchase(100), ledger.Purchase(200));

        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);

        Assert.Equal((1, 2), (await NumberOfAsync(ledger, first.Id), await NumberOfAsync(ledger, second.Id)));
        Assert.Equal([1, 2], (await ListAsync(ledger)).Select(row => row.Number).Order());
    }

    [Fact]
    public async Task AC_54_4_FinancesInboxLeadsEachWaitingEntryWithItsNumber()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (first, second) = (ledger.Purchase(100) with { Submit = true }, ledger.Purchase(200) with { Submit = true });
        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);

        var inbox = await InboxAsync(ledger);

        Assert.Equal([(first.Id, 1), (second.Id, 2)], inbox.Select(row => (row.Id, row.Number)));
    }

    [Fact]
    public async Task AC_54_4_TheInboxShowsTheEntrysOwnNumberNotItsPlaceInTheInbox()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (draft, waiting) = (ledger.Purchase(100), ledger.Purchase(200) with { Submit = true });
        await ledger.RecordAsync(draft);
        await ledger.RecordAsync(waiting);

        var inbox = await InboxAsync(ledger);

        Assert.Equal(2, Assert.Single(inbox).Number);
    }

    [Fact]
    public async Task AC_54_1_R5_EachProjectCountsItsOwnAndTakesNoneOfAnotherProjectsNumbers()
    {
        var (ledger, other) = (await Ledger.WithFinanceAsync(factory), await Ledger.WithFinanceAsync(factory));
        await other.RecordAsync(other.Purchase(100));
        await other.RecordAsync(other.Purchase(200));
        var request = ledger.Purchase(300);

        await ledger.RecordAsync(request);

        Assert.Equal(1, await NumberOfAsync(ledger, request.Id));
    }

    [Fact]
    public async Task AC_54_2_ADraftAndAPendingEntryHaveTheirNumberAndADeletedDraftLeavesItsNumberUnused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (draft, pending, next) = (ledger.Purchase(100), ledger.Purchase(200) with { Submit = true }, ledger.Purchase(300));
        await ledger.RecordAsync(draft);
        await ledger.RecordAsync(pending);
        var before = (await ListAsync(ledger)).Select(row => (row.Status, row.Number)).ToList();

        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{draft.Id}")).EnsureSuccessStatusCode();
        await ledger.RecordAsync(next);

        Assert.Equal([(EntryStatus.Draft, 1), (EntryStatus.PendingApproval, 2)], before.OrderBy(row => row.Number));
        Assert.Equal([2, 3], (await ListAsync(ledger)).Select(row => row.Number).Order());
    }

    [Fact]
    public async Task AC_54_3_ChangingMovingApprovingRemovingAndRestoringAnEntryKeepsItsNumber()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (remover, removerId) = await ledger.MemberAsync("Reader");
        await factory.GiveAsync(removerId, ledger.ProjectId, [Permission.RemoveApprovedEntries]);
        await ledger.RecordAsync(ledger.Purchase(50));
        var request = ledger.Purchase(100);
        await ledger.RecordAsync(request);
        var numbers = new List<int>();

        (await ledger.ChangeAsync(request with { Amount = 150 })).EnsureSuccessStatusCode();
        numbers.Add(await NumberOfAsync(ledger, request.Id));
        (await ledger.ChangeAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, 150) with { Id = request.Id })).EnsureSuccessStatusCode();
        numbers.Add(await NumberOfAsync(ledger, request.Id));
        await ledger.ApproveAsync(request.Id);
        numbers.Add(await NumberOfAsync(ledger, request.Id));
        (await remover.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}")).EnsureSuccessStatusCode();
        numbers.Add(await NumberOfAsync(ledger, request.Id));
        (await remover.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/restore", null)).EnsureSuccessStatusCode();
        numbers.Add(await NumberOfAsync(ledger, request.Id));

        Assert.Equal([2, 2, 2, 2, 2], numbers);
        Assert.Equal(EntryKind.Revenue, await ledger.ReadAsync(db => db.Entries.Where(entry => entry.Id == request.Id).Select(entry => entry.Kind).SingleAsync()));
    }

    [Fact]
    public async Task FR_54_AnUploadSentAgainAnswersItsNumberAndTakesNoOther()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (request, next) = (ledger.Purchase(100), ledger.Purchase(200));
        await ledger.RecordAsync(request);

        var again = await ledger.PostAsync(request);
        await ledger.RecordAsync(next);

        Assert.Equal(1, (await again.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Number);
        Assert.Equal(2, await NumberOfAsync(ledger, next.Id));
    }

    [Fact]
    public async Task G1_EntriesArrivingAtTheSameMomentEachTakeANumberOfTheirOwn()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var requests = Enumerable.Range(1, 8).Select(amount => ledger.Purchase(amount)).ToList();

        var answers = await Task.WhenAll(requests.Select(request => ledger.PostAsync(request)));

        Assert.All(answers, answer => answer.EnsureSuccessStatusCode());
        Assert.Equal(Enumerable.Range(1, 8), (await ListAsync(ledger)).Select(row => row.Number).Order());
    }

    [Fact]
    public async Task D3_TheDatabaseRefusesASecondEntryOfTheProjectWithTheSameNumber()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(100);
        await ledger.RecordAsync(request);

        await factory.WithDatabaseAsync(async db =>
        {
            var kept = await db.Entries.SingleAsync(entry => entry.Id == request.Id);
            db.Entries.Add(new Entry { Id = Guid.CreateVersion7(), ProjectId = kept.ProjectId, Number = kept.Number, Kind = kept.Kind, EntryDate = kept.EntryDate, Amount = 1, AccountId = kept.AccountId, CreatedByUserId = kept.CreatedByUserId, CreatedAt = kept.CreatedAt });

            var refused = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

            Assert.Contains("IX_Entries_ProjectId_Number", refused.InnerException!.Message);
        });
    }

    [Fact]
    public async Task AC_54_5_TheNumberFilterListsThatEntryAloneAndANumberNoEntryHasListsNothing()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var requests = new[] { ledger.Purchase(100), ledger.Purchase(200), ledger.Purchase(300) };
        foreach (var request in requests)
        {
            await ledger.RecordAsync(request);
        }

        Assert.Equal([requests[1].Id], (await ListAsync(ledger, "?number=2")).Select(row => row.Id));
        Assert.Equal([requests[1].Id], (await ListAsync(ledger, "?number=2&text=200")).Select(row => row.Id));
        Assert.Empty(await ListAsync(ledger, "?number=2&text=300"));
        Assert.Empty(await ListAsync(ledger, "?number=99"));
        Assert.Equal(3, (await ListAsync(ledger, "?number=0")).Count);
    }

    [Fact]
    public async Task AC_54_5_R9_ANumberOfAnEntryThePersonMayNotSeeListsNothingAsANumberNoEntryHas()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");
        await ledger.RecordAsync(ledger.Purchase(100));

        Assert.Empty(await ListAsync(ledger, "?number=1", field));
        Assert.Single(await ListAsync(ledger, "?number=1"));
    }
}
