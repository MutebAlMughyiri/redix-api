using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <summary>
    /// The audit trail becomes the History table (redix-app #89, D15), and every row learns the project of its record (D11, D14): renamed in place, so not one row is lost, and
    /// filled once with the append-only trigger paused for that alone; nothing else in any row changes (AC-21.1).
    /// </summary>
    public partial class HistoryTable : Migration
    {
        private const string NoProject = "00000000-0000-0000-0000-000000000000";

        private static readonly string[] TablesNamingTheirProject = ["Accounts", "Attachments", "Contracts", "Documents", "Entries", "ProjectPermissions", "ProjectRoleNames", "Reconciliations", "Suppliers", "Units"];

        private static readonly string[] PartsOfAnEntry = ["ExpenseDetails", "PurchaseDetails", "ContractorPaymentDetails", "CapitalMovementDetails", "RevenueDetails", "EntryAttachments", "Invoices", "EntryLinks"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER AuditTrailIsAppendOnly;");
            migrationBuilder.RenameTable(name: "AuditTrail", newName: "History");
            migrationBuilder.Sql("EXEC sp_rename N'PK_AuditTrail', N'PK_History', N'OBJECT'; EXEC sp_rename N'FK_AuditTrail_Users_UserId', N'FK_History_Users_UserId', N'OBJECT';");
            migrationBuilder.RenameIndex(name: "IX_AuditTrail_UserId", table: "History", newName: "IX_History_UserId");

            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "History", type: "uniqueidentifier", nullable: false, defaultValue: Guid.Empty);

            // The record is a project, names its project, or hangs off an entry, a contract or a category: as HistoryRecord.Of tells a new row's project.
            migrationBuilder.Sql("UPDATE History SET ProjectId = RecordId WHERE TableName = 'Projects';");
            foreach (var table in TablesNamingTheirProject)
            {
                migrationBuilder.Sql($"UPDATE h SET ProjectId = t.ProjectId FROM History h JOIN [{table}] t ON t.Id = h.RecordId WHERE h.TableName = '{table}';");
            }
            migrationBuilder.Sql($"UPDATE h SET ProjectId = e.ProjectId FROM History h JOIN Entries e ON e.Id = h.RecordId WHERE h.TableName IN ({string.Join(", ", PartsOfAnEntry.Select(table => $"'{table}'"))});");
            migrationBuilder.Sql("UPDATE h SET ProjectId = c.ProjectId FROM History h JOIN Contracts c ON c.Id = h.RecordId WHERE h.TableName = 'ContractAttachments';");
            migrationBuilder.Sql("UPDATE h SET ProjectId = c.ProjectId FROM History h JOIN Items i ON i.Id = h.RecordId JOIN Categories c ON c.Id = i.CategoryId WHERE h.TableName = 'Items';");

            // A record deleted since: the project its kept values name; then the project of another row about the same record, since a change keeps only what it changed;
            // then, for the parts of a deleted entry, the project of the entry's own rows.
            migrationBuilder.Sql($"""
                UPDATE History SET ProjectId = TRY_CAST(COALESCE(JSON_VALUE(Before, '$.ProjectId'), JSON_VALUE(After, '$.ProjectId')) AS uniqueidentifier)
                WHERE ProjectId = '{NoProject}' AND TRY_CAST(COALESCE(JSON_VALUE(Before, '$.ProjectId'), JSON_VALUE(After, '$.ProjectId')) AS uniqueidentifier) IS NOT NULL;
                """);
            migrationBuilder.Sql($"""
                UPDATE h SET ProjectId = k.ProjectId FROM History h
                JOIN (SELECT TableName, RecordId, MAX(CAST(ProjectId AS char(36))) AS ProjectId FROM History WHERE ProjectId <> '{NoProject}' GROUP BY TableName, RecordId) k
                  ON k.TableName = h.TableName AND k.RecordId = h.RecordId
                WHERE h.ProjectId = '{NoProject}';
                """);
            migrationBuilder.Sql($"""
                UPDATE h SET ProjectId = e.ProjectId FROM History h
                JOIN (SELECT RecordId, MAX(CAST(ProjectId AS char(36))) AS ProjectId FROM History WHERE TableName = 'Entries' AND ProjectId <> '{NoProject}' GROUP BY RecordId) e ON e.RecordId = h.RecordId
                WHERE h.ProjectId = '{NoProject}' AND h.TableName IN ({string.Join(", ", PartsOfAnEntry.Select(table => $"'{table}'"))});
                """);

            migrationBuilder.CreateIndex(name: "IX_History_ProjectId_Id", table: "History", columns: ["ProjectId", "Id"])
                .Annotation("SqlServer:Include", new[] { "At", "UserId", "Action", "TableName", "RecordId" });
            migrationBuilder.Sql("""
                CREATE TRIGGER HistoryIsAppendOnly ON History INSTEAD OF UPDATE, DELETE AS
                THROW 50021, 'The History table is append-only: its rows are never changed or deleted (AC-21.1).', 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER HistoryIsAppendOnly;");
            migrationBuilder.DropIndex(name: "IX_History_ProjectId_Id", table: "History");
            migrationBuilder.DropColumn(name: "ProjectId", table: "History");
            migrationBuilder.RenameIndex(name: "IX_History_UserId", table: "History", newName: "IX_AuditTrail_UserId");
            migrationBuilder.Sql("EXEC sp_rename N'PK_History', N'PK_AuditTrail', N'OBJECT'; EXEC sp_rename N'FK_History_Users_UserId', N'FK_AuditTrail_Users_UserId', N'OBJECT';");
            migrationBuilder.RenameTable(name: "History", newName: "AuditTrail");
            migrationBuilder.Sql("""
                CREATE TRIGGER AuditTrailIsAppendOnly ON AuditTrail INSTEAD OF UPDATE, DELETE AS
                THROW 50021, 'The audit trail is append-only: its records are never changed or deleted (AC-21.1).', 1;
                """);
        }
    }
}
