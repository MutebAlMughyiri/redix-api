using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Audit;
using Redix.Api.Entries;
using Redix.Api.Notifications;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-039: an invoice that arrived after the expense was entered.</summary>
[Collection(DatabaseCollection.Name)]
public class InvoiceAttachTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static string Address(Ledger ledger, string vatNumber = "", decimal total = 0, string date = "") =>
        $"/projects/{ledger.ProjectId}/entries/attachable?itemId={ledger.ItemId}&vatNumber={vatNumber}&total={total}&date={(date.Length == 0 ? "0001-01-01" : date)}";

    private static async Task<List<AttachableEntryView>> AttachableAsync(HttpClient client, Ledger ledger, string vatNumber = "", decimal total = 0, string date = "") =>
        (await client.GetFromJsonAsync<List<AttachableEntryView>>(Address(ledger, vatNumber, total, date), Json))!;

    private static AttachInvoiceRequest Invoice(Ledger ledger, Guid attachmentId, string number = "2617", decimal total = 0, string sellerVatNumber = "") =>
        new(attachmentId, number, ledger.Today, total, sellerVatNumber);

    private static Task<string> SupplierVatNumberAsync(Ledger ledger) =>
        ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == ledger.SupplierId).Select(row => row.VatNumber).SingleAsync());

    private static Task<HttpResponseMessage> AttachAsync(HttpClient client, Ledger ledger, Guid entryId, AttachInvoiceRequest request) =>
        client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/invoice", request, Json);

    private static async Task<(HttpClient Client, ExpenseRequest Request)> FieldRecordsAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("Field");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody, 0) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        var request = ledger.WithoutInvoice() with { AccountId = account.Id, Submit = true };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/expenses", request, Json)).EnsureSuccessStatusCode();
        return (field, request);
    }

    [Fact]
    public async Task OnlyAnExpenseWithoutAnInvoiceInThatItemIsOffered()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var without = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(without);
        await ledger.RecordAsync(ledger.Purchase());

        var offered = await AttachableAsync(ledger.Client, ledger);

        Assert.Equal(without.Id, Assert.Single(offered).Id);
    }

    [Fact]
    public async Task AC_49_2_TheOneMatchingTheQrCodeIsMarkedAndComesFirst()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await factory.WithDatabaseAsync(db => db.Projects.Where(row => row.Id == ledger.ProjectId).ExecuteUpdateAsync(row => row.SetProperty(project => project.StartDate, ledger.Today.AddMonths(-1))));
        var vatNumber = await SupplierVatNumberAsync(ledger);
        var matching = ledger.WithoutInvoice() with { Submit = true, Amount = 6000, PaymentDate = ledger.Today.AddDays(-3) };
        var far = ledger.WithoutInvoice() with { Submit = true, Amount = 6002, PaymentDate = ledger.Today };
        var late = ledger.WithoutInvoice() with { Submit = true, Amount = 6000, PaymentDate = ledger.Today.AddDays(-4) };
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
        await ledger.RecordAsync(ledger.WithoutInvoice() with { Submit = true });
        var (field, own) = await FieldRecordsAsync(ledger);

        Assert.Equal(own.Id, Assert.Single(await AttachableAsync(field, ledger)).Id);
        Assert.Equal(2, (await AttachableAsync(ledger.Client, ledger)).Count);
    }

    [Fact]
    public async Task AttachingPutsTheInvoiceOnTheEntryAndWritesItInTheTrail()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true, Amount = 115 };
        await ledger.RecordAsync(request);
        var file = await ledger.FileAsync();

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, file));

        Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
        var invoice = await ledger.ReadAsync(db => db.Invoices.SingleAsync(row => row.EntryId == request.Id));
        Assert.Equal(("2617", 115m, 15m), (invoice.Number, invoice.Total, invoice.Vat));
        Assert.Equal(file, (await ledger.ReadAsync(db => db.EntryAttachments.SingleAsync(row => row.EntryId == request.Id))).AttachmentId);
        Assert.Empty((await ledger.ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == request.Id))).WithoutInvoiceReason);
        Assert.Contains(await factory.AuditTrailOfAsync(request.Id), row => row.Action == AuditAction.Update);
    }

    [Fact]
    public async Task AnInvoiceWithNoNumberIsAttached()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, await ledger.FileAsync(), ""));

        Assert.Equal(HttpStatusCode.OK, answer.StatusCode);
        Assert.Empty((await ledger.ReadAsync(db => db.Invoices.SingleAsync(row => row.EntryId == request.Id))).Number);
    }

    [Fact]
    public async Task AWaitingEntryStaysWaitingAndADraftStaysADraft()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var waiting = ledger.WithoutInvoice() with { Submit = true };
        var draft = ledger.WithoutInvoice();
        await ledger.RecordAsync(waiting);
        await ledger.RecordAsync(draft);

        await AttachAsync(ledger.Client, ledger, waiting.Id, Invoice(ledger, await ledger.FileAsync(), "1", 999));
        await AttachAsync(ledger.Client, ledger, draft.Id, Invoice(ledger, await ledger.FileAsync(), "2", 999));

        Assert.Equal(EntryStatus.PendingApproval, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == waiting.Id))).Status);
        Assert.Equal(EntryStatus.Draft, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == draft.Id))).Status);
    }

    [Fact]
    public async Task AC_49_3_AnApprovedEntryGoesBackToApprovalAndFinanceIsTold()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (other, _) = await ledger.MemberAsync("Finance");
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, await ledger.FileAsync()));

        Assert.Equal(EntryStatus.PendingApproval, (await answer.Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Status);
        Assert.Equal(Entry.NotApproved, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id))).ApprovedAt);
        Assert.Contains((await other.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!, row => row.EntryId == request.Id && row.Kind == NotificationKind.EntrySubmitted);
    }

    [Fact]
    public async Task AC_49_3_FinanceSeesTheDifferenceWhenItDecides()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var vatNumber = await SupplierVatNumberAsync(ledger);
        var request = ledger.WithoutInvoice() with { Submit = true, Amount = 100 };
        await ledger.RecordAsync(request);

        await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, await ledger.FileAsync(), total: 150, sellerVatNumber: "300000000000099"));

        var pending = (await ledger.Client.GetFromJsonAsync<List<PendingEntryView>>($"/projects/{ledger.ProjectId}/entries/pending", Json))!.Single(row => row.Id == request.Id);
        Assert.Equal((100m, 150m, "300000000000099", vatNumber), (pending.Amount, pending.InvoiceTotal, pending.SellerVatNumber, pending.SupplierVatNumber));
    }

    [Fact]
    public async Task AC_49_3_TheSameInvoiceCannotGoOnTwoExpensesEvenIfConfirmed()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = ledger.WithoutInvoice() with { Submit = true };
        var second = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);
        await AttachAsync(ledger.Client, ledger, first.Id, Invoice(ledger, await ledger.FileAsync(), "2617"));

        var repeated = await AttachAsync(ledger.Client, ledger, second.Id, Invoice(ledger, await ledger.FileAsync(), "2617"));

        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        Assert.Equal("duplicateInvoice", await Ledger.CodeOf(repeated));
    }

    [Fact]
    public async Task AFileAlreadyOnAnEntryIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = ledger.WithoutInvoice() with { Submit = true };
        var second = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);
        var file = await ledger.FileAsync();
        await AttachAsync(ledger.Client, ledger, first.Id, Invoice(ledger, file, "1"));

        var repeated = await AttachAsync(ledger.Client, ledger, second.Id, Invoice(ledger, file, "2"));

        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(repeated));
    }

    [Fact]
    public async Task SomeoneElsesEntryIsRefusedToField()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        var (field, _) = await ledger.MemberAsync("Field");

        var refused = await AttachAsync(field, ledger, request.Id, Invoice(ledger, await ledger.FileAsync()));

        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.Equal("notYourEntry", await Ledger.CodeOf(refused));
    }

    [Fact]
    public async Task WhatCannotTakeAnInvoiceIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var hasOne = ledger.Purchase();
        await ledger.RecordAsync(hasOne);
        var deduction = ledger.WithoutInvoice() with { Amount = -50 };
        await ledger.RecordAsync(deduction);

        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, hasOne.Id, Invoice(ledger, await ledger.FileAsync()))));
        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, deduction.Id, Invoice(ledger, await ledger.FileAsync()))));
        Assert.DoesNotContain(await AttachableAsync(ledger.Client, ledger), row => row.Id == deduction.Id);
    }

    [Fact]
    public async Task AReversedEntryIsNotOffered()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/reverse", new RejectEntryRequest("خطأ"), Json)).EnsureSuccessStatusCode();

        Assert.Empty(await AttachableAsync(ledger.Client, ledger));
    }

    [Fact]
    public async Task WhatWasTypedIsChecked()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        var file = await ledger.FileAsync();

        Assert.Equal("invalidInvoiceNumber", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, file, new string('1', Redix.Api.Entries.Invoice.NumberMaxLength + 1)))));
        Assert.Equal("invalidInvoiceDate", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, file) with { InvoiceDate = new DateOnly(2000, 1, 1) })));
        Assert.Equal("invalidAmount", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, file, total: -1))));
        Assert.Equal("invalidVatNumber", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, file, sellerVatNumber: "3000000000000030"))));
        Assert.Equal("unknownAttachment", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, request.Id, Invoice(ledger, Guid.NewGuid()))));
    }
}
