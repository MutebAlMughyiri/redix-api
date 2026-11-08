using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoBareAddWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1839efbc-9109-c577-1bba-73e6f603caf8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4696d695-79a4-8ac6-703c-a25a63a7363b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "", "common.add" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1839efbc-9109-c577-1bba-73e6f603caf8"), "ar", new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "إضافة" },
                    { new Guid("4696d695-79a4-8ac6-703c-a25a63a7363b"), "en", new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "Add" }
                });
        }
    }
}
