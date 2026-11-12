using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExportSettingsWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "", "projects.exportSettings" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("7277f98f-f4ab-08c2-7cdd-ed34a4cc3b8c"), "en", new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "Export project settings" },
                    { new Guid("e09c9942-0742-1b5d-1385-68735c76100c"), "ar", new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "تصدير إعدادات المشروع" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7277f98f-f4ab-08c2-7cdd-ed34a4cc3b8c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e09c9942-0742-1b5d-1385-68735c76100c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"));
        }
    }
}
