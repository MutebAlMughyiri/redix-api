using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Entries;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class WithoutInvoiceTests(MigratedApiFactory factory)
{
    private Task<Ledger> LedgerAsync() => Ledger.WithFinanceAsync(factory);

    private static async Task<List<ExceptionView>> ExceptionsAsync(Ledger ledger) =>
        (await ledger.Client.GetFromJsonAsync<List<ExceptionView>>($"/projects/{ledger.ProjectId}/expenses/exceptions", Ledger.Json))!;

    [Fact]
    public async Task AC_09_1_ExpenseWithNoFileAndNoReasonCannotBeSent()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(ledger.Purchase() with { Submit = true }, "invoiceRequired");
        await ledger.AssertRefusedAsync(ledger.Purchase() with { Submit = true, WithoutInvoiceReason = "   " }, "invoiceRequired");
    }

    [Fact]
    public async Task AC_09_1_ExpenseWithAReasonIsSentPendingAndShowsInTheExceptionsList()
    {
        var ledger = await LedgerAsync();
        var request = ledger.WithoutInvoice() with { Submit = true };

        var view = await ledger.RecordAsync(request);

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
        var exception = Assert.Single(await ExceptionsAsync(ledger));
        Assert.Equal(request.Id, exception.EntryId);
        Assert.Equal(EntryStatus.PendingApproval, exception.Status);
        Assert.Equal(200m, exception.Amount);
        Assert.Equal("شراء نقدي من السوق", exception.Reason);
        Assert.False(string.IsNullOrEmpty(exception.ItemName));
    }

    [Fact]
    public async Task OpenQuestion2_ThereIsNoCapOnAnExpenseWithoutAnInvoice()
    {
        var ledger = await LedgerAsync();

        var view = await ledger.RecordAsync(ledger.WithoutInvoice(Money.Largest) with { Submit = true });

        Assert.Equal(Money.Largest, view.Amount);
    }

    [Fact]
    public async Task AnExpenseWithoutAnInvoiceHasNoInvoiceRowAndNoVatSoTheVatStatementNeverSeesIt()
    {
        var ledger = await LedgerAsync();
        var request = ledger.WithoutInvoice(1150m) with { Submit = true };

        var view = await ledger.RecordAsync(request);

        Assert.Equal(0m, view.Vat);
        Assert.Equal(1150m, view.Total);
        Assert.False(await ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == request.Id)));
        var entry = await ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == request.Id));
        Assert.Equal(1150m, entry.Amount);
        Assert.Equal(ledger.Today, entry.EntryDate);
    }

    [Fact]
    public async Task ItsDateIsThePaymentDateWhichIsRequiredAndInThePeriod()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { PaymentDate = default }, "invalidPaymentDate");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { PaymentDate = ledger.Today.AddDays(1) }, "invalidPaymentDate");
    }

    [Fact]
    public async Task AReasonBesideAFileIsRefusedAndAReasonTooLongIsRefused()
    {
        var ledger = await LedgerAsync();

        await ledger.AssertRefusedAsync(ledger.WithoutInvoice() with { AttachmentId = await ledger.FileAsync() }, "invoiceAndReason");
        await ledger.AssertRefusedAsync(ledger.WithoutInvoice(reason: new string('س', Projects.Project.ReasonMaxLength + 1)), "invalidWithoutInvoiceReason");
    }

    [Fact]
    public async Task ADraftIsNotInTheExceptionsListAndARejectedOneLeavesIt()
    {
        var ledger = await LedgerAsync();
        await ledger.RecordAsync(ledger.WithoutInvoice());
        Assert.Empty(await ExceptionsAsync(ledger));

        var sent = ledger.WithoutInvoice() with { Submit = true };
        await ledger.RecordAsync(sent);
        Assert.Single(await ExceptionsAsync(ledger));

        await ledger.SetStatusAsync(sent.Id, EntryStatus.Rejected);
        Assert.Empty(await ExceptionsAsync(ledger));
    }

    [Fact]
    public async Task AnExpenseWithAFileIsNotInTheExceptionsList()
    {
        var ledger = await LedgerAsync();

        await ledger.RecordAsync(ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Submit = true });

        Assert.Empty(await ExceptionsAsync(ledger));
    }

    [Fact]
    public async Task BR_33_TheExceptionsListIsOnlyForWhoReadsTheProject()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var (reader, _) = await ledger.MemberAsync("users.roleReader");
        await ledger.RecordAsync(ledger.WithoutInvoice() with { Submit = true });

        Assert.Single((await reader.GetFromJsonAsync<List<ExceptionView>>($"/projects/{ledger.ProjectId}/expenses/exceptions", Ledger.Json))!);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.GetAsync($"/projects/{ledger.ProjectId}/expenses/exceptions")).StatusCode);
    }

    [Fact]
    public async Task FR_21_TheReasonIsInTheAuditTrail()
    {
        var ledger = await LedgerAsync();
        var request = ledger.WithoutInvoice() with { Submit = true };

        await ledger.RecordAsync(request);

        var records = await factory.AuditTrailOfAsync(request.Id);
        Assert.Contains(records, record => record.Action == AuditAction.Submit);
        Assert.Equal("شراء نقدي من السوق", JsonDocument.Parse(records.Single(record => record.TableName == "ExpenseDetails").After).RootElement.GetProperty("WithoutInvoiceReason").GetString());
    }
}
