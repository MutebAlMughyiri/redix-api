using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntriesIndexCoversTheFigures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries",
                columns: new[] { "ProjectId", "Status" })
                .Annotation("SqlServer:Include", new[] { "Kind", "Amount", "AccountId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries",
                columns: new[] { "ProjectId", "Status" });
        }
    }
}
