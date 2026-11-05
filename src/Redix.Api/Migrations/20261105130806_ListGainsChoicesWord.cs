using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ListGainsChoicesWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "", "columns.newChoices" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("6e8565c5-7d06-7747-3f2f-f31b32de6009"), "ar", new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "خيارات جديدة، كل خيار في سطر. وتبقى الخيارات أعلاه لأن القيود تحملها." },
                    { new Guid("cc1e4606-7762-e2bc-66f5-0224b550e1d0"), "en", new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "New choices, one on each line. Those above stay, because entries hold them." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6e8565c5-7d06-7747-3f2f-f31b32de6009"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc1e4606-7762-e2bc-66f5-0224b550e1d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"));
        }
    }
}
