using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SignInAndPasswordRefusedWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "", "signIn.refused" },
                    { new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "", "changePassword.refused" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("07e30917-32ab-8658-a444-ca12fddb1229"), "en", new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "The server could not sign you in. Try again." },
                    { new Guid("549b8139-4dbe-0355-7f02-9561f6f4d77e"), "en", new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "The server could not change your password. Try again." },
                    { new Guid("6e5a5f70-08f1-5fa0-7090-1e66d1afdcf4"), "ar", new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "تعذّر على الخادم تغيير كلمة المرور. حاول مرة أخرى." },
                    { new Guid("eb34bc4f-46f7-e1f3-76c0-ec50de20a9f9"), "ar", new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "تعذّر على الخادم تسجيل دخولك. حاول مرة أخرى." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07e30917-32ab-8658-a444-ca12fddb1229"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("549b8139-4dbe-0355-7f02-9561f6f4d77e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6e5a5f70-08f1-5fa0-7090-1e66d1afdcf4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb34bc4f-46f7-e1f3-76c0-ec50de20a9f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"));
        }
    }
}
