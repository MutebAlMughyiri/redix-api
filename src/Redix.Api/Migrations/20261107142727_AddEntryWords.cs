using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00d5981b-744a-50d5-842a-2246aed3a34c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("088a6cbf-03c8-3144-17ef-71f61f5c0fcf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("313acc71-6830-3e3a-b79b-d0ccfb384f6e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49f73665-df67-cae7-4293-246b32c87cf7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a7962a8-cf01-3fa1-ff0a-6c5039855a1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ba314ee-52b4-30f4-57f3-0ac5c664ee0f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("616bb83c-44a2-8482-f1b5-636d46e76b39"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a6b1eda-22fc-9945-7b2d-88eacb56fed0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71ac38db-1766-433d-2876-6a28d610b81f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f81435b-711a-b969-5505-46d13786830e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a178b654-4d77-a92d-9911-613af738fd5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b7600b23-02f6-da47-b67c-4ee9751568a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0e0e21e-2b4b-24ec-e972-f4b74d91414c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2feb7c7-f067-5b72-acf8-d8b204cd6104"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c89fb114-b98b-2496-6265-2718ec88059b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "", "entries.found" },
                    { new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "", "entries.clearFilters" },
                    { new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "", "entries.table" },
                    { new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "", "entries.enteredBy" },
                    { new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "", "entries.contractorPayments" },
                    { new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "", "entries.search" },
                    { new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "", "entries.purchases" },
                    { new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "", "entries.period" },
                    { new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "", "entries.new" },
                    { new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "", "entries.searchHint" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("092d0b8a-f7fe-14b4-c03f-25f7028d963b"), "ar", new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "الفترة" },
                    { new Guid("1a58c845-64ab-a927-e20c-3f4893a1a068"), "ar", new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "البحث في القيود" },
                    { new Guid("20cc0dfa-08a1-7a9c-7b86-d4ba5e5f3e3c"), "ar", new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "{0} قيد" },
                    { new Guid("2bf004c2-bc55-cbed-49ee-42f3fce6a5d8"), "en", new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "Clear filters" },
                    { new Guid("32ba74b5-198d-4e98-1601-ea8a617ebcfc"), "ar", new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "أدخله {0}" },
                    { new Guid("3eb23b5b-fd16-1f57-0f5f-334051a6e206"), "en", new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "Item, supplier, who entered it, number or amount" },
                    { new Guid("41ce2cd4-bd69-986e-f46f-738db04bab78"), "en", new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"), "{0} entries" },
                    { new Guid("4fe0d45b-bada-bc47-9016-a1bd40f6d23f"), "en", new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "Table" },
                    { new Guid("68047c66-4617-69f6-81e8-7edf53c7a81b"), "ar", new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "البند أو المورد أو من أدخله أو الرقم أو المبلغ" },
                    { new Guid("6bbb42c7-a370-3160-55f0-0f3f44885030"), "ar", new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "دفعات مقاولين" },
                    { new Guid("7f0f7f9d-5122-a2a0-84ed-d9b1df17f168"), "ar", new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "إضافة قيد" },
                    { new Guid("80e4223f-d7d0-074d-d05c-6bab80ead9ba"), "en", new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "Period" },
                    { new Guid("83c0fdf7-9e65-1478-9d0c-f25c7db06694"), "ar", new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "مسح التصفية" },
                    { new Guid("844f5420-59c6-f292-73c8-444e7caf41dd"), "en", new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "Search entries" },
                    { new Guid("87895462-7ad3-1510-af16-0ef1cb9dd232"), "ar", new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "مشتريات" },
                    { new Guid("933d15b2-8b99-72b0-077b-358732d39b51"), "ar", new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "الجدول" },
                    { new Guid("abcb61ac-ca6e-fd7d-c4a0-e2faacf69ca7"), "en", new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "Contractor payments" },
                    { new Guid("bc0681ee-d10f-180f-f3e1-e69128b2d8c0"), "en", new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "Add entry" },
                    { new Guid("d864ad82-617c-4f49-776f-9bece77202ee"), "en", new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "By {0}" },
                    { new Guid("df4bea51-871a-3c27-66cf-faf13c74541d"), "en", new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "Purchases" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("092d0b8a-f7fe-14b4-c03f-25f7028d963b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a58c845-64ab-a927-e20c-3f4893a1a068"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20cc0dfa-08a1-7a9c-7b86-d4ba5e5f3e3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bf004c2-bc55-cbed-49ee-42f3fce6a5d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32ba74b5-198d-4e98-1601-ea8a617ebcfc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3eb23b5b-fd16-1f57-0f5f-334051a6e206"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41ce2cd4-bd69-986e-f46f-738db04bab78"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fe0d45b-bada-bc47-9016-a1bd40f6d23f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68047c66-4617-69f6-81e8-7edf53c7a81b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6bbb42c7-a370-3160-55f0-0f3f44885030"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7f0f7f9d-5122-a2a0-84ed-d9b1df17f168"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80e4223f-d7d0-074d-d05c-6bab80ead9ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83c0fdf7-9e65-1478-9d0c-f25c7db06694"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("844f5420-59c6-f292-73c8-444e7caf41dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87895462-7ad3-1510-af16-0ef1cb9dd232"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("933d15b2-8b99-72b0-077b-358732d39b51"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("abcb61ac-ca6e-fd7d-c4a0-e2faacf69ca7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc0681ee-d10f-180f-f3e1-e69128b2d8c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d864ad82-617c-4f49-776f-9bece77202ee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df4bea51-871a-3c27-66cf-faf13c74541d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("12dd46fc-7db1-d9d5-b7e0-8f5134e5c573"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("85f57724-f694-714f-0253-f870bfdb25a2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"));

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "", "contractorPayments.contract" },
                    { new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "", "expenses.subgroup" },
                    { new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "", "contractorPayments.new" },
                    { new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "", "expenses.generalSubgroup" },
                    { new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "", "contractorPayments.date" },
                    { new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "", "expenses.new" },
                    { new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "", "entries.latest" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00d5981b-744a-50d5-842a-2246aed3a34c"), "en", new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "General" },
                    { new Guid("088a6cbf-03c8-3144-17ef-71f61f5c0fcf"), "ar", new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "العقد" },
                    { new Guid("313acc71-6830-3e3a-b79b-d0ccfb384f6e"), "ar", new Guid("4861aab6-d719-1fb5-e675-aeec1bf8bcca"), "عام" },
                    { new Guid("49f73665-df67-cae7-4293-246b32c87cf7"), "ar", new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "آخر قيودي" },
                    { new Guid("4a7962a8-cf01-3fa1-ff0a-6c5039855a1c"), "ar", new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "المجموعة الفرعية" },
                    { new Guid("5ba314ee-52b4-30f4-57f3-0ac5c664ee0f"), "en", new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "New purchase expense" },
                    { new Guid("616bb83c-44a2-8482-f1b5-636d46e76b39"), "ar", new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "التاريخ" },
                    { new Guid("6a6b1eda-22fc-9945-7b2d-88eacb56fed0"), "en", new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "My latest entries" },
                    { new Guid("71ac38db-1766-433d-2876-6a28d610b81f"), "en", new Guid("15c28535-89bc-0b53-9a07-2e2ab6313ee0"), "Subgroup" },
                    { new Guid("9f81435b-711a-b969-5505-46d13786830e"), "en", new Guid("0cd22008-4e3c-cb6e-f801-b4ea43ff3402"), "Contract" },
                    { new Guid("a178b654-4d77-a92d-9911-613af738fd5a"), "ar", new Guid("c89fb114-b98b-2496-6265-2718ec88059b"), "مصروف شراء جديد" },
                    { new Guid("b7600b23-02f6-da47-b67c-4ee9751568a2"), "en", new Guid("6f574846-3aa0-5e99-c771-90ff632f264e"), "Date" },
                    { new Guid("f0e0e21e-2b4b-24ec-e972-f4b74d91414c"), "en", new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "Contractor payment" },
                    { new Guid("f2feb7c7-f067-5b72-acf8-d8b204cd6104"), "ar", new Guid("4219e4e9-ba66-9c71-f0de-100b7f9b5040"), "دفعة مقاول" }
                });
        }
    }
}
