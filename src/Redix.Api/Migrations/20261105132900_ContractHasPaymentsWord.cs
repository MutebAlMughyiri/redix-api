using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ContractHasPaymentsWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "", "contracts.contractHasPayments" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("08f5d8be-844d-c493-94ba-a5ffe9a7e853"), "en", new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "This contract has payments, so its contractor, item and unit cannot change." },
                    { new Guid("4bc91b90-0791-5b58-c656-fb2f12c46a47"), "ar", new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "لهذا العقد دفعات، فلا يتغير مقاوله ولا بنده ولا وحدته." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08f5d8be-844d-c493-94ba-a5ffe9a7e853"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bc91b90-0791-5b58-c656-fb2f12c46a47"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"));
        }
    }
}
