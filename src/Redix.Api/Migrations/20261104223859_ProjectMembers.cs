using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Users_UserId",
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
                    { new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "", "users.projectsHint" },
                    { new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "", "users.everyProject" },
                    { new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "", "users.ownerIsInEveryProject" },
                    { new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "", "users.roleFinance" },
                    { new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "", "users.roleField" },
                    { new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "", "users.projects" },
                    { new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "", "users.noProjects" },
                    { new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "", "users.roleReader" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("15d4c430-9bd7-87da-10f5-2c89fb534713"), "en", new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "Finance" },
                    { new Guid("2bf2f2ea-36c7-2a81-296b-604fd037bf81"), "ar", new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "المالك موجود في كل المشاريع." },
                    { new Guid("3d5743b9-fc24-6245-e2d3-253f8d5a85df"), "ar", new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "حدد المشروع واختر الدور. لا يرى المستخدم غير هذه المشاريع." },
                    { new Guid("44898532-cbd5-b0b4-00ca-be6ee30e7984"), "ar", new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "مشرف مالي في كل المشاريع" },
                    { new Guid("68bf8eb8-f805-3c39-a6dc-b91e072ab147"), "en", new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "Reader" },
                    { new Guid("79810273-c3f0-a7b6-089a-74cf4883a743"), "en", new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "An owner is already in every project." },
                    { new Guid("803d6961-e6f7-3611-f0b9-775459859f95"), "en", new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "Tick a project and choose the role. The user sees only these projects." },
                    { new Guid("86018635-2aad-43d7-91e2-9af58611ae3d"), "ar", new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "لا مشاريع بعد" },
                    { new Guid("91dc02b3-d294-7b9d-f490-b85bae163d15"), "ar", new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "ميداني" },
                    { new Guid("93b4a40b-4efb-7496-4ab1-0ded8b7c25ba"), "en", new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "No projects yet" },
                    { new Guid("9a402a51-ff39-2ef1-99cc-f0c53b9020c0"), "ar", new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "المشاريع والأدوار" },
                    { new Guid("a0e2e8bd-ae2d-429b-dce4-2499861ebcb4"), "en", new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "Financial supervisor in every project" },
                    { new Guid("ae5f116d-805c-ae24-0210-1e211da86d98"), "en", new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "Projects and roles" },
                    { new Guid("d84474b3-01de-e50d-54ce-a9218d6d0f5f"), "ar", new Guid("77960312-298e-1e22-7acd-02337306d3f1"), "مالي" },
                    { new Guid("ea35d520-82a3-9934-c95d-228d3191a681"), "ar", new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"), "قارئ" },
                    { new Guid("fbccd63d-445f-9f7c-3b7b-76b5fdb18e76"), "en", new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"), "Field" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId_UserId",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserId",
                table: "ProjectMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15d4c430-9bd7-87da-10f5-2c89fb534713"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bf2f2ea-36c7-2a81-296b-604fd037bf81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d5743b9-fc24-6245-e2d3-253f8d5a85df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("44898532-cbd5-b0b4-00ca-be6ee30e7984"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("68bf8eb8-f805-3c39-a6dc-b91e072ab147"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("79810273-c3f0-a7b6-089a-74cf4883a743"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("803d6961-e6f7-3611-f0b9-775459859f95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86018635-2aad-43d7-91e2-9af58611ae3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91dc02b3-d294-7b9d-f490-b85bae163d15"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93b4a40b-4efb-7496-4ab1-0ded8b7c25ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a402a51-ff39-2ef1-99cc-f0c53b9020c0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a0e2e8bd-ae2d-429b-dce4-2499861ebcb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae5f116d-805c-ae24-0210-1e211da86d98"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d84474b3-01de-e50d-54ce-a9218d6d0f5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ea35d520-82a3-9934-c95d-228d3191a681"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fbccd63d-445f-9f7c-3b7b-76b5fdb18e76"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("138d9838-1214-18c4-353a-e046867ef1c6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("77960312-298e-1e22-7acd-02337306d3f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("81ceb8c8-23db-23c3-759e-aacc9e6977ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b90f2992-b1db-a571-cd82-f1141528a015"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e8a2d7ac-0888-b318-e628-f592d98abaad"));
        }
    }
}
