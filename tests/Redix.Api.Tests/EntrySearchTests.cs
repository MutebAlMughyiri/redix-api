using System.Globalization;
using System.Net.Http.Json;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>redix-app #104: the entries list searched and filtered on the API, a page at a time.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class EntrySearchTests(MigratedApiFactory factory)
{
    private static readonly DateOnly Day = new(2026, 11, 10);

    private static EntryRowView Row(string supplierName = "مؤسسة الأفق", EntryKind kind = EntryKind.Purchase, EntryStatus status = EntryStatus.Approved, bool isMine = true, decimal amount = 6000m, int number = 312) =>
        new(Guid.CreateVersion7(), number, kind, status, "", Day, amount, "البلاط", supplierName, "", "", "نواف", isMine, "", "", "البنك", "", 0m);

    private static EntrySearch Search(string text = "", bool mine = false, DateOnly from = default, DateOnly to = default, int number = 0) => new(text, mine, [], [], from, to, number);

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
    public void App_104_EachFilterGivenMustHold()
    {
        Assert.True((Search() with { Kinds = ["purchase"] }).Keeps(Row()));
        Assert.False((Search() with { Kinds = ["contractorPayment", "revenue"] }).Keeps(Row()));
        Assert.True((Search() with { Statuses = ["approved"] }).Keeps(Row()));
        Assert.False((Search() with { Statuses = ["rejected"] }).Keeps(Row()));
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

    [Fact]
    public async Task App_104_WhereEntriesStandWhoseTheyAreAndTheirPeriodAreReadFromTheQuery()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.ApproveAsync(await ledger.PostAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, 100m)));
        await ledger.RecordAsync(ledger.Purchase(300m) with { Submit = true });
        var (reader, _) = await ledger.MemberAsync("Reader");
        var path = $"/projects/{ledger.ProjectId}/entries";
        var today = ledger.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var tomorrow = ledger.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        async Task<List<EntryRowView>> ListAsync(HttpClient client, string query) => (await client.GetFromJsonAsync<List<EntryRowView>>($"{path}?{query}", Ledger.Json))!;
        var every = await ListAsync(ledger.Client, "");

        var waiting = await ListAsync(ledger.Client, "statuses=pendingApproval");
        var thisDay = await ListAsync(ledger.Client, $"from={today}&to={today}");
        var fromTomorrow = await ListAsync(ledger.Client, $"from={tomorrow}");
        var readersOwn = await ListAsync(reader, "mine=true");

        Assert.Equal([300m], waiting.Select(row => row.Amount));
        Assert.Equal(every.Select(row => row.Id), thisDay.Select(row => row.Id));
        Assert.Empty(fromTomorrow);
        Assert.NotEmpty(await ListAsync(reader, ""));
        Assert.Empty(readersOwn);
    }

    [Fact]
    public void App_104_AnAmountOf0IsNoAmountSoTyping0FindsNothingByIt()
    {
        Assert.False(ArabicSpelling.Holds("0", 0m, "حساب المؤسسة"));
        Assert.True(ArabicSpelling.Holds("المؤسسه", 0m, "حساب المؤسسة"));
        Assert.True(ArabicSpelling.Holds("1,000", 1000m));
    }
}
