using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ColumnValuesOnTheEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColumnValues",
                table: "Entries",
                type: "json",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.Sql("""
                UPDATE entry SET ColumnValues = details.CustomValues FROM Entries entry JOIN ExpenseDetails details ON details.EntryId = entry.Id;
                UPDATE entry SET ColumnValues = JSON_MODIFY(CAST(entry.ColumnValues AS nvarchar(max)), '$.product', purchase.Product)
                FROM Entries entry JOIN PurchaseDetails purchase ON purchase.EntryId = entry.Id WHERE purchase.Product <> '';
                UPDATE entry SET ColumnValues = JSON_MODIFY(CAST(entry.ColumnValues AS nvarchar(max)), '$.unitPrice', FORMAT(purchase.UnitPrice, '0.####', 'en-US'))
                FROM Entries entry JOIN PurchaseDetails purchase ON purchase.EntryId = entry.Id WHERE purchase.UnitPrice <> 0;
                UPDATE entry SET ColumnValues = JSON_MODIFY(CAST(entry.ColumnValues AS nvarchar(max)), '$.quantity', FORMAT(purchase.Quantity, '0.####', 'en-US'))
                FROM Entries entry JOIN PurchaseDetails purchase ON purchase.EntryId = entry.Id WHERE purchase.Quantity <> 0;
                UPDATE DefaultColumns SET IsRequired = 0 WHERE [Key] = 'supplier';
                UPDATE ItemColumns SET IsRequired = 0 WHERE [Key] = 'supplier';
                """);

            migrationBuilder.DropColumn(
                name: "Product",
                table: "PurchaseDetails");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PurchaseDetails");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "PurchaseDetails");

            migrationBuilder.DropColumn(
                name: "CustomValues",
                table: "ExpenseDetails");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                column: "IsRequired",
                value: false);
            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                column: "IsRequired",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "PurchaseDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "PurchaseDetails",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "PurchaseDetails",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomValues",
                table: "ExpenseDetails",
                type: "json",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.Sql("""
                UPDATE purchase SET
                    Product = LEFT(ISNULL(JSON_VALUE(CAST(entry.ColumnValues AS nvarchar(max)), '$.product'), ''), 200),
                    UnitPrice = ISNULL(TRY_CAST(JSON_VALUE(CAST(entry.ColumnValues AS nvarchar(max)), '$.unitPrice') AS decimal(19,4)), 0),
                    Quantity = ISNULL(TRY_CAST(JSON_VALUE(CAST(entry.ColumnValues AS nvarchar(max)), '$.quantity') AS decimal(19,4)), 0)
                FROM PurchaseDetails purchase JOIN Entries entry ON entry.Id = purchase.EntryId;
                UPDATE details SET CustomValues = JSON_MODIFY(JSON_MODIFY(JSON_MODIFY(CAST(entry.ColumnValues AS nvarchar(max)), '$.product', NULL), '$.unitPrice', NULL), '$.quantity', NULL)
                FROM ExpenseDetails details JOIN Entries entry ON entry.Id = details.EntryId;
                UPDATE DefaultColumns SET IsRequired = 1 WHERE [Key] = 'supplier' AND IsCore = 1 AND ItemType IN (0, 2);
                UPDATE ItemColumns SET IsRequired = 1 WHERE [Key] = 'supplier' AND IsCore = 1 AND EXISTS (SELECT 1 FROM Items WHERE Items.Id = ItemColumns.ItemId AND Items.Type IN (0, 2));
                """);

            migrationBuilder.DropColumn(
                name: "ColumnValues",
                table: "Entries");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                column: "IsRequired",
                value: true);
            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                column: "IsRequired",
                value: true);
        }
    }
}
