using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntriesShownWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a4b402bc-e919-54e5-baf3-9bbf207ff9cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a89b5b26-0a78-a41b-4356-c49f7683e6b0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("53038282-4269-1366-be57-56958dad11a6"), "", "generalSettings.entriesShown" },
                    { new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "", "entries.showMore" },
                    { new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "", "generalSettings.entriesShownHint" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("555eb16f-bbf6-874c-fa77-89bba965b2ee"), "en", new Guid("53038282-4269-1366-be57-56958dad11a6"), "Entries shown" },
                    { new Guid("64276b86-36c3-7d98-efa2-b35f315bcbe9"), "ar", new Guid("53038282-4269-1366-be57-56958dad11a6"), "عدد القيود المعروضة" },
                    { new Guid("76d8affa-b976-88a7-850c-cb610593e451"), "ar", new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "عدد القيود في الرئيسية، وعدد ما يضيفه كل «عرض المزيد»." },
                    { new Guid("d153a608-8621-9b9c-b829-d9b4f844a1b6"), "ar", new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "عرض المزيد" },
                    { new Guid("ea1779aa-19c7-fe8f-d884-f6e8146264a0"), "en", new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "How many entries the home shows, and how many each Show more adds." },
                    { new Guid("eb88ea9a-4dc7-5cc7-a84c-8facbb35040c"), "en", new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "Show more" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("555eb16f-bbf6-874c-fa77-89bba965b2ee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64276b86-36c3-7d98-efa2-b35f315bcbe9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76d8affa-b976-88a7-850c-cb610593e451"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d153a608-8621-9b9c-b829-d9b4f844a1b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea1779aa-19c7-fe8f-d884-f6e8146264a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb88ea9a-4dc7-5cc7-a84c-8facbb35040c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53038282-4269-1366-be57-56958dad11a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "", "entries.seeAll" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("a4b402bc-e919-54e5-baf3-9bbf207ff9cc"), "ar", new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "كل القيود" },
                    { new Guid("a89b5b26-0a78-a41b-4356-c49f7683e6b0"), "en", new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "All entries" }
                });
        }
    }
}
