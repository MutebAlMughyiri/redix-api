using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ItemColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefaultColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCore = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ShowInReports = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultColumns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCore = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ShowInReports = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemColumns_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "Options", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"), (byte)3, true, true, (byte)0, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"), (byte)4, false, false, (byte)2, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 11 },
                    { new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"), (byte)1, false, false, (byte)0, "quantity", "العدد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 6 },
                    { new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"), (byte)4, false, false, (byte)0, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 11 },
                    { new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"), (byte)4, true, false, (byte)1, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("1140a207-69a5-9621-94a2-448e30d34e91"), (byte)2, true, false, (byte)2, "discount", "الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"), (byte)2, true, false, (byte)0, "total", "الإجمالي", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 10 },
                    { new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"), (byte)2, false, false, (byte)2, "unitPrice", "سعر الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 5 },
                    { new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"), (byte)2, true, false, (byte)2, "amount", "المبلغ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 16 },
                    { new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"), (byte)4, true, false, (byte)1, "outsideContract", "داخل أو خارج العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 5 },
                    { new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"), (byte)0, false, false, (byte)0, "product", "الصنف", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 4 },
                    { new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"), (byte)1, false, false, (byte)2, "quantity", "العدد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 6 },
                    { new Guid("47874291-b290-c9e7-623d-b0d47f539b63"), (byte)0, false, false, (byte)1, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 6 },
                    { new Guid("4c72599a-620a-bd34-a9a4-837f4bce891a"), (byte)0, true, false, (byte)1, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("4c9d7893-60b7-58aa-d675-486e13466312"), (byte)0, false, false, (byte)2, "product", "الصنف", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 4 },
                    { new Guid("51d1ca64-8375-50f4-f73f-92b9a50d1dcf"), (byte)2, true, true, (byte)2, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"), (byte)4, true, false, (byte)2, "outsideContract", "داخل أو خارج العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 17 },
                    { new Guid("566c8741-e471-8621-606e-1ef5c743d5ca"), (byte)2, true, false, (byte)1, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 11 },
                    { new Guid("59042844-8ddf-6aaa-b31d-fe2c6886cb71"), (byte)4, false, false, (byte)0, "unit", "الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 },
                    { new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"), (byte)4, true, true, (byte)0, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 12 },
                    { new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"), (byte)4, true, true, (byte)1, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 4 },
                    { new Guid("64b82c9d-8230-e5f0-75ff-6cad318443e2"), (byte)2, true, false, (byte)1, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 10 },
                    { new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"), (byte)5, true, true, (byte)0, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 13 },
                    { new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"), (byte)4, true, true, (byte)1, "contract", "العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"), (byte)4, true, true, (byte)2, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"), (byte)3, true, true, (byte)2, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"), (byte)0, false, false, (byte)2, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 14 },
                    { new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"), (byte)5, true, true, (byte)1, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("9a6b3b3a-b69f-31db-e5e5-8183084d9db5"), (byte)4, false, false, (byte)2, "unit", "الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 },
                    { new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"), (byte)4, true, true, (byte)2, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 12 },
                    { new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"), (byte)2, true, true, (byte)1, "amount", "المبلغ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("b07fdd34-db99-3860-7f42-6a006a167443"), (byte)5, true, true, (byte)2, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 13 },
                    { new Guid("b81ba454-4df5-80d1-864d-e2b27314cc73"), (byte)0, true, true, (byte)0, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("bdf1c6d1-a35d-76e6-7e49-eef4a61be2c4"), (byte)2, true, true, (byte)0, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("c0baa49c-72e1-f645-7695-2c2441eee40d"), (byte)2, true, false, (byte)0, "discount", "الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("c45b6129-36e7-a373-23d2-a2a5dcb2e1d2"), (byte)2, true, false, (byte)0, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("c9a5b977-5bcd-ab11-87f3-2fd7c0175290"), (byte)0, true, true, (byte)2, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"), (byte)4, false, false, (byte)1, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 },
                    { new Guid("e494d97a-7ad8-ca92-29ff-ac42c6d13486"), (byte)2, true, false, (byte)2, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"), (byte)2, true, false, (byte)2, "total", "الإجمالي", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 10 },
                    { new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"), (byte)0, false, false, (byte)0, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 14 },
                    { new Guid("f8e8fd13-8804-05b5-08ce-8e6e114e7fcf"), (byte)3, true, true, (byte)1, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"), (byte)4, true, false, (byte)2, "contract", "العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 15 },
                    { new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"), (byte)2, false, false, (byte)0, "unitPrice", "سعر الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 5 },
                    { new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"), (byte)4, true, true, (byte)0, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "", "columns.hint" },
                    { new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "", "columns.thisItem" },
                    { new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "", "columns.refused" },
                    { new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "", "columns.invalidOrder" },
                    { new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "", "columns.projectFinished" },
                    { new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "", "columns.choices" },
                    { new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "", "columns.new" },
                    { new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "", "columns.typeList" },
                    { new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "", "columns.applyExplain" },
                    { new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "", "columns.showInReports" },
                    { new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "", "columns.typeNumber" },
                    { new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "", "columns.applyToAll" },
                    { new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "", "columns.typeText" },
                    { new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "", "columns.template" },
                    { new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "", "columns.applyQuestion" },
                    { new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "", "columns.typeDate" },
                    { new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "", "columns.custom" },
                    { new Guid("662954f9-3b36-95d7-56a7-00b109f487d7"), "", "columns.deleteExplain" },
                    { new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "", "columns.empty" },
                    { new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "", "columns.templateOnly" },
                    { new Guid("75732723-aba7-1685-bd11-04205305acb8"), "", "columns.save" },
                    { new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "", "columns.invalidOptions" },
                    { new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "", "columns.required" },
                    { new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "", "columns.typeAmount" },
                    { new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "", "columns.noConnection" },
                    { new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "", "columns.name" },
                    { new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "", "columns.invalidName" },
                    { new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "", "columns.optional" },
                    { new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "", "columns.title" },
                    { new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "", "columns.type" },
                    { new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "", "columns.coreColumn" },
                    { new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "", "columns.typeFile" },
                    { new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "", "columns.edit" },
                    { new Guid("c488fde9-af3a-1449-62f0-bc2ff6a85095"), "", "columns.delete" },
                    { new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "", "columns.core" },
                    { new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "", "columns.inReports" },
                    { new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "", "columns.loadFailed" },
                    { new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "", "columns.invalidDataType" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00c8ad86-e393-8e9a-5e5a-82ddaa6f3e79"), "en", new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "Pick list" },
                    { new Guid("037d21e1-1750-d5ce-c081-385636de93b3"), "ar", new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "تعديل العمود" },
                    { new Guid("0d6fa4f6-d760-576e-706e-0377e17f4a64"), "en", new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "Choices, one on each line" },
                    { new Guid("114dbec0-4cc5-4cba-28a3-99f86713f98d"), "ar", new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "القالب الافتراضي" },
                    { new Guid("14acffbb-a1d0-8ca3-e896-be0d2d3f681b"), "en", new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "Add column" },
                    { new Guid("14d3fff4-1489-ba20-c250-d96e60396e5a"), "ar", new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "مبلغ" },
                    { new Guid("1fc9db1e-a140-30ae-94a6-de5301f312a4"), "ar", new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "إضافة عمود" },
                    { new Guid("26fde679-57b8-9897-9447-4325b22141c5"), "ar", new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "مخصص" },
                    { new Guid("281181ed-562a-0723-ed54-132a566f1fee"), "ar", new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "الخيارات، كل خيار في سطر" },
                    { new Guid("29901006-c674-5027-89b3-3434b3969906"), "ar", new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "رُفض هذا التغيير." },
                    { new Guid("2ac24559-5a33-259a-1a18-275eeef1fad6"), "ar", new Guid("75732723-aba7-1685-bd11-04205305acb8"), "حفظ" },
                    { new Guid("2b366df5-55ed-956b-e04c-3a5b372f8804"), "en", new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "Enter at least one choice, of up to 100 characters each." },
                    { new Guid("2da36dce-3c83-bbd3-2753-bbea5efbe22a"), "ar", new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "تاريخ" },
                    { new Guid("3122d1d1-1453-7af9-d90a-3e3337be3f89"), "en", new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "This item only" },
                    { new Guid("3be681f9-b871-25f4-e0d4-f47a864b86e0"), "en", new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "Columns" },
                    { new Guid("3cb92c85-c471-1548-2b32-cd2e9e319124"), "en", new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "The default template decides what new items of this type start with. Existing items change only if you apply this to them." },
                    { new Guid("40948408-1d44-86d2-8337-d200bf82d4c0"), "ar", new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "النوع" },
                    { new Guid("42c3e0ae-58be-2da9-d2f4-6bac2dde9c1e"), "en", new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "Number" },
                    { new Guid("466ab419-06f6-009d-1104-0bb20f4db6f0"), "en", new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "Apply to all items" },
                    { new Guid("4aa27900-4bf0-8c10-8c8b-86195647bff3"), "ar", new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "العمود الأساسي لا يُحذف، ولا يتغير منه إلا اسمه." },
                    { new Guid("4b018095-5cb7-213b-98f3-92879e976176"), "ar", new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "اختياري" },
                    { new Guid("4d0ec420-b829-c811-b576-920c93d178f8"), "ar", new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "اسحب لتغيير الترتيب. الأعمدة المقفلة أساسية للحسابات والكشف الضريبي: تُعاد تسميتها ولا تُحذف." },
                    { new Guid("5548523f-fa04-ec88-a798-66102b09d8fe"), "ar", new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "الأعمدة" },
                    { new Guid("5aad714e-d090-9cd2-4484-cd6faa8d9403"), "ar", new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "اختر أحد الأنواع المعروضة." },
                    { new Guid("5d383655-2a22-cacb-7882-c6a6d557a4d4"), "en", new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "This change was refused." },
                    { new Guid("61069e80-7cef-b368-0966-1530951e816c"), "en", new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "Column name" },
                    { new Guid("6d786445-66af-9d78-71f8-89ac8954bc53"), "ar", new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("6eec3541-5454-0d53-fe0e-6948aba80b43"), "ar", new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("6fdd527a-b27f-dd2c-6d2e-5881c25c0545"), "en", new Guid("75732723-aba7-1685-bd11-04205305acb8"), "Save" },
                    { new Guid("7089d880-d7c3-8133-bf4b-7c51ff67b491"), "en", new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "Drag to reorder. Locked columns are what the calculations and the tax statement rely on: they can be renamed, not deleted." },
                    { new Guid("730eac13-98bc-14e8-0019-c7e8c9f8d16c"), "en", new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "Shown in printed reports" },
                    { new Guid("74acd799-0bf7-d406-faf6-8c8cf270d806"), "ar", new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "أدخل خياراً واحداً على الأقل، لا يزيد كل خيار على 100 حرف." },
                    { new Guid("79f6892b-0096-8d06-5c60-82dbc99147a5"), "en", new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "No connection to the server. Try again." },
                    { new Guid("80635ccc-ce5f-79aa-2cf4-b3973026ea7d"), "en", new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "Required at entry" },
                    { new Guid("81c2003a-b0b1-a3cb-e346-4adb4f598f3f"), "ar", new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "تطبيق على كل البنود؟" },
                    { new Guid("8551d4c0-bb19-f50a-7f57-b3c01804cdc3"), "ar", new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "في التقارير" },
                    { new Guid("862e9633-f73e-a600-dffb-d5c8ff123092"), "en", new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "Date" },
                    { new Guid("8934e942-a3d8-1826-db65-6e31da247756"), "en", new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "File" },
                    { new Guid("8ac02635-5ac2-7569-39b0-3438445f8278"), "en", new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "Text" },
                    { new Guid("8ac7932b-a2dd-3832-c878-036a22095b91"), "en", new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "Enter a name of 1 to 100 characters." },
                    { new Guid("8e4491d6-a31e-58a8-908c-f19b774e3c68"), "ar", new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "هذا البند فقط" },
                    { new Guid("9094a335-5dba-db86-a44d-a3a9472516b9"), "en", new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "The list has changed. Try again." },
                    { new Guid("9653b522-3d12-8d64-9642-86c6e6e4cb2b"), "ar", new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "القالب فقط" },
                    { new Guid("97e8294e-1f74-1317-ecd2-67725ab8a2e1"), "ar", new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "القالب الافتراضي يحدد ما تبدأ به البنود الجديدة من هذا النوع. ولا تتغير البنود الموجودة إلا إذا طبقت هذا عليها." },
                    { new Guid("9f6e51bd-7939-6ac7-09c5-779c98cb4281"), "ar", new Guid("662954f9-3b36-95d7-56a7-00b109f487d7"), "يختفي من الإدخال، وتبقى قيمه في القيود السابقة وسجل التدقيق." },
                    { new Guid("a1cf9081-a9f6-ee30-2b1e-a620a6445f8e"), "en", new Guid("c488fde9-af3a-1449-62f0-bc2ff6a85095"), "Delete column" },
                    { new Guid("a2a581fb-9169-4524-ef5f-375da2594e38"), "en", new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "Choose one of the types offered." },
                    { new Guid("a449c304-b61a-bae9-2eab-5af525df991b"), "ar", new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "ملف" },
                    { new Guid("ab1c5d98-e73b-e783-0037-c747996e34a0"), "en", new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "Template only" },
                    { new Guid("af514d7d-67e2-587c-83b8-202cc2636710"), "en", new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "Default template" },
                    { new Guid("b03d52e8-83d6-b8a1-86c2-fac93863fd9d"), "ar", new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "قائمة اختيار" },
                    { new Guid("b552ee0f-c866-4baa-19c4-95e295e85519"), "ar", new Guid("c488fde9-af3a-1449-62f0-bc2ff6a85095"), "حذف العمود" },
                    { new Guid("bc903481-d646-1e34-15d8-2250873d7d9a"), "ar", new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "اسم العمود" },
                    { new Guid("bd7127e5-b543-02fb-a27f-f22c19745cc5"), "ar", new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "تطبيق على كل البنود" },
                    { new Guid("c0552221-6525-e831-8a63-ba0c017f4490"), "ar", new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "لا توجد أعمدة." },
                    { new Guid("c7f17b65-f7f6-0634-2b1a-12e4372565a7"), "en", new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "Core column: it cannot be deleted" },
                    { new Guid("ca24b425-e2db-c35d-3376-c1e637bc52c1"), "en", new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "Apply to all items?" },
                    { new Guid("caebcc3a-a820-07c2-d094-1425014d9551"), "en", new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "This project is finished, so nothing in it changes." },
                    { new Guid("cd927d0b-dd88-d853-edef-9ddae47fb67d"), "en", new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "Edit column" },
                    { new Guid("cea63ab5-8b1b-16d1-b785-82bb19ffa618"), "en", new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "A core column cannot be deleted, and only its name changes." },
                    { new Guid("d4e97a31-d9fa-2649-5699-a6e3baff2fb9"), "ar", new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "نص" },
                    { new Guid("d6198828-358d-c4b2-aec0-70281e50b55e"), "en", new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "Could not read the columns. Check the network and try again." },
                    { new Guid("d693debd-5ae5-74dd-7e16-bab96575f9a4"), "en", new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "Optional" },
                    { new Guid("d6f5189d-13fa-0e1d-e17b-0fc419621101"), "ar", new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "عمود أساسي لا يُحذف" },
                    { new Guid("dae04a10-ded3-a030-8933-75fcda2cb729"), "en", new Guid("662954f9-3b36-95d7-56a7-00b109f487d7"), "It leaves entry. Values already recorded stay on their entries and in the audit trail." },
                    { new Guid("dec2ca4f-b171-73d9-0664-232f6dd31143"), "en", new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "In reports" },
                    { new Guid("ec5c3b41-0426-b222-161a-6980f742ff2f"), "ar", new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "يظهر في التقارير المطبوعة" },
                    { new Guid("ec72daf3-bb87-dda8-8795-6258d81cbb08"), "en", new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "Custom" },
                    { new Guid("ee2f60c8-35d5-2f41-9b87-89aa5d128677"), "ar", new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "تعذّرت قراءة الأعمدة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("efc1753c-bf8a-5afa-acdb-e934fac17f9a"), "en", new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "Type" },
                    { new Guid("efedce5b-f492-e2b1-7347-701aa375e793"), "en", new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "Amount" },
                    { new Guid("f031e97b-9ca7-bdbc-1037-a2330976af4f"), "ar", new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("f31b62b4-8c82-04e8-6785-1e45af6dceff"), "ar", new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "إلزامي عند الإدخال" },
                    { new Guid("f3cf1efb-219e-5c7e-574c-528917603c06"), "ar", new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "رقم" },
                    { new Guid("f8b97354-eefe-e959-bfc6-f51027311cbc"), "en", new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "No columns." },
                    { new Guid("fafe640b-8a07-33d9-6e53-65ef4636e24f"), "ar", new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "تغيّرت القائمة. حاول مرة أخرى." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_ProjectId_ItemType_Key",
                table: "DefaultColumns",
                columns: new[] { "ProjectId", "ItemType", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemColumns_ItemId_Key",
                table: "ItemColumns",
                columns: new[] { "ItemId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultColumns");

            migrationBuilder.DropTable(
                name: "ItemColumns");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00c8ad86-e393-8e9a-5e5a-82ddaa6f3e79"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("037d21e1-1750-d5ce-c081-385636de93b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0d6fa4f6-d760-576e-706e-0377e17f4a64"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("114dbec0-4cc5-4cba-28a3-99f86713f98d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("14acffbb-a1d0-8ca3-e896-be0d2d3f681b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("14d3fff4-1489-ba20-c250-d96e60396e5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1fc9db1e-a140-30ae-94a6-de5301f312a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("26fde679-57b8-9897-9447-4325b22141c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("281181ed-562a-0723-ed54-132a566f1fee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29901006-c674-5027-89b3-3434b3969906"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ac24559-5a33-259a-1a18-275eeef1fad6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b366df5-55ed-956b-e04c-3a5b372f8804"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2da36dce-3c83-bbd3-2753-bbea5efbe22a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3122d1d1-1453-7af9-d90a-3e3337be3f89"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3be681f9-b871-25f4-e0d4-f47a864b86e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3cb92c85-c471-1548-2b32-cd2e9e319124"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40948408-1d44-86d2-8337-d200bf82d4c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42c3e0ae-58be-2da9-d2f4-6bac2dde9c1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("466ab419-06f6-009d-1104-0bb20f4db6f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4aa27900-4bf0-8c10-8c8b-86195647bff3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b018095-5cb7-213b-98f3-92879e976176"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d0ec420-b829-c811-b576-920c93d178f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5548523f-fa04-ec88-a798-66102b09d8fe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5aad714e-d090-9cd2-4484-cd6faa8d9403"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d383655-2a22-cacb-7882-c6a6d557a4d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61069e80-7cef-b368-0966-1530951e816c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6d786445-66af-9d78-71f8-89ac8954bc53"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6eec3541-5454-0d53-fe0e-6948aba80b43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6fdd527a-b27f-dd2c-6d2e-5881c25c0545"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7089d880-d7c3-8133-bf4b-7c51ff67b491"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("730eac13-98bc-14e8-0019-c7e8c9f8d16c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("74acd799-0bf7-d406-faf6-8c8cf270d806"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("79f6892b-0096-8d06-5c60-82dbc99147a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80635ccc-ce5f-79aa-2cf4-b3973026ea7d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81c2003a-b0b1-a3cb-e346-4adb4f598f3f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8551d4c0-bb19-f50a-7f57-b3c01804cdc3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("862e9633-f73e-a600-dffb-d5c8ff123092"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8934e942-a3d8-1826-db65-6e31da247756"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ac02635-5ac2-7569-39b0-3438445f8278"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ac7932b-a2dd-3832-c878-036a22095b91"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e4491d6-a31e-58a8-908c-f19b774e3c68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9094a335-5dba-db86-a44d-a3a9472516b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9653b522-3d12-8d64-9642-86c6e6e4cb2b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97e8294e-1f74-1317-ecd2-67725ab8a2e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f6e51bd-7939-6ac7-09c5-779c98cb4281"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a1cf9081-a9f6-ee30-2b1e-a620a6445f8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2a581fb-9169-4524-ef5f-375da2594e38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a449c304-b61a-bae9-2eab-5af525df991b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ab1c5d98-e73b-e783-0037-c747996e34a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af514d7d-67e2-587c-83b8-202cc2636710"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b03d52e8-83d6-b8a1-86c2-fac93863fd9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b552ee0f-c866-4baa-19c4-95e295e85519"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc903481-d646-1e34-15d8-2250873d7d9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd7127e5-b543-02fb-a27f-f22c19745cc5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0552221-6525-e831-8a63-ba0c017f4490"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c7f17b65-f7f6-0634-2b1a-12e4372565a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca24b425-e2db-c35d-3376-c1e637bc52c1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("caebcc3a-a820-07c2-d094-1425014d9551"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd927d0b-dd88-d853-edef-9ddae47fb67d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cea63ab5-8b1b-16d1-b785-82bb19ffa618"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d4e97a31-d9fa-2649-5699-a6e3baff2fb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6198828-358d-c4b2-aec0-70281e50b55e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d693debd-5ae5-74dd-7e16-bab96575f9a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6f5189d-13fa-0e1d-e17b-0fc419621101"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dae04a10-ded3-a030-8933-75fcda2cb729"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dec2ca4f-b171-73d9-0664-232f6dd31143"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ec5c3b41-0426-b222-161a-6980f742ff2f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ec72daf3-bb87-dda8-8795-6258d81cbb08"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee2f60c8-35d5-2f41-9b87-89aa5d128677"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efc1753c-bf8a-5afa-acdb-e934fac17f9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efedce5b-f492-e2b1-7347-701aa375e793"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f031e97b-9ca7-bdbc-1037-a2330976af4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f31b62b4-8c82-04e8-6785-1e45af6dceff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f3cf1efb-219e-5c7e-574c-528917603c06"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8b97354-eefe-e959-bfc6-f51027311cbc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fafe640b-8a07-33d9-6e53-65ef4636e24f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("118f0575-b4a4-103c-ec45-82886336345a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5362c8a3-f990-2309-67ec-d817a0810062"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("662954f9-3b36-95d7-56a7-00b109f487d7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75732723-aba7-1685-bd11-04205305acb8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c488fde9-af3a-1449-62f0-bc2ff6a85095"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"));
        }
    }
}
