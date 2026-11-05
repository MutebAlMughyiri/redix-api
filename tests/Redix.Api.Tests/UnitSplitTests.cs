using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Reports;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-060: a shared expense split across units. No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class UnitSplitTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<(Ledger Ledger, Guid First, Guid Second, Guid Shared)> TwoUnitsAsync(MigratedApiFactory factory)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/units", new { name = "الفلة الجنوبية" }, Json)).EnsureSuccessStatusCode();
        var units = await ledger.ReadAsync(db => db.Set<Unit>().Where(unit => unit.ProjectId == ledger.ProjectId).ToListAsync());
        var own = units.Where(unit => !unit.IsShared).OrderBy(unit => unit.SortOrder).ToList();
        return (ledger, own[0].Id, own[1].Id, units.Single(unit => unit.IsShared).Id);
    }

    private static Task<List<UnitTotalView>> TotalsAsync(Ledger ledger) => ledger.Client.GetFromJsonAsync<List<UnitTotalView>>($"/projects/{ledger.ProjectId}/reports/units", Json)!;

    [Fact]
    public async Task ASharedExpenseSplitByPercentageReachesEachUnitsTotal()
    {
        var (ledger, first, second, shared) = await TwoUnitsAsync(factory);
        var split = ledger.WithoutInvoice(1000) with { UnitId = shared, Submit = true, UnitShares = [new(first, 70), new(second, 30)] };
        await ledger.RecordAsync(split);
        var direct = ledger.WithoutInvoice(200) with { UnitId = first, Submit = true };
        await ledger.RecordAsync(direct);
        var unsplit = ledger.WithoutInvoice(50) with { UnitId = shared, Submit = true };
        await ledger.RecordAsync(unsplit);
        foreach (var id in new[] { split.Id, direct.Id, unsplit.Id })
        {
            await ledger.SetStatusAsync(id, EntryStatus.Approved);
        }

        var totals = await TotalsAsync(ledger);

        Assert.Equal((200m, 700m, 900m), (totals.Single(row => row.UnitId == first).Direct, totals.Single(row => row.UnitId == first).FromShared, totals.Single(row => row.UnitId == first).Total));
        Assert.Equal(300m, totals.Single(row => row.UnitId == second).Total);
        Assert.Equal(50m, totals.Single(row => row.UnitId == shared).Total);
        Assert.True(totals.Last().IsShared);
    }

    [Fact]
    public async Task OnlyAnExpenseOnTheSharedUnitIsSplitAndThePartsMakeAHundred()
    {
        var (ledger, first, second, shared) = await TwoUnitsAsync(factory);

        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { UnitId = first, UnitShares = [new(first, 50), new(second, 50)] }, "splitNeedsSharedUnit");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { UnitId = shared, UnitShares = [new(first, 50), new(second, 40)] }, "invalidSplit");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { UnitId = shared, UnitShares = [new(first, 50), new(first, 50)] }, "invalidSplit");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { UnitId = shared, UnitShares = [new(first, 100), new(shared, 0)] }, "invalidSplit");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { UnitId = shared, UnitShares = [new(first, 33.333m), new(second, 66.667m)] }, "invalidSplit");
    }

    [Fact]
    public async Task AChangedSplitReplacesTheOldOneAndTheEntryShowsIt()
    {
        var (ledger, first, second, shared) = await TwoUnitsAsync(factory);
        var draft = ledger.WithoutInvoice(1000) with { UnitId = shared, UnitShares = [new(first, 50), new(second, 50)] };
        await ledger.RecordAsync(draft);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/expenses/{draft.Id}", draft with { UnitShares = [new(first, 25), new(second, 75)] }, Json)).EnsureSuccessStatusCode();

        var view = await ledger.Client.GetFromJsonAsync<EntryView>($"/projects/{ledger.ProjectId}/entries/{draft.Id}", Json);
        Assert.Equal([25m, 75m], view!.UnitShares.OrderBy(share => share.Percent).Select(share => share.Percent));
    }

    [Fact]
    public async Task ReversingASplitExpenseTakesEachUnitsPartBack()
    {
        var (ledger, first, second, shared) = await TwoUnitsAsync(factory);
        var split = ledger.WithoutInvoice(1000) with { UnitId = shared, Submit = true, UnitShares = [new(first, 60), new(second, 40)] };
        await ledger.RecordAsync(split);
        await ledger.SetStatusAsync(split.Id, EntryStatus.Approved);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{split.Id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.All(await TotalsAsync(ledger), row => Assert.Equal(0m, row.Total));
    }
}
