using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OnePaymentDatePerExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("33da3f4e-3b46-9511-9b44-823e41d0a288"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6168034f-99eb-5f12-333a-1a9cc2057e84"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6eed0024-7660-1862-1b0b-04620a0a1c8a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7362e7d5-1e75-db2f-a9c5-aeb170f5aba9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82fb32a3-d7b6-6047-2289-8461dee7459d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a79de21-2d68-ea09-dab8-7587dc173831"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97f65854-495a-9aa0-316b-e3eb8f7f1700"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac478550-e87f-db96-6a74-cda8f49eccbd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c118522f-5873-dd12-6e69-9dd875544b89"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f5000d02-b4c6-1fde-1f9a-b17edf5da1a2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"));

            migrationBuilder.DropColumn(
                name: "BankFees",
                table: "ExpenseDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bank fees typed before this migration are not brought back: only a backup taken before it has them.
            migrationBuilder.AddColumn<decimal>(
                name: "BankFees",
                table: "ExpenseDetails",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "", "expenses.invalidPaymentAmount" },
                    { new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "", "expenses.paymentAmount" },
                    { new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "", "expenses.invalidBankFees" },
                    { new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "", "expenses.bankFees" },
                    { new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "", "expenses.paymentDiffers" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("33da3f4e-3b46-9511-9b44-823e41d0a288"), "ar", new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "الدفع يختلف عن الفاتورة" },
                    { new Guid("6168034f-99eb-5f12-333a-1a9cc2057e84"), "en", new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "Enter bank fees of zero or more, with at most two decimals." },
                    { new Guid("6eed0024-7660-1862-1b0b-04620a0a1c8a"), "ar", new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "أدخل المبلغ المدفوع، بخانتين عشريتين كحد أقصى." },
                    { new Guid("7362e7d5-1e75-db2f-a9c5-aeb170f5aba9"), "ar", new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "رسوم بنكية" },
                    { new Guid("82fb32a3-d7b6-6047-2289-8461dee7459d"), "en", new Guid("318c1e7b-c0ec-4def-53d8-fae8cd46c56e"), "Enter the amount paid, with at most two decimals." },
                    { new Guid("8a79de21-2d68-ea09-dab8-7587dc173831"), "ar", new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "المبلغ المدفوع" },
                    { new Guid("97f65854-495a-9aa0-316b-e3eb8f7f1700"), "ar", new Guid("75d05756-a223-4e00-4d44-08e85c749ce2"), "أدخل رسوماً بنكية صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("ac478550-e87f-db96-6a74-cda8f49eccbd"), "en", new Guid("a3da815d-fa5c-9317-c319-7d7c45f2619b"), "Bank fees" },
                    { new Guid("c118522f-5873-dd12-6e69-9dd875544b89"), "en", new Guid("ff093f4b-41d2-37dc-03d6-323291864bd3"), "The payment differs from the invoice" },
                    { new Guid("f5000d02-b4c6-1fde-1f9a-b17edf5da1a2"), "en", new Guid("44e256b4-f64b-09db-e5aa-d3c79a63ad7a"), "Amount paid" }
                });
        }
    }
}
