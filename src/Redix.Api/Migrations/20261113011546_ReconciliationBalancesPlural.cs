using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReconciliationBalancesPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReconciliationBalance_Accounts_AccountId",
                table: "ReconciliationBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_ReconciliationBalance_Reconciliations_ReconciliationId",
                table: "ReconciliationBalance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReconciliationBalance",
                table: "ReconciliationBalance");

            migrationBuilder.RenameTable(
                name: "ReconciliationBalance",
                newName: "ReconciliationBalances");

            migrationBuilder.RenameIndex(
                name: "IX_ReconciliationBalance_AccountId",
                table: "ReconciliationBalances",
                newName: "IX_ReconciliationBalances_AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReconciliationBalances",
                table: "ReconciliationBalances",
                columns: new[] { "ReconciliationId", "AccountId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ReconciliationBalances_Accounts_AccountId",
                table: "ReconciliationBalances",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReconciliationBalances_Reconciliations_ReconciliationId",
                table: "ReconciliationBalances",
                column: "ReconciliationId",
                principalTable: "Reconciliations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReconciliationBalances_Accounts_AccountId",
                table: "ReconciliationBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_ReconciliationBalances_Reconciliations_ReconciliationId",
                table: "ReconciliationBalances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReconciliationBalances",
                table: "ReconciliationBalances");

            migrationBuilder.RenameTable(
                name: "ReconciliationBalances",
                newName: "ReconciliationBalance");

            migrationBuilder.RenameIndex(
                name: "IX_ReconciliationBalances_AccountId",
                table: "ReconciliationBalance",
                newName: "IX_ReconciliationBalance_AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReconciliationBalance",
                table: "ReconciliationBalance",
                columns: new[] { "ReconciliationId", "AccountId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ReconciliationBalance_Accounts_AccountId",
                table: "ReconciliationBalance",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReconciliationBalance_Reconciliations_ReconciliationId",
                table: "ReconciliationBalance",
                column: "ReconciliationId",
                principalTable: "Reconciliations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
