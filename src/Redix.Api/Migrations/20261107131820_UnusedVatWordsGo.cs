using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnusedVatWordsGo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0036f2ac-e636-e1a9-99a6-ebdeb1224b44"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("202a2d50-cdcb-cd4d-086e-f1ce3d63fc95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a054c59-58c5-1358-cf5b-aed2093d13a8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("341fa924-18e3-e734-2ca0-1953306bd34e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a5f23e4-87b0-56c2-8d21-98be1c46bbfc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63125c9a-c8fb-5092-006c-60ec095262b6"));

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
                keyValue: new Guid("81237a0a-e485-782c-0da1-b9a4a62c04fb"));

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
                keyValue: new Guid("e0aec402-0dda-07fa-caaf-9d7bd5b0ab0d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e896cd4e-d02a-83e3-a289-8a34554dae6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc631c02-8883-0c28-6d20-2cc72c681ada"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"));

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
                keyValue: new Guid("afcce444-d00b-85f9-85f2-852634ce1728"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "", "reports.pending" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("67b5df64-8570-2aa0-0696-63cc08ff7b6d"), "ar", new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "بانتظار الاعتماد" },
                    { new Guid("8a386dd3-8b1f-feae-32b6-30aea7a2b47a"), "en", new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "Waiting for approval" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("67b5df64-8570-2aa0-0696-63cc08ff7b6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a386dd3-8b1f-feae-32b6-30aea7a2b47a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "", "vat.refused" },
                    { new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "", "vat.invalidPeriod" },
                    { new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "", "vat.loadFailed" },
                    { new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "", "vat.noConnection" },
                    { new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "", "vat.invalidVatNumber" },
                    { new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "", "vat.total" },
                    { new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "", "vat.pending" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0036f2ac-e636-e1a9-99a6-ebdeb1224b44"), "en", new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "VAT number not valid" },
                    { new Guid("202a2d50-cdcb-cd4d-086e-f1ce3d63fc95"), "en", new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "The period ends before it starts." },
                    { new Guid("2a054c59-58c5-1358-cf5b-aed2093d13a8"), "en", new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "This could not be done." },
                    { new Guid("341fa924-18e3-e734-2ca0-1953306bd34e"), "ar", new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "تعذرت قراءة الكشف. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("5a5f23e4-87b0-56c2-8d21-98be1c46bbfc"), "ar", new Guid("08bea4b0-6159-8e5c-fa42-2a4ff53513c1"), "تعذر تنفيذ ذلك." },
                    { new Guid("63125c9a-c8fb-5092-006c-60ec095262b6"), "ar", new Guid("d175e84f-4ef7-2b76-97c8-7fc8383de28f"), "رقم ضريبي غير صالح" },
                    { new Guid("7664423d-1a88-c5fe-5063-7445fe6e7242"), "ar", new Guid("2389d8e5-7b86-4f28-7db7-33b931b6404b"), "تنتهي الفترة قبل أن تبدأ." },
                    { new Guid("76a464a0-6ae2-83f5-a205-9eb30fa0d8c4"), "ar", new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("81237a0a-e485-782c-0da1-b9a4a62c04fb"), "en", new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "Total" },
                    { new Guid("c7d586bc-3958-7626-5afe-cd8e0e5d9975"), "en", new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "Waiting for approval" },
                    { new Guid("c9250933-3897-e2c8-2082-c633b0f0a512"), "en", new Guid("258b0b75-9233-a0ba-b600-7f66adfe2336"), "Could not read the statement. Check the connection and try again." },
                    { new Guid("e0aec402-0dda-07fa-caaf-9d7bd5b0ab0d"), "ar", new Guid("e278bd3a-a0f9-b4fd-d439-25ea6affe354"), "الإجمالي" },
                    { new Guid("e896cd4e-d02a-83e3-a289-8a34554dae6d"), "ar", new Guid("f9b5d9e0-9b3c-0091-d7b8-3862c10ae960"), "بانتظار الاعتماد" },
                    { new Guid("fc631c02-8883-0c28-6d20-2cc72c681ada"), "en", new Guid("afcce444-d00b-85f9-85f2-852634ce1728"), "No connection to the server. Try again." }
                });
        }
    }
}
