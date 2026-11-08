using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryPageWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("390837e8-d1ce-72ae-45a3-5069c30c99d1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dfcd76a2-953b-6b21-d692-0069970cbfed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f2375ed9-4ece-1db5-01ba-d3b8070e1310"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "", "entries.view" },
                    { new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "", "entries.notYours" },
                    { new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "", "entries.approvedCannotChange" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"),
                column: "Value",
                value: "حذف");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"),
                column: "Value",
                value: "Remove");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1159240c-afc5-71b4-acd8-65dbd6445c3c"), "ar", new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "قيد" },
                    { new Guid("68ea08a0-2f03-84b6-83dc-c12f10efe48f"), "en", new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "Entry" },
                    { new Guid("7a2f08fa-fe87-fe62-b1b1-c028ad316f93"), "en", new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "Only whoever entered it, or a manager, changes it." },
                    { new Guid("7d1b4f10-f58e-d845-d981-2a2a8ec9cfd0"), "en", new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "An approved entry does not change." },
                    { new Guid("d5d0dd62-a7ae-8094-514b-86c95f46999b"), "ar", new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "القيد المعتمد لا يتغيّر." },
                    { new Guid("fa50bd92-9f48-51ed-2903-db0574483b1a"), "ar", new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "يغيّره من أدخله أو المدير فقط." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1159240c-afc5-71b4-acd8-65dbd6445c3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68ea08a0-2f03-84b6-83dc-c12f10efe48f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a2f08fa-fe87-fe62-b1b1-c028ad316f93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d1b4f10-f58e-d845-d981-2a2a8ec9cfd0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5d0dd62-a7ae-8094-514b-86c95f46999b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa50bd92-9f48-51ed-2903-db0574483b1a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("f2375ed9-4ece-1db5-01ba-d3b8070e1310"), "", "swipe.hint" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"),
                column: "Value",
                value: "عكس القيد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"),
                column: "Value",
                value: "Reverse");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("390837e8-d1ce-72ae-45a3-5069c30c99d1"), "ar", new Guid("f2375ed9-4ece-1db5-01ba-d3b8070e1310"), "اسحب السطر لحذفه" },
                    { new Guid("dfcd76a2-953b-6b21-d692-0069970cbfed"), "en", new Guid("f2375ed9-4ece-1db5-01ba-d3b8070e1310"), "Swipe a row to remove it" }
                });
        }
    }
}
