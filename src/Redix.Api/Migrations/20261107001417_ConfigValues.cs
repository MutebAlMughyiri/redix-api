using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigValues", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ConfigValues",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[,]
                {
                    { new Guid("9399f72c-0f36-d3d5-fd62-b9f982d68edf"), "fileTimeoutSeconds", "120" },
                    { new Guid("961162d7-c9c8-5b82-c642-4c5a6cdbe393"), "requestTimeoutSeconds", "10" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigValues_Key",
                table: "ConfigValues",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigValues");
        }
    }
}
