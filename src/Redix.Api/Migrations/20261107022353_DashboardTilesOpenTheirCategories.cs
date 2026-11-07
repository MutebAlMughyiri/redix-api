using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DashboardTilesOpenTheirCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71af5bb6-8cc2-7036-583a-61d5c4a48c73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77083b08-3985-7d75-9099-9639cd7e831e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "", "dashboard.revenueByCategory" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("3b814a1d-0103-ae4a-865f-6e94a1776f43"), "en", new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "Revenue by category" },
                    { new Guid("5cf73c67-94e3-866c-fa78-d1e14fc0bceb"), "ar", new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "الإيرادات حسب التصنيف" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b814a1d-0103-ae4a-865f-6e94a1776f43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5cf73c67-94e3-866c-fa78-d1e14fc0bceb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "", "dashboard.cost" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("71af5bb6-8cc2-7036-583a-61d5c4a48c73"), "ar", new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "التكلفة" },
                    { new Guid("77083b08-3985-7d75-9099-9639cd7e831e"), "en", new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "Cost" }
                });
        }
    }
}
