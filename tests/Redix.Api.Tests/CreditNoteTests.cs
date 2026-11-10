using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>A credit note is a total coming back, linked to the bill it returns and never more than what is left of it; a deduction is a negative total that says why in its notes (redix-api #96, D13, D14). No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class CreditNoteTests(MigratedApiFactory factory)
{
    private Task<Ledger> LedgerAsync() => Ledger.WithFinanceAsync(factory);

    private static EntryRequest CreditNote(Ledger ledger, decimal amount, Guid originalEntryId) => ledger.Purchase(amount) with { IsCreditNote = true, OriginalEntryId = originalEntryId };

    /// <summary>A bill of 1,150, sent.</summary>
    private static async Task<Guid> BillAsync(Ledger ledger)
    {
        var request = ledger.Purchase(1150m) with { Submit = true };
        await ledger.RecordAsync(request);
        return request.Id;
    }

    private static async Task<List<CreditableInvoiceView>> CreditableAsync(Ledger ledger, HttpClient? client = null) =>
        (await (client ?? ledger.Client).GetFromJsonAsync<List<CreditableInvoiceView>>($"/projects/{ledger.ProjectId}/expenses/creditable?supplierId={ledger.SupplierId}", Ledger.Json))!;

    /// <summary>The item's total as the design defines it (BR-02): what entries of the item amount to, by Entries.Amount, here whatever their status.</summary>
    private static Task<decimal> ItemTotalAsync(Ledger ledger) => ledger.ReadAsync(db => (
        from details in db.ExpenseDetails
        join entry in db.Entries on details.EntryId equals entry.Id
        where entry.ProjectId == ledger.ProjectId && details.ItemId == ledger.ItemId
        select entry.Amount).SumAsync());

    [Fact]
    public async Task AC_14_2_ACreditNoteIsANegativeTotalLinkedToItsBillAndTakesFromTheItem()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);
        var note = CreditNote(ledger, 320m, bill) with { Submit = true };

        var view = await ledger.RecordAsync(note);

        Assert.Equal(-320m, view.Amount);
        Assert.Equal(830m, await ItemTotalAsync(ledger));
        var link = await ledger.ReadAsync(db => db.EntryLinks.SingleAsync(row => row.EntryId == note.Id));
        Assert.Equal((EntryLinkType.CreditNoteOf, bill), (link.Type, link.LinkedEntryId));
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == note.Id)));
    }

    [Fact]
    public async Task D13_ACreditNoteNeedsItsBillAndOnlyACreditNoteNamesOne()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);

        await ledger.AssertRefusedAsync(ledger.Purchase(10m) with { IsCreditNote = true }, "creditNoteNeedsOriginal");
        await ledger.AssertRefusedAsync(ledger.Purchase(10m) with { OriginalEntryId = bill }, "unknownOriginalInvoice");
    }

    [Fact]
    public async Task D13_ACreditNoteCannotGoAboveItsBill()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);

        await ledger.AssertRefusedAsync(CreditNote(ledger, 1150.01m, bill), "creditNoteAboveOriginal");
        Assert.Equal(-1150m, (await ledger.RecordAsync(CreditNote(ledger, 1150m, bill))).Amount);
    }

    [Fact]
    public async Task D13_TheCreditNotesOfOneBillTogetherCannotGoAboveIt()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);
        await ledger.RecordAsync(CreditNote(ledger, 1000m, bill));

        await ledger.AssertRefusedAsync(CreditNote(ledger, 200m, bill), "creditNoteAboveOriginal");
        await ledger.RecordAsync(CreditNote(ledger, 150m, bill));
        Assert.Equal(0m, Assert.Single(await CreditableAsync(ledger)).Returnable);
    }

    [Fact]
    public async Task ARejectedCreditNoteNoLongerTakesFromItsBillAndADraftOneDoes()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);
        var note = CreditNote(ledger, 1150m, bill);

        await ledger.RecordAsync(note);
        Assert.Equal(0m, (await CreditableAsync(ledger)).Single().Returnable);

        await ledger.SetStatusAsync(note.Id, EntryStatus.Rejected);
        Assert.Equal(1150m, (await CreditableAsync(ledger)).Single().Returnable);
    }

    [Fact]
    public async Task OnlyABillOfTheSameSupplierThatWasSentCanBeReturned()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var draft = ledger.Purchase();
        await ledger.RecordAsync(draft);
        var otherBill = await BillAsync(other);
        var rejected = await BillAsync(ledger);
        await ledger.SetStatusAsync(rejected, EntryStatus.Rejected);
        var deduction = ledger.Deduction(-50m) with { Submit = true };
        await ledger.RecordAsync(deduction);

        foreach (var entryId in new[] { draft.Id, otherBill, rejected, deduction.Id, Guid.CreateVersion7() })
        {
            await ledger.AssertRefusedAsync(CreditNote(ledger, 1m, entryId), "unknownOriginalInvoice");
        }
        Assert.Empty(await CreditableAsync(ledger));
    }

    [Fact]
    public async Task ACreditNoteIsTypedAsAPlainAmount()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);

        await ledger.AssertRefusedAsync(CreditNote(ledger, -10m, bill) with { Notes = "مرتجع" }, "invalidAmount");
    }

    [Fact]
    public async Task AC_14_1_ADeductionOf136ReducesTheItemTotalAndNamesNoBill()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(ledger.Purchase(1000m));

        var deduction = ledger.Deduction(-136m, "خصم تأخير التوريد");
        await ledger.RecordAsync(deduction);

        Assert.Equal(864m, await ItemTotalAsync(ledger));
        Assert.Equal("خصم تأخير التوريد", (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == deduction.Id))).Notes);
        Assert.False(await ledger.ReadAsync(db => db.EntryLinks.AnyAsync(row => row.EntryId == deduction.Id)));
    }

    [Fact]
    public async Task FR_21_TheLinkIsInTheHistory()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);
        var note = CreditNote(ledger, 10m, bill);

        await ledger.RecordAsync(note);

        var trail = await factory.HistoryOfAsync(note.Id);
        var link = JsonDocument.Parse(trail.Single(record => record.TableName == "EntryLinks").After).RootElement;
        Assert.Equal(bill, link.GetProperty("LinkedEntryId").GetGuid());
    }

    [Fact]
    public async Task BR_31_AnyoneWhoReadsTheProjectSeesWhatCanBeReturned()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var (reader, _) = await ledger.MemberAsync("Reader");
        var bill = await BillAsync(ledger);

        var row = Assert.Single(await CreditableAsync(ledger, reader));
        Assert.Equal((bill, 1150m, ledger.Today, ""), (row.EntryId, row.Returnable, row.Date, row.Number));
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.GetAsync($"/projects/{ledger.ProjectId}/expenses/creditable?supplierId={ledger.SupplierId}")).StatusCode);
    }

    [Fact]
    public async Task AFinishedProjectRecordsNoCreditNote()
    {
        var ledger = await LedgerAsync();
        var bill = await BillAsync(ledger);
        await ledger.SetStatusAsync(bill, EntryStatus.Approved);
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null)).EnsureSuccessStatusCode();

        var response = await ledger.PostAsync(CreditNote(ledger, 10m, bill));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
