using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Blueprints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blueprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blueprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blueprints_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blueprints_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blueprints_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0a9b82c5-154a-be13-aee9-e0409fcdb1b4"), "", "blueprints.unknownAttachment" },
                    { new Guid("1132e8ef-56a5-78f9-ce59-21836e99b0e6"), "", "blueprints.unknownUnit" },
                    { new Guid("3c67bbbe-17ac-a125-0d31-0c8b8318a061"), "", "blueprints.blueprintAlreadyKept" },
                    { new Guid("3ccc24ec-4eb5-e382-9a1f-bf529970527a"), "", "blueprints.projectFinished" },
                    { new Guid("4aec8e9a-e9fa-296a-3bc8-d21421d35527"), "", "blueprints.removed" },
                    { new Guid("4c265a19-5eee-9e94-fe32-147f5a37c2c3"), "", "blueprints.removeExplain" },
                    { new Guid("55209840-a152-fa3f-6768-b0e0131dd18b"), "", "blueprints.remove" },
                    { new Guid("6122e7b1-dd4f-0861-f9a5-a373ca3715e2"), "", "blueprints.saved" },
                    { new Guid("6a5342c9-2ce5-69ab-daf9-b065e7213fa8"), "", "blueprints.none" },
                    { new Guid("6f1e3dd1-9eb9-fa32-2d40-b86321ae8600"), "", "blueprints.save" },
                    { new Guid("6f64c628-1215-b122-f40e-9276c9ddd670"), "", "blueprints.invalidTitle" },
                    { new Guid("73b03e1c-2d18-732c-4af3-2df3573c821f"), "", "blueprints.refused" },
                    { new Guid("8748b042-883f-d596-d36c-44b904250aa4"), "", "blueprints.for" },
                    { new Guid("9518c738-75a2-f404-6254-eeef0dcc6dac"), "", "blueprints.project" },
                    { new Guid("b1dba068-3897-3042-646d-795240b739aa"), "", "blueprints.titleField" },
                    { new Guid("cf0b24d5-3b1c-5d1a-d991-7382431f3514"), "", "blueprints.file" },
                    { new Guid("d40c2ff8-fad4-b516-30b1-4b798634010c"), "", "blueprints.removeQuestion" },
                    { new Guid("e6fbfc8d-c294-393f-a39e-406a0c0bb684"), "", "blueprints.loadFailed" },
                    { new Guid("eccbe712-90d1-8a3f-5be2-127115802b51"), "", "blueprints.title" },
                    { new Guid("f03e9671-a0ff-01d8-69c0-a9cc7ff37271"), "", "blueprints.noConnection" },
                    { new Guid("f3e83606-58ca-d917-e1b9-567d2a0d5a02"), "", "blueprints.add" },
                    { new Guid("ff78fbd0-ee46-2d52-c35c-456bd382dd18"), "", "blueprints.fileHint" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01ce0aa5-751c-77c0-4010-73a959077d8a"), "en", new Guid("4aec8e9a-e9fa-296a-3bc8-d21421d35527"), "The blueprint is removed from the list." },
                    { new Guid("0469f645-b307-939d-46c1-b1ca86140ae9"), "en", new Guid("cf0b24d5-3b1c-5d1a-d991-7382431f3514"), "Choose the file" },
                    { new Guid("09089290-5de8-4240-a478-03dcde651bad"), "en", new Guid("e6fbfc8d-c294-393f-a39e-406a0c0bb684"), "The blueprints could not be read." },
                    { new Guid("0de37523-ef57-b40d-ab8b-858b864426ec"), "ar", new Guid("9518c738-75a2-f404-6254-eeef0dcc6dac"), "المشروع كاملاً" },
                    { new Guid("0eb68ecc-823c-9911-e5ae-579dfa6a78ae"), "en", new Guid("6122e7b1-dd4f-0861-f9a5-a373ca3715e2"), "The blueprint is kept." },
                    { new Guid("189442eb-ce34-191a-27d8-f07785176728"), "ar", new Guid("ff78fbd0-ee46-2d52-c35c-456bd382dd18"), "ملف PDF، أو صورة أصغر من 1 ميغابايت." },
                    { new Guid("21509e1c-d0c3-2f01-0b82-04972867d5f8"), "ar", new Guid("0a9b82c5-154a-be13-aee9-e0409fcdb1b4"), "اختر ملف المخطط أولاً." },
                    { new Guid("37846440-3c47-555c-e16d-4e1a6d8e266c"), "ar", new Guid("3c67bbbe-17ac-a125-0d31-0c8b8318a061"), "هذا الملف محفوظ كمخطط من قبل." },
                    { new Guid("413b4277-5e73-b755-9abb-f6fbca376981"), "en", new Guid("6a5342c9-2ce5-69ab-daf9-b065e7213fa8"), "No blueprint is kept yet." },
                    { new Guid("4992b91a-05dc-c0f5-642a-673c0200adb3"), "ar", new Guid("e6fbfc8d-c294-393f-a39e-406a0c0bb684"), "تعذّرت قراءة المخططات." },
                    { new Guid("4fd3a414-3e37-fff4-207d-1bee622bf8e3"), "en", new Guid("d40c2ff8-fad4-b516-30b1-4b798634010c"), "Remove this blueprint from the list?" },
                    { new Guid("503cecd8-df01-7d83-c430-594c6759bf67"), "en", new Guid("6f64c628-1215-b122-f40e-9276c9ddd670"), "Say what the blueprint shows, in 200 letters at most." },
                    { new Guid("578a3a51-d844-5eea-06cc-c9a5008a4230"), "ar", new Guid("b1dba068-3897-3042-646d-795240b739aa"), "عنوان المخطط" },
                    { new Guid("5b43e7fd-109a-e3e0-56aa-f20dd019615c"), "ar", new Guid("55209840-a152-fa3f-6768-b0e0131dd18b"), "إزالة من القائمة" },
                    { new Guid("617810c2-057f-1430-4220-f898294c7029"), "ar", new Guid("3ccc24ec-4eb5-e382-9a1f-bf529970527a"), "هذا المشروع مغلق." },
                    { new Guid("63219b41-6162-a11b-f448-d3377e91825b"), "en", new Guid("ff78fbd0-ee46-2d52-c35c-456bd382dd18"), "A PDF, or a picture under 1 MB." },
                    { new Guid("69a2b9da-ea2a-6835-be78-a8fa93ec1fa9"), "ar", new Guid("4c265a19-5eee-9e94-fe32-147f5a37c2c3"), "يبقى ملفه محفوظاً كسائر الملفات." },
                    { new Guid("6aa5fb1d-0f51-b034-3298-6464974f4535"), "en", new Guid("0a9b82c5-154a-be13-aee9-e0409fcdb1b4"), "Choose the blueprint's file first." },
                    { new Guid("6b66abc2-481d-2d86-2aa7-ce1e1af2aa03"), "en", new Guid("1132e8ef-56a5-78f9-ce59-21836e99b0e6"), "Choose the project or one of its units." },
                    { new Guid("73a89b9d-0faa-6abd-c95c-91cde881a2a5"), "en", new Guid("3c67bbbe-17ac-a125-0d31-0c8b8318a061"), "This file is already kept as a blueprint." },
                    { new Guid("757ad734-796c-8b00-8fc9-44f08036f640"), "en", new Guid("b1dba068-3897-3042-646d-795240b739aa"), "What it shows" },
                    { new Guid("7c8a52cb-088f-7b81-037c-b4affd824209"), "ar", new Guid("6f1e3dd1-9eb9-fa32-2d40-b86321ae8600"), "حفظ" },
                    { new Guid("8164f9bf-03e0-ccb0-7b94-1a5b7f0737e0"), "en", new Guid("6f1e3dd1-9eb9-fa32-2d40-b86321ae8600"), "Keep" },
                    { new Guid("8715f26b-8038-1069-6a0f-6c378d2f8539"), "en", new Guid("8748b042-883f-d596-d36c-44b904250aa4"), "For" },
                    { new Guid("8bc477ab-d68a-2b96-465f-bdae34826762"), "en", new Guid("73b03e1c-2d18-732c-4af3-2df3573c821f"), "This could not be done." },
                    { new Guid("8d4f7964-619a-336e-16ac-a5ef65634d35"), "en", new Guid("9518c738-75a2-f404-6254-eeef0dcc6dac"), "The whole project" },
                    { new Guid("90913383-3f47-8283-b483-7b7d4ee0a548"), "en", new Guid("eccbe712-90d1-8a3f-5be2-127115802b51"), "Blueprints" },
                    { new Guid("9a4bc5e1-2ccd-fc7c-335f-caab576ace11"), "ar", new Guid("d40c2ff8-fad4-b516-30b1-4b798634010c"), "إزالة هذا المخطط من القائمة؟" },
                    { new Guid("9f017cf5-fb5f-c26f-3f21-2285a8450560"), "en", new Guid("f03e9671-a0ff-01d8-69c0-a9cc7ff37271"), "No connection. Try again when online." },
                    { new Guid("a3764f12-b69b-e88e-8c71-5eaf8ec36045"), "ar", new Guid("8748b042-883f-d596-d36c-44b904250aa4"), "خاص بـ" },
                    { new Guid("a86df643-bfe1-b81c-c1a9-f1ddaea9fc43"), "ar", new Guid("eccbe712-90d1-8a3f-5be2-127115802b51"), "المخططات" },
                    { new Guid("aed4df0f-62f0-3e04-4df7-60bb5038325c"), "ar", new Guid("73b03e1c-2d18-732c-4af3-2df3573c821f"), "تعذّر تنفيذ ذلك." },
                    { new Guid("b473767f-b751-1672-500e-04cf5c2901df"), "ar", new Guid("6f64c628-1215-b122-f40e-9276c9ddd670"), "اذكر عنوان المخطط في 200 حرف على الأكثر." },
                    { new Guid("c1158e18-17ad-4aea-ce28-3b9c613a4886"), "ar", new Guid("6122e7b1-dd4f-0861-f9a5-a373ca3715e2"), "تم حفظ المخطط." },
                    { new Guid("c2fa3b8d-bd50-4f4e-6322-3c103fcaa488"), "en", new Guid("3ccc24ec-4eb5-e382-9a1f-bf529970527a"), "This project is closed." },
                    { new Guid("cb1709ab-c26f-30cc-4dd3-6395f6d6c610"), "ar", new Guid("f03e9671-a0ff-01d8-69c0-a9cc7ff37271"), "لا يوجد اتصال. حاول مرة أخرى عند الاتصال." },
                    { new Guid("ccf4f31c-7cdc-407e-674c-fa229bf03e1f"), "en", new Guid("55209840-a152-fa3f-6768-b0e0131dd18b"), "Remove from the list" },
                    { new Guid("cd2f0990-8702-6421-0ab8-e058a31d3f56"), "ar", new Guid("1132e8ef-56a5-78f9-ce59-21836e99b0e6"), "اختر المشروع أو إحدى وحداته." },
                    { new Guid("d7a982d9-f5ef-63d3-f3d1-4089328f6b57"), "en", new Guid("4c265a19-5eee-9e94-fe32-147f5a37c2c3"), "Its file stays kept, as every file does." },
                    { new Guid("db2279c2-cd1b-0783-3736-424368419f84"), "en", new Guid("f3e83606-58ca-d917-e1b9-567d2a0d5a02"), "Add a blueprint" },
                    { new Guid("e5f3031b-f79e-ad2d-d733-02cb3ac014e2"), "ar", new Guid("cf0b24d5-3b1c-5d1a-d991-7382431f3514"), "اختر الملف" },
                    { new Guid("edf86e67-b5f1-3fd2-013e-ca363ee85af3"), "ar", new Guid("6a5342c9-2ce5-69ab-daf9-b065e7213fa8"), "لا توجد مخططات بعد." },
                    { new Guid("ee6645e4-902f-3b15-d367-2b94a6e32279"), "ar", new Guid("4aec8e9a-e9fa-296a-3bc8-d21421d35527"), "تمت إزالة المخطط من القائمة." },
                    { new Guid("fcaece09-bdcc-a7f4-84e5-e4df19383fc0"), "ar", new Guid("f3e83606-58ca-d917-e1b9-567d2a0d5a02"), "إضافة مخطط" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blueprints_AttachmentId",
                table: "Blueprints",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Blueprints_ProjectId_AttachmentId",
                table: "Blueprints",
                columns: new[] { "ProjectId", "AttachmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blueprints_UploadedByUserId",
                table: "Blueprints",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blueprints");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01ce0aa5-751c-77c0-4010-73a959077d8a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0469f645-b307-939d-46c1-b1ca86140ae9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09089290-5de8-4240-a478-03dcde651bad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0de37523-ef57-b40d-ab8b-858b864426ec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0eb68ecc-823c-9911-e5ae-579dfa6a78ae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("189442eb-ce34-191a-27d8-f07785176728"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("21509e1c-d0c3-2f01-0b82-04972867d5f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37846440-3c47-555c-e16d-4e1a6d8e266c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("413b4277-5e73-b755-9abb-f6fbca376981"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4992b91a-05dc-c0f5-642a-673c0200adb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fd3a414-3e37-fff4-207d-1bee622bf8e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("503cecd8-df01-7d83-c430-594c6759bf67"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("578a3a51-d844-5eea-06cc-c9a5008a4230"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b43e7fd-109a-e3e0-56aa-f20dd019615c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("617810c2-057f-1430-4220-f898294c7029"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63219b41-6162-a11b-f448-d3377e91825b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("69a2b9da-ea2a-6835-be78-a8fa93ec1fa9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6aa5fb1d-0f51-b034-3298-6464974f4535"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b66abc2-481d-2d86-2aa7-ce1e1af2aa03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73a89b9d-0faa-6abd-c95c-91cde881a2a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("757ad734-796c-8b00-8fc9-44f08036f640"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7c8a52cb-088f-7b81-037c-b4affd824209"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8164f9bf-03e0-ccb0-7b94-1a5b7f0737e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8715f26b-8038-1069-6a0f-6c378d2f8539"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8bc477ab-d68a-2b96-465f-bdae34826762"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8d4f7964-619a-336e-16ac-a5ef65634d35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90913383-3f47-8283-b483-7b7d4ee0a548"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a4bc5e1-2ccd-fc7c-335f-caab576ace11"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f017cf5-fb5f-c26f-3f21-2285a8450560"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a3764f12-b69b-e88e-8c71-5eaf8ec36045"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a86df643-bfe1-b81c-c1a9-f1ddaea9fc43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aed4df0f-62f0-3e04-4df7-60bb5038325c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b473767f-b751-1672-500e-04cf5c2901df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1158e18-17ad-4aea-ce28-3b9c613a4886"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2fa3b8d-bd50-4f4e-6322-3c103fcaa488"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb1709ab-c26f-30cc-4dd3-6395f6d6c610"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ccf4f31c-7cdc-407e-674c-fa229bf03e1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd2f0990-8702-6421-0ab8-e058a31d3f56"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7a982d9-f5ef-63d3-f3d1-4089328f6b57"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db2279c2-cd1b-0783-3736-424368419f84"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5f3031b-f79e-ad2d-d733-02cb3ac014e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("edf86e67-b5f1-3fd2-013e-ca363ee85af3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee6645e4-902f-3b15-d367-2b94a6e32279"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcaece09-bdcc-a7f4-84e5-e4df19383fc0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0a9b82c5-154a-be13-aee9-e0409fcdb1b4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1132e8ef-56a5-78f9-ce59-21836e99b0e6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3c67bbbe-17ac-a125-0d31-0c8b8318a061"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3ccc24ec-4eb5-e382-9a1f-bf529970527a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4aec8e9a-e9fa-296a-3bc8-d21421d35527"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4c265a19-5eee-9e94-fe32-147f5a37c2c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("55209840-a152-fa3f-6768-b0e0131dd18b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6122e7b1-dd4f-0861-f9a5-a373ca3715e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6a5342c9-2ce5-69ab-daf9-b065e7213fa8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f1e3dd1-9eb9-fa32-2d40-b86321ae8600"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f64c628-1215-b122-f40e-9276c9ddd670"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("73b03e1c-2d18-732c-4af3-2df3573c821f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8748b042-883f-d596-d36c-44b904250aa4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9518c738-75a2-f404-6254-eeef0dcc6dac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b1dba068-3897-3042-646d-795240b739aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cf0b24d5-3b1c-5d1a-d991-7382431f3514"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d40c2ff8-fad4-b516-30b1-4b798634010c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e6fbfc8d-c294-393f-a39e-406a0c0bb684"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eccbe712-90d1-8a3f-5be2-127115802b51"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f03e9671-a0ff-01d8-69c0-a9cc7ff37271"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f3e83606-58ca-d917-e1b9-567d2a0d5a02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff78fbd0-ee46-2d52-c35c-456bd382dd18"));
        }
    }
}
