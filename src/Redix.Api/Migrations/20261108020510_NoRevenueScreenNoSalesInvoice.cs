using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoRevenueScreenNoSalesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoice");

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
                keyValue: new Guid("0ade3f45-967d-b9d7-207d-29b741afd3f5"));

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
                keyValue: new Guid("1a9a486a-c230-7f33-d1a8-2c97e69b1604"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("34fc25ae-a507-817d-5be0-31052a75c46b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b221698-6956-a455-caa1-118d8a67c756"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41473377-a79e-b35a-1f98-1820fe2e0c5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("501e6637-f41d-0ed9-34cd-645dd466ebe8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6286fa79-17bd-d23f-78e4-c4a136741219"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a8d9a1b-3995-200d-5748-cfa8c611e060"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6f225d23-0434-443e-eb35-f7979da7cd29"));

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
                keyValue: new Guid("7f220c36-6daa-7d42-a3ca-e4dda7366184"));

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
                keyValue: new Guid("8ebe400b-b9d2-c6ed-b02f-6f9a3fef662f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a50a8ded-83de-6cce-6be3-9618f4bf259f"));

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
                keyValue: new Guid("c1c0436c-2e0e-7fb3-baff-9f844f637086"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd205636-f07e-d600-d43d-1b87f1197fb0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d30d7b41-b07c-718c-3b0a-107712534613"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d476da59-e6da-4a8d-609f-52e43376456e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"));

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
                keyValue: new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesInvoice",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuyerVatNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false)
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
                    { new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "", "revenue.item" },
                    { new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "", "revenue.buyer" },
                    { new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "", "revenue.unknownSale" },
                    { new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "", "revenue.save" },
                    { new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "", "revenue.invalidBuyerName" },
                    { new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "", "revenue.duplicateInvoice" },
                    { new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "", "revenue.invoiceDate" },
                    { new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "", "revenue.invalidInvoiceDate" },
                    { new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "", "revenue.saved" },
                    { new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "", "revenue.invalidInvoiceNumber" },
                    { new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "", "revenue.buyerVatNumber" },
                    { new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "", "revenue.newSale" },
                    { new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "", "revenue.empty" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0458e8e5-5916-79e7-4194-18a3093798a7"), "ar", new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "اكتب اسم المشتري." },
                    { new Guid("047ceb71-19ce-51c4-2011-d8f1a8e9d5f0"), "en", new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "That sale was not found, or has an invoice." },
                    { new Guid("0ade3f45-967d-b9d7-207d-29b741afd3f5"), "ar", new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "البند" },
                    { new Guid("118eb76b-dab0-e7fc-1f7f-80a7101abad2"), "en", new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "Sales invoice" },
                    { new Guid("11f67e3b-fa67-94b7-6fe8-891c5a269d8b"), "ar", new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "رقم الفاتورة مستخدم." },
                    { new Guid("1a9a486a-c230-7f33-d1a8-2c97e69b1604"), "ar", new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "اكتب رقم الفاتورة." },
                    { new Guid("34fc25ae-a507-817d-5be0-31052a75c46b"), "en", new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "The invoice's date is outside the project's period." },
                    { new Guid("3b221698-6956-a455-caa1-118d8a67c756"), "ar", new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "حُفظ." },
                    { new Guid("41473377-a79e-b35a-1f98-1820fe2e0c5d"), "ar", new Guid("584c15a1-027b-9534-06df-798b0a7a3cbe"), "لم يُعثر على ذلك البيع، أو له فاتورة." },
                    { new Guid("501e6637-f41d-0ed9-34cd-645dd466ebe8"), "en", new Guid("3fa83908-3d6c-ab8a-26c0-09f0d35c735a"), "Item" },
                    { new Guid("6286fa79-17bd-d23f-78e4-c4a136741219"), "en", new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "No revenue yet." },
                    { new Guid("6a8d9a1b-3995-200d-5748-cfa8c611e060"), "en", new Guid("d598fac9-8645-e5ef-867d-2e39a3f8978b"), "Type the invoice's number." },
                    { new Guid("6f225d23-0434-443e-eb35-f7979da7cd29"), "ar", new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "المشتري" },
                    { new Guid("7a29ecd2-1d55-056e-ef1d-03fc54b2b773"), "ar", new Guid("ffbfd0a6-07c6-b443-41cf-7cd2815e603a"), "لا إيرادات بعد." },
                    { new Guid("7caf3c40-9763-dd6d-42e4-7e19240c3fc2"), "ar", new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "تاريخ الفاتورة" },
                    { new Guid("7f220c36-6daa-7d42-a3ca-e4dda7366184"), "ar", new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "حفظ" },
                    { new Guid("81932110-b482-f6e0-ea70-eafdb1323700"), "ar", new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "الرقم الضريبي للمشتري" },
                    { new Guid("81e9d1c2-b2f3-8791-a232-6cb536e538ea"), "en", new Guid("5dd19319-0c04-ccd1-2ccc-8ee89d6ca12d"), "Save" },
                    { new Guid("8ebe400b-b9d2-c6ed-b02f-6f9a3fef662f"), "en", new Guid("891fecc6-619c-cb76-3498-4af157e0b8c8"), "Type the buyer's name." },
                    { new Guid("a50a8ded-83de-6cce-6be3-9618f4bf259f"), "en", new Guid("ba88539f-bc5c-2d4e-2772-3ab432668945"), "This invoice number is already used." },
                    { new Guid("b14c6e3a-3d7c-51e8-7cce-8313527fb652"), "ar", new Guid("c3c804c7-01f9-fbe8-8d3e-9e74915fb449"), "تاريخ الفاتورة خارج فترة المشروع." },
                    { new Guid("b2ae1889-c3f1-95ab-7080-7bb599e34d29"), "en", new Guid("d4d87ddb-4151-4679-de32-e6f658cffab5"), "Saved." },
                    { new Guid("c1c0436c-2e0e-7fb3-baff-9f844f637086"), "en", new Guid("bdbfe3a5-744c-2fb7-a938-a2a4bccdc87a"), "Invoice date" },
                    { new Guid("cd205636-f07e-d600-d43d-1b87f1197fb0"), "en", new Guid("55308c5c-bc6e-17fe-fc55-27c94dbe096f"), "Buyer" },
                    { new Guid("d30d7b41-b07c-718c-3b0a-107712534613"), "ar", new Guid("fc4402e1-89f0-8e19-8991-8e0a8381bd80"), "فاتورة مبيعات" },
                    { new Guid("d476da59-e6da-4a8d-609f-52e43376456e"), "en", new Guid("dffa66ac-66d8-fe13-0134-b667625c09f3"), "Buyer's VAT number" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoice_Number",
                table: "SalesInvoice",
                column: "Number");
        }
    }
}
