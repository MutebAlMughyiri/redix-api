using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OneDeductionWordAndAnOptionalReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2be95438-64af-07aa-0f70-3726bed1accf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aea6dc08-82c8-66dd-0b18-d8ffe2d56221"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3444d843-0b82-f08c-f421-f027bf0cf64c"),
                column: "Value",
                value: "سند الاستلام أو إيصال التحويل");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62a48d5e-9f21-2f32-bed4-30bbca2d8e16"),
                column: "Value",
                value: "Receipt voucher or transfer receipt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "", "contractorPayments.deductionNeedsNotes" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3444d843-0b82-f08c-f421-f027bf0cf64c"),
                column: "Value",
                value: "سند الاستلام أو إيصال التحويل (إلزامي)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62a48d5e-9f21-2f32-bed4-30bbca2d8e16"),
                column: "Value",
                value: "Receipt voucher or transfer receipt (required)");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2be95438-64af-07aa-0f70-3726bed1accf"), "en", new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "A negative amount is a deduction: say why in the notes." },
                    { new Guid("aea6dc08-82c8-66dd-0b18-d8ffe2d56221"), "ar", new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "المبلغ السالب خصم: اذكر سببه في الملاحظات." }
                });
        }
    }
}
