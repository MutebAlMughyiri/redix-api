using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class SignIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedSignIns = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    RefreshExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "", "signIn.username" },
                    { new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "", "signIn.noConnection" },
                    { new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "", "signIn.password" },
                    { new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "", "signIn.submit" },
                    { new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "", "signIn.hint" },
                    { new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "", "signIn.invalidCredentials" },
                    { new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "", "signIn.sessionExpired" },
                    { new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "", "signIn.forgotPassword" },
                    { new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "", "signIn.accountLocked" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("02d6f999-9388-217e-daf8-563539c673e2"), "ar", new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "دخول" },
                    { new Guid("0aa2162e-3d6b-8b42-8300-a302de0fd0b7"), "ar", new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "كلمة المرور" },
                    { new Guid("148f90b2-75cf-925a-961a-a2899215aded"), "ar", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "نسيت كلمة المرور؟ تواصل مع المشرف المالي." },
                    { new Guid("15c5b681-8dd9-1ff5-0189-06a4e7aef403"), "en", new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "Sign in to follow your projects' expenses" },
                    { new Guid("18714d75-c699-01e4-11e5-d15d8de353c6"), "en", new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "The username or password is wrong." },
                    { new Guid("1ab2159c-07c7-fb5d-20a0-3db8ed883882"), "ar", new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "انتهت الجلسة. سجّل الدخول مرة أخرى." },
                    { new Guid("22391e2d-43d6-2517-ddca-a5241abfc811"), "en", new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "Sign in" },
                    { new Guid("328a90cd-7775-101d-2339-ecebd31005b2"), "en", new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "Your session ended. Sign in again." },
                    { new Guid("61aeab36-2741-caf4-712f-b9ee7ced5772"), "en", new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "Password" },
                    { new Guid("6a3837c1-fc33-e41f-c8ad-86cac17ed328"), "en", new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "Username" },
                    { new Guid("7d30e2ff-930c-063b-52cf-8bef179bc274"), "ar", new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "اسم المستخدم أو كلمة المرور غير صحيحة." },
                    { new Guid("84d6d2ba-ce62-325e-310c-430c9a86e30e"), "ar", new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("97d500dd-a11d-076d-f290-06b7dd34604b"), "ar", new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "سجّل الدخول لمتابعة مصروفات مشاريعك" },
                    { new Guid("b34ac086-f169-703d-ad3d-48bb542f5be4"), "ar", new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "اسم المستخدم" },
                    { new Guid("b6ff2b8f-e772-604e-3c3c-87062810560d"), "en", new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "No connection to the server. Try again." },
                    { new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"), "en", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "Forgot your password? Contact the financial supervisor." },
                    { new Guid("dc07afa9-eda1-de49-25de-a215d4f96d02"), "en", new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "Too many wrong tries. Try again later." },
                    { new Guid("dc7066b8-452a-d89d-5dd4-8a06c9e4c22d"), "ar", new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_RefreshTokenHash",
                table: "Devices",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("02d6f999-9388-217e-daf8-563539c673e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0aa2162e-3d6b-8b42-8300-a302de0fd0b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("148f90b2-75cf-925a-961a-a2899215aded"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c5b681-8dd9-1ff5-0189-06a4e7aef403"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18714d75-c699-01e4-11e5-d15d8de353c6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1ab2159c-07c7-fb5d-20a0-3db8ed883882"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("22391e2d-43d6-2517-ddca-a5241abfc811"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("328a90cd-7775-101d-2339-ecebd31005b2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61aeab36-2741-caf4-712f-b9ee7ced5772"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a3837c1-fc33-e41f-c8ad-86cac17ed328"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d30e2ff-930c-063b-52cf-8bef179bc274"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("84d6d2ba-ce62-325e-310c-430c9a86e30e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97d500dd-a11d-076d-f290-06b7dd34604b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b34ac086-f169-703d-ad3d-48bb542f5be4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b6ff2b8f-e772-604e-3c3c-87062810560d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc07afa9-eda1-de49-25de-a215d4f96d02"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc7066b8-452a-d89d-5dd4-8a06c9e4c22d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("13989443-8496-3325-85a0-79bccd168cfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("51c49503-a627-70d1-dfb4-89178f387607"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"));
        }
    }
}
