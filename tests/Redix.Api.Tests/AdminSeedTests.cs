using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class AdminSeedTests(MigratedApiFactory factory)
{
    private const string BeforeUsersExist = "20261101003509_InitialCreate";

    [Fact]
    public Task AMigrationThatStopsBeforeTheUsersTableSeedsNothingAndSucceeds_AsARollbackDoes() =>
        factory.OnScratchDatabaseAsync("RedixSeedTests", async db =>
        {
            db.GetService<IMigrator>().Migrate(BeforeUsersExist);
            await db.GetService<IMigrator>().MigrateAsync(BeforeUsersExist);

            Assert.Equal([BeforeUsersExist], db.Database.GetAppliedMigrations());
        });
}
