using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectFileReportWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("12a68918-2406-6e91-9142-a6672951b7a0"), "", "projects.loadFileDoneNoReport" },
                    { new Guid("ce437593-153a-4b4d-3f66-33f9be258484"), "", "projects.loadFile.reportAlreadyIssued" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("079f3d90-f5e8-3597-e353-4e18e31bdac9"), "ar", new Guid("12a68918-2406-6e91-9142-a6672951b7a0"), "حُمّل الملف: {0} قيدًا في {1} جدولًا، والأرصدة الفعلية." },
                    { new Guid("160ca3d8-f279-1fb7-6939-c72228b9d2dc"), "en", new Guid("ce437593-153a-4b4d-3f66-33f9be258484"), "Report {0} was already issued in this project." },
                    { new Guid("3f596cb1-cf0b-6116-c0c5-079aa1214339"), "ar", new Guid("ce437593-153a-4b4d-3f66-33f9be258484"), "التقرير {0} صدر في هذا المشروع من قبل." },
                    { new Guid("6ac2f5ef-86ff-f206-61cf-dad21a590595"), "en", new Guid("12a68918-2406-6e91-9142-a6672951b7a0"), "File loaded: {0} entries in {1} tables, and the actual balances." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("079f3d90-f5e8-3597-e353-4e18e31bdac9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("160ca3d8-f279-1fb7-6939-c72228b9d2dc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f596cb1-cf0b-6116-c0c5-079aa1214339"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6ac2f5ef-86ff-f206-61cf-dad21a590595"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12a68918-2406-6e91-9142-a6672951b7a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ce437593-153a-4b4d-3f66-33f9be258484"));
        }
    }
}
