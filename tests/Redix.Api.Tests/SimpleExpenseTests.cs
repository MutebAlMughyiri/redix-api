using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Attachments;
using Redix.Api.Entries;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>An expense is a total the project spent: on an item, from a supplier, on a day, paid one way from one account, with its file when there is one (redix-api #96). No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class SimpleExpenseTests(MigratedApiFactory factory)
{
    private static async Task<Guid> FileOfAsync(Ledger ledger, byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Ledger.Json))!.Id;
    }

    private static byte[] AnInvoiceFile() => [0xFF, 0xD8, 0xFF, 0xE0, .. Guid.NewGuid().ToByteArray()];

    [Fact]
    public async Task AC_1_AnExpenseIsSentWithItsItemSupplierDateTotalAccountAndMethodAndNoFile()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var expense = ledger.Purchase(1150m) with { Submit = true };

        var response = await ledger.PostAsync(expense);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var view = (await response.Content.ReadFromJsonAsync<ExpenseView>(Ledger.Json))!;
        Assert.Equal((EntryStatus.PendingApproval, 1150m), (view.Status, view.Amount));
        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == expense.Id));
        Assert.Equal((ledger.Today, 1150m), (entry.EntryDate, entry.Amount));
    }

    [Fact]
    public async Task D11_ANewExpenseWritesNoInvoiceRow()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var expense = ledger.Purchase(1150m) with { AttachmentId = await ledger.FileAsync(), Submit = true };

        await ledger.RecordAsync(expense);

        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == expense.Id)));
    }

    [Fact]
    public async Task D11_ADraftWithNoFileIsSentLaterWithNoFile()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var draft = ledger.Purchase(500m);
        await ledger.RecordAsync(draft);

        var sent = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{draft.Id}/submit", null);

        sent.EnsureSuccessStatusCode();
        Assert.Equal(EntryStatus.PendingApproval, (await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == draft.Id))).Status);
    }

    [Fact]
    public async Task D14_ADeductionSaysWhyInItsNotes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        await ledger.AssertRefusedAsync(ledger.Purchase(-136m), "deductionNeedsNotes");
        await ledger.AssertRefusedAsync(ledger.Purchase(-136m) with { Notes = "   " }, "deductionNeedsNotes");
        Assert.Equal(-136m, (await ledger.RecordAsync(ledger.Deduction(-136m))).Amount);
    }

    [Fact]
    public async Task BR_22_TheSameFileOnASecondExpenseIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var attachmentId = await FileOfAsync(ledger, AnInvoiceFile());
        await ledger.RecordAsync(ledger.Purchase() with { AttachmentId = attachmentId });

        var response = await ledger.PostAsync(ledger.Purchase() with { AttachmentId = attachmentId });

        Assert.Equal((HttpStatusCode.Conflict, "invoiceAlreadyAttached"), (response.StatusCode, await Ledger.CodeOf(response)));
    }

    [Fact]
    public async Task BR_22_TheSameFileSentAgainIsTheSameFileAndIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var bytes = AnInvoiceFile();
        await ledger.RecordAsync(ledger.Purchase() with { AttachmentId = await FileOfAsync(ledger, bytes) });

        var response = await ledger.PostAsync(ledger.Purchase() with { AttachmentId = await FileOfAsync(ledger, bytes) });

        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task AFileKeptAndOnNoEntryIsFreeToUse()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await FileOfAsync(ledger, AnInvoiceFile());

        Assert.Equal(HttpStatusCode.Created, (await ledger.PostAsync(ledger.Purchase() with { AttachmentId = await FileOfAsync(ledger, AnInvoiceFile()) })).StatusCode);
    }
}
