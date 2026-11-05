using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StructureSourceId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subgroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subgroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subgroups_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "IsTemplate", "Name", "RejectionReason", "StartDate", "Status", "StructureSourceId" },
                values: new object[] { new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, "القالب الافتراضي", "", new DateOnly(1900, 1, 1), (byte)0, new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("4139aa5d-5f6c-852a-0bd8-bd8796609bd3"), "ManageStructure", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "", "categories.disable" },
                    { new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "", "projects.unknownSource" },
                    { new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "", "categories.typeContractorPayments" },
                    { new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "", "categories.disabled" },
                    { new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "", "categories.empty" },
                    { new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "", "categories.itemType" },
                    { new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "", "categories.subgroupsHint" },
                    { new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "", "categories.rename" },
                    { new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "", "categories.items" },
                    { new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "", "categories.refused" },
                    { new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "", "subgroups.new" },
                    { new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "", "categories.subgroupName" },
                    { new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "", "categories.invalidOrder" },
                    { new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "", "categories.typeBoth" },
                    { new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "", "categories.addItem" },
                    { new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "", "categories.hint" },
                    { new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "", "categories.generalSubgroup" },
                    { new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "", "categories.invalidName" },
                    { new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "", "projects.defaultTemplate" },
                    { new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "", "categories.noSubgroups" },
                    { new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "", "projects.structureSource" },
                    { new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "", "categories.enable" },
                    { new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "", "categories.categoryName" },
                    { new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "", "categories.loadFailed" },
                    { new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "", "categories.noConnection" },
                    { new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "", "categories.new" },
                    { new Guid("da7a2b71-6655-c965-411b-18f830383110"), "", "categories.save" },
                    { new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "", "categories.projectFinished" },
                    { new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "", "categories.subgroups" },
                    { new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "", "categories.noItems" },
                    { new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "", "categories.typePurchases" },
                    { new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "", "categories.itemName" },
                    { new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "", "categories.title" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "الإشراف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 4 },
                    { new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "التشطيب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 3 },
                    { new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "الرسوم والفواتير", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 1 },
                    { new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "العظم", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 2 },
                    { new Guid("c4aece2a-cb9d-fb1d-ae30-dce8baba5959"), true, "تكلفة الأراضي", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 0 }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("043efcff-c662-3319-0b57-0d8af9f681f6"), "en", new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "Enable" },
                    { new Guid("05bb45c1-0d7c-4b16-9aae-57f9b44b9016"), "ar", new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "تصنيف رئيسي جديد" },
                    { new Guid("0981f535-f51d-256b-5c2f-84f30714826e"), "ar", new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "مشتريات + دفعات مقاولين" },
                    { new Guid("0bae938b-e2c2-4159-2d1d-05b03fe55f1b"), "en", new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "Add subgroup" },
                    { new Guid("0cedd85b-3270-c582-ad10-383b2de3f783"), "en", new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "The list has changed. Try again." },
                    { new Guid("0d8c25c4-56ca-9b8f-3142-e97166f2c4fb"), "ar", new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "النوع" },
                    { new Guid("0e755cfb-0c15-00fc-6132-9e14233bff8d"), "en", new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "Add item" },
                    { new Guid("1080f1df-1544-78e1-ebda-23e9ad133ff9"), "ar", new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "اسم التصنيف الرئيسي" },
                    { new Guid("161d8696-af3d-d208-7c39-9b6b65cda057"), "en", new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "Subgroups" },
                    { new Guid("208411dd-ce72-67c3-a31d-8d412d16b299"), "en", new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "Categories and items" },
                    { new Guid("2119540b-c48d-a9b7-334c-ae3970962eef"), "ar", new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "تغيّرت القائمة. حاول مرة أخرى." },
                    { new Guid("27bfc3bc-4fab-e70c-06e0-7c119ec9eb04"), "en", new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "The default template" },
                    { new Guid("27f4c817-48d8-bfb4-a1ba-8a7c4cb8eca6"), "en", new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "Category name" },
                    { new Guid("2cd5b879-4ca7-cc70-c26a-75721504586e"), "en", new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "Enter a name of 1 to 100 characters." },
                    { new Guid("347be3b6-2650-f9c5-c8c1-1cedfb05e279"), "en", new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "This project is finished, so nothing in it changes." },
                    { new Guid("35b99f1b-b2e3-3691-a79c-b5cd8f230bab"), "en", new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "Disabled" },
                    { new Guid("372a4c05-537b-1f7a-a6cb-2c304c7c8002"), "en", new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "Items" },
                    { new Guid("3b6258bb-7b88-0f8a-c2b1-9a920d4a4c4a"), "en", new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "Purchases + contractor payments" },
                    { new Guid("3dc04493-e929-0bb4-1223-bc4346454f03"), "ar", new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "لا توجد تصنيفات بعد. أضف أولها." },
                    { new Guid("401c8cca-a079-3e8e-456c-d8c40d4aa32d"), "en", new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "Purchases" },
                    { new Guid("41572b00-4ec1-ef5e-5aa2-92ccb7c06d66"), "en", new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "Drag to reorder. This is the order of the periodic report." },
                    { new Guid("431730d8-ae62-73dd-e18d-0a0c6ab23f4c"), "ar", new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "تعطيل" },
                    { new Guid("47a0d312-b1df-5b6b-a174-b23da1dbe394"), "en", new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "No categories yet. Add the first one." },
                    { new Guid("4c6d93f7-809c-8ad8-3629-b050816d64bb"), "ar", new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("4c79b3f8-f4eb-4a61-43c0-62adf90e2c96"), "en", new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "The item's general group has no name to change." },
                    { new Guid("5282be38-4c0a-dca4-1d1d-0a8c014e4091"), "en", new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "New category" },
                    { new Guid("54ecf1ad-1033-8a8f-b26f-ae5546ceafca"), "ar", new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"), "المجموعات الفرعية" },
                    { new Guid("5ee3f7c1-1ef6-aac4-40ca-a6b730919937"), "ar", new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "معطّل" },
                    { new Guid("601a51fe-2550-0959-c7b8-8fa8a54362d1"), "ar", new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "تفعيل" },
                    { new Guid("6df2c889-7f00-be9c-4a2d-eb07d1e61e7d"), "en", new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "No connection to the server. Try again." },
                    { new Guid("6f346cf2-9d16-9a0a-26a7-7ecc2eff58c8"), "ar", new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("752f40d1-0f70-4b91-7dc3-3fdbf30b0821"), "ar", new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "إعادة تسمية" },
                    { new Guid("782d21e6-d160-be0b-2191-7d9906073b55"), "en", new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "Could not read the categories. Check the network and try again." },
                    { new Guid("7cdd9127-daab-476d-3ddd-b00205d8c298"), "ar", new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "لا توجد بنود بعد." },
                    { new Guid("86257ea3-ea66-634f-49d3-70becf1f1bb3"), "en", new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "Type" },
                    { new Guid("8af62ca9-469a-42cc-dcf6-fb697f8d22eb"), "ar", new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "رُفض هذا التغيير." },
                    { new Guid("8e9bb63d-f963-eac9-97a7-eb08beb1c27c"), "ar", new Guid("da7a2b71-6655-c965-411b-18f830383110"), "حفظ" },
                    { new Guid("91752244-d7e2-8f15-3f95-790ba9c1143d"), "en", new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "No items yet." },
                    { new Guid("96715ee9-f05a-0a87-509e-fae8ab7a766b"), "ar", new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "اسحب لتغيير الترتيب، وهو ترتيب التقرير الدوري." },
                    { new Guid("98964e9f-9ceb-029d-034e-2c64d373e010"), "en", new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "You cannot copy from that project." },
                    { new Guid("9bdd52bf-2f1a-7874-5aef-342c747881b6"), "en", new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "Subgroup name" },
                    { new Guid("a2720be4-dcfc-f46f-58bc-b9567fed8010"), "ar", new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "التصنيفات والبنود" },
                    { new Guid("a6796ce7-0546-4c15-1f2e-c0053daa2fa0"), "ar", new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "مشتريات" },
                    { new Guid("ae5fa845-65f5-62a7-2053-5597d359d418"), "ar", new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "نسخ التصنيفات والبنود من" },
                    { new Guid("aea28216-c2bb-678d-5a3a-932ed1169692"), "ar", new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "إضافة بند" },
                    { new Guid("b6952d60-4568-045f-be8c-67d1baae49d8"), "en", new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "This change was refused." },
                    { new Guid("b85f36f4-b9e6-2619-25c8-b21f8407bf21"), "en", new Guid("da7a2b71-6655-c965-411b-18f830383110"), "Save" },
                    { new Guid("ba0c67b4-6d66-92e9-4c57-f84b89225a7a"), "en", new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "Copy categories and items from" },
                    { new Guid("bb265a13-bdb8-5b09-13ba-b0c1d1cb66aa"), "ar", new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "البنود" },
                    { new Guid("cc8c938f-db5f-9c86-8816-7a2ef1af490f"), "en", new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "Each subgroup prints as its own table in the item's report. An entry with no subgroup goes to the item's general group." },
                    { new Guid("d22e65e2-1391-5cab-1550-854a410fb3df"), "ar", new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("d5d3a3a6-b28d-7c2a-01a7-35f65fdb2cff"), "en", new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "Contractor payments" },
                    { new Guid("deba4109-3767-e9f1-1013-381fc48fb0af"), "en", new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "Disable" },
                    { new Guid("decb554a-544e-f514-ad1b-d6fec5bed241"), "ar", new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "إضافة مجموعة فرعية" },
                    { new Guid("e200befd-e428-3154-ce16-c39684370923"), "ar", new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "اسم المجموعة الفرعية" },
                    { new Guid("e57b4282-78c8-f2e6-571b-c47126609ec1"), "ar", new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "اسم البند" },
                    { new Guid("e5d69077-a483-8894-8bb1-38f7d026719a"), "ar", new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "القالب الافتراضي" },
                    { new Guid("ea2b89c2-6ba0-2cfa-8597-955a77052eaf"), "ar", new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "المجموعة العامة للبند ليس لها اسم يُغيَّر." },
                    { new Guid("eb46ecc9-bc7f-cba1-b232-bea10ee8b09b"), "ar", new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"), "تُطبع كل مجموعة فرعية جدولاً مستقلاً في تقرير البند. والقيد بلا مجموعة يدخل في المجموعة العامة للبند." },
                    { new Guid("f0ab1024-2f96-9cf1-926e-57734e20391c"), "en", new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "This item has no subgroups." },
                    { new Guid("f20e7f66-2dc8-3714-1ca6-f8f5a058fb59"), "ar", new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "تعذّرت قراءة التصنيفات. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("f2d919b0-d8d3-435b-810f-9ccf54e57363"), "ar", new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "لا يمكنك النسخ من ذلك المشروع." },
                    { new Guid("f613c221-b425-f4df-fda9-3ce24271f3e4"), "en", new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "Item name" },
                    { new Guid("f95be2b8-d4f7-93db-30bd-00dd37dd4fd5"), "ar", new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "دفعات مقاولين" },
                    { new Guid("fd72c743-bed0-ba53-dea8-fd727c7907c0"), "en", new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "Rename" },
                    { new Guid("fde190f4-ccbb-ddb4-3d7c-9863cc08a7ca"), "ar", new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"), "لا توجد مجموعات فرعية لهذا البند." }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "SortOrder", "Type" },
                values: new object[,]
                {
                    { new Guid("04af0eb6-f27d-1f55-1999-a027e57d9ccb"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "ردود الماء", 7, (byte)2 },
                    { new Guid("04f4b5bd-08cb-fe77-fb77-b0dc67a41e94"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الحفر", 0, (byte)2 },
                    { new Guid("0cf9ec40-d505-1642-076c-34362aacc748"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "درابزين", 8, (byte)2 },
                    { new Guid("12e6ecda-c096-25f0-782e-4dbc5b1b4de3"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الحديد", 1, (byte)0 },
                    { new Guid("180b728a-47d2-5142-5427-5033a619ea95"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "التأمين", 3, (byte)2 },
                    { new Guid("21280dd7-483e-17c2-640f-03bc3cdb8988"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "حجر الواجهة", 9, (byte)2 },
                    { new Guid("2614447c-52a2-553a-15d8-0011ddfafd1c"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "أبواب خارجية", 2, (byte)2 },
                    { new Guid("286990e1-6c68-33cf-b0b3-78059b23b697"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الكهرباء", 5, (byte)2 },
                    { new Guid("2ac089f0-6d06-7441-4b4e-874fe1543f43"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "لياسة", 3, (byte)2 },
                    { new Guid("2e248f9f-9064-45a0-334f-c53e38d678fb"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "معدات تشطيب", 12, (byte)2 },
                    { new Guid("314438d3-36aa-8eaa-328a-cc9beb5767b6"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "دفعات العظم", 4, (byte)1 },
                    { new Guid("673de086-8cee-5596-1b93-acb0acfef09f"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "بويه", 6, (byte)2 },
                    { new Guid("6ad84f04-d71e-3e7f-8479-cedd8b6f49d9"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "البناء", 3, (byte)2 },
                    { new Guid("6da2f643-7b61-6067-d461-5397f6c43410"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المكتب المشرف", 2, (byte)2 },
                    { new Guid("6e86db2d-d5ec-b6fa-53cc-7cdde0c14b44"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "العوازل", 11, (byte)2 },
                    { new Guid("6f303ab6-00bc-ae69-5f2a-8331b61bc6f0"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المحاسب", 4, (byte)2 },
                    { new Guid("748258b8-99e2-f7a3-a9b2-06052a9b95f7"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "أخرى", 5, (byte)2 },
                    { new Guid("7b8220e1-c3e9-06c2-3615-53b8032e7262"), new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "إشراف ميداني", 1, (byte)2 },
                    { new Guid("8435b564-68b8-4b1e-5176-750bb0b72b3d"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "السباكة", 0, (byte)2 },
                    { new Guid("84820fed-9506-6e9d-9308-53c9841a97e9"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "جبس", 4, (byte)2 },
                    { new Guid("9c89740e-854b-393e-45d5-e649ad5de00c"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "رسوم الأمانة", 1, (byte)2 },
                    { new Guid("9fca4a36-6136-161d-07bb-cea835e1d6f7"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "أغراض الموقع", 8, (byte)2 },
                    { new Guid("ae991b59-acb1-7420-d09b-e6895207aac0"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "أبواب داخلية", 10, (byte)2 },
                    { new Guid("aebccc22-25aa-9838-9062-e867eccb8b8e"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "الكهرباء", 1, (byte)2 },
                    { new Guid("cf854e63-cad5-1c83-90a2-c6164c86b6c4"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "ألمنيوم", 7, (byte)2 },
                    { new Guid("db415f43-3b91-e7b2-4041-3330e8c49b76"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المكتب الهندسي", 0, (byte)2 },
                    { new Guid("dcfd5629-5844-9427-bb79-53273463077a"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "السباكة", 6, (byte)2 },
                    { new Guid("eb1a681e-7168-48f9-5fc5-c1d1351d58db"), new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "إشراف مالي", 0, (byte)2 },
                    { new Guid("f0085a42-9276-ee42-6602-5b1e32f49e10"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الخرسانة", 2, (byte)0 },
                    { new Guid("f55f1253-ff90-54ed-3d8a-833d0f5a06e1"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "رواتب الحارس", 13, (byte)2 },
                    { new Guid("f64be08a-09b1-c732-261f-c4bb8f2d37f5"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "بلاط", 5, (byte)2 }
                });

            migrationBuilder.InsertData(
                table: "Subgroups",
                columns: new[] { "Id", "ItemId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("000586db-98e1-becf-6b50-01778c91e7b3"), new Guid("aebccc22-25aa-9838-9062-e867eccb8b8e"), "", 0 },
                    { new Guid("2c0e710e-bd86-15fd-1ad8-0573a254ca24"), new Guid("dcfd5629-5844-9427-bb79-53273463077a"), "", 0 },
                    { new Guid("303c5901-677c-31f2-da36-31fe95e53b63"), new Guid("f55f1253-ff90-54ed-3d8a-833d0f5a06e1"), "", 0 },
                    { new Guid("324cafe0-f959-ca2f-3b19-c167197ebad6"), new Guid("84820fed-9506-6e9d-9308-53c9841a97e9"), "", 0 },
                    { new Guid("32e345ff-943f-de0d-89b5-cc978b79b33f"), new Guid("180b728a-47d2-5142-5427-5033a619ea95"), "", 0 },
                    { new Guid("40e0cf07-7729-cd68-160f-a568dd140b39"), new Guid("6e86db2d-d5ec-b6fa-53cc-7cdde0c14b44"), "", 0 },
                    { new Guid("4f8f065e-ed62-4984-2820-ab3cca39c455"), new Guid("04af0eb6-f27d-1f55-1999-a027e57d9ccb"), "", 0 },
                    { new Guid("53a7bf92-62ce-d80a-b5fb-abcbffa9b60c"), new Guid("314438d3-36aa-8eaa-328a-cc9beb5767b6"), "", 0 },
                    { new Guid("5d5d6e60-1047-0933-b1f2-3633529247ca"), new Guid("6ad84f04-d71e-3e7f-8479-cedd8b6f49d9"), "", 0 },
                    { new Guid("5e79f6ac-ca2f-dc3c-ccb6-8a27761bf6f4"), new Guid("6f303ab6-00bc-ae69-5f2a-8331b61bc6f0"), "", 0 },
                    { new Guid("61c56bb2-138f-5f41-9383-448b817bf4e5"), new Guid("21280dd7-483e-17c2-640f-03bc3cdb8988"), "", 0 },
                    { new Guid("70863205-936d-e8bd-e9ae-4379f5816f36"), new Guid("6da2f643-7b61-6067-d461-5397f6c43410"), "", 0 },
                    { new Guid("75d39cb5-7c79-4382-963b-8d59e93672c4"), new Guid("12e6ecda-c096-25f0-782e-4dbc5b1b4de3"), "", 0 },
                    { new Guid("7860a42d-3edf-8147-730c-265d34ccc642"), new Guid("db415f43-3b91-e7b2-4041-3330e8c49b76"), "", 0 },
                    { new Guid("897a23be-99c3-c327-63f8-b9811ac20ce4"), new Guid("eb1a681e-7168-48f9-5fc5-c1d1351d58db"), "", 0 },
                    { new Guid("8a97233b-3293-cc71-872d-7c709e093b93"), new Guid("0cf9ec40-d505-1642-076c-34362aacc748"), "", 0 },
                    { new Guid("91632167-ef32-8dff-d059-6eaac4554047"), new Guid("748258b8-99e2-f7a3-a9b2-06052a9b95f7"), "", 0 },
                    { new Guid("b2837e7b-6a2e-7e75-9d10-b27c6a1bc4ba"), new Guid("9fca4a36-6136-161d-07bb-cea835e1d6f7"), "", 0 },
                    { new Guid("b48fe98b-9432-34ab-eab0-45773afbae03"), new Guid("286990e1-6c68-33cf-b0b3-78059b23b697"), "", 0 },
                    { new Guid("b54806d8-db61-13c8-e192-2ac11678df46"), new Guid("673de086-8cee-5596-1b93-acb0acfef09f"), "", 0 },
                    { new Guid("b753077c-a5b1-ec2d-a6ec-0a71153ab451"), new Guid("f0085a42-9276-ee42-6602-5b1e32f49e10"), "", 0 },
                    { new Guid("bcf76c0a-8f53-704f-b0b1-b222df5dcfa7"), new Guid("f64be08a-09b1-c732-261f-c4bb8f2d37f5"), "", 0 },
                    { new Guid("cabb8175-0a11-9c2d-be34-ff846246e9f5"), new Guid("2ac089f0-6d06-7441-4b4e-874fe1543f43"), "", 0 },
                    { new Guid("d9f40360-24f3-9803-6883-fa7c04572073"), new Guid("ae991b59-acb1-7420-d09b-e6895207aac0"), "", 0 },
                    { new Guid("ddbd3ea0-92f3-ead5-0baa-4720989b526d"), new Guid("2e248f9f-9064-45a0-334f-c53e38d678fb"), "", 0 },
                    { new Guid("e646dc59-f645-5878-3949-e406f4d0e555"), new Guid("cf854e63-cad5-1c83-90a2-c6164c86b6c4"), "", 0 },
                    { new Guid("e897f26e-7fd9-5b34-30a9-8ca97fa411a9"), new Guid("8435b564-68b8-4b1e-5176-750bb0b72b3d"), "", 0 },
                    { new Guid("f228ea29-3d05-6e30-f10e-92c94cf4dec4"), new Guid("9c89740e-854b-393e-45d5-e649ad5de00c"), "", 0 },
                    { new Guid("f5d0be5f-23b0-db35-9da9-7a158a70a153"), new Guid("7b8220e1-c3e9-06c2-3615-53b8032e7262"), "", 0 },
                    { new Guid("f6a3834a-8a64-0781-c69b-13d67503c10e"), new Guid("04f4b5bd-08cb-fe77-fb77-b0dc67a41e94"), "", 0 },
                    { new Guid("fc247740-3368-0e14-c6ff-255d25cd1b01"), new Guid("2614447c-52a2-553a-15d8-0011ddfafd1c"), "", 0 }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure')");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ProjectId",
                table: "Categories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                table: "Items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subgroups_ItemId",
                table: "Subgroups",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subgroups");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4139aa5d-5f6c-852a-0bd8-bd8796609bd3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("043efcff-c662-3319-0b57-0d8af9f681f6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("05bb45c1-0d7c-4b16-9aae-57f9b44b9016"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0981f535-f51d-256b-5c2f-84f30714826e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0bae938b-e2c2-4159-2d1d-05b03fe55f1b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0cedd85b-3270-c582-ad10-383b2de3f783"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0d8c25c4-56ca-9b8f-3142-e97166f2c4fb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e755cfb-0c15-00fc-6132-9e14233bff8d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1080f1df-1544-78e1-ebda-23e9ad133ff9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("161d8696-af3d-d208-7c39-9b6b65cda057"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("208411dd-ce72-67c3-a31d-8d412d16b299"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2119540b-c48d-a9b7-334c-ae3970962eef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27bfc3bc-4fab-e70c-06e0-7c119ec9eb04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27f4c817-48d8-bfb4-a1ba-8a7c4cb8eca6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2cd5b879-4ca7-cc70-c26a-75721504586e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("347be3b6-2650-f9c5-c8c1-1cedfb05e279"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35b99f1b-b2e3-3691-a79c-b5cd8f230bab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("372a4c05-537b-1f7a-a6cb-2c304c7c8002"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b6258bb-7b88-0f8a-c2b1-9a920d4a4c4a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3dc04493-e929-0bb4-1223-bc4346454f03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("401c8cca-a079-3e8e-456c-d8c40d4aa32d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41572b00-4ec1-ef5e-5aa2-92ccb7c06d66"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("431730d8-ae62-73dd-e18d-0a0c6ab23f4c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("47a0d312-b1df-5b6b-a174-b23da1dbe394"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c6d93f7-809c-8ad8-3629-b050816d64bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c79b3f8-f4eb-4a61-43c0-62adf90e2c96"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5282be38-4c0a-dca4-1d1d-0a8c014e4091"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54ecf1ad-1033-8a8f-b26f-ae5546ceafca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ee3f7c1-1ef6-aac4-40ca-a6b730919937"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("601a51fe-2550-0959-c7b8-8fa8a54362d1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6df2c889-7f00-be9c-4a2d-eb07d1e61e7d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6f346cf2-9d16-9a0a-26a7-7ecc2eff58c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("752f40d1-0f70-4b91-7dc3-3fdbf30b0821"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("782d21e6-d160-be0b-2191-7d9906073b55"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cdd9127-daab-476d-3ddd-b00205d8c298"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86257ea3-ea66-634f-49d3-70becf1f1bb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8af62ca9-469a-42cc-dcf6-fb697f8d22eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e9bb63d-f963-eac9-97a7-eb08beb1c27c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91752244-d7e2-8f15-3f95-790ba9c1143d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("96715ee9-f05a-0a87-509e-fae8ab7a766b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98964e9f-9ceb-029d-034e-2c64d373e010"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bdd52bf-2f1a-7874-5aef-342c747881b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2720be4-dcfc-f46f-58bc-b9567fed8010"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a6796ce7-0546-4c15-1f2e-c0053daa2fa0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae5fa845-65f5-62a7-2053-5597d359d418"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aea28216-c2bb-678d-5a3a-932ed1169692"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b6952d60-4568-045f-be8c-67d1baae49d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85f36f4-b9e6-2619-25c8-b21f8407bf21"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba0c67b4-6d66-92e9-4c57-f84b89225a7a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bb265a13-bdb8-5b09-13ba-b0c1d1cb66aa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc8c938f-db5f-9c86-8816-7a2ef1af490f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d22e65e2-1391-5cab-1550-854a410fb3df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5d3a3a6-b28d-7c2a-01a7-35f65fdb2cff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("deba4109-3767-e9f1-1013-381fc48fb0af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("decb554a-544e-f514-ad1b-d6fec5bed241"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e200befd-e428-3154-ce16-c39684370923"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e57b4282-78c8-f2e6-571b-c47126609ec1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5d69077-a483-8894-8bb1-38f7d026719a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea2b89c2-6ba0-2cfa-8597-955a77052eaf"));

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
                keyValue: new Guid("f20e7f66-2dc8-3714-1ca6-f8f5a058fb59"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2d919b0-d8d3-435b-810f-9ccf54e57363"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f613c221-b425-f4df-fda9-3ce24271f3e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f95be2b8-d4f7-93db-30bd-00dd37dd4fd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd72c743-bed0-ba53-dea8-fd727c7907c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fde190f4-ccbb-ddb4-3d7c-9863cc08a7ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e766766-fe39-e5a9-283e-84266293cb52"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("25297a4f-30c0-9a5a-da50-56f37976040a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("420c7e21-3622-70d2-9e25-606eafec4461"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("858460ee-2646-7a25-aafd-17e36724328b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7b41bf3-08cf-eee5-462d-c4852658a6ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b196425c-3768-95bb-5c37-17a7807feb19"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("da7a2b71-6655-c965-411b-18f830383110"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e261048d-9d0e-adfc-47d7-c94b165fb8dc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f7037acc-0425-5b6e-b289-86cade476b41"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"));

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StructureSourceId",
                table: "Projects");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject')");
        }
    }
}
