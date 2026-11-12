using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalHistoryWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1354ddba-a368-af89-4f5d-ef95c9b077c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6db13d7-0a0d-c42d-e0dc-cd5cf80bf1ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "", "history.entryApproved" },
                    { new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "", "history.entryRejected" },
                    { new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "", "approvals.history" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1a4c4f3e-3fe2-7d5d-794e-5ea391d9be98"), "ar", new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "السجل" },
                    { new Guid("267604cc-9d02-d756-d785-7fd2e3b2d03a"), "ar", new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "اعتماد: {0}" },
                    { new Guid("54b7566e-df19-911a-587d-3fec3321a9f1"), "en", new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "Rejected: {0}, because {1}" },
                    { new Guid("591e93bf-4d7d-4960-163d-c20d7eb72311"), "en", new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "Approved: {0}" },
                    { new Guid("9d7feb18-5b2e-8d12-1792-05188d3f865d"), "ar", new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "رفض: {0}، والسبب: {1}" },
                    { new Guid("a0cd70d9-d4a3-c98a-eb0e-1ea41f8086ca"), "en", new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "History" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a4c4f3e-3fe2-7d5d-794e-5ea391d9be98"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("267604cc-9d02-d756-d785-7fd2e3b2d03a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54b7566e-df19-911a-587d-3fec3321a9f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("591e93bf-4d7d-4960-163d-c20d7eb72311"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d7feb18-5b2e-8d12-1792-05188d3f865d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0cd70d9-d4a3-c98a-eb0e-1ea41f8086ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "", "reports.revenueAndExpenses" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1354ddba-a368-af89-4f5d-ef95c9b077c2"), "en", new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "Revenue and expenses" },
                    { new Guid("f6db13d7-0a0d-c42d-e0dc-cd5cf80bf1ba"), "ar", new Guid("154538a6-9cb8-c1a1-f668-ff761eaa4c86"), "الإيرادات والمصروفات" }
                });
        }
    }
}
