using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>One entry for every table (redix-api #110): one route records and changes any entry, a partner's and revenue alike, with the same drafts, resends and fixes as an expense.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class OneEntryTests(MigratedApiFactory factory)
{
    private static async Task<Guid> PartnerAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name), Ledger.Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Ledger.Json))!.Single(row => row.Name == name).Id;
    }

    private static Task<List<TableCategoryView>> TablesAsync(Ledger ledger, HttpClient client) => client.GetFromJsonAsync<List<TableCategoryView>>($"/projects/{ledger.ProjectId}/entry-tables", Ledger.Json)!;

    private static Task<Entry> EntryAsync(Ledger ledger, Guid id) => ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == id));

    [Fact]
    public async Task A_110_R8_R13_APartnerEntryAndARevenueEntryAreKeptAsDraftsThatTellNobody()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var contribution = await ledger.PartnerEntryAsync(partner, 500) with { Submit = false };
        var revenue = await ledger.RevenueAsync(ItemType.OtherRevenue, 70) with { Submit = false };

        Assert.Equal(HttpStatusCode.Created, (await ledger.PostAsync(contribution)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await ledger.PostAsync(revenue)).StatusCode);

        Assert.Equal((EntryStatus.Draft, EntryStatus.Draft), ((await EntryAsync(ledger, contribution.Id)).Status, (await EntryAsync(ledger, revenue.Id)).Status));
        Assert.Empty(await ledger.ReadAsync(db => db.EntryDecisions.Where(row => row.EntryId == contribution.Id || row.EntryId == revenue.Id).ToListAsync()));
        Assert.Empty(await ledger.ReadAsync(db => db.Notifications.Where(row => row.EntryId == contribution.Id || row.EntryId == revenue.Id).ToListAsync()));
    }

    [Fact]
    public async Task A_110_R13_APartnerEntrySentForApprovalWaitsAsAnExpenseDoes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var contribution = await ledger.PartnerEntryAsync(partner, 500);

        (await ledger.PostAsync(contribution)).EnsureSuccessStatusCode();

        Assert.Equal(EntryStatus.PendingApproval, (await EntryAsync(ledger, contribution.Id)).Status);
        Assert.Equal(EntryDecisionAction.Submit, (await ledger.ReadAsync(db => db.EntryDecisions.SingleAsync(row => row.EntryId == contribution.Id))).Action);
    }

    [Fact]
    public async Task A_110_R15_APartnerEntryPostedTwiceIsOneEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var contribution = await ledger.PartnerEntryAsync(partner, 500);

        var first = await ledger.PostAsync(contribution);
        var again = await ledger.PostAsync(contribution);

        Assert.Equal((HttpStatusCode.Created, HttpStatusCode.OK), (first.StatusCode, again.StatusCode));
        Assert.Single(await ledger.ReadAsync(db => db.CapitalMovementDetails.Where(row => row.PartnerId == partner).ToListAsync()));
    }

    [Fact]
    public async Task A_110_R14_ARejectedPartnerEntryIsFixedAndSentAgain()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var contribution = await ledger.PartnerEntryAsync(partner, 500);
        (await ledger.PostAsync(contribution)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{contribution.Id}/reject", new RejectEntryRequest("أرفق إيصال التحويل"), Ledger.Json)).EnsureSuccessStatusCode();

        var fixedOne = await ledger.ChangeAsync(contribution with { Amount = 600, AttachmentId = await ledger.FileAsync() });

        Assert.Equal(HttpStatusCode.OK, fixedOne.StatusCode);
        var entry = await EntryAsync(ledger, contribution.Id);
        Assert.Equal((EntryStatus.PendingApproval, 600m, EntryKind.CapitalIn), (entry.Status, entry.Amount, entry.Kind));
        Assert.Equal(EntryAttachmentRole.BankReceipt, (await ledger.ReadAsync(db => db.EntryAttachments.SingleAsync(row => row.EntryId == contribution.Id))).Role);
    }

    [Fact]
    public async Task A_110_G3_ADraftMovedToAnotherKindOfTableBecomesThatKindAndKeepsNothingOfTheOld()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var purchase = ledger.Purchase(200) with { Product = "بلاط" };
        await ledger.RecordAsync(purchase);

        var moved = await ledger.ChangeAsync(await ledger.PartnerEntryAsync(partner, 200, isWithdrawal: true) with { Id = purchase.Id, Submit = false });

        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
        Assert.Equal(EntryKind.CapitalOut, (await EntryAsync(ledger, purchase.Id)).Kind);
        Assert.Empty(await ledger.ReadAsync(db => db.PurchaseDetails.Where(row => row.EntryId == purchase.Id).ToListAsync()));
        Assert.Empty(await ledger.ReadAsync(db => db.ExpenseDetails.Where(row => row.EntryId == purchase.Id).ToListAsync()));
        Assert.Equal(CapitalMovementType.Withdrawal, (await ledger.ReadAsync(db => db.CapitalMovementDetails.SingleAsync(row => row.EntryId == purchase.Id))).Type);
    }

    [Fact]
    public async Task A_110_R11_ThePickerAndTheRouteOfferTheSameTables()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var (field, _) = await ledger.MemberAsync("Field");
        var (reader, _) = await ledger.MemberAsync("Reader");

        var financeItems = (await TablesAsync(ledger, ledger.Client)).SelectMany(category => category.Items).ToList();
        var fieldItems = (await TablesAsync(ledger, field)).SelectMany(category => category.Items).ToList();
        var refused = await ledger.PostAsync(await ledger.PartnerEntryAsync(partner, 100), field);

        Assert.Contains(financeItems, item => item.Tables.Any(table => table.Id == partner));
        Assert.Contains(financeItems, item => item.IsRevenue);
        Assert.DoesNotContain(fieldItems, item => item.IsRevenue);
        Assert.Contains(fieldItems, item => item.Id == ledger.ItemId);
        Assert.Equal((HttpStatusCode.BadRequest, "unknownTable"), (refused.StatusCode, await Ledger.CodeOf(refused)));
        Assert.Empty(await TablesAsync(ledger, reader));
    }

    [Fact]
    public async Task A_110_D7_RevenueAndAPartnersMoneyAreAboveZeroAndAskNoPaymentMethod()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");

        await ledger.AssertRefusedAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, -70, "خصم"), "invalidAmount");
        await ledger.AssertRefusedAsync(await ledger.PartnerEntryAsync(partner, -70, notes: "خصم"), "invalidAmount");
        await ledger.AssertRefusedAsync(ledger.Purchase() with { PaymentMethodId = Guid.Empty }, "unknownPaymentMethod");

        Assert.Equal(HttpStatusCode.Created, (await ledger.PostAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, 70))).StatusCode);
    }

    [Fact]
    public async Task A_110_R14_AnEntryReadToBeChangedNamesItsTableWhateverItsKind()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var partner = await PartnerAsync(ledger, "شريك");
        var contribution = await ledger.PartnerEntryAsync(partner, 500);
        var revenue = await ledger.RevenueAsync(ItemType.OtherRevenue, 70);
        var purchase = ledger.Purchase();
        foreach (var request in new[] { contribution, revenue, purchase })
        {
            (await ledger.PostAsync(request)).EnsureSuccessStatusCode();
        }

        async Task<Guid> TableOfAsync(Guid id) => (await ledger.Client.GetFromJsonAsync<EntryView>($"/projects/{ledger.ProjectId}/entries/{id}", Ledger.Json))!.TableId;

        Assert.Equal((partner, revenue.TableId, ledger.TableId), (await TableOfAsync(contribution.Id), await TableOfAsync(revenue.Id), await TableOfAsync(purchase.Id)));
    }
}
