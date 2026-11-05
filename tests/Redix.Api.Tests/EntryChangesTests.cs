using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Audit;
using Redix.Api.Entries;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-033, UC-034 and UC-035: changing an entry before approval, and reversing one after.</summary>
[Collection(DatabaseCollection.Name)]
public class EntryChangesTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static Task<HttpResponseMessage> EditAsync(HttpClient client, Ledger ledger, ExpenseRequest request) =>
        client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/expenses/{request.Id}", request, Json);

    private static Task<HttpResponseMessage> DeleteAsync(HttpClient client, Ledger ledger, Guid id) =>
        client.PostAsync($"/projects/{ledger.ProjectId}/entries/{id}/delete", null);

    private static Task<HttpResponseMessage> ReverseAsync(HttpClient client, Ledger ledger, Guid id, string reason = "إدخال خاطئ") =>
        client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reverse", new RejectEntryRequest(reason), Json);

    private static async Task ApproveAsync(Ledger ledger, Guid id) =>
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([id]), Json)).EnsureSuccessStatusCode();

    private static async Task AssertRefused(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, await Ledger.CodeOf(response));
    }

    private static async Task<(HttpClient Client, ExpenseRequest Request)> FieldDraftAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("users.roleField");
        var custody = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody, 0) { CustodianId = userId }, Json);
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        custody.EnsureSuccessStatusCode();
        var request = ledger.WithoutInvoice() with { AccountId = account.Id };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/expenses", request, Json)).EnsureSuccessStatusCode();
        return (field, request);
    }

    [Fact]
    public async Task AnAuthorChangesTheirDraftAndItHoldsTheNewValues()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(1150m);
        await ledger.RecordAsync(request);

        (await EditAsync(ledger.Client, ledger, request with { Amount = 2300m, Notes = "بعد التصحيح" })).EnsureSuccessStatusCode();

        var (entry, invoice) = await ledger.ReadAsync(async db => (await db.Entries.SingleAsync(row => row.Id == request.Id), await db.Invoices.SingleAsync(row => row.EntryId == request.Id)));
        Assert.Equal(2300m, entry.Amount);
        Assert.Equal("بعد التصحيح", entry.Notes);
        Assert.Equal(300m, invoice.Vat);
        Assert.Equal(EntryStatus.Draft, entry.Status);
    }

    [Fact]
    public async Task AChangeKeepsWhoEnteredItAndWhen()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, request) = await FieldDraftAsync(ledger);
        var before = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        factory.Time.Advance(TimeSpan.FromMinutes(1));

        (await EditAsync(ledger.Client, ledger, request with { Notes = "من المالي", AccountId = ledger.AccountId })).EnsureSuccessStatusCode();

        var after = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal((before.CreatedByUserId, before.CreatedAt), (after.CreatedByUserId, after.CreatedAt));
        Assert.Equal(EntryStatus.Draft, after.Status);
    }

    [Fact]
    public async Task EveryChangeIsInTheAuditTrailWithTheValuesBeforeAndAfter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(1150m);
        await ledger.RecordAsync(request);

        (await EditAsync(ledger.Client, ledger, request with { Amount = 2300m })).EnsureSuccessStatusCode();

        var update = (await factory.AuditTrailOfAsync(request.Id)).Last(record => record.Action == AuditAction.Update && record.TableName == "Entries");
        Assert.Equal(1150m, JsonDocument.Parse(update.Before).RootElement.GetProperty("Amount").GetDecimal());
        Assert.Equal(2300m, JsonDocument.Parse(update.After).RootElement.GetProperty("Amount").GetDecimal());
    }

    [Fact]
    public async Task AnEditGoesThroughTheChecksOfANewEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase();
        await ledger.RecordAsync(request);

        var refused = await EditAsync(ledger.Client, ledger, request with { SupplierId = Guid.CreateVersion7() });

        await AssertRefused(refused, HttpStatusCode.BadRequest, "unknownSupplier");
        Assert.Equal(request.SupplierId, await ledger.ReadAsync(db => db.PurchaseDetails.Where(row => row.EntryId == request.Id).Select(row => row.SupplierId).SingleAsync()));
    }

    [Fact]
    public async Task ARejectedEntryIsFixedAndSentAgainInOneEdit()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/reject", new RejectEntryRequest("المبلغ خطأ"), Json)).EnsureSuccessStatusCode();

        (await EditAsync(ledger.Client, ledger, request with { Amount = 300m })).EnsureSuccessStatusCode();

        Assert.Equal(EntryStatus.PendingApproval, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == request.Id).Select(row => row.Status).SingleAsync()));
    }

    [Fact]
    public async Task AnEditKeepsItsOwnFileAndInvoiceNumberWithoutCallingThemRepeats()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase() with { AttachmentId = await ledger.FileAsync() };
        await ledger.RecordAsync(request);

        (await EditAsync(ledger.Client, ledger, request with { Notes = "ملاحظة" })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TheFieldCannotChangeWhatWaitsForADecisionOrSomeoneElsesEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request) = await FieldDraftAsync(ledger);
        var theirs = ledger.Purchase();
        await ledger.RecordAsync(theirs);
        await AssertRefused(await EditAsync(field, ledger, theirs with { Notes = "لي" }), HttpStatusCode.Forbidden, "notYourEntry");

        (await field.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/submit", null)).EnsureSuccessStatusCode();

        await AssertRefused(await EditAsync(field, ledger, request with { Notes = "بعد الإرسال" }), HttpStatusCode.Conflict, "entryWaitingForDecision");
        await AssertRefused(await DeleteAsync(field, ledger, request.Id), HttpStatusCode.Conflict, "entryWaitingForDecision");
    }

    [Fact]
    public async Task FinanceChangesAnyEntryBeforeApproval()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request) = await FieldDraftAsync(ledger);
        (await field.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/submit", null)).EnsureSuccessStatusCode();

        (await EditAsync(ledger.Client, ledger, request with { Notes = "صححها المالي", AccountId = ledger.AccountId })).EnsureSuccessStatusCode();

        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal(("صححها المالي", EntryStatus.PendingApproval), (entry.Notes, entry.Status));
    }

    [Fact]
    public async Task AC_16_2_AnOfflineEditOfAnApprovedEntryIsRefusedAndTheServersVersionStays()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice(500m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);

        var refused = await EditAsync(ledger.Client, ledger, request with { Amount = 9m });

        await AssertRefused(refused, HttpStatusCode.Conflict, "entryApproved");
        Assert.Equal(500m, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == request.Id).Select(row => row.Amount).SingleAsync()));
    }

    [Fact]
    public async Task AC_50_2_AnApprovedEntrysNotesMayChangeAndItIsNotDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/notes", new ChangeNotesRequest("ملاحظة بعد الاعتماد"), Json)).EnsureSuccessStatusCode();

        Assert.Equal("ملاحظة بعد الاعتماد", await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == request.Id).Select(row => row.Notes).SingleAsync()));
        await AssertRefused(await DeleteAsync(ledger.Client, ledger, request.Id), HttpStatusCode.Conflict, "entryApproved");
    }

    [Fact]
    public async Task ADeletedEntryLeavesNothingButTheAuditTrail()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(request);

        (await DeleteAsync(ledger.Client, ledger, request.Id)).EnsureSuccessStatusCode();

        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(row => row.Id == request.Id)));
        Assert.False(await ledger.ReadAsync(db => db.ExpenseDetails.AnyAsync(row => row.EntryId == request.Id)));
        Assert.Contains(await factory.AuditTrailOfAsync(request.Id), record => record.Action == AuditAction.Delete);
    }

    [Fact]
    public async Task AnInvoiceACreditNoteReturnsIsNotDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var invoice = ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(invoice);
        await ledger.RecordAsync(ledger.Purchase(100m) with { IsCreditNote = true, OriginalEntryId = invoice.Id });

        await AssertRefused(await DeleteAsync(ledger.Client, ledger, invoice.Id), HttpStatusCode.Conflict, "entryHasCreditNote");
    }

    [Fact]
    public async Task AC_50_1_AReversalIsAnOppositeEntryTiedToTheOriginalAndTheNetEffectIsZero()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(1150m) with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);
        var original = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));

        var response = await ReverseAsync(ledger.Client, ledger, request.Id);

        var reversal = (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!;
        Assert.Equal(EntryStatus.Approved, reversal.Status);
        var opposite = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == reversal.Id));
        Assert.Equal(-1150m, opposite.Amount);
        var link = await ledger.ReadAsync(db => db.EntryLinks.SingleAsync(row => row.EntryId == reversal.Id));
        Assert.Equal((EntryLinkType.Reversal, request.Id, "إدخال خاطئ"), (link.Type, link.LinkedEntryId, link.Reason));
        var after = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal((original.Amount, original.Status, original.Notes), (after.Amount, after.Status, after.Notes));
        var vat = await ledger.ReadAsync(db => db.Invoices.Where(row => row.EntryId == request.Id || row.EntryId == reversal.Id).SumAsync(row => row.Vat));
        Assert.Equal(0m, vat);
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single();
        Assert.Equal(account.OpeningBalance, account.Balance);
    }

    [Fact]
    public async Task OnlyAnApprovedEntryIsReversedOnceAndWithAReason()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);

        await AssertRefused(await ReverseAsync(ledger.Client, ledger, request.Id), HttpStatusCode.Conflict, "entryNotApproved");

        await ApproveAsync(ledger, request.Id);
        await AssertRefused(await ReverseAsync(ledger.Client, ledger, request.Id, "  "), HttpStatusCode.BadRequest, "reasonRequired");

        var reversal = (await (await ReverseAsync(ledger.Client, ledger, request.Id)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!;
        await AssertRefused(await ReverseAsync(ledger.Client, ledger, request.Id), HttpStatusCode.Conflict, "alreadyReversed");
        await AssertRefused(await ReverseAsync(ledger.Client, ledger, reversal.Id), HttpStatusCode.Conflict, "alreadyReversed");
    }

    [Fact]
    public async Task OnlyWhoeverDecidesEntriesReversesThem()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("users.roleField");
        var request = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);

        Assert.Equal(HttpStatusCode.Forbidden, (await ReverseAsync(field, ledger, request.Id)).StatusCode);
    }
}
