using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceNumberOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every project's copy of the column, beside the template's rows below: an invoice's number is no longer required.
            migrationBuilder.Sql("UPDATE DefaultColumns SET IsRequired = 0 WHERE [Key] = N'invoiceNumber' AND IsCore = 1");
            migrationBuilder.Sql("UPDATE ItemColumns SET IsRequired = 0 WHERE [Key] = N'invoiceNumber' AND IsCore = 1");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b81ba454-4df5-80d1-864d-e2b27314cc73"),
                column: "IsRequired",
                value: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c9a5b977-5bcd-ab11-87f3-2fd7c0175290"),
                column: "IsRequired",
                value: false);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"),
                column: "Value",
                value: "Write the invoice number in 50 characters at most.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"),
                column: "Value",
                value: "اكتب رقم الفاتورة في 50 حرفاً على الأكثر.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A purchase's number was required; a contractor payment's never was.
            migrationBuilder.Sql("UPDATE DefaultColumns SET IsRequired = 1 WHERE [Key] = N'invoiceNumber' AND IsCore = 1 AND ItemType <> 1");
            migrationBuilder.Sql("UPDATE c SET IsRequired = 1 FROM ItemColumns c JOIN Items i ON i.Id = c.ItemId WHERE c.[Key] = N'invoiceNumber' AND c.IsCore = 1 AND i.Type <> 1");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b81ba454-4df5-80d1-864d-e2b27314cc73"),
                column: "IsRequired",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c9a5b977-5bcd-ab11-87f3-2fd7c0175290"),
                column: "IsRequired",
                value: true);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"),
                column: "Value",
                value: "Type the invoice's number.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"),
                column: "Value",
                value: "اكتب رقم الفاتورة.");
        }
    }
}
