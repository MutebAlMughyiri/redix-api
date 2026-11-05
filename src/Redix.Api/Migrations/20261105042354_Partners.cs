using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Partners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "", "partners.new" },
                    { new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "", "partners.save" },
                    { new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "", "partners.title" },
                    { new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "", "partners.unknownUser" },
                    { new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "", "partners.invalidName" },
                    { new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "", "partners.noLogin" },
                    { new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "", "partners.noConnection" },
                    { new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "", "partners.userTaken" },
                    { new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "", "partners.name" },
                    { new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "", "partners.edit" },
                    { new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "", "partners.loadFailed" },
                    { new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "", "partners.empty" },
                    { new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "", "partners.login" },
                    { new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "", "partners.loginHint" },
                    { new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "", "partners.refused" },
                    { new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "", "partners.projectFinished" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("081a2a29-b1e5-88bc-f6d2-3ea5fbd6cacf"), "ar", new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "إضافة شريك" },
                    { new Guid("120281a5-3274-9341-8634-2243ab566e58"), "ar", new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("183d3872-728e-3d6c-1183-50f72e934090"), "ar", new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "تعديل الشريك" },
                    { new Guid("1b00c567-b3f6-2aeb-73be-225050e44cb9"), "ar", new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("1ba6638c-5385-2d57-0ebb-c5cca4f90ffb"), "ar", new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "بحساب الدخول يرى رأس ماله في هذا المشروع دون غيره." },
                    { new Guid("2317fb4a-267e-1a66-0e25-6ad9a7679716"), "en", new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "Enter a name of 1 to 100 characters." },
                    { new Guid("2334d59c-fb2b-0cb5-082a-042802e4cc7f"), "en", new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "No connection to the server. Try again." },
                    { new Guid("30aafa0c-fe08-5359-fc11-2a58f5724dd1"), "en", new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "No login" },
                    { new Guid("3d3020e9-002e-e254-0252-b2ce4977a76a"), "en", new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "That person is not in this project." },
                    { new Guid("4de5fa8c-00e2-64ad-1ba3-99f49ec65a14"), "en", new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "That login is already another partner's in this project." },
                    { new Guid("56e1972c-425a-0b33-f0a7-6e289ce698dd"), "en", new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "Could not read the partners. Check the network and try again." },
                    { new Guid("5de87bc7-f42c-c035-6afa-f960a91c28d8"), "en", new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "This change was refused." },
                    { new Guid("63dc14e6-9f5c-1095-d06f-6cfcb7ba755f"), "ar", new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "لا يوجد شركاء." },
                    { new Guid("66382a84-cc98-5860-4f0b-11163479a10f"), "en", new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "Edit partner" },
                    { new Guid("7670ffa3-16f0-0c95-dfa1-1bae4b6c1213"), "ar", new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "حفظ" },
                    { new Guid("7dd4e58a-92f6-31cb-2d37-122bff506f60"), "ar", new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "الشركاء" },
                    { new Guid("81d93df7-0d57-1298-acd9-2532ff271c6d"), "en", new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "Add partner" },
                    { new Guid("8762d56e-cd0b-b2a5-5b61-402481900344"), "en", new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "No partners." },
                    { new Guid("878e14cc-7138-534e-699c-4edb6817f77c"), "ar", new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "بدون حساب دخول" },
                    { new Guid("9a112cb9-5f3e-498f-bf95-e13593dc6336"), "en", new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "With a login, they see their own capital in this project and nobody else's." },
                    { new Guid("ac662de6-7f16-d09a-4d2a-6527c2c0a9b2"), "ar", new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "هذا الشخص ليس في هذا المشروع." },
                    { new Guid("adb5f6b8-03d3-7006-8d6e-de86af2c4463"), "en", new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "Partners" },
                    { new Guid("b2b57447-a5d1-43e8-02f3-b5abc254eff1"), "ar", new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "تعذّرت قراءة الشركاء. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("c3a105d6-6e87-5737-dfa3-93a88690d257"), "en", new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "This project is finished, so nothing in it changes." },
                    { new Guid("c8c85611-21fd-2ba7-ceb1-e823a91c8b08"), "ar", new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "اسم الشريك" },
                    { new Guid("cd51533c-c98b-9db6-0722-0a42d0dbe6a6"), "en", new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "Save" },
                    { new Guid("cfe22ac3-c042-7163-7235-2d1d4f9704f1"), "ar", new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "حساب الدخول هذا لشريك آخر في هذا المشروع." },
                    { new Guid("e38ba2fd-e815-7c4b-134e-9702bece859b"), "en", new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "Partner name" },
                    { new Guid("e6ebdc9a-0eef-7821-1fcb-24851593c2fd"), "ar", new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "حساب دخوله" },
                    { new Guid("eeeed37e-ca44-83a6-0d16-1cf0c9cc94e3"), "en", new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "Their login" },
                    { new Guid("f70934cc-417e-17e4-ddac-84742eb16a2d"), "ar", new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("f9f2a38b-5773-971a-573f-976c841d794d"), "ar", new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "رُفض هذا التغيير." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] <> '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("081a2a29-b1e5-88bc-f6d2-3ea5fbd6cacf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("120281a5-3274-9341-8634-2243ab566e58"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("183d3872-728e-3d6c-1183-50f72e934090"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b00c567-b3f6-2aeb-73be-225050e44cb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ba6638c-5385-2d57-0ebb-c5cca4f90ffb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2317fb4a-267e-1a66-0e25-6ad9a7679716"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2334d59c-fb2b-0cb5-082a-042802e4cc7f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("30aafa0c-fe08-5359-fc11-2a58f5724dd1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d3020e9-002e-e254-0252-b2ce4977a76a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4de5fa8c-00e2-64ad-1ba3-99f49ec65a14"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56e1972c-425a-0b33-f0a7-6e289ce698dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5de87bc7-f42c-c035-6afa-f960a91c28d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63dc14e6-9f5c-1095-d06f-6cfcb7ba755f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("66382a84-cc98-5860-4f0b-11163479a10f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7670ffa3-16f0-0c95-dfa1-1bae4b6c1213"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7dd4e58a-92f6-31cb-2d37-122bff506f60"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("81d93df7-0d57-1298-acd9-2532ff271c6d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8762d56e-cd0b-b2a5-5b61-402481900344"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("878e14cc-7138-534e-699c-4edb6817f77c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a112cb9-5f3e-498f-bf95-e13593dc6336"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac662de6-7f16-d09a-4d2a-6527c2c0a9b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("adb5f6b8-03d3-7006-8d6e-de86af2c4463"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2b57447-a5d1-43e8-02f3-b5abc254eff1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c3a105d6-6e87-5737-dfa3-93a88690d257"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8c85611-21fd-2ba7-ceb1-e823a91c8b08"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd51533c-c98b-9db6-0722-0a42d0dbe6a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cfe22ac3-c042-7163-7235-2d1d4f9704f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e38ba2fd-e815-7c4b-134e-9702bece859b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6ebdc9a-0eef-7821-1fcb-24851593c2fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eeeed37e-ca44-83a6-0d16-1cf0c9cc94e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f70934cc-417e-17e4-ddac-84742eb16a2d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9f2a38b-5773-971a-573f-976c841d794d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("30a04719-0aab-9967-2f78-c57274e87419"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f8571475-b130-7a9c-37fb-305e561984e7"));
        }
    }
}
