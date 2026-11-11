using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class HistorySearchWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("26d2f27f-1bf0-cd21-de22-193675cf876e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("788bf328-781f-7ab3-9b54-6bd2908e8aa4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88cd47f4-c279-ebe2-9b64-360b63587623"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0e20d03-0801-d147-1f92-f084e7d197c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b387e1e0-396a-4592-4bd8-288b84b909f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcdbce38-dcc3-b303-1564-61e08f824aba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "", "history.partner" },
                    { new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "", "history.account" },
                    { new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "", "history.all" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("51d0682e-0e80-81a6-206a-aa1dbcd1768c"), "en", new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "All" },
                    { new Guid("55b45fdc-f947-833e-1fb3-957cd14ea4fc"), "ar", new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "الشريك" },
                    { new Guid("7403d59f-215f-18b4-1dba-71c8f1aa4c7b"), "en", new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "Account" },
                    { new Guid("bb67ef87-cdbe-de54-d31f-2d269f8a6576"), "ar", new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "الحساب" },
                    { new Guid("dd73dad7-06cd-1f93-6496-ec5376c11b58"), "en", new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "Partner" },
                    { new Guid("e6652c43-48e0-9aa1-c34c-a6b26b1951a5"), "ar", new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "الكل" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("51d0682e-0e80-81a6-206a-aa1dbcd1768c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55b45fdc-f947-833e-1fb3-957cd14ea4fc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7403d59f-215f-18b4-1dba-71c8f1aa4c7b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bb67ef87-cdbe-de54-d31f-2d269f8a6576"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd73dad7-06cd-1f93-6496-ec5376c11b58"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6652c43-48e0-9aa1-c34c-a6b26b1951a5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "", "history.allPartners" },
                    { new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "", "history.allAccounts" },
                    { new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "", "entries.all" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("26d2f27f-1bf0-cd21-de22-193675cf876e"), "ar", new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "كل الحسابات" },
                    { new Guid("788bf328-781f-7ab3-9b54-6bd2908e8aa4"), "en", new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "All accounts" },
                    { new Guid("88cd47f4-c279-ebe2-9b64-360b63587623"), "en", new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "All partners" },
                    { new Guid("a0e20d03-0801-d147-1f92-f084e7d197c0"), "ar", new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "كل الشركاء" },
                    { new Guid("b387e1e0-396a-4592-4bd8-288b84b909f4"), "ar", new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "الكل" },
                    { new Guid("fcdbce38-dcc3-b303-1564-61e08f824aba"), "en", new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "All" }
                });
        }
    }
}
