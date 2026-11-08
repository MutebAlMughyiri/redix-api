using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OneTextPerMovementAndNoUnusedWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1afd9746-e907-358a-5fae-91a359b89317"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("218355fe-589d-59e8-14e8-bdb6facad513"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("398ea309-4580-473b-3c60-cbce927e681b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41f27a78-58c3-df90-d339-925afc1d55e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f352c7a-7a79-ad94-a0d2-19f1401d6b6a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b6934ed-7b02-e171-3a94-f3b3441512e7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d19dbbc-dbc0-28b5-dc87-25b064a2625a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61dbb54d-4f76-9f0f-91bd-202a3a229424"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("639193ec-14d7-6da1-88dd-1407f44034b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a6a10fb-bec1-098f-9a58-2a347e251b53"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b4ef07d-b414-eca8-8b5d-07c364aa01df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("768b19cc-aea5-4e1a-dc73-518a39d7fa4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80da283b-b8da-4c71-5b15-1864ad122c35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82796d89-8715-48eb-b8b4-22e21b09a550"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88992b82-bacc-3e21-1bf2-fa10bb524339"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1e43600-0b8a-ad08-cdf4-e936acb3290e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8408a74-7d35-32f5-aa53-b270071b5410"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b9851c93-1378-b029-25f2-03f1e9fdb0ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0efb2c9-d9cb-f88d-5771-dae59a5cc617"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5c9d567-f42c-4f75-1691-5ddfb6d34464"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d651bece-29d9-3cf5-0be1-443eefcb462f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e62cb5fe-0240-4708-0dd2-309159d1014a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eedc95a7-38d9-4a61-dc8e-a6c254ad7634"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f89d53a5-983b-a25c-20fb-15d6fef8578c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("62d78811-d09f-1739-d022-2abea4b7f972"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b12efad9-a4d3-575e-31f4-780b64154463"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"));

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CapitalMovementDetails");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "", "categories.capitalItemExists" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1c714eed-e194-96c3-172c-b29084aeb4d5"), "ar", new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "لهذا المشروع بند رأس المال بالفعل." },
                    { new Guid("9331a3a3-33e8-6f93-aabb-6e1f1ea8c92d"), "en", new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "This project already has its capital item." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c714eed-e194-96c3-172c-b29084aeb4d5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9331a3a3-33e8-6f93-aabb-6e1f1ea8c92d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CapitalMovementDetails",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "", "reports.noContracts" },
                    { new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "", "reports.lastIssued" },
                    { new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "", "approvals.failedSome" },
                    { new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "", "revenue.vatRefund" },
                    { new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "", "reports.cost" },
                    { new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "", "reports.chooseItem" },
                    { new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "", "approvals.selectHint" },
                    { new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "", "expenses.duplicateTitle" },
                    { new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "", "approvals.approvedSome" },
                    { new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "", "reports.earlier" },
                    { new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "", "expenses.duplicateExplain" },
                    { new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "", "revenue.sale" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1afd9746-e907-358a-5fae-91a359b89317"), "en", new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "Cost" },
                    { new Guid("218355fe-589d-59e8-14e8-bdb6facad513"), "ar", new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "تم الاعتماد" },
                    { new Guid("398ea309-4580-473b-3c60-cbce927e681b"), "ar", new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "بيع" },
                    { new Guid("41f27a78-58c3-df90-d339-925afc1d55e6"), "ar", new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "استرجاع ضريبة" },
                    { new Guid("4f352c7a-7a79-ad94-a0d2-19f1401d6b6a"), "ar", new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "التكلفة" },
                    { new Guid("5b6934ed-7b02-e171-3a94-f3b3441512e7"), "en", new Guid("f96fc7bd-523a-4538-96c1-e9a2cc5e1aba"), "Sale" },
                    { new Guid("5d19dbbc-dbc0-28b5-dc87-25b064a2625a"), "en", new Guid("b12efad9-a4d3-575e-31f4-780b64154463"), "Approved" },
                    { new Guid("61dbb54d-4f76-9f0f-91bd-202a3a229424"), "ar", new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "اختر البند" },
                    { new Guid("639193ec-14d7-6da1-88dd-1407f44034b3"), "en", new Guid("5ca3ee2f-8613-cb66-9d7d-d1fbe27f7eb2"), "VAT refund" },
                    { new Guid("6a6a10fb-bec1-098f-9a58-2a347e251b53"), "en", new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "Choose the item" },
                    { new Guid("6b4ef07d-b414-eca8-8b5d-07c364aa01df"), "en", new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "Last issued" },
                    { new Guid("768b19cc-aea5-4e1a-dc73-518a39d7fa4e"), "ar", new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "آخر إصدار" },
                    { new Guid("80da283b-b8da-4c71-5b15-1864ad122c35"), "ar", new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "يبدو أن هذه الفاتورة مُدخلة مسبقاً" },
                    { new Guid("82796d89-8715-48eb-b8b4-22e21b09a550"), "en", new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "Could not be approved" },
                    { new Guid("88992b82-bacc-3e21-1bf2-fa10bb524339"), "ar", new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "نفس المورد ورقم الفاتورة موجودان في القيد السابق أعلاه. احفظ فقط إن كانت فاتورة مختلفة." },
                    { new Guid("b1e43600-0b8a-ad08-cdf4-e936acb3290e"), "ar", new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "التقارير السابقة" },
                    { new Guid("b8408a74-7d35-32f5-aa53-b270071b5410"), "en", new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "Earlier reports" },
                    { new Guid("b9851c93-1378-b029-25f2-03f1e9fdb0ef"), "ar", new Guid("06979a48-314b-bc29-bc0d-ff7057f30b5c"), "تعذر اعتماده" },
                    { new Guid("c0efb2c9-d9cb-f88d-5771-dae59a5cc617"), "en", new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "No contracts yet." },
                    { new Guid("d5c9d567-f42c-4f75-1691-5ddfb6d34464"), "en", new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "Select entries to approve together" },
                    { new Guid("d651bece-29d9-3cf5-0be1-443eefcb462f"), "ar", new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "لا عقود بعد." },
                    { new Guid("e62cb5fe-0240-4708-0dd2-309159d1014a"), "ar", new Guid("705a7775-5e8e-f3d9-e897-93e451e8d827"), "حدّد قيوداً للاعتماد الجماعي" },
                    { new Guid("eedc95a7-38d9-4a61-dc8e-a6c254ad7634"), "en", new Guid("7b60f9ee-6402-b5f4-bfc6-07353d499dbe"), "This invoice looks entered already" },
                    { new Guid("f89d53a5-983b-a25c-20fb-15d6fef8578c"), "en", new Guid("d47cd49a-1a6a-7da3-9bed-39818b30f783"), "The same supplier and invoice number exist in the earlier entry above. Save it only if this is a different invoice." }
                });
        }
    }
}
