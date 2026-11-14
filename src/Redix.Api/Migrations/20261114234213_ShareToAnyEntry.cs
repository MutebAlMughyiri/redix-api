using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ShareToAnyEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f56e07d-596d-ef3f-827a-f27a040975cf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40a32818-66b6-8438-813f-65754333fd20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68d3dfdf-8118-7a8c-49b9-fcb433f4533a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6bacabe1-5d51-8314-c738-7229c9956a02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73774f15-36cb-daba-df7e-e30ff27ce97c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("804a5fd6-855f-8499-9de1-3c6790f551c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a313c2bf-1e85-873a-4b19-41aad6bbb75f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b784bf3d-ca05-0a9f-f3d4-d5ed4dd8023b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("c024bb98-9f03-273d-c391-aeaf45b8c8c6"), "", "share.matches" },
                    { new Guid("ed149f97-58a9-24a9-6074-3b093362b6a3"), "", "share.searchEntries" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24787303-6dcc-7ffa-2fab-61938696a525"),
                column: "Value",
                value: "The file was attached to “{0}”.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b62d510d-1bc8-8dc8-8ca5-d19c3296fd86"),
                column: "Value",
                value: "أُرفق الملف بـ«{0}».");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("29168218-1be6-b6f3-1ad2-30cd165a1c69"), "ar", new Guid("ed149f97-58a9-24a9-6074-3b093362b6a3"), "ابحث في القيود" },
                    { new Guid("619ed52f-6950-7133-4095-504203bfee1b"), "ar", new Guid("c024bb98-9f03-273d-c391-aeaf45b8c8c6"), "يطابق الفاتورة" },
                    { new Guid("b9507cf0-bc84-7764-20dc-edc07fd79dac"), "en", new Guid("c024bb98-9f03-273d-c391-aeaf45b8c8c6"), "Matches the bill" },
                    { new Guid("fba69549-0814-07d4-75e1-68ca613816a7"), "en", new Guid("ed149f97-58a9-24a9-6074-3b093362b6a3"), "Search the entries" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29168218-1be6-b6f3-1ad2-30cd165a1c69"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("619ed52f-6950-7133-4095-504203bfee1b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b9507cf0-bc84-7764-20dc-edc07fd79dac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fba69549-0814-07d4-75e1-68ca613816a7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c024bb98-9f03-273d-c391-aeaf45b8c8c6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ed149f97-58a9-24a9-6074-3b093362b6a3"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "", "share.expense" },
                    { new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "", "share.chooseItem" },
                    { new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "", "share.empty" },
                    { new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "", "share.matching" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24787303-6dcc-7ffa-2fab-61938696a525"),
                column: "Value",
                value: "The invoice was attached.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b62d510d-1bc8-8dc8-8ca5-d19c3296fd86"),
                column: "Value",
                value: "أُرفقت الفاتورة.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("3f56e07d-596d-ef3f-827a-f27a040975cf"), "ar", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "لا مصروف بلا فاتورة تحت هذا البند." },
                    { new Guid("40a32818-66b6-8438-813f-65754333fd20"), "ar", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "اختر البند لتظهر مصروفاته." },
                    { new Guid("68d3dfdf-8118-7a8c-49b9-fcb433f4533a"), "en", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "Choose the item to see its expenses." },
                    { new Guid("6bacabe1-5d51-8314-c738-7229c9956a02"), "en", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "Matching" },
                    { new Guid("73774f15-36cb-daba-df7e-e30ff27ce97c"), "ar", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "مطابق" },
                    { new Guid("804a5fd6-855f-8499-9de1-3c6790f551c8"), "ar", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "المصروف" },
                    { new Guid("a313c2bf-1e85-873a-4b19-41aad6bbb75f"), "en", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "No expense without an invoice under this item." },
                    { new Guid("b784bf3d-ca05-0a9f-f3d4-d5ed4dd8023b"), "en", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "Expense" }
                });
        }
    }
}
