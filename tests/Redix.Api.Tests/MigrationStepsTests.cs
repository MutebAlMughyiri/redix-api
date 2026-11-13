using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>
/// Each migration step after the baseline carries the rows a database holds today into its new shape: a scratch database is migrated to the step before, given rows as they are
/// stored today, then migrated through the step, and read with SQL alone, since the model is that of the last step.
/// </summary>
[Collection(FourthDatabaseCollection.Name)]
public class MigrationStepsTests(MigratedApiFactory factory)
{
    /// <summary>Writes <paramref name="today"/> on a database migrated to the step before <paramref name="step"/>, migrates through <paramref name="step"/>, and lets <paramref name="read"/> read it.</summary>
    private Task ThroughAsync(string step, string today, Func<RedixDbContext, Task> read) =>
        factory.OnScratchDatabaseAsync("RedixMigrationStepTests", async db =>
        {
            var migrator = db.GetService<IMigrator>();
            var steps = db.Database.GetMigrations().ToList();
            var index = steps.FindIndex(name => name.EndsWith($"_{step}"));
            await migrator.MigrateAsync(steps[index - 1]);
            await db.Database.ExecuteSqlRawAsync(today);
            await migrator.MigrateAsync(steps[index]);
            await read(db);
        });

    private static Task<List<string>> ValuesAsync(RedixDbContext db, string sql) => db.Database.SqlQueryRaw<string>(sql).ToListAsync();

    /// <summary>A login as it was stored before redix-api #126, active or disabled as a whole.</summary>
    private static string LoginWithIsActive(Guid id, bool isActive) =>
        $"INSERT INTO Users (Id, Username, DisplayName, Mobile, PasswordHash, MustChangePassword, IsActive, FailedSignIns, LockedUntil, CreatedAt) VALUES ('{id}', '{id:N}', N'', '', '', 0, {(isActive ? 1 : 0)}, 0, '1900-01-01', '2026-11-01');";

    private static string User(Guid id) =>
        $"INSERT INTO Users (Id, Username, DisplayName, Mobile, PasswordHash, MustChangePassword, FailedSignIns, LockedUntil, CreatedAt) VALUES ('{id}', '{id:N}', N'', '', '', 0, 0, '1900-01-01', '2026-11-01');";

    /// <summary>A project as it was stored before findings row 214, naming the project its structure was copied from in a column of its own.</summary>
    private static string ProjectWithStructureSourceId(Guid id, Guid structureSourceId = default) =>
        $"INSERT INTO Projects (Id, Name, StartDate, Status, RejectionReason, Description, IsTemplate, StructureSourceId) VALUES ('{id}', N'غصون', '2026-11-01', 0, N'', N'', 0, '{structureSourceId}');";

    private static string Project(Guid id) =>
        $"INSERT INTO Projects (Id, Name, StartDate, Status, RejectionReason, Description, IsTemplate) VALUES ('{id}', N'غصون', '2026-11-01', 0, N'', N'', 0);";

    private static string Notification(Guid id, Guid userId, Guid projectId, Guid entryId = default) =>
        $"INSERT INTO Notifications (Id, UserId, ProjectId, EntryId, Kind, At, IsRead) VALUES ('{id}', '{userId}', '{projectId}', '{entryId}', 0, '2026-11-01', 0);";

    private static string Account(Guid id, Guid projectId) =>
        $"INSERT INTO Accounts (Id, ProjectId, Type, Name, IsActive) VALUES ('{id}', '{projectId}', 0, N'حساب', 1);";

    private static string Entry(Guid id, Guid projectId, Guid accountId, Guid userId) =>
        $"INSERT INTO Entries (Id, ProjectId, Kind, Status, EntryDate, Amount, AccountId, Notes, CreatedByUserId, CreatedAt, ApprovedAt) VALUES ('{id}', '{projectId}', 0, 0, '2026-11-01', 100, '{accountId}', N'', '{userId}', '2026-11-01', '1900-01-01');";

    private static string Partner(Guid id, Guid projectId, Guid userId, bool isDeleted = false) =>
        $"INSERT INTO Partners (Id, ProjectId, UserId, Name, ItemId, SortOrder, IsDeleted) VALUES ('{id}', '{projectId}', '{userId}', N'شريك', '{Guid.Empty}', 1, {(isDeleted ? 1 : 0)});";

    private static string Reconciliation(Guid id, Guid projectId, Guid userId) =>
        $"INSERT INTO Reconciliations (Id, ProjectId, At, UserId) VALUES ('{id}', '{projectId}', '2026-11-01', '{userId}');";

