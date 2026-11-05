using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoneyMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.CreateTable(
                name: "CapitalMovementDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CashReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapitalMovementDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_CapitalMovementDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapitalMovementDetails_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerPockets",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerPockets", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_PartnerPockets_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerPockets_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reconciliations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reconciliations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationBalance",
                columns: table => new
                {
                    ReconciliationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualBalance = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationBalance", x => new { x.ReconciliationId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_ReconciliationBalance_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationBalance_Reconciliations_ReconciliationId",
                        column: x => x.ReconciliationId,
                        principalTable: "Reconciliations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("c7f4a654-572d-95ea-4d74-7ac8c594fe39"), "MoveMoney", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("08ac9790-1b59-784b-f7e4-95d2001ab440"), "", "transfers.description" },
                    { new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "", "capital.receiptDiffers" },
                    { new Guid("0e5dbece-685e-ddaf-7346-ab720f223e6d"), "", "capital.settlementAndExit" },
                    { new Guid("12dee883-46e2-1760-edae-1ae87739c466"), "", "capital.carryIn" },
                    { new Guid("131d5c36-f4df-4116-8a87-496f2b173c90"), "", "capital.targetProjectFinished" },
                    { new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "", "reconciliations.surplus" },
                    { new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "", "capital.withdrawal" },
                    { new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "", "approvals.addToCapital" },
                    { new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "", "capital.description" },
                    { new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "", "transfers.amount" },
                    { new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "", "accounts.reconcile" },
                    { new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "", "reconciliations.invalidAmount" },
                    { new Guid("2947ee0e-8693-df6a-f16e-772993743f60"), "", "capital.noConnection" },
                    { new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "", "transfers.invalidNotes" },
                    { new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "", "reconciliations.actualTotal" },
                    { new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "", "approvals.paidBy" },
                    { new Guid("33fd8cb1-4c21-0fe8-f918-3232696f1509"), "", "capital.movement" },
                    { new Guid("34f06359-9fdd-3694-1843-c4796e1a3c3c"), "", "capital.useCarry" },
                    { new Guid("3d786fce-bf95-cba4-8c8b-3f82659f7b75"), "", "transfers.invalidDate" },
                    { new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "", "transfers.to" },
                    { new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "", "reconciliations.never" },
                    { new Guid("42b6a274-7add-5e52-5bed-4255134ef780"), "", "capital.invalidNotes" },
                    { new Guid("44c7e9dc-42b6-87e5-da00-6a4c86f7c464"), "", "transfers.invoiceAlreadyAttached" },
                    { new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "", "capital.receiptAmount" },
                    { new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "", "reconciliations.unknownAccount" },
                    { new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "", "capital.left" },
                    { new Guid("58e1e1ec-57af-babd-352b-60948010e4ff"), "", "capital.noTargetProject" },
                    { new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "", "transfers.from" },
                    { new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "", "reconciliations.owed" },
                    { new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "", "capital.receiptHint" },
                    { new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "", "reconciliations.loadFailed" },
                    { new Guid("6868bf1a-7f58-abc5-e5f2-9423f3583c0b"), "", "transfers.receipt" },
                    { new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "", "transfers.noConnection" },
                    { new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "", "reconciliations.books" },
                    { new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "", "capital.invalidReason" },
                    { new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "", "capital.active" },
                    { new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "", "transfers.date" },
                    { new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "", "capital.reverse" },
                    { new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "", "capital.type" },
                    { new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "", "accounts.typePartnerPocket" },
                    { new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "", "transfers.save" },
                    { new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "", "capital.receiptRequired" },
                    { new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "", "reconciliations.noConnection" },
                    { new Guid("7f8e752c-d0ca-016a-0c98-46a91831f82f"), "", "capital.moveTitle" },
                    { new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "", "capital.partnerLeft" },
                    { new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "", "approvals.owedBack" },
                    { new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "", "capital.date" },
                    { new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "", "transfers.refused" },
                    { new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "", "capital.partner" },
                    { new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "", "reconciliations.accountMissing" },
                    { new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "", "capital.loadFailed" },
                    { new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "", "reconciliations.difference" },
                    { new Guid("953c2b0f-61d6-4868-deab-aedf50f3e206"), "", "capital.inKind" },
                    { new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "", "reconciliations.refused" },
                    { new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "", "entries.tiedProjectRefused" },
                    { new Guid("9c7c9263-970f-5773-324e-24508ec84b64"), "", "capital.invalidAmount" },
                    { new Guid("9c86d593-d4d2-fcb8-7dc2-bff92bb94747"), "", "capital.unknownPartner" },
                    { new Guid("9de629bb-c4a8-6179-e0d8-36584d32359e"), "", "capital.sameProject" },
                    { new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "", "capital.amount" },
                    { new Guid("a971dc40-ac12-dde4-adcb-622cb68b640f"), "", "capital.targetPartner" },
                    { new Guid("a98676c3-6be5-87ba-11e3-78ecb152e69b"), "", "share.capitalReceipt" },
                    { new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "", "reconciliations.title" },
                    { new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "", "reconciliations.differenceExplain" },
                    { new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "", "capital.receipt" },
                    { new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "", "capital.cashReason" },
                    { new Guid("b4a807aa-2305-9bca-e838-c8b5ff45f3ed"), "", "capital.targetAccount" },
                    { new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "", "reconciliations.lastUpdated" },
                    { new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "", "capital.receiptAttached" },
                    { new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "", "capital.receiptRequiredWord" },
                    { new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "", "reconciliations.enterTitle" },
                    { new Guid("bfa1fa05-f26d-8943-7f56-8d4c078e2257"), "", "transfers.latest" },
                    { new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "", "capital.share" },
                    { new Guid("c0a8bdb1-a6c9-5390-95a4-fb49750ef056"), "", "transfers.unknownAttachment" },
                    { new Guid("c17501cc-c934-986e-b341-9af805118535"), "", "transfers.loadFailed" },
                    { new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "", "capital.paidInCash" },
                    { new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "", "capital.contribution" },
                    { new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "", "capital.save" },
                    { new Guid("c768947e-2f79-d2e3-b55b-090ebe86ebf3"), "", "capital.refused" },
                    { new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "", "transfers.title" },
                    { new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "", "capital.reversed" },
                    { new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "", "capital.total" },
                    { new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "", "reconciliations.save" },
                    { new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "", "accounts.invalidType" },
                    { new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "", "transfers.invalidAmount" },
                    { new Guid("d731c9fc-f78f-ef52-1c7c-422497baaec3"), "", "capital.unknownAccount" },
                    { new Guid("d78a1d43-4602-993e-c0e4-c80249b0e2dd"), "", "capital.invalidDate" },
                    { new Guid("d98464da-fea3-ea77-833c-72f09c2519f4"), "", "capital.invoiceAlreadyAttached" },
                    { new Guid("dc95b2fe-3d7d-325d-3967-fa8823add2aa"), "", "capital.unknownAttachment" },
                    { new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "", "approvals.invalidTreatment" },
                    { new Guid("df8ddbc3-91ab-4e33-46e0-c213e9972453"), "", "accounts.transfer" },
                    { new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "", "capital.account" },
                    { new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "", "transfers.sameAccount" },
                    { new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "", "accounts.pocketFollowsPartner" },
                    { new Guid("eb90bb5d-5598-ad0e-2afd-235e61c818c0"), "", "capital.targetProjectRefused" },
                    { new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "", "capital.cash" },
                    { new Guid("ee41fc05-238a-7fa4-74ac-26f0d2eb2cd0"), "", "transfers.done" },
                    { new Guid("efb26472-c9f1-d09d-a446-ff4b9b4b6f11"), "", "capital.saved" },
                    { new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "", "capital.balance" },
                    { new Guid("f273d941-0faa-8823-6abd-e6ccd5c738c7"), "", "capital.refundableAdvance" },
                    { new Guid("f337be3a-926a-6830-d1e4-51029f1c6751"), "", "capital.targetProject" },
                    { new Guid("f3911e30-8e7e-446b-5cdc-5abc892256c6"), "", "capital.settlementNotFull" },
                    { new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "", "approvals.notPaidByPartner" },
                    { new Guid("fab455e4-8730-850d-072a-e3362d32dc40"), "", "capital.carryOut" },
                    { new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "", "transfers.unknownAccount" },
                    { new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "", "capital.movements" },
                    { new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "", "capital.noMovements" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0302765a-207a-ef61-b080-edcafcd746be"), "en", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "Share %" },
                    { new Guid("0358ca3a-b06f-1b0d-6774-63f4d87ed9bf"), "en", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "The description is too long." },
                    { new Guid("03c52fe2-d77d-0c74-573c-cbfa80d68f68"), "ar", new Guid("d78a1d43-4602-993e-c0e4-c80249b0e2dd"), "التاريخ خارج فترة المشروع." },
                    { new Guid("05da0418-2ef1-8e52-1fd3-ececb22e393c"), "en", new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "Bank transfer receipt" },
                    { new Guid("07acd0ad-901f-43ff-a2c9-ee8e9480a02f"), "ar", new Guid("b4a807aa-2305-9bca-e838-c8b5ff45f3ed"), "الحساب فيه" },
                    { new Guid("0849e532-c3d5-0a6f-ee5d-ff0ae1adb6b8"), "ar", new Guid("c0a8bdb1-a6c9-5390-95a4-fb49750ef056"), "لم يُعثر على الملف. اختره مجدداً." },
                    { new Guid("088b4415-9e5f-98dc-cd84-933231642cd3"), "ar", new Guid("f337be3a-926a-6830-d1e4-51029f1c6751"), "المشروع المرحَّل إليه" },
                    { new Guid("0c01fce5-3d51-8aa0-2a5c-a50452f4252e"), "en", new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "Add to capital" },
                    { new Guid("0c771f3b-91ad-f26f-f404-f1d30227451d"), "ar", new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "لا حركات بعد." },
                    { new Guid("0e1d6f19-e18a-ba67-da68-7bf5ca0fb299"), "en", new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "No connection to the server. Try again." },
                    { new Guid("0ef4fa30-5d7f-43af-b1e2-1297a6914764"), "ar", new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "الفرق يعني مصروفاً لم يُسجَّل أو رسوماً بنكية. راجع حركات الحسابات منذ آخر مطابقة." },
                    { new Guid("0f201428-1dbf-942f-846e-6bab11651c61"), "ar", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "الحسابات والمطابقة" },
                    { new Guid("0f939def-ba49-d8b0-6cd9-c8374be39c6f"), "en", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "This could not be done." },
                    { new Guid("13fe814f-e97b-240a-1d07-2d2f58c3db74"), "en", new Guid("a971dc40-ac12-dde4-adcb-622cb68b640f"), "Partner there" },
                    { new Guid("1649079c-ff19-3707-a4e7-6526e35265b9"), "en", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "Description" },
                    { new Guid("1704fe25-8eec-50ed-f814-655e4aafc8c7"), "ar", new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "في الدفاتر" },
                    { new Guid("18c6b73a-3314-576e-e09a-3046a2426b66"), "en", new Guid("44c7e9dc-42b6-87e5-da00-6a4c86f7c464"), "This file is already on an entry." },
                    { new Guid("19b78b19-b279-9cf5-640e-48fc50f7b0b1"), "en", new Guid("9c86d593-d4d2-fcb8-7dc2-bff92bb94747"), "Choose the partner." },
                    { new Guid("1a1944db-a39a-8ece-3821-6ade63ff1f5e"), "en", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "Transfer" },
                    { new Guid("1a94905d-b7db-9426-6022-52ecf68252da"), "en", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "Date" },
                    { new Guid("1ee041aa-9dc1-8d41-3e06-a22ff68ebaf6"), "ar", new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "حساب الشريك يتبع الشريك." },
                    { new Guid("1ff16692-1a30-8b9e-7760-46a40d30c6b6"), "ar", new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "المبلغ في الإيصال يخالف هذه الحركة." },
                    { new Guid("20ae5177-1128-f50d-dec6-edf0138c926a"), "ar", new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "من شريك" },
                    { new Guid("21ba1404-a7f0-25d2-d982-e500627dad9e"), "en", new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "Choose what becomes of it." },
                    { new Guid("22d24d9b-e183-8d43-ba45-667101bb358c"), "ar", new Guid("eb90bb5d-5598-ad0e-2afd-235e61c818c0"), "لا تدير الأموال في ذلك المشروع." },
                    { new Guid("23375d30-be30-7728-7613-49a2ef55fc47"), "en", new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "Could not read the reconciliation. Check the connection and try again." },
                    { new Guid("24e3411b-a03b-2249-6666-030c5600255a"), "en", new Guid("6868bf1a-7f58-abc5-e5f2-9423f3583c0b"), "A picture or PDF of the receipt" },
                    { new Guid("24fc50d0-951c-b5c1-b71c-743af5799106"), "ar", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "الحساب" },
                    { new Guid("25ad6845-2996-1d4b-049a-adec75ec8f75"), "ar", new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "لا مطابقة بعد." },
                    { new Guid("28c424cc-02e9-834b-44a0-55936e13e836"), "en", new Guid("953c2b0f-61d6-4868-deab-aedf50f3e206"), "In kind or other" },
                    { new Guid("28c68a9b-b43c-e11b-a3db-4747ce7b04a3"), "en", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "Account" },
                    { new Guid("2924c1e9-3455-6329-eefc-c20015df9d83"), "ar", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "الوصف" },
                    { new Guid("2a92db36-c3f4-4a8e-e087-56044b39adc2"), "ar", new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "نقداً" },
                    { new Guid("2b4ba849-7fe1-ff6b-964d-491b9e8c990c"), "ar", new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "دُفع نقداً بلا إيصال" },
                    { new Guid("2de57f71-b125-f830-cce1-d9acfe45a9ea"), "en", new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "From a partner" },
                    { new Guid("2e9a14ad-4748-d7ef-1723-cd3be17fdb01"), "en", new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "Why" },
                    { new Guid("2ed1793f-4d04-1d7c-3010-a9f805f79d32"), "en", new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "No partner paid this." },
                    { new Guid("2fe21422-63b6-1782-b29a-934f085e976a"), "ar", new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "الحركات" },
                    { new Guid("32179368-0d1d-84b6-fcdd-d899caf3d8e4"), "en", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "Accounts and reconciliation" },
                    { new Guid("35888947-ef7f-4379-26f3-fd5103c78588"), "ar", new Guid("9c7c9263-970f-5773-324e-24508ec84b64"), "اكتب مبلغاً أكبر من الصفر بخانتين عشريتين كحد أقصى." },
                    { new Guid("35d6a48f-8a15-9d7b-02a7-90a5855f0fa3"), "ar", new Guid("ee41fc05-238a-7fa4-74ac-26f0d2eb2cd0"), "نُقل المبلغ." },
                    { new Guid("36661599-e814-80b8-2bb6-dc1b66ee8caa"), "en", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "Movement type" },
                    { new Guid("382033e1-e8cd-efaf-116f-894462850a27"), "ar", new Guid("efb26472-c9f1-d09d-a446-ff4b9b4b6f11"), "حُفظت الحركة." },
                    { new Guid("3ba98db2-f9a0-2e22-cf41-cb29101fec5b"), "ar", new Guid("42b6a274-7add-5e52-5bed-4255134ef780"), "الوصف طويل جداً." },
                    { new Guid("3df3c585-78d9-6f9d-c618-5e51af2f3075"), "en", new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "An account is no longer in the project." },
                    { new Guid("3f565137-3037-3eea-e282-54342b0f3fc5"), "en", new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "Cash" },
                    { new Guid("3f7dd83a-8c5b-a11e-79e9-6980dcf22ea7"), "en", new Guid("c768947e-2f79-d2e3-b55b-090ebe86ebf3"), "This could not be done." },
                    { new Guid("3fdeabd3-789a-f9da-6d6a-07c4249b8bb7"), "en", new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "This kind of account comes with its partner." },
                    { new Guid("415926f7-be77-7e60-32c6-b148d88126b2"), "ar", new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "معكوسة" },
                    { new Guid("41f93c88-580e-726b-319e-227d53528a1f"), "ar", new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "منسحب" },
                    { new Guid("434311bd-4a54-392b-6bcf-e6e48504337e"), "en", new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "No reconciliation yet." },
                    { new Guid("4377bf43-44e3-4564-cc1a-6d2706d71b4d"), "ar", new Guid("dc95b2fe-3d7d-325d-3967-fa8823add2aa"), "لم يُعثر على الملف. اختره مجدداً." },
                    { new Guid("4383a71c-b456-fe03-7978-70949c20f39d"), "en", new Guid("08ac9790-1b59-784b-f7e4-95d2001ab440"), "Description" },
                    { new Guid("447a8b30-8528-f56c-4696-db39d6f11178"), "en", new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "A partner's pocket follows the partner." },
                    { new Guid("482337dd-a382-4307-5f05-0d97482cec16"), "en", new Guid("3d786fce-bf95-cba4-8c8b-3f82659f7b75"), "The date is outside the project's period." },
                    { new Guid("48531794-f5d1-7533-83d3-01e9f7139f52"), "en", new Guid("131d5c36-f4df-4116-8a87-496f2b173c90"), "That project is closed." },
                    { new Guid("48ecb1c6-2d74-1a48-87a5-f5a39260e539"), "ar", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "تحويل بين الحسابات" },
                    { new Guid("4a30ce9d-805d-235d-f7b8-44dd6fec981c"), "en", new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "Attach the bank receipt, or say why it was paid in cash." },
                    { new Guid("4a61a8f1-9f82-5227-2223-ecff83997d51"), "ar", new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "السبب" },
                    { new Guid("4aa98f96-75ac-2ae6-cec9-f1709d7133dc"), "en", new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "Type the balance of every account." },
                    { new Guid("4c266476-3087-ee7b-69e8-3a18e9c47b0e"), "en", new Guid("ee41fc05-238a-7fa4-74ac-26f0d2eb2cd0"), "The money was moved." },
                    { new Guid("4cd7e391-c11a-644f-23f1-581e915808bf"), "ar", new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "تعذرت قراءة المطابقة. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("4cd9a712-c7ad-99f7-7061-b957811b6793"), "ar", new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "مجموع الأرصدة الفعلية" },
                    { new Guid("4df36ad6-8f02-af10-ef8c-69d7e61a70ea"), "en", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "Choose both accounts." },
                    { new Guid("4f61f961-4af2-7954-983f-157807d88e85"), "ar", new Guid("7f8e752c-d0ca-016a-0c98-46a91831f82f"), "حركة رأس مال" },
                    { new Guid("4f8e9555-39a8-17dd-3fb1-2883764f6841"), "ar", new Guid("9de629bb-c4a8-6179-e0d8-36584d32359e"), "اختر مشروعاً آخر." },
                    { new Guid("500e9ea9-5c04-5553-4a7e-f79933275c60"), "ar", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "يرتبط هذا بقيد في مشروع لا يمكنك تغييره." },
                    { new Guid("50187024-acc6-c3e6-4494-304b03281c45"), "ar", new Guid("bfa1fa05-f26d-8943-7f56-8d4c078e2257"), "آخر التحويلات" },
                    { new Guid("50a061a0-b5b3-fb25-e183-e6311159a40b"), "ar", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "إلى حساب" },
                    { new Guid("50a837fd-6102-8de2-2be6-168dc9f95e86"), "en", new Guid("12dee883-46e2-1760-edae-1ae87739c466"), "Carry from a project" },
                    { new Guid("50d20cae-4408-fdd2-39a8-47d8014b6fd8"), "en", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "Amount" },
                    { new Guid("536b42a4-341d-c61f-7387-65a9cef080b8"), "ar", new Guid("08ac9790-1b59-784b-f7e4-95d2001ab440"), "الوصف" },
                    { new Guid("541b0f09-c1b1-dcfd-db58-224a26d77bf0"), "en", new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "Save the balances" },
                    { new Guid("543184d3-ee5b-cf4f-a6f5-138fd410f649"), "ar", new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "يُضاف إلى رأس المال" },
                    { new Guid("54c9ba2e-2749-c51e-50c0-ccd1c56f7ef0"), "en", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "Type an amount above zero, with at most two decimals." },
                    { new Guid("5695bc1a-35f5-7512-673e-2f7cbb309647"), "en", new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "Withdrew" },
                    { new Guid("5834fc2a-cf04-d2c6-2af3-290eb5a4883a"), "en", new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "In the books" },
                    { new Guid("5f256c0e-9331-3556-0f84-da17cc43fc48"), "en", new Guid("33fd8cb1-4c21-0fe8-f918-3232696f1509"), "Movement" },
                    { new Guid("5f5f83b3-7d16-a2de-28ef-a9f4cd7e34f7"), "ar", new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "أرفق الإيصال البنكي، أو اذكر سبب الدفع نقداً." },
                    { new Guid("64ffce39-7460-8b8e-946c-49d4db39d129"), "en", new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "Amount on the receipt" },
                    { new Guid("6740acb9-caf0-ce05-919f-a656798e7dba"), "en", new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "Total capital" },
                    { new Guid("677320db-1297-f1da-6f17-906d6bc3a995"), "ar", new Guid("a971dc40-ac12-dde4-adcb-622cb68b640f"), "الشريك فيه" },
                    { new Guid("69ff256e-564e-2525-5a6c-451d7e6a5592"), "en", new Guid("d98464da-fea3-ea77-833c-72f09c2519f4"), "This file is already on an entry." },
                    { new Guid("6cd5258f-0d14-d780-9b60-861f76fc0262"), "ar", new Guid("f273d941-0faa-8823-6abd-e6ccd5c738c7"), "سلفة مستردة" },
                    { new Guid("6d1a646c-a942-01a3-32a4-885b192f3d03"), "en", new Guid("f273d941-0faa-8823-6abd-e6ccd5c738c7"), "Refundable advance" },
                    { new Guid("6fe229df-5870-3913-b2a4-0ad2dcb32b88"), "en", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "From account" },
                    { new Guid("70ebae12-6786-5571-bf14-cc6b7c44343b"), "ar", new Guid("2947ee0e-8693-df6a-f16e-772993743f60"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("713aef6d-b027-7b8d-478a-4924cb333e42"), "ar", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "من حساب" },
                    { new Guid("719faf28-945b-6645-53bb-1160f38e092d"), "en", new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "A difference means an expense not recorded, or bank fees. Check the accounts' movements since the last reconciliation." },
                    { new Guid("72a747e5-aa1e-46a0-df21-2ce3cb338e9b"), "en", new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "Contribution" },
                    { new Guid("72abe461-ae43-cd25-dad1-4b187f9a3c03"), "ar", new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("72e5cf78-de07-cce0-de7f-40b2df611cc1"), "en", new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"), "Reversed" },
                    { new Guid("742e5634-c91e-e4b1-98b6-96be500f9b34"), "ar", new Guid("6868bf1a-7f58-abc5-e5f2-9423f3583c0b"), "صورة أو PDF للإيصال" },
                    { new Guid("76bc6f1c-7782-2775-c1f1-ffb5c3413e81"), "en", new Guid("9de629bb-c4a8-6179-e0d8-36584d32359e"), "Choose another project." },
                    { new Guid("793b44c1-0764-c1af-e669-20042eb036e6"), "ar", new Guid("a98676c3-6be5-87ba-11e3-78ecb152e69b"), "إيصال بنكي لحركة رأس مال" },
                    { new Guid("7ad7896a-63e3-f7e6-8968-c3ed2b1004ac"), "ar", new Guid("34f06359-9fdd-3694-1843-c4796e1a3c3c"), "رحّل الرصيد من خياره." },
                    { new Guid("7c0ca056-d10c-cd89-7991-61b1d9ddea13"), "en", new Guid("0e5dbece-685e-ddaf-7346-ab720f223e6d"), "Settlement and exit" },
                    { new Guid("7c235ba1-1c26-19c7-2e4f-8810321b77b7"), "en", new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "Reconciliation difference" },
                    { new Guid("7e4e2ae5-06ef-6405-4006-e9372858893b"), "en", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "To account" },
                    { new Guid("7e515384-dacb-02bd-1d0f-f419f0caacb9"), "en", new Guid("9c7c9263-970f-5773-324e-24508ec84b64"), "Type an amount above zero, with at most two decimals." },
                    { new Guid("7ed71510-912d-aa33-4e50-cad2744fbbe0"), "ar", new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "المبلغ في الإيصال" },
                    { new Guid("7fefdbce-3238-63eb-de35-6573500c2c6b"), "ar", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "التاريخ" },
                    { new Guid("8190fb7b-9169-86a9-a6c6-50131f977630"), "ar", new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "صورة أو PDF للإيصال" },
                    { new Guid("826a7f52-5662-75b4-3b2d-7777fb431589"), "en", new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "Movements" },
                    { new Guid("851d14ad-683a-b566-849f-1c0bb3495a6e"), "en", new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "Receipt attached" },
                    { new Guid("8632bda2-9563-ad8a-3d6a-2a45faa77f41"), "ar", new Guid("d731c9fc-f78f-ef52-1c7c-422497baaec3"), "اختر الحساب." },
                    { new Guid("873a71aa-5b82-2a81-2cad-9844aa232bfa"), "ar", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "اختر حسابين مختلفين." },
                    { new Guid("87bfdb56-a7c5-b7ef-05b3-4484ed154ddf"), "ar", new Guid("0e5dbece-685e-ddaf-7346-ab720f223e6d"), "تصفية وانسحاب" },
                    { new Guid("898a76ff-8007-4504-53f7-e245883f1c73"), "en", new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "The amount on the receipt differs from this movement." },
                    { new Guid("89b6418a-3e01-256e-d8dc-ad8d0fe69f99"), "ar", new Guid("9c86d593-d4d2-fcb8-7dc2-bff92bb94747"), "اختر الشريك." },
                    { new Guid("8aa3d54a-3e63-2088-7d1a-36e5165f67b1"), "ar", new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "اكتب رصيد كل حساب." },
                    { new Guid("8e485542-fdb9-ffb2-62a8-034488d54cb9"), "ar", new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "مستحق للشركاء" },
                    { new Guid("900ba73a-0bf7-f3ab-58b7-b3dbdf7d0346"), "en", new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "Active" },
                    { new Guid("905a100a-a19a-d676-d359-f4925baf4cd8"), "ar", new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "مستحق له" },
                    { new Guid("9073838a-2f46-ad27-73cf-c6b0ae20d925"), "ar", new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "عكس" },
                    { new Guid("90d159c7-8552-ddb9-3351-32035bda46f8"), "en", new Guid("34f06359-9fdd-3694-1843-c4796e1a3c3c"), "Carry a balance from its own choice." },
                    { new Guid("90f43dca-e1ec-8d3c-8414-d54bb088b575"), "ar", new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "هذا النوع من الحسابات يأتي مع شريكه." },
                    { new Guid("91833eb7-b5e5-2ffd-a8f0-da882b176f87"), "ar", new Guid("131d5c36-f4df-4116-8a87-496f2b173c90"), "ذلك المشروع مغلق." },
                    { new Guid("91a310b2-092b-6c89-176c-eb345c0790dd"), "en", new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"), "Reverse" },
                    { new Guid("92db7af7-3317-c382-5f7d-45edc73e851f"), "ar", new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "اكتب كل رصيد رقماً بخانتين عشريتين كحد أقصى." },
                    { new Guid("92dbd782-4b36-fc8e-72ca-c99499fe348c"), "en", new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "Paid by" },
                    { new Guid("9425664c-693a-9603-e81b-0a461866916d"), "en", new Guid("b4a807aa-2305-9bca-e838-c8b5ff45f3ed"), "Account there" },
                    { new Guid("9599a4cf-e2cc-f5ff-9adc-c329b1b4ee65"), "ar", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "تعذر تنفيذ ذلك." },
                    { new Guid("963654cb-6b5c-7c54-c49d-f5f9c10e4b81"), "en", new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "This could not be done." },
                    { new Guid("973d2b22-768d-85e8-4ff3-24e43cce082f"), "en", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "Transfer between accounts" },
                    { new Guid("98a581ea-5c05-c066-de09-cea8bcf61958"), "en", new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "Owed to partners" },
                    { new Guid("98eb31d2-d9bc-7242-8d04-5ac4ada9f0e4"), "ar", new Guid("c768947e-2f79-d2e3-b55b-090ebe86ebf3"), "تعذر تنفيذ ذلك." },
                    { new Guid("9924ff9a-241a-bef7-bdd6-052d9b7b5cb9"), "ar", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "المبلغ" },
                    { new Guid("99a0b490-267d-c832-264e-98e50f051126"), "ar", new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "إجمالي رأس المال" },
                    { new Guid("9a336727-b6fe-d5b0-991a-145fadb70768"), "en", new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "Could not read the capital. Check the connection and try again." },
                    { new Guid("9be0b638-cbaa-f44e-c135-f37750e46dad"), "en", new Guid("fab455e4-8730-850d-072a-e3362d32dc40"), "Carry to a project" },
                    { new Guid("9e981665-3ac8-1a4e-7838-c3566161ffd4"), "ar", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "النسبة %" },
                    { new Guid("9f56048b-7076-42e9-b45e-773e748ed574"), "en", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "Choose two different accounts." },
                    { new Guid("9fc284d2-9d51-ce3b-12d8-5d05159c7014"), "ar", new Guid("58e1e1ec-57af-babd-352b-60948010e4ff"), "لا تدير الأموال في مشروع آخر." },
                    { new Guid("9ff9754f-f4e1-b3fe-f887-e2ae30fa7693"), "ar", new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "تعذرت قراءة رأس المال. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("a016f665-a43f-9bdf-66cc-e75b940df834"), "ar", new Guid("f3911e30-8e7e-446b-5cdc-5abc892256c6"), "التصفية تشمل كل رصيد الشريك." },
                    { new Guid("a3130911-3155-f557-2577-9b8a0c1a002d"), "en", new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "This partner withdrew." },
                    { new Guid("a587b83b-1d84-0db1-339e-8884b8f2dd82"), "en", new Guid("2947ee0e-8693-df6a-f16e-772993743f60"), "No connection to the server. Try again." },
                    { new Guid("a6a6383f-360f-8f06-a772-a88c1d293f82"), "ar", new Guid("fab455e4-8730-850d-072a-e3362d32dc40"), "ترحيل إلى مشروع" },
                    { new Guid("a7f89259-31b7-742f-d09f-4fb2854f1675"), "en", new Guid("d78a1d43-4602-993e-c0e4-c80249b0e2dd"), "The date is outside the project's period." },
                    { new Guid("a9ad7d28-fdca-635f-3d9f-f26a294ed364"), "en", new Guid("bfa1fa05-f26d-8943-7f56-8d4c078e2257"), "Latest transfers" },
                    { new Guid("aa2989ed-a339-0d4b-e9bd-5f39bfd1307d"), "en", new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "No movements yet." },
                    { new Guid("aa811d02-ece9-283e-27e7-73e6d67f1110"), "en", new Guid("dc95b2fe-3d7d-325d-3967-fa8823add2aa"), "The file was not found. Choose it again." },
                    { new Guid("aaaef9fb-7bc9-1a27-fe73-cffa18868e1d"), "en", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "Balance" },
                    { new Guid("ab42ff3d-54ab-b1e9-8e77-e26c369c58dd"), "ar", new Guid("c17501cc-c934-986e-b341-9af805118535"), "تعذرت قراءة التحويلات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("ac308006-c3ec-d1ee-34a7-ee41e5367cb0"), "en", new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "Last updated" },
                    { new Guid("acd533e1-525a-5937-827f-a364d9cd551c"), "ar", new Guid("33fd8cb1-4c21-0fe8-f918-3232696f1509"), "حركة" },
                    { new Guid("acf71d90-fd30-2fc7-42d9-d0590588bf22"), "en", new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "Owed back" },
                    { new Guid("acfaff0f-ac36-8f96-fa9e-2aaaac35fe13"), "en", new Guid("a98676c3-6be5-87ba-11e3-78ecb152e69b"), "A capital movement's bank receipt" },
                    { new Guid("af243e15-b620-b02e-987a-0d805fb6e46e"), "ar", new Guid("3d786fce-bf95-cba4-8c8b-3f82659f7b75"), "التاريخ خارج فترة المشروع." },
                    { new Guid("b00a0fed-698c-9c72-9181-486aca8bc155"), "en", new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "A picture or PDF of the receipt" },
                    { new Guid("b0f7b8d5-978c-bc00-135f-c8e18d3c6d5d"), "en", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "This goes with an entry in a project you cannot change." },
                    { new Guid("b488c2f2-6fc7-daf2-ab23-0fe7690d6e05"), "en", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "Amount" },
                    { new Guid("b7e01401-517e-e321-19ce-aec59ac3c1c3"), "ar", new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "الأرصدة الفعلية" },
                    { new Guid("b805840a-f2f0-1ab8-2378-f95582cccae6"), "en", new Guid("f3911e30-8e7e-446b-5cdc-5abc892256c6"), "A settlement takes all the partner holds." },
                    { new Guid("b8f0476b-4232-76d7-9823-ee7e84360d10"), "ar", new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "السبب طويل جداً." },
                    { new Guid("bbdee45f-ebf9-9cc2-954b-dd9cbbbff299"), "ar", new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "تحديث الأرصدة الفعلية" },
                    { new Guid("bf72b835-ae88-6b56-b8fb-18a69ef55559"), "ar", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "الوصف طويل جداً." },
                    { new Guid("c13317b9-4c83-6689-87a5-b3482bcd1fe0"), "en", new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "Type each balance as a number with at most two decimals." },
                    { new Guid("c148e72c-fe84-d06f-6bed-1e2a96249cf1"), "ar", new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "إيصال مرفق" },
                    { new Guid("c1e1c235-8459-4f41-ce44-d0cd181b02c4"), "en", new Guid("42b6a274-7add-5e52-5bed-4255134ef780"), "The description is too long." },
                    { new Guid("c2d75122-0484-0453-3314-154cc0b08f1f"), "ar", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "الرصيد" },
                    { new Guid("c4aa4bdf-d2b5-b2ca-f1a7-efe008e33552"), "en", new Guid("58e1e1ec-57af-babd-352b-60948010e4ff"), "You move money in no other project." },
                    { new Guid("c4c17013-5b81-a908-b7ad-3b9c525c22e0"), "en", new Guid("efb26472-c9f1-d09d-a446-ff4b9b4b6f11"), "The movement was saved." },
                    { new Guid("c511fa6f-7451-64e6-3eda-51a76fc8b563"), "ar", new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "اختر ما يترتب عليه." },
                    { new Guid("c5f1ac9c-fcfa-1f5e-789f-7eaea6fc1ee6"), "en", new Guid("eb90bb5d-5598-ad0e-2afd-235e61c818c0"), "You do not move money in that project." },
                    { new Guid("c74f6343-0490-cf1a-d3d9-7684f4e06813"), "ar", new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "إضافة مبلغ" },
                    { new Guid("c84862fd-64a9-df39-5f68-c36a13b3a42f"), "ar", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("c8ecc720-3aaf-cad3-ed60-ea5a5bb6c190"), "ar", new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "سحب مبلغ" },
                    { new Guid("c960eb09-a0e1-625b-9c21-e79b157c6f07"), "ar", new Guid("12dee883-46e2-1760-edae-1ae87739c466"), "ترحيل من مشروع" },
                    { new Guid("ca4b8363-fa30-36db-66c9-2c2f91126eb3"), "ar", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "تحويل" },
                    { new Guid("ca8a90fa-e652-27cf-eaf8-92f776185a07"), "ar", new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "تعذر تنفيذ ذلك." },
                    { new Guid("d29f07c7-34c0-cd1f-ea79-61cde3f238f3"), "en", new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "Withdrawal" },
                    { new Guid("d2f2d153-21a7-f53b-6c5e-ba95e8edb447"), "en", new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "Computed surplus (what the accounts should hold)" },
                    { new Guid("d31d47f2-f844-03a9-734f-3d521cd67db5"), "ar", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "اكتب مبلغاً أكبر من الصفر بخانتين عشريتين كحد أقصى." },
                    { new Guid("d3f316f8-3a18-12dd-b233-9eb443e6ecb8"), "en", new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "Required" },
                    { new Guid("d43eed7b-fbc5-9495-b344-9a70fc5eb7bb"), "en", new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "Paid in cash, with no receipt" },
                    { new Guid("d615c164-349f-8abb-3bf9-1816e56250e9"), "en", new Guid("df8ddbc3-91ab-4e33-46e0-c213e9972453"), "Transfer between accounts" },
                    { new Guid("d6d04633-6379-18fa-64c5-8d0a0fe4ce91"), "ar", new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "آخر تحديث" },
                    { new Guid("d850eb67-7433-6417-be41-71062508507e"), "en", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "No connection to the server. Try again." },
                    { new Guid("d894983e-cd89-4065-ffa2-24e8be2274e3"), "ar", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "التاريخ" },
                    { new Guid("d8a6acd7-8d35-9eb7-b772-48390b3cbdd9"), "ar", new Guid("df8ddbc3-91ab-4e33-46e0-c213e9972453"), "تحويل بين الحسابات" },
                    { new Guid("d9b4342e-66ce-7d2b-f139-fd9b65ad80d4"), "ar", new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "الفائض المحسوب (المفروض بالحساب)" },
                    { new Guid("de9d990b-9606-991b-27f4-4199a6db8e05"), "en", new Guid("7f8e752c-d0ca-016a-0c98-46a91831f82f"), "Capital movement" },
                    { new Guid("deec1ebb-84a0-6c1e-4e9c-7bf6a361097a"), "ar", new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "لم يعد أحد الحسابات في المشروع." },
                    { new Guid("e2a795fe-66f5-a3bf-a45a-6e443ddbc04f"), "ar", new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "إيصال التحويل البنكي" },
                    { new Guid("e37149b4-afd7-407d-d143-1dfaf55c5e6d"), "en", new Guid("d731c9fc-f78f-ef52-1c7c-422497baaec3"), "Choose the account." },
                    { new Guid("e51d665f-3d79-94f2-18d1-7ae7c443ecbc"), "en", new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "Total of actual balances" },
                    { new Guid("e5b75f53-41f2-178b-ef25-a52e7adb0936"), "en", new Guid("c0a8bdb1-a6c9-5390-95a4-fb49750ef056"), "The file was not found. Choose it again." },
                    { new Guid("e64dbe64-5c66-07e5-b91f-6434c70108b5"), "ar", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "الشريك" },
                    { new Guid("e71eea87-53cd-9cfd-ad88-009cd0a7a45a"), "ar", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "اختر الحسابين." },
                    { new Guid("e9b39b25-e0b4-5c6a-b958-a9e297e20c09"), "ar", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "المبلغ" },
                    { new Guid("e9bd7ccd-ee23-403d-3c5a-486560ecda73"), "ar", new Guid("953c2b0f-61d6-4868-deab-aedf50f3e206"), "عيني أو غيره" },
                    { new Guid("e9e9eb5b-7a7a-158d-14c8-5b9cddf9fd80"), "ar", new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "إلزامي" },
                    { new Guid("ea37b653-cff1-3ffc-cb79-b3f78e05d996"), "ar", new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "حفظ الأرصدة" },
                    { new Guid("ea3c1ee1-238d-8acc-a4c4-45749a6a5f30"), "en", new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "Actual balances" },
                    { new Guid("eca259fc-c4b5-87ce-1c0b-864dd60f2ad4"), "ar", new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "دفعه" },
                    { new Guid("edf51534-21f3-4c1f-7cd1-471283b44066"), "en", new Guid("c17501cc-c934-986e-b341-9af805118535"), "Could not read the transfers. Check the connection and try again." },
                    { new Guid("eee66bc5-8de0-c896-d146-3329eba9c822"), "en", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "Save the movement" },
                    { new Guid("f022bfd7-dee9-65fd-a3a6-e275b4f1b722"), "en", new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "The reason is too long." },
                    { new Guid("f33e4a47-dc77-7ca7-4d70-ca47915933fa"), "ar", new Guid("d98464da-fea3-ea77-833c-72f09c2519f4"), "هذا الملف مرفق بقيد آخر." },
                    { new Guid("f48606fe-2058-2ca8-f8d1-d98193977b09"), "ar", new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "انسحب هذا الشريك." },
                    { new Guid("f4b5ed49-c7f6-7739-fde7-4714ef49f5c6"), "en", new Guid("f337be3a-926a-6830-d1e4-51029f1c6751"), "Target project" },
                    { new Guid("f6a144d9-2f6c-16df-5756-2a3c8e816df3"), "ar", new Guid("44c7e9dc-42b6-87e5-da00-6a4c86f7c464"), "هذا الملف مرفق بقيد آخر." },
                    { new Guid("f6bef1aa-f1f7-fc19-c85f-8ecb287c9215"), "en", new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "Update actual balances" },
                    { new Guid("f7269c82-69c8-c0d5-a7d9-4b53d9147aab"), "en", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "Partner" },
                    { new Guid("f7295045-a04d-27e3-f29f-d4d4d2b6de83"), "en", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "Date" },
                    { new Guid("f9c7207b-d1a7-015e-f3b5-ba217e465eb3"), "ar", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "نوع الحركة" },
                    { new Guid("faf4fd23-3926-23c0-26a5-878a4a23517d"), "ar", new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "فرق المطابقة" },
                    { new Guid("fc50eeb5-a80f-20eb-413d-6259a1e1c88d"), "ar", new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "لم يدفعه شريك." },
                    { new Guid("fd2f2622-0a2e-44bb-3b1a-feadc263aec1"), "ar", new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "نشط" },
                    { new Guid("fd846041-794a-7018-17db-e9e25479bb90"), "ar", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "حفظ الحركة" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.CreateIndex(
                name: "IX_CapitalMovementDetails_PartnerId",
                table: "CapitalMovementDetails",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPockets_PartnerId",
                table: "PartnerPockets",
                column: "PartnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationBalance_AccountId",
                table: "ReconciliationBalance",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_ProjectId_At",
                table: "Reconciliations",
                columns: new[] { "ProjectId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_UserId",
                table: "Reconciliations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapitalMovementDetails");

            migrationBuilder.DropTable(
                name: "PartnerPockets");

            migrationBuilder.DropTable(
                name: "ReconciliationBalance");

            migrationBuilder.DropTable(
                name: "Reconciliations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c7f4a654-572d-95ea-4d74-7ac8c594fe39"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0302765a-207a-ef61-b080-edcafcd746be"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0358ca3a-b06f-1b0d-6774-63f4d87ed9bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03c52fe2-d77d-0c74-573c-cbfa80d68f68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("05da0418-2ef1-8e52-1fd3-ececb22e393c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07acd0ad-901f-43ff-a2c9-ee8e9480a02f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0849e532-c3d5-0a6f-ee5d-ff0ae1adb6b8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("088b4415-9e5f-98dc-cd84-933231642cd3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c01fce5-3d51-8aa0-2a5c-a50452f4252e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c771f3b-91ad-f26f-f404-f1d30227451d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e1d6f19-e18a-ba67-da68-7bf5ca0fb299"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ef4fa30-5d7f-43af-b1e2-1297a6914764"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f201428-1dbf-942f-846e-6bab11651c61"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f939def-ba49-d8b0-6cd9-c8374be39c6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("13fe814f-e97b-240a-1d07-2d2f58c3db74"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1649079c-ff19-3707-a4e7-6526e35265b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1704fe25-8eec-50ed-f814-655e4aafc8c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18c6b73a-3314-576e-e09a-3046a2426b66"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("19b78b19-b279-9cf5-640e-48fc50f7b0b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a1944db-a39a-8ece-3821-6ade63ff1f5e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a94905d-b7db-9426-6022-52ecf68252da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ee041aa-9dc1-8d41-3e06-a22ff68ebaf6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ff16692-1a30-8b9e-7760-46a40d30c6b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20ae5177-1128-f50d-dec6-edf0138c926a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("21ba1404-a7f0-25d2-d982-e500627dad9e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("22d24d9b-e183-8d43-ba45-667101bb358c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("23375d30-be30-7728-7613-49a2ef55fc47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24e3411b-a03b-2249-6666-030c5600255a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24fc50d0-951c-b5c1-b71c-743af5799106"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25ad6845-2996-1d4b-049a-adec75ec8f75"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28c424cc-02e9-834b-44a0-55936e13e836"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28c68a9b-b43c-e11b-a3db-4747ce7b04a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2924c1e9-3455-6329-eefc-c20015df9d83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a92db36-c3f4-4a8e-e087-56044b39adc2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b4ba849-7fe1-ff6b-964d-491b9e8c990c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2de57f71-b125-f830-cce1-d9acfe45a9ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e9a14ad-4748-d7ef-1723-cd3be17fdb01"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ed1793f-4d04-1d7c-3010-a9f805f79d32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2fe21422-63b6-1782-b29a-934f085e976a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32179368-0d1d-84b6-fcdd-d899caf3d8e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35888947-ef7f-4379-26f3-fd5103c78588"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35d6a48f-8a15-9d7b-02a7-90a5855f0fa3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36661599-e814-80b8-2bb6-dc1b66ee8caa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("382033e1-e8cd-efaf-116f-894462850a27"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3ba98db2-f9a0-2e22-cf41-cb29101fec5b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3df3c585-78d9-6f9d-c618-5e51af2f3075"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f565137-3037-3eea-e282-54342b0f3fc5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f7dd83a-8c5b-a11e-79e9-6980dcf22ea7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3fdeabd3-789a-f9da-6d6a-07c4249b8bb7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("415926f7-be77-7e60-32c6-b148d88126b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41f93c88-580e-726b-319e-227d53528a1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("434311bd-4a54-392b-6bcf-e6e48504337e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4377bf43-44e3-4564-cc1a-6d2706d71b4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4383a71c-b456-fe03-7978-70949c20f39d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("447a8b30-8528-f56c-4696-db39d6f11178"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("482337dd-a382-4307-5f05-0d97482cec16"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48531794-f5d1-7533-83d3-01e9f7139f52"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48ecb1c6-2d74-1a48-87a5-f5a39260e539"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a30ce9d-805d-235d-f7b8-44dd6fec981c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a61a8f1-9f82-5227-2223-ecff83997d51"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4aa98f96-75ac-2ae6-cec9-f1709d7133dc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c266476-3087-ee7b-69e8-3a18e9c47b0e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4cd7e391-c11a-644f-23f1-581e915808bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4cd9a712-c7ad-99f7-7061-b957811b6793"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4df36ad6-8f02-af10-ef8c-69d7e61a70ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f61f961-4af2-7954-983f-157807d88e85"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f8e9555-39a8-17dd-3fb1-2883764f6841"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("500e9ea9-5c04-5553-4a7e-f79933275c60"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50187024-acc6-c3e6-4494-304b03281c45"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50a061a0-b5b3-fb25-e183-e6311159a40b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50a837fd-6102-8de2-2be6-168dc9f95e86"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50d20cae-4408-fdd2-39a8-47d8014b6fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("536b42a4-341d-c61f-7387-65a9cef080b8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("541b0f09-c1b1-dcfd-db58-224a26d77bf0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("543184d3-ee5b-cf4f-a6f5-138fd410f649"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54c9ba2e-2749-c51e-50c0-ccd1c56f7ef0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5695bc1a-35f5-7512-673e-2f7cbb309647"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5834fc2a-cf04-d2c6-2af3-290eb5a4883a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f256c0e-9331-3556-0f84-da17cc43fc48"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f5f83b3-7d16-a2de-28ef-a9f4cd7e34f7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64ffce39-7460-8b8e-946c-49d4db39d129"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6740acb9-caf0-ce05-919f-a656798e7dba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("677320db-1297-f1da-6f17-906d6bc3a995"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("69ff256e-564e-2525-5a6c-451d7e6a5592"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6cd5258f-0d14-d780-9b60-861f76fc0262"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6d1a646c-a942-01a3-32a4-885b192f3d03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6fe229df-5870-3913-b2a4-0ad2dcb32b88"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("70ebae12-6786-5571-bf14-cc6b7c44343b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("713aef6d-b027-7b8d-478a-4924cb333e42"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("719faf28-945b-6645-53bb-1160f38e092d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("72a747e5-aa1e-46a0-df21-2ce3cb338e9b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("72abe461-ae43-cd25-dad1-4b187f9a3c03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("72e5cf78-de07-cce0-de7f-40b2df611cc1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("742e5634-c91e-e4b1-98b6-96be500f9b34"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76bc6f1c-7782-2775-c1f1-ffb5c3413e81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("793b44c1-0764-c1af-e669-20042eb036e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ad7896a-63e3-f7e6-8968-c3ed2b1004ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7c0ca056-d10c-cd89-7991-61b1d9ddea13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7c235ba1-1c26-19c7-2e4f-8810321b77b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7e4e2ae5-06ef-6405-4006-e9372858893b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7e515384-dacb-02bd-1d0f-f419f0caacb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ed71510-912d-aa33-4e50-cad2744fbbe0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7fefdbce-3238-63eb-de35-6573500c2c6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8190fb7b-9169-86a9-a6c6-50131f977630"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("826a7f52-5662-75b4-3b2d-7777fb431589"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("851d14ad-683a-b566-849f-1c0bb3495a6e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8632bda2-9563-ad8a-3d6a-2a45faa77f41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("873a71aa-5b82-2a81-2cad-9844aa232bfa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87bfdb56-a7c5-b7ef-05b3-4484ed154ddf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("898a76ff-8007-4504-53f7-e245883f1c73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89b6418a-3e01-256e-d8dc-ad8d0fe69f99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8aa3d54a-3e63-2088-7d1a-36e5165f67b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e485542-fdb9-ffb2-62a8-034488d54cb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("900ba73a-0bf7-f3ab-58b7-b3dbdf7d0346"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("905a100a-a19a-d676-d359-f4925baf4cd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9073838a-2f46-ad27-73cf-c6b0ae20d925"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90d159c7-8552-ddb9-3351-32035bda46f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90f43dca-e1ec-8d3c-8414-d54bb088b575"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91833eb7-b5e5-2ffd-a8f0-da882b176f87"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91a310b2-092b-6c89-176c-eb345c0790dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92db7af7-3317-c382-5f7d-45edc73e851f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92dbd782-4b36-fc8e-72ca-c99499fe348c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9425664c-693a-9603-e81b-0a461866916d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9599a4cf-e2cc-f5ff-9adc-c329b1b4ee65"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("963654cb-6b5c-7c54-c49d-f5f9c10e4b81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("973d2b22-768d-85e8-4ff3-24e43cce082f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98a581ea-5c05-c066-de09-cea8bcf61958"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98eb31d2-d9bc-7242-8d04-5ac4ada9f0e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9924ff9a-241a-bef7-bdd6-052d9b7b5cb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("99a0b490-267d-c832-264e-98e50f051126"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a336727-b6fe-d5b0-991a-145fadb70768"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9be0b638-cbaa-f44e-c135-f37750e46dad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e981665-3ac8-1a4e-7838-c3566161ffd4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f56048b-7076-42e9-b45e-773e748ed574"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fc284d2-9d51-ce3b-12d8-5d05159c7014"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ff9754f-f4e1-b3fe-f887-e2ae30fa7693"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a016f665-a43f-9bdf-66cc-e75b940df834"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a3130911-3155-f557-2577-9b8a0c1a002d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a587b83b-1d84-0db1-339e-8884b8f2dd82"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a6a6383f-360f-8f06-a772-a88c1d293f82"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7f89259-31b7-742f-d09f-4fb2854f1675"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a9ad7d28-fdca-635f-3d9f-f26a294ed364"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa2989ed-a339-0d4b-e9bd-5f39bfd1307d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa811d02-ece9-283e-27e7-73e6d67f1110"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aaaef9fb-7bc9-1a27-fe73-cffa18868e1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ab42ff3d-54ab-b1e9-8e77-e26c369c58dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac308006-c3ec-d1ee-34a7-ee41e5367cb0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acd533e1-525a-5937-827f-a364d9cd551c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acf71d90-fd30-2fc7-42d9-d0590588bf22"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acfaff0f-ac36-8f96-fa9e-2aaaac35fe13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af243e15-b620-b02e-987a-0d805fb6e46e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b00a0fed-698c-9c72-9181-486aca8bc155"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0f7b8d5-978c-bc00-135f-c8e18d3c6d5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b488c2f2-6fc7-daf2-ab23-0fe7690d6e05"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b7e01401-517e-e321-19ce-aec59ac3c1c3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b805840a-f2f0-1ab8-2378-f95582cccae6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8f0476b-4232-76d7-9823-ee7e84360d10"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bbdee45f-ebf9-9cc2-954b-dd9cbbbff299"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf72b835-ae88-6b56-b8fb-18a69ef55559"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c13317b9-4c83-6689-87a5-b3482bcd1fe0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c148e72c-fe84-d06f-6bed-1e2a96249cf1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1e1c235-8459-4f41-ce44-d0cd181b02c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2d75122-0484-0453-3314-154cc0b08f1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4aa4bdf-d2b5-b2ca-f1a7-efe008e33552"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4c17013-5b81-a908-b7ad-3b9c525c22e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c511fa6f-7451-64e6-3eda-51a76fc8b563"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5f1ac9c-fcfa-1f5e-789f-7eaea6fc1ee6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c74f6343-0490-cf1a-d3d9-7684f4e06813"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c84862fd-64a9-df39-5f68-c36a13b3a42f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8ecc720-3aaf-cad3-ed60-ea5a5bb6c190"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c960eb09-a0e1-625b-9c21-e79b157c6f07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca4b8363-fa30-36db-66c9-2c2f91126eb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca8a90fa-e652-27cf-eaf8-92f776185a07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d29f07c7-34c0-cd1f-ea79-61cde3f238f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d2f2d153-21a7-f53b-6c5e-ba95e8edb447"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d31d47f2-f844-03a9-734f-3d521cd67db5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d3f316f8-3a18-12dd-b233-9eb443e6ecb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d43eed7b-fbc5-9495-b344-9a70fc5eb7bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d615c164-349f-8abb-3bf9-1816e56250e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6d04633-6379-18fa-64c5-8d0a0fe4ce91"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d850eb67-7433-6417-be41-71062508507e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d894983e-cd89-4065-ffa2-24e8be2274e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8a6acd7-8d35-9eb7-b772-48390b3cbdd9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9b4342e-66ce-7d2b-f139-fd9b65ad80d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("de9d990b-9606-991b-27f4-4199a6db8e05"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("deec1ebb-84a0-6c1e-4e9c-7bf6a361097a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2a795fe-66f5-a3bf-a45a-6e443ddbc04f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e37149b4-afd7-407d-d143-1dfaf55c5e6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e51d665f-3d79-94f2-18d1-7ae7c443ecbc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5b75f53-41f2-178b-ef25-a52e7adb0936"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e64dbe64-5c66-07e5-b91f-6434c70108b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e71eea87-53cd-9cfd-ad88-009cd0a7a45a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9b39b25-e0b4-5c6a-b958-a9e297e20c09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9bd7ccd-ee23-403d-3c5a-486560ecda73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9e9eb5b-7a7a-158d-14c8-5b9cddf9fd80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea37b653-cff1-3ffc-cb79-b3f78e05d996"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea3c1ee1-238d-8acc-a4c4-45749a6a5f30"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eca259fc-c4b5-87ce-1c0b-864dd60f2ad4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("edf51534-21f3-4c1f-7cd1-471283b44066"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eee66bc5-8de0-c896-d146-3329eba9c822"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f022bfd7-dee9-65fd-a3a6-e275b4f1b722"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f33e4a47-dc77-7ca7-4d70-ca47915933fa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f48606fe-2058-2ca8-f8d1-d98193977b09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4b5ed49-c7f6-7739-fde7-4714ef49f5c6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6a144d9-2f6c-16df-5756-2a3c8e816df3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6bef1aa-f1f7-fc19-c85f-8ecb287c9215"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7269c82-69c8-c0d5-a7d9-4b53d9147aab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7295045-a04d-27e3-f29f-d4d4d2b6de83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9c7207b-d1a7-015e-f3b5-ba217e465eb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("faf4fd23-3926-23c0-26a5-878a4a23517d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc50eeb5-a80f-20eb-413d-6259a1e1c88d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd2f2622-0a2e-44bb-3b1a-feadc263aec1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd846041-794a-7018-17db-e9e25479bb90"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("08ac9790-1b59-784b-f7e4-95d2001ab440"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0a351e62-d993-89df-8883-2186f5541dd0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e5dbece-685e-ddaf-7346-ab720f223e6d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12dee883-46e2-1760-edae-1ae87739c466"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("131d5c36-f4df-4116-8a87-496f2b173c90"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1ad78c31-0366-8535-b690-d668cfead1df"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2947ee0e-8693-df6a-f16e-772993743f60"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33fd8cb1-4c21-0fe8-f918-3232696f1509"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("34f06359-9fdd-3694-1843-c4796e1a3c3c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d786fce-bf95-cba4-8c8b-3f82659f7b75"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e1c052e-8583-4a49-c195-97db89e57466"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("42b6a274-7add-5e52-5bed-4255134ef780"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("44c7e9dc-42b6-87e5-da00-6a4c86f7c464"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("58e1e1ec-57af-babd-352b-60948010e4ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("64c47648-79d3-d784-3faa-dcec68821de9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6868bf1a-7f58-abc5-e5f2-9423f3583c0b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6badde79-3784-70f8-5315-9ed699c74888"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("760df9e8-0e1c-d474-ffc9-cee2e2cd98c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f8e752c-d0ca-016a-0c98-46a91831f82f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("82f07c7e-807a-7e02-3000-29c512083948"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8b35c979-312d-449d-541a-49971dd1d280"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("953c2b0f-61d6-4868-deab-aedf50f3e206"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9c7c9263-970f-5773-324e-24508ec84b64"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9c86d593-d4d2-fcb8-7dc2-bff92bb94747"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9de629bb-c4a8-6179-e0d8-36584d32359e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a971dc40-ac12-dde4-adcb-622cb68b640f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a98676c3-6be5-87ba-11e3-78ecb152e69b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ae277cab-a814-238a-ccce-45a1d5483326"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4a807aa-2305-9bca-e838-c8b5ff45f3ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bfa1fa05-f26d-8943-7f56-8d4c078e2257"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c0a8bdb1-a6c9-5390-95a4-fb49750ef056"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c17501cc-c934-986e-b341-9af805118535"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c768947e-2f79-d2e3-b55b-090ebe86ebf3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d0e1db96-0500-364b-5ffc-8900ea1a3671"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d731c9fc-f78f-ef52-1c7c-422497baaec3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d78a1d43-4602-993e-c0e4-c80249b0e2dd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d98464da-fea3-ea77-833c-72f09c2519f4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dc95b2fe-3d7d-325d-3967-fa8823add2aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("df8ddbc3-91ab-4e33-46e0-c213e9972453"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eb90bb5d-5598-ad0e-2afd-235e61c818c0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ee41fc05-238a-7fa4-74ac-26f0d2eb2cd0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("efb26472-c9f1-d09d-a446-ff4b9b4b6f11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f273d941-0faa-8823-6abd-e6ccd5c738c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f337be3a-926a-6830-d1e4-51029f1c6751"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f3911e30-8e7e-446b-5cdc-5abc892256c6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fab455e4-8730-850d-072a-e3362d32dc40"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles', 'ApproveEntries')");
        }
    }
}
