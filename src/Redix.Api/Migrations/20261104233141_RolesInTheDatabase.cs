using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RolesInTheDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_ProjectId_UserId",
                table: "ProjectMembers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1683a75e-e83b-46b6-c8f0-c2aa8454a355"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bf2f2ea-36c7-2a81-296b-604fd037bf81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("387924af-6253-0256-9ea8-c17696668154"));

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
                keyValue: new Guid("70316694-acfe-d92c-0563-160a0dac969e"));

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
                keyValue: new Guid("fc16c245-10f9-3042-8c9c-7086d476a790"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("138d9838-1214-18c4-353a-e046867ef1c6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"));

                                    migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "ProjectMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_TranslationKeys_Key",
                table: "TranslationKeys",
                column: "Key");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Scope = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.CheckConstraint("CK_Roles_Scope", "[Scope] IN ('System', 'Project')");
                    table.ForeignKey(
                        name: "FK_Roles_TranslationKeys_WordKey",
                        column: x => x.WordKey,
                        principalTable: "TranslationKeys",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.CheckConstraint("CK_RolePermissions_Permission", "[Permission] IN ('StartProject', 'ManageLogins', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject')");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Scope", "SortOrder", "WordKey" },
                values: new object[,]
                {
                    { new Guid("3fee0350-b8df-dbb4-9bcc-b0039085dccd"), "Project", 4, "users.roleReader" },
                    { new Guid("8a930250-60d5-b402-464a-0545e0253004"), "Project", 2, "users.roleFinance" },
                    { new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451"), "Project", 3, "users.roleField" }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "", "users.roleSystem" },
                    { new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "", "users.otherProject" },
                    { new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "", "users.invalidRole" },
                    { new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "", "users.unknownUser" },
                    { new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "", "users.addExisting" },
                    { new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "", "users.lastManager" },
                    { new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "", "users.rolesHint" },
                    { new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "", "users.roleManager" },
                    { new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "", "users.roles" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0721816f-e32a-820f-bb96-5c63fd7521c7"),
                column: "Value",
                value: "لا مشاريع بعد.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("148f90b2-75cf-925a-961a-a2899215aded"),
                column: "Value",
                value: "نسيت كلمة المرور؟ تواصل مع مدير مشروعك.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"),
                column: "Value",
                value: "Forgot your password? Contact your project manager.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d34b8d94-c323-e735-45e5-b4840f194257"),
                column: "Value",
                value: "No projects yet.");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("3c35a012-dd63-53fb-9008-d3fc0de2de8c"), "ReadProject", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("6a214907-1cf8-f706-4228-eef3d5373001"), "ManageProject", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("7663b4e6-3671-1d36-3bec-3a25d84330d4"), "ReadProject", new Guid("3fee0350-b8df-dbb4-9bcc-b0039085dccd") },
                    { new Guid("7a0b2030-6690-e681-2973-197db92c573a"), "SeeAllCapital", new Guid("8a930250-60d5-b402-464a-0545e0253004") },
                    { new Guid("ef858e4c-4540-dc8b-794e-05d851b32885"), "ReadProject", new Guid("f50f9eae-0001-8d5d-c4b1-e47737eac451") }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Scope", "SortOrder", "WordKey" },
                values: new object[,]
                {
                    { new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd"), "Project", 1, "users.roleManager" },
                    { new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080"), "System", 1, "users.roleSystem" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0e1d056a-dfe5-3769-8ef4-3d6fc8536a23"), "ar", new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "يحتاج المشروع إلى مدير مشروع واحد على الأقل." },
                    { new Guid("1beea644-8d14-203f-7bfc-a76c377ad475"), "ar", new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "مسؤول النظام" },
                    { new Guid("1d40d7e9-9b93-793a-26f0-2da01ec0a8ec"), "ar", new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "إضافة مستخدم موجود" },
                    { new Guid("1d99d4ff-c3a1-f931-b2b5-eb0f28664540"), "en", new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "This person also works in another project, so the system admin does this." },
                    { new Guid("3d54d177-caa7-2cf9-b250-a7faf032fa4e"), "en", new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "System admin" },
                    { new Guid("4b06f579-83c0-eb52-d25f-2f106c47ca1d"), "en", new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "No user has this username." },
                    { new Guid("4decc0a4-2985-7259-41a7-955a02ace1b5"), "ar", new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "يعمل هذا الشخص في مشروع آخر أيضاً، لذا يتولى ذلك مسؤول النظام." },
                    { new Guid("6207a112-1d6b-b0f0-a4c9-4e0f0f04cd07"), "ar", new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "مدير المشروع" },
                    { new Guid("65355d35-f035-39ad-0ba2-8c0840226745"), "en", new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "That role cannot be given here." },
                    { new Guid("6a39a281-3d2c-31bc-3e66-f2d958f44601"), "ar", new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "حدد ما يقوم به هذا الشخص في هذا المشروع. إلغاء كل الأدوار يخرجه منه." },
                    { new Guid("6ddf9830-c72a-17ae-6149-02b6b5e4a47a"), "en", new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "A project needs at least one project manager." },
                    { new Guid("750cb2f0-6200-d700-0def-3711392692f3"), "en", new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"), "Project manager" },
                    { new Guid("82a7f452-d9bd-c965-b0db-090a461692ce"), "en", new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "Roles" },
                    { new Guid("a55a378b-9ecb-380f-5c8f-9188ea84affe"), "en", new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "Add an existing user" },
                    { new Guid("aa310a01-1201-bc09-43ea-51d727a91762"), "ar", new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"), "الأدوار" },
                    { new Guid("c6027d46-ab5d-17cd-0a22-8c9721382d00"), "en", new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"), "Tick what this person does in this project. Removing every role takes them out of it." },
                    { new Guid("d167311b-e9b1-4918-045b-fcea8aba6486"), "ar", new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"), "لا يمكن إسناد هذا الدور هنا." },
                    { new Guid("efbd0bc2-fa6c-4e87-6cfe-8878bc5284f3"), "ar", new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "لا يوجد مستخدم بهذا الاسم." }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("43bca88f-c711-0d3e-7fc4-ba5d36094bb7"), "ManageUsers", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") },
                    { new Guid("578f9580-4389-dee4-365c-a6d94e78b230"), "ManageLogins", new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080") },
                    { new Guid("5d9e61be-da59-a993-55f5-c5013028c46f"), "StartProject", new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080") },
                    { new Guid("6d013eea-b4b2-27df-4b1c-fbf617b5794a"), "SeeAllCapital", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") },
                    { new Guid("dae8d6db-6f49-9c82-c97b-bab24a781600"), "ReadProject", new Guid("e469cf77-7a5b-a1a5-425b-4a42e26c96bd") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId_UserId_RoleId",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_RoleId",
                table: "ProjectMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_Permission",
                table: "RolePermissions",
                columns: new[] { "RoleId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_WordKey",
                table: "Roles",
                column: "WordKey");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            // Each project role becomes its row. Every project needs a manager, and the owners were in every project, so they manage them all and hold the system admin's role.
            migrationBuilder.Sql("""
                UPDATE ProjectMembers SET RoleId = CASE Role
                    WHEN 0 THEN '8a930250-60d5-b402-464a-0545e0253004'
                    WHEN 1 THEN 'f50f9eae-0001-8d5d-c4b1-e47737eac451'
                    ELSE '3fee0350-b8df-dbb4-9bcc-b0039085dccd'
                END;
                INSERT INTO UserRoles (Id, UserId, RoleId)
                    SELECT NEWID(), Id, 'e5c8a57b-dca1-df52-59f4-a0520caa9080' FROM Users WHERE IsOwner = 1;
                INSERT INTO ProjectMembers (Id, ProjectId, UserId, RoleId)
                    SELECT NEWID(), Projects.Id, Users.Id, 'e469cf77-7a5b-a1a5-425b-4a42e26c96bd' FROM Projects CROSS JOIN Users WHERE Users.IsOwner = 1;
                """);

            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ProjectMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Roles_RoleId",
                table: "ProjectMembers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Roles_RoleId",
                table: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TranslationKeys_Key",
                table: "TranslationKeys");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_ProjectId_UserId_RoleId",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_RoleId",
                table: "ProjectMembers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e1d056a-dfe5-3769-8ef4-3d6fc8536a23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1beea644-8d14-203f-7bfc-a76c377ad475"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d40d7e9-9b93-793a-26f0-2da01ec0a8ec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d99d4ff-c3a1-f931-b2b5-eb0f28664540"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d54d177-caa7-2cf9-b250-a7faf032fa4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b06f579-83c0-eb52-d25f-2f106c47ca1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4decc0a4-2985-7259-41a7-955a02ace1b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6207a112-1d6b-b0f0-a4c9-4e0f0f04cd07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65355d35-f035-39ad-0ba2-8c0840226745"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a39a281-3d2c-31bc-3e66-f2d958f44601"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6ddf9830-c72a-17ae-6149-02b6b5e4a47a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("750cb2f0-6200-d700-0def-3711392692f3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("82a7f452-d9bd-c965-b0db-090a461692ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a55a378b-9ecb-380f-5c8f-9188ea84affe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa310a01-1201-bc09-43ea-51d727a91762"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6027d46-ab5d-17cd-0a22-8c9721382d00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d167311b-e9b1-4918-045b-fcea8aba6486"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efbd0bc2-fa6c-4e87-6cfe-8878bc5284f3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39457e5d-d993-e945-7e48-548736d938d2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("40570779-c6e2-c38a-39f2-f3e822f57c3f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6fa63112-1d37-fb77-60d9-ac7be94f2cfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a62c2d72-be90-19d8-a03f-098a766b52c3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e9c66f3b-3cb8-b275-e894-07f9ba54567e"));

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "Role",
                table: "ProjectMembers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "", "users.projectsHint" },
                    { new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "", "users.owner" },
                    { new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "", "users.everyProject" },
                    { new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "", "users.ownerCannotBeDisabled" },
                    { new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "", "users.ownerIsInEveryProject" },
                    { new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "", "users.projects" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0721816f-e32a-820f-bb96-5c63fd7521c7"),
                column: "Value",
                value: "لا توجد مشاريع بعد. أنشئ أول مشروع.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("148f90b2-75cf-925a-961a-a2899215aded"),
                column: "Value",
                value: "نسيت كلمة المرور؟ تواصل مع المشرف المالي.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"),
                column: "Value",
                value: "Forgot your password? Contact the financial supervisor.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d34b8d94-c323-e735-45e5-b4840f194257"),
                column: "Value",
                value: "No projects yet. Create the first one.");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1683a75e-e83b-46b6-c8f0-c2aa8454a355"), "ar", new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "مالك" },
                    { new Guid("2bf2f2ea-36c7-2a81-296b-604fd037bf81"), "ar", new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "المالك موجود في كل المشاريع." },
                    { new Guid("387924af-6253-0256-9ea8-c17696668154"), "ar", new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "لا يمكن تعطيل المالك." },
                    { new Guid("3d5743b9-fc24-6245-e2d3-253f8d5a85df"), "ar", new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "حدد المشروع واختر الدور. لا يرى المستخدم غير هذه المشاريع." },
                    { new Guid("44898532-cbd5-b0b4-00ca-be6ee30e7984"), "ar", new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "مشرف مالي في كل المشاريع" },
                    { new Guid("70316694-acfe-d92c-0563-160a0dac969e"), "en", new Guid("1cfee5a9-d209-2e9d-94ef-0cb498c59a66"), "Owner" },
                    { new Guid("79810273-c3f0-a7b6-089a-74cf4883a743"), "en", new Guid("52aecb0e-293c-99d7-e301-7c1170f69229"), "An owner is already in every project." },
                    { new Guid("803d6961-e6f7-3611-f0b9-775459859f95"), "en", new Guid("138d9838-1214-18c4-353a-e046867ef1c6"), "Tick a project and choose the role. The user sees only these projects." },
                    { new Guid("9a402a51-ff39-2ef1-99cc-f0c53b9020c0"), "ar", new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "المشاريع والأدوار" },
                    { new Guid("a0e2e8bd-ae2d-429b-dce4-2499861ebcb4"), "en", new Guid("24c9a50b-452a-095c-fd8c-109f226a3d2e"), "Financial supervisor in every project" },
                    { new Guid("ae5f116d-805c-ae24-0210-1e211da86d98"), "en", new Guid("b1c16daf-471f-19d3-bd2f-d991ff5983dd"), "Projects and roles" },
                    { new Guid("fc16c245-10f9-3042-8c9c-7086d476a790"), "en", new Guid("330531e9-4e44-a81e-24b5-2570e74d8624"), "An owner cannot be disabled." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId_UserId",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);
        }
    }
}
