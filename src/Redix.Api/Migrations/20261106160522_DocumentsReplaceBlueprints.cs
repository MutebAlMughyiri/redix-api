using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DocumentsReplaceBlueprints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Blueprints_Attachments_AttachmentId", table: "Blueprints");
            migrationBuilder.DropForeignKey(name: "FK_Blueprints_Projects_ProjectId", table: "Blueprints");
            migrationBuilder.DropForeignKey(name: "FK_Blueprints_Users_UploadedByUserId", table: "Blueprints");
            migrationBuilder.DropPrimaryKey(name: "PK_Blueprints", table: "Blueprints");
            migrationBuilder.DropColumn(name: "UnitId", table: "Blueprints");
            migrationBuilder.RenameTable(name: "Blueprints", newName: "Documents");
            migrationBuilder.RenameColumn(name: "Title", table: "Documents", newName: "Name");
            migrationBuilder.RenameIndex(name: "IX_Blueprints_AttachmentId", table: "Documents", newName: "IX_Documents_AttachmentId");
            migrationBuilder.RenameIndex(name: "IX_Blueprints_ProjectId_AttachmentId", table: "Documents", newName: "IX_Documents_ProjectId_AttachmentId");
            migrationBuilder.RenameIndex(name: "IX_Blueprints_UploadedByUserId", table: "Documents", newName: "IX_Documents_UploadedByUserId");
            migrationBuilder.AddColumn<string>(name: "Group", table: "Documents", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "");
            migrationBuilder.AddPrimaryKey(name: "PK_Documents", table: "Documents", column: "Id");
            migrationBuilder.AddForeignKey(name: "FK_Documents_Attachments_AttachmentId", table: "Documents", column: "AttachmentId", principalTable: "Attachments", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Documents_Projects_ProjectId", table: "Documents", column: "ProjectId", principalTable: "Projects", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Documents_Users_UploadedByUserId", table: "Documents", column: "UploadedByUserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "", "documents.projectFinished" },
                    { new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "", "documents.save" },
                    { new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "", "documents.loadFailed" },
                    { new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "", "documents.fileHint" },
                    { new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "", "documents.add" },
                    { new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "", "documents.fileTooLarge" },
                    { new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "", "documents.group" },
                    { new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "", "documents.refused" },
                    { new Guid("68500f85-3483-5951-a047-618c73bc9477"), "", "documents.title" },
                    { new Guid("6f109267-5cbe-944b-90db-082cd706d0bc"), "", "documents.removed" },
                    { new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "", "documents.file" },
                    { new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "", "documents.saved" },
                    { new Guid("941f1467-59ae-ae24-3970-1c18d4746456"), "", "documents.removeExplain" },
                    { new Guid("a050fc9a-315b-24e2-f181-3b8fb2b3fc2a"), "", "documents.removeQuestion" },
                    { new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "", "documents.invalidName" },
                    { new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "", "documents.fileRequired" },
                    { new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "", "documents.none" },
                    { new Guid("cad4486c-357c-6ce3-b37c-106c3bd591ed"), "", "documents.remove" },
                    { new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "", "documents.invalidGroup" },
                    { new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "", "documents.documentAlreadyKept" },
                    { new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "", "documents.groupHint" },
                    { new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "", "documents.name" },
                    { new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "", "documents.noConnection" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0495e296-76a3-7f2d-6095-99e3403c7a72"), "en", new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "Group (optional)" },
                    { new Guid("066b040f-742a-34d0-ec4a-c186fb4afe0b"), "en", new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "No connection. Try again when online." },
                    { new Guid("15d50fa4-ef16-7294-3d21-ad604e4e7c0c"), "ar", new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "لا توجد مستندات بعد." },
                    { new Guid("1f1084ef-11f8-9df5-e5e3-ce743b8bdc4c"), "ar", new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "يجب أن يكون المستند أصغر من 25 ميغابايت." },
                    { new Guid("1f77ea74-bed9-79bf-ff3f-2d1ff54c117b"), "en", new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "No document is kept yet." },
                    { new Guid("25958a45-636a-ade3-55e1-23a2f6d2f54e"), "ar", new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "أي ملف أصغر من 25 ميغابايت." },
                    { new Guid("2e8f0b1c-564a-71e9-0c87-eddeb3dbedb7"), "en", new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "This project is closed." },
                    { new Guid("3521bdfb-9682-8d84-ff2c-617976ec9c64"), "ar", new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "تم حفظ المستند." },
                    { new Guid("36b06183-7d8d-53f1-84db-bf1bb1ad8127"), "ar", new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "اكتب اسم المستند في 200 حرف على الأكثر." },
                    { new Guid("39132977-0184-7595-23c6-0eceed12cd78"), "en", new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "This could not be done." },
                    { new Guid("39290c37-9535-99cc-8a6b-35e76c645283"), "ar", new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "تعذّر تنفيذ ذلك." },
                    { new Guid("39faff0b-ad09-11bf-0780-5f7a50f0c8d4"), "en", new Guid("cad4486c-357c-6ce3-b37c-106c3bd591ed"), "Remove from the list" },
                    { new Guid("486f5007-354e-5cde-fed4-214f32923336"), "en", new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "Give the document a name, in 200 letters at most." },
                    { new Guid("4ce36ed0-86e8-249f-0599-583bad82b688"), "en", new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "A document must be smaller than 25 MB." },
                    { new Guid("50aa967c-017d-15cf-cb7f-91fc7f4c8bd5"), "en", new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "The documents could not be read." },
                    { new Guid("548bc422-73b8-dab0-577a-47f7c8601ba6"), "en", new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "Keep" },
                    { new Guid("62b48a3f-caf9-2d3a-690b-df740cb0ca33"), "ar", new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "اكتب التصنيف في 100 حرف على الأكثر." },
                    { new Guid("6637e359-3cbd-6721-679a-1a9bd73ed6bf"), "ar", new Guid("cad4486c-357c-6ce3-b37c-106c3bd591ed"), "إزالة من القائمة" },
                    { new Guid("73ff9d1f-755d-294f-e431-ba0032867da0"), "en", new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "Choose the document's file first." },
                    { new Guid("84c15413-b773-41ee-90ec-58fe36b8ab4d"), "en", new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "Choose the file" },
                    { new Guid("85cd55c0-1fa0-c1df-0c8e-68ac47526a74"), "en", new Guid("941f1467-59ae-ae24-3970-1c18d4746456"), "Its file stays kept, as every file does." },
                    { new Guid("879f3ae7-53de-7cb9-73d3-b30a59a22285"), "ar", new Guid("941f1467-59ae-ae24-3970-1c18d4746456"), "يبقى ملفه محفوظاً كسائر الملفات." },
                    { new Guid("97f4c6f5-779f-a628-7939-cbc37c60496d"), "ar", new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "التصنيف (اختياري)" },
                    { new Guid("9851423d-3fcb-3bbd-f6bc-567c5d7330b0"), "en", new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "The document is kept." },
                    { new Guid("9dc2e435-783d-e7c0-b7e2-4495e0daf5f7"), "ar", new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "لا يوجد اتصال. حاول مرة أخرى عند الاتصال." },
                    { new Guid("a5381c88-a2ca-e791-31dd-92b6a440e978"), "ar", new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "تظهر مستندات التصنيف الواحد معاً تحت اسمه." },
                    { new Guid("a69839b2-8b2d-b69c-9d4e-6f96224a1dcc"), "ar", new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "هذا المشروع مغلق." },
                    { new Guid("a7369150-03f1-bc5e-2543-eb77771c1621"), "en", new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "This file is already kept as a document." },
                    { new Guid("ad109564-a804-084c-d974-68775e98883d"), "ar", new Guid("6f109267-5cbe-944b-90db-082cd706d0bc"), "تمت إزالة المستند من القائمة." },
                    { new Guid("b0695acc-5528-7f25-ae5c-150a4d6020cb"), "en", new Guid("68500f85-3483-5951-a047-618c73bc9477"), "Documents" },
                    { new Guid("b1792b8a-a1e3-89c2-f37c-f259114c181f"), "ar", new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "هذا الملف محفوظ كمستند من قبل." },
                    { new Guid("b3a6474e-638a-a21a-f02a-9ca69cb912aa"), "en", new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "Name the group in 100 letters at most." },
                    { new Guid("c273214f-2ba7-4ea2-46a5-efb2ea550078"), "ar", new Guid("a050fc9a-315b-24e2-f181-3b8fb2b3fc2a"), "إزالة هذا المستند من القائمة؟" },
                    { new Guid("c61ae08f-d679-9668-1872-76d337dbdcd6"), "en", new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "Documents of one group show together under its name." },
                    { new Guid("c97c5262-719b-0e92-7a7e-a0bc7063aae1"), "ar", new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "تعذّرت قراءة المستندات." },
                    { new Guid("c9aa1bbc-849a-9597-1ed4-fd2c2a12f1eb"), "en", new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "Any file under 25 MB." },
                    { new Guid("cc05b2f4-4642-dacd-5d54-6dce22b3999f"), "ar", new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "إضافة مستند" },
                    { new Guid("ce993095-af66-6b79-ddee-0bc79c52bfa1"), "ar", new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "حفظ" },
                    { new Guid("d78a7206-209e-3a8e-a670-5541f32e20f9"), "ar", new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "اختر ملف المستند أولاً." },
                    { new Guid("dc6ccf6a-e9db-1982-9f95-901dad2c1735"), "en", new Guid("a050fc9a-315b-24e2-f181-3b8fb2b3fc2a"), "Remove this document from the list?" },
                    { new Guid("dcd05699-76db-cf6a-3a42-abda97b739fd"), "en", new Guid("6f109267-5cbe-944b-90db-082cd706d0bc"), "The document is removed from the list." },
                    { new Guid("e8040fc1-e214-a8c5-dd64-7c22c78e8c09"), "ar", new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "اسم المستند" },
                    { new Guid("eb40b015-7cd0-8bbb-2498-79d928b5bd53"), "ar", new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "اختر الملف" },
                    { new Guid("f73af20e-80ba-4eca-7421-89eeb1918f6d"), "en", new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "Add a document" },
                    { new Guid("fa0c92da-a754-b892-d9fe-3c2942d9f0c2"), "en", new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "Its name" },
                    { new Guid("fb3f132b-2ca9-03de-c872-bc5acf3909da"), "ar", new Guid("68500f85-3483-5951-a047-618c73bc9477"), "المستندات" }
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Documents_Attachments_AttachmentId", table: "Documents");
            migrationBuilder.DropForeignKey(name: "FK_Documents_Projects_ProjectId", table: "Documents");
            migrationBuilder.DropForeignKey(name: "FK_Documents_Users_UploadedByUserId", table: "Documents");
            migrationBuilder.DropPrimaryKey(name: "PK_Documents", table: "Documents");
            migrationBuilder.DropColumn(name: "Group", table: "Documents");
            migrationBuilder.RenameTable(name: "Documents", newName: "Blueprints");
            migrationBuilder.RenameColumn(name: "Name", table: "Blueprints", newName: "Title");
            migrationBuilder.RenameIndex(name: "IX_Documents_AttachmentId", table: "Blueprints", newName: "IX_Blueprints_AttachmentId");
            migrationBuilder.RenameIndex(name: "IX_Documents_ProjectId_AttachmentId", table: "Blueprints", newName: "IX_Blueprints_ProjectId_AttachmentId");
            migrationBuilder.RenameIndex(name: "IX_Documents_UploadedByUserId", table: "Blueprints", newName: "IX_Blueprints_UploadedByUserId");
            migrationBuilder.AddColumn<Guid>(name: "UnitId", table: "Blueprints", type: "uniqueidentifier", nullable: false, defaultValue: Guid.Empty);
            migrationBuilder.AddPrimaryKey(name: "PK_Blueprints", table: "Blueprints", column: "Id");
            migrationBuilder.AddForeignKey(name: "FK_Blueprints_Attachments_AttachmentId", table: "Blueprints", column: "AttachmentId", principalTable: "Attachments", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Blueprints_Projects_ProjectId", table: "Blueprints", column: "ProjectId", principalTable: "Projects", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Blueprints_Users_UploadedByUserId", table: "Blueprints", column: "UploadedByUserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0495e296-76a3-7f2d-6095-99e3403c7a72"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("066b040f-742a-34d0-ec4a-c186fb4afe0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15d50fa4-ef16-7294-3d21-ad604e4e7c0c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f1084ef-11f8-9df5-e5e3-ce743b8bdc4c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f77ea74-bed9-79bf-ff3f-2d1ff54c117b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25958a45-636a-ade3-55e1-23a2f6d2f54e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e8f0b1c-564a-71e9-0c87-eddeb3dbedb7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3521bdfb-9682-8d84-ff2c-617976ec9c64"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36b06183-7d8d-53f1-84db-bf1bb1ad8127"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39132977-0184-7595-23c6-0eceed12cd78"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39290c37-9535-99cc-8a6b-35e76c645283"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39faff0b-ad09-11bf-0780-5f7a50f0c8d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("486f5007-354e-5cde-fed4-214f32923336"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ce36ed0-86e8-249f-0599-583bad82b688"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50aa967c-017d-15cf-cb7f-91fc7f4c8bd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("548bc422-73b8-dab0-577a-47f7c8601ba6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62b48a3f-caf9-2d3a-690b-df740cb0ca33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6637e359-3cbd-6721-679a-1a9bd73ed6bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73ff9d1f-755d-294f-e431-ba0032867da0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("84c15413-b773-41ee-90ec-58fe36b8ab4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85cd55c0-1fa0-c1df-0c8e-68ac47526a74"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("879f3ae7-53de-7cb9-73d3-b30a59a22285"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97f4c6f5-779f-a628-7939-cbc37c60496d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9851423d-3fcb-3bbd-f6bc-567c5d7330b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9dc2e435-783d-e7c0-b7e2-4495e0daf5f7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a5381c88-a2ca-e791-31dd-92b6a440e978"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a69839b2-8b2d-b69c-9d4e-6f96224a1dcc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7369150-03f1-bc5e-2543-eb77771c1621"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ad109564-a804-084c-d974-68775e98883d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0695acc-5528-7f25-ae5c-150a4d6020cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1792b8a-a1e3-89c2-f37c-f259114c181f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b3a6474e-638a-a21a-f02a-9ca69cb912aa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c273214f-2ba7-4ea2-46a5-efb2ea550078"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c61ae08f-d679-9668-1872-76d337dbdcd6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c97c5262-719b-0e92-7a7e-a0bc7063aae1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9aa1bbc-849a-9597-1ed4-fd2c2a12f1eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc05b2f4-4642-dacd-5d54-6dce22b3999f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce993095-af66-6b79-ddee-0bc79c52bfa1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d78a7206-209e-3a8e-a670-5541f32e20f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc6ccf6a-e9db-1982-9f95-901dad2c1735"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dcd05699-76db-cf6a-3a42-abda97b739fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e8040fc1-e214-a8c5-dd64-7c22c78e8c09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb40b015-7cd0-8bbb-2498-79d928b5bd53"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f73af20e-80ba-4eca-7421-89eeb1918f6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa0c92da-a754-b892-d9fe-3c2942d9f0c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb3f132b-2ca9-03de-c872-bc5acf3909da"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18651164-b6f9-a403-1f85-a016015fc38d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37d49fba-1eda-de57-c176-21f08074b25e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("68500f85-3483-5951-a047-618c73bc9477"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f109267-5cbe-944b-90db-082cd706d0bc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("941f1467-59ae-ae24-3970-1c18d4746456"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a050fc9a-315b-24e2-f181-3b8fb2b3fc2a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b868c743-57b3-966c-bf96-67292b30293c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cad4486c-357c-6ce3-b37c-106c3bd591ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"));

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

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

        }
    }
}
