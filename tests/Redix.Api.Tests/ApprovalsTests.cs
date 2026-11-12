using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.History;
using Redix.Api.Entries;
using Redix.Api.Suppliers;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-031 and UC-032: Finance's inbox and its decisions.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class ApprovalsTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<Guid> SentAsync(Ledger ledger, decimal amount = 1150m)
    {
        var request = ledger.Purchase(amount) with { Submit = true };
        await ledger.RecordAsync(request);
        return request.Id;
    }

    private static Task<List<PendingEntryView>?> PendingAsync(HttpClient client, Guid projectId) =>
        client.GetFromJsonAsync<List<PendingEntryView>>($"/projects/{projectId}/entries/pending", Json);

    private static async Task<ApprovalView> ApproveAsync(Ledger ledger, params Guid[] ids)
    {
        var response = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([.. ids]), Json);
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ApprovalView>(Json))!;
    }

    private static Task<HttpResponseMessage> RejectAsync(Ledger ledger, Guid id, string reason) =>
        ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reject", new RejectEntryRequest(reason), Json);

    private static async Task<EntryStatus> StatusOfAsync(Ledger ledger, Guid id) => await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == id).Select(row => row.Status).SingleAsync());

    [Fact]
    public async Task AC_18_1_TheInboxIsOldestFirstWithTheFileWhenThereIsOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var older = await SentAsync(ledger, 100m);
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var file = await ledger.FileAsync();
        var newer = ledger.Purchase() with { Submit = true, AttachmentId = file };
        await ledger.RecordAsync(newer);

        var pending = (await PendingAsync(ledger.Client, ledger.ProjectId))!;

        Assert.Equal([older, newer.Id], pending.Select(row => row.Id));
        Assert.Equal(("", ""), (pending[0].AttachmentId, pending[0].FileName));
        Assert.Equal(file.ToString(), pending[1].AttachmentId);
        Assert.Equal(("image/jpeg", "invoice.jpg"), (pending[1].ContentType, pending[1].FileName));
        Assert.Equal("شركة النخيل", pending[1].SupplierName);
        Assert.Equal(1150m, pending[1].Amount);
    }

    [Fact]
    public async Task ADraftAndWhatIsDecidedAreNotInTheInbox()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.RecordAsync(ledger.Purchase(200m));
        var approved = await SentAsync(ledger);
        await ApproveAsync(ledger, approved);

        Assert.Empty((await PendingAsync(ledger.Client, ledger.ProjectId))!);
    }

    [Theory]
    [InlineData("Field")]
    [InlineData("Reader")]
    [InlineData("Manager")]
    public async Task OnlyWhoeverDecidesEntriesSeesTheInboxOrDecides(string role)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger);
        var (client, _) = await ledger.MemberAsync(role);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{ledger.ProjectId}/entries/pending")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([id]), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{id}/reject", new RejectEntryRequest("لا"), Json)).StatusCode);
    }

    [Fact]
    public async Task AnApprovedEntryCountsAndMarksWhenItWasApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger, 500m);
        var (status, at) = await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == id).Select(row => ValueTuple.Create(row.Status, row.ApprovedAt)).SingleAsync());
        Assert.Equal((EntryStatus.PendingApproval, Entry.NotApproved), (status, at));

        var answer = await ApproveAsync(ledger, id);

        Assert.Equal([id], answer.Approved);
        Assert.Empty(answer.Failed);
        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == id));
        Assert.Equal(EntryStatus.Approved, entry.Status);
        Assert.NotEqual(Entry.NotApproved, entry.ApprovedAt);
    }

    [Fact]
    public async Task AnAccountsBalanceMovesOnlyWithWhatIsApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var approved = await SentAsync(ledger, 300m);
        await SentAsync(ledger, 50m);
        await ledger.RecordAsync(ledger.Purchase(70m));

        await ApproveAsync(ledger, approved);

        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single();
        Assert.Equal(-300m, account.Balance);
    }

    [Fact]
    public async Task AC_19_2_ABulkApprovalNamesEveryFailureAndApprovesTheRest()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await SentAsync(ledger);
        var second = await SentAsync(ledger);
        var draft = ledger.Purchase(200m);
        await ledger.RecordAsync(draft);
        var unknown = Guid.CreateVersion7();

        var answer = await ApproveAsync(ledger, first, draft.Id, unknown, second);

        Assert.Equal([first, second], answer.Approved);
        Assert.Equal([new EntryFailureView(draft.Id, "entryNotPending"), new EntryFailureView(unknown, "unknownEntry")], answer.Failed);
        Assert.Equal(EntryStatus.Draft, await StatusOfAsync(ledger, draft.Id));
    }

    [Fact]
    public async Task AnEntryOfAnotherProjectIsUnknownHere()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(other);

        var answer = await ApproveAsync(ledger, id);

        Assert.Equal("unknownEntry", answer.Failed.Single().Code);
        Assert.Equal(EntryStatus.PendingApproval, await StatusOfAsync(other, id));
    }

    [Fact]
    public async Task AC_19_1_ARejectionNeedsAReasonAndReturnsTheEntryToItsAuthorWithIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger);

        var refused = await RejectAsync(ledger, id, "   ");
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("reasonRequired", await Ledger.CodeOf(refused));
        Assert.Equal(EntryStatus.PendingApproval, await StatusOfAsync(ledger, id));

        (await RejectAsync(ledger, id, "صورة الفاتورة غير واضحة")).EnsureSuccessStatusCode();

        var mine = (await ledger.Client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries/mine", Json))!.Single();
        Assert.Equal((EntryStatus.Rejected, "صورة الفاتورة غير واضحة"), (mine.Status, mine.RejectionReason));
    }

    [Fact]
    public async Task ARejectedEntryIsSentAgainAndCountsOnlyOnceApproved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger);
        (await RejectAsync(ledger, id, "المبلغ لا يطابق")).EnsureSuccessStatusCode();

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{id}/submit", null)).EnsureSuccessStatusCode();
        Assert.Single((await PendingAsync(ledger.Client, ledger.ProjectId))!);

        Assert.Equal([id], (await ApproveAsync(ledger, id)).Approved);
    }

    [Fact]
    public async Task WhatIsAlreadyDecidedCannotBeDecidedAgain()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger);
        await ApproveAsync(ledger, id);

        var refused = await RejectAsync(ledger, id, "متأخر");

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("entryNotPending", await Ledger.CodeOf(refused));
        Assert.Equal("entryNotPending", (await ApproveAsync(ledger, id)).Failed.Single().Code);
    }

    private async Task<(Ledger Ledger, HttpClient Field, Guid SupplierId)> PendingSupplierAsync()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");
        var added = await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/suppliers", new SupplierRequest("مورد من الموقع"), Json);
        var supplier = (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!;
        Assert.Equal(SupplierStatus.PendingApproval, await ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == supplier.Id).Select(row => row.Status).SingleAsync()));
        return (ledger, field, supplier.Id);
    }

    [Fact]
    public async Task AC_44_1_ApprovingTheFirstExpenseOnAPendingSupplierApprovesItToo()
    {
        var (ledger, _, supplierId) = await PendingSupplierAsync();
        var request = ledger.Purchase(200m) with { SupplierId = supplierId, Submit = true };
        await ledger.RecordAsync(request);

        await ApproveAsync(ledger, request.Id);

        Assert.Equal(SupplierStatus.Approved, await ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == supplierId).Select(row => row.Status).SingleAsync()));
    }

    [Fact]
    public async Task AC_44_1_RejectingItLeavesTheSupplierPending()
    {
        var (ledger, _, supplierId) = await PendingSupplierAsync();
        var request = ledger.Purchase(200m) with { SupplierId = supplierId, Submit = true };
        await ledger.RecordAsync(request);

        (await RejectAsync(ledger, request.Id, "البند غير صحيح")).EnsureSuccessStatusCode();

        Assert.Equal(SupplierStatus.PendingApproval, await ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == supplierId).Select(row => row.Status).SingleAsync()));
    }

    [Fact]
    public async Task EachSendingAndDecisionIsRecordedWithWhoAndWhy()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var id = await SentAsync(ledger);
        (await RejectAsync(ledger, id, "المبلغ لا يطابق")).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{id}/submit", null)).EnsureSuccessStatusCode();
        await ApproveAsync(ledger, id);

        var decisions = await ledger.ReadAsync(db => db.EntryDecisions.Where(row => row.EntryId == id).OrderBy(row => row.Id).ToListAsync());

        Assert.Equal([EntryDecisionAction.Submit, EntryDecisionAction.Reject, EntryDecisionAction.Submit, EntryDecisionAction.Approve], decisions.Select(row => row.Action));
        Assert.Equal("المبلغ لا يطابق", decisions[1].Reason);
        var trail = await factory.HistoryOfAsync(id);
        Assert.Contains(trail, record => record.Action == HistoryAction.Approve);
        Assert.Contains(trail, record => record.Action == HistoryAction.Reject);
    }
}
