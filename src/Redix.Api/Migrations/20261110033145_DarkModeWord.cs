using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DarkModeWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "", "settings.darkMode" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("3f8a9725-8632-6380-41b4-f9dc04a66af1"), "en", new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "Dark mode" },
                    { new Guid("972c19b6-535e-c396-c199-7edb1b8ddd33"), "ar", new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "الوضع الداكن" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f8a9725-8632-6380-41b4-f9dc04a66af1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("972c19b6-535e-c396-c199-7edb1b8ddd33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"));
        }
    }
}
