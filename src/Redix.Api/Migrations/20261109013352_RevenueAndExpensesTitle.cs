using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RevenueAndExpensesTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44418224-6446-7b08-0aec-96566cfd1cef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6140c7de-1782-ec48-ba41-e735c4ab95df"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "", "reports.revenueAndExpenses" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1354ddba-a368-af89-4f5d-ef95c9b077c2"), "en", new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "Revenue and expenses" },
                    { new Guid("f6db13d7-0a0d-c42d-e0dc-cd5cf80bf1ba"), "ar", new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "الإيرادات والمصروفات" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1354ddba-a368-af89-4f5d-ef95c9b077c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6db13d7-0a0d-c42d-e0dc-cd5cf80bf1ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "", "reports.item" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("44418224-6446-7b08-0aec-96566cfd1cef"), "ar", new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "تقرير بند" },
                    { new Guid("6140c7de-1782-ec48-ba41-e735c4ab95df"), "en", new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "Item report" }
                });
        }
    }
}
