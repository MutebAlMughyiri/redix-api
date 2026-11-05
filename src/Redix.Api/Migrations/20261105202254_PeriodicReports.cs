using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PeriodicReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodicReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpensesTotal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Snapshot = table.Column<string>(type: "json", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodicReports_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "", "reports.fileFailed" },
                    { new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "", "reports.lastIssued" },
                    { new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "", "reports.issueQuestion" },
                    { new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "", "reports.print" },
                    { new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "", "reports.pdf" },
                    { new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "", "reports.issued" },
                    { new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "", "reports.liquidation" },
                    { new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "", "reports.share" },
                    { new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "", "reports.excel" },
                    { new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "", "reports.current" },
                    { new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "", "reports.reportNumber" },
                    { new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "", "reports.noneIssued" },
                    { new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "", "reports.periodic" },
                    { new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "", "reports.invoiceImages" },
                    { new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "", "reports.previous" },
                    { new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "", "reports.issueExplain" },
                    { new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "", "reports.difference" },
                    { new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "", "reports.earlier" },
                    { new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "", "reports.issue" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("012f16b1-576f-a02e-7f1d-c85144b13a98"), "ar", new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "الفرق" },
                    { new Guid("03ca8285-ff93-b8f9-88a5-47b271085b20"), "ar", new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "يشمل المعتمد الآن ولا يتغير بعد إصداره. والقيود المنتظرة للاعتماد ({1}) تظهر سطراً واحداً." },
                    { new Guid("097d4f60-34e6-98b0-7ac5-f8bf4dc4094f"), "ar", new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "PDF" },
                    { new Guid("0b75c2d4-2ab2-a2d7-2f1a-c21e97c487ca"), "ar", new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "تقرير رقم" },
                    { new Guid("0ce7c2a1-49ab-2f75-f509-822ff45276a7"), "ar", new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "إصدار رقم {0}" },
                    { new Guid("0f2caf8d-cff5-5aa7-7185-1f611aa4a91c"), "ar", new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "مشاركة" },
                    { new Guid("132c066e-906d-1ba2-5901-4128c31482c4"), "en", new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "Periodic report" },
                    { new Guid("1dd5a3ec-3250-4148-240c-4229f730a11d"), "ar", new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "صور الفواتير" },
                    { new Guid("1e3330ab-ea34-a807-bbff-1974fcb9139f"), "en", new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "Issue report number {0}?" },
                    { new Guid("3bd0132e-5742-36d8-8b81-e60612cc0b9d"), "en", new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "Invoice images" },
                    { new Guid("4a0a5a42-b0a2-d246-1638-baebdcff664e"), "ar", new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "التصفية" },
                    { new Guid("557e9a49-f05b-f047-e5c3-394a09b66b44"), "en", new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "Report issued." },
                    { new Guid("589761c4-52c2-a344-d24f-82285a13b785"), "en", new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "Previous report's expenses" },
                    { new Guid("6b4ef07d-b414-eca8-8b5d-07c364aa01df"), "en", new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "Last issued" },
                    { new Guid("71632ad2-1d92-9230-0086-1a3ab8ae3500"), "ar", new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "إصدار التقرير رقم {0}؟" },
                    { new Guid("726046d8-680b-7fad-96fc-7266129b4766"), "en", new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "Print" },
                    { new Guid("75032098-1f4b-e633-77fa-5227d31d59e7"), "en", new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "Excel" },
                    { new Guid("768b19cc-aea5-4e1a-dc73-518a39d7fa4e"), "ar", new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"), "آخر إصدار" },
                    { new Guid("78b2e4f0-8a07-7c6c-ee1c-b2ba51436fe4"), "en", new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "PDF" },
                    { new Guid("868ad98d-7996-0d9a-0dd6-55d62e1cc411"), "ar", new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "Excel" },
                    { new Guid("92175432-7671-3975-1c42-4aeb395b7604"), "ar", new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "التقرير الدوري" },
                    { new Guid("93f949e4-ae8a-dc67-08b1-ec6d7c87d1fa"), "ar", new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "مصروفات التقرير السابق" },
                    { new Guid("a0d95ad8-fbd9-e3c8-f8d9-a7ffe20e8b65"), "ar", new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "طباعة" },
                    { new Guid("a956fb7c-c040-bacf-adf3-4973c480a159"), "en", new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "Issue number {0}" },
                    { new Guid("af3bbf36-2c99-551f-16b7-e385f0b53fe7"), "en", new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "Report number" },
                    { new Guid("b1e43600-0b8a-ad08-cdf4-e936acb3290e"), "ar", new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "التقارير السابقة" },
                    { new Guid("b8408a74-7d35-32f5-aa53-b270071b5410"), "en", new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"), "Earlier reports" },
                    { new Guid("ba95605f-ef31-ca7b-f253-710466eafe8f"), "ar", new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "صدر التقرير." },
                    { new Guid("c859bdac-e662-373f-60ee-7ba8a6d733c7"), "en", new Guid("36387219-5110-e631-5aed-f9636ed99d65"), "Liquidation" },
                    { new Guid("cd21c1fa-d9c1-2d32-9a24-8f480a5fa245"), "en", new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "This report's expenses" },
                    { new Guid("d0b17e79-fb7c-c582-6fa4-bae0246f619c"), "ar", new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "لم يُصدر تقرير بعد." },
                    { new Guid("d5323b40-30fd-f6e5-aac2-6a6eafe2c2cd"), "en", new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "It holds what is approved now and never changes. {1} entries waiting for approval show as one line." },
                    { new Guid("db223d83-c6f8-65a2-2f8b-00a82101b117"), "en", new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "Difference" },
                    { new Guid("e182625f-6fe9-334e-6634-9375d0227bc1"), "en", new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "No report issued yet." },
                    { new Guid("e6db5caa-81d1-c6fa-1e33-3d8850e1ff59"), "ar", new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "تعذر جلب الملف. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f4fb997a-ef7b-2a82-55fb-a3989ba202d4"), "en", new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "Share" },
                    { new Guid("f8ba640a-51f5-1d85-a4ac-5a2f52d2573c"), "en", new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "Could not get the file. Check the connection and try again." },
                    { new Guid("faae8e7c-b2bd-8cc7-487b-9060311107eb"), "ar", new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "مصروفات التقرير الحالي" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicReports_IssuedByUserId",
                table: "PeriodicReports",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicReports_ProjectId_Number",
                table: "PeriodicReports",
                columns: new[] { "ProjectId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodicReports");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("012f16b1-576f-a02e-7f1d-c85144b13a98"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03ca8285-ff93-b8f9-88a5-47b271085b20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("097d4f60-34e6-98b0-7ac5-f8bf4dc4094f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0b75c2d4-2ab2-a2d7-2f1a-c21e97c487ca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ce7c2a1-49ab-2f75-f509-822ff45276a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f2caf8d-cff5-5aa7-7185-1f611aa4a91c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("132c066e-906d-1ba2-5901-4128c31482c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1dd5a3ec-3250-4148-240c-4229f730a11d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1e3330ab-ea34-a807-bbff-1974fcb9139f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3bd0132e-5742-36d8-8b81-e60612cc0b9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a0a5a42-b0a2-d246-1638-baebdcff664e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("557e9a49-f05b-f047-e5c3-394a09b66b44"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("589761c4-52c2-a344-d24f-82285a13b785"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6b4ef07d-b414-eca8-8b5d-07c364aa01df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("71632ad2-1d92-9230-0086-1a3ab8ae3500"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("726046d8-680b-7fad-96fc-7266129b4766"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("75032098-1f4b-e633-77fa-5227d31d59e7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("768b19cc-aea5-4e1a-dc73-518a39d7fa4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("78b2e4f0-8a07-7c6c-ee1c-b2ba51436fe4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("868ad98d-7996-0d9a-0dd6-55d62e1cc411"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92175432-7671-3975-1c42-4aeb395b7604"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93f949e4-ae8a-dc67-08b1-ec6d7c87d1fa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0d95ad8-fbd9-e3c8-f8d9-a7ffe20e8b65"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a956fb7c-c040-bacf-adf3-4973c480a159"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af3bbf36-2c99-551f-16b7-e385f0b53fe7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1e43600-0b8a-ad08-cdf4-e936acb3290e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8408a74-7d35-32f5-aa53-b270071b5410"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba95605f-ef31-ca7b-f253-710466eafe8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c859bdac-e662-373f-60ee-7ba8a6d733c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cd21c1fa-d9c1-2d32-9a24-8f480a5fa245"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0b17e79-fb7c-c582-6fa4-bae0246f619c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5323b40-30fd-f6e5-aac2-6a6eafe2c2cd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db223d83-c6f8-65a2-2f8b-00a82101b117"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e182625f-6fe9-334e-6634-9375d0227bc1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6db5caa-81d1-c6fa-1e33-3d8850e1ff59"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4fb997a-ef7b-2a82-55fb-a3989ba202d4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8ba640a-51f5-1d85-a4ac-5a2f52d2573c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("faae8e7c-b2bd-8cc7-487b-9060311107eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("03efddcb-5de9-7c2a-ea10-94d0bc58c8ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("36387219-5110-e631-5aed-f9636ed99d65"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3c09dceb-ebff-699f-f371-c74219300a75"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bc871a93-5f71-0ee8-40f4-33ed3a5934d4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("be76222d-384e-52df-0251-68ae7904f8d9"));
        }
    }
}
