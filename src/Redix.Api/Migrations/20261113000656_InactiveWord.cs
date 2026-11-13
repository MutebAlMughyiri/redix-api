using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InactiveWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("e56fee5b-cff2-ce1d-9364-2dfba2e9ff97"), "", "projects.inactive" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("10be42a1-8a46-056e-ea33-79958d378069"), "en", new Guid("e56fee5b-cff2-ce1d-9364-2dfba2e9ff97"), "Your account is inactive in this project." },
                    { new Guid("b4cbadfc-4ed8-38e7-8e3e-d47c5393f107"), "ar", new Guid("e56fee5b-cff2-ce1d-9364-2dfba2e9ff97"), "حسابك غير مفعّل في هذا المشروع." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("10be42a1-8a46-056e-ea33-79958d378069"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4cbadfc-4ed8-38e7-8e3e-d47c5393f107"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e56fee5b-cff2-ce1d-9364-2dfba2e9ff97"));
        }
    }
}
