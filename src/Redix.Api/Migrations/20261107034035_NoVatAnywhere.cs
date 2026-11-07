using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoVatAnywhere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // An old "without invoice" reason, and a late liquidation line's, go into the entry's notes before their column goes (redix-api 96, D17).
            migrationBuilder.Sql("UPDATE e SET Notes = LEFT(CASE WHEN e.Notes = N'' THEN d.WithoutInvoiceReason ELSE e.Notes + N' · ' + d.WithoutInvoiceReason END, 1000) FROM Entries e JOIN ExpenseDetails d ON d.EntryId = e.Id WHERE d.WithoutInvoiceReason <> N''");

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("1140a207-69a5-9621-94a2-448e30d34e91"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("4c72599a-620a-bd34-a9a4-837f4bce891a"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("51d1ca64-8375-50f4-f73f-92b9a50d1dcf"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("566c8741-e471-8621-606e-1ef5c743d5ca"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("64b82c9d-8230-e5f0-75ff-6cad318443e2"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("bdf1c6d1-a35d-76e6-7e49-eef4a61be2c4"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c0baa49c-72e1-f645-7695-2c2441eee40d"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("c45b6129-36e7-a373-23d2-a2a5dcb2e1d2"));

            migrationBuilder.DeleteData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("e494d97a-7ad8-ca92-29ff-ac42c6d13486"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("00d248c7-ea8b-ed71-2f7f-ce322c9f1258"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("01cf41f7-ed5b-f1cf-8b2a-1d051bf64085"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0407605a-c96a-62d3-bc2f-593ad9a0ef00"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06234c06-75c3-633d-2e9f-3ea5aaf4ad95"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("063c36a7-a52c-8f99-e157-75c6014b7495"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06e7cc96-233a-feff-fc9a-59d637f7711a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08df4a9a-d031-634c-94ff-b80d94fde8ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0918cfaa-584a-1cd7-2336-631ea3e9744f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c4759ef-5824-4cec-a20f-8c54fbc5e149"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0c993812-c58a-b23c-a1a1-32bdb1725ab0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1290264a-2564-28fb-8e04-7b4441b3e1db"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("13d39e10-b67f-03c9-c8f6-3057f38b0af1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1868e492-4f67-5a38-f1c5-94ba867c68c4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18b9eee0-307a-772a-3cdf-850d83fd3cf2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1a427fc1-75fe-0119-213c-c42f895e2cc4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1b34439b-bc6b-f639-14dd-60b8309bf48e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d97dfe4-9d43-bd8a-36a3-10fa2f97fc6b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1dd5a3ec-3250-4148-240c-4229f730a11d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1eb22d52-0e48-69f4-d7a2-9f97ca41c3cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2189999a-26ec-0e91-c40c-2bd7013a6b4a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("219b0a0f-7708-c0e6-18aa-63eb3a731635"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("219fb21a-c044-a9a1-4399-ec9f1a973530"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2530fde2-55a4-8e4d-56d7-d6b3ba50b359"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("27a8cd3e-9204-cd9a-7b8e-2f2515ad6651"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2875094f-ba99-8b34-983c-94b96a237f23"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2acfa645-34f0-8e44-617f-f0d43aca161f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2b4c55af-b6cc-a3c0-2db2-04175d90bdb2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("315ed240-d0a6-17d4-861a-534545a30bf0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("319b04ff-9576-d428-0f5c-cd53a14b659a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3453af31-1f8b-4ba0-a77f-9db47b4522a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("345e8887-df72-c02b-8fdd-693edd48bc7f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("36a5e80a-68be-3423-76e3-68c3ddb7489e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37dd53a5-8cb6-fa40-b5b2-47fc5b23838b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37f592a8-7898-35b8-5303-0d71a758d231"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("387021e9-5405-4797-4e87-3d3d33910e1d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38fb4888-c1a7-518e-cac4-c8ddfaeee5ea"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3bce480a-80a8-f0d3-de9b-db146228836c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3bd0132e-5742-36d8-8b81-e60612cc0b9d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("40228410-a508-c207-207f-3bbd2306fe47"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("419eb08a-a2ed-58e3-0fd4-e61caf5bab35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("41e67c6a-8c44-72db-6a8f-2f37885b0177"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4215fd4f-12ab-106b-5bea-9edc6c8505e6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("427d3d93-145b-8785-a13b-7e328b3b3342"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42b11d5b-3c05-ad62-af3a-d4fda7bd6be8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("42c8491e-c8dc-6c57-9a43-6afff6c3e625"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("451805aa-edd0-dded-205c-8d0256534263"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("479ad873-fd5c-3870-a619-c821d697ae19"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bad9f12-4048-d065-7b0e-82673dee5da3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4bf70892-9364-1c40-ed5d-63d8648f7d62"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4ec8849f-c441-a63e-edbd-db1420919e48"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f3c018f-d6b1-4858-9889-c94b96e83c35"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4fdf3500-ebb4-fa5b-9ea8-2a0c56521c43"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("50ee8361-d5f7-062d-8c0b-279a7299e279"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("510e01f3-e1ca-c47d-e6a4-ca19b2df1dd5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("51472944-cc8f-fe92-78ff-3fbfb7f039ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("532b69a3-60e9-8647-6559-e3acd1738cd4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("55d18128-aa0e-664b-5841-1617d11d396a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("570a0a45-5474-0e51-8881-7f9dd4efad09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("581144f7-f4d9-f8fc-f8f0-c1ddf19658d3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("58272d8e-6d4f-35b8-1384-a2997376c82d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5968a925-f1cf-617b-95d3-002099b5d26e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ad01b72-e378-f4d7-ad2b-b031a3284bcc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("611b8227-ef2e-4523-3ba8-f844e53abdce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("61cbc328-18e3-a3d2-6e9b-0779a96923cd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("62ac94e0-6f89-6f7f-4d04-8afd2124667e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6464325f-be76-420a-abb5-13da941abd1d"));

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
                keyValue: new Guid("6e85d605-5f1c-41da-e2b0-48490e8d8250"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("74760369-b920-3a2f-85d4-c16521ba7eca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("79702266-80a0-d5ba-69ce-711592ddd3df"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a8c69fe-882b-9791-4643-5fb093bf03bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7bce868e-859a-9ae7-0fc8-5f0581f8e8a7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7d1a4ce8-de62-91d5-8783-bd99c9b02736"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("80e7ea34-7d5c-920f-0c17-67b9e86fdea3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("822bc6f0-94ad-a561-f348-a5617fe138cc"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8583a7e3-69c4-e8b5-c6f4-a426c27c6413"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86d7a850-1543-d609-addb-941191a53738"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88038e8e-367f-4188-88cd-f94cf17c5015"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8b083dea-98ea-d793-adb2-a095463d1f99"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c4cde6e-3654-36bd-bd3c-7863f0f2c7e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8c565c7c-4d57-4277-c947-c689982936c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9173dd85-3bfd-049f-535b-33b878e5b178"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("91e65c04-d70c-6dbd-e49c-21464eca2190"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("92e4c872-996f-de60-d154-9da571ab8691"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("95c3885a-caa6-6c2a-cb5d-4706238ea0c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98301475-ea8c-23e8-8073-8e4ca59465f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("99f01651-9702-ffae-5a56-3fc7db751226"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a2d6894-d009-ec9f-938c-42db8e238018"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ce69fbc-f56f-a571-2104-1955da8f8ccd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9ce75e50-2054-4b5f-03ce-5c8d12d8d5e0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9fb68fda-b1f8-f83f-b70b-2bba8ef2a843"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2b7234c-1538-9015-0a75-63604bb8e72b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a48c5b1a-c866-f75e-f4ba-a729b8bbce68"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a5b6ed39-bd8f-c33a-b6bc-f64749c88618"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a6d03bd0-29a0-e796-bfd0-c7c8da86a3a5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a71d72c3-6858-04ec-553f-194aabd202ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8d7b5b7-eeea-04f7-f68f-a3c5a5f95e94"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a8efe087-99df-b9e4-4b01-787565f9d2b7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa1aae2e-d7b1-9901-05e2-2a7612a3be3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac8bf060-5f66-2664-b3e4-6b0d4e29723e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("adece867-dd5a-24b1-900c-90a9c2ca7114"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aff86b12-169b-6cef-cd3b-735ff2f92d9a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b252af9a-1813-f294-391a-1dfee24f3984"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b2c0c9d4-3e8f-3c67-61c4-4e177e660867"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bbf07f8f-5ce8-63d7-c2af-8c3d93fa735f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bd47f371-748c-d182-1828-5e2323a2128d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bdd98fb3-a3a8-2eb1-fabe-3b726242c402"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("be50484a-72ad-01a9-81b5-bfb48f7cef07"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf309a0e-b409-2873-fd9b-ee5bdd2006bd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf697ab2-75e5-ee2c-1aaf-f55e29dcc2c9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0a8abcf-db11-8db8-ce81-50a11250d401"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c455c684-5893-66b9-5eb0-f47858bee224"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6e4f087-55f6-9813-1537-d3ca1ae41cb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cbc3d0de-8d6e-8d92-0d41-0edd92fc3eff"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ce39e479-d2df-5488-2ca2-9b21bb9f8da4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0fe5a27-c910-a240-1021-4bc47430f116"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d493159f-4ba7-6691-202b-9939403bfc4c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d7034bd1-03ca-5a31-deff-faf88b85e392"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d8a3fc6d-f1ff-e81e-20e4-cee141fd6706"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d994c2f3-f8af-b180-fb12-1a40fa8d64e9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("da0048f7-95c7-bf7b-03f7-d334d3704f0b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc0ed562-912b-048f-98bc-13ac6eb6e2e3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dd394de8-3820-ea29-69b2-374476e406d9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("de436f1f-1700-1ae3-c753-59c35d561fc4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("df7b0129-43a3-513a-fa97-56b09eacc008"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e4b4ac84-d6a3-994d-cf6c-a4afda351b98"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e6947b4d-9d20-19c5-6a58-7587aae59fa1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f0ff4b3f-82ca-45a5-7573-21163e6e6e81"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f77bbe10-14f6-5371-5406-a52050dcaa41"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f8577729-6ee9-fe5a-b8eb-d9a88cde0f01"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fa34bf44-118e-0109-822e-87769e4a9f1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("faf7f2a7-915b-14ae-7e49-9347e241ac7c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fc08db74-cb75-7ae5-647f-30ecafe7a528"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fcba39e4-fade-72f3-405c-948e93b63078"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("fde3ef5f-1e6d-96ca-a54f-383e780c1566"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("39326569-bfee-321b-18c2-de9542077919"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("448b37d3-a767-2874-ec18-b020696848f5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("52314600-de79-f3ab-38c6-5ded7893e819"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("536dbb67-b590-59b3-3de2-a0635356666a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("58c59be4-d26b-e903-ffab-2905910659c8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("71600c58-1298-79d7-4df1-acdbbf463377"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8b869839-f215-257c-3c40-13288e65d0d0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8f382797-5142-3cb6-5646-b245912d0d89"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"));

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
                keyValue: new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e949a227-37d6-6e19-399a-55bca00c797a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e970f65e-3490-c978-4180-795440d833be"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"));

            migrationBuilder.DropColumn(
                name: "AmountAfterDiscount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "AmountBeforeDiscount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "TaxType",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "VatPeriodStart",
                table: "SalesInvoice");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WithoutInvoiceReason",
                table: "ExpenseDetails");

            migrationBuilder.InsertData(
                table: "ConfigValues",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { new Guid("9bb147da-c022-65eb-9451-57b6a512e0c7"), "vatRatePercent", "15" });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { true, 6 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 12);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { false, 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                column: "IsRequired",
                value: false);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { false, 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { true, 6 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "", "expenses.deductionNeedsNotes" },
                    { new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "", "revenue.total" },
                    { new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "", "expenses.date" },
                    { new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "", "expenses.creditNoteNeedsOriginal" },
                    { new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "", "contractorPayments.deductionNeedsNotes" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"),
                column: "Value",
                value: "Recorded as a negative against the bill it returns");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25198551-2c29-9579-5a95-a9f6ac99c0a9"),
                column: "Value",
                value: "طرق الدفع");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38ed7541-948c-8ec5-4256-6644057d5cef"),
                column: "Value",
                value: "The date must be within the project's period and not in the future.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"),
                column: "Value",
                value: "يُسجَّل بالسالب على الفاتورة التي يرجعها");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb3e0bec-d780-3c81-2b66-b608dd235e6f"),
                column: "Value",
                value: "يجب أن يقع التاريخ ضمن فترة المشروع وألا يكون في المستقبل.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7b7479b-e8b5-c733-2a06-1b4a1c8250b9"),
                column: "Value",
                value: "Payment methods");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("2be95438-64af-07aa-0f70-3726bed1accf"), "en", new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "A negative amount is a deduction: say why in the notes." },
                    { new Guid("45a53f0c-d82a-832e-b674-ccdccf84fddb"), "ar", new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "اختر الفاتورة التي يرجعها هذا المرتجع." },
                    { new Guid("48d1b75f-6c76-2b8a-5411-49d61e13f079"), "en", new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"), "Choose the bill this credit note returns." },
                    { new Guid("73b9b7d9-252a-12b3-0f7a-a20582ea149e"), "ar", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "الإجمالي" },
                    { new Guid("8e817f1d-9085-f6aa-c189-cad8f32fe7b5"), "ar", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "المبلغ السالب خصم: اذكر سببه في الملاحظات." },
                    { new Guid("99d1d398-6591-bfc7-d441-4a9accd86709"), "ar", new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "التاريخ" },
                    { new Guid("ac802dc0-5481-f4c4-a770-1a12f7da8cb4"), "en", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "A negative amount is a deduction: say why in the notes." },
                    { new Guid("aea6dc08-82c8-66dd-0b18-d8ffe2d56221"), "ar", new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"), "المبلغ السالب خصم: اذكر سببه في الملاحظات." },
                    { new Guid("b015f457-0983-afca-0f2b-bc7d6609589d"), "en", new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "Date" },
                    { new Guid("b3d2cce1-0cb6-36ce-a025-1d132c6893cf"), "en", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "Total" }
                });

            // Every project's own columns follow the template: no VAT columns (D26), no tax invoice on a contractor payment (D16), the file never required (D3, D16), and the total always.
            migrationBuilder.Sql("DELETE c FROM ItemColumns c WHERE c.[Key] IN (N'amountBeforeDiscount', N'discount', N'vat') OR (c.[Key] = N'invoiceNumber' AND EXISTS (SELECT 1 FROM Items i WHERE i.Id = c.ItemId AND i.Type = 1))");
            migrationBuilder.Sql("DELETE FROM DefaultColumns WHERE [Key] IN (N'amountBeforeDiscount', N'discount', N'vat') OR ([Key] = N'invoiceNumber' AND ItemType = 1)");
            migrationBuilder.Sql("UPDATE ItemColumns SET IsRequired = CASE [Key] WHEN N'total' THEN 1 ELSE 0 END WHERE [Key] IN (N'attachment', N'total')");
            migrationBuilder.Sql("UPDATE DefaultColumns SET IsRequired = CASE [Key] WHEN N'total' THEN 1 ELSE 0 END WHERE [Key] IN (N'attachment', N'total')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The reasons moved into notes, each project's VAT rate, and every sale's tax figures are not brought back: only a backup taken before this migration has them.
            migrationBuilder.DeleteData(
                table: "ConfigValues",
                keyColumn: "Id",
                keyValue: new Guid("9bb147da-c022-65eb-9451-57b6a512e0c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2be95438-64af-07aa-0f70-3726bed1accf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("45a53f0c-d82a-832e-b674-ccdccf84fddb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("48d1b75f-6c76-2b8a-5411-49d61e13f079"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("73b9b7d9-252a-12b3-0f7a-a20582ea149e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8e817f1d-9085-f6aa-c189-cad8f32fe7b5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("99d1d398-6591-bfc7-d441-4a9accd86709"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ac802dc0-5481-f4c4-a770-1a12f7da8cb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aea6dc08-82c8-66dd-0b18-d8ffe2d56221"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b015f457-0983-afca-0f2b-bc7d6609589d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b3d2cce1-0cb6-36ce-a025-1d132c6893cf"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9b62df50-4782-25e5-db95-b0535b1cbc29"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b2930b4f-5662-5caa-af13-08539cb21a06"));

            migrationBuilder.AddColumn<decimal>(
                name: "AmountAfterDiscount",
                table: "SalesInvoice",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBeforeDiscount",
                table: "SalesInvoice",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "SalesInvoice",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "SalesInvoice",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "TaxType",
                table: "SalesInvoice",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VatPeriodStart",
                table: "SalesInvoice",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "Projects",
                type: "decimal(19,4)",
                precision: 19,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WithoutInvoiceReason",
                table: "ExpenseDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"),
                column: "SortOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { false, 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"),
                column: "SortOrder",
                value: 15);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"),
                column: "SortOrder",
                value: 16);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { true, 12 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"),
                column: "IsRequired",
                value: true);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"),
                column: "SortOrder",
                value: 11);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("b07fdd34-db99-3860-7f42-6a006a167443"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { true, 12 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"),
                columns: new[] { "IsRequired", "SortOrder" },
                values: new object[] { false, 9 });

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"),
                column: "SortOrder",
                value: 13);

            migrationBuilder.UpdateData(
                table: "DefaultColumns",
                keyColumn: "Id",
                keyValue: new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"),
                column: "SortOrder",
                value: 14);

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "Options", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("1140a207-69a5-9621-94a2-448e30d34e91"), (byte)2, true, false, (byte)2, "discount", "الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("4c72599a-620a-bd34-a9a4-837f4bce891a"), (byte)0, true, false, (byte)1, "invoiceNumber", "رقم الفاتورة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("51d1ca64-8375-50f4-f73f-92b9a50d1dcf"), (byte)2, true, true, (byte)2, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 6 },
                    { new Guid("566c8741-e471-8621-606e-1ef5c743d5ca"), (byte)2, true, false, (byte)1, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 11 },
                    { new Guid("64b82c9d-8230-e5f0-75ff-6cad318443e2"), (byte)2, true, false, (byte)1, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 10 },
                    { new Guid("bdf1c6d1-a35d-76e6-7e49-eef4a61be2c4"), (byte)2, true, true, (byte)0, "amountBeforeDiscount", "المبلغ قبل الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 6 },
                    { new Guid("c0baa49c-72e1-f645-7695-2c2441eee40d"), (byte)2, true, false, (byte)0, "discount", "الخصم", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("c45b6129-36e7-a373-23d2-a2a5dcb2e1d2"), (byte)2, true, false, (byte)0, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("e494d97a-7ad8-ca92-29ff-ac42c6d13486"), (byte)2, true, false, (byte)2, "vat", "الضريبة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 }
                });

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"),
                column: "VatRate",
                value: 0.15m);

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "", "expenses.invoiceAndReason" },
                    { new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "", "revenue.taxType" },
                    { new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "", "vat.declareLater" },
                    { new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "", "vat.withoutInvoice" },
                    { new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "", "expenses.beforeVat" },
                    { new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "", "vat.declaredLater" },
                    { new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "", "approvals.sellerDiffers" },
                    { new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "", "expenses.deductionNeedsReason" },
                    { new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "", "revenue.taxVat" },
                    { new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "", "expenses.afterDiscount" },
                    { new Guid("39326569-bfee-321b-18c2-de9542077919"), "", "expenses.includingVat" },
                    { new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "", "expenses.withoutInvoiceReason" },
                    { new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "", "expenses.vat" },
                    { new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "", "expenses.duplicateInvoice" },
                    { new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "", "revenue.statementRefund" },
                    { new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "", "revenue.quarter" },
                    { new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "", "expenses.invalidInvoiceAmount" },
                    { new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "", "expenses.notTaxable" },
                    { new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "", "expenses.discount" },
                    { new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "", "vat.notTaxable" },
                    { new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "", "vat.afterDiscount" },
                    { new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "", "contractorPayments.deductionNeedsReason" },
                    { new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "", "expenses.invalidInvoiceDate" },
                    { new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "", "expenses.invoiceNumber" },
                    { new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "", "expenses.receiptRequired" },
                    { new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "", "revenue.invalidDiscount" },
                    { new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "", "vat.netDue" },
                    { new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "", "expenses.qrVatDiffers" },
                    { new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "", "reports.invoiceImages" },
                    { new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "", "vat.noFile" },
                    { new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "", "vat.purchasesVat" },
                    { new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "", "expenses.amountIncludingVat" },
                    { new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "", "revenue.amountBeforeTax" },
                    { new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "", "vat.creditNotes" },
                    { new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "", "expenses.withoutInvoice" },
                    { new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "", "vat.salesVat" },
                    { new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "", "revenue.refundDifference" },
                    { new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "", "expenses.noOriginalInvoice" },
                    { new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "", "approvals.includingVat" },
                    { new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "", "expenses.invoiceDate" },
                    { new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "", "approvals.invoiceTotal" },
                    { new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "", "expenses.invalidInvoiceNumber" },
                    { new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "", "paymentMethods.vatHint" },
                    { new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "", "expenses.invalidDiscount" },
                    { new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "", "revenue.discount" },
                    { new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "", "vat.noVatNumber" },
                    { new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "", "vat.netRefund" },
                    { new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "", "expenses.paymentDate" },
                    { new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "", "contractorPayments.hasInvoice" },
                    { new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "", "contractorPayments.deductionReason" },
                    { new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "", "expenses.invalidWithoutInvoiceReason" },
                    { new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "", "vat.exceptions" },
                    { new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "", "expenses.saveAnyway" },
                    { new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "", "share.goesBack" },
                    { new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "", "vat.realEstateTax" },
                    { new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "", "paymentMethods.vatRate" },
                    { new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "", "paymentMethods.vatPercent" },
                    { new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "", "expenses.invoiceRequired" },
                    { new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "", "approvals.withoutInvoice" },
                    { new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "", "revenue.taxRealEstateTransaction" },
                    { new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "", "expenses.creditNoteNeedsInvoice" },
                    { new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "", "projects.invalidVatRate" },
                    { new Guid("e970f65e-3490-c978-4180-795440d833be"), "", "contractorPayments.invoiceExplain" },
                    { new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "", "vat.noExceptions" },
                    { new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "", "expenses.amountBeforeVat" },
                    { new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "", "offline.sendAnyway" },
                    { new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "", "paymentMethods.changeVatRate" },
                    { new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "", "vat.purchaseInvoices" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"),
                column: "Value",
                value: "Recorded as a negative; linking it to the original invoice is optional");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("25198551-2c29-9579-5a95-a9f6ac99c0a9"),
                column: "Value",
                value: "طرق الدفع والضريبة");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("38ed7541-948c-8ec5-4256-6644057d5cef"),
                column: "Value",
                value: "The payment date must be within the project's period and not in the future.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"),
                column: "Value",
                value: "يُسجَّل بالسالب، وربطه بالفاتورة الأصلية اختياري");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cb3e0bec-d780-3c81-2b66-b608dd235e6f"),
                column: "Value",
                value: "يجب أن يقع تاريخ الدفع ضمن فترة المشروع وألا يتجاوز اليوم.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7b7479b-e8b5-c733-2a06-1b4a1c8250b9"),
                column: "Value",
                value: "Payment methods and VAT");

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("00d248c7-ea8b-ed71-2f7f-ce322c9f1258"), "ar", new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "المبلغ قبل الضريبة" },
                    { new Guid("01cf41f7-ed5b-f1cf-8b2a-1d051bf64085"), "ar", new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "ضريبة المشتريات" },
                    { new Guid("0407605a-c96a-62d3-bc2f-593ad9a0ef00"), "en", new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "credit notes" },
                    { new Guid("06234c06-75c3-633d-2e9f-3ea5aaf4ad95"), "ar", new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "بعد الخصم" },
                    { new Guid("063c36a7-a52c-8f99-e157-75c6014b7495"), "en", new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "Net due" },
                    { new Guid("06e7cc96-233a-feff-fc9a-59d637f7711a"), "en", new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "Not linked to an invoice" },
                    { new Guid("08df4a9a-d031-634c-94ff-b80d94fde8ac"), "en", new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "Declare in the next quarter" },
                    { new Guid("0918cfaa-584a-1cd7-2336-631ea3e9744f"), "ar", new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "بدون فاتورة" },
                    { new Guid("0c4759ef-5824-4cec-a20f-8c54fbc5e149"), "ar", new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "اذكر سبب الخصم." },
                    { new Guid("0c993812-c58a-b23c-a1a1-32bdb1725ab0"), "en", new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "Amount before VAT" },
                    { new Guid("1290264a-2564-28fb-8e04-7b4441b3e1db"), "en", new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "Add the transfer receipt or the receipt voucher before sending." },
                    { new Guid("13d39e10-b67f-03c9-c8f6-3057f38b0af1"), "ar", new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "قبل الضريبة بعد الخصم" },
                    { new Guid("1868e492-4f67-5a38-f1c5-94ba867c68c4"), "ar", new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "أدخل خصماً لا يتجاوز المبلغ، بخانتين عشريتين كحد أقصى." },
                    { new Guid("18b9eee0-307a-772a-3cdf-850d83fd3cf2"), "en", new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "The date must be within the project's period and not in the future." },
                    { new Guid("1a427fc1-75fe-0119-213c-c42f895e2cc4"), "ar", new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "للدفعة فاتورة ضريبية" },
                    { new Guid("1b34439b-bc6b-f639-14dd-60b8309bf48e"), "en", new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "No invoice" },
                    { new Guid("1d97dfe4-9d43-bd8a-36a3-10fa2f97fc6b"), "ar", new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "الرقم الضريبي في الفاتورة يخالف رقم المورد" },
                    { new Guid("1dd5a3ec-3250-4148-240c-4229f730a11d"), "ar", new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "صور الفواتير" },
                    { new Guid("1eb22d52-0e48-69f4-d7a2-9f97ca41c3cb"), "ar", new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "أدخل مبلغ الفاتورة قبل الضريبة، أكبر من صفر وبخانتين عشريتين على الأكثر." },
                    { new Guid("2189999a-26ec-0e91-c40c-2bd7013a6b4a"), "ar", new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "إرسال رغم ذلك" },
                    { new Guid("219b0a0f-7708-c0e6-18aa-63eb3a731635"), "ar", new Guid("8b869839-f215-257c-3c40-13288e65d0d0"), "غير مربوط بفاتورة" },
                    { new Guid("219fb21a-c044-a9a1-4399-ec9f1a973530"), "en", new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "Quarter" },
                    { new Guid("2530fde2-55a4-8e4d-56d7-d6b3ba50b359"), "en", new Guid("38c1315d-cece-ac60-b65e-0e6a39cdf601"), "Before VAT, after discount" },
                    { new Guid("27a8cd3e-9204-cd9a-7b8e-2f2515ad6651"), "en", new Guid("39326569-bfee-321b-18c2-de9542077919"), "Including VAT" },
                    { new Guid("2875094f-ba99-8b34-983c-94b96a237f23"), "ar", new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "الضريبة" },
                    { new Guid("2acfa645-34f0-8e44-617f-f0d43aca161f"), "en", new Guid("58c59be4-d26b-e903-ffab-2905910659c8"), "After discount" },
                    { new Guid("2b4c55af-b6cc-a3c0-2db2-04175d90bdb2"), "ar", new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "لا استثناءات." },
                    { new Guid("315ed240-d0a6-17d4-861a-534545a30bf0"), "ar", new Guid("5a90215d-8e19-7efa-60e6-692b27cd34e3"), "يجب أن يقع التاريخ ضمن فترة المشروع وألا يتجاوز اليوم." },
                    { new Guid("319b04ff-9576-d428-0f5c-cd53a14b659a"), "ar", new Guid("49452b05-4dd2-819d-9d64-21adeb5006ff"), "الربع" },
                    { new Guid("3453af31-1f8b-4ba0-a77f-9db47b4522a9"), "ar", new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "هذه الفاتورة غير خاضعة للضريبة" },
                    { new Guid("345e8887-df72-c02b-8fdd-693edd48bc7f"), "en", new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "Sales VAT" },
                    { new Guid("36a5e80a-68be-3423-76e3-68c3ddb7489e"), "ar", new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "المبلغ شامل الضريبة" },
                    { new Guid("37dd53a5-8cb6-fa40-b5b2-47fc5b23838b"), "ar", new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "لا يتجاوز الخصم المبلغ." },
                    { new Guid("37f592a8-7898-35b8-5303-0d71a758d231"), "ar", new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "الفرق" },
                    { new Guid("387021e9-5405-4797-4e87-3d3d33910e1d"), "en", new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "Invoice number" },
                    { new Guid("38fb4888-c1a7-518e-cac4-c8ddfaeee5ea"), "ar", new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "الفرق (مسترد)" },
                    { new Guid("3bce480a-80a8-f0d3-de9b-db146228836c"), "ar", new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "الخصم" },
                    { new Guid("3bd0132e-5742-36d8-8b81-e60612cc0b9d"), "en", new Guid("6f4ec94e-a710-9909-4460-eac3400ee247"), "Invoice images" },
                    { new Guid("40228410-a508-c207-207f-3bbd2306fe47"), "en", new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "An expense without an invoice carries no invoice file." },
                    { new Guid("419eb08a-a2ed-58e3-0fd4-e61caf5bab35"), "en", new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "VAT rate" },
                    { new Guid("41e67c6a-8c44-72db-6a8f-2f37885b0177"), "ar", new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "بدون فاتورة" },
                    { new Guid("4215fd4f-12ab-106b-5bea-9edc6c8505e6"), "ar", new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "سبب عدم وجود فاتورة" },
                    { new Guid("427d3d93-145b-8785-a13b-7e328b3b3342"), "ar", new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "قبل الضريبة" },
                    { new Guid("42b11d5b-3c05-ad62-af3a-d4fda7bd6be8"), "ar", new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "للمرتجع رقمه وتاريخه الخاصان، فلا يكون بدون فاتورة." },
                    { new Guid("42c8491e-c8dc-6c57-9a43-6afff6c3e625"), "ar", new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "تاريخ الدفع" },
                    { new Guid("451805aa-edd0-dded-205c-8d0256534263"), "en", new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "Real estate transaction tax" },
                    { new Guid("479ad873-fd5c-3870-a619-c821d697ae19"), "en", new Guid("7ac78403-8c65-8f50-f9a5-e472439fdcac"), "Purchases VAT" },
                    { new Guid("4bad9f12-4048-d065-7b0e-82673dee5da3"), "ar", new Guid("f1c51566-e5e4-84f1-c0da-cc133832638c"), "المبلغ قبل الضريبة" },
                    { new Guid("4bf70892-9364-1c40-ed5d-63d8648f7d62"), "ar", new Guid("5d7051fd-39fd-3359-14e6-9d2911d660ee"), "أضف إيصال التحويل أو سند الاستلام قبل الإرسال." },
                    { new Guid("4ec8849f-c441-a63e-edbd-db1420919e48"), "en", new Guid("eeeb9b17-b6ee-d56b-d32c-fe1f57cf5ace"), "No exceptions." },
                    { new Guid("4f3c018f-d6b1-4858-9889-c94b96e83c35"), "en", new Guid("1512645c-c7fb-e5c6-4261-9d7b67bafc6f"), "Before VAT" },
                    { new Guid("4fdf3500-ebb4-fa5b-9ea8-2a0c56521c43"), "ar", new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "مدرجة في ربع لاحق" },
                    { new Guid("50ee8361-d5f7-062d-8c0b-279a7299e279"), "en", new Guid("dae028e9-ad68-bbcc-9efe-537cb1012894"), "A credit note has its own number and date, so it cannot be without an invoice." },
                    { new Guid("510e01f3-e1ca-c47d-e6a4-ca19b2df1dd5"), "ar", new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "ضريبة التصرفات العقارية" },
                    { new Guid("51472944-cc8f-fe92-78ff-3fbfb7f039ac"), "en", new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "This supplier's invoice number is already entered." },
                    { new Guid("532b69a3-60e9-8647-6559-e3acd1738cd4"), "en", new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "VAT" },
                    { new Guid("55d18128-aa0e-664b-5841-1617d11d396a"), "ar", new Guid("41bd2f90-dee4-67b8-a61b-550266149a69"), "رقم فاتورة هذا المورد مُدخل مسبقاً." },
                    { new Guid("570a0a45-5474-0e51-8881-7f9dd4efad09"), "en", new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "Discount" },
                    { new Guid("581144f7-f4d9-f8fc-f8f0-c1ddf19658d3"), "en", new Guid("5f5e25c2-19af-2f61-e248-57338ea12ba9"), "The discount cannot be above the amount." },
                    { new Guid("58272d8e-6d4f-35b8-1384-a2997376c82d"), "ar", new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "أدخل رقم الفاتورة، حتى 50 حرفاً." },
                    { new Guid("5968a925-f1cf-617b-95d3-002099b5d26e"), "ar", new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "إجمالي الفاتورة" },
                    { new Guid("5ad01b72-e378-f4d7-ad2b-b031a3284bcc"), "ar", new Guid("d6e3ba3a-6892-f3c9-af60-14b1ebda49fd"), "ضريبة التصرفات العقارية" },
                    { new Guid("611b8227-ef2e-4523-3ba8-f844e53abdce"), "ar", new Guid("8498f5b8-e365-7e70-c0be-ea4a75b171e2"), "ضريبة المبيعات" },
                    { new Guid("61cbc328-18e3-a3d2-6e9b-0779a96923cd"), "en", new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "Including VAT" },
                    { new Guid("62ac94e0-6f89-6f7f-4d04-8afd2124667e"), "ar", new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "المبلغ السالب خصم بلا فاتورة. فعّل بدون فاتورة واذكر السبب." },
                    { new Guid("6464325f-be76-420a-abb5-13da941abd1d"), "en", new Guid("56b2809b-800a-2dfd-bb1e-0dc47ae1e996"), "Discount" },
                    { new Guid("692cfad9-fc7f-0973-1da8-4c0545758d07"), "en", new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "Change VAT rate" },
                    { new Guid("6953004b-db75-d71d-3b08-dceb3ff75133"), "en", new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "Rate (%)" },
                    { new Guid("6e85d605-5f1c-41da-e2b0-48490e8d8250"), "en", new Guid("f319f805-8eef-cbce-6d05-752b5c30ff46"), "Send anyway" },
                    { new Guid("74760369-b920-3a2f-85d4-c16521ba7eca"), "en", new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "Why it is deducted" },
                    { new Guid("79702266-80a0-d5ba-69ce-711592ddd3df"), "en", new Guid("80abf007-9b28-f9ce-0bd0-e3190a886022"), "Amount including VAT" },
                    { new Guid("7a8c69fe-882b-9791-4643-5fb093bf03bd"), "en", new Guid("e970f65e-3490-c978-4180-795440d833be"), "It enters the VAT statement under the contractor's name." },
                    { new Guid("7bce868e-859a-9ae7-0fc8-5f0581f8e8a7"), "en", new Guid("9d56be88-02dd-b860-fb24-03d8bc0f2f1d"), "Enter a discount no larger than the amount, with at most two decimals." },
                    { new Guid("7d1a4ce8-de62-91d5-8783-bd99c9b02736"), "en", new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "No file" },
                    { new Guid("80e7ea34-7d5c-920f-0c17-67b9e86fdea3"), "ar", new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "بلا رقم ضريبي" },
                    { new Guid("822bc6f0-94ad-a561-f348-a5617fe138cc"), "ar", new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "تُحسب القيود الجديدة بهذه النسبة، وتبقى ضريبة القيود المحفوظة كما هي." },
                    { new Guid("8583a7e3-69c4-e8b5-c6f4-a426c27c6413"), "ar", new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "المتوقع في الكشف" },
                    { new Guid("86d7a850-1543-d609-addb-941191a53738"), "ar", new Guid("8f382797-5142-3cb6-5646-b245912d0d89"), "شامل ضريبة" },
                    { new Guid("88038e8e-367f-4188-88cd-f94cf17c5015"), "en", new Guid("448b37d3-a767-2874-ec18-b020696848f5"), "The statement expected" },
                    { new Guid("8b083dea-98ea-d793-adb2-a095463d1f99"), "ar", new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "الضريبة" },
                    { new Guid("8c4cde6e-3654-36bd-bd3c-7863f0f2c7e3"), "en", new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "Save anyway" },
                    { new Guid("8c565c7c-4d57-4277-c947-c689982936c5"), "en", new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "This expense is approved. Attaching returns it to approval." },
                    { new Guid("9173dd85-3bfd-049f-535b-33b878e5b178"), "ar", new Guid("d0321e9a-7916-a2a8-6f05-552739279ca7"), "نسبة ضريبة القيمة المضافة" },
                    { new Guid("91e65c04-d70c-6dbd-e49c-21464eca2190"), "en", new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "Not taxable" },
                    { new Guid("92e4c872-996f-de60-d154-9da571ab8691"), "ar", new Guid("01cdc3b3-1870-8ce1-c01a-dd0c219e00de"), "المصروف بدون فاتورة لا يُرفق به ملف فاتورة." },
                    { new Guid("95c3885a-caa6-6c2a-cb5d-4706238ea0c5"), "en", new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "Add the invoice's picture or PDF before sending." },
                    { new Guid("98301475-ea8c-23e8-8073-8e4ca59465f1"), "en", new Guid("406c1a1b-53c0-cd2a-4df9-6b27b6fe0d06"), "VAT" },
                    { new Guid("99f01651-9702-ffae-5a56-3fc7db751226"), "ar", new Guid("e970f65e-3490-c978-4180-795440d833be"), "تدخل كشف الضريبة باسم المقاول." },
                    { new Guid("9a2d6894-d009-ec9f-938c-42db8e238018"), "en", new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "Enter a rate from 0 to 100, with at most two decimals." },
                    { new Guid("9ce69fbc-f56f-a571-2104-1955da8f8ccd"), "en", new Guid("2bfccd6e-2282-b87c-ed63-0c35b7676c02"), "VAT number on the invoice differs from the supplier's" },
                    { new Guid("9ce75e50-2054-4b5f-03ce-5c8d12d8d5e0"), "en", new Guid("9ab0428c-4eda-f5db-2c4e-28d64886ab84"), "New entries use this rate. Saved entries keep their VAT." },
                    { new Guid("9fb68fda-b1f8-f83f-b70b-2bba8ef2a843"), "ar", new Guid("573d6027-c8f8-b8ed-5f18-155384155a62"), "غير خاضعة" },
                    { new Guid("a2b7234c-1538-9015-0a75-63604bb8e72b"), "en", new Guid("52314600-de79-f3ab-38c6-5ded7893e819"), "Enter the invoice's amount before VAT, above zero with at most two decimals." },
                    { new Guid("a48c5b1a-c866-f75e-f4ba-a729b8bbce68"), "en", new Guid("536dbb67-b590-59b3-3de2-a0635356666a"), "This invoice is not taxable" },
                    { new Guid("a5b6ed39-bd8f-c33a-b6bc-f64749c88618"), "ar", new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "قائمة الاستثناءات" },
                    { new Guid("a6d03bd0-29a0-e796-bfd0-c7c8da86a3a5"), "en", new Guid("8223f6d5-e1d8-9e10-d18b-625d7947bbe7"), "Without an invoice" },
                    { new Guid("a71d72c3-6858-04ec-553f-194aabd202ce"), "en", new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "Say why there is no invoice, in up to 500 characters." },
                    { new Guid("a8d7b5b7-eeea-04f7-f68f-a3c5a5f95e94"), "en", new Guid("8124e7f0-fbb1-326d-9da1-7c40067ec596"), "Amount before tax" },
                    { new Guid("a8efe087-99df-b9e4-4b01-787565f9d2b7"), "en", new Guid("a1604573-6e30-2311-c7c5-2674f5bd31a6"), "Net (refund)" },
                    { new Guid("aa1aae2e-d7b1-9901-05e2-2a7612a3be3d"), "ar", new Guid("076b36bb-3153-38b5-5c9b-0f0c5db3cf04"), "إدراج في الربع التالي" },
                    { new Guid("ac8bf060-5f66-2664-b3e4-6b0d4e29723e"), "ar", new Guid("0e510b73-70ba-c174-5695-d0a9b5736f34"), "بلا فاتورة" },
                    { new Guid("adece867-dd5a-24b1-900c-90a9c2ca7114"), "ar", new Guid("71600c58-1298-79d7-4df1-acdbbf463377"), "بلا ملف" },
                    { new Guid("aff86b12-169b-6cef-cd3b-735ff2f92d9a"), "ar", new Guid("e949a227-37d6-6e19-399a-55bca00c797a"), "أدخل نسبة من 0 إلى 100، بخانتين عشريتين على الأكثر." },
                    { new Guid("b252af9a-1813-f294-391a-1dfee24f3984"), "ar", new Guid("f4bebb32-89dd-834b-172e-f2648461c9f2"), "تغيير نسبة الضريبة" },
                    { new Guid("b2c0c9d4-3e8f-3c67-61c4-4e177e660867"), "ar", new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "فواتير المشتريات" },
                    { new Guid("bbf07f8f-5ce8-63d7-c2af-8c3d93fa735f"), "en", new Guid("d5649a27-1638-d95d-0c9c-c2b28901a4c7"), "Without invoice" },
                    { new Guid("bd47f371-748c-d182-1828-5e2323a2128d"), "ar", new Guid("5a91e337-2178-992a-0da0-4ab598ceff6b"), "رقم الفاتورة" },
                    { new Guid("bdd98fb3-a3a8-2eb1-fabe-3b726242c402"), "en", new Guid("3bbbb998-34d8-a46a-3c7d-b0fa7aa719d0"), "Why there is no invoice" },
                    { new Guid("be50484a-72ad-01a9-81b5-bfb48f7cef07"), "en", new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "The VAT on the invoice differs from the computed VAT by more than 0.01. You can still save." },
                    { new Guid("bf309a0e-b409-2873-fd9b-ee5bdd2006bd"), "ar", new Guid("39326569-bfee-321b-18c2-de9542077919"), "شامل الضريبة" },
                    { new Guid("bf697ab2-75e5-ee2c-1aaf-f55e29dcc2c9"), "en", new Guid("ce292a9a-625d-67be-3c92-a76a2795650a"), "Real estate transaction tax" },
                    { new Guid("c0a8abcf-db11-8db8-ce81-50a11250d401"), "en", new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "Date" },
                    { new Guid("c455c684-5893-66b9-5eb0-f47858bee224"), "ar", new Guid("8fe6a9ef-ebae-ee5c-23c8-8965728b4717"), "التاريخ" },
                    { new Guid("c6e4f087-55f6-9813-1537-d3ca1ae41cb4"), "en", new Guid("ab22102c-6a28-9715-f794-c861ba7f7797"), "This payment has a tax invoice" },
                    { new Guid("cbc3d0de-8d6e-8d92-0d41-0edd92fc3eff"), "en", new Guid("931634e5-011c-0f54-ad50-eaf0a66efdcf"), "Invoice total" },
                    { new Guid("ce39e479-d2df-5488-2ca2-9b21bb9f8da4"), "en", new Guid("ff1c1202-0403-10c4-492e-00aa8aba0121"), "Purchase invoices" },
                    { new Guid("d0fe5a27-c910-a240-1021-4bc47430f116"), "ar", new Guid("b17e6037-5299-68d4-0753-f2b9db888e54"), "اذكر سبب عدم وجود فاتورة، حتى 500 حرف." },
                    { new Guid("d493159f-4ba7-6691-202b-9939403bfc4c"), "ar", new Guid("ab7fe299-7369-2a04-a9cf-b39f74bcbca3"), "سبب الخصم" },
                    { new Guid("d7034bd1-03ca-5a31-deff-faf88b85e392"), "ar", new Guid("818bce46-8040-3d01-2e9e-88569346f9e4"), "مرتجعات" },
                    { new Guid("d8a3fc6d-f1ff-e81e-20e4-cee141fd6706"), "ar", new Guid("d30afc1b-e321-d5a7-bc87-7c490c35efb0"), "أضف صورة الفاتورة أو ملف PDF قبل الإرسال." },
                    { new Guid("d994c2f3-f8af-b180-fb12-1a40fa8d64e9"), "en", new Guid("a3d53521-72fe-698b-c0b6-667e1b8dcce4"), "Payment date" },
                    { new Guid("da0048f7-95c7-bf7b-03f7-d334d3704f0b"), "en", new Guid("968a8709-f7f1-487b-c900-aaa17ba5cebf"), "Enter the invoice number, up to 50 characters." },
                    { new Guid("dc0ed562-912b-048f-98bc-13ac6eb6e2e3"), "ar", new Guid("69c7e1e2-d11a-be6d-bbcf-675f4df1a717"), "الفرق المستحق" },
                    { new Guid("dd394de8-3820-ea29-69b2-374476e406d9"), "en", new Guid("b48ded17-8de1-9362-5b28-0da9d01396f9"), "Exceptions to review" },
                    { new Guid("de436f1f-1700-1ae3-c753-59c35d561fc4"), "en", new Guid("873df19b-9da2-db9a-d9f8-437bd298855c"), "Difference" },
                    { new Guid("df7b0129-43a3-513a-fa97-56b09eacc008"), "ar", new Guid("37f9b855-2c7e-8040-08e8-6fbebb4e1840"), "ضريبة القيمة المضافة" },
                    { new Guid("e4b4ac84-d6a3-994d-cf6c-a4afda351b98"), "ar", new Guid("9d58103d-5eff-1b1f-70f7-3164f369efbc"), "الخصم" },
                    { new Guid("e6947b4d-9d20-19c5-6a58-7587aae59fa1"), "en", new Guid("1864d182-aed8-e4e4-fd56-a067661cba77"), "Declared in a later quarter" },
                    { new Guid("f0ff4b3f-82ca-45a5-7573-21163e6e6e81"), "en", new Guid("59744ae4-1def-c1c0-b1c7-a439bc91a988"), "Say why it is deducted." },
                    { new Guid("f77bbe10-14f6-5371-5406-a52050dcaa41"), "en", new Guid("2e1ade53-6e86-002e-70d0-c7e877035721"), "A negative amount is a deduction with no invoice. Turn on Without an invoice and say why." },
                    { new Guid("f8577729-6ee9-fe5a-b8eb-d9a88cde0f01"), "ar", new Guid("c153a218-ff68-1db3-bcec-66409d1e0695"), "حفظ رغم ذلك" },
                    { new Guid("fa34bf44-118e-0109-822e-87769e4a9f1c"), "ar", new Guid("c7b08d54-7f41-a0a4-8779-6d6a64aec7aa"), "هذا المصروف معتمد. الإرفاق يعيده إلى الاعتماد." },
                    { new Guid("faf7f2a7-915b-14ae-7e49-9347e241ac7c"), "en", new Guid("9e545fa3-be30-8a8b-d680-04c703b614e7"), "No VAT number" },
                    { new Guid("fc08db74-cb75-7ae5-647f-30ecafe7a528"), "ar", new Guid("d1c5e793-bb20-bc7d-8c91-a020fc9b22e3"), "النسبة (%)" },
                    { new Guid("fcba39e4-fade-72f3-405c-948e93b63078"), "ar", new Guid("6e557b3b-e956-03e3-f70f-5a43ed39e9b8"), "ضريبة الفاتورة تختلف عن الضريبة المحسوبة بأكثر من 0.01. يمكنك الحفظ رغم ذلك." },
                    { new Guid("fde3ef5f-1e6d-96ca-a54f-383e780c1566"), "en", new Guid("0248058f-e7b3-6604-d83f-abd29fb8e400"), "Tax" }
                });
        }
    }
}
