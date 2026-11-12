using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Auth;
using Redix.Api.Structure;

namespace Redix.Api.Tests;

/// <summary>redix-api #87: every enum the database stores has its lookup table, LK_{enum}, holding exactly its members, and every column holding the enum points at it.</summary>
[Collection(FourthDatabaseCollection.Name)]
public class LookupTablesTests(MigratedApiFactory factory)
{
    [Fact]
    public void R87_EveryEnumColumnPointsAtItsLookupTable()
    {
        var columns = Model().GetEntityTypes().Where(entity => !IsLookup(entity)).SelectMany(entity => entity.GetProperties())
            .Where(column => column.ClrType.IsEnum && !WaitsForItsOwnColumns(column)).ToList();

        var unpointed = columns.Where(column => !column.GetContainingForeignKeys()
                .Any(key => key.PrincipalEntityType.GetTableName() == $"LK_{column.ClrType.Name}" && key.DeleteBehavior == DeleteBehavior.Restrict))
            .Select(column => $"{column.DeclaringType.ShortName()}.{column.Name}");

        Assert.NotEmpty(columns);
        Assert.Empty(unpointed);
    }

    [Fact]
    public async Task R87_EveryLookupTableHoldsExactlyItsEnumsMembers()
    {
        var lookups = Model().GetEntityTypes().Where(IsLookup).Select(entity => (Table: entity.GetTableName()!, Members: entity.ClrType.GetGenericArguments()[0])).ToList();
        var expected = lookups.SelectMany(lookup => Enum.GetValues(lookup.Members).Cast<Enum>().Select(member => $"{lookup.Table} {Convert.ToInt64(member)} {member}")).Order();
        var stored = new List<string>();

        await factory.WithDatabaseAsync(async db => stored.AddRange(await db.Database.SqlQueryRaw<string>(EveryLookupRow).ToListAsync()));

        Assert.NotEmpty(lookups);
        Assert.Equal(expected, stored.Order());
    }

    [Fact]
    public async Task R87_TheDatabaseRefusesANumberNoMemberOfTheEnumHas()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            const string sql = "INSERT INTO Accounts (Id, ProjectId, Type, Name, IsActive) VALUES (NEWID(), {0}, {1}, N'حساب', 1)";

            var refused = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(sql, StructureSeed.TemplateId, (byte)99));

            Assert.Contains("FK_Accounts_LK_AccountType_Type", refused.Message);
        });
    }

    [Fact]
    public async Task R87_TheDatabaseRefusesANameNoMemberOfTheEnumHas()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            var admin = await db.Users.SingleAsync(user => user.Username == MigratedApiFactory.AdminUsername);
            const string sql = "INSERT INTO History (At, ProjectId, UserId, Action, TableName, RecordId, [Before], [After]) VALUES (SYSUTCDATETIME(), {0}, {1}, {2}, 'Users', {1}, {3}, {3})";

            var refused = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(sql, Guid.Empty, admin.Id, "NotAnAction", "{}"));

            Assert.Contains("FK_History_LK_HistoryAction_Action", refused.Message);
        });
    }

    /// <summary>Every row of every table of the database named LK_..., as "table id name": the database builds the one statement from its own list of tables.</summary>
    private const string EveryLookupRow = """
        DECLARE @rows nvarchar(max) = (
            SELECT STRING_AGG(CAST(CONCAT('SELECT CONCAT(', QUOTENAME(name, ''''), ', '' '', Id, '' '', Name) AS Value FROM ', QUOTENAME(name)) AS nvarchar(max)), ' UNION ALL ')
            FROM sys.tables WHERE name LIKE 'LK[_]%');
        EXEC sp_executesql @rows;
        """;

    private IModel Model()
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RedixDbContext>().Model;
    }

    /// <summary>The permission columns keep their names and their check constraints until a true/false column per permission replaces them (redix-api #120).</summary>
    private static bool WaitsForItsOwnColumns(IProperty column) => column.ClrType == typeof(Permission);

    private static bool IsLookup(IEntityType entity) => entity.ClrType.IsGenericType && entity.ClrType.GetGenericTypeDefinition() == typeof(Lookup<>);
}
