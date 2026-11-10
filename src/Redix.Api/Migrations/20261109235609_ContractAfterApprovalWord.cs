using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ContractAfterApprovalWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "", "contracts.afterApproval" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("325f1b4c-18a8-84bc-db9d-e816d18f4534"), "en", new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "after approval" },
                    { new Guid("5ceaf5e5-6bd3-8f12-1ae3-553a30d1c1e2"), "ar", new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "بعد الاعتماد" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("325f1b4c-18a8-84bc-db9d-e816d18f4534"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ceaf5e5-6bd3-8f12-1ae3-553a30d1c1e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"));
        }
    }
}
