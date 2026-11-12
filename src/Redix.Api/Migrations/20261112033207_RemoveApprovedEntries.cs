using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovedEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryLinks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01694d75-7033-d04e-06d9-306c04c2db34"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0a41375a-0274-0c0c-437a-61f50545564c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ad10ec7-88f3-277a-b0bc-eb9e58b8a4c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0eb3ccf6-1dbe-47fc-c422-d56ea0e8468f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("16879e8e-745e-5921-3ea0-7f1680a38f62"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ed669e7-afc6-36ca-7823-5d19ecb95952"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f6b8966-00b7-308d-7b23-f7b41f6687ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("31335350-a480-235b-d0b4-c0f3d2918f8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("353550c9-aef4-1a44-71f7-b2b06a51a9a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("364aea71-34c3-b58a-083d-84d7e26b7a09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36f70000-08c5-72d2-deee-fac0a3d1c321"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37019142-7349-cf37-1450-a08c64ecf923"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("386c21e1-bb40-666d-117d-7fe443230bd2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("415926f7-be77-7e60-32c6-b148d88126b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("45a53f0c-d82a-832e-b674-ccdccf84fddb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48d1b75f-6c76-2b8a-5411-49d61e13f079"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e9f3d8f-f146-c123-5b76-7404413f51ff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("591b3266-4151-1e1c-6a7a-7214427bfda5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f517095-b4f4-06d1-9094-4fd7bdf60c66"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("72e5cf78-de07-cce0-de7f-40b2df611cc1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("74293746-66ae-0f3e-4d08-05c0ca97fdb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7b58b828-c2cf-e53c-8478-cbe48ed06600"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83181a9f-53d2-9d80-c082-6ff417656f32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c0601ee-c4da-4edc-022a-e11c367fb930"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9073838a-2f46-ad27-73cf-c6b0ae20d925"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("910e13a3-9bbd-343b-d29e-ce3ff1f94bfb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91a310b2-092b-6c89-176c-eb345c0790dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a250177a-544c-04fc-930b-cf7b5fff9c3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8f39fc0-eeed-f4bf-5205-1b9379bb2c18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acfc8c1c-7588-4d1f-ff57-316c21eeb9a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1bea701-6f9c-b217-c4e9-217a2c00f16b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b57355cb-cfb7-9027-c354-74805711707a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be853c47-44da-82bf-9b52-4f1a8daea502"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2309f13-f67c-b053-d1d6-2a5511eb2cd3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c7ba7166-640a-9c59-2812-f69518b7e9a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8b21ef9-b0a3-e4a3-95a4-df794a11f315"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce5c80aa-7aea-17f4-c433-fc160700ca3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d518697f-d192-4e67-cf26-77c7932d3e1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6df82f5-26ef-9d4c-348b-0b6a574c5654"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("da3c932a-97a1-f9bc-a825-824932e35d3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("235b58bb-a013-b56d-484b-3275929fac9f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("72685414-803a-e33f-9629-d4021c1518ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "", "entries.remove" },
                    { new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "", "permissions.removeApprovedEntriesHint" },
                    { new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "", "permissions.removeApprovedEntries" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"),
                column: "Value",
                value: "Recorded as its own row with a minus amount");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"),
                column: "Value",
                value: "يُسجَّل صفاً مستقلاً بمبلغ سالب");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b38ae88e-cbfb-aae2-39c4-c4179c696ea6"),
                column: "Value",
                value: "يعتمد ما ينتظر أو يرفضه.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b49b5d53-aac8-c57c-5092-36c83230e76d"),
                column: "Value",
                value: "Approves or rejects what waits.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0dcb0200-576a-1061-7dda-d5109bd5dae7"), "en", new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "Removes an approved entry: it leaves every list and figure, and its history stays." },
                    { new Guid("3d99b2a2-3292-4ead-2398-247109cce373"), "ar", new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "حذف القيود المعتمدة" },
                    { new Guid("46d429e9-91ff-ab3f-a5c3-6cc7e61e32d6"), "en", new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "Remove" },
                    { new Guid("751e7cc9-ecbb-9284-bce2-a3fa0af82a05"), "ar", new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "حذف" },
                    { new Guid("aa6d1b53-6702-9735-845b-c7279b35a913"), "en", new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "Remove approved entries" },
                    { new Guid("c3fb2f69-2c21-ee75-975e-113586377cb3"), "ar", new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "يحذف القيد المعتمد: يخرج من كل القوائم والأرقام، ويبقى سجله." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0dcb0200-576a-1061-7dda-d5109bd5dae7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d99b2a2-3292-4ead-2398-247109cce373"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46d429e9-91ff-ab3f-a5c3-6cc7e61e32d6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("751e7cc9-ecbb-9284-bce2-a3fa0af82a05"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa6d1b53-6702-9735-845b-c7279b35a913"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c3fb2f69-2c21-ee75-975e-113586377cb3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("689a982e-37ec-94f4-2980-b26a0563e156"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"));

            migrationBuilder.CreateTable(
                name: "EntryLinks",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    LinkedEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryLinks", x => new { x.EntryId, x.Type });
                    table.ForeignKey(
                        name: "FK_EntryLinks_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryLinks_Entries_LinkedEntryId",
                        column: x => x.LinkedEntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "", "entries.entryNotApproved" },
                    { new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "", "expenses.creditNoteAboveOriginal" },
                    { new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "", "expenses.returnable" },
                    { new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "", "entries.reversed" },
                    { new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "", "entries.reverseExplain" },
                    { new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "", "expenses.entryHasCreditNote" },
                    { new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "", "entries.reverse" },
                    { new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "", "capital.reverse" },
                    { new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "", "expenses.entryNotApproved" },
                    { new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "", "expenses.noCreditableInvoice" },
                    { new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "", "entries.entryHasCreditNote" },
                    { new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "", "expenses.alreadyReversed" },
                    { new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "", "expenses.creditNoteNeedsOriginal" },
                    { new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "", "history.reversed" },
                    { new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "", "approvals.originalNotApproved" },
                    { new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "", "capital.reversed" },
                    { new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "", "entries.alreadyReversed" },
                    { new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "", "expenses.originalInvoice" },
                    { new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "", "entries.reverseReason" },
                    { new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "", "expenses.unknownOriginalInvoice" },
                    { new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "", "entries.reverseTitle" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"),
                column: "Value",
                value: "Recorded as a negative against the bill it returns");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"),
                column: "Value",
                value: "يُسجَّل بالسالب على الفاتورة التي يرجعها");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b38ae88e-cbfb-aae2-39c4-c4179c696ea6"),
                column: "Value",
                value: "يعتمد ما ينتظر أو يرفضه، ويعكس القيد المعتمد.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b49b5d53-aac8-c57c-5092-36c83230e76d"),
                column: "Value",
                value: "Approves or rejects what waits, and reverses an approved entry.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01694d75-7033-d04e-06d9-306c04c2db34"), "ar", new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "عُكس هذا القيد من قبل." },
                    { new Guid("0a41375a-0274-0c0c-437a-61f50545564c"), "ar", new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "عكس: {0}" },
                    { new Guid("0ad10ec7-88f3-277a-b0bc-eb9e58b8a4c2"), "en", new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "This entry was already reversed." },
                    { new Guid("0eb3ccf6-1dbe-47fc-c422-d56ea0e8468f"), "ar", new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "الفاتورة الأصلية" },
                    { new Guid("16879e8e-745e-5921-3ea0-7f1680a38f62"), "en", new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "Only an approved entry can be reversed." },
                    { new Guid("1ed669e7-afc6-36ca-7823-5d19ecb95952"), "en", new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "Reverse this approved entry" },
                    { new Guid("1f6b8966-00b7-308d-7b23-f7b41f6687ef"), "en", new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "A credit note returns this invoice, so it cannot be deleted." },
                    { new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"), "ar", new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "حذف" },
                    { new Guid("31335350-a480-235b-d0b4-c0f3d2918f8f"), "ar", new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "معكوس" },
                    { new Guid("353550c9-aef4-1a44-71f7-b2b06a51a9a7"), "en", new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "This supplier has no invoice to return yet." },
                    { new Guid("364aea71-34c3-b58a-083d-84d7e26b7a09"), "en", new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "The invoice this credit note returns is not approved." },
                    { new Guid("36f70000-08c5-72d2-deee-fac0a3d1c321"), "ar", new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "اختر فاتورة لهذا المورد أُرسلت للاعتماد." },
                    { new Guid("37019142-7349-cf37-1450-a08c64ecf923"), "en", new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "Choose an invoice of this supplier that was sent for approval." },
                    { new Guid("386c21e1-bb40-666d-117d-7fe443230bd2"), "ar", new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "السبب (إلزامي)" },
                    { new Guid("415926f7-be77-7e60-32c6-b148d88126b2"), "ar", new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "معكوسة" },
                    { new Guid("45a53f0c-d82a-832e-b674-ccdccf84fddb"), "ar", new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "اختر الفاتورة التي يرجعها هذا المرتجع." },
                    { new Guid("48d1b75f-6c76-2b8a-5411-49d61e13f079"), "en", new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "Choose the bill this credit note returns." },
                    { new Guid("4e9f3d8f-f146-c123-5b76-7404413f51ff"), "en", new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "This entry was already reversed." },
                    { new Guid("591b3266-4151-1e1c-6a7a-7214427bfda5"), "ar", new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "عكس هذا القيد المعتمد" },
                    { new Guid("5f517095-b4f4-06d1-9094-4fd7bdf60c66"), "en", new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "Reversed: {0}" },
                    { new Guid("72e5cf78-de07-cce0-de7f-40b2df611cc1"), "en", new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "Reversed" },
                    { new Guid("74293746-66ae-0f3e-4d08-05c0ca97fdb8"), "en", new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "Reason (required)" },
                    { new Guid("7b58b828-c2cf-e53c-8478-cbe48ed06600"), "ar", new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "لا توجد لهذا المورد فاتورة يمكن إرجاعها بعد." },
                    { new Guid("83181a9f-53d2-9d80-c082-6ff417656f32"), "ar", new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "لا يُعكس إلا القيد المعتمد." },
                    { new Guid("8c0601ee-c4da-4edc-022a-e11c367fb930"), "ar", new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "مرتجع يعيد هذه الفاتورة، فلا تُحذف." },
                    { new Guid("9073838a-2f46-ad27-73cf-c6b0ae20d925"), "ar", new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "عكس" },
                    { new Guid("910e13a3-9bbd-343b-d29e-ce3ff1f94bfb"), "ar", new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "مرتجع يعيد هذه الفاتورة، فلا تُحذف." },
                    { new Guid("91a310b2-092b-6c89-176c-eb345c0790dd"), "en", new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "Reverse" },
                    { new Guid("a250177a-544c-04fc-930b-cf7b5fff9c3d"), "en", new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "Reversed" },
                    { new Guid("a8f39fc0-eeed-f4bf-5205-1b9379bb2c18"), "ar", new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "عُكس هذا القيد من قبل." },
                    { new Guid("acfc8c1c-7588-4d1f-ff57-316c21eeb9a4"), "ar", new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "لا يمكن أن يتجاوز المرتجع المتبقي من فاتورته الأصلية." },
                    { new Guid("b1bea701-6f9c-b217-c4e9-217a2c00f16b"), "en", new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "Original invoice" },
                    { new Guid("b57355cb-cfb7-9027-c354-74805711707a"), "en", new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "Only an approved entry can be reversed." },
                    { new Guid("be853c47-44da-82bf-9b52-4f1a8daea502"), "ar", new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "المتبقي للإرجاع" },
                    { new Guid("c2309f13-f67c-b053-d1d6-2a5511eb2cd3"), "en", new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "Left to return" },
                    { new Guid("c7ba7166-640a-9c59-2812-f69518b7e9a3"), "ar", new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "لا يُعكس إلا القيد المعتمد." },
                    { new Guid("c8b21ef9-b0a3-e4a3-95a4-df794a11f315"), "en", new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "A credit note returns this invoice, so it cannot be deleted." },
                    { new Guid("ce5c80aa-7aea-17f4-c433-fc160700ca3d"), "ar", new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "الفاتورة التي يعيدها هذا المرتجع غير معتمدة." },
                    { new Guid("d518697f-d192-4e67-cf26-77c7932d3e1e"), "ar", new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "يُضاف قيد مقابل مرتبط بهذا القيد ومعتمد فوراً. يظهران في التقارير وصافيهما صفر. ولا يتغيّر هذا القيد." },
                    { new Guid("d6df82f5-26ef-9d4c-348b-0b6a574c5654"), "en", new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "A credit note cannot go above what is left of its original invoice." },
                    { new Guid("da3c932a-97a1-f9bc-a825-824932e35d3c"), "en", new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "An opposite entry is added, tied to this one and approved at once. Both show in the reports, and their net is zero. This one does not change." },
                    { new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"), "en", new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "Remove" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");

            migrationBuilder.CreateIndex(
                name: "IX_EntryLinks_LinkedEntryId",
                table: "EntryLinks",
                column: "LinkedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryLinks_OneReversal",
                table: "EntryLinks",
                column: "LinkedEntryId",
                unique: true,
                filter: "[Type] = 2");
        }
    }
}
