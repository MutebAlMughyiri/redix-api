using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ItemHeldByContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "", "categories.itemHasContract" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("3ac93d3f-8a62-cde3-9bf2-803c92011b99"), "en", new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "A contract names this item, so its type cannot change." },
                    { new Guid("a8e5cd8b-4ec2-0c35-933c-4dcfe0aa0dfb"), "ar", new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "عقد يذكر هذا البند، فلا يتغيّر نوعه." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3ac93d3f-8a62-cde3-9bf2-803c92011b99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8e5cd8b-4ec2-0c35-933c-4dcfe0aa0dfb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("be9ac772-02a4-1393-7189-495e3103dfad"));
        }
    }
}
