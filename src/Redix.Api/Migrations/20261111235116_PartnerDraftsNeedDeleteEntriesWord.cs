using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PartnerDraftsNeedDeleteEntriesWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "", "partners.myDraftsNeedDeleteEntries" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("ab39459a-caa2-fbf3-5060-86259feda8a0"), "en", new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "Your drafts name them, and deleting those needs Delete entries." },
                    { new Guid("fae7bcdb-f6d4-c5da-8b34-12361e353a10"), "ar", new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "مسوداتك تذكره، وحذفها يحتاج صلاحية حذف القيود." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ab39459a-caa2-fbf3-5060-86259feda8a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fae7bcdb-f6d4-c5da-8b34-12361e353a10"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"));
        }
    }
}
