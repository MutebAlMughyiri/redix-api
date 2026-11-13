using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class UserDefaultProjectsKeyedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefaultProjects",
                table: "UserDefaultProjects");

            migrationBuilder.DropIndex(
                name: "IX_UserDefaultProjects_UserId",
                table: "UserDefaultProjects");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserDefaultProjects");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefaultProjects",
                table: "UserDefaultProjects",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDefaultProjects",
                table: "UserDefaultProjects");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserDefaultProjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE UserDefaultProjects SET Id = NEWID();");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDefaultProjects",
                table: "UserDefaultProjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultProjects_UserId",
                table: "UserDefaultProjects",
                column: "UserId",
                unique: true);
        }
    }
}
