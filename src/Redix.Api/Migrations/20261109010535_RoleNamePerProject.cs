using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoleNamePerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectRoleNames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoleNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRoleNames_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoleNames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "", "users.roleName" },
                    { new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "", "users.invalidRoleName" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2e095970-ebf6-f721-2dd4-83933473db8e"), "en", new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "Role name" },
                    { new Guid("63cf4e6e-f5b2-ef02-c4a8-706e6c9d8e27"), "ar", new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "المسمى الوظيفي 50 حرفاً على الأكثر." },
                    { new Guid("9af83acd-dd3b-1a26-b4a3-c342a99bd1da"), "en", new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "A role name is at most 50 characters." },
                    { new Guid("e291a276-e0d0-09ac-3edb-7ce78116064e"), "ar", new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "المسمى الوظيفي" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoleNames_ProjectId_UserId",
                table: "ProjectRoleNames",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoleNames_UserId",
                table: "ProjectRoleNames",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectRoleNames");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2e095970-ebf6-f721-2dd4-83933473db8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63cf4e6e-f5b2-ef02-c4a8-706e6c9d8e27"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9af83acd-dd3b-1a26-b4a3-c342a99bd1da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e291a276-e0d0-09ac-3edb-7ce78116064e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e2a52a90-73ad-0e59-a413-69652745704f"));
        }
    }
}
