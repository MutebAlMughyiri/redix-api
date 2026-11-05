using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnitNameTakenWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "", "projects.unitNameTaken" },
                    { new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "", "register.unitNameTaken" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("19b938d8-3205-1c4f-42b2-f49d3fd0923d"), "en", new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "A unit of this project already has this name." },
                    { new Guid("219c0ede-cfac-b34f-2a5b-2dfbc66e31ca"), "ar", new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "توجد وحدة بهذا الاسم في هذا المشروع." },
                    { new Guid("dda76acf-8e8a-0661-d7e1-c84317b3d395"), "en", new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "Two units cannot have the same name." },
                    { new Guid("e83097b1-5f6a-96e5-d929-147658347f21"), "ar", new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "لا يصح أن تحمل وحدتان الاسم نفسه." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("19b938d8-3205-1c4f-42b2-f49d3fd0923d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("219c0ede-cfac-b34f-2a5b-2dfbc66e31ca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dda76acf-8e8a-0661-d7e1-c84317b3d395"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e83097b1-5f6a-96e5-d929-147658347f21"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"));
        }
    }
}
