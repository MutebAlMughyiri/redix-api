using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OfflineWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("084575ec-a8f0-3e07-ae65-8cc66b51bf1e"), "", "offline.discardQuestion" },
                    { new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "", "offline.waiting" },
                    { new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "", "offline.empty" },
                    { new Guid("76ac5b37-e17a-f6fb-bef7-a8b1287d7231"), "", "offline.discardExplain" },
                    { new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "", "offline.sendAgain" },
                    { new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "", "offline.title" },
                    { new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "", "expenses.keptOnPhone" },
                    { new Guid("f2a4897f-3f7b-e84c-25d6-f2aaff87e1df"), "", "offline.discard" },
                    { new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "", "offline.sendAnyway" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("04301270-828b-102c-3467-c07ef8c4bdf1"), "ar", new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "حُفظ على هذا الجهاز، ويُرسل عند عودة الشبكة." },
                    { new Guid("1178de5d-f4f2-5ede-ad0a-6b1efd151989"), "ar", new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "لا شيء بانتظار المزامنة." },
                    { new Guid("2189999a-26ec-0e91-c40c-2bd7013a6b4a"), "ar", new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "إرسال رغم ذلك" },
                    { new Guid("29815d63-2083-1ef2-b576-afd54f6bffab"), "ar", new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "إعادة الإرسال" },
                    { new Guid("311694a2-2026-d939-49cc-dfcd8d502270"), "ar", new Guid("f2a4897f-3f7b-e84c-25d6-f2aaff87e1df"), "حذف" },
                    { new Guid("65618f07-639c-16da-2294-965a110f51c8"), "en", new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "Send again" },
                    { new Guid("6e85d605-5f1c-41da-e2b0-48490e8d8250"), "en", new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "Send anyway" },
                    { new Guid("737f1238-3111-f960-e7b7-221b60e4afba"), "en", new Guid("f2a4897f-3f7b-e84c-25d6-f2aaff87e1df"), "Delete" },
                    { new Guid("85bd0522-9c77-0872-6759-b8b1d3e1428b"), "en", new Guid("76ac5b37-e17a-f6fb-bef7-a8b1287d7231"), "It was never sent, so it will be gone for good." },
                    { new Guid("97d9b24d-2d25-2676-84ab-6d91f4feabc8"), "en", new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "Waiting to sync" },
                    { new Guid("a144a294-df3c-6231-b06d-e572e6bad7d3"), "ar", new Guid("084575ec-a8f0-3e07-ae65-8cc66b51bf1e"), "حذف هذا القيد من الجهاز؟" },
                    { new Guid("a1f915e5-7b4b-d8f5-38f6-481e06ad5e94"), "ar", new Guid("76ac5b37-e17a-f6fb-bef7-a8b1287d7231"), "لم يُرسل قط، فسيذهب نهائياً." },
                    { new Guid("a899736c-4db2-7a95-9ee7-48468b544825"), "en", new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "Nothing is waiting." },
                    { new Guid("b0f17453-e313-2166-78b6-a5a32e5e3b00"), "en", new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "Saved on this phone. It will be sent when the network returns." },
                    { new Guid("baf1facc-15a9-ec26-6ec0-142223e611f5"), "en", new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "Waiting for the network" },
                    { new Guid("dd28f535-f3d8-84ba-a0b5-0a8440423208"), "ar", new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"), "بانتظار الشبكة" },
                    { new Guid("e5e5629f-598f-db97-8e0d-aa2760cddd95"), "ar", new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "بانتظار المزامنة" },
                    { new Guid("f29e53dc-b0e2-08a4-863a-862138c9cf3c"), "en", new Guid("084575ec-a8f0-3e07-ae65-8cc66b51bf1e"), "Delete this entry from the phone?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04301270-828b-102c-3467-c07ef8c4bdf1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1178de5d-f4f2-5ede-ad0a-6b1efd151989"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2189999a-26ec-0e91-c40c-2bd7013a6b4a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29815d63-2083-1ef2-b576-afd54f6bffab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("311694a2-2026-d939-49cc-dfcd8d502270"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65618f07-639c-16da-2294-965a110f51c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6e85d605-5f1c-41da-e2b0-48490e8d8250"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("737f1238-3111-f960-e7b7-221b60e4afba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85bd0522-9c77-0872-6759-b8b1d3e1428b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97d9b24d-2d25-2676-84ab-6d91f4feabc8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a144a294-df3c-6231-b06d-e572e6bad7d3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a1f915e5-7b4b-d8f5-38f6-481e06ad5e94"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a899736c-4db2-7a95-9ee7-48468b544825"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0f17453-e313-2166-78b6-a5a32e5e3b00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("baf1facc-15a9-ec26-6ec0-142223e611f5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd28f535-f3d8-84ba-a0b5-0a8440423208"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5e5629f-598f-db97-8e0d-aa2760cddd95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f29e53dc-b0e2-08a4-863a-862138c9cf3c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("084575ec-a8f0-3e07-ae65-8cc66b51bf1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31201cdc-11c9-15b7-8319-28b1f71a6c67"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("76ac5b37-e17a-f6fb-bef7-a8b1287d7231"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f2a4897f-3f7b-e84c-25d6-f2aaff87e1df"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"));
        }
    }
}
