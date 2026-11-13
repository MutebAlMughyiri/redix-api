using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ListChoicesWithSigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Options",
                table: "ItemColumns",
                newName: "Choices");

            migrationBuilder.RenameColumn(
                name: "Options",
                table: "DefaultColumns",
                newName: "Choices");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "", "columns.sign" },
                    { new Guid("33186c1e-53a3-b835-22ff-2c88c41232a8"), "", "columns.signedListExists" },
                    { new Guid("7c8ac24d-4e48-51ce-c825-a5bcdb589d75"), "", "columns.signPlus" },
                    { new Guid("ef27daa6-a8f0-1419-97e1-8d945a94deff"), "", "columns.signMinus" },
                    { new Guid("fbb8026f-ca96-eba5-9437-ac9447250276"), "", "columns.signNone" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("3204939f-84d8-2bd6-1921-c19947528631"), "en", new Guid("ef27daa6-a8f0-1419-97e1-8d945a94deff"), "Minus (−)" },
                    { new Guid("5eb00446-ae21-97ad-602e-4d28f14c2325"), "ar", new Guid("fbb8026f-ca96-eba5-9437-ac9447250276"), "بلا إشارة" },
                    { new Guid("7f2a7431-ac6e-5028-0440-f83d99c91a4d"), "en", new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "Sign" },
                    { new Guid("9590ea1f-33cd-eaa2-cd37-20193c7736ad"), "ar", new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "الإشارة" },
                    { new Guid("a3947043-8ca0-a723-76a0-7da2ffe83434"), "en", new Guid("33186c1e-53a3-b835-22ff-2c88c41232a8"), "This table already has a list whose choices give the amount a sign, and a table takes one." },
                    { new Guid("a64ea42c-986d-6a28-4ef2-eaaa71902aee"), "ar", new Guid("ef27daa6-a8f0-1419-97e1-8d945a94deff"), "سالب (−)" },
                    { new Guid("be04e3ed-916f-aa61-1261-71f6e3439534"), "ar", new Guid("33186c1e-53a3-b835-22ff-2c88c41232a8"), "لهذا الجدول قائمة تعطي خياراتها المبلغ إشارة، والجدول يأخذ قائمة واحدة." },
                    { new Guid("c9811628-c8e3-98a6-5149-acfdaeb675e9"), "en", new Guid("fbb8026f-ca96-eba5-9437-ac9447250276"), "No sign" },
                    { new Guid("ca9fc061-2d94-c0f3-ce16-9205350ee5a5"), "ar", new Guid("7c8ac24d-4e48-51ce-c825-a5bcdb589d75"), "موجب (+)" },
                    { new Guid("cdcc5583-2907-1f30-a0a3-925ad2a620e5"), "en", new Guid("7c8ac24d-4e48-51ce-c825-a5bcdb589d75"), "Plus (+)" }
                });
            foreach (var table in new[] { "ItemColumns", "DefaultColumns" })
            {
                migrationBuilder.Sql($"""
                    UPDATE kept SET Choices = (
                        SELECT LOWER(CONVERT(nvarchar(36), NEWID())) AS Id, choice.[value] AS Name, 0 AS Sign
                        FROM OPENJSON(CAST(kept.Choices AS nvarchar(max))) choice ORDER BY CAST(choice.[key] AS int) FOR JSON PATH)
                    FROM {table} kept WHERE CAST(kept.Choices AS nvarchar(max)) <> '[]';
                    """);
            }
            migrationBuilder.Sql("""
                DECLARE @changed int = 1;
                WHILE @changed > 0
                BEGIN
                    UPDATE details SET CustomValues = JSON_MODIFY(CAST(details.CustomValues AS nvarchar(max)), '$."' + chosen.[Key] + '"', chosen.Id)
                    FROM ExpenseDetails details
                    CROSS APPLY (
                        SELECT TOP 1 kept.[Key], choice.Id FROM ItemColumns kept
                        CROSS APPLY OPENJSON(CAST(kept.Choices AS nvarchar(max))) WITH (Id nvarchar(36) '$.Id', Name nvarchar(max) '$.Name') choice
                        WHERE kept.ItemId = details.ItemId AND kept.DataType = 4 AND kept.[Key] LIKE 'custom.%'
                            AND JSON_VALUE(CAST(details.CustomValues AS nvarchar(max)), '$."' + kept.[Key] + '"') = choice.Name) chosen;
                    SET @changed = @@ROWCOUNT;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @changed int = 1;
                WHILE @changed > 0
                BEGIN
                    UPDATE details SET CustomValues = JSON_MODIFY(CAST(details.CustomValues AS nvarchar(max)), '$."' + chosen.[Key] + '"', chosen.Name)
                    FROM ExpenseDetails details
                    CROSS APPLY (
                        SELECT TOP 1 kept.[Key], choice.Name FROM ItemColumns kept
                        CROSS APPLY OPENJSON(CAST(kept.Choices AS nvarchar(max))) WITH (Id nvarchar(36) '$.Id', Name nvarchar(max) '$.Name') choice
                        WHERE kept.ItemId = details.ItemId AND kept.DataType = 4 AND kept.[Key] LIKE 'custom.%'
                            AND JSON_VALUE(CAST(details.CustomValues AS nvarchar(max)), '$."' + kept.[Key] + '"') = choice.Id) chosen;
                    SET @changed = @@ROWCOUNT;
                END
                """);
            foreach (var table in new[] { "ItemColumns", "DefaultColumns" })
            {
                migrationBuilder.Sql($"""
                    UPDATE kept SET Choices = ISNULL((
                        SELECT '[' + STRING_AGG('"' + STRING_ESCAPE(JSON_VALUE(choice.[value], '$.Name'), 'json') + '"', ',') WITHIN GROUP (ORDER BY CAST(choice.[key] AS int)) + ']'
                        FROM OPENJSON(CAST(kept.Choices AS nvarchar(max))) choice), '[]')
                    FROM {table} kept;
                    """);
            }

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3204939f-84d8-2bd6-1921-c19947528631"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5eb00446-ae21-97ad-602e-4d28f14c2325"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f2a7431-ac6e-5028-0440-f83d99c91a4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9590ea1f-33cd-eaa2-cd37-20193c7736ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a3947043-8ca0-a723-76a0-7da2ffe83434"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a64ea42c-986d-6a28-4ef2-eaaa71902aee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be04e3ed-916f-aa61-1261-71f6e3439534"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9811628-c8e3-98a6-5149-acfdaeb675e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca9fc061-2d94-c0f3-ce16-9205350ee5a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cdcc5583-2907-1f30-a0a3-925ad2a620e5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33186c1e-53a3-b835-22ff-2c88c41232a8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c8ac24d-4e48-51ce-c825-a5bcdb589d75"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ef27daa6-a8f0-1419-97e1-8d945a94deff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fbb8026f-ca96-eba5-9437-ac9447250276"));

            migrationBuilder.RenameColumn(
                name: "Choices",
                table: "ItemColumns",
                newName: "Options");

            migrationBuilder.RenameColumn(
                name: "Choices",
                table: "DefaultColumns",
                newName: "Options");
        }
    }
}
