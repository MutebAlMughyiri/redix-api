using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ContractTableOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("161d8696-af3d-d208-7c39-9b6b65cda057"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54ecf1ad-1033-8a8f-b26f-ae5546ceafca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc8c938f-db5f-9c86-8816-7a2ef1af490f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb46ecc9-bc7f-cba1-b232-bea10ee8b09b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0ab1024-2f96-9cf1-926e-57734e20391c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fde190f4-ccbb-ddb4-3d7c-9863cc08a7ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // A contract takes its place after its item's subgroups, contracts in the order they were added, as the sheet printed them until now.
            migrationBuilder.Sql("""
                UPDATE c SET SortOrder = ranked.Place
                FROM Contracts c
                JOIN (
                    SELECT k.Id, (SELECT ISNULL(MAX(s.SortOrder), -1) FROM Subgroups s WHERE s.ItemId = k.ItemId) + ROW_NUMBER() OVER (PARTITION BY k.ItemId ORDER BY k.Id) AS Place
                    FROM Contracts k
                ) ranked ON ranked.Id = c.Id;
                """);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "", "categories.tablesHint" },
                    { new Guid("350662dd-a114-636f-e070-06a358c487b9"), "", "categories.tables" },
                    { new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "", "categories.tableContract" },
                    { new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "", "categories.tableRevenue" },
                    { new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "", "categories.tablePurchases" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("04765bdd-0b0e-34a1-7da3-17a41856b902"), "ar", new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "دفعات عقد" },
                    { new Guid("4f2eb84e-eaaf-aebd-a72e-d21a44f6fee0"), "en", new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "Revenue" },
                    { new Guid("62004d03-a8ee-83c5-3037-6918571c762a"), "en", new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "The item's sheet prints its tables in this order. Drag a table to move it." },
                    { new Guid("7578fbbd-7e55-11e4-5b50-403bed9e3cc2"), "en", new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "Contract payments" },
                    { new Guid("77d396c5-4df5-b053-ca34-0cc68a360bc1"), "en", new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "Purchases" },
                    { new Guid("885236d2-1ea5-dd20-cb66-d1efc37f2970"), "ar", new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "إيرادات" },
                    { new Guid("946bb534-8991-a648-dd54-288248c4eeff"), "ar", new Guid("350662dd-a114-636f-e070-06a358c487b9"), "جداول البند" },
                    { new Guid("9a3c784d-2c8b-7ba4-ce2e-f1628ae9d6e6"), "en", new Guid("350662dd-a114-636f-e070-06a358c487b9"), "Tables" },
                    { new Guid("c0dac685-2657-4194-7881-9d6e1a67aa95"), "ar", new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "تُطبع جداول البند في ورقته بهذا الترتيب. اسحب الجدول لتغيير مكانه." },
                    { new Guid("df521f26-396a-8be5-8eee-ad658f66c2af"), "ar", new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "مشتريات" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04765bdd-0b0e-34a1-7da3-17a41856b902"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f2eb84e-eaaf-aebd-a72e-d21a44f6fee0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62004d03-a8ee-83c5-3037-6918571c762a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7578fbbd-7e55-11e4-5b50-403bed9e3cc2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77d396c5-4df5-b053-ca34-0cc68a360bc1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("885236d2-1ea5-dd20-cb66-d1efc37f2970"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("946bb534-8991-a648-dd54-288248c4eeff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a3c784d-2c8b-7ba4-ce2e-f1628ae9d6e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0dac685-2657-4194-7881-9d6e1a67aa95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df521f26-396a-8be5-8eee-ad658f66c2af"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("350662dd-a114-636f-e070-06a358c487b9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"));

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Contracts");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "", "categories.subgroupsHint" },
                    { new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "", "categories.noSubgroups" },
                    { new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "", "categories.subgroups" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("161d8696-af3d-d208-7c39-9b6b65cda057"), "en", new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "Subgroups" },
                    { new Guid("54ecf1ad-1033-8a8f-b26f-ae5546ceafca"), "ar", new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "المجموعات الفرعية" },
                    { new Guid("cc8c938f-db5f-9c86-8816-7a2ef1af490f"), "en", new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "Each subgroup prints as its own table in the item's report. An entry with no subgroup goes to the item's general group." },
                    { new Guid("eb46ecc9-bc7f-cba1-b232-bea10ee8b09b"), "ar", new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "تُطبع كل مجموعة فرعية جدولاً مستقلاً في تقرير البند. والقيد بلا مجموعة يدخل في المجموعة العامة للبند." },
                    { new Guid("f0ab1024-2f96-9cf1-926e-57734e20391c"), "en", new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "This item has no subgroups." },
                    { new Guid("fde190f4-ccbb-ddb4-3d7c-9863cc08a7ca"), "ar", new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "لا توجد مجموعات فرعية لهذا البند." }
                });
        }
    }
}
