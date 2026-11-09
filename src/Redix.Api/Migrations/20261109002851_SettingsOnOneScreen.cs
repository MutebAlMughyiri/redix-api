using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SettingsOnOneScreen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8fed7ed6-0863-202e-42c8-660b835b2271"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c33dd6a2-b653-f564-0dfc-7c5be9117567"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "", "permissions.mine" },
                    { new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "", "permissions.mineHint" },
                    { new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "", "projects.importSettings" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("066e36b0-f0c2-e3ac-88ab-5c0fa06d8688"), "en", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "Import project settings" },
                    { new Guid("27589247-c918-bdbf-ba5a-5014b4d3537f"), "ar", new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "ما تستطيعه في هذا المشروع. يغيّره مدير المشروع فقط." },
                    { new Guid("3f1f6a1f-06c3-9d56-47e4-724c1b5644e1"), "en", new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "My permissions" },
                    { new Guid("4d64a021-be54-2a4b-95e2-de8b150566bb"), "ar", new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "صلاحياتي" },
                    { new Guid("8fe7cd36-03ff-f7db-ac7a-b3c8fd0f3e03"), "en", new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "What you may do in this project. Only the project manager changes it." },
                    { new Guid("9ea691bc-4176-625f-6d08-f3a9ad22a4f5"), "ar", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "استيراد إعدادات المشروع" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("066e36b0-f0c2-e3ac-88ab-5c0fa06d8688"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27589247-c918-bdbf-ba5a-5014b4d3537f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f1f6a1f-06c3-9d56-47e4-724c1b5644e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d64a021-be54-2a4b-95e2-de8b150566bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8fe7cd36-03ff-f7db-ac7a-b3c8fd0f3e03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ea691bc-4176-625f-6d08-f3a9ad22a4f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "", "myAccount.title" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("8fed7ed6-0863-202e-42c8-660b835b2271"), "en", new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "My account" },
                    { new Guid("c33dd6a2-b653-f564-0dfc-7c5be9117567"), "ar", new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "حسابي" }
                });
        }
    }
}
