using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class InactivePerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InactiveMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InactiveMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InactiveMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InactiveMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InactiveMembers_ProjectId_UserId",
                table: "InactiveMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InactiveMembers_UserId",
                table: "InactiveMembers",
                column: "UserId");

            // A login disabled until now becomes inactive in every project he holds a permission in (redix-api #126), before the column goes.
            migrationBuilder.Sql("""
                INSERT INTO InactiveMembers (Id, ProjectId, UserId)
                SELECT NEWID(), held.ProjectId, held.UserId
                FROM (SELECT DISTINCT ProjectId, UserId FROM ProjectPermissions) held
                JOIN Users ON Users.Id = held.UserId
                WHERE Users.IsActive = 0;
                """);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Someone inactive in every project he is in becomes a disabled login again.
            migrationBuilder.Sql("""
                UPDATE Users SET IsActive = 0
                WHERE EXISTS (SELECT 1 FROM InactiveMembers inactive WHERE inactive.UserId = Users.Id)
                    AND NOT EXISTS (SELECT 1 FROM ProjectPermissions held WHERE held.UserId = Users.Id
                        AND NOT EXISTS (SELECT 1 FROM InactiveMembers inactive WHERE inactive.UserId = held.UserId AND inactive.ProjectId = held.ProjectId));
                """);

            migrationBuilder.DropTable(
                name: "InactiveMembers");
        }
    }
}
