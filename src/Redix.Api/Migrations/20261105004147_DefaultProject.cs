using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class DefaultProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12724a61-4b5a-0647-1228-94eefe4524bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d9a0a5c-7a6b-9337-316e-473c1e550569"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8d67b336-f618-fc31-452a-987821e6b3c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9b969ef2-cce0-4909-b584-4e169218bfc0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("27b76d7a-c620-546c-ce3a-fb1a42f47534"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"));

            migrationBuilder.AddColumn<bool>(
                name: "OpenDefaultProject",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserDefaultProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefaultProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDefaultProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDefaultProjects_Users_UserId",
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
                    { new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "", "projects.openDefault" },
                    { new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "", "settings.switchProject" },
                    { new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "", "projects.defaultProject" },
                    { new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "", "projects.choose" },
                    { new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "", "projects.openDefaultHint" },
                    { new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "", "projects.chooseHint" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01fa9773-222c-dea8-68a7-92b0d7ea490b"), "ar", new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "لن تظهر هذه الشاشة عند الدخول" },
                    { new Guid("0f4cbc18-8dbb-5cd1-e75e-dbf2be7030a2"), "en", new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "Choose a project" },
                    { new Guid("1b204491-bfd5-d807-4748-9e68b7189da5"), "en", new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "Open the default project directly" },
                    { new Guid("29d8b4a3-086b-f8b1-3147-7ed493115514"), "en", new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "Switch project" },
                    { new Guid("42c1a391-380a-2b1d-abb2-6b5dd5285825"), "ar", new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"), "فتح المشروع الافتراضي مباشرة" },
                    { new Guid("8167b690-1b8a-2eba-bd1f-d6fe763feb0c"), "en", new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "Default project" },
                    { new Guid("89edeeb2-a4dc-d538-6e16-1f03541fac4f"), "ar", new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "تظهر المشاريع المصرّح لك بها فقط" },
                    { new Guid("8c9ae49b-64f3-b76e-753a-d982f4845c44"), "en", new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"), "This screen will not show at sign-in." },
                    { new Guid("9a719b5e-fc79-3ab2-64af-caaaef29e274"), "ar", new Guid("52258464-29b6-3faa-92f6-fb62564c9634"), "تبديل المشروع" },
                    { new Guid("b217d252-53b9-2e21-82d3-7624fbd1ad9a"), "en", new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "Only the projects you have access to show." },
                    { new Guid("c70e93d6-7e24-5ec2-a36a-6e21d3803a2e"), "ar", new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "المشروع الافتراضي" },
                    { new Guid("fe638376-2f4f-1dbe-b53a-7cca9772baef"), "ar", new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "اختر المشروع" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultProjects_ProjectId",
                table: "UserDefaultProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultProjects_UserId",
                table: "UserDefaultProjects",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDefaultProjects");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01fa9773-222c-dea8-68a7-92b0d7ea490b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f4cbc18-8dbb-5cd1-e75e-dbf2be7030a2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b204491-bfd5-d807-4748-9e68b7189da5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("29d8b4a3-086b-f8b1-3147-7ed493115514"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42c1a391-380a-2b1d-abb2-6b5dd5285825"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8167b690-1b8a-2eba-bd1f-d6fe763feb0c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("89edeeb2-a4dc-d538-6e16-1f03541fac4f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c9ae49b-64f3-b76e-753a-d982f4845c44"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a719b5e-fc79-3ab2-64af-caaaef29e274"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b217d252-53b9-2e21-82d3-7624fbd1ad9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c70e93d6-7e24-5ec2-a36a-6e21d3803a2e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fe638376-2f4f-1dbe-b53a-7cca9772baef"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41f08b4f-d262-8f6b-05cb-3a7b21532454"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52258464-29b6-3faa-92f6-fb62564c9634"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("be963f70-31fd-15f5-e72e-8e15162a53e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"));

            migrationBuilder.DropColumn(
                name: "OpenDefaultProject",
                table: "Users");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("27b76d7a-c620-546c-ce3a-fb1a42f47534"), "", "app.subtitle" },
                    { new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "", "projects.title" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("12724a61-4b5a-0647-1228-94eefe4524bf"), "en", new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "Projects" },
                    { new Guid("1d9a0a5c-7a6b-9337-316e-473c1e550569"), "ar", new Guid("27b76d7a-c620-546c-ce3a-fb1a42f47534"), "مصروفات المشاريع" },
                    { new Guid("8d67b336-f618-fc31-452a-987821e6b3c7"), "en", new Guid("27b76d7a-c620-546c-ce3a-fb1a42f47534"), "Project expenses" },
                    { new Guid("9b969ef2-cce0-4909-b584-4e169218bfc0"), "ar", new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "المشاريع" }
                });
        }
    }
}
