using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectFileWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("066e36b0-f0c2-e3ac-88ab-5c0fa06d8688"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ea691bc-4176-625f-6d08-f3a9ad22a4f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("122146e1-dbae-9615-6c0f-a578c252a24f"), "", "projects.loadFile.accountNotInProject" },
                    { new Guid("1f276524-41cb-b102-59f0-cee30a6b7cdc"), "", "projects.loadFile.newerVersion" },
                    { new Guid("209e222a-49d8-9221-0063-0860a884c8f3"), "", "projects.loadFile.notAProjectFile" },
                    { new Guid("330f528c-a81f-ef85-6f8c-4bdada29dbda"), "", "projects.loadFile.unknownChoice" },
                    { new Guid("68df6719-94a4-e395-1221-5e218553b1a0"), "", "projects.loadFile.rowsRefused" },
                    { new Guid("79ed99ec-782f-d5f7-3378-9e1eee018c80"), "", "projects.loadFile.projectHasEntries" },
                    { new Guid("9677cc1f-81f0-8b20-61b2-7ae7493c4a07"), "", "projects.loadFileWhat" },
                    { new Guid("96cac967-ac07-9741-d90c-fe5c4282d798"), "", "projects.loadFile.attachmentNotInFile" },
                    { new Guid("a0202a65-d2f4-39ba-03e6-8dcdcd16142f"), "", "projects.loadFileConfirm" },
                    { new Guid("b4a6fbe0-87f2-acc4-009c-f82fea364c26"), "", "projects.loadFile" },
                    { new Guid("f360b8e0-a251-dbc1-98c0-c913cbb85854"), "", "projects.loadFileDone" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("06936da7-828e-e93a-8934-adac3550f783"), "ar", new Guid("330f528c-a81f-ef85-6f8c-4bdada29dbda"), "«{0}» ليس من اختيارات «{1}»." },
                    { new Guid("20e2a8bb-134f-f968-8a03-224d0c5c447e"), "ar", new Guid("b4a6fbe0-87f2-acc4-009c-f82fea364c26"), "تحميل ملف المشروع" },
                    { new Guid("2736b046-a056-5f0f-e023-efd0de2e3b69"), "en", new Guid("1f276524-41cb-b102-59f0-cee30a6b7cdc"), "The file is from a newer version of REDIX." },
                    { new Guid("2b9bcea8-c3c7-3e6a-fed1-e7a431685418"), "en", new Guid("a0202a65-d2f4-39ba-03e6-8dcdcd16142f"), "Load “{0}” into “{1}”?" },
                    { new Guid("49c44264-61b7-c9b2-301f-9f1e7bf68fab"), "ar", new Guid("a0202a65-d2f4-39ba-03e6-8dcdcd16142f"), "تحميل «{0}» في «{1}»؟" },
                    { new Guid("4ab73b47-f1e4-7bdd-d1e1-2996994036cc"), "ar", new Guid("209e222a-49d8-9221-0063-0860a884c8f3"), "هذا ليس ملف مشروع من REDIX." },
                    { new Guid("520393fa-d197-5c87-2074-5348e6810c30"), "en", new Guid("122146e1-dbae-9615-6c0f-a578c252a24f"), "The account “{0}” is not in the project." },
                    { new Guid("61590136-4dbc-48fa-3653-0de036d0341c"), "ar", new Guid("122146e1-dbae-9615-6c0f-a578c252a24f"), "الحساب «{0}» غير موجود في المشروع." },
                    { new Guid("71195d05-95f9-83d6-e3bc-da41d870cf5d"), "ar", new Guid("96cac967-ac07-9741-d90c-fe5c4282d798"), "الملف لا يحمل المرفقات؛ اترك «المرفق» فارغًا." },
                    { new Guid("726fb0e2-153b-d980-1dd3-a6af4911c6c8"), "ar", new Guid("68df6719-94a4-e395-1221-5e218553b1a0"), "لم يُحفظ شيء. صحّح هذه الأسطر في الملف ثم حمّله مرة أخرى:" },
                    { new Guid("73be7f52-d1fb-6edf-1a93-83147fc00ff6"), "en", new Guid("9677cc1f-81f0-8b20-61b2-7ae7493c4a07"), "Missing tables are added, the entries are recorded and approved in your name, the actual balances entered and the file's report issued. Nothing is saved if one row is refused." },
                    { new Guid("746f2ac7-d210-2b5d-ab76-eb6350b1d200"), "en", new Guid("330f528c-a81f-ef85-6f8c-4bdada29dbda"), "“{0}” is not one of the choices of “{1}”." },
                    { new Guid("7c1d4aee-d66d-0e0b-b0e0-af43e0c4547e"), "en", new Guid("96cac967-ac07-9741-d90c-fe5c4282d798"), "The file carries no attachments; leave «المرفق» empty." },
                    { new Guid("802294dd-ef09-03ce-43bd-8f8ecd39b5f6"), "ar", new Guid("79ed99ec-782f-d5f7-3378-9e1eee018c80"), "في المشروع قيود، فلا يُحمّل فيه ملف." },
                    { new Guid("869f7aac-ec3d-3ca4-95d2-5d9f3050f71e"), "en", new Guid("f360b8e0-a251-dbc1-98c0-c913cbb85854"), "File loaded: {0} entries in {1} tables, the actual balances, and report {2}." },
                    { new Guid("90bf7771-9dee-9982-20b5-ab6ab6b713e7"), "en", new Guid("b4a6fbe0-87f2-acc4-009c-f82fea364c26"), "Load the project file" },
                    { new Guid("c389d28f-2714-6fb5-f536-8e9e64220894"), "en", new Guid("79ed99ec-782f-d5f7-3378-9e1eee018c80"), "The project already holds entries, so no file can be loaded into it." },
                    { new Guid("dbd7311a-7b25-d3c0-5eb5-e188a494e4a4"), "ar", new Guid("1f276524-41cb-b102-59f0-cee30a6b7cdc"), "الملف من إصدار أحدث من REDIX." },
                    { new Guid("e61c43e4-8763-9ce4-688a-b7dea0e11097"), "ar", new Guid("9677cc1f-81f0-8b20-61b2-7ae7493c4a07"), "تُضاف الجداول الناقصة، وتُسجَّل القيود وتُعتمد باسمك، وتُدخل الأرصدة الفعلية، ويصدر تقرير الملف. لا يُحفظ شيء إن رُفض سطر واحد." },
                    { new Guid("ea4502a7-ce08-b6a6-166d-4f843ad688b5"), "ar", new Guid("f360b8e0-a251-dbc1-98c0-c913cbb85854"), "حُمّل الملف: {0} قيدًا في {1} جدولًا، والأرصدة الفعلية، والتقرير {2}." },
                    { new Guid("f1287c7f-9ebb-365a-f403-9123ae3ef0ca"), "en", new Guid("209e222a-49d8-9221-0063-0860a884c8f3"), "This is not a REDIX project file." },
                    { new Guid("fd6ab9e8-80c9-8441-d3fb-4cc8cf8b30c3"), "en", new Guid("68df6719-94a4-e395-1221-5e218553b1a0"), "Nothing was saved. Fix these rows in the file, then load it again:" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06936da7-828e-e93a-8934-adac3550f783"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20e2a8bb-134f-f968-8a03-224d0c5c447e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2736b046-a056-5f0f-e023-efd0de2e3b69"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b9bcea8-c3c7-3e6a-fed1-e7a431685418"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49c44264-61b7-c9b2-301f-9f1e7bf68fab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ab73b47-f1e4-7bdd-d1e1-2996994036cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("520393fa-d197-5c87-2074-5348e6810c30"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61590136-4dbc-48fa-3653-0de036d0341c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71195d05-95f9-83d6-e3bc-da41d870cf5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("726fb0e2-153b-d980-1dd3-a6af4911c6c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73be7f52-d1fb-6edf-1a93-83147fc00ff6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("746f2ac7-d210-2b5d-ab76-eb6350b1d200"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7c1d4aee-d66d-0e0b-b0e0-af43e0c4547e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("802294dd-ef09-03ce-43bd-8f8ecd39b5f6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("869f7aac-ec3d-3ca4-95d2-5d9f3050f71e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90bf7771-9dee-9982-20b5-ab6ab6b713e7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c389d28f-2714-6fb5-f536-8e9e64220894"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dbd7311a-7b25-d3c0-5eb5-e188a494e4a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e61c43e4-8763-9ce4-688a-b7dea0e11097"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea4502a7-ce08-b6a6-166d-4f843ad688b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f1287c7f-9ebb-365a-f403-9123ae3ef0ca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd6ab9e8-80c9-8441-d3fb-4cc8cf8b30c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("122146e1-dbae-9615-6c0f-a578c252a24f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1f276524-41cb-b102-59f0-cee30a6b7cdc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("209e222a-49d8-9221-0063-0860a884c8f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("330f528c-a81f-ef85-6f8c-4bdada29dbda"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("68df6719-94a4-e395-1221-5e218553b1a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79ed99ec-782f-d5f7-3378-9e1eee018c80"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9677cc1f-81f0-8b20-61b2-7ae7493c4a07"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("96cac967-ac07-9741-d90c-fe5c4282d798"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a0202a65-d2f4-39ba-03e6-8dcdcd16142f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4a6fbe0-87f2-acc4-009c-f82fea364c26"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f360b8e0-a251-dbc1-98c0-c913cbb85854"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "", "projects.importSettings" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("066e36b0-f0c2-e3ac-88ab-5c0fa06d8688"), "en", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "Import project settings" },
                    { new Guid("9ea691bc-4176-625f-6d08-f3a9ad22a4f5"), "ar", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "استيراد إعدادات المشروع" }
                });
        }
    }
}
