using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SharedSettingsWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04fac440-5447-6f74-cb3e-beade2f428d5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0dc91d60-c309-90a8-583b-29f24651409b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("555eb16f-bbf6-874c-fa77-89bba965b2ee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5cc6bba5-6613-2892-3e72-5feab68d8c5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("64276b86-36c3-7d98-efa2-b35f315bcbe9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76d8affa-b976-88a7-850c-cb610593e451"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("76de0119-2172-a198-4763-844f1a9a05ed"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8290689e-2e70-b72f-eff1-761b0d2a9695"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a9070b1-2af3-6b99-a2c4-29b522cbfb76"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8f4a8d17-47f3-6822-41cc-b806013bb8ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95a954d2-4cf5-497b-7b6d-34e8c7bfed75"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f6ec8dc-7a7e-bb74-7e2f-8c069958baf4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a48e5aae-8851-95c8-b440-5fa518b12842"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b130dd15-6cf1-87eb-c083-ca3cf6135e1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf714f93-2a1f-629a-bb60-2edb51025389"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c532c4a2-d031-dba7-d453-865987152951"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d186998a-5aed-fc1f-8646-8134f16da248"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea1779aa-19c7-fe8f-d884-f6e8146264a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ed32b049-bd8e-372e-ebd7-931f3535d192"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f3e418a7-d1bf-4423-e4d8-7606a8d5f7aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("53038282-4269-1366-be57-56958dad11a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"));

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
                    { new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "", "sharedSettings.entriesShownHint" },
                    { new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "", "sharedSettings.defaultProject" },
                    { new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "", "sharedSettings.switchProject" },
                    { new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "", "sharedSettings.defaultProjectHint" },
                    { new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "", "sharedSettings.themeSystem" },
                    { new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "", "sharedSettings.themeLight" },
                    { new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "", "sharedSettings.themeDark" },
                    { new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "", "sharedSettings.entriesShown" },
                    { new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "", "sharedSettings.appearance" },
                    { new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "", "sharedSettings.title" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0fa683df-3120-2dab-ce21-b44b92f449c5"), "ar", new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "تبديل المشروع" },
                    { new Guid("176bcd59-73ed-e737-fea1-237072c8372d"), "ar", new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "النظام" },
                    { new Guid("1be91f58-09d1-680a-2bcf-434059164ea3"), "ar", new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "داكن" },
                    { new Guid("32296e23-e84b-e1d7-56f5-5d374ed44256"), "en", new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "Switch project" },
                    { new Guid("4a4a6268-c323-067a-3622-36dfdab57fc7"), "ar", new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "عدد القيود المعروضة" },
                    { new Guid("52d36e53-7194-c20f-2584-4f0fba64d6b3"), "en", new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "Appearance" },
                    { new Guid("5413b105-ff81-dda9-eab6-5ff0e881a869"), "en", new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "Settings" },
                    { new Guid("62de20e4-bb93-0d2d-774d-1c6a445d63df"), "en", new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "Tick the project that opens when you sign in. Untick it to see the list instead." },
                    { new Guid("666670a1-0c62-3c57-6caf-029c8395b848"), "ar", new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "المظهر" },
                    { new Guid("6bdf5f67-e9dc-22bc-1da6-8b7ae4f6ec0b"), "en", new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "Light" },
                    { new Guid("6cc8e010-24d1-9c49-cd1a-0e8ef2f6ae2c"), "ar", new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "فاتح" },
                    { new Guid("7970121f-bb93-19c7-7b59-17644b42fb4d"), "en", new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "Default project" },
                    { new Guid("90dbd797-b11c-75d6-96ed-00f46da23f80"), "ar", new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "عدد القيود في الرئيسية، وعدد ما يضيفه كل «عرض المزيد»." },
                    { new Guid("a68616c7-7ae7-0795-859d-532446851903"), "en", new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "Entries shown" },
                    { new Guid("b1ad78b1-29af-25bb-25f7-5bf7b9a19b12"), "ar", new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "الإعدادات" },
                    { new Guid("d1885949-a744-d536-a983-56f47fbd09e1"), "ar", new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "المشروع الافتراضي" },
                    { new Guid("d6b3e79a-de10-b5a3-d80c-aab6df19ca35"), "en", new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "Dark" },
                    { new Guid("daaf9e11-a6e3-f149-7cd5-91049a23c2e2"), "en", new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "How many entries the home shows, and how many each Show more adds." },
                    { new Guid("e4060706-fac1-de53-48f3-3c3c72d10922"), "ar", new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("e9901f44-61d1-3c11-b67c-9d13e8436cc5"), "en", new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fa683df-3120-2dab-ce21-b44b92f449c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("176bcd59-73ed-e737-fea1-237072c8372d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1be91f58-09d1-680a-2bcf-434059164ea3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32296e23-e84b-e1d7-56f5-5d374ed44256"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a4a6268-c323-067a-3622-36dfdab57fc7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52d36e53-7194-c20f-2584-4f0fba64d6b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5413b105-ff81-dda9-eab6-5ff0e881a869"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62de20e4-bb93-0d2d-774d-1c6a445d63df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("666670a1-0c62-3c57-6caf-029c8395b848"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6bdf5f67-e9dc-22bc-1da6-8b7ae4f6ec0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6cc8e010-24d1-9c49-cd1a-0e8ef2f6ae2c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7970121f-bb93-19c7-7b59-17644b42fb4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90dbd797-b11c-75d6-96ed-00f46da23f80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a68616c7-7ae7-0795-859d-532446851903"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1ad78b1-29af-25bb-25f7-5bf7b9a19b12"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d1885949-a744-d536-a983-56f47fbd09e1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6b3e79a-de10-b5a3-d80c-aab6df19ca35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("daaf9e11-a6e3-f149-7cd5-91049a23c2e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4060706-fac1-de53-48f3-3c3c72d10922"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9901f44-61d1-3c11-b67c-9d13e8436cc5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b9b1e037-874f-9086-e211-5b66759e9303"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "", "generalSettings.defaultProjectHint" },
                    { new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "", "generalSettings.themeLight" },
                    { new Guid("53038282-4269-1366-be57-56958dad11a6"), "", "generalSettings.entriesShown" },
                    { new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "", "generalSettings.entriesShownHint" },
                    { new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "", "generalSettings.themeSystem" },
                    { new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "", "generalSettings.themeDark" },
                    { new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "", "generalSettings.switchProject" },
                    { new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "", "generalSettings.appearance" },
                    { new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "", "generalSettings.defaultProject" },
                    { new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "", "generalSettings.title" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("04fac440-5447-6f74-cb3e-beade2f428d5"), "ar", new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "تبديل المشروع" },
                    { new Guid("0dc91d60-c309-90a8-583b-29f24651409b"), "ar", new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "النظام" },
                    { new Guid("555eb16f-bbf6-874c-fa77-89bba965b2ee"), "en", new Guid("53038282-4269-1366-be57-56958dad11a6"), "Entries shown" },
                    { new Guid("5cc6bba5-6613-2892-3e72-5feab68d8c5f"), "ar", new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "فاتح" },
                    { new Guid("64276b86-36c3-7d98-efa2-b35f315bcbe9"), "ar", new Guid("53038282-4269-1366-be57-56958dad11a6"), "عدد القيود المعروضة" },
                    { new Guid("76d8affa-b976-88a7-850c-cb610593e451"), "ar", new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "عدد القيود في الرئيسية، وعدد ما يضيفه كل «عرض المزيد»." },
                    { new Guid("76de0119-2172-a198-4763-844f1a9a05ed"), "en", new Guid("7c0675ad-0ac1-8f57-7ce1-3ac6f9acfcd0"), "System" },
                    { new Guid("8290689e-2e70-b72f-eff1-761b0d2a9695"), "ar", new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("8a9070b1-2af3-6b99-a2c4-29b522cbfb76"), "ar", new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "المظهر" },
                    { new Guid("8f4a8d17-47f3-6822-41cc-b806013bb8ab"), "en", new Guid("bbd52e0f-38bc-104b-91b5-cf5aa50801d0"), "Switch project" },
                    { new Guid("95a954d2-4cf5-497b-7b6d-34e8c7bfed75"), "en", new Guid("e197ff82-0114-e69a-45cd-99467e47eba9"), "Appearance" },
                    { new Guid("9f6ec8dc-7a7e-bb74-7e2f-8c069958baf4"), "en", new Guid("0b30ab2b-fd33-4f8d-a55b-041f9f86feae"), "Tick the project that opens when you sign in. Untick it to see the list instead." },
                    { new Guid("a48e5aae-8851-95c8-b440-5fa518b12842"), "en", new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "Dark" },
                    { new Guid("b130dd15-6cf1-87eb-c083-ca3cf6135e1e"), "en", new Guid("1efac1a6-f5a6-5ad3-a43b-f54268a613f5"), "Light" },
                    { new Guid("bf714f93-2a1f-629a-bb60-2edb51025389"), "ar", new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "الإعدادات" },
                    { new Guid("c532c4a2-d031-dba7-d453-865987152951"), "ar", new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "المشروع الافتراضي" },
                    { new Guid("d186998a-5aed-fc1f-8646-8134f16da248"), "en", new Guid("f146410b-eb35-20d5-bb4c-81a27a39e607"), "Default project" },
                    { new Guid("ea1779aa-19c7-fe8f-d884-f6e8146264a0"), "en", new Guid("6b38ca68-e080-1d1b-d552-23c9c87fad0a"), "How many entries the home shows, and how many each Show more adds." },
                    { new Guid("ed32b049-bd8e-372e-ebd7-931f3535d192"), "ar", new Guid("7f045587-a0fa-3148-b7a0-5b4e0497c480"), "داكن" },
                    { new Guid("f3e418a7-d1bf-4423-e4d8-7606a8d5f7aa"), "en", new Guid("f63f2faf-8687-e64a-236f-2d46048c67b3"), "Settings" }
                });
        }
    }
}
