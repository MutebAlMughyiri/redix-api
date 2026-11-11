using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SearchWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "", "entries.kind" },
                    { new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "", "common.searchIn" },
                    { new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "", "entries.status" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("02152fcc-f2d4-300a-e52e-8948b647b0dd"), "ar", new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "النوع" },
                    { new Guid("09472c98-9f15-bdb9-1965-9ab8fd80ad40"), "en", new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "Search {0}" },
                    { new Guid("1f26116e-ec23-1c04-fe2d-fd8d4157b7a2"), "ar", new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "الحالة" },
                    { new Guid("56961017-ea49-5af0-a1c2-6260eee19d0e"), "en", new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "Status" },
                    { new Guid("d268dba9-834d-f034-f2e4-cf910a79ee59"), "en", new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "Kind" },
                    { new Guid("e3398233-b521-bba9-a5a2-05fd743d420c"), "ar", new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "ابحث في {0}" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02152fcc-f2d4-300a-e52e-8948b647b0dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09472c98-9f15-bdb9-1965-9ab8fd80ad40"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f26116e-ec23-1c04-fe2d-fd8d4157b7a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56961017-ea49-5af0-a1c2-6260eee19d0e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d268dba9-834d-f034-f2e4-cf910a79ee59"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e3398233-b521-bba9-a5a2-05fd743d420c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"));
        }
    }
}
