using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OnlyAddedAccountsPayAnExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: new Guid("c1112bdf-cdf2-e0c5-efd6-9b34a0c26c18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c01fce5-3d51-8aa0-2a5c-a50452f4252e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("21ba1404-a7f0-25d2-d982-e500627dad9e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ed1793f-4d04-1d7c-3010-a9f805f79d32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("468bdf08-324e-e5b1-02aa-834e4fad5745"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("543184d3-ee5b-cf4f-a6f5-138fd410f649"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("905a100a-a19a-d676-d359-f4925baf4cd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92dbd782-4b36-fc8e-72ca-c99499fe348c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acf71d90-fd30-2fc7-42d9-d0590588bf22"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c511fa6f-7451-64e6-3eda-51a76fc8b563"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8908226-2e4b-3e46-01ad-6660928b3556"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eca259fc-c4b5-87ce-1c0b-864dd60f2ad4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc50eeb5-a80f-20eb-413d-6259a1e1c88d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"));

            migrationBuilder.DropColumn(
                name: "PartnerTreatment",
                table: "ExpenseDetails");

            // Every project's own copy of سداد goes too; one an expense was paid with stays for that expense, disabled (redix-api 96, D25).
            migrationBuilder.Sql("DELETE FROM PaymentMethods WHERE Name = N'سداد' AND Id NOT IN (SELECT PaymentMethodId FROM ExpenseDetails)");
            migrationBuilder.Sql("UPDATE PaymentMethods SET IsActive = 0 WHERE Name = N'سداد'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // What each partner-paid expense was treated as, and each project's own سداد, are not brought back: only a backup taken before this migration has them.
            migrationBuilder.AddColumn<byte>(
                name: "PartnerTreatment",
                table: "ExpenseDetails",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[] { new Guid("c1112bdf-cdf2-e0c5-efd6-9b34a0c26c18"), true, "سداد", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 3 });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "", "approvals.addToCapital" },
                    { new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "", "approvals.paidBy" },
                    { new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "", "approvals.owedBack" },
                    { new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "", "expenses.paymentSource" },
                    { new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "", "approvals.invalidTreatment" },
                    { new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "", "approvals.notPaidByPartner" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0c01fce5-3d51-8aa0-2a5c-a50452f4252e"), "en", new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "Add to capital" },
                    { new Guid("21ba1404-a7f0-25d2-d982-e500627dad9e"), "en", new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "Choose what becomes of it." },
                    { new Guid("2ed1793f-4d04-1d7c-3010-a9f805f79d32"), "en", new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "No partner paid this." },
                    { new Guid("468bdf08-324e-e5b1-02aa-834e4fad5745"), "en", new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "Payment source" },
                    { new Guid("543184d3-ee5b-cf4f-a6f5-138fd410f649"), "ar", new Guid("1691bde4-ce40-e8af-94c4-29b36ba82263"), "يُضاف إلى رأس المال" },
                    { new Guid("905a100a-a19a-d676-d359-f4925baf4cd8"), "ar", new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "مستحق له" },
                    { new Guid("92dbd782-4b36-fc8e-72ca-c99499fe348c"), "en", new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "Paid by" },
                    { new Guid("acf71d90-fd30-2fc7-42d9-d0590588bf22"), "en", new Guid("87a76df0-b568-62a1-1c52-6861cb5ced94"), "Owed back" },
                    { new Guid("c511fa6f-7451-64e6-3eda-51a76fc8b563"), "ar", new Guid("ddd9dc40-dd7b-761b-0768-0d5cbcd55821"), "اختر ما يترتب عليه." },
                    { new Guid("d8908226-2e4b-3e46-01ad-6660928b3556"), "ar", new Guid("9fc7231e-3919-cb42-5bd5-703d0dafd9bf"), "مصدر الدفع" },
                    { new Guid("eca259fc-c4b5-87ce-1c0b-864dd60f2ad4"), "ar", new Guid("2ede21ba-9b50-aad9-9df0-48059892c06b"), "دفعه" },
                    { new Guid("fc50eeb5-a80f-20eb-413d-6259a1e1c88d"), "ar", new Guid("f95556a3-c419-6ae3-2fa2-2e0e58b3c093"), "لم يدفعه شريك." }
                });
        }
    }
}