    /// <summary>An item of <paramref name="type"/> in a category of its own, with its general group.</summary>
    private static string ItemWithGroup(Guid projectId, Guid itemId, Guid subgroupId, int type) =>
        $"INSERT INTO Categories (Id, ProjectId, Name, SortOrder, IsActive) VALUES ('{itemId}', '{projectId}', N'تصنيف', 0, 1);" +
        $"INSERT INTO Items (Id, CategoryId, Name, Type, SortOrder, IsActive, ShowsTablePercentages) VALUES ('{itemId}', '{itemId}', N'بند', {type}, 0, 1, 0);" +
        $"INSERT INTO Subgroups (Id, ItemId, Name, SortOrder) VALUES ('{subgroupId}', '{itemId}', N'', 0);";

    private static string PaymentMethod(Guid id, Guid projectId) =>
        $"INSERT INTO PaymentMethods (Id, ProjectId, Name, SortOrder, IsActive) VALUES ('{id}', '{projectId}', N'شبكة', 0, 1);";

    private static string Supplier(Guid id, Guid projectId, Guid userId) =>
        $"INSERT INTO Suppliers (Id, ProjectId, Name, VatNumber, Mobile, Status, IsActive, CreatedByUserId, IsGeneralCash, NormalizedName) VALUES ('{id}', '{projectId}', N'مورد', '', '', 1, 1, '{userId}', 0, N'مورد');";

    private static string EntryOf(Guid id, Guid projectId, Guid accountId, Guid userId, int kind, decimal amount) =>
        $"INSERT INTO Entries (Id, ProjectId, Kind, Status, EntryDate, Amount, AccountId, Notes, CreatedByUserId, CreatedAt, ApprovedAt) VALUES ('{id}', '{projectId}', {kind}, 2, '2026-11-01', {amount.ToString(CultureInfo.InvariantCulture)}, '{accountId}', N'', '{userId}', '2026-11-01', '2026-11-02');";

    private static Task<List<string>> ColumnValueAsync(RedixDbContext db, Guid entryId, string key) =>
        ValuesAsync(db, $"SELECT LOWER(JSON_VALUE(CAST(ColumnValues AS nvarchar(max)), '$.\"{key}\"')) AS Value FROM Entries WHERE Id = '{entryId}'");

    private static string Held(Guid projectId, Guid userId, string permission = "ReadProject") =>
        $"INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission) VALUES (NEWID(), '{projectId}', '{userId}', '{permission}');";

    [Fact]
    public Task R126_InactivePerProject_ADisabledLoginBecomesInactiveInEveryProjectHeHoldsAPermissionInAndTheLoginWideSwitchGoes()
    {
        var (disabled, active, first, second) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(LoginWithIsActive(disabled, isActive: false), LoginWithIsActive(active, isActive: true), ProjectWithStructureSourceId(first), ProjectWithStructureSourceId(second),
            Held(first, disabled), Held(first, disabled, "RecordEntries"), Held(second, disabled), Held(first, active));

        return ThroughAsync("InactivePerProject", today, async db =>
        {
            Assert.Equal(new[] { $"{first} {disabled}", $"{second} {disabled}" }.Order(), (await ValuesAsync(db, "SELECT LOWER(CONCAT(ProjectId, ' ', UserId)) AS Value FROM InactiveMembers")).Order());
            Assert.Empty(await ValuesAsync(db, "SELECT name AS Value FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsActive'"));
        });
    }

    [Fact]
    public Task R213_NotificationProjectKey_EveryNotificationKeepsItsProjectWhichTheDatabaseNowNames()
    {
        var (user, project, notification) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

        return ThroughAsync("NotificationProjectKey", string.Concat(User(user), ProjectWithStructureSourceId(project), Notification(notification, user, project)), async db =>
        {
            Assert.Equal([$"{notification} {project}"], await ValuesAsync(db, "SELECT LOWER(CONCAT(Id, ' ', ProjectId)) AS Value FROM Notifications"));
            Assert.Equal(["FK_Notifications_Projects_ProjectId"], await ValuesAsync(db, "SELECT name AS Value FROM sys.foreign_keys WHERE name = 'FK_Notifications_Projects_ProjectId'"));
        });
    }

