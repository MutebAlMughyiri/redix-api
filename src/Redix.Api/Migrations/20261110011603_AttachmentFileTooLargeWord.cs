using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentFileTooLargeWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "", "attachments.fileTooLarge" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("4538616f-59a3-cf02-5dc7-44e579d9c65c"), "en", new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "A file must be smaller than 25 MB." },
                    { new Guid("e46aa106-94d9-1082-7dd1-b7cd04c860ba"), "ar", new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "يجب أن يكون الملف أقل من 25 ميجابايت." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4538616f-59a3-cf02-5dc7-44e579d9c65c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e46aa106-94d9-1082-7dd1-b7cd04c860ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"));
        }
    }
}
