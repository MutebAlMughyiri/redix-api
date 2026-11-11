using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SearchIconWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "", "common.search" },
                    { new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "", "common.filter" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("042648c9-dacd-554c-342e-264f1b6f3fb7"), "en", new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "Search" },
                    { new Guid("b809ee8f-3e36-fa4d-5711-e8ca9399c91e"), "ar", new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "تصفية" },
                    { new Guid("ce9e01fe-93ce-8325-0b26-25fdf38f6bb4"), "ar", new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "بحث" },
                    { new Guid("e773f6e7-f71c-3206-a1c7-a10243befe56"), "en", new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "Filter" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("042648c9-dacd-554c-342e-264f1b6f3fb7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b809ee8f-3e36-fa4d-5711-e8ca9399c91e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce9e01fe-93ce-8325-0b26-25fdf38f6bb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e773f6e7-f71c-3206-a1c7-a10243befe56"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"));
        }
    }
}
