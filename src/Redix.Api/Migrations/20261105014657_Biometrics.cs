using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Biometrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UsesBiometrics",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "", "biometrics.failed" },
                    { new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "", "biometrics.promptCancel" },
                    { new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "", "biometrics.usePassword" },
                    { new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "", "biometrics.signIn" },
                    { new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "", "biometrics.promptTitle" },
                    { new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "", "biometrics.hint" },
                    { new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "", "biometrics.or" },
                    { new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "", "biometrics.notAvailable" },
                    { new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "", "biometrics.notChanged" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("333035c5-99f6-df96-d24d-5caa61992920"), "ar", new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "تعذّر التغيير. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("48ede494-1658-ee76-7203-b3ab21167e7f"), "en", new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "Could not turn it on. Set up a fingerprint or face on this phone, then try again." },
                    { new Guid("4b7095cb-e444-982d-4e80-d75142520183"), "en", new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "Open REDIX" },
                    { new Guid("4c7d3176-5677-9880-e2ef-17f1db36bb93"), "en", new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "Opening the app asks for it, and you stay signed in until you sign out." },
                    { new Guid("569045a4-b515-3955-379c-4e55af8f5fac"), "en", new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "Could not change this. Check the network and try again." },
                    { new Guid("5beb3b76-4b3e-0558-bc95-90d8a35362eb"), "en", new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "or" },
                    { new Guid("71627720-2929-5eda-a1b8-1bf9760a8303"), "ar", new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "تعذّر التحقق منك. حاول مرة أخرى، أو استخدم كلمة المرور." },
                    { new Guid("72dd481f-5b91-301c-1d5a-63a693734e0b"), "ar", new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "استخدم كلمة المرور لتسجيل الدخول." },
                    { new Guid("87773873-a24c-66c4-8303-47c93e0d89ad"), "ar", new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "كلمة المرور" },
                    { new Guid("8adb7508-af93-ac7b-ec3f-c2eba709908f"), "en", new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "Use password" },
                    { new Guid("8bc23a05-b085-6ee1-1e87-6a3fa3ce1988"), "ar", new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "الدخول بالبصمة أو الوجه" },
                    { new Guid("946b9583-a5c5-e30f-b864-6fc57763e4ce"), "ar", new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "فتح REDIX" },
                    { new Guid("9ec7c2f3-b222-77e1-c31f-3974db486e7d"), "ar", new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "يطلبها التطبيق عند فتحه، وتبقى مسجّلاً حتى تخرج." },
                    { new Guid("a7d434ab-ab55-f24f-9f24-295044b0b2f3"), "ar", new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "أو" },
                    { new Guid("c02775dc-0135-6f76-5c2f-e6ffab2bd0a0"), "en", new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "Sign in with fingerprint or face" },
                    { new Guid("c886e719-2e70-cc39-2946-485d7987ede4"), "ar", new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "تعذّر التفعيل. أضف بصمة أو وجهاً في إعدادات الجوال، ثم حاول مرة أخرى." },
                    { new Guid("d6e673fc-07a2-8a1b-09f7-6b3fab96c693"), "en", new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "Could not confirm it is you. Try again, or use your password." },
                    { new Guid("eec075fa-f5f6-87b0-d5de-603142937bfa"), "en", new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "Use your password to sign in." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("333035c5-99f6-df96-d24d-5caa61992920"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48ede494-1658-ee76-7203-b3ab21167e7f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b7095cb-e444-982d-4e80-d75142520183"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c7d3176-5677-9880-e2ef-17f1db36bb93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("569045a4-b515-3955-379c-4e55af8f5fac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5beb3b76-4b3e-0558-bc95-90d8a35362eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71627720-2929-5eda-a1b8-1bf9760a8303"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("72dd481f-5b91-301c-1d5a-63a693734e0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87773873-a24c-66c4-8303-47c93e0d89ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8adb7508-af93-ac7b-ec3f-c2eba709908f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8bc23a05-b085-6ee1-1e87-6a3fa3ce1988"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("946b9583-a5c5-e30f-b864-6fc57763e4ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ec7c2f3-b222-77e1-c31f-3974db486e7d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7d434ab-ab55-f24f-9f24-295044b0b2f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c02775dc-0135-6f76-5c2f-e6ffab2bd0a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c886e719-2e70-cc39-2946-485d7987ede4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6e673fc-07a2-8a1b-09f7-6b3fab96c693"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eec075fa-f5f6-87b0-d5de-603142937bfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"));

            migrationBuilder.DropColumn(
                name: "UsesBiometrics",
                table: "Devices");
        }
    }
}
