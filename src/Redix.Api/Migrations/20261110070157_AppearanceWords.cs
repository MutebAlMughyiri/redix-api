using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e98e96c5-7a16-1f35-ea6d-23225a6d6846"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb401c0d-0dee-c2f4-a2bf-e6b3b271ed14"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "", "generalSettings.themeLight" },
                    { new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "", "generalSettings.themeSystem" },
                    { new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "", "generalSettings.themeDark" },
                    { new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "", "generalSettings.appearance" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0dc91d60-c309-90a8-583b-29f24651409b"), "ar", new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "النظام" },
                    { new Guid("5cc6bba5-6613-2892-3e72-5feab68d8c5f"), "ar", new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "فاتح" },
                    { new Guid("76de0119-2172-a198-4763-844f1a9a05ed"), "en", new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "System" },
                    { new Guid("8a9070b1-2af3-6b99-a2c4-29b522cbfb76"), "ar", new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "المظهر" },
                    { new Guid("95a954d2-4cf5-497b-7b6d-34e8c7bfed75"), "en", new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "Appearance" },
                    { new Guid("a48e5aae-8851-95c8-b440-5fa518b12842"), "en", new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "Dark" },
                    { new Guid("b130dd15-6cf1-87eb-c083-ca3cf6135e1e"), "en", new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "Light" },
                    { new Guid("ed32b049-bd8e-372e-ebd7-931f3535d192"), "ar", new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "داكن" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0dc91d60-c309-90a8-583b-29f24651409b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5cc6bba5-6613-2892-3e72-5feab68d8c5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76de0119-2172-a198-4763-844f1a9a05ed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a9070b1-2af3-6b99-a2c4-29b522cbfb76"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95a954d2-4cf5-497b-7b6d-34e8c7bfed75"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a48e5aae-8851-95c8-b440-5fa518b12842"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b130dd15-6cf1-87eb-c083-ca3cf6135e1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ed32b049-bd8e-372e-ebd7-931f3535d192"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "", "generalSettings.darkMode" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("e98e96c5-7a16-1f35-ea6d-23225a6d6846"), "en", new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "Dark mode" },
                    { new Guid("eb401c0d-0dee-c2f4-a2bf-e6b3b271ed14"), "ar", new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "الوضع الداكن" }
                });
        }
    }
}
