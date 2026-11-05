using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryLinksForCreditNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntryLinks",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    LinkedEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryLinks", x => new { x.EntryId, x.Type });
                    table.ForeignKey(
                        name: "FK_EntryLinks_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryLinks_Entries_LinkedEntryId",
                        column: x => x.LinkedEntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "", "expenses.loadFailed" },
                    { new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "", "expenses.creditNoteAboveOriginal" },
                    { new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "", "expenses.returnable" },
                    { new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "", "expenses.deductionNeedsReason" },
                    { new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "", "expenses.creditNoteExplain" },
                    { new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "", "expenses.creditNote" },
                    { new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "", "expenses.noCreditableInvoice" },
                    { new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "", "expenses.noOriginalInvoice" },
                    { new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "", "expenses.creditNoteNeedsInvoice" },
                    { new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "", "expenses.originalInvoice" },
                    { new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "", "expenses.unknownOriginalInvoice" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("06e7cc96-233a-feff-fc9a-59d637f7711a"), "en", new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "Not linked to an invoice" },
                    { new Guid("0eb3ccf6-1dbe-47fc-c422-d56ea0e8468f"), "ar", new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "الفاتورة الأصلية" },
                    { new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"), "en", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "Recorded as a negative; linking it to the original invoice is optional" },
                    { new Guid("219b0a0f-7708-c0e6-18aa-63eb3a731635"), "ar", new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "غير مربوط بفاتورة" },
                    { new Guid("353550c9-aef4-1a44-71f7-b2b06a51a9a7"), "en", new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "This supplier has no invoice to return yet." },
                    { new Guid("36f70000-08c5-72d2-deee-fac0a3d1c321"), "ar", new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "اختر فاتورة لهذا المورد أُرسلت للاعتماد." },
                    { new Guid("37019142-7349-cf37-1450-a08c64ecf923"), "en", new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"), "Choose an invoice of this supplier that was sent for approval." },
                    { new Guid("42b11d5b-3c05-ad62-af3a-d4fda7bd6be8"), "ar", new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "للمرتجع رقمه وتاريخه الخاصان، فلا يكون بدون فاتورة." },
                    { new Guid("50ee8361-d5f7-062d-8c0b-279a7299e279"), "en", new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "A credit note has its own number and date, so it cannot be without an invoice." },
                    { new Guid("62ac94e0-6f89-6f7f-4d04-8afd2124667e"), "ar", new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "المبلغ السالب خصم بلا فاتورة. فعّل بدون فاتورة واذكر السبب." },
                    { new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"), "ar", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "يُسجَّل بالسالب، وربطه بالفاتورة الأصلية اختياري" },
                    { new Guid("7b58b828-c2cf-e53c-8478-cbe48ed06600"), "ar", new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"), "لا توجد لهذا المورد فاتورة يمكن إرجاعها بعد." },
                    { new Guid("8759e2d3-541a-56b4-0e4d-3818a7d4f732"), "ar", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "مرتجع (إشعار دائن)" },
                    { new Guid("acfc8c1c-7588-4d1f-ff57-316c21eeb9a4"), "ar", new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "لا يمكن أن يتجاوز المرتجع المتبقي من فاتورته الأصلية." },
                    { new Guid("b1bea701-6f9c-b217-c4e9-217a2c00f16b"), "en", new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"), "Original invoice" },
                    { new Guid("be853c47-44da-82bf-9b52-4f1a8daea502"), "ar", new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "المتبقي للإرجاع" },
                    { new Guid("c2309f13-f67c-b053-d1d6-2a5511eb2cd3"), "en", new Guid("235b58bb-a013-b56d-484b-3275929fac9f"), "Left to return" },
                    { new Guid("c65ff10d-e85f-0923-a7c8-2f7ae4855757"), "en", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "Credit note" },
                    { new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"), "ar", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "تعذّرت قراءة الفواتير." },
                    { new Guid("d6df82f5-26ef-9d4c-348b-0b6a574c5654"), "en", new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"), "A credit note cannot go above what is left of its original invoice." },
                    { new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"), "en", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "The invoices could not be read." },
                    { new Guid("f77bbe10-14f6-5371-5406-a52050dcaa41"), "en", new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "A negative amount is a deduction with no invoice. Turn on Without an invoice and say why." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntryLinks_LinkedEntryId",
                table: "EntryLinks",
                column: "LinkedEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryLinks");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06e7cc96-233a-feff-fc9a-59d637f7711a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0eb3ccf6-1dbe-47fc-c422-d56ea0e8468f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("219b0a0f-7708-c0e6-18aa-63eb3a731635"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("353550c9-aef4-1a44-71f7-b2b06a51a9a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36f70000-08c5-72d2-deee-fac0a3d1c321"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37019142-7349-cf37-1450-a08c64ecf923"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42b11d5b-3c05-ad62-af3a-d4fda7bd6be8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50ee8361-d5f7-062d-8c0b-279a7299e279"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62ac94e0-6f89-6f7f-4d04-8afd2124667e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7b58b828-c2cf-e53c-8478-cbe48ed06600"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8759e2d3-541a-56b4-0e4d-3818a7d4f732"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("acfc8c1c-7588-4d1f-ff57-316c21eeb9a4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b1bea701-6f9c-b217-c4e9-217a2c00f16b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be853c47-44da-82bf-9b52-4f1a8daea502"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c2309f13-f67c-b053-d1d6-2a5511eb2cd3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c65ff10d-e85f-0923-a7c8-2f7ae4855757"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d6df82f5-26ef-9d4c-348b-0b6a574c5654"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f77bbe10-14f6-5371-5406-a52050dcaa41"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1fa9acf7-0b4a-cb33-2c6b-92a535596b0d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("235b58bb-a013-b56d-484b-3275929fac9f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7f7a43e9-e415-4905-8160-e2de3ff1d90a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8b869839-f215-257c-3c40-13288e65d0d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dd222740-ec12-f979-f92b-d8946a1f4293"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ec6bd3ce-2065-9244-3217-653bd057a20d"));
        }
    }
}
