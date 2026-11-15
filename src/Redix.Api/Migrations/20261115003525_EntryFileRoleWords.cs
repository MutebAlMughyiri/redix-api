using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryFileRoleWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("33d98d62-0eaa-dbe0-962e-0377226cef49"), "", "entries.fileOther" },
                    { new Guid("4f9fa42f-814e-2bce-0215-75f4dfa42c18"), "", "entries.fileInvoice" },
                    { new Guid("df1dc28e-effe-45b6-528b-226a34d4ec90"), "", "entries.fileReceipt" },
                    { new Guid("e254479f-8933-1d9f-523f-4d98e6b45110"), "", "entries.fileBankReceipt" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2a278a86-4ea0-0953-3c14-72f1855b17c8"), "ar", new Guid("df1dc28e-effe-45b6-528b-226a34d4ec90"), "سند استلام" },
                    { new Guid("503797d5-5dbc-026f-1a35-ac7e36bb69f3"), "en", new Guid("e254479f-8933-1d9f-523f-4d98e6b45110"), "Transfer receipt" },
                    { new Guid("514cdfb3-9c9a-38c2-29fe-b3bf2a1aa266"), "en", new Guid("33d98d62-0eaa-dbe0-962e-0377226cef49"), "File" },
                    { new Guid("65bdb956-ec05-7232-fd10-b65cc71b48a7"), "en", new Guid("4f9fa42f-814e-2bce-0215-75f4dfa42c18"), "Invoice" },
                    { new Guid("c47d4781-7deb-8cc5-264a-425e17571a71"), "ar", new Guid("e254479f-8933-1d9f-523f-4d98e6b45110"), "إيصال تحويل" },
                    { new Guid("e3fba847-515e-d7e9-9d6c-99b12001fde0"), "ar", new Guid("33d98d62-0eaa-dbe0-962e-0377226cef49"), "ملف" },
                    { new Guid("f183a32b-362b-101d-2791-fcefe1050944"), "ar", new Guid("4f9fa42f-814e-2bce-0215-75f4dfa42c18"), "فاتورة" },
                    { new Guid("fbd734ac-3d7e-97db-2b5e-fe06e389a0d4"), "en", new Guid("df1dc28e-effe-45b6-528b-226a34d4ec90"), "Receipt voucher" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a278a86-4ea0-0953-3c14-72f1855b17c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("503797d5-5dbc-026f-1a35-ac7e36bb69f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("514cdfb3-9c9a-38c2-29fe-b3bf2a1aa266"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65bdb956-ec05-7232-fd10-b65cc71b48a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c47d4781-7deb-8cc5-264a-425e17571a71"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e3fba847-515e-d7e9-9d6c-99b12001fde0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f183a32b-362b-101d-2791-fcefe1050944"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbd734ac-3d7e-97db-2b5e-fe06e389a0d4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33d98d62-0eaa-dbe0-962e-0377226cef49"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4f9fa42f-814e-2bce-0215-75f4dfa42c18"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("df1dc28e-effe-45b6-528b-226a34d4ec90"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e254479f-8933-1d9f-523f-4d98e6b45110"));
        }
    }
}
