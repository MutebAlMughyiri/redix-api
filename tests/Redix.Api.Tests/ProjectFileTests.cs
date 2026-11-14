using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.History;
using Redix.Api.Import;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Reports;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

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

    /// <summary>The project's first manager, who registered it and holds <see cref="Permission.ManageProject"/> (D1), as its History says.</summary>
    private static Task<Guid> ManagerOfAsync(Guid projectId, RedixDbContext db) =>
        db.History.Where(row => row.RecordId == projectId && row.Action == HistoryAction.RegisterProject).Select(row => row.UserId).FirstAsync();

    /// <summary>The day every row of <see cref="LoadedFile"/> is dated: inside the project's period, which starts on 1 Nov 2026, and before today.</summary>
    private const string RowDate = "2026-11-02";

    /// <summary>The lists of <see cref="LoadedFile"/>: the ledger's account and one it lacks, a payment method and a supplier it has and one of each it lacks, and the lists of their own.</summary>
    private static readonly object[] Lists =
    [
        ListOf("account", "الحساب", "حساب المؤسسة", "حساب غير موجود"),
        ListOf("paymentMethod", "طريقة الدفع", "شبكة", "طريقة الاختبار"),
        ListOf("supplier", "المورد", "شركة النخيل", "مورد الاختبار"),
        ListOf("entryType", "نوع القيد", "شراء", "مرتجع"),
        ListOf("movementType", "نوع الحركة", "إضافة مبلغ", "سحب مبلغ", "إضافة"),
        ListOf("outsideContract", "الاتفاق", "داخل الاتفاق", "خارج الاتفاق"),
    ];

    private static object ListOf(string key, string name, params string[] labels) =>
        new { key, name, options = labels.Select((label, index) => new { id = $"{key}-{index + 1}", label, sign = "" }) };

    /// <summary>A table of a project file: its columns are its rows' keys, each typed as the workbook types it.</summary>
    private static object TableOf(string name, string kind, Dictionary<string, object>[] rows, decimal contractAmount = 0)
    {
        var keys = rows.SelectMany(row => row.Keys).Distinct().ToList();
        var columns = keys.Select(key => key switch
        {
            "supplier" or "account" or "paymentMethod" or "entryType" or "movementType" or "outsideContract" => new { key, name = key, type = "list", list = key },
            "date" => new { key, name = key, type = "date", list = "" },
            "total" or "amount" or "unitPrice" => new { key, name = key, type = "amount", list = "" },
            "quantity" => new { key, name = key, type = "number", list = "" },
            "attachment" => new { key, name = key, type = "file", list = "" },
            _ => new { key, name = key, type = "text", list = "" },
        });
        return new { name, kind, columns, rows, contractAmount = contractAmount == 0 ? (object)"" : contractAmount };
    }

    /// <summary>
    /// A project file made up for the test: report 7 and what the ledger's account really held; in the template's التشطيب › بلاط a purchase, a credit note, a new sub-table and a new
    /// contract; a new category with a new item; a new partner of رأس المال putting in 1,000 and taking out 200; and a VAT refund of 15. Its expenses are 469.75, its revenue 815, its
    /// surplus 345.25. <paramref name="change"/> changes a row before it is written, by its table's name and its place; <paramref name="parts"/> change the rest.
    /// </summary>
    private static string LoadedFile(Action<string, int, Dictionary<string, object>> change, FileParts parts)
    {
        Dictionary<string, object> Purchase(string description, string supplier, string entryType, string method, decimal total) =>
            new() { ["description"] = description, ["supplier"] = supplier, ["date"] = RowDate, ["entryType"] = entryType, ["account"] = "account-1", ["paymentMethod"] = method, ["attachment"] = "", ["total"] = total };
        var tables = new Dictionary<string, Dictionary<string, object>[]>
        {
            ["بلاط"] = [Purchase("بلاط أرضي", "supplier-1", "entryType-1", "paymentMethod-1", 100), Purchase("بلاط مرتجع", "supplier-1", "entryType-2", "paymentMethod-1", 20.25m)],
            ["مواد الاختبار"] = [new(Purchase("مواد", "supplier-2", "entryType-1", "paymentMethod-2", 50)) { ["unitPrice"] = 10, ["quantity"] = 5 }],
            ["دفعات مقاول الاختبار"] = [new() { ["description"] = "دفعة أولى", ["date"] = RowDate, ["outsideContract"] = "outsideContract-1", ["account"] = "account-1", ["paymentMethod"] = "paymentMethod-1", ["attachment"] = "", ["amount"] = 300 }],
            ["بند الاختبار"] = [Purchase("صنف", "supplier-1", "entryType-1", "paymentMethod-1", 40)],
            ["شريك الاختبار"] =
            [
                new() { ["movementType"] = "movementType-1", ["description"] = "إيداع", ["date"] = RowDate, ["account"] = "account-1", ["attachment"] = "", ["amount"] = 1000 },
                new() { ["movementType"] = "movementType-2", ["description"] = "سحب", ["date"] = RowDate, ["account"] = "account-1", ["attachment"] = "", ["amount"] = 200 },
            ],
            ["استرجاع الضرائب"] = [new() { ["description"] = "استرجاع", ["date"] = RowDate, ["account"] = "account-1", ["amount"] = 15 }],
        };
        foreach (var name in parts.WithRows ? [] : tables.Keys.ToList())
        {
            tables[name] = [];
        }
        foreach (var (name, rows) in tables)
        {
            foreach (var (row, index) in rows.Select((row, index) => (row, index + 1)))
            {
                change(name, index, row);
            }
        }
        object[] categories =
        [
            new { name = "التشطيب", items = new[] { new { name = "بلاط", type = "both", tables = new[]
            {
                TableOf("بلاط", "purchases", tables["بلاط"]), TableOf("مواد الاختبار", "purchases", tables["مواد الاختبار"]),
                TableOf("دفعات مقاول الاختبار", "contractorPayments", tables["دفعات مقاول الاختبار"], 1000),
            } } } },
            new { name = "فئة الاختبار", items = new[] { new { name = "بند الاختبار", type = "purchases", tables = new[] { TableOf("بند الاختبار", "purchases", tables["بند الاختبار"]) } } } },
            new { name = "رأس المال", items = new[] { new { name = "رأس المال", type = "capital", tables = new[] { TableOf("شريك الاختبار", "capital", tables["شريك الاختبار"]) } } } },
            new { name = "استرجاع ضرائب", items = new[] { new { name = "استرجاع الضرائب", type = "vatRefund", tables = new[] { TableOf("استرجاع الضرائب", "vatRefund", tables["استرجاع الضرائب"]) } } } },
            .. parts.MoreCategories,
        ];
        return JsonSerializer.Serialize(new
        {
            format = "redix.project",
            version = 1,
            project = "مشروع الاختبار",
            report = parts.Report,
            accounts = parts.Accounts,
            lists = Lists,
            categories,
        });
    }

    /// <summary><see cref="LoadedFile"/> with nothing changed.</summary>
    private static string LoadedFile() => LoadedFile((_, _, _) => { }, Parts);

    private static string LoadedFile(Action<string, int, Dictionary<string, object>> change) => LoadedFile(change, Parts);

    private static string LoadedFile(FileParts parts) => LoadedFile((_, _, _) => { }, parts);

    /// <summary>What <see cref="LoadedFile"/> holds besides its rows: report 7, what the ledger's account really held, nothing more, and its rows.</summary>
    private static readonly FileParts Parts = new(new { number = 7, date = "2026-11-10", time = "09:30" }, [new { name = "حساب المؤسسة", actualBalance = 345.25m }], [], true);

    /// <summary>The parts of a made-up project file a test may change.</summary>
    private sealed record FileParts(object Report, object[] Accounts, object[] MoreCategories, bool WithRows);

    /// <summary>When <see cref="LoadedFile"/>'s report 7 was issued: 10 Nov 2026 at 09:30 in Saudi Arabia.</summary>
    private static readonly DateTime ReportTime = new(2026, 11, 10, 6, 30, 0, DateTimeKind.Utc);

    /// <summary>Sends <paramref name="text"/> as the app sends a picked file, to load it into the project <paramref name="projectId"/>.</summary>
    private static Task<HttpResponseMessage> LoadAsync(HttpClient client, Guid projectId, string text) =>
        client.PostAsync($"/projects/{projectId}/project-file", new MultipartFormDataContent { { new StringContent(text), "file", "project.json" } });

    private static async Task<List<ProjectFileRefusal>> RefusedAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var answer = await response.Content.ReadFromJsonAsync<JsonElement>(Ledger.Json);
        Assert.Equal("rowsRefused", answer.GetProperty("code").GetString());
        return answer.GetProperty("rows").Deserialize<List<ProjectFileRefusal>>(Ledger.Json)!;
    }

    /// <summary>Whether anything of a load stayed in the project: an entry, the file's new category, its new supplier, a report or a reconciliation.</summary>
    private Task<bool> AnythingSavedAsync(Ledger ledger) => ledger.ReadAsync(async db =>
        await db.Entries.AnyAsync(entry => entry.ProjectId == ledger.ProjectId)
        || await db.Categories.AnyAsync(category => category.ProjectId == ledger.ProjectId && category.Name == "فئة الاختبار")
        || await db.Suppliers.AnyAsync(supplier => supplier.ProjectId == ledger.ProjectId && supplier.Name == "مورد الاختبار")
        || await db.PeriodicReports.AnyAsync(report => report.ProjectId == ledger.ProjectId)
        || await db.Reconciliations.AnyAsync(reconciliation => reconciliation.ProjectId == ledger.ProjectId));

    [Fact]
    public async Task AC_53_1_D5_D6_D7_D13_AFileLoadsItsMissingStructureAndEveryRowApprovedInTheManagersNameTellingNoDecider()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, deciderId) = await ledger.MemberAsync("Finance");

        var response = await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile());

        Assert.Equal(new ProjectFileLoadedView(8, 6, 7), await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectFileLoadedView>(Ledger.Json));
        await factory.WithDatabaseAsync(async db =>
        {
            var managerId = await ManagerOfAsync(ledger.ProjectId, db);
            var entries = await db.Entries.Where(entry => entry.ProjectId == ledger.ProjectId).ToListAsync();
            Assert.Equal(8, entries.Count);
            Assert.All(entries, entry => Assert.Equal((EntryStatus.Approved, managerId), (entry.Status, entry.CreatedByUserId)));
            var categories = await db.Categories.Where(category => category.ProjectId == ledger.ProjectId).OrderBy(category => category.SortOrder).Select(category => category.Name).ToListAsync();
            Assert.Equal("فئة الاختبار", categories[^1]);
            Assert.True(await db.Subgroups.AnyAsync(subgroup => subgroup.Name == "مواد الاختبار" && db.Items.Any(item => item.Id == subgroup.ItemId && item.Name == "بلاط" && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == ledger.ProjectId))));
            var contract = await (from row in db.Contracts join supplier in db.Suppliers on row.SupplierId equals supplier.Id where row.ProjectId == ledger.ProjectId select new { supplier.Name, row.Value, row.IsOpenValue }).SingleAsync();
            Assert.Equal(("مقاول الاختبار", 1000m, false), (contract.Name, contract.Value, contract.IsOpenValue));
            Assert.True(await db.Partners.AnyAsync(partner => partner.ProjectId == ledger.ProjectId && partner.Name == "شريك الاختبار"));
            Assert.True(await db.Suppliers.AnyAsync(supplier => supplier.ProjectId == ledger.ProjectId && supplier.Name == "مورد الاختبار" && supplier.Status == SupplierStatus.Approved));
            Assert.True(await db.PaymentMethods.AnyAsync(method => method.ProjectId == ledger.ProjectId && method.Name == "طريقة الاختبار"));
            Assert.False(await db.Notifications.AnyAsync(told => told.ProjectId == ledger.ProjectId && told.UserId == deciderId));
        });
    }

    [Fact]
    public async Task AC_53_6_AfterTheLoadTheSummaryItsRevenueItsExpensesAndItsSurplusEqualTheFiles()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        (await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile())).EnsureSuccessStatusCode();

        var summary = (await ledger.Client.GetFromJsonAsync<SummaryView>($"/projects/{ledger.ProjectId}/summary", Ledger.Json))!;
        Assert.Equal((815m, 469.75m, 345.25m), (summary.Revenue, summary.Expenses, summary.Surplus));
        Assert.Equal(40m, summary.Categories.Single(category => category.Name == "فئة الاختبار").Total);
        Assert.Equal(429.75m, summary.Categories.Single(category => category.Name == "التشطيب").Total);
        Assert.Equal(800m, summary.RevenueCategories.Single(category => category.Name == "رأس المال").Total);
    }

    [Fact]
    public async Task AC_53_7_AC_53_8_D9_TheFilesReportIsIssuedWithItsNumberAndTimeAndItsActualBalancesAreTheLatestReconciliation()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        (await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile())).EnsureSuccessStatusCode();

        var issued = (await ledger.Client.GetFromJsonAsync<List<PeriodicReportRowView>>($"/projects/{ledger.ProjectId}/periodic-reports", Ledger.Json))!;
        Assert.Equal((7, ReportTime, 469.75m), (issued.Single().Number, issued.Single().IssuedAt, issued.Single().Expenses));
        Assert.Equal(8, (await ledger.Client.GetFromJsonAsync<NextReportView>($"/projects/{ledger.ProjectId}/periodic-reports/next", Ledger.Json))!.Number);
        var reconciliation = (await ledger.Client.GetFromJsonAsync<ReconciliationView>($"/projects/{ledger.ProjectId}/reconciliations/latest", Ledger.Json))!;
        Assert.Equal((345.25m, 0m), (reconciliation.Balances.Single().ActualBalance, reconciliation.Difference));
    }

    [Fact]
    public async Task D9_D2_AFileWithNoReportNoAccountsAndNoRowsLoadsItsStructureAloneAndIssuesNothing()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var text = LoadedFile(Parts with { Report = new { number = 0, date = "", time = "" }, Accounts = [], WithRows = false });

        var response = await LoadAsync(ledger.Client, ledger.ProjectId, text);

        Assert.Equal(new ProjectFileLoadedView(0, 0, 0), await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectFileLoadedView>(Ledger.Json));
        Assert.True(await ledger.ReadAsync(db => db.Categories.AnyAsync(category => category.ProjectId == ledger.ProjectId && category.Name == "فئة الاختبار")));
        Assert.False(await ledger.ReadAsync(db => db.PeriodicReports.AnyAsync(report => report.ProjectId == ledger.ProjectId)));
        Assert.False(await ledger.ReadAsync(db => db.Reconciliations.AnyAsync(reconciliation => reconciliation.ProjectId == ledger.ProjectId)));
    }

    [Fact]
    public async Task D12_G7_NoLoadedEntryShowsAsNewWhileOneApprovedAfterTheLoadDoes()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile())).EnsureSuccessStatusCode();
        var tiles = await ledger.ReadAsync(db => db.Items.Where(item => item.Name == "بلاط" && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == ledger.ProjectId && category.Name == "التشطيب")).Select(item => new { item.Id, General = item.Subgroups.Single(subgroup => subgroup.Name == "").Id }).SingleAsync());

        await ledger.ApproveAsync(await ledger.PostAsync(ledger.Purchase(75) with { ItemId = tiles.Id, TableId = tiles.General, Submit = true }));

        var report = (await ledger.Client.GetFromJsonAsync<ItemReportView>($"/projects/{ledger.ProjectId}/reports/items/{tiles.Id}", Ledger.Json))!;
        var rows = report.Subgroups.SelectMany(subgroup => subgroup.Rows).Concat(report.Contracts.SelectMany(contract => contract.Payments)).ToList();
        Assert.Equal([75m], rows.Where(row => row.IsNew).Select(row => row.Amount));
        Assert.Equal(4, rows.Count(row => !row.IsNew));
        Assert.All(await ledger.ReadAsync(db => db.Entries.Where(entry => entry.ProjectId == ledger.ProjectId && entry.Amount != 75).Select(entry => entry.ApprovedAt).ToListAsync()), approvedAt => Assert.Equal(ReportTime, approvedAt));
    }

    [Fact]
    public async Task AC_53_2_WithoutManageProjectTheRouteAnswers403AndNothingIsSaved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await ledger.MemberAsync("Field");

        Assert.Equal(HttpStatusCode.Forbidden, (await LoadAsync(field, ledger.ProjectId, LoadedFile())).StatusCode);
        Assert.False(await AnythingSavedAsync(ledger));
    }

    [Fact]
    public async Task AC_53_3_D15_AProjectHoldingAnEntrySaysSoAndRefusesTheFileWithProjectHasEntries()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        async Task<bool> HoldsEntriesAsync() => (await ledger.Client.GetFromJsonAsync<List<ProjectView>>("/projects", Ledger.Json))!.Single(project => project.Id == ledger.ProjectId).HoldsEntries;
        Assert.False(await HoldsEntriesAsync());

        (await ledger.PostAsync(ledger.Purchase(10))).EnsureSuccessStatusCode();

        Assert.True(await HoldsEntriesAsync());
        var response = await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile());
        Assert.Equal((HttpStatusCode.Conflict, "projectHasEntries"), (response.StatusCode, await Ledger.CodeOf(response)));
        Assert.Equal(1, await ledger.ReadAsync(db => db.Entries.CountAsync(entry => entry.ProjectId == ledger.ProjectId)));
    }

    [Fact]
    public async Task AC_53_4_AC_53_5_G3_ARowAddEntryRefusesKeepsItsCodeEveryRefusedRowIsListedAndNothingIsSaved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var tomorrow = ledger.Today.AddDays(1).ToString("yyyy-MM-dd");
        var text = LoadedFile((table, row, values) =>
        {
            if (table == "بلاط" && row == 2) values["date"] = tomorrow;
            if (table == "بند الاختبار") values["account"] = "account-2";
            if (table == "شريك الاختبار" && row == 1) values["attachment"] = "فتح المرفق";
        });

        var refused = await RefusedAsync(await LoadAsync(ledger.Client, ledger.ProjectId, text));

        Assert.Equal(
        [
            ("بلاط", 2, ProjectFileOwner.Entry, "invalidDate", ""),
            ("بند الاختبار", 1, ProjectFileOwner.File, "accountNotInProject", "حساب غير موجود"),
            ("شريك الاختبار", 1, ProjectFileOwner.File, "attachmentNotInFile", "فتح المرفق"),
        ], refused.Select(row => (row.Table, row.Row, row.Owner, row.Code, row.Value)));
        Assert.False(await AnythingSavedAsync(ledger));
    }

    [Fact]
    public async Task G4_AnOptionTheFilesListsDoNotHoldOrAChoiceTheTablesListDoesNotHaveIsAnUnknownChoiceNamingItsColumn()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var text = LoadedFile((table, row, values) =>
        {
            if (table == "بلاط" && row == 1) values["entryType"] = "entryType-9";
            if (table == "شريك الاختبار" && row == 2) values["movementType"] = "movementType-3";
        });

        var refused = await RefusedAsync(await LoadAsync(ledger.Client, ledger.ProjectId, text));

        Assert.Equal(
        [
            ("بلاط", 1, "unknownChoice", "entryType-9", "entryType"),
            ("شريك الاختبار", 2, "unknownChoice", "إضافة", "movementType"),
        ], refused.Select(row => (row.Table, row.Row, row.Code, row.Value, row.Column)));
        Assert.False(await AnythingSavedAsync(ledger));
    }

    [Fact]
    public async Task D14_APartItsOwnerRefusesIsListedWithItsOwnersCodeAndNothingIsSaved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        object[] secondCapital = [new { name = "رأس المال", items = new[] { new { name = "رأس مال آخر", type = "capital", tables = Array.Empty<object>() } } }];

        var refused = await RefusedAsync(await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile(Parts with { MoreCategories = secondCapital })));

        Assert.Equal(("رأس المال", "رأس مال آخر", 0, ProjectFileOwner.Structure, "capitalItemExists"), (refused.Single().Category, refused.Single().Item, refused.Single().Row, refused.Single().Owner, refused.Single().Code));
        Assert.False(await AnythingSavedAsync(ledger));
    }

    [Fact]
    public async Task AC_53_8_D6_AnAccountOfTheActualBalancesTheProjectLacksIsRefusedAndNothingIsSaved()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var refused = await RefusedAsync(await LoadAsync(ledger.Client, ledger.ProjectId, LoadedFile(Parts with { Accounts = [new { name = "حساب المؤسسة", actualBalance = 300m }, new { name = "حساب غير موجود", actualBalance = 45.25m }] })));

        Assert.Equal((ProjectFileOwner.File, "accountNotInProject", "حساب غير موجود"), (refused.Single().Owner, refused.Single().Code, refused.Single().Value));
        Assert.False(await AnythingSavedAsync(ledger));
    }

    [Theory]
    [InlineData("""{ "format": "another.app", "version": 1 }""", "notAProjectFile")]
    [InlineData("""{ "format": "redix.project", "version": 2 }""", "newerVersion")]
    public async Task AC_53_9_TheRouteRefusesAFileThatIsNotAProjectFileOrIsNewerAndNothingChanges(string text, string code)
    {
        var ledger = await Ledger.WithFinanceAsync(factory);

        var response = await LoadAsync(ledger.Client, ledger.ProjectId, text);

        Assert.Equal((HttpStatusCode.BadRequest, code), (response.StatusCode, await Ledger.CodeOf(response)));
        Assert.False(await AnythingSavedAsync(ledger));
    }
}
