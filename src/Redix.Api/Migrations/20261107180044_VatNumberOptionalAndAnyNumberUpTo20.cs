using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class VatNumberOptionalAndAnyNumberUpTo20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48699ff2-feb6-f8a3-65b5-89452c221355"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60383cf9-62ec-5f53-1e2b-6cff79449eb3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"));

            migrationBuilder.AlterColumn<string>(
                name: "VatNumber",
                table: "Suppliers",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldUnicode: false,
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "BuyerVatNumber",
                table: "SalesInvoice",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldUnicode: false,
                oldMaxLength: 15);

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"),
                column: "Value",
                value: "الرقم الضريبي رقم من 20 خانة على الأكثر.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bf7c0c2-aeb6-3336-fb8b-00349b3b63c4"),
                column: "Value",
                value: "A VAT number is a number of at most 20 digits.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52bc5dd2-764d-ebd8-dbee-37a32bd0a6e2"),
                column: "Value",
                value: "VAT number (optional)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"),
                column: "Value",
                value: "A VAT number is a number of at most 20 digits.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89cf7cc3-dd8e-19b2-b9dd-eaccbf453565"),
                column: "Value",
                value: "الرقم الضريبي (اختياري)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8f89f3c-cc3f-066c-eb00-2997b444efaf"),
                column: "Value",
                value: "الرقم الضريبي رقم من 20 خانة على الأكثر.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VatNumber",
                table: "Suppliers",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "BuyerVatNumber",
                table: "SalesInvoice",
                type: "varchar(15)",
                unicode: false,
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "", "suppliers.notVatRegistered" });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0fec8f51-75ac-58d3-d747-d202f8d013e1"),
                column: "Value",
                value: "الرقم الضريبي 15 خانة، يبدأ وينتهي بالرقم 3.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bf7c0c2-aeb6-3336-fb8b-00349b3b63c4"),
                column: "Value",
                value: "A VAT number is 15 digits, starting and ending with 3.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("52bc5dd2-764d-ebd8-dbee-37a32bd0a6e2"),
                column: "Value",
                value: "VAT number (15 digits)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8251d222-f1df-c0f8-8570-a68bda92db04"),
                column: "Value",
                value: "The VAT number is 15 digits, starting and ending with 3.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89cf7cc3-dd8e-19b2-b9dd-eaccbf453565"),
                column: "Value",
                value: "الرقم الضريبي (15 رقماً)");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8f89f3c-cc3f-066c-eb00-2997b444efaf"),
                column: "Value",
                value: "الرقم الضريبي 15 رقماً، يبدأ بالرقم 3 وينتهي به.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("48699ff2-feb6-f8a3-65b5-89452c221355"), "ar", new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "غير مسجل بالضريبة" },
                    { new Guid("60383cf9-62ec-5f53-1e2b-6cff79449eb3"), "en", new Guid("f2b72244-b8ee-dde4-7e71-c447b7d73775"), "Not VAT-registered" }
                });
        }
    }
}
