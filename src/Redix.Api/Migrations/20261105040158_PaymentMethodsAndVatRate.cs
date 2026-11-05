using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodsAndVatRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "Projects",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0.15m);

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("9f6a8063-8e6d-345c-f226-9f353471286b"), true, "شبكة", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 0 },
                    { new Guid("c1112bdf-cdf2-e0c5-efd6-9b34a0c26c18"), true, "سداد", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 3 },
                    { new Guid("f67ccb16-dde0-e041-046c-bf08645cb38a"), true, "كاش", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 2 },
                    { new Guid("f9f1ae53-17fc-c6ec-f24e-d4b87c1f4db7"), true, "تحويل", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 1 }
                });

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "VatRate",
                value: 0.15m);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("618d2584-1949-ab7c-512d-daaf8a7ad632"), "ManageSettings", new Guid("8a930250-60d5-b402-464a-0545e0253004") });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "", "paymentMethods.enable" },
                    { new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "", "paymentMethods.title" },
                    { new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "", "paymentMethods.disabled" },
                    { new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "", "paymentMethods.empty" },
                    { new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "", "paymentMethods.refused" },
                    { new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "", "paymentMethods.noConnection" },
                    { new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "", "paymentMethods.invalidName" },
                    { new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "", "paymentMethods.projectFinished" },
                    { new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "", "paymentMethods.rename" },
                    { new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "", "common.add" },
                    { new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "", "paymentMethods.vatHint" },
                    { new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "", "paymentMethods.save" },
                    { new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "", "paymentMethods.loadFailed" },
                    { new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "", "paymentMethods.name" },
                    { new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "", "paymentMethods.new" },
                    { new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "", "paymentMethods.vatRate" },
                    { new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "", "paymentMethods.vatPercent" },
                    { new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "", "paymentMethods.disable" },
                    { new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "", "projects.invalidVatRate" },
                    { new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "", "paymentMethods.changeVatRate" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("003d61df-901e-682d-f351-a99c427e6991"), "ar", new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "لا توجد طرق دفع." },
                    { new Guid("07ed9ffb-59cd-7c8b-c16b-81de7bd3cb93"), "en", new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "Rename" },
                    { new Guid("146ab521-5928-c96f-0a9c-1f6fec92ad08"), "ar", new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("1839efbc-9109-c577-1bba-73e6f603caf8"), "ar", new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "إضافة" },
                    { new Guid("185f4148-a84f-ba09-74aa-4f1009ba4478"), "ar", new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "رُفض هذا التغيير." },
                    { new Guid("235b8609-5286-fb5e-fe0d-937707400e17"), "ar", new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "حفظ" },
                    { new Guid("2507f7f2-32de-9ba5-9c66-b7a7219dd9e2"), "en", new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "No connection to the server. Try again." },
                    { new Guid("25198551-2c29-9579-5a95-a9f6ac99c0a9"), "ar", new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "طرق الدفع والضريبة" },
                    { new Guid("3308bc99-1fe4-57fd-32d3-4d34abb4ce29"), "en", new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "Could not read the payment methods. Check the network and try again." },
                    { new Guid("36044e90-1f94-6a8f-1be4-c5bd3e2a1de2"), "en", new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "Payment method name" },
                    { new Guid("40babdd9-1424-a07f-7a62-73d37a70eaa1"), "ar", new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("419eb08a-a2ed-58e3-0fd4-e61caf5bab35"), "en", new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "VAT rate" },
                    { new Guid("4696d695-79a4-8ac6-703c-a25a63a7363b"), "en", new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"), "Add" },
                    { new Guid("4b716d34-3ce8-0d24-9bb5-04681af2ab53"), "en", new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "Enter a name of 1 to 100 characters." },
                    { new Guid("5cebf35d-a35b-4eca-e25e-da3715e5bf45"), "en", new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "Add payment method" },
                    { new Guid("6451fd66-1c98-369d-6587-36f245056e8f"), "ar", new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "تعطيل" },
                    { new Guid("65eeaa02-dda1-cfd6-7afe-cb61a7a16e76"), "ar", new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "إضافة طريقة دفع" },
                    { new Guid("692cfad9-fc7f-0973-1da8-4c0545758d07"), "en", new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "Change VAT rate" },
                    { new Guid("6953004b-db75-d71d-3b08-dceb3ff75133"), "en", new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "Rate (%)" },
                    { new Guid("73a2d377-b9bd-d063-79a8-301f64586de4"), "en", new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "Disabled" },
                    { new Guid("822bc6f0-94ad-a561-f348-a5617fe138cc"), "ar", new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "تُحسب القيود الجديدة بهذه النسبة، وتبقى ضريبة القيود المحفوظة كما هي." },
                    { new Guid("8698db59-8216-c3b5-2004-ecdcf4c879af"), "en", new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "Enable" },
                    { new Guid("9173dd85-3bfd-049f-535b-33b878e5b178"), "ar", new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "نسبة ضريبة القيمة المضافة" },
                    { new Guid("9a2d6894-d009-ec9f-938c-42db8e238018"), "en", new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "Enter a rate from 0 to 100, with at most two decimals." },
                    { new Guid("9ce75e50-2054-4b5f-03ce-5c8d12d8d5e0"), "en", new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "New entries use this rate. Saved entries keep their VAT." },
                    { new Guid("a149ee7e-8928-d208-5427-9bb3c7ac87fa"), "en", new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "This project is finished, so nothing in it changes." },
                    { new Guid("a234616e-c390-b179-ca76-e267ec03f326"), "ar", new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "معطّلة" },
                    { new Guid("a43294f3-1f35-071b-20cd-887c0d87ef3e"), "en", new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "This change was refused." },
                    { new Guid("aff86b12-169b-6cef-cd3b-735ff2f92d9a"), "ar", new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "أدخل نسبة من 0 إلى 100، بخانتين عشريتين على الأكثر." },
                    { new Guid("b24dadbc-2c8f-402f-129f-a954e9631890"), "ar", new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "تعذّرت قراءة طرق الدفع. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("b252af9a-1813-f294-391a-1dfee24f3984"), "ar", new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "تغيير نسبة الضريبة" },
                    { new Guid("b6059236-48f8-38c1-ee34-96f03c35bee8"), "en", new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "No payment methods." },
                    { new Guid("c71f9322-5af5-1974-ed24-53969b26e4d8"), "ar", new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("d2712a09-f97d-c1e1-9bd3-93c462deaead"), "ar", new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "إعادة تسمية" },
                    { new Guid("dc89417e-d584-78fa-f8ad-ae03f0d34cbd"), "ar", new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "اسم طريقة الدفع" },
                    { new Guid("ec4581f7-676d-06c7-9300-88ca040dcb92"), "ar", new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "تفعيل" },
                    { new Guid("ee7c7d45-5dbd-0a6a-190f-b662d47e0868"), "en", new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "Disable" },
                    { new Guid("f4b91e3a-cc3d-e2a1-7873-47a7091658bc"), "en", new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "Save" },
                    { new Guid("f7b7479b-e8b5-c733-2a06-1b4a1c8250b9"), "en", new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "Payment methods and VAT" },
                    { new Guid("fc08db74-cb75-7ae5-647f-30ecafe7a528"), "ar", new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "النسبة (%)" }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure', 'ManageSettings')");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ProjectId",
                table: "PaymentMethods",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("618d2584-1949-ab7c-512d-daaf8a7ad632"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("003d61df-901e-682d-f351-a99c427e6991"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("07ed9ffb-59cd-7c8b-c16b-81de7bd3cb93"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("146ab521-5928-c96f-0a9c-1f6fec92ad08"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1839efbc-9109-c577-1bba-73e6f603caf8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("185f4148-a84f-ba09-74aa-4f1009ba4478"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("235b8609-5286-fb5e-fe0d-937707400e17"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2507f7f2-32de-9ba5-9c66-b7a7219dd9e2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25198551-2c29-9579-5a95-a9f6ac99c0a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3308bc99-1fe4-57fd-32d3-4d34abb4ce29"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36044e90-1f94-6a8f-1be4-c5bd3e2a1de2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40babdd9-1424-a07f-7a62-73d37a70eaa1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("419eb08a-a2ed-58e3-0fd4-e61caf5bab35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4696d695-79a4-8ac6-703c-a25a63a7363b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4b716d34-3ce8-0d24-9bb5-04681af2ab53"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5cebf35d-a35b-4eca-e25e-da3715e5bf45"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6451fd66-1c98-369d-6587-36f245056e8f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("65eeaa02-dda1-cfd6-7afe-cb61a7a16e76"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("692cfad9-fc7f-0973-1da8-4c0545758d07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6953004b-db75-d71d-3b08-dceb3ff75133"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73a2d377-b9bd-d063-79a8-301f64586de4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("822bc6f0-94ad-a561-f348-a5617fe138cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8698db59-8216-c3b5-2004-ecdcf4c879af"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9173dd85-3bfd-049f-535b-33b878e5b178"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a2d6894-d009-ec9f-938c-42db8e238018"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ce75e50-2054-4b5f-03ce-5c8d12d8d5e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a149ee7e-8928-d208-5427-9bb3c7ac87fa"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a234616e-c390-b179-ca76-e267ec03f326"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a43294f3-1f35-071b-20cd-887c0d87ef3e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aff86b12-169b-6cef-cd3b-735ff2f92d9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b24dadbc-2c8f-402f-129f-a954e9631890"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b252af9a-1813-f294-391a-1dfee24f3984"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b6059236-48f8-38c1-ee34-96f03c35bee8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c71f9322-5af5-1974-ed24-53969b26e4d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d2712a09-f97d-c1e1-9bd3-93c462deaead"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc89417e-d584-78fa-f8ad-ae03f0d34cbd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ec4581f7-676d-06c7-9300-88ca040dcb92"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ee7c7d45-5dbd-0a6a-190f-b662d47e0868"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4b91e3a-cc3d-e2a1-7873-47a7091658bc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7b7479b-e8b5-c733-2a06-1b4a1c8250b9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc08db74-cb75-7ae5-647f-30ecafe7a528"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("51ff4955-892c-26c8-462a-c9c821be7286"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("96a11976-3fc0-1ed6-0ad6-84a34ed45346"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1157602-7ea9-5700-6aa7-0366db512410"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e949a227-37d6-6e19-399a-55bca00c797a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"));

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Projects");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject', 'ManageStructure')");
        }
    }
}
