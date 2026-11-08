using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CapitalUnderRevenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "", "entries.capitalGroup" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("12dee4c0-f443-892e-5728-c2e00f9ca781"), "en", new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "Revenue · Capital" },
                    { new Guid("2f7e5d09-b834-ea9a-0aa6-985e5ac0a9f7"), "ar", new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "الإيرادات · رأس المال" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12dee4c0-f443-892e-5728-c2e00f9ca781"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f7e5d09-b834-ea9a-0aa6-985e5ac0a9f7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"));
        }
    }
}
