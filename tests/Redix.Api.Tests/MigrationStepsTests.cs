using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

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

    private static string Project(Guid id, Guid structureSourceId = default) =>
        $"INSERT INTO Projects (Id, Name, StartDate, Status, RejectionReason, Description, IsTemplate, StructureSourceId) VALUES ('{id}', N'غصون', '2026-11-01', 0, N'', N'', 0, '{structureSourceId}');";

    private static string Notification(Guid id, Guid userId, Guid projectId, Guid entryId = default) =>
        $"INSERT INTO Notifications (Id, UserId, ProjectId, EntryId, Kind, At, IsRead) VALUES ('{id}', '{userId}', '{projectId}', '{entryId}', 0, '2026-11-01', 0);";

    private static string Account(Guid id, Guid projectId) =>
        $"INSERT INTO Accounts (Id, ProjectId, Type, Name, IsActive) VALUES ('{id}', '{projectId}', 0, N'حساب', 1);";

    private static string Entry(Guid id, Guid projectId, Guid accountId, Guid userId) =>
        $"INSERT INTO Entries (Id, ProjectId, Kind, Status, EntryDate, Amount, AccountId, Notes, CreatedByUserId, CreatedAt, ApprovedAt) VALUES ('{id}', '{projectId}', 0, 0, '2026-11-01', 100, '{accountId}', N'', '{userId}', '2026-11-01', '1900-01-01');";

    private static string Partner(Guid id, Guid projectId, Guid userId, bool isDeleted = false) =>
        $"INSERT INTO Partners (Id, ProjectId, UserId, Name, ItemId, SortOrder, IsDeleted) VALUES ('{id}', '{projectId}', '{userId}', N'شريك', '{Guid.Empty}', 1, {(isDeleted ? 1 : 0)});";

    private static string Held(Guid projectId, Guid userId, string permission = "ReadProject") =>
        $"INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission) VALUES (NEWID(), '{projectId}', '{userId}', '{permission}');";

    [Fact]
    public Task R126_InactivePerProject_ADisabledLoginBecomesInactiveInEveryProjectHeHoldsAPermissionInAndTheLoginWideSwitchGoes()
    {
        var (disabled, active, first, second) = (Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());
        var today = string.Concat(LoginWithIsActive(disabled, isActive: false), LoginWithIsActive(active, isActive: true), Project(first), Project(second),
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

        return ThroughAsync("NotificationProjectKey", string.Concat(User(user), Project(project), Notification(notification, user, project)), async db =>
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
        var today = string.Concat(User(user), Project(project), Project(copied, project), Project(copiedFromNothing, Guid.CreateVersion7()), Account(account, project), Entry(entry, project, account, user),
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
}
