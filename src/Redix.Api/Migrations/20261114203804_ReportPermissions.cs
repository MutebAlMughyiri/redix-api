using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("5283a467-001e-3bea-c928-22ed0cc7cb93"), "", "reports.needsGenerateReports" },
                    { new Guid("8c48905e-7c40-7b33-0ed2-055ffa5f1a42"), "", "permissions.seeReportsHint" },
                    { new Guid("c1b65c5d-e1b1-8c6c-93b9-1bc3611571c1"), "", "permissions.generateReportsHint" },
                    { new Guid("dceb659c-3204-4714-46bc-1cf025fdd6ba"), "", "permissions.generateReports" },
                    { new Guid("eb536699-1b9d-c16a-63f5-2eaf2400264f"), "", "permissions.seeReports" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"),
                column: "Value",
                value: "Capital movements, revenue and reconciling.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"),
                column: "Value",
                value: "Sees every partner's capital.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4ba7de7-217d-f868-d3cc-259546894792"),
                column: "Value",
                value: "حركات رأس المال والإيرادات والمطابقة.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"),
                column: "Value",
                value: "يرى رأس مال كل الشركاء.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("037dd6ac-4432-0d3f-e036-dd962d201955"), "en", new Guid("c1b65c5d-e1b1-8c6c-93b9-1bc3611571c1"), "Issues the periodic report and takes every report's Excel and PDF, its capital showing only the partners one sees." },
                    { new Guid("2f558d32-6579-2df7-dcdf-5ab3698ccf48"), "ar", new Guid("8c48905e-7c40-7b33-0ed2-055ffa5f1a42"), "يفتح التقرير الدوري وتقارير البنود والعقود وكشف الضريبة." },
                    { new Guid("4a7ef005-bb9a-d734-43f7-6928adca26e4"), "ar", new Guid("dceb659c-3204-4714-46bc-1cf025fdd6ba"), "يُصدر التقارير" },
                    { new Guid("4b1062a6-a39c-aa01-9a65-0ab903289291"), "ar", new Guid("5283a467-001e-3bea-c928-22ed0cc7cb93"), "يحتاج صلاحية «يُصدر التقارير»." },
                    { new Guid("a62998e6-eecd-4f02-19c8-d1cab4b830db"), "en", new Guid("5283a467-001e-3bea-c928-22ed0cc7cb93"), "Needs Generate reports." },
                    { new Guid("af04aeeb-3030-3593-db5d-66f1be7eeaf6"), "en", new Guid("8c48905e-7c40-7b33-0ed2-055ffa5f1a42"), "Opens the periodic report, the item reports, the contracts' reports and the VAT statement." },
                    { new Guid("afe6bbd5-0769-c1d4-057d-52a2712e8f77"), "en", new Guid("eb536699-1b9d-c16a-63f5-2eaf2400264f"), "See reports" },
                    { new Guid("ca238628-5f42-1312-35ee-a884f303d6f1"), "en", new Guid("dceb659c-3204-4714-46bc-1cf025fdd6ba"), "Generate reports" },
                    { new Guid("e829d1d2-5e97-186b-2a5e-d3a2c41530a2"), "ar", new Guid("c1b65c5d-e1b1-8c6c-93b9-1bc3611571c1"), "يُصدر التقرير الدوري ويأخذ ملف Excel وPDF لكل تقرير، ولا يظهر فيه من رأس المال إلا الشركاء الذين يراهم." },
                    { new Guid("f9f3d79d-0932-4065-1eae-4a2eb3471038"), "ar", new Guid("eb536699-1b9d-c16a-63f5-2eaf2400264f"), "يرى التقارير" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney', 'SeeReports', 'GenerateReports')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney', 'SeeReports', 'GenerateReports') AND [Permission] <> 'ApproveProjects'");
            migrationBuilder.Sql("""
                INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission)
                SELECT NEWID(), held.ProjectId, held.UserId, 'SeeReports' FROM ProjectPermissions held
                WHERE held.Permission = 'ReadProject'
                    AND NOT EXISTS (SELECT 1 FROM ProjectPermissions own WHERE own.ProjectId = held.ProjectId AND own.UserId = held.UserId AND own.Permission = 'SeeReports');
                INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission)
                SELECT NEWID(), held.ProjectId, held.UserId, 'GenerateReports' FROM (SELECT DISTINCT ProjectId, UserId FROM ProjectPermissions WHERE Permission IN ('MoveMoney', 'SeeAllCapital')) held
                WHERE NOT EXISTS (SELECT 1 FROM ProjectPermissions own WHERE own.ProjectId = held.ProjectId AND own.UserId = held.UserId AND own.Permission = 'GenerateReports');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ProjectPermissions WHERE Permission IN ('SeeReports', 'GenerateReports');");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("037dd6ac-4432-0d3f-e036-dd962d201955"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f558d32-6579-2df7-dcdf-5ab3698ccf48"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a7ef005-bb9a-d734-43f7-6928adca26e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b1062a6-a39c-aa01-9a65-0ab903289291"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a62998e6-eecd-4f02-19c8-d1cab4b830db"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af04aeeb-3030-3593-db5d-66f1be7eeaf6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("afe6bbd5-0769-c1d4-057d-52a2712e8f77"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca238628-5f42-1312-35ee-a884f303d6f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e829d1d2-5e97-186b-2a5e-d3a2c41530a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9f3d79d-0932-4065-1eae-4a2eb3471038"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5283a467-001e-3bea-c928-22ed0cc7cb93"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8c48905e-7c40-7b33-0ed2-055ffa5f1a42"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c1b65c5d-e1b1-8c6c-93b9-1bc3611571c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dceb659c-3204-4714-46bc-1cf025fdd6ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eb536699-1b9d-c16a-63f5-2eaf2400264f"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"),
                column: "Value",
                value: "Capital movements, revenue, reconciling and the periodic report.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"),
                column: "Value",
                value: "Sees every partner's capital, and takes the report files.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4ba7de7-217d-f868-d3cc-259546894792"),
                column: "Value",
                value: "حركات رأس المال والإيرادات والمطابقة والتقرير الدوري.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"),
                column: "Value",
                value: "يرى رأس مال كل الشركاء، ويأخذ ملفات التقارير.");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");
        }
    }
}
