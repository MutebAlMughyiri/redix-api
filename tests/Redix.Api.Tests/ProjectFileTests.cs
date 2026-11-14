using Redix.Api.Import;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>UC-071, FR-53: a project's file, read and loaded into REDIX (redix-api #132). Every file here is made up for the test.</summary>
public class ProjectFileTests
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
}
