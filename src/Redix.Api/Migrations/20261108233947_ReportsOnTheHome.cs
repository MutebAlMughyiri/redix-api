using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportsOnTheHome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60fadec6-7224-7fea-5d13-e021300d4c1a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2a96fd3-7748-de55-a631-d1631801442d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6709191c-fcba-8e91-2a28-d2b4ac929653"),
                column: "Value",
                value: "استرداد ضريبة القيمة المضافة");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91e412fa-0eab-013c-4d7b-b8a582d7ba0e"),
                column: "Value",
                value: "VAT refund");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bbdee45f-ebf9-9cc2-954b-dd9cbbbff299"),
                column: "Value",
                value: "تحديث");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6bef1aa-f1f7-fc19-c85f-8ecb287c9215"),
                column: "Value",
                value: "Update");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "", "reports.contracts" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6709191c-fcba-8e91-2a28-d2b4ac929653"),
                column: "Value",
                value: "كشف ضريبة القيمة المضافة");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91e412fa-0eab-013c-4d7b-b8a582d7ba0e"),
                column: "Value",
                value: "VAT statement");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bbdee45f-ebf9-9cc2-954b-dd9cbbbff299"),
                column: "Value",
                value: "تحديث الأرصدة الفعلية");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6bef1aa-f1f7-fc19-c85f-8ecb287c9215"),
                column: "Value",
                value: "Update actual balances");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("60fadec6-7224-7fea-5d13-e021300d4c1a"), "en", new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "Contracts and what remains" },
                    { new Guid("f2a96fd3-7748-de55-a631-d1631801442d"), "ar", new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "العقود والمتبقي" }
                });
        }
    }
}
