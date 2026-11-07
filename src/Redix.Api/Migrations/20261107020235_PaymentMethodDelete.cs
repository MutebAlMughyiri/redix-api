using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "", "paymentMethods.paymentMethodUsed" },
                    { new Guid("67c23c81-f71f-5d99-2fd2-b6f0e4c72978"), "", "paymentMethods.deleteQuestion" },
                    { new Guid("7934c26c-1710-2aab-355c-df6a49ddf926"), "", "paymentMethods.deleteExplain" },
                    { new Guid("e1df62d4-0bfe-c696-74e2-130d142c5624"), "", "paymentMethods.delete" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0f61202b-d205-523e-3593-54f90ea4d69d"), "ar", new Guid("7934c26c-1710-2aab-355c-df6a49ddf926"), "تُحذف من القائمة نهائياً. الطريقة التي دُفع بها مصروف لا تُحذف، بل تُعطَّل." },
                    { new Guid("1dce543d-8dfb-f3c4-c97b-8bb193c5b7f0"), "en", new Guid("7934c26c-1710-2aab-355c-df6a49ddf926"), "It leaves the list for good. A method an expense was paid with is not deleted: disable it instead." },
                    { new Guid("2551a7f7-7bad-e207-94fb-5f8b82826080"), "ar", new Guid("e1df62d4-0bfe-c696-74e2-130d142c5624"), "حذف" },
                    { new Guid("2ebc8975-4868-0691-9f4d-ec60f4ae9464"), "en", new Guid("67c23c81-f71f-5d99-2fd2-b6f0e4c72978"), "Delete this payment method?" },
                    { new Guid("5b7d4119-911c-89ed-de40-8264f72b4b80"), "ar", new Guid("67c23c81-f71f-5d99-2fd2-b6f0e4c72978"), "حذف طريقة الدفع هذه؟" },
                    { new Guid("a72b8281-60b9-f850-ca0a-8aecd004292a"), "ar", new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "دُفع بهذه الطريقة مصروف، فلا تُحذف. عطّلها بدلاً من ذلك." },
                    { new Guid("cee8b3b7-7ded-97ac-6fd6-88da5ae90030"), "en", new Guid("e1df62d4-0bfe-c696-74e2-130d142c5624"), "Delete" },
                    { new Guid("f59132b7-d118-05d5-e3fc-823af0df4506"), "en", new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "An expense was paid with this method, so it is not deleted. Disable it instead." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0f61202b-d205-523e-3593-54f90ea4d69d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1dce543d-8dfb-f3c4-c97b-8bb193c5b7f0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2551a7f7-7bad-e207-94fb-5f8b82826080"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2ebc8975-4868-0691-9f4d-ec60f4ae9464"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5b7d4119-911c-89ed-de40-8264f72b4b80"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a72b8281-60b9-f850-ca0a-8aecd004292a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cee8b3b7-7ded-97ac-6fd6-88da5ae90030"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f59132b7-d118-05d5-e3fc-823af0df4506"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("15b622c9-20e3-855c-21c4-24453272e534"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("67c23c81-f71f-5d99-2fd2-b6f0e4c72978"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7934c26c-1710-2aab-355c-df6a49ddf926"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e1df62d4-0bfe-c696-74e2-130d142c5624"));
        }
    }
}