    [Fact]
    public Task R214_LinkTablesForOptionalIds_AnIdThatNamesARowBecomesARowAndAnEmptyOneOrOneNamingNothingNone()
    {
        var (user, project, copied, copiedFromNothing, account, entry) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var (withLogin, withoutLogin, deletedWithLogin) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var (aboutEntry, aboutProject, aboutNoEntry) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(User(user), ProjectWithStructureSourceId(project), ProjectWithStructureSourceId(copied, project), ProjectWithStructureSourceId(copiedFromNothing, Guid.CreateVersion7()), Account(account, project), Entry(entry, project, account, user),
            Partner(withLogin, project, user), Partner(withoutLogin, project, Guid.Empty), Partner(deletedWithLogin, project, user, isDeleted: true),
            Notification(aboutEntry, user, project, entry), Notification(aboutProject, user, project), Notification(aboutNoEntry, user, project, Guid.CreateVersion7()));

        return ThroughAsync("LinkTablesForOptionalIds", today, async db =>
        {
            Assert.Equal([$"{withLogin} {project} {user}"], await ValuesAsync(db, "SELECT LOWER(CONCAT(PartnerId, ' ', ProjectId, ' ', UserId)) AS Value FROM PartnerUsers"));
            Assert.Equal([$"{aboutEntry} {entry}"], await ValuesAsync(db, "SELECT LOWER(CONCAT(NotificationId, ' ', EntryId)) AS Value FROM NotificationEntries"));
            Assert.Equal([$"{copied} {project}"], await ValuesAsync(db, "SELECT LOWER(CONCAT(ProjectId, ' ', StructureSourceId)) AS Value FROM ProjectStructureSources"));
            Assert.Equal(3, (await ValuesAsync(db, "SELECT CAST(Id AS nvarchar(36)) AS Value FROM Partners")).Count);
            Assert.Equal(3, (await ValuesAsync(db, "SELECT CAST(Id AS nvarchar(36)) AS Value FROM Notifications")).Count);
            Assert.Empty(await ValuesAsync(db, """
                SELECT CONCAT(OBJECT_NAME(object_id), '.', name) AS Value FROM sys.columns
                WHERE (object_id = OBJECT_ID('Partners') AND name = 'UserId') OR (object_id = OBJECT_ID('Notifications') AND name = 'EntryId') OR (object_id = OBJECT_ID('Projects') AND name = 'StructureSourceId')
                """));
        });
    }

    [Fact]
    public Task R215_ReconciliationBalancesPlural_EveryBalanceStaysInTheTableNamedInThePlural()
    {
        var (user, project, account, reconciliation) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(User(user), Project(project), Account(account, project), Reconciliation(reconciliation, project, user),
            $"INSERT INTO ReconciliationBalance (ReconciliationId, AccountId, ActualBalance) VALUES ('{reconciliation}', '{account}', 700.25);");

        return ThroughAsync("ReconciliationBalancesPlural", today, async db =>
        {
            Assert.Equal([$"{reconciliation} {account} 700.2500"], await ValuesAsync(db, "SELECT LOWER(CONCAT(ReconciliationId, ' ', AccountId, ' ', ActualBalance)) AS Value FROM ReconciliationBalances"));
            Assert.Empty(await ValuesAsync(db, "SELECT name AS Value FROM sys.tables WHERE name = 'ReconciliationBalance'"));
        });
    }

    [Fact]
    public Task R129_ListChoicesWithSigns_EveryChoiceGetsAnIdAndNoSignAndEveryValueNamingOneHoldsItsId()
    {
        var (user, project, account, item, subgroup, method, entry, column) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(User(user), Project(project), Account(account, project), ItemWithGroup(project, item, subgroup, 2), PaymentMethod(method, project), EntryOf(entry, project, account, user, 0, 100m),
            $"INSERT INTO ItemColumns (Id, ItemId, IsHidden, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Options) VALUES ('{column}', '{item}', 0, 'custom.direction', N'الجهة', 4, 0, 0, 1, 0, N'[\"شرق\",\"غرب\"]');",
            $"INSERT INTO ExpenseDetails (EntryId, ItemId, SubgroupId, PaymentMethodId, CustomValues) VALUES ('{entry}', '{item}', '{subgroup}', '{method}', N'{{\"custom.direction\":\"غرب\"}}');");

        return ThroughAsync("ListChoicesWithSigns", today, async db =>
        {
            Assert.Equal(["شرق 0", "غرب 0"], (await ValuesAsync(db, $"SELECT CONCAT(choice.Name, ' ', choice.Sign) AS Value FROM ItemColumns CROSS APPLY OPENJSON(CAST(Choices AS nvarchar(max))) WITH (Name nvarchar(100) '$.Name', Sign int '$.Sign') choice WHERE Id = '{column}'")).Order());
            var west = Assert.Single(await ValuesAsync(db, $"SELECT LOWER(JSON_VALUE(CAST(Choices AS nvarchar(max)), '$[1].Id')) AS Value FROM ItemColumns WHERE Id = '{column}'"));
            Assert.True(Guid.TryParse(west, out _));
            Assert.Equal([west], await ValuesAsync(db, $"SELECT LOWER(JSON_VALUE(CAST(CustomValues AS nvarchar(max)), '$.\"custom.direction\"')) AS Value FROM ExpenseDetails WHERE EntryId = '{entry}'"));
        });
    }

