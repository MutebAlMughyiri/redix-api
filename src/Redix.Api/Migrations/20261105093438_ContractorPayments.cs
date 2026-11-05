using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ContractorPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractorPaymentDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOutsideContract = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPaymentDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "", "contractorPayments.contract" },
                    { new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "", "contractorPayments.outsideExplain" },
                    { new Guid("37c64841-3318-0fcc-042d-03697eccd2be"), "", "expenses.unknownContract" },
                    { new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "", "contractorPayments.new" },
                    { new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "", "expenses.invalidInvoiceAmount" },
                    { new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "", "contractorPayments.deductionNeedsReason" },
                    { new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "", "contracts.pending" },
                    { new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "", "expenses.receiptRequired" },
                    { new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "", "contractorPayments.receipt" },
                    { new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "", "contractorPayments.date" },
                    { new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "", "contractorPayments.inside" },
                    { new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "", "contractorPayments.overrun" },
                    { new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "", "contractorPayments.hasInvoice" },
                    { new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "", "contractorPayments.deductionReason" },
                    { new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "", "contractorPayments.remainingAfter" },
                    { new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "", "contractorPayments.savedOverrun" },
                    { new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "", "contractorPayments.outside" },
                    { new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "", "contractorPayments.kind" },
                    { new Guid("e970f65e-3490-c978-4180-795440d833be"), "", "contractorPayments.invoiceExplain" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("088a6cbf-03c8-3144-17ef-71f61f5c0fcf"), "ar", new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "العقد" },
                    { new Guid("0a8817c6-9ecd-8cb0-c1c7-677aa8de819c"), "ar", new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "نوع الدفعة" },
                    { new Guid("0c4759ef-5824-4cec-a20f-8c54fbc5e149"), "ar", new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "اذكر سبب الخصم." },
                    { new Guid("112551b6-8585-fdc1-faeb-11431178178a"), "ar", new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "خارج العقد" },
                    { new Guid("1290264a-2564-28fb-8e04-7b4441b3e1db"), "en", new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "Add the transfer receipt or the receipt voucher before sending." },
                    { new Guid("19a24d30-08c4-a6c0-a2ce-41e7f4e0de18"), "ar", new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "حُفظت، والعقد متجاوَز." },
                    { new Guid("1a427fc1-75fe-0119-213c-c42f895e2cc4"), "ar", new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "للدفعة فاتورة ضريبية" },
                    { new Guid("1c709a19-039b-058f-2c81-4ed5138bcb90"), "en", new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "Outside the contract: it counts in the item's total, and does not come off the remaining." },
                    { new Guid("1eb22d52-0e48-69f4-d7a2-9f97ca41c3cb"), "ar", new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "أدخل مبلغ الفاتورة قبل الضريبة، أكبر من صفر وبخانتين عشريتين على الأكثر." },
                    { new Guid("31c261e4-1282-737d-25f2-357e045933d6"), "ar", new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "بانتظار الاعتماد" },
                    { new Guid("3444d843-0b82-f08c-f421-f027bf0cf64c"), "ar", new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "سند الاستلام أو إيصال التحويل (إلزامي)" },
                    { new Guid("3a09b055-42a9-3a82-585d-6bfcace2eca4"), "ar", new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "المتبقي بعد الدفعة" },
                    { new Guid("46fca222-75ab-9a64-f590-5d22e9323f40"), "en", new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "Remaining after this payment" },
                    { new Guid("4bf70892-9364-1c40-ed5d-63d8648f7d62"), "ar", new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "أضف إيصال التحويل أو سند الاستلام قبل الإرسال." },
                    { new Guid("4e9aeff7-9a8d-2e0b-c1b0-b0dd1ec83844"), "en", new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "Waiting for approval" },
                    { new Guid("5007f4c9-6b81-dd0d-1204-2aaba8bced79"), "en", new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "Saved, with the contract overrun." },
                    { new Guid("5d76814e-eef5-9a3b-5973-050a2ef7b76f"), "en", new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "Inside the contract" },
                    { new Guid("60e46676-b9a3-ffb7-61e6-e8855338bea0"), "ar", new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "خارج العقد: تدخل إجمالي البند ولا تُخصم من المتبقي." },
                    { new Guid("616bb83c-44a2-8482-f1b5-636d46e76b39"), "ar", new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "التاريخ" },
                    { new Guid("62a48d5e-9f21-2f32-bed4-30bbca2d8e16"), "en", new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "Receipt voucher or transfer receipt (required)" },
                    { new Guid("6b757b47-168d-097f-5021-899839234480"), "ar", new Guid("37c64841-3318-0fcc-042d-03697eccd2be"), "اختر عقداً." },
                    { new Guid("74760369-b920-3a2f-85d4-c16521ba7eca"), "en", new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "Why it is deducted" },
                    { new Guid("7a8c69fe-882b-9791-4643-5fb093bf03bd"), "en", new Guid("e970f65e-3490-c978-4180-795440d833be"), "It enters the VAT statement under the contractor's name." },
                    { new Guid("99f01651-9702-ffae-5a56-3fc7db751226"), "ar", new Guid("e970f65e-3490-c978-4180-795440d833be"), "تدخل كشف الضريبة باسم المقاول." },
                    { new Guid("9f81435b-711a-b969-5505-46d13786830e"), "en", new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "Contract" },
                    { new Guid("a2b7234c-1538-9015-0a75-63604bb8e72b"), "en", new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "Enter the invoice's amount before VAT, above zero with at most two decimals." },
                    { new Guid("b7600b23-02f6-da47-b67c-4ee9751568a2"), "en", new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "Date" },
                    { new Guid("bdb6b091-da55-9df8-aec1-87f72da3beb1"), "ar", new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "هذه الدفعة تتجاوز قيمة العقد. يمكنك الحفظ رغم ذلك." },
                    { new Guid("be95c09e-c195-465e-d12a-042ab44453f5"), "ar", new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "داخل العقد" },
                    { new Guid("c6e4f087-55f6-9813-1537-d3ca1ae41cb4"), "en", new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "This payment has a tax invoice" },
                    { new Guid("d493159f-4ba7-6691-202b-9939403bfc4c"), "ar", new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "سبب الخصم" },
                    { new Guid("e221131d-1d27-0c1c-9d54-757378cccd48"), "en", new Guid("37c64841-3318-0fcc-042d-03697eccd2be"), "Choose a contract." },
                    { new Guid("e5691cf3-c78a-28e8-dd12-e34c8fdda7fc"), "en", new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "Outside the contract" },
                    { new Guid("ef7d5929-ecd8-e00d-4e5b-aa1964e2f9d0"), "en", new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "Payment type" },
                    { new Guid("efa82456-f11c-30e9-f1b9-40dfe26a452b"), "en", new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "This payment goes beyond the contract's value. You can still save it." },
                    { new Guid("f0e0e21e-2b4b-24ec-e972-f4b74d91414c"), "en", new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "Contractor payment" },
                    { new Guid("f0ff4b3f-82ca-45a5-7573-21163e6e6e81"), "en", new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "Say why it is deducted." },
                    { new Guid("f2feb7c7-f067-5b72-acf8-d8b204cd6104"), "ar", new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "دفعة مقاول" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPaymentDetails_ContractId",
                table: "ContractorPaymentDetails",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractorPaymentDetails");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("088a6cbf-03c8-3144-17ef-71f61f5c0fcf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0a8817c6-9ecd-8cb0-c1c7-677aa8de819c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c4759ef-5824-4cec-a20f-8c54fbc5e149"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("112551b6-8585-fdc1-faeb-11431178178a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1290264a-2564-28fb-8e04-7b4441b3e1db"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("19a24d30-08c4-a6c0-a2ce-41e7f4e0de18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a427fc1-75fe-0119-213c-c42f895e2cc4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c709a19-039b-058f-2c81-4ed5138bcb90"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1eb22d52-0e48-69f4-d7a2-9f97ca41c3cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("31c261e4-1282-737d-25f2-357e045933d6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3444d843-0b82-f08c-f421-f027bf0cf64c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3a09b055-42a9-3a82-585d-6bfcace2eca4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46fca222-75ab-9a64-f590-5d22e9323f40"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bf70892-9364-1c40-ed5d-63d8648f7d62"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e9aeff7-9a8d-2e0b-c1b0-b0dd1ec83844"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5007f4c9-6b81-dd0d-1204-2aaba8bced79"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d76814e-eef5-9a3b-5973-050a2ef7b76f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60e46676-b9a3-ffb7-61e6-e8855338bea0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("616bb83c-44a2-8482-f1b5-636d46e76b39"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62a48d5e-9f21-2f32-bed4-30bbca2d8e16"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b757b47-168d-097f-5021-899839234480"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("74760369-b920-3a2f-85d4-c16521ba7eca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a8c69fe-882b-9791-4643-5fb093bf03bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("99f01651-9702-ffae-5a56-3fc7db751226"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f81435b-711a-b969-5505-46d13786830e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2b7234c-1538-9015-0a75-63604bb8e72b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b7600b23-02f6-da47-b67c-4ee9751568a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bdb6b091-da55-9df8-aec1-87f72da3beb1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be95c09e-c195-465e-d12a-042ab44453f5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6e4f087-55f6-9813-1537-d3ca1ae41cb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d493159f-4ba7-6691-202b-9939403bfc4c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e221131d-1d27-0c1c-9d54-757378cccd48"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5691cf3-c78a-28e8-dd12-e34c8fdda7fc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ef7d5929-ecd8-e00d-4e5b-aa1964e2f9d0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efa82456-f11c-30e9-f1b9-40dfe26a452b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0e0e21e-2b4b-24ec-e972-f4b74d91414c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0ff4b3f-82ca-45a5-7573-21163e6e6e81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2feb7c7-f067-5b72-acf8-d8b204cd6104"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37bce61a-7169-feb4-7b00-807d656de57c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37c64841-3318-0fcc-042d-03697eccd2be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52314600-de79-f3ab-38c6-5ded7893e819"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("59926246-1594-60fb-c542-7c8b332640fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99616917-0265-ac67-cc5f-f394388643ae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dddacedd-b79c-aad1-66ba-682a14134923"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e970f65e-3490-c978-4180-795440d833be"));
        }
    }
}
