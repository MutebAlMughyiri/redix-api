using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoMatchWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20cc0dfa-08a1-7a9c-7b86-d4ba5e5f3e3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41ce2cd4-bd69-986e-f46f-738db04bab78"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "", "users.noMatch" },
                    { new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "", "categories.noTableMatch" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2cb2b3b6-de2c-891d-1a09-8f21e8a6e737"), "ar", new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "لا يوجد جدول مطابق." },
                    { new Guid("71e4cc35-36cc-62ee-f963-39d00a94175d"), "ar", new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "لا يوجد مستخدم مطابق." },
                    { new Guid("d783a220-203d-52d7-daeb-bfe3e3437318"), "en", new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "No user matches." },
                    { new Guid("df20d423-6470-a0ec-1caf-cb2aa3d86f60"), "en", new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "No table matches." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2cb2b3b6-de2c-891d-1a09-8f21e8a6e737"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71e4cc35-36cc-62ee-f963-39d00a94175d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d783a220-203d-52d7-daeb-bfe3e3437318"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df20d423-6470-a0ec-1caf-cb2aa3d86f60"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "", "entries.found" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("20cc0dfa-08a1-7a9c-7b86-d4ba5e5f3e3c"), "ar", new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "{0} قيد" },
                    { new Guid("41ce2cd4-bd69-986e-f46f-738db04bab78"), "en", new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "{0} entries" }
                });
        }
    }
}
