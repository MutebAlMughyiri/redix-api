using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Suppliers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VatNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDefaultItems",
                columns: table => new
                {
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDefaultItems", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_SupplierDefaultItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierDefaultItems_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "", "suppliers.unknownItem" },
                    { new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "", "suppliers.holder" },
                    { new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "", "suppliers.enable" },
                    { new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "", "suppliers.new" },
                    { new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "", "suppliers.invalidName" },
                    { new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "", "suppliers.refused" },
                    { new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "", "suppliers.empty" },
                    { new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "", "suppliers.vatNumber" },
                    { new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "", "suppliers.loadFailed" },
                    { new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "", "suppliers.edit" },
                    { new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "", "suppliers.invalidVatNumber" },
                    { new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "", "suppliers.save" },
                    { new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "", "suppliers.invalidMobile" },
                    { new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "", "suppliers.disabled" },
                    { new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "", "suppliers.disable" },
                    { new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "", "suppliers.name" },
                    { new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "", "suppliers.projectFinished" },
                    { new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "", "suppliers.defaultItem" },
                    { new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "", "suppliers.vatNumberTaken" },
                    { new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "", "suppliers.noConnection" },
                    { new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "", "suppliers.pendingApproval" },
                    { new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "", "suppliers.mobile" },
                    { new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "", "suppliers.noDefaultItem" },
                    { new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "", "suppliers.notVatRegistered" },
                    { new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "", "suppliers.title" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("071fa6a0-78b2-c504-9999-af8c61367f80"), "en", new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "This change was refused." },
                    { new Guid("11ac03e8-39e7-43d0-bd2e-b1324d4948e6"), "en", new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "Mobile (optional)" },
                    { new Guid("24623385-75dc-1b1d-f09d-4275a629af8f"), "en", new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "Enter a name of 1 to 100 characters." },
                    { new Guid("35742bd6-0307-d220-896c-2547f138bff5"), "ar", new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "رُفض هذا التغيير." },
                    { new Guid("36d8c20b-c689-f2dd-ab27-3f928773bb2f"), "ar", new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "اكتب رقم جوال من 20 حرفاً على الأكثر." },
                    { new Guid("3c3356dc-3043-1024-6602-f7c34a0928a4"), "en", new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "Save" },
                    { new Guid("4526860c-d2fe-f2ea-1eac-2ae72eecea42"), "en", new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "Could not read the suppliers. Check the network and try again." },
                    { new Guid("46b64c20-2b81-7a57-d946-fd080564a7a7"), "en", new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "Existing supplier:" },
                    { new Guid("46d98895-fe5b-ddb2-e53f-43a93274f540"), "ar", new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "الجوال (اختياري)" },
                    { new Guid("4715a33f-814c-637f-2761-52c6980886ac"), "ar", new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "البند الافتراضي (اختياري)" },
                    { new Guid("48699ff2-feb6-f8a3-65b5-89452c221355"), "ar", new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "غير مسجل بالضريبة" },
                    { new Guid("4bf7c0c2-aeb6-3336-fb8b-00349b3b63c4"), "en", new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "A VAT number is 15 digits, starting and ending with 3." },
                    { new Guid("52bc5dd2-764d-ebd8-dbee-37a32bd0a6e2"), "en", new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "VAT number (15 digits)" },
                    { new Guid("60383cf9-62ec-5f53-1e2b-6cff79449eb3"), "en", new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "Not VAT-registered" },
                    { new Guid("63f8f177-9942-10b6-6829-d8051cabd3f6"), "en", new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "No connection to the server. Try again." },
                    { new Guid("66227c72-1351-8dfc-69e2-97d3f4fb7271"), "ar", new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "اسم المورد" },
                    { new Guid("663136a2-da1f-da9f-4642-d3c32e267abf"), "ar", new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "هذا البند ليس في هذا المشروع." },
                    { new Guid("73ca43f1-8e42-9306-ef47-be4891a43920"), "en", new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "No suppliers." },
                    { new Guid("752af740-649a-e8af-ce73-3d0e9c1d0b16"), "ar", new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "تعذّرت قراءة الموردين. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("7d7142f1-4e13-9a45-ec84-f4faeccd11bb"), "en", new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "Disable" },
                    { new Guid("83fc62c8-1559-2c4c-2e5c-7ea95ff738e1"), "ar", new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "تفعيل" },
                    { new Guid("847c3a84-117e-ad30-29c4-1258d291ce1d"), "ar", new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "المورد الموجود:" },
                    { new Guid("85fb396c-3eff-6813-7c0a-c5ea328b8978"), "ar", new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "معطّل" },
                    { new Guid("895ee1c1-759c-d715-05f7-f271ab4bae15"), "en", new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "This VAT number already belongs to another supplier." },
                    { new Guid("89cf7cc3-dd8e-19b2-b9dd-eaccbf453565"), "ar", new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "الرقم الضريبي (15 رقماً)" },
                    { new Guid("9096a3b0-ca65-08d0-32b3-e9e28031682c"), "en", new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "Pending approval" },
                    { new Guid("90d5a9ae-4874-5667-a181-8b26575f1585"), "ar", new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "لا يوجد موردون." },
                    { new Guid("93edac82-6c8c-8291-b19d-64da58f9be72"), "ar", new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("9a13e3bc-22f3-786b-a963-311d54740fd4"), "ar", new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "بدون بند افتراضي" },
                    { new Guid("a8a9e2fa-1432-ae6c-4a58-d26c39d08af4"), "en", new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "Edit supplier" },
                    { new Guid("adfacaa2-2fa0-7ed2-7bae-8aeda504f4c8"), "ar", new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "تعديل المورد" },
                    { new Guid("af50ad36-e9a3-998a-e935-02dc53d0d6ca"), "en", new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "Suppliers" },
                    { new Guid("aff7468c-16c7-951f-64d5-f5334986c495"), "en", new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "Add supplier" },
                    { new Guid("b3066af4-7e85-5095-a251-39f799f99fb4"), "ar", new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "الموردون" },
                    { new Guid("b537132f-7209-b701-ca9a-68e307f410e5"), "ar", new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "إضافة مورد" },
                    { new Guid("b641739c-7ebd-1d23-8003-c810aabd7db7"), "en", new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "Disabled" },
                    { new Guid("b8f89f3c-cc3f-066c-eb00-2997b444efaf"), "ar", new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "الرقم الضريبي 15 رقماً، يبدأ بالرقم 3 وينتهي به." },
                    { new Guid("bba8a844-a32c-636f-5cc5-668db02c3193"), "en", new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "Default item (optional)" },
                    { new Guid("bc9a7f02-eedd-add1-96e6-cc8b0fb2a04c"), "ar", new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("c2ce3aec-ea61-478f-c365-054426c5c54b"), "ar", new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "بانتظار الاعتماد" },
                    { new Guid("ca53b9e7-fec4-993d-1fb6-a1c8b2b7d39c"), "ar", new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "حفظ" },
                    { new Guid("d24f07e9-da5b-373f-d492-871258154c78"), "en", new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "Enter a mobile of up to 20 characters." },
                    { new Guid("d493ade0-b55e-bceb-90d3-2150965e9689"), "ar", new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "تعطيل" },
                    { new Guid("d5615cd2-56f0-9ba8-49f3-c895c4999b67"), "en", new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "Enable" },
                    { new Guid("d9349297-5a8a-54ff-af18-aa2487e8e9cb"), "en", new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "That item is not in this project." },
                    { new Guid("dbb25a89-ddf0-72f1-c985-e645573ae3b5"), "ar", new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "هذا الرقم الضريبي لمورد آخر." },
                    { new Guid("e1f480fb-09d9-7e1b-a1d5-7c4b8c163e33"), "ar", new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("e4b27f47-6391-5082-1c40-9983b685b345"), "en", new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "This project is finished, so nothing in it changes." },
                    { new Guid("eb12f83c-0111-7126-047d-d7f7dbe69efd"), "en", new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "Supplier name" },
                    { new Guid("fb9de578-4f34-3b76-9072-6d50ea72aff1"), "en", new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "No default item" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDefaultItems_ItemId",
                table: "SupplierDefaultItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedByUserId",
                table: "Suppliers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ProjectId_VatNumber",
                table: "Suppliers",
                columns: new[] { "ProjectId", "VatNumber" },
                unique: true,
                filter: "[VatNumber] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierDefaultItems");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("071fa6a0-78b2-c504-9999-af8c61367f80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("11ac03e8-39e7-43d0-bd2e-b1324d4948e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24623385-75dc-1b1d-f09d-4275a629af8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35742bd6-0307-d220-896c-2547f138bff5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36d8c20b-c689-f2dd-ab27-3f928773bb2f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3c3356dc-3043-1024-6602-f7c34a0928a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4526860c-d2fe-f2ea-1eac-2ae72eecea42"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46b64c20-2b81-7a57-d946-fd080564a7a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46d98895-fe5b-ddb2-e53f-43a93274f540"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4715a33f-814c-637f-2761-52c6980886ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48699ff2-feb6-f8a3-65b5-89452c221355"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bf7c0c2-aeb6-3336-fb8b-00349b3b63c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52bc5dd2-764d-ebd8-dbee-37a32bd0a6e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60383cf9-62ec-5f53-1e2b-6cff79449eb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63f8f177-9942-10b6-6829-d8051cabd3f6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("66227c72-1351-8dfc-69e2-97d3f4fb7271"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("663136a2-da1f-da9f-4642-d3c32e267abf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73ca43f1-8e42-9306-ef47-be4891a43920"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("752af740-649a-e8af-ce73-3d0e9c1d0b16"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d7142f1-4e13-9a45-ec84-f4faeccd11bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83fc62c8-1559-2c4c-2e5c-7ea95ff738e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("847c3a84-117e-ad30-29c4-1258d291ce1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85fb396c-3eff-6813-7c0a-c5ea328b8978"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("895ee1c1-759c-d715-05f7-f271ab4bae15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89cf7cc3-dd8e-19b2-b9dd-eaccbf453565"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9096a3b0-ca65-08d0-32b3-e9e28031682c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90d5a9ae-4874-5667-a181-8b26575f1585"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93edac82-6c8c-8291-b19d-64da58f9be72"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a13e3bc-22f3-786b-a963-311d54740fd4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8a9e2fa-1432-ae6c-4a58-d26c39d08af4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("adfacaa2-2fa0-7ed2-7bae-8aeda504f4c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af50ad36-e9a3-998a-e935-02dc53d0d6ca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aff7468c-16c7-951f-64d5-f5334986c495"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b3066af4-7e85-5095-a251-39f799f99fb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b537132f-7209-b701-ca9a-68e307f410e5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b641739c-7ebd-1d23-8003-c810aabd7db7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8f89f3c-cc3f-066c-eb00-2997b444efaf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bba8a844-a32c-636f-5cc5-668db02c3193"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc9a7f02-eedd-add1-96e6-cc8b0fb2a04c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2ce3aec-ea61-478f-c365-054426c5c54b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca53b9e7-fec4-993d-1fb6-a1c8b2b7d39c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d24f07e9-da5b-373f-d492-871258154c78"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d493ade0-b55e-bceb-90d3-2150965e9689"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5615cd2-56f0-9ba8-49f3-c895c4999b67"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9349297-5a8a-54ff-af18-aa2487e8e9cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dbb25a89-ddf0-72f1-c985-e645573ae3b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1f480fb-09d9-7e1b-a1d5-7c4b8c163e33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4b27f47-6391-5082-1c40-9983b685b345"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb12f83c-0111-7126-047d-d7f7dbe69efd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb9de578-4f34-3b76-9072-6d50ea72aff1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("32492e3e-b952-a698-a19c-971373c185e4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40f283f1-6f76-952a-01a6-fce856633438"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7313aa92-61f1-1081-7685-b3f184740dda"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("999558f1-820f-444d-8020-cf4d0aa34358"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"));
        }
    }
}
