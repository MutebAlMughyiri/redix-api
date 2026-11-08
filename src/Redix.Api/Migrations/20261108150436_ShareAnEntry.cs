using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ShareAnEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "", "entryShare.item" },
                    { new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "", "entryShare.partner" },
                    { new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "", "approvals.showAll" },
                    { new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "", "entryShare.supplier" },
                    { new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "", "entryShare.author" },
                    { new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "", "entryShare.account" },
                    { new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "", "entryShare.amount" },
                    { new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "", "entryShare.contractor" },
                    { new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "", "entries.notFound" },
                    { new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "", "entryShare.paymentMethod" },
                    { new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "", "entryShare.notes" },
                    { new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "", "entryShare.status" },
                    { new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "", "entries.share" },
                    { new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "", "entryShare.date" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("017a7088-2510-cf18-f303-7c913dfdff72"), "ar", new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "الحساب" },
                    { new Guid("088a74a6-b6ac-6ea6-c0a6-1b1170ca3c57"), "en", new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "Share" },
                    { new Guid("10b6922b-86f9-57d7-cbc7-53b5bf7ef7bf"), "ar", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "ملاحظات" },
                    { new Guid("12625a28-c749-d22e-8953-0439c90d7e8d"), "ar", new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "البند" },
                    { new Guid("25d53029-97f7-fc40-34c8-aab53e019a6a"), "en", new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "Show everything waiting" },
                    { new Guid("2e4e68bb-e544-1020-5b36-75b89607dba9"), "en", new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "Entered by" },
                    { new Guid("36ec9ba4-e1bb-1782-3014-5944541d9262"), "ar", new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "عرض كل ما ينتظر" },
                    { new Guid("45df6f33-3423-3174-92a3-8f0b8badad1f"), "en", new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "Account" },
                    { new Guid("49b79303-ffb5-f8a2-aa1a-fed2994c4c57"), "ar", new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "تعذّر العثور على هذا القيد: ربما حُذف، أو هو في مشروع لست فيه." },
                    { new Guid("4a48352c-d1ce-e8bc-5fe2-a97dc6759744"), "ar", new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "الشريك" },
                    { new Guid("67feda3a-4efa-983d-c8d9-1c0eaf5a01b6"), "en", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "Notes" },
                    { new Guid("749a7613-2e98-7878-e198-2e17e5f384f1"), "en", new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "Partner" },
                    { new Guid("759d5049-bb47-77d4-c446-9278ce4f8123"), "ar", new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "المقاول" },
                    { new Guid("89ebe5ba-1a6e-f3df-6e4c-403d2fed261d"), "en", new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "This entry could not be found: it may have been deleted, or it is in a project you are not in." },
                    { new Guid("8bb75904-7efa-680a-8051-903dfbf18b1d"), "en", new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "Contractor" },
                    { new Guid("9866dab7-5227-ade8-af4e-f2e51ebc52c6"), "ar", new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "أدخله" },
                    { new Guid("b36bbb66-29c0-88e0-83bd-0f3d35e06844"), "ar", new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "التاريخ" },
                    { new Guid("b66f44e7-feb6-d98e-0458-d4ea0162537c"), "en", new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "Date" },
                    { new Guid("b917dc50-1777-e7e0-56c3-84a3a693e355"), "ar", new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "مشاركة" },
                    { new Guid("bfaea1c6-4c42-8aa1-abf4-e15d28ef1535"), "en", new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "Payment method" },
                    { new Guid("c027e59b-44bd-490e-557f-e1beae6980a3"), "ar", new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "المبلغ" },
                    { new Guid("d00a0545-3aa7-4db1-bfc5-9418a5a548f0"), "ar", new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "المورد" },
                    { new Guid("d0f3663b-c7de-8580-ab10-8f6a0201da8e"), "en", new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "Item" },
                    { new Guid("e2b5213e-d00c-171a-f71e-edfdef4804fe"), "en", new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "Supplier" },
                    { new Guid("e9f4f448-07d6-a616-b57d-ce81a31bd3f0"), "en", new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "Amount" },
                    { new Guid("ed84fea8-e269-239d-d688-0f9a9d87e678"), "en", new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "Status" },
                    { new Guid("ee1ca999-68a1-5ab9-e3ca-20b296d00172"), "ar", new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "الحالة" },
                    { new Guid("fa040b2d-e38d-f90e-557c-9d4311521cd5"), "ar", new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "طريقة الدفع" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("017a7088-2510-cf18-f303-7c913dfdff72"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("088a74a6-b6ac-6ea6-c0a6-1b1170ca3c57"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("10b6922b-86f9-57d7-cbc7-53b5bf7ef7bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12625a28-c749-d22e-8953-0439c90d7e8d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25d53029-97f7-fc40-34c8-aab53e019a6a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e4e68bb-e544-1020-5b36-75b89607dba9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36ec9ba4-e1bb-1782-3014-5944541d9262"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("45df6f33-3423-3174-92a3-8f0b8badad1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49b79303-ffb5-f8a2-aa1a-fed2994c4c57"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a48352c-d1ce-e8bc-5fe2-a97dc6759744"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("67feda3a-4efa-983d-c8d9-1c0eaf5a01b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("749a7613-2e98-7878-e198-2e17e5f384f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("759d5049-bb47-77d4-c446-9278ce4f8123"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89ebe5ba-1a6e-f3df-6e4c-403d2fed261d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8bb75904-7efa-680a-8051-903dfbf18b1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9866dab7-5227-ade8-af4e-f2e51ebc52c6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b36bbb66-29c0-88e0-83bd-0f3d35e06844"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b66f44e7-feb6-d98e-0458-d4ea0162537c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b917dc50-1777-e7e0-56c3-84a3a693e355"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bfaea1c6-4c42-8aa1-abf4-e15d28ef1535"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c027e59b-44bd-490e-557f-e1beae6980a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d00a0545-3aa7-4db1-bfc5-9418a5a548f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0f3663b-c7de-8580-ab10-8f6a0201da8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2b5213e-d00c-171a-f71e-edfdef4804fe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9f4f448-07d6-a616-b57d-ce81a31bd3f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ed84fea8-e269-239d-d688-0f9a9d87e678"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee1ca999-68a1-5ab9-e3ca-20b296d00172"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa040b2d-e38d-f90e-557c-9d4311521cd5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6717342a-504e-3687-226a-0638210dd5f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b3300888-6503-ff69-d52e-dd2f55114352"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"));
        }
    }
}
