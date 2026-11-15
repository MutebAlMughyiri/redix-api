using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class WidgetReadAtWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("08fdd21b-60ae-c635-2dfa-6d0908929bae"), "", "home.widgetReadAt" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("bc90d556-3652-b349-a7a6-5f5e71e562b0"), "ar", new Guid("08fdd21b-60ae-c635-2dfa-6d0908929bae"), "آخر تحديث {0}" },
                    { new Guid("eb20515c-3acf-c4ea-0108-963137c1f8c3"), "en", new Guid("08fdd21b-60ae-c635-2dfa-6d0908929bae"), "Updated {0}" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc90d556-3652-b349-a7a6-5f5e71e562b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb20515c-3acf-c4ea-0108-963137c1f8c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("08fdd21b-60ae-c635-2dfa-6d0908929bae"));
        }
    }
}
