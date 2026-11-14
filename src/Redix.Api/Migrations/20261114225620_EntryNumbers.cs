using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EntryNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastEntryNumber",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                schema: "project",
                table: "Entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "LastEntryNumber",
                value: 0);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("33999386-2336-9cd6-f2d8-e187dca5902e"), "", "entries.numberOf" },
                    { new Guid("c0ace170-8a8a-86ac-d864-a0979bccde54"), "", "entries.numberAsk" },
                    { new Guid("e22205cc-cd6f-31da-75ed-c7229f25a888"), "", "entries.number" },
                    { new Guid("f7d16307-c731-c06e-24fd-67db5b9847e1"), "", "entries.numberFilterSet" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("0ee13ce5-6a02-1fe3-f30b-55c62aac9019"), "en", new Guid("33999386-2336-9cd6-f2d8-e187dca5902e"), "Entry {0}" },
                    { new Guid("26c69020-9e93-f861-6d89-29699ede677c"), "en", new Guid("c0ace170-8a8a-86ac-d864-a0979bccde54"), "Find by entry number" },
                    { new Guid("2aa279f7-f3c5-fe2e-588e-68db175df53d"), "en", new Guid("e22205cc-cd6f-31da-75ed-c7229f25a888"), "Entry number" },
                    { new Guid("79a97831-6558-e948-2ba7-9c3dfb5b3fbf"), "ar", new Guid("f7d16307-c731-c06e-24fd-67db5b9847e1"), "رقم القيد: {0}" },
                    { new Guid("91d55e9f-7de3-e291-4f6d-8cfd87b11ed4"), "en", new Guid("f7d16307-c731-c06e-24fd-67db5b9847e1"), "Entry number: {0}" },
                    { new Guid("9d0d3943-4f5a-5a11-d4c6-fec5ee1ff965"), "ar", new Guid("e22205cc-cd6f-31da-75ed-c7229f25a888"), "رقم القيد" },
                    { new Guid("c9e0455b-b86d-785e-ef75-9c5254632000"), "ar", new Guid("c0ace170-8a8a-86ac-d864-a0979bccde54"), "ابحث برقم القيد" },
                    { new Guid("f9c42a30-85d1-4e36-d7ca-96094a01cb6c"), "ar", new Guid("33999386-2336-9cd6-f2d8-e187dca5902e"), "قيد {0}" }
                });

            migrationBuilder.Sql("""
                WITH numbered AS (SELECT Number, ROW_NUMBER() OVER (PARTITION BY ProjectId ORDER BY EntryDate, CreatedAt, Id) AS Next FROM project.Entries)
                UPDATE numbered SET Number = Next;
                UPDATE Projects SET LastEntryNumber = ISNULL((SELECT MAX(Number) FROM project.Entries WHERE Entries.ProjectId = Projects.Id), 0);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ProjectId_Number",
                schema: "project",
                table: "Entries",
                columns: new[] { "ProjectId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entries_ProjectId_Number",
                schema: "project",
                table: "Entries");

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0ee13ce5-6a02-1fe3-f30b-55c62aac9019"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("26c69020-9e93-f861-6d89-29699ede677c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2aa279f7-f3c5-fe2e-588e-68db175df53d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("79a97831-6558-e948-2ba7-9c3dfb5b3fbf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91d55e9f-7de3-e291-4f6d-8cfd87b11ed4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d0d3943-4f5a-5a11-d4c6-fec5ee1ff965"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c9e0455b-b86d-785e-ef75-9c5254632000"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f9c42a30-85d1-4e36-d7ca-96094a01cb6c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("33999386-2336-9cd6-f2d8-e187dca5902e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c0ace170-8a8a-86ac-d864-a0979bccde54"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e22205cc-cd6f-31da-75ed-c7229f25a888"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f7d16307-c731-c06e-24fd-67db5b9847e1"));

            migrationBuilder.DropColumn(
                name: "LastEntryNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Number",
                schema: "project",
                table: "Entries");
        }
    }
}
