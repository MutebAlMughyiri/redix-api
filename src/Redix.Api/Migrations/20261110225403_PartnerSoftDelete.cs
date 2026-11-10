using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PartnerSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Partners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "", "partners.delete" },
                    { new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "", "partners.deleted" },
                    { new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "", "partners.deleteQuestion" },
                    { new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "", "partners.deleteKeepsMovements" },
                    { new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "", "partners.partnerNotDeletable" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("05eb65ed-3eb2-9dc3-0625-17d9e0491794"), "ar", new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "يختفي من الشركاء ومن إضافة قيد، وتبقى حركاته في السجلات والتقارير." },
                    { new Guid("46f448aa-8ba6-ee59-f620-17e6b19a2ed0"), "ar", new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "حذف الشريك؟" },
                    { new Guid("53792625-fd5b-2d21-30ef-290f67988a2a"), "ar", new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "تم حذف الشريك." },
                    { new Guid("797a2634-a804-c5e8-d043-001d135ddebf"), "en", new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "A partner is deleted only at zero capital with nothing waiting." },
                    { new Guid("879ba48c-03e4-2bb8-59a5-1c18d126ce1b"), "ar", new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "حذف" },
                    { new Guid("8e45b5bb-7f01-5d36-875a-1d3ab5deef47"), "en", new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "Delete" },
                    { new Guid("9057b474-7571-8890-57a4-5013e0327e68"), "en", new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "Delete the partner?" },
                    { new Guid("90edf415-ebb0-6eb2-34c0-da83a9a2427f"), "en", new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "Partner deleted." },
                    { new Guid("f845e027-dfd4-22b0-b7c6-d43587f7d7e4"), "en", new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "They leave Partners and Add entry; their movements stay in the books and reports." },
                    { new Guid("fa6b554a-91eb-31c5-6c93-9d6f1b9d273c"), "ar", new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "لا يُحذف الشريك إلا ورأس ماله صفر ولا ينتظر له قيد." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] <> '00000000-0000-0000-0000-000000000000' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("05eb65ed-3eb2-9dc3-0625-17d9e0491794"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46f448aa-8ba6-ee59-f620-17e6b19a2ed0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("53792625-fd5b-2d21-30ef-290f67988a2a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("797a2634-a804-c5e8-d043-001d135ddebf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("879ba48c-03e4-2bb8-59a5-1c18d126ce1b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e45b5bb-7f01-5d36-875a-1d3ab5deef47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9057b474-7571-8890-57a4-5013e0327e68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90edf415-ebb0-6eb2-34c0-da83a9a2427f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f845e027-dfd4-22b0-b7c6-d43587f7d7e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa6b554a-91eb-31c5-6c93-9d6f1b9d273c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("08357229-1958-81f5-3bac-8309b59996a4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"));

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Partners");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] <> '00000000-0000-0000-0000-000000000000'");
        }
    }
}