    [Fact]
    public Task R129_R6_ColumnValuesOnTheEntry_EveryValueTheProductThePriceAndTheQuantityMoveToTheEntryUnchangedAndTheSupplierIsOptional()
    {
        var (user, project, account, item, subgroup, method, supplier, entry) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(User(user), Project(project), Account(account, project), ItemWithGroup(project, item, subgroup, 2), PaymentMethod(method, project), Supplier(supplier, project, user), EntryOf(entry, project, account, user, 0, 1150m),
            $"INSERT INTO ItemColumns (Id, ItemId, IsHidden, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices) VALUES (NEWID(), '{item}', 0, 'supplier', N'المورد', 4, 1, 1, 0, 0, N'[]');",
            $"INSERT INTO ExpenseDetails (EntryId, ItemId, SubgroupId, PaymentMethodId, CustomValues) VALUES ('{entry}', '{item}', '{subgroup}', '{method}', N'{{\"custom.area\":\"125.5\"}}');",
            $"INSERT INTO PurchaseDetails (EntryId, SupplierId, Product, UnitPrice, Quantity) VALUES ('{entry}', '{supplier}', N'أسمنت', 230, 5);");

        return ThroughAsync("ColumnValuesOnTheEntry", today, async db =>
        {
            Assert.Equal(["125.5"], await ColumnValueAsync(db, entry, "custom.area"));
            Assert.Equal(["أسمنت"], await ColumnValueAsync(db, entry, "product"));
            Assert.Equal(["230"], await ColumnValueAsync(db, entry, "unitPrice"));
            Assert.Equal(["5"], await ColumnValueAsync(db, entry, "quantity"));
            Assert.Equal(["1150.0000"], await ValuesAsync(db, $"SELECT CONVERT(nvarchar(40), Amount) AS Value FROM Entries WHERE Id = '{entry}'"));
            Assert.Equal([$"{supplier}"], await ValuesAsync(db, $"SELECT LOWER(CONVERT(nvarchar(36), SupplierId)) AS Value FROM PurchaseDetails WHERE EntryId = '{entry}'"));
            Assert.Equal(["0"], await ValuesAsync(db, $"SELECT CONVERT(nvarchar(1), IsRequired) AS Value FROM ItemColumns WHERE ItemId = '{item}' AND [Key] = 'supplier'"));
            Assert.Empty(await ValuesAsync(db, """
                SELECT CONCAT(OBJECT_NAME(object_id), '.', name) AS Value FROM sys.columns
                WHERE (object_id = OBJECT_ID('PurchaseDetails') AND name IN ('Product', 'UnitPrice', 'Quantity')) OR (object_id = OBJECT_ID('ExpenseDetails') AND name = 'CustomValues')
                """));
        });
    }

