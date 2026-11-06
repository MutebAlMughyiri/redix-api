using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RevenueItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46701a14-d71b-7f64-dc21-57cbf7b6709d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d73a526a-092c-54e1-6034-fe598e82ba73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee3eb843-01f8-53d6-d36f-cdd5d9fa1de0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcdbb307-dd56-19c2-46df-76c6fa41d38d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c93c695f-77af-13cb-2846-908a246bb63a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"));

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "RevenueDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubgroupId",
                table: "RevenueDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"), true, "إيرادات أخرى", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 6 },
                    { new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"), true, "استرجاع ضرائب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 5 },
                    { new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"), true, "فلوس البيع المودعة بالحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 7 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "", "categories.typeOtherRevenue" },
                    { new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "", "categories.sideRevenue" },
                    { new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "", "columns.revenueTakesNoColumns" },
                    { new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "", "revenue.item" },
                    { new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "", "categories.side" },
                    { new Guid("ba5a6ecc-8c9e-5dd2-2a12-0ed466a103fc"), "", "revenue.unknownItem" },
                    { new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "", "categories.sideExpenses" },
                    { new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "", "categories.typeSale" },
                    { new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "", "categories.typeVatRefund" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba23d8c5-6e94-4825-6fce-77c835560065"),
                column: "Value",
                value: "Choose an expense item that takes purchases, or a revenue item.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0c57f3c-deab-8bba-ec5f-faabfc51c1fd"),
                column: "Value",
                value: "اختر بند مصروفات يقبل المشتريات، أو بند إيرادات.");

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "ShowsTablePercentages", "SortOrder", "Type" },
                values: new object[,]
                {
                    { new Guid("04c58bf8-0504-85a6-4245-d2d42aff4322"), new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"), true, "إيرادات أخرى", false, 0, (byte)5 },
                    { new Guid("6657f024-a507-a98a-da2d-7a4f4c083f38"), new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"), true, "البيع", false, 0, (byte)3 },
                    { new Guid("c6b680bb-5976-3505-4910-573ac8ec190c"), new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"), true, "استرجاع الضرائب", false, 0, (byte)4 }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00f3bd6e-7747-4829-d2a6-b5df1f1020f6"), "en", new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "A revenue item asks no columns." },
                    { new Guid("02d2904b-f846-23e7-ee77-8dc6c4786e77"), "ar", new Guid("ba5a6ecc-8c9e-5dd2-2a12-0ed466a103fc"), "اختر بند إيرادات من هذا المشروع؛ وفاتورة البيع تحتاج بند بيع." },
                    { new Guid("07c54d7d-0963-ab0b-5c18-ef8a5941a0b1"), "ar", new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "الجانب" },
                    { new Guid("0ade3f45-967d-b9d7-207d-29b741afd3f5"), "ar", new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "البند" },
                    { new Guid("124b8d0f-fae2-1a28-2117-d89166c86ef6"), "en", new Guid("ba5a6ecc-8c9e-5dd2-2a12-0ed466a103fc"), "Choose a revenue item of this project; a sales invoice needs a sale item." },
                    { new Guid("15782241-c102-6309-baac-1a2a461ea4ed"), "ar", new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "إيرادات" },
                    { new Guid("1db2a7f4-85c6-31e4-913a-4debaffc8b3e"), "ar", new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "إيرادات أخرى" },
                    { new Guid("2de39c18-51d9-1ba3-6999-8b678d3a0f49"), "ar", new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "بند الإيرادات لا يطلب أعمدة." },
                    { new Guid("501e6637-f41d-0ed9-34cd-645dd466ebe8"), "en", new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "Item" },
                    { new Guid("5a746b04-23f3-04dd-fe7e-c8dc90f18f07"), "en", new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "Side" },
                    { new Guid("5d4419e7-9875-0537-dd7a-9010446a6834"), "en", new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "VAT refund" },
                    { new Guid("61bf3f95-c591-a317-7f75-ba3ec8c90a02"), "ar", new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "استرجاع ضريبة" },
                    { new Guid("7a116032-67e6-9680-1cb9-7d52e88ab8c9"), "en", new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "Expenses" },
                    { new Guid("9867ceec-41c9-cf30-3f34-6610ddf4add3"), "en", new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "Other revenue" },
                    { new Guid("b4501234-fb15-2c4a-a472-b95b1eba84ea"), "en", new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "Revenue" },
                    { new Guid("ca063417-6d1b-586c-ba51-935dc5891835"), "ar", new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "بيع" },
                    { new Guid("dee8c0c9-4367-ec4b-ca1e-4a18947057b2"), "ar", new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "مصروفات" },
                    { new Guid("f3c40fde-2ce5-4b76-37d6-2d7e01d6249f"), "en", new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "Sale" }
                });

            migrationBuilder.InsertData(
                table: "Subgroups",
                columns: new[] { "Id", "ItemId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("03a246b8-d8fe-78f2-4617-4eaa516e05c7"), new Guid("c6b680bb-5976-3505-4910-573ac8ec190c"), "", 0 },
                    { new Guid("aec8376e-4f0b-3a1e-ebfa-01c56fbc6676"), new Guid("6657f024-a507-a98a-da2d-7a4f4c083f38"), "", 0 },
                    { new Guid("baa2771a-7861-a2ed-dc09-02e30607f220"), new Guid("04c58bf8-0504-85a6-4245-d2d42aff4322"), "", 0 }
                });

            // Every project that has a structure gets the template's three revenue categories, one item each, after its own categories; each revenue entry moves to the
            // item of its old kind (VatRefund 0, Sale 1, Other 2), and only then does the kind go. A project without a structure takes them from the template when approved.
            migrationBuilder.Sql("""
                DECLARE @kinds TABLE (Place int, CategoryName nvarchar(100), ItemName nvarchar(100), ItemType tinyint, OldType tinyint);
                INSERT @kinds VALUES (0, N'استرجاع ضرائب', N'استرجاع الضرائب', 4, 0), (1, N'إيرادات أخرى', N'إيرادات أخرى', 5, 2), (2, N'فلوس البيع المودعة بالحساب', N'البيع', 3, 1);
                DECLARE @project uniqueidentifier, @first int, @place int, @categoryName nvarchar(100), @itemName nvarchar(100), @itemType tinyint, @oldType tinyint;
                DECLARE @category uniqueidentifier, @item uniqueidentifier, @subgroup uniqueidentifier;
                DECLARE projects CURSOR LOCAL FOR SELECT p.Id FROM Projects p WHERE p.IsTemplate = 0 AND EXISTS (SELECT 1 FROM Categories c WHERE c.ProjectId = p.Id);
                OPEN projects;
                FETCH NEXT FROM projects INTO @project;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SELECT @first = MAX(SortOrder) + 1 FROM Categories WHERE ProjectId = @project;
                    DECLARE kinds CURSOR LOCAL FOR SELECT Place, CategoryName, ItemName, ItemType, OldType FROM @kinds ORDER BY Place;
                    OPEN kinds;
                    FETCH NEXT FROM kinds INTO @place, @categoryName, @itemName, @itemType, @oldType;
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        SELECT @category = NEWID(), @item = NEWID(), @subgroup = NEWID();
                        INSERT Categories (Id, ProjectId, Name, SortOrder, IsActive) VALUES (@category, @project, @categoryName, @first + @place, 1);
                        INSERT Items (Id, CategoryId, Name, Type, SortOrder, IsActive, ShowsTablePercentages) VALUES (@item, @category, @itemName, @itemType, 0, 1, 0);
                        INSERT Subgroups (Id, ItemId, Name, SortOrder) VALUES (@subgroup, @item, N'', 0);
                        UPDATE d SET ItemId = @item, SubgroupId = @subgroup FROM RevenueDetails d JOIN Entries e ON e.Id = d.EntryId WHERE e.ProjectId = @project AND d.Type = @oldType;
                        FETCH NEXT FROM kinds INTO @place, @categoryName, @itemName, @itemType, @oldType;
                    END
                    CLOSE kinds;
                    DEALLOCATE kinds;
                    FETCH NEXT FROM projects INTO @project;
                END
                CLOSE projects;
                DEALLOCATE projects;
                """);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "RevenueDetails");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDetails_ItemId",
                table: "RevenueDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDetails_SubgroupId",
                table: "RevenueDetails",
                column: "SubgroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueDetails_Items_ItemId",
                table: "RevenueDetails",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueDetails_Subgroups_SubgroupId",
                table: "RevenueDetails",
                column: "SubgroupId",
                principalTable: "Subgroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevenueDetails_Items_ItemId",
                table: "RevenueDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_RevenueDetails_Subgroups_SubgroupId",
                table: "RevenueDetails");

            migrationBuilder.DropIndex(
                name: "IX_RevenueDetails_ItemId",
                table: "RevenueDetails");

            migrationBuilder.DropIndex(
                name: "IX_RevenueDetails_SubgroupId",
                table: "RevenueDetails");

            migrationBuilder.DeleteData(
                table: "Subgroups",
                keyColumn: "Id",
                keyValue: new Guid("03a246b8-d8fe-78f2-4617-4eaa516e05c7"));

            migrationBuilder.DeleteData(
                table: "Subgroups",
                keyColumn: "Id",
                keyValue: new Guid("aec8376e-4f0b-3a1e-ebfa-01c56fbc6676"));

            migrationBuilder.DeleteData(
                table: "Subgroups",
                keyColumn: "Id",
                keyValue: new Guid("baa2771a-7861-a2ed-dc09-02e30607f220"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00f3bd6e-7747-4829-d2a6-b5df1f1020f6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02d2904b-f846-23e7-ee77-8dc6c4786e77"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07c54d7d-0963-ab0b-5c18-ef8a5941a0b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ade3f45-967d-b9d7-207d-29b741afd3f5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("124b8d0f-fae2-1a28-2117-d89166c86ef6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15782241-c102-6309-baac-1a2a461ea4ed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1db2a7f4-85c6-31e4-913a-4debaffc8b3e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2de39c18-51d9-1ba3-6999-8b678d3a0f49"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("501e6637-f41d-0ed9-34cd-645dd466ebe8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a746b04-23f3-04dd-fe7e-c8dc90f18f07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d4419e7-9875-0537-dd7a-9010446a6834"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61bf3f95-c591-a317-7f75-ba3ec8c90a02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a116032-67e6-9680-1cb9-7d52e88ab8c9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9867ceec-41c9-cf30-3f34-6610ddf4add3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4501234-fb15-2c4a-a472-b95b1eba84ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca063417-6d1b-586c-ba51-935dc5891835"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dee8c0c9-4367-ec4b-ca1e-4a18947057b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f3c40fde-2ce5-4b76-37d6-2d7e01d6249f"));

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("04c58bf8-0504-85a6-4245-d2d42aff4322"));

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("6657f024-a507-a98a-da2d-7a4f4c083f38"));

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("c6b680bb-5976-3505-4910-573ac8ec190c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ba5a6ecc-8c9e-5dd2-2a12-0ed466a103fc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c623b333-e28e-2731-732c-449230d2c0f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"));

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "RevenueDetails");

            migrationBuilder.DropColumn(
                name: "SubgroupId",
                table: "RevenueDetails");

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "RevenueDetails",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "", "revenue.type" },
                    { new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "", "revenue.other" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba23d8c5-6e94-4825-6fce-77c835560065"),
                column: "Value",
                value: "Choose an item that takes purchases.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0c57f3c-deab-8bba-ec5f-faabfc51c1fd"),
                column: "Value",
                value: "اختر بنداً يقبل المشتريات.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("46701a14-d71b-7f64-dc21-57cbf7b6709d"), "ar", new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "النوع" },
                    { new Guid("d73a526a-092c-54e1-6034-fe598e82ba73"), "en", new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "Type" },
                    { new Guid("ee3eb843-01f8-53d6-d36f-cdd5d9fa1de0"), "ar", new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "أخرى" },
                    { new Guid("fcdbb307-dd56-19c2-46df-76c6fa41d38d"), "en", new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "Other" }
                });
        }
    }
}
