using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SubTableWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "", "categories.itemTakesNoPurchases" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("4f1afe05-9708-61a5-d3f7-3d010bc2c796"), "ar", new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "الجدول الفرعي للمشتريات، فلا يكون إلا لبند يأخذ مشتريات." },
                    { new Guid("8694c1f7-f024-e5be-9eb4-18435cdbae24"), "en", new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "A sub-table holds purchases, so only an item that takes purchases has one." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f1afe05-9708-61a5-d3f7-3d010bc2c796"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8694c1f7-f024-e5be-9eb4-18435cdbae24"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"));
        }
    }
}
