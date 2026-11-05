using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntriesWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "", "entries.entryNotApproved" },
                    { new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "", "entries.pendingApproval" },
                    { new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "", "entries.entryWaitingForDecision" },
                    { new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "", "entries.reversed" },
                    { new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "", "entries.reverseExplain" },
                    { new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "", "entries.seeAll" },
                    { new Guid("49918492-7c51-8293-228b-6515961524d9"), "", "entries.reasonRequired" },
                    { new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "", "entries.draft" },
                    { new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "", "entries.rejected" },
                    { new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "", "entries.noConnection" },
                    { new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "", "entries.all" },
                    { new Guid("67b07ccb-bbad-f754-7e2c-95b922c6ebf6"), "", "entries.deleteQuestion" },
                    { new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "", "entries.reason" },
                    { new Guid("6c1f74f9-0fb1-ccd3-850a-850c6038fa84"), "", "entries.delete" },
                    { new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "", "entries.entryApproved" },
                    { new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "", "entries.reverse" },
                    { new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "", "entries.refused" },
                    { new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "", "entries.fix" },
                    { new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "", "entries.entryHasCreditNote" },
                    { new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "", "entries.loadFailed" },
                    { new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "", "entries.send" },
                    { new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "", "entries.approved" },
                    { new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "", "entries.edit" },
                    { new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "", "entries.title" },
                    { new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "", "entries.empty" },
                    { new Guid("d0f1c346-3d32-bd88-74e9-9f55fcb3b4ad"), "", "entries.deleteExplain" },
                    { new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "", "entries.alreadyReversed" },
                    { new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "", "entries.latest" },
                    { new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "", "entries.reverseReason" },
                    { new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "", "entries.mine" },
                    { new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "", "entries.reverseTitle" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("028fb5de-c7b6-a668-18ef-8bab350c4b20"), "en", new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "Approved" },
                    { new Guid("02c9eaf9-787d-6bbf-7f4b-e12e0f0f9b8f"), "ar", new Guid("d0f1c346-3d32-bd88-74e9-9f55fcb3b4ad"), "لم يُعتمد قط، فلم يُحتسب. ويبقى ما كان فيه في سجل التدقيق." },
                    { new Guid("037588db-c795-9cd1-edef-1fa9dc5dd008"), "ar", new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "بانتظار الاعتماد" },
                    { new Guid("0de9850d-d3f9-d187-e743-fb6b2fc077cd"), "ar", new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "تعذر تنفيذ ذلك." },
                    { new Guid("0f0a8dec-222e-d150-8db8-ef94320526f5"), "en", new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "Fix and send again" },
                    { new Guid("19c0565e-ddba-8a29-eb90-642d7f8a4387"), "en", new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "This waits for Finance's decision, so it cannot change now." },
                    { new Guid("1ed669e7-afc6-36ca-7823-5d19ecb95952"), "en", new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "Reverse this approved entry" },
                    { new Guid("2030e402-8de2-fac2-5d2d-7056400c69de"), "en", new Guid("6c1f74f9-0fb1-ccd3-850a-850c6038fa84"), "Delete" },
                    { new Guid("254a86f6-7f8c-3a8b-0cb7-2bcac036d61f"), "ar", new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "تعديل" },
                    { new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"), "ar", new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "عكس القيد" },
                    { new Guid("2f76b791-3886-e852-4cf5-312a600d7dd9"), "ar", new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("31335350-a480-235b-d0b4-c0f3d2918f8f"), "ar", new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "معكوس" },
                    { new Guid("3194494e-f789-a642-1cae-3623afddcb1e"), "ar", new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "ينتظر هذا القيد قرار المالي، فلا يتغيّر الآن." },
                    { new Guid("34891a4c-1dd4-73ee-27c7-30ae57f1f9f2"), "en", new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "Mine" },
                    { new Guid("380f2856-0edb-7026-540f-ac27975e4e54"), "ar", new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "القيود" },
                    { new Guid("386c21e1-bb40-666d-117d-7fe443230bd2"), "ar", new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "السبب (إلزامي)" },
                    { new Guid("3b9fe6f5-dcd2-ca08-5dca-8c3e5edea319"), "ar", new Guid("49918492-7c51-8293-228b-6515961524d9"), "اذكر السبب." },
                    { new Guid("43e7cdbb-a032-18e8-65a0-08beeff9c39e"), "en", new Guid("d0f1c346-3d32-bd88-74e9-9f55fcb3b4ad"), "It was never approved, so nothing counted it. The audit trail keeps what it held." },
                    { new Guid("44c40e8f-3807-b8d4-f80c-f31bc7159333"), "en", new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "This could not be done." },
                    { new Guid("49f73665-df67-cae7-4293-246b32c87cf7"), "ar", new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "آخر قيودي" },
                    { new Guid("4d1f8109-7ad0-3ebc-b142-2fcbc2be5288"), "en", new Guid("67b07ccb-bbad-f754-7e2c-95b922c6ebf6"), "Delete this entry?" },
                    { new Guid("4e9f3d8f-f146-c123-5b76-7404413f51ff"), "en", new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "This entry was already reversed." },
                    { new Guid("591b3266-4151-1e1c-6a7a-7214427bfda5"), "ar", new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"), "عكس هذا القيد المعتمد" },
                    { new Guid("63ed8453-6f35-eee2-3d9d-22ce3bdb9a02"), "ar", new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "اعتمد المالي هذا القيد قبل وصول تعديلك، فلم يُطبَّق تعديلك." },
                    { new Guid("6813967c-e70e-9293-8c2e-4bcc0e109817"), "en", new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "Waiting for approval" },
                    { new Guid("6a6b1eda-22fc-9945-7b2d-88eacb56fed0"), "en", new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"), "My latest entries" },
                    { new Guid("6b407479-99dd-22f6-527e-f9e4de15c2fb"), "ar", new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "السبب" },
                    { new Guid("6b42f598-8569-1fdf-eb47-7abda83e7ff6"), "en", new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "Send for approval" },
                    { new Guid("7171e170-0567-d5af-bc06-a6c9c8369cc3"), "ar", new Guid("6c1f74f9-0fb1-ccd3-850a-850c6038fa84"), "حذف" },
                    { new Guid("73300cc3-2450-fd26-b078-e38691f5ca9d"), "ar", new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "مرفوض" },
                    { new Guid("74293746-66ae-0f3e-4d08-05c0ca97fdb8"), "en", new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"), "Reason (required)" },
                    { new Guid("753fa9ff-b2c7-9e47-86e8-87c8c3e57748"), "en", new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "Edit" },
                    { new Guid("766077bb-e32a-60a9-c67e-776ab9f647f7"), "ar", new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "لا قيود بعد." },
                    { new Guid("7c83c5b7-e684-a93a-c6f0-b8e0bb0f8e15"), "en", new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "Reason" },
                    { new Guid("83181a9f-53d2-9d80-c082-6ff417656f32"), "ar", new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "لا يُعكس إلا القيد المعتمد." },
                    { new Guid("88e57667-defe-76b6-5c1e-e2ab43ad1350"), "en", new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "Rejected" },
                    { new Guid("910e13a3-9bbd-343b-d29e-ce3ff1f94bfb"), "ar", new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "مرتجع يعيد هذه الفاتورة، فلا تُحذف." },
                    { new Guid("9265d48b-0aee-184a-2357-5c80fc637298"), "en", new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "Could not read the entries. Check the connection and try again." },
                    { new Guid("9f77f9c2-a131-c951-3994-74d0d68009e6"), "ar", new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "مسودة" },
                    { new Guid("a024db6b-8b4b-a0bc-5103-ce2e0fd895ef"), "ar", new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "معتمد" },
                    { new Guid("a250177a-544c-04fc-930b-cf7b5fff9c3d"), "en", new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"), "Reversed" },
                    { new Guid("a4b402bc-e919-54e5-baf3-9bbf207ff9cc"), "ar", new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "كل القيود" },
                    { new Guid("a89b5b26-0a78-a41b-4356-c49f7683e6b0"), "en", new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"), "All entries" },
                    { new Guid("a8f39fc0-eeed-f4bf-5205-1b9379bb2c18"), "ar", new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"), "عُكس هذا القيد من قبل." },
                    { new Guid("aa0bd5ac-e38d-d709-5598-6f24b17750f2"), "ar", new Guid("67b07ccb-bbad-f754-7e2c-95b922c6ebf6"), "حذف هذا القيد؟" },
                    { new Guid("aba727bb-c6fd-dedd-f8b8-0356a52a044c"), "en", new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "Finance approved this before your change arrived, so your change was not applied." },
                    { new Guid("ac10a866-e15d-3df6-71fd-267c394ad399"), "ar", new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "قيودي" },
                    { new Guid("b387e1e0-396a-4592-4bd8-288b84b909f4"), "ar", new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "الكل" },
                    { new Guid("b4d51a4e-0907-9ec9-2c38-015a9d6a7217"), "en", new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "Draft" },
                    { new Guid("b57355cb-cfb7-9027-c354-74805711707a"), "en", new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"), "Only an approved entry can be reversed." },
                    { new Guid("b74efbbc-e2b5-96e9-28f9-654a876eac0b"), "en", new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "No entries yet." },
                    { new Guid("c8b21ef9-b0a3-e4a3-95a4-df794a11f315"), "en", new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"), "A credit note returns this invoice, so it cannot be deleted." },
                    { new Guid("caeac573-0954-ef80-378f-ea758267a0af"), "ar", new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "إرسال للاعتماد" },
                    { new Guid("cd61fbfa-ad7e-a23b-5b5c-e781be3f0056"), "en", new Guid("49918492-7c51-8293-228b-6515961524d9"), "Say why." },
                    { new Guid("d3a8c4a6-c289-7be1-fac0-d10b7ca5612a"), "en", new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "No connection to the server. Try again." },
                    { new Guid("d518697f-d192-4e67-cf26-77c7932d3e1e"), "ar", new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "يُضاف قيد مقابل مرتبط بهذا القيد ومعتمد فوراً. يظهران في التقارير وصافيهما صفر. ولا يتغيّر هذا القيد." },
                    { new Guid("da3c932a-97a1-f9bc-a825-824932e35d3c"), "en", new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"), "An opposite entry is added, tied to this one and approved at once. Both show in the reports, and their net is zero. This one does not change." },
                    { new Guid("dec6a131-a057-7d43-38b3-ec5d4403a656"), "ar", new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "تعديل وإعادة إرسال" },
                    { new Guid("f54525a2-3ac5-203a-e96f-df5ad53f8a99"), "ar", new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "تعذرت قراءة القيود. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f8382ac4-1a11-7d3e-04e5-9ce9f7fb051d"), "en", new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "Entries" },
                    { new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"), "en", new Guid("72685414-803a-e33f-9629-d4021c1518ed"), "Reverse" },
                    { new Guid("fcdbce38-dcc3-b303-1564-61e08f824aba"), "en", new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"), "All" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("028fb5de-c7b6-a668-18ef-8bab350c4b20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02c9eaf9-787d-6bbf-7f4b-e12e0f0f9b8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("037588db-c795-9cd1-edef-1fa9dc5dd008"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0de9850d-d3f9-d187-e743-fb6b2fc077cd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f0a8dec-222e-d150-8db8-ef94320526f5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("19c0565e-ddba-8a29-eb90-642d7f8a4387"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ed669e7-afc6-36ca-7823-5d19ecb95952"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2030e402-8de2-fac2-5d2d-7056400c69de"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("254a86f6-7f8c-3a8b-0cb7-2bcac036d61f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("276e0c02-f441-86d6-e744-ec59a7f911bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f76b791-3886-e852-4cf5-312a600d7dd9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("31335350-a480-235b-d0b4-c0f3d2918f8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3194494e-f789-a642-1cae-3623afddcb1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("34891a4c-1dd4-73ee-27c7-30ae57f1f9f2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("380f2856-0edb-7026-540f-ac27975e4e54"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("386c21e1-bb40-666d-117d-7fe443230bd2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b9fe6f5-dcd2-ca08-5dca-8c3e5edea319"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("43e7cdbb-a032-18e8-65a0-08beeff9c39e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44c40e8f-3807-b8d4-f80c-f31bc7159333"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("49f73665-df67-cae7-4293-246b32c87cf7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d1f8109-7ad0-3ebc-b142-2fcbc2be5288"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e9f3d8f-f146-c123-5b76-7404413f51ff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("591b3266-4151-1e1c-6a7a-7214427bfda5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63ed8453-6f35-eee2-3d9d-22ce3bdb9a02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6813967c-e70e-9293-8c2e-4bcc0e109817"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a6b1eda-22fc-9945-7b2d-88eacb56fed0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b407479-99dd-22f6-527e-f9e4de15c2fb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b42f598-8569-1fdf-eb47-7abda83e7ff6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7171e170-0567-d5af-bc06-a6c9c8369cc3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73300cc3-2450-fd26-b078-e38691f5ca9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("74293746-66ae-0f3e-4d08-05c0ca97fdb8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("753fa9ff-b2c7-9e47-86e8-87c8c3e57748"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("766077bb-e32a-60a9-c67e-776ab9f647f7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7c83c5b7-e684-a93a-c6f0-b8e0bb0f8e15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83181a9f-53d2-9d80-c082-6ff417656f32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88e57667-defe-76b6-5c1e-e2ab43ad1350"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("910e13a3-9bbd-343b-d29e-ce3ff1f94bfb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9265d48b-0aee-184a-2357-5c80fc637298"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f77f9c2-a131-c951-3994-74d0d68009e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a024db6b-8b4b-a0bc-5103-ce2e0fd895ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a250177a-544c-04fc-930b-cf7b5fff9c3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a4b402bc-e919-54e5-baf3-9bbf207ff9cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a89b5b26-0a78-a41b-4356-c49f7683e6b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8f39fc0-eeed-f4bf-5205-1b9379bb2c18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa0bd5ac-e38d-d709-5598-6f24b17750f2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aba727bb-c6fd-dedd-f8b8-0356a52a044c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac10a866-e15d-3df6-71fd-267c394ad399"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b387e1e0-396a-4592-4bd8-288b84b909f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b4d51a4e-0907-9ec9-2c38-015a9d6a7217"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b57355cb-cfb7-9027-c354-74805711707a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b74efbbc-e2b5-96e9-28f9-654a876eac0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8b21ef9-b0a3-e4a3-95a4-df794a11f315"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("caeac573-0954-ef80-378f-ea758267a0af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd61fbfa-ad7e-a23b-5b5c-e781be3f0056"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d3a8c4a6-c289-7be1-fac0-d10b7ca5612a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d518697f-d192-4e67-cf26-77c7932d3e1e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("da3c932a-97a1-f9bc-a825-824932e35d3c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dec6a131-a057-7d43-38b3-ec5d4403a656"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f54525a2-3ac5-203a-e96f-df5ad53f8a99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8382ac4-1a11-7d3e-04e5-9ce9f7fb051d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbd93af4-c12c-a781-50d7-8b94e1597b2b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcdbce38-dcc3-b303-1564-61e08f824aba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("07ebee71-3de0-cd34-05c1-4ac7531e3e4f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31777cd1-60a8-52f7-7344-3f55662a5459"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("31fddf41-54af-9aee-2106-9788e3b924b3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37bda13f-26e0-1aa4-fc29-45772db0ddc8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49918492-7c51-8293-228b-6515961524d9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("533f51e7-d239-a203-6b25-506321cf9034"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("60ebd03f-285e-78b7-dded-d778c38b36e8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("67b07ccb-bbad-f754-7e2c-95b922c6ebf6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6c1f74f9-0fb1-ccd3-850a-850c6038fa84"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("72685414-803a-e33f-9629-d4021c1518ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8d9f89f7-6f6f-2c60-f2fd-97b9cb038610"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9373c407-fce7-6561-e99a-48237e06394f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d0f1c346-3d32-bd88-74e9-9f55fcb3b4ad"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d496a29e-93aa-0fa0-edad-cdf08c4c269f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d61db440-d69d-54a4-1c7f-f2700847fd0e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e90fe5e4-95c3-2916-834e-4b9060c80126"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f02ebbf0-5d27-3984-ab37-0aa1b719436a"));
        }
    }
}
