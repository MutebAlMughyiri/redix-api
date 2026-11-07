using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PermissionsPerPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPermissions", x => x.Id);
                    table.CheckConstraint("CK_ProjectPermissions_Permission", "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");
                    table.ForeignKey(
                        name: "FK_ProjectPermissions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPermissions", x => x.Id);
                    table.CheckConstraint("CK_SystemPermissions_Permission", "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");
                    table.ForeignKey(
                        name: "FK_SystemPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Each person keeps everything their roles held in each project, and whoever recorded entries keeps deleting them (D8); the admin's role becomes his permission.
            migrationBuilder.Sql("""
                INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission)
                SELECT NEWID(), m.ProjectId, m.UserId, h.Permission
                FROM ProjectMembers m JOIN RolePermissions h ON h.RoleId = m.RoleId
                WHERE h.Permission <> 'ApproveProjects'
                GROUP BY m.ProjectId, m.UserId, h.Permission
                """);
            migrationBuilder.Sql("""
                INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission)
                SELECT NEWID(), ProjectId, UserId, 'DeleteEntries' FROM ProjectPermissions WHERE Permission = 'RecordEntries'
                """);
            migrationBuilder.Sql("""
                INSERT INTO SystemPermissions (Id, UserId, Permission)
                SELECT NEWID(), u.UserId, h.Permission
                FROM UserRoles u JOIN RolePermissions h ON h.RoleId = u.RoleId
                GROUP BY u.UserId, h.Permission
                """);

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TranslationKeys_Key",
                table: "TranslationKeys");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15d4c430-9bd7-87da-10f5-2c89fb534713"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("292e45e6-371c-fa95-2f53-c15f374eca31"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d4ef630-76d1-5dd7-4d87-825918355a32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6207a112-1d6b-b0f0-a4c9-4e0f0f04cd07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65355d35-f035-39ad-0ba2-8c0840226745"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68bf8eb8-f805-3c39-a6dc-b91e072ab147"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a39a281-3d2c-31bc-3e66-f2d958f44601"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("750cb2f0-6200-d700-0def-3711392692f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82a7f452-d9bd-c965-b0db-090a461692ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91dc02b3-d294-7b9d-f490-b85bae163d15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa310a01-1201-bc09-43ea-51d727a91762"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6027d46-ab5d-17cd-0a22-8c9721382d00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d167311b-e9b1-4918-045b-fcea8aba6486"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d84474b3-01de-e50d-54ce-a9218d6d0f5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea35d520-82a3-9934-c95d-228d3191a681"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbccd63d-445f-9f7c-3b7b-76b5fdb18e76"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("77960312-298e-1e22-7acd-02337306d3f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "", "permissions.manageStructureHint" },
                    { new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "", "permissions.manageAnyEntry" },
                    { new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "", "permissions.moveMoney" },
                    { new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "", "permissions.manageStructure" },
                    { new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "", "permissions.attachFiles" },
                    { new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "", "users.invalidPermission" },
                    { new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "", "permissions.attachFilesHint" },
                    { new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "", "permissions.count" },
                    { new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "", "permissions.addSupplier" },
                    { new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "", "permissions.seeAllCapital" },
                    { new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "", "permissions.deleteEntries" },
                    { new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "", "permissions.readProjectHint" },
                    { new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "", "permissions.manageUsersHint" },
                    { new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "", "permissions.addSupplierHint" },
                    { new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "", "permissions.readProject" },
                    { new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "", "permissions.approveEntries" },
                    { new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "", "permissions.chargeAnyAccountHint" },
                    { new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "", "permissions.manageUsers" },
                    { new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "", "permissions.seeAllCapitalHint" },
                    { new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "", "permissions.moveMoneyHint" },
                    { new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "", "permissions.manageSettingsHint" },
                    { new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "", "permissions.deleteEntriesHint" },
                    { new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "", "permissions.manageAnyEntryHint" },
                    { new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "", "permissions.recordEntriesHint" },
                    { new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "", "permissions.chargeAnyAccount" },
                    { new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "", "permissions.recordEntries" },
                    { new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "", "permissions.title" },
                    { new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "", "permissions.manageSettings" },
                    { new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "", "permissions.manageProjectHint" },
                    { new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "", "permissions.manageProject" },
                    { new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "", "permissions.approveEntriesHint" },
                    { new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "", "permissions.hint" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e1d056a-dfe5-3769-8ef4-3d6fc8536a23"),
                column: "Value",
                value: "يحتاج المشروع إلى شخص واحد على الأقل يدير مستخدميه.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6ddf9830-c72a-17ae-6149-02b6b5e4a47a"),
                column: "Value",
                value: "A project needs at least one person who manages its users.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0108cac3-ae40-1d8e-ffef-80182714d7f9"), "en", new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "Shape the expense structure" },
                    { new Guid("01e7b8f2-9e43-0653-f692-033646bc89ee"), "ar", new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "إضافة مورد" },
                    { new Guid("0fb0788c-2336-f27e-6aa9-6f16ffeab7a7"), "ar", new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "اسمه ووحداته ومستنداته ونسبة إنجازه، وإغلاقه." },
                    { new Guid("12cbb5f1-d3c2-5615-26b8-d77519bd1577"), "en", new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "See all capital" },
                    { new Guid("150d26f0-7e27-62ce-2c91-3540ca5090ad"), "ar", new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "الصرف من أي حساب" },
                    { new Guid("1b2a4711-eaf4-067c-d7a6-cacf178c503c"), "ar", new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "إرفاق الملفات" },
                    { new Guid("239cf61f-56bf-d755-6952-72a2077a6580"), "en", new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "Manage the project" },
                    { new Guid("272d3ca4-41dd-217c-e709-209d69047098"), "ar", new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "حدد ما يستطيعه هذا الشخص في هذا المشروع. إلغاء «يرى المشروع» يخرجه منه." },
                    { new Guid("2c84e7a6-efe3-7964-f07a-b0fa11204277"), "en", new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "This permission cannot be given in a project." },
                    { new Guid("2c93f1b1-55c0-5b36-a327-093f24ae119d"), "en", new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "Without it, only from the accounts in their custody." },
                    { new Guid("3b1e4a56-a7bf-5801-7c87-d3d47f5705f9"), "ar", new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "رأس مال كل الشركاء" },
                    { new Guid("3e554d08-bf9d-2b80-22c7-f429c30841f4"), "ar", new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "إدارة المستخدمين" },
                    { new Guid("435fbe16-ecb6-5264-b242-245654ea7164"), "ar", new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "هيكل المصروفات" },
                    { new Guid("446c4ca9-bcb0-419f-9fc4-36c73099562e"), "en", new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "Move money" },
                    { new Guid("48457951-e286-f89f-0ab8-aee07a66da24"), "ar", new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "التصنيفات والبنود والجداول والأعمدة." },
                    { new Guid("49573c45-24a9-23e1-82ac-ff89d8cedea7"), "en", new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "Pay from any account" },
                    { new Guid("50338268-3dfb-dd9a-3c78-9c0aea2a098c"), "en", new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "Add a supplier" },
                    { new Guid("50c76735-8650-fb66-8df0-19d905c97439"), "ar", new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "يعدّل ويرسل ويحذف قيداً أدخله غيره، أو قيداً ينتظر، قبل اعتماده." },
                    { new Guid("52a794de-c72a-091a-14e7-c5bf6eab62d4"), "ar", new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "يضيف الأشخاص إلى المشروع ويحدد صلاحياتهم." },
                    { new Guid("59304d44-5c0d-b30f-d917-51ae73d98459"), "en", new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "Permissions" },
                    { new Guid("677a2272-33e1-01de-c567-fe1f1ca1eabf"), "en", new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "Photographs or chooses the file of an invoice, a receipt or a contract." },
                    { new Guid("734a8a46-3da8-7ad3-56bd-1f917da195a9"), "en", new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "Categories, items, subgroups and columns." },
                    { new Guid("767f61e8-ecd7-f948-dd14-0a5aeaa9368b"), "ar", new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "الصلاحيات" },
                    { new Guid("77a8d9ee-a77a-84fb-74be-c5c1446b005b"), "en", new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "Keep the settings" },
                    { new Guid("7a5676a9-acbc-ae3c-e4da-02d9382b7a9a"), "en", new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "Enters purchases and contractor payments." },
                    { new Guid("7cec74d2-98ca-19ea-2405-4cd1058e2bd0"), "en", new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "Deletes a bill or a payment not yet approved: their own, or someone else's with \"Handle others' entries\"." },
                    { new Guid("80569a26-c2bb-f965-5922-9e8596342460"), "en", new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "Permissions: {0}" },
                    { new Guid("807b69e5-8dc5-d43c-9c86-20c3623e9c93"), "ar", new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "بدونها يصرف من الحسابات التي في عهدته فقط." },
                    { new Guid("86d887f0-969a-12cf-fd9a-3e3e1807118f"), "ar", new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "يحذف فاتورة أو دفعة لم تُعتمد: قيده، أو قيد غيره مع «إدارة قيود الآخرين»." },
                    { new Guid("8f73d678-3680-27ec-09ad-9157ead98e74"), "en", new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "Handle others' entries" },
                    { new Guid("914fe1a3-aa21-52c0-13fa-61a8254e8f71"), "ar", new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "إدارة الإعدادات" },
                    { new Guid("94ed53b4-1874-de23-89c5-fd33eb1a3c38"), "en", new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "Edits, sends and deletes an entry someone else entered, or one waiting, before approval." },
                    { new Guid("96ec8e27-85a4-43c1-2ff0-cafb4a3cb2de"), "en", new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "Opens the project and reads everything in it." },
                    { new Guid("97abc77c-ac8c-2c9b-9ef6-a800eaa81918"), "ar", new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "إدارة قيود الآخرين" },
                    { new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"), "en", new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "Transfers, capital movements, revenue, reconciling, the periodic report and liquidation." },
                    { new Guid("a10cf312-27dc-b82d-7a2b-83c8bb01496d"), "ar", new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "لا تُمنح هذه الصلاحية داخل مشروع." },
                    { new Guid("a7fb47fb-313d-542c-4658-5ad4dd23dbfd"), "ar", new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "يُدخل المشتريات ودفعات المقاولين." },
                    { new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"), "en", new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "Sees every partner's capital, not only their own, and takes the report files." },
                    { new Guid("b1214456-32e9-8dc0-3d56-d7c1d8275c2e"), "en", new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "Its name, units, documents and completion, and closing it." },
                    { new Guid("b38ae88e-cbfb-aae2-39c4-c4179c696ea6"), "ar", new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "يعتمد ما ينتظر أو يرفضه، ويعكس القيد المعتمد." },
                    { new Guid("b49b5d53-aac8-c57c-5092-36c83230e76d"), "en", new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "Approves or rejects what waits, and reverses an approved entry." },
                    { new Guid("b6c86106-2a29-f9b1-0270-cbf71ffa6ed7"), "en", new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "Attach files" },
                    { new Guid("bc93c7e7-2261-ebd4-1dac-fbf16d28a8bd"), "ar", new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "إدارة المشروع" },
                    { new Guid("c2fa55bc-06f0-5e62-285a-85e5c807e92f"), "en", new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "Approve entries" },
                    { new Guid("c41f921e-119e-040a-9441-d51a2f13b60b"), "en", new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "Record expenses" },
                    { new Guid("c4ba7de7-217d-f868-d3cc-259546894792"), "ar", new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "التحويلات وحركات رأس المال والإيرادات والمطابقة والتقرير الدوري والتصفية." },
                    { new Guid("c51f677b-d4a4-1c2a-8348-c71be175079d"), "ar", new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "حذف القيود" },
                    { new Guid("ce50e0fd-6902-50f2-4525-5059edf59f3e"), "ar", new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "يرى المشروع" },
                    { new Guid("cea439cd-63b8-f5e2-a1a1-4f3ef4a6accd"), "ar", new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "يفتح المشروع ويقرأ كل ما فيه." },
                    { new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"), "ar", new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "يرى رأس مال كل الشركاء لا رأس ماله وحده، ويأخذ ملفات التقارير." },
                    { new Guid("d123a465-22e3-4308-5306-4c179554a338"), "ar", new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "طرق الدفع ونسبة الضريبة والحسابات والشركاء والموردون والعقود." },
                    { new Guid("d24f10d0-0ca1-1835-8d7b-a85d68f5dd46"), "en", new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "While entering; without \"Keep the settings\" the supplier waits for approval." },
                    { new Guid("dcfd8610-399a-ac1b-e836-557c7ced95bd"), "en", new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "See the project" },
                    { new Guid("dda850e5-37a4-874e-2ee0-0fc01febaf4e"), "ar", new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "الصلاحيات: {0}" },
                    { new Guid("ddc67816-317d-043b-e2a6-7b8fe596ea8b"), "ar", new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "تسجيل المصروفات" },
                    { new Guid("e13605a7-1868-edf4-99ad-ca08023ecda9"), "en", new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "Tick what this person may do in this project. Unticking \"See the project\" takes them out of it." },
                    { new Guid("eb241407-0e68-8e0f-9524-ec9ffb88bc8a"), "ar", new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "أثناء الإدخال، وبدون «إدارة الإعدادات» ينتظر المورد الاعتماد." },
                    { new Guid("ee6ad155-fce7-080f-7d48-9427942009c1"), "ar", new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "يصوّر أو يختار ملف الفاتورة أو الإيصال أو العقد." },
                    { new Guid("eec1dec6-5897-4be1-fc18-966c245f7fbf"), "ar", new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "الحركات المالية" },
                    { new Guid("ef4a5786-3d73-ef98-1744-e6b6c6e4637c"), "en", new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "Payment methods, VAT rate, accounts, partners, suppliers and contracts." },
                    { new Guid("ef876afe-f68b-15ee-9de0-5ae887176e8b"), "en", new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "Adds people to the project and ticks their permissions." },
                    { new Guid("f2ac9cf2-6b26-e5ee-fcf6-ee7754119317"), "en", new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "Manage users" },
                    { new Guid("fbb8f8aa-105f-b9ef-68ea-2a3ae2eceb64"), "en", new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "Delete entries" },
                    { new Guid("fccd9a35-73ad-f8e1-3f0a-d8f82b98dc77"), "ar", new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "اعتماد القيود" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissions_ProjectId_UserId_Permission",
                table: "ProjectPermissions",
                columns: new[] { "ProjectId", "UserId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissions_UserId",
                table: "ProjectPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_UserId_Permission",
                table: "SystemPermissions",
                columns: new[] { "UserId", "Permission" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ticked permissions do not map back onto five roles: going down leaves every project without its people, so only a backup taken before this migration goes back with them.
            migrationBuilder.DropTable(
                name: "ProjectPermissions");

            migrationBuilder.DropTable(
                name: "SystemPermissions");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0108cac3-ae40-1d8e-ffef-80182714d7f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01e7b8f2-9e43-0653-f692-033646bc89ee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fb0788c-2336-f27e-6aa9-6f16ffeab7a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12cbb5f1-d3c2-5615-26b8-d77519bd1577"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("150d26f0-7e27-62ce-2c91-3540ca5090ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b2a4711-eaf4-067c-d7a6-cacf178c503c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("239cf61f-56bf-d755-6952-72a2077a6580"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("272d3ca4-41dd-217c-e709-209d69047098"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2c84e7a6-efe3-7964-f07a-b0fa11204277"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2c93f1b1-55c0-5b36-a327-093f24ae119d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b1e4a56-a7bf-5801-7c87-d3d47f5705f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3e554d08-bf9d-2b80-22c7-f429c30841f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("435fbe16-ecb6-5264-b242-245654ea7164"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("446c4ca9-bcb0-419f-9fc4-36c73099562e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48457951-e286-f89f-0ab8-aee07a66da24"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49573c45-24a9-23e1-82ac-ff89d8cedea7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50338268-3dfb-dd9a-3c78-9c0aea2a098c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50c76735-8650-fb66-8df0-19d905c97439"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52a794de-c72a-091a-14e7-c5bf6eab62d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("59304d44-5c0d-b30f-d917-51ae73d98459"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("677a2272-33e1-01de-c567-fe1f1ca1eabf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("734a8a46-3da8-7ad3-56bd-1f917da195a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("767f61e8-ecd7-f948-dd14-0a5aeaa9368b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77a8d9ee-a77a-84fb-74be-c5c1446b005b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a5676a9-acbc-ae3c-e4da-02d9382b7a9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cec74d2-98ca-19ea-2405-4cd1058e2bd0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80569a26-c2bb-f965-5922-9e8596342460"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("807b69e5-8dc5-d43c-9c86-20c3623e9c93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86d887f0-969a-12cf-fd9a-3e3e1807118f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f73d678-3680-27ec-09ad-9157ead98e74"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("914fe1a3-aa21-52c0-13fa-61a8254e8f71"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94ed53b4-1874-de23-89c5-fd33eb1a3c38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("96ec8e27-85a4-43c1-2ff0-cafb4a3cb2de"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97abc77c-ac8c-2c9b-9ef6-a800eaa81918"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a10cf312-27dc-b82d-7a2b-83c8bb01496d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7fb47fb-313d-542c-4658-5ad4dd23dbfd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1214456-32e9-8dc0-3d56-d7c1d8275c2e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b38ae88e-cbfb-aae2-39c4-c4179c696ea6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b49b5d53-aac8-c57c-5092-36c83230e76d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b6c86106-2a29-f9b1-0270-cbf71ffa6ed7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc93c7e7-2261-ebd4-1dac-fbf16d28a8bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2fa55bc-06f0-5e62-285a-85e5c807e92f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c41f921e-119e-040a-9441-d51a2f13b60b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4ba7de7-217d-f868-d3cc-259546894792"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c51f677b-d4a4-1c2a-8348-c71be175079d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce50e0fd-6902-50f2-4525-5059edf59f3e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cea439cd-63b8-f5e2-a1a1-4f3ef4a6accd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d123a465-22e3-4308-5306-4c179554a338"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d24f10d0-0ca1-1835-8d7b-a85d68f5dd46"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dcfd8610-399a-ac1b-e836-557c7ced95bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dda850e5-37a4-874e-2ee0-0fc01febaf4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ddc67816-317d-043b-e2a6-7b8fe596ea8b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e13605a7-1868-edf4-99ad-ca08023ecda9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb241407-0e68-8e0f-9524-ec9ffb88bc8a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee6ad155-fce7-080f-7d48-9427942009c1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eec1dec6-5897-4be1-fc18-966c245f7fbf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ef4a5786-3d73-ef98-1744-e6b6c6e4637c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ef876afe-f68b-15ee-9de0-5ae887176e8b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2ac9cf2-6b26-e5ee-fcf6-ee7754119317"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbb8f8aa-105f-b9ef-68ea-2a3ae2eceb64"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fccd9a35-73ad-f8e1-3f0a-d8f82b98dc77"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4f42daeb-c624-5557-7811-db10c51c86af"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53e4fc4e-1871-f976-c534-cede7db19503"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("887ed9de-786e-0196-93ae-39846a2947f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("baad61b8-835d-de2c-6889-754d605598ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_TranslationKeys_Key",
                table: "TranslationKeys",
                column: "Key");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Scope = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    WordKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.CheckConstraint("CK_Roles_Scope", "[Scope] IN ('System', 'Project')");
                    table.ForeignKey(
                        name: "FK_Roles_TranslationKeys_WordKey",
                        column: x => x.WordKey,
                        principalTable: "TranslationKeys",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.CheckConstraint("CK_RolePermissions_Permission", "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "", "users.invalidRole" },
                    { new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "", "users.rolesHint" },
                    { new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "", "users.roleFinance" },
                    { new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "", "users.roleField" },
                    { new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "", "users.roleManager" },
                    { new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "", "users.roleAdmin" },
                    { new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "", "users.roleReader" },
                    { new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "", "users.roles" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e1d056a-dfe5-3769-8ef4-3d6fc8536a23"),
                column: "Value",
                value: "يحتاج المشروع إلى مدير مشروع واحد على الأقل.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6ddf9830-c72a-17ae-6149-02b6b5e4a47a"),
                column: "Value",
                value: "A project needs at least one project manager.");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Scope", "SortOrder", "WordKey" },
                values: new object[,]
                {
                    { new Guid("3fee0350-b8df-dbb4-9bcc-b0039085dccd"), "Project", 4, "users.roleReader" },
                    { new Guid("6a87517c-47ed-b83f-195d-c84e7a2720e0"), "System", 1, "users.roleAdmin" },
                    { new Guid("8a930250-60d5-b402-464a-0545e0253004"), "Project", 2, "users.roleFinance" },
                    { new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd"), "Project", 1, "users.roleManager" },
                    { new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451"), "Project", 3, "users.roleField" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("15d4c430-9bd7-87da-10f5-2c89fb534713"), "en", new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "Finance" },
                    { new Guid("292e45e6-371c-fa95-2f53-c15f374eca31"), "en", new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "Admin" },
                    { new Guid("4d4ef630-76d1-5dd7-4d87-825918355a32"), "ar", new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "مسؤول النظام" },
                    { new Guid("6207a112-1d6b-b0f0-a4c9-4e0f0f04cd07"), "ar", new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "مدير المشروع" },
                    { new Guid("65355d35-f035-39ad-0ba2-8c0840226745"), "en", new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "That role cannot be given here." },
                    { new Guid("68bf8eb8-f805-3c39-a6dc-b91e072ab147"), "en", new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "Reader" },
                    { new Guid("6a39a281-3d2c-31bc-3e66-f2d958f44601"), "ar", new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "حدد ما يقوم به هذا الشخص في هذا المشروع. إلغاء كل الأدوار يخرجه منه." },
                    { new Guid("750cb2f0-6200-d700-0def-3711392692f3"), "en", new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "Project manager" },
                    { new Guid("82a7f452-d9bd-c965-b0db-090a461692ce"), "en", new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "Roles" },
                    { new Guid("91dc02b3-d294-7b9d-f490-b85bae163d15"), "ar", new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "ميداني" },
                    { new Guid("aa310a01-1201-bc09-43ea-51d727a91762"), "ar", new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "الأدوار" },
                    { new Guid("c6027d46-ab5d-17cd-0a22-8c9721382d00"), "en", new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "Tick what this person does in this project. Removing every role takes them out of it." },
                    { new Guid("d167311b-e9b1-4918-045b-fcea8aba6486"), "ar", new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "لا يمكن إسناد هذا الدور هنا." },
                    { new Guid("d84474b3-01de-e50d-54ce-a9218d6d0f5f"), "ar", new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "مالي" },
                    { new Guid("ea35d520-82a3-9934-c95d-228d3191a681"), "ar", new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "قارئ" },
                    { new Guid("fbccd63d-445f-9f7c-3b7b-76b5fdb18e76"), "en", new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "Field" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("017f8267-5712-23a9-ab09-f3654a3f5802"), "RecordEntries", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") },
                    { new Guid("229fb904-b3ae-c168-60c9-52379dc1ac5f"), "ApproveProjects", new Guid("6a87517c-47ed-b83f-195d-c84e7a2720e0") },
                    { new Guid("3c35a012-dd63-53fb-9008-d3fc0de2de8c"), "ReadProject", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("4139aa5d-5f6c-852a-0bd8-bd8796609bd3"), "ManageStructure", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("43bca88f-c711-0d3e-7fc4-ba5d36094bb7"), "ManageUsers", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") },
                    { new Guid("55dcc157-3b4b-5c90-7196-d28f141da267"), "AttachFiles", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") },
                    { new Guid("618d2584-1949-ab7c-512d-daaf8a7ad632"), "ManageSettings", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("67937e5c-2761-3f8b-fe1a-397f2c237edb"), "RecordEntries", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("6a214907-1cf8-f706-4228-eef3d5373001"), "ManageProject", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("6d013eea-b4b2-27df-4b1c-fbf617b5794a"), "SeeAllCapital", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") },
                    { new Guid("6d3b8981-4510-44da-705f-0ef97bbe9d28"), "ManageAnyEntry", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("7663b4e6-3671-1d36-3bec-3a25d84330d4"), "ReadProject", new Guid("3fee0350-b8df-dbb4-9bcc-b0039085dccd") },
                    { new Guid("7a0b2030-6690-e681-2973-197db92c573a"), "SeeAllCapital", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("83613343-4e53-c553-7699-7c123d5c27bb"), "ChargeAnyAccount", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("b806ca26-2b86-5e82-b58e-d6d9a19ee416"), "AddSupplier", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("c79fdab1-3bb3-ec46-2886-93b77a5fe831"), "AddSupplier", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") },
                    { new Guid("c7f4a654-572d-95ea-4d74-7ac8c594fe39"), "MoveMoney", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("d2bdc53e-00df-5caa-8f2c-13197cb0e190"), "ApproveEntries", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("da2b0bde-147b-579a-ce58-6486192222af"), "AttachFiles", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("dae8d6db-6f49-9c82-c97b-bab24a781600"), "ReadProject", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") },
                    { new Guid("ef858e4c-4540-dc8b-794e-05d851b32885"), "ReadProject", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId_UserId_RoleId",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_RoleId",
                table: "ProjectMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserId",
                table: "ProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_Permission",
                table: "RolePermissions",
                columns: new[] { "RoleId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_WordKey",
                table: "Roles",
                column: "WordKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }
    }
}
