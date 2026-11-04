using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "", "projects.addUnit" },
                    { new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "", "projects.finished" },
                    { new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "", "projects.refused" },
                    { new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "", "projects.create" },
                    { new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "", "projects.readOnly" },
                    { new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "", "projects.title" },
                    { new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "", "projects.cancel" },
                    { new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "", "projects.startDate" },
                    { new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "", "projects.units" },
                    { new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "", "projects.invalidName" },
                    { new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "", "projects.unitName" },
                    { new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "", "projects.closeQuestion" },
                    { new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "", "projects.name" },
                    { new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "", "projects.running" },
                    { new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "", "projects.close" },
                    { new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "", "projects.empty" },
                    { new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "", "projects.loadFailed" },
                    { new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "", "projects.retry" },
                    { new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "", "projects.noConnection" },
                    { new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "", "projects.projectFinished" },
                    { new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "", "projects.noUnits" },
                    { new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "", "projects.new" },
                    { new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "", "settings.title" },
                    { new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "", "projects.closeExplain" },
                    { new Guid("fa5bdd50-f61f-02fc-2071-d4aeff12e8b6"), "", "projects.removeUnit" }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("06cdf1d1-ec2f-45af-2031-d78d8fd5520d"), "ar", new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "لا توجد وحدات بعد." },
                    { new Guid("0721816f-e32a-820f-bb96-5c63fd7521c7"), "ar", new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "لا توجد مشاريع بعد. أنشئ أول مشروع." },
                    { new Guid("08adf53e-6994-bfcc-5fa9-c8d334f74fd8"), "ar", new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "إلغاء" },
                    { new Guid("09e93ad6-ebbc-9c19-bcbf-4bb788b171c5"), "en", new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "It becomes read-only: no new entries or approvals, while its reports stay available. This cannot be undone." },
                    { new Guid("0e73c805-b6f4-5720-08ce-412fef5605cb"), "en", new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "Could not load the projects. Check the network and try again." },
                    { new Guid("12724a61-4b5a-0647-1228-94eefe4524bf"), "en", new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "Projects" },
                    { new Guid("12a980ae-b097-3725-cff5-32568b5fb03e"), "ar", new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "إغلاق هذا المشروع؟" },
                    { new Guid("14d60cda-88d0-1d1e-5f97-6b0ab628e492"), "ar", new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "هذا المشروع منتهٍ، فلا يمكن تغييره." },
                    { new Guid("18676f79-7399-dc0a-08f9-50b1c735c102"), "ar", new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "الوحدات" },
                    { new Guid("1aad7d09-1819-c93c-dffb-d8d49214eec8"), "ar", new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "منتهٍ" },
                    { new Guid("28971838-a830-84b5-20ad-3b5074952704"), "en", new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "Enter a name of 1 to 100 characters." },
                    { new Guid("449fcb9f-8f5a-3d13-f1c5-54e9ffb328be"), "ar", new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "إنشاء المشروع" },
                    { new Guid("46a579cf-a469-0b9b-068b-fb03907c3bf7"), "ar", new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("4c362fb6-4bd8-ffa0-3fd5-629ea30055b3"), "en", new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "Create project" },
                    { new Guid("4eb76426-4b41-58e7-8875-205425073ad0"), "en", new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"), "Cancel" },
                    { new Guid("4f563c54-300d-21dc-0483-658ec3fd4076"), "ar", new Guid("fa5bdd50-f61f-02fc-2071-d4aeff12e8b6"), "حذف الوحدة" },
                    { new Guid("5ce294ad-7754-003c-589e-13241db904ac"), "en", new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "Try again" },
                    { new Guid("63780978-6ece-9380-8ede-8e57b671d82c"), "en", new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "Settings" },
                    { new Guid("6e1e3fca-cb3a-8803-c3f4-0ac65c4e9b5d"), "ar", new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "إضافة وحدة" },
                    { new Guid("8105e699-5616-ec64-5169-6206f7046852"), "ar", new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "اسم المشروع" },
                    { new Guid("83787c0e-8f10-309b-63e0-40b3cd31c972"), "en", new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "Unit name" },
                    { new Guid("88a95bdc-3789-74b3-3d09-39aa55ad829b"), "ar", new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "تاريخ البدء" },
                    { new Guid("8d623765-3340-4981-e339-e7484d9f17bb"), "en", new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "No connection to the server. Try again." },
                    { new Guid("94951e20-563c-f5b5-277f-26c31f938577"), "en", new Guid("fa5bdd50-f61f-02fc-2071-d4aeff12e8b6"), "Remove unit" },
                    { new Guid("98ad487b-af55-b8eb-5d3e-e4724c6aaf3e"), "ar", new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "رفض الخادم هذا التغيير." },
                    { new Guid("9a7c5537-28f1-40ef-99dc-364374c6ad50"), "en", new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "Read-only" },
                    { new Guid("9b969ef2-cce0-4909-b584-4e169218bfc0"), "ar", new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"), "المشاريع" },
                    { new Guid("9d3a6b16-ec79-bde0-dedf-f0f8e1eccc28"), "ar", new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "تعذّر تحميل المشاريع. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("a24348de-03a7-5ff6-b5b8-94f8a049aa1c"), "en", new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "Units" },
                    { new Guid("a9cf36d2-cbee-1a9b-8907-2edab4f83431"), "en", new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "The server refused this change." },
                    { new Guid("aa67a7e4-b40e-9580-2506-09f7c3284e90"), "en", new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "No units yet." },
                    { new Guid("ae2b97fa-fbea-6d7b-25dc-14a69959b463"), "ar", new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "مشروع جديد" },
                    { new Guid("af44cf6f-a01a-577d-cf6b-64f7e72348cb"), "en", new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "Running" },
                    { new Guid("b03e0175-efcc-ad93-7cb5-395ae811fd64"), "en", new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "Add unit" },
                    { new Guid("b745a96c-1c16-9063-e1a4-0b9ba5460f77"), "ar", new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "اسم الوحدة" },
                    { new Guid("bfaf5ea8-514f-0617-4494-226d64ecebb4"), "en", new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "Close project" },
                    { new Guid("c0ed5273-1e9c-305e-3b72-8a5bfd47fe2e"), "ar", new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"), "الإعدادات" },
                    { new Guid("c6ec0d92-3b21-7092-6f78-42e9d80b9aa1"), "en", new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "Start date" },
                    { new Guid("ca78df77-b12f-3d6a-9afd-29c2dbf86a5b"), "en", new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "Project name" },
                    { new Guid("cf97457f-9011-3ee6-e375-8314bbc0be29"), "en", new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "New project" },
                    { new Guid("d0c4b90d-4596-ead2-f8b9-3cbd3b220ef2"), "en", new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "Close this project?" },
                    { new Guid("d10ee5f4-88f7-6fb3-3dca-042147cde652"), "ar", new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"), "جارٍ" },
                    { new Guid("d34b8d94-c323-e735-45e5-b4840f194257"), "en", new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "No projects yet. Create the first one." },
                    { new Guid("dc0ccf4e-494e-e2d0-8594-d68f23e3fb31"), "ar", new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "للقراءة فقط" },
                    { new Guid("e20ceb0f-32db-12d6-b76f-38021ed02301"), "ar", new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "إغلاق المشروع" },
                    { new Guid("ebb8cac9-5da0-39b8-4540-43a5a99fc371"), "ar", new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"), "حاول مرة أخرى" },
                    { new Guid("efd91d5a-c430-77c3-2092-fd37d7e34618"), "ar", new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("f5afafaa-3467-ef54-360e-a79c4e83155c"), "ar", new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "يصبح للقراءة فقط: لا إدخال ولا اعتماد، وتبقى تقاريره متاحة. لا يمكن التراجع عن ذلك." },
                    { new Guid("f662c899-4abb-c968-a0b8-d4a1bf4254c2"), "en", new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "This project is finished, so it cannot change." },
                    { new Guid("f7e5f447-88c0-ee39-482d-6a12bd87e639"), "en", new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "Finished" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("06cdf1d1-ec2f-45af-2031-d78d8fd5520d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0721816f-e32a-820f-bb96-5c63fd7521c7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("08adf53e-6994-bfcc-5fa9-c8d334f74fd8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("09e93ad6-ebbc-9c19-bcbf-4bb788b171c5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("0e73c805-b6f4-5720-08ce-412fef5605cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12724a61-4b5a-0647-1228-94eefe4524bf"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("12a980ae-b097-3725-cff5-32568b5fb03e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("14d60cda-88d0-1d1e-5f97-6b0ab628e492"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("18676f79-7399-dc0a-08f9-50b1c735c102"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1aad7d09-1819-c93c-dffb-d8d49214eec8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("28971838-a830-84b5-20ad-3b5074952704"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("449fcb9f-8f5a-3d13-f1c5-54e9ffb328be"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("46a579cf-a469-0b9b-068b-fb03907c3bf7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c362fb6-4bd8-ffa0-3fd5-629ea30055b3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4eb76426-4b41-58e7-8875-205425073ad0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4f563c54-300d-21dc-0483-658ec3fd4076"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5ce294ad-7754-003c-589e-13241db904ac"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("63780978-6ece-9380-8ede-8e57b671d82c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6e1e3fca-cb3a-8803-c3f4-0ac65c4e9b5d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8105e699-5616-ec64-5169-6206f7046852"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("83787c0e-8f10-309b-63e0-40b3cd31c972"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("88a95bdc-3789-74b3-3d09-39aa55ad829b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8d623765-3340-4981-e339-e7484d9f17bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("94951e20-563c-f5b5-277f-26c31f938577"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("98ad487b-af55-b8eb-5d3e-e4724c6aaf3e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9a7c5537-28f1-40ef-99dc-364374c6ad50"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9b969ef2-cce0-4909-b584-4e169218bfc0"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d3a6b16-ec79-bde0-dedf-f0f8e1eccc28"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a24348de-03a7-5ff6-b5b8-94f8a049aa1c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a9cf36d2-cbee-1a9b-8907-2edab4f83431"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("aa67a7e4-b40e-9580-2506-09f7c3284e90"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae2b97fa-fbea-6d7b-25dc-14a69959b463"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af44cf6f-a01a-577d-cf6b-64f7e72348cb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b03e0175-efcc-ad93-7cb5-395ae811fd64"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b745a96c-1c16-9063-e1a4-0b9ba5460f77"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bfaf5ea8-514f-0617-4494-226d64ecebb4"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0ed5273-1e9c-305e-3b72-8a5bfd47fe2e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c6ec0d92-3b21-7092-6f78-42e9d80b9aa1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ca78df77-b12f-3d6a-9afd-29c2dbf86a5b"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cf97457f-9011-3ee6-e375-8314bbc0be29"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d0c4b90d-4596-ead2-f8b9-3cbd3b220ef2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d10ee5f4-88f7-6fb3-3dca-042147cde652"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d34b8d94-c323-e735-45e5-b4840f194257"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc0ccf4e-494e-e2d0-8594-d68f23e3fb31"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("e20ceb0f-32db-12d6-b76f-38021ed02301"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ebb8cac9-5da0-39b8-4540-43a5a99fc371"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("efd91d5a-c430-77c3-2092-fd37d7e34618"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f5afafaa-3467-ef54-360e-a79c4e83155c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f662c899-4abb-c968-a0b8-d4a1bf4254c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f7e5f447-88c0-ee39-482d-6a12bd87e639"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("42a8d72c-ec3f-c387-5ac5-f285424f5310"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4825b12b-ea0e-a90e-b30f-27310529ae46"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4dceab17-bb05-a54a-9b67-78115174e661"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8fc41031-0fe8-d5de-4bc7-5e612d15d4ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aca7de3b-4f59-66c5-96f6-ec752a6213b0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c8675013-b514-82b4-c81b-2a801ab38051"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e4b6fd5b-c018-8a6f-cfb9-eb702bc257f8"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fa5bdd50-f61f-02fc-2071-d4aeff12e8b6"));
        }
    }
}
