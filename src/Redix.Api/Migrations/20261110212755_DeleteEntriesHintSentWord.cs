using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteEntriesHintSentWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cec74d2-98ca-19ea-2405-4cd1058e2bd0"),
                column: "Value",
                value: "Deletes a bill or a payment not yet approved: their own, or one someone else sent, with \"Handle others' entries\".");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86d887f0-969a-12cf-fd9a-3e3e1807118f"),
                column: "Value",
                value: "يحذف فاتورة أو دفعة لم تُعتمد: قيده، أو قيداً أرسله غيره مع «إدارة قيود الآخرين».");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cec74d2-98ca-19ea-2405-4cd1058e2bd0"),
                column: "Value",
                value: "Deletes a bill or a payment not yet approved: their own, or someone else's with \"Handle others' entries\".");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86d887f0-969a-12cf-fd9a-3e3e1807118f"),
                column: "Value",
                value: "يحذف فاتورة أو دفعة لم تُعتمد: قيده، أو قيد غيره مع «إدارة قيود الآخرين».");
        }
    }
}
