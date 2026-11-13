using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryTypeAndMovementTypeLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapitalMovementDetails_LK_CapitalMovementType_Type",
                table: "CapitalMovementDetails");

            migrationBuilder.DropTable(
                name: "LK_CapitalMovementType");

            migrationBuilder.DropIndex(
                name: "IX_CapitalMovementDetails_Type",
                table: "CapitalMovementDetails");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CapitalMovementDetails");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 8 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 7 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                column: "SortOrder",
                value: 10);

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
                keyValue: new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 1 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("47874291-b290-c9e7-623d-b0d47f539b63"),
                column: "SortOrder",
                value: 7);

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
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 2 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 6 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 2 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "ShowInReports",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 0 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 4 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "ShowInReports",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"),
                column: "ShowInReports",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { true, 0 });

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "Choices", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("004a7c57-89da-a874-7ffa-7a27effd5dea"), "[]", (byte)4, true, true, (byte)5, "account", "الحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("02904bf0-fec1-5ac2-1582-68709464cf3a"), "[]", (byte)2, true, true, (byte)3, "amount", "المبلغ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("0b510b9a-240d-92ab-f483-d515f9f4688b"), "[]", (byte)3, true, true, (byte)3, "date", "التاريخ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("1482b663-6f2d-1db5-e613-6e92806181ac"), "[]", (byte)4, true, true, (byte)3, "account", "الحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("2b7202cf-e3b9-3180-0bf1-3e47b35aef68"), "[]", (byte)4, true, true, (byte)4, "account", "الحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("2ffc3346-424f-d5f2-3a75-920eafdc11d5"), "[]", (byte)3, true, true, (byte)6, "date", "التاريخ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 1 },
                    { new Guid("370236a2-0610-8215-5daf-2c5ffd4693fd"), "[]", (byte)5, true, false, (byte)6, "attachment", "المرفق", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("3d827454-9694-6ff2-28c4-569b6018f8c2"), "[{\"Id\":\"06460ff0-42d7-ca58-2032-a1bd1543eca0\",\"Name\":\"\\u0634\\u0631\\u0627\\u0621\",\"Sign\":1},{\"Id\":\"6ad91ed9-316c-826e-8f15-e5f277fdeba2\",\"Name\":\"\\u0645\\u0631\\u062A\\u062C\\u0639\",\"Sign\":2}]", (byte)4, true, false, (byte)0, "entryType", "نوع القيد", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 5 },
                    { new Guid("5323f8d3-85d8-7441-d2b8-6b82e1c49799"), "[]", (byte)2, true, true, (byte)4, "amount", "المبلغ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("761f75ab-5b2c-54b5-bf2c-cb33549282a7"), "[{\"Id\":\"18475559-dc5d-29ae-f1c8-36b2b080151f\",\"Name\":\"\\u0625\\u0636\\u0627\\u0641\\u0629 \\u0645\\u0628\\u0644\\u063A\",\"Sign\":1},{\"Id\":\"90f8b88a-68ff-f475-364a-5ee21644377d\",\"Name\":\"\\u0633\\u062D\\u0628 \\u0645\\u0628\\u0644\\u063A\",\"Sign\":2}]", (byte)4, true, false, (byte)6, "movementType", "نوع الحركة", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("7963c6ff-00e8-04d0-097e-237ea962f822"), "[]", (byte)5, true, false, (byte)4, "attachment", "المرفق", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 2 },
                    { new Guid("a86b609b-23cf-09aa-c30e-d60fb69bf945"), "[]", (byte)4, true, true, (byte)6, "account", "الحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("adcb67a0-4f83-a907-6ace-a708e3db9201"), "[]", (byte)2, true, true, (byte)5, "amount", "المبلغ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("ae2fbb98-749c-acb9-5fbb-80030480bf14"), "[]", (byte)5, true, false, (byte)3, "attachment", "المرفق", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 2 },
                    { new Guid("b1730260-d0eb-d24b-cba5-c3600ff06050"), "[{\"Id\":\"06460ff0-42d7-ca58-2032-a1bd1543eca0\",\"Name\":\"\\u0634\\u0631\\u0627\\u0621\",\"Sign\":1},{\"Id\":\"6ad91ed9-316c-826e-8f15-e5f277fdeba2\",\"Name\":\"\\u0645\\u0631\\u062A\\u062C\\u0639\",\"Sign\":2}]", (byte)4, true, false, (byte)2, "entryType", "نوع القيد", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 6 },
                    { new Guid("cd03e67e-c425-6ae8-1679-a29d65256a97"), "[]", (byte)5, true, false, (byte)5, "attachment", "المرفق", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 2 },
                    { new Guid("e62df9db-1d9a-cae6-95e4-7b4570cec11d"), "[]", (byte)3, true, true, (byte)5, "date", "التاريخ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("f18f23f2-2553-89de-c553-4c4e5b1f0930"), "[]", (byte)2, true, true, (byte)6, "amount", "المبلغ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 4 },
                    { new Guid("f8ab4fb8-46d0-a6ad-9e6f-31dc24fd45e4"), "[]", (byte)3, true, true, (byte)4, "date", "التاريخ", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00f3bd6e-7747-4829-d2a6-b5df1f1020f6"),
                column: "Value",
                value: "No column is added to a revenue item: it asks what its template gives it.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2de39c18-51d9-1ba3-6999-8b678d3a0f49"),
                column: "Value",
                value: "لا يُضاف عمود إلى بند إيرادات: يطلب ما يعطيه قالبه.");

            migrationBuilder.Sql("""
                DECLARE @template uniqueidentifier = 'f11ea9ed-0623-ebf7-abff-3f4e8714248a';

                -- Each project's own template takes what the default template now has and it lacks: «نوع القيد», and the columns of رأس المال and of revenue.
                INSERT INTO DefaultColumns (Id, ProjectId, ItemType, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices)
                SELECT NEWID(), project.Id, seeded.ItemType, seeded.[Key], seeded.Name, seeded.DataType, seeded.IsCore, seeded.IsRequired, seeded.ShowInReports, seeded.SortOrder, seeded.Choices
                FROM DefaultColumns seeded CROSS JOIN Projects project
                WHERE seeded.ProjectId = @template AND project.Id <> @template
                    AND (seeded.[Key] IN ('entryType', 'movementType') OR seeded.ItemType IN (3, 4, 5, 6))
                    AND EXISTS (SELECT 1 FROM DefaultColumns own WHERE own.ProjectId = project.Id)
                    AND NOT EXISTS (SELECT 1 FROM DefaultColumns own WHERE own.ProjectId = project.Id AND own.ItemType = seeded.ItemType AND own.[Key] = seeded.[Key]);

                -- Each item takes the same from its project's template, the choices with their ids.
                INSERT INTO ItemColumns (Id, ItemId, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices, IsHidden)
                SELECT NEWID(), item.Id, own.[Key], own.Name, own.DataType, own.IsCore, own.IsRequired, own.ShowInReports, own.SortOrder, own.Choices, 0
                FROM Items item JOIN Categories category ON category.Id = item.CategoryId
                JOIN DefaultColumns own ON own.ProjectId = category.ProjectId AND own.ItemType = item.Type
                WHERE category.ProjectId <> @template
                    AND (own.[Key] IN ('entryType', 'movementType') OR own.ItemType IN (3, 4, 5, 6))
                    AND NOT EXISTS (SELECT 1 FROM ItemColumns kept WHERE kept.ItemId = item.Id AND kept.[Key] = own.[Key]);

                -- What the approved workbook prints: the supplier, the account, the payment method, the file and the place in the contract.
                UPDATE kept SET ShowInReports = seeded.ShowInReports
                FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId
                JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = item.Type AND seeded.[Key] = kept.[Key]
                WHERE kept.[Key] IN ('supplier', 'account', 'paymentMethod', 'attachment', 'outsideContract');
                UPDATE own SET ShowInReports = seeded.ShowInReports
                FROM DefaultColumns own JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = own.ItemType AND seeded.[Key] = own.[Key]
                WHERE own.ProjectId <> @template AND own.[Key] IN ('supplier', 'account', 'paymentMethod', 'attachment', 'outsideContract');

                -- The columns of the template in its order, then a person's own in theirs.
                WITH placed AS (
                    SELECT kept.SortOrder, ROW_NUMBER() OVER (PARTITION BY kept.ItemId ORDER BY CASE WHEN seeded.Id IS NULL THEN 1 ELSE 0 END, ISNULL(seeded.SortOrder, 0), kept.SortOrder) - 1 AS Place
                    FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId
                    LEFT JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = item.Type AND seeded.[Key] = kept.[Key])
                UPDATE placed SET SortOrder = Place;
                WITH placed AS (
                    SELECT own.SortOrder, ROW_NUMBER() OVER (PARTITION BY own.ProjectId, own.ItemType ORDER BY CASE WHEN seeded.Id IS NULL THEN 1 ELSE 0 END, ISNULL(seeded.SortOrder, 0), own.SortOrder) - 1 AS Place
                    FROM DefaultColumns own
                    LEFT JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = own.ItemType AND seeded.[Key] = own.[Key]
                    WHERE own.ProjectId <> @template)
                UPDATE placed SET SortOrder = Place;

                -- Every entry keeps its figure: a purchase kept negative was a credit note, «مرتجع»; any other is «شراء». Capital out is «سحب مبلغ», capital in «إضافة مبلغ».
                UPDATE Entries SET ColumnValues = JSON_MODIFY(CAST(ColumnValues AS nvarchar(max)), '$.entryType', CASE WHEN Amount < 0 THEN '6ad91ed9-316c-826e-8f15-e5f277fdeba2' ELSE '06460ff0-42d7-ca58-2032-a1bd1543eca0' END)
                WHERE Kind = 0;
                UPDATE Entries SET ColumnValues = JSON_MODIFY(CAST(ColumnValues AS nvarchar(max)), '$.movementType', CASE WHEN Kind = 3 THEN '90f8b88a-68ff-f475-364a-5ee21644377d' ELSE '18475559-dc5d-29ae-f1c8-36b2b080151f' END)
                WHERE Kind IN (2, 3);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("004a7c57-89da-a874-7ffa-7a27effd5dea"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("02904bf0-fec1-5ac2-1582-68709464cf3a"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0b510b9a-240d-92ab-f483-d515f9f4688b"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1482b663-6f2d-1db5-e613-6e92806181ac"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2b7202cf-e3b9-3180-0bf1-3e47b35aef68"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ffc3346-424f-d5f2-3a75-920eafdc11d5"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("370236a2-0610-8215-5daf-2c5ffd4693fd"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3d827454-9694-6ff2-28c4-569b6018f8c2"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5323f8d3-85d8-7441-d2b8-6b82e1c49799"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("761f75ab-5b2c-54b5-bf2c-cb33549282a7"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("7963c6ff-00e8-04d0-097e-237ea962f822"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a86b609b-23cf-09aa-c30e-d60fb69bf945"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("adcb67a0-4f83-a907-6ace-a708e3db9201"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("ae2fbb98-749c-acb9-5fbb-80030480bf14"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b1730260-d0eb-d24b-cba5-c3600ff06050"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("cd03e67e-c425-6ae8-1679-a29d65256a97"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e62df9db-1d9a-cae6-95e4-7b4570cec11d"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f18f23f2-2553-89de-c553-4c4e5b1f0930"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f8ab4fb8-46d0-a6ad-9e6f-31dc24fd45e4"));

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "CapitalMovementDetails",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "LK_CapitalMovementType",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_CapitalMovementType", x => x.Id);
                    table.UniqueConstraint("AK_LK_CapitalMovementType_Name", x => x.Name);
                });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 6 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 6 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"),
                column: "SortOrder",
                value: 8);

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
                keyValue: new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 5 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("47874291-b290-c9e7-623d-b0d47f539b63"),
                column: "SortOrder",
                value: 6);

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
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 12 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 7 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 4 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "ShowInReports",
                value: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 1 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 7 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "ShowInReports",
                value: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 8 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"),
                column: "ShowInReports",
                value: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                columns: new[] { "ShowInReports", "SortOrder" },
                values: new object[] { false, 1 });

            migrationBuilder.InsertData(
                table: "LK_CapitalMovementType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Contribution" },
                    { (byte)1, "Withdrawal" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00f3bd6e-7747-4829-d2a6-b5df1f1020f6"),
                column: "Value",
                value: "A revenue item asks no columns.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2de39c18-51d9-1ba3-6999-8b678d3a0f49"),
                column: "Value",
                value: "بند الإيرادات لا يطلب أعمدة.");

            migrationBuilder.CreateIndex(
                name: "IX_CapitalMovementDetails_Type",
                table: "CapitalMovementDetails",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_CapitalMovementDetails_LK_CapitalMovementType_Type",
                table: "CapitalMovementDetails",
                column: "Type",
                principalTable: "LK_CapitalMovementType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                DECLARE @template uniqueidentifier = 'f11ea9ed-0623-ebf7-abff-3f4e8714248a';
                UPDATE details SET Type = CASE entry.Kind WHEN 3 THEN 1 ELSE 0 END FROM CapitalMovementDetails details JOIN Entries entry ON entry.Id = details.EntryId;
                UPDATE Entries SET ColumnValues = JSON_MODIFY(JSON_MODIFY(CAST(ColumnValues AS nvarchar(max)), '$.entryType', NULL), '$.movementType', NULL);
                DELETE kept FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId WHERE kept.[Key] IN ('entryType', 'movementType') OR item.Type IN (3, 4, 5, 6);
                DELETE FROM DefaultColumns WHERE ProjectId <> @template AND ([Key] IN ('entryType', 'movementType') OR ItemType IN (3, 4, 5, 6));
                UPDATE kept SET ShowInReports = seeded.ShowInReports
                FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId
                JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = item.Type AND seeded.[Key] = kept.[Key]
                WHERE kept.[Key] IN ('supplier', 'account', 'paymentMethod', 'attachment', 'outsideContract');
                UPDATE own SET ShowInReports = seeded.ShowInReports
                FROM DefaultColumns own JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = own.ItemType AND seeded.[Key] = own.[Key]
                WHERE own.ProjectId <> @template AND own.[Key] IN ('supplier', 'account', 'paymentMethod', 'attachment', 'outsideContract');
                WITH placed AS (
                    SELECT kept.SortOrder, ROW_NUMBER() OVER (PARTITION BY kept.ItemId ORDER BY CASE WHEN seeded.Id IS NULL THEN 1 ELSE 0 END, ISNULL(seeded.SortOrder, 0), kept.SortOrder) - 1 AS Place
                    FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId
                    LEFT JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = item.Type AND seeded.[Key] = kept.[Key])
                UPDATE placed SET SortOrder = Place;
                WITH placed AS (
                    SELECT own.SortOrder, ROW_NUMBER() OVER (PARTITION BY own.ProjectId, own.ItemType ORDER BY CASE WHEN seeded.Id IS NULL THEN 1 ELSE 0 END, ISNULL(seeded.SortOrder, 0), own.SortOrder) - 1 AS Place
                    FROM DefaultColumns own
                    LEFT JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = own.ItemType AND seeded.[Key] = own.[Key]
                    WHERE own.ProjectId <> @template)
                UPDATE placed SET SortOrder = Place;
                """);
        }
    }
}
