using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfirmWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "", "entries.removeQuestion" },
                    { new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "", "swipe.question" },
                    { new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "", "common.undo" },
                    { new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "", "swipe.removed" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("009cb907-266d-c459-8d48-670708f7e3e6"), "ar", new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "حذفه؟" },
                    { new Guid("1d9788c3-5124-5869-63ae-8644ef92f74a"), "ar", new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "حذف هذا القيد المعتمد؟" },
                    { new Guid("1e79722a-e349-6e16-bf81-1e6d89752e97"), "en", new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "Remove it?" },
                    { new Guid("4faf3b4a-e2c7-30ab-f1e0-5347398e6127"), "en", new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "Removed." },
                    { new Guid("5bc47b9c-6b4e-b110-7c46-44c7b4491220"), "en", new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "Remove this approved entry?" },
                    { new Guid("7b181123-b765-2047-838f-02a6733014de"), "ar", new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "تراجع" },
                    { new Guid("8ef0379e-38ee-015d-7acb-862575173b37"), "ar", new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "حُذف." },
                    { new Guid("a77e7b43-a910-603b-c0b9-fcf5735c0f84"), "en", new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "Undo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("009cb907-266d-c459-8d48-670708f7e3e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d9788c3-5124-5869-63ae-8644ef92f74a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1e79722a-e349-6e16-bf81-1e6d89752e97"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4faf3b4a-e2c7-30ab-f1e0-5347398e6127"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5bc47b9c-6b4e-b110-7c46-44c7b4491220"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7b181123-b765-2047-838f-02a6733014de"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ef0379e-38ee-015d-7acb-862575173b37"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a77e7b43-a910-603b-c0b9-fcf5735c0f84"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"));
        }
    }
}
