using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Import;
using Redix.Api.Notifications;
using Redix.Api.Reports;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>UC-071, FR-53: a project's file, read and loaded into REDIX (redix-api #132). Every file and figure here is made up for the test.</summary>
[Collection(ThirdDatabaseCollection.Name)]
public class ProjectFileTests(MigratedApiFactory factory)
{
    /// <summary>A small project file in #130's shape with D11: one purchase table of two rows, one account and its actual balance, and report 7.</summary>
    private const string SmallFile = """
        {
          "format": "redix.project", "version": 1, "project": "مشروع الاختبار",
          "report": { "number": 7, "date": "2026-11-02", "time": "04:40" },
          "accounts": [ { "name": "حساب الاختبار", "actualBalance": 1250.5 } ],
          "lists": [
            { "key": "account", "name": "الحساب", "options": [ { "id": "account-1", "label": "حساب الاختبار", "sign": "" } ] },
            { "key": "entryType", "name": "نوع القيد", "options": [ { "id": "entryType-1", "label": "شراء", "sign": "+" }, { "id": "entryType-2", "label": "مرتجع", "sign": "-" } ] }
          ],
          "categories": [ { "name": "فئة الاختبار", "items": [ { "name": "بند الاختبار", "type": "both", "tables": [
            { "name": "بند الاختبار", "kind": "purchases",
              "columns": [ { "key": "description", "name": "الوصف", "type": "text" }, { "key": "entryType", "name": "نوع القيد", "type": "list", "list": "entryType" },
                           { "key": "account", "name": "الحساب", "type": "list", "list": "account" }, { "key": "total", "name": "التكلفة", "type": "amount" } ],
              "rows": [ { "description": "صف أول", "entryType": "entryType-1", "account": "account-1", "total": 100 },
                        { "description": "صف ثان", "entryType": "entryType-2", "account": "account-1", "total": 20.25 } ],
              "details": [ { "label": "المساحة", "value": 300 } ] },
            { "name": "دفعات مقاول الاختبار", "kind": "contractorPayments", "columns": [], "rows": [], "contractAmount": "" } ] } ] } ]
        }
        """;

    [Fact]
    public void AC_53_9_AProjectFileIsReadWithItsReportItsActualBalancesItsListsAndItsTables()
    {
        var (file, refusal) = ProjectFile.Read(SmallFile);

        Assert.Equal("", refusal);
        Assert.Equal((7, new DateTime(2026, 11, 2, 1, 40, 0, DateTimeKind.Utc)), (file.Report.Number, file.Report.At));
        Assert.Equal(("حساب الاختبار", 1250.5m), (file.Accounts.Single().Name, file.Accounts.Single().ActualBalance));
        Assert.Equal(["account-1", "entryType-1", "entryType-2"], file.Lists.SelectMany(list => list.Options).Select(option => option.Id));
        var item = file.Categories.Single().Items.Single();
        Assert.Equal(ItemType.Both, item.Type);
        Assert.Equal([ItemType.Purchases, ItemType.ContractorPayments], item.Tables.Select(table => table.Kind));
        var purchases = item.Tables[0];
        Assert.Equal(ColumnDataType.List, purchases.Columns.Single(column => column.Key == "entryType").Type);
        Assert.Equal("entryType", purchases.Columns.Single(column => column.Key == "entryType").List);
        Assert.Equal([100m, 20.25m], purchases.Rows.Select(row => row["total"].GetDecimal()));
    }

