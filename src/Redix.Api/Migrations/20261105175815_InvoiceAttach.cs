using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceAttach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerVatNumber",
                table: "Invoices",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "", "share.supplierDiffers" },
                    { new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "", "approvals.sellerDiffers" },
                    { new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "", "share.loadFailed" },
                    { new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "", "share.notYourEntry" },
                    { new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "", "share.noProject" },
                    { new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "", "share.invalidVatNumber" },
                    { new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "", "share.unknownAttachment" },
                    { new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "", "share.entryNotAttachable" },
                    { new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "", "share.project" },
                    { new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "", "share.noConnection" },
                    { new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "", "share.newExpense" },
                    { new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "", "share.title" },
                    { new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "", "share.attach" },
                    { new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "", "share.invoiceAlreadyAttached" },
                    { new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "", "share.invalidAmount" },
                    { new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "", "approvals.invoiceTotal" },
                    { new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "", "share.expense" },
                    { new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "", "share.attached" },
                    { new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "", "share.chooseItem" },
                    { new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "", "share.invalidInvoiceDate" },
                    { new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "", "share.duplicateInvoice" },
                    { new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "", "share.goesBack" },
                    { new Guid("db920181-5bba-2173-698f-310a22fe706d"), "", "share.refused" },
                    { new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "", "share.empty" },
                    { new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "", "share.matching" },
                    { new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "", "share.invalidInvoiceNumber" },
                    { new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "", "share.amountDiffers" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1d97dfe4-9d43-bd8a-36a3-10fa2f97fc6b"), "ar", new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "الرقم الضريبي في الفاتورة يخالف رقم المورد" },
                    { new Guid("24787303-6dcc-7ffa-2fab-61938696a525"), "en", new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "The invoice was attached." },
                    { new Guid("28469952-9edd-366a-04dc-327698d9bda4"), "en", new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "No connection to the server. Try again." },
                    { new Guid("29050156-82f7-1eae-7a5c-2cbe0b75973e"), "en", new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "You have no project to enter expenses in." },
                    { new Guid("38a21b97-90ba-d6bd-7aa2-f2439749b1b4"), "ar", new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "هذا الملف مرفق بمصروف آخر." },
                    { new Guid("3b8d0e13-2dee-42dd-9262-626de90992ec"), "ar", new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "إرفاق بالمصروف المحدد" },
                    { new Guid("3f56e07d-596d-ef3f-827a-f27a040975cf"), "ar", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "لا مصروف بلا فاتورة تحت هذا البند." },
                    { new Guid("40a32818-66b6-8438-813f-65754333fd20"), "ar", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "اختر البند لتظهر مصروفاته." },
                    { new Guid("42f25b7f-49cb-97b4-3e1b-271cfc3b2498"), "en", new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "The invoice's date is outside the project's period." },
                    { new Guid("43658527-3e52-7bb6-c8f9-7b6fa0f71bb6"), "ar", new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("44e6dde1-cc1d-635a-b8b7-4ed2c1f66101"), "ar", new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "المشروع" },
                    { new Guid("538c09d8-5500-f6bc-777c-307b54eb3edc"), "ar", new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "لا يقبل هذا المصروف فاتورة." },
                    { new Guid("5968a925-f1cf-617b-95d3-002099b5d26e"), "ar", new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "إجمالي الفاتورة" },
                    { new Guid("59c429f7-b163-b2bc-6eb1-7ae5ab5d9edd"), "en", new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "Attach an invoice" },
                    { new Guid("5b4be8c6-59f1-7da9-4a93-8eb7f224c147"), "ar", new Guid("db920181-5bba-2173-698f-310a22fe706d"), "تعذر تنفيذ ذلك." },
                    { new Guid("5db81562-0e7e-9605-0453-6db25836ace8"), "ar", new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "في الفاتورة {0}، وفي القيد {1}." },
                    { new Guid("62289094-f771-e3c7-d0b6-d12a800243df"), "ar", new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "إرفاق فاتورة" },
                    { new Guid("6531f12b-15dc-fb39-d3a8-777e72fb20ef"), "en", new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "This expense is someone else's." },
                    { new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"), "en", new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "Type the invoice's number." },
                    { new Guid("68d3dfdf-8118-7a8c-49b9-fcb433f4533a"), "en", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "Choose the item to see its expenses." },
                    { new Guid("6bacabe1-5d51-8314-c738-7229c9956a02"), "en", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "Matching" },
                    { new Guid("73774f15-36cb-daba-df7e-e30ff27ce97c"), "ar", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "مطابق" },
                    { new Guid("788309bd-1fd5-2e34-ff21-c505c5269e49"), "ar", new Guid("a4270a12-a85c-e19f-9d95-600561f59f73"), "تاريخ الفاتورة خارج فترة المشروع." },
                    { new Guid("804a5fd6-855f-8499-9de1-3c6790f551c8"), "ar", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "المصروف" },
                    { new Guid("85d83d9a-c79b-105e-575e-1c2bce433b68"), "ar", new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "ليس لديك مشروع تُدخل فيه مصروفات." },
                    { new Guid("8a6d4c58-ac90-1435-4cb6-fa22329dfbe0"), "en", new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "The VAT number on the invoice is not this supplier's." },
                    { new Guid("8c565c7c-4d57-4277-c947-c689982936c5"), "en", new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "This expense is approved. Attaching returns it to approval." },
                    { new Guid("8ebf05b7-1607-da43-d933-797d6592d256"), "en", new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "On the invoice {0}, on the entry {1}." },
                    { new Guid("8f90319f-8ea2-22e1-c4cf-ebda60acc97c"), "ar", new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "لم يُعثر على الملف. شاركه مجدداً." },
                    { new Guid("989de814-a48f-8539-d5ca-dba88255d90a"), "ar", new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "إجمالي الفاتورة غير صحيح." },
                    { new Guid("9ce69fbc-f56f-a571-2104-1955da8f8ccd"), "en", new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "VAT number on the invoice differs from the supplier's" },
                    { new Guid("9f846c51-e181-2eeb-aaea-fbdd2d6fb7ae"), "ar", new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "هذا المصروف لغيرك." },
                    { new Guid("a313c2bf-1e85-873a-4b19-41aad6bbb75f"), "en", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "No expense without an invoice under this item." },
                    { new Guid("a923e1fd-9c91-e68f-9719-4f3675d1d172"), "en", new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"), "The invoice's total is not valid." },
                    { new Guid("ac0aa3f0-4aae-d942-c831-1bf8a847ed88"), "en", new Guid("db920181-5bba-2173-698f-310a22fe706d"), "This could not be done." },
                    { new Guid("b10d821b-3ca9-8e02-f22c-0bfc2a145db7"), "en", new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "Attach to the chosen expense" },
                    { new Guid("b11e1995-d39e-e358-f91e-bedc7c1a2511"), "ar", new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "تعذرت قراءة المصروفات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("b1ff9c33-75f4-5ed0-52f5-feb521768b24"), "en", new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "The file was not found. Share it again." },
                    { new Guid("b62d510d-1bc8-8dc8-8ca5-d19c3296fd86"), "ar", new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "أُرفقت الفاتورة." },
                    { new Guid("b77e71a1-02d2-c068-6796-b8be0b29316a"), "en", new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "This file is already on an expense." },
                    { new Guid("b784bf3d-ca05-0a9f-f3d4-d5ed4dd8023b"), "en", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "Expense" },
                    { new Guid("b9c4ad48-52e9-cdc2-0e51-076189c925f0"), "en", new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "New expense with this invoice" },
                    { new Guid("bf4b3ecf-eddc-3231-37fc-0aa8b7cea2ac"), "ar", new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "الرقم الضريبي في الفاتورة ليس لهذا المورد." },
                    { new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"), "ar", new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"), "اكتب رقم الفاتورة." },
                    { new Guid("cbc3d0de-8d6e-8d92-0d41-0edd92fc3eff"), "en", new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "Invoice total" },
                    { new Guid("d371833c-264b-9a90-6e92-8146b718c4d3"), "en", new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "Could not read the expenses. Check the connection and try again." },
                    { new Guid("d4fc8ea8-0edd-9c7c-6013-587236e7e06b"), "en", new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "This expense cannot take an invoice." },
                    { new Guid("d7796efd-3ec8-2eaa-9984-fa9f5818e73f"), "en", new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "Project" },
                    { new Guid("e9a26130-4e4f-727a-5cec-51a4a34372d1"), "ar", new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "هذه الفاتورة مرفقة بمصروف آخر." },
                    { new Guid("ea726598-7799-0795-c17b-6bee405d7d9c"), "en", new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "The VAT number on this invoice cannot be read." },
                    { new Guid("f04af644-87a0-d63b-7d77-e34d0a797e5d"), "ar", new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "مصروف جديد بهذه الفاتورة" },
                    { new Guid("f0df95d1-3505-f60d-bf82-2b16a41f17c5"), "ar", new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"), "تعذرت قراءة الرقم الضريبي في هذه الفاتورة." },
                    { new Guid("f458f59b-4adf-eeb0-c7dc-8a849dd358bb"), "en", new Guid("c66a23da-16f7-380b-3333-12807981e0ca"), "This invoice is already on another expense." },
                    { new Guid("fa34bf44-118e-0109-822e-87769e4a9f1c"), "ar", new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "هذا المصروف معتمد. الإرفاق يعيده إلى الاعتماد." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d97dfe4-9d43-bd8a-36a3-10fa2f97fc6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24787303-6dcc-7ffa-2fab-61938696a525"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28469952-9edd-366a-04dc-327698d9bda4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29050156-82f7-1eae-7a5c-2cbe0b75973e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38a21b97-90ba-d6bd-7aa2-f2439749b1b4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b8d0e13-2dee-42dd-9262-626de90992ec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f56e07d-596d-ef3f-827a-f27a040975cf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40a32818-66b6-8438-813f-65754333fd20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42f25b7f-49cb-97b4-3e1b-271cfc3b2498"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("43658527-3e52-7bb6-c8f9-7b6fa0f71bb6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44e6dde1-cc1d-635a-b8b7-4ed2c1f66101"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("538c09d8-5500-f6bc-777c-307b54eb3edc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5968a925-f1cf-617b-95d3-002099b5d26e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("59c429f7-b163-b2bc-6eb1-7ae5ab5d9edd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b4be8c6-59f1-7da9-4a93-8eb7f224c147"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5db81562-0e7e-9605-0453-6db25836ace8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62289094-f771-e3c7-d0b6-d12a800243df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6531f12b-15dc-fb39-d3a8-777e72fb20ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6769ad18-ba6b-1d75-48b3-b9f952eafe9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68d3dfdf-8118-7a8c-49b9-fcb433f4533a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6bacabe1-5d51-8314-c738-7229c9956a02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73774f15-36cb-daba-df7e-e30ff27ce97c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("788309bd-1fd5-2e34-ff21-c505c5269e49"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("804a5fd6-855f-8499-9de1-3c6790f551c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85d83d9a-c79b-105e-575e-1c2bce433b68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a6d4c58-ac90-1435-4cb6-fa22329dfbe0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c565c7c-4d57-4277-c947-c689982936c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ebf05b7-1607-da43-d933-797d6592d256"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f90319f-8ea2-22e1-c4cf-ebda60acc97c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("989de814-a48f-8539-d5ca-dba88255d90a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ce69fbc-f56f-a571-2104-1955da8f8ccd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f846c51-e181-2eeb-aaea-fbdd2d6fb7ae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a313c2bf-1e85-873a-4b19-41aad6bbb75f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a923e1fd-9c91-e68f-9719-4f3675d1d172"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac0aa3f0-4aae-d942-c831-1bf8a847ed88"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b10d821b-3ca9-8e02-f22c-0bfc2a145db7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b11e1995-d39e-e358-f91e-bedc7c1a2511"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1ff9c33-75f4-5ed0-52f5-feb521768b24"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b62d510d-1bc8-8dc8-8ca5-d19c3296fd86"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b77e71a1-02d2-c068-6796-b8be0b29316a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b784bf3d-ca05-0a9f-f3d4-d5ed4dd8023b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b9c4ad48-52e9-cdc2-0e51-076189c925f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf4b3ecf-eddc-3231-37fc-0aa8b7cea2ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5f504de-55e1-85b2-62f9-b10a34461abe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cbc3d0de-8d6e-8d92-0d41-0edd92fc3eff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d371833c-264b-9a90-6e92-8146b718c4d3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d4fc8ea8-0edd-9c7c-6013-587236e7e06b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7796efd-3ec8-2eaa-9984-fa9f5818e73f"));

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
                keyValue: new Guid("f04af644-87a0-d63b-7d77-e34d0a797e5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0df95d1-3505-f60d-bf82-2b16a41f17c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f458f59b-4adf-eeb0-c7dc-8a849dd358bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa34bf44-118e-0109-822e-87769e4a9f1c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21722db6-09db-e46d-376b-05fe398dc252"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4741ac22-8f9f-5c75-e37f-fde844d0149c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("900ce495-cca9-546f-36f6-c787eaaaf79d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"));

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
                keyValue: new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("db920181-5bba-2173-698f-310a22fe706d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f485b7b5-ba1f-693e-f534-5e6933c75122"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"));

            migrationBuilder.DropColumn(
                name: "SellerVatNumber",
                table: "Invoices");
        }
    }
}
