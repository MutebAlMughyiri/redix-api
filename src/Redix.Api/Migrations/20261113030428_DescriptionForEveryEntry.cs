using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionForEveryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("47874291-b290-c9e7-623d-b0d47f539b63"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c9d7893-60b7-58aa-d675-486e13466312"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b8aeded-f822-3f31-7828-f561301e7316"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("10b6922b-86f9-57d7-cbc7-53b5bf7ef7bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("67feda3a-4efa-983d-c8d9-1c0eaf5a01b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e817f1d-9085-f6aa-c189-cad8f32fe7b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac802dc0-5481-f4c4-a770-1a12f7da8cb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc859c76-e6b0-a1c0-840c-a360e6953c7a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c14c3c92-b93e-3325-805e-3b69ea979ca4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ecb26879-92f7-96d4-dfb1-981c14462a18"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"));

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Entries",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Entries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("004a7c57-89da-a874-7ffa-7a27effd5dea"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("02904bf0-fec1-5ac2-1582-68709464cf3a"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0b510b9a-240d-92ab-f483-d515f9f4688b"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1482b663-6f2d-1db5-e613-6e92806181ac"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "التكلفة", 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2b7202cf-e3b9-3180-0bf1-3e47b35aef68"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "Name",
                value: "سعر الحبة");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ffc3346-424f-d5f2-3a75-920eafdc11d5"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "الدفعة", 11 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "الاتفاق", 2 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("370236a2-0610-8215-5daf-2c5ffd4693fd"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5323f8d3-85d8-7441-d2b8-6b82e1c49799"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "الاتفاق", 3 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("7963c6ff-00e8-04d0-097e-237ea962f822"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"),
                column: "Name",
                value: "الدفعة");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a86b609b-23cf-09aa-c30e-d60fb69bf945"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("adcb67a0-4f83-a907-6ace-a708e3db9201"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("ae2fbb98-749c-acb9-5fbb-80030480bf14"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("cd03e67e-c425-6ae8-1679-a29d65256a97"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e62df9db-1d9a-cae6-95e4-7b4570cec11d"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "التكلفة", 12 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f18f23f2-2553-89de-c553-4c4e5b1f0930"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f8ab4fb8-46d0-a6ad-9e6f-31dc24fd45e4"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f8e8fd13-8804-05b5-08ce-8e6e114e7fcf"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "Name",
                value: "سعر الحبة");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "Choices", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("22298279-50fc-a929-0d12-a11e21a61f9b"), "[]", (byte)0, true, false, (byte)2, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("41ac23a6-424b-a276-8585-eef6b88b0134"), "[]", (byte)0, true, false, (byte)0, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("69b95075-7fd9-5bbc-12a1-d3ef3d5d3b97"), "[]", (byte)0, true, false, (byte)1, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("76c516c2-a53a-d177-4971-0ae8209c1326"), "[]", (byte)0, true, false, (byte)6, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 1 },
                    { new Guid("932d87a8-7dca-f61c-c2d4-c1027fc0155c"), "[]", (byte)0, true, false, (byte)5, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("9cfba664-c0f5-d43e-ae65-621f47b4b622"), "[]", (byte)0, true, false, (byte)4, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("ee37d79a-d888-aa80-cc9c-a840a1af60c5"), "[]", (byte)0, true, false, (byte)3, "description", "الوصف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("3b48bd64-d462-3fa0-ce67-bda55ab24a71"), "", "expenses.deductionNeedsDescription" },
                    { new Guid("5402e38c-6d01-918d-5fbf-30c0d38d240e"), "", "expenses.description" },
                    { new Guid("6f6cbed3-d1e5-c64c-64f8-a19598e38057"), "", "expenses.invalidDescription" },
                    { new Guid("73c4e091-06c5-a441-3fee-6b292e68b1b1"), "", "entryShare.description" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("112551b6-8585-fdc1-faeb-11431178178a"),
                column: "Value",
                value: "خارج الاتفاق");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d76814e-eef5-9a3b-5973-050a2ef7b76f"),
                column: "Value",
                value: "Inside the agreement");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be95c09e-c195-465e-d12a-042ab44453f5"),
                column: "Value",
                value: "داخل الاتفاق");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5691cf3-c78a-28e8-dd12-e34c8fdda7fc"),
                column: "Value",
                value: "Outside the agreement");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("098ecb23-99c9-9c48-5a22-0af346dd0b63"), "en", new Guid("3b48bd64-d462-3fa0-ce67-bda55ab24a71"), "A negative amount is a deduction: say why in the description." },
                    { new Guid("0ff1a32b-d455-bfe1-c484-6257b36c2219"), "en", new Guid("5402e38c-6d01-918d-5fbf-30c0d38d240e"), "Description" },
                    { new Guid("1bb92277-e8eb-10ec-6632-5562078cee28"), "en", new Guid("6f6cbed3-d1e5-c64c-64f8-a19598e38057"), "The description is too long." },
                    { new Guid("2d2cf0b7-1e6c-c72a-c7ea-c5239da0df9f"), "en", new Guid("73c4e091-06c5-a441-3fee-6b292e68b1b1"), "Description" },
                    { new Guid("4b999f5e-e850-ed9a-7a6b-52cad67fbdab"), "ar", new Guid("73c4e091-06c5-a441-3fee-6b292e68b1b1"), "الوصف" },
                    { new Guid("69d03169-909d-ebcf-9284-5dbbf86ad35f"), "ar", new Guid("5402e38c-6d01-918d-5fbf-30c0d38d240e"), "الوصف" },
                    { new Guid("f6992c55-2bbd-bd04-13dc-0621fb0f354f"), "ar", new Guid("6f6cbed3-d1e5-c64c-64f8-a19598e38057"), "الوصف طويل جداً." },
                    { new Guid("fdf27ac4-cc6a-c2d3-670c-9443671907b9"), "ar", new Guid("3b48bd64-d462-3fa0-ce67-bda55ab24a71"), "المبلغ السالب خصم: اذكر سببه في الوصف." }
                });

            migrationBuilder.Sql("""
                DECLARE @template uniqueidentifier = 'f11ea9ed-0623-ebf7-abff-3f4e8714248a';

                -- One description: a purchase's product and its notes become «الوصف», joined as «product · notes» when both were typed, so nothing typed is lost.
                UPDATE Entries SET Description = CASE WHEN Description = N'' THEN JSON_VALUE(CAST(ColumnValues AS nvarchar(max)), '$.product')
                    ELSE JSON_VALUE(CAST(ColumnValues AS nvarchar(max)), '$.product') + N' · ' + Description END
                WHERE ISNULL(JSON_VALUE(CAST(ColumnValues AS nvarchar(max)), '$.product'), N'') <> N'';
                UPDATE Entries SET ColumnValues = JSON_MODIFY(CAST(ColumnValues AS nvarchar(max)), '$.product', NULL)
                WHERE JSON_VALUE(CAST(ColumnValues AS nvarchar(max)), '$.product') IS NOT NULL;

                -- «الصنف» and «ملاحظات» go from every template and item; «الوصف» comes in, as the default template holds it.
                DELETE FROM ItemColumns WHERE [Key] IN ('product', 'notes');
                DELETE FROM DefaultColumns WHERE [Key] IN ('product', 'notes');
                INSERT INTO DefaultColumns (Id, ProjectId, ItemType, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices)
                SELECT NEWID(), project.Id, seeded.ItemType, seeded.[Key], seeded.Name, seeded.DataType, seeded.IsCore, seeded.IsRequired, seeded.ShowInReports, seeded.SortOrder, seeded.Choices
                FROM DefaultColumns seeded CROSS JOIN Projects project
                WHERE seeded.ProjectId = @template AND project.Id <> @template AND seeded.[Key] = 'description'
                    AND EXISTS (SELECT 1 FROM DefaultColumns own WHERE own.ProjectId = project.Id)
                    AND NOT EXISTS (SELECT 1 FROM DefaultColumns own WHERE own.ProjectId = project.Id AND own.ItemType = seeded.ItemType AND own.[Key] = seeded.[Key]);
                INSERT INTO ItemColumns (Id, ItemId, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices, IsHidden)
                SELECT NEWID(), item.Id, own.[Key], own.Name, own.DataType, own.IsCore, own.IsRequired, own.ShowInReports, own.SortOrder, own.Choices, 0
                FROM Items item JOIN Categories category ON category.Id = item.CategoryId
                JOIN DefaultColumns own ON own.ProjectId = category.ProjectId AND own.ItemType = item.Type
                WHERE category.ProjectId <> @template AND own.[Key] = 'description'
                    AND NOT EXISTS (SELECT 1 FROM ItemColumns kept WHERE kept.ItemId = item.Id AND kept.[Key] = own.[Key]);

                -- The approved workbook's words, unless a project named the column itself.
                UPDATE kept SET Name = seeded.Name
                FROM ItemColumns kept JOIN Items item ON item.Id = kept.ItemId
                JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = item.Type AND seeded.[Key] = kept.[Key]
                WHERE (kept.[Key] = 'unitPrice' AND kept.Name = N'سعر الوحدة') OR (kept.[Key] = 'total' AND kept.Name = N'الإجمالي') OR (kept.[Key] = 'amount' AND kept.Name = N'المبلغ') OR (kept.[Key] = 'outsideContract' AND kept.Name = N'داخل أو خارج العقد');
                UPDATE kept SET Name = seeded.Name
                FROM DefaultColumns kept
                JOIN DefaultColumns seeded ON seeded.ProjectId = @template AND seeded.ItemType = kept.ItemType AND seeded.[Key] = kept.[Key]
                WHERE kept.ProjectId <> @template AND ((kept.[Key] = 'unitPrice' AND kept.Name = N'سعر الوحدة') OR (kept.[Key] = 'total' AND kept.Name = N'الإجمالي') OR (kept.[Key] = 'amount' AND kept.Name = N'المبلغ') OR (kept.[Key] = 'outsideContract' AND kept.Name = N'داخل أو خارج العقد'));

                -- The template's order: «الوصف» first, after «نوع الحركة» on رأس المال; a person's own after the template's.
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("22298279-50fc-a929-0d12-a11e21a61f9b"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("41ac23a6-424b-a276-8585-eef6b88b0134"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("69b95075-7fd9-5bbc-12a1-d3ef3d5d3b97"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("76c516c2-a53a-d177-4971-0ae8209c1326"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("932d87a8-7dca-f61c-c2d4-c1027fc0155c"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9cfba664-c0f5-d43e-ae65-621f47b4b622"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("ee37d79a-d888-aa80-cc9c-a840a1af60c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("098ecb23-99c9-9c48-5a22-0af346dd0b63"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ff1a32b-d455-bfe1-c484-6257b36c2219"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1bb92277-e8eb-10ec-6632-5562078cee28"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2d2cf0b7-1e6c-c72a-c7ea-c5239da0df9f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b999f5e-e850-ed9a-7a6b-52cad67fbdab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("69d03169-909d-ebcf-9284-5dbbf86ad35f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6992c55-2bbd-bd04-13dc-0621fb0f354f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fdf27ac4-cc6a-c2d3-670c-9443671907b9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3b48bd64-d462-3fa0-ce67-bda55ab24a71"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5402e38c-6d01-918d-5fbf-30c0d38d240e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f6cbed3-d1e5-c64c-64f8-a19598e38057"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("73c4e091-06c5-a441-3fee-6b292e68b1b1"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Entries",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Entries",
                newName: "Notes");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("004a7c57-89da-a874-7ffa-7a27effd5dea"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("02904bf0-fec1-5ac2-1582-68709464cf3a"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0b510b9a-240d-92ab-f483-d515f9f4688b"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1482b663-6f2d-1db5-e613-6e92806181ac"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "الإجمالي", 10 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2b7202cf-e3b9-3180-0bf1-3e47b35aef68"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "Name",
                value: "سعر الوحدة");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ffc3346-424f-d5f2-3a75-920eafdc11d5"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "المبلغ", 12 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "داخل أو خارج العقد", 1 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("370236a2-0610-8215-5daf-2c5ffd4693fd"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5323f8d3-85d8-7441-d2b8-6b82e1c49799"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "داخل أو خارج العقد", 2 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("7963c6ff-00e8-04d0-097e-237ea962f822"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"),
                column: "SortOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"),
                column: "Name",
                value: "المبلغ");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("a86b609b-23cf-09aa-c30e-d60fb69bf945"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("adcb67a0-4f83-a907-6ace-a708e3db9201"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("ae2fbb98-749c-acb9-5fbb-80030480bf14"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("cd03e67e-c425-6ae8-1679-a29d65256a97"),
                column: "SortOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e62df9db-1d9a-cae6-95e4-7b4570cec11d"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                columns: new[] { "Name", "SortOrder" },
                values: new object[] { "الإجمالي", 13 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f18f23f2-2553-89de-c553-4c4e5b1f0930"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f8ab4fb8-46d0-a6ad-9e6f-31dc24fd45e4"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f8e8fd13-8804-05b5-08ce-8e6e114e7fcf"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "Name",
                value: "سعر الوحدة");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "Choices", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"), "[]", (byte)0, false, false, (byte)0, "product", "الصنف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("47874291-b290-c9e7-623d-b0d47f539b63"), "[]", (byte)0, false, false, (byte)1, "notes", "ملاحظات", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 7 },
                    { new Guid("4c9d7893-60b7-58aa-d675-486e13466312"), "[]", (byte)0, false, false, (byte)2, "product", "الصنف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"), "[]", (byte)0, false, false, (byte)2, "notes", "ملاحظات", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 11 },
                    { new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"), "[]", (byte)0, false, false, (byte)0, "notes", "ملاحظات", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "", "expenses.deductionNeedsNotes" },
                    { new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "", "expenses.notes" },
                    { new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "", "entryShare.notes" },
                    { new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "", "expenses.invalidNotes" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("112551b6-8585-fdc1-faeb-11431178178a"),
                column: "Value",
                value: "خارج العقد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d76814e-eef5-9a3b-5973-050a2ef7b76f"),
                column: "Value",
                value: "Inside the contract");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be95c09e-c195-465e-d12a-042ab44453f5"),
                column: "Value",
                value: "داخل العقد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5691cf3-c78a-28e8-dd12-e34c8fdda7fc"),
                column: "Value",
                value: "Outside the contract");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0b8aeded-f822-3f31-7828-f561301e7316"), "ar", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "الملاحظات طويلة جداً." },
                    { new Guid("10b6922b-86f9-57d7-cbc7-53b5bf7ef7bf"), "ar", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "ملاحظات" },
                    { new Guid("67feda3a-4efa-983d-c8d9-1c0eaf5a01b6"), "en", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "Notes" },
                    { new Guid("8e817f1d-9085-f6aa-c189-cad8f32fe7b5"), "ar", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "المبلغ السالب خصم: اذكر سببه في الملاحظات." },
                    { new Guid("ac802dc0-5481-f4c4-a770-1a12f7da8cb4"), "en", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "A negative amount is a deduction: say why in the notes." },
                    { new Guid("bc859c76-e6b0-a1c0-840c-a360e6953c7a"), "ar", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "ملاحظات" },
                    { new Guid("c14c3c92-b93e-3325-805e-3b69ea979ca4"), "en", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "Notes" },
                    { new Guid("ecb26879-92f7-96d4-dfb1-981c14462a18"), "en", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "The notes are too long." }
                });


            migrationBuilder.Sql("""
                DECLARE @template uniqueidentifier = 'f11ea9ed-0623-ebf7-abff-3f4e8714248a';
                DELETE FROM ItemColumns WHERE [Key] = 'description';
                DELETE FROM DefaultColumns WHERE [Key] = 'description' AND ProjectId <> @template;
                UPDATE kept SET Name = CASE WHEN kept.[Key] = 'unitPrice' AND kept.Name = N'سعر الحبة' THEN N'سعر الوحدة' WHEN kept.[Key] = 'total' AND kept.Name = N'التكلفة' THEN N'الإجمالي' WHEN kept.[Key] = 'amount' AND kept.Name = N'الدفعة' THEN N'المبلغ' WHEN kept.[Key] = 'outsideContract' AND kept.Name = N'الاتفاق' THEN N'داخل أو خارج العقد' ELSE kept.Name END FROM ItemColumns kept;
                UPDATE kept SET Name = CASE WHEN kept.[Key] = 'unitPrice' AND kept.Name = N'سعر الحبة' THEN N'سعر الوحدة' WHEN kept.[Key] = 'total' AND kept.Name = N'التكلفة' THEN N'الإجمالي' WHEN kept.[Key] = 'amount' AND kept.Name = N'الدفعة' THEN N'المبلغ' WHEN kept.[Key] = 'outsideContract' AND kept.Name = N'الاتفاق' THEN N'داخل أو خارج العقد' ELSE kept.Name END FROM DefaultColumns kept WHERE kept.ProjectId <> @template;
                INSERT INTO DefaultColumns (Id, ProjectId, ItemType, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices)
                SELECT NEWID(), project.Id, seeded.ItemType, seeded.[Key], seeded.Name, seeded.DataType, seeded.IsCore, seeded.IsRequired, seeded.ShowInReports, seeded.SortOrder, seeded.Choices
                FROM DefaultColumns seeded CROSS JOIN Projects project
                WHERE seeded.ProjectId = @template AND project.Id <> @template AND seeded.[Key] IN ('product', 'notes')
                    AND EXISTS (SELECT 1 FROM DefaultColumns own WHERE own.ProjectId = project.Id);
                INSERT INTO ItemColumns (Id, ItemId, [Key], Name, DataType, IsCore, IsRequired, ShowInReports, SortOrder, Choices, IsHidden)
                SELECT NEWID(), item.Id, own.[Key], own.Name, own.DataType, own.IsCore, own.IsRequired, own.ShowInReports, own.SortOrder, own.Choices, 0
                FROM Items item JOIN Categories category ON category.Id = item.CategoryId
                JOIN DefaultColumns own ON own.ProjectId = category.ProjectId AND own.ItemType = item.Type
                WHERE category.ProjectId <> @template AND own.[Key] IN ('product', 'notes');
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
