using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class CreditNoteTests(MigratedApiFactory factory)
{
    private Task<Ledger> LedgerAsync() => Ledger.WithFinanceAsync(factory);

    private static ExpenseRequest CreditNote(Ledger ledger, decimal amount, string number = "") => ledger.Purchase(amount, number) with { IsCreditNote = true };

    /// <summary>A sent invoice of 1,000 before VAT, less 100 discount: 900 after it, 135 VAT, 1,035 in all.</summary>
    private static async Task<Guid> DiscountedInvoiceAsync(Ledger ledger)
    {
        var request = ledger.Purchase(1000m) with { AmountIncludesVat = false, Discount = 100m, AttachmentId = await ledger.FileAsync(), Submit = true };
        var view = await ledger.RecordAsync(request);
        Assert.Equal(1035m, view.Total);
        return request.Id;
    }

    private static Task<Invoice> InvoiceOfAsync(Ledger ledger, Guid entryId) => ledger.ReadAsync(db => db.Invoices.SingleAsync(row => row.EntryId == entryId));

    private static async Task<List<CreditableInvoiceView>> CreditableAsync(Ledger ledger, HttpClient? client = null) =>
        (await (client ?? ledger.Client).GetFromJsonAsync<List<CreditableInvoiceView>>($"/projects/{ledger.ProjectId}/expenses/creditable?supplierId={ledger.SupplierId}", Ledger.Json))!;

    /// <summary>The item's total as the design defines it (BR-02): what entries of the item amount to, by Entries.Amount, here whatever their status.</summary>
    private static Task<decimal> ItemTotalAsync(Ledger ledger) => ledger.ReadAsync(db => (
        from details in db.ExpenseDetails
        join entry in db.Entries on details.EntryId equals entry.Id
        where entry.ProjectId == ledger.ProjectId && details.ItemId == ledger.ItemId
        select entry.Amount).SumAsync());

    private sealed record VatPurchases(decimal After, decimal Vat, decimal Total);

    /// <summary>The purchases of a VAT statement added up over the quarter (UC-056), with no regard to what each invoice is.</summary>
    private static Task<VatPurchases> VatPurchasesAsync(Ledger ledger) => ledger.ReadAsync(async db =>
    {
        var invoices = await (from invoice in db.Invoices join entry in db.Entries on invoice.EntryId equals entry.Id where entry.ProjectId == ledger.ProjectId select invoice).ToListAsync();
        return new VatPurchases(invoices.Sum(row => row.AmountAfterDiscount), invoices.Sum(row => row.Vat), invoices.Sum(row => row.Total));
    });

    [Fact]
    public void BR_18_EveryFigureOfACreditNoteIsNegativeAndStillAddsUp()
    {
        var figures = InvoiceFigures.Of(230m, includesVat: true, discount: 20m, vatRate: 0.15m).Negated();

        Assert.Equal(-200m, figures.AmountBeforeDiscount);
        Assert.Equal(-20m, figures.Discount);
        Assert.Equal(-180m, figures.AmountAfterDiscount);
        Assert.Equal(-27m, figures.Vat);
        Assert.Equal(-207m, figures.Total);
        Assert.Equal(figures.AmountAfterDiscount, figures.AmountBeforeDiscount - figures.Discount);
        Assert.Equal(figures.Total, figures.AmountAfterDiscount + figures.Vat);
    }

    [Theory]
    [InlineData(900, 135, 0.15)]
    [InlineData(5217.3913, 782.6087, 0.15)]
    [InlineData(1000, 50, 0.05)]
    [InlineData(1000, 0, 0)]
    [InlineData(0, 0, 0)]
    public void TheRateOfAnInvoiceComesBackFromItsKeptFigures(double after, double vat, double rate)
    {
        Assert.Equal((decimal)rate, InvoiceFigures.RateOf((decimal)after, (decimal)vat));
    }

    [Fact]
    public async Task AC_14_2_ACreditNoteOf320WithItsOwnNumberAndAnEarlierDateIsSavedAndShowsNegativeInTheVatStatement()
    {
        var ledger = await LedgerAsync();
        var original = ledger.Purchase(1150m, "F-100") with { AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(original);
        var earlier = ledger.Today.AddDays(-1);
        await factory.WithDatabaseAsync(db => db.Projects.Where(project => project.Id == ledger.ProjectId).ExecuteUpdateAsync(project => project.SetProperty(row => row.StartDate, earlier.AddDays(-30))));
        var note = CreditNote(ledger, 320m, "CN-7") with { InvoiceDate = earlier, OriginalEntryId = original.Id, AttachmentId = await ledger.FileAsync(), Submit = true };

        var view = await ledger.RecordAsync(note);

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
        Assert.Equal(-320m, view.Total);
        Assert.Equal(-320m, view.Amount);
        var invoice = await InvoiceOfAsync(ledger, note.Id);
        Assert.True(invoice.IsCreditNote);
        Assert.Equal("CN-7", invoice.Number);
        Assert.Equal(earlier, invoice.InvoiceDate);
        Assert.Equal(-278.2609m, invoice.AmountAfterDiscount);
        Assert.Equal(-41.7391m, invoice.Vat);
        Assert.Equal(-320m, invoice.Total);
        var link = await ledger.ReadAsync(db => db.EntryLinks.SingleAsync(row => row.EntryId == note.Id));
        Assert.Equal(EntryLinkType.CreditNoteOf, link.Type);
        Assert.Equal(original.Id, link.LinkedEntryId);
        var purchases = await VatPurchasesAsync(ledger);
        Assert.Equal(830m, purchases.Total);
    }

    [Fact]
    public async Task BR_18_ACreditNoteNeedNotBeLinked()
    {
        var ledger = await LedgerAsync();
        var note = CreditNote(ledger, 115m);

        var view = await ledger.RecordAsync(note);

        Assert.Equal(-115m, view.Total);
        Assert.False(await ledger.ReadAsync(db => db.EntryLinks.AnyAsync(row => row.EntryId == note.Id)));
    }

    [Fact]
    public async Task BR_18_AWholeCreditNoteOnAPartlyDiscountedInvoiceTakesAwayExactlyWhatItCounted()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);

        await ledger.RecordAsync(CreditNote(ledger, 1035m) with { OriginalEntryId = original });

        var purchases = await VatPurchasesAsync(ledger);
        Assert.Equal(0m, purchases.After);
        Assert.Equal(0m, purchases.Vat);
        Assert.Equal(0m, purchases.Total);
        Assert.Equal(0m, await ItemTotalAsync(ledger));
    }

    [Fact]
    public async Task BR_18_APartCreditNoteOnAPartlyDiscountedInvoiceLeavesTheRestCounted()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);

        await ledger.RecordAsync(CreditNote(ledger, 345m) with { OriginalEntryId = original });

        var purchases = await VatPurchasesAsync(ledger);
        Assert.Equal(600m, purchases.After);
        Assert.Equal(90m, purchases.Vat);
        Assert.Equal(690m, purchases.Total);
        Assert.Equal(690m, await ItemTotalAsync(ledger));
    }

    [Fact]
    public async Task BR_18_ALinkedCreditNoteCannotGoAboveTheInvoicesValueAfterItsDiscount()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);

        await ledger.AssertRefusedAsync(CreditNote(ledger, 1035.01m) with { OriginalEntryId = original }, "creditNoteAboveOriginal");
        await ledger.AssertRefusedAsync(CreditNote(ledger, 1150m) with { OriginalEntryId = original }, "creditNoteAboveOriginal");
    }

    [Fact]
    public async Task BR_18_TheCreditNotesOfOneInvoiceTogetherCannotGoAboveItsValue()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);

        await ledger.RecordAsync(CreditNote(ledger, 1000m) with { OriginalEntryId = original });
        Assert.Equal(35m, (await CreditableAsync(ledger)).Single().Returnable);

        await ledger.AssertRefusedAsync(CreditNote(ledger, 35.01m) with { OriginalEntryId = original }, "creditNoteAboveOriginal");
        await ledger.RecordAsync(CreditNote(ledger, 35m) with { OriginalEntryId = original });
        Assert.Equal(0m, (await CreditableAsync(ledger)).Single().Returnable);
    }

    [Fact]
    public async Task ARejectedCreditNoteNoLongerTakesFromItsInvoiceAndADraftOneDoes()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);
        var note = CreditNote(ledger, 1035m) with { OriginalEntryId = original };

        await ledger.RecordAsync(note);
        Assert.Equal(0m, (await CreditableAsync(ledger)).Single().Returnable);

        await ledger.SetStatusAsync(note.Id, EntryStatus.Rejected);
        Assert.Equal(1035m, (await CreditableAsync(ledger)).Single().Returnable);
    }

    [Fact]
    public async Task ALinkedCreditNoteFollowsTheRateItsInvoiceWasComputedAtNotTodays()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/vat-rate", new SetVatRateRequest(5m), Ledger.Json)).EnsureSuccessStatusCode();
        var note = CreditNote(ledger, 1035m) with { OriginalEntryId = original };

        var view = await ledger.RecordAsync(note);

        Assert.Equal(-135m, view.Vat);
        Assert.Equal(-900m, view.AmountAfterDiscount);
    }

    [Fact]
    public async Task ACreditNoteOfAnInvoiceWithNoVatHasNoVat()
    {
        var ledger = await LedgerAsync();
        var request = ledger.Purchase(500m) with { IsNotTaxable = true, AttachmentId = await ledger.FileAsync(), Submit = true };
        await ledger.RecordAsync(request);

        var view = await ledger.RecordAsync(CreditNote(ledger, 500m) with { OriginalEntryId = request.Id });

        Assert.Equal(0m, view.Vat);
        Assert.Equal(-500m, view.Total);
    }

    [Fact]
    public async Task OnlyAnInvoiceOfTheSameSupplierThatWasSentCanBeReturned()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var draft = ledger.Purchase();
        await ledger.RecordAsync(draft);
        var otherInvoice = other.Purchase() with { AttachmentId = await other.FileAsync(), Submit = true };
        await other.RecordAsync(otherInvoice);
        var rejected = await DiscountedInvoiceAsync(ledger);
        await ledger.SetStatusAsync(rejected, EntryStatus.Rejected);
        var note = CreditNote(ledger, 10m);
        await ledger.RecordAsync(note);

        foreach (var entryId in new[] { draft.Id, otherInvoice.Id, rejected, note.Id, Guid.CreateVersion7() })
        {
            await ledger.AssertRefusedAsync(CreditNote(ledger, 1m) with { OriginalEntryId = entryId }, "unknownOriginalInvoice");
        }
        await ledger.AssertRefusedAsync(ledger.Purchase() with { OriginalEntryId = draft.Id }, "unknownOriginalInvoice");
        Assert.Empty(await CreditableAsync(ledger));
    }

    [Fact]
    public async Task ACreditNoteHasAnInvoiceAndIsTypedAsAPlainAmount()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { IsCreditNote = true }, "creditNoteNeedsInvoice");
        await ledger.AssertRefusedAsync(CreditNote(ledger, 10m) with { InvoiceNumber = new string('1', Invoice.NumberMaxLength + 1) }, "invalidInvoiceNumber");
        await ledger.AssertRefusedAsync(CreditNote(ledger, -10m), "deductionNeedsReason");
    }

    [Fact]
    public async Task FR_09_ACreditNoteIsSentWithItsFile()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(CreditNote(ledger, 10m) with { Submit = true }, "invoiceRequired");
    }

    [Fact]
    public async Task AC_17_1_ACreditNotesNumberEnteredAgainWarnsAsAnInvoicesDoes()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(CreditNote(ledger, 10m, "CN-9"));

        var response = await ledger.PostAsync(CreditNote(ledger, 20m, "CN-9"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("duplicateInvoice", await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task AC_14_1_ADeductionOf136ReducesTheItemTotalAndHasNoInvoice()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(ledger.WithoutInvoice(1000m));
        var deduction = ledger.WithoutInvoice(-136m, "خصم من المبلط") with { Submit = true };

        var view = await ledger.RecordAsync(deduction);

        Assert.Equal(-136m, view.Amount);
        Assert.Equal(0m, view.Vat);
        Assert.Equal(864m, await ItemTotalAsync(ledger));
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == deduction.Id)));
    }

    [Fact]
    public async Task ADeductionSaysWhyAndGoesOnTheExceptionsListHavingNoInvoice()
    {
        var ledger = await LedgerAsync();
        var deduction = ledger.WithoutInvoice(-136m, "خصم من المبلط") with { Submit = true };
        await ledger.RecordAsync(deduction);

        await ledger.AssertRefusedAsync(ledger.Purchase(-136m), "deductionNeedsReason");
        var exceptions = (await ledger.Client.GetFromJsonAsync<List<ExceptionView>>($"/projects/{ledger.ProjectId}/expenses/exceptions", Ledger.Json))!;
        Assert.Equal(-136m, Assert.Single(exceptions).Amount);
    }

    [Fact]
    public async Task ADeductionHasNoDiscount()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(ledger.WithoutInvoice(-136m) with { Discount = 1m }, "invalidDiscount");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice(-1.234m), "invalidAmount");
    }

    [Fact]
    public async Task FR_21_TheLinkIsInTheAuditTrail()
    {
        var ledger = await LedgerAsync();
        var original = await DiscountedInvoiceAsync(ledger);
        var note = CreditNote(ledger, 10m) with { OriginalEntryId = original };

        await ledger.RecordAsync(note);

        var trail = await factory.AuditTrailOfAsync(note.Id);
        var link = JsonDocument.Parse(trail.Single(record => record.TableName == "EntryLinks").After).RootElement;
        Assert.Equal(original, link.GetProperty("LinkedEntryId").GetGuid());
    }

    [Fact]
    public async Task BR_31_AnyoneWhoReadsTheProjectSeesWhatCanBeReturned()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var (reader, _) = await ledger.MemberAsync("Reader");
        var original = await DiscountedInvoiceAsync(ledger);

        var row = Assert.Single(await CreditableAsync(ledger, reader));
        Assert.Equal(original, row.EntryId);
        Assert.Equal(1035m, row.Returnable);
        Assert.Equal(0.15m, row.VatRate);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.GetAsync($"/projects/{ledger.ProjectId}/expenses/creditable?supplierId={ledger.SupplierId}")).StatusCode);
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
