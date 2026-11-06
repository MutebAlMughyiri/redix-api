using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class OneReversalPerEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EntryLinks_OneReversal",
                table: "EntryLinks",
                column: "LinkedEntryId",
                unique: true,
                filter: "[Type] = 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntryLinks_OneReversal",
                table: "EntryLinks");
        }
    }
}
