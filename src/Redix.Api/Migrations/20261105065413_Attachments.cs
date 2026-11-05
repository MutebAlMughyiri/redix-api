using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Attachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoragePath = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SizeBytes = table.Column<int>(type: "int", nullable: false),
                    Sha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => new { x.ContractId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("55dcc157-3b4b-5c90-7196-d28f141da267"), "AttachFiles", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") },
                    { new Guid("da2b0bde-147b-579a-ce58-6486192222af"), "AttachFiles", new Guid("8a930250-60d5-b402-464a-0545e0253004") }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "", "attachments.refused" },
                    { new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "", "contracts.unknownAttachment" },
                    { new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "", "attachments.unreadable" },
                    { new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "", "attachments.pickFile" },
                    { new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "", "attachments.noConnection" },
                    { new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "", "attachments.takePhoto" },
                    { new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "", "attachments.projectFinished" },
                    { new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "", "expenses.new" },
                    { new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "", "attachments.cameraFailed" },
                    { new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "", "attachments.imageTooLarge" },
                    { new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "", "attachments.invoicePhoto" },
                    { new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "", "attachments.retake" },
                    { new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "", "contracts.documents" },
                    { new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "", "contracts.addDocument" },
                    { new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "", "attachments.invalidFileType" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("02f020dc-196d-e1fb-2324-eb94acb99a1a"), "en", new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "The picture must be smaller than 1 MB." },
                    { new Guid("17c483cb-3273-e620-d826-c6f5f484fb17"), "en", new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "No connection to the server. Try again." },
                    { new Guid("1d2217b8-1b88-c5ea-baed-35fe0e740bb3"), "en", new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "Only a picture or a PDF can be kept." },
                    { new Guid("23308247-efaf-66c5-c5f5-d175f94a1c7c"), "ar", new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "تصوير الفاتورة" },
                    { new Guid("2b94ed2d-2fe6-83a6-c2ab-5457442fd805"), "ar", new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "صورة الفاتورة" },
                    { new Guid("2cb3a343-530f-50f8-72f8-7a83cc651996"), "ar", new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "تعذّر فتح الكاميرا. اختر الفاتورة من ملفاتك." },
                    { new Guid("2cba0c38-b315-245e-dc47-2d09d0065507"), "ar", new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "يجب أن تكون الصورة أقل من 1 ميجابايت." },
                    { new Guid("2dba8ecd-54f7-38e6-7c05-351a3812354a"), "en", new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "Take a picture" },
                    { new Guid("35903837-025e-bc6c-7e73-7c042765c7e9"), "en", new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "Add a document" },
                    { new Guid("3a6ee065-0acb-c978-ceb4-9f363647374a"), "ar", new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "يمكن حفظ صورة أو ملف PDF فقط." },
                    { new Guid("43095f56-19bc-18a8-547a-e9c76293573d"), "ar", new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "هذا المستند ليس في هذا المشروع." },
                    { new Guid("463b4b29-7cc8-0c26-5dfa-c77bc476263f"), "ar", new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "تعذّرت قراءة الملف. اختر ملفاً آخر." },
                    { new Guid("5ba314ee-52b4-30f4-57f3-0ac5c664ee0f"), "en", new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "New purchase expense" },
                    { new Guid("666a4015-5ba1-6afb-363b-3c9d6ad3648f"), "en", new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "This project is finished. Nothing in it can change." },
                    { new Guid("82df2b30-b051-34b8-4054-c03c225c78b3"), "en", new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "That document is not in this project." },
                    { new Guid("a178b654-4d77-a92d-9911-613af738fd5a"), "ar", new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "مصروف شراء جديد" },
                    { new Guid("a1e04957-1404-e807-dd72-5678e5bfdc5b"), "en", new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "Take again" },
                    { new Guid("a40204fc-d2db-3ae7-d9d8-908a4ab4765b"), "en", new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "That file could not be read. Pick another." },
                    { new Guid("aa9bbd9b-89ab-252f-cfb9-203794c8f144"), "ar", new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "تعذّر حفظ الملف." },
                    { new Guid("b165a593-83fe-23e6-f8d9-c1118168f082"), "ar", new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "إعادة التصوير" },
                    { new Guid("b2903c28-0c9f-6203-6f3b-d759fc2a3e8f"), "en", new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "Documents" },
                    { new Guid("b921f07d-8c0d-ae7a-a19d-38e9e38e6200"), "ar", new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "إضافة مستند" },
                    { new Guid("bcff7fa0-bddf-3307-61de-2134c2caacbd"), "ar", new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "اختيار من الملفات" },
                    { new Guid("df992583-74f1-ed92-3cf7-1add9451b823"), "en", new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "Pick from files" },
                    { new Guid("e1e22577-9284-d54e-26c9-72202ddf4fa0"), "ar", new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("e9fd8ffe-e017-a51d-a59a-365eed5432b2"), "en", new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "Invoice picture" },
                    { new Guid("f52868e6-a9ec-1988-10ae-c30eb66d8392"), "ar", new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "هذا المشروع منتهٍ، ولا يمكن تغيير شيء فيه." },
                    { new Guid("f9fc0c3d-5718-1b19-c2f8-95f42a77bf6e"), "ar", new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "المستندات" },
                    { new Guid("f9feee8a-40ca-c720-467e-dc4895a3610a"), "en", new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "The camera could not open. Pick the invoice from your files." },
                    { new Guid("fca2ccc2-20cd-c741-f93b-a15a1b4d3494"), "en", new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "The file could not be kept." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'AttachFiles')");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ProjectId_Sha256",
                table: "Attachments",
                columns: new[] { "ProjectId", "Sha256" });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedByUserId",
                table: "Attachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_AttachmentId",
                table: "ContractAttachments",
                column: "AttachmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAttachments");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("55dcc157-3b4b-5c90-7196-d28f141da267"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("da2b0bde-147b-579a-ce58-6486192222af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02f020dc-196d-e1fb-2324-eb94acb99a1a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("17c483cb-3273-e620-d826-c6f5f484fb17"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d2217b8-1b88-c5ea-baed-35fe0e740bb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("23308247-efaf-66c5-c5f5-d175f94a1c7c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b94ed2d-2fe6-83a6-c2ab-5457442fd805"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2cb3a343-530f-50f8-72f8-7a83cc651996"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2cba0c38-b315-245e-dc47-2d09d0065507"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2dba8ecd-54f7-38e6-7c05-351a3812354a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35903837-025e-bc6c-7e73-7c042765c7e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3a6ee065-0acb-c978-ceb4-9f363647374a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("43095f56-19bc-18a8-547a-e9c76293573d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("463b4b29-7cc8-0c26-5dfa-c77bc476263f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ba314ee-52b4-30f4-57f3-0ac5c664ee0f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("666a4015-5ba1-6afb-363b-3c9d6ad3648f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82df2b30-b051-34b8-4054-c03c225c78b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a178b654-4d77-a92d-9911-613af738fd5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a1e04957-1404-e807-dd72-5678e5bfdc5b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a40204fc-d2db-3ae7-d9d8-908a4ab4765b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa9bbd9b-89ab-252f-cfb9-203794c8f144"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b165a593-83fe-23e6-f8d9-c1118168f082"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2903c28-0c9f-6203-6f3b-d759fc2a3e8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b921f07d-8c0d-ae7a-a19d-38e9e38e6200"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bcff7fa0-bddf-3307-61de-2134c2caacbd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df992583-74f1-ed92-3cf7-1add9451b823"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1e22577-9284-d54e-26c9-72202ddf4fa0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9fd8ffe-e017-a51d-a59a-365eed5432b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f52868e6-a9ec-1988-10ae-c30eb66d8392"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9fc0c3d-5718-1b19-c2f8-95f42a77bf6e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9feee8a-40ca-c720-467e-dc4895a3610a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fca2ccc2-20cd-c741-f93b-a15a1b4d3494"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1aed4663-400d-2540-4649-dc62be40737c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c89fb114-b98b-2496-6265-2718ec88059b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e8475111-cb44-b492-0147-e7f68f661565"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount')");
        }
    }
}
