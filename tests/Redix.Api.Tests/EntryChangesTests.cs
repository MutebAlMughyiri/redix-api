using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>UC-033 and UC-034: changing an entry before approval. Removing one after it is in <see cref="RemoveApprovedEntryTests"/> (redix-api #119).</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class EntryChangesTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static Task<HttpResponseMessage> EditAsync(HttpClient client, Ledger ledger, EntryRequest request) =>
        client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}", request, Json);

    private static Task<HttpResponseMessage> DeleteAsync(HttpClient client, Ledger ledger, Guid id) =>
        client.DeleteAsync($"/projects/{ledger.ProjectId}/entries/{id}");

    private static async Task ApproveAsync(Ledger ledger, Guid id) =>
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([id]), Json)).EnsureSuccessStatusCode();

    private static async Task AssertRefused(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, await Ledger.CodeOf(response));
    }

    private static async Task<(HttpClient Client, EntryRequest Request, Guid UserId)> FieldDraftAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("Field");
        var custody = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody) { CustodianId = userId }, Json);
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        custody.EnsureSuccessStatusCode();
        var request = ledger.Purchase(200m) with { AccountId = account.Id };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", request, Json)).EnsureSuccessStatusCode();
        return (field, request, userId);
    }

    private static Task<HttpResponseMessage> RowAsync(HttpClient client, Ledger ledger, Guid id) => client.GetAsync($"/projects/{ledger.ProjectId}/entries/{id}/row");

    [Fact]
    public async Task Row_201_AnOpenedEntryReadsItselfByItsId()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase(450m) with { Submit = true });
        await ApproveAsync(ledger, entry.Id);

        var approved = (await (await RowAsync(ledger.Client, ledger, entry.Id)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryRowView>(Json))!;

        Assert.Equal((entry.Id, EntryStatus.Approved, 450m), (approved.Id, approved.Status, approved.Amount));
    }

    [Fact]
    public async Task Row_201_SomeoneElsesDraftAndAnEntryThatDoesNotExistAreNotFound()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (author, _) = await ledger.MemberAsync("Finance");
        var draft = ledger.Purchase(200m);
        (await ledger.PostAsync(draft, author)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.OK, (await RowAsync(author, ledger, draft.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await RowAsync(ledger.Client, ledger, draft.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await RowAsync(ledger.Client, ledger, Guid.CreateVersion7())).StatusCode);
    }

    [Fact]
    public async Task NotesLongerThanTheLimitAreRefusedAndNotesOfTheLimitAreKept()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase());
        var path = $"/projects/{ledger.ProjectId}/entries/{entry.Id}/notes";

        var refused = await ledger.Client.PutAsJsonAsync(path, new ChangeNotesRequest(new string('ن', Entry.NotesMaxLength + 1)), Json);
        var kept = await ledger.Client.PutAsJsonAsync(path, new ChangeNotesRequest(new string('ن', Entry.NotesMaxLength)), Json);

        await AssertRefused(refused, HttpStatusCode.BadRequest, "invalidNotes");
        Assert.Equal(HttpStatusCode.OK, kept.StatusCode);
    }

    [Fact]
    public async Task AnAuthorChangesTheirDraftAndItHoldsTheNewValues()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(1150m);
        await ledger.RecordAsync(request);

        (await EditAsync(ledger.Client, ledger, request with { Amount = 2300m, Notes = "بعد التصحيح" })).EnsureSuccessStatusCode();

        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal(2300m, entry.Amount);
        Assert.Equal("بعد التصحيح", entry.Notes);
        Assert.Equal(EntryStatus.Draft, entry.Status);
    }

    [Fact]
    public async Task AChangeKeepsWhoEnteredItAndWhen()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request, _) = await FieldDraftAsync(ledger);
        (await field.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/submit", null)).EnsureSuccessStatusCode();
        var before = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        factory.Time.Advance(TimeSpan.FromMinutes(1));

        (await EditAsync(ledger.Client, ledger, request with { Notes = "من المالي", AccountId = ledger.AccountId })).EnsureSuccessStatusCode();

        var after = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal((before.CreatedByUserId, before.CreatedAt), (after.CreatedByUserId, after.CreatedAt));
        Assert.Equal(EntryStatus.PendingApproval, after.Status);
    }

    [Fact]
    public async Task EveryChangeIsInTheHistoryWithTheValuesBeforeAndAfter()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(1150m);
        await ledger.RecordAsync(request);

        (await EditAsync(ledger.Client, ledger, request with { Amount = 2300m })).EnsureSuccessStatusCode();

        var update = (await factory.HistoryOfAsync(request.Id)).Last(record => record.Action == HistoryAction.Update && record.TableName == "Entries");
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
        var request = ledger.Purchase(200m) with { Submit = true };
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

    [Theory]
    [InlineData(EntryStatus.Draft)]
    [InlineData(EntryStatus.PendingApproval)]
    [InlineData(EntryStatus.Rejected)]
    public async Task ABillNotApprovedIsRemoved(EntryStatus status)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = status != EntryStatus.Draft };
        await ledger.RecordAsync(request);
        if (status == EntryStatus.Rejected)
        {
            (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/reject", new RejectEntryRequest("المبلغ خطأ"), Json)).EnsureSuccessStatusCode();
        }
        Assert.Equal(status, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id))).Status);

        (await DeleteAsync(ledger.Client, ledger, request.Id)).EnsureSuccessStatusCode();

        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(row => row.Id == request.Id)));
    }

    [Fact]
    public async Task DeletingIsItsOwnPermission()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request, fieldId) = await FieldDraftAsync(ledger);
        await factory.WithDatabaseAsync(db => db.ProjectPermissions.Where(row => row.UserId == fieldId && row.Permission == Permission.DeleteEntries).ExecuteDeleteAsync());

        (await EditAsync(field, ledger, request with { Notes = "تصحيح" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteAsync(field, ledger, request.Id)).StatusCode);

        await factory.GiveAsync(fieldId, ledger.ProjectId, [Permission.DeleteEntries]);
        (await DeleteAsync(field, ledger, request.Id)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TheFieldCannotChangeWhatWaitsForADecisionOrSomeoneElsesEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request, _) = await FieldDraftAsync(ledger);
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
        var (field, request, _) = await FieldDraftAsync(ledger);
        (await field.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/submit", null)).EnsureSuccessStatusCode();

        (await EditAsync(ledger.Client, ledger, request with { Notes = "صححها المالي", AccountId = ledger.AccountId })).EnsureSuccessStatusCode();

        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal(("صححها المالي", EntryStatus.PendingApproval), (entry.Notes, entry.Status));
    }

    [Fact]
    public async Task BR_42_ADraftIsItsAuthorsAloneUntilItIsSent()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request, _) = await FieldDraftAsync(ledger);
        var path = $"/projects/{ledger.ProjectId}/entries/{request.Id}";
        var corrected = request with { Notes = "صححها المالي", AccountId = ledger.AccountId };

        var listed = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;
        Assert.DoesNotContain(listed, row => row.Id == request.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.GetAsync(path)).StatusCode);
        await AssertRefused(await ledger.Client.PostAsync($"{path}/submit", null), HttpStatusCode.Forbidden, "notYourEntry");
        await AssertRefused(await EditAsync(ledger.Client, ledger, corrected), HttpStatusCode.Forbidden, "notYourEntry");
        await AssertRefused(await ledger.Client.PutAsJsonAsync($"{path}/notes", new ChangeNotesRequest("ملاحظة المالي"), Json), HttpStatusCode.Forbidden, "notYourEntry");
        await AssertRefused(await DeleteAsync(ledger.Client, ledger, request.Id), HttpStatusCode.Forbidden, "notYourEntry");
        await AssertRefused(await ledger.Client.PostAsJsonAsync($"{path}/invoice", new AttachInvoiceRequest(await ledger.FileAsync()), Json), HttpStatusCode.Forbidden, "notYourEntry");
        (await field.GetAsync(path)).EnsureSuccessStatusCode();

        (await field.PostAsync($"{path}/submit", null)).EnsureSuccessStatusCode();

        listed = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;
        Assert.Contains(listed, row => row.Id == request.Id);
        (await ledger.Client.GetAsync(path)).EnsureSuccessStatusCode();
        (await EditAsync(ledger.Client, ledger, corrected)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task App_105_R3_TheListGivesOnlyTheNewestAsManyAsAskedAndEveryOneWhenNotAsked()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        foreach (var amount in new[] { 100m, 200m, 300m })
        {
            await ledger.RecordAsync(await ledger.RevenueAsync(ItemType.OtherRevenue, amount));
        }
        var path = $"/projects/{ledger.ProjectId}/entries";

        var every = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>(path, Json))!;
        var newest = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"{path}?take=2", Json))!;

        Assert.True(every.Count >= 3);
        Assert.Equal(every.Take(2).Select(row => row.Id), newest.Select(row => row.Id));
    }

    [Fact]
    public async Task AC_16_2_AnOfflineEditOfAnApprovedEntryIsRefusedAndTheServersVersionStays()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(500m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);

        var refused = await EditAsync(ledger.Client, ledger, request with { Amount = 9m });

        await AssertRefused(refused, HttpStatusCode.Conflict, "entryApproved");
        Assert.Equal(500m, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == request.Id).Select(row => row.Amount).SingleAsync()));
    }

    [Fact]
    public async Task AC_50_2_AnApprovedEntrysNotesMayChangeAndDeletingIsNotEnoughToRemoveIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ApproveAsync(ledger, request.Id);

        (await ledger.Client.PutAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/notes", new ChangeNotesRequest("ملاحظة بعد الاعتماد"), Json)).EnsureSuccessStatusCode();

        Assert.Equal("ملاحظة بعد الاعتماد", await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == request.Id).Select(row => row.Notes).SingleAsync()));
        Assert.Equal(HttpStatusCode.Forbidden, (await DeleteAsync(ledger.Client, ledger, request.Id)).StatusCode);
    }

    [Fact]
    public async Task ADeletedEntryLeavesNothingButTheHistory()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(request);

        (await DeleteAsync(ledger.Client, ledger, request.Id)).EnsureSuccessStatusCode();

        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(row => row.Id == request.Id)));
        Assert.False(await ledger.ReadAsync(db => db.ExpenseDetails.AnyAsync(row => row.EntryId == request.Id)));
        Assert.Contains(await factory.HistoryOfAsync(request.Id), record => record.Action == HistoryAction.Delete);
    }

    [Fact]
    public async Task UC_083_AnEntryOnTheListCarriesItsNotesAccountAndPaymentMethodForItsSharedText()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var sent = ledger.Purchase(300m) with { Submit = true, Notes = "دفعة أولى" };
        await ledger.RecordAsync(sent);
        var methods = (await ledger.Client.GetFromJsonAsync<List<Settings.PaymentMethodView>>($"/projects/{ledger.ProjectId}/payment-methods", Json))!;

        var row = Assert.Single((await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!);

        Assert.Equal(("دفعة أولى", "حساب المؤسسة", methods.Single(method => method.Id == ledger.MethodId).Name), (row.Notes, row.AccountName, row.PaymentMethodName));
    }

    [Fact]
    public async Task EveryoneInTheProjectSeesItsEntriesWithTheirStatusAndOnlyTheirOwnDrafts()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var draft = ledger.Purchase(200m);
        await ledger.RecordAsync(draft);
        var sent = ledger.Purchase(300m) with { Submit = true };
        await ledger.RecordAsync(sent);
        var (reader, _) = await ledger.MemberAsync("Reader");

        var seen = (await reader.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;
        var own = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries", Json))!;

        var row = Assert.Single(seen);
        Assert.Equal((sent.Id, EntryStatus.PendingApproval, 300m, false), (row.Id, row.Status, row.Amount, row.IsMine));
        Assert.Equal("شركة النخيل", row.SupplierName);
        Assert.Equal(2, own.Count);
        Assert.All(own, entry => Assert.True(entry.IsMine));
    }
}
