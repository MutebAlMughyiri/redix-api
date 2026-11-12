using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Tests;

/// <summary>Muteb, 12 Nov 2026: the history of what was approved and rejected, beside the accounts' and the partners' (redix-app #101).</summary>
[Collection(SecondDatabaseCollection.Name)]
public class ApprovalHistoryTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<List<ApprovalHistoryLine>> HistoryAsync(Ledger ledger, string query = "") =>
        (await ledger.Client.GetFromJsonAsync<List<ApprovalHistoryLine>>($"/projects/{ledger.ProjectId}/approvals/history{query}", Json))!;

    /// <summary>A purchase of 1150 approved, then one of 300 rejected because «فاتورة مكررة», both by Finance, here named «متعب».</summary>
    private async Task<Ledger> DecidedAsync()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.ApproveAsync(await ledger.PostAsync(ledger.Purchase(1150m) with { Submit = true }));
        var rejected = (await (await ledger.PostAsync(ledger.Purchase(300m) with { Submit = true })).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id;
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{rejected}/reject", new RejectEntryRequest("فاتورة مكررة"), Json)).EnsureSuccessStatusCode();
        await factory.WithDatabaseAsync(db => db.Users.Where(user => db.History.Any(row => row.ProjectId == ledger.ProjectId && row.TableName == "Entries" && row.UserId == user.Id))
            .ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "متعب")));
        return ledger;
    }

    [Fact]
    public async Task Muteb12Nov_EachApprovalAndRejectionIsALineNewestFirst_ARejectionWithItsReason()
    {
        var ledger = await DecidedAsync();

        var lines = await HistoryAsync(ledger);

        Assert.Equal([ApprovalHistoryKind.Rejected, ApprovalHistoryKind.Approved], lines.Select(line => line.Kind));
        Assert.Equal((300m, "فاتورة مكررة"), (lines[0].Amount, lines[0].Reason));
        Assert.All(lines, line => Assert.Equal("متعب", line.By));
        Assert.Equal((1150m, ""), (lines[1].Amount, lines[1].Reason));
        Assert.All(lines, line => Assert.NotEmpty(line.Item));
    }

    [Fact]
    public async Task Muteb12Nov_AKindKeepsTheDecisionsOnEntriesOfThatKind_AndAnythingElseKeepsEvery()
    {
        var ledger = await DecidedAsync();

        Assert.Equal(2, (await HistoryAsync(ledger, "?kind=purchase")).Count);
        Assert.Empty(await HistoryAsync(ledger, "?kind=contractorPayment"));
        Assert.Equal(2, (await HistoryAsync(ledger, "?kind=")).Count);
    }

    [Fact]
    public async Task R104_WhatWasTypedKeepsTheLinesWhoseReasonOrAmountHoldsIt()
    {
        var ledger = await DecidedAsync();

        Assert.Equal([ApprovalHistoryKind.Rejected], (await HistoryAsync(ledger, "?text=مكرره")).Select(line => line.Kind));
        Assert.Equal([ApprovalHistoryKind.Approved], (await HistoryAsync(ledger, "?text=1150")).Select(line => line.Kind));
    }

    [Fact]
    public async Task Muteb12Nov_AnotherProjectsDecisionsAreNotInIt()
    {
        await DecidedAsync();
        var other = await Ledger.WithFinanceAsync(factory);

        Assert.Empty(await HistoryAsync(other));
    }

    [Fact]
    public async Task Muteb12Nov_WhoeverDoesNotApproveEntriesCannotReadIt()
    {
        var ledger = await DecidedAsync();
        var (reader, _) = await ledger.MemberAsync("Reader");

        Assert.Equal(HttpStatusCode.Forbidden, (await reader.GetAsync($"/projects/{ledger.ProjectId}/approvals/history")).StatusCode);
    }
}
