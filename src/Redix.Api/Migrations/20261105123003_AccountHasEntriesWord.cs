using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AccountHasEntriesWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "", "accounts.accountHasEntries" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1d18f432-1755-ba9b-d98b-e2c9c241c151"), "ar", new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "هذا الحساب عليه قيود، فلا يتغير رصيده الافتتاحي." },
                    { new Guid("5ece3a4c-3b6e-c7c6-4b32-1e2fcd503b38"), "en", new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "This account has entries, so its opening balance cannot change." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d18f432-1755-ba9b-d98b-e2c9c241c151"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ece3a4c-3b6e-c7c6-4b32-1e2fcd503b38"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"));
        }
    }
}
