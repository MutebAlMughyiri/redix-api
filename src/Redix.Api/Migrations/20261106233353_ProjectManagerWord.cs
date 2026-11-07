using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectManagerWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "", "projects.manager" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("4d5fff76-1477-81dc-d9f5-5b5565a11806"), "ar", new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "مدير المشروع" },
                    { new Guid("5e73fbae-4e0c-47d7-7488-38202dc6bf36"), "en", new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "Project manager" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d5fff76-1477-81dc-d9f5-5b5565a11806"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5e73fbae-4e0c-47d7-7488-38202dc6bf36"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"));
        }
    }
}
