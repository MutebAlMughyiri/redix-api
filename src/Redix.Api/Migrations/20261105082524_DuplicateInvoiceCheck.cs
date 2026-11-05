using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DuplicateInvoiceCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56c5b8da-3d46-862f-3f6e-050f0c9c26bc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98837731-b5d0-ec39-e37f-4a9c2ea124a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "", "expenses.invoiceAlreadyAttached" },
                    { new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "", "expenses.duplicateInvoice" },
                    { new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "", "expenses.duplicateHidden" },
                    { new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "", "expenses.duplicateTitle" },
                    { new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "", "expenses.saveAnyway" },
                    { new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "", "expenses.duplicateExplain" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1119f52f-d29e-839b-f71e-694ac237f060"),
                column: "Value",
                value: "Unit price");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("864be42c-280f-d6f1-123c-a95722be794f"),
                column: "Value",
                value: "سعر الوحدة");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a24b383-6e39-03d6-94eb-d0c37f7f167d"),
                column: "Value",
                value: "تعذّرت قراءة رمز QR من هذه الفاتورة. عبّئ الحقول يدوياً.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"),
                column: "Value",
                value: "Quantity");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"),
                column: "Value",
                value: "العدد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0927f44-a92f-8efd-8beb-77c7cf7b66ee"),
                column: "Value",
                value: "No QR code could be read from this invoice. Fill the fields yourself.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("07ca8f05-3adf-959a-8805-fe9984c4873f"), "en", new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "The same supplier and invoice number exist in a project you have no access to. Save it only if this is a different invoice." },
                    { new Guid("190594d7-d9d2-c6f7-9c7d-693b648abe5c"), "en", new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "This file is already on another expense." },
                    { new Guid("1aa13478-96a9-7965-566a-7f766ac0bbe8"), "ar", new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "هذا الملف مرفق بمصروف آخر." },
                    { new Guid("51472944-cc8f-fe92-78ff-3fbfb7f039ac"), "en", new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "This supplier's invoice number is already entered." },
                    { new Guid("55d18128-aa0e-664b-5841-1617d11d396a"), "ar", new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "رقم فاتورة هذا المورد مُدخل مسبقاً." },
                    { new Guid("65c87a54-3324-2b56-95a4-3d61e1ecf589"), "ar", new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "نفس المورد ورقم الفاتورة موجودان في مشروع لا تملك صلاحية الوصول إليه. احفظ فقط إن كانت فاتورة مختلفة." },
                    { new Guid("80da283b-b8da-4c71-5b15-1864ad122c35"), "ar", new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "يبدو أن هذه الفاتورة مُدخلة مسبقاً" },
                    { new Guid("88992b82-bacc-3e21-1bf2-fa10bb524339"), "ar", new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "نفس المورد ورقم الفاتورة موجودان في القيد السابق أعلاه. احفظ فقط إن كانت فاتورة مختلفة." },
                    { new Guid("8c4cde6e-3654-36bd-bd3c-7863f0f2c7e3"), "en", new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "Save anyway" },
                    { new Guid("eedc95a7-38d9-4a61-dc8e-a6c254ad7634"), "en", new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "This invoice looks entered already" },
                    { new Guid("f8577729-6ee9-fe5a-b8eb-d9a88cde0f01"), "ar", new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "حفظ رغم ذلك" },
                    { new Guid("f89d53a5-983b-a25c-20fb-15d6fef8578c"), "en", new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "The same supplier and invoice number exist in the earlier entry above. Save it only if this is a different invoice." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Number",
                table: "Invoices");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07ca8f05-3adf-959a-8805-fe9984c4873f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("190594d7-d9d2-c6f7-9c7d-693b648abe5c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1aa13478-96a9-7965-566a-7f766ac0bbe8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("51472944-cc8f-fe92-78ff-3fbfb7f039ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55d18128-aa0e-664b-5841-1617d11d396a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65c87a54-3324-2b56-95a4-3d61e1ecf589"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80da283b-b8da-4c71-5b15-1864ad122c35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88992b82-bacc-3e21-1bf2-fa10bb524339"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c4cde6e-3654-36bd-bd3c-7863f0f2c7e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eedc95a7-38d9-4a61-dc8e-a6c254ad7634"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8577729-6ee9-fe5a-b8eb-d9a88cde0f01"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f89d53a5-983b-a25c-20fb-15d6fef8578c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "", "expenses.loadFailed" },
                    { new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "", "expenses.required" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1119f52f-d29e-839b-f71e-694ac237f060"),
                column: "Value",
                value: "Unit price (optional)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("864be42c-280f-d6f1-123c-a95722be794f"),
                column: "Value",
                value: "سعر الوحدة (اختياري)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a24b383-6e39-03d6-94eb-d0c37f7f167d"),
                column: "Value",
                value: "تعذّرت قراءة رمز QR. عبّئ الحقول يدوياً.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"),
                column: "Value",
                value: "Quantity (optional)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"),
                column: "Value",
                value: "العدد (اختياري)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0927f44-a92f-8efd-8beb-77c7cf7b66ee"),
                column: "Value",
                value: "The QR code could not be read. Fill the fields yourself.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("56c5b8da-3d46-862f-3f6e-050f0c9c26bc"), "ar", new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "إلزامي" },
                    { new Guid("98837731-b5d0-ec39-e37f-4a9c2ea124a6"), "en", new Guid("b4a77062-42a5-ffac-3c7a-46334d2c15dd"), "Required" },
                    { new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"), "ar", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "تعذّرت القراءة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"), "en", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "Could not read this. Check the network and try again." }
                });
        }
    }
}
