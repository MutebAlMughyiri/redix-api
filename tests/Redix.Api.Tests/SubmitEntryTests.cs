using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class SubmitEntryTests(MigratedApiFactory factory)
{
    private Task<Ledger> LedgerAsync() => Ledger.WithFinanceAsync(factory);

    private static Task<HttpResponseMessage> SubmitAsync(Ledger ledger, Guid entryId, HttpClient? client = null) =>
        (client ?? ledger.Client).PostAsync($"/projects/{ledger.ProjectId}/entries/{entryId}/submit", null);

    private static async Task<JsonElement> ReadAsync(Ledger ledger, Guid entryId, HttpClient? client = null)
    {
        var response = await (client ?? ledger.Client).GetAsync($"/projects/{ledger.ProjectId}/entries/{entryId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<Guid> DraftAsync(Ledger ledger, ExpenseRequest request)
    {
        await ledger.RecordAsync(request);
        return request.Id;
    }

    private static Task<Entry> EntryOfAsync(Ledger ledger, Guid entryId) => ledger.ReadAsync(db => db.Entries.SingleAsync(row => row.Id == entryId));

    private static async Task<List<string>> MissingOfAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("entryIncomplete", body.GetProperty("code").GetString());
        return body.GetProperty("missing").EnumerateArray().Select(key => key.GetString()!).ToList();
    }

    [Fact]
    public async Task UC_028_ADraftWithItsFileIsSentAndIsPending()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.Purchase() with { AttachmentId = await ledger.FileAsync() });
        Assert.Equal(EntryStatus.Draft, (await EntryOfAsync(ledger, entryId)).Status);

        var response = await SubmitAsync(ledger, entryId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("pendingApproval", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("entry").GetProperty("status").GetString());
        Assert.Equal(EntryStatus.PendingApproval, (await EntryOfAsync(ledger, entryId)).Status);
    }

    [Fact]
    public async Task AC_09_1_ADraftWithNoFileAndNoReasonIsRefusedNamingTheFile()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.Purchase());

        var missing = await MissingOfAsync(await SubmitAsync(ledger, entryId));

        Assert.Equal(["attachment"], missing);
        Assert.Equal(EntryStatus.Draft, (await EntryOfAsync(ledger, entryId)).Status);
    }

    [Fact]
    public async Task AC_09_1_ADraftThatSaysWhyThereIsNoInvoiceIsSentAndJoinsTheExceptionsList()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());

        (await SubmitAsync(ledger, entryId)).EnsureSuccessStatusCode();

        var exceptions = (await ledger.Client.GetFromJsonAsync<List<ExceptionView>>($"/projects/{ledger.ProjectId}/expenses/exceptions", Ledger.Json))!;
        Assert.Equal(entryId, Assert.Single(exceptions).EntryId);
    }

    [Fact]
    public async Task FR_41_EveryRequiredColumnLeftEmptyIsNamedWithTheFile()
    {
        var ledger = await LedgerAsync();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/columns", new AddColumnRequest("المرحلة", ColumnDataType.Text, true, false, []), Ledger.Json)).EnsureSuccessStatusCode();
        var custom = (await ledger.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/columns", Ledger.Json))!.Single(column => column.Name == "المرحلة").Key;
        await factory.WithDatabaseAsync(db => db.Set<ItemColumn>().Where(column => column.ItemId == ledger.ItemId && column.Key == "product").ExecuteUpdateAsync(column => column.SetProperty(row => row.IsRequired, true)));
        var entryId = await DraftAsync(ledger, ledger.Purchase());

        var missing = await MissingOfAsync(await SubmitAsync(ledger, entryId));

        Assert.Equal("attachment", missing[0]);
        Assert.Contains("product", missing);
        Assert.Contains(custom, missing);
        Assert.Equal(3, missing.Count);
    }

    [Fact]
    public async Task FR_41_WhatADraftHoldsIsWhatAnswersItsColumns()
    {
        var ledger = await LedgerAsync();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/columns", new AddColumnRequest("المرحلة", ColumnDataType.Text, false, false, []), Ledger.Json)).EnsureSuccessStatusCode();
        var custom = (await ledger.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{ledger.ProjectId}/items/{ledger.ItemId}/columns", Ledger.Json))!.Single(column => column.Name == "المرحلة").Key;
        var entryId = await DraftAsync(ledger, ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Product = "أسمنت", Notes = "دفعة أولى", CustomValues = new() { [custom] = "الأرضي" } });
        await factory.WithDatabaseAsync(db => db.Set<ItemColumn>().Where(column => column.ItemId == ledger.ItemId && (column.Key == "product" || column.Key == "notes" || column.Key == custom)).ExecuteUpdateAsync(column => column.SetProperty(row => row.IsRequired, true)));

        (await SubmitAsync(ledger, entryId)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ADraftContractorPaymentNeedsItsReceiptAndOnceSentWaitsOnItsContract()
    {
        var ledger = await LedgerAsync();
        var item = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories/offered?itemType={ItemType.ContractorPayments}", Ledger.Json))!.SelectMany(category => category.Items).First();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, item.Id, ledger.Project.Units[0].Id, 34000m, false), Ledger.Json)).EnsureSuccessStatusCode();
        var contractId = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Ledger.Json))!.Single().Id;
        ContractorPaymentRequest Payment() => new(Guid.CreateVersion7()) { ContractId = contractId, PaymentDate = ledger.Today, Amount = 3000m, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId };
        var noReceipt = Payment();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contractor-payments", noReceipt, Ledger.Json)).EnsureSuccessStatusCode();
        var withReceipt = Payment() with { AttachmentId = await ledger.FileAsync() };
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contractor-payments", withReceipt, Ledger.Json)).EnsureSuccessStatusCode();

        Assert.Equal(["attachment"], await MissingOfAsync(await SubmitAsync(ledger, noReceipt.Id)));
        (await SubmitAsync(ledger, withReceipt.Id)).EnsureSuccessStatusCode();

        var contract = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Ledger.Json))!.Single();
        Assert.Equal(3000m, contract.Pending);
        Assert.Equal(0m, contract.Paid);
    }

    [Fact]
    public async Task UC_033_ARejectedEntryCanBeSentAgain()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice() with { Submit = true });
        await ledger.SetStatusAsync(entryId, EntryStatus.Rejected);

        (await SubmitAsync(ledger, entryId)).EnsureSuccessStatusCode();

        Assert.Equal(EntryStatus.PendingApproval, (await EntryOfAsync(ledger, entryId)).Status);
    }

    [Theory]
    [InlineData(EntryStatus.PendingApproval)]
    [InlineData(EntryStatus.Approved)]
    public async Task AnEntryAlreadySentCannotBeSentAgain(EntryStatus status)
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());
        await ledger.SetStatusAsync(entryId, status);

        var response = await SubmitAsync(ledger, entryId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("entryNotSubmittable", await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task FieldSendsTheirOwnEntryAndNotAnothersAndFinanceSendsAnyones()
    {
        var ledger = await LedgerAsync();
        var (field, fieldId) = await ledger.MemberAsync("users.roleField");
        var financeEntry = await DraftAsync(ledger, ledger.WithoutInvoice());

        var refused = await SubmitAsync(ledger, financeEntry, field);
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        Assert.Equal("notYourEntry", await Ledger.CodeOf(refused));

        var (other, otherId) = await ledger.MemberAsync("users.roleField");
        var fieldEntries = new[] { Guid.CreateVersion7(), Guid.CreateVersion7() };
        await factory.WithDatabaseAsync(async db =>
        {
            var source = await db.Entries.AsNoTracking().SingleAsync(row => row.Id == financeEntry);
            var details = await db.ExpenseDetails.AsNoTracking().SingleAsync(row => row.EntryId == financeEntry);
            foreach (var entryId in fieldEntries)
            {
                db.Add(new Entry { Id = entryId, ProjectId = source.ProjectId, Kind = source.Kind, Status = EntryStatus.Draft, EntryDate = source.EntryDate, Amount = source.Amount, AccountId = source.AccountId, CreatedByUserId = fieldId, CreatedAt = source.CreatedAt });
                db.Add(new ExpenseDetails { EntryId = entryId, ItemId = details.ItemId, SubgroupId = details.SubgroupId, UnitId = details.UnitId, PaymentMethodId = details.PaymentMethodId, WithoutInvoiceReason = "نقدي" });
            }
            await db.SaveChangesAsync();
        });

        (await SubmitAsync(ledger, fieldEntries[0], field)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Forbidden, (await SubmitAsync(ledger, fieldEntries[1], other)).StatusCode);
        (await SubmitAsync(ledger, fieldEntries[1])).EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("users.roleReader")]
    [InlineData("users.roleManager")]
    public async Task WhoHoldsNoRecordEntriesCannotSendAnEntry(string role)
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());
        var (client, _) = await ledger.MemberAsync(role);

        Assert.Equal(HttpStatusCode.Forbidden, (await SubmitAsync(ledger, entryId, client)).StatusCode);
    }

    [Fact]
    public async Task BR_33_AnEntryOfAnotherProjectIsNotFoundAndSomeoneOutsideIsRefused()
    {
        var ledger = await LedgerAsync();
        var other = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());

        Assert.Equal(HttpStatusCode.NotFound, (await SubmitAsync(other, entryId)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.Client.GetAsync($"/projects/{other.ProjectId}/entries/{entryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.GetAsync($"/projects/{ledger.ProjectId}/entries/{entryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await SubmitAsync(ledger, entryId, other.Client)).StatusCode);
    }

    [Fact]
    public async Task AC_04_1_AFinishedProjectSendsNothing()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/close", null)).EnsureSuccessStatusCode();

        var response = await SubmitAsync(ledger, entryId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("projectFinished", await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task FR_21_SendingIsInTheAuditTrailAndARefusalIsNot()
    {
        var ledger = await LedgerAsync();
        var refused = await DraftAsync(ledger, ledger.Purchase());
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());

        await SubmitAsync(ledger, refused);
        await SubmitAsync(ledger, entryId);

        Assert.DoesNotContain(await factory.AuditTrailOfAsync(refused), record => record.Action == AuditAction.Submit);
        var sent = (await factory.AuditTrailOfAsync(entryId)).Single(record => record.Action == AuditAction.Submit);
        Assert.Equal("Entries", sent.TableName);
    }

    [Fact]
    public async Task ReadingADraftAnswersWhatItsKindHasAndEmptyForTheRest()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.Purchase(1150m, "F-55") with { Product = "أسمنت", AttachmentId = await ledger.FileAsync() });
        var (reader, _) = await ledger.MemberAsync("users.roleReader");

        var entry = await ReadAsync(ledger, entryId, reader);

        Assert.Equal("draft", entry.GetProperty("entry").GetProperty("status").GetString());
        Assert.Equal(1150m, entry.GetProperty("entry").GetProperty("amount").GetDecimal());
        Assert.Equal("أسمنت", entry.GetProperty("purchase").GetProperty("product").GetString());
        Assert.Equal("F-55", entry.GetProperty("invoice").GetProperty("number").GetString());
        Assert.Equal(150m, entry.GetProperty("invoice").GetProperty("vat").GetDecimal());
        Assert.Equal(Guid.Empty, entry.GetProperty("contractorPayment").GetProperty("entryId").GetGuid());
        Assert.Equal("invoice", Assert.Single(entry.GetProperty("attachments").EnumerateArray()).GetProperty("role").GetString());
    }

    [Fact]
    public async Task ReadingAnExpenseWithoutAnInvoiceAnswersItsReasonAndAnEmptyInvoice()
    {
        var ledger = await LedgerAsync();
        var entryId = await DraftAsync(ledger, ledger.WithoutInvoice());

        var entry = await ReadAsync(ledger, entryId);

        Assert.Equal("شراء نقدي من السوق", entry.GetProperty("expense").GetProperty("withoutInvoiceReason").GetString());
        Assert.Equal(Guid.Empty, entry.GetProperty("invoice").GetProperty("entryId").GetGuid());
    }

    [Fact]
    public async Task ReadingACreditNoteAnswersWhatItIsLinkedTo()
    {
        var ledger = await LedgerAsync();
        var original = await DraftAsync(ledger, ledger.Purchase() with { AttachmentId = await ledger.FileAsync(), Submit = true });
        var note = await DraftAsync(ledger, ledger.Purchase(100m) with { IsCreditNote = true, OriginalEntryId = original });

        var entry = await ReadAsync(ledger, note);

        Assert.True(entry.GetProperty("invoice").GetProperty("isCreditNote").GetBoolean());
        Assert.Equal(original, Assert.Single(entry.GetProperty("links").EnumerateArray()).GetProperty("linkedEntryId").GetGuid());
    }
}
