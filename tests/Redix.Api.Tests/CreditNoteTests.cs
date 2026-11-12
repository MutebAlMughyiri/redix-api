using System.Net;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>
/// A credit note, a مرتجع, is a plain entry with a minus amount, its own row as in Muteb's statement, naming no bill (redix-api #119, D4); a deduction is a negative total that says why in
/// its notes (redix-api #96, D14). No figure here is real.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class CreditNoteTests(MigratedApiFactory factory)
{
    private Task<Ledger> LedgerAsync() => Ledger.WithFinanceAsync(factory);

    private static EntryRequest CreditNote(Ledger ledger, decimal amount) => ledger.Purchase(amount) with { IsCreditNote = true };

    /// <summary>The item's total as the design defines it (BR-02): what entries of the item amount to, by Entries.Amount, here whatever their status.</summary>
    private static Task<decimal> ItemTotalAsync(Ledger ledger) => ledger.ReadAsync(db => (
        from details in db.ExpenseDetails
        join entry in db.Entries on details.EntryId equals entry.Id
        where entry.ProjectId == ledger.ProjectId && details.ItemId == ledger.ItemId
        select entry.Amount).SumAsync());

    [Fact]
    public async Task AC_14_2_D4_ACreditNoteIsANegativeTotalOfItsOwnThatTakesFromTheItem()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(ledger.Purchase(1150m) with { Submit = true });

        var view = await ledger.RecordAsync(CreditNote(ledger, 320m) with { Submit = true });

        Assert.Equal(-320m, view.Amount);
        Assert.Equal(830m, await ItemTotalAsync(ledger));
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == view.Id)));
    }

    [Fact]
    public async Task D4_ACreditNoteNeedsNoBillAndIsNotHeldToWhatAnyBillCounted()
    {
        var ledger = await LedgerAsync();

        var alone = await ledger.RecordAsync(CreditNote(ledger, 500m));
        await ledger.RecordAsync(ledger.Purchase(100m) with { Submit = true });
        var above = await ledger.RecordAsync(CreditNote(ledger, 100.01m));

        Assert.Equal((-500m, -100.01m), (alone.Amount, above.Amount));
    }

    [Fact]
    public async Task D4_ACreditNoteIsApprovedAloneWhereverItsBillStands()
    {
        var ledger = await LedgerAsync();
        var bill = ledger.Purchase(1150m) with { Submit = true };
        await ledger.RecordAsync(bill);
        var note = CreditNote(ledger, 100m) with { Submit = true, AttachmentId = await ledger.FileAsync() };
        await ledger.RecordAsync(note);

        await ledger.ApproveAsync(note.Id);

        Assert.Equal(EntryStatus.Approved, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == note.Id).Select(row => row.Status).SingleAsync()));
        Assert.Equal(EntryStatus.PendingApproval, await ledger.ReadAsync(db => db.Entries.Where(row => row.Id == bill.Id).Select(row => row.Status).SingleAsync()));
    }

    [Fact]
    public async Task D4_WhatACreditNoteCouldReturnIsNoLongerAsked()
    {
        var ledger = await LedgerAsync();

        var answer = await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/expenses/creditable?supplierId={ledger.SupplierId}");

        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
    }

    [Fact]
    public async Task ACreditNoteIsTypedAsAPlainAmount()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(CreditNote(ledger, -10m) with { Notes = "مرتجع" }, "invalidAmount");
    }

    [Fact]
    public async Task AC_14_1_ADeductionOf136ReducesTheItemTotalAndSaysWhyInItsNotes()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(ledger.Purchase(1000m));

        var deduction = ledger.Deduction(-136m, "خصم تأخير التوريد");
        await ledger.RecordAsync(deduction);

        Assert.Equal(864m, await ItemTotalAsync(ledger));
        Assert.Equal("خصم تأخير التوريد", (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == deduction.Id))).Notes);
    }

    [Fact]
    public async Task AFinishedProjectRecordsNoCreditNote()
    {
        var ledger = await LedgerAsync();
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null)).EnsureSuccessStatusCode();

        var response = await ledger.PostAsync(CreditNote(ledger, 10m));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
