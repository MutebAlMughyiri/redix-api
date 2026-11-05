using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Dashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPercent",
                table: "Projects",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "CompletionPercent",
                value: 0m);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "", "reports.noContracts" },
                    { new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "", "dashboard.projectFinished" },
                    { new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "", "reports.refused" },
                    { new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "", "reports.openValue" },
                    { new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "", "reports.grandTotal" },
                    { new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "", "reports.itemTotal" },
                    { new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "", "dashboard.difference" },
                    { new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "", "reports.outsideContract" },
                    { new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "", "dashboard.save" },
                    { new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "", "reports.noConnection" },
                    { new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "", "dashboard.expenses" },
                    { new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "", "reports.description" },
                    { new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "", "dashboard.pendingHint" },
                    { new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "", "dashboard.cost" },
                    { new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "", "reports.invoiceNumber" },
                    { new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "", "dashboard.percent" },
                    { new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "", "dashboard.pending" },
                    { new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "", "dashboard.revenue" },
                    { new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "", "dashboard.completion" },
                    { new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "", "reports.cost" },
                    { new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "", "reports.chooseItem" },
                    { new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "", "reports.quantity" },
                    { new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "", "dashboard.noConnection" },
                    { new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "", "reports.unitPrice" },
                    { new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "", "dashboard.surplus" },
                    { new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "", "reports.partners" },
                    { new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "", "dashboard.surplusWithoutSales" },
                    { new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "", "dashboard.setCompletion" },
                    { new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "", "dashboard.byCategory" },
                    { new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "", "dashboard.refused" },
                    { new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "", "reports.generalSubgroup" },
                    { new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "", "reports.remaining" },
                    { new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "", "reports.paid" },
                    { new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "", "dashboard.invalidCompletion" },
                    { new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "", "reports.loadFailed" },
                    { new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "", "reports.overrun" },
                    { new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "", "reports.contractValue" },
                    { new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "", "reports.item" },
                    { new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "", "dashboard.loadFailed" },
                    { new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "", "reports.date" },
                    { new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "", "reports.contracts" },
                    { new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "", "reports.number" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01e5480e-eb6a-432c-009f-a7b7e95570b6"), "ar", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "التاريخ" },
                    { new Guid("03ed9b06-9e66-1525-6501-372fe562da51"), "en", new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "General" },
                    { new Guid("0ac451ab-833e-5b48-b0bd-cd41b29ca1ae"), "ar", new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "فرق المطابقة مع الأرصدة" },
                    { new Guid("0bbcbcb5-e7d8-1b5a-0eb1-42e984ad4aca"), "en", new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "Tap to review with the invoices" },
                    { new Guid("0c903942-d4fb-fcf1-0db7-aeeb446cc075"), "ar", new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "حفظ" },
                    { new Guid("108a9b9d-ad74-14d8-4e85-4dbc192fb791"), "en", new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "This project is closed." },
                    { new Guid("112b349b-af69-d360-8cd2-85fb5a676293"), "en", new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "Paid more than the contract" },
                    { new Guid("1473b04d-8954-569c-b771-953dbf7b2930"), "en", new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "Set completion" },
                    { new Guid("16c5e91a-4e90-c01d-f96b-1858d5f1b456"), "en", new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "This could not be done." },
                    { new Guid("1afd9746-e907-358a-5fae-91a359b89317"), "en", new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "Cost" },
                    { new Guid("2102fe1f-64c3-cf99-10e9-8c1eccac3852"), "ar", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "رقم الفاتورة" },
                    { new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"), "en", new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "{0} entries wait for your approval" },
                    { new Guid("27551e78-834a-b2be-5c05-da732d084acd"), "en", new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "Partners and capital" },
                    { new Guid("387d53d4-e312-064b-8ff4-8a4b3eaaedce"), "en", new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "Item total" },
                    { new Guid("38ff3397-850c-11cc-2d9f-f965502ea687"), "ar", new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"), "اضغط للمراجعة مع صور الفواتير" },
                    { new Guid("40161a06-bbb8-c042-0e96-45bd4735dcb9"), "ar", new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "إجمالي البند" },
                    { new Guid("44418224-6446-7b08-0aec-96566cfd1cef"), "ar", new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "تقرير بند" },
                    { new Guid("46253d83-6248-a436-b5a1-8961fd9bde78"), "en", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "Description" },
                    { new Guid("4c616d31-bed1-d391-9d2b-a1ae970fc863"), "en", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "No." },
                    { new Guid("4cac6532-1c33-9bcd-0505-068fbc63c665"), "en", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "Outside the contract" },
                    { new Guid("4f352c7a-7a79-ad94-a0d2-19f1401d6b6a"), "ar", new Guid("62d78811-d09f-1739-d022-2abea4b7f972"), "التكلفة" },
                    { new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"), "ar", new Guid("559748fd-2331-110f-f452-2c94c8a2f260"), "{0} مصروفات بانتظار اعتمادك" },
                    { new Guid("4ffb7347-a7b7-b809-201f-6493d715e4b0"), "en", new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "Total of all" },
                    { new Guid("5002d330-f762-a54f-f666-d4af30207d5a"), "ar", new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("557adec9-b08c-04eb-fd9e-7f19cd6a79ab"), "en", new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "Without sales" },
                    { new Guid("5e8b416f-733e-2c61-df31-17fe5ac5e604"), "ar", new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "مدفوع أكثر من العقد" },
                    { new Guid("60fadec6-7224-7fea-5d13-e021300d4c1a"), "en", new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "Contracts and what remains" },
                    { new Guid("6140c7de-1782-ec48-ba41-e735c4ab95df"), "en", new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"), "Item report" },
                    { new Guid("6165aa82-b26e-9df9-c816-a3d605685987"), "en", new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "Paid" },
                    { new Guid("61dbb54d-4f76-9f0f-91bd-202a3a229424"), "ar", new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "اختر البند" },
                    { new Guid("626d0a87-530c-3d94-54b0-67ff5f17fd22"), "en", new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"), "Save" },
                    { new Guid("6a6a10fb-bec1-098f-9a58-2a347e251b53"), "en", new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"), "Choose the item" },
                    { new Guid("71af5bb6-8cc2-7036-583a-61d5c4a48c73"), "ar", new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "التكلفة" },
                    { new Guid("7312ee8f-623d-19de-7965-4304250d4330"), "ar", new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "تعذرت قراءة التقرير. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("77083b08-3985-7d75-9099-9639cd7e831e"), "en", new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"), "Cost" },
                    { new Guid("7b2407f3-7db4-f958-76ff-e6fc4218b48f"), "en", new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "Percentage" },
                    { new Guid("7d0c01e1-12f6-dbc2-6f85-63eef5198c33"), "en", new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "Could not read the figures. Check the connection and try again." },
                    { new Guid("82e5e61e-97de-faaf-5e8a-a0639a099841"), "en", new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "No connection to the server. Try again." },
                    { new Guid("8a0a9852-488d-9072-e30c-7c6f8b3dd59b"), "ar", new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "المدفوع" },
                    { new Guid("8eab892f-9a86-e0df-d4a0-95fdff3b909a"), "ar", new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "الشركاء ورأس المال" },
                    { new Guid("901d38cd-65bb-a8ed-3402-46399c5d98a5"), "en", new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "Remaining" },
                    { new Guid("93433267-b4c8-f691-4f0b-6d126f8fafe7"), "ar", new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"), "هذا المشروع مغلق." },
                    { new Guid("935e5aea-3540-b23a-6eb8-338e5bb7b8a6"), "ar", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "م" },
                    { new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"), "en", new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "Total revenue" },
                    { new Guid("966b4a96-2eb3-5460-2714-aae81b49465f"), "en", new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "This could not be done." },
                    { new Guid("98f62942-8ec3-1862-f134-6cf46723655b"), "ar", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "الوصف" },
                    { new Guid("9a0e3ef5-bbe8-c872-ad04-77208f74bbac"), "ar", new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "اكتب نسبة من 0 إلى 100." },
                    { new Guid("9af8f4df-bbce-fe91-634a-d6a013604197"), "en", new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"), "Reconciliation difference" },
                    { new Guid("9c574008-128a-9bf4-a055-13697eefbb93"), "en", new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "No connection to the server. Try again." },
                    { new Guid("9f8f0047-8a7c-db10-e0e5-ebf700fa47e5"), "ar", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "العدد" },
                    { new Guid("a2254fbd-8540-9b79-3b14-427aba720006"), "en", new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "Expenses by category" },
                    { new Guid("a7fdd2d9-fd87-5970-f912-314438cd8ae9"), "ar", new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "قيمة العقد" },
                    { new Guid("b21d00f0-e6ac-0cc1-cc81-7eedd631dcda"), "en", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "Invoice no." },
                    { new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"), "ar", new Guid("5d82c713-7243-60fc-fdc2-7434c99bab87"), "مجموع الإيرادات" },
                    { new Guid("bffad5a3-ae8b-237c-d9c2-102afc62a0c0"), "en", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "Quantity" },
                    { new Guid("c0efb2c9-d9cb-f88d-5771-dae59a5cc617"), "en", new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "No contracts yet." },
                    { new Guid("c2073572-67a8-6929-8913-d87672d3b3ba"), "en", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "Unit price" },
                    { new Guid("c485cdcb-a72f-e65e-dcf0-081649bfa1ab"), "en", new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "Could not read the report. Check the connection and try again." },
                    { new Guid("c598c497-473c-93ec-0ac6-a20be7918b72"), "en", new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"), "Type a percentage from 0 to 100." },
                    { new Guid("cc3870ac-104c-49e4-e40d-ef8c85f38af0"), "en", new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "Open value" },
                    { new Guid("d008b5b0-7595-a29f-9cf9-19a214988e4d"), "ar", new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "الفائض (المبلغ المفروض بالحساب)" },
                    { new Guid("d06a7fa9-124b-a4a5-a47a-e70eb74eeab8"), "ar", new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "قيمة مفتوحة" },
                    { new Guid("d07c2efb-9b03-7d3d-adba-f66426f4c0c7"), "ar", new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "المتبقي" },
                    { new Guid("d1970d56-03c6-46ec-aad0-fb7034b4c13e"), "en", new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "Contract value" },
                    { new Guid("d2b0830e-d2ed-9e32-ec2c-d81942647d73"), "en", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "Date" },
                    { new Guid("d45fcf59-6198-13cc-1962-4df6c356b9dd"), "ar", new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "نسبة الإنجاز" },
                    { new Guid("d59eaa11-d0b8-238b-ff49-9ec23de9389b"), "ar", new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"), "تحديث نسبة الإنجاز" },
                    { new Guid("d651bece-29d9-3cf5-0be1-443eefcb462f"), "ar", new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"), "لا عقود بعد." },
                    { new Guid("d86f484a-4a46-6ebc-bb1a-63bc188663f8"), "ar", new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("db4a6104-5e3b-5410-c4ed-29390ae2aabb"), "ar", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "سعر الحبة" },
                    { new Guid("dd05bec7-4fcd-6d78-58e4-259c17bc0018"), "ar", new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "تعذر تنفيذ ذلك." },
                    { new Guid("e0531341-4ac5-1ebc-36ce-317b4c39a8a6"), "ar", new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"), "تعذرت قراءة الأرقام. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("e0eeb6f4-1e7d-6e35-6b3f-5d5d99d032af"), "en", new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "Total expenses" },
                    { new Guid("e2e72137-d344-f48e-440b-4fb203a802f4"), "ar", new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"), "مجموع المصروفات" },
                    { new Guid("e5363956-8d6c-ad82-b964-582d9b662150"), "en", new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"), "Surplus (what the accounts should hold)" },
                    { new Guid("e578855c-1e0f-0d35-05f8-b74185534636"), "ar", new Guid("50f26485-a173-102e-1d66-878f59f53c42"), "النسبة" },
                    { new Guid("ee66f33a-25fc-f1a1-591a-e601c2910591"), "ar", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "خارج العقد" },
                    { new Guid("eee79376-22ae-89c0-6768-76832ea9ac97"), "en", new Guid("5ea0b9ae-225b-160f-9c05-8cce74b9149c"), "Completion" },
                    { new Guid("f003bc9f-146d-fc7a-68ea-d1673fc1ac33"), "ar", new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"), "بدون البيع" },
                    { new Guid("f0fb6378-fa35-2388-1599-e2031a182022"), "ar", new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"), "تعذر تنفيذ ذلك." },
                    { new Guid("f2a96fd3-7748-de55-a631-d1631801442d"), "ar", new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"), "العقود والمتبقي" },
                    { new Guid("fac0b0e6-4155-8db6-6c9e-05de1e5a335f"), "ar", new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "اجمالي الكل" },
                    { new Guid("fb393a9c-4886-a9f9-0e2b-69034ea2fe20"), "ar", new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "عام" },
                    { new Guid("ff2b6273-9c51-af34-be96-2627870ec1aa"), "ar", new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"), "المصروفات حسب التصنيف" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01e5480e-eb6a-432c-009f-a7b7e95570b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03ed9b06-9e66-1525-6501-372fe562da51"));

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
                keyValue: new Guid("0c903942-d4fb-fcf1-0db7-aeeb446cc075"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("108a9b9d-ad74-14d8-4e85-4dbc192fb791"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("112b349b-af69-d360-8cd2-85fb5a676293"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1473b04d-8954-569c-b771-953dbf7b2930"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("16c5e91a-4e90-c01d-f96b-1858d5f1b456"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1afd9746-e907-358a-5fae-91a359b89317"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2102fe1f-64c3-cf99-10e9-8c1eccac3852"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27551e78-834a-b2be-5c05-da732d084acd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("387d53d4-e312-064b-8ff4-8a4b3eaaedce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38ff3397-850c-11cc-2d9f-f965502ea687"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40161a06-bbb8-c042-0e96-45bd4735dcb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44418224-6446-7b08-0aec-96566cfd1cef"));

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
                keyValue: new Guid("4cac6532-1c33-9bcd-0505-068fbc63c665"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f352c7a-7a79-ad94-a0d2-19f1401d6b6a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ffb7347-a7b7-b809-201f-6493d715e4b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5002d330-f762-a54f-f666-d4af30207d5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("557adec9-b08c-04eb-fd9e-7f19cd6a79ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5e8b416f-733e-2c61-df31-17fe5ac5e604"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60fadec6-7224-7fea-5d13-e021300d4c1a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6140c7de-1782-ec48-ba41-e735c4ab95df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6165aa82-b26e-9df9-c816-a3d605685987"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61dbb54d-4f76-9f0f-91bd-202a3a229424"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("626d0a87-530c-3d94-54b0-67ff5f17fd22"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a6a10fb-bec1-098f-9a58-2a347e251b53"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71af5bb6-8cc2-7036-583a-61d5c4a48c73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7312ee8f-623d-19de-7965-4304250d4330"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77083b08-3985-7d75-9099-9639cd7e831e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7b2407f3-7db4-f958-76ff-e6fc4218b48f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d0c01e1-12f6-dbc2-6f85-63eef5198c33"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82e5e61e-97de-faaf-5e8a-a0639a099841"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8a0a9852-488d-9072-e30c-7c6f8b3dd59b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8eab892f-9a86-e0df-d4a0-95fdff3b909a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("901d38cd-65bb-a8ed-3402-46399c5d98a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93433267-b4c8-f691-4f0b-6d126f8fafe7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("935e5aea-3540-b23a-6eb8-338e5bb7b8a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9374cee1-f7fb-03a2-2b0b-c3a69d22343b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("966b4a96-2eb3-5460-2714-aae81b49465f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98f62942-8ec3-1862-f134-6cf46723655b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a0e3ef5-bbe8-c872-ad04-77208f74bbac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9af8f4df-bbce-fe91-634a-d6a013604197"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c574008-128a-9bf4-a055-13697eefbb93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f8f0047-8a7c-db10-e0e5-ebf700fa47e5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2254fbd-8540-9b79-3b14-427aba720006"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7fdd2d9-fd87-5970-f912-314438cd8ae9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b21d00f0-e6ac-0cc1-cc81-7eedd631dcda"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b85220d6-eb1f-f65c-0549-2cc171cae932"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bffad5a3-ae8b-237c-d9c2-102afc62a0c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0efb2c9-d9cb-f88d-5771-dae59a5cc617"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2073572-67a8-6929-8913-d87672d3b3ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c485cdcb-a72f-e65e-dcf0-081649bfa1ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c598c497-473c-93ec-0ac6-a20be7918b72"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cc3870ac-104c-49e4-e40d-ef8c85f38af0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d008b5b0-7595-a29f-9cf9-19a214988e4d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d06a7fa9-124b-a4a5-a47a-e70eb74eeab8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d07c2efb-9b03-7d3d-adba-f66426f4c0c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d1970d56-03c6-46ec-aad0-fb7034b4c13e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d2b0830e-d2ed-9e32-ec2c-d81942647d73"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d45fcf59-6198-13cc-1962-4df6c356b9dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d59eaa11-d0b8-238b-ff49-9ec23de9389b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d651bece-29d9-3cf5-0be1-443eefcb462f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d86f484a-4a46-6ebc-bb1a-63bc188663f8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db4a6104-5e3b-5410-c4ed-29390ae2aabb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd05bec7-4fcd-6d78-58e4-259c17bc0018"));

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
                keyValue: new Guid("e578855c-1e0f-0d35-05f8-b74185534636"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee66f33a-25fc-f1a1-591a-e601c2910591"));

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
                keyValue: new Guid("f0fb6378-fa35-2388-1599-e2031a182022"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f2a96fd3-7748-de55-a631-d1631801442d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fac0b0e6-4155-8db6-6c9e-05de1e5a335f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb393a9c-4886-a9f9-0e2b-69034ea2fe20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ff2b6273-9c51-af34-be96-2627870ec1aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("005afad8-c1bc-24c5-e94c-d7f38ac2ecfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("01fb4be4-c6ac-bb5b-0e57-58b9563e7de1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("089564f3-2b91-b254-3745-8116aeef59dc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("112a86e8-66c9-4a81-3496-49ce2a5360f6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("18bd135b-d31c-b651-a588-29f349aeef05"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1f59b3b9-2f0e-2790-69b1-10fcd306b258"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21aca3f3-82e3-592b-81b3-6245694cad25"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("342ff198-443d-dabe-16b0-4b1b96685fa0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("344efc4d-554c-4f6d-3b07-6f93b6171921"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("50f26485-a173-102e-1d66-878f59f53c42"));

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
                keyValue: new Guid("62d78811-d09f-1739-d022-2abea4b7f972"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6401d2a5-bdf5-8730-42c8-371bd61a753b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9600b728-83c3-2261-cb2d-16062a980dc8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a87245ac-fd89-63d5-b495-2f640cf53e70"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("af558e39-9fce-83ab-348e-22bf0ea37faa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b08ef8eb-f4a2-c4dd-7c76-352e06aac283"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b26762bb-2b76-6156-fbed-21e028bd794f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b3767b85-7582-e8fd-ca77-2c53feebf507"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cb6da22f-ee8d-af12-f5b0-56c339b8d9a2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d4480892-540b-1557-b068-b1fa0157b882"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dcfc4b11-786f-e75b-7712-128fa2b7fe96"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("efe80ddb-d2ca-9c15-02ce-92b7761c57cf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f445d2bf-ac6e-a357-1a80-e0fca57a2876"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"));

            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                table: "Projects");
        }
    }
}
