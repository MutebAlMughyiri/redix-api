using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.History;
using Redix.Api.Entries;
using Redix.Api.Notifications;

namespace Redix.Api.Tests;

/// <summary>UC-039: a file that arrived after the expense was entered goes on it, and nothing else changes (redix-api #96, D15).</summary>
[Collection(DatabaseCollection.Name)]
public class InvoiceAttachTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static string Address(Ledger ledger, string vatNumber = "", decimal total = 0, string date = "") =>
        $"/projects/{ledger.ProjectId}/entries/attachable?itemId={ledger.ItemId}&vatNumber={vatNumber}&total={total}&date={(date.Length == 0 ? "0001-01-01" : date)}";

    private static async Task<List<AttachableEntryView>> AttachableAsync(HttpClient client, Ledger ledger, string vatNumber = "", decimal total = 0, string date = "") =>
        (await client.GetFromJsonAsync<List<AttachableEntryView>>(Address(ledger, vatNumber, total, date), Json))!;

    private static AttachInvoiceRequest Invoice(Guid attachmentId) => new(attachmentId);

    private static Task<string> SupplierVatNumberAsync(Ledger ledger) =>
        ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == ledger.SupplierId).Select(row => row.VatNumber).SingleAsync());

    private static Task<HttpResponseMessage> AttachAsync(HttpClient client, Ledger ledger, Guid entryId, AttachInvoiceRequest request) =>
        client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/invoice", request, Json);

    private static async Task<(HttpClient Client, EntryRequest Request)> FieldRecordsAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("Field");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        var request = ledger.Purchase(200m) with { AccountId = account.Id, Submit = true };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", request, Json)).EnsureSuccessStatusCode();
        return (field, request);
    }

    [Fact]
    public async Task OnlyAnExpenseWithNoFileInThatItemIsOffered()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var without = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(without);
        await ledger.RecordAsync(ledger.Purchase() with { AttachmentId = await ledger.FileAsync() });

        var offered = await AttachableAsync(ledger.Client, ledger);

        Assert.Equal(without.Id, Assert.Single(offered).Id);
    }

    [Fact]
    public async Task AC_49_2_TheOneMatchingTheQrCodeIsMarkedAndComesFirst()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await factory.WithDatabaseAsync(db => db.Projects.Where(row => row.Id == ledger.ProjectId).ExecuteUpdateAsync(row => row.SetProperty(project => project.StartDate, ledger.Today.AddMonths(-1))));
        var vatNumber = await SupplierVatNumberAsync(ledger);
        var matching = ledger.Purchase(200m) with { Submit = true, Amount = 6000, Date = ledger.Today.AddDays(-3) };
        var far = ledger.Purchase(200m) with { Submit = true, Amount = 6002, Date = ledger.Today };
        var late = ledger.Purchase(200m) with { Submit = true, Amount = 6000, Date = ledger.Today.AddDays(-4) };
        await ledger.RecordAsync(matching);
        await ledger.RecordAsync(far);
        await ledger.RecordAsync(late);

        var offered = await AttachableAsync(ledger.Client, ledger, vatNumber, 6000.5m, $"{ledger.Today:yyyy-MM-dd}");

        Assert.Equal(matching.Id, offered[0].Id);
        Assert.True(offered[0].IsMatching);
        Assert.False(offered[1].IsMatching);
        Assert.False(offered[2].IsMatching);
    }

    [Fact]
    public async Task AC_49_4_FieldSeesOnlyTheirOwn()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(200m) with { Submit = true });
        var (field, own) = await FieldRecordsAsync(ledger);

        Assert.Equal(own.Id, Assert.Single(await AttachableAsync(field, ledger)).Id);
        Assert.Equal(2, (await AttachableAsync(ledger.Client, ledger)).Count);
    }

    [Fact]
    public async Task D15_AttachingPutsOnlyTheFileOnTheEntryAndWritesItInTheTrail()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(115m) with { Submit = true };
        await ledger.RecordAsync(request);
        var file = await ledger.FileAsync();
        var trailBefore = (await factory.HistoryOfAsync(request.Id)).Count;

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(file));

        Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
        var link = await ledger.ReadAsync(db => db.EntryAttachments.SingleAsync(row => row.EntryId == request.Id));
        Assert.Equal((file, EntryAttachmentRole.Invoice), (link.AttachmentId, link.Role));
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == request.Id)));
        Assert.Equal(115m, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id))).Amount);
        Assert.Equal(HistoryAction.Create, Assert.Single((await factory.HistoryOfAsync(request.Id)).Skip(trailBefore)).Action);
    }

    [Fact]
    public async Task AWaitingEntryStaysWaitingAndADraftStaysADraft()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var waiting = ledger.Purchase(200m) with { Submit = true };
        var draft = ledger.Purchase(200m);
        await ledger.RecordAsync(waiting);
        await ledger.RecordAsync(draft);

        await AttachAsync(ledger.Client, ledger, waiting.Id, Invoice(await ledger.FileAsync()));
        await AttachAsync(ledger.Client, ledger, draft.Id, Invoice(await ledger.FileAsync()));

        Assert.Equal(EntryStatus.PendingApproval, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == waiting.Id))).Status);
        Assert.Equal(EntryStatus.Draft, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == draft.Id))).Status);
    }

    [Fact]
    public async Task D15_AnApprovedEntryStaysApprovedAndNobodyIsAskedAgain()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (other, _) = await ledger.MemberAsync("Finance");
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);

        var toldBefore = (await other.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!.Count;

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(await ledger.FileAsync()));

        Assert.Equal(EntryStatus.Approved, (await answer.Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Status);
        Assert.Equal(EntryStatus.Approved, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id))).Status);
        Assert.Equal(toldBefore, (await other.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!.Count);
    }

    [Fact]
    public async Task AFileAlreadyOnAnEntryIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = ledger.Purchase(200m) with { Submit = true };
        var second = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);
        var file = await ledger.FileAsync();
        await AttachAsync(ledger.Client, ledger, first.Id, Invoice(file));

        var repeated = await AttachAsync(ledger.Client, ledger, second.Id, Invoice(file));

        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(repeated));
    }

    [Fact]
    public async Task SomeoneElsesEntryIsRefusedToField()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);
        var (field, _) = await ledger.MemberAsync("Field");

        var refused = await AttachAsync(field, ledger, request.Id, Invoice(await ledger.FileAsync()));

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.Equal("notYourEntry", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task WhatCannotTakeAFileIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var hasOne = ledger.Purchase() with { AttachmentId = await ledger.FileAsync() };
        await ledger.RecordAsync(hasOne);
        var deduction = ledger.Deduction(-50m);
        await ledger.RecordAsync(deduction);

        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, hasOne.Id, Invoice(await ledger.FileAsync()))));
        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, deduction.Id, Invoice(await ledger.FileAsync()))));
        Assert.DoesNotContain(await AttachableAsync(ledger.Client, ledger), row => row.Id == deduction.Id);
    }

    [Fact]
    public async Task App_119_ARemovedEntryIsNotOffered()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Removed);

        Assert.Empty(await AttachableAsync(ledger.Client, ledger));
    }

    [Fact]
    public async Task AFileOfNoSuchAttachmentIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);

        Assert.Equal("unknownAttachment", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(Guid.NewGuid()))));
    }
}
