using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateHiddenWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07ca8f05-3adf-959a-8805-fe9984c4873f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65c87a54-3324-2b56-95a4-3d61e1ecf589"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "", "expenses.duplicateHidden" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("07ca8f05-3adf-959a-8805-fe9984c4873f"), "en", new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "The same supplier and invoice number exist in a project you have no access to. Save it only if this is a different invoice." },
                    { new Guid("65c87a54-3324-2b56-95a4-3d61e1ecf589"), "ar", new Guid("6820867f-f714-d5b1-fd69-f7959cd9da74"), "نفس المورد ورقم الفاتورة موجودان في مشروع لا تملك صلاحية الوصول إليه. احفظ فقط إن كانت فاتورة مختلفة." }
                });
        }
    }
}
