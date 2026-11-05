using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "", "reports.title" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("34cb8af5-110f-9768-fcd3-fccba97ad946"), "en", new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "Reports" },
                    { new Guid("b1cdc609-141d-be64-30cb-05a2c5b15e2d"), "ar", new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "التقارير" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("34cb8af5-110f-9768-fcd3-fccba97ad946"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1cdc609-141d-be64-30cb-05a2c5b15e2d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"));
        }
    }
}
