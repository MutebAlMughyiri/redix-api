using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class WaitingScreenWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("baf1facc-15a9-ec26-6ec0-142223e611f5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd28f535-f3d8-84ba-a0b5-0a8440423208"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "", "offline.autoSend" },
                    { new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "", "offline.withFile" },
                    { new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "", "offline.sendNow" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01f958b8-b855-db32-d142-c5383d9e71c8"), "ar", new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "مع فاتورة" },
                    { new Guid("09afb887-8981-690b-b38b-fb47566b827e"), "ar", new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "تُرسل هذه القيود وحدها حين تعود الشبكة." },
                    { new Guid("0cb93e87-3cc1-4e86-065e-c620093a7cb2"), "ar", new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "إرسال الآن" },
                    { new Guid("1eea6e9f-ed9f-533d-7926-0c9f1598add0"), "en", new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "These entries are sent by themselves once the network is back." },
                    { new Guid("4e4da4bd-b738-b38d-65d4-a8485cb49214"), "en", new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "With a bill" },
                    { new Guid("bfffb476-e097-b544-4e6b-cbe22bbfd7d4"), "en", new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "Send now" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01f958b8-b855-db32-d142-c5383d9e71c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09afb887-8981-690b-b38b-fb47566b827e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0cb93e87-3cc1-4e86-065e-c620093a7cb2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1eea6e9f-ed9f-533d-7926-0c9f1598add0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e4da4bd-b738-b38d-65d4-a8485cb49214"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bfffb476-e097-b544-4e6b-cbe22bbfd7d4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "", "offline.waiting" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("baf1facc-15a9-ec26-6ec0-142223e611f5"), "en", new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "Waiting for the network" },
                    { new Guid("dd28f535-f3d8-84ba-a0b5-0a8440423208"), "ar", new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "بانتظار الشبكة" }
                });
        }
    }
}
