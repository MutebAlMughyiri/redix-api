using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SummaryAndGeneralSettingsWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ac451ab-833e-5b48-b0bd-cd41b29ca1ae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0bbcbcb5-e7d8-1b5a-0eb1-42e984ad4aca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29d8b4a3-086b-f8b1-3147-7ed493115514"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38ff3397-850c-11cc-2d9f-f965502ea687"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f8a9725-8632-6380-41b4-f9dc04a66af1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("401e8689-1f2c-5e6c-fb9e-e4d60525e39d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("557adec9-b08c-04eb-fd9e-7f19cd6a79ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63780978-6ece-9380-8ede-8e57b671d82c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d0c01e1-12f6-dbc2-6f85-63eef5198c33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("972c19b6-535e-c396-c199-7edb1b8ddd33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a719b5e-fc79-3ab2-64af-caaaef29e274"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9af8f4df-bbce-fe91-634a-d6a013604197"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2254fbd-8540-9b79-3b14-427aba720006"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b08f3025-6404-a6ac-ff51-83cd50007fd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0ed5273-1e9c-305e-3b72-8a5bfd47fe2e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c15f7bde-7cd3-96b1-e563-5b90f8b3fd04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d008b5b0-7595-a29f-9cf9-19a214988e4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d45fcf59-6198-13cc-1962-4df6c356b9dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9f3ffef-aafb-df6b-916a-04d7458fa652"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0531341-4ac5-1ebc-36ce-317b4c39a8a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0eeb6f4-1e7d-6e35-6b3f-5d5d99d032af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2e72137-d344-f48e-440b-4fb203a802f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e5363956-8d6c-ad82-b964-582d9b662150"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eee79376-22ae-89c0-6768-76832ea9ac97"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f003bc9f-146d-fc7a-68ea-d1673fc1ac33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ff2b6273-9c51-af34-be96-2627870ec1aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52258464-29b6-3faa-92f6-fb62564c9634"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("559748fd-2331-110f-f452-2c94c8a2f260"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "", "generalSettings.defaultProjectHint" },
                    { new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "", "summary.surplus" },
                    { new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "", "summary.completion" },
                    { new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "", "summary.surplusWithoutSales" },
                    { new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "", "summary.pendingHint" },
                    { new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "", "summary.expenses" },
                    { new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "", "summary.byCategory" },
                    { new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "", "generalSettings.darkMode" },
                    { new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "", "summary.revenue" },
                    { new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "", "summary.difference" },
                    { new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "", "generalSettings.switchProject" },
                    { new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "", "summary.loadFailed" },
                    { new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "", "summary.pending" },
                    { new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "", "generalSettings.defaultProject" },
                    { new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "", "generalSettings.title" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("04fac440-5447-6f74-cb3e-beade2f428d5"), "ar", new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "تبديل المشروع" },
                    { new Guid("0a28ffc6-76ae-d470-bdb4-d42afadd268c"), "ar", new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "المصروفات حسب التصنيف" },
                    { new Guid("100b9879-686b-70c8-59a4-ee2ffd481c3d"), "en", new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "Without sales" },
                    { new Guid("10aaca51-635a-f28f-cacb-6cb7b7918bdd"), "en", new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "Tap to review with the invoices" },
                    { new Guid("1e171aa3-57a0-9bb4-22fd-a9715f281476"), "en", new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "Expenses" },
                    { new Guid("2f368baa-4303-b9e6-0b4b-952fbd670f75"), "ar", new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "فرق المطابقة مع الأرصدة" },
                    { new Guid("301f3e9d-68bd-7ad1-ba03-9e08433295f9"), "en", new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "Surplus (what the accounts should hold)" },
                    { new Guid("5ae095d9-2383-e5e9-6307-ddd0d545cba7"), "en", new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "Revenue" },
                    { new Guid("6083b683-5093-cb17-369d-1ff78c50d106"), "ar", new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "الإيرادات" },
                    { new Guid("730f2470-b846-db63-6151-f0ce073b316c"), "ar", new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "الفائض (المبلغ المفروض بالحساب)" },
                    { new Guid("734da565-e045-324a-1b27-e6dded36c8ba"), "en", new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "Completion" },
                    { new Guid("8290689e-2e70-b72f-eff1-761b0d2a9695"), "ar", new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("8f4a8d17-47f3-6822-41cc-b806013bb8ab"), "en", new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "Switch project" },
                    { new Guid("8f9cb151-d853-c355-7287-045a2fce25b3"), "ar", new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "المصروفات" },
                    { new Guid("9f6ec8dc-7a7e-bb74-7e2f-8c069958baf4"), "en", new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "Tick the project that opens when you sign in. Untick it to see the list instead." },
                    { new Guid("bf714f93-2a1f-629a-bb60-2edb51025389"), "ar", new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "الإعدادات" },
                    { new Guid("c047d356-3223-83cd-6643-144f0f6788df"), "ar", new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "بدون البيع" },
                    { new Guid("c0e46a69-e649-a9c3-b5fb-5a76924abebd"), "ar", new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "اضغط للمراجعة مع صور الفواتير" },
                    { new Guid("c532c4a2-d031-dba7-d453-865987152951"), "ar", new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "المشروع الافتراضي" },
                    { new Guid("c63a9606-d26a-1d6c-2098-3506b39467a4"), "en", new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "Expenses by category" },
                    { new Guid("c9014313-f285-f4e0-fa68-eb7d04eab008"), "en", new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "Could not read the figures. Check the connection and try again." },
                    { new Guid("d093b1e5-a8b6-e763-d9a4-c05c6508600d"), "ar", new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "بانتظار اعتمادك: {0}" },
                    { new Guid("d186998a-5aed-fc1f-8646-8134f16da248"), "en", new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "Default project" },
                    { new Guid("d441dd0e-330b-feaf-153f-099d902bfa6c"), "en", new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "Waiting for your approval: {0}" },
                    { new Guid("e98e96c5-7a16-1f35-ea6d-23225a6d6846"), "en", new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "Dark mode" },
                    { new Guid("ea0ba365-45bc-a997-1790-4678e3a7ea68"), "ar", new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "تعذرت قراءة الأرقام. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("eb401c0d-0dee-c2f4-a2bf-e6b3b271ed14"), "ar", new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"), "الوضع الداكن" },
                    { new Guid("ede1dd51-994b-e938-966e-25fb3bf3d09e"), "ar", new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "نسبة الإنجاز" },
                    { new Guid("f3e418a7-d1bf-4423-e4d8-7606a8d5f7aa"), "en", new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "Settings" },
                    { new Guid("ff92b910-f0e2-53b5-4f8e-04afde6cb610"), "en", new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "Reconciliation difference" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04fac440-5447-6f74-cb3e-beade2f428d5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0a28ffc6-76ae-d470-bdb4-d42afadd268c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("100b9879-686b-70c8-59a4-ee2ffd481c3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("10aaca51-635a-f28f-cacb-6cb7b7918bdd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1e171aa3-57a0-9bb4-22fd-a9715f281476"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f368baa-4303-b9e6-0b4b-952fbd670f75"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("301f3e9d-68bd-7ad1-ba03-9e08433295f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ae095d9-2383-e5e9-6307-ddd0d545cba7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6083b683-5093-cb17-369d-1ff78c50d106"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("730f2470-b846-db63-6151-f0ce073b316c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("734da565-e045-324a-1b27-e6dded36c8ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8290689e-2e70-b72f-eff1-761b0d2a9695"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f4a8d17-47f3-6822-41cc-b806013bb8ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f9cb151-d853-c355-7287-045a2fce25b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f6ec8dc-7a7e-bb74-7e2f-8c069958baf4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf714f93-2a1f-629a-bb60-2edb51025389"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c047d356-3223-83cd-6643-144f0f6788df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0e46a69-e649-a9c3-b5fb-5a76924abebd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c532c4a2-d031-dba7-d453-865987152951"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c63a9606-d26a-1d6c-2098-3506b39467a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9014313-f285-f4e0-fa68-eb7d04eab008"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d093b1e5-a8b6-e763-d9a4-c05c6508600d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d186998a-5aed-fc1f-8646-8134f16da248"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d441dd0e-330b-feaf-153f-099d902bfa6c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e98e96c5-7a16-1f35-ea6d-23225a6d6846"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea0ba365-45bc-a997-1790-4678e3a7ea68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb401c0d-0dee-c2f4-a2bf-e6b3b271ed14"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ede1dd51-994b-e938-966e-25fb3bf3d09e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f3e418a7-d1bf-4423-e4d8-7606a8d5f7aa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ff92b910-f0e2-53b5-4f8e-04afde6cb610"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33309176-122e-b83f-89ad-7b666e92093f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a81cac79-7208-4f80-8bda-b8881cd8323b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "", "dashboard.difference" },
                    { new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "", "dashboard.expenses" },
                    { new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "", "dashboard.pendingHint" },
                    { new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "", "settings.switchProject" },
                    { new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "", "dashboard.pending" },
                    { new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "", "dashboard.revenue" },
                    { new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "", "dashboard.completion" },
                    { new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "", "settings.defaultProjectHint" },
                    { new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "", "settings.darkMode" },
                    { new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "", "dashboard.surplus" },
                    { new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "", "dashboard.surplusWithoutSales" },
                    { new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "", "dashboard.byCategory" },
                    { new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "", "settings.defaultProject" },
                    { new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "", "settings.title" },
                    { new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "", "dashboard.loadFailed" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0ac451ab-833e-5b48-b0bd-cd41b29ca1ae"), "ar", new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "فرق المطابقة مع الأرصدة" },
                    { new Guid("0bbcbcb5-e7d8-1b5a-0eb1-42e984ad4aca"), "en", new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "Tap to review with the invoices" },
                    { new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"), "en", new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "Waiting for your approval: {0}" },
                    { new Guid("29d8b4a3-086b-f8b1-3147-7ed493115514"), "en", new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "Switch project" },
                    { new Guid("38ff3397-850c-11cc-2d9f-f965502ea687"), "ar", new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "اضغط للمراجعة مع صور الفواتير" },
                    { new Guid("3f8a9725-8632-6380-41b4-f9dc04a66af1"), "en", new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "Dark mode" },
                    { new Guid("401e8689-1f2c-5e6c-fb9e-e4d60525e39d"), "ar", new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "المشروع الافتراضي" },
                    { new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"), "ar", new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "بانتظار اعتمادك: {0}" },
                    { new Guid("557adec9-b08c-04eb-fd9e-7f19cd6a79ab"), "en", new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "Without sales" },
                    { new Guid("63780978-6ece-9380-8ede-8e57b671d82c"), "en", new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "Settings" },
                    { new Guid("7d0c01e1-12f6-dbc2-6f85-63eef5198c33"), "en", new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "Could not read the figures. Check the connection and try again." },
                    { new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"), "en", new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "Revenue" },
                    { new Guid("972c19b6-535e-c396-c199-7edb1b8ddd33"), "ar", new Guid("93237c57-3ce7-8e41-52fa-8620884cfc06"), "الوضع الداكن" },
                    { new Guid("9a719b5e-fc79-3ab2-64af-caaaef29e274"), "ar", new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "تبديل المشروع" },
                    { new Guid("9af8f4df-bbce-fe91-634a-d6a013604197"), "en", new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "Reconciliation difference" },
                    { new Guid("a2254fbd-8540-9b79-3b14-427aba720006"), "en", new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "Expenses by category" },
                    { new Guid("b08f3025-6404-a6ac-ff51-83cd50007fd5"), "en", new Guid("c75a69df-d529-56c2-3263-f4d0508ca209"), "Default project" },
                    { new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"), "ar", new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "الإيرادات" },
                    { new Guid("c0ed5273-1e9c-305e-3b72-8a5bfd47fe2e"), "ar", new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "الإعدادات" },
                    { new Guid("c15f7bde-7cd3-96b1-e563-5b90f8b3fd04"), "ar", new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("d008b5b0-7595-a29f-9cf9-19a214988e4d"), "ar", new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "الفائض (المبلغ المفروض بالحساب)" },
                    { new Guid("d45fcf59-6198-13cc-1962-4df6c356b9dd"), "ar", new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "نسبة الإنجاز" },
                    { new Guid("d9f3ffef-aafb-df6b-916a-04d7458fa652"), "en", new Guid("7f6e7447-8283-655a-3b1a-54520b2d1b2d"), "Tick the project that opens when you sign in. Untick it to see the list instead." },
                    { new Guid("e0531341-4ac5-1ebc-36ce-317b4c39a8a6"), "ar", new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "تعذرت قراءة الأرقام. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("e0eeb6f4-1e7d-6e35-6b3f-5d5d99d032af"), "en", new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "Expenses" },
                    { new Guid("e2e72137-d344-f48e-440b-4fb203a802f4"), "ar", new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "المصروفات" },
                    { new Guid("e5363956-8d6c-ad82-b964-582d9b662150"), "en", new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "Surplus (what the accounts should hold)" },
                    { new Guid("eee79376-22ae-89c0-6768-76832ea9ac97"), "en", new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "Completion" },
                    { new Guid("f003bc9f-146d-fc7a-68ea-d1673fc1ac33"), "ar", new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "بدون البيع" },
                    { new Guid("ff2b6273-9c51-af34-be96-2627870ec1aa"), "ar", new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "المصروفات حسب التصنيف" }
                });
        }
    }
}
