using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "", "expenses.notYourEntry" },
                    { new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "", "expenses.entryHasCreditNote" },
                    { new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "", "expenses.entryNotApproved" },
                    { new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "", "expenses.entryWaitingForDecision" },
                    { new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "", "expenses.alreadyReversed" },
                    { new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "", "expenses.entryApproved" },
                    { new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "", "expenses.reasonRequired" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01694d75-7033-d04e-06d9-306c04c2db34"), "ar", new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "عُكس هذا القيد من قبل." },
                    { new Guid("0ad10ec7-88f3-277a-b0bc-eb9e58b8a4c2"), "en", new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"), "This entry was already reversed." },
                    { new Guid("16879e8e-745e-5921-3ea0-7f1680a38f62"), "en", new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "Only an approved entry can be reversed." },
                    { new Guid("1f6b8966-00b7-308d-7b23-f7b41f6687ef"), "en", new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "A credit note returns this invoice, so it cannot be deleted." },
                    { new Guid("30ad34e1-f173-0b01-e898-449911fbf599"), "ar", new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "اعتمد المالي هذا القيد قبل وصول تعديلك، فلم يُطبَّق تعديلك." },
                    { new Guid("322f0e08-e8a6-23cc-5ffd-8c980e0c320a"), "en", new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "Finance approved this before your change arrived, so your change was not applied." },
                    { new Guid("4d3851dd-c698-e1cf-46c8-8e457e1a9c34"), "ar", new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "اذكر السبب." },
                    { new Guid("63767ad6-b82c-0813-b699-b3f70dedb365"), "ar", new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "أدخل غيرك هذا القيد." },
                    { new Guid("8c0601ee-c4da-4edc-022a-e11c367fb930"), "ar", new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"), "مرتجع يعيد هذه الفاتورة، فلا تُحذف." },
                    { new Guid("afe9a25c-46ab-4369-d0e6-c992d6c2e5d4"), "ar", new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "ينتظر هذا القيد قرار المالي، فلا يتغيّر الآن." },
                    { new Guid("b2098f9f-8854-55e4-20c6-dad34dbf4cb5"), "en", new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "Someone else entered this." },
                    { new Guid("b908cf51-8972-34ea-abb4-58f7bfd7488f"), "en", new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "Say why." },
                    { new Guid("c7ba7166-640a-9c59-2812-f69518b7e9a3"), "ar", new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"), "لا يُعكس إلا القيد المعتمد." },
                    { new Guid("f7dae556-d44e-0abe-6a1e-27968a00713d"), "en", new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "This waits for Finance's decision, so it cannot change now." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01694d75-7033-d04e-06d9-306c04c2db34"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ad10ec7-88f3-277a-b0bc-eb9e58b8a4c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("16879e8e-745e-5921-3ea0-7f1680a38f62"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1f6b8966-00b7-308d-7b23-f7b41f6687ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("30ad34e1-f173-0b01-e898-449911fbf599"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("322f0e08-e8a6-23cc-5ffd-8c980e0c320a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d3851dd-c698-e1cf-46c8-8e457e1a9c34"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63767ad6-b82c-0813-b699-b3f70dedb365"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c0601ee-c4da-4edc-022a-e11c367fb930"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("afe9a25c-46ab-4369-d0e6-c992d6c2e5d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2098f9f-8854-55e4-20c6-dad34dbf4cb5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b908cf51-8972-34ea-abb4-58f7bfd7488f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c7ba7166-640a-9c59-2812-f69518b7e9a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7dae556-d44e-0abe-6a1e-27968a00713d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3c41c81f-0bd5-6108-a85f-b49d449924a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("765796e5-4f37-dbab-d121-c4047ec5eec6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9180bb24-8611-f55d-1c59-278fe50adf46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"));
        }
    }
}
