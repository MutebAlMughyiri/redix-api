using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Contracts;
using Redix.Api.History;
using Redix.Api.Entries;
using Redix.Api.Notifications;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>
/// UC-039, redix-app #115: a file shared to REDIX goes on any entry its person may handle, of any kind, an approved one too, beside the files it has, and nothing else about the entry
/// changes (redix-api #96, D15).
/// </summary>
[Collection(DatabaseCollection.Name)]
public class InvoiceAttachTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    /// <summary>What the share screen lists: the entries <paramref name="client"/> may put a file on (D6).</summary>
    private static async Task<List<Guid>> AttachableAsync(HttpClient client, Ledger ledger) =>
        [.. (await client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries?attachable=true", Json))!.Select(row => row.Id)];

    /// <summary>What the share screen pins over the list: the entries a QR code matches (AC-49.2).</summary>
    private static async Task<List<EntryRowView>> MatchesAsync(HttpClient client, Ledger ledger, string vatNumber, decimal total, DateOnly date) =>
        (await client.GetFromJsonAsync<List<EntryRowView>>($"/projects/{ledger.ProjectId}/entries/attachable?vatNumber={vatNumber}&total={total}&date={date:yyyy-MM-dd}", Json))!;

    private static AttachInvoiceRequest Invoice(Guid attachmentId) => new(attachmentId);

    private static Task<string> SupplierVatNumberAsync(Ledger ledger) =>
        ledger.ReadAsync(db => db.Suppliers.Where(row => row.Id == ledger.SupplierId).Select(row => row.VatNumber).SingleAsync());

    private static Task<HttpResponseMessage> AttachAsync(HttpClient client, Ledger ledger, Guid entryId, AttachInvoiceRequest request) =>
        client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/invoice", request, Json);

    private static Task<List<EntryAttachment>> FilesOfAsync(Ledger ledger, Guid entryId) => ledger.ReadAsync(db => db.EntryAttachments.Where(row => row.EntryId == entryId).ToListAsync());

    /// <summary>The files the entry's page lists, as the entry's own route answers them (AC-46.10g).</summary>
    private static async Task<List<EntryAttachmentView>> PageFilesAsync(Ledger ledger, Guid entryId) =>
        (await ledger.Client.GetFromJsonAsync<EntryView>($"/projects/{ledger.ProjectId}/entries/{entryId}", Json))!.Attachments;

    private static async Task<(HttpClient Client, EntryRequest Request)> FieldRecordsAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("Field");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        var request = ledger.Purchase(200m) with { AccountId = account.Id, Submit = true };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", request, Json)).EnsureSuccessStatusCode();
        return (field, request);
    }

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name), Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    /// <summary>A payment of 3,000 on a new contract of the ledger's supplier, in the first item that takes contractor payments.</summary>
    private static async Task<EntryRequest> ContractorPaymentAsync(Ledger ledger)
    {
        var item = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories/offered?itemType={ItemType.ContractorPayments}", Json))!.SelectMany(category => category.Items).First();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, item.Id, 34000m, false), Json)).EnsureSuccessStatusCode();
        var contract = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Json))!.Last();
        return new EntryRequest(Guid.CreateVersion7()) { ItemId = item.Id, TableId = contract.Id, Date = ledger.Today, Amount = 3000m, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId, Submit = true };
    }

    [Fact]
    public async Task AC_46_10c_D5_EveryKindThatPaysSomethingIsListedWithOrWithoutAFile()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (purchase, withFile, payment) = (ledger.Purchase(200m) with { Submit = true }, ledger.Purchase() with { AttachmentId = await ledger.FileAsync() }, await ContractorPaymentAsync(ledger));
        var (revenue, money) = (await ledger.RevenueAsync(ItemType.OtherRevenue, 70), await ledger.PartnerEntryAsync(await PartnerAsync(ledger, "هيثم"), 500));
        foreach (var request in new[] { purchase, withFile, payment, revenue, money })
        {
            await ledger.RecordAsync(request);
        }

        Assert.Equal(new[] { purchase.Id, withFile.Id, payment.Id, revenue.Id, money.Id }.Order(), (await AttachableAsync(ledger.Client, ledger)).Order());
    }

    [Fact]
    public async Task AC_46_10c_ADeductionACreditNoteAndARemovedEntryAreNotListedAndCannotTakeOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (deduction, creditNote, removed) = (ledger.Deduction(-50m), ledger.CreditNote(30m), ledger.Purchase(200m) with { Submit = true });
        foreach (var request in new[] { deduction, creditNote, removed })
        {
            await ledger.RecordAsync(request);
        }
        await ledger.SetStatusAsync(removed.Id, EntryStatus.Removed);

        Assert.Empty(await AttachableAsync(ledger.Client, ledger));
        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, deduction.Id, Invoice(await ledger.FileAsync()))));
        Assert.Equal("entryNotAttachable", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, creditNote.Id, Invoice(await ledger.FileAsync()))));
        Assert.Equal(HttpStatusCode.NotFound, (await AttachAsync(ledger.Client, ledger, removed.Id, Invoice(await ledger.FileAsync()))).StatusCode);
    }

    [Fact]
    public async Task AC_46_10f_FieldListsAndAttachesToTheirOwnAlone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var finances = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(finances);
        var (field, own) = await FieldRecordsAsync(ledger);

        Assert.Equal([own.Id], await AttachableAsync(field, ledger));
        Assert.Equal(HttpStatusCode.OK, (await AttachAsync(field, ledger, own.Id, Invoice(await ledger.FileAsync()))).StatusCode);
        var refused = await AttachAsync(field, ledger, finances.Id, Invoice(await ledger.FileAsync()));
        Assert.Equal((HttpStatusCode.Forbidden, "notYourEntry"), (refused.StatusCode, await Ledger.CodeOf(refused)));
    }

    [Fact]
    public async Task AC_46_10f_WhoeverManagesAnyEntryListsAndAttachesToOthersButNeverToTheirDrafts()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, sent) = await FieldRecordsAsync(ledger);
        var draft = sent with { Id = Guid.CreateVersion7(), Submit = false };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", draft, Json)).EnsureSuccessStatusCode();

        Assert.Equal([sent.Id], await AttachableAsync(ledger.Client, ledger));
        Assert.Contains(draft.Id, await AttachableAsync(field, ledger));
        Assert.Equal(HttpStatusCode.OK, (await AttachAsync(ledger.Client, ledger, sent.Id, Invoice(await ledger.FileAsync()))).StatusCode);
        var refused = await AttachAsync(ledger.Client, ledger, draft.Id, Invoice(await ledger.FileAsync()));
        Assert.Equal((HttpStatusCode.Forbidden, "notYourEntry"), (refused.StatusCode, await Ledger.CodeOf(refused)));
    }

    [Fact]
    public async Task AC_49_2_D6_TheQrCodesMatchAloneIsAnsweredForThePinOverTheList()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await factory.WithDatabaseAsync(db => db.Projects.Where(row => row.Id == ledger.ProjectId).ExecuteUpdateAsync(row => row.SetProperty(project => project.StartDate, ledger.Today.AddMonths(-1))));
        var vatNumber = await SupplierVatNumberAsync(ledger);
        var matching = ledger.Purchase(200m) with { Submit = true, Amount = 6000, Date = ledger.Today.AddDays(-3) };
        var far = ledger.Purchase(200m) with { Submit = true, Amount = 6002, Date = ledger.Today };
        var late = ledger.Purchase(200m) with { Submit = true, Amount = 6000, Date = ledger.Today.AddDays(-4) };
        foreach (var request in new[] { matching, far, late })
        {
            await ledger.RecordAsync(request);
        }

        var matches = await MatchesAsync(ledger.Client, ledger, vatNumber, 6000.5m, ledger.Today);

        Assert.Equal((matching.Id, vatNumber), (Assert.Single(matches).Id, matches[0].SupplierVatNumber));
        Assert.Equal(1, matches[0].Number);
        Assert.Empty(await MatchesAsync(ledger.Client, ledger, "", 6000.5m, ledger.Today));
        Assert.Empty(await MatchesAsync(ledger.Client, ledger, MigratedApiFactory.NewVatNumber(), 6000.5m, ledger.Today));
    }

    [Fact]
    public async Task AC_46_10a_R10_AFileGoesOnAnApprovedEntryItsFiguresStayAndAHistoryLineIsWritten()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(115m) with { Submit = true };
        await ledger.RecordAsync(request);
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        var file = await ledger.FileAsync();
        var trailBefore = (await factory.HistoryOfAsync(request.Id)).Count;

        var answer = await AttachAsync(ledger.Client, ledger, request.Id, Invoice(file));

        Assert.Equal(EntryStatus.Approved, (await answer.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Status);
        var link = Assert.Single(await FilesOfAsync(ledger, request.Id));
        Assert.Equal((file, EntryAttachmentRole.Invoice), (link.AttachmentId, link.Role));
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == request.Id)));
        var kept = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal((115m, EntryStatus.Approved), (kept.Amount, kept.Status));
        Assert.Equal(HistoryAction.Create, Assert.Single((await factory.HistoryOfAsync(request.Id)).Skip(trailBefore)).Action);
    }

    [Fact]
    public async Task AC_46_10e_R5_AnEntryWithAFileIsListedAndKeepsBothFilesAfterTheAttach()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await ledger.FileAsync();
        var request = ledger.Purchase() with { AttachmentId = first, Submit = true };
        await ledger.RecordAsync(request);
        var second = await ledger.FileAsync();

        Assert.Contains(request.Id, await AttachableAsync(ledger.Client, ledger));
        (await AttachAsync(ledger.Client, ledger, request.Id, Invoice(second))).EnsureSuccessStatusCode();

        Assert.Equal(new[] { first, second }.Order(), (await FilesOfAsync(ledger, request.Id)).Select(link => link.AttachmentId).Order());
    }

    [Fact]
    public async Task AC_46_10g_R9_TheEntrysPageCarriesEveryFileItHoldsTheOldestFirstEachWithItsRoleNameTypeAndDate()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = await ledger.FileAsync();
        var request = ledger.Purchase() with { AttachmentId = first, Submit = true };
        await ledger.RecordAsync(request);
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var second = await ledger.FileAsync();
        (await AttachAsync(ledger.Client, ledger, request.Id, Invoice(second))).EnsureSuccessStatusCode();

        var files = await PageFilesAsync(ledger, request.Id);

        Assert.Equal([first, second], files.Select(file => file.AttachmentId));
        Assert.All(files, file => Assert.Equal((EntryAttachmentRole.Invoice, "invoice.jpg", "image/jpeg"), (file.Role, file.FileName, file.ContentType)));
        Assert.Equal(TimeSpan.FromMinutes(1), files[1].UploadedAt - files[0].UploadedAt);
    }

    [Fact]
    public async Task R5_AnEditKeepsTheFileSharedOntoTheEntryAndAFileChosenInTheFormReplacesOnlyItsOwn()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var own = await ledger.FileAsync();
        var request = ledger.Purchase() with { AttachmentId = own };
        await ledger.RecordAsync(request);
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var shared = await ledger.FileAsync();
        (await AttachAsync(ledger.Client, ledger, request.Id, Invoice(shared))).EnsureSuccessStatusCode();

        (await ledger.ChangeAsync(request with { Description = "بعد المشاركة" })).EnsureSuccessStatusCode();
        var afterEdit = (await FilesOfAsync(ledger, request.Id)).Select(link => link.AttachmentId).Order().ToList();
        factory.Time.Advance(TimeSpan.FromMinutes(1));
        var chosen = await ledger.FileAsync();
        (await ledger.ChangeAsync(request with { AttachmentId = chosen })).EnsureSuccessStatusCode();

        Assert.Equal(new[] { own, shared }.Order(), afterEdit);
        Assert.Equal(new[] { shared, chosen }.Order(), (await FilesOfAsync(ledger, request.Id)).Select(link => link.AttachmentId).Order());
    }

    [Fact]
    public async Task AC_46_10g_AnEntryWithNoFileCarriesNoneAndNoOtherEntrysFile()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var bare = ledger.Purchase(200m) with { Submit = true };
        var other = ledger.Purchase(300m) with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(bare);
        await ledger.RecordAsync(other);

        Assert.Empty(await PageFilesAsync(ledger, bare.Id));
        Assert.Single(await PageFilesAsync(ledger, other.Id));
    }

    [Fact]
    public async Task D5_EachFileTakesItsTablesRole()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (payment, revenue, money) = (await ContractorPaymentAsync(ledger), await ledger.RevenueAsync(ItemType.OtherRevenue, 70), await ledger.PartnerEntryAsync(await PartnerAsync(ledger, "هيثم"), 500));
        foreach (var request in new[] { payment, revenue, money })
        {
            await ledger.RecordAsync(request);
            (await AttachAsync(ledger.Client, ledger, request.Id, Invoice(await ledger.FileAsync()))).EnsureSuccessStatusCode();
        }

        var roles = await Task.WhenAll(new[] { payment, revenue, money }.Select(async request => Assert.Single(await FilesOfAsync(ledger, request.Id)).Role));

        Assert.Equal([EntryAttachmentRole.Receipt, EntryAttachmentRole.Other, EntryAttachmentRole.BankReceipt], roles);
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
        Assert.Equal(toldBefore, (await other.GetFromJsonAsync<List<NotificationView>>("/notifications", Json))!.Count);
    }

    [Fact]
    public async Task G1_BR_22_AFileAlreadyOnAnEntryIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var first = ledger.Purchase(200m) with { Submit = true };
        var second = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(first);
        await ledger.RecordAsync(second);
        var file = await ledger.FileAsync();
        await AttachAsync(ledger.Client, ledger, first.Id, Invoice(file));

        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, second.Id, Invoice(file))));
        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(await AttachAsync(ledger.Client, ledger, first.Id, Invoice(file))));
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
