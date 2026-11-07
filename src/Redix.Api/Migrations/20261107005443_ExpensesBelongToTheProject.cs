using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpensesBelongToTheProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Units_UnitId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseDetails_Units_UnitId",
                table: "ExpenseDetails");

            migrationBuilder.DropTable(
                name: "UnitShares");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseDetails_UnitId",
                table: "ExpenseDetails");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_UnitId",
                table: "Contracts");

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("59042844-8ddf-6aaa-b31d-fe2c6886cb71"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9a6b3b3a-b69f-31db-e5e5-8183084d9db5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("024fc536-6032-372a-0cc7-47527306116f"));

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
                keyValue: new Guid("56c9c8cc-c8c9-038d-11d8-5c045fcb0503"));

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
                keyValue: new Guid("6cb36bdf-aabd-ddf0-ffb6-dd62b558e5dd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6efc9471-fe5d-676a-95e4-1a65063ad0eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("783c3df5-020d-dbe4-b741-e43bad28a1ad"));

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
                keyValue: new Guid("997d342f-967d-1e2a-e8b2-18ab1eb903a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f0f8df4-d525-0dbc-65ef-680f3759746e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fcd217a-deb1-4746-1356-42cdb2da6483"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a43e219c-95b9-8a61-0643-8744929da343"));

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
                keyValue: new Guid("cae4d12f-1e01-bfe4-a70c-239463019d5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8550351-2c7d-269e-e937-4a6ae40fa623"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e0c936f7-6e01-fc2c-ea48-dbdee78b66b8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e2e2ee96-23d6-6b7b-0413-c5763ad858a0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f55728f6-77e7-00eb-ae1c-b3cff8380629"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fb473810-2457-12d5-52e9-99469cc261d4"));

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
                keyValue: new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"));

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
                keyValue: new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c96bdf63-848a-9489-58d4-69828fe27483"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"));

            // The shared unit served only expenses and contracts, and every project's copy of the unit column only the expense form (redix-api 96, D8, D9).
            migrationBuilder.Sql("DELETE FROM Units WHERE IsShared = 1");
            migrationBuilder.Sql("DELETE FROM ItemColumns WHERE [Key] = N'unit'");
            migrationBuilder.Sql("DELETE FROM DefaultColumns WHERE [Key] = N'unit'");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "ExpenseDetails");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Contracts");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1140a207-69a5-9621-94a2-448e30d34e91"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 15);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c9d7893-60b7-58aa-d675-486e13466312"),
                column: "SortOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("51d1ca64-8375-50f4-f73f-92b9a50d1dcf"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 16);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("bdf1c6d1-a35d-76e6-7e49-eef4a61be2c4"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c0baa49c-72e1-f645-7695-2c2441eee40d"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c45b6129-36e7-a373-23d2-a2a5dcb2e1d2"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e494d97a-7ad8-ca92-29ff-ac42c6d13486"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 14);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08f5d8be-844d-c493-94ba-a5ffe9a7e853"),
                column: "Value",
                value: "This contract has payments, so its contractor and item cannot change.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bc91b90-0791-5b58-c656-fb2f12c46a47"),
                column: "Value",
                value: "لهذا العقد دفعات، فلا يتغير مقاوله ولا بنده.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9774b8eb-6c82-b6f0-b56b-92a58fb57467"),
                column: "Value",
                value: "It leaves this project.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0777b87-cdcc-965d-80a4-b5f3a843b845"),
                column: "Value",
                value: "تُحذف من هذا المشروع.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The units expenses and contracts were recorded against, their splits and the shared units are not brought back: only a backup taken before this migration has them.
            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "ExpenseDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1140a207-69a5-9621-94a2-448e30d34e91"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 16);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"),
                column: "SortOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c9d7893-60b7-58aa-d675-486e13466312"),
                column: "SortOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("51d1ca64-8375-50f4-f73f-92b9a50d1dcf"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 17);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 14);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("bdf1c6d1-a35d-76e6-7e49-eef4a61be2c4"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c0baa49c-72e1-f645-7695-2c2441eee40d"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c45b6129-36e7-a373-23d2-a2a5dcb2e1d2"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e494d97a-7ad8-ca92-29ff-ac42c6d13486"),
                column: "SortOrder",
                value: 9);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 14);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 15);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"),
                column: "SortOrder",
                value: 5);

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "Options", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("59042844-8ddf-6aaa-b31d-fe2c6886cb71"), (byte)4, false, false, (byte)0, "unit", "الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 },
                    { new Guid("9a6b3b3a-b69f-31db-e5e5-8183084d9db5"), (byte)4, false, false, (byte)2, "unit", "الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("093181e1-3f51-88c3-d596-265935a828c1"), "", "expenses.splitNeedsSharedUnit" },
                    { new Guid("0df52050-b16c-725c-2f99-5ef4fc859aed"), "", "expenses.splitEqual" },
                    { new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "", "reports.units" },
                    { new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "", "expenses.unknownUnit" },
                    { new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "", "projects.unitInUse" },
                    { new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "", "expenses.split" },
                    { new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "", "contracts.unknownUnit" },
                    { new Guid("99d2c12b-b195-dbcf-6613-49181cbee1e5"), "", "expenses.invalidSplit" },
                    { new Guid("a972c947-2698-eb26-a30f-ed0d1d6d6e0f"), "", "expenses.splitTotal" },
                    { new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"), "", "expenses.splitPercent" },
                    { new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "", "projects.sharedUnit" },
                    { new Guid("c5af721d-05bd-b90d-05fa-5241236275a0"), "", "reports.unitFromShared" },
                    { new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "", "contracts.unit" },
                    { new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "", "expenses.unit" },
                    { new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"), "", "reports.unsplit" },
                    { new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "", "reports.unitDirect" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08f5d8be-844d-c493-94ba-a5ffe9a7e853"),
                column: "Value",
                value: "This contract has payments, so its contractor, item and unit cannot change.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bc91b90-0791-5b58-c656-fb2f12c46a47"),
                column: "Value",
                value: "لهذا العقد دفعات، فلا يتغير مقاوله ولا بنده ولا وحدته.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9774b8eb-6c82-b6f0-b56b-92a58fb57467"),
                column: "Value",
                value: "It leaves this project. A unit that entries or contracts use cannot be deleted.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0777b87-cdcc-965d-80a4-b5f3a843b845"),
                column: "Value",
                value: "تُحذف من هذا المشروع. ولا تُحذف وحدة عليها قيود أو عقود.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("024fc536-6032-372a-0cc7-47527306116f"), "ar", new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "اختر الوحدة." },
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
                    { new Guid("56c9c8cc-c8c9-038d-11d8-5c045fcb0503"), "en", new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "Unit" },
                    { new Guid("619f5014-ad63-3cf1-64b9-5281580d66e9"), "ar", new Guid("093181e1-3f51-88c3-d596-265935a828c1"), "لا يوزَّع إلا المصروف المسجل على المشترك." },
                    { new Guid("6c3a122a-1b5b-ea1e-cb78-4f556010bf27"), "en", new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "Split between the units" },
                    { new Guid("6cb36bdf-aabd-ddf0-ffb6-dd62b558e5dd"), "en", new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "The shared unit cannot be renamed or deleted." },
                    { new Guid("6efc9471-fe5d-676a-95e4-1a65063ad0eb"), "ar", new Guid("d6d1034b-6dcd-15b5-84f6-f3214c4a2b02"), "الوحدة" },
                    { new Guid("783c3df5-020d-dbe4-b741-e43bad28a1ad"), "ar", new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "الوحدة" },
                    { new Guid("83ef785c-9ff0-5788-9b5a-928ea809ab67"), "en", new Guid("aadc5b20-cca1-e823-9000-188fb4269be1"), "By percentage" },
                    { new Guid("9173b557-7307-37cc-b3df-38dffe17dfdd"), "en", new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "Recorded against it" },
                    { new Guid("997d342f-967d-1e2a-e8b2-18ab1eb903a0"), "en", new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "Choose a unit of this project." },
                    { new Guid("9f0f8df4-d525-0dbc-65ef-680f3759746e"), "ar", new Guid("91bb55ac-5d59-91e6-e7cc-c307f336a941"), "اختر وحدة من هذا المشروع." },
                    { new Guid("9fcd217a-deb1-4746-1356-42cdb2da6483"), "ar", new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "الوحدات" },
                    { new Guid("a43e219c-95b9-8a61-0643-8744929da343"), "en", new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "Entries or contracts use this unit, so it cannot be deleted." },
                    { new Guid("b8c673c5-e255-a104-1008-40364934f72d"), "ar", new Guid("73893fbc-38ce-5ea2-b40f-9421d9439be3"), "توزيع على الوحدات" },
                    { new Guid("c66f177c-45b2-510c-7c58-7bd04c48131f"), "ar", new Guid("f6f5effc-4a2c-2758-dc54-25d329e013de"), "مسجل عليها" },
                    { new Guid("cae4d12f-1e01-bfe4-a70c-239463019d5f"), "ar", new Guid("c11644b9-5e78-0b99-05ba-004904ce9afd"), "الوحدة المشتركة لا يُعدَّل اسمها ولا تُحذف." },
                    { new Guid("d8550351-2c7d-269e-e937-4a6ae40fa623"), "en", new Guid("24c97faf-6d73-138a-41dc-e553fbb9f911"), "Units" },
                    { new Guid("e0c936f7-6e01-fc2c-ea48-dbdee78b66b8"), "ar", new Guid("46d6661d-ea85-afec-5ce0-df44c4c9d2e7"), "هذه الوحدة عليها قيود أو عقود، فلا تُحذف." },
                    { new Guid("e2e2ee96-23d6-6b7b-0413-c5763ad858a0"), "en", new Guid("edf8c6e8-f53b-4645-bad3-292aef0dd0c4"), "Shared, not split" },
                    { new Guid("f55728f6-77e7-00eb-ae1c-b3cff8380629"), "en", new Guid("3b05c94c-c35e-6e4f-acda-a0d83eafcc11"), "Choose a unit." },
                    { new Guid("fb473810-2457-12d5-52e9-99469cc261d4"), "en", new Guid("c96bdf63-848a-9489-58d4-69828fe27483"), "Unit" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_UnitId",
                table: "ExpenseDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_UnitId",
                table: "Contracts",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitShares_UnitId",
                table: "UnitShares",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Units_UnitId",
                table: "Contracts",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseDetails_Units_UnitId",
                table: "ExpenseDetails",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
