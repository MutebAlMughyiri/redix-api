using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class LandCostItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "SortOrder", "Type" },
                values: new object[] { new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"), new Guid("c4aece2a-cb9d-fb1d-ae30-dce8baba5959"), true, "تكلفة الأرض", 0, (byte)2 });

            migrationBuilder.InsertData(
                table: "Subgroups",
                columns: new[] { "Id", "ItemId", "Name", "SortOrder" },
                values: new object[] { new Guid("c2db4d48-a4a5-e23f-d04c-09564fe6b494"), new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"), "", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subgroups",
                keyColumn: "Id",
                keyValue: new Guid("c2db4d48-a4a5-e23f-d04c-09564fe6b494"));

            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "Id",
                keyValue: new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"));
        }
    }
}
