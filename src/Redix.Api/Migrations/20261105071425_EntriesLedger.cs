using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntriesLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryAttachments",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryAttachments", x => new { x.EntryId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_EntryAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttachments_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubgroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankFees = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    WithoutInvoiceReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PartnerTreatment = table.Column<byte>(type: "tinyint", nullable: false),
                    CustomValues = table.Column<string>(type: "json", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Subgroups_SubgroupId",
                        column: x => x.SubgroupId,
                        principalTable: "Subgroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountBeforeDiscount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    AmountAfterDiscount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Vat = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    IsCreditNote = table.Column<bool>(type: "bit", nullable: false),
                    VatPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_Invoices_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("017f8267-5712-23a9-ab09-f3654a3f5802"), "RecordEntries", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") },
                    { new Guid("67937e5c-2761-3f8b-fe1a-397f2c237edb"), "RecordEntries", new Guid("8a930250-60d5-b402-464a-0545e0253004") }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "", "expenses.projectFinished" },
                    { new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "", "expenses.loadFailed" },
                    { new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "", "expenses.saveDraft" },
                    { new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "", "expenses.unknownPaymentMethod" },
                    { new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "", "expenses.beforeVat" },
                    { new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "", "expenses.subgroup" },
                    { new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "", "expenses.requiredColumnMissing" },
                    { new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "", "expenses.qrUnreadable" },
                    { new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "", "expenses.product" },
                    { new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "", "expenses.invalidPaymentAmount" },
                    { new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "", "expenses.qrSupplierMissing" },
                    { new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "", "expenses.refused" },
                    { new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "", "expenses.sent" },
                    { new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "", "expenses.afterDiscount" },
                    { new Guid("39326569-bfee-321b-18c2-de9542077919"), "", "expenses.includingVat" },
                    { new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "", "expenses.unknownUnit" },
                    { new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "", "expenses.unknownAttachment" },
                    { new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "", "expenses.send" },
                    { new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "", "expenses.notes" },
                    { new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "", "expenses.vat" },
                    { new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "", "expenses.paymentAmount" },
                    { new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "", "expenses.generalSubgroup" },
                    { new Guid("4b92ea62-b7c3-0d87-3e6a-69d81e6a0dab"), "", "expenses.unknownSubgroup" },
                    { new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "", "expenses.invalidQuantity" },
                    { new Guid("51abb597-0670-6810-42f8-ade90ed52984"), "", "expenses.invalidPaymentDate" },
                    { new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "", "expenses.notTaxable" },
                    { new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "", "expenses.discount" },
                    { new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "", "expenses.account" },
                    { new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "", "expenses.invalidInvoiceDate" },
                    { new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "", "expenses.invoiceNumber" },
                    { new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "", "expenses.paymentMethod" },
                    { new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "", "expenses.supplier" },
                    { new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "", "expenses.noConnection" },
                    { new Guid("627e20df-8451-b393-4dca-65e11f806396"), "", "expenses.item" },
                    { new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "", "expenses.saved" },
                    { new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "", "expenses.qrVatDiffers" },
                    { new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "", "expenses.invalidBankFees" },
                    { new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "", "expenses.invalidAmount" },
                    { new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "", "expenses.amountIncludingVat" },
                    { new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "", "expenses.invoiceDate" },
                    { new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "", "expenses.quantity" },
                    { new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "", "expenses.invalidInvoiceNumber" },
                    { new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "", "expenses.unknownSupplier" },
                    { new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "", "expenses.invalidDiscount" },
                    { new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "", "suppliers.mostUsed" },
                    { new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "", "expenses.paymentSource" },
                    { new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "", "expenses.invalidUnitPrice" },
                    { new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "", "expenses.invalidColumnValue" },
                    { new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "", "expenses.paymentDate" },
                    { new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "", "expenses.bankFees" },
                    { new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "", "expenses.noAccount" },
                    { new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "", "expenses.qrRead" },
                    { new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "", "expenses.required" },
                    { new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "", "expenses.invalidNotes" },
                    { new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "", "expenses.entryExists" },
                    { new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "", "expenses.total" },
                    { new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "", "expenses.invalidEntryId" },
                    { new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "", "expenses.invoiceRequired" },
                    { new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "", "expenses.unit" },
                    { new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "", "expenses.unitPrice" },
                    { new Guid("e02871f2-6721-3c8c-50c7-63840fb2d62d"), "", "expenses.itemNotOffered" },
                    { new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "", "expenses.qrAddSupplier" },
                    { new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "", "expenses.invalidProduct" },
                    { new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "", "expenses.amountBeforeVat" },
                    { new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "", "expenses.accountNotOffered" },
                    { new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "", "expenses.unknownColumn" },
                    { new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "", "expenses.paymentDiffers" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00d5981b-744a-50d5-842a-2246aed3a34c"), "en", new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "General" },
                    { new Guid("0196d2e3-c509-25d3-0f25-514282022409"), "ar", new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "أدخل مبلغاً أكبر من صفر بخانتين عشريتين كحد أقصى، أو سعر الوحدة والعدد." },
                    { new Guid("024fc536-6032-372a-0cc7-47527306116f"), "ar", new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "اختر الوحدة." },
                    { new Guid("08d2d678-d7ab-602a-e539-8777e2253396"), "ar", new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "الأكثر استخداماً" },
                    { new Guid("0b8aeded-f822-3f31-7828-f561301e7316"), "ar", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "الملاحظات طويلة جداً." },
                    { new Guid("0c993812-c58a-b23c-a1a1-32bdb1725ab0"), "en", new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "Amount before VAT" },
                    { new Guid("0fb0adac-ef68-a968-4e72-ac835fd3432c"), "ar", new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "قُرئ رمز QR وعُبّئت الحقول" },
                    { new Guid("1119f52f-d29e-839b-f71e-694ac237f060"), "en", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "Unit price (optional)" },
                    { new Guid("124d6e57-efac-9edb-915e-5b9de753a68a"), "ar", new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "هذه القيمة لا تناسب هذا الحقل." },
                    { new Guid("12dda6b9-b9b1-c919-bf06-920e193cd448"), "en", new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "No supplier in the register holds this VAT number." },
                    { new Guid("13a81f25-c935-caae-0d87-3c2744dcfd52"), "ar", new Guid("4b92ea62-b7c3-0d87-3e6a-69d81e6a0dab"), "اختر مجموعة فرعية من هذا البند." },
                    { new Guid("13d39e10-b67f-03c9-c8f6-3057f38b0af1"), "ar", new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "قبل الضريبة بعد الخصم" },
                    { new Guid("147de803-538b-c5fc-a985-5e2d35976d90"), "ar", new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("171ff7a2-3a7e-630d-b4be-8c83ed40e3e2"), "en", new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "Choose a payment method." },
                    { new Guid("1868e492-4f67-5a38-f1c5-94ba867c68c4"), "ar", new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "أدخل خصماً لا يتجاوز المبلغ، بخانتين عشريتين كحد أقصى." },
                    { new Guid("18b9eee0-307a-772a-3cdf-850d83fd3cf2"), "en", new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "The date must be within the project's period and not in the future." },
                    { new Guid("1e4b5bb0-edc6-c704-8c95-42f19d64fcb8"), "en", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "Product" },
                    { new Guid("2430dbfc-5f1b-45ef-8811-612833d7ecd5"), "ar", new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "أُرسل للاعتماد." },
                    { new Guid("2530fde2-55a4-8e4d-56d7-d6b3ba50b359"), "en", new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "Before VAT, after discount" },
                    { new Guid("27a8cd3e-9204-cd9a-7b8e-2f2515ad6651"), "en", new Guid("39326569-bfee-321b-18c2-de9542077919"), "Including VAT" },
                    { new Guid("2875094f-ba99-8b34-983c-94b96a237f23"), "ar", new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "الضريبة" },
                    { new Guid("28b8f94f-21b6-a554-a8da-6a5a742f7e6a"), "en", new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "Total" },
                    { new Guid("313acc71-6830-3e3a-b79b-d0ccfb384f6e"), "ar", new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "عام" },
                    { new Guid("315ed240-d0a6-17d4-861a-534545a30bf0"), "ar", new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "يجب أن يقع التاريخ ضمن فترة المشروع وألا يتجاوز اليوم." },
                    { new Guid("318b2e5d-a71b-bd65-783a-6a65f36a2080"), "en", new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "Sent for approval." },
                    { new Guid("33616a11-2b8d-d277-7512-8785320e996d"), "ar", new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "تعذّر حفظ المصروف." },
                    { new Guid("33da3f4e-3b46-9511-9b44-823e41d0a288"), "ar", new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "الدفع يختلف عن الفاتورة" },
                    { new Guid("3453af31-1f8b-4ba0-a77f-9db47b4522a9"), "ar", new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "هذه الفاتورة غير خاضعة للضريبة" },
                    { new Guid("3466bc9b-1e3e-fe1c-1b56-02a0189e625d"), "ar", new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "اختر حساباً يحق لك الدفع منه." },
                    { new Guid("347526c5-7a89-31e1-0ca3-fdc0b77a6961"), "en", new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "Choose an account you may pay from." },
                    { new Guid("36a5e80a-68be-3423-76e3-68c3ddb7489e"), "ar", new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "المبلغ شامل الضريبة" },
                    { new Guid("387021e9-5405-4797-4e87-3d3d33910e1d"), "en", new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "Invoice number" },
                    { new Guid("38ed7541-948c-8ec5-4256-6644057d5cef"), "en", new Guid("51abb597-0670-6810-42f8-ade90ed52984"), "The payment date must be within the project's period and not in the future." },
                    { new Guid("3bce480a-80a8-f0d3-de9b-db146228836c"), "ar", new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "الخصم" },
                    { new Guid("3f2be9a4-93e6-2c34-ede6-6e5391f9fd1c"), "ar", new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "هذا الملف ليس في هذا المشروع." },
                    { new Guid("41405262-df7c-3c8b-fdfb-e8e631c21de3"), "ar", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "اسم الصنف طويل جداً." },
                    { new Guid("427d3d93-145b-8785-a13b-7e328b3b3342"), "ar", new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "قبل الضريبة" },
                    { new Guid("42c8491e-c8dc-6c57-9a43-6afff6c3e625"), "ar", new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "تاريخ الدفع" },
                    { new Guid("468bdf08-324e-e5b1-02aa-834e4fad5745"), "en", new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "Payment source" },
                    { new Guid("46a2ed3a-2dc9-27fc-b5a2-4da491272e38"), "ar", new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "الحساب" },
                    { new Guid("4a7962a8-cf01-3fa1-ff0a-6c5039855a1c"), "ar", new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "المجموعة الفرعية" },
                    { new Guid("4bad9f12-4048-d065-7b0e-82673dee5da3"), "ar", new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "المبلغ قبل الضريبة" },
                    { new Guid("4f3c018f-d6b1-4858-9889-c94b96e83c35"), "en", new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "Before VAT" },
                    { new Guid("52177e46-4d0b-4890-8b73-665ac066089f"), "ar", new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "اختر طريقة الدفع." },
                    { new Guid("56c5b8da-3d46-862f-3f6e-050f0c9c26bc"), "ar", new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "إلزامي" },
                    { new Guid("56c9c8cc-c8c9-038d-11d8-5c045fcb0503"), "en", new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "Unit" },
                    { new Guid("58272d8e-6d4f-35b8-1384-a2997376c82d"), "ar", new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "أدخل رقم الفاتورة، حتى 50 حرفاً." },
                    { new Guid("5ab5e673-8ab1-52ac-c703-155216c8cc0d"), "en", new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "Payment method" },
                    { new Guid("5b38f0e3-42cb-c02e-3dff-d310dd14ccf9"), "en", new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "Save draft" },
                    { new Guid("603072cd-6548-61dc-14b3-cd1da13e7e1a"), "ar", new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "طريقة الدفع" },
                    { new Guid("6168034f-99eb-5f12-333a-1a9cc2057e84"), "en", new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "Enter bank fees of zero or more, with at most two decimals." },
                    { new Guid("6464325f-be76-420a-abb5-13da941abd1d"), "en", new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "Discount" },
                    { new Guid("66abbd85-3ef4-f41c-b0b6-50d3118dff0c"), "en", new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "Fill the required field." },
                    { new Guid("6c78f16a-120e-7f9c-9061-c96cb5af2029"), "en", new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "Account" },
                    { new Guid("6d79cb8d-b572-f652-f60d-9655ba9cc976"), "en", new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "Saved as a draft." },
                    { new Guid("6eed0024-7660-1862-1b0b-04620a0a1c8a"), "ar", new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "أدخل المبلغ المدفوع، بخانتين عشريتين كحد أقصى." },
                    { new Guid("6efc9471-fe5d-676a-95e4-1a65063ad0eb"), "ar", new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "الوحدة" },
                    { new Guid("70e7a079-1e69-1216-7a77-5dda1d81908a"), "en", new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "That file is not in this project." },
                    { new Guid("7183af42-9714-2db6-7275-93695c65dfaa"), "en", new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "This item no longer asks for that field. Open the expense again." },
                    { new Guid("71ac38db-1766-433d-2876-6a28d610b81f"), "en", new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "Subgroup" },
                    { new Guid("7362e7d5-1e75-db2f-a9c5-aeb170f5aba9"), "ar", new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "رسوم بنكية" },
                    { new Guid("7438bcb4-a721-9b62-e298-638f67ed13a8"), "en", new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "Send for approval" },
                    { new Guid("75f18932-a0ef-fc8b-fb07-47939cf3f08c"), "en", new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "You hold no account to pay from. Ask Finance to hand you one." },
                    { new Guid("7755f159-824e-6727-668c-be21e71aa518"), "en", new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "Supplier" },
                    { new Guid("78216797-7f73-756c-b9d5-1d9957a172b0"), "en", new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "This project is finished. Nothing in it can change." },
                    { new Guid("78a0e7e2-d5a5-a1d1-44e7-baba704c660a"), "ar", new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "حُفظ كمسودة." },
                    { new Guid("79702266-80a0-d5ba-69ce-711592ddd3df"), "en", new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "Amount including VAT" },
                    { new Guid("7bce868e-859a-9ae7-0fc8-5f0581f8e8a7"), "en", new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "Enter a discount no larger than the amount, with at most two decimals." },
                    { new Guid("7daefe63-0ef4-9919-195b-061255c4093c"), "ar", new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "الإجمالي" },
                    { new Guid("82fb32a3-d7b6-6047-2289-8461dee7459d"), "en", new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "Enter the amount paid, with at most two decimals." },
                    { new Guid("864be42c-280f-d6f1-123c-a95722be794f"), "ar", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "سعر الوحدة (اختياري)" },
                    { new Guid("87e5906d-abb8-d36f-3ed2-b81c5eea3202"), "ar", new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "لا تملك حساباً تدفع منه. اطلب من المالي تسليمك حساباً." },
                    { new Guid("881d39af-4af9-584b-f8fe-5bed589c9968"), "en", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "Enter a quantity of zero or more, with at most two decimals." },
                    { new Guid("88338f31-66dd-4b57-3553-2a3f42403585"), "ar", new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "هذا المشروع منتهٍ، ولا يمكن تغيير شيء فيه." },
                    { new Guid("8a24b383-6e39-03d6-94eb-d0c37f7f167d"), "ar", new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "تعذّرت قراءة رمز QR. عبّئ الحقول يدوياً." },
                    { new Guid("8a79de21-2d68-ea09-dab8-7587dc173831"), "ar", new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "المبلغ المدفوع" },
                    { new Guid("9011182f-5ced-e079-387c-6366fcea47dd"), "ar", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "أدخل عدداً صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("94ed2e62-29b3-a221-6dcd-b2f8b5afe7a4"), "ar", new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "عبّئ الحقل الإلزامي." },
                    { new Guid("95c3885a-caa6-6c2a-cb5d-4706238ea0c5"), "en", new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "Add the invoice's picture or PDF before sending." },
                    { new Guid("9628e6ed-ded8-d1df-83b2-b3f252b6d9ea"), "en", new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "Most used" },
                    { new Guid("97f65854-495a-9aa0-316b-e3eb8f7f1700"), "ar", new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "أدخل رسوماً بنكية صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("98301475-ea8c-23e8-8073-8e4ca59465f1"), "en", new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "VAT" },
                    { new Guid("98837731-b5d0-ec39-e37f-4a9c2ea124a6"), "en", new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "Required" },
                    { new Guid("98ad406e-6ed3-0b3e-08eb-9c54d2b906da"), "en", new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "Choose a supplier from the register." },
                    { new Guid("9a0c3da3-69d6-dfeb-d337-0933ec362005"), "ar", new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "إضافة هذا المورد" },
                    { new Guid("9c7e6d82-bae9-ec48-9970-49b1322fdf63"), "ar", new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "اختر مورداً من القائمة." },
                    { new Guid("9d99d97b-ed59-b769-49e7-841d8a3ba7f4"), "ar", new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "تعذّر حفظ المصروف." },
                    { new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"), "en", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "Quantity (optional)" },
                    { new Guid("9f6c3b52-21fc-6462-aaff-8753a461ac25"), "ar", new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "حفظ مسودة" },
                    { new Guid("9f7742bd-2445-1eed-acf2-19e9bbbeb8da"), "en", new Guid("e02871f2-6721-3c8c-50c7-63840fb2d62d"), "Choose an item that takes purchases." },
                    { new Guid("a11904de-fb13-e9ca-5108-deaa84092b2d"), "en", new Guid("4b92ea62-b7c3-0d87-3e6a-69d81e6a0dab"), "Choose a subgroup of this item." },
                    { new Guid("a32a43bd-b85b-c6e3-4c54-f75b314bfe3b"), "ar", new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "المورد" },
                    { new Guid("a48c5b1a-c866-f75e-f4ba-a729b8bbce68"), "en", new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "This invoice is not taxable" },
                    { new Guid("a831aa9c-0c16-8050-a4cd-ffcf2f830f0b"), "en", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "Item" },
                    { new Guid("ac478550-e87f-db96-6a74-cda8f49eccbd"), "en", new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "Bank fees" },
                    { new Guid("afd60553-d9e0-d075-a45d-bc2746653e11"), "en", new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "No connection to the server. Try again." },
                    { new Guid("b332f743-46cc-e453-3d01-e294eb643509"), "en", new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "This expense is already saved." },
                    { new Guid("ba4d6398-6834-732c-e75a-760262abe5ea"), "ar", new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "هذا المصروف محفوظ مسبقاً." },
                    { new Guid("bc859c76-e6b0-a1c0-840c-a360e6953c7a"), "ar", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "ملاحظات" },
                    { new Guid("bd47f371-748c-d182-1828-5e2323a2128d"), "ar", new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "رقم الفاتورة" },
                    { new Guid("be50484a-72ad-01a9-81b5-bfb48f7cef07"), "en", new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "The VAT on the invoice differs from the computed VAT by more than 0.01. You can still save." },
                    { new Guid("bf309a0e-b409-2873-fd9b-ee5bdd2006bd"), "ar", new Guid("39326569-bfee-321b-18c2-de9542077919"), "شامل الضريبة" },
                    { new Guid("c0a8abcf-db11-8db8-ce81-50a11250d401"), "en", new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "Date" },
                    { new Guid("c118522f-5873-dd12-6e69-9dd875544b89"), "en", new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "The payment differs from the invoice" },
                    { new Guid("c14c3c92-b93e-3325-805e-3b69ea979ca4"), "en", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "Notes" },
                    { new Guid("c1e4bcdf-26c8-401a-d875-07309a8e32a4"), "ar", new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "إرسال للاعتماد" },
                    { new Guid("c455c684-5893-66b9-5eb0-f47858bee224"), "ar", new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "التاريخ" },
                    { new Guid("c595b861-7d34-9c84-97f6-41948f51bd1f"), "en", new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "QR code read and fields filled" },
                    { new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"), "ar", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "تعذّرت القراءة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("c946555a-b3a9-5762-e807-bca9521cc28b"), "ar", new Guid("e02871f2-6721-3c8c-50c7-63840fb2d62d"), "اختر بنداً يقبل المشتريات." },
                    { new Guid("c9628631-0fec-75a3-8e06-74305a66a871"), "ar", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "البند" },
                    { new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"), "ar", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "العدد (اختياري)" },
                    { new Guid("cab370f6-b4cc-4094-42cb-9196f7e26070"), "en", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "Enter a unit price of zero or more, with at most two decimals." },
                    { new Guid("cb3e0bec-d780-3c81-2b66-b608dd235e6f"), "ar", new Guid("51abb597-0670-6810-42f8-ade90ed52984"), "يجب أن يقع تاريخ الدفع ضمن فترة المشروع وألا يتجاوز اليوم." },
                    { new Guid("d3fa33a9-cbd0-39f6-88d5-d5d6f3af2d51"), "en", new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "Enter an amount above zero with at most two decimals, or a unit price and a quantity." },
                    { new Guid("d5ca4622-0846-9b0f-90b6-005f52f01989"), "ar", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "الصنف" },
                    { new Guid("d86deaf5-bfa1-6019-dfae-bd572c4844a9"), "en", new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "The expense could not be saved." },
                    { new Guid("d8908226-2e4b-3e46-01ad-6660928b3556"), "ar", new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "مصدر الدفع" },
                    { new Guid("d8a3fc6d-f1ff-e81e-20e4-cee141fd6706"), "ar", new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "أضف صورة الفاتورة أو ملف PDF قبل الإرسال." },
                    { new Guid("d96f8ded-1abb-c561-23c3-f6ae7a577b0f"), "en", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "The product name is too long." },
                    { new Guid("d994c2f3-f8af-b180-fb12-1a40fa8d64e9"), "en", new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "Payment date" },
                    { new Guid("da0048f7-95c7-bf7b-03f7-d334d3704f0b"), "en", new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "Enter the invoice number, up to 50 characters." },
                    { new Guid("db5dcb9f-e254-f1c9-c2f0-f539c6203ba9"), "en", new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "That value does not fit this field." },
                    { new Guid("dcd2ad27-d516-05c6-dcb9-b813fe8ba62d"), "en", new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "The expense could not be saved." },
                    { new Guid("df49099d-4369-3ac2-87af-0ac47c1b4b8f"), "ar", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "أدخل سعر وحدة صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("e4f1ff32-b9ad-4487-35af-8e5389d38792"), "ar", new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "لم يعد هذا البند يطلب هذا الحقل. افتح المصروف من جديد." },
                    { new Guid("ec804f07-bd20-8d37-f843-e20c7eb2d63a"), "ar", new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "لا يوجد في القائمة مورد بهذا الرقم الضريبي." },
                    { new Guid("ecb26879-92f7-96d4-dfb1-981c14462a18"), "en", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "The notes are too long." },
                    { new Guid("f0927f44-a92f-8efd-8beb-77c7cf7b66ee"), "en", new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "The QR code could not be read. Fill the fields yourself." },
                    { new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"), "en", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "Could not read this. Check the network and try again." },
                    { new Guid("f5000d02-b4c6-1fde-1f9a-b17edf5da1a2"), "en", new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "Amount paid" },
                    { new Guid("f55728f6-77e7-00eb-ae1c-b3cff8380629"), "en", new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "Choose a unit." },
                    { new Guid("f9e5116e-d37e-cff4-e334-dc93bf7e9214"), "en", new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "Add this supplier" },
                    { new Guid("fcba39e4-fade-72f3-405c-948e93b63078"), "ar", new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "ضريبة الفاتورة تختلف عن الضريبة المحسوبة بأكثر من 0.01. يمكنك الحفظ رغم ذلك." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'AttachFiles')");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_AccountId",
                table: "Entries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CreatedByUserId",
                table: "Entries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttachments_AttachmentId",
                table: "EntryAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_ItemId",
                table: "ExpenseDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_PaymentMethodId",
                table: "ExpenseDetails",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_SubgroupId",
                table: "ExpenseDetails",
                column: "SubgroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_UnitId",
                table: "ExpenseDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_SupplierId",
                table: "PurchaseDetails",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryAttachments");

            migrationBuilder.DropTable(
                name: "ExpenseDetails");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PurchaseDetails");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("017f8267-5712-23a9-ab09-f3654a3f5802"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("67937e5c-2761-3f8b-fe1a-397f2c237edb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00d5981b-744a-50d5-842a-2246aed3a34c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0196d2e3-c509-25d3-0f25-514282022409"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("024fc536-6032-372a-0cc7-47527306116f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08d2d678-d7ab-602a-e539-8777e2253396"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b8aeded-f822-3f31-7828-f561301e7316"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c993812-c58a-b23c-a1a1-32bdb1725ab0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fb0adac-ef68-a968-4e72-ac835fd3432c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1119f52f-d29e-839b-f71e-694ac237f060"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("124d6e57-efac-9edb-915e-5b9de753a68a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12dda6b9-b9b1-c919-bf06-920e193cd448"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("13a81f25-c935-caae-0d87-3c2744dcfd52"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("13d39e10-b67f-03c9-c8f6-3057f38b0af1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("147de803-538b-c5fc-a985-5e2d35976d90"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("171ff7a2-3a7e-630d-b4be-8c83ed40e3e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1868e492-4f67-5a38-f1c5-94ba867c68c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18b9eee0-307a-772a-3cdf-850d83fd3cf2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1e4b5bb0-edc6-c704-8c95-42f19d64fcb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2430dbfc-5f1b-45ef-8811-612833d7ecd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2530fde2-55a4-8e4d-56d7-d6b3ba50b359"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27a8cd3e-9204-cd9a-7b8e-2f2515ad6651"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2875094f-ba99-8b34-983c-94b96a237f23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28b8f94f-21b6-a554-a8da-6a5a742f7e6a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("313acc71-6830-3e3a-b79b-d0ccfb384f6e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("315ed240-d0a6-17d4-861a-534545a30bf0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("318b2e5d-a71b-bd65-783a-6a65f36a2080"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("33616a11-2b8d-d277-7512-8785320e996d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("33da3f4e-3b46-9511-9b44-823e41d0a288"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3453af31-1f8b-4ba0-a77f-9db47b4522a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3466bc9b-1e3e-fe1c-1b56-02a0189e625d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("347526c5-7a89-31e1-0ca3-fdc0b77a6961"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36a5e80a-68be-3423-76e3-68c3ddb7489e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("387021e9-5405-4797-4e87-3d3d33910e1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38ed7541-948c-8ec5-4256-6644057d5cef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3bce480a-80a8-f0d3-de9b-db146228836c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f2be9a4-93e6-2c34-ede6-6e5391f9fd1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41405262-df7c-3c8b-fdfb-e8e631c21de3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("427d3d93-145b-8785-a13b-7e328b3b3342"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42c8491e-c8dc-6c57-9a43-6afff6c3e625"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("468bdf08-324e-e5b1-02aa-834e4fad5745"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46a2ed3a-2dc9-27fc-b5a2-4da491272e38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a7962a8-cf01-3fa1-ff0a-6c5039855a1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bad9f12-4048-d065-7b0e-82673dee5da3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f3c018f-d6b1-4858-9889-c94b96e83c35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52177e46-4d0b-4890-8b73-665ac066089f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56c5b8da-3d46-862f-3f6e-050f0c9c26bc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56c9c8cc-c8c9-038d-11d8-5c045fcb0503"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("58272d8e-6d4f-35b8-1384-a2997376c82d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ab5e673-8ab1-52ac-c703-155216c8cc0d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b38f0e3-42cb-c02e-3dff-d310dd14ccf9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("603072cd-6548-61dc-14b3-cd1da13e7e1a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6168034f-99eb-5f12-333a-1a9cc2057e84"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6464325f-be76-420a-abb5-13da941abd1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("66abbd85-3ef4-f41c-b0b6-50d3118dff0c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6c78f16a-120e-7f9c-9061-c96cb5af2029"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6d79cb8d-b572-f652-f60d-9655ba9cc976"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6eed0024-7660-1862-1b0b-04620a0a1c8a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6efc9471-fe5d-676a-95e4-1a65063ad0eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("70e7a079-1e69-1216-7a77-5dda1d81908a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7183af42-9714-2db6-7275-93695c65dfaa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71ac38db-1766-433d-2876-6a28d610b81f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7362e7d5-1e75-db2f-a9c5-aeb170f5aba9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7438bcb4-a721-9b62-e298-638f67ed13a8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("75f18932-a0ef-fc8b-fb07-47939cf3f08c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7755f159-824e-6727-668c-be21e71aa518"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("78216797-7f73-756c-b9d5-1d9957a172b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("78a0e7e2-d5a5-a1d1-44e7-baba704c660a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("79702266-80a0-d5ba-69ce-711592ddd3df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7bce868e-859a-9ae7-0fc8-5f0581f8e8a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7daefe63-0ef4-9919-195b-061255c4093c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82fb32a3-d7b6-6047-2289-8461dee7459d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("864be42c-280f-d6f1-123c-a95722be794f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87e5906d-abb8-d36f-3ed2-b81c5eea3202"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("881d39af-4af9-584b-f8fe-5bed589c9968"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88338f31-66dd-4b57-3553-2a3f42403585"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a24b383-6e39-03d6-94eb-d0c37f7f167d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a79de21-2d68-ea09-dab8-7587dc173831"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9011182f-5ced-e079-387c-6366fcea47dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94ed2e62-29b3-a221-6dcd-b2f8b5afe7a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95c3885a-caa6-6c2a-cb5d-4706238ea0c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9628e6ed-ded8-d1df-83b2-b3f252b6d9ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97f65854-495a-9aa0-316b-e3eb8f7f1700"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98301475-ea8c-23e8-8073-8e4ca59465f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98837731-b5d0-ec39-e37f-4a9c2ea124a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98ad406e-6ed3-0b3e-08eb-9c54d2b906da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a0c3da3-69d6-dfeb-d337-0933ec362005"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c7e6d82-bae9-ec48-9970-49b1322fdf63"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d99d97b-ed59-b769-49e7-841d8a3ba7f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f6c3b52-21fc-6462-aaff-8753a461ac25"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f7742bd-2445-1eed-acf2-19e9bbbeb8da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a11904de-fb13-e9ca-5108-deaa84092b2d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a32a43bd-b85b-c6e3-4c54-f75b314bfe3b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a48c5b1a-c866-f75e-f4ba-a729b8bbce68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a831aa9c-0c16-8050-a4cd-ffcf2f830f0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac478550-e87f-db96-6a74-cda8f49eccbd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("afd60553-d9e0-d075-a45d-bc2746653e11"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b332f743-46cc-e453-3d01-e294eb643509"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba4d6398-6834-732c-e75a-760262abe5ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc859c76-e6b0-a1c0-840c-a360e6953c7a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd47f371-748c-d182-1828-5e2323a2128d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be50484a-72ad-01a9-81b5-bfb48f7cef07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf309a0e-b409-2873-fd9b-ee5bdd2006bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0a8abcf-db11-8db8-ce81-50a11250d401"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c118522f-5873-dd12-6e69-9dd875544b89"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c14c3c92-b93e-3325-805e-3b69ea979ca4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1e4bcdf-26c8-401a-d875-07309a8e32a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c455c684-5893-66b9-5eb0-f47858bee224"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c595b861-7d34-9c84-97f6-41948f51bd1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c946555a-b3a9-5762-e807-bca9521cc28b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9628631-0fec-75a3-8e06-74305a66a871"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cab370f6-b4cc-4094-42cb-9196f7e26070"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb3e0bec-d780-3c81-2b66-b608dd235e6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d3fa33a9-cbd0-39f6-88d5-d5d6f3af2d51"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5ca4622-0846-9b0f-90b6-005f52f01989"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d86deaf5-bfa1-6019-dfae-bd572c4844a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8908226-2e4b-3e46-01ad-6660928b3556"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8a3fc6d-f1ff-e81e-20e4-cee141fd6706"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d96f8ded-1abb-c561-23c3-f6ae7a577b0f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d994c2f3-f8af-b180-fb12-1a40fa8d64e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("da0048f7-95c7-bf7b-03f7-d334d3704f0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db5dcb9f-e254-f1c9-c2f0-f539c6203ba9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dcd2ad27-d516-05c6-dcb9-b813fe8ba62d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df49099d-4369-3ac2-87af-0ac47c1b4b8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4f1ff32-b9ad-4487-35af-8e5389d38792"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ec804f07-bd20-8d37-f843-e20c7eb2d63a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ecb26879-92f7-96d4-dfb1-981c14462a18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0927f44-a92f-8efd-8beb-77c7cf7b66ee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f5000d02-b4c6-1fde-1f9a-b17edf5da1a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f55728f6-77e7-00eb-ae1c-b3cff8380629"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9e5116e-d37e-cff4-e334-dc93bf7e9214"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcba39e4-fade-72f3-405c-948e93b63078"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39326569-bfee-321b-18c2-de9542077919"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4b92ea62-b7c3-0d87-3e6a-69d81e6a0dab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("51abb597-0670-6810-42f8-ade90ed52984"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("536dbb67-b590-59b3-3de2-a0635356666a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("627e20df-8451-b393-4dca-65e11f806396"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e02871f2-6721-3c8c-50c7-63840fb2d62d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'AttachFiles')");
        }
    }
}
