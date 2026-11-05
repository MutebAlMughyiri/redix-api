using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnitSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnitShares",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitShares", x => new { x.EntryId, x.UnitId });
                    table.ForeignKey(
                        name: "FK_UnitShares_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitShares_Units_UnitId",
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
                    { new Guid("093181e1-3f51-88c3-d596-265935a828c1"), "", "expenses.splitNeedsSharedUnit" },
                    { new Guid("0df52050-b16c-725c-2f99-5ef4fc859aed"), "", "expenses.splitEqual" },
                    { new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "", "reports.surplus" },
                    { new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "", "reports.units" },
                    { new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "", "expenses.split" },
                    { new Guid("99d2c12b-b195-dbcf-6613-49181cbee1e5"), "", "expenses.invalidSplit" },
                    { new Guid("a972c947-2698-eb26-a30f-ed0d1d6d6e0f"), "", "expenses.splitTotal" },
                    { new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"), "", "expenses.splitPercent" },
                    { new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"), "", "reports.unitFromShared" },
                    { new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"), "", "reports.unsplit" },
                    { new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "", "reports.unitDirect" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"),
                column: "Value",
                value: "Waiting for your approval: {0}");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"),
                column: "Value",
                value: "بانتظار اعتمادك: {0}");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("03ef1711-7dda-1d9d-5da3-f27542b20c7a"), "ar", new Guid("99d2c12b-b195-dbcf-6613-49181cbee1e5"), "أعطِ كل وحدة نسبة أكبر من الصفر، مجموعها 100%." },
                    { new Guid("0522d40b-3c09-0198-c0c0-1f1e0c53f9df"), "ar", new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"), "نصيبها من المشترك" },
                    { new Guid("0884b375-fee8-e6c5-265e-73ea132d60a1"), "en", new Guid("99d2c12b-b195-dbcf-6613-49181cbee1e5"), "Give each unit a part above zero, together 100%." },
                    { new Guid("09885d93-5575-7bfa-d422-967cef0197e4"), "en", new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"), "Its part of the shared" },
                    { new Guid("0c16bf37-bcca-0e62-d6a7-37e6efcadb13"), "ar", new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"), "بالنسبة" },
                    { new Guid("2416b1fd-1f6c-084d-7124-a7d0d9d400e7"), "ar", new Guid("0df52050-b16c-725c-2f99-5ef4fc859aed"), "بالتساوي" },
                    { new Guid("29911313-8522-e2f1-e809-7b2f0cf7fdba"), "en", new Guid("a972c947-2698-eb26-a30f-ed0d1d6d6e0f"), "Together" },
                    { new Guid("4d093e97-07b2-aa82-392b-52b64676e11e"), "ar", new Guid("a972c947-2698-eb26-a30f-ed0d1d6d6e0f"), "المجموع" },
                    { new Guid("4d5643ba-31a5-7f4f-902d-a3f629ab61cc"), "ar", new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"), "مشترك غير موزع" },
                    { new Guid("4e97d436-e047-7d8a-fb97-6ba86e827eb7"), "en", new Guid("093181e1-3f51-88c3-d596-265935a828c1"), "Only an expense on the shared unit is split." },
                    { new Guid("5550d815-7059-892f-79d5-5d81b347dafa"), "en", new Guid("0df52050-b16c-725c-2f99-5ef4fc859aed"), "Equally" },
                    { new Guid("619f5014-ad63-3cf1-64b9-5281580d66e9"), "ar", new Guid("093181e1-3f51-88c3-d596-265935a828c1"), "لا يوزَّع إلا المصروف المسجل على المشترك." },
                    { new Guid("6c3a122a-1b5b-ea1e-cb78-4f556010bf27"), "en", new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "Split between the units" },
                    { new Guid("83ef785c-9ff0-5788-9b5a-928ea809ab67"), "en", new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"), "By percentage" },
                    { new Guid("9173b557-7307-37cc-b3df-38dffe17dfdd"), "en", new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "Recorded against it" },
                    { new Guid("9fcd217a-deb1-4746-1356-42cdb2da6483"), "ar", new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "الوحدات" },
                    { new Guid("ab1b4a87-d462-984b-3577-8dc13f929aba"), "ar", new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "الفائض" },
                    { new Guid("b8c673c5-e255-a104-1008-40364934f72d"), "ar", new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "توزيع على الوحدات" },
                    { new Guid("c66f177c-45b2-510c-7c58-7bd04c48131f"), "ar", new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "مسجل عليها" },
                    { new Guid("d8550351-2c7d-269e-e937-4a6ae40fa623"), "en", new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "Units" },
                    { new Guid("e2e2ee96-23d6-6b7b-0413-c5763ad858a0"), "en", new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"), "Shared, not split" },
                    { new Guid("fb3a7b61-8742-26eb-f8da-436cb72b4de6"), "en", new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "Surplus" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitShares_UnitId",
                table: "UnitShares",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitShares");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03ef1711-7dda-1d9d-5da3-f27542b20c7a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0522d40b-3c09-0198-c0c0-1f1e0c53f9df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0884b375-fee8-e6c5-265e-73ea132d60a1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09885d93-5575-7bfa-d422-967cef0197e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c16bf37-bcca-0e62-d6a7-37e6efcadb13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2416b1fd-1f6c-084d-7124-a7d0d9d400e7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29911313-8522-e2f1-e809-7b2f0cf7fdba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d093e97-07b2-aa82-392b-52b64676e11e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d5643ba-31a5-7f4f-902d-a3f629ab61cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e97d436-e047-7d8a-fb97-6ba86e827eb7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5550d815-7059-892f-79d5-5d81b347dafa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("619f5014-ad63-3cf1-64b9-5281580d66e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6c3a122a-1b5b-ea1e-cb78-4f556010bf27"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83ef785c-9ff0-5788-9b5a-928ea809ab67"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9173b557-7307-37cc-b3df-38dffe17dfdd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fcd217a-deb1-4746-1356-42cdb2da6483"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ab1b4a87-d462-984b-3577-8dc13f929aba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8c673c5-e255-a104-1008-40364934f72d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c66f177c-45b2-510c-7c58-7bd04c48131f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8550351-2c7d-269e-e937-4a6ae40fa623"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2e2ee96-23d6-6b7b-0413-c5763ad858a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb3a7b61-8742-26eb-f8da-436cb72b4de6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("093181e1-3f51-88c3-d596-265935a828c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0df52050-b16c-725c-2f99-5ef4fc859aed"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99d2c12b-b195-dbcf-6613-49181cbee1e5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a972c947-2698-eb26-a30f-ed0d1d6d6e0f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"));

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("24e44838-df9f-6c90-d847-6a16c9c5ea69"),
                column: "Value",
                value: "{0} entries wait for your approval");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fce2d73-d49b-33a0-6ff1-cc72e64c1a43"),
                column: "Value",
                value: "{0} مصروفات بانتظار اعتمادك");
        }
    }
}
