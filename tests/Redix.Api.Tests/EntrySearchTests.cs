using System.Net.Http.Json;
using Redix.Api.Entries;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>redix-app #104: the entries list searched and filtered on the API, a page at a time.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class EntrySearchTests(MigratedApiFactory factory)
{
    private static readonly DateOnly Day = new(2026, 11, 10);

    private static EntryRowView Row(string supplierName = "مؤسسة الأفق", EntryKind kind = EntryKind.Purchase, EntryStatus status = EntryStatus.Approved, bool isReversed = false, bool isMine = true, decimal amount = 6000m) =>
        new(Guid.CreateVersion7(), kind, status, "", Day, amount, "البلاط", supplierName, "", "نواف", isMine, isReversed, "", "", "البنك", "", 0m);

    private static EntrySearch Search(string text = "", bool mine = false, DateOnly from = default, DateOnly to = default) => new(text, mine, [], [], from, to);

    [Fact]
    public void App_104_TextIsMatchedWhicheverWayAnArabicWordIsSpelledAndByTheAmountEitherWayItIsWritten()
    {
        Assert.True(Search("الافق").Keeps(Row()));
        Assert.True(Search("6000").Keeps(Row()));
        Assert.True(Search("6,000.00").Keeps(Row()));
        Assert.True(Search("نواف").Keeps(Row()));
        Assert.False(Search("النخيل").Keeps(Row()));
        Assert.False(Search().Narrows);
    }

    [Fact]
    public void App_104_EachFilterGivenMustHoldAndAReversedEntryStandsReversed()
    {
        Assert.True((Search() with { Kinds = ["purchase"] }).Keeps(Row()));
        Assert.False((Search() with { Kinds = ["contractorPayment", "revenue"] }).Keeps(Row()));
        Assert.True((Search() with { Statuses = ["reversed"] }).Keeps(Row(isReversed: true)));
        Assert.False((Search() with { Statuses = ["approved"] }).Keeps(Row(isReversed: true)));
        Assert.True((Search() with { Statuses = ["draft"] }).Keeps(Row(status: EntryStatus.Draft)));
        Assert.False(Search(mine: true).Keeps(Row(isMine: false)));
        Assert.True(Search(from: Day, to: Day).Keeps(Row()));
        Assert.False(Search(from: Day.AddDays(1)).Keeps(Row()));
        Assert.False(Search(to: Day.AddDays(-1)).Keeps(Row()));
    }

    [Fact]
    public async Task App_104_TheListGivesOnlyWhatTheSearchKeepsTheNewestFirstAPageAtATime()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        foreach (var amount in new[] { 100m, 200m })
        {
            await ledger.RecordAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, amount));
        }
        foreach (var amount in new[] { 300m, 400m, 500m })
        {
            await ledger.RecordAsync(ledger.Purchase(amount) with { Submit = true });
        }
        var path = $"/projects/{ledger.ProjectId}/entries";
        var every = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>(path, Ledger.Json))!;

        var purchases = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"{path}?kinds=purchase&kinds=contractorPayment&take=2", Ledger.Json))!;
        var byAmount = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"{path}?text=200", Ledger.Json))!;

        Assert.Equal(every.Where(row => row.Kind == EntryKind.Purchase).Take(2).Select(row => row.Id), purchases.Select(row => row.Id));
        Assert.Equal([200m], byAmount.Select(row => row.Amount));
    }
}
