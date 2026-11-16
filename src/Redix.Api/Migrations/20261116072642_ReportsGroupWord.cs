using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportsGroupWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("10ace059-16e7-ff81-b77e-937c568a778a"), "", "permissions.reportsGroup" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("44cfe90f-9273-f3e9-72e3-f9793157ce13"), "ar", new Guid("10ace059-16e7-ff81-b77e-937c568a778a"), "التقارير" },
                    { new Guid("fdd839bf-056a-c043-c979-55fcd24999b7"), "en", new Guid("10ace059-16e7-ff81-b77e-937c568a778a"), "Reports" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44cfe90f-9273-f3e9-72e3-f9793157ce13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fdd839bf-056a-c043-c979-55fcd24999b7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("10ace059-16e7-ff81-b77e-937c568a778a"));
        }
    }
}
