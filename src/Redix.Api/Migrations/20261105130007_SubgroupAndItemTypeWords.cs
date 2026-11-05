using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SubgroupAndItemTypeWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("538f3cd3-08a5-34d3-84c6-ba44f8dbe5c0"), "", "subgroups.deleteQuestion" },
                    { new Guid("715a4055-0c6f-a9c3-0a38-1af080e61d86"), "", "subgroups.deleteExplain" },
                    { new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "", "categories.subgroupInUse" },
                    { new Guid("e74ea481-4167-704c-56e4-d84d5469f3c5"), "", "subgroups.delete" },
                    { new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "", "categories.changeType" },
                    { new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "", "categories.itemHasEntries" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c79b3f8-f4eb-4a61-43c0-62adf90e2c96"),
                column: "Value",
                value: "The item's general group cannot be renamed or removed.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea2b89c2-6ba0-2cfa-8597-955a77052eaf"),
                column: "Value",
                value: "المجموعة العامة للبند لا يُعدَّل اسمها ولا تُحذف.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("03e6fd0e-410f-cf71-33b2-d8f3a59c5f4b"), "en", new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "Entries use this subgroup, so it cannot be removed." },
                    { new Guid("06165536-e25c-a27a-e773-144bdf8d66fd"), "en", new Guid("715a4055-0c6f-a9c3-0a38-1af080e61d86"), "It leaves this item. A subgroup that entries use cannot be removed." },
                    { new Guid("289c88c8-4e1b-bb42-de45-d60c1a2af8f3"), "ar", new Guid("e74ea481-4167-704c-56e4-d84d5469f3c5"), "حذف المجموعة الفرعية" },
                    { new Guid("2f55b30b-3fa7-74ca-b2bf-7906150e78d8"), "en", new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "Change type" },
                    { new Guid("84f84050-1de7-71bd-cfb6-3308e4bcf250"), "en", new Guid("538f3cd3-08a5-34d3-84c6-ba44f8dbe5c0"), "Remove this subgroup?" },
                    { new Guid("8f5e3085-0901-5721-a7b4-959fa2e4ed46"), "ar", new Guid("715a4055-0c6f-a9c3-0a38-1af080e61d86"), "تُحذف من هذا البند. ولا تُحذف مجموعة فرعية عليها قيود." },
                    { new Guid("ae52cbd8-f8c4-48f3-ece8-9a4bc6b23fa8"), "en", new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "This item has entries, so its type cannot change." },
                    { new Guid("b2cded1d-07e0-eff2-6a6d-bfe6faee7fc7"), "ar", new Guid("538f3cd3-08a5-34d3-84c6-ba44f8dbe5c0"), "حذف هذه المجموعة الفرعية؟" },
                    { new Guid("bd451b52-7f45-f998-5f7d-02061883875a"), "ar", new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "تغيير النوع" },
                    { new Guid("cfb7da61-781a-08e3-db09-46e586a455c0"), "ar", new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "هذا البند عليه قيود، فلا يتغير نوعه." },
                    { new Guid("dad5370a-e85b-e4a7-d61f-bf177539c682"), "en", new Guid("e74ea481-4167-704c-56e4-d84d5469f3c5"), "Remove subgroup" },
                    { new Guid("f380c4e6-8af7-55c3-1b9e-9e667ea8b473"), "ar", new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "هذه المجموعة الفرعية عليها قيود، فلا تُحذف." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03e6fd0e-410f-cf71-33b2-d8f3a59c5f4b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06165536-e25c-a27a-e773-144bdf8d66fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("289c88c8-4e1b-bb42-de45-d60c1a2af8f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f55b30b-3fa7-74ca-b2bf-7906150e78d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("84f84050-1de7-71bd-cfb6-3308e4bcf250"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f5e3085-0901-5721-a7b4-959fa2e4ed46"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae52cbd8-f8c4-48f3-ece8-9a4bc6b23fa8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2cded1d-07e0-eff2-6a6d-bfe6faee7fc7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd451b52-7f45-f998-5f7d-02061883875a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cfb7da61-781a-08e3-db09-46e586a455c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dad5370a-e85b-e4a7-d61f-bf177539c682"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f380c4e6-8af7-55c3-1b9e-9e667ea8b473"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("538f3cd3-08a5-34d3-84c6-ba44f8dbe5c0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("715a4055-0c6f-a9c3-0a38-1af080e61d86"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e74ea481-4167-704c-56e4-d84d5469f3c5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c79b3f8-f4eb-4a61-43c0-62adf90e2c96"),
                column: "Value",
                value: "The item's general group has no name to change.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea2b89c2-6ba0-2cfa-8597-955a77052eaf"),
                column: "Value",
                value: "المجموعة العامة للبند ليس لها اسم يُغيَّر.");
        }
    }
}
