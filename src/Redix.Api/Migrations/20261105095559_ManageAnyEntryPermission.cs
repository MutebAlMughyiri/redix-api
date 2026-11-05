using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ManageAnyEntryPermission : Migration
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
                values: new object[] { new Guid("6d3b8981-4510-44da-705f-0ef97bbe9d28"), "ManageAnyEntry", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "", "entries.entryNotSubmittable" },
                    { new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "", "entries.notYourEntry" },
                    { new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "", "entries.entryIncomplete" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("28f60970-e91b-5d53-df85-bc71ff454cb5"), "ar", new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "أكمل ما ينقص هذا القيد قبل الإرسال." },
                    { new Guid("31f22691-54a1-6fc9-5e37-2892bd568a5b"), "en", new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "Fill what this entry still lacks before sending." },
                    { new Guid("3ae42790-423a-73e8-a74a-bf251b414c75"), "ar", new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "لا يرسل هذا القيد إلا من أدخله أو المالي." },
                    { new Guid("4a060192-53ee-f888-519a-df94387b5b47"), "ar", new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "هذا القيد مُرسل مسبقاً." },
                    { new Guid("88b34e8c-459b-f0cc-58ea-0b17c7efe3d0"), "en", new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "Only who entered it, or Finance, can send this entry." },
                    { new Guid("9feb97fb-ca3c-c4ef-a9f1-c8e152bff433"), "en", new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "This entry was already sent." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'AttachFiles')");
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
                keyValue: new Guid("6d3b8981-4510-44da-705f-0ef97bbe9d28"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28f60970-e91b-5d53-df85-bc71ff454cb5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("31f22691-54a1-6fc9-5e37-2892bd568a5b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3ae42790-423a-73e8-a74a-bf251b414c75"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a060192-53ee-f888-519a-df94387b5b47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88b34e8c-459b-f0cc-58ea-0b17c7efe3d0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9feb97fb-ca3c-c4ef-a9f1-c8e152bff433"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("349c6178-e979-bb67-feaf-ae7e84931477"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"));

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'AttachFiles')");
        }
    }
}
