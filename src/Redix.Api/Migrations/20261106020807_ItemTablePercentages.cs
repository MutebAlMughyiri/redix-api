using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ItemTablePercentages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowsTablePercentages",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("04af0eb6-f27d-1f55-1999-a027e57d9ccb"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("04f4b5bd-08cb-fe77-fb77-b0dc67a41e94"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("0cf9ec40-d505-1642-076c-34362aacc748"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("12e6ecda-c096-25f0-782e-4dbc5b1b4de3"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("180b728a-47d2-5142-5427-5033a619ea95"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("21280dd7-483e-17c2-640f-03bc3cdb8988"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("2614447c-52a2-553a-15d8-0011ddfafd1c"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("286990e1-6c68-33cf-b0b3-78059b23b697"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("2ac089f0-6d06-7441-4b4e-874fe1543f43"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("2e248f9f-9064-45a0-334f-c53e38d678fb"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("314438d3-36aa-8eaa-328a-cc9beb5767b6"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("673de086-8cee-5596-1b93-acb0acfef09f"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("6ad84f04-d71e-3e7f-8479-cedd8b6f49d9"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("6da2f643-7b61-6067-d461-5397f6c43410"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("6e86db2d-d5ec-b6fa-53cc-7cdde0c14b44"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("6f303ab6-00bc-ae69-5f2a-8331b61bc6f0"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("748258b8-99e2-f7a3-a9b2-06052a9b95f7"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("7b8220e1-c3e9-06c2-3615-53b8032e7262"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("8435b564-68b8-4b1e-5176-750bb0b72b3d"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("84820fed-9506-6e9d-9308-53c9841a97e9"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("9c89740e-854b-393e-45d5-e649ad5de00c"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("9fca4a36-6136-161d-07bb-cea835e1d6f7"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("ae991b59-acb1-7420-d09b-e6895207aac0"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("aebccc22-25aa-9838-9062-e867eccb8b8e"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("cf854e63-cad5-1c83-90a2-c6164c86b6c4"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("db415f43-3b91-e7b2-4041-3330e8c49b76"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("dcfd5629-5844-9427-bb79-53273463077a"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("eb1a681e-7168-48f9-5fc5-c1d1351d58db"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("f0085a42-9276-ee42-6602-5b1e32f49e10"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("f55f1253-ff90-54ed-3d8a-833d0f5a06e1"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.UpdateData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("f64be08a-09b1-c732-261f-c4bb8f2d37f5"),
                column: "ShowsTablePercentages",
                value: false);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "", "categories.tablePercentages" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("55a55129-cf99-6c5e-9c96-0492a445c900"), "en", new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "Calculate a percentage for each table" },
                    { new Guid("c4f98685-f8b8-3f24-613b-cee24f2327f6"), "ar", new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "حساب النسبة لكل جدول" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55a55129-cf99-6c5e-9c96-0492a445c900"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c4f98685-f8b8-3f24-613b-cee24f2327f6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("be2b4779-1157-7818-3138-61de471fdb61"));

            migrationBuilder.DropColumn(
                name: "ShowsTablePercentages",
                table: "Items");
        }
    }
}
