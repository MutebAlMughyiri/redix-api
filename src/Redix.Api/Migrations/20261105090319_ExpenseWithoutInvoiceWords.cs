using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpenseWithoutInvoiceWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "", "expenses.invoiceAndReason" },
                    { new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "", "expenses.withoutInvoiceReason" },
                    { new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "", "expenses.amount" },
                    { new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "", "expenses.withoutInvoice" },
                    { new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "", "expenses.invalidWithoutInvoiceReason" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0918cfaa-584a-1cd7-2336-631ea3e9744f"), "ar", new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "بدون فاتورة" },
                    { new Guid("40228410-a508-c207-207f-3bbd2306fe47"), "en", new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "An expense without an invoice carries no invoice file." },
                    { new Guid("4215fd4f-12ab-106b-5bea-9edc6c8505e6"), "ar", new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "سبب عدم وجود فاتورة" },
                    { new Guid("5eaf9d86-54a9-f83a-77e8-069c3663096a"), "ar", new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "المبلغ" },
                    { new Guid("92e4c872-996f-de60-d154-9da571ab8691"), "ar", new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "المصروف بدون فاتورة لا يُرفق به ملف فاتورة." },
                    { new Guid("a6d03bd0-29a0-e796-bfd0-c7c8da86a3a5"), "en", new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "Without an invoice" },
                    { new Guid("a71d72c3-6858-04ec-553f-194aabd202ce"), "en", new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "Say why there is no invoice, in up to 500 characters." },
                    { new Guid("bdd98fb3-a3a8-2eb1-fabe-3b726242c402"), "en", new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "Why there is no invoice" },
                    { new Guid("d0fe5a27-c910-a240-1021-4bc47430f116"), "ar", new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "اذكر سبب عدم وجود فاتورة، حتى 500 حرف." },
                    { new Guid("e89b9bc6-73cc-48d5-fbbc-79baa7352dae"), "en", new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "Amount" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0918cfaa-584a-1cd7-2336-631ea3e9744f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40228410-a508-c207-207f-3bbd2306fe47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4215fd4f-12ab-106b-5bea-9edc6c8505e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5eaf9d86-54a9-f83a-77e8-069c3663096a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92e4c872-996f-de60-d154-9da571ab8691"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a6d03bd0-29a0-e796-bfd0-c7c8da86a3a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a71d72c3-6858-04ec-553f-194aabd202ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bdd98fb3-a3a8-2eb1-fabe-3b726242c402"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0fe5a27-c910-a240-1021-4bc47430f116"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e89b9bc6-73cc-48d5-fbbc-79baa7352dae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"));
        }
    }
}
