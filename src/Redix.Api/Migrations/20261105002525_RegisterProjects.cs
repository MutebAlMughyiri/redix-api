using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class RegisterProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("578f9580-4389-dee4-365c-a6d94e78b230"));

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5d9e61be-da59-a993-55f5-c5013028c46f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1beea644-8d14-203f-7bfc-a76c377ad475"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3d54d177-caa7-2cf9-b250-a7faf032fa4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("86018635-2aad-43d7-91e2-9af58611ae3d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("93b4a40b-4efb-7496-4ab1-0ded8b7c25ba"));

                        migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b90f2992-b1db-a571-cd82-f1141528a015"));

                        migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "", "register.invalidName" },
                    { new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "", "register.noConnection" },
                    { new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "", "register.invalidUsername" },
                    { new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "", "projects.pending" },
                    { new Guid("46821251-c294-7141-3e30-25a81e924b85"), "", "registered.noConnection" },
                    { new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "", "register.tooManyTries" },
                    { new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "", "registered.refused" },
                    { new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "", "signIn.registerProject" },
                    { new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "", "register.refused" },
                    { new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "", "registered.empty" },
                    { new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "", "registered.loadFailed" },
                    { new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "", "register.usernameTaken" },
                    { new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "", "register.weakPassword" },
                    { new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "", "users.roleAdmin" },
                    { new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "", "register.sent" },
                    { new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "", "registered.approve" },
                    { new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "", "projects.rejected" },
                    { new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "", "registered.reject" },
                    { new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "", "registered.reason" },
                    { new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "", "registered.title" },
                    { new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "", "register.hint" },
                    { new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "", "register.invalidMobile" },
                    { new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "", "registered.invalidReason" },
                    { new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "", "registered.notPending" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d99d4ff-c3a1-f931-b2b5-eb0f28664540"),
                column: "Value",
                value: "This person also works in another project, so this cannot be changed here.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("449fcb9f-8f5a-3d13-f1c5-54e9ffb328be"),
                column: "Value",
                value: "إرسال للاعتماد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c362fb6-4bd8-ffa0-3fd5-629ea30055b3"),
                column: "Value",
                value: "Send for approval");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4decc0a4-2985-7259-41a7-955a02ace1b5"),
                column: "Value",
                value: "يعمل هذا الشخص في مشروع آخر أيضاً، لذا لا يمكن تغيير ذلك من هنا.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae2b97fa-fbea-6d7b-25dc-14a69959b463"),
                column: "Value",
                value: "تسجيل مشروع جديد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cf97457f-9011-3ee6-e375-8314bbc0be29"),
                column: "Value",
                value: "Register new project");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Scope", "SortOrder", "WordKey" },
                values: new object[] { new Guid("6a87517c-47ed-b83f-195d-c84e7a2720e0"), "System", 1, "users.roleAdmin" });

            // The system admin becomes the admin, so whoever held the role keeps it.
            migrationBuilder.Sql("UPDATE UserRoles SET RoleId = '6a87517c-47ed-b83f-195d-c84e7a2720e0' WHERE RoleId = 'e5c8a57b-dca1-df52-59f4-a0520caa9080';");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"));

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("03a02752-8052-3eb3-96e8-24ba04e8f556"), "en", new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "Register new project" },
                    { new Guid("105f2a98-a822-c10a-1097-4243746aab20"), "ar", new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("1aeac483-353c-794e-193c-2adc6092b0fd"), "ar", new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "رفض الخادم هذا التغيير." },
                    { new Guid("1c245e64-6423-331c-ed4b-b7de9730980a"), "ar", new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "سبب الرفض" },
                    { new Guid("1d22c626-af58-e2fb-f54c-3f520687f432"), "ar", new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "محاولات كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("288b3ed2-44de-4224-4401-a4fd33dd2249"), "ar", new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "تسجيل مشروع جديد" },
                    { new Guid("292e45e6-371c-fa95-2f53-c15f374eca31"), "en", new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "Admin" },
                    { new Guid("2bd0e7d9-7feb-df5a-7bf0-b2dc0647b86d"), "ar", new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "اكتب اسم مستخدم من 1 إلى 50 حرفاً." },
                    { new Guid("2c9fd444-231d-e12c-678e-12af42f77887"), "ar", new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "رفض" },
                    { new Guid("32755b57-4823-0526-ba64-bb34b002aa09"), "en", new Guid("46821251-c294-7141-3e30-25a81e924b85"), "No connection to the server. Try again." },
                    { new Guid("3669e773-c686-f408-6244-03e64f12f3f1"), "en", new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "Rejected" },
                    { new Guid("37d92a93-331b-1055-425f-dcb139479d91"), "en", new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "No connection to the server. Try again." },
                    { new Guid("39873145-6091-6ad4-ebeb-049e648bd62e"), "en", new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "Reject" },
                    { new Guid("4d4ef630-76d1-5dd7-4d87-825918355a32"), "ar", new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"), "مسؤول النظام" },
                    { new Guid("53ec6b65-33b3-1612-9b7a-60fa12921bda"), "ar", new Guid("46821251-c294-7141-3e30-25a81e924b85"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("54bdde86-da79-445c-ac1c-9ad69e73be17"), "ar", new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "اسم المستخدم هذا مستخدم. إن كان لك، سجّل الدخول وسجّل المشروع من مشاريعك." },
                    { new Guid("54beb956-04c1-7183-a384-1f4f43661ae7"), "ar", new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "اكتب السبب، في 500 حرف على الأكثر." },
                    { new Guid("58102dec-646d-a5fa-8874-5fa6a4f7f70c"), "en", new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "The project opens once the admin approves it. You will be its project manager." },
                    { new Guid("5a0f7ab0-36c6-9c34-44b5-ec5cf2524ab6"), "en", new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "Enter a name of 1 to 100 characters." },
                    { new Guid("5f61238f-f858-0bbb-d858-f34a1ddd6f74"), "en", new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "Too many tries. Try again later." },
                    { new Guid("684108bb-42ce-dcff-5934-43e67dfaa0c2"), "ar", new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "رفض الخادم هذا التغيير." },
                    { new Guid("6a726cfd-e081-1803-61b4-784a9d2629a9"), "ar", new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "سبق البت في هذا المشروع." },
                    { new Guid("6fda4d74-06d1-bc92-5d0f-b1630324fcfb"), "ar", new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "يُفتح المشروع بعد اعتماد مسؤول النظام له، وستكون مديره." },
                    { new Guid("7430aaa3-a28b-adbc-b8d4-80b5ed10dd27"), "ar", new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "مرفوض" },
                    { new Guid("7a888b79-0b27-2870-0879-9a584a4670d8"), "ar", new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "استخدم 8 أحرف على الأقل." },
                    { new Guid("7dfc43b1-3ff5-6f98-03c9-ed46275b8798"), "en", new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "Approve" },
                    { new Guid("8ea23b2c-fee7-13c6-fe53-ded4229378bb"), "ar", new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "المشاريع المسجّلة" },
                    { new Guid("905893a9-de3c-66e5-f0e7-d7e758856e4e"), "en", new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "The server refused this change." },
                    { new Guid("928c8257-9d8d-84f6-e981-a7b5142b80ce"), "en", new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "Why it is rejected" },
                    { new Guid("9794af56-343f-ddd7-463d-8091f5855175"), "en", new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "Write why, in up to 500 characters." },
                    { new Guid("9baea722-2831-1387-5e7d-fdf507b22847"), "en", new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "Use at least 8 characters." },
                    { new Guid("9d53dc65-7117-df0f-ba60-09d4c1f66aba"), "ar", new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "بانتظار الاعتماد" },
                    { new Guid("a2dffc10-7bb3-8b28-3ca8-fd28c16f4796"), "en", new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "Enter a username of 1 to 50 characters." },
                    { new Guid("a576bf4f-43cb-3ea8-fffb-e0e9613e5817"), "en", new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "Waiting for approval" },
                    { new Guid("abb8b122-d8ac-fb98-a28a-69b5e98870a3"), "en", new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "This username is taken. If it is yours, sign in and register from your projects." },
                    { new Guid("af4f8727-67f9-44b2-06fa-515f686c3108"), "en", new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "Sent for approval. Sign in to follow it." },
                    { new Guid("afe970f2-f153-8dc1-9469-6dce6a9350ef"), "en", new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "This project was already decided." },
                    { new Guid("b3b28dfb-1d6d-fb8d-60cc-2bdd682b8a12"), "ar", new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "اعتماد" },
                    { new Guid("b65204a1-3ffe-2b75-5944-24abbcd9f56f"), "ar", new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("bc790e39-1fe5-7142-d642-a78d632d8aa5"), "en", new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "Could not load the registered projects. Check the network and try again." },
                    { new Guid("bf5db913-233c-6507-301a-a8f3c720810e"), "ar", new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "أُرسل للاعتماد. سجّل الدخول لمتابعته." },
                    { new Guid("c0b7c618-9586-c57d-b1e5-c456603972ca"), "en", new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "The server refused this change." },
                    { new Guid("c3af4e7e-e7c1-612a-1b41-dc81586582d8"), "ar", new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "تعذّر تحميل المشاريع المسجّلة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("cfaf598e-7e06-247e-fd2f-782f2cad5df6"), "en", new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "Registered projects" },
                    { new Guid("d9605ec2-dce2-3c02-f021-931b25374d79"), "ar", new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "لا مشاريع بانتظار الاعتماد." },
                    { new Guid("dc93e950-2e06-1e46-381c-3782e8f8084c"), "en", new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "Enter a mobile of up to 20 characters." },
                    { new Guid("ddc946d7-c8dd-f2fd-49b3-d9da769ad66c"), "en", new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "No projects are waiting." },
                    { new Guid("f07e3552-07f3-0f88-e991-31daabc44809"), "ar", new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "اكتب رقم جوال من 20 حرفاً على الأكثر." }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[] { new Guid("229fb904-b3ae-c168-60c9-52379dc1ac5f"), "ApproveProjects", new Guid("6a87517c-47ed-b83f-195d-c84e7a2720e0") });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("229fb904-b3ae-c168-60c9-52379dc1ac5f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("03a02752-8052-3eb3-96e8-24ba04e8f556"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("105f2a98-a822-c10a-1097-4243746aab20"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1aeac483-353c-794e-193c-2adc6092b0fd"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1c245e64-6423-331c-ed4b-b7de9730980a"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d22c626-af58-e2fb-f54c-3f520687f432"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("288b3ed2-44de-4224-4401-a4fd33dd2249"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("292e45e6-371c-fa95-2f53-c15f374eca31"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2bd0e7d9-7feb-df5a-7bf0-b2dc0647b86d"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("2c9fd444-231d-e12c-678e-12af42f77887"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("32755b57-4823-0526-ba64-bb34b002aa09"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("3669e773-c686-f408-6244-03e64f12f3f1"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("37d92a93-331b-1055-425f-dcb139479d91"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("39873145-6091-6ad4-ebeb-049e648bd62e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4d4ef630-76d1-5dd7-4d87-825918355a32"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("53ec6b65-33b3-1612-9b7a-60fa12921bda"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54bdde86-da79-445c-ac1c-9ad69e73be17"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("54beb956-04c1-7183-a384-1f4f43661ae7"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("58102dec-646d-a5fa-8874-5fa6a4f7f70c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5a0f7ab0-36c6-9c34-44b5-ec5cf2524ab6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("5f61238f-f858-0bbb-d858-f34a1ddd6f74"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("684108bb-42ce-dcff-5934-43e67dfaa0c2"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6a726cfd-e081-1803-61b4-784a9d2629a9"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("6fda4d74-06d1-bc92-5d0f-b1630324fcfb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7430aaa3-a28b-adbc-b8d4-80b5ed10dd27"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7a888b79-0b27-2870-0879-9a584a4670d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("7dfc43b1-3ff5-6f98-03c9-ed46275b8798"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("8ea23b2c-fee7-13c6-fe53-ded4229378bb"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("905893a9-de3c-66e5-f0e7-d7e758856e4e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("928c8257-9d8d-84f6-e981-a7b5142b80ce"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9794af56-343f-ddd7-463d-8091f5855175"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9baea722-2831-1387-5e7d-fdf507b22847"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("9d53dc65-7117-df0f-ba60-09d4c1f66aba"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a2dffc10-7bb3-8b28-3ca8-fd28c16f4796"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("a576bf4f-43cb-3ea8-fffb-e0e9613e5817"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("abb8b122-d8ac-fb98-a28a-69b5e98870a3"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("af4f8727-67f9-44b2-06fa-515f686c3108"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("afe970f2-f153-8dc1-9469-6dce6a9350ef"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b3b28dfb-1d6d-fb8d-60cc-2bdd682b8a12"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("b65204a1-3ffe-2b75-5944-24abbcd9f56f"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bc790e39-1fe5-7142-d642-a78d632d8aa5"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("bf5db913-233c-6507-301a-a8f3c720810e"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c0b7c618-9586-c57d-b1e5-c456603972ca"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("c3af4e7e-e7c1-612a-1b41-dc81586582d8"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cfaf598e-7e06-247e-fd2f-782f2cad5df6"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("d9605ec2-dce2-3c02-f021-931b25374d79"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("dc93e950-2e06-1e46-381c-3782e8f8084c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ddc946d7-c8dd-f2fd-49b3-d9da769ad66c"));

            migrationBuilder.DeleteData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("f07e3552-07f3-0f88-e991-31daabc44809"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6a87517c-47ed-b83f-195d-c84e7a2720e0"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("46821251-c294-7141-3e30-25a81e924b85"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"));

            migrationBuilder.DeleteData(
                table: "TranslationKeys",
                keyColumn: "Id",
                keyValue: new Guid("aba96efe-224a-d326-a2f1-3b58e392f70d"));

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Projects");

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "", "users.roleSystem" },
                    { new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "", "users.noProjects" }
                });

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("1d99d4ff-c3a1-f931-b2b5-eb0f28664540"),
                column: "Value",
                value: "This person also works in another project, so the system admin does this.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("449fcb9f-8f5a-3d13-f1c5-54e9ffb328be"),
                column: "Value",
                value: "إنشاء المشروع");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4c362fb6-4bd8-ffa0-3fd5-629ea30055b3"),
                column: "Value",
                value: "Create project");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("4decc0a4-2985-7259-41a7-955a02ace1b5"),
                column: "Value",
                value: "يعمل هذا الشخص في مشروع آخر أيضاً، لذا يتولى ذلك مسؤول النظام.");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("ae2b97fa-fbea-6d7b-25dc-14a69959b463"),
                column: "Value",
                value: "مشروع جديد");

            migrationBuilder.UpdateData(
                table: "Translations",
                keyColumn: "Id",
                keyValue: new Guid("cf97457f-9011-3ee6-e375-8314bbc0be29"),
                column: "Value",
                value: "New project");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Scope", "SortOrder", "WordKey" },
                values: new object[] { new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080"), "System", 1, "users.roleSystem" });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("1beea644-8d14-203f-7bfc-a76c377ad475"), "ar", new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "مسؤول النظام" },
                    { new Guid("3d54d177-caa7-2cf9-b250-a7faf032fa4e"), "en", new Guid("1d0bfe7c-785a-3170-264f-a825aa706b7a"), "System admin" },
                    { new Guid("86018635-2aad-43d7-91e2-9af58611ae3d"), "ar", new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "لا مشاريع بعد" },
                    { new Guid("93b4a40b-4efb-7496-4ab1-0ded8b7c25ba"), "en", new Guid("b90f2992-b1db-a571-cd82-f1141528a015"), "No projects yet" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("578f9580-4389-dee4-365c-a6d94e78b230"), "ManageLogins", new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080") },
                    { new Guid("5d9e61be-da59-a993-55f5-c5013028c46f"), "StartProject", new Guid("e5c8a57b-dca1-df52-59f4-a0520caa9080") }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_Permission",
                table: "RolePermissions",
                sql: "[Permission] IN ('StartProject', 'ManageLogins', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'ManageProject')");
        }
    }
}
