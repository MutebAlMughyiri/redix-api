using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class CapitalIsATable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12dee4c0-f443-892e-5728-c2e00f9ca781"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f7e5d09-b834-ea9a-0aa6-985e5ac0a9f7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"));

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "Partners",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Partners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[] { new Guid("7d56b915-89c1-0e21-4392-a31e37428c69"), true, "رأس المال", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 5 });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "", "categories.itemHasPartners" },
                    { new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "", "categories.typeCapital" },
                    { new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "", "partners.item" },
                    { new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "", "partners.itemNotOffered" }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "ShowsTablePercentages", "SortOrder", "Type" },
                values: new object[] { new Guid("f2d90dd8-4d88-c252-27ca-0160324d3cd4"), new Guid("7d56b915-89c1-0e21-4392-a31e37428c69"), true, "رأس المال", true, 0, (byte)6 });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("244047f7-5b2e-d51e-70eb-9e428069bf9b"), "en", new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "Partners are this item's tables, so its type cannot change." },
                    { new Guid("274ca25e-b961-63f2-3d96-41a19e7a034a"), "ar", new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "بند رأس المال" },
                    { new Guid("2f269167-9dbe-50fa-a95b-d4048a566e2a"), "ar", new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "رأس المال" },
                    { new Guid("64a38862-a25b-36b3-207e-560bd60a8ba6"), "en", new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"), "Capital item" },
                    { new Guid("683dc7c7-c7a9-cc35-2bd5-bd74cb20762a"), "ar", new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "اختر بنداً من رأس المال في هذا المشروع لجدول الشريك." },
                    { new Guid("6b2b13eb-57e7-b453-f82c-6b4e1b7cc388"), "en", new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "Capital" },
                    { new Guid("c382ae11-8729-e60f-48e0-99b0a70a17fc"), "ar", new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "الشركاء جداول هذا البند، فلا يتغيّر نوعه." },
                    { new Guid("eb310cf0-0aeb-4a41-afa5-9e6ef8401eee"), "en", new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "Choose a capital item of this project for the partner's table." }
                });

            migrationBuilder.InsertData(
                table: "Subgroups",
                columns: new[] { "Id", "ItemId", "Name", "SortOrder" },
                values: new object[] { new Guid("7f1e80a0-63f0-0351-5262-2a5a02392d3a"), new Guid("f2d90dd8-4d88-c252-27ca-0160324d3cd4"), "", 0 });

            // Every project with a structure gets a category رأس المال holding an item رأس المال, before its first category of revenue items, or last when it has none; its partners
            // become that item's tables in the order they were added, which their time-ordered ids give as text, not as SQL Server sorts a uniqueidentifier (redix-api #109). A project still waiting for approval takes it from the template when approved.
            migrationBuilder.Sql("""
                DECLARE @capital TABLE (ProjectId uniqueidentifier PRIMARY KEY, Place int, CategoryId uniqueidentifier, ItemId uniqueidentifier);
                INSERT INTO @capital (ProjectId, Place, CategoryId, ItemId)
                SELECT category.ProjectId, COALESCE(MIN(revenue.SortOrder), MAX(category.SortOrder) + 1), NEWID(), NEWID()
                FROM Categories category
                LEFT JOIN (SELECT DISTINCT holding.Id, holding.SortOrder FROM Categories holding JOIN Items item ON item.CategoryId = holding.Id WHERE item.Type >= 3) revenue ON revenue.Id = category.Id
                WHERE category.ProjectId <> 'f11ea9ed-0623-ebf7-abff-3f4e8714248a'
                GROUP BY category.ProjectId;

                UPDATE category SET SortOrder = category.SortOrder + 1 FROM Categories category JOIN @capital capital ON capital.ProjectId = category.ProjectId WHERE category.SortOrder >= capital.Place;
                INSERT INTO Categories (Id, IsActive, Name, ProjectId, SortOrder) SELECT CategoryId, 1, N'رأس المال', ProjectId, Place FROM @capital;
                INSERT INTO Items (Id, CategoryId, IsActive, Name, ShowsTablePercentages, SortOrder, Type) SELECT ItemId, CategoryId, 1, N'رأس المال', 1, 0, 6 FROM @capital;
                INSERT INTO Subgroups (Id, ItemId, Name, SortOrder) SELECT NEWID(), ItemId, N'', 0 FROM @capital;

                UPDATE partner SET ItemId = capital.ItemId, SortOrder = placed.Place
                FROM Partners partner
                JOIN @capital capital ON capital.ProjectId = partner.ProjectId
                JOIN (SELECT Id, ROW_NUMBER() OVER (PARTITION BY ProjectId ORDER BY CONVERT(char(36), Id)) AS Place FROM Partners) placed ON placed.Id = partner.Id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subgroups",
                keyColumn: "Id",
                keyValue: new Guid("7f1e80a0-63f0-0351-5262-2a5a02392d3a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("244047f7-5b2e-d51e-70eb-9e428069bf9b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("274ca25e-b961-63f2-3d96-41a19e7a034a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f269167-9dbe-50fa-a95b-d4048a566e2a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64a38862-a25b-36b3-207e-560bd60a8ba6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("683dc7c7-c7a9-cc35-2bd5-bd74cb20762a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b2b13eb-57e7-b453-f82c-6b4e1b7cc388"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c382ae11-8729-e60f-48e0-99b0a70a17fc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb310cf0-0aeb-4a41-afa5-9e6ef8401eee"));

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("f2d90dd8-4d88-c252-27ca-0160324d3cd4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a6b9e30-8016-72ce-0899-328e433edcf8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7d56b915-89c1-0e21-4392-a31e37428c69"));

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Partners");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "", "entries.capitalGroup" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("12dee4c0-f443-892e-5728-c2e00f9ca781"), "en", new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "Revenue · Capital" },
                    { new Guid("2f7e5d09-b834-ea9a-0aa6-985e5ac0a9f7"), "ar", new Guid("4c540222-763c-2ad3-8ccd-52cf4c975b9d"), "الإيرادات · رأس المال" }
                });
        }
    }
}
