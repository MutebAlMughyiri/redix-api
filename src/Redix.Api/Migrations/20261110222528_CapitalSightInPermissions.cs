using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CapitalSightInPermissions : Migration
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

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("274ca25e-b961-63f2-3d96-41a19e7a034a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64a38862-a25b-36b3-207e-560bd60a8ba6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"));

            migrationBuilder.CreateTable(
                name: "ChosenPartners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChosenPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // A partner's login used to let that person see the partner's capital; it is now only whose account the partner is (redix-api #106). So that nobody sees less, each
            // login still in the project becomes a partner chosen for that person, who holds SeeChosenCapital unless they see all.
            migrationBuilder.Sql("""
                INSERT INTO ChosenPartners (Id, ProjectId, UserId, PartnerId)
                SELECT NEWID(), partner.ProjectId, partner.UserId, partner.Id FROM Partners partner
                WHERE partner.UserId <> '00000000-0000-0000-0000-000000000000'
                    AND EXISTS (SELECT 1 FROM ProjectPermissions held WHERE held.ProjectId = partner.ProjectId AND held.UserId = partner.UserId AND held.Permission = 'ReadProject')
                    AND NOT EXISTS (SELECT 1 FROM ProjectPermissions held WHERE held.ProjectId = partner.ProjectId AND held.UserId = partner.UserId AND held.Permission = 'SeeAllCapital');
                INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission)
                SELECT NEWID(), chosen.ProjectId, chosen.UserId, 'SeeChosenCapital' FROM (SELECT DISTINCT ProjectId, UserId FROM ChosenPartners) chosen;
                """);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "", "permissions.seeChosenCapitalHint" },
                    { new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "", "permissions.seeChosenCapital" },
                    { new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "", "users.unknownPartner" },
                    { new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "", "permissions.capitalGroup" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ba6638c-5385-2d57-0ebb-c5cca4f90ffb"),
                column: "Value",
                value: "حساب هذا الشريك في التطبيق. وما يراه أي شخص من رأس المال يُحدَّد في الصلاحيات.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("683dc7c7-c7a9-cc35-2bd5-bd74cb20762a"),
                column: "Value",
                value: "بند رأس المال في هذا المشروع معطّل أو غير موجود، فلا يُضاف شريك.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a112cb9-5f3e-498f-bf95-e13593dc6336"),
                column: "Value",
                value: "Whose account this partner is in the app. What anyone sees of the capital is ticked in Permissions.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"),
                column: "Value",
                value: "Sees every partner's capital, and takes the report files.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"),
                column: "Value",
                value: "يرى رأس مال كل الشركاء، ويأخذ ملفات التقارير.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6ebdc9a-0eef-7821-1fcb-24851593c2fd"),
                column: "Value",
                value: "حساب الشريك");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb310cf0-0aeb-4a41-afa5-9e6ef8401eee"),
                column: "Value",
                value: "This project's capital item is disabled or missing, so a partner cannot be added.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eeeed37e-ca44-83a6-0d16-1cf0c9cc94e3"),
                column: "Value",
                value: "Partner's account");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0199efec-33f4-a9f2-cb18-93268bda0778"), "ar", new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "رأس مال شركاء محددين" },
                    { new Guid("0986f892-50b0-1c00-b9b5-1a3acced1fb1"), "en", new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "Sees only the capital of the partners ticked under it." },
                    { new Guid("0c5e8604-a75a-b0fb-bfca-31f5c2e5e9aa"), "ar", new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "رأس المال" },
                    { new Guid("3cdc625a-c1a7-ea31-6477-fed56a1b1fd7"), "ar", new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "هذا الشريك ليس في هذا المشروع." },
                    { new Guid("41cf1f59-890c-c866-c203-4da297d3830c"), "en", new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "Capital" },
                    { new Guid("67f430e2-0127-d902-8316-332c98f880bf"), "ar", new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "يرى رأس مال الشركاء المؤشَّرين تحته فقط." },
                    { new Guid("7cc07d3c-fde6-76b8-3c7a-de0c36cbd338"), "en", new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "Chosen partners' capital" },
                    { new Guid("d4b31d3d-581e-ada5-8dbd-4b13375d4d7d"), "en", new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "That partner is not in this project." }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_PartnerId",
                table: "ChosenPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_ProjectId_UserId_PartnerId",
                table: "ChosenPartners",
                columns: new[] { "ProjectId", "UserId", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_UserId",
                table: "ChosenPartners",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChosenPartners");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0199efec-33f4-a9f2-cb18-93268bda0778"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0986f892-50b0-1c00-b9b5-1a3acced1fb1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c5e8604-a75a-b0fb-bfca-31f5c2e5e9aa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3cdc625a-c1a7-ea31-6477-fed56a1b1fd7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41cf1f59-890c-c866-c203-4da297d3830c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("67f430e2-0127-d902-8316-332c98f880bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7cc07d3c-fde6-76b8-3c7a-de0c36cbd338"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d4b31d3d-581e-ada5-8dbd-4b13375d4d7d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "", "partners.item" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ba6638c-5385-2d57-0ebb-c5cca4f90ffb"),
                column: "Value",
                value: "بحساب الدخول يرى رأس ماله في هذا المشروع دون غيره.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("683dc7c7-c7a9-cc35-2bd5-bd74cb20762a"),
                column: "Value",
                value: "اختر بنداً من رأس المال في هذا المشروع لجدول الشريك.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a112cb9-5f3e-498f-bf95-e13593dc6336"),
                column: "Value",
                value: "With a login, they see their own capital in this project and nobody else's.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"),
                column: "Value",
                value: "Sees every partner's capital, not only their own, and takes the report files.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"),
                column: "Value",
                value: "يرى رأس مال كل الشركاء لا رأس ماله وحده، ويأخذ ملفات التقارير.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6ebdc9a-0eef-7821-1fcb-24851593c2fd"),
                column: "Value",
                value: "حساب دخوله");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb310cf0-0aeb-4a41-afa5-9e6ef8401eee"),
                column: "Value",
                value: "Choose a capital item of this project for the partner's table.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eeeed37e-ca44-83a6-0d16-1cf0c9cc94e3"),
                column: "Value",
                value: "Their login");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("274ca25e-b961-63f2-3d96-41a19e7a034a"), "ar", new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "بند رأس المال" },
                    { new Guid("64a38862-a25b-36b3-207e-560bd60a8ba6"), "en", new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "Capital item" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemPermissions_Permission",
                table: "SystemPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectPermissions_Permission",
                table: "ProjectPermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");
        }
    }
}