    [Theory]
    [InlineData("this is not JSON")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{ "format": "another.app", "version": 1 }""")]
    [InlineData("""{ "format": "redix.project", "version": 0 }""")]
    [InlineData("""{ "format": "redix.project", "version": 1, "categories": "not a list" }""")]
    [InlineData("""{ "format": "redix.project", "version": 1, "categories": [ { "name": null, "items": [] } ] }""")]
    [InlineData("""{ "format": "redix.project", "version": 1, "categories": [ { "name": "فئة", "items": [ { "name": "بند", "type": "notAType", "tables": [] } ] } ] }""")]
    [InlineData("""{ "format": "redix.project", "version": 1, "report": { "number": 7, "date": "2 Nov", "time": "04:40" } }""")]
    public void AC_53_9_AFileThatIsNotARedixProjectFileIsRefusedAndNothingIsRead(string text)
    {
        var (file, refusal) = ProjectFile.Read(text);

        Assert.Equal(("NotAProjectFile", ProjectFile.None), (refusal, file));
    }

    [Fact]
    public void AC_53_9_AFileOfANewerVersionIsRefusedBeforeItsShapeIsRead()
    {
        var (file, refusal) = ProjectFile.Read("""{ "format": "redix.project", "version": 2, "categories": "a shape this version does not know" }""");

        Assert.Equal(("NewerVersion", ProjectFile.None), (refusal, file));
    }

    [Fact]
    public void D9_D2_AFileWithNoReportNoAccountsAndNoRowsIsReadAsItsStructureAlone()
    {
        var (file, refusal) = ProjectFile.Read("""{ "format": "redix.project", "version": 1, "categories": [ { "name": "فئة", "items": [ { "name": "بند", "type": "purchases", "tables": [] } ] } ] }""");

        Assert.Equal(("", 0), (refusal, file.Report.Number));
        Assert.Empty(file.Accounts);
        Assert.Equal("بند", file.Categories.Single().Items.Single().Name);
    }

    [Fact]
    public async Task D7_AnEntryRecordedForAProjectFileTellsNoDeciderWhileAddEntryTellsThem()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (recorder, recorderId) = await ledger.MemberAsync("Finance");
        var typed = ledger.Purchase(100) with { Submit = true };
        var loaded = ledger.Purchase(200) with { Submit = true };

        (await ledger.PostAsync(typed, recorder)).EnsureSuccessStatusCode();
        await factory.WithDatabaseAsync(async db =>
        {
            var held = (await UserAccess.OfAsync(recorderId, db)).In(ledger.ProjectId);
            var table = (await EntryTable.OfferedAsync(ledger.ProjectId, held, db)).Single(row => row.Id == loaded.TableId);
            var (_, code, _, entry) = await EntryRecording.RecordAsync(ledger.ProjectId, loaded, table, recorderId, held, Entry.None, tellDeciders: false, db, factory.Time);
            Assert.Equal(("", EntryStatus.PendingApproval), (code, entry.Status));
        });

        var told = await ledger.ReadAsync(db => db.Notifications.Where(row => row.ProjectId == ledger.ProjectId && row.Kind == NotificationKind.EntrySubmitted).SelectMany(row => row.Entries).Select(row => row.EntryId).ToListAsync());
        Assert.Equal([typed.Id], told);
        Assert.Equal(1, await ledger.ReadAsync(db => db.EntryDecisions.CountAsync(row => row.EntryId == loaded.Id && row.Action == EntryDecisionAction.Submit)));
    }

    [Fact]
    public async Task D7_AnEntryRecordedForAProjectFilePassesTheChecksOfAddEntry()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var future = ledger.Purchase(200) with { Date = ledger.Today.AddDays(1), Submit = true };

        await factory.WithDatabaseAsync(async db =>
        {
            var userId = await ManagerOfAsync(ledger.ProjectId, db);
            var held = (await UserAccess.OfAsync(userId, db)).In(ledger.ProjectId);
            var table = (await EntryTable.OfferedAsync(ledger.ProjectId, held, db)).Single(row => row.Id == future.TableId);
            var (status, code, _, entry) = await EntryRecording.RecordAsync(ledger.ProjectId, future, table, userId, held, Entry.None, tellDeciders: false, db, factory.Time);
            Assert.Equal((400, "InvalidDate", Entry.None), (status, code, entry));
        });

        Assert.False(await ledger.ReadAsync(db => db.Entries.AnyAsync(row => row.Id == future.Id)));
    }

    [Fact]
    public async Task D9_AReportIsIssuedWithTheNumberAndTheTimeItIsGivenAndTheNextOneFollowsIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var fileTime = new DateTime(2026, 11, 2, 1, 40, 0, DateTimeKind.Utc);

        await factory.WithDatabaseAsync(async db => await PeriodicReportsEndpoints.IssueAsync(ledger.ProjectId, 28, fileTime, await ManagerOfAsync(ledger.ProjectId, db), db));

        var issued = (await ledger.Client.GetFromJsonAsync<List<PeriodicReportRowView>>($"/projects/{ledger.ProjectId}/periodic-reports", Ledger.Json))!;
        Assert.Equal((28, fileTime), (issued.Single().Number, issued.Single().IssuedAt));
        Assert.Equal(29, (await ledger.Client.GetFromJsonAsync<NextReportView>($"/projects/{ledger.ProjectId}/periodic-reports/next", Ledger.Json))!.Number);
        var next = await (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/periodic-reports", null)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<PeriodicReportRowView>(Ledger.Json);
        Assert.Equal((29, factory.Time.GetUtcNow().UtcDateTime), (next!.Number, next.IssuedAt));
    }

    /// <summary>The project's first manager, who registered it and holds <see cref="Permission.ManageProject"/> (D1).</summary>
    private static Task<Guid> ManagerOfAsync(Guid projectId, RedixDbContext db) =>
        db.ProjectPermissions.Where(row => row.ProjectId == projectId && row.Permission == Permission.ManageProject).OrderBy(row => row.Id).Select(row => row.UserId).FirstAsync();
}
