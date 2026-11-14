using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EmailAndCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("148f90b2-75cf-925a-961a-a2899215aded"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LK_EmailCodePurpose",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_EmailCodePurpose", x => x.Id);
                    table.UniqueConstraint("AK_LK_EmailCodePurpose_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "EmailCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<byte>(type: "tinyint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CodeHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tries = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailCodes_LK_EmailCodePurpose_Purpose",
                        column: x => x.Purpose,
                        principalTable: "LK_EmailCodePurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LK_EmailCodePurpose",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "ResetPassword" },
                    { (byte)1, "ConfirmEmail" }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0078755e-fe71-0cc1-fba0-e0fe08f1a9a4"), "", "auth.refused" },
                    { new Guid("0f1273e8-0259-c356-6f3a-6b413f32d9cc"), "", "email.resetSubject" },
                    { new Guid("177c1464-42ab-28f5-1eb4-3739c67b210a"), "", "users.emailInvalid" },
                    { new Guid("21ceede3-21ae-2854-40bf-95c045923481"), "", "auth.weakPassword" },
                    { new Guid("2afc343e-ea58-f111-eb20-b8e92d2865f2"), "", "auth.resetWithCode" },
                    { new Guid("35f920df-3d53-15e4-b3ed-b71a1b5a8a3d"), "", "users.emailTaken" },
                    { new Guid("3944a672-7bf7-9b01-e851-8ea46c4486e6"), "", "email.confirmBody" },
                    { new Guid("395a3ec8-b7f4-8c32-9c97-9d81eae053c1"), "", "auth.codeSent" },
                    { new Guid("425d9bf8-853a-5248-d188-a39b1b97ab99"), "", "auth.forgotPassword" },
                    { new Guid("484320d9-3953-c816-e0af-e3b208e1f76e"), "", "users.email" },
                    { new Guid("78ac9645-a011-3e87-805b-443d02a19fe5"), "", "auth.noConnection" },
                    { new Guid("8202733f-efc7-45a8-74ec-cbf2f1fb819c"), "", "auth.tooManyRequests" },
                    { new Guid("848b21cd-1175-4299-3a3f-d8b374502dda"), "", "email.confirmSubject" },
                    { new Guid("88aa2bf0-44f2-a75d-905a-b1e7b345459a"), "", "settings.emailConfirm" },
                    { new Guid("aa92d498-c7fa-b241-be88-3e74b000c4f7"), "", "auth.loginOrEmail" },
                    { new Guid("aa92f3cb-09b1-2692-071c-bb47bfcd05b0"), "", "auth.codeWrong" },
                    { new Guid("b0f7932e-e269-c2de-e2b3-ab00e3913d37"), "", "auth.sendCode" },
                    { new Guid("c767f935-2770-6049-5b0d-7902749cebe1"), "", "email.resetBody" },
                    { new Guid("e3d786ea-b1a9-dfb1-ddc6-66d90ee58b1d"), "", "auth.code" },
                    { new Guid("eafbb093-26f7-a4cd-40ec-e5ae600af9fa"), "", "auth.passwordReset" },
                    { new Guid("f7a2170a-dcb9-a05e-4eb7-3552a4b74a9a"), "", "settings.myEmail" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00b06e02-5c71-1d59-9561-20fc9d43b8fd"), "en", new Guid("8202733f-efc7-45a8-74ec-cbf2f1fb819c"), "Too many requests. Try again shortly." },
                    { new Guid("048ade37-2b59-e8d9-e0c1-f47a3b93af95"), "ar", new Guid("2afc343e-ea58-f111-eb20-b8e92d2865f2"), "تغيير كلمة المرور" },
                    { new Guid("04bb39d4-a6d8-ce0c-b8c7-324d759a2e97"), "en", new Guid("aa92d498-c7fa-b241-be88-3e74b000c4f7"), "Username or email" },
                    { new Guid("11ff82aa-8b73-4862-e485-e9b07a2dc02a"), "ar", new Guid("eafbb093-26f7-a4cd-40ec-e5ae600af9fa"), "تغيّرت كلمة المرور. سجّل الدخول." },
                    { new Guid("27f8ea12-d060-e555-be7c-038dd5585d46"), "ar", new Guid("848b21cd-1175-4299-3a3f-d8b374502dda"), "تأكيد بريدك في REDIX" },
                    { new Guid("2ff6931b-efef-53da-c33c-3bc54e2bb3a6"), "en", new Guid("484320d9-3953-c816-e0af-e3b208e1f76e"), "Email" },
                    { new Guid("30cc6ec5-25ed-5d74-d829-10cb41600706"), "ar", new Guid("e3d786ea-b1a9-dfb1-ddc6-66d90ee58b1d"), "الرمز" },
                    { new Guid("3194820d-6a10-2d7b-3e4a-389c57642c79"), "en", new Guid("21ceede3-21ae-2854-40bf-95c045923481"), "Use at least 8 characters with a letter and a digit, not the user name." },
                    { new Guid("41e0bf6a-430c-b9f4-d872-5a81f2d6c0a8"), "ar", new Guid("f7a2170a-dcb9-a05e-4eb7-3552a4b74a9a"), "بريدي" },
                    { new Guid("445b1a14-a4cf-f3df-fa1e-4776352df3c6"), "ar", new Guid("aa92f3cb-09b1-2692-071c-bb47bfcd05b0"), "الرمز غير صحيح أو انتهت صلاحيته." },
                    { new Guid("50755f85-5c37-4e47-eb76-1af73e9a829e"), "en", new Guid("aa92f3cb-09b1-2692-071c-bb47bfcd05b0"), "The code is wrong or has expired." },
                    { new Guid("56846382-06df-9b95-d2a8-3ba862a798e4"), "en", new Guid("395a3ec8-b7f4-8c32-9c97-9d81eae053c1"), "If the account has an email, a code valid for 15 minutes was sent to it." },
                    { new Guid("5bd6dd59-a029-880b-3093-0e0137c19315"), "en", new Guid("88aa2bf0-44f2-a75d-905a-b1e7b345459a"), "Enter the code sent to {0}." },
                    { new Guid("6891e2a1-e603-7737-b8da-0f3c7a3dc52f"), "ar", new Guid("3944a672-7bf7-9b01-e851-8ea46c4486e6"), "رمز تأكيد بريدك: {0}. صالح 15 دقيقة." },
                    { new Guid("7333d02a-7218-38db-08ce-92fdde6b8f19"), "en", new Guid("0f1273e8-0259-c356-6f3a-6b413f32d9cc"), "Your password reset code" },
                    { new Guid("761b5acc-0567-38c6-062a-b06999fc0338"), "ar", new Guid("b0f7932e-e269-c2de-e2b3-ab00e3913d37"), "أرسل الرمز" },
                    { new Guid("77db85f5-7288-2b37-c075-b58669bc2edd"), "ar", new Guid("8202733f-efc7-45a8-74ec-cbf2f1fb819c"), "طلبات كثيرة. حاول بعد قليل." },
                    { new Guid("841020b5-846b-97d0-4ecd-1a1db5737a64"), "ar", new Guid("c767f935-2770-6049-5b0d-7902749cebe1"), "رمزك في REDIX: {0}. صالح 15 دقيقة، لمرة واحدة. إن لم تطلبه فتجاهل هذه الرسالة." },
                    { new Guid("92c5e7a8-34de-9ec6-7ee5-b77a3d67c962"), "en", new Guid("848b21cd-1175-4299-3a3f-d8b374502dda"), "Confirm your REDIX email" },
                    { new Guid("9dd34245-bfdc-8df7-fa0b-56c01abb9ec5"), "ar", new Guid("0078755e-fe71-0cc1-fba0-e0fe08f1a9a4"), "رفض الخادم هذا التغيير." },
                    { new Guid("9e25d13d-0af5-9ae8-addb-6293411fce4b"), "ar", new Guid("78ac9645-a011-3e87-805b-443d02a19fe5"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("9f28cbfa-e84d-a12d-2644-14de82e0a513"), "ar", new Guid("88aa2bf0-44f2-a75d-905a-b1e7b345459a"), "أدخل الرمز المرسل إلى {0}." },
                    { new Guid("add30c97-8a23-8d69-d2da-28196e8e5074"), "ar", new Guid("35f920df-3d53-15e4-b3ed-b71a1b5a8a3d"), "هذا البريد لحساب آخر." },
                    { new Guid("b0f8d620-2910-fc1b-7645-ed4fc5a267f7"), "en", new Guid("c767f935-2770-6049-5b0d-7902749cebe1"), "Your REDIX code: {0}. Valid for 15 minutes, once. If you did not ask for it, ignore this email." },
                    { new Guid("ba97eb9f-bf02-3d3b-4193-aef9129001a3"), "en", new Guid("177c1464-42ab-28f5-1eb4-3739c67b210a"), "The email is not valid." },
                    { new Guid("bd3984a5-d275-b4d6-61f2-a982c210e029"), "en", new Guid("35f920df-3d53-15e4-b3ed-b71a1b5a8a3d"), "This email belongs to another account." },
                    { new Guid("bfacfea8-119e-7b5e-8159-4b8cb68aec00"), "en", new Guid("e3d786ea-b1a9-dfb1-ddc6-66d90ee58b1d"), "Code" },
                    { new Guid("c00523c4-3975-cb52-9dbb-36802628b34d"), "ar", new Guid("395a3ec8-b7f4-8c32-9c97-9d81eae053c1"), "إن كان للحساب بريد، أرسلنا إليه رمزًا صالحًا 15 دقيقة." },
                    { new Guid("c1e2b98a-9efc-e432-3294-943a37a17f52"), "ar", new Guid("425d9bf8-853a-5248-d188-a39b1b97ab99"), "نسيت كلمة المرور؟" },
                    { new Guid("c5261c71-bd03-8fe2-ebd7-52662399054f"), "en", new Guid("425d9bf8-853a-5248-d188-a39b1b97ab99"), "Forgot your password?" },
                    { new Guid("c698616f-077d-f7ed-7857-a9c13e9b523a"), "en", new Guid("f7a2170a-dcb9-a05e-4eb7-3552a4b74a9a"), "My email" },
                    { new Guid("d44c06e9-31bc-b39d-bff2-47b3bc9af26e"), "en", new Guid("b0f7932e-e269-c2de-e2b3-ab00e3913d37"), "Send the code" },
                    { new Guid("d84f7965-ee0b-b26f-6d80-ed0cebd738eb"), "ar", new Guid("177c1464-42ab-28f5-1eb4-3739c67b210a"), "البريد غير صحيح." },
                    { new Guid("df068e77-6ab8-61dc-5be6-c150f5470603"), "ar", new Guid("0f1273e8-0259-c356-6f3a-6b413f32d9cc"), "رمز إعادة تعيين كلمة المرور" },
                    { new Guid("e306a9a9-8b9f-ad9e-4630-1d9e3678cfbb"), "ar", new Guid("21ceede3-21ae-2854-40bf-95c045923481"), "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم." },
                    { new Guid("e497b79c-073e-c89e-8425-b2555955b393"), "en", new Guid("2afc343e-ea58-f111-eb20-b8e92d2865f2"), "Change the password" },
                    { new Guid("e53ef0cb-75d4-3aae-7d24-25784aa18f97"), "en", new Guid("3944a672-7bf7-9b01-e851-8ea46c4486e6"), "Your email confirmation code: {0}. Valid for 15 minutes." },
                    { new Guid("e6461fbe-c74d-cb25-a4b8-16bdf3c34941"), "ar", new Guid("484320d9-3953-c816-e0af-e3b208e1f76e"), "البريد" },
                    { new Guid("e9fa02e2-cf06-8410-b240-e97e0e103379"), "en", new Guid("78ac9645-a011-3e87-805b-443d02a19fe5"), "No connection to the server. Try again." },
                    { new Guid("eb26ca0a-7736-faf7-33f3-da22f2eec877"), "en", new Guid("0078755e-fe71-0cc1-fba0-e0fe08f1a9a4"), "The server refused this change." },
                    { new Guid("eedf895b-d4a6-0314-bed7-ee30ec33eb80"), "en", new Guid("eafbb093-26f7-a4cd-40ec-e5ae600af9fa"), "The password was changed. Sign in." },
                    { new Guid("fd5df400-dbce-8276-4cb0-d4c4a28c2753"), "ar", new Guid("aa92d498-c7fa-b241-be88-3e74b000c4f7"), "اسم المستخدم أو البريد" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_EmailCodes_Purpose",
                table: "EmailCodes",
                column: "Purpose");

            migrationBuilder.CreateIndex(
                name: "IX_EmailCodes_UserId_Purpose",
                table: "EmailCodes",
                columns: new[] { "UserId", "Purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailCodes");

            migrationBuilder.DropTable(
                name: "LK_EmailCodePurpose");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00b06e02-5c71-1d59-9561-20fc9d43b8fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("048ade37-2b59-e8d9-e0c1-f47a3b93af95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("04bb39d4-a6d8-ce0c-b8c7-324d759a2e97"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("11ff82aa-8b73-4862-e485-e9b07a2dc02a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27f8ea12-d060-e555-be7c-038dd5585d46"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ff6931b-efef-53da-c33c-3bc54e2bb3a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("30cc6ec5-25ed-5d74-d829-10cb41600706"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3194820d-6a10-2d7b-3e4a-389c57642c79"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41e0bf6a-430c-b9f4-d872-5a81f2d6c0a8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("445b1a14-a4cf-f3df-fa1e-4776352df3c6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50755f85-5c37-4e47-eb76-1af73e9a829e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("56846382-06df-9b95-d2a8-3ba862a798e4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5bd6dd59-a029-880b-3093-0e0137c19315"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6891e2a1-e603-7737-b8da-0f3c7a3dc52f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7333d02a-7218-38db-08ce-92fdde6b8f19"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("761b5acc-0567-38c6-062a-b06999fc0338"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("77db85f5-7288-2b37-c075-b58669bc2edd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("841020b5-846b-97d0-4ecd-1a1db5737a64"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92c5e7a8-34de-9ec6-7ee5-b77a3d67c962"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9dd34245-bfdc-8df7-fa0b-56c01abb9ec5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9e25d13d-0af5-9ae8-addb-6293411fce4b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f28cbfa-e84d-a12d-2644-14de82e0a513"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("add30c97-8a23-8d69-d2da-28196e8e5074"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b0f8d620-2910-fc1b-7645-ed4fc5a267f7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ba97eb9f-bf02-3d3b-4193-aef9129001a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd3984a5-d275-b4d6-61f2-a982c210e029"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bfacfea8-119e-7b5e-8159-4b8cb68aec00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c00523c4-3975-cb52-9dbb-36802628b34d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c1e2b98a-9efc-e432-3294-943a37a17f52"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c5261c71-bd03-8fe2-ebd7-52662399054f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c698616f-077d-f7ed-7857-a9c13e9b523a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d44c06e9-31bc-b39d-bff2-47b3bc9af26e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d84f7965-ee0b-b26f-6d80-ed0cebd738eb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df068e77-6ab8-61dc-5be6-c150f5470603"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e306a9a9-8b9f-ad9e-4630-1d9e3678cfbb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e497b79c-073e-c89e-8425-b2555955b393"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e53ef0cb-75d4-3aae-7d24-25784aa18f97"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6461fbe-c74d-cb25-a4b8-16bdf3c34941"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9fa02e2-cf06-8410-b240-e97e0e103379"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eb26ca0a-7736-faf7-33f3-da22f2eec877"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("eedf895b-d4a6-0314-bed7-ee30ec33eb80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fd5df400-dbce-8276-4cb0-d4c4a28c2753"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0078755e-fe71-0cc1-fba0-e0fe08f1a9a4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0f1273e8-0259-c356-6f3a-6b413f32d9cc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("177c1464-42ab-28f5-1eb4-3739c67b210a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("21ceede3-21ae-2854-40bf-95c045923481"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2afc343e-ea58-f111-eb20-b8e92d2865f2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("35f920df-3d53-15e4-b3ed-b71a1b5a8a3d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3944a672-7bf7-9b01-e851-8ea46c4486e6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("395a3ec8-b7f4-8c32-9c97-9d81eae053c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("425d9bf8-853a-5248-d188-a39b1b97ab99"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("484320d9-3953-c816-e0af-e3b208e1f76e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("78ac9645-a011-3e87-805b-443d02a19fe5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8202733f-efc7-45a8-74ec-cbf2f1fb819c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("848b21cd-1175-4299-3a3f-d8b374502dda"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("88aa2bf0-44f2-a75d-905a-b1e7b345459a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aa92d498-c7fa-b241-be88-3e74b000c4f7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aa92f3cb-09b1-2692-071c-bb47bfcd05b0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b0f7932e-e269-c2de-e2b3-ab00e3913d37"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c767f935-2770-6049-5b0d-7902749cebe1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e3d786ea-b1a9-dfb1-ddc6-66d90ee58b1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eafbb093-26f7-a4cd-40ec-e5ae600af9fa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f7a2170a-dcb9-a05e-4eb7-3552a4b74a9a"));

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[] { new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "", "signIn.forgotPassword" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("148f90b2-75cf-925a-961a-a2899215aded"), "ar", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "نسيت كلمة المرور؟ تواصل مع مدير مشروعك." },
                    { new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"), "en", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "Forgot your password? Contact your project manager." }
                });
        }
    }
}
