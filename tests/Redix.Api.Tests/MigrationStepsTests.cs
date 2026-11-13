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
}
