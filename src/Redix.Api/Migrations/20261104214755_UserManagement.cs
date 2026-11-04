using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UserManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08adf53e-6994-bfcc-5fa9-c8d334f74fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4eb76426-4b41-58e7-8875-205425073ad0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ce294ad-7754-003c-589e-13241db904ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ebb8cac9-5da0-39b8-4540-43a5a99fc371"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"));

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Users",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Users SET DisplayName = Username");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "", "users.edit" },
                    { new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "", "users.nameHint" },
                    { new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "", "users.resetExplain" },
                    { new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "", "users.weakPassword" },
                    { new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "", "users.temporaryPasswordHint" },
                    { new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "", "users.owner" },
                    { new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "", "users.disableQuestion" },
                    { new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "", "users.ownerCannotBeDisabled" },
                    { new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "", "users.disable" },
                    { new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "", "users.new" },
                    { new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "", "users.add" },
                    { new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "", "users.invalidMobile" },
                    { new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "", "users.temporaryPassword" },
                    { new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "", "users.enable" },
                    { new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "", "users.invalidName" },
                    { new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "", "users.invalidUsername" },
                    { new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "", "users.loadFailed" },
                    { new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "", "users.resetPassword" },
                    { new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "", "users.save" },
                    { new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "", "users.title" },
                    { new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "", "users.mobile" },
                    { new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "", "common.retry" },
                    { new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "", "users.refused" },
                    { new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "", "users.usernameTaken" },
                    { new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "", "common.cancel" },
                    { new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "", "users.username" },
                    { new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "", "users.disabled" },
                    { new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "", "users.disableExplain" },
                    { new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "", "users.name" },
                    { new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "", "users.noConnection" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("063f734c-da60-f010-7745-3fa11ed3b50b"), "en", new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "No connection to the server. Try again." },
                    { new Guid("09091696-ccfc-2c4b-2fb6-30a990acc27b"), "ar", new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "اكتب رقم جوال من 20 حرفاً على الأكثر." },
                    { new Guid("154467ea-dd73-094d-112a-d6a4252d5034"), "ar", new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "يُسجَّل خروجه، ويجب أن يختار كلمة مرور جديدة عند دخوله التالي." },
                    { new Guid("15c5dddc-fd5b-d50c-9eca-fda7d18221c9"), "ar", new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "تعديل" },
                    { new Guid("1683a75e-e83b-46b6-c8f0-c2aa8454a355"), "ar", new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "مالك" },
                    { new Guid("1951c2e7-f090-4df1-5aee-5f8f05986cd2"), "en", new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "Temporary password" },
                    { new Guid("1c0564a0-4f59-daac-217c-202925930cf3"), "ar", new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("1c11b7d7-e069-0b2b-2ef4-a67430835f22"), "en", new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "New user" },
                    { new Guid("1cad9dfd-1dff-8acf-d4b7-af03de63d8ba"), "en", new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "Try again" },
                    { new Guid("225734b9-d03f-0039-6ab9-f13027c19c5d"), "en", new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "Save" },
                    { new Guid("23ad0c7a-5843-e59f-7422-d8df769f75c0"), "en", new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "Name" },
                    { new Guid("3042eff0-ba9f-2cb5-8bde-09ff6a02d391"), "ar", new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "إلغاء" },
                    { new Guid("33cec58f-f17f-ec8b-196f-200d1e78db83"), "en", new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "Disable" },
                    { new Guid("387924af-6253-0256-9ea8-c17696668154"), "ar", new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "لا يمكن تعطيل المالك." },
                    { new Guid("40334390-4fa3-9236-1fa2-098fbe4d0e41"), "en", new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "Cancel" },
                    { new Guid("40eff482-ba9e-dfc4-5d89-2e2c87694911"), "ar", new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "إضافة المستخدم" },
                    { new Guid("465e2404-09f6-8303-578a-c669cf18aa29"), "en", new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "Enter a username of 1 to 50 characters." },
                    { new Guid("467aa725-2bac-e2f2-44d0-10e6dfdb3734"), "en", new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "Disabled" },
                    { new Guid("50256a83-ee73-f9bd-8abc-1426bdcfc3a8"), "ar", new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "اكتب اسم مستخدم من 1 إلى 50 حرفاً." },
                    { new Guid("509149e7-bae6-f85b-cfab-ce48ad7efd41"), "ar", new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "رفض الخادم هذا التغيير." },
                    { new Guid("51428f67-c525-7ce4-c883-d52a40432206"), "en", new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "They are signed out at their next action, and cannot sign in until enabled." },
                    { new Guid("528d2dd3-7f1a-20fe-8a40-55ac99701dd0"), "en", new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "Enter a name of 1 to 100 characters." },
                    { new Guid("59543b95-d796-b41a-fbe1-d2085e9d87af"), "ar", new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "الجوال" },
                    { new Guid("5d8b19f0-31ee-0468-e90c-0c17a6cae0f0"), "ar", new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "تُغيَّر عند أول دخول" },
                    { new Guid("5fb7173c-f1be-c1e4-e8d2-df3e2ebf7a94"), "ar", new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "حفظ" },
                    { new Guid("6316b8e7-2f05-0e2b-341e-98ae3e9e2a5b"), "en", new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "Enter a mobile of up to 20 characters." },
                    { new Guid("63d12455-aeed-2107-bf58-856365e611fd"), "ar", new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "إعادة تعيين كلمة المرور" },
                    { new Guid("70316694-acfe-d92c-0563-160a0dac969e"), "en", new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "Owner" },
                    { new Guid("763474b4-80bd-24cd-b736-1e61d551dd3c"), "ar", new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "الاسم" },
                    { new Guid("76acd042-4716-fdee-de80-d419292e9604"), "ar", new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "تعطيل" },
                    { new Guid("76e19e53-274c-bdd5-57cf-73e421bcc386"), "ar", new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "تفعيل" },
                    { new Guid("7898a82b-821a-556b-1853-a79e5c7944b2"), "ar", new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "مستخدم جديد" },
                    { new Guid("7dc0dba1-da42-fba6-46a2-adcf62ec1493"), "en", new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "Mobile" },
                    { new Guid("7f1c7913-3212-6c3d-8bc6-938f059a033e"), "en", new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "Use at least 8 characters." },
                    { new Guid("824ddda2-086c-0620-f7f9-1d01831345e8"), "en", new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "Users" },
                    { new Guid("86a308aa-bd62-a572-52c0-513e73e9b87e"), "en", new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "The server refused this change." },
                    { new Guid("8728059e-aa91-1f07-1cc4-550ffdcc7787"), "en", new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "Add user" },
                    { new Guid("8adcd323-1366-adc4-cd92-5bc04d3aa8f2"), "ar", new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "المستخدمون" },
                    { new Guid("8baa24c5-a100-2237-c76f-1ad966f9f9b4"), "en", new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "Edit" },
                    { new Guid("9c3a7252-4527-c446-ab4f-ef96b47b9bc6"), "en", new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "They are signed out, and must choose a new password at their next sign-in." },
                    { new Guid("a68e5578-e7f1-59ca-104b-a58c554d208c"), "en", new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "Disable this user?" },
                    { new Guid("a791b7b6-01c0-611a-fa72-142e915ae419"), "ar", new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "حاول مرة أخرى" },
                    { new Guid("a8fd20c5-eedd-da6a-7a8a-dd5e35e7c419"), "ar", new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "استخدم 8 أحرف على الأقل." },
                    { new Guid("b32b53f8-6ae7-5b8a-4d87-e1924588dfb5"), "en", new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "Changed at first sign-in" },
                    { new Guid("b85b59c8-a2cb-53a1-0a6e-4543545e9872"), "ar", new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "تعذّر تحميل المستخدمين. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("bc4dd455-c1c0-c109-0c10-cb19970e85c8"), "ar", new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("bc73d6f9-aa26-0808-a87b-6ae8fcdcf2a6"), "ar", new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "تعطيل هذا المستخدم؟" },
                    { new Guid("bd07f295-e1fe-244d-c14c-8db6968c80ab"), "ar", new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "الاسم كما يظهر في القيود" },
                    { new Guid("bd567479-be43-96a7-14ff-495c1c53b137"), "ar", new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "يُسجَّل خروجه عند أول إجراء تالٍ، ولا يستطيع الدخول حتى يُفعَّل." },
                    { new Guid("c91218ab-8fe6-4e4f-19f3-41ee196eecae"), "ar", new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "كلمة مرور مؤقتة" },
                    { new Guid("d0bd8ac0-7611-d6ea-4225-70813262f85d"), "en", new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "Enable" },
                    { new Guid("d34f2b37-d8e2-af17-a407-cfb54eee8f9c"), "en", new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "Could not load the users. Check the network and try again." },
                    { new Guid("d90f5917-14c9-b5ec-bbb4-daf7ee1bc8b7"), "en", new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "Username" },
                    { new Guid("e34d1ae3-d6c8-96c7-2629-cc1d38e22b70"), "en", new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "This username is taken. Choose another." },
                    { new Guid("ecb7fbf3-ee85-8368-44c7-7e094c1cb98f"), "ar", new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "اسم المستخدم هذا مستخدم. اختر غيره." },
                    { new Guid("ee253398-1340-6bd6-c74c-055af47c8aa2"), "en", new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "The name as it shows on entries" },
                    { new Guid("f09e94cd-299b-32b9-e872-f062bff0c962"), "ar", new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "اسم المستخدم" },
                    { new Guid("f2897876-7a7e-6d7c-37eb-ff9c14a73c31"), "ar", new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "معطّل" },
                    { new Guid("f56cf8ff-87f7-d8a4-a5f6-3ca02ec209fc"), "en", new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "Reset password" },
                    { new Guid("fc16c245-10f9-3042-8c9c-7086d476a790"), "en", new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "An owner cannot be disabled." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("063f734c-da60-f010-7745-3fa11ed3b50b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09091696-ccfc-2c4b-2fb6-30a990acc27b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("154467ea-dd73-094d-112a-d6a4252d5034"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c5dddc-fd5b-d50c-9eca-fda7d18221c9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1683a75e-e83b-46b6-c8f0-c2aa8454a355"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1951c2e7-f090-4df1-5aee-5f8f05986cd2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c0564a0-4f59-daac-217c-202925930cf3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c11b7d7-e069-0b2b-2ef4-a67430835f22"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1cad9dfd-1dff-8acf-d4b7-af03de63d8ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("225734b9-d03f-0039-6ab9-f13027c19c5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("23ad0c7a-5843-e59f-7422-d8df769f75c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3042eff0-ba9f-2cb5-8bde-09ff6a02d391"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("33cec58f-f17f-ec8b-196f-200d1e78db83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("387924af-6253-0256-9ea8-c17696668154"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40334390-4fa3-9236-1fa2-098fbe4d0e41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40eff482-ba9e-dfc4-5d89-2e2c87694911"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("465e2404-09f6-8303-578a-c669cf18aa29"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("467aa725-2bac-e2f2-44d0-10e6dfdb3734"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50256a83-ee73-f9bd-8abc-1426bdcfc3a8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("509149e7-bae6-f85b-cfab-ce48ad7efd41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("51428f67-c525-7ce4-c883-d52a40432206"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("528d2dd3-7f1a-20fe-8a40-55ac99701dd0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("59543b95-d796-b41a-fbe1-d2085e9d87af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d8b19f0-31ee-0468-e90c-0c17a6cae0f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5fb7173c-f1be-c1e4-e8d2-df3e2ebf7a94"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6316b8e7-2f05-0e2b-341e-98ae3e9e2a5b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63d12455-aeed-2107-bf58-856365e611fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("70316694-acfe-d92c-0563-160a0dac969e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("763474b4-80bd-24cd-b736-1e61d551dd3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76acd042-4716-fdee-de80-d419292e9604"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76e19e53-274c-bdd5-57cf-73e421bcc386"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7898a82b-821a-556b-1853-a79e5c7944b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7dc0dba1-da42-fba6-46a2-adcf62ec1493"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f1c7913-3212-6c3d-8bc6-938f059a033e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("824ddda2-086c-0620-f7f9-1d01831345e8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86a308aa-bd62-a572-52c0-513e73e9b87e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8728059e-aa91-1f07-1cc4-550ffdcc7787"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8adcd323-1366-adc4-cd92-5bc04d3aa8f2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8baa24c5-a100-2237-c76f-1ad966f9f9b4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c3a7252-4527-c446-ab4f-ef96b47b9bc6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a68e5578-e7f1-59ca-104b-a58c554d208c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a791b7b6-01c0-611a-fa72-142e915ae419"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8fd20c5-eedd-da6a-7a8a-dd5e35e7c419"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b32b53f8-6ae7-5b8a-4d87-e1924588dfb5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85b59c8-a2cb-53a1-0a6e-4543545e9872"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc4dd455-c1c0-c109-0c10-cb19970e85c8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc73d6f9-aa26-0808-a87b-6ae8fcdcf2a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd07f295-e1fe-244d-c14c-8db6968c80ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd567479-be43-96a7-14ff-495c1c53b137"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c91218ab-8fe6-4e4f-19f3-41ee196eecae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0bd8ac0-7611-d6ea-4225-70813262f85d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d34f2b37-d8e2-af17-a407-cfb54eee8f9c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d90f5917-14c9-b5ec-bbb4-daf7ee1bc8b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e34d1ae3-d6c8-96c7-2629-cc1d38e22b70"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ecb7fbf3-ee85-8368-44c7-7e094c1cb98f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee253398-1340-6bd6-c74c-055af47c8aa2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f09e94cd-299b-32b9-e872-f062bff0c962"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2897876-7a7e-6d7c-37eb-ff9c14a73c31"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f56cf8ff-87f7-d8a4-a5f6-3ca02ec209fc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc16c245-10f9-3042-8c9c-7086d476a790"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1a09023f-e56b-1467-f632-099aef723ce3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1a928387-fb91-5a82-e042-9b39ad419467"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a4113921-35de-ca8a-a341-140a9fb13728"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"));

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Users");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "", "projects.cancel" },
                    { new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "", "projects.retry" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("08adf53e-6994-bfcc-5fa9-c8d334f74fd8"), "ar", new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "إلغاء" },
                    { new Guid("4eb76426-4b41-58e7-8875-205425073ad0"), "en", new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "Cancel" },
                    { new Guid("5ce294ad-7754-003c-589e-13241db904ac"), "en", new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "Try again" },
                    { new Guid("ebb8cac9-5da0-39b8-4540-43a5a99fc371"), "ar", new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "حاول مرة أخرى" }
                });
        }
    }
}
