using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class TableSearchWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "", "entries.tableSearchHint" },
                    { new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "", "entries.noTableMatch" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0b0b8889-94ab-cda6-eeae-99a0fc39b180"), "ar", new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "لا جدول يطابق البحث." },
                    { new Guid("711df39f-5dbc-5c25-7d1b-e77a43111bb5"), "en", new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "Search by item or contractor" },
                    { new Guid("ab67a1c3-cee3-5c60-5257-9ec07bab9e7c"), "ar", new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "بحث بالبند أو المقاول" },
                    { new Guid("e1f5a845-9682-8efa-0269-5929fc8776cc"), "en", new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "No table matches the search." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b0b8889-94ab-cda6-eeae-99a0fc39b180"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("711df39f-5dbc-5c25-7d1b-e77a43111bb5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ab67a1c3-cee3-5c60-5257-9ec07bab9e7c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1f5a845-9682-8efa-0269-5929fc8776cc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("133520ba-c89a-4c91-3190-0369147b3f45"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"));
        }
    }
}
