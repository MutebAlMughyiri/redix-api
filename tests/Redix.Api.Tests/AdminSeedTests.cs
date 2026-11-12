using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Redix.Api.Tests;

[Collection(FourthDatabaseCollection.Name)]
public class AdminSeedTests(MigratedApiFactory factory)
{
    [Fact]
    public Task AMigrationThatStopsBeforeTheUsersTableSeedsNothingAndSucceeds_AsARollbackDoes() =>
        factory.OnScratchDatabaseAsync("RedixSeedTests", async db =>
        {
            db.GetService<IMigrator>().Migrate(Migration.InitialDatabase);
            await db.GetService<IMigrator>().MigrateAsync(Migration.InitialDatabase);

            Assert.Empty(db.Database.GetAppliedMigrations());
        });
}
