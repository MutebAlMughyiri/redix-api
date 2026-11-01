using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "", "changePassword.accountLocked" },
                    { new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "", "changePassword.invalidCredentials" },
                    { new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "", "changePassword.mismatch" },
                    { new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "", "changePassword.hint" },
                    { new Guid("531379ae-8cde-2467-8484-33a498b15916"), "", "changePassword.new" },
                    { new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "", "changePassword.weakPassword" },
                    { new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "", "changePassword.submit" },
                    { new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "", "changePassword.confirm" },
                    { new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "", "changePassword.title" },
                    { new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "", "changePassword.current" },
                    { new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "", "changePassword.noConnection" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0ec63acf-9f28-8066-4b9b-5fdf973cac8e"), "en", new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "Too many wrong tries. Try again later." },
                    { new Guid("2b67fe74-5528-3992-bcd0-3e7a1021f46b"), "ar", new Guid("531379ae-8cde-2467-8484-33a498b15916"), "كلمة المرور الجديدة" },
                    { new Guid("4cab34f1-8cee-3437-bb76-36922a8569a6"), "en", new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "Change your password" },
                    { new Guid("50e985a8-3305-7a16-931c-9c25ff2c7fa0"), "en", new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "New password again" },
                    { new Guid("5155c37f-54a1-18cc-606f-89fcbfd53f03"), "ar", new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "اختر كلمة مرور جديدة من 8 أحرف على الأقل قبل أن تبدأ." },
                    { new Guid("62f4cc21-86b1-0826-244e-85914e951e0a"), "en", new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "No connection to the server. Try again." },
                    { new Guid("669900fc-707d-4455-33c3-9dce46e0c16b"), "ar", new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "استخدم 8 أحرف على الأقل، مختلفة عن كلمة المرور الحالية." },
                    { new Guid("80d2c9f2-d915-2c8f-8466-16d22da29919"), "ar", new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("85d8e46b-f92d-477e-f068-b357c4577eaa"), "ar", new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "كلمة المرور الحالية غير صحيحة." },
                    { new Guid("8806813a-6bd7-0912-fc60-2a0fc87bffb6"), "en", new Guid("531379ae-8cde-2467-8484-33a498b15916"), "New password" },
                    { new Guid("a85c3058-afec-c7fc-4c92-4db6589fd659"), "ar", new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "حفظ" },
                    { new Guid("b4e4d543-fa12-5ad4-302a-cb1db1520ac1"), "en", new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "Use at least 8 characters, different from the current password." },
                    { new Guid("b82762a3-5c4f-7acf-8286-1cac80a17553"), "en", new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "Current password" },
                    { new Guid("beefe2bf-5d6c-1d75-ffff-880119124f02"), "ar", new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "كلمة المرور الحالية" },
                    { new Guid("d266db2b-6ac5-0e45-62a9-27104b44cbfc"), "ar", new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "كلمتا المرور الجديدتان غير متطابقتين." },
                    { new Guid("d892dc31-36a4-2be8-747b-cb26dba2bffd"), "en", new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "The new passwords do not match." },
                    { new Guid("dcd0b709-c7c4-92bc-a383-0184ef386d3b"), "ar", new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "غيّر كلمة المرور" },
                    { new Guid("eb521ae0-de5c-7bc9-87e2-080781de3586"), "ar", new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("f6983ba5-47fd-5dfc-eaad-bb8f51b06be7"), "en", new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "Save" },
                    { new Guid("f6d020f1-7439-a0f5-c5c3-2134260f96e1"), "en", new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "The current password is wrong." },
                    { new Guid("f6f0000d-cdaf-e0c3-ecb0-1fcca7ef1aff"), "en", new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "Choose a new password of at least 8 characters before you start." },
                    { new Guid("f794e624-9cf8-c21a-b718-7aac4f727c68"), "ar", new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "أعد كتابة كلمة المرور الجديدة" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ec63acf-9f28-8066-4b9b-5fdf973cac8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b67fe74-5528-3992-bcd0-3e7a1021f46b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4cab34f1-8cee-3437-bb76-36922a8569a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50e985a8-3305-7a16-931c-9c25ff2c7fa0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5155c37f-54a1-18cc-606f-89fcbfd53f03"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62f4cc21-86b1-0826-244e-85914e951e0a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("669900fc-707d-4455-33c3-9dce46e0c16b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80d2c9f2-d915-2c8f-8466-16d22da29919"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85d8e46b-f92d-477e-f068-b357c4577eaa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8806813a-6bd7-0912-fc60-2a0fc87bffb6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a85c3058-afec-c7fc-4c92-4db6589fd659"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4e4d543-fa12-5ad4-302a-cb1db1520ac1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b82762a3-5c4f-7acf-8286-1cac80a17553"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("beefe2bf-5d6c-1d75-ffff-880119124f02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d266db2b-6ac5-0e45-62a9-27104b44cbfc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d892dc31-36a4-2be8-747b-cb26dba2bffd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dcd0b709-c7c4-92bc-a383-0184ef386d3b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb521ae0-de5c-7bc9-87e2-080781de3586"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6983ba5-47fd-5dfc-eaad-bb8f51b06be7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6d020f1-7439-a0f5-c5c3-2134260f96e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f6f0000d-cdaf-e0c3-ecb0-1fcca7ef1aff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f794e624-9cf8-c21a-b718-7aac4f727c68"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("204b1daa-eaae-c1eb-c979-646389e17604"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("531379ae-8cde-2467-8484-33a498b15916"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("87e50af1-6ea7-1755-99f0-e54063095107"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"));
        }
    }
}
