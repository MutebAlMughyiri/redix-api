using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoPartnerFlagNoRevenuePeriodNoBillNumberColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b81ba454-4df5-80d1-864d-e2b27314cc73"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c9a5b977-5bcd-ab11-87f3-2fd7c0175290"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41f93c88-580e-726b-319e-227d53528a1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("449b8219-4168-4400-8e51-fe12a63171da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5695bc1a-35f5-7512-673e-2f7cbb309647"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68a29b1e-952a-5e91-5c31-590bd7fa8843"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ce333b1-8dc6-d55b-fd1a-350ddb9bc4dc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85a0be7e-2f0d-7b76-55ae-d3413f2d91ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("900ba73a-0bf7-f3ab-58b7-b3dbdf7d0346"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a3130911-3155-f557-2577-9b8a0c1a002d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f48606fe-2058-2ca8-f8d1-d98193977b09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd2f2622-0a2e-44bb-3b1a-feadc263aec1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0447cea6-3786-1f47-5238-d936034b480c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("82f07c7e-807a-7e02-3000-29c512083948"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"));

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "RevenueDetails");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "RevenueDetails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Partners");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c9d7893-60b7-58aa-d675-486e13466312"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02d2904b-f846-23e7-ee77-8dc6c4786e77"),
                column: "Value",
                value: "اختر بند إيرادات من هذا المشروع.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("124b8d0f-fae2-1a28-2117-d89166c86ef6"),
                column: "Value",
                value: "Choose a revenue item of this project.");

            // Findings row 150: no bill number is entered since D2, so the column leaves every project's template and every item; core columns keep no values.
            migrationBuilder.Sql("DELETE FROM ItemColumns WHERE [Key] = 'invoiceNumber'");
            migrationBuilder.Sql("DELETE FROM DefaultColumns WHERE [Key] = 'invoiceNumber'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodEnd",
                table: "RevenueDetails",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodStart",
                table: "RevenueDetails",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Partners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c9d7893-60b7-58aa-d675-486e13466312"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "Options", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("b81ba454-4df5-80d1-864d-e2b27314cc73"), (byte)0, true, false, (byte)0, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("c9a5b977-5bcd-ab11-87f3-2fd7c0175290"), (byte)0, true, false, (byte)2, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "", "revenue.invalidVatNumber" },
                    { new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "", "capital.left" },
                    { new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "", "capital.active" },
                    { new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "", "capital.partnerLeft" },
                    { new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "", "revenue.invoiceNumber" },
                    { new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "", "revenue.invalidPeriod" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02d2904b-f846-23e7-ee77-8dc6c4786e77"),
                column: "Value",
                value: "اختر بند إيرادات من هذا المشروع؛ وفاتورة البيع تحتاج بند بيع.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("124b8d0f-fae2-1a28-2117-d89166c86ef6"),
                column: "Value",
                value: "Choose a revenue item of this project; a sales invoice needs a sale item.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"), "ar", new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "الرقم الضريبي رقم من 20 خانة على الأكثر." },
                    { new Guid("41f93c88-580e-726b-319e-227d53528a1f"), "ar", new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "منسحب" },
                    { new Guid("449b8219-4168-4400-8e51-fe12a63171da"), "en", new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "Invoice number" },
                    { new Guid("5695bc1a-35f5-7512-673e-2f7cbb309647"), "en", new Guid("4c884dfa-7259-27b9-c5d7-a329eca8a623"), "Withdrew" },
                    { new Guid("68a29b1e-952a-5e91-5c31-590bd7fa8843"), "ar", new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "اختر الربع." },
                    { new Guid("7ce333b1-8dc6-d55b-fd1a-350ddb9bc4dc"), "en", new Guid("ed7e6b87-7525-1753-bd76-4c3935863295"), "Choose the quarter." },
                    { new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"), "en", new Guid("0447cea6-3786-1f47-5238-d936034b480c"), "A VAT number is a number of at most 20 digits." },
                    { new Guid("85a0be7e-2f0d-7b76-55ae-d3413f2d91ab"), "ar", new Guid("88e76b4b-219e-f4a5-7c60-b1ea8c8acb0b"), "رقم الفاتورة" },
                    { new Guid("900ba73a-0bf7-f3ab-58b7-b3dbdf7d0346"), "en", new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "Active" },
                    { new Guid("a3130911-3155-f557-2577-9b8a0c1a002d"), "en", new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "This partner withdrew." },
                    { new Guid("f48606fe-2058-2ca8-f8d1-d98193977b09"), "ar", new Guid("82f07c7e-807a-7e02-3000-29c512083948"), "انسحب هذا الشريك." },
                    { new Guid("fd2f2622-0a2e-44bb-3b1a-feadc263aec1"), "ar", new Guid("7439b8ef-a460-3965-fb58-4b0a4d9214f8"), "نشط" }
                });
        }
    }
}
