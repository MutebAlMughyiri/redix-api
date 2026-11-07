using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ShorterDashboardWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"),
                column: "Value",
                value: "Revenue");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"),
                column: "Value",
                value: "الإيرادات");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0eeb6f4-1e7d-6e35-6b3f-5d5d99d032af"),
                column: "Value",
                value: "Expenses");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2e72137-d344-f48e-440b-4fb203a802f4"),
                column: "Value",
                value: "المصروفات");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"),
                column: "Value",
                value: "Total revenue");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"),
                column: "Value",
                value: "مجموع الإيرادات");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0eeb6f4-1e7d-6e35-6b3f-5d5d99d032af"),
                column: "Value",
                value: "Total expenses");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2e72137-d344-f48e-440b-4fb203a802f4"),
                column: "Value",
                value: "مجموع المصروفات");
        }
    }
}