    [Fact]
    public Task R129_EntryTypeAndMovementTypeLists_EveryEntryKeepsItsFigureAndTakesTheChoiceItsSignSays()
    {
        var (user, project, account, item, subgroup, method, capital, capitalGroup) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var (bought, returned, partner, takenOut) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var template = StructureSeed.TemplateId;
        var today = string.Concat(User(user), Project(project), Account(account, project), ItemWithGroup(project, item, subgroup, 2), ItemWithGroup(project, capital, capitalGroup, 6), PaymentMethod(method, project),
            $"INSERT INTO DefaultColumns (Id, ProjectId, ItemType, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices) SELECT NEWID(), '{project}', ItemType, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices FROM DefaultColumns WHERE ProjectId = '{template}';",
            $"INSERT INTO ItemColumns (Id, ItemId, IsHidden, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices) SELECT NEWID(), '{item}', 0, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices FROM DefaultColumns WHERE ProjectId = '{project}' AND ItemType = 2;",
            EntryOf(bought, project, account, user, 0, 1150m), EntryOf(returned, project, account, user, 0, -320m), EntryOf(takenOut, project, account, user, 3, 100m),
            $"INSERT INTO ExpenseDetails (EntryId, ItemId, SubgroupId, PaymentMethodId) VALUES ('{bought}', '{item}', '{subgroup}', '{method}'), ('{returned}', '{item}', '{subgroup}', '{method}');",
            $"INSERT INTO Partners (Id, ProjectId, Name, ItemId, SortOrder, IsDeleted) VALUES ('{partner}', '{project}', N'شريك', '{capital}', 1, 0);",
            $"INSERT INTO CapitalMovementDetails (EntryId, PartnerId, Type) VALUES ('{takenOut}', '{partner}', 1);");

        return ThroughAsync("EntryTypeAndMovementTypeLists", today, async db =>
        {
            Assert.Equal([$"{StructureSeed.PurchaseChoiceId}"], await ColumnValueAsync(db, bought, "entryType"));
            Assert.Equal([$"{StructureSeed.CreditNoteChoiceId}"], await ColumnValueAsync(db, returned, "entryType"));
            Assert.Equal([$"{StructureSeed.WithdrawalChoiceId}"], await ColumnValueAsync(db, takenOut, "movementType"));
            Assert.Equal(new[] { $"{bought} 0 1150.0000", $"{returned} 0 -320.0000", $"{takenOut} 3 100.0000" }.Order(),
                (await ValuesAsync(db, $"SELECT LOWER(CONCAT(Id, ' ', Kind, ' ', Amount)) AS Value FROM Entries WHERE ProjectId = '{project}'")).Order());
            Assert.Equal(["entryType"], await ValuesAsync(db, $"SELECT [Key] AS Value FROM ItemColumns WHERE ItemId = '{item}' AND [Key] = 'entryType' AND JSON_VALUE(CAST(Choices AS nvarchar(max)), '$[1].Name') = N'مرتجع'"));
            Assert.Equal(["movementType", "date", "account", "attachment", "amount"], await ValuesAsync(db, $"SELECT [Key] AS Value FROM ItemColumns WHERE ItemId = '{capital}' ORDER BY SortOrder"));
            Assert.Equal(await ValuesAsync(db, $"SELECT [Key] AS Value FROM DefaultColumns WHERE ProjectId = '{template}' AND ItemType = 2 ORDER BY SortOrder"),
                await ValuesAsync(db, $"SELECT [Key] AS Value FROM ItemColumns WHERE ItemId = '{item}' ORDER BY SortOrder"));
            Assert.Equal(["1"], await ValuesAsync(db, $"SELECT CONVERT(nvarchar(1), ShowInReports) AS Value FROM ItemColumns WHERE ItemId = '{item}' AND [Key] = 'account'"));
            Assert.Empty(await ValuesAsync(db, "SELECT name AS Value FROM sys.columns WHERE object_id = OBJECT_ID('CapitalMovementDetails') AND name = 'Type'"));
            Assert.Empty(await ValuesAsync(db, "SELECT name AS Value FROM sys.tables WHERE name = 'LK_CapitalMovementType'"));
        });
    }

    [Fact]
    public Task R216_UserDefaultProjectsKeyedByUser_EveryDefaultStaysKeyedByItsUserAndItsOwnIdGoes()
    {
        var (user, other, project) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(User(user), User(other), Project(project),
            $"INSERT INTO UserDefaultProjects (Id, UserId, ProjectId) VALUES (NEWID(), '{user}', '{project}'), (NEWID(), '{other}', '{project}');");

        return ThroughAsync("UserDefaultProjectsKeyedByUser", today, async db =>
        {
            Assert.Equal(new[] { $"{user} {project}", $"{other} {project}" }.Order(), (await ValuesAsync(db, "SELECT LOWER(CONCAT(UserId, ' ', ProjectId)) AS Value FROM UserDefaultProjects")).Order());
            Assert.Equal(["UserId"], await ValuesAsync(db, """
                SELECT COL_NAME(part.object_id, part.column_id) AS Value FROM sys.index_columns part
                JOIN sys.indexes primary_key ON primary_key.object_id = part.object_id AND primary_key.index_id = part.index_id
                WHERE primary_key.is_primary_key = 1 AND part.object_id = OBJECT_ID('UserDefaultProjects')
                """));
            Assert.Empty(await ValuesAsync(db, "SELECT name AS Value FROM sys.columns WHERE object_id = OBJECT_ID('UserDefaultProjects') AND name = 'Id'"));
        });
    }
}
