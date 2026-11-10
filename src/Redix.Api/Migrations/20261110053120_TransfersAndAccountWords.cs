using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class TransfersAndAccountWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1572dc11-faf7-c9d5-b7f5-38f177012ec4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d18f432-1755-ba9b-d98b-e2c9c241c151"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4829d758-fd83-2fd2-d504-0e05c79b2b87"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ece3a4c-3b6e-c7c6-4b32-1e2fcd503b38"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6c3e6672-c81a-f1c4-eeab-10b19313241b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a7ee204a-f3ec-9e89-5335-d5d52ca5b79f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"));

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.CheckConstraint("CK_Transfers_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_Transfers_TwoAccounts", "[FromAccountId] <> [ToAccountId]");
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "", "transfers.amount" },
                    { new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "", "history.accountEnabled" },
                    { new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "", "history.loadFailed" },
                    { new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "", "transfers.invalidNotes" },
                    { new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "", "history.allAccounts" },
                    { new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "", "history.accountDeleted" },
                    { new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "", "entryShare.accountShort" },
                    { new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "", "transfers.insufficientBalance" },
                    { new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "", "transfers.to" },
                    { new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "", "history.transfer" },
                    { new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "", "history.approvedBy" },
                    { new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "", "history.nameWas" },
                    { new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "", "transfers.from" },
                    { new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "", "accounts.banks" },
                    { new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "", "accounts.accountInUse" },
                    { new Guid("666082ac-4221-7539-3047-ab60621f529c"), "", "history.more" },
                    { new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "", "transfers.noConnection" },
                    { new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "", "history.entryOut" },
                    { new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "", "history.accountChanged" },
                    { new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "", "transfers.date" },
                    { new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "", "accounts.counted" },
                    { new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "", "transfers.save" },
                    { new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "", "history.accountCreated" },
                    { new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "", "history.empty" },
                    { new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "", "history.entryIn" },
                    { new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "", "transfers.accountDisabled" },
                    { new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "", "transfers.refused" },
                    { new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "", "accounts.delete" },
                    { new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "", "accounts.deleteCannotUndo" },
                    { new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "", "transfers.projectFinished" },
                    { new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "", "history.reversed" },
                    { new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "", "accounts.deleteQuestion" },
                    { new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "", "transfers.button" },
                    { new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "", "transfers.title" },
                    { new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "", "history.accountDisabled" },
                    { new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "", "history.custodianChanged" },
                    { new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "", "transfers.notes" },
                    { new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "", "transfers.invalidAmount" },
                    { new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "", "history.unknownAccount" },
                    { new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "", "accounts.custodies" },
                    { new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "", "transfers.sameAccount" },
                    { new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "", "accounts.deleted" },
                    { new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "", "history.mine" },
                    { new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "", "accounts.history" },
                    { new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "", "history.check" },
                    { new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "", "transfers.balanceNow" },
                    { new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "", "approvals.accountShort" },
                    { new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "", "transfers.unknownAccount" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("001c2473-59c6-4c79-c8bd-cd7210f93645"), "en", new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "Transfer" },
                    { new Guid("016917c7-21f1-ceb8-7da5-2e121135e908"), "ar", new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "تنبيه: رصيد الحساب لا يكفي لهذا المبلغ" },
                    { new Guid("0358ca3a-b06f-1b0d-6774-63f4d87ed9bf"), "en", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "Notes may have at most 1000 characters." },
                    { new Guid("043422b3-7647-3753-2954-63f4b1ee1393"), "en", new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "Used by entries, checks or transfers, so it can only be disabled." },
                    { new Guid("05142cdd-c151-b525-bd99-edcd858edad7"), "en", new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "Changed {0}: {1}" },
                    { new Guid("0a41375a-0274-0c0c-437a-61f50545564c"), "ar", new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "عكس: {0}" },
                    { new Guid("0c2280a7-51fc-c927-b6d1-46d24cd80569"), "en", new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "Note: the account does not hold enough for this amount." },
                    { new Guid("0f939def-ba49-d8b0-6cd9-c8374be39c6f"), "en", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "This could not be done." },
                    { new Guid("110abad0-c036-72f8-fc60-20ac37e7da13"), "en", new Guid("666082ac-4221-7539-3047-ab60621f529c"), "More" },
                    { new Guid("1162fef6-abd5-2771-b60b-369e63b8cc46"), "ar", new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "حذف الحساب؟" },
                    { new Guid("14049f43-4660-131a-8696-4c91c9c705ba"), "en", new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "What I did" },
                    { new Guid("15272de3-a481-c904-1650-6276f93f0055"), "ar", new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "مطابقة الأرصدة الفعلية: {0}" },
                    { new Guid("155550a3-e5ea-154b-8eb6-392790d4101a"), "ar", new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "هذا الحساب ليس في هذا المشروع." },
                    { new Guid("180821d0-2354-c568-12e3-f08d135d7c36"), "ar", new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "تعذّرت قراءة السجل. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("1a1944db-a39a-8ece-3821-6ade63ff1f5e"), "en", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "Transfer" },
                    { new Guid("1a94905d-b7db-9426-6022-52ecf68252da"), "en", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "Date" },
                    { new Guid("1c9de493-b47c-ff5c-5bde-caaba82c15d0"), "en", new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "Could not read the history. Check the network and try again." },
                    { new Guid("1d0c0b18-decd-79e5-9acb-c571abe35e8e"), "ar", new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "الحسابات البنكية" },
                    { new Guid("20787cab-bf3a-2686-0d90-59eb2db4f1c9"), "ar", new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "إضافة الحساب {0}" },
                    { new Guid("210ebaf7-0b22-b12b-5ad0-b31c0c2c98c7"), "ar", new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "المبلغ أكبر من رصيد الحساب الآن." },
                    { new Guid("267bc664-e781-2e21-8b08-3c4274e4220c"), "en", new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "Holds now: {0}" },
                    { new Guid("26d2f27f-1bf0-cd21-de22-193675cf876e"), "ar", new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "كل الحسابات" },
                    { new Guid("26df8882-7ec8-aaf4-881f-fbab3ac7b634"), "en", new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "Approved by {0}" },
                    { new Guid("282e2102-a165-f0e1-7b59-00b3394a0c10"), "en", new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "That account is not in this project." },
                    { new Guid("28e7c9e3-024d-b988-d0c8-26f5783ed79b"), "ar", new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "تعديل {0}: {1}" },
                    { new Guid("2c5baad7-2442-ae4f-712b-bb71b86a5407"), "en", new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "Bank accounts" },
                    { new Guid("3682633d-086d-2cf3-2391-b09233d41b8b"), "ar", new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("37bc8571-e4db-517a-46f8-63d7277fae6b"), "ar", new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "{0} أصبح مع {1}" },
                    { new Guid("38646627-4699-0b85-957e-8138cab4482a"), "en", new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "History" },
                    { new Guid("3c319db8-027f-e7a4-2605-8f25f8c22b5a"), "ar", new Guid("666082ac-4221-7539-3047-ab60621f529c"), "المزيد" },
                    { new Guid("3cef95e3-7edd-406a-526d-393293701900"), "ar", new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "الفعلي في آخر مطابقة: {0}" },
                    { new Guid("3f90be0e-f505-e27b-d744-336d177d557f"), "ar", new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "حذف" },
                    { new Guid("459b6e4d-681a-e768-1672-3a84823203b6"), "ar", new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "{0} {1} إلى {2}" },
                    { new Guid("48ecb1c6-2d74-1a48-87a5-f5a39260e539"), "ar", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "تحويل بين الحسابات" },
                    { new Guid("4a30c706-b1fe-298a-3ac6-5c0b52fc3ef3"), "ar", new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "العهد النقدية" },
                    { new Guid("4df36ad6-8f02-af10-ef8c-69d7e61a70ea"), "en", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "An account is no longer in the project." },
                    { new Guid("50a061a0-b5b3-fb25-e183-e6311159a40b"), "ar", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "إلى" },
                    { new Guid("50d20cae-4408-fdd2-39a8-47d8014b6fd8"), "en", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "Amount" },
                    { new Guid("54c9ba2e-2749-c51e-50c0-ccd1c56f7ef0"), "en", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "Type an amount above zero." },
                    { new Guid("55f602c4-4b1c-3db4-f7f9-f9f18bb77996"), "en", new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "This cannot be undone." },
                    { new Guid("574ed884-311c-943e-4016-03d72adff5db"), "en", new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "Added the account {0}" },
                    { new Guid("5d45640a-3d73-e747-8693-b34a9d3d76c6"), "ar", new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "مستخدم في قيود أو مطابقات أو تحويلات، فيمكن تعطيله فقط." },
                    { new Guid("5eca2ccd-c0ce-08aa-2f05-ef3535940ec1"), "en", new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "{0} {1} into {2}" },
                    { new Guid("5f517095-b4f4-06d1-9094-4fd7bdf60c66"), "en", new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"), "Reversed: {0}" },
                    { new Guid("601b7b56-a12b-d87b-23a5-f42b65290567"), "en", new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "{0} holds {1}; {2} waits on it, {3} short." },
                    { new Guid("60b7af07-64c6-8e07-8e6d-6a00230dd157"), "en", new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "{0} {1} out of {2}" },
                    { new Guid("6314437b-e392-fd3e-b551-ff02bfa40154"), "en", new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "Delete the account?" },
                    { new Guid("6fe229df-5870-3913-b2a4-0ad2dcb32b88"), "en", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "From" },
                    { new Guid("713aef6d-b027-7b8d-478a-4924cb333e42"), "ar", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "من" },
                    { new Guid("7726ace2-b636-acb7-1c29-43fc1a702dec"), "en", new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "Counted the actual balances: {0}" },
                    { new Guid("788bf328-781f-7ab3-9b54-6bd2908e8aa4"), "en", new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"), "All accounts" },
                    { new Guid("7d891732-73d3-f482-711d-c81e23098136"), "en", new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "Deleted {0}" },
                    { new Guid("7e4e2ae5-06ef-6405-4006-e9372858893b"), "en", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "To" },
                    { new Guid("7fefdbce-3238-63eb-de35-6573500c2c6b"), "ar", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "التاريخ" },
                    { new Guid("85783e6a-f3b4-0258-5d18-fa09d7058fa5"), "ar", new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "حذف {0}" },
                    { new Guid("86440aa5-6daf-329e-6911-46d350bc2278"), "ar", new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "السجل" },
                    { new Guid("873a71aa-5b82-2a81-2cad-9844aa232bfa"), "ar", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "اختر حسابا آخر للتحويل إليه." },
                    { new Guid("88453c8f-99fc-01bd-f0c5-2ac1e5d4f4dc"), "en", new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "the name was {0}" },
                    { new Guid("8dce01ff-e163-0770-3cef-03294fe56646"), "en", new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "Notes" },
                    { new Guid("9599a4cf-e2cc-f5ff-9adc-c329b1b4ee65"), "ar", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "تعذر تنفيذ ذلك." },
                    { new Guid("973d2b22-768d-85e8-4ff3-24e43cce082f"), "en", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "Transfer between accounts" },
                    { new Guid("97e1b99f-765a-724b-d8cf-c9ace29c9513"), "en", new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "Delete" },
                    { new Guid("9c62c0bb-c98d-6df4-8cb1-35d85bec3e34"), "en", new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "Nothing yet." },
                    { new Guid("9c745c7a-7cc4-98c8-10d9-bf768249bbbe"), "ar", new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "تعطيل {0}" },
                    { new Guid("9f49ce8e-c825-8608-5c4f-812f4c92fe07"), "ar", new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "ما قمت به" },
                    { new Guid("9f56048b-7076-42e9-b45e-773e748ed574"), "en", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "Choose another account to transfer to." },
                    { new Guid("9fb4f1ac-eba2-e933-f119-714caaf01e96"), "en", new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "Enabled {0}" },
                    { new Guid("a79038d5-81be-3a89-e011-75b4fa2e6e9e"), "ar", new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "اعتمده {0}" },
                    { new Guid("abb1ad24-6a3f-8d72-1c07-ad9ce7148799"), "ar", new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "ملاحظات" },
                    { new Guid("abd7b6ae-81cc-5edb-6b57-a33d2ab65144"), "ar", new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "لا شيء بعد." },
                    { new Guid("b1e8bccf-9c1a-cc4a-2ef0-f1d8ce0d1665"), "ar", new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "رصيده الآن: {0}" },
                    { new Guid("b557a25f-694a-e762-3979-a4a9ee538a31"), "ar", new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "تحويل {0} من {1} إلى {2}" },
                    { new Guid("b996a55c-99c7-a85e-7feb-6daec1d8a748"), "en", new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "Disabled {0}" },
                    { new Guid("be76414a-bec5-9328-1aad-667a4376acee"), "en", new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "{0} is now held by {1}" },
                    { new Guid("bf72b835-ae88-6b56-b8fb-18a69ef55559"), "ar", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "الملاحظات 1000 حرف على الأكثر." },
                    { new Guid("c8226c38-6ed8-3a89-e2a8-96da616eab76"), "en", new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "Counted at the last check: {0}" },
                    { new Guid("c84862fd-64a9-df39-5f68-c36a13b3a42f"), "ar", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("ca4b8363-fa30-36db-66c9-2c2f91126eb3"), "ar", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "تحويل" },
                    { new Guid("d0caac3a-dd13-a692-ddd3-5672d6cb6eee"), "en", new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "This project is finished, so nothing in it changes." },
                    { new Guid("d1113ba1-239d-cf5f-8ab2-aef570600dc4"), "ar", new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "تم حذف الحساب." },
                    { new Guid("d31d47f2-f844-03a9-734f-3d521cd67db5"), "ar", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "اكتب مبلغا أكبر من صفر." },
                    { new Guid("d62b6c6b-cd23-ba5c-ad5f-9bc340f92f6c"), "en", new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "Transferred {0} from {1} to {2}" },
                    { new Guid("d65613b2-a7af-4f13-ffc8-9e6d0edd23fa"), "en", new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "A disabled account takes no transfer." },
                    { new Guid("d850eb67-7433-6417-be41-71062508507e"), "en", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "No connection to the server. Try again." },
                    { new Guid("d8bbe973-07e0-f170-47ea-6f188018793a"), "ar", new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "{0} {1} من {2}" },
                    { new Guid("d8d3f074-8584-cc73-8c87-a238712d42ab"), "ar", new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "الحساب المعطل لا يقبل التحويل." },
                    { new Guid("dae75be4-a8c8-f10d-0bb6-5049c866c748"), "en", new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "Account deleted." },
                    { new Guid("db0d1ce8-769e-0f7e-e39e-0b1e8305bb9f"), "ar", new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "تفعيل {0}" },
                    { new Guid("dc3cdff0-1164-a105-a310-b60e54f4e8a4"), "ar", new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "تحويل" },
                    { new Guid("dfac1ddd-60c1-8e56-04e8-b9515077db46"), "ar", new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "لا يمكن التراجع عن الحذف." },
                    { new Guid("e1f94ecb-f861-48f4-2103-dbea7cc2dd65"), "ar", new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "رصيد {0} {1}، وينتظر عليه {2}، فينقص {3}" },
                    { new Guid("e71eea87-53cd-9cfd-ad88-009cd0a7a45a"), "ar", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "لم يعد أحد الحسابين في المشروع." },
                    { new Guid("e9b39b25-e0b4-5c6a-b958-a9e297e20c09"), "ar", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "المبلغ" },
                    { new Guid("f46516ad-db2a-ed9c-eb91-a6bc9f81838f"), "en", new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "That is more than the account holds now." },
                    { new Guid("f8eb3403-bc93-f6ba-efe3-9c482b3f8710"), "en", new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "Cash custodies" },
                    { new Guid("fe14f56a-6cba-8192-5772-1db987c46607"), "ar", new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "الاسم كان {0}" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_CreatedByUserId",
                table: "Transfers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromAccountId",
                table: "Transfers",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ProjectId",
                table: "Transfers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ToAccountId",
                table: "Transfers",
                column: "ToAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("001c2473-59c6-4c79-c8bd-cd7210f93645"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("016917c7-21f1-ceb8-7da5-2e121135e908"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0358ca3a-b06f-1b0d-6774-63f4d87ed9bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("043422b3-7647-3753-2954-63f4b1ee1393"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("05142cdd-c151-b525-bd99-edcd858edad7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0a41375a-0274-0c0c-437a-61f50545564c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c2280a7-51fc-c927-b6d1-46d24cd80569"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f939def-ba49-d8b0-6cd9-c8374be39c6f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("110abad0-c036-72f8-fc60-20ac37e7da13"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1162fef6-abd5-2771-b60b-369e63b8cc46"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("14049f43-4660-131a-8696-4c91c9c705ba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15272de3-a481-c904-1650-6276f93f0055"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("155550a3-e5ea-154b-8eb6-392790d4101a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("180821d0-2354-c568-12e3-f08d135d7c36"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a1944db-a39a-8ece-3821-6ade63ff1f5e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a94905d-b7db-9426-6022-52ecf68252da"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c9de493-b47c-ff5c-5bde-caaba82c15d0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d0c0b18-decd-79e5-9acb-c571abe35e8e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("20787cab-bf3a-2686-0d90-59eb2db4f1c9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("210ebaf7-0b22-b12b-5ad0-b31c0c2c98c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("267bc664-e781-2e21-8b08-3c4274e4220c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("26d2f27f-1bf0-cd21-de22-193675cf876e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("26df8882-7ec8-aaf4-881f-fbab3ac7b634"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("282e2102-a165-f0e1-7b59-00b3394a0c10"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28e7c9e3-024d-b988-d0c8-26f5783ed79b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2c5baad7-2442-ae4f-712b-bb71b86a5407"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3682633d-086d-2cf3-2391-b09233d41b8b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37bc8571-e4db-517a-46f8-63d7277fae6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38646627-4699-0b85-957e-8138cab4482a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3c319db8-027f-e7a4-2605-8f25f8c22b5a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3cef95e3-7edd-406a-526d-393293701900"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3f90be0e-f505-e27b-d744-336d177d557f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("459b6e4d-681a-e768-1672-3a84823203b6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48ecb1c6-2d74-1a48-87a5-f5a39260e539"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4a30c706-b1fe-298a-3ac6-5c0b52fc3ef3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4df36ad6-8f02-af10-ef8c-69d7e61a70ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50a061a0-b5b3-fb25-e183-e6311159a40b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50d20cae-4408-fdd2-39a8-47d8014b6fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54c9ba2e-2749-c51e-50c0-ccd1c56f7ef0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55f602c4-4b1c-3db4-f7f9-f9f18bb77996"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("574ed884-311c-943e-4016-03d72adff5db"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5d45640a-3d73-e747-8693-b34a9d3d76c6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5eca2ccd-c0ce-08aa-2f05-ef3535940ec1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f517095-b4f4-06d1-9094-4fd7bdf60c66"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("601b7b56-a12b-d87b-23a5-f42b65290567"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("60b7af07-64c6-8e07-8e6d-6a00230dd157"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6314437b-e392-fd3e-b551-ff02bfa40154"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6fe229df-5870-3913-b2a4-0ad2dcb32b88"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("713aef6d-b027-7b8d-478a-4924cb333e42"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7726ace2-b636-acb7-1c29-43fc1a702dec"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("788bf328-781f-7ab3-9b54-6bd2908e8aa4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d891732-73d3-f482-711d-c81e23098136"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7e4e2ae5-06ef-6405-4006-e9372858893b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7fefdbce-3238-63eb-de35-6573500c2c6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("85783e6a-f3b4-0258-5d18-fa09d7058fa5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86440aa5-6daf-329e-6911-46d350bc2278"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("873a71aa-5b82-2a81-2cad-9844aa232bfa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88453c8f-99fc-01bd-f0c5-2ac1e5d4f4dc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8dce01ff-e163-0770-3cef-03294fe56646"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9599a4cf-e2cc-f5ff-9adc-c329b1b4ee65"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("973d2b22-768d-85e8-4ff3-24e43cce082f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("97e1b99f-765a-724b-d8cf-c9ace29c9513"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c62c0bb-c98d-6df4-8cb1-35d85bec3e34"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9c745c7a-7cc4-98c8-10d9-bf768249bbbe"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f49ce8e-c825-8608-5c4f-812f4c92fe07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9f56048b-7076-42e9-b45e-773e748ed574"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fb4f1ac-eba2-e933-f119-714caaf01e96"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a79038d5-81be-3a89-e011-75b4fa2e6e9e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("abb1ad24-6a3f-8d72-1c07-ad9ce7148799"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("abd7b6ae-81cc-5edb-6b57-a33d2ab65144"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1e8bccf-9c1a-cc4a-2ef0-f1d8ce0d1665"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b557a25f-694a-e762-3979-a4a9ee538a31"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b996a55c-99c7-a85e-7feb-6daec1d8a748"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be76414a-bec5-9328-1aad-667a4376acee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf72b835-ae88-6b56-b8fb-18a69ef55559"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8226c38-6ed8-3a89-e2a8-96da616eab76"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c84862fd-64a9-df39-5f68-c36a13b3a42f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca4b8363-fa30-36db-66c9-2c2f91126eb3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0caac3a-dd13-a692-ddd3-5672d6cb6eee"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d1113ba1-239d-cf5f-8ab2-aef570600dc4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d31d47f2-f844-03a9-734f-3d521cd67db5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d62b6c6b-cd23-ba5c-ad5f-9bc340f92f6c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d65613b2-a7af-4f13-ffc8-9e6d0edd23fa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d850eb67-7433-6417-be41-71062508507e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8bbe973-07e0-f170-47ea-6f188018793a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8d3f074-8584-cc73-8c87-a238712d42ab"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dae75be4-a8c8-f10d-0bb6-5049c866c748"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("db0d1ce8-769e-0f7e-e39e-0b1e8305bb9f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc3cdff0-1164-a105-a310-b60e54f4e8a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dfac1ddd-60c1-8e56-04e8-b9515077db46"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e1f94ecb-f861-48f4-2103-dbea7cc2dd65"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e71eea87-53cd-9cfd-ad88-009cd0a7a45a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e9b39b25-e0b4-5c6a-b958-a9e297e20c09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f46516ad-db2a-ed9c-eb91-a6bc9f81838f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8eb3403-bc93-f6ba-efe3-9c482b3f8710"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fe14f56a-6cba-8192-5772-1db987c46607"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1ad78c31-0366-8535-b690-d668cfead1df"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("336ceda2-1ef4-8711-5089-ebca466b1a73"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("343d054d-7afb-5693-0444-a7954e933b1e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3e1c052e-8583-4a49-c195-97db89e57466"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49a34959-b87d-1860-f555-25771b6897e8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("666082ac-4221-7539-3047-ab60621f529c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6badde79-3784-70f8-5315-9ed699c74888"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6ff27195-8804-5460-9908-204f4bf75070"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a30144a0-067f-9738-be76-58ce171612a5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ad95b12e-1ba6-6c6c-b01c-0d8c2b0e709b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f23354f3-a95d-0349-e199-002851a61c15"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"));

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "Accounts",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "", "accounts.openingBalance" },
                    { new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "", "accounts.invalidOpeningBalance" },
                    { new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "", "accounts.accountHasEntries" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1572dc11-faf7-c9d5-b7f5-38f177012ec4"), "ar", new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "الرصيد الافتتاحي" },
                    { new Guid("1d18f432-1755-ba9b-d98b-e2c9c241c151"), "ar", new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "هذا الحساب عليه قيود، فلا يتغير رصيده الافتتاحي." },
                    { new Guid("4829d758-fd83-2fd2-d504-0e05c79b2b87"), "ar", new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "أدخل الرصيد الافتتاحي بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("5ece3a4c-3b6e-c7c6-4b32-1e2fcd503b38"), "en", new Guid("6557b35c-28ae-0339-5b20-90d6fb2482b1"), "This account has entries, so its opening balance cannot change." },
                    { new Guid("6c3e6672-c81a-f1c4-eeab-10b19313241b"), "en", new Guid("3dc858b4-fee2-1200-5223-57d9bc02c8cc"), "Enter an opening balance in numbers, with at most two decimals." },
                    { new Guid("a7ee204a-f3ec-9e89-5335-d5d52ca5b79f"), "en", new Guid("0b8adb2a-5662-9973-7dc1-6a4e6e1b2c8c"), "Opening balance" }
                });
        }
    }
}
