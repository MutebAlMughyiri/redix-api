using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Approvals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.CreateTable(
                name: "EntryDecisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryDecisions_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryDecisions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("d2bdc53e-00df-5caa-8f2c-13197cb0e190"), "ApproveEntries", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "", "approvals.reasonDuplicate" },
                    { new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "", "approvals.failedSome" },
                    { new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "", "approvals.reasonHint" },
                    { new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "", "approvals.noConnection" },
                    { new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "", "approvals.enteredBy" },
                    { new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "", "approvals.approve" },
                    { new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "", "approvals.loadFailed" },
                    { new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "", "approvals.title" },
                    { new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "", "approvals.quickReason" },
                    { new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "", "approvals.reject" },
                    { new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "", "approvals.reasonRequired" },
                    { new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "", "approvals.reasonAmount" },
                    { new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "", "approvals.selectHint" },
                    { new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "", "approvals.empty" },
                    { new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "", "approvals.approveSelected" },
                    { new Guid("819f545f-f676-205c-39b4-9f252160b826"), "", "approvals.refused" },
                    { new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "", "approvals.includingVat" },
                    { new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "", "approvals.entryNotPending" },
                    { new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "", "approvals.fileFailed" },
                    { new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "", "approvals.all" },
                    { new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "", "approvals.openFile" },
                    { new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "", "approvals.reasonUnclear" },
                    { new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "", "approvals.approvedSome" },
                    { new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "", "approvals.originalNotApproved" },
                    { new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "", "approvals.unknownEntry" },
                    { new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "", "approvals.withoutInvoice" },
                    { new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "", "approvals.purchases" },
                    { new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "", "approvals.rejectSend" },
                    { new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "", "approvals.reason" },
                    { new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "", "approvals.reasonItem" },
                    { new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "", "approvals.contractorPayments" },
                    { new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "", "approvals.rejectTitle" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("03b39f24-015d-5117-5b7b-ed8f9e7ee1c7"), "en", new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "Nothing is waiting for approval." },
                    { new Guid("04e59762-1d55-c7e0-2cc8-ce7863419500"), "en", new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "Open the file" },
                    { new Guid("06164929-a665-1198-ec1f-a6e2278dbb3a"), "en", new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "Could not read what is waiting. Check the connection and try again." },
                    { new Guid("12d4dbef-b9a3-c10f-d5cf-2739afb93706"), "ar", new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "السبب (إلزامي)" },
                    { new Guid("1c80da2b-03c3-c8c8-8abb-7274da8f1a9c"), "ar", new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "تعذر فتح الملف." },
                    { new Guid("218355fe-589d-59e8-14e8-bdb6facad513"), "ar", new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "تم الاعتماد" },
                    { new Guid("22cef7e2-36dd-770d-ced0-8d5b085caed8"), "ar", new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "اذكر سبب الرفض." },
                    { new Guid("2a6ffd43-a6d2-9a19-2806-2d3ba4f09be0"), "ar", new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "أدخله" },
                    { new Guid("2f48c14b-20cc-5567-9008-15088029f85d"), "ar", new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "المبلغ لا يطابق الفاتورة" },
                    { new Guid("32077232-7d2c-03e6-7827-370f417338c8"), "en", new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "Someone already decided this entry." },
                    { new Guid("364aea71-34c3-b58a-083d-84d7e26b7a09"), "en", new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "The invoice this credit note returns is not approved." },
                    { new Guid("41e67c6a-8c44-72db-6a8f-2f37885b0177"), "ar", new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "بدون فاتورة" },
                    { new Guid("443b2d31-c9c4-7214-cbde-71b4a0e0f983"), "en", new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "Entered by" },
                    { new Guid("44b7f447-f095-ece2-d777-1535fd54a35e"), "en", new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "Reason (required)" },
                    { new Guid("49b88906-65b7-e97a-03a7-b77839b70e35"), "ar", new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "بانتظار الاعتماد" },
                    { new Guid("551a5636-c500-9bee-823f-1e76d3761df4"), "ar", new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "اعتماد" },
                    { new Guid("56c052ca-bee3-7631-652a-52c9e77f9042"), "en", new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "The reason reaches who entered it, and the entry returns to them to fix and send again." },
                    { new Guid("5d19dbbc-dbc0-28b5-dc87-25b064a2625a"), "en", new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "Approved" },
                    { new Guid("61cbc328-18e3-a3d2-6e9b-0779a96923cd"), "en", new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "Including VAT" },
                    { new Guid("644b6cb5-ef60-82a9-dce7-c84a3a6d72b1"), "ar", new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("693d629a-fe16-83d6-8e03-19cfbb917165"), "ar", new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "الكل" },
                    { new Guid("71329c37-e1c2-c006-923b-458fb2aa7639"), "en", new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "Reject" },
                    { new Guid("7433a2e0-7a84-7689-9953-0d337a0118d2"), "en", new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "Approve selected" },
                    { new Guid("793dd720-f479-ed71-932f-0c1c98df1087"), "en", new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "The amount does not match the invoice" },
                    { new Guid("7cfff014-ce15-fd80-fa6a-1cc505275f3f"), "en", new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "No connection to the server. Try again." },
                    { new Guid("82796d89-8715-48eb-b8b4-22e21b09a550"), "en", new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "Could not be approved" },
                    { new Guid("839b885e-723e-6b9b-9ee1-40cac7accf67"), "ar", new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "مشتريات" },
                    { new Guid("855942fd-638f-0214-64e2-939ead8a7376"), "ar", new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "رفض وإرسال السبب" },
                    { new Guid("86930c14-e8d1-8a00-7d35-1db59f2d309a"), "ar", new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "صورة الفاتورة غير واضحة" },
                    { new Guid("86d7a850-1543-d609-addb-941191a53738"), "ar", new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "شامل ضريبة" },
                    { new Guid("8ec27bfb-8b44-3252-6a5b-fc8e2758df54"), "ar", new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "اعتماد المحدد" },
                    { new Guid("90c5a3e6-4ad5-691e-d348-74da5e463ad6"), "ar", new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "يصل السبب لمن أدخله، ويعود القيد له للتعديل وإعادة الإرسال." },
                    { new Guid("94e33314-5b6a-29aa-4f31-c965847747e8"), "en", new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "Contractor payments" },
                    { new Guid("95e26c3a-3ccc-aec4-d36c-f245c0887c56"), "ar", new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "البند غير صحيح" },
                    { new Guid("9a577061-dc73-d48f-f237-47fa260c1483"), "ar", new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "فاتورة مكررة" },
                    { new Guid("9d85559e-6be3-186a-1200-acaa8a1d6b94"), "en", new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "Approve" },
                    { new Guid("a058e5f4-fd5f-ed10-d0b3-7853475b6212"), "en", new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "Reject the expense" },
                    { new Guid("a6902aee-08d2-3320-b4e2-03e40172436b"), "en", new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "Purchases" },
                    { new Guid("a88558ba-bb33-e988-ed57-87f27785bff9"), "ar", new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "دفعات مقاولين" },
                    { new Guid("b0af1a92-9ebc-9d56-9d0f-20647e0c25b9"), "en", new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "The item is wrong" },
                    { new Guid("b1a8fd40-fb65-e3ee-3821-eb0ea0cad85a"), "ar", new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "فتح الملف" },
                    { new Guid("b32662e1-ac20-3ef9-0d7e-5d6dd85158d3"), "en", new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "All" },
                    { new Guid("b9851c93-1378-b029-25f2-03f1e9fdb0ef"), "ar", new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "تعذر اعتماده" },
                    { new Guid("ba526b82-2fa1-8b38-b8db-5158c720e391"), "en", new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "The file could not be opened." },
                    { new Guid("bbf07f8f-5ce8-63d7-c2af-8c3d93fa735f"), "en", new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "Without invoice" },
                    { new Guid("c612bdfb-b377-78c1-3b1f-987856a54580"), "en", new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "Reject and send the reason" },
                    { new Guid("ce5c80aa-7aea-17f4-c433-fc160700ca3d"), "ar", new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"), "الفاتورة التي يعيدها هذا المرتجع غير معتمدة." },
                    { new Guid("d04ce8da-1296-b46c-3ef8-bab65fd39e5c"), "en", new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "A repeated invoice" },
                    { new Guid("d4fb6ff0-b19d-d02e-7359-104e3308068d"), "ar", new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "سبب سريع" },
                    { new Guid("d5c9d567-f42c-4f75-1691-5ddfb6d34464"), "en", new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "Select entries to approve together" },
                    { new Guid("d5e21278-1717-4036-641e-8b0d7797277d"), "en", new Guid("819f545f-f676-205c-39b4-9f252160b826"), "This could not be done." },
                    { new Guid("d94d2fd7-57a3-2f53-4977-0dc14eec5101"), "ar", new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "رفض" },
                    { new Guid("e62cb5fe-0240-4708-0dd2-309159d1014a"), "ar", new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "حدّد قيوداً للاعتماد الجماعي" },
                    { new Guid("e8299e2f-fcbd-ac3e-feee-9f81c3ea1eb4"), "ar", new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "هذا القيد لم يعد هنا." },
                    { new Guid("efc5d5da-391a-a123-fc6b-eeed7bc13c5e"), "ar", new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "رفض المصروف" },
                    { new Guid("f0027403-0a50-4a40-94c4-365c9024e797"), "ar", new Guid("819f545f-f676-205c-39b4-9f252160b826"), "تعذر تنفيذ ذلك." },
                    { new Guid("f2da35e5-3b03-99a2-c729-2d87804062db"), "en", new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "Waiting for approval" },
                    { new Guid("f401ad31-bcc4-056e-48a2-85618c6cc068"), "ar", new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "لا شيء بانتظار الاعتماد." },
                    { new Guid("f632250b-caa1-c286-6db0-532735828b18"), "en", new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "The invoice's picture is not clear" },
                    { new Guid("f8288b56-1c40-60c8-d18e-b0fdc84783b4"), "ar", new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "تعذرت قراءة ما ينتظر. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f88687a8-b936-fede-3159-9db15e0eeabe"), "en", new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "This entry is no longer here." },
                    { new Guid("f8b063d7-3f00-126f-7aef-cfc899cc7dc6"), "ar", new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "قرر أحدهم هذا القيد من قبل." },
                    { new Guid("fb73e8c3-b65d-7031-bf76-909bd16e8128"), "en", new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "Say why it is rejected." },
                    { new Guid("ffd38969-1890-8b22-fd24-8a302ffdc79d"), "en", new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "Quick reason" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles', 'ApproveEntries')");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_EntryId",
                table: "EntryDecisions",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_UserId",
                table: "EntryDecisions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryDecisions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d2bdc53e-00df-5caa-8f2c-13197cb0e190"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03b39f24-015d-5117-5b7b-ed8f9e7ee1c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04e59762-1d55-c7e0-2cc8-ce7863419500"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06164929-a665-1198-ec1f-a6e2278dbb3a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12d4dbef-b9a3-c10f-d5cf-2739afb93706"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c80da2b-03c3-c8c8-8abb-7274da8f1a9c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("218355fe-589d-59e8-14e8-bdb6facad513"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("22cef7e2-36dd-770d-ced0-8d5b085caed8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a6ffd43-a6d2-9a19-2806-2d3ba4f09be0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f48c14b-20cc-5567-9008-15088029f85d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32077232-7d2c-03e6-7827-370f417338c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("364aea71-34c3-b58a-083d-84d7e26b7a09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41e67c6a-8c44-72db-6a8f-2f37885b0177"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("443b2d31-c9c4-7214-cbde-71b4a0e0f983"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44b7f447-f095-ece2-d777-1535fd54a35e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49b88906-65b7-e97a-03a7-b77839b70e35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("551a5636-c500-9bee-823f-1e76d3761df4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56c052ca-bee3-7631-652a-52c9e77f9042"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d19dbbc-dbc0-28b5-dc87-25b064a2625a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61cbc328-18e3-a3d2-6e9b-0779a96923cd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("644b6cb5-ef60-82a9-dce7-c84a3a6d72b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("693d629a-fe16-83d6-8e03-19cfbb917165"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71329c37-e1c2-c006-923b-458fb2aa7639"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7433a2e0-7a84-7689-9953-0d337a0118d2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("793dd720-f479-ed71-932f-0c1c98df1087"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cfff014-ce15-fd80-fa6a-1cc505275f3f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82796d89-8715-48eb-b8b4-22e21b09a550"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("839b885e-723e-6b9b-9ee1-40cac7accf67"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("855942fd-638f-0214-64e2-939ead8a7376"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86930c14-e8d1-8a00-7d35-1db59f2d309a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86d7a850-1543-d609-addb-941191a53738"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ec27bfb-8b44-3252-6a5b-fc8e2758df54"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90c5a3e6-4ad5-691e-d348-74da5e463ad6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94e33314-5b6a-29aa-4f31-c965847747e8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95e26c3a-3ccc-aec4-d36c-f245c0887c56"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a577061-dc73-d48f-f237-47fa260c1483"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d85559e-6be3-186a-1200-acaa8a1d6b94"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a058e5f4-fd5f-ed10-d0b3-7853475b6212"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a6902aee-08d2-3320-b4e2-03e40172436b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a88558ba-bb33-e988-ed57-87f27785bff9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0af1a92-9ebc-9d56-9d0f-20647e0c25b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1a8fd40-fb65-e3ee-3821-eb0ea0cad85a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b32662e1-ac20-3ef9-0d7e-5d6dd85158d3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b9851c93-1378-b029-25f2-03f1e9fdb0ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba526b82-2fa1-8b38-b8db-5158c720e391"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bbf07f8f-5ce8-63d7-c2af-8c3d93fa735f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c612bdfb-b377-78c1-3b1f-987856a54580"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce5c80aa-7aea-17f4-c433-fc160700ca3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d04ce8da-1296-b46c-3ef8-bab65fd39e5c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d4fb6ff0-b19d-d02e-7359-104e3308068d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5c9d567-f42c-4f75-1691-5ddfb6d34464"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5e21278-1717-4036-641e-8b0d7797277d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d94d2fd7-57a3-2f53-4977-0dc14eec5101"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e62cb5fe-0240-4708-0dd2-309159d1014a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e8299e2f-fcbd-ac3e-feee-9f81c3ea1eb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efc5d5da-391a-a123-fc6b-eeed7bc13c5e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0027403-0a50-4a40-94c4-365c9024e797"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2da35e5-3b03-99a2-c729-2d87804062db"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f401ad31-bcc4-056e-48a2-85618c6cc068"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f632250b-caa1-c286-6db0-532735828b18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8288b56-1c40-60c8-d18e-b0fdc84783b4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f88687a8-b936-fede-3159-9db15e0eeabe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8b063d7-3f00-126f-7aef-cfc899cc7dc6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb73e8c3-b65d-7031-bf76-909bd16e8128"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ffd38969-1890-8b22-fd24-8a302ffdc79d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("819f545f-f676-205c-39b4-9f252160b826"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8f382797-5142-3cb6-5646-b245912d0d89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b12efad9-a4d3-575e-31f4-780b64154463"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b69a5f55-cf11-eda2-9d54-f11d11e89ab8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dc6108e5-d00e-7366-079d-c085383fd361"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles')");
        }
    }
}
