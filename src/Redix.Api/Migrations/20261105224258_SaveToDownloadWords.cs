using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SaveToDownloadWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "", "reports.downloaded" },
                    { new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "", "reports.save" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2160835e-82fb-6d29-e008-01dfd0e5d412"), "ar", new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "حفظ في التنزيلات" },
                    { new Guid("5dc3589c-a39c-5097-5ff2-a7991c592b6f"), "en", new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "Saved in the phone's Download folder." },
                    { new Guid("70d7bb76-225d-0a11-df22-f67cc426495b"), "ar", new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "تم الحفظ في مجلد التنزيلات في الهاتف." },
                    { new Guid("88604375-6641-00e0-c3a3-001883a46a1f"), "en", new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "Save to Download" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2160835e-82fb-6d29-e008-01dfd0e5d412"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5dc3589c-a39c-5097-5ff2-a7991c592b6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("70d7bb76-225d-0a11-df22-f67cc426495b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88604375-6641-00e0-c3a3-001883a46a1f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"));
        }
    }
}
