using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PartnerHistoryWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "", "history.allPartners" },
                    { new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "", "history.capitalOut" },
                    { new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "", "history.partnerDeleted" },
                    { new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "", "history.partnerAccount" },
                    { new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "", "partners.history" },
                    { new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "", "history.unknownPartner" },
                    { new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "", "history.capitalIn" },
                    { new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "", "history.partnerChanged" },
                    { new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "", "history.partnerCreated" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("15de3ae3-f14a-ee73-b0dc-b196a00cfa88"), "ar", new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "رأس مال {0} من {1} إلى {2}" },
                    { new Guid("16fcbcfc-20ab-15a0-4bdb-61e594e17745"), "en", new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "Changed the partner {0}: {1}" },
                    { new Guid("47b7d4c5-b8b2-0602-891c-e8747f5e994e"), "ar", new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "هذا الشريك ليس في هذا المشروع." },
                    { new Guid("480cde1f-ec1c-df55-65ad-d0857a68ec85"), "en", new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "Added the partner {0}" },
                    { new Guid("562b0891-0800-e684-ec3b-0f528948a91e"), "ar", new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "حذف الشريك {0}" },
                    { new Guid("5652f47e-a4ff-58bf-1bb7-0b5f1d251317"), "ar", new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "تعديل الشريك {0}: {1}" },
                    { new Guid("5ba06fc6-a02a-810b-bb75-852e3e2b6bb6"), "en", new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "Deleted the partner {0}" },
                    { new Guid("6612c2a8-4db5-ff66-b871-060ac4ca758d"), "en", new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "History" },
                    { new Guid("802fa457-18b6-ef8f-835a-4dc814b65498"), "en", new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "Withdrew {0} for {1} from {2}" },
                    { new Guid("88cd47f4-c279-ebe2-9b64-360b63587623"), "en", new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "All partners" },
                    { new Guid("890aae98-e1fa-05b6-c938-b7f516ec44f8"), "en", new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "Capital {0} from {1} into {2}" },
                    { new Guid("8cc9437d-8965-496d-3359-273f355fc542"), "en", new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "The partner {0}'s account: {1}" },
                    { new Guid("8d166f86-6476-b963-2ada-3bb6c4bba42a"), "ar", new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "السجل" },
                    { new Guid("96803f30-457b-d836-4a29-eebe739af674"), "ar", new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "إضافة الشريك {0}" },
                    { new Guid("a0e20d03-0801-d147-1f92-f084e7d197c0"), "ar", new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"), "كل الشركاء" },
                    { new Guid("b50efa0c-f73b-3a6d-1e99-8b79c73ff267"), "en", new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "That partner is not in this project." },
                    { new Guid("c4182e16-8b04-06eb-d26c-a55114c0c2b4"), "ar", new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "حساب الشريك {0}: {1}" },
                    { new Guid("d7794096-77d7-6539-5d2f-14ec8917f148"), "ar", new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "سحب {0} لـ{1} من {2}" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15de3ae3-f14a-ee73-b0dc-b196a00cfa88"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("16fcbcfc-20ab-15a0-4bdb-61e594e17745"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("47b7d4c5-b8b2-0602-891c-e8747f5e994e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("480cde1f-ec1c-df55-65ad-d0857a68ec85"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("562b0891-0800-e684-ec3b-0f528948a91e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5652f47e-a4ff-58bf-1bb7-0b5f1d251317"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ba06fc6-a02a-810b-bb75-852e3e2b6bb6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6612c2a8-4db5-ff66-b871-060ac4ca758d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("802fa457-18b6-ef8f-835a-4dc814b65498"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88cd47f4-c279-ebe2-9b64-360b63587623"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("890aae98-e1fa-05b6-c938-b7f516ec44f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8cc9437d-8965-496d-3359-273f355fc542"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8d166f86-6476-b963-2ada-3bb6c4bba42a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("96803f30-457b-d836-4a29-eebe739af674"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0e20d03-0801-d147-1f92-f084e7d197c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b50efa0c-f73b-3a6d-1e99-8b79c73ff267"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4182e16-8b04-06eb-d26c-a55114c0c2b4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7794096-77d7-6539-5d2f-14ec8917f148"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e1e4791-4f58-d359-c0b2-9b81a680e8ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("696720c5-0c62-2420-70d4-04467ae4060c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af34908b-a227-3b20-f64c-5789425cc04e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"));
        }
    }
}
