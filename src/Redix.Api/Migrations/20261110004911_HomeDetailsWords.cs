using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class HomeDetailsWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "", "home.hideDetails" },
                    { new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "", "home.showDetails" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("18739dda-0f8a-cb22-82e9-cc1c583dcc04"), "en", new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "Show details" },
                    { new Guid("c692285c-b140-52e3-3ef7-240e8ba9fa6f"), "en", new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "Hide details" },
                    { new Guid("cf56166f-ccdd-60b1-38b8-a78c5fbbfb52"), "ar", new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "إخفاء التفاصيل" },
                    { new Guid("d82b471d-24c9-f838-c1f3-ff3a97cc2189"), "ar", new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "إظهار التفاصيل" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18739dda-0f8a-cb22-82e9-cc1c583dcc04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c692285c-b140-52e3-3ef7-240e8ba9fa6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cf56166f-ccdd-60b1-38b8-a78c5fbbfb52"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d82b471d-24c9-f838-c1f3-ff3a97cc2189"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"));
        }
    }
}
