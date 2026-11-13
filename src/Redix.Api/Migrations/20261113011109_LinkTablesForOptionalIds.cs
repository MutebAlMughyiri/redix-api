using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkTablesForOptionalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationEntries",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationEntries", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_NotificationEntries_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationEntries_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerUsers",
                columns: table => new
                {
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerUsers", x => x.PartnerId);
                    table.ForeignKey(
                        name: "FK_PartnerUsers_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStructureSources",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StructureSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStructureSources", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_ProjectStructureSources_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectStructureSources_Projects_StructureSourceId",
                        column: x => x.StructureSourceId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // An id that names a row becomes a row of its link table (findings row 214); an empty one, or one naming nothing, none. A deleted partner keeps no login.
            migrationBuilder.Sql("""
                INSERT INTO PartnerUsers (PartnerId, ProjectId, UserId)
                SELECT partner.Id, partner.ProjectId, partner.UserId FROM Partners partner
                WHERE partner.IsDeleted = 0 AND EXISTS (SELECT 1 FROM Users WHERE Users.Id = partner.UserId);
                INSERT INTO NotificationEntries (NotificationId, EntryId)
                SELECT notification.Id, notification.EntryId FROM Notifications notification
                WHERE EXISTS (SELECT 1 FROM Entries WHERE Entries.Id = notification.EntryId);
                INSERT INTO ProjectStructureSources (ProjectId, StructureSourceId)
                SELECT project.Id, project.StructureSourceId FROM Projects project
                WHERE EXISTS (SELECT 1 FROM Projects source WHERE source.Id = project.StructureSourceId);
                """);

            migrationBuilder.DropIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "StructureSourceId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "EntryId",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId",
                table: "Partners",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationEntries_EntryId",
                table: "NotificationEntries",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerUsers_ProjectId_UserId",
                table: "PartnerUsers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerUsers_UserId",
                table: "PartnerUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStructureSources_StructureSourceId",
                table: "ProjectStructureSources",
                column: "StructureSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partners_ProjectId",
                table: "Partners");

            migrationBuilder.AddColumn<Guid>(
                name: "StructureSourceId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Partners",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE Partners SET UserId = login.UserId FROM Partners JOIN PartnerUsers login ON login.PartnerId = Partners.Id;
                UPDATE Notifications SET EntryId = link.EntryId FROM Notifications JOIN NotificationEntries link ON link.NotificationId = Notifications.Id;
                UPDATE Projects SET StructureSourceId = source.StructureSourceId FROM Projects JOIN ProjectStructureSources source ON source.ProjectId = Projects.Id;
                """);

            migrationBuilder.DropTable(
                name: "NotificationEntries");

            migrationBuilder.DropTable(
                name: "PartnerUsers");

            migrationBuilder.DropTable(
                name: "ProjectStructureSources");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "StructureSourceId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] <> '00000000-0000-0000-0000-000000000000' AND [IsDeleted] = 0");
        }
    }
}
