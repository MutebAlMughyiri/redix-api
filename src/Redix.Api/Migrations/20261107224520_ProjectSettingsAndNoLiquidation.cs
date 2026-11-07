using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectSettingsAndNoLiquidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Liquidation is removed (Muteb, 8 Nov 2026): its movements (type 7) and what hangs on them go, with no reader left for them. Late expenses and revenue stay.
            migrationBuilder.Sql("""
                SELECT EntryId INTO #Liquidation FROM CapitalMovementDetails WHERE Type = 7;
                DELETE FROM EntryAttachments WHERE EntryId IN (SELECT EntryId FROM #Liquidation);
                DELETE FROM EntryDecisions WHERE EntryId IN (SELECT EntryId FROM #Liquidation);
                DELETE FROM EntryLinks WHERE EntryId IN (SELECT EntryId FROM #Liquidation) OR LinkedEntryId IN (SELECT EntryId FROM #Liquidation);
                DELETE FROM CapitalMovementDetails WHERE Type = 7;
                DELETE FROM Entries WHERE Id IN (SELECT EntryId FROM #Liquidation);
                DROP TABLE #Liquidation;
                """);

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01fa9773-222c-dea8-68a7-92b0d7ea490b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f3ef5ac-285e-6176-5d27-e41f86a03114"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1476922c-1699-51ad-ec2f-703d2d5d4acd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("14a6e1bd-5a0b-ddc1-e316-abd553c87aae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("181d8abb-b72e-4b6d-ee30-3f8143a778f2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b204491-bfd5-d807-4748-9e68b7189da5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b5dbc82-9bd5-f67f-8799-878a34237c99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d276a2e-693a-2f80-32e0-7fa56ccaa2f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f0d6a28-50c8-e164-4d9c-6afaacb7ab6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20985fbe-18e1-b140-a6b6-daa2cbc16d1a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("212a14f2-8c5b-8d45-3922-803079bbbe83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2209644f-178c-a172-79c2-a88e66753697"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bb823a4-afcf-2e9f-4a01-b05f06dd0ffd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3063c96f-4f0c-3bde-9bea-d30f29fcd9b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3647c8d9-e49c-f00d-18d5-5d19e0a2d315"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("376a6684-a674-8e61-0c34-eca05c7c1b06"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b814a1d-0103-ae4a-865f-6e94a1776f43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("413c8e33-9074-1d4c-1e52-584a3c9f462b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41c24de3-a743-2d2a-9616-80e737b7c9e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42c1a391-380a-2b1d-abb2-6b5dd5285825"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42eff101-5d18-09b7-d5bf-2f564301086e"));

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
                keyValue: new Guid("4a0a5a42-b0a2-d246-1638-baebdcff664e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4da6f79c-4986-37e0-98e1-328596f3b55e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50902e08-40f5-85e9-a14d-51ec013dbdaf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52b1c353-bd46-1428-11f1-1b3654f1c102"));

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
                keyValue: new Guid("5cf73c67-94e3-866c-fa78-d1e14fc0bceb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5e8362bc-1f01-cc74-1b3d-161a7827f2a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5eec2fad-14c6-5123-1f1f-8a0c748fcbde"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62d178e3-514f-a6ee-b465-2e04df9276c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("686132ab-e386-4220-ca42-12480a98d8e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7532f470-f8ef-fa47-5363-eaab93a7fa8a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77afa977-3682-1324-dc25-4f56ec1ddd60"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7ec270f0-6548-4ff6-7457-249ed8b5a14c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8305d368-a0f5-cb42-e001-99fa61d1d928"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8596d182-7f81-6a2b-2fce-9bbc7d13d7fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c9ae49b-64f3-b76e-753a-d982f4845c44"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ec1c022-ae64-86dd-b9dd-73e7480594a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94f02ac4-ec30-8fa3-5376-2582adc9640c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e32ae9f-a44c-e641-bb1e-cf6ac533cd5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9eb1d243-636e-5012-8903-fa6691d2f679"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0316e5c-38cd-6872-2e27-955e56fb4f41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a1a21a02-a134-acb8-3b1b-19e9d83dd2c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a87ee58e-7104-6b53-261f-8755ad423a9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af44cf6f-a01a-577d-cf6b-64f7e72348cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b416538d-88e1-08f4-6422-1d4ece5060e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b6c1b002-3319-e673-2dbb-96145536ba2a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b887bc1c-1a29-1677-556e-bb69f35e23cb"));

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
                keyValue: new Guid("c1d9d47a-9e2c-d240-e10c-55fa1d7d9511"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c40f40f1-bb3b-f76b-2cf0-fcea961fdc5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4cf616f-c659-2528-4a37-add2c38c517a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c546ba65-ee62-a31e-8428-74795869a8d0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6dcd7cb-1528-75bb-8bf5-ef15fef83753"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c859bdac-e662-373f-60ee-7ba8a6d733c7"));

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
                keyValue: new Guid("d10ee5f4-88f7-6fb3-3dca-042147cde652"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d3747326-e320-f435-dd51-daa3a64575f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7e78449-1936-2343-6d0d-32746aea4c38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e15589c7-490a-092d-ab41-1c40e3f42479"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e3d3b7a3-6c14-faf6-89c8-5b1ece5ab024"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4dc9526-3c24-7c38-24f8-52f2208c67da"));

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
                keyValue: new Guid("eb5e77f8-6c3f-a041-4f0f-bbfd223f18aa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4024adf-fa8b-16a0-53de-8eff92ced802"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8474702-408e-9dd4-cd19-5698355a707a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbf13a83-0fb7-e7ee-c1c4-be3662b83aed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbf55fae-4cce-ff38-2b99-a7f44e017c8b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0213446a-8644-d22f-874f-cc35f2c9c3e6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0db62c42-0434-bf8e-1965-63cbabee6752"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1493cf6e-a2fe-3b06-9120-ecb9f84aaec0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1659585e-040c-53cb-aa54-a6c9b25ec2d5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("17398f8a-5f3d-21b0-d9d6-5507f99c5528"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("244400f9-b98c-3570-42df-a9205dd5c843"));

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
                keyValue: new Guid("36387219-5110-e631-5aed-f9636ed99d65"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3f071fbc-e41a-3b0e-820d-a5b00e168bf1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("46e98734-759f-aaa8-7c76-50ba076086d9"));

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
                keyValue: new Guid("59e02361-2efd-6c13-e10b-31dd51827e33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("649ac112-55cc-0d09-f17b-82cf6972af58"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("94f31a2a-95ea-4b2b-db84-0994898bc99d"));

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
                keyValue: new Guid("af285bfa-936f-201a-6605-7c062c92be61"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b3a43f40-9310-b867-5316-82973dcf8fcb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c47c7453-9601-018e-53e4-e368bd4cad38"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d274cd87-fc95-0149-b7c5-984ad5e89340"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ee439aba-83f4-d59c-5de0-222b3b9e65e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f4ea1db4-4a12-4857-656b-5d3ac1ec96b5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fd379b83-1e61-3eb8-4370-fa0efbce9ef6"));

            migrationBuilder.DropColumn(
                name: "OpenDefaultProject",
                table: "Users");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "", "projects.defaultSet" },
                    { new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "", "projects.active" },
                    { new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "", "settings.defaultProjectHint" },
                    { new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "", "settings.defaultProject" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"),
                column: "Value",
                value: "Transfers, capital movements, revenue, reconciling and the periodic report.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4ba7de7-217d-f868-d3cc-259546894792"),
                column: "Value",
                value: "التحويلات وحركات رأس المال والإيرادات والمطابقة والتقرير الدوري.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("39882be5-77c6-ee30-84b4-0396ba31a973"), "ar", new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "{0} هو مشروعك الافتراضي الآن، ويُفتح عند الدخول." },
                    { new Guid("3e53413a-f45c-c5a7-f646-c88e19bf6d4f"), "en", new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "{0} is now your default project. It opens when you sign in." },
                    { new Guid("401e8689-1f2c-5e6c-fb9e-e4d60525e39d"), "ar", new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "المشروع الافتراضي" },
                    { new Guid("b08f3025-6404-a6ac-ff51-83cd50007fd5"), "en", new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "Default project" },
                    { new Guid("b8395f34-5f8a-c127-d6ec-9feca2e315b6"), "ar", new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "نشط" },
                    { new Guid("c15f7bde-7cd3-96b1-e563-5b90f8b3fd04"), "ar", new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("c6b0a403-2128-32cd-ce1a-8f6af5c8f074"), "en", new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "Active" },
                    { new Guid("d9f3ffef-aafb-df6b-916a-04d7458fa652"), "en", new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "Tick the project that opens when you sign in. Untick it to see the list instead." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39882be5-77c6-ee30-84b4-0396ba31a973"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3e53413a-f45c-c5a7-f646-c88e19bf6d4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("401e8689-1f2c-5e6c-fb9e-e4d60525e39d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b08f3025-6404-a6ac-ff51-83cd50007fd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8395f34-5f8a-c127-d6ec-9feca2e315b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c15f7bde-7cd3-96b1-e563-5b90f8b3fd04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6b0a403-2128-32cd-ce1a-8f6af5c8f074"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9f3ffef-aafb-df6b-916a-04d7458fa652"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"));

            migrationBuilder.AddColumn<bool>(
                name: "OpenDefaultProject",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0213446a-8644-d22f-874f-cc35f2c9c3e6"), "", "liquidation.title" },
                    { new Guid("0db62c42-0434-bf8e-1965-63cbabee6752"), "", "liquidation.deficit" },
                    { new Guid("1493cf6e-a2fe-3b06-9120-ecb9f84aaec0"), "", "liquidation.question" },
                    { new Guid("1659585e-040c-53cb-aa54-a6c9b25ec2d5"), "", "liquidation.distributed" },
                    { new Guid("17398f8a-5f3d-21b0-d9d6-5507f99c5528"), "", "capital.incompleteLiquidation" },
                    { new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "", "capital.receiptAlreadyAttached" },
                    { new Guid("244400f9-b98c-3570-42df-a9205dd5c843"), "", "liquidation.done" },
                    { new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "", "liquidation.record" },
                    { new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "", "liquidation.attachReceipt" },
                    { new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "", "reports.liquidation" },
                    { new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "", "liquidation.late" },
                    { new Guid("3f071fbc-e41a-3b0e-820d-a5b00e168bf1"), "", "liquidation.explain" },
                    { new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "", "capital.unknownItem" },
                    { new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "", "projects.openDefault" },
                    { new Guid("46e98734-759f-aaa8-7c76-50ba076086d9"), "", "liquidation.date" },
                    { new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "", "liquidation.amount" },
                    { new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "", "capital.projectNotClosed" },
                    { new Guid("59e02361-2efd-6c13-e10b-31dd51827e33"), "", "liquidation.questionExplain" },
                    { new Guid("649ac112-55cc-0d09-f17b-82cf6972af58"), "", "liquidation.carry" },
                    { new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "", "liquidation.receiptAttached" },
                    { new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "", "projects.running" },
                    { new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "", "liquidation.lateRevenue" },
                    { new Guid("94f31a2a-95ea-4b2b-db84-0994898bc99d"), "", "capital.useLiquidation" },
                    { new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "", "projects.liquidateFirst" },
                    { new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "", "liquidation.item" },
                    { new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "", "liquidation.description" },
                    { new Guid("af285bfa-936f-201a-6605-7c062c92be61"), "", "liquidation.pending" },
                    { new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "", "liquidation.noReceipt" },
                    { new Guid("b3a43f40-9310-b867-5316-82973dcf8fcb"), "", "liquidation.confirm" },
                    { new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "", "projects.openDefaultHint" },
                    { new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "", "dashboard.revenueByCategory" },
                    { new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "", "liquidation.lateExpense" },
                    { new Guid("c47c7453-9601-018e-53e4-e368bd4cad38"), "", "capital.liquidation" },
                    { new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "", "liquidation.lateRecorded" },
                    { new Guid("d274cd87-fc95-0149-b7c5-984ad5e89340"), "", "liquidation.payOut" },
                    { new Guid("ee439aba-83f4-d59c-5de0-222b3b9e65e2"), "", "liquidation.deficitExplain" },
                    { new Guid("f4ea1db4-4a12-4857-656b-5d3ac1ec96b5"), "", "liquidation.surplus" },
                    { new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "", "liquidation.lateExplain" },
                    { new Guid("fd379b83-1e61-3eb8-4370-fa0efbce9ef6"), "", "liquidation.nothing" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"),
                column: "Value",
                value: "Transfers, capital movements, revenue, reconciling, the periodic report and liquidation.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4ba7de7-217d-f868-d3cc-259546894792"),
                column: "Value",
                value: "التحويلات وحركات رأس المال والإيرادات والمطابقة والتقرير الدوري والتصفية.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01fa9773-222c-dea8-68a7-92b0d7ea490b"), "ar", new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "لن تظهر هذه الشاشة عند الدخول" },
                    { new Guid("0f3ef5ac-285e-6176-5d27-e41f86a03114"), "ar", new Guid("d274cd87-fc95-0149-b7c5-984ad5e89340"), "صرف" },
                    { new Guid("1476922c-1699-51ad-ec2f-703d2d5d4acd"), "ar", new Guid("fd379b83-1e61-3eb8-4370-fa0efbce9ef6"), "لا يوجد ما يُوزَّع." },
                    { new Guid("14a6e1bd-5a0b-ddc1-e316-abd553c87aae"), "ar", new Guid("b3a43f40-9310-b867-5316-82973dcf8fcb"), "تصفية وإغلاق" },
                    { new Guid("181d8abb-b72e-4b6d-ee30-3f8143a778f2"), "en", new Guid("1659585e-040c-53cb-aa54-a6c9b25ec2d5"), "Shared out" },
                    { new Guid("1b204491-bfd5-d807-4748-9e68b7189da5"), "en", new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "Open the default project directly" },
                    { new Guid("1b5dbc82-9bd5-f67f-8799-878a34237c99"), "ar", new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "سُجِّل ووُزِّع على الشركاء." },
                    { new Guid("1d276a2e-693a-2f80-32e0-7fa56ccaa2f4"), "en", new Guid("fd379b83-1e61-3eb8-4370-fa0efbce9ef6"), "Nothing is left to share out." },
                    { new Guid("1f0d6a28-50c8-e164-4d9c-6afaacb7ab6f"), "en", new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "It is shared among the partners by their shares at closing: each pays in their part of an expense, and takes their part of revenue." },
                    { new Guid("20985fbe-18e1-b140-a6b6-daa2cbc16d1a"), "en", new Guid("17398f8a-5f3d-21b0-d9d6-5507f99c5528"), "Say how each partner's part goes." },
                    { new Guid("212a14f2-8c5b-8d45-3922-803079bbbe83"), "ar", new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "لا يزال فائضه لم يوزَّع على الشركاء، فصفِّه بدلاً من ذلك، وبالتصفية يُغلق." },
                    { new Guid("2209644f-178c-a172-79c2-a88e66753697"), "ar", new Guid("1493cf6e-a2fe-3b06-9120-ecb9f84aaec0"), "تصفية هذا المشروع وإغلاقه؟" },
                    { new Guid("2bb823a4-afcf-2e9f-4a01-b05f06dd0ffd"), "ar", new Guid("17398f8a-5f3d-21b0-d9d6-5507f99c5528"), "حدّد طريقة صرف حصة كل شريك." },
                    { new Guid("3063c96f-4f0c-3bde-9bea-d30f29fcd9b1"), "ar", new Guid("0db62c42-0434-bf8e-1965-63cbabee6752"), "العجز النهائي" },
                    { new Guid("3647c8d9-e49c-f00d-18d5-5d19e0a2d315"), "ar", new Guid("c47c7453-9601-018e-53e4-e368bd4cad38"), "حصة التصفية" },
                    { new Guid("376a6684-a674-8e61-0c34-eca05c7c1b06"), "ar", new Guid("af285bfa-936f-201a-6605-7c062c92be61"), "بانتظار الاعتماد: {0}. اعتمدها أو ارفضها قبل التصفية." },
                    { new Guid("3b814a1d-0103-ae4a-865f-6e94a1776f43"), "en", new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "Revenue by category" },
                    { new Guid("413c8e33-9074-1d4c-1e52-584a3c9f462b"), "en", new Guid("b3a43f40-9310-b867-5316-82973dcf8fcb"), "Liquidate and close" },
                    { new Guid("41c24de3-a743-2d2a-9616-80e737b7c9e0"), "ar", new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "إرفاق الإيصال" },
                    { new Guid("42c1a391-380a-2b1d-abb2-6b5dd5285825"), "ar", new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "فتح المشروع الافتراضي مباشرة" },
                    { new Guid("42eff101-5d18-09b7-d5bf-2f564301086e"), "ar", new Guid("3f071fbc-e41a-3b0e-820d-a5b00e168bf1"), "يأخذ كل شريك نسبته من الفائض صرفاً أو ترحيلاً إلى مشروع لاحق، ثم يُغلق المشروع ولا يتغير فيه شيء بعدها." },
                    { new Guid("45429a16-2a16-915f-3882-28f0f2bfed99"), "ar", new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "لا يوجد إيصال بعد" },
                    { new Guid("4956f064-b12d-28e0-c5b1-051a0dd60c3a"), "en", new Guid("2be0475f-6671-2bd8-5206-cdf82bba7fc4"), "Attach receipt" },
                    { new Guid("4a0a5a42-b0a2-d246-1638-baebdcff664e"), "ar", new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "التصفية" },
                    { new Guid("4da6f79c-4986-37e0-98e1-328596f3b55e"), "en", new Guid("d274cd87-fc95-0149-b7c5-984ad5e89340"), "Pay out" },
                    { new Guid("50902e08-40f5-85e9-a14d-51ec013dbdaf"), "en", new Guid("244400f9-b98c-3570-42df-a9205dd5c843"), "The project is liquidated and closed." },
                    { new Guid("52b1c353-bd46-1428-11f1-1b3654f1c102"), "en", new Guid("ee439aba-83f4-d59c-5de0-222b3b9e65e2"), "Each partner pays their share of the deficit in, or it comes off their balance in a later project, and the project closes." },
                    { new Guid("563031c8-7783-4fd3-17f1-347183abac42"), "en", new Guid("9ecb3887-438b-b959-0225-ac73ebde0ecd"), "Its surplus is still to be shared among the partners, so liquidate it instead: that closes it." },
                    { new Guid("59b7b483-20e6-0d4f-320c-f22efaad162b"), "ar", new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "إيراد متأخر" },
                    { new Guid("5cf73c67-94e3-866c-fa78-d1e14fc0bceb"), "ar", new Guid("bec51f8c-2fde-78a4-2b2a-8d7fb5664d3c"), "الإيرادات حسب التصنيف" },
                    { new Guid("5e8362bc-1f01-cc74-1b3d-161a7827f2a6"), "ar", new Guid("ee439aba-83f4-d59c-5de0-222b3b9e65e2"), "يدفع كل شريك نسبته من العجز، أو تُخصم من رصيده في مشروع لاحق، ثم يُغلق المشروع." },
                    { new Guid("5eec2fad-14c6-5123-1f1f-8a0c748fcbde"), "ar", new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "تم إرفاق الإيصال." },
                    { new Guid("62d178e3-514f-a6ee-b465-2e04df9276c4"), "ar", new Guid("244400f9-b98c-3570-42df-a9205dd5c843"), "تمت تصفية المشروع وإغلاقه." },
                    { new Guid("686132ab-e386-4220-ca42-12480a98d8e0"), "ar", new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "لهذه الحصة إيصالها من قبل." },
                    { new Guid("7532f470-f8ef-fa47-5363-eaab93a7fa8a"), "en", new Guid("3f071fbc-e41a-3b0e-820d-a5b00e168bf1"), "Each partner takes their share of the surplus, paid out or carried to a later project, and the project closes. Nothing in it changes after that." },
                    { new Guid("77afa977-3682-1324-dc25-4f56ec1ddd60"), "en", new Guid("46e98734-759f-aaa8-7c76-50ba076086d9"), "Date" },
                    { new Guid("7ec270f0-6548-4ff6-7457-249ed8b5a14c"), "en", new Guid("0db62c42-0434-bf8e-1965-63cbabee6752"), "Final deficit" },
                    { new Guid("8305d368-a0f5-cb42-e001-99fa61d1d928"), "en", new Guid("94f31a2a-95ea-4b2b-db84-0994898bc99d"), "A partner's part of the liquidation is shared out when the project is liquidated." },
                    { new Guid("8596d182-7f81-6a2b-2fce-9bbc7d13d7fd"), "en", new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "Amount" },
                    { new Guid("8c9ae49b-64f3-b76e-753a-d982f4845c44"), "en", new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "This screen will not show at sign-in." },
                    { new Guid("8ec1c022-ae64-86dd-b9dd-73e7480594a3"), "en", new Guid("6677ac85-94ad-7cd6-dab3-5f601539c3be"), "The receipt is attached." },
                    { new Guid("94f02ac4-ec30-8fa3-5376-2582adc9640c"), "ar", new Guid("f4ea1db4-4a12-4857-656b-5d3ac1ec96b5"), "الفائض النهائي" },
                    { new Guid("9e32ae9f-a44c-e641-bb1e-cf6ac533cd5a"), "en", new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "Record a late expense or revenue" },
                    { new Guid("9eb1d243-636e-5012-8903-fa6691d2f679"), "ar", new Guid("649ac112-55cc-0d09-f17b-82cf6972af58"), "ترحيل" },
                    { new Guid("a0316e5c-38cd-6872-2e27-955e56fb4f41"), "en", new Guid("b37d5cd8-a532-c8da-e119-a2851c3dc58b"), "No receipt yet" },
                    { new Guid("a1a21a02-a134-acb8-3b1b-19e9d83dd2c0"), "en", new Guid("f4ea1db4-4a12-4857-656b-5d3ac1ec96b5"), "Final surplus" },
                    { new Guid("a87ee58e-7104-6b53-261f-8755ad423a9d"), "en", new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "Record" },
                    { new Guid("af44cf6f-a01a-577d-cf6b-64f7e72348cb"), "en", new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "Running" },
                    { new Guid("b416538d-88e1-08f4-6422-1d4ece5060e4"), "en", new Guid("59e02361-2efd-6c13-e10b-31dd51827e33"), "Each part is entered as chosen and the project closes. This cannot be undone." },
                    { new Guid("b6c1b002-3319-e673-2dbb-96145536ba2a"), "ar", new Guid("94f31a2a-95ea-4b2b-db84-0994898bc99d"), "تُوزَّع حصة الشريك من التصفية عند تصفية المشروع." },
                    { new Guid("b887bc1c-1a29-1677-556e-bb69f35e23cb"), "en", new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "Only a closed project takes a late expense or revenue." },
                    { new Guid("b9d61caa-18aa-7ce7-fd0a-131060a36c1f"), "ar", new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "البند" },
                    { new Guid("ba23d8c5-6e94-4825-6fce-77c835560065"), "en", new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "Choose an expense item that takes purchases, or a revenue item." },
                    { new Guid("c1d9d47a-9e2c-d240-e10c-55fa1d7d9511"), "en", new Guid("c47c7453-9601-018e-53e4-e368bd4cad38"), "Liquidation share" },
                    { new Guid("c40f40f1-bb3b-f76b-2cf0-fcea961fdc5a"), "ar", new Guid("59e02361-2efd-6c13-e10b-31dd51827e33"), "تُقيَّد كل حصة كما اخترت ويُغلق المشروع. لا يمكن التراجع عن ذلك." },
                    { new Guid("c4cf616f-c659-2528-4a37-add2c38c517a"), "ar", new Guid("3e7c8c12-6059-c2cf-2217-dc01f51ea080"), "تسجيل مصروف أو إيراد متأخر" },
                    { new Guid("c546ba65-ee62-a31e-8428-74795869a8d0"), "ar", new Guid("0213446a-8644-d22f-874f-cc35f2c9c3e6"), "التصفية والإغلاق" },
                    { new Guid("c6dcd7cb-1528-75bb-8bf5-ef15fef83753"), "en", new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "Description" },
                    { new Guid("c859bdac-e662-373f-60ee-7ba8a6d733c7"), "en", new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "Liquidation" },
                    { new Guid("cb98a194-5e2e-835b-b8d4-48b0211043b0"), "ar", new Guid("53d96146-35f2-ff26-1a30-34f622ea0f6e"), "المبلغ" },
                    { new Guid("cc181cbf-7679-59ce-f35b-37a08964c41e"), "ar", new Guid("26492bd0-3bfc-4b14-dc48-ae331b07f456"), "تسجيل" },
                    { new Guid("cd475901-fd44-ee35-8327-3249d52c2b5f"), "ar", new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "مصروف متأخر" },
                    { new Guid("ce1c860d-79f0-de5e-905d-ccf1a261c251"), "en", new Guid("93a5fc72-0f6b-6f24-03e8-5f5850de5ccb"), "Late revenue" },
                    { new Guid("d0c57f3c-deab-8bba-ec5f-faabfc51c1fd"), "ar", new Guid("40137928-dfde-90c3-74c1-245a0a374a1e"), "اختر بند مصروفات يقبل المشتريات، أو بند إيرادات." },
                    { new Guid("d0cb2bda-d4e3-b3d0-97bb-77e0b228d33a"), "ar", new Guid("54d7466b-07c3-fecc-2029-80a607b67cdd"), "لا يُسجَّل مصروف أو إيراد متأخر إلا في مشروع مغلق." },
                    { new Guid("d10ee5f4-88f7-6fb3-3dca-042147cde652"), "ar", new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "جارٍ" },
                    { new Guid("d3747326-e320-f435-dd51-daa3a64575f4"), "en", new Guid("1493cf6e-a2fe-3b06-9120-ecb9f84aaec0"), "Liquidate and close this project?" },
                    { new Guid("d7e78449-1936-2343-6d0d-32746aea4c38"), "en", new Guid("c36a0dcc-1dd3-e179-ea3e-08d00755251f"), "Late expense" },
                    { new Guid("e15589c7-490a-092d-ab41-1c40e3f42479"), "en", new Guid("0213446a-8644-d22f-874f-cc35f2c9c3e6"), "Liquidate and close" },
                    { new Guid("e3d3b7a3-6c14-faf6-89c8-5b1ece5ab024"), "en", new Guid("c70850d4-ac42-0cfc-4e09-b7311dc82aac"), "Recorded and shared among the partners." },
                    { new Guid("e4dc9526-3c24-7c38-24f8-52f2208c67da"), "ar", new Guid("1659585e-040c-53cb-aa54-a6c9b25ec2d5"), "ما وُزِّع" },
                    { new Guid("e6890e09-24b3-4e3a-72aa-7fadb095f251"), "en", new Guid("18635f0c-752f-7e03-65f7-a738c2cfdcf0"), "This part already has its receipt." },
                    { new Guid("e6c9a973-faab-0d72-d84f-e5e24076c644"), "ar", new Guid("aaf37bef-eaac-bcff-806c-474a60d7981a"), "الوصف" },
                    { new Guid("eb5e77f8-6c3f-a041-4f0f-bbfd223f18aa"), "ar", new Guid("46e98734-759f-aaa8-7c76-50ba076086d9"), "التاريخ" },
                    { new Guid("f4024adf-fa8b-16a0-53de-8eff92ced802"), "en", new Guid("a2b0ccd0-6829-ec5e-1f22-2a5eb525479d"), "Item" },
                    { new Guid("f8474702-408e-9dd4-cd19-5698355a707a"), "en", new Guid("af285bfa-936f-201a-6605-7c062c92be61"), "Waiting for approval: {0}. Approve or reject them before liquidating." },
                    { new Guid("fbf13a83-0fb7-e7ee-c1c4-be3662b83aed"), "ar", new Guid("fb4170e6-bff8-ba29-5fa3-bcef4ce1358c"), "يوزَّع على الشركاء بنسبهم عند الإغلاق: يدفع كل شريك حصته من المصروف، ويأخذ حصته من الإيراد." },
                    { new Guid("fbf55fae-4cce-ff38-2b99-a7f44e017c8b"), "en", new Guid("649ac112-55cc-0d09-f17b-82cf6972af58"), "Carry" }
                });
        }
    }
}
