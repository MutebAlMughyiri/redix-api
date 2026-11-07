using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CompletionIsComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c903942-d4fb-fcf1-0db7-aeeb446cc075"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("108a9b9d-ad74-14d8-4e85-4dbc192fb791"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1473b04d-8954-569c-b771-953dbf7b2930"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("626d0a87-530c-3d94-54b0-67ff5f17fd22"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7b2407f3-7db4-f958-76ff-e6fc4218b48f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93433267-b4c8-f691-4f0b-6d126f8fafe7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("966b4a96-2eb3-5460-2714-aae81b49465f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a0e3ef5-bbe8-c872-ad04-77208f74bbac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c574008-128a-9bf4-a055-13697eefbb93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c598c497-473c-93ec-0ac6-a20be7918b72"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d59eaa11-d0b8-238b-ff49-9ec23de9389b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d86f484a-4a46-6ebc-bb1a-63bc188663f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e578855c-1e0f-0d35-05f8-b74185534636"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0fb6378-fa35-2388-1599-e2031a182022"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("50f26485-a173-102e-1d66-878f59f53c42"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"));

            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                table: "Projects");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fb0788c-2336-f27e-6aa9-6f16ffeab7a7"),
                column: "Value",
                value: "اسمه ووحداته ومستنداته، وإغلاقه.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1214456-32e9-8dc0-3d56-d7c1d8275c2e"),
                column: "Value",
                value: "Its name, units and documents, and closing it.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The completion each project had typed is not brought back: only a backup taken before this migration has it.
            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPercent",
                table: "Projects",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "CompletionPercent",
                value: 0m);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "", "dashboard.projectFinished" },
                    { new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "", "dashboard.save" },
                    { new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "", "dashboard.percent" },
                    { new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "", "dashboard.noConnection" },
                    { new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "", "dashboard.setCompletion" },
                    { new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "", "dashboard.refused" },
                    { new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "", "dashboard.invalidCompletion" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fb0788c-2336-f27e-6aa9-6f16ffeab7a7"),
                column: "Value",
                value: "اسمه ووحداته ومستنداته ونسبة إنجازه، وإغلاقه.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1214456-32e9-8dc0-3d56-d7c1d8275c2e"),
                column: "Value",
                value: "Its name, units, documents and completion, and closing it.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0c903942-d4fb-fcf1-0db7-aeeb446cc075"), "ar", new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "حفظ" },
                    { new Guid("108a9b9d-ad74-14d8-4e85-4dbc192fb791"), "en", new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "This project is closed." },
                    { new Guid("1473b04d-8954-569c-b771-953dbf7b2930"), "en", new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "Set completion" },
                    { new Guid("626d0a87-530c-3d94-54b0-67ff5f17fd22"), "en", new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "Save" },
                    { new Guid("7b2407f3-7db4-f958-76ff-e6fc4218b48f"), "en", new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "Percentage" },
                    { new Guid("93433267-b4c8-f691-4f0b-6d126f8fafe7"), "ar", new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "هذا المشروع مغلق." },
                    { new Guid("966b4a96-2eb3-5460-2714-aae81b49465f"), "en", new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "This could not be done." },
                    { new Guid("9a0e3ef5-bbe8-c872-ad04-77208f74bbac"), "ar", new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "اكتب نسبة من 0 إلى 100." },
                    { new Guid("9c574008-128a-9bf4-a055-13697eefbb93"), "en", new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "No connection to the server. Try again." },
                    { new Guid("c598c497-473c-93ec-0ac6-a20be7918b72"), "en", new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "Type a percentage from 0 to 100." },
                    { new Guid("d59eaa11-d0b8-238b-ff49-9ec23de9389b"), "ar", new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "تحديث نسبة الإنجاز" },
                    { new Guid("d86f484a-4a46-6ebc-bb1a-63bc188663f8"), "ar", new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("e578855c-1e0f-0d35-05f8-b74185534636"), "ar", new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "النسبة" },
                    { new Guid("f0fb6378-fa35-2388-1599-e2031a182022"), "ar", new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "تعذر تنفيذ ذلك." }
                });
        }
    }
}
