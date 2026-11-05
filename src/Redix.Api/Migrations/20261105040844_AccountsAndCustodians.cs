using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AccountsAndCustodians : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountCustodians",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountCustodians", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_AccountCustodians_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountCustodians_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("83613343-4e53-c553-7699-7c123d5c27bb"), "ChargeAnyAccount", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "", "accounts.openingBalance" },
                    { new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "", "accounts.save" },
                    { new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "", "accounts.name" },
                    { new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "", "accounts.typeCashCustody" },
                    { new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "", "accounts.riyal" },
                    { new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "", "accounts.invalidOpeningBalance" },
                    { new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "", "accounts.refused" },
                    { new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "", "accounts.edit" },
                    { new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "", "accounts.noConnection" },
                    { new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "", "accounts.type" },
                    { new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "", "accounts.loadFailed" },
                    { new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "", "accounts.myCustody" },
                    { new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "", "accounts.projectFinished" },
                    { new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "", "accounts.new" },
                    { new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "", "accounts.disable" },
                    { new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "", "accounts.typeBank" },
                    { new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "", "accounts.noCustodian" },
                    { new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "", "accounts.enable" },
                    { new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "", "accounts.disabled" },
                    { new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "", "accounts.custodian" },
                    { new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "", "accounts.unknownCustodian" },
                    { new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "", "accounts.custodianRequired" },
                    { new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "", "accounts.title" },
                    { new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "", "accounts.empty" },
                    { new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "", "accounts.invalidName" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0194614f-2539-de35-39b3-094c9688dd92"), "en", new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "This project is finished, so nothing in it changes." },
                    { new Guid("0438231d-e912-50dd-ade9-0a2554ffbced"), "en", new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "Disable" },
                    { new Guid("1572dc11-faf7-c9d5-b7f5-38f177012ec4"), "ar", new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "الرصيد الافتتاحي" },
                    { new Guid("17a58459-d062-3ba7-257b-24a449d0b144"), "en", new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "Edit account" },
                    { new Guid("22146e94-fe67-f051-9da8-2dd86d39af15"), "ar", new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("235d063c-bf70-6448-b2ec-fa9ad672b541"), "ar", new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "نوع الحساب" },
                    { new Guid("30f76099-5b8a-18f1-9531-e3453a7dfb6f"), "ar", new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "حفظ" },
                    { new Guid("348910dc-dafd-320d-f96e-17a6b3661905"), "ar", new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "رصيد عهدتي" },
                    { new Guid("39b8e0c1-f63c-2507-adcd-402f32d51626"), "ar", new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "ر.س" },
                    { new Guid("3a0588a7-d759-de4a-7071-f47e2f087a93"), "en", new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "Account name" },
                    { new Guid("3d2b74b3-6e67-f200-669d-3c233f673ffb"), "en", new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "Could not read the accounts. Check the network and try again." },
                    { new Guid("3ef5499c-aee8-ca07-94dc-a6285b6a58cd"), "ar", new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "تفعيل" },
                    { new Guid("44519fdc-d77f-895e-ee9f-ab385469bd1d"), "en", new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "Enter a name of 1 to 100 characters." },
                    { new Guid("4758ca2d-8d17-6839-fe36-a59e11b66946"), "en", new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "No accounts." },
                    { new Guid("4829d758-fd83-2fd2-d504-0e05c79b2b87"), "ar", new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "أدخل الرصيد الافتتاحي بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("49ab3c64-49aa-ec85-00f4-e054f0505e80"), "ar", new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "تعديل الحساب" },
                    { new Guid("4b527f46-f4f9-6b26-9be2-0aaf5707e9bc"), "en", new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "Accounts" },
                    { new Guid("4ce2ac01-8adc-17dc-f3c8-9648a2a17dda"), "en", new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "Account type" },
                    { new Guid("4eb452f8-1e7e-9efe-1db5-d355f4b1546e"), "ar", new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "رُفض هذا التغيير." },
                    { new Guid("50e3db7b-4c86-f7df-3d11-affe944993af"), "en", new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "Save" },
                    { new Guid("54376eb9-a582-9b74-e237-923b178fe856"), "en", new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "A cash custody needs someone who holds it." },
                    { new Guid("55b02ed6-bb61-9ff8-add0-9d282000d545"), "en", new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "Cash custody" },
                    { new Guid("58003d22-9775-6a8b-bbd1-480fdaacb2f6"), "en", new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "Nobody" },
                    { new Guid("588b37f2-3f61-b827-9c14-5c3273a9a0e3"), "ar", new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "الحسابات" },
                    { new Guid("59d60906-33ff-eafd-e3b2-0a6c4e871b4f"), "ar", new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "حساب بنكي" },
                    { new Guid("5d4802f1-1f09-666c-fa63-0f7834e92d9e"), "en", new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "Bank account" },
                    { new Guid("6365be7d-cd57-7759-7547-5c4268748814"), "en", new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "My custody" },
                    { new Guid("647864bf-e1c4-0e4f-b8cd-9c257b7456f9"), "ar", new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "معطّل" },
                    { new Guid("6c3e6672-c81a-f1c4-eeab-10b19313241b"), "en", new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "Enter an opening balance in numbers, with at most two decimals." },
                    { new Guid("76258564-fab3-3334-1530-5f4e83393ad2"), "ar", new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "عهدة نقدية" },
                    { new Guid("81cb2052-68e8-057b-f249-12566f3c415c"), "ar", new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "اسم الحساب" },
                    { new Guid("84aa7fdb-4037-21e6-cd46-7de194092a45"), "en", new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "Enable" },
                    { new Guid("85a40146-829e-b0af-aa57-c9a06b7e7506"), "en", new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "This change was refused." },
                    { new Guid("8672a050-5585-4022-cb0e-bd2bf3dc8182"), "en", new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "That person is not in this project." },
                    { new Guid("9920bf18-86ad-e7a5-dc7c-7a3dccf87bea"), "ar", new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "إضافة حساب" },
                    { new Guid("9b49bb69-d420-4b2c-2bc1-faf08cb86074"), "en", new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "Disabled" },
                    { new Guid("9f49461f-69c8-9e89-bf30-f308d8b56552"), "ar", new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "لا توجد حسابات." },
                    { new Guid("a7ee204a-f3ec-9e89-5335-d5d52ca5b79f"), "en", new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "Opening balance" },
                    { new Guid("a99bb7fd-bafd-8d55-18a8-f71b540e2c23"), "ar", new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "العهدة النقدية تحتاج إلى صاحب عهدة." },
                    { new Guid("b1ddb234-4c7a-8d36-3327-ae1ff4088e2e"), "en", new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "SAR" },
                    { new Guid("b546f5c9-a722-8229-a43b-d4730a92982f"), "ar", new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("bb55ce0c-b381-050a-d78f-e1d8b7f9f452"), "ar", new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "لا أحد" },
                    { new Guid("bc717084-a565-0d2c-ce9c-3409534ba099"), "ar", new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "هذا الشخص ليس في هذا المشروع." },
                    { new Guid("c65b0944-c196-84c2-18e8-614d03c94fc6"), "en", new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "No connection to the server. Try again." },
                    { new Guid("d079cf4f-4f08-d1f5-9750-45543cd0a5d8"), "en", new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "Custodian" },
                    { new Guid("e354a690-42e6-edd7-f26d-bd7d9de2d4a6"), "en", new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "Add account" },
                    { new Guid("e3929ee3-ac15-caab-1154-3dcfef5b0fdb"), "ar", new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "تعطيل" },
                    { new Guid("eac29f40-7500-4caf-55cf-9c3f53a0686b"), "ar", new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("ecd1c0b8-0061-64c3-8c02-f81e7a5a3f38"), "ar", new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "تعذّرت قراءة الحسابات. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("ef0227cb-8383-d67f-53d5-426a964faf93"), "ar", new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "صاحب العهدة" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'ChargeAnyAccount')");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCustodians_UserId",
                table: "AccountCustodians",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProjectId",
                table: "Accounts",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountCustodians");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("83613343-4e53-c553-7699-7c123d5c27bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0194614f-2539-de35-39b3-094c9688dd92"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0438231d-e912-50dd-ade9-0a2554ffbced"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1572dc11-faf7-c9d5-b7f5-38f177012ec4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("17a58459-d062-3ba7-257b-24a449d0b144"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("22146e94-fe67-f051-9da8-2dd86d39af15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("235d063c-bf70-6448-b2ec-fa9ad672b541"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("30f76099-5b8a-18f1-9531-e3453a7dfb6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("348910dc-dafd-320d-f96e-17a6b3661905"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39b8e0c1-f63c-2507-adcd-402f32d51626"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3a0588a7-d759-de4a-7071-f47e2f087a93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d2b74b3-6e67-f200-669d-3c233f673ffb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3ef5499c-aee8-ca07-94dc-a6285b6a58cd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44519fdc-d77f-895e-ee9f-ab385469bd1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4758ca2d-8d17-6839-fe36-a59e11b66946"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4829d758-fd83-2fd2-d504-0e05c79b2b87"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49ab3c64-49aa-ec85-00f4-e054f0505e80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b527f46-f4f9-6b26-9be2-0aaf5707e9bc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ce2ac01-8adc-17dc-f3c8-9648a2a17dda"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4eb452f8-1e7e-9efe-1db5-d355f4b1546e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50e3db7b-4c86-f7df-3d11-affe944993af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54376eb9-a582-9b74-e237-923b178fe856"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55b02ed6-bb61-9ff8-add0-9d282000d545"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("58003d22-9775-6a8b-bbd1-480fdaacb2f6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("588b37f2-3f61-b827-9c14-5c3273a9a0e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("59d60906-33ff-eafd-e3b2-0a6c4e871b4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d4802f1-1f09-666c-fa63-0f7834e92d9e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6365be7d-cd57-7759-7547-5c4268748814"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("647864bf-e1c4-0e4f-b8cd-9c257b7456f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6c3e6672-c81a-f1c4-eeab-10b19313241b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76258564-fab3-3334-1530-5f4e83393ad2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81cb2052-68e8-057b-f249-12566f3c415c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("84aa7fdb-4037-21e6-cd46-7de194092a45"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85a40146-829e-b0af-aa57-c9a06b7e7506"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8672a050-5585-4022-cb0e-bd2bf3dc8182"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9920bf18-86ad-e7a5-dc7c-7a3dccf87bea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9b49bb69-d420-4b2c-2bc1-faf08cb86074"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f49461f-69c8-9e89-bf30-f308d8b56552"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7ee204a-f3ec-9e89-5335-d5d52ca5b79f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a99bb7fd-bafd-8d55-18a8-f71b540e2c23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1ddb234-4c7a-8d36-3327-ae1ff4088e2e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b546f5c9-a722-8229-a43b-d4730a92982f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bb55ce0c-b381-050a-d78f-e1d8b7f9f452"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc717084-a565-0d2c-ce9c-3409534ba099"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c65b0944-c196-84c2-18e8-614d03c94fc6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d079cf4f-4f08-d1f5-9750-45543cd0a5d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e354a690-42e6-edd7-f26d-bd7d9de2d4a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e3929ee3-ac15-caab-1154-3dcfef5b0fdb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eac29f40-7500-4caf-55cf-9c3f53a0686b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ecd1c0b8-0061-64c3-8c02-f81e7a5a3f38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ef0227cb-8383-d67f-53d5-426a964faf93"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49c3c74e-801c-8788-8358-3d5146a78313"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ea46842b-3d2a-8204-111e-18e19350629a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f190c394-52dc-db41-6381-e5b58e874199"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings')");
        }
    }
}
