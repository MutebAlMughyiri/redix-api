using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Reports;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>UC-049, UC-050, UC-063: the summary, the item report, and completion and cost. No figure here is real.</summary>
[Collection(FourthDatabaseCollection.Name)]
public class SummaryTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<SummaryView> SummaryAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<SummaryView>($"/projects/{projectId}/summary", Json))!;

    private static async Task ApprovedAsync(Ledger ledger, EntryRequest request)
    {
        await ledger.RecordAsync(request with { Submit = true, AttachmentId = await ledger.FileAsync() });
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
    }

    [Fact]
    public async Task AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var waiting = ledger.Purchase(300) with { Submit = true };
        await ledger.RecordAsync(waiting);

        var before = await SummaryAsync(ledger.Client, ledger.ProjectId);
        Assert.Equal((1, 300m, 0m), (before.Waiting.ExpensesCount, before.Waiting.ExpensesAmount, before.Expenses));

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([waiting.Id]), Json)).EnsureSuccessStatusCode();

        var after = await SummaryAsync(ledger.Client, ledger.ProjectId);
        Assert.Equal((0, 300m, -300m), (after.Waiting.ExpensesCount, after.Expenses, after.Surplus));
        var category = after.Categories.Single(row => row.Items.Any(item => item.Id == ledger.ItemId));
        Assert.Equal(300m, category.Total);
        Assert.Equal(300m, category.Items.Single(item => item.Id == ledger.ItemId).Total);
        Assert.Equal(after.Expenses, after.Categories.Sum(row => row.Total));
    }

    [Fact]
    public async Task TheSummaryCarriesTheReconciliationDifferenceOnceThereIsOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        Assert.False((await SummaryAsync(ledger.Client, ledger.ProjectId)).WasReconciled);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 25)]), Json)).EnsureSuccessStatusCode();

        var summary = await SummaryAsync(ledger.Client, ledger.ProjectId);
        Assert.True(summary.WasReconciled);
        Assert.Equal(25m, summary.ReconciliationDifference);
    }

    [Theory]
    [InlineData(800, 200, 25)]
    [InlineData(300, 100, 33.33)]
    [InlineData(100, 150, 150)]
    [InlineData(0, 200, 0)]
    public void UC_063_CompletionIsExpensesAsAPercentageOfRevenue(decimal capital, decimal expenses, decimal percent) =>
        Assert.Equal(percent, new ProjectFigures(capital, 0, 0, 0, expenses).CompletionPercent);

    [Fact]
    public async Task UC_063_EveryoneSeesTheCompletionAndTheCostAndNobodyTypesIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.Purchase(200));
        var (reader, _) = await ledger.MemberAsync("Reader");

        var seen = await SummaryAsync(reader, ledger.ProjectId);

        Assert.Equal((0m, 200m), (seen.CompletionPercent, seen.Expenses));
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/completion", new { percent = 50 }, Json)).StatusCode);
    }

    [Fact]
    public async Task AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var item = await ledger.ReadAsync(db => db.Items.Include(row => row.Subgroups).SingleAsync(row => row.Id == ledger.ItemId));
        await factory.WithDatabaseAsync(db => db.Items.Where(row => row.Id == ledger.ItemId).ExecuteUpdateAsync(row => row.SetProperty(entry => entry.Type, ItemType.Both)));
        await ApprovedAsync(ledger, ledger.Purchase(115) with { Description = "بلاط" });
        await ApprovedAsync(ledger, ledger.Purchase(50));
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, ledger.ItemId, 1000, false), Json)).EnsureSuccessStatusCode();
        var contract = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Json))!.Single();
        var payment = new EntryRequest(Guid.CreateVersion7()) { ItemId = ledger.ItemId, TableId = contract.Id, AttachmentId = await ledger.FileAsync(), Date = ledger.Today, Amount = 300, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId, Submit = true };
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", payment, Json)).EnsureSuccessStatusCode();
        await ledger.SetStatusAsync(payment.Id, EntryStatus.Approved);

        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}", Json))!;

        Assert.Equal(465m, report.Total);
        Assert.Equal(report.Total, report.Subgroups.Sum(row => row.Total) + report.Contracts.Sum(row => row.PaymentsTotal));
        var table = report.Subgroups.Single(row => row.Columns.Count > 0 && row.Rows.Any(line => line.Values[0] == "بلاط"));
        var purchase = table.Rows.Single(row => row.Values[0] == "بلاط");
        Assert.Equal(("description", "شركة النخيل", 115m), (table.Columns[0].Key, purchase.Values[table.Columns.FindIndex(column => column.Name == "المورد")], purchase.Amount));
        var contractRow = Assert.Single(report.Contracts);
        Assert.Equal((1000m, 300m, 700m, false), (contractRow.Value, contractRow.Paid, contractRow.Remaining, contractRow.IsOverrun));
        Assert.Equal(item.Name, report.ItemName);
    }

    [Fact]
    public async Task AnItemOfAnotherProjectIsNotFound()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var other = await Ledger.WithFinanceAsync(factory);

        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/reports/items/{other.ItemId}")).StatusCode);
    }

    [Fact]
    public async Task AnItemsOwnColumnsPrintWhenTheyShowInReports()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var column = new ItemColumn { ItemId = ledger.ItemId, Key = Column.NewCustomKey(), Name = "اللون", DataType = ColumnDataType.Text, ShowInReports = true, SortOrder = 99 };
        await factory.WithDatabaseAsync(async db =>
        {
            db.Add(column);
            await db.SaveChangesAsync();
        });
        await ApprovedAsync(ledger, ledger.Purchase(50) with { ColumnValues = new Dictionary<string, string> { [column.Key] = "أبيض" } });

        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}", Json))!;

        Assert.Contains("اللون", report.Subgroups.Single().Columns.Select(printed => printed.Name));
        Assert.Contains("أبيض", report.Subgroups.Single().Rows.Single().Values);
    }
}
