using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class LateEntriesAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "", "capital.receiptAlreadyAttached" },
                    { new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "", "liquidation.record" },
                    { new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "", "liquidation.attachReceipt" },
                    { new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "", "liquidation.late" },
                    { new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "", "capital.unknownItem" },
                    { new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "", "liquidation.amount" },
                    { new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "", "capital.projectNotClosed" },
                    { new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "", "liquidation.receiptAttached" },
                    { new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "", "capital.invalidFileType" },
                    { new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "", "liquidation.lateRevenue" },
                    { new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "", "projects.liquidateFirst" },
                    { new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "", "liquidation.item" },
                    { new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "", "liquidation.description" },
                    { new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "", "liquidation.noReceipt" },
                    { new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "", "liquidation.lateExpense" },
                    { new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "", "capital.imageTooLarge" },
                    { new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "", "liquidation.lateRecorded" },
                    { new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "", "liquidation.lateExplain" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1b5dbc82-9bd5-f67f-8799-878a34237c99"), "ar", new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "سُجِّل ووُزِّع على الشركاء." },
                    { new Guid("1f0d6a28-50c8-e164-4d9c-6afaacb7ab6f"), "en", new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "It is shared among the partners by their shares at closing: each pays in their part of an expense, and takes their part of revenue." },
                    { new Guid("212a14f2-8c5b-8d45-3922-803079bbbe83"), "ar", new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "لا يزال فائضه لم يوزَّع على الشركاء، فصفِّه بدلاً من ذلك، وبالتصفية يُغلق." },
                    { new Guid("41c24de3-a743-2d2a-9616-80e737b7c9e0"), "ar", new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "إرفاق الإيصال" },
                    { new Guid("45429a16-2a16-915f-3882-28f0f2bfed99"), "ar", new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "لا يوجد إيصال بعد" },
                    { new Guid("4956f064-b12d-28e0-c5b1-051a0dd60c3a"), "en", new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "Attach receipt" },
                    { new Guid("563031c8-7783-4fd3-17f1-347183abac42"), "en", new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "Its surplus is still to be shared among the partners, so liquidate it instead: that closes it." },
                    { new Guid("59b7b483-20e6-0d4f-320c-f22efaad162b"), "ar", new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "إيراد متأخر" },
                    { new Guid("5eec2fad-14c6-5123-1f1f-8a0c748fcbde"), "ar", new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "تم إرفاق الإيصال." },
                    { new Guid("686132ab-e386-4220-ca42-12480a98d8e0"), "ar", new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "لهذه الحصة إيصالها من قبل." },
                    { new Guid("8596d182-7f81-6a2b-2fce-9bbc7d13d7fd"), "en", new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "Amount" },
                    { new Guid("8974ef2b-80b2-37ab-7465-c3a72c4d72ef"), "ar", new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "اختر صورة أو ملف PDF." },
                    { new Guid("8ec1c022-ae64-86dd-b9dd-73e7480594a3"), "en", new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "The receipt is attached." },
                    { new Guid("9e32ae9f-a44c-e641-bb1e-cf6ac533cd5a"), "en", new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "Record a late expense or revenue" },
                    { new Guid("a0316e5c-38cd-6872-2e27-955e56fb4f41"), "en", new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "No receipt yet" },
                    { new Guid("a87ee58e-7104-6b53-261f-8755ad423a9d"), "en", new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "Record" },
                    { new Guid("b887bc1c-1a29-1677-556e-bb69f35e23cb"), "en", new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "Only a closed project takes a late expense or revenue." },
                    { new Guid("b8e00284-3351-ecbf-a316-01198df27aee"), "ar", new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "الصورة كبيرة جداً؛ يجب أن تكون أصغر من 1 ميغابايت." },
                    { new Guid("b9d61caa-18aa-7ce7-fd0a-131060a36c1f"), "ar", new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "البند" },
                    { new Guid("ba23d8c5-6e94-4825-6fce-77c835560065"), "en", new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "Choose an item that takes purchases." },
                    { new Guid("c4cf616f-c659-2528-4a37-add2c38c517a"), "ar", new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "تسجيل مصروف أو إيراد متأخر" },
                    { new Guid("c6dcd7cb-1528-75bb-8bf5-ef15fef83753"), "en", new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "Description" },
                    { new Guid("cb98a194-5e2e-835b-b8d4-48b0211043b0"), "ar", new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "المبلغ" },
                    { new Guid("cc181cbf-7679-59ce-f35b-37a08964c41e"), "ar", new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "تسجيل" },
                    { new Guid("cd475901-fd44-ee35-8327-3249d52c2b5f"), "ar", new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "مصروف متأخر" },
                    { new Guid("ce1c860d-79f0-de5e-905d-ccf1a261c251"), "en", new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "Late revenue" },
                    { new Guid("d0c57f3c-deab-8bba-ec5f-faabfc51c1fd"), "ar", new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "اختر بنداً يقبل المشتريات." },
                    { new Guid("d0cb2bda-d4e3-b3d0-97bb-77e0b228d33a"), "ar", new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "لا يُسجَّل مصروف أو إيراد متأخر إلا في مشروع مغلق." },
                    { new Guid("d7e78449-1936-2343-6d0d-32746aea4c38"), "en", new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "Late expense" },
                    { new Guid("ddbdacd8-3021-3287-d720-ad056334d754"), "en", new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "The picture is too large; it must be under 1 MB." },
                    { new Guid("e3d3b7a3-6c14-faf6-89c8-5b1ece5ab024"), "en", new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "Recorded and shared among the partners." },
                    { new Guid("e461b3d6-12af-fcae-11cc-2783ac594cae"), "en", new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "Choose a picture or a PDF." },
                    { new Guid("e6890e09-24b3-4e3a-72aa-7fadb095f251"), "en", new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "This part already has its receipt." },
                    { new Guid("e6c9a973-faab-0d72-d84f-e5e24076c644"), "ar", new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "الوصف" },
                    { new Guid("f4024adf-fa8b-16a0-53de-8eff92ced802"), "en", new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "Item" },
                    { new Guid("fbf13a83-0fb7-e7ee-c1c4-be3662b83aed"), "ar", new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "يوزَّع على الشركاء بنسبهم عند الإغلاق: يدفع كل شريك حصته من المصروف، ويأخذ حصته من الإيراد." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b5dbc82-9bd5-f67f-8799-878a34237c99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f0d6a28-50c8-e164-4d9c-6afaacb7ab6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("212a14f2-8c5b-8d45-3922-803079bbbe83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41c24de3-a743-2d2a-9616-80e737b7c9e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("45429a16-2a16-915f-3882-28f0f2bfed99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4956f064-b12d-28e0-c5b1-051a0dd60c3a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("563031c8-7783-4fd3-17f1-347183abac42"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("59b7b483-20e6-0d4f-320c-f22efaad162b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5eec2fad-14c6-5123-1f1f-8a0c748fcbde"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("686132ab-e386-4220-ca42-12480a98d8e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8596d182-7f81-6a2b-2fce-9bbc7d13d7fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8974ef2b-80b2-37ab-7465-c3a72c4d72ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ec1c022-ae64-86dd-b9dd-73e7480594a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e32ae9f-a44c-e641-bb1e-cf6ac533cd5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0316e5c-38cd-6872-2e27-955e56fb4f41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a87ee58e-7104-6b53-261f-8755ad423a9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b887bc1c-1a29-1677-556e-bb69f35e23cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8e00284-3351-ecbf-a316-01198df27aee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b9d61caa-18aa-7ce7-fd0a-131060a36c1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba23d8c5-6e94-4825-6fce-77c835560065"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4cf616f-c659-2528-4a37-add2c38c517a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6dcd7cb-1528-75bb-8bf5-ef15fef83753"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb98a194-5e2e-835b-b8d4-48b0211043b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc181cbf-7679-59ce-f35b-37a08964c41e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd475901-fd44-ee35-8327-3249d52c2b5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce1c860d-79f0-de5e-905d-ccf1a261c251"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0c57f3c-deab-8bba-ec5f-faabfc51c1fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0cb2bda-d4e3-b3d0-97bb-77e0b228d33a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7e78449-1936-2343-6d0d-32746aea4c38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ddbdacd8-3021-3287-d720-ad056334d754"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e3d3b7a3-6c14-faf6-89c8-5b1ece5ab024"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e461b3d6-12af-fcae-11cc-2783ac594cae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6890e09-24b3-4e3a-72aa-7fadb095f251"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6c9a973-faab-0d72-d84f-e5e24076c644"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4024adf-fa8b-16a0-53de-8eff92ced802"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbf13a83-0fb7-e7ee-c1c4-be3662b83aed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7e9ea7f0-6d30-5238-6983-354604749559"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"));
        }
    }
}
