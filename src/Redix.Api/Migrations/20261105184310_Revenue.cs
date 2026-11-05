using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Revenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevenueDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_RevenueDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoice",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuyerVatNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountBeforeDiscount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    AmountAfterDiscount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    TaxType = table.Column<byte>(type: "tinyint", nullable: false),
                    VatPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoice", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_SalesInvoice_Entries_EntryId",
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
                    { new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "", "vat.q4" },
                    { new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "", "revenue.taxType" },
                    { new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "", "revenue.invalidVatNumber" },
                    { new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "", "vat.declareLater" },
                    { new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "", "vat.refused" },
                    { new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "", "vat.withoutInvoice" },
                    { new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "", "vat.declaredLater" },
                    { new Guid("1c891c17-20fe-13a6-77ea-82f216f91ca6"), "", "revenue.unknownAttachment" },
                    { new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "", "vat.invalidPeriod" },
                    { new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "", "vat.loadFailed" },
                    { new Guid("3027597b-3d9d-3d2c-c9a3-bc8bef773bf5"), "", "revenue.account" },
                    { new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "", "revenue.taxVat" },
                    { new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "", "revenue.date" },
                    { new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "", "revenue.loadFailed" },
                    { new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "", "vat.title" },
                    { new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "", "revenue.statementRefund" },
                    { new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "", "revenue.quarter" },
                    { new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "", "revenue.buyer" },
                    { new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "", "vat.notTaxable" },
                    { new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "", "revenue.unknownSale" },
                    { new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "", "vat.afterDiscount" },
                    { new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "", "revenue.vatRefund" },
                    { new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "", "revenue.save" },
                    { new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "", "revenue.invalidDiscount" },
                    { new Guid("640ad877-bf82-1804-e1d6-69abbd7f98d7"), "", "revenue.invalidAmount" },
                    { new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "", "vat.netDue" },
                    { new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "", "vat.h1" },
                    { new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "", "vat.noFile" },
                    { new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "", "vat.halfYear" },
                    { new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "", "vat.purchasesVat" },
                    { new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "", "vat.q2" },
                    { new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "", "vat.q3" },
                    { new Guid("7e5b1a8f-008a-8f51-d0b0-61eac4a9ff71"), "", "revenue.new" },
                    { new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "", "vat.from" },
                    { new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "", "revenue.amountBeforeTax" },
                    { new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "", "vat.creditNotes" },
                    { new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "", "vat.salesVat" },
                    { new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "", "vat.to" },
                    { new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "", "revenue.refundDifference" },
                    { new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "", "revenue.invoiceNumber" },
                    { new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "", "revenue.invalidBuyerName" },
                    { new Guid("9986841e-810b-8962-d4e1-a388352f38e9"), "", "revenue.description" },
                    { new Guid("99f25918-f1e7-dde4-0f2f-cd67937c72da"), "", "revenue.invoiceAlreadyAttached" },
                    { new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "", "revenue.discount" },
                    { new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "", "vat.noVatNumber" },
                    { new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "", "vat.netRefund" },
                    { new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "", "vat.noConnection" },
                    { new Guid("b24cade7-d2d0-3e04-5df3-7ed583fe5446"), "", "revenue.refused" },
                    { new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "", "vat.exceptions" },
                    { new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "", "vat.h2" },
                    { new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "", "revenue.duplicateInvoice" },
                    { new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "", "revenue.amount" },
                    { new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "", "revenue.invoiceDate" },
                    { new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "", "revenue.invalidInvoiceDate" },
                    { new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "", "revenue.type" },
                    { new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "", "vat.realEstateTax" },
                    { new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "", "vat.invalidVatNumber" },
                    { new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "", "revenue.saved" },
                    { new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "", "revenue.invalidInvoiceNumber" },
                    { new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "", "revenue.taxRealEstateTransaction" },
                    { new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "", "vat.custom" },
                    { new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "", "revenue.title" },
                    { new Guid("dfcb343d-720c-3164-c03d-5549447750fe"), "", "revenue.noConnection" },
                    { new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "", "revenue.buyerVatNumber" },
                    { new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "", "vat.total" },
                    { new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "", "revenue.other" },
                    { new Guid("e85316d8-05a6-2c9b-fff1-74ae9049a30b"), "", "revenue.invalidDate" },
                    { new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "", "revenue.invalidPeriod" },
                    { new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "", "vat.q1" },
                    { new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "", "vat.noExceptions" },
                    { new Guid("ef7dc8b6-066b-9c91-2370-29beab7ba230"), "", "revenue.invalidNotes" },
                    { new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "", "revenue.file" },
                    { new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "", "revenue.sale" },
                    { new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "", "vat.pending" },
                    { new Guid("fbe18f17-7aca-e801-0031-793076552587"), "", "vat.quarter" },
                    { new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "", "revenue.newSale" },
                    { new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "", "vat.purchaseInvoices" },
                    { new Guid("ffb92055-46ed-1867-1094-cd3c1b0e529e"), "", "revenue.unknownAccount" },
                    { new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "", "revenue.empty" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0036f2ac-e636-e1a9-99a6-ebdeb1224b44"), "en", new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "VAT number not valid" },
                    { new Guid("00d248c7-ea8b-ed71-2f7f-ce322c9f1258"), "ar", new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "المبلغ قبل الضريبة" },
                    { new Guid("01cf41f7-ed5b-f1cf-8b2a-1d051bf64085"), "ar", new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "ضريبة المشتريات" },
                    { new Guid("0407605a-c96a-62d3-bc2f-593ad9a0ef00"), "en", new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "credit notes" },
                    { new Guid("0458e8e5-5916-79e7-4194-18a3093798a7"), "ar", new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "اكتب اسم المشتري." },
                    { new Guid("047ceb71-19ce-51c4-2011-d8f1a8e9d5f0"), "en", new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "That sale was not found, or has an invoice." },
                    { new Guid("06234c06-75c3-633d-2e9f-3ea5aaf4ad95"), "ar", new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "بعد الخصم" },
                    { new Guid("063c36a7-a52c-8f99-e157-75c6014b7495"), "en", new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "Net due" },
                    { new Guid("08df4a9a-d031-634c-94ff-b80d94fde8ac"), "en", new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "Declare in the next quarter" },
                    { new Guid("09510e0e-2ea5-f556-e437-b3127d6aba74"), "en", new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "Second" },
                    { new Guid("09673683-2250-fca5-7d94-213d082127f4"), "en", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "Date" },
                    { new Guid("0aa0b73c-9411-1cc4-2af9-282797c2984a"), "ar", new Guid("640ad877-bf82-1804-e1d6-69abbd7f98d7"), "اكتب مبلغاً أكبر من الصفر بخانتين عشريتين كحد أقصى." },
                    { new Guid("0c2d5090-4780-be30-87c9-a319275c554b"), "ar", new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "النصف الأول" },
                    { new Guid("0c9043cf-263c-c229-2ef6-4cce26151b41"), "ar", new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "إلى" },
                    { new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"), "ar", new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "الرقم الضريبي 15 خانة، يبدأ وينتهي بالرقم 3." },
                    { new Guid("118eb76b-dab0-e7fc-1f7f-80a7101abad2"), "en", new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "Sales invoice" },
                    { new Guid("11f67e3b-fa67-94b7-6fe8-891c5a269d8b"), "ar", new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "رقم الفاتورة مستخدم." },
                    { new Guid("1237f62d-0322-4fec-2ca6-2eb54316df70"), "en", new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "Fourth" },
                    { new Guid("13156b78-e9ec-28e7-9d92-6b9fe768c8ed"), "ar", new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "الرابع" },
                    { new Guid("1a9a486a-c230-7f33-d1a8-2c97e69b1604"), "ar", new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "اكتب رقم الفاتورة." },
                    { new Guid("1b34439b-bc6b-f639-14dd-60b8309bf48e"), "en", new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "No invoice" },
                    { new Guid("1f980580-b6c5-9051-bd64-d30c396deca1"), "ar", new Guid("e85316d8-05a6-2c9b-fff1-74ae9049a30b"), "التاريخ خارج فترة المشروع." },
                    { new Guid("202a2d50-cdcb-cd4d-086e-f1ce3d63fc95"), "en", new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "The period ends before it starts." },
                    { new Guid("219fb21a-c044-a9a1-4399-ec9f1a973530"), "en", new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "Quarter" },
                    { new Guid("2a054c59-58c5-1358-cf5b-aed2093d13a8"), "en", new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "This could not be done." },
                    { new Guid("2acfa645-34f0-8e44-617f-f0d43aca161f"), "en", new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "After discount" },
                    { new Guid("2b4c55af-b6cc-a3c0-2db2-04175d90bdb2"), "ar", new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "لا استثناءات." },
                    { new Guid("2bd3db63-bb80-8b0c-0632-7c6948235300"), "en", new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "Custom period" },
                    { new Guid("2bff715c-9281-4599-0e61-058d2d452510"), "en", new Guid("1c891c17-20fe-13a6-77ea-82f216f91ca6"), "The file was not found. Choose it again." },
                    { new Guid("2d68c01d-fabb-2296-f41a-3a3e3b828b21"), "en", new Guid("dfcb343d-720c-3164-c03d-5549447750fe"), "No connection to the server. Try again." },
                    { new Guid("2e094557-6d7e-b3d9-da37-fdc7da3de402"), "ar", new Guid("ef7dc8b6-066b-9c91-2370-29beab7ba230"), "الوصف طويل جداً." },
                    { new Guid("319b04ff-9576-d428-0f5c-cd53a14b659a"), "ar", new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "الربع" },
                    { new Guid("341fa924-18e3-e734-2ca0-1953306bd34e"), "ar", new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "تعذرت قراءة الكشف. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("345e8887-df72-c02b-8fdd-693edd48bc7f"), "en", new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "Sales VAT" },
                    { new Guid("34fc25ae-a507-817d-5be0-31052a75c46b"), "en", new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "The invoice's date is outside the project's period." },
                    { new Guid("37dd53a5-8cb6-fa40-b5b2-47fc5b23838b"), "ar", new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "لا يتجاوز الخصم المبلغ." },
                    { new Guid("37f592a8-7898-35b8-5303-0d71a758d231"), "ar", new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "الفرق" },
                    { new Guid("38fb4888-c1a7-518e-cac4-c8ddfaeee5ea"), "ar", new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "الفرق (مسترد)" },
                    { new Guid("398ea309-4580-473b-3c60-cbce927e681b"), "ar", new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "بيع" },
                    { new Guid("3ab7caf6-b51d-590f-a6d9-b4f5dc29b4ad"), "ar", new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "صورة أو PDF" },
                    { new Guid("3b221698-6956-a455-caa1-118d8a67c756"), "ar", new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "حُفظ." },
                    { new Guid("3cbf9aa9-b363-af0d-1a0d-75315fd6558d"), "ar", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "الإيرادات" },
                    { new Guid("41473377-a79e-b35a-1f98-1820fe2e0c5d"), "ar", new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "لم يُعثر على ذلك البيع، أو له فاتورة." },
                    { new Guid("41f27a78-58c3-df90-d339-925afc1d55e6"), "ar", new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "استرجاع ضريبة" },
                    { new Guid("449b8219-4168-4400-8e51-fe12a63171da"), "en", new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "Invoice number" },
                    { new Guid("451805aa-edd0-dded-205c-8d0256534263"), "en", new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "Real estate transaction tax" },
                    { new Guid("45cd37a8-56af-fe3a-4d76-36ddece53523"), "ar", new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "فترة مخصصة" },
                    { new Guid("4605ecfe-b3d8-5df9-7683-6fc44f1d4271"), "en", new Guid("3027597b-3d9d-3d2c-c9a3-bc8bef773bf5"), "Account" },
                    { new Guid("46701a14-d71b-7f64-dc21-57cbf7b6709d"), "ar", new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "النوع" },
                    { new Guid("479ad873-fd5c-3870-a619-c821d697ae19"), "en", new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "Purchases VAT" },
                    { new Guid("4ba46a6e-b644-a163-270c-1e7188679b11"), "en", new Guid("fbe18f17-7aca-e801-0031-793076552587"), "Quarter" },
                    { new Guid("4ec8849f-c441-a63e-edbd-db1420919e48"), "en", new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "No exceptions." },
                    { new Guid("4f6d5a3b-31b2-cf76-afe7-f874c1ca805e"), "en", new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "From" },
                    { new Guid("4fdf3500-ebb4-fa5b-9ea8-2a0c56521c43"), "ar", new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "مدرجة في ربع لاحق" },
                    { new Guid("510e01f3-e1ca-c47d-e6a4-ca19b2df1dd5"), "ar", new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "ضريبة التصرفات العقارية" },
                    { new Guid("520015e9-be6b-69ab-16b6-f810d9fbe647"), "ar", new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "الأول" },
                    { new Guid("532b69a3-60e9-8647-6559-e3acd1738cd4"), "en", new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "VAT" },
                    { new Guid("570a0a45-5474-0e51-8881-7f9dd4efad09"), "en", new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "Discount" },
                    { new Guid("581144f7-f4d9-f8fc-f8f0-c1ddf19658d3"), "en", new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "The discount cannot be above the amount." },
                    { new Guid("5a5f23e4-87b0-56c2-8d21-98be1c46bbfc"), "ar", new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "تعذر تنفيذ ذلك." },
                    { new Guid("5ad01b72-e378-f4d7-ad2b-b031a3284bcc"), "ar", new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "ضريبة التصرفات العقارية" },
                    { new Guid("5b292da7-398a-d2af-cf83-3234c3e9934b"), "en", new Guid("9986841e-810b-8962-d4e1-a388352f38e9"), "Description" },
                    { new Guid("5b6934ed-7b02-e171-3a94-f3b3441512e7"), "en", new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "Sale" },
                    { new Guid("5d4c3a67-6fe3-707c-c495-92181accdc47"), "en", new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "First half" },
                    { new Guid("60f9c445-c74f-c2dd-b1cc-164d6312b080"), "en", new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "A picture or PDF" },
                    { new Guid("611b8227-ef2e-4523-3ba8-f844e53abdce"), "ar", new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "ضريبة المبيعات" },
                    { new Guid("61cf7604-0c16-0dcd-f40e-c9c713232c15"), "ar", new Guid("9986841e-810b-8962-d4e1-a388352f38e9"), "الوصف" },
                    { new Guid("6286fa79-17bd-d23f-78e4-c4a136741219"), "en", new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "No revenue yet." },
                    { new Guid("63125c9a-c8fb-5092-006c-60ec095262b6"), "ar", new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "رقم ضريبي غير صالح" },
                    { new Guid("639193ec-14d7-6da1-88dd-1407f44034b3"), "en", new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "VAT refund" },
                    { new Guid("6531a1ef-2aa5-7feb-68c6-10fe52e4fb7d"), "en", new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "Second half" },
                    { new Guid("65a6f0e8-5be5-0bf6-4bc7-6a1eba373a6d"), "en", new Guid("ffb92055-46ed-1867-1094-cd3c1b0e529e"), "Choose the account." },
                    { new Guid("6709191c-fcba-8e91-2a28-d2b4ac929653"), "ar", new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "كشف ضريبة القيمة المضافة" },
                    { new Guid("68a29b1e-952a-5e91-5c31-590bd7fa8843"), "ar", new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "اختر الربع." },
                    { new Guid("6a8d9a1b-3995-200d-5748-cfa8c611e060"), "en", new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "Type the invoice's number." },
                    { new Guid("6b621a9c-9560-cb17-001d-2af22256e54e"), "en", new Guid("7e5b1a8f-008a-8f51-d0b0-61eac4a9ff71"), "Revenue" },
                    { new Guid("6bb36050-f13f-084c-04de-c5e950eaf0f8"), "ar", new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "المبلغ" },
                    { new Guid("6f225d23-0434-443e-eb35-f7979da7cd29"), "ar", new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "المشتري" },
                    { new Guid("6ffd35cb-8d0f-7e38-71b9-d3a50653ca1c"), "ar", new Guid("ffb92055-46ed-1867-1094-cd3c1b0e529e"), "اختر الحساب." },
                    { new Guid("7664423d-1a88-c5fe-5063-7445fe6e7242"), "ar", new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "تنتهي الفترة قبل أن تبدأ." },
                    { new Guid("76a464a0-6ae2-83f5-a205-9eb30fa0d8c4"), "ar", new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("7a29ecd2-1d55-056e-ef1d-03fc54b2b773"), "ar", new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "لا إيرادات بعد." },
                    { new Guid("7caf3c40-9763-dd6d-42e4-7e19240c3fc2"), "ar", new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "تاريخ الفاتورة" },
                    { new Guid("7ce333b1-8dc6-d55b-fd1a-350ddb9bc4dc"), "en", new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "Choose the quarter." },
                    { new Guid("7d1a4ce8-de62-91d5-8783-bd99c9b02736"), "en", new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "No file" },
                    { new Guid("7d494342-0f9d-1d35-e158-0e199f9296ab"), "ar", new Guid("dfcb343d-720c-3164-c03d-5549447750fe"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("7f220c36-6daa-7d42-a3ca-e4dda7366184"), "ar", new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "حفظ" },
                    { new Guid("7f6b4c73-3edc-416f-1703-8c3ed226d550"), "ar", new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "الثاني" },
                    { new Guid("7fa24744-2a43-b4f2-3e6a-76c638faa527"), "en", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "Revenue" },
                    { new Guid("80e7ea34-7d5c-920f-0c17-67b9e86fdea3"), "ar", new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "بلا رقم ضريبي" },
                    { new Guid("81237a0a-e485-782c-0da1-b9a4a62c04fb"), "en", new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "Total" },
                    { new Guid("81932110-b482-f6e0-ea70-eafdb1323700"), "ar", new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "الرقم الضريبي للمشتري" },
                    { new Guid("81e9d1c2-b2f3-8791-a232-6cb536e538ea"), "en", new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "Save" },
                    { new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"), "en", new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "The VAT number is 15 digits, starting and ending with 3." },
                    { new Guid("82c826a3-aa0c-ee8f-fb01-6b4bd7e2b926"), "ar", new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "نصف سنة" },
                    { new Guid("8583a7e3-69c4-e8b5-c6f4-a426c27c6413"), "ar", new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "المتوقع في الكشف" },
                    { new Guid("85a0be7e-2f0d-7b76-55ae-d3413f2d91ab"), "ar", new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "رقم الفاتورة" },
                    { new Guid("873d2519-e1c3-6937-8789-61be872b97ec"), "en", new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "To" },
                    { new Guid("87880210-6ae9-b2c1-98d3-5e1f97635647"), "en", new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "First" },
                    { new Guid("88038e8e-367f-4188-88cd-f94cf17c5015"), "en", new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "The statement expected" },
                    { new Guid("8b083dea-98ea-d793-adb2-a095463d1f99"), "ar", new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "الضريبة" },
                    { new Guid("8ebe400b-b9d2-c6ed-b02f-6f9a3fef662f"), "en", new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "Type the buyer's name." },
                    { new Guid("90180e09-a012-d150-28db-4544dad4c9a5"), "ar", new Guid("b24cade7-d2d0-3e04-5df3-7ed583fe5446"), "تعذر تنفيذ ذلك." },
                    { new Guid("91e412fa-0eab-013c-4d7b-b8a582d7ba0e"), "en", new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "VAT statement" },
                    { new Guid("91e65c04-d70c-6dbd-e49c-21464eca2190"), "en", new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "Not taxable" },
                    { new Guid("945e5f2d-6620-e680-e0ac-3471da6f7b4a"), "en", new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "Amount" },
                    { new Guid("9e51c001-0df9-7e10-a3e3-e301d893295a"), "ar", new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "الثالث" },
                    { new Guid("9f664deb-947a-1053-d25e-5162d6394700"), "ar", new Guid("7e5b1a8f-008a-8f51-d0b0-61eac4a9ff71"), "إيراد" },
                    { new Guid("9fb68fda-b1f8-f83f-b70b-2bba8ef2a843"), "ar", new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "غير خاضعة" },
                    { new Guid("a0b8aca2-51e2-afce-7da7-2db09b819432"), "ar", new Guid("99f25918-f1e7-dde4-0f2f-cd67937c72da"), "هذا الملف مرفق بقيد آخر." },
                    { new Guid("a1b38c34-d9f0-8715-1447-e4a4af33a402"), "en", new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "Could not read the revenue. Check the connection and try again." },
                    { new Guid("a50a8ded-83de-6cce-6be3-9618f4bf259f"), "en", new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "This invoice number is already used." },
                    { new Guid("a5b6ed39-bd8f-c33a-b6bc-f64749c88618"), "ar", new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "قائمة الاستثناءات" },
                    { new Guid("a5ce9137-4240-bc78-3c24-e90a9a55309a"), "ar", new Guid("1c891c17-20fe-13a6-77ea-82f216f91ca6"), "لم يُعثر على الملف. اختره مجدداً." },
                    { new Guid("a8d7b5b7-eeea-04f7-f68f-a3c5a5f95e94"), "en", new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "Amount before tax" },
                    { new Guid("a8efe087-99df-b9e4-4b01-787565f9d2b7"), "en", new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "Net (refund)" },
                    { new Guid("a9f655de-d43a-88d3-af1d-57abc0a81622"), "ar", new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "النصف الثاني" },
                    { new Guid("aa1aae2e-d7b1-9901-05e2-2a7612a3be3d"), "ar", new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "إدراج في الربع التالي" },
                    { new Guid("ac8bf060-5f66-2664-b3e4-6b0d4e29723e"), "ar", new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "بلا فاتورة" },
                    { new Guid("ad3e5092-c017-23ce-0de8-58beca6d2ac0"), "ar", new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "تعذرت قراءة الإيرادات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("adece867-dd5a-24b1-900c-90a9c2ca7114"), "ar", new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "بلا ملف" },
                    { new Guid("b14c6e3a-3d7c-51e8-7cce-8313527fb652"), "ar", new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "تاريخ الفاتورة خارج فترة المشروع." },
                    { new Guid("b2ae1889-c3f1-95ab-7080-7bb599e34d29"), "en", new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "Saved." },
                    { new Guid("b2c0c9d4-3e8f-3c67-61c4-4e177e660867"), "ar", new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "فواتير المشتريات" },
                    { new Guid("b5b933f6-6ef5-c85b-9c77-d6982a1d88a1"), "ar", new Guid("3027597b-3d9d-3d2c-c9a3-bc8bef773bf5"), "الحساب" },
                    { new Guid("b733ece1-a370-1e9e-ab91-62d12c153a13"), "en", new Guid("ef7dc8b6-066b-9c91-2370-29beab7ba230"), "The description is too long." },
                    { new Guid("bf697ab2-75e5-ee2c-1aaf-f55e29dcc2c9"), "en", new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "Real estate transaction tax" },
                    { new Guid("c1c0436c-2e0e-7fb3-baff-9f844f637086"), "en", new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "Invoice date" },
                    { new Guid("c7d586bc-3958-7626-5afe-cd8e0e5d9975"), "en", new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "Waiting for approval" },
                    { new Guid("c9250933-3897-e2c8-2082-c633b0f0a512"), "en", new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "Could not read the statement. Check the connection and try again." },
                    { new Guid("cd205636-f07e-d600-d43d-1b87f1197fb0"), "en", new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "Buyer" },
                    { new Guid("ce39e479-d2df-5488-2ca2-9b21bb9f8da4"), "en", new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "Purchase invoices" },
                    { new Guid("ce98fab6-382b-23aa-8678-1c0d30ec55bf"), "ar", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "التاريخ" },
                    { new Guid("d0137f61-0897-c280-669f-6bec27a159fd"), "ar", new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "من" },
                    { new Guid("d0aafd6d-6353-7e9c-1110-ff9739342ef7"), "en", new Guid("99f25918-f1e7-dde4-0f2f-cd67937c72da"), "This file is already on an entry." },
                    { new Guid("d30d7b41-b07c-718c-3b0a-107712534613"), "ar", new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "فاتورة مبيعات" },
                    { new Guid("d476da59-e6da-4a8d-609f-52e43376456e"), "en", new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "Buyer's VAT number" },
                    { new Guid("d7034bd1-03ca-5a31-deff-faf88b85e392"), "ar", new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "مرتجعات" },
                    { new Guid("d73a526a-092c-54e1-6034-fe598e82ba73"), "en", new Guid("c93c695f-77af-13cb-2846-908a246bb63a"), "Type" },
                    { new Guid("dc0ed562-912b-048f-98bc-13ac6eb6e2e3"), "ar", new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "الفرق المستحق" },
                    { new Guid("dd394de8-3820-ea29-69b2-374476e406d9"), "en", new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "Exceptions to review" },
                    { new Guid("dda3a7cc-82bb-ac87-9eb3-a821b440a783"), "en", new Guid("b24cade7-d2d0-3e04-5df3-7ed583fe5446"), "This could not be done." },
                    { new Guid("de436f1f-1700-1ae3-c753-59c35d561fc4"), "en", new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "Difference" },
                    { new Guid("de8446f0-0df6-3c4a-2cce-f280a721c2b3"), "en", new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "Half year" },
                    { new Guid("df7b0129-43a3-513a-fa97-56b09eacc008"), "ar", new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "ضريبة القيمة المضافة" },
                    { new Guid("e0aec402-0dda-07fa-caaf-9d7bd5b0ab0d"), "ar", new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "الإجمالي" },
                    { new Guid("e4b4ac84-d6a3-994d-cf6c-a4afda351b98"), "ar", new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "الخصم" },
                    { new Guid("e6947b4d-9d20-19c5-6a58-7587aae59fa1"), "en", new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "Declared in a later quarter" },
                    { new Guid("e896cd4e-d02a-83e3-a289-8a34554dae6d"), "ar", new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "بانتظار الاعتماد" },
                    { new Guid("e964c709-b218-8312-9cc8-5492b306673d"), "ar", new Guid("fbe18f17-7aca-e801-0031-793076552587"), "ربع" },
                    { new Guid("eb5f442e-046c-64f6-ac20-058bebdd4078"), "en", new Guid("e85316d8-05a6-2c9b-fff1-74ae9049a30b"), "The date is outside the project's period." },
                    { new Guid("ee3eb843-01f8-53d6-d36f-cdd5d9fa1de0"), "ar", new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "أخرى" },
                    { new Guid("f0add891-c0c5-230c-e66b-54b4ef4141fb"), "en", new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "Third" },
                    { new Guid("f71bccff-38c6-3ab5-ffff-9ce1dfe339ed"), "en", new Guid("640ad877-bf82-1804-e1d6-69abbd7f98d7"), "Type an amount above zero, with at most two decimals." },
                    { new Guid("faf7f2a7-915b-14ae-7e49-9347e241ac7c"), "en", new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "No VAT number" },
                    { new Guid("fc631c02-8883-0c28-6d20-2cc72c681ada"), "en", new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "No connection to the server. Try again." },
                    { new Guid("fcdbb307-dd56-19c2-46df-76c6fa41d38d"), "en", new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"), "Other" },
                    { new Guid("fde3ef5f-1e6d-96ca-a54f-383e780c1566"), "en", new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "Tax" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_Number",
                table: "SalesInvoice",
                column: "Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevenueDetails");

            migrationBuilder.DropTable(
                name: "SalesInvoice");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0036f2ac-e636-e1a9-99a6-ebdeb1224b44"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00d248c7-ea8b-ed71-2f7f-ce322c9f1258"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01cf41f7-ed5b-f1cf-8b2a-1d051bf64085"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0407605a-c96a-62d3-bc2f-593ad9a0ef00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0458e8e5-5916-79e7-4194-18a3093798a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("047ceb71-19ce-51c4-2011-d8f1a8e9d5f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06234c06-75c3-633d-2e9f-3ea5aaf4ad95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("063c36a7-a52c-8f99-e157-75c6014b7495"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08df4a9a-d031-634c-94ff-b80d94fde8ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09510e0e-2ea5-f556-e437-b3127d6aba74"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09673683-2250-fca5-7d94-213d082127f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0aa0b73c-9411-1cc4-2af9-282797c2984a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c2d5090-4780-be30-87c9-a319275c554b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c9043cf-263c-c229-2ef6-4cce26151b41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("118eb76b-dab0-e7fc-1f7f-80a7101abad2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("11f67e3b-fa67-94b7-6fe8-891c5a269d8b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1237f62d-0322-4fec-2ca6-2eb54316df70"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("13156b78-e9ec-28e7-9d92-6b9fe768c8ed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a9a486a-c230-7f33-d1a8-2c97e69b1604"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b34439b-bc6b-f639-14dd-60b8309bf48e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f980580-b6c5-9051-bd64-d30c396deca1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("202a2d50-cdcb-cd4d-086e-f1ce3d63fc95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("219fb21a-c044-a9a1-4399-ec9f1a973530"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a054c59-58c5-1358-cf5b-aed2093d13a8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2acfa645-34f0-8e44-617f-f0d43aca161f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b4c55af-b6cc-a3c0-2db2-04175d90bdb2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bd3db63-bb80-8b0c-0632-7c6948235300"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bff715c-9281-4599-0e61-058d2d452510"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2d68c01d-fabb-2296-f41a-3a3e3b828b21"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e094557-6d7e-b3d9-da37-fdc7da3de402"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("319b04ff-9576-d428-0f5c-cd53a14b659a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("341fa924-18e3-e734-2ca0-1953306bd34e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("345e8887-df72-c02b-8fdd-693edd48bc7f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("34fc25ae-a507-817d-5be0-31052a75c46b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37dd53a5-8cb6-fa40-b5b2-47fc5b23838b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37f592a8-7898-35b8-5303-0d71a758d231"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38fb4888-c1a7-518e-cac4-c8ddfaeee5ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("398ea309-4580-473b-3c60-cbce927e681b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3ab7caf6-b51d-590f-a6d9-b4f5dc29b4ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b221698-6956-a455-caa1-118d8a67c756"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3cbf9aa9-b363-af0d-1a0d-75315fd6558d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41473377-a79e-b35a-1f98-1820fe2e0c5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41f27a78-58c3-df90-d339-925afc1d55e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("449b8219-4168-4400-8e51-fe12a63171da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("451805aa-edd0-dded-205c-8d0256534263"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("45cd37a8-56af-fe3a-4d76-36ddece53523"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4605ecfe-b3d8-5df9-7683-6fc44f1d4271"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46701a14-d71b-7f64-dc21-57cbf7b6709d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("479ad873-fd5c-3870-a619-c821d697ae19"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ba46a6e-b644-a163-270c-1e7188679b11"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ec8849f-c441-a63e-edbd-db1420919e48"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f6d5a3b-31b2-cf76-afe7-f874c1ca805e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fdf3500-ebb4-fa5b-9ea8-2a0c56521c43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("510e01f3-e1ca-c47d-e6a4-ca19b2df1dd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("520015e9-be6b-69ab-16b6-f810d9fbe647"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("532b69a3-60e9-8647-6559-e3acd1738cd4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("570a0a45-5474-0e51-8881-7f9dd4efad09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("581144f7-f4d9-f8fc-f8f0-c1ddf19658d3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a5f23e4-87b0-56c2-8d21-98be1c46bbfc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ad01b72-e378-f4d7-ad2b-b031a3284bcc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b292da7-398a-d2af-cf83-3234c3e9934b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b6934ed-7b02-e171-3a94-f3b3441512e7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d4c3a67-6fe3-707c-c495-92181accdc47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60f9c445-c74f-c2dd-b1cc-164d6312b080"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("611b8227-ef2e-4523-3ba8-f844e53abdce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61cf7604-0c16-0dcd-f40e-c9c713232c15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6286fa79-17bd-d23f-78e4-c4a136741219"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63125c9a-c8fb-5092-006c-60ec095262b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("639193ec-14d7-6da1-88dd-1407f44034b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6531a1ef-2aa5-7feb-68c6-10fe52e4fb7d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65a6f0e8-5be5-0bf6-4bc7-6a1eba373a6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6709191c-fcba-8e91-2a28-d2b4ac929653"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68a29b1e-952a-5e91-5c31-590bd7fa8843"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a8d9a1b-3995-200d-5748-cfa8c611e060"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b621a9c-9560-cb17-001d-2af22256e54e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6bb36050-f13f-084c-04de-c5e950eaf0f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6f225d23-0434-443e-eb35-f7979da7cd29"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6ffd35cb-8d0f-7e38-71b9-d3a50653ca1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7664423d-1a88-c5fe-5063-7445fe6e7242"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76a464a0-6ae2-83f5-a205-9eb30fa0d8c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a29ecd2-1d55-056e-ef1d-03fc54b2b773"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7caf3c40-9763-dd6d-42e4-7e19240c3fc2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ce333b1-8dc6-d55b-fd1a-350ddb9bc4dc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d1a4ce8-de62-91d5-8783-bd99c9b02736"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d494342-0f9d-1d35-e158-0e199f9296ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f220c36-6daa-7d42-a3ca-e4dda7366184"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f6b4c73-3edc-416f-1703-8c3ed226d550"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7fa24744-2a43-b4f2-3e6a-76c638faa527"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80e7ea34-7d5c-920f-0c17-67b9e86fdea3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81237a0a-e485-782c-0da1-b9a4a62c04fb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81932110-b482-f6e0-ea70-eafdb1323700"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81e9d1c2-b2f3-8791-a232-6cb536e538ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82c826a3-aa0c-ee8f-fb01-6b4bd7e2b926"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8583a7e3-69c4-e8b5-c6f4-a426c27c6413"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85a0be7e-2f0d-7b76-55ae-d3413f2d91ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("873d2519-e1c3-6937-8789-61be872b97ec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87880210-6ae9-b2c1-98d3-5e1f97635647"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88038e8e-367f-4188-88cd-f94cf17c5015"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8b083dea-98ea-d793-adb2-a095463d1f99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ebe400b-b9d2-c6ed-b02f-6f9a3fef662f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90180e09-a012-d150-28db-4544dad4c9a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91e412fa-0eab-013c-4d7b-b8a582d7ba0e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91e65c04-d70c-6dbd-e49c-21464eca2190"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("945e5f2d-6620-e680-e0ac-3471da6f7b4a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e51c001-0df9-7e10-a3e3-e301d893295a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f664deb-947a-1053-d25e-5162d6394700"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fb68fda-b1f8-f83f-b70b-2bba8ef2a843"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0b8aca2-51e2-afce-7da7-2db09b819432"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a1b38c34-d9f0-8715-1447-e4a4af33a402"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a50a8ded-83de-6cce-6be3-9618f4bf259f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a5b6ed39-bd8f-c33a-b6bc-f64749c88618"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a5ce9137-4240-bc78-3c24-e90a9a55309a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8d7b5b7-eeea-04f7-f68f-a3c5a5f95e94"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8efe087-99df-b9e4-4b01-787565f9d2b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a9f655de-d43a-88d3-af1d-57abc0a81622"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa1aae2e-d7b1-9901-05e2-2a7612a3be3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac8bf060-5f66-2664-b3e4-6b0d4e29723e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ad3e5092-c017-23ce-0de8-58beca6d2ac0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("adece867-dd5a-24b1-900c-90a9c2ca7114"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b14c6e3a-3d7c-51e8-7cce-8313527fb652"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2ae1889-c3f1-95ab-7080-7bb599e34d29"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2c0c9d4-3e8f-3c67-61c4-4e177e660867"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b5b933f6-6ef5-c85b-9c77-d6982a1d88a1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b733ece1-a370-1e9e-ab91-62d12c153a13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf697ab2-75e5-ee2c-1aaf-f55e29dcc2c9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1c0436c-2e0e-7fb3-baff-9f844f637086"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c7d586bc-3958-7626-5afe-cd8e0e5d9975"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9250933-3897-e2c8-2082-c633b0f0a512"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd205636-f07e-d600-d43d-1b87f1197fb0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce39e479-d2df-5488-2ca2-9b21bb9f8da4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce98fab6-382b-23aa-8678-1c0d30ec55bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0137f61-0897-c280-669f-6bec27a159fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0aafd6d-6353-7e9c-1110-ff9739342ef7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d30d7b41-b07c-718c-3b0a-107712534613"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d476da59-e6da-4a8d-609f-52e43376456e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7034bd1-03ca-5a31-deff-faf88b85e392"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d73a526a-092c-54e1-6034-fe598e82ba73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc0ed562-912b-048f-98bc-13ac6eb6e2e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd394de8-3820-ea29-69b2-374476e406d9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dda3a7cc-82bb-ac87-9eb3-a821b440a783"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("de436f1f-1700-1ae3-c753-59c35d561fc4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("de8446f0-0df6-3c4a-2cce-f280a721c2b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df7b0129-43a3-513a-fa97-56b09eacc008"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0aec402-0dda-07fa-caaf-9d7bd5b0ab0d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4b4ac84-d6a3-994d-cf6c-a4afda351b98"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6947b4d-9d20-19c5-6a58-7587aae59fa1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e896cd4e-d02a-83e3-a289-8a34554dae6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e964c709-b218-8312-9cc8-5492b306673d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb5f442e-046c-64f6-ac20-058bebdd4078"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee3eb843-01f8-53d6-d36f-cdd5d9fa1de0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0add891-c0c5-230c-e66b-54b4ef4141fb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f71bccff-38c6-3ab5-ffff-9ce1dfe339ed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("faf7f2a7-915b-14ae-7e49-9347e241ac7c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc631c02-8883-0c28-6d20-2cc72c681ada"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcdbb307-dd56-19c2-46df-76c6fa41d38d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fde3ef5f-1e6d-96ca-a54f-383e780c1566"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("00095999-1d8f-0540-4945-641c6d0f4997"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0447cea6-3786-1f47-5238-d936034b480c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1c891c17-20fe-13a6-77ea-82f216f91ca6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3027597b-3d9d-3d2c-c9a3-bc8bef773bf5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("448b37d3-a767-2874-ec18-b020696848f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("58c59be4-d26b-e903-ffab-2905910659c8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("640ad877-bf82-1804-e1d6-69abbd7f98d7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("71600c58-1298-79d7-4df1-acdbbf463377"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7e5b1a8f-008a-8f51-d0b0-61eac4a9ff71"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9986841e-810b-8962-d4e1-a388352f38e9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99f25918-f1e7-dde4-0f2f-cd67937c72da"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("afcce444-d00b-85f9-85f2-852634ce1728"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b24cade7-d2d0-3e04-5df3-7ed583fe5446"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c93c695f-77af-13cb-2846-908a246bb63a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dfcb343d-720c-3164-c03d-5549447750fe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7483b74-f7c5-7661-f669-3775e5c40c9d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e85316d8-05a6-2c9b-fff1-74ae9049a30b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ef7dc8b6-066b-9c91-2370-29beab7ba230"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fbe18f17-7aca-e801-0031-793076552587"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffb92055-46ed-1867-1094-cd3c1b0e529e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"));
        }
    }
}
