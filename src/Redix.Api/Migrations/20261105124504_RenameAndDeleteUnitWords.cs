using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameAndDeleteUnitWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "", "projects.unitInUse" },
                    { new Guid("48659bd6-f4bf-8125-08a9-775e6e52c0bb"), "", "projects.removeUnitExplain" },
                    { new Guid("93caa0c9-b3c8-2f69-7583-e7fe363eeaba"), "", "projects.removeUnitQuestion" },
                    { new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "", "projects.renameUnit" },
                    { new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "", "projects.rename" },
                    { new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "", "projects.sharedUnit" },
                    { new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "", "projects.save" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("151e7dd2-0a1c-5be2-accf-309ef2321dec"), "en", new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "Save" },
                    { new Guid("5232747e-71ea-3536-984e-a2d41608aca4"), "en", new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "Rename project" },
                    { new Guid("5308ff98-93ed-ddfa-846f-ac63511369e4"), "ar", new Guid("93caa0c9-b3c8-2f69-7583-e7fe363eeaba"), "حذف هذه الوحدة؟" },
                    { new Guid("6cb36bdf-aabd-ddf0-ffb6-dd62b558e5dd"), "en", new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "The shared unit cannot be renamed or deleted." },
                    { new Guid("9774b8eb-6c82-b6f0-b56b-92a58fb57467"), "en", new Guid("48659bd6-f4bf-8125-08a9-775e6e52c0bb"), "It leaves this project. A unit that entries or contracts use cannot be deleted." },
                    { new Guid("a43e219c-95b9-8a61-0643-8744929da343"), "en", new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "Entries or contracts use this unit, so it cannot be deleted." },
                    { new Guid("b0777b87-cdcc-965d-80a4-b5f3a843b845"), "ar", new Guid("48659bd6-f4bf-8125-08a9-775e6e52c0bb"), "تُحذف من هذا المشروع. ولا تُحذف وحدة عليها قيود أو عقود." },
                    { new Guid("c0e152d4-1ecf-107f-aac7-e708dad6c1f8"), "ar", new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "تعديل اسم المشروع" },
                    { new Guid("c52a61be-a397-1527-3563-a20bbbb342d5"), "ar", new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "حفظ" },
                    { new Guid("cae4d12f-1e01-bfe4-a70c-239463019d5f"), "ar", new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "الوحدة المشتركة لا يُعدَّل اسمها ولا تُحذف." },
                    { new Guid("cfddfb4c-09a6-4628-c7c8-4461c40df153"), "ar", new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "تعديل اسم الوحدة" },
                    { new Guid("d1dd8d2a-d8a3-6956-539d-207fdc82e6fa"), "en", new Guid("93caa0c9-b3c8-2f69-7583-e7fe363eeaba"), "Delete this unit?" },
                    { new Guid("d55fb92a-b4cc-448d-ce63-1dfa92003294"), "en", new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "Rename unit" },
                    { new Guid("e0c936f7-6e01-fc2c-ea48-dbdee78b66b8"), "ar", new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "هذه الوحدة عليها قيود أو عقود، فلا تُحذف." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("151e7dd2-0a1c-5be2-accf-309ef2321dec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5232747e-71ea-3536-984e-a2d41608aca4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5308ff98-93ed-ddfa-846f-ac63511369e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6cb36bdf-aabd-ddf0-ffb6-dd62b558e5dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9774b8eb-6c82-b6f0-b56b-92a58fb57467"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a43e219c-95b9-8a61-0643-8744929da343"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0777b87-cdcc-965d-80a4-b5f3a843b845"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0e152d4-1ecf-107f-aac7-e708dad6c1f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c52a61be-a397-1527-3563-a20bbbb342d5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cae4d12f-1e01-bfe4-a70c-239463019d5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cfddfb4c-09a6-4628-c7c8-4461c40df153"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d1dd8d2a-d8a3-6956-539d-207fdc82e6fa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d55fb92a-b4cc-448d-ce63-1dfa92003294"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0c936f7-6e01-fc2c-ea48-dbdee78b66b8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("48659bd6-f4bf-8125-08a9-775e6e52c0bb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("93caa0c9-b3c8-2f69-7583-e7fe363eeaba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"));
        }
    }
}
