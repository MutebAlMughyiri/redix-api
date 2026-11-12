using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentFileEmptyWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "", "attachments.fileEmpty" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("84b082ac-44dd-dd3e-1bda-41c632d65cf4"), "ar", new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "الملف فارغ. اختر ملفاً آخر." },
                    { new Guid("ea197b2d-f1ec-396b-c690-1cf5abc648fe"), "en", new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "That file is empty. Pick another." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("84b082ac-44dd-dd3e-1bda-41c632d65cf4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea197b2d-f1ec-396b-c690-1cf5abc648fe"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"));
        }
    }
}
