using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryFieldIsItemWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b0b8889-94ab-cda6-eeae-99a0fc39b180"),
                column: "Value",
                value: "لا بند يطابق البحث.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fe0d45b-bada-bc47-9016-a1bd40f6d23f"),
                column: "Value",
                value: "Item");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("933d15b2-8b99-72b0-077b-358732d39b51"),
                column: "Value",
                value: "البند");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1f5a845-9682-8efa-0269-5929fc8776cc"),
                column: "Value",
                value: "No item matches the search.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b0b8889-94ab-cda6-eeae-99a0fc39b180"),
                column: "Value",
                value: "لا جدول يطابق البحث.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fe0d45b-bada-bc47-9016-a1bd40f6d23f"),
                column: "Value",
                value: "Table");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("933d15b2-8b99-72b0-077b-358732d39b51"),
                column: "Value",
                value: "الجدول");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1f5a845-9682-8efa-0269-5929fc8776cc"),
                column: "Value",
                value: "No table matches the search.");
        }
    }
}
