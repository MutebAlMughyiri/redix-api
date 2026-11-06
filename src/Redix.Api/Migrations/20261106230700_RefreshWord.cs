using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefreshWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("8d36c17f-5f52-f8db-80dd-02a8dd94a7b6"), "", "common.refresh" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0008eb57-3947-e71a-93bc-adfd8b01df7d"), "en", new Guid("8d36c17f-5f52-f8db-80dd-02a8dd94a7b6"), "Refresh" },
                    { new Guid("c090adcd-0aad-228c-0be0-a2cfc29a68f3"), "ar", new Guid("8d36c17f-5f52-f8db-80dd-02a8dd94a7b6"), "تحديث" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0008eb57-3947-e71a-93bc-adfd8b01df7d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c090adcd-0aad-228c-0be0-a2cfc29a68f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8d36c17f-5f52-f8db-80dd-02a8dd94a7b6"));
        }
    }
}
