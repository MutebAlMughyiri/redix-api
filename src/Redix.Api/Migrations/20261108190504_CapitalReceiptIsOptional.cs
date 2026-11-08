using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CapitalReceiptIsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A capital movement's file is optional like every entry's (Muteb, 8 Nov 2026), so "paid in cash" and its reason go; a reason already typed joins the entry's notes.
            migrationBuilder.Sql("""
                UPDATE entry SET Notes = LEFT(CASE WHEN entry.Notes = N'' THEN details.CashReason ELSE entry.Notes + N' · ' + details.CashReason END, 1000)
                FROM Entries entry JOIN CapitalMovementDetails details ON details.EntryId = entry.Id
                WHERE details.CashReason <> N'';
                """);

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("05da0418-2ef1-8e52-1fd3-ececb22e393c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ff16692-1a30-8b9e-7760-46a40d30c6b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2a92db36-c3f4-4a8e-e087-56044b39adc2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b4ba849-7fe1-ff6b-964d-491b9e8c990c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e9a14ad-4748-d7ef-1723-cd3be17fdb01"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f565137-3037-3eea-e282-54342b0f3fc5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a30ce9d-805d-235d-f7b8-44dd6fec981c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a61a8f1-9f82-5227-2223-ecff83997d51"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f5f83b3-7d16-a2de-28ef-a9f4cd7e34f7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64ffce39-7460-8b8e-946c-49d4db39d129"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ed71510-912d-aa33-4e50-cad2744fbbe0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("898a76ff-8007-4504-53f7-e245883f1c73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8f0476b-4232-76d7-9823-ee7e84360d10"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d3f316f8-3a18-12dd-b233-9eb443e6ecb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d43eed7b-fbc5-9495-b344-9a70fc5eb7bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2a795fe-66f5-a3bf-a45a-6e443ddbc04f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9e9eb5b-7a7a-158d-14c8-5b9cddf9fd80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f022bfd7-dee9-65fd-a3a6-e275b4f1b722"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0a351e62-d993-89df-8883-2186f5541dd0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"));

            migrationBuilder.DropColumn(
                name: "CashReason",
                table: "CapitalMovementDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CashReason",
                table: "CapitalMovementDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "", "capital.receiptDiffers" },
                    { new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "", "capital.receiptAmount" },
                    { new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "", "capital.invalidReason" },
                    { new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "", "capital.receiptRequired" },
                    { new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "", "capital.receipt" },
                    { new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "", "capital.cashReason" },
                    { new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "", "capital.receiptRequiredWord" },
                    { new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "", "capital.paidInCash" },
                    { new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "", "capital.cash" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("05da0418-2ef1-8e52-1fd3-ececb22e393c"), "en", new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "Bank transfer receipt" },
                    { new Guid("1ff16692-1a30-8b9e-7760-46a40d30c6b6"), "ar", new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "المبلغ في الإيصال يخالف هذه الحركة." },
                    { new Guid("2a92db36-c3f4-4a8e-e087-56044b39adc2"), "ar", new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "نقداً" },
                    { new Guid("2b4ba849-7fe1-ff6b-964d-491b9e8c990c"), "ar", new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "دُفع نقداً بلا إيصال" },
                    { new Guid("2e9a14ad-4748-d7ef-1723-cd3be17fdb01"), "en", new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "Why" },
                    { new Guid("3f565137-3037-3eea-e282-54342b0f3fc5"), "en", new Guid("ecc38ac2-207f-bc8b-ad39-9cc445b4177b"), "Cash" },
                    { new Guid("4a30ce9d-805d-235d-f7b8-44dd6fec981c"), "en", new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "Attach the bank receipt, or say why it was paid in cash." },
                    { new Guid("4a61a8f1-9f82-5227-2223-ecff83997d51"), "ar", new Guid("b49a01cf-0619-9ade-fe19-4a99fd2e4388"), "السبب" },
                    { new Guid("5f5f83b3-7d16-a2de-28ef-a9f4cd7e34f7"), "ar", new Guid("7b9eb655-00de-97d2-bee3-e081cec24c8e"), "أرفق الإيصال البنكي، أو اذكر سبب الدفع نقداً." },
                    { new Guid("64ffce39-7460-8b8e-946c-49d4db39d129"), "en", new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "Amount on the receipt" },
                    { new Guid("7ed71510-912d-aa33-4e50-cad2744fbbe0"), "ar", new Guid("45e403c0-39c2-dcb5-9adf-378c85a35da0"), "المبلغ في الإيصال" },
                    { new Guid("898a76ff-8007-4504-53f7-e245883f1c73"), "en", new Guid("0a351e62-d993-89df-8883-2186f5541dd0"), "The amount on the receipt differs from this movement." },
                    { new Guid("b8f0476b-4232-76d7-9823-ee7e84360d10"), "ar", new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "السبب طويل جداً." },
                    { new Guid("d3f316f8-3a18-12dd-b233-9eb443e6ecb8"), "en", new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "Required" },
                    { new Guid("d43eed7b-fbc5-9495-b344-9a70fc5eb7bb"), "en", new Guid("c1a586fe-cebf-8cb7-a168-15001eaa2b8e"), "Paid in cash, with no receipt" },
                    { new Guid("e2a795fe-66f5-a3bf-a45a-6e443ddbc04f"), "ar", new Guid("b0f4dbc5-7fc7-82b4-83db-e627bb5cb23b"), "إيصال التحويل البنكي" },
                    { new Guid("e9e9eb5b-7a7a-158d-14c8-5b9cddf9fd80"), "ar", new Guid("bd98d737-6932-3fe0-af89-033c6b20536f"), "إلزامي" },
                    { new Guid("f022bfd7-dee9-65fd-a3a6-e275b4f1b722"), "en", new Guid("7076a257-f3d8-6b5d-9900-2248f0ed5bd5"), "The reason is too long." }
                });
        }
    }
}
