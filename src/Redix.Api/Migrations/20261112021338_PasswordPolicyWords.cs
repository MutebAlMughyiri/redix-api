using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PasswordPolicyWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "", "common.passwordRules" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("669900fc-707d-4455-33c3-9dce46e0c16b"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل فيها حرف ورقم، ليست اسم المستخدم، ومختلفة عن كلمة المرور الحالية.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a888b79-0b27-2870-0879-9a584a4670d8"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f1c7913-3212-6c3d-8bc6-938f059a033e"),
                column: "Value",
                value: "Use at least 8 characters with a letter and a digit, not the user name.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9baea722-2831-1387-5e7d-fdf507b22847"),
                column: "Value",
                value: "Use at least 8 characters with a letter and a digit, not the user name.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8fd20c5-eedd-da6a-7a8a-dd5e35e7c419"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4e4d543-fa12-5ad4-302a-cb1db1520ac1"),
                column: "Value",
                value: "Use at least 8 characters with a letter and a digit, not your user name, and different from the current password.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1aeca5a0-c348-8d0b-d78f-68a77bc8cbd2"), "ar", new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "8 أحرف على الأقل، فيها حرف ورقم، وليست اسم المستخدم." },
                    { new Guid("71e5ae9f-3dfd-8342-d089-89a008936b52"), "en", new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "At least 8 characters, with a letter and a digit, not the user name." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1aeca5a0-c348-8d0b-d78f-68a77bc8cbd2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71e5ae9f-3dfd-8342-d089-89a008936b52"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("669900fc-707d-4455-33c3-9dce46e0c16b"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل، مختلفة عن كلمة المرور الحالية.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a888b79-0b27-2870-0879-9a584a4670d8"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f1c7913-3212-6c3d-8bc6-938f059a033e"),
                column: "Value",
                value: "Use at least 8 characters.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9baea722-2831-1387-5e7d-fdf507b22847"),
                column: "Value",
                value: "Use at least 8 characters.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8fd20c5-eedd-da6a-7a8a-dd5e35e7c419"),
                column: "Value",
                value: "استخدم 8 أحرف على الأقل.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4e4d543-fa12-5ad4-302a-cb1db1520ac1"),
                column: "Value",
                value: "Use at least 8 characters, different from the current password.");
        }
    }
}
