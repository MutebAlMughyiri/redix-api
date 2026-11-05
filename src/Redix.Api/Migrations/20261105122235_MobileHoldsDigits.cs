using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class MobileHoldsDigits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09091696-ccfc-2c4b-2fb6-30a990acc27b"),
                column: "Value",
                value: "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36d8c20b-c689-f2dd-ab27-3f928773bb2f"),
                column: "Value",
                value: "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6316b8e7-2f05-0e2b-341e-98ae3e9e2a5b"),
                column: "Value",
                value: "Enter a mobile in digits, up to 20, with a + at the start if it has one.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d24f07e9-da5b-373f-d492-871258154c78"),
                column: "Value",
                value: "Enter a mobile in digits, up to 20, with a + at the start if it has one.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc93e950-2e06-1e46-381c-3782e8f8084c"),
                column: "Value",
                value: "Enter a mobile in digits, up to 20, with a + at the start if it has one.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f07e3552-07f3-0f88-e991-31daabc44809"),
                column: "Value",
                value: "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09091696-ccfc-2c4b-2fb6-30a990acc27b"),
                column: "Value",
                value: "اكتب رقم جوال من 20 حرفاً على الأكثر.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36d8c20b-c689-f2dd-ab27-3f928773bb2f"),
                column: "Value",
                value: "اكتب رقم جوال من 20 حرفاً على الأكثر.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6316b8e7-2f05-0e2b-341e-98ae3e9e2a5b"),
                column: "Value",
                value: "Enter a mobile of up to 20 characters.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d24f07e9-da5b-373f-d492-871258154c78"),
                column: "Value",
                value: "Enter a mobile of up to 20 characters.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc93e950-2e06-1e46-381c-3782e8f8084c"),
                column: "Value",
                value: "Enter a mobile of up to 20 characters.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f07e3552-07f3-0f88-e991-31daabc44809"),
                column: "Value",
                value: "اكتب رقم جوال من 20 حرفاً على الأكثر.");
        }
    }
}
