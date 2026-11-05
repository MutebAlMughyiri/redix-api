using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ContractsAndSharedUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    IsOpenValue = table.Column<bool>(type: "bit", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "", "contracts.unknownSupplier" },
                    { new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "", "contracts.title" },
                    { new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "", "contracts.invalidNotes" },
                    { new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "", "contracts.itemNotOffered" },
                    { new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "", "contracts.overrun" },
                    { new Guid("3d391363-227c-a3d1-1197-343359825878"), "", "contracts.enable" },
                    { new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "", "contracts.invalidValue" },
                    { new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "", "contracts.invalidUnitPrice" },
                    { new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "", "contracts.filterAll" },
                    { new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "", "contracts.openValue" },
                    { new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "", "contracts.new" },
                    { new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "", "contracts.filterOpenValue" },
                    { new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "", "contracts.edit" },
                    { new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "", "contracts.invalidQuantity" },
                    { new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "", "contracts.item" },
                    { new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "", "contracts.notes" },
                    { new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "", "contracts.disabled" },
                    { new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "", "contracts.filterOverrun" },
                    { new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "", "contracts.unknownUnit" },
                    { new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "", "contracts.open" },
                    { new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "", "contracts.contractor" },
                    { new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "", "contracts.projectFinished" },
                    { new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "", "contracts.contractValue" },
                    { new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "", "contracts.running" },
                    { new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "", "contracts.value" },
                    { new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "", "contracts.notStarted" },
                    { new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "", "contracts.remaining" },
                    { new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "", "contracts.empty" },
                    { new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "", "contracts.unit" },
                    { new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "", "contracts.quantity" },
                    { new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "", "contracts.disable" },
                    { new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "", "contracts.unitPrice" },
                    { new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "", "contracts.noConnection" },
                    { new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "", "contracts.paid" },
                    { new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "", "contracts.save" },
                    { new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "", "contracts.loadFailed" },
                    { new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "", "contracts.refused" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("08006225-d160-6d0c-4a8e-8f3e2f17108f"), "en", new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "Overrun" },
                    { new Guid("0974e52e-f3f7-3334-2e71-541773bcb02d"), "en", new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "Amend contract" },
                    { new Guid("0b331b74-2be5-1a3a-3047-8bc54608314e"), "en", new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "Choose a contractor from the suppliers." },
                    { new Guid("0ba613ef-d4ae-2ad6-aee9-079960838311"), "en", new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "Could not read the contracts. Check the network and try again." },
                    { new Guid("0c168d51-1606-135d-f1de-ea85ce1e9720"), "ar", new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "الكل" },
                    { new Guid("0facd6a8-fa10-5475-6c5d-76b601fa1c82"), "ar", new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "العقود" },
                    { new Guid("152c3616-4f8d-9b31-29bb-80bbf2516298"), "en", new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "Open-value contract" },
                    { new Guid("1b616b32-bb3d-9703-a449-e093e8e844f4"), "ar", new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "إضافة عقد" },
                    { new Guid("209d2417-44f5-d683-e394-752c3d511abe"), "ar", new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "عقد مفتوح القيمة" },
                    { new Guid("20a25d80-5e0e-ed88-74f3-701c588ef8e2"), "ar", new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("2154434c-62c9-9f9c-e09c-aeca936ac2a6"), "ar", new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "جارٍ" },
                    { new Guid("22755869-6663-f268-d51f-497c0788873e"), "ar", new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "الملاحظات 1000 حرف على الأكثر." },
                    { new Guid("23723111-6d1f-43ef-6e30-3d194bd9b0eb"), "en", new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "Enter a contract value above zero, with at most two decimals." },
                    { new Guid("252dd856-9447-ec03-bcaa-cb6bbfcfa9f9"), "en", new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "Add contract" },
                    { new Guid("2f4ae0b3-2478-0c99-50c8-a6afa3e85cda"), "en", new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "No contracts." },
                    { new Guid("3654c337-1a43-1e43-80b7-617e072188ae"), "ar", new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "تجاوز العقد" },
                    { new Guid("3b92cf4f-4399-afdb-e1c1-fd65582728b7"), "ar", new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "تعذّرت قراءة العقود. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("4f3c3c31-8d21-d6dd-34a8-de6f99ccb374"), "ar", new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "أدخل الكمية بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("5127f1d4-aaf0-0c02-5306-6efa98c1f692"), "ar", new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "أدخل قيمة عقد أكبر من صفر، بخانتين عشريتين على الأكثر." },
                    { new Guid("540d4f62-b9f6-1734-fab6-c4d279d128c7"), "en", new Guid("3d391363-227c-a3d1-1197-343359825878"), "Enable" },
                    { new Guid("542f775e-6703-e590-e114-497279f476fe"), "ar", new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "تعطيل" },
                    { new Guid("55f26691-8729-b53a-ed7f-1c7a5ad7f141"), "en", new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "Disabled" },
                    { new Guid("580da880-4154-0edc-b4c0-2915a596e72b"), "en", new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "This project is finished, so nothing in it changes." },
                    { new Guid("5a8e491f-ac5f-d72d-7306-ebbc65ea4fb4"), "ar", new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "القيمة" },
                    { new Guid("5d5d2855-6b1e-47ed-76e3-2a7f0007d6e4"), "en", new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "Enter a quantity in numbers, with at most two decimals." },
                    { new Guid("60a70f8c-d012-a361-030e-cbb86aaa43b1"), "en", new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "Item" },
                    { new Guid("60f953f1-891d-e1c6-c956-8d24450e1d04"), "ar", new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "مفتوح" },
                    { new Guid("61e0d155-53d8-c8bf-5289-99820f68b691"), "ar", new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "متجاوز" },
                    { new Guid("6251cae4-8958-0c60-a083-f58fd5709b15"), "en", new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "Enter a unit price in numbers, with at most two decimals." },
                    { new Guid("6960ad34-64e9-5004-bbb8-1c929d4d6fd8"), "en", new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "Paid" },
                    { new Guid("6b561eae-1f5b-b5d4-609c-5f2136247149"), "en", new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "Contract overrun" },
                    { new Guid("6c2ca628-066c-4303-9242-78e65f36e8e3"), "ar", new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("71de789d-4059-45ea-db83-2c5cc9128034"), "ar", new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "لا توجد عقود." },
                    { new Guid("783c3df5-020d-dbe4-b741-e43bad28a1ad"), "ar", new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "الوحدة" },
                    { new Guid("8118e655-cf60-98c7-9308-bc6939298674"), "en", new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "Unit price (optional)" },
                    { new Guid("8232e654-0733-83e4-4675-ee1fcbd48707"), "ar", new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "أدخل سعر الوحدة بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("82d0d5ad-1b42-5a41-6628-6d2ec02a262f"), "ar", new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "الملاحظات والتعديلات" },
                    { new Guid("87e50819-ceb5-d8e5-fa5c-46048a550f84"), "en", new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "Notes may have at most 1000 characters." },
                    { new Guid("90a05b67-d0ec-636b-c805-679cbd892cab"), "ar", new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "اختر مقاولاً من الموردين." },
                    { new Guid("94e1bf81-f4a3-2918-c488-0d4268a32f18"), "en", new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "This change was refused." },
                    { new Guid("95dbf3fb-a165-cec6-1a24-2c2fbd1d4232"), "ar", new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "رُفض هذا التغيير." },
                    { new Guid("95ea06a1-8525-89cd-c56d-6c69f02bc1b6"), "en", new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "Contract value" },
                    { new Guid("9626b2aa-641e-c783-229a-02a934de527e"), "en", new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "Value" },
                    { new Guid("97523ac0-00fb-3869-9de8-2b4bb8ac0c55"), "ar", new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "البند" },
                    { new Guid("97d70963-922a-314a-df3f-4b06c18577a5"), "ar", new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "تعديل العقد" },
                    { new Guid("992bba4c-016d-41f8-567d-0fb671857be4"), "en", new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "No connection to the server. Try again." },
                    { new Guid("997d342f-967d-1e2a-e8b2-18ab1eb903a0"), "en", new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "Choose a unit of this project." },
                    { new Guid("9e2d6a22-9713-50b7-a89b-4fa23c3cf61c"), "en", new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "Notes and amendments" },
                    { new Guid("9f0f8df4-d525-0dbc-65ef-680f3759746e"), "ar", new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "اختر وحدة من هذا المشروع." },
                    { new Guid("a66ae456-9a16-df2c-3895-350064537fa8"), "en", new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "Quantity (optional)" },
                    { new Guid("a8c8a8ac-b5c5-7612-edec-21fd5c8477b6"), "en", new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "Contractor" },
                    { new Guid("a960ef51-2a68-48c4-95b6-13f35684e777"), "ar", new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "الكمية (اختياري)" },
                    { new Guid("abc1ee24-d071-836d-7f75-8ecdf9e633de"), "ar", new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "اختر بنداً يقبل دفعات المقاولين." },
                    { new Guid("ae1abfb5-d65e-ac30-5ded-2c07ddcc4733"), "ar", new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "حفظ" },
                    { new Guid("b5634a95-8e1e-48dc-2586-fd20e5230c7e"), "en", new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "Contracts" },
                    { new Guid("be1ead27-a400-a768-2fcf-bbcd40acc118"), "en", new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "Choose an item that takes contractor payments." },
                    { new Guid("c3df0f18-b200-87a1-b710-ccecac22ab91"), "en", new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "All" },
                    { new Guid("c43b3cc7-cb30-0b9c-3d7f-4d2e5444ce4f"), "en", new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "Disable" },
                    { new Guid("c44b7041-88bb-9204-1c8e-ef6c96c080d7"), "ar", new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "المدفوع" },
                    { new Guid("cb2158d5-9575-9aa9-a683-498ebf016a2b"), "ar", new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "قيمة العقد" },
                    { new Guid("cee4d6a4-0475-784e-a0c2-d65b8dd5b565"), "ar", new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "معطّل" },
                    { new Guid("d5050b2f-a083-f3c0-e96e-d1ac8142bbbf"), "en", new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "Remaining" },
                    { new Guid("d6be266c-60a2-6060-8a47-bd1a0cfc9d24"), "en", new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "In progress" },
                    { new Guid("d6d2eee3-e183-5d03-14b3-591ffce846e3"), "ar", new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "المتبقي" },
                    { new Guid("d958d35e-dec3-8479-9d91-34ff80e10e1b"), "ar", new Guid("3d391363-227c-a3d1-1197-343359825878"), "تفعيل" },
                    { new Guid("de955506-0678-30c5-aba0-c74616789580"), "en", new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "Open" },
                    { new Guid("e0c35421-b088-3b0e-75e6-57634ec027b8"), "ar", new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "لم يبدأ" },
                    { new Guid("e893aaaf-469c-f4ca-539c-e6aef0e71e61"), "ar", new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "سعر الوحدة (اختياري)" },
                    { new Guid("f28a3021-8b54-711a-7701-de758bf8865f"), "ar", new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "المقاول" },
                    { new Guid("f5607183-c3cd-db73-1da7-240d88805f86"), "en", new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "Not started" },
                    { new Guid("fb08cc33-c75d-3522-1170-450e5c0f761b"), "ar", new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "مفتوح القيمة" },
                    { new Guid("fb473810-2457-12d5-52e9-99469cc261d4"), "en", new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "Unit" },
                    { new Guid("fcf4ee5c-1885-92d8-9b2e-b9a19486d0ef"), "en", new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "Save" },
                    { new Guid("fd15300e-1d04-c7dc-6b3b-d47608e75824"), "en", new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "Open value" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ItemId",
                table: "Contracts",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId",
                table: "Contracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SupplierId",
                table: "Contracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_UnitId",
                table: "Contracts",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08006225-d160-6d0c-4a8e-8f3e2f17108f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0974e52e-f3f7-3334-2e71-541773bcb02d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b331b74-2be5-1a3a-3047-8bc54608314e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ba613ef-d4ae-2ad6-aee9-079960838311"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c168d51-1606-135d-f1de-ea85ce1e9720"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0facd6a8-fa10-5475-6c5d-76b601fa1c82"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("152c3616-4f8d-9b31-29bb-80bbf2516298"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b616b32-bb3d-9703-a449-e093e8e844f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("209d2417-44f5-d683-e394-752c3d511abe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20a25d80-5e0e-ed88-74f3-701c588ef8e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2154434c-62c9-9f9c-e09c-aeca936ac2a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("22755869-6663-f268-d51f-497c0788873e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("23723111-6d1f-43ef-6e30-3d194bd9b0eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("252dd856-9447-ec03-bcaa-cb6bbfcfa9f9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2f4ae0b3-2478-0c99-50c8-a6afa3e85cda"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3654c337-1a43-1e43-80b7-617e072188ae"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3b92cf4f-4399-afdb-e1c1-fd65582728b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f3c3c31-8d21-d6dd-34a8-de6f99ccb374"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5127f1d4-aaf0-0c02-5306-6efa98c1f692"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("540d4f62-b9f6-1734-fab6-c4d279d128c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("542f775e-6703-e590-e114-497279f476fe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55f26691-8729-b53a-ed7f-1c7a5ad7f141"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("580da880-4154-0edc-b4c0-2915a596e72b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a8e491f-ac5f-d72d-7306-ebbc65ea4fb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d5d2855-6b1e-47ed-76e3-2a7f0007d6e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60a70f8c-d012-a361-030e-cbb86aaa43b1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60f953f1-891d-e1c6-c956-8d24450e1d04"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61e0d155-53d8-c8bf-5289-99820f68b691"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6251cae4-8958-0c60-a083-f58fd5709b15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6960ad34-64e9-5004-bbb8-1c929d4d6fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b561eae-1f5b-b5d4-609c-5f2136247149"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6c2ca628-066c-4303-9242-78e65f36e8e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71de789d-4059-45ea-db83-2c5cc9128034"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("783c3df5-020d-dbe4-b741-e43bad28a1ad"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8118e655-cf60-98c7-9308-bc6939298674"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8232e654-0733-83e4-4675-ee1fcbd48707"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82d0d5ad-1b42-5a41-6628-6d2ec02a262f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("87e50819-ceb5-d8e5-fa5c-46048a550f84"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90a05b67-d0ec-636b-c805-679cbd892cab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94e1bf81-f4a3-2918-c488-0d4268a32f18"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95dbf3fb-a165-cec6-1a24-2c2fbd1d4232"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95ea06a1-8525-89cd-c56d-6c69f02bc1b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9626b2aa-641e-c783-229a-02a934de527e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97523ac0-00fb-3869-9de8-2b4bb8ac0c55"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97d70963-922a-314a-df3f-4b06c18577a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("992bba4c-016d-41f8-567d-0fb671857be4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("997d342f-967d-1e2a-e8b2-18ab1eb903a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e2d6a22-9713-50b7-a89b-4fa23c3cf61c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f0f8df4-d525-0dbc-65ef-680f3759746e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a66ae456-9a16-df2c-3895-350064537fa8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8c8a8ac-b5c5-7612-edec-21fd5c8477b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a960ef51-2a68-48c4-95b6-13f35684e777"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("abc1ee24-d071-836d-7f75-8ecdf9e633de"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae1abfb5-d65e-ac30-5ded-2c07ddcc4733"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b5634a95-8e1e-48dc-2586-fd20e5230c7e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be1ead27-a400-a768-2fcf-bbcd40acc118"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c3df0f18-b200-87a1-b710-ccecac22ab91"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c43b3cc7-cb30-0b9c-3d7f-4d2e5444ce4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c44b7041-88bb-9204-1c8e-ef6c96c080d7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb2158d5-9575-9aa9-a683-498ebf016a2b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cee4d6a4-0475-784e-a0c2-d65b8dd5b565"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5050b2f-a083-f3c0-e96e-d1ac8142bbbf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6be266c-60a2-6060-8a47-bd1a0cfc9d24"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6d2eee3-e183-5d03-14b3-591ffce846e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d958d35e-dec3-8479-9d91-34ff80e10e1b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("de955506-0678-30c5-aba0-c74616789580"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0c35421-b088-3b0e-75e6-57634ec027b8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e893aaaf-469c-f4ca-539c-e6aef0e71e61"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f28a3021-8b54-711a-7701-de758bf8865f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f5607183-c3cd-db73-1da7-240d88805f86"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb08cc33-c75d-3522-1170-450e5c0f761b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb473810-2457-12d5-52e9-99469cc261d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcf4ee5c-1885-92d8-9b2e-b9a19486d0ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd15300e-1d04-c7dc-6b3b-d47608e75824"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("118c1718-9436-8431-a45c-33cc878270cd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("29ec94e3-2193-25b2-910d-684e12785662"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d391363-227c-a3d1-1197-343359825878"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7be7f869-b472-557b-1492-eccce29349c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c96bdf63-848a-9489-58d4-69828fe27483"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"));

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Units");
        }
    }
}
