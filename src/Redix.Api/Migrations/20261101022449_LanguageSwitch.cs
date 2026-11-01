using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class LanguageSwitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "", "myAccount.title" },
                    { new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "", "myAccount.arabic" },
                    { new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "", "myAccount.language" },
                    { new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "", "myAccount.languageNeedsNetwork" },
                    { new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "", "myAccount.english" },
                    { new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "", "myAccount.changePassword" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("027ac94c-989d-d5e0-6f51-f61e965e007c"), "en", new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "العربية" },
                    { new Guid("0b5ef6da-d918-2900-a478-7a86826e0386"), "en", new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "Language" },
                    { new Guid("174383bb-088c-aada-edec-7ebab2952741"), "ar", new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "تغيير اللغة يحتاج إلى شبكة." },
                    { new Guid("206878f7-b25b-3829-67aa-b5b21bd45fb9"), "en", new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "English" },
                    { new Guid("5a97c78e-f491-6297-cb10-c5289e0bc149"), "ar", new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "اللغة" },
                    { new Guid("8fed7ed6-0863-202e-42c8-660b835b2271"), "en", new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "My account" },
                    { new Guid("a4fbb7ad-15f5-6f16-b848-c5986c260232"), "en", new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "Switching the language needs a network." },
                    { new Guid("a904eb69-401c-27a9-c536-528d86269314"), "ar", new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "العربية" },
                    { new Guid("c33dd6a2-b653-f564-0dfc-7c5be9117567"), "ar", new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"), "حسابي" },
                    { new Guid("db328cd6-399b-da9d-2bde-b64c7cbf076f"), "ar", new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "تغيير كلمة المرور" },
                    { new Guid("ed09774d-483c-6953-ee99-c08ad6da3024"), "ar", new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "English" },
                    { new Guid("eff07d2b-a70d-ca41-d55f-c958e3438efa"), "en", new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "Change password" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("027ac94c-989d-d5e0-6f51-f61e965e007c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b5ef6da-d918-2900-a478-7a86826e0386"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("174383bb-088c-aada-edec-7ebab2952741"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("206878f7-b25b-3829-67aa-b5b21bd45fb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a97c78e-f491-6297-cb10-c5289e0bc149"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8fed7ed6-0863-202e-42c8-660b835b2271"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a4fbb7ad-15f5-6f16-b848-c5986c260232"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a904eb69-401c-27a9-c536-528d86269314"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c33dd6a2-b653-f564-0dfc-7c5be9117567"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db328cd6-399b-da9d-2bde-b64c7cbf076f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ed09774d-483c-6953-ee99-c08ad6da3024"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eff07d2b-a70d-ca41-d55f-c958e3438efa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79732691-82a9-94d8-1e77-1e2e61f11150"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"));
        }
    }
}
