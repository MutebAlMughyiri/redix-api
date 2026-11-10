using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ManageAnyEntryHintSentWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50c76735-8650-fb66-8df0-19d905c97439"),
                column: "Value",
                value: "يعدّل ويرسل ويحذف قيداً أرسله غيره، أو قيداً ينتظر، قبل اعتماده.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94ed53b4-1874-de23-89c5-fd33eb1a3c38"),
                column: "Value",
                value: "Edits, sends and deletes an entry someone else sent, or one waiting, before approval.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50c76735-8650-fb66-8df0-19d905c97439"),
                column: "Value",
                value: "يعدّل ويرسل ويحذف قيداً أدخله غيره، أو قيداً ينتظر، قبل اعتماده.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94ed53b4-1874-de23-89c5-fd33eb1a3c38"),
                column: "Value",
                value: "Edits, sends and deletes an entry someone else entered, or one waiting, before approval.");
        }
    }
}
