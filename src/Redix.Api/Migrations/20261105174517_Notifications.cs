using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
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
                    { new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "", "notifications.projectRejected" },
                    { new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "", "notifications.entryApproved" },
                    { new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "", "notifications.refused" },
                    { new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "", "notifications.loadFailed" },
                    { new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "", "notifications.entrySubmitted" },
                    { new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "", "notifications.noConnection" },
                    { new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "", "notifications.empty" },
                    { new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "", "notifications.title" },
                    { new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "", "notifications.projectApproved" },
                    { new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "", "notifications.entryRejected" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("01b01836-d63d-bea2-6190-8832bbb5d9ff"), "en", new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "An entry waits for your decision" },
                    { new Guid("0aa18445-a76f-8514-98bb-30066af547f4"), "en", new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "Your project was approved" },
                    { new Guid("23f84925-6f6c-6530-6aab-1923098f37b0"), "ar", new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "قيد ينتظر قرارك" },
                    { new Guid("2679e75f-a430-fa83-92a9-3762735e6c2c"), "en", new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "This could not be done." },
                    { new Guid("2fc0c662-b7cd-be4b-b658-ad5be4f14aec"), "ar", new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "لا جديد." },
                    { new Guid("3c8b6ed6-25aa-82c8-16e8-16f7693254c7"), "ar", new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("4e98f9ce-ded1-b9ea-e26f-843d8ef1ff9e"), "en", new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "Could not read the notifications. Check the connection and try again." },
                    { new Guid("581f18a7-e7b3-01dd-6d56-85814037152d"), "ar", new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "رُفض قيدك" },
                    { new Guid("675e3729-3695-4e2f-cc2d-70263a08c6e6"), "en", new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "No connection to the server. Try again." },
                    { new Guid("7554ce76-1b7b-2135-e342-bd788a577769"), "en", new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "Notifications" },
                    { new Guid("75610cda-762f-7651-81a7-aad2c40e9b7c"), "ar", new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "تعذر تنفيذ ذلك." },
                    { new Guid("960a2bad-7be7-a210-a515-4267ad9cc0a6"), "ar", new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "لم يُعتمد مشروعك" },
                    { new Guid("ae2cb54d-7049-b5e0-ccae-a31896f5c137"), "ar", new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "اعتُمد قيدك" },
                    { new Guid("b8b9d691-a292-2c39-cc4e-06abc62eca20"), "en", new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "Your project was not approved" },
                    { new Guid("bdf1673c-d93e-d96a-0a56-5cc98a228ecd"), "en", new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "Nothing new." },
                    { new Guid("c96dd33e-153d-d72e-fa54-c3d5d4cdab01"), "en", new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "Your entry was rejected" },
                    { new Guid("ccebc95d-a5f9-0ea8-36ca-a53657efc666"), "ar", new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "اعتُمد مشروعك" },
                    { new Guid("ebe3e3ae-c8f1-bdfb-8e81-5a070fbb108a"), "ar", new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "الإشعارات" },
                    { new Guid("f42cf6f5-f78b-73c9-2157-38d242581fd8"), "ar", new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "تعذرت قراءة الإشعارات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("ffb372f1-15eb-cc43-6e5d-4b9eb0400e8f"), "en", new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "Your entry was approved" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_At",
                table: "Notifications",
                columns: new[] { "UserId", "At" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01b01836-d63d-bea2-6190-8832bbb5d9ff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0aa18445-a76f-8514-98bb-30066af547f4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("23f84925-6f6c-6530-6aab-1923098f37b0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2679e75f-a430-fa83-92a9-3762735e6c2c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2fc0c662-b7cd-be4b-b658-ad5be4f14aec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3c8b6ed6-25aa-82c8-16e8-16f7693254c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4e98f9ce-ded1-b9ea-e26f-843d8ef1ff9e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("581f18a7-e7b3-01dd-6d56-85814037152d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("675e3729-3695-4e2f-cc2d-70263a08c6e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7554ce76-1b7b-2135-e342-bd788a577769"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("75610cda-762f-7651-81a7-aad2c40e9b7c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("960a2bad-7be7-a210-a515-4267ad9cc0a6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae2cb54d-7049-b5e0-ccae-a31896f5c137"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b8b9d691-a292-2c39-cc4e-06abc62eca20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bdf1673c-d93e-d96a-0a56-5cc98a228ecd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c96dd33e-153d-d72e-fa54-c3d5d4cdab01"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ccebc95d-a5f9-0ea8-36ca-a53657efc666"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ebe3e3ae-c8f1-bdfb-8e81-5a070fbb108a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f42cf6f5-f78b-73c9-2157-38d242581fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ffb372f1-15eb-cc43-6e5d-4b9eb0400e8f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"));
        }
    }
}
