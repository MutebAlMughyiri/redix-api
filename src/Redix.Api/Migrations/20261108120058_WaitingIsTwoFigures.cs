using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class WaitingIsTwoFigures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("67b5df64-8570-2aa0-0696-63cc08ff7b6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a386dd3-8b1f-feae-32b6-30aea7a2b47a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "", "reports.pendingRevenue" },
                    { new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "", "reports.pendingExpenses" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("815055e7-277d-551f-4ac2-04c96bf001fd"), "en", new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "Expenses waiting for approval" },
                    { new Guid("acb692b0-9dbe-3ff4-501e-370319a4f298"), "en", new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "Revenue and capital waiting for approval" },
                    { new Guid("ba97a245-f1fa-b26e-5326-f194dd39bc50"), "ar", new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "مصروفات بانتظار الاعتماد" },
                    { new Guid("f8bb37f4-3968-5312-e1aa-58d9f1a9e1e1"), "ar", new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "إيرادات ورأس مال بانتظار الاعتماد" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("815055e7-277d-551f-4ac2-04c96bf001fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acb692b0-9dbe-3ff4-501e-370319a4f298"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba97a245-f1fa-b26e-5326-f194dd39bc50"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8bb37f4-3968-5312-e1aa-58d9f1a9e1e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "", "reports.pending" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("67b5df64-8570-2aa0-0696-63cc08ff7b6d"), "ar", new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "بانتظار الاعتماد" },
                    { new Guid("8a386dd3-8b1f-feae-32b6-30aea7a2b47a"), "en", new Guid("105a61fb-48e6-9370-ee7a-ef3defca2301"), "Waiting for approval" }
                });
        }
    }
}
