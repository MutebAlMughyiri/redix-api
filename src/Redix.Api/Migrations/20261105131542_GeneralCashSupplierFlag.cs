using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class GeneralCashSupplierFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGeneralCash",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE [Suppliers] SET [IsGeneralCash] = 1
                WHERE [Id] IN (SELECT [Id] FROM (SELECT [Id], ROW_NUMBER() OVER (PARTITION BY [ProjectId] ORDER BY [Id]) AS [Place] FROM [Suppliers] WHERE [Name] = N'مورد نقدي عام' AND [VatNumber] = '') AS [Named] WHERE [Place] = 1)
                """);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "", "suppliers.generalCashSupplier" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0f60dc1f-f376-69e0-4bd0-272dedec8780"), "ar", new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "يبقى المورد النقدي العام في القائمة لأن المصروفات التي بلا مورد تحتاجه. ويمكنك تغيير اسمه." },
                    { new Guid("bcfbb0d1-d1a2-b1af-916f-ca151fb92825"), "en", new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "The general cash supplier stays on the register, because expenses with no supplier need it. You can rename it." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f60dc1f-f376-69e0-4bd0-272dedec8780"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bcfbb0d1-d1a2-b1af-916f-ca151fb92825"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"));

            migrationBuilder.DropColumn(
                name: "IsGeneralCash",
                table: "Suppliers");
        }
    }
}
