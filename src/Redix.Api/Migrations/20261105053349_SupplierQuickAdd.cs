using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplierQuickAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("b806ca26-2b86-5e82-b58e-d6d9a19ee416"), "AddSupplier", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("c79fdab1-3bb3-ec46-2886-93b77a5fe831"), "AddSupplier", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "", "suppliers.useExisting" },
                    { new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "", "suppliers.pendingHint" },
                    { new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "", "suppliers.quickAdd" },
                    { new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "", "suppliers.addAndPick" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("041a1376-8b31-2afa-32a1-666b1ce425f8"), "ar", new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "يُعتمد المورد الجديد مع أول مصروف عليه." },
                    { new Guid("0700fe3f-fc77-1294-7b69-eb07064e7a88"), "ar", new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "مورد جديد" },
                    { new Guid("2fa225cc-a6be-2ea6-0785-529de756469c"), "ar", new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "إضافة واختيار" },
                    { new Guid("4910a692-db3a-6a64-8b1e-285a54fda81d"), "en", new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "Add and choose" },
                    { new Guid("95ccb243-4dfa-ffef-1f95-e0798ee51e92"), "en", new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "New supplier" },
                    { new Guid("bc3e2fa3-9a60-26bc-52db-dc18efd88e3d"), "ar", new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "استخدام هذا المورد" },
                    { new Guid("ef94a4f6-c87b-0c79-2d6e-e227016332d4"), "en", new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "Use this supplier" },
                    { new Guid("fd5de387-cdb7-ed3c-8e81-910210d4c221"), "en", new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "A new supplier is approved with its first expense." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b806ca26-2b86-5e82-b58e-d6d9a19ee416"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c79fdab1-3bb3-ec46-2886-93b77a5fe831"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("041a1376-8b31-2afa-32a1-666b1ce425f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0700fe3f-fc77-1294-7b69-eb07064e7a88"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2fa225cc-a6be-2ea6-0785-529de756469c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4910a692-db3a-6a64-8b1e-285a54fda81d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95ccb243-4dfa-ffef-1f95-e0798ee51e92"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc3e2fa3-9a60-26bc-52db-dc18efd88e3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ef94a4f6-c87b-0c79-2d6e-e227016332d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd5de387-cdb7-ed3c-8e81-910210d4c221"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'ChargeAnyAccount')");
        }
    }
}
