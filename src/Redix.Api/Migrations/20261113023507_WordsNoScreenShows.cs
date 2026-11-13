using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class WordsNoScreenShows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04eb0792-159c-a993-c916-898766c448be"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1119f52f-d29e-839b-f71e-694ac237f060"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1e4b5bb0-edc6-c704-8c95-42f19d64fcb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2102fe1f-64c3-cf99-10e9-8c1eccac3852"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41405262-df7c-3c8b-fdfb-e8e631c21de3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4cac6532-1c33-9bcd-0505-068fbc63c665"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("864be42c-280f-d6f1-123c-a95722be794f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8759e2d3-541a-56b4-0e4d-3818a7d4f732"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("881d39af-4af9-584b-f8fe-5bed589c9968"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9011182f-5ced-e079-387c-6366fcea47dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f8f0047-8a7c-db10-e0e5-ebf700fa47e5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a772dd27-8908-b6ee-b335-5eb9f457a10c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1854c01-8eff-bb0e-f8eb-2c31556ef60f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b21d00f0-e6ac-0cc1-cc81-7eedd631dcda"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bffad5a3-ae8b-237c-d9c2-102afc62a0c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2073572-67a8-6929-8913-d87672d3b3ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c65ff10d-e85f-0923-a7c8-2f7ae4855757"));

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
                keyValue: new Guid("d5ca4622-0846-9b0f-90b6-005f52f01989"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d96f8ded-1abb-c561-23c3-f6ae7a577b0f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db4a6104-5e3b-5410-c4ed-29390ae2aabb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df49099d-4369-3ac2-87af-0ac47c1b4b8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee66f33a-25fc-f1a1-591a-e601c2910591"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8c98edc-6929-0fee-95b7-20cd2b6150cd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18bd135b-d31c-b651-a588-29f349aeef05"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "", "reports.outsideContract" },
                    { new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "", "expenses.product" },
                    { new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "", "reports.invoiceNumber" },
                    { new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "", "expenses.invalidQuantity" },
                    { new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "", "expenses.creditNoteExplain" },
                    { new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "", "expenses.creditNote" },
                    { new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "", "reports.quantity" },
                    { new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "", "entries.withdrawalHint" },
                    { new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "", "expenses.quantity" },
                    { new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "", "expenses.invalidUnitPrice" },
                    { new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "", "reports.unitPrice" },
                    { new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "", "expenses.unitPrice" },
                    { new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "", "expenses.invalidProduct" },
                    { new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "", "entries.withdrawal" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("04eb0792-159c-a993-c916-898766c448be"), "ar", new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "يسحب الشريك هذا المبلغ من رأس ماله" },
                    { new Guid("1119f52f-d29e-839b-f71e-694ac237f060"), "en", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "Unit price" },
                    { new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"), "en", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "Recorded as its own row with a minus amount" },
                    { new Guid("1e4b5bb0-edc6-c704-8c95-42f19d64fcb8"), "en", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "Product" },
                    { new Guid("2102fe1f-64c3-cf99-10e9-8c1eccac3852"), "ar", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "رقم الفاتورة" },
                    { new Guid("41405262-df7c-3c8b-fdfb-e8e631c21de3"), "ar", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "اسم الصنف طويل جداً." },
                    { new Guid("4cac6532-1c33-9bcd-0505-068fbc63c665"), "en", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "Outside the contract" },
                    { new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"), "ar", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "يُسجَّل صفاً مستقلاً بمبلغ سالب" },
                    { new Guid("864be42c-280f-d6f1-123c-a95722be794f"), "ar", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "سعر الوحدة" },
                    { new Guid("8759e2d3-541a-56b4-0e4d-3818a7d4f732"), "ar", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "مرتجع (إشعار دائن)" },
                    { new Guid("881d39af-4af9-584b-f8fe-5bed589c9968"), "en", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "Enter a quantity of zero or more, with at most two decimals." },
                    { new Guid("9011182f-5ced-e079-387c-6366fcea47dd"), "ar", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "أدخل عدداً صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"), "en", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "Quantity" },
                    { new Guid("9f8f0047-8a7c-db10-e0e5-ebf700fa47e5"), "ar", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "العدد" },
                    { new Guid("a772dd27-8908-b6ee-b335-5eb9f457a10c"), "en", new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "The partner takes this amount out of their capital" },
                    { new Guid("b1854c01-8eff-bb0e-f8eb-2c31556ef60f"), "ar", new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "سحب" },
                    { new Guid("b21d00f0-e6ac-0cc1-cc81-7eedd631dcda"), "en", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "Invoice no." },
                    { new Guid("bffad5a3-ae8b-237c-d9c2-102afc62a0c0"), "en", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "Quantity" },
                    { new Guid("c2073572-67a8-6929-8913-d87672d3b3ba"), "en", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "Unit price" },
                    { new Guid("c65ff10d-e85f-0923-a7c8-2f7ae4855757"), "en", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "Credit note" },
                    { new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"), "ar", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "العدد" },
                    { new Guid("cab370f6-b4cc-4094-42cb-9196f7e26070"), "en", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "Enter a unit price of zero or more, with at most two decimals." },
                    { new Guid("d5ca4622-0846-9b0f-90b6-005f52f01989"), "ar", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "الصنف" },
                    { new Guid("d96f8ded-1abb-c561-23c3-f6ae7a577b0f"), "en", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "The product name is too long." },
                    { new Guid("db4a6104-5e3b-5410-c4ed-29390ae2aabb"), "ar", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "سعر الحبة" },
                    { new Guid("df49099d-4369-3ac2-87af-0ac47c1b4b8f"), "ar", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "أدخل سعر وحدة صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("ee66f33a-25fc-f1a1-591a-e601c2910591"), "ar", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "خارج العقد" },
                    { new Guid("f8c98edc-6929-0fee-95b7-20cd2b6150cd"), "en", new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "Withdrawal" }
                });
        }
    }
}
