using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Projects",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "Description",
                value: "");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "", "projectInfo.descriptionHint" },
                    { new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "", "projectInfo.saved" },
                    { new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "", "projectInfo.save" },
                    { new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "", "register.invalidDescription" },
                    { new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "", "projects.invalidDescription" },
                    { new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "", "projectInfo.title" },
                    { new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "", "projectInfo.noDescription" },
                    { new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "", "projects.description" },
                    { new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "", "projectInfo.description" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0dd3117a-bc8f-045b-0666-990256916994"), "en", new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "More info" },
                    { new Guid("35d9c4dd-ace8-15c7-6b9e-9eadb39adf1d"), "en", new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "Description saved." },
                    { new Guid("443a77c0-8ef8-fa94-ade1-6086bf39ab58"), "ar", new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "وصف المشروع (اختياري)" },
                    { new Guid("4dc21e06-42a9-6f17-d96f-ed405c48837b"), "ar", new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "حفظ الوصف" },
                    { new Guid("5284c6cd-ad42-2fe2-758b-ab26b6604296"), "en", new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "Description (optional)" },
                    { new Guid("58ce52c3-cee6-964d-4d1f-94c1f0f198ac"), "ar", new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "وصف المشروع" },
                    { new Guid("5adf10df-28fe-3666-c786-7971235bfff9"), "en", new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "Write at most 2,000 characters." },
                    { new Guid("71291288-a49c-e478-8098-df8320952222"), "en", new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "Save description" },
                    { new Guid("93e20883-0a9f-f218-9a4c-4980016e9729"), "ar", new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "لا يوجد وصف بعد." },
                    { new Guid("9670223c-b75d-cd3d-733e-8f7ed2affae2"), "ar", new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "اكتب 2000 حرف على الأكثر." },
                    { new Guid("9c0826bb-a284-807a-ab76-29915cc13f90"), "ar", new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "معلومات المشروع" },
                    { new Guid("c34eaa54-94a9-7c67-77c9-b947237ec412"), "en", new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "What the project is, for everyone in it. Up to 2,000 characters." },
                    { new Guid("c8c1fdac-7014-338a-4fae-9feecf7d82d7"), "en", new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "Description" },
                    { new Guid("cca85eda-03bd-e0c3-b4b6-cf0f8333f803"), "en", new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "No description yet." },
                    { new Guid("dbca40e4-b309-7d4b-bfc1-ba2cc32c009a"), "en", new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "Write at most 2,000 characters." },
                    { new Guid("e0f53c3f-798a-5275-358b-7c227333fb0a"), "ar", new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "تم حفظ الوصف." },
                    { new Guid("f6a9e449-4bfe-3b41-11e9-82896d45db61"), "ar", new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "اكتب 2000 حرف على الأكثر." },
                    { new Guid("fbdb6b97-7753-071c-aa8c-13a058488d70"), "ar", new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "ما هو المشروع، لكل من فيه. حتى 2000 حرف." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0dd3117a-bc8f-045b-0666-990256916994"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("35d9c4dd-ace8-15c7-6b9e-9eadb39adf1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("443a77c0-8ef8-fa94-ade1-6086bf39ab58"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4dc21e06-42a9-6f17-d96f-ed405c48837b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5284c6cd-ad42-2fe2-758b-ab26b6604296"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("58ce52c3-cee6-964d-4d1f-94c1f0f198ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5adf10df-28fe-3666-c786-7971235bfff9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71291288-a49c-e478-8098-df8320952222"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93e20883-0a9f-f218-9a4c-4980016e9729"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9670223c-b75d-cd3d-733e-8f7ed2affae2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c0826bb-a284-807a-ab76-29915cc13f90"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c34eaa54-94a9-7c67-77c9-b947237ec412"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8c1fdac-7014-338a-4fae-9feecf7d82d7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cca85eda-03bd-e0c3-b4b6-cf0f8333f803"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dbca40e4-b309-7d4b-bfc1-ba2cc32c009a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0f53c3f-798a-5275-358b-7c227333fb0a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6a9e449-4bfe-3b41-11e9-82896d45db61"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbdb6b97-7753-071c-aa8c-13a058488d70"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"));

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Projects");
        }
    }
}
