using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceKeepsOnlyItsNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42f25b7f-49cb-97b4-3e1b-271cfc3b2498"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("788309bd-1fd5-2e34-ff21-c505c5269e49"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("989de814-a48f-8539-d5ca-dba88255d90a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a923e1fd-9c91-e68f-9719-4f3675d1d172"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9a26130-4e4f-727a-5cec-51a4a34372d1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea726598-7799-0795-c17b-6bee405d7d9c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0df95d1-3505-f60d-bf82-2b16a41f17c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f458f59b-4adf-eeb0-c7dc-8a849dd358bb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c66a23da-16f7-380b-3333-12807981e0ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"));

            migrationBuilder.DropColumn(
                name: "AmountAfterDiscount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AmountBeforeDiscount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsCreditNote",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerVatNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Vat",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatPeriodStart",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountAfterDiscount",
                table: "Invoices",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBeforeDiscount",
                table: "Invoices",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Invoices",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InvoiceDate",
                table: "Invoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsCreditNote",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SellerVatNumber",
                table: "Invoices",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Invoices",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "Invoices",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VatPeriodStart",
                table: "Invoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "", "share.invalidVatNumber" },
                    { new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "", "share.invalidAmount" },
                    { new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "", "share.invalidInvoiceDate" },
                    { new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "", "share.duplicateInvoice" },
                    { new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "", "share.invalidInvoiceNumber" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("42f25b7f-49cb-97b4-3e1b-271cfc3b2498"), "en", new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "The invoice's date is outside the project's period." },
                    { new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"), "en", new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "Write the invoice number in 50 characters at most." },
                    { new Guid("788309bd-1fd5-2e34-ff21-c505c5269e49"), "ar", new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "تاريخ الفاتورة خارج فترة المشروع." },
                    { new Guid("989de814-a48f-8539-d5ca-dba88255d90a"), "ar", new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "إجمالي الفاتورة غير صحيح." },
                    { new Guid("a923e1fd-9c91-e68f-9719-4f3675d1d172"), "en", new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "The invoice's total is not valid." },
                    { new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"), "ar", new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "اكتب رقم الفاتورة في 50 حرفاً على الأكثر." },
                    { new Guid("e9a26130-4e4f-727a-5cec-51a4a34372d1"), "ar", new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "هذه الفاتورة مرفقة بمصروف آخر." },
                    { new Guid("ea726598-7799-0795-c17b-6bee405d7d9c"), "en", new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "The VAT number on this invoice cannot be read." },
                    { new Guid("f0df95d1-3505-f60d-bf82-2b16a41f17c5"), "ar", new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "تعذرت قراءة الرقم الضريبي في هذه الفاتورة." },
                    { new Guid("f458f59b-4adf-eeb0-c7dc-8a849dd358bb"), "en", new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "This invoice is already on another expense." }
                });
        }
    }
}
