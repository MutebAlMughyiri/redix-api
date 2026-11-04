using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SignOutWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "", "myAccount.signOut" },
                    { new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "", "myAccount.signOutQuestion" },
                    { new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "", "myAccount.signOutExplain" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0f4b495b-6989-2d80-15e2-9a25233f6292"), "en", new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "Sign out of this phone?" },
                    { new Guid("38060b25-7a69-e435-2f93-c4e0dcb4a2b1"), "en", new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "You will need your username and password to sign in again." },
                    { new Guid("6843f540-24f0-3f19-4b3c-73fca46ea856"), "ar", new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "تسجيل الخروج" },
                    { new Guid("951a7c24-cdd1-025b-d06c-182c2035dd5a"), "ar", new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "تسجيل الخروج من هذا الجوال؟" },
                    { new Guid("c1507bb1-74bd-4279-ebf2-90f4220fd276"), "en", new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "Sign out" },
                    { new Guid("d9244a6a-d6a1-eb5e-3350-9d37c89cf15e"), "ar", new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "ستحتاج إلى اسم المستخدم وكلمة المرور للدخول مرة أخرى." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f4b495b-6989-2d80-15e2-9a25233f6292"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38060b25-7a69-e435-2f93-c4e0dcb4a2b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6843f540-24f0-3f19-4b3c-73fca46ea856"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("951a7c24-cdd1-025b-d06c-182c2035dd5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1507bb1-74bd-4279-ebf2-90f4220fd276"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9244a6a-d6a1-eb5e-3350-9d37c89cf15e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aa61ccff-4503-4423-7299-9244e9807aba"));
        }
    }
}
