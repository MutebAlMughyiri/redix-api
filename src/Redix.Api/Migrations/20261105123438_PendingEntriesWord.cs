using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PendingEntriesWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "", "projects.pendingEntries" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2ab4d5c5-614e-362d-d207-fbb7c5a34bdc"), "ar", new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "في هذا المشروع قيود بانتظار الاعتماد. اعتمدها أو ارفضها أولاً." },
                    { new Guid("a39f0718-fbab-9144-900c-5272495dc673"), "en", new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "Entries in this project are waiting for approval. Approve or reject them first." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ab4d5c5-614e-362d-d207-fbb7c5a34bdc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a39f0718-fbab-9144-900c-5272495dc673"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"));
        }
    }
}
