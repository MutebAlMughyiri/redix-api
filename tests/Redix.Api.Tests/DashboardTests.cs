using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Reports;
using Redix.Api.Structure;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-049, UC-050, UC-063: the dashboard, the item report, and completion and cost. No figure here is real.</summary>
[Collection(DatabaseCollection.Name)]
public class DashboardTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<DashboardView> DashboardAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<DashboardView>($"/projects/{projectId}/dashboard", Json))!;

    private static async Task<Guid> ApprovedAsync(Ledger ledger, ExpenseRequest request)
    {
        await ledger.RecordAsync(request with { Submit = true, AttachmentId = request.WithoutInvoiceReason.Length > 0 ? Guid.Empty : await ledger.FileAsync() });
        await ledger.SetStatusAsync(request.Id, EntryStatus.Approved);
        return request.Id;
    }

    [Fact]
    public async Task AC_34_1_AnApprovalShowsAtOnceInTheSurplusThePendingAndTheCategories()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var waiting = ledger.WithoutInvoice(300) with { Submit = true };
        await ledger.RecordAsync(waiting);

        var before = await DashboardAsync(ledger.Client, ledger.ProjectId);
        Assert.Equal((1, 300m, 0m), (before.PendingCount, before.PendingAmount, before.Expenses));

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([waiting.Id]), Json)).EnsureSuccessStatusCode();

        var after = await DashboardAsync(ledger.Client, ledger.ProjectId);
        Assert.Equal((0, 300m, -300m), (after.PendingCount, after.Expenses, after.Surplus));
        var category = after.Categories.Single(row => row.Items.Any(item => item.Id == ledger.ItemId));
        Assert.Equal(300m, category.Total);
        Assert.Equal(300m, category.Items.Single(item => item.Id == ledger.ItemId).Total);
        Assert.Equal(after.Expenses, after.Categories.Sum(row => row.Total));
    }

    [Fact]
    public async Task TheDashboardCarriesTheReconciliationDifferenceOnceThereIsOne()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        Assert.False((await DashboardAsync(ledger.Client, ledger.ProjectId)).WasReconciled);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/reconciliations", new ReconcileRequest([new(ledger.AccountId, 25)]), Json)).EnsureSuccessStatusCode();

        var dashboard = await DashboardAsync(ledger.Client, ledger.ProjectId);
        Assert.True(dashboard.WasReconciled);
        Assert.Equal(25m, dashboard.ReconciliationDifference);
    }

    [Fact]
    public async Task UC_063_TheManagerSetsTheCompletionAndEveryoneSeesItAndTheCost()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ApprovedAsync(ledger, ledger.WithoutInvoice(200));
        var (reader, _) = await ledger.MemberAsync("Reader");

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/completion", new CompletionRequest(42.5m), Json)).EnsureSuccessStatusCode();

        var seen = await DashboardAsync(reader, ledger.ProjectId);
        Assert.Equal((42.5m, 200m), (seen.CompletionPercent, seen.Expenses));
        Assert.Equal(HttpStatusCode.Forbidden, (await reader.PostAsJsonAsync($"/projects/{ledger.ProjectId}/completion", new CompletionRequest(50), Json)).StatusCode);
        Assert.Equal("invalidCompletion", await Ledger.CodeOf(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/completion", new CompletionRequest(101), Json)));
        Assert.Equal("invalidCompletion", await Ledger.CodeOf(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/completion", new CompletionRequest(10.555m), Json)));
    }

    [Fact]
    public async Task AC_30_1_AnItemsSubgroupsAndContractPaymentsAddUpToItsTotal()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var item = await ledger.ReadAsync(db => db.Items.Include(row => row.Subgroups).SingleAsync(row => row.Id == ledger.ItemId));
        await factory.WithDatabaseAsync(db => db.Items.Where(row => row.Id == ledger.ItemId).ExecuteUpdateAsync(row => row.SetProperty(entry => entry.Type, ItemType.Both)));
        await ApprovedAsync(ledger, ledger.Purchase(115, "P1") with { Product = "بلاط" });
        await ApprovedAsync(ledger, ledger.WithoutInvoice(50));
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contracts", new AddContractRequest(ledger.SupplierId, ledger.ItemId, ledger.Project.Units[0].Id, 1000, false), Json)).EnsureSuccessStatusCode();
        var contract = (await ledger.Client.GetFromJsonAsync<List<ContractView>>($"/projects/{ledger.ProjectId}/contracts", Json))!.Single();
        var payment = new ContractorPaymentRequest(Guid.CreateVersion7()) { ContractId = contract.Id, AttachmentId = await ledger.FileAsync(), PaymentDate = ledger.Today, Amount = 300, PaymentMethodId = ledger.MethodId, AccountId = ledger.AccountId, Submit = true };
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/contractor-payments", payment, Json)).EnsureSuccessStatusCode();
        await ledger.SetStatusAsync(payment.Id, EntryStatus.Approved);

        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}", Json))!;

        Assert.Equal(465m, report.Total);
        Assert.Equal(report.Total, report.Subgroups.Sum(row => row.Total) + report.Contracts.Sum(row => row.PaymentsTotal));
        var purchase = report.Subgroups.SelectMany(row => row.Rows).Single(row => row.InvoiceNumber == "P1");
        Assert.Equal(("بلاط", "شركة النخيل", 115m), (purchase.Description, purchase.SupplierName, purchase.Amount));
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
        await ApprovedAsync(ledger, ledger.WithoutInvoice(50) with { CustomValues = new Dictionary<string, string> { [column.Key] = "أبيض" } });

        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{ledger.ItemId}", Json))!;

        Assert.Contains("اللون", report.Columns);
        Assert.Contains("أبيض", report.Subgroups.Single().Rows.Single().Values);
    }
}
