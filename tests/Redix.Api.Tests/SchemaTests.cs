using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Redix.Api.Tests;

/// <summary>
/// redix-api #123, step 1: REDIX's core tables stay in <c>dbo</c> and every table whose rows belong to one project is in the schema <c>project</c>; a table declares its side with
/// <see cref="IProjectRow"/> (D5), and one that names a project but declares no side fails here.
/// </summary>
[Collection(FourthDatabaseCollection.Name)]
public class SchemaTests(MigratedApiFactory factory)
{
    /// <summary>
    /// The core tables that name a project and still stay core, each for its reason: who may do what in which project is what lets a person sign in once and see the projects they
    /// belong to; History stays whole in step 1, and the notifications stay core, since the bell spans projects (D6).
    /// </summary>
    private static readonly string[] CoreTablesNamingAProject =
        ["ProjectPermissions", "ProjectRoleNames", "InactiveMembers", "UserDefaultProjects", "ProjectStructureSources", "History", "Notifications", "NotificationEntries"];

    [Fact]
    public async Task R123_D5_EveryTableMarkedAsAProjectsRowIsInTheSchemaProject()
    {
        var marked = Model().GetEntityTypes().Where(IsProjectRow).Select(type => type.GetTableName()!).ToList();

        var stored = await TablesInAsync(IProjectRow.Schema);

        Assert.Equal(31, marked.Count);
        Assert.Equal(marked.Order(), stored.Order());
        Assert.Contains("Entries", stored);
    }

    [Fact]
    public async Task R123_D6_EveryCoreTableStaysInDbo_TheLookupsHistoryAndTheNotificationsToo()
    {
        var core = Model().GetEntityTypes().Where(type => !IsProjectRow(type)).Select(type => type.GetTableName()!).ToList();

        var stored = await TablesInAsync("dbo");

        Assert.Equal(core.Append("__EFMigrationsHistory").Order(), stored.Order());
        Assert.Contains("History", stored);
        Assert.Contains("Notifications", stored);
        Assert.Contains("LK_EntryKind", stored);
        Assert.Contains("Projects", stored);
        Assert.Contains("EmailCodes", stored);
        Assert.Contains("LK_EmailCodePurpose", stored);
    }

    [Fact]
    public void R123_D5_EveryTableThatNamesAProjectDeclaresItsSide()
    {
        Assert.Empty(UndeclaredTables(Model()));
    }

    [Fact]
    public void R123_D5_ATableThatNamesAProjectAndDeclaresNoSideFailsTheCheck()
    {
        using var db = new UndeclaredSideDb();

        Assert.Equal(["RowHangingFromAProjectRow", "RowNamingAProject"], UndeclaredTables(db.Model).Order());
    }

    private static bool IsProjectRow(IReadOnlyEntityType type) => type.ClrType.IsAssignableTo(typeof(IProjectRow));

    /// <summary>The tables that hold a project's id, or a key to a project's table, yet are neither marked <see cref="IProjectRow"/> nor a core table that names a project.</summary>
    private static IEnumerable<string> UndeclaredTables(IModel model) => model.GetEntityTypes()
        .Where(type => !IsProjectRow(type) && !CoreTablesNamingAProject.Contains(type.GetTableName()))
        .Where(type => type.FindProperty("ProjectId") is not null || type.GetForeignKeys().Any(key => IsProjectRow(key.PrincipalEntityType)))
        .Select(type => type.GetTableName()!);

    private async Task<List<string>> TablesInAsync(string schema)
    {
        var tables = new List<string>();
        await factory.WithDatabaseAsync(async db => tables.AddRange(await db.Database.SqlQuery<string>($"SELECT name AS Value FROM sys.tables WHERE SCHEMA_NAME(schema_id) = {schema}").ToListAsync()));
        return tables;
    }

    private IModel Model()
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RedixDbContext>().Model;
    }

    /// <summary>A model of four tables: one marked, one core with nothing of a project, and two that name a project without declaring a side.</summary>
    private class UndeclaredSideDb : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer("Server=none");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectRow>();
            modelBuilder.Entity<CoreRow>();
            modelBuilder.Entity<RowNamingAProject>();
            modelBuilder.Entity<RowHangingFromAProjectRow>().HasOne<ProjectRow>().WithMany().HasForeignKey(row => row.ProjectRowId);
        }
    }

    private class ProjectRow : IProjectRow
    {
        public Guid Id { get; init; }

        public Guid ProjectId { get; init; }
    }

    private class CoreRow
    {
        public Guid Id { get; init; }
    }

    private class RowNamingAProject
    {
        public Guid Id { get; init; }

        public Guid ProjectId { get; init; }
    }

    private class RowHangingFromAProjectRow
    {
        public Guid Id { get; init; }

        public Guid ProjectRowId { get; init; }
    }
}
