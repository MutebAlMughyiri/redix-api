using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class IssueExplainTwoWaitingLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03ca8285-ff93-b8f9-88a5-47b271085b20"),
                column: "Value",
                value: "يشمل المعتمد الآن ولا يتغير بعد إصداره. والقيود المنتظرة للاعتماد ({1}) تظهر منفصلة: المصروفات في سطر، والإيرادات ورأس المال في سطر.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5323b40-30fd-f6e5-aac2-6a6eafe2c2cd"),
                column: "Value",
                value: "It holds what is approved now and never changes. Entries waiting for approval ({1}) show apart: expenses on one line, revenue and capital on another.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03ca8285-ff93-b8f9-88a5-47b271085b20"),
                column: "Value",
                value: "يشمل المعتمد الآن ولا يتغير بعد إصداره. والقيود المنتظرة للاعتماد ({1}) تظهر سطراً واحداً.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d5323b40-30fd-f6e5-aac2-6a6eafe2c2cd"),
                column: "Value",
                value: "It holds what is approved now and never changes. {1} entries waiting for approval show as one line.");
        }
    }
}
