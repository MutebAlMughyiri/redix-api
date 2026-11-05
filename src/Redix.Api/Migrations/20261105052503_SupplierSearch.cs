using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplierSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "", "suppliers.noMatch" },
                    { new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "", "suppliers.searchHint" },
                    { new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "", "suppliers.pick" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0542b3d1-aa00-dac4-5f0b-1aa4cd8201ae"), "ar", new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "بحث بالاسم أو الرقم الضريبي" },
                    { new Guid("1b8b407c-d0d6-a289-6397-05be25db444f"), "en", new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "Choose a supplier" },
                    { new Guid("49accb3e-48f9-31a0-f342-911c8d471fdf"), "ar", new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "لا يوجد مورد مطابق." },
                    { new Guid("4ee59b92-f034-5383-e583-43a359df6b79"), "en", new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "No supplier matches." },
                    { new Guid("587e484b-e94d-2e55-53f6-285520e88efe"), "ar", new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "اختر المورد" },
                    { new Guid("c8e7b8ce-e873-6ba2-cf12-331a066b55df"), "en", new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "Search by name or VAT number" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ProjectId_Name",
                table: "Suppliers",
                columns: new[] { "ProjectId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_ProjectId_Name",
                table: "Suppliers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0542b3d1-aa00-dac4-5f0b-1aa4cd8201ae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b8b407c-d0d6-a289-6397-05be25db444f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49accb3e-48f9-31a0-f342-911c8d471fdf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ee59b92-f034-5383-e583-43a359df6b79"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("587e484b-e94d-2e55-53f6-285520e88efe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8e7b8ce-e873-6ba2-cf12-331a066b55df"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("14591419-de9f-6241-35d5-f33de2c90d77"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"));
        }
    }
}
