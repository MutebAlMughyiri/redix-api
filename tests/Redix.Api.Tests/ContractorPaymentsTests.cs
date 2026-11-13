using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.History;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

[Collection(SecondDatabaseCollection.Name)]
public class ContractorPaymentsTests(MigratedApiFactory factory)
{
    private sealed record Setup(Ledger Ledger, Guid ContractId, Guid ItemId);

    private async Task<Setup> ContractAsync(decimal value = 34000m, bool isOpenValue = false)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        return await ContractOfAsync(ledger, value, isOpenValue);
    }

    private static async Task<Setup> ContractOfAsync(Ledger ledger, decimal value, bool isOpenValue)
    {
        var item = (await ledger.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ledger.ProjectId}/categories/offered?itemType={ItemType.ContractorPayments}", Ledger.Json))!.SelectMany(category => category.Items).First();
        var response = await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, item.Id, value, isOpenValue), Ledger.Json);
        response.EnsureSuccessStatusCode();
        var contract = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Ledger.Json))!.Last();
        return new Setup(ledger, contract.Id, item.Id);
    }

    private static EntryRequest Payment(Setup setup, decimal amount = 3000m) => new(Guid.CreateVersion7())
    {
        ItemId = setup.ItemId,
        TableId = setup.ContractId,
        Date = setup.Ledger.Today,
        Amount = amount,
        PaymentMethodId = setup.Ledger.MethodId,
        AccountId = setup.Ledger.AccountId,
    };

    private static Task<HttpResponseMessage> PostAsync(Setup setup, EntryRequest request, HttpClient? client = null) =>
        (client ?? setup.Ledger.Client).PostAsJsonAsync($"/projects/{setup.Ledger.ProjectId}/entries", request, Ledger.Json);

    private static async Task<ContractorPaymentView> RecordAsync(Setup setup, EntryRequest request)
    {
        var response = await PostAsync(setup, request);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ContractorPaymentView>(Ledger.Json))!;
    }

    /// <summary>A payment recorded through the API and sent with its receipt, and then approved as UC-032 "Approve or reject entries" will: by changing its status.</summary>
    private static async Task<EntryRequest> ApprovedAsync(Setup setup, decimal amount, bool isOutsideContract = false)
    {
        var request = Payment(setup, amount) with { IsOutsideContract = isOutsideContract, AttachmentId = await setup.Ledger.FileAsync(), Submit = true };
        await RecordAsync(setup, request);
        await setup.Ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        return request;
    }

    private static async Task<ContractView> ContractOfAsync(Setup setup) =>
        (await setup.Ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{setup.Ledger.ProjectId}/contracts", Ledger.Json))!.Single(row => row.Id == setup.ContractId);

    private static async Task AssertRefusedAsync(Setup setup, EntryRequest request, string code)
    {
        var response = await PostAsync(setup, request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await Ledger.CodeOf(response));
    }

    /// <summary>The item's total as the design defines it (BR-02): the approved entries of the item, by Entries.Amount.</summary>
    private static Task<decimal> ItemTotalAsync(Setup setup) => setup.Ledger.ReadAsync(db => (
        from details in db.ExpenseDetails
        join entry in db.Entries on details.EntryId equals entry.Id
        where details.ItemId == setup.ItemId && entry.ProjectId == setup.Ledger.ProjectId && entry.Status == EntryStatus.Approved
        select entry.Amount).SumAsync());

    private static async Task<Reports.ItemReportView> ReportOfAsync(Setup setup) =>
        (await setup.Ledger.Client.GetFromJsonAsync<Reports.ItemReportView>($"/projects/{setup.Ledger.ProjectId}/reports/items/{setup.ItemId}", Ledger.Json))!;

    [Fact]
    public async Task EditingAWaitingPaymentCountsItOnceKeepsTheOthersAndAnswersOk()
    {
        var setup = await ContractAsync(10000m);
        var request = Payment(setup) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true };
        await RecordAsync(setup, request);
        await RecordAsync(setup, Payment(setup, 1000m) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true });

        var edited = await setup.Ledger.Client.PutAsJsonAsync($"/projects/{setup.Ledger.ProjectId}/entries/{request.Id}", request with { Amount = 4000m }, Ledger.Json);

        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        var view = (await edited.Content.ReadFromJsonAsync<ContractorPaymentView>(Ledger.Json))!;
        Assert.Equal((1000m, 5000m, false), (view.Pending, view.RemainingAfter, view.IsOverrun));
    }

    [Fact]
    public async Task TheItemReportLeavesAnOpenValueContractNothingRemainingAndNoOverrun()
    {
        var setup = await ContractAsync(0m, isOpenValue: true);
        await ApprovedAsync(setup, 5000m);

        var contract = (await ReportOfAsync(setup)).Contracts.Single(row => row.Id == setup.ContractId);

        Assert.Equal((5000m, 0m, false), (contract.Paid, contract.Remaining, contract.IsOverrun));
    }

    [Fact]
    public async Task TheItemReportLeavesAValuedContractItsValueLessWhatWasPaid()
    {
        var setup = await ContractAsync(10000m);
        await ApprovedAsync(setup, 4000m);

        var contract = (await ReportOfAsync(setup)).Contracts.Single(row => row.Id == setup.ContractId);

        Assert.Equal((4000m, 6000m, false), (contract.Paid, contract.Remaining, contract.IsOverrun));
    }

    [Fact]
    public async Task AC_07_1_ThePaymentsOfA34000ContractLeaveItsRemainingAndOneMoreOverrunsItWithAnAlertAndIsSaved()
    {
        var setup = await ContractAsync();
        await ApprovedAsync(setup, 30000m);

        var before = await ContractOfAsync(setup);
        Assert.Equal(30000m, before.Paid);
        Assert.Equal(4000m, before.Remaining);
        Assert.False(before.IsOverrun);

        var request = Payment(setup, 5000m) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true };
        var view = await RecordAsync(setup, request);

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
        Assert.Equal(-1000m, view.RemainingAfter);
        Assert.True(view.IsOverrun);
        Assert.Equal(30000m, view.Paid);

        await setup.Ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        var after = await ContractOfAsync(setup);
        Assert.Equal(35000m, after.Paid);
        Assert.Equal(-1000m, after.Remaining);
        Assert.True(after.IsOverrun);
    }

    [Fact]
    public async Task AC_07_2_AnOpenValueContractAddsUpWithNoRemainingAndNoAlert()
    {
        var setup = await ContractAsync(0m, isOpenValue: true);
        await ApprovedAsync(setup, 12000m);

        var view = await RecordAsync(setup, Payment(setup, 5000m));
        var contract = await ContractOfAsync(setup);

        Assert.Equal(0m, view.RemainingAfter);
        Assert.False(view.IsOverrun);
        Assert.Equal(12000m, contract.Paid);
        Assert.Equal(0m, contract.Remaining);
        Assert.False(contract.IsOverrun);
    }

    [Fact]
    public async Task BR_10_OnlyAnApprovedPaymentIsPaidAndAPendingOneWaitsBesideIt()
    {
        var setup = await ContractAsync();
        var request = Payment(setup, 5000m) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true };

        await RecordAsync(setup, request);

        var pending = await ContractOfAsync(setup);
        Assert.Equal(0m, pending.Paid);
        Assert.Equal(5000m, pending.Pending);
        Assert.Equal(34000m, pending.Remaining);

        await setup.Ledger.SetStatusAsync(request.Id, EntryStatus.Approved);

        var approved = await ContractOfAsync(setup);
        Assert.Equal(5000m, approved.Paid);
        Assert.Equal(0m, approved.Pending);
        Assert.Equal(29000m, approved.Remaining);
    }

    [Fact]
    public async Task ADraftAndARejectedPaymentCountForNothing()
    {
        var setup = await ContractAsync();
        await RecordAsync(setup, Payment(setup, 5000m));
        var rejected = await ApprovedAsync(setup, 7000m);
        await setup.Ledger.SetStatusAsync(rejected.Id, EntryStatus.Rejected);

        var contract = await ContractOfAsync(setup);

        Assert.Equal(0m, contract.Paid);
        Assert.Equal(0m, contract.Pending);
    }

    [Fact]
    public async Task TheAlertWhileEnteringCountsWhatWaitsSoTwoPendingPaymentsCannotOverrunUnseen()
    {
        var setup = await ContractAsync();
        await RecordAsync(setup, Payment(setup, 30000m) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true });

        var view = await RecordAsync(setup, Payment(setup, 5000m));

        Assert.Equal(0m, view.Paid);
        Assert.Equal(30000m, view.Pending);
        Assert.Equal(-1000m, view.RemainingAfter);
        Assert.True(view.IsOverrun);
    }

    [Fact]
    public async Task AC_13_1_APaymentOutsideTheContractEntersTheItemTotalAndLeavesTheRemainingAlone()
    {
        var setup = await ContractAsync();
        await ApprovedAsync(setup, 1000m);

        var view = await RecordAsync(setup, Payment(setup, 2000m) with { IsOutsideContract = true });
        Assert.Equal(33000m, view.RemainingAfter);
        await ApprovedAsync(setup, 2000m, isOutsideContract: true);

        var contract = await ContractOfAsync(setup);
        Assert.Equal(1000m, contract.Paid);
        Assert.Equal(33000m, contract.Remaining);
        Assert.Equal(3000m, await ItemTotalAsync(setup));
    }

    [Fact]
    public async Task AC_14_1_ADeductionOf136ReducesTheItemTotalAndThePaidOnTheContract()
    {
        var setup = await ContractAsync();
        await ApprovedAsync(setup, 1000m);
        var deduction = Payment(setup, -136m) with { Notes = "خصم من المبلط", Submit = true };

        await RecordAsync(setup, deduction);
        await setup.Ledger.SetStatusAsync(deduction.Id, EntryStatus.Approved);

        var contract = await ContractOfAsync(setup);
        Assert.Equal(864m, contract.Paid);
        Assert.Equal(33136m, contract.Remaining);
        Assert.Equal(864m, await ItemTotalAsync(setup));
    }

    [Fact]
    public async Task D14_ADeductionSaysWhyInItsNotes()
    {
        var setup = await ContractAsync();

        await AssertRefusedAsync(setup, Payment(setup, -136m), "deductionNeedsNotes");
        Assert.Equal(-136m, (await RecordAsync(setup, Payment(setup, -136m) with { Notes = "خصم" })).Amount);
    }

    [Fact]
    public async Task D16_APaymentIsSentWithNoReceipt()
    {
        var setup = await ContractAsync();

        var sent = await RecordAsync(setup, Payment(setup) with { Submit = true });

        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
    }

    [Fact]
    public async Task TheReceiptIsLinkedAsAReceiptAndAFileAlreadyOnAnEntryIsRefused()
    {
        var setup = await ContractAsync();
        var fileId = await setup.Ledger.FileAsync();
        var request = Payment(setup) with { AttachmentId = fileId, Submit = true };

        await RecordAsync(setup, request);

        var link = await setup.Ledger.ReadAsync(db => db.EntryAttachments.SingleAsync(row => row.EntryId == request.Id));
        Assert.Equal(EntryAttachmentRole.Receipt, link.Role);
        var again = await PostAsync(setup, Payment(setup) with { AttachmentId = fileId });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("invoiceAlreadyAttached", await Ledger.CodeOf(again));
    }

    [Fact]
    public async Task AC_16_3_APaymentSentAgainLandsOnce()
    {
        var setup = await ContractAsync();
        var request = Payment(setup);
        await RecordAsync(setup, request);

        var again = await PostAsync(setup, request);

        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(1, await setup.Ledger.ReadAsync(db => db.Entries.CountAsync(row => row.Id == request.Id)));
    }

    [Fact]
    public async Task R11_APaymentGoesUnderItsItemsGeneralSubgroupEvenWhenTheItemHasNamedOnes()
    {
        var setup = await ContractAsync();
        (await setup.Ledger.Client.PostAsJsonAsync($"/projects/{setup.Ledger.ProjectId}/items/{setup.ItemId}/subgroups", new NameRequest("الدور الأرضي"))).EnsureSuccessStatusCode();
        var request = Payment(setup);

        await RecordAsync(setup, request);

        var details = await setup.Ledger.ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == request.Id));
        var subgroup = await setup.Ledger.ReadAsync(db => db.Set<Subgroup>().SingleAsync(row => row.Id == details.SubgroupId));
        Assert.Equal(Subgroup.GeneralName, subgroup.Name);
    }

    [Fact]
    public async Task ThePaymentTakesItsItemAndUnitFromTheContractInTheItemsGeneralSubgroup()
    {
        var setup = await ContractAsync();
        var request = Payment(setup);

        await RecordAsync(setup, request);

        var (entry, details, payment) = await setup.Ledger.ReadAsync(async db => (
            await db.Entries.SingleAsync(row => row.Id == request.Id),
            await db.ExpenseDetails.SingleAsync(row => row.EntryId == request.Id),
            await db.ContractorPaymentDetails.SingleAsync(row => row.EntryId == request.Id)));
        Assert.Equal(EntryKind.ContractorPayment, entry.Kind);
        Assert.Equal(3000m, entry.Amount);
        Assert.Equal(setup.Ledger.Today, entry.EntryDate);
        Assert.Equal(setup.ItemId, details.ItemId);
        Assert.Equal(setup.ContractId, payment.ContractId);
        Assert.False(payment.IsOutsideContract);
        var subgroup = await setup.Ledger.ReadAsync(db => db.Set<Subgroup>().SingleAsync(row => row.Id == details.SubgroupId));
        Assert.Equal(Subgroup.GeneralName, subgroup.Name);
    }

    [Fact]
    public async Task D16_APaymentWritesNoInvoiceRow()
    {
        var setup = await ContractAsync();
        var request = Payment(setup);

        await RecordAsync(setup, request);

        Assert.False(await setup.Ledger.ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == request.Id)));
    }

    [Fact]
    public async Task WhatWasTypedIsRefusedWhenItCannotBeKept()
    {
        var setup = await ContractAsync();
        var today = setup.Ledger.Today;

        await AssertRefusedAsync(setup, Payment(setup, 0m), "invalidAmount");
        await AssertRefusedAsync(setup, Payment(setup, 1.234m), "invalidAmount");
        await AssertRefusedAsync(setup, Payment(setup) with { Notes = new string('م', Entry.NotesMaxLength + 1) }, "invalidNotes");
        await AssertRefusedAsync(setup, Payment(setup) with { Date = default }, "invalidDate");
        await AssertRefusedAsync(setup, Payment(setup) with { Date = today.AddDays(1) }, "invalidDate");
    }

    [Fact]
    public async Task ADisabledContractOrOneOfAnotherProjectIsRefused()
    {
        var setup = await ContractAsync();
        var other = await ContractAsync();

        await AssertRefusedAsync(setup, Payment(setup) with { TableId = other.ContractId }, "unknownTable");
        (await setup.Ledger.Client.PostAsync($"/projects/{setup.Ledger.ProjectId}/contracts/{setup.ContractId}/disable", null)).EnsureSuccessStatusCode();
        await AssertRefusedAsync(setup, Payment(setup), "unknownTable");
    }

    [Fact]
    public async Task AC_38_1_AContractWhoseItemIsDisabledTakesNoPayment()
    {
        var setup = await ContractAsync();
        await factory.WithDatabaseAsync(db => db.Items.Where(item => item.Id == setup.ItemId).ExecuteUpdateAsync(item => item.SetProperty(row => row.IsActive, false)));

        await AssertRefusedAsync(setup, Payment(setup), "unknownTable");
    }

    [Fact]
    public async Task OpenQuestion4_FieldChargesAnAccountTheyHoldAndNoOther()
    {
        var setup = await ContractAsync();
        var (field, fieldId) = await setup.Ledger.MemberAsync("Field");

        var refused = await PostAsync(setup, Payment(setup), field);
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("accountNotOffered", await Ledger.CodeOf(refused));

        (await setup.Ledger.Client.PostAsJsonAsync($"/projects/{setup.Ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody) { CustodianId = fieldId }, Ledger.Json)).EnsureSuccessStatusCode();
        var custody = (await setup.Ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{setup.Ledger.ProjectId}/accounts", Ledger.Json))!.Single(account => account.Name == "عهدة نواف");

        var response = await PostAsync(setup, Payment(setup) with { AccountId = custody.Id }, field);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task FR_41_ARequiredCustomColumnIsAskedOfAPaymentWhenSending()
    {
        var setup = await ContractAsync();
        (await setup.Ledger.Client.PostAsJsonAsync($"/projects/{setup.Ledger.ProjectId}/items/{setup.ItemId}/columns", new AddColumnRequest("المرحلة", ColumnDataType.Text, true, false, []), Ledger.Json)).EnsureSuccessStatusCode();
        var key = (await setup.Ledger.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{setup.Ledger.ProjectId}/items/{setup.ItemId}/columns", Ledger.Json))!.Single(column => column.Name == "المرحلة").Key;
        var request = Payment(setup) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true };

        var refused = await PostAsync(setup, request);
        var answer = await refused.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("requiredColumnMissing", answer.GetProperty("code").GetString());
        Assert.Equal(key, answer.GetProperty("columnKey").GetString());

        await RecordAsync(setup, request with { ColumnValues = new Dictionary<string, string> { [key] = "الدور الأرضي" } });
    }

    [Fact]
    public async Task FR_41_WhatOnlyAPurchaseHoldsIsNeverAskedOfAPayment()
    {
        var setup = await ContractAsync();
        await factory.WithDatabaseAsync(db => db.Set<ItemColumn>().Where(column => column.ItemId == setup.ItemId && column.Key == "product").ExecuteUpdateAsync(column => column.SetProperty(row => row.IsRequired, true)));

        var view = await RecordAsync(setup, Payment(setup) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true });

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
    }

    [Fact]
    public async Task AnEmptyIdIsRefused()
    {
        var setup = await ContractAsync();

        await AssertRefusedAsync(setup, Payment(setup) with { Id = Guid.Empty }, "invalidEntryId");
    }

    [Fact]
    public async Task AC_04_1_AFinishedProjectRecordsNoPayment()
    {
        var setup = await ContractAsync();
        (await setup.Ledger.Client.PostAsync($"/projects/{setup.Ledger.ProjectId}/close", null)).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Payment(setup));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("projectFinished", await Ledger.CodeOf(response));
    }

    [Fact]
    public async Task FR_21_APaymentIsInTheHistory()
    {
        var setup = await ContractAsync();
        var request = Payment(setup) with { AttachmentId = await setup.Ledger.FileAsync(), Submit = true };

        await RecordAsync(setup, request);

        var trail = await factory.HistoryOfAsync(request.Id);
        Assert.Equal(["Entries", "ExpenseDetails", "ContractorPaymentDetails", "EntryAttachments"], trail.Select(record => record.TableName));
        Assert.Equal(HistoryAction.Submit, trail[0].Action);
    }

    /// <summary>Two contracts on one item, 3000 paid on the first and 777 on the second, both approved.</summary>
    private async Task<(Setup First, Setup Second)> TwoPaidContractsAsync()
    {
        var first = await ContractAsync();
        var second = await ContractOfAsync(first.Ledger, 5000m, false);
        await ApprovedAsync(first, 3000m);
        await ApprovedAsync(second, 777m);
        return (first, second);
    }

    [Fact]
    public async Task Muteb12Nov_AContractsReportIsItsItemsReportHoldingThatContractsTableAlone()
    {
        var (first, _) = await TwoPaidContractsAsync();

        var report = (await first.Ledger.Client.GetFromJsonAsync<Reports.ItemReportView>($"/projects/{first.Ledger.ProjectId}/reports/contracts/{first.ContractId}", Ledger.Json))!;

        Assert.Equal(first.ItemId, report.ItemId);
        Assert.Empty(report.Subgroups);
        var contract = Assert.Single(report.Contracts);
        Assert.Equal(first.ContractId, contract.Id);
        Assert.Equal([3000m], contract.Payments.Select(payment => payment.Amount));
        Assert.Equal(3000m, contract.PaymentsTotal);
    }

    [Fact]
    public async Task Muteb12Nov_AContractsPdfAndWorkbookHoldItsOwnPaymentsAndNotAnothersOnTheSameItem()
    {
        var (first, _) = await TwoPaidContractsAsync();
        var address = $"/projects/{first.Ledger.ProjectId}/reports/contracts/{first.ContractId}";

        var pdf = await first.Ledger.Client.GetByteArrayAsync($"{address}/pdf");
        using var workbook = new ClosedXML.Excel.XLWorkbook(new MemoryStream(await first.Ledger.Client.GetByteArrayAsync($"{address}/xlsx")));
        var amounts = workbook.Worksheets.SelectMany(sheet => sheet.CellsUsed()).Where(cell => cell.DataType == ClosedXML.Excel.XLDataType.Number).Select(cell => (decimal)cell.Value.GetNumber()).ToList();

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        Assert.Contains(3000m, amounts);
        Assert.DoesNotContain(777m, amounts);
    }

    [Fact]
    public async Task Muteb12Nov_AContractThatIsNotThisProjectsHasNoReportAndNoFiles()
    {
        var setup = await ContractAsync();
        var address = $"/projects/{setup.Ledger.ProjectId}/reports/contracts/{Guid.CreateVersion7()}";

        foreach (var route in new[] { address, $"{address}/pdf", $"{address}/xlsx" })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await setup.Ledger.Client.GetAsync(route)).StatusCode);
        }
    }
}
