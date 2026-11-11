using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PartnerDraftsWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "", "partners.deleteRemovesMyDrafts" },
                    { new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "", "partners.draftsBy" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("41862f4c-661b-5d95-c629-8f00a0937b4f"), "en", new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "A draft by {0} names them." },
                    { new Guid("4278aa6c-1d4e-bef1-c377-5695d3ef30b9"), "en", new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "Your drafts naming them are deleted with them:" },
                    { new Guid("6e82f6c8-d992-41a4-0633-34ad24225097"), "ar", new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "وتُحذف معه مسوداتك التي تذكره:" },
                    { new Guid("ea134a38-26b0-5b1f-83a7-aa03b4db147b"), "ar", new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "مسودة لـ{0} تذكره." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41862f4c-661b-5d95-c629-8f00a0937b4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4278aa6c-1d4e-bef1-c377-5695d3ef30b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6e82f6c8-d992-41a4-0633-34ad24225097"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea134a38-26b0-5b1f-83a7-aa03b4db147b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"));
        }
    }
}
