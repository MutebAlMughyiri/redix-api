using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PartnerPocketsGo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // redix-api #99: no expense is paid from a partner's pocket since redix-api #96 phase 2, and no pocket holds an entry or an opening balance, so each pocket and its account go.
            // A pocket an entry still named would stop this here, loudly, rather than lose the entry.
            migrationBuilder.Sql("SELECT AccountId INTO #Pockets FROM PartnerPockets; DELETE FROM PartnerPockets; DELETE FROM Accounts WHERE Id IN (SELECT AccountId FROM #Pockets); DROP TABLE #Pockets;");

            migrationBuilder.DropTable(
                name: "PartnerPockets");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ee041aa-9dc1-8d41-3e06-a22ff68ebaf6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20ae5177-1128-f50d-dec6-edf0138c926a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2de57f71-b125-f830-cce1-d9acfe45a9ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3fdeabd3-789a-f9da-6d6a-07c4249b8bb7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("447a8b30-8528-f56c-4696-db39d6f11178"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e485542-fdb9-ffb2-62a8-034488d54cb9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("90f43dca-e1ec-8d3c-8414-d54bb088b575"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98a581ea-5c05-c066-de09-cea8bcf61958"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The pockets and their accounts are not made again: they held nothing.
            migrationBuilder.CreateTable(
                name: "PartnerPockets",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerPockets", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_PartnerPockets_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerPockets_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "", "reconciliations.owed" },
                    { new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "", "accounts.typePartnerPocket" },
                    { new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "", "accounts.invalidType" },
                    { new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "", "accounts.pocketFollowsPartner" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1ee041aa-9dc1-8d41-3e06-a22ff68ebaf6"), "ar", new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "حساب الشريك يتبع الشريك." },
                    { new Guid("20ae5177-1128-f50d-dec6-edf0138c926a"), "ar", new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "من شريك" },
                    { new Guid("2de57f71-b125-f830-cce1-d9acfe45a9ea"), "en", new Guid("79264bd1-93d2-1f9e-0e14-4e4cdb1a0410"), "From a partner" },
                    { new Guid("3fdeabd3-789a-f9da-6d6a-07c4249b8bb7"), "en", new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "This kind of account comes with its partner." },
                    { new Guid("447a8b30-8528-f56c-4696-db39d6f11178"), "en", new Guid("e72bd3ff-914a-fef7-a59d-dbdbb9bbb42c"), "A partner's pocket follows the partner." },
                    { new Guid("8e485542-fdb9-ffb2-62a8-034488d54cb9"), "ar", new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "مستحق للشركاء" },
                    { new Guid("90f43dca-e1ec-8d3c-8414-d54bb088b575"), "ar", new Guid("d641f5c0-dfb4-5bc6-1d54-406312602368"), "هذا النوع من الحسابات يأتي مع شريكه." },
                    { new Guid("98a581ea-5c05-c066-de09-cea8bcf61958"), "en", new Guid("5d720efc-9e93-5c44-e1f3-eeddac22505f"), "Owed to partners" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerPockets_PartnerId",
                table: "PartnerPockets",
                column: "PartnerId",
                unique: true);
        }
    }
}
