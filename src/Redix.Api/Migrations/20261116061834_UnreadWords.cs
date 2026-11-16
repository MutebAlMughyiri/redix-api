using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnreadWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01e5480e-eb6a-432c-009f-a7b7e95570b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0302765a-207a-ef61-b080-edcafcd746be"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09673683-2250-fca5-7d94-213d082127f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f201428-1dbf-942f-846e-6bab11651c61"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1649079c-ff19-3707-a4e7-6526e35265b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24fc50d0-951c-b5c1-b71c-743af5799106"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28c68a9b-b43c-e11b-a3db-4747ce7b04a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2924c1e9-3455-6329-eefc-c20015df9d83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32179368-0d1d-84b6-fcdd-d899caf3d8e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36661599-e814-80b8-2bb6-dc1b66ee8caa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3cbf9aa9-b363-af0d-1a0d-75315fd6558d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46253d83-6248-a436-b5a1-8961fd9bde78"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c616d31-bed1-d391-9d2b-a1ae970fc863"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("500e9ea9-5c04-5553-4a7e-f79933275c60"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73b9b7d9-252a-12b3-0f7a-a20582ea149e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f2a7431-ac6e-5028-0440-f83d99c91a4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7fa24744-2a43-b4f2-3e6a-76c638faa527"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("935e5aea-3540-b23a-6eb8-338e5bb7b8a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9590ea1f-33cd-eaa2-cd37-20193c7736ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98f62942-8ec3-1862-f134-6cf46723655b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9924ff9a-241a-bef7-bdd6-052d9b7b5cb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e981665-3ac8-1a4e-7838-c3566161ffd4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a831aa9c-0c16-8050-a4cd-ffcf2f830f0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aaaef9fb-7bc9-1a27-fe73-cffa18868e1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0f7b8d5-978c-bc00-135f-c8e18d3c6d5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b3d2cce1-0cb6-36ce-a025-1d132c6893cf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b488c2f2-6fc7-daf2-ab23-0fe7690d6e05"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2d75122-0484-0453-3314-154cc0b08f1f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c7e5ba40-7c21-9b54-4352-81aadbfeb5f2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9628631-0fec-75a3-8e06-74305a66a871"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce98fab6-382b-23aa-8678-1c0d30ec55bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d2b0830e-d2ed-9e32-ec2c-d81942647d73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d894983e-cd89-4065-ffa2-24e8be2274e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dcec7463-fb3d-212e-5bd4-05380592cb6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e64dbe64-5c66-07e5-b91f-6434c70108b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eee66bc5-8de0-c896-d146-3329eba9c822"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7269c82-69c8-c0d5-a7d9-4b53d9147aab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7295045-a04d-27e3-f29f-d4d4d2b6de83"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9c7207b-d1a7-015e-f3b5-ba217e465eb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd846041-794a-7018-17db-e9e25479bb90"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("627e20df-8451-b393-4dca-65e11f806396"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8b35c979-312d-449d-541a-49971dd1d280"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "", "columns.sign" },
                    { new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "", "capital.description" },
                    { new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "", "reports.description" },
                    { new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "", "revenue.total" },
                    { new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "", "revenue.date" },
                    { new Guid("627e20df-8451-b393-4dca-65e11f806396"), "", "expenses.item" },
                    { new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "", "capital.type" },
                    { new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "", "capital.date" },
                    { new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "", "capital.partner" },
                    { new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "", "capital.noPartners" },
                    { new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "", "entries.tiedProjectRefused" },
                    { new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "", "capital.amount" },
                    { new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "", "reconciliations.title" },
                    { new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "", "capital.share" },
                    { new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "", "capital.save" },
                    { new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "", "revenue.title" },
                    { new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "", "capital.account" },
                    { new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "", "reports.date" },
                    { new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "", "capital.balance" },
                    { new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "", "reports.number" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01e5480e-eb6a-432c-009f-a7b7e95570b6"), "ar", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "التاريخ" },
                    { new Guid("0302765a-207a-ef61-b080-edcafcd746be"), "en", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "Share %" },
                    { new Guid("09673683-2250-fca5-7d94-213d082127f4"), "en", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "Date" },
                    { new Guid("0f201428-1dbf-942f-846e-6bab11651c61"), "ar", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "الحسابات والمطابقة" },
                    { new Guid("1649079c-ff19-3707-a4e7-6526e35265b9"), "en", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "Description" },
                    { new Guid("24fc50d0-951c-b5c1-b71c-743af5799106"), "ar", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "الحساب" },
                    { new Guid("28c68a9b-b43c-e11b-a3db-4747ce7b04a3"), "en", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "Account" },
                    { new Guid("2924c1e9-3455-6329-eefc-c20015df9d83"), "ar", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "الوصف" },
                    { new Guid("32179368-0d1d-84b6-fcdd-d899caf3d8e4"), "en", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "Accounts and reconciliation" },
                    { new Guid("36661599-e814-80b8-2bb6-dc1b66ee8caa"), "en", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "Movement type" },
                    { new Guid("3cbf9aa9-b363-af0d-1a0d-75315fd6558d"), "ar", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "الإيرادات" },
                    { new Guid("46253d83-6248-a436-b5a1-8961fd9bde78"), "en", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "Description" },
                    { new Guid("4c616d31-bed1-d391-9d2b-a1ae970fc863"), "en", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "No." },
                    { new Guid("500e9ea9-5c04-5553-4a7e-f79933275c60"), "ar", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "يرتبط هذا بقيد في مشروع لا يمكنك تغييره." },
                    { new Guid("73b9b7d9-252a-12b3-0f7a-a20582ea149e"), "ar", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "الإجمالي" },
                    { new Guid("7f2a7431-ac6e-5028-0440-f83d99c91a4d"), "en", new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "Sign" },
                    { new Guid("7fa24744-2a43-b4f2-3e6a-76c638faa527"), "en", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "Revenue" },
                    { new Guid("935e5aea-3540-b23a-6eb8-338e5bb7b8a6"), "ar", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "م" },
                    { new Guid("9590ea1f-33cd-eaa2-cd37-20193c7736ad"), "ar", new Guid("171eeff3-87f8-03aa-838a-91d8808e4b96"), "الإشارة" },
                    { new Guid("98f62942-8ec3-1862-f134-6cf46723655b"), "ar", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "الوصف" },
                    { new Guid("9924ff9a-241a-bef7-bdd6-052d9b7b5cb9"), "ar", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "المبلغ" },
                    { new Guid("9e981665-3ac8-1a4e-7838-c3566161ffd4"), "ar", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "النسبة %" },
                    { new Guid("a831aa9c-0c16-8050-a4cd-ffcf2f830f0b"), "en", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "Item" },
                    { new Guid("aaaef9fb-7bc9-1a27-fe73-cffa18868e1d"), "en", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "Balance" },
                    { new Guid("b0f7b8d5-978c-bc00-135f-c8e18d3c6d5d"), "en", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "This goes with an entry in a project you cannot change." },
                    { new Guid("b3d2cce1-0cb6-36ce-a025-1d132c6893cf"), "en", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "Total" },
                    { new Guid("b488c2f2-6fc7-daf2-ab23-0fe7690d6e05"), "en", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "Amount" },
                    { new Guid("c2d75122-0484-0453-3314-154cc0b08f1f"), "ar", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "الرصيد" },
                    { new Guid("c7e5ba40-7c21-9b54-4352-81aadbfeb5f2"), "en", new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "There is no partner to share it among." },
                    { new Guid("c9628631-0fec-75a3-8e06-74305a66a871"), "ar", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "البند" },
                    { new Guid("ce98fab6-382b-23aa-8678-1c0d30ec55bf"), "ar", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "التاريخ" },
                    { new Guid("d2b0830e-d2ed-9e32-ec2c-d81942647d73"), "en", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "Date" },
                    { new Guid("d894983e-cd89-4065-ffa2-24e8be2274e3"), "ar", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "التاريخ" },
                    { new Guid("dcec7463-fb3d-212e-5bd4-05380592cb6b"), "ar", new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "لا يوجد شريك لتوزيعه عليه." },
                    { new Guid("e64dbe64-5c66-07e5-b91f-6434c70108b5"), "ar", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "الشريك" },
                    { new Guid("eee66bc5-8de0-c896-d146-3329eba9c822"), "en", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "Save the movement" },
                    { new Guid("f7269c82-69c8-c0d5-a7d9-4b53d9147aab"), "en", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "Partner" },
                    { new Guid("f7295045-a04d-27e3-f29f-d4d4d2b6de83"), "en", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "Date" },
                    { new Guid("f9c7207b-d1a7-015e-f3b5-ba217e465eb3"), "ar", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "نوع الحركة" },
                    { new Guid("fd846041-794a-7018-17db-e9e25479bb90"), "ar", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "حفظ الحركة" }
                });
        }
    }
}
