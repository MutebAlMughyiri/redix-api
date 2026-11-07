using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-061: liquidating and closing a project (FR-51, BR-27). No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class LiquidationTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<Guid> PartnerAsync(HttpClient client, Guid projectId, string name)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/partners", new PartnerRequest(name), Json)).EnsureSuccessStatusCode();
        return (await client.GetFromJsonAsync<List<PartnerView>>($"/projects/{projectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    private static async Task ContributeAsync(Ledger ledger, Guid partnerId, decimal amount) =>
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/capital-movements", new CapitalMovementRequest(partnerId, CapitalMovementType.Contribution, ledger.Today, amount, ledger.AccountId, "") { CashReason = "نقداً" }, Json)).EnsureSuccessStatusCode();

    private static async Task SpendAsync(Ledger ledger, decimal amount)
    {
        var expense = ledger.Purchase(amount) with { Submit = true };
        await ledger.RecordAsync(expense);
        await ledger.SetStatusAsync(expense.Id, EntryStatus.Approved);
    }

    private static async Task<(Ledger Ledger, Guid First, Guid Second)> TwoPartnersAsync(MigratedApiFactory factory)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await PartnerAsync(ledger.Client, ledger.ProjectId, "سالم");
        var second = await PartnerAsync(ledger.Client, ledger.ProjectId, "فهد");
        await ContributeAsync(ledger, first, 600);
        await ContributeAsync(ledger, second, 400);
        await SpendAsync(ledger, 100);
        return (ledger, first, second);
    }

    private static async Task<LiquidationView> ViewAsync(Ledger ledger) => (await ledger.Client.GetFromJsonAsync<LiquidationView>($"/projects/{ledger.ProjectId}/liquidation", Json))!;

    private static Task<HttpResponseMessage> LiquidateAsync(Ledger ledger, params List<LiquidationLineRequest> lines) =>
        ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/liquidation", new LiquidationRequest(ledger.Today, lines), Json);

    [Fact]
    public async Task BR_27_EachPartnerTakesTheirShareOfTheSurplusAndThePartsMakeItExactly()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        foreach (var name in new[] { "سالم", "فهد", "نايف" })
        {
            await ContributeAsync(ledger, await PartnerAsync(ledger.Client, ledger.ProjectId, name), 100);
        }
        await SpendAsync(ledger, 200);

        var view = await ViewAsync(ledger);

        Assert.Equal((100m, 0), (view.Surplus, view.PendingCount));
        Assert.Equal([33.33m, 33.33m, 33.34m], view.Parts.Select(part => part.Amount));
    }

    [Fact]
    public async Task FR_51_PaidOutTheSurplusIsTheLiquidationTheCapitalStaysAndTheProjectCloses()
    {
        var (ledger, first, second) = await TwoPartnersAsync(factory);
        Assert.Equal([540m, 360m], (await ViewAsync(ledger)).Parts.OrderByDescending(part => part.Amount).Select(part => part.Amount));

        (await LiquidateAsync(ledger, new LiquidationLineRequest(first, ledger.AccountId), new LiquidationLineRequest(second, ledger.AccountId))).EnsureSuccessStatusCode();

        var figures = await ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));
        Assert.Equal((1000m, 900m, 0m), (figures.Capital, figures.Liquidation, figures.Surplus));
        var capital = (await ledger.Client.GetFromJsonAsync<CapitalView>($"/projects/{ledger.ProjectId}/capital", Json))!;
        Assert.Equal([600m, 400m], capital.Partners.Select(partner => partner.Balance));
        Assert.Single((await ledger.Client.GetFromJsonAsync<List<CapitalMovementView>>($"/projects/{ledger.ProjectId}/partners/{first}/movements", Json))!);
        var view = await ViewAsync(ledger);
        Assert.Equal((0m, 900m), (view.Surplus, view.Distributed.Sum(share => share.Amount)));
        Assert.Equal(ProjectStatus.Finished, await ledger.ReadAsync(db => db.Projects.Where(project => project.Id == ledger.ProjectId).Select(project => project.Status).SingleAsync()));
        Assert.Equal("projectFinished", await Ledger.CodeOf(await LiquidateAsync(ledger)));
    }

    [Fact]
    public async Task FR_51_APartCarriedReachesThePartnerInTheLaterProject()
    {
        var (ledger, first, second) = await TwoPartnersAsync(factory);
        var later = await factory.ApprovedProjectAsync(ledger.Client, "غصون 7", "الفلة");
        var user = await ledger.ReadAsync(db => db.ProjectPermissions.Where(row => row.ProjectId == ledger.ProjectId).Select(row => row.UserId).FirstAsync());
        await factory.GiveAsync(user, later.Id, Holds.Of("Finance"));
        var laterPartner = await PartnerAsync(ledger.Client, later.Id, "سالم");
        (await ledger.Client.PostAsJsonAsync($"/projects/{later.Id}/accounts", new AddAccountRequest("حساب", AccountType.Bank, 0), Json)).EnsureSuccessStatusCode();
        var laterAccount = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{later.Id}/accounts", Json))!.Single(row => row.Type == AccountType.Bank).Id;

        (await LiquidateAsync(ledger,
            new LiquidationLineRequest(first, ledger.AccountId) { TargetProjectId = later.Id, TargetPartnerId = laterPartner, TargetAccountId = laterAccount },
            new LiquidationLineRequest(second, ledger.AccountId))).EnsureSuccessStatusCode();

        var there = (await ledger.Client.GetFromJsonAsync<CapitalView>($"/projects/{later.Id}/capital", Json))!;
        Assert.Equal(540m, there.Partners.Single().Balance);
        Assert.Equal(900m, (await ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db))).Liquidation);
    }

    [Fact]
    public async Task BR_27_ADeficitIsPaidInByShareAndLeavesNothing()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger.Client, ledger.ProjectId, "سالم");
        await ContributeAsync(ledger, partner, 100);
        await SpendAsync(ledger, 300);
        Assert.Equal(-200m, Assert.Single((await ViewAsync(ledger)).Parts).Amount);

        (await LiquidateAsync(ledger, new LiquidationLineRequest(partner, ledger.AccountId))).EnsureSuccessStatusCode();

        var figures = await ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));
        Assert.Equal((100m, -200m, 0m), (figures.Capital, figures.Liquidation, figures.Surplus));
    }

    [Fact]
    public async Task BR_35_EntriesWaitingAndALineMissingOrExtraAreRefusedAndNothingIsEntered()
    {
        var (ledger, first, second) = await TwoPartnersAsync(factory);

        Assert.Equal("incompleteLiquidation", await Ledger.CodeOf(await LiquidateAsync(ledger, new LiquidationLineRequest(first, ledger.AccountId))));
        Assert.Equal("incompleteLiquidation", await Ledger.CodeOf(await LiquidateAsync(ledger, new LiquidationLineRequest(first, ledger.AccountId), new LiquidationLineRequest(first, ledger.AccountId), new LiquidationLineRequest(second, ledger.AccountId))));
        Assert.Equal("unknownAccount", await Ledger.CodeOf(await LiquidateAsync(ledger, new LiquidationLineRequest(first, Guid.NewGuid()), new LiquidationLineRequest(second, ledger.AccountId))));
        await ledger.RecordAsync(ledger.Purchase(5) with { Submit = true });
        var waiting = await LiquidateAsync(ledger, new LiquidationLineRequest(first, ledger.AccountId), new LiquidationLineRequest(second, ledger.AccountId));

        Assert.Equal((HttpStatusCode.Conflict, "pendingEntries"), (waiting.StatusCode, await Ledger.CodeOf(waiting)));
        Assert.Equal(0m, (await ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db))).Liquidation);
    }

    [Fact]
    public async Task APartOfTheLiquidationCannotBeEnteredAsAPlainMovement()
    {
        var (ledger, first, _) = await TwoPartnersAsync(factory);

        var refused = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/capital-movements", new CapitalMovementRequest(first, CapitalMovementType.Liquidation, ledger.Today, 10, ledger.AccountId, "") { CashReason = "نقداً" }, Json);

        Assert.Equal("useLiquidation", await Ledger.CodeOf(refused));
    }

    private async Task<(Ledger Ledger, Guid First, Guid Second)> LiquidatedAsync()
    {
        var (ledger, first, second) = await TwoPartnersAsync(factory);
        (await LiquidateAsync(ledger, new LiquidationLineRequest(first, ledger.AccountId), new LiquidationLineRequest(second, ledger.AccountId))).EnsureSuccessStatusCode();
        return (ledger, first, second);
    }

    private static Task<HttpResponseMessage> LateAsync(Ledger ledger, LateEntryRequest request) => ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/liquidation/late", request, Json);

    [Fact]
    public async Task BR_27_ALateExpenseIsPaidInAndLateRevenueTakenByTheSharesAtClosingAndTheSurplusStaysNothing()
    {
        var (ledger, _, _) = await LiquidatedAsync();

        (await LateAsync(ledger, new LateEntryRequest(ledger.ItemId, ledger.Today, 100, ledger.AccountId, "فاتورة كهرباء متأخرة"))).EnsureSuccessStatusCode();
        (await LateAsync(ledger, new LateEntryRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 50, ledger.AccountId, "استرداد تأمين"))).EnsureSuccessStatusCode();

        var figures = await ledger.ReadAsync(db => ProjectFigures.OfAsync(ledger.ProjectId, db));
        Assert.Equal((200m, 850m, 0m), (figures.Expenses, figures.Liquidation, figures.Surplus));
        var shared = (await ViewAsync(ledger)).Distributed.Where(share => share.Description != "تصفية المشروع").Select(share => share.Amount).Order();
        Assert.Equal([-60m, -40m, 20m, 30m], shared);
    }

    [Fact]
    public async Task FR_51_ALateEntryNeedsAClosedProjectAndAnItemThatTakesPurchases()
    {
        var (ledger, _, _) = await TwoPartnersAsync(factory);
        Assert.Equal("projectNotClosed", await Ledger.CodeOf(await LateAsync(ledger, new LateEntryRequest(await ledger.RevenueItemAsync(ItemType.OtherRevenue), ledger.Today, 50, ledger.AccountId, "إيراد"))));
        var (closed, _, _) = await LiquidatedAsync();

        Assert.Equal("unknownItem", await Ledger.CodeOf(await LateAsync(closed, new LateEntryRequest(Guid.Empty, closed.Today, 50, closed.AccountId, "مصروف"))));
    }

    [Fact]
    public async Task BR_23_APayoutsReceiptIsAttachedAfterTheProjectClosesAndOnlyOnce()
    {
        var (ledger, _, _) = await LiquidatedAsync();
        var part = (await ViewAsync(ledger)).Distributed.First();
        Assert.Equal("", part.ReceiptId);

        Task<HttpResponseMessage> AttachAsync()
        {
            var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, .. Guid.NewGuid().ToByteArray()]);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            return ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/liquidation/{part.EntryId}/receipt", new MultipartFormDataContent { { file, "file", "receipt.jpg" } });
        }

        (await AttachAsync()).EnsureSuccessStatusCode();
        Assert.NotEqual("", (await ViewAsync(ledger)).Distributed.Single(share => share.EntryId == part.EntryId).ReceiptId);
        Assert.Equal("receiptAlreadyAttached", await Ledger.CodeOf(await AttachAsync()));
    }

    [Fact]
    public async Task BR_27_APlainCloseIsRefusedWhileThereIsASurplusToShare()
    {
        var (ledger, _, _) = await TwoPartnersAsync(factory);

        var refused = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null);

        Assert.Equal((HttpStatusCode.Conflict, "liquidateFirst"), (refused.StatusCode, await Ledger.CodeOf(refused)));
    }
}
