using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LK_AccountType",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_AccountType", x => x.Id);
                    table.UniqueConstraint("AK_LK_AccountType_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_CapitalMovementType",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_CapitalMovementType", x => x.Id);
                    table.UniqueConstraint("AK_LK_CapitalMovementType_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_ColumnDataType",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_ColumnDataType", x => x.Id);
                    table.UniqueConstraint("AK_LK_ColumnDataType_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_EntryAttachmentRole",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_EntryAttachmentRole", x => x.Id);
                    table.UniqueConstraint("AK_LK_EntryAttachmentRole_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_EntryDecisionAction",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_EntryDecisionAction", x => x.Id);
                    table.UniqueConstraint("AK_LK_EntryDecisionAction_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_EntryKind",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_EntryKind", x => x.Id);
                    table.UniqueConstraint("AK_LK_EntryKind_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_EntryStatus",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_EntryStatus", x => x.Id);
                    table.UniqueConstraint("AK_LK_EntryStatus_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_HistoryAction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_HistoryAction", x => x.Id);
                    table.UniqueConstraint("AK_LK_HistoryAction_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_ItemType",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_ItemType", x => x.Id);
                    table.UniqueConstraint("AK_LK_ItemType_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_NotificationKind",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_NotificationKind", x => x.Id);
                    table.UniqueConstraint("AK_LK_NotificationKind_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_ProjectStatus",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_ProjectStatus", x => x.Id);
                    table.UniqueConstraint("AK_LK_ProjectStatus_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "LK_SupplierStatus",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LK_SupplierStatus", x => x.Id);
                    table.UniqueConstraint("AK_LK_SupplierStatus_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "TranslationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedSignIns = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    StructureSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_LK_ProjectStatus_Status",
                        column: x => x.Status,
                        principalTable: "LK_ProjectStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TranslationKeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_TranslationKeys_TranslationKeyId",
                        column: x => x.TranslationKeyId,
                        principalTable: "TranslationKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    RefreshExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    UsesBiometrics = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TableName = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Before = table.Column<string>(type: "json", nullable: false),
                    After = table.Column<string>(type: "json", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                    table.ForeignKey(
                        name: "FK_History_LK_HistoryAction_Action",
                        column: x => x.Action,
                        principalTable: "LK_HistoryAction",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_History_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                        name: "FK_Notifications_LK_NotificationKind_Kind",
                        column: x => x.Kind,
                        principalTable: "LK_NotificationKind",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPermissions", x => x.Id);
                    table.CheckConstraint("CK_SystemPermissions_Permission", "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney')");
                    table.ForeignKey(
                        name: "FK_SystemPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_LK_AccountType_Type",
                        column: x => x.Type,
                        principalTable: "LK_AccountType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoragePath = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SizeBytes = table.Column<int>(type: "int", nullable: false),
                    Sha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
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
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DefaultColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCore = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ShowInReports = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultColumns_LK_ColumnDataType_DataType",
                        column: x => x.DataType,
                        principalTable: "LK_ColumnDataType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DefaultColumns_LK_ItemType_ItemType",
                        column: x => x.ItemType,
                        principalTable: "LK_ItemType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DefaultColumns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "PeriodicReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpensesTotal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Snapshot = table.Column<string>(type: "json", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodicReports_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPermissions", x => x.Id);
                    table.CheckConstraint("CK_ProjectPermissions_Permission", "[Permission] IN ('ApproveProjects', 'ManageUsers', 'ReadProject', 'SeeAllCapital', 'SeeChosenCapital', 'ManageProject', 'ManageStructure', 'ManageSettings', 'AddSupplier', 'ChargeAnyAccount', 'RecordEntries', 'ManageAnyEntry', 'DeleteEntries', 'RemoveApprovedEntries', 'AttachFiles', 'ApproveEntries', 'MoveMoney') AND [Permission] <> 'ApproveProjects'");
                    table.ForeignKey(
                        name: "FK_ProjectPermissions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRoleNames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoleNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRoleNames_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoleNames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reconciliations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reconciliations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VatNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsGeneralCash = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_LK_SupplierStatus_Status",
                        column: x => x.Status,
                        principalTable: "LK_SupplierStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDefaultProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefaultProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDefaultProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDefaultProjects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccountCustodians",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountCustodians", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_AccountCustodians_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountCustodians_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_LK_EntryKind_Kind",
                        column: x => x.Kind,
                        principalTable: "LK_EntryKind",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_LK_EntryStatus_Status",
                        column: x => x.Status,
                        principalTable: "LK_EntryStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ShowsTablePercentages = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_LK_ItemType_Type",
                        column: x => x.Type,
                        principalTable: "LK_ItemType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChosenPartners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChosenPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChosenPartners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationBalance",
                columns: table => new
                {
                    ReconciliationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualBalance = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationBalance", x => new { x.ReconciliationId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_ReconciliationBalance_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationBalance_Reconciliations_ReconciliationId",
                        column: x => x.ReconciliationId,
                        principalTable: "Reconciliations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapitalMovementDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapitalMovementDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_CapitalMovementDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapitalMovementDetails_LK_CapitalMovementType_Type",
                        column: x => x.Type,
                        principalTable: "LK_CapitalMovementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapitalMovementDetails_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryAttachments",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryAttachments", x => new { x.EntryId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_EntryAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttachments_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryAttachments_LK_EntryAttachmentRole_Role",
                        column: x => x.Role,
                        principalTable: "LK_EntryAttachmentRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryDecisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryDecisions_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryDecisions_LK_EntryDecisionAction_Action",
                        column: x => x.Action,
                        principalTable: "LK_EntryDecisionAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryDecisions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_Invoices_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    IsOpenValue = table.Column<bool>(type: "bit", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<byte>(type: "tinyint", nullable: false),
                    IsCore = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ShowInReports = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemColumns_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemColumns_LK_ColumnDataType_DataType",
                        column: x => x.DataType,
                        principalTable: "LK_ColumnDataType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subgroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subgroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subgroups_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDefaultItems",
                columns: table => new
                {
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDefaultItems", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_SupplierDefaultItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierDefaultItems_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => new { x.ContractId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorPaymentDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOutsideContract = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPaymentDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubgroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomValues = table.Column<string>(type: "json", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Subgroups_SubgroupId",
                        column: x => x.SubgroupId,
                        principalTable: "Subgroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevenueDetails",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubgroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueDetails", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_RevenueDetails_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevenueDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevenueDetails_Subgroups_SubgroupId",
                        column: x => x.SubgroupId,
                        principalTable: "Subgroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ConfigValues",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[,]
                {
                    { new Guid("9399f72c-0f36-d3d5-fd62-b9f982d68edf"), "fileTimeoutSeconds", "120" },
                    { new Guid("961162d7-c9c8-5b82-c642-4c5a6cdbe393"), "requestTimeoutSeconds", "10" },
                    { new Guid("9bb147da-c022-65eb-9451-57b6a512e0c7"), "vatRatePercent", "15" }
                });

            migrationBuilder.InsertData(
                table: "LK_AccountType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Bank" },
                    { (byte)1, "CashCustody" }
                });

            migrationBuilder.InsertData(
                table: "LK_CapitalMovementType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Contribution" },
                    { (byte)1, "Withdrawal" }
                });

            migrationBuilder.InsertData(
                table: "LK_ColumnDataType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Text" },
                    { (byte)1, "Number" },
                    { (byte)2, "Amount" },
                    { (byte)3, "Date" },
                    { (byte)4, "List" },
                    { (byte)5, "File" }
                });

            migrationBuilder.InsertData(
                table: "LK_EntryAttachmentRole",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Invoice" },
                    { (byte)1, "Receipt" },
                    { (byte)2, "BankReceipt" },
                    { (byte)3, "Other" }
                });

            migrationBuilder.InsertData(
                table: "LK_EntryDecisionAction",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Submit" },
                    { (byte)1, "Approve" },
                    { (byte)2, "Reject" }
                });

            migrationBuilder.InsertData(
                table: "LK_EntryKind",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Purchase" },
                    { (byte)1, "ContractorPayment" },
                    { (byte)2, "CapitalIn" },
                    { (byte)3, "CapitalOut" },
                    { (byte)4, "Revenue" }
                });

            migrationBuilder.InsertData(
                table: "LK_EntryStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Draft" },
                    { (byte)1, "PendingApproval" },
                    { (byte)2, "Approved" },
                    { (byte)3, "Rejected" },
                    { (byte)4, "Removed" }
                });

            migrationBuilder.InsertData(
                table: "LK_HistoryAction",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "SignIn" },
                    { 1, "SignOut" },
                    { 2, "SignInFailed" },
                    { 3, "AccountLocked" },
                    { 4, "ChangePassword" },
                    { 5, "Create" },
                    { 6, "Update" },
                    { 7, "ResetPassword" },
                    { 8, "Disable" },
                    { 9, "Enable" },
                    { 10, "AssignProject" },
                    { 11, "RemoveProject" },
                    { 12, "RegisterProject" },
                    { 13, "ApproveProject" },
                    { 14, "Approve" },
                    { 15, "Reject" },
                    { 16, "RejectProject" },
                    { 17, "TurnOnBiometrics" },
                    { 18, "TurnOffBiometrics" },
                    { 19, "Submit" },
                    { 20, "Attach" },
                    { 21, "CloseProject" },
                    { 22, "Delete" },
                    { 23, "Restore" },
                    { 24, "Reverse" }
                });

            migrationBuilder.InsertData(
                table: "LK_ItemType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Purchases" },
                    { (byte)1, "ContractorPayments" },
                    { (byte)2, "Both" },
                    { (byte)3, "Sale" },
                    { (byte)4, "VatRefund" },
                    { (byte)5, "OtherRevenue" },
                    { (byte)6, "Capital" }
                });

            migrationBuilder.InsertData(
                table: "LK_NotificationKind",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "EntrySubmitted" },
                    { (byte)1, "EntryApproved" },
                    { (byte)2, "EntryRejected" },
                    { (byte)3, "ProjectApproved" },
                    { (byte)4, "ProjectRejected" }
                });

            migrationBuilder.InsertData(
                table: "LK_ProjectStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Active" },
                    { (byte)1, "Finished" },
                    { (byte)2, "PendingApproval" },
                    { (byte)3, "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "LK_SupplierStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)0, "Approved" },
                    { (byte)1, "PendingApproval" }
                });

            migrationBuilder.InsertData(
                table: "TranslationKeys",
                columns: new[] { "Id", "Description", "Key" },
                values: new object[,]
                {
                    { new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "", "vat.q4" },
                    { new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "", "projects.addUnit" },
                    { new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "", "permissions.manageStructureHint" },
                    { new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "", "reports.fileFailed" },
                    { new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "", "notifications.projectRejected" },
                    { new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "", "home.hideDetails" },
                    { new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "", "categories.typeOtherRevenue" },
                    { new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "", "approvals.reasonDuplicate" },
                    { new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "", "signIn.username" },
                    { new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "", "suppliers.unknownItem" },
                    { new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "", "categories.disable" },
                    { new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "", "reports.refused" },
                    { new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "", "reports.openValue" },
                    { new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "", "partners.new" },
                    { new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "", "categories.itemHasPartners" },
                    { new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "", "changePassword.accountLocked" },
                    { new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "", "projectInfo.descriptionHint" },
                    { new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "", "signIn.noConnection" },
                    { new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "", "projects.unknownSource" },
                    { new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "", "partners.delete" },
                    { new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "", "reports.grandTotal" },
                    { new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "", "permissions.manageAnyEntry" },
                    { new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "", "projects.finished" },
                    { new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "", "expenses.projectFinished" },
                    { new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "", "columns.hint" },
                    { new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "", "documents.projectFinished" },
                    { new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "", "reports.itemTotal" },
                    { new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "", "users.edit" },
                    { new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "", "entries.pendingApproval" },
                    { new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "", "reports.issueQuestion" },
                    { new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "", "myAccount.signOut" },
                    { new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "", "categories.typeContractorPayments" },
                    { new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "", "categories.capitalItemExists" },
                    { new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "", "reports.print" },
                    { new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "", "expenses.loadFailed" },
                    { new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "", "register.invalidName" },
                    { new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "", "documents.save" },
                    { new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "", "categories.disabled" },
                    { new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "", "columns.thisItem" },
                    { new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "", "approvals.reasonHint" },
                    { new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "", "expenses.saveDraft" },
                    { new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "", "reports.surplus" },
                    { new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "", "summary.surplus" },
                    { new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "", "permissions.moveMoney" },
                    { new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "", "expenses.notYourEntry" },
                    { new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "", "projects.refused" },
                    { new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "", "accounts.save" },
                    { new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "", "contracts.unknownSupplier" },
                    { new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "", "columns.refused" },
                    { new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "", "columns.invalidOrder" },
                    { new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "", "attachments.refused" },
                    { new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "", "suppliers.holder" },
                    { new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "", "expenses.unknownPaymentMethod" },
                    { new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "", "entries.tableSearchHint" },
                    { new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "", "biometrics.failed" },
                    { new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "", "signIn.password" },
                    { new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "", "reconciliations.surplus" },
                    { new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "", "suppliers.noMatch" },
                    { new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "", "users.nameHint" },
                    { new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "", "capital.withdrawal" },
                    { new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "", "register.noConnection" },
                    { new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "", "paymentMethods.paymentMethodUsed" },
                    { new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "", "columns.projectFinished" },
                    { new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "", "suppliers.useExisting" },
                    { new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "", "entryShare.item" },
                    { new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "", "entries.entryNotSubmittable" },
                    { new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "", "expenses.requiredColumnMissing" },
                    { new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "", "categories.empty" },
                    { new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "", "documents.loadFailed" },
                    { new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "", "users.resetExplain" },
                    { new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "", "reports.outsideContract" },
                    { new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "", "entries.remove" },
                    { new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "", "capital.description" },
                    { new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "", "users.weakPassword" },
                    { new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "", "accounts.name" },
                    { new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "", "permissions.manageStructure" },
                    { new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "", "users.temporaryPasswordHint" },
                    { new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "", "offline.autoSend" },
                    { new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "", "transfers.amount" },
                    { new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "", "categories.itemType" },
                    { new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "", "contracts.unknownAttachment" },
                    { new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "", "sharedSettings.entriesShownHint" },
                    { new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "", "columns.choices" },
                    { new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "", "permissions.attachFiles" },
                    { new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "", "accounts.reconcile" },
                    { new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "", "reconciliations.invalidAmount" },
                    { new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "", "changePassword.invalidCredentials" },
                    { new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "", "accounts.typeCashCustody" },
                    { new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "", "register.invalidUsername" },
                    { new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "", "notifications.entryApproved" },
                    { new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "", "reports.noConnection" },
                    { new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "", "users.disableQuestion" },
                    { new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "", "permissions.mine" },
                    { new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "", "contracts.title" },
                    { new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "", "changePassword.mismatch" },
                    { new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "", "summary.completion" },
                    { new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "", "partners.save" },
                    { new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "", "share.supplierDiffers" },
                    { new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "", "history.partner" },
                    { new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "", "users.invalidPermission" },
                    { new Guid("21d95887-8bcc-8670-37f5-426441d6e91a"), "", "expenses.invalidDate" },
                    { new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "", "approvals.noConnection" },
                    { new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "", "suppliers.enable" },
                    { new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "", "contracts.invalidNotes" },
                    { new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "", "history.accountEnabled" },
                    { new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "", "history.loadFailed" },
                    { new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "", "projects.create" },
                    { new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "", "sharedSettings.defaultProject" },
                    { new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "", "approvals.enteredBy" },
                    { new Guid("27d5e8ae-ac71-1977-0de4-6b4fee3d166e"), "", "app.title" },
                    { new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "", "changePassword.hint" },
                    { new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "", "categories.sideRevenue" },
                    { new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "", "projects.readOnly" },
                    { new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "", "approvals.approve" },
                    { new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "", "partners.title" },
                    { new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "", "biometrics.promptCancel" },
                    { new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "", "columns.new" },
                    { new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "", "contracts.itemNotOffered" },
                    { new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "", "reports.description" },
                    { new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "", "expenses.deductionNeedsNotes" },
                    { new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "", "entries.clearFilters" },
                    { new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "", "biometrics.usePassword" },
                    { new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "", "transfers.invalidNotes" },
                    { new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "", "entries.table" },
                    { new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "", "suppliers.new" },
                    { new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "", "attachments.fileEmpty" },
                    { new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "", "entryShare.partner" },
                    { new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "", "suppliers.invalidName" },
                    { new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "", "sharedSettings.switchProject" },
                    { new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "", "approvals.loadFailed" },
                    { new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "", "categories.itemTakesNoPurchases" },
                    { new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "", "reconciliations.actualTotal" },
                    { new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "", "expenses.qrUnreadable" },
                    { new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "", "categories.tablesHint" },
                    { new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "", "reports.pdf" },
                    { new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "", "categories.typeCapital" },
                    { new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "", "documents.fileHint" },
                    { new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "", "reports.issued" },
                    { new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "", "expenses.product" },
                    { new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "", "partners.unknownUser" },
                    { new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "", "entries.entryWaitingForDecision" },
                    { new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "", "accounts.riyal" },
                    { new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "", "share.loadFailed" },
                    { new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "", "suppliers.refused" },
                    { new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "", "notifications.refused" },
                    { new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "", "summary.surplusWithoutSales" },
                    { new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "", "home.showDetails" },
                    { new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "", "approvals.title" },
                    { new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "", "categories.rename" },
                    { new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "", "signIn.refused" },
                    { new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "", "history.accountDeleted" },
                    { new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "", "users.disable" },
                    { new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "", "entries.notYourEntry" },
                    { new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "", "paymentMethods.enable" },
                    { new Guid("350662dd-a114-636f-e070-06a358c487b9"), "", "categories.tables" },
                    { new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "", "entryShare.accountShort" },
                    { new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "", "sharedSettings.defaultProjectHint" },
                    { new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "", "suppliers.empty" },
                    { new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "", "share.notYourEntry" },
                    { new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "", "expenses.qrSupplierMissing" },
                    { new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "", "expenses.refused" },
                    { new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "", "categories.items" },
                    { new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "", "contractorPayments.outsideExplain" },
                    { new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "", "documents.add" },
                    { new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "", "expenses.sent" },
                    { new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "", "columns.typeList" },
                    { new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "", "projects.pending" },
                    { new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "", "contracts.overrun" },
                    { new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "", "users.otherProject" },
                    { new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "", "reports.invoiceNumber" },
                    { new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "", "columns.applyExplain" },
                    { new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "", "partners.invalidName" },
                    { new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "", "revenue.total" },
                    { new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "", "approvals.quickReason" },
                    { new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "", "share.noProject" },
                    { new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "", "columns.showInReports" },
                    { new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "", "reports.share" },
                    { new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "", "partners.noLogin" },
                    { new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "", "columns.revenueTakesNoColumns" },
                    { new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "", "transfers.insufficientBalance" },
                    { new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "", "expenses.unknownAttachment" },
                    { new Guid("3d391363-227c-a3d1-1197-343359825878"), "", "contracts.enable" },
                    { new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "", "revenue.date" },
                    { new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "", "expenses.send" },
                    { new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "", "notifications.loadFailed" },
                    { new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "", "transfers.to" },
                    { new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "", "contracts.afterApproval" },
                    { new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "", "reports.excel" },
                    { new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "", "history.transfer" },
                    { new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "", "columns.typeNumber" },
                    { new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "", "revenue.loadFailed" },
                    { new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "", "categories.refused" },
                    { new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "", "history.approvedBy" },
                    { new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "", "expenses.notes" },
                    { new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "", "columns.applyToAll" },
                    { new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "", "contracts.invalidValue" },
                    { new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "", "reconciliations.never" },
                    { new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "", "suppliers.vatNumber" },
                    { new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "", "expenses.invoiceAlreadyAttached" },
                    { new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "", "subgroups.new" },
                    { new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "", "contracts.invalidUnitPrice" },
                    { new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "", "categories.subgroupName" },
                    { new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "", "approvals.reject" },
                    { new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "", "documents.fileTooLarge" },
                    { new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "", "vat.title" },
                    { new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "", "notifications.entrySubmitted" },
                    { new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "", "reports.current" },
                    { new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "", "reports.downloaded" },
                    { new Guid("46821251-c294-7141-3e30-25a81e924b85"), "", "registered.noConnection" },
                    { new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "", "users.unknownUser" },
                    { new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "", "users.new" },
                    { new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "", "accounts.refused" },
                    { new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "", "reports.reportNumber" },
                    { new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "", "share.unknownAttachment" },
                    { new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "", "projects.defaultSet" },
                    { new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "", "accounts.edit" },
                    { new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "", "partners.deleted" },
                    { new Guid("49918492-7c51-8293-228b-6515961524d9"), "", "entries.reasonRequired" },
                    { new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "", "history.nameWas" },
                    { new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "", "approvals.reasonRequired" },
                    { new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "", "permissions.seeChosenCapitalHint" },
                    { new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "", "accounts.noConnection" },
                    { new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "", "categories.invalidOrder" },
                    { new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "", "projects.startDate" },
                    { new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "", "reconciliations.unknownAccount" },
                    { new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "", "projects.units" },
                    { new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "", "register.tooManyTries" },
                    { new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "", "attachments.fileTooLarge" },
                    { new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "", "approvals.showAll" },
                    { new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "", "approvals.reasonAmount" },
                    { new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "", "reports.noneIssued" },
                    { new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "", "projects.invalidName" },
                    { new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "", "users.add" },
                    { new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "", "reports.title" },
                    { new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "", "projects.unitName" },
                    { new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "", "expenses.invalidQuantity" },
                    { new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "", "share.entryNotAttachable" },
                    { new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "", "documents.group" },
                    { new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "", "suppliers.pendingHint" },
                    { new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "", "permissions.attachFilesHint" },
                    { new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "", "entryShare.supplier" },
                    { new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "", "expenses.date" },
                    { new Guid("501441ed-9b5d-2cd1-5b0b-f9429cc42810"), "", "categories.tableCapital" },
                    { new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "", "reports.periodic" },
                    { new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "", "suppliers.loadFailed" },
                    { new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "", "columns.typeText" },
                    { new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "", "signIn.submit" },
                    { new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "", "share.project" },
                    { new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "", "paymentMethods.title" },
                    { new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "", "paymentMethods.disabled" },
                    { new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "", "summary.pendingHint" },
                    { new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "", "signIn.hint" },
                    { new Guid("531379ae-8cde-2467-8484-33a498b15916"), "", "changePassword.new" },
                    { new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "", "entries.draft" },
                    { new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "", "columns.template" },
                    { new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "", "columns.applyQuestion" },
                    { new Guid("53945eef-ca15-df6a-f8d8-901efa608b7c"), "", "documents.download" },
                    { new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "", "categories.typeBoth" },
                    { new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "", "permissions.count" },
                    { new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "", "users.invalidMobile" },
                    { new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "", "attachments.unreadable" },
                    { new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "", "offline.empty" },
                    { new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "", "biometrics.signIn" },
                    { new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "", "categories.addItem" },
                    { new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "", "paymentMethods.empty" },
                    { new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "", "projects.active" },
                    { new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "", "transfers.from" },
                    { new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "", "contracts.pending" },
                    { new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "", "expenses.account" },
                    { new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "", "columns.typeDate" },
                    { new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "", "paymentMethods.refused" },
                    { new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "", "accounts.banks" },
                    { new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "", "entries.showMore" },
                    { new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "", "columns.custom" },
                    { new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "", "suppliers.edit" },
                    { new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "", "partners.noConnection" },
                    { new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "", "expenses.paymentMethod" },
                    { new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "", "accounts.accountInUse" },
                    { new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "", "contracts.filterAll" },
                    { new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "", "expenses.creditNoteExplain" },
                    { new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "", "documents.refused" },
                    { new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "", "entries.rejected" },
                    { new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "", "expenses.supplier" },
                    { new Guid("5d7d57cb-582d-ae44-3442-32006955b507"), "", "expenses.unknownTable" },
                    { new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "", "partners.userTaken" },
                    { new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "", "users.temporaryPassword" },
                    { new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "", "expenses.creditNote" },
                    { new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "", "contractorPayments.receipt" },
                    { new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "", "share.noConnection" },
                    { new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "", "users.addExisting" },
                    { new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "", "entries.noConnection" },
                    { new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "", "capital.receiptHint" },
                    { new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "", "expenses.noConnection" },
                    { new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "", "contracts.openValue" },
                    { new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "", "paymentMethods.noConnection" },
                    { new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "", "changePassword.refused" },
                    { new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "", "suppliers.invalidVatNumber" },
                    { new Guid("627e20df-8451-b393-4dca-65e11f806396"), "", "expenses.item" },
                    { new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "", "share.newExpense" },
                    { new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "", "reconciliations.loadFailed" },
                    { new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "", "expenses.saved" },
                    { new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "", "history.entryApproved" },
                    { new Guid("666082ac-4221-7539-3047-ab60621f529c"), "", "history.more" },
                    { new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "", "entryShare.author" },
                    { new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "", "users.enable" },
                    { new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "", "projects.defaultProject" },
                    { new Guid("68500f85-3483-5951-a047-618c73bc9477"), "", "documents.title" },
                    { new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "", "offline.withFile" },
                    { new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "", "permissions.removeApprovedEntriesHint" },
                    { new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "", "history.account" },
                    { new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "", "history.capitalOut" },
                    { new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "", "vat.h1" },
                    { new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "", "entries.reason" },
                    { new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "", "transfers.noConnection" },
                    { new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "", "entries.entryApproved" },
                    { new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "", "reconciliations.books" },
                    { new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "", "users.lastManager" },
                    { new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "", "share.title" },
                    { new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "", "registered.refused" },
                    { new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "", "contracts.new" },
                    { new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "", "changePassword.weakPassword" },
                    { new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "", "reports.quantity" },
                    { new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "", "signIn.registerProject" },
                    { new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "", "permissions.addSupplier" },
                    { new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "", "columns.empty" },
                    { new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "", "documents.file" },
                    { new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "", "permissions.seeAllCapital" },
                    { new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "", "history.entryOut" },
                    { new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "", "columns.templateOnly" },
                    { new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "", "accounts.type" },
                    { new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "", "approvals.empty" },
                    { new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "", "projects.closeQuestion" },
                    { new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "", "history.accountChanged" },
                    { new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "", "suppliers.save" },
                    { new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "", "entries.refused" },
                    { new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "", "permissions.deleteEntries" },
                    { new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "", "transfers.date" },
                    { new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "", "permissions.readProjectHint" },
                    { new Guid("75732723-aba7-1685-bd11-04205305acb8"), "", "columns.save" },
                    { new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "", "myAccount.signOutQuestion" },
                    { new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "", "changePassword.submit" },
                    { new Guid("75c17f5e-cb87-87cc-3022-bcf9fa626525"), "", "capital.pendingEntries" },
                    { new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "", "vat.halfYear" },
                    { new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "", "columns.invalidOptions" },
                    { new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "", "capital.type" },
                    { new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "", "entries.withdrawalHint" },
                    { new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "", "accounts.counted" },
                    { new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "", "approvals.approveSelected" },
                    { new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "", "changePassword.confirm" },
                    { new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "", "expenses.amount" },
                    { new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "", "expenses.invalidAmount" },
                    { new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "", "register.refused" },
                    { new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "", "sharedSettings.themeSystem" },
                    { new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "", "partners.deleteRemovesMyDrafts" },
                    { new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "", "permissions.manageUsersHint" },
                    { new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "", "partners.name" },
                    { new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "", "reports.previous" },
                    { new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "", "permissions.addSupplierHint" },
                    { new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "", "entries.view" },
                    { new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "", "contracts.filterOpenValue" },
                    { new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "", "offline.sendNow" },
                    { new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "", "sharedSettings.themeLight" },
                    { new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "", "transfers.save" },
                    { new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "", "documents.saved" },
                    { new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "", "changePassword.title" },
                    { new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "", "contracts.edit" },
                    { new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "", "history.accountCreated" },
                    { new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "", "reports.pendingRevenue" },
                    { new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "", "columns.required" },
                    { new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "", "vat.q2" },
                    { new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "", "entries.kind" },
                    { new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "", "contracts.invalidQuantity" },
                    { new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "", "entryShare.account" },
                    { new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "", "history.all" },
                    { new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "", "suppliers.invalidMobile" },
                    { new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "", "entries.fix" },
                    { new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "", "vat.q3" },
                    { new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "", "entryShare.amount" },
                    { new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "", "history.empty" },
                    { new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "", "paymentMethods.invalidName" },
                    { new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "", "columns.typeAmount" },
                    { new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "", "projects.name" },
                    { new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "", "contracts.item" },
                    { new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "", "columns.noConnection" },
                    { new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "", "paymentMethods.projectFinished" },
                    { new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "", "capital.invalidFileType" },
                    { new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "", "history.entryIn" },
                    { new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "", "projectInfo.saved" },
                    { new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "", "reconciliations.noConnection" },
                    { new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "", "entryShare.contractor" },
                    { new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "", "vat.from" },
                    { new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "", "contracts.notes" },
                    { new Guid("819f545f-f676-205c-39b4-9f252160b826"), "", "approvals.refused" },
                    { new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "", "permissions.readProject" },
                    { new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "", "projectInfo.save" },
                    { new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "", "share.attach" },
                    { new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "", "permissions.approveEntries" },
                    { new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "", "users.invalidName" },
                    { new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "", "paymentMethods.rename" },
                    { new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "", "contracts.disabled" },
                    { new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "", "users.invalidUsername" },
                    { new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "", "share.invoiceAlreadyAttached" },
                    { new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "", "transfers.accountDisabled" },
                    { new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "", "myAccount.arabic" },
                    { new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "", "categories.hint" },
                    { new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "", "summary.expenses" },
                    { new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "", "projects.unitNameTaken" },
                    { new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "", "entries.enteredBy" },
                    { new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "", "partners.deleteQuestion" },
                    { new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "", "columns.name" },
                    { new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "", "vat.to" },
                    { new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "", "registered.empty" },
                    { new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "", "offline.sendAgain" },
                    { new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "", "changePassword.current" },
                    { new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "", "partners.edit" },
                    { new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "", "permissions.chargeAnyAccountHint" },
                    { new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "", "register.invalidDescription" },
                    { new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "", "summary.byCategory" },
                    { new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "", "expenses.entryWaitingForDecision" },
                    { new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "", "capital.date" },
                    { new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "", "entries.notFound" },
                    { new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "", "transfers.refused" },
                    { new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "", "projects.invalidDescription" },
                    { new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "", "registered.loadFailed" },
                    { new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "", "users.loadFailed" },
                    { new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "", "entries.contractorPayments" },
                    { new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "", "capital.partner" },
                    { new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "", "partners.itemNotOffered" },
                    { new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "", "contracts.filterOverrun" },
                    { new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "", "permissions.manageUsers" },
                    { new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "", "partners.draftsBy" },
                    { new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "", "reports.issueExplain" },
                    { new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "", "capital.noPartners" },
                    { new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "", "accounts.loadFailed" },
                    { new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "", "columns.invalidName" },
                    { new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "", "register.usernameTaken" },
                    { new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "", "attachments.pickFile" },
                    { new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "", "expenses.quantity" },
                    { new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "", "common.searchIn" },
                    { new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "", "approvals.entryNotPending" },
                    { new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "", "contracts.open" },
                    { new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "", "reconciliations.accountMissing" },
                    { new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "", "permissions.seeAllCapitalHint" },
                    { new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "", "capital.loadFailed" },
                    { new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "", "entries.loadFailed" },
                    { new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "", "entries.send" },
                    { new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "", "reconciliations.difference" },
                    { new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "", "entries.search" },
                    { new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "", "projects.close" },
                    { new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "", "permissions.moveMoneyHint" },
                    { new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "", "columns.optional" },
                    { new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "", "entries.purchases" },
                    { new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "", "approvals.fileFailed" },
                    { new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "", "entryShare.paymentMethod" },
                    { new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "", "columns.title" },
                    { new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "", "accounts.delete" },
                    { new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "", "accounts.myCustody" },
                    { new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "", "users.resetPassword" },
                    { new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "", "approvals.all" },
                    { new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "", "projects.renameUnit" },
                    { new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "", "contractorPayments.inside" },
                    { new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "", "categories.generalSubgroup" },
                    { new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "", "notifications.noConnection" },
                    { new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "", "suppliers.disabled" },
                    { new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "", "expenses.unknownSupplier" },
                    { new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "", "share.expense" },
                    { new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "", "reconciliations.refused" },
                    { new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "", "entries.tiedProjectRefused" },
                    { new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "", "contracts.contractHasPayments" },
                    { new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "", "approvals.openFile" },
                    { new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "", "entryShare.notes" },
                    { new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "", "approvals.reasonUnclear" },
                    { new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "", "myAccount.language" },
                    { new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "", "categories.invalidName" },
                    { new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "", "contractorPayments.overrun" },
                    { new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "", "partners.loadFailed" },
                    { new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "", "users.noMatch" },
                    { new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "", "myAccount.languageNeedsNetwork" },
                    { new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "", "suppliers.mostUsed" },
                    { new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "", "share.attached" },
                    { new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "", "entries.approved" },
                    { new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "", "contracts.contractor" },
                    { new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "", "categories.noTableMatch" },
                    { new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "", "history.partnerDeleted" },
                    { new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "", "sharedSettings.themeDark" },
                    { new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "", "paymentMethods.save" },
                    { new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "", "partners.deleteKeepsMovements" },
                    { new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "", "entries.period" },
                    { new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "", "notifications.empty" },
                    { new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "", "accounts.projectFinished" },
                    { new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "", "expenses.invalidUnitPrice" },
                    { new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "", "suppliers.quickAdd" },
                    { new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "", "categories.subgroupInUse" },
                    { new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "", "expenses.invalidColumnValue" },
                    { new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "", "columns.type" },
                    { new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "", "register.weakPassword" },
                    { new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "", "columns.coreColumn" },
                    { new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "", "suppliers.generalCashSupplier" },
                    { new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "", "users.save" },
                    { new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "", "accounts.deleteCannotUndo" },
                    { new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "", "history.entryRejected" },
                    { new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "", "notifications.title" },
                    { new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "", "share.chooseItem" },
                    { new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "", "permissions.manageSettingsHint" },
                    { new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "", "expenses.noAccount" },
                    { new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "", "users.title" },
                    { new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "", "categories.tableContract" },
                    { new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "", "contracts.projectFinished" },
                    { new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "", "projects.choose" },
                    { new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "", "permissions.deleteEntriesHint" },
                    { new Guid("a600acec-e37d-ba1c-1ec4-322b87ac9712"), "", "swipe.remove" },
                    { new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "", "users.mobile" },
                    { new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "", "projects.defaultTemplate" },
                    { new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "", "reports.unitPrice" },
                    { new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "", "contracts.contractValue" },
                    { new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "", "accounts.new" },
                    { new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "", "partners.partnerNotDeletable" },
                    { new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "", "documents.invalidName" },
                    { new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "", "capital.amount" },
                    { new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "", "categories.side" },
                    { new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "", "projects.empty" },
                    { new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "", "columns.newChoices" },
                    { new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "", "reports.partners" },
                    { new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "", "projects.structureSource" },
                    { new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "", "entries.edit" },
                    { new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "", "attachments.noConnection" },
                    { new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "", "reports.difference" },
                    { new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "", "transfers.projectFinished" },
                    { new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "", "partners.myDraftsNeedDeleteEntries" },
                    { new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "", "myAccount.signOutExplain" },
                    { new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "", "partners.empty" },
                    { new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "", "projects.loadFailed" },
                    { new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "", "common.passwordRules" },
                    { new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "", "partners.login" },
                    { new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "", "reconciliations.title" },
                    { new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "", "sharedSettings.entriesShown" },
                    { new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "", "categories.enable" },
                    { new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "", "myAccount.english" },
                    { new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "", "accounts.disable" },
                    { new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "", "permissions.mineHint" },
                    { new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "", "entries.removeQuestion" },
                    { new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "", "history.partnerAccount" },
                    { new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "", "partners.loginHint" },
                    { new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "", "reconciliations.differenceExplain" },
                    { new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "", "partners.history" },
                    { new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "", "accounts.typeBank" },
                    { new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "", "columns.typeFile" },
                    { new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "", "offline.title" },
                    { new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "", "common.retry" },
                    { new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "", "permissions.manageAnyEntryHint" },
                    { new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "", "biometrics.promptTitle" },
                    { new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "", "entries.entryIncomplete" },
                    { new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "", "accounts.noCustodian" },
                    { new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "", "categories.categoryName" },
                    { new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "", "projects.exportSettings" },
                    { new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "", "expenses.qrRead" },
                    { new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "", "entryShare.status" },
                    { new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "", "contracts.running" },
                    { new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "", "register.sent" },
                    { new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "", "attachments.takePhoto" },
                    { new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "", "projects.rename" },
                    { new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "", "reports.generalSubgroup" },
                    { new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "", "vat.h2" },
                    { new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "", "expenses.invalidNotes" },
                    { new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "", "contracts.value" },
                    { new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "", "users.refused" },
                    { new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "", "reconciliations.lastUpdated" },
                    { new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "", "permissions.recordEntriesHint" },
                    { new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "", "expenses.entryExists" },
                    { new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "", "summary.revenue" },
                    { new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "", "reports.remaining" },
                    { new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "", "suppliers.disable" },
                    { new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "", "documents.fileRequired" },
                    { new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "", "accounts.enable" },
                    { new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "", "entries.title" },
                    { new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "", "suppliers.name" },
                    { new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "", "registered.approve" },
                    { new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "", "sharedSettings.appearance" },
                    { new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "", "summary.difference" },
                    { new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "", "paymentMethods.loadFailed" },
                    { new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "", "projects.noConnection" },
                    { new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "", "permissions.chargeAnyAccount" },
                    { new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "", "documents.none" },
                    { new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "", "revenue.amount" },
                    { new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "", "attachments.projectFinished" },
                    { new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "", "capital.receiptAttached" },
                    { new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "", "entries.status" },
                    { new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "", "categories.tableRevenue" },
                    { new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "", "contracts.notStarted" },
                    { new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "", "expenses.total" },
                    { new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "", "categories.loadFailed" },
                    { new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "", "contractorPayments.remainingAfter" },
                    { new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "", "history.unknownPartner" },
                    { new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "", "users.usernameTaken" },
                    { new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "", "paymentMethods.name" },
                    { new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "", "categories.tablePercentages" },
                    { new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "", "reports.issue" },
                    { new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "", "categories.itemHasContract" },
                    { new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "", "reconciliations.enterTitle" },
                    { new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "", "columns.edit" },
                    { new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "", "capital.share" },
                    { new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "", "approvals.unknownEntry" },
                    { new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "", "signIn.invalidCredentials" },
                    { new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "", "accounts.deleteQuestion" },
                    { new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "", "suppliers.projectFinished" },
                    { new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "", "contractorPayments.savedOverrun" },
                    { new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "", "projects.pendingEntries" },
                    { new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "", "history.capitalIn" },
                    { new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "", "capital.contribution" },
                    { new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "", "categories.noConnection" },
                    { new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "", "entries.notYours" },
                    { new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "", "transfers.button" },
                    { new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "", "capital.save" },
                    { new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "", "contracts.remaining" },
                    { new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "", "capital.imageTooLarge" },
                    { new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "", "users.roleName" },
                    { new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "", "categories.sideExpenses" },
                    { new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "", "sharedSettings.title" },
                    { new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "", "entries.empty" },
                    { new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "", "contracts.empty" },
                    { new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "", "approvals.history" },
                    { new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "", "projects.projectFinished" },
                    { new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "", "reports.paid" },
                    { new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "", "contracts.quantity" },
                    { new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "", "projects.noUnits" },
                    { new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "", "columns.core" },
                    { new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "", "projectInfo.title" },
                    { new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "", "summary.loadFailed" },
                    { new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "", "suppliers.searchHint" },
                    { new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "", "reports.loadFailed" },
                    { new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "", "summary.pending" },
                    { new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "", "contracts.disable" },
                    { new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "", "permissions.recordEntries" },
                    { new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "", "documents.invalidGroup" },
                    { new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "", "permissions.seeChosenCapital" },
                    { new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "", "projects.rejected" },
                    { new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "", "transfers.title" },
                    { new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "", "projects.new" },
                    { new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "", "contracts.unitPrice" },
                    { new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "", "entries.share" },
                    { new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "", "common.search" },
                    { new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "", "expenses.invalidEntryId" },
                    { new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "", "columns.inReports" },
                    { new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "", "paymentMethods.new" },
                    { new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "", "history.accountDisabled" },
                    { new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "", "swipe.question" },
                    { new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "", "projectInfo.noDescription" },
                    { new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "", "reports.pendingExpenses" },
                    { new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "", "reports.overrun" },
                    { new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "", "signIn.sessionExpired" },
                    { new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "", "suppliers.defaultItem" },
                    { new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "", "history.custodianChanged" },
                    { new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "", "contracts.noConnection" },
                    { new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "", "capital.total" },
                    { new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "", "attachments.cameraFailed" },
                    { new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "", "attachments.imageTooLarge" },
                    { new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "", "registered.reject" },
                    { new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "", "documents.documentAlreadyKept" },
                    { new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "", "biometrics.hint" },
                    { new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "", "accounts.disabled" },
                    { new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "", "reports.contractValue" },
                    { new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "", "myAccount.changePassword" },
                    { new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "", "permissions.title" },
                    { new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "", "suppliers.vatNumberTaken" },
                    { new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "", "permissions.manageSettings" },
                    { new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "", "transfers.notes" },
                    { new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "", "reconciliations.save" },
                    { new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "", "suppliers.noConnection" },
                    { new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "", "expenses.entryApproved" },
                    { new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "", "transfers.invalidAmount" },
                    { new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "", "common.cancel" },
                    { new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "", "categories.new" },
                    { new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "", "register.unitNameTaken" },
                    { new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "", "signIn.forgotPassword" },
                    { new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "", "biometrics.or" },
                    { new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "", "documents.groupHint" },
                    { new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "", "projects.save" },
                    { new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "", "columns.loadFailed" },
                    { new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "", "expenses.keptOnPhone" },
                    { new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "", "vat.custom" },
                    { new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "", "expenses.unitPrice" },
                    { new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "", "users.username" },
                    { new Guid("da7a2b71-6655-c965-411b-18f830383110"), "", "categories.save" },
                    { new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "", "suppliers.addAndPick" },
                    { new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "", "approvals.purchases" },
                    { new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "", "contractorPayments.outside" },
                    { new Guid("db920181-5bba-2173-698f-310a22fe706d"), "", "share.refused" },
                    { new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "", "suppliers.pendingApproval" },
                    { new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "", "paymentMethods.disable" },
                    { new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "", "approvals.rejectSend" },
                    { new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "", "revenue.title" },
                    { new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "", "contractorPayments.kind" },
                    { new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "", "permissions.manageProjectHint" },
                    { new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "", "users.unknownPartner" },
                    { new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "", "users.disabled" },
                    { new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "", "attachments.invoicePhoto" },
                    { new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "", "history.unknownAccount" },
                    { new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "", "categories.projectFinished" },
                    { new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "", "history.partnerChanged" },
                    { new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "", "registered.reason" },
                    { new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "", "biometrics.notAvailable" },
                    { new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "", "share.empty" },
                    { new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "", "permissions.manageProject" },
                    { new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "", "permissions.capitalGroup" },
                    { new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "", "projects.description" },
                    { new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "", "capital.account" },
                    { new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "", "documents.name" },
                    { new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "", "entryShare.date" },
                    { new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "", "common.undo" },
                    { new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "", "attachments.retake" },
                    { new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "", "accounts.custodies" },
                    { new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "", "approvals.reason" },
                    { new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "", "users.invalidRoleName" },
                    { new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "", "approvals.reasonItem" },
                    { new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "", "transfers.sameAccount" },
                    { new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "", "contracts.documents" },
                    { new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "", "categories.typeSale" },
                    { new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "", "users.disableExplain" },
                    { new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "", "share.matching" },
                    { new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "", "swipe.removed" },
                    { new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "", "permissions.approveEntriesHint" },
                    { new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "", "notifications.projectApproved" },
                    { new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "", "accounts.custodian" },
                    { new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "", "contracts.addDocument" },
                    { new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "", "categories.changeType" },
                    { new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "", "accounts.deleted" },
                    { new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "", "entries.new" },
                    { new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "", "entries.approvedCannotChange" },
                    { new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "", "contracts.paid" },
                    { new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "", "history.mine" },
                    { new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "", "accounts.unknownCustodian" },
                    { new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "", "projects.manager" },
                    { new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "", "signIn.accountLocked" },
                    { new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "", "accounts.custodianRequired" },
                    { new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "", "expenses.reasonRequired" },
                    { new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "", "entries.mine" },
                    { new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "", "registered.title" },
                    { new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "", "suppliers.pick" },
                    { new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "", "expenses.qrAddSupplier" },
                    { new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "", "suppliers.mobile" },
                    { new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "", "suppliers.noDefaultItem" },
                    { new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "", "categories.tablePurchases" },
                    { new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "", "columns.invalidDataType" },
                    { new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "", "vat.q1" },
                    { new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "", "documents.noConnection" },
                    { new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "", "approvals.contractorPayments" },
                    { new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "", "accounts.history" },
                    { new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "", "reports.date" },
                    { new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "", "categories.noItems" },
                    { new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "", "contracts.save" },
                    { new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "", "projects.importSettings" },
                    { new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "", "history.check" },
                    { new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "", "expenses.invalidProduct" },
                    { new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "", "capital.balance" },
                    { new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "", "accounts.title" },
                    { new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "", "entries.noTableMatch" },
                    { new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "", "projects.chooseHint" },
                    { new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "", "transfers.balanceNow" },
                    { new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "", "accounts.empty" },
                    { new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "", "attachments.invalidFileType" },
                    { new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "", "entries.withdrawal" },
                    { new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "", "partners.refused" },
                    { new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "", "users.name" },
                    { new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "", "history.partnerCreated" },
                    { new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "", "approvals.rejectTitle" },
                    { new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "", "permissions.removeApprovedEntries" },
                    { new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "", "share.amountDiffers" },
                    { new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "", "register.hint" },
                    { new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "", "expenses.accountNotOffered" },
                    { new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "", "categories.typePurchases" },
                    { new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "", "projects.closeExplain" },
                    { new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "", "contracts.loadFailed" },
                    { new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "", "categories.itemHasEntries" },
                    { new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "", "projectInfo.description" },
                    { new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "", "reports.save" },
                    { new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "", "partners.projectFinished" },
                    { new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "", "categories.itemName" },
                    { new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "", "suppliers.title" },
                    { new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "", "revenue.file" },
                    { new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "", "approvals.accountShort" },
                    { new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "", "entries.searchHint" },
                    { new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "", "notifications.entryRejected" },
                    { new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "", "expenses.unknownColumn" },
                    { new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "", "categories.title" },
                    { new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "", "register.invalidMobile" },
                    { new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "", "contracts.refused" },
                    { new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "", "permissions.hint" },
                    { new Guid("fbe18f17-7aca-e801-0031-793076552587"), "", "vat.quarter" },
                    { new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "", "transfers.unknownAccount" },
                    { new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "", "capital.movements" },
                    { new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "", "registered.invalidReason" },
                    { new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "", "reports.number" },
                    { new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "", "registered.notPending" },
                    { new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "", "changePassword.noConnection" },
                    { new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "", "common.filter" },
                    { new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "", "capital.noMovements" },
                    { new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "", "biometrics.notChanged" },
                    { new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "", "users.noConnection" },
                    { new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "", "accounts.invalidName" },
                    { new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "", "categories.typeVatRefund" }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Description", "IsTemplate", "Name", "RejectionReason", "StartDate", "Status", "StructureSourceId" },
                values: new object[] { new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), "", true, "القالب الافتراضي", "", new DateOnly(1900, 1, 1), (byte)0, new Guid("00000000-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "Id", "Language", "TranslationKeyId", "Value" },
                values: new object[,]
                {
                    { new Guid("001c2473-59c6-4c79-c8bd-cd7210f93645"), "en", new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "Transfer" },
                    { new Guid("003d61df-901e-682d-f351-a99c427e6991"), "ar", new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "لا توجد طرق دفع." },
                    { new Guid("009cb907-266d-c459-8d48-670708f7e3e6"), "ar", new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "حذفه؟" },
                    { new Guid("00c8ad86-e393-8e9a-5e5a-82ddaa6f3e79"), "en", new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "Pick list" },
                    { new Guid("00f3bd6e-7747-4829-d2a6-b5df1f1020f6"), "en", new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "A revenue item asks no columns." },
                    { new Guid("0108cac3-ae40-1d8e-ffef-80182714d7f9"), "en", new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "Shape the expense structure" },
                    { new Guid("012f16b1-576f-a02e-7f1d-c85144b13a98"), "ar", new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "الفرق" },
                    { new Guid("016917c7-21f1-ceb8-7da5-2e121135e908"), "ar", new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "تنبيه: رصيد الحساب لا يكفي لهذا المبلغ" },
                    { new Guid("017a7088-2510-cf18-f303-7c913dfdff72"), "ar", new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "الحساب" },
                    { new Guid("0194614f-2539-de35-39b3-094c9688dd92"), "en", new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "This project is finished, so nothing in it changes." },
                    { new Guid("0196d2e3-c509-25d3-0f25-514282022409"), "ar", new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "أدخل مبلغاً أكبر من صفر بخانتين عشريتين كحد أقصى، أو سعر الوحدة والعدد." },
                    { new Guid("0199efec-33f4-a9f2-cb18-93268bda0778"), "ar", new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "رأس مال شركاء محددين" },
                    { new Guid("01b01836-d63d-bea2-6190-8832bbb5d9ff"), "en", new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "An entry waits for your decision" },
                    { new Guid("01e5480e-eb6a-432c-009f-a7b7e95570b6"), "ar", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "التاريخ" },
                    { new Guid("01e7b8f2-9e43-0653-f692-033646bc89ee"), "ar", new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "إضافة مورد" },
                    { new Guid("01f958b8-b855-db32-d142-c5383d9e71c8"), "ar", new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "مع فاتورة" },
                    { new Guid("02152fcc-f2d4-300a-e52e-8948b647b0dd"), "ar", new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "النوع" },
                    { new Guid("027ac94c-989d-d5e0-6f51-f61e965e007c"), "en", new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "العربية" },
                    { new Guid("028fb5de-c7b6-a668-18ef-8bab350c4b20"), "en", new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "Approved" },
                    { new Guid("02d6f999-9388-217e-daf8-563539c673e2"), "ar", new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "دخول" },
                    { new Guid("02f020dc-196d-e1fb-2324-eb94acb99a1a"), "en", new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "The picture must be smaller than 1 MB." },
                    { new Guid("0302765a-207a-ef61-b080-edcafcd746be"), "en", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "Share %" },
                    { new Guid("0358ca3a-b06f-1b0d-6774-63f4d87ed9bf"), "en", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "Notes may have at most 1000 characters." },
                    { new Guid("037588db-c795-9cd1-edef-1fa9dc5dd008"), "ar", new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "بانتظار الاعتماد" },
                    { new Guid("037d21e1-1750-d5ce-c081-385636de93b3"), "ar", new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "تعديل العمود" },
                    { new Guid("03a02752-8052-3eb3-96e8-24ba04e8f556"), "en", new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "Register new project" },
                    { new Guid("03b39f24-015d-5117-5b7b-ed8f9e7ee1c7"), "en", new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "Nothing is waiting for approval." },
                    { new Guid("03ca8285-ff93-b8f9-88a5-47b271085b20"), "ar", new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "يشمل المعتمد الآن ولا يتغير بعد إصداره. والقيود المنتظرة للاعتماد ({0}) تظهر منفصلة: المصروفات في سطر، والإيرادات ورأس المال في سطر." },
                    { new Guid("03e6fd0e-410f-cf71-33b2-d8f3a59c5f4b"), "en", new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "Entries use this subgroup, so it cannot be removed." },
                    { new Guid("03ed9b06-9e66-1525-6501-372fe562da51"), "en", new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "General" },
                    { new Guid("041a1376-8b31-2afa-32a1-666b1ce425f8"), "ar", new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "يُعتمد المورد الجديد مع أول مصروف عليه." },
                    { new Guid("042648c9-dacd-554c-342e-264f1b6f3fb7"), "en", new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "Search" },
                    { new Guid("04301270-828b-102c-3467-c07ef8c4bdf1"), "ar", new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "حُفظ على هذا الجهاز، ويُرسل عند عودة الشبكة." },
                    { new Guid("043422b3-7647-3753-2954-63f4b1ee1393"), "en", new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "Used by entries, checks or transfers, so it can only be disabled." },
                    { new Guid("0438231d-e912-50dd-ade9-0a2554ffbced"), "en", new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "Disable" },
                    { new Guid("043efcff-c662-3319-0b57-0d8af9f681f6"), "en", new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "Enable" },
                    { new Guid("04765bdd-0b0e-34a1-7da3-17a41856b902"), "ar", new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "دفعات عقد" },
                    { new Guid("0495e296-76a3-7f2d-6095-99e3403c7a72"), "en", new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "Group (optional)" },
                    { new Guid("04e59762-1d55-c7e0-2cc8-ce7863419500"), "en", new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "Open the file" },
                    { new Guid("04eb0792-159c-a993-c916-898766c448be"), "ar", new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "يسحب الشريك هذا المبلغ من رأس ماله" },
                    { new Guid("05142cdd-c151-b525-bd99-edcd858edad7"), "en", new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "Changed {0}: {1}" },
                    { new Guid("0542b3d1-aa00-dac4-5f0b-1aa4cd8201ae"), "ar", new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "بحث بالاسم أو الرقم الضريبي" },
                    { new Guid("05bb45c1-0d7c-4b16-9aae-57f9b44b9016"), "ar", new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "تصنيف رئيسي جديد" },
                    { new Guid("05eb65ed-3eb2-9dc3-0625-17d9e0491794"), "ar", new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "يختفي من الشركاء ومن إضافة قيد، وتبقى حركاته في السجلات والتقارير." },
                    { new Guid("06164929-a665-1198-ec1f-a6e2278dbb3a"), "en", new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "Could not read what is waiting. Check the connection and try again." },
                    { new Guid("063f734c-da60-f010-7745-3fa11ed3b50b"), "en", new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "No connection to the server. Try again." },
                    { new Guid("066b040f-742a-34d0-ec4a-c186fb4afe0b"), "en", new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "No connection. Try again when online." },
                    { new Guid("066e36b0-f0c2-e3ac-88ab-5c0fa06d8688"), "en", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "Import project settings" },
                    { new Guid("06cdf1d1-ec2f-45af-2031-d78d8fd5520d"), "ar", new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "لا توجد وحدات بعد." },
                    { new Guid("0700fe3f-fc77-1294-7b69-eb07064e7a88"), "ar", new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "مورد جديد" },
                    { new Guid("071fa6a0-78b2-c504-9999-af8c61367f80"), "en", new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "This change was refused." },
                    { new Guid("0721816f-e32a-820f-bb96-5c63fd7521c7"), "ar", new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "لا مشاريع بعد." },
                    { new Guid("07c54d7d-0963-ab0b-5c18-ef8a5941a0b1"), "ar", new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "الجانب" },
                    { new Guid("07e30917-32ab-8658-a444-ca12fddb1229"), "en", new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "The server could not sign you in. Try again." },
                    { new Guid("07ed9ffb-59cd-7c8b-c16b-81de7bd3cb93"), "en", new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "Rename" },
                    { new Guid("08006225-d160-6d0c-4a8e-8f3e2f17108f"), "en", new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "Overrun" },
                    { new Guid("081a2a29-b1e5-88bc-f6d2-3ea5fbd6cacf"), "ar", new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "إضافة شريك" },
                    { new Guid("088a74a6-b6ac-6ea6-c0a6-1b1170ca3c57"), "en", new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "Share" },
                    { new Guid("08d2d678-d7ab-602a-e539-8777e2253396"), "ar", new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "الأكثر استخداماً" },
                    { new Guid("08f5d8be-844d-c493-94ba-a5ffe9a7e853"), "en", new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "This contract has payments, so its contractor and item cannot change." },
                    { new Guid("09091696-ccfc-2c4b-2fb6-30a990acc27b"), "ar", new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +." },
                    { new Guid("092d0b8a-f7fe-14b4-c03f-25f7028d963b"), "ar", new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "الفترة" },
                    { new Guid("09472c98-9f15-bdb9-1965-9ab8fd80ad40"), "en", new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "Search {0}" },
                    { new Guid("09510e0e-2ea5-f556-e437-b3127d6aba74"), "en", new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "Second" },
                    { new Guid("09673683-2250-fca5-7d94-213d082127f4"), "en", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "Date" },
                    { new Guid("0974e52e-f3f7-3334-2e71-541773bcb02d"), "en", new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "Amend contract" },
                    { new Guid("097d4f60-34e6-98b0-7ac5-f8bf4dc4094f"), "ar", new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "PDF" },
                    { new Guid("0981f535-f51d-256b-5c2f-84f30714826e"), "ar", new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "مشتريات + دفعات مقاولين" },
                    { new Guid("0986f892-50b0-1c00-b9b5-1a3acced1fb1"), "en", new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "Sees only the capital of the partners ticked under it." },
                    { new Guid("09afb887-8981-690b-b38b-fb47566b827e"), "ar", new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "تُرسل هذه القيود وحدها حين تعود الشبكة." },
                    { new Guid("09e93ad6-ebbc-9c19-bcbf-4bb788b171c5"), "en", new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "It becomes read-only: no new entries or approvals, while its reports stay available. This cannot be undone." },
                    { new Guid("0a28ffc6-76ae-d470-bdb4-d42afadd268c"), "ar", new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "المصروفات حسب التصنيف" },
                    { new Guid("0a8817c6-9ecd-8cb0-c1c7-677aa8de819c"), "ar", new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "نوع الدفعة" },
                    { new Guid("0aa18445-a76f-8514-98bb-30066af547f4"), "en", new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "Your project was approved" },
                    { new Guid("0aa2162e-3d6b-8b42-8300-a302de0fd0b7"), "ar", new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "كلمة المرور" },
                    { new Guid("0b0b8889-94ab-cda6-eeae-99a0fc39b180"), "ar", new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "لا بند يطابق البحث." },
                    { new Guid("0b331b74-2be5-1a3a-3047-8bc54608314e"), "en", new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "Choose a contractor from the suppliers." },
                    { new Guid("0b5ef6da-d918-2900-a478-7a86826e0386"), "en", new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "Language" },
                    { new Guid("0b75c2d4-2ab2-a2d7-2f1a-c21e97c487ca"), "ar", new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "تقرير رقم" },
                    { new Guid("0b8aeded-f822-3f31-7828-f561301e7316"), "ar", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "الملاحظات طويلة جداً." },
                    { new Guid("0ba613ef-d4ae-2ad6-aee9-079960838311"), "en", new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "Could not read the contracts. Check the network and try again." },
                    { new Guid("0bae938b-e2c2-4159-2d1d-05b03fe55f1b"), "en", new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "Add subgroup" },
                    { new Guid("0bdc8545-9fe0-dd3e-b82c-70ce7a1f262b"), "en", new Guid("27d5e8ae-ac71-1977-0de4-6b4fee3d166e"), "REDIX" },
                    { new Guid("0c168d51-1606-135d-f1de-ea85ce1e9720"), "ar", new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "الكل" },
                    { new Guid("0c2280a7-51fc-c927-b6d1-46d24cd80569"), "en", new Guid("3532dea3-fa32-67e2-371b-a94dc6c0bc7d"), "Note: the account does not hold enough for this amount." },
                    { new Guid("0c2d5090-4780-be30-87c9-a319275c554b"), "ar", new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "النصف الأول" },
                    { new Guid("0c5e8604-a75a-b0fb-bfca-31f5c2e5e9aa"), "ar", new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "رأس المال" },
                    { new Guid("0c771f3b-91ad-f26f-f404-f1d30227451d"), "ar", new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "لا حركات بعد." },
                    { new Guid("0c9043cf-263c-c229-2ef6-4cce26151b41"), "ar", new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "إلى" },
                    { new Guid("0cb93e87-3cc1-4e86-065e-c620093a7cb2"), "ar", new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "إرسال الآن" },
                    { new Guid("0ce7c2a1-49ab-2f75-f509-822ff45276a7"), "ar", new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "إصدار رقم {0}" },
                    { new Guid("0cedd85b-3270-c582-ad10-383b2de3f783"), "en", new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "The list has changed. Try again." },
                    { new Guid("0d6fa4f6-d760-576e-706e-0377e17f4a64"), "en", new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "Choices, one on each line" },
                    { new Guid("0d8c25c4-56ca-9b8f-3142-e97166f2c4fb"), "ar", new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "النوع" },
                    { new Guid("0dcb0200-576a-1061-7dda-d5109bd5dae7"), "en", new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "Removes an approved entry: it leaves every list and figure, and its history stays." },
                    { new Guid("0dd3117a-bc8f-045b-0666-990256916994"), "en", new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "More info" },
                    { new Guid("0de9850d-d3f9-d187-e743-fb6b2fc077cd"), "ar", new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "تعذر تنفيذ ذلك." },
                    { new Guid("0e1d056a-dfe5-3769-8ef4-3d6fc8536a23"), "ar", new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "يحتاج المشروع إلى شخص واحد على الأقل يدير مستخدميه." },
                    { new Guid("0e1d6f19-e18a-ba67-da68-7bf5ca0fb299"), "en", new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "No connection to the server. Try again." },
                    { new Guid("0e73c805-b6f4-5720-08ce-412fef5605cb"), "en", new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "Could not load the projects. Check the network and try again." },
                    { new Guid("0e755cfb-0c15-00fc-6132-9e14233bff8d"), "en", new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "Add item" },
                    { new Guid("0ec63acf-9f28-8066-4b9b-5fdf973cac8e"), "en", new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "Too many wrong tries. Try again later." },
                    { new Guid("0edb0ee7-9fe2-250b-d9b7-4906dd30c7a2"), "ar", new Guid("21d95887-8bcc-8670-37f5-426441d6e91a"), "يجب أن يقع التاريخ ضمن فترة المشروع وألا يكون في المستقبل." },
                    { new Guid("0ef4fa30-5d7f-43af-b1e2-1297a6914764"), "ar", new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "الفرق يعني مصروفاً لم يُسجَّل أو رسوماً بنكية. راجع حركات الحسابات منذ آخر مطابقة." },
                    { new Guid("0f0a8dec-222e-d150-8db8-ef94320526f5"), "en", new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "Fix and send again" },
                    { new Guid("0f201428-1dbf-942f-846e-6bab11651c61"), "ar", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "الحسابات والمطابقة" },
                    { new Guid("0f2caf8d-cff5-5aa7-7185-1f611aa4a91c"), "ar", new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "مشاركة" },
                    { new Guid("0f4b495b-6989-2d80-15e2-9a25233f6292"), "en", new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "Sign out of this phone?" },
                    { new Guid("0f4cbc18-8dbb-5cd1-e75e-dbf2be7030a2"), "en", new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "Choose a project" },
                    { new Guid("0f60dc1f-f376-69e0-4bd0-272dedec8780"), "ar", new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "يبقى المورد النقدي العام في القائمة لأن المصروفات التي بلا مورد تحتاجه. ويمكنك تغيير اسمه." },
                    { new Guid("0f939def-ba49-d8b0-6cd9-c8374be39c6f"), "en", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "This could not be done." },
                    { new Guid("0fa683df-3120-2dab-ce21-b44b92f449c5"), "ar", new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "تبديل المشروع" },
                    { new Guid("0facd6a8-fa10-5475-6c5d-76b601fa1c82"), "ar", new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "العقود" },
                    { new Guid("0fb0788c-2336-f27e-6aa9-6f16ffeab7a7"), "ar", new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "اسمه ووحداته ومستنداته، وإغلاقه." },
                    { new Guid("0fb0adac-ef68-a968-4e72-ac835fd3432c"), "ar", new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "قُرئ رمز QR وعُبّئت الحقول" },
                    { new Guid("100b9879-686b-70c8-59a4-ee2ffd481c3d"), "en", new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "Without sales" },
                    { new Guid("105f2a98-a822-c10a-1097-4243746aab20"), "ar", new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("1080f1df-1544-78e1-ebda-23e9ad133ff9"), "ar", new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "اسم التصنيف الرئيسي" },
                    { new Guid("10aaca51-635a-f28f-cacb-6cb7b7918bdd"), "en", new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "Tap to review with the invoices" },
                    { new Guid("10b6922b-86f9-57d7-cbc7-53b5bf7ef7bf"), "ar", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "ملاحظات" },
                    { new Guid("110abad0-c036-72f8-fc60-20ac37e7da13"), "en", new Guid("666082ac-4221-7539-3047-ab60621f529c"), "More" },
                    { new Guid("1119f52f-d29e-839b-f71e-694ac237f060"), "en", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "Unit price" },
                    { new Guid("112551b6-8585-fdc1-faeb-11431178178a"), "ar", new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "خارج العقد" },
                    { new Guid("112b349b-af69-d360-8cd2-85fb5a676293"), "en", new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "Paid more than the contract" },
                    { new Guid("114dbec0-4cc5-4cba-28a3-99f86713f98d"), "ar", new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "القالب الافتراضي" },
                    { new Guid("1159240c-afc5-71b4-acd8-65dbd6445c3c"), "ar", new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "قيد" },
                    { new Guid("1162fef6-abd5-2771-b60b-369e63b8cc46"), "ar", new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "حذف الحساب؟" },
                    { new Guid("1178de5d-f4f2-5ede-ad0a-6b1efd151989"), "ar", new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "لا شيء بانتظار المزامنة." },
                    { new Guid("11ac03e8-39e7-43d0-bd2e-b1324d4948e6"), "en", new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "Mobile (optional)" },
                    { new Guid("120281a5-3274-9341-8634-2243ab566e58"), "ar", new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("1237f62d-0322-4fec-2ca6-2eb54316df70"), "en", new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "Fourth" },
                    { new Guid("124d6e57-efac-9edb-915e-5b9de753a68a"), "ar", new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "هذه القيمة لا تناسب هذا الحقل." },
                    { new Guid("12625a28-c749-d22e-8953-0439c90d7e8d"), "ar", new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "البند" },
                    { new Guid("12a980ae-b097-3725-cff5-32568b5fb03e"), "ar", new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "إغلاق هذا المشروع؟" },
                    { new Guid("12cbb5f1-d3c2-5615-26b8-d77519bd1577"), "en", new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "See all capital" },
                    { new Guid("12d4dbef-b9a3-c10f-d5cf-2739afb93706"), "ar", new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "السبب (إلزامي)" },
                    { new Guid("12dda6b9-b9b1-c919-bf06-920e193cd448"), "en", new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "No supplier in the register holds this VAT number." },
                    { new Guid("13156b78-e9ec-28e7-9d92-6b9fe768c8ed"), "ar", new Guid("00095999-1d8f-0540-4945-641c6d0f4997"), "الرابع" },
                    { new Guid("132c066e-906d-1ba2-5901-4128c31482c4"), "en", new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "Periodic report" },
                    { new Guid("14049f43-4660-131a-8696-4c91c9c705ba"), "en", new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "What I did" },
                    { new Guid("146ab521-5928-c96f-0a9c-1f6fec92ad08"), "ar", new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("147de803-538b-c5fc-a985-5e2d35976d90"), "ar", new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("148f90b2-75cf-925a-961a-a2899215aded"), "ar", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "نسيت كلمة المرور؟ تواصل مع مدير مشروعك." },
                    { new Guid("14acffbb-a1d0-8ca3-e896-be0d2d3f681b"), "en", new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "Add column" },
                    { new Guid("14d3fff4-1489-ba20-c250-d96e60396e5a"), "ar", new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "مبلغ" },
                    { new Guid("14d60cda-88d0-1d1e-5f97-6b0ab628e492"), "ar", new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "هذا المشروع منتهٍ، فلا يمكن تغييره." },
                    { new Guid("150d26f0-7e27-62ce-2c91-3540ca5090ad"), "ar", new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "الصرف من أي حساب" },
                    { new Guid("151e7dd2-0a1c-5be2-accf-309ef2321dec"), "en", new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "Save" },
                    { new Guid("15272de3-a481-c904-1650-6276f93f0055"), "ar", new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "مطابقة الأرصدة الفعلية: {0}" },
                    { new Guid("152c3616-4f8d-9b31-29bb-80bbf2516298"), "en", new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "Open-value contract" },
                    { new Guid("154467ea-dd73-094d-112a-d6a4252d5034"), "ar", new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "يُسجَّل خروجه، ويجب أن يختار كلمة مرور جديدة عند دخوله التالي." },
                    { new Guid("155550a3-e5ea-154b-8eb6-392790d4101a"), "ar", new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "هذا الحساب ليس في هذا المشروع." },
                    { new Guid("15782241-c102-6309-baac-1a2a461ea4ed"), "ar", new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "إيرادات" },
                    { new Guid("15c4e47d-c133-91cd-1390-391c811dbcfb"), "en", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "Recorded as its own row with a minus amount" },
                    { new Guid("15c5b681-8dd9-1ff5-0189-06a4e7aef403"), "en", new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "Sign in to follow your projects' expenses" },
                    { new Guid("15c5dddc-fd5b-d50c-9eca-fda7d18221c9"), "ar", new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "تعديل" },
                    { new Guid("15d50fa4-ef16-7294-3d21-ad604e4e7c0c"), "ar", new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "لا توجد مستندات بعد." },
                    { new Guid("15de3ae3-f14a-ee73-b0dc-b196a00cfa88"), "ar", new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "رأس مال {0} من {1} إلى {2}" },
                    { new Guid("1649079c-ff19-3707-a4e7-6526e35265b9"), "en", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "Description" },
                    { new Guid("16c5e91a-4e90-c01d-f96b-1858d5f1b456"), "en", new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "This could not be done." },
                    { new Guid("16fcbcfc-20ab-15a0-4bdb-61e594e17745"), "en", new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "Changed the partner {0}: {1}" },
                    { new Guid("1704fe25-8eec-50ed-f814-655e4aafc8c7"), "ar", new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "في الدفاتر" },
                    { new Guid("171ff7a2-3a7e-630d-b4be-8c83ed40e3e2"), "en", new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "Choose a payment method." },
                    { new Guid("174383bb-088c-aada-edec-7ebab2952741"), "ar", new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "تغيير اللغة يحتاج إلى شبكة." },
                    { new Guid("176bcd59-73ed-e737-fea1-237072c8372d"), "ar", new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "النظام" },
                    { new Guid("17a58459-d062-3ba7-257b-24a449d0b144"), "en", new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "Edit account" },
                    { new Guid("17c483cb-3273-e620-d826-c6f5f484fb17"), "en", new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "No connection to the server. Try again." },
                    { new Guid("180821d0-2354-c568-12e3-f08d135d7c36"), "ar", new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "تعذّرت قراءة السجل. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("183d3872-728e-3d6c-1183-50f72e934090"), "ar", new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "تعديل الشريك" },
                    { new Guid("185f4148-a84f-ba09-74aa-4f1009ba4478"), "ar", new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "رُفض هذا التغيير." },
                    { new Guid("18676f79-7399-dc0a-08f9-50b1c735c102"), "ar", new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "الوحدات" },
                    { new Guid("18714d75-c699-01e4-11e5-d15d8de353c6"), "en", new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "The username or password is wrong." },
                    { new Guid("18739dda-0f8a-cb22-82e9-cc1c583dcc04"), "en", new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "Show details" },
                    { new Guid("190594d7-d9d2-c6f7-9c7d-693b648abe5c"), "en", new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "This file is already on another expense." },
                    { new Guid("1951c2e7-f090-4df1-5aee-5f8f05986cd2"), "en", new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "Temporary password" },
                    { new Guid("19a24d30-08c4-a6c0-a2ce-41e7f4e0de18"), "ar", new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "حُفظت، والعقد متجاوَز." },
                    { new Guid("19b938d8-3205-1c4f-42b2-f49d3fd0923d"), "en", new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "A unit of this project already has this name." },
                    { new Guid("19c0565e-ddba-8a29-eb90-642d7f8a4387"), "en", new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "This waits for Finance's decision, so it cannot change now." },
                    { new Guid("1a1944db-a39a-8ece-3821-6ade63ff1f5e"), "en", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "Transfer" },
                    { new Guid("1a4c4f3e-3fe2-7d5d-794e-5ea391d9be98"), "ar", new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "السجل" },
                    { new Guid("1a58c845-64ab-a927-e20c-3f4893a1a068"), "ar", new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "البحث في القيود" },
                    { new Guid("1a94905d-b7db-9426-6022-52ecf68252da"), "en", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "Date" },
                    { new Guid("1aa13478-96a9-7965-566a-7f766ac0bbe8"), "ar", new Guid("41714f5b-9070-a72c-ef3d-83d05f9063b7"), "هذا الملف مرفق بمصروف آخر." },
                    { new Guid("1aad7d09-1819-c93c-dffb-d8d49214eec8"), "ar", new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "منتهٍ" },
                    { new Guid("1ab2159c-07c7-fb5d-20a0-3db8ed883882"), "ar", new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "انتهت الجلسة. سجّل الدخول مرة أخرى." },
                    { new Guid("1aeac483-353c-794e-193c-2adc6092b0fd"), "ar", new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "رفض الخادم هذا التغيير." },
                    { new Guid("1aeca5a0-c348-8d0b-d78f-68a77bc8cbd2"), "ar", new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "8 أحرف على الأقل، فيها حرف ورقم، وليست اسم المستخدم." },
                    { new Guid("1b00c567-b3f6-2aeb-73be-225050e44cb9"), "ar", new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("1b2a4711-eaf4-067c-d7a6-cacf178c503c"), "ar", new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "إرفاق الملفات" },
                    { new Guid("1b616b32-bb3d-9703-a449-e093e8e844f4"), "ar", new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "إضافة عقد" },
                    { new Guid("1b8b407c-d0d6-a289-6397-05be25db444f"), "en", new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "Choose a supplier" },
                    { new Guid("1ba6638c-5385-2d57-0ebb-c5cca4f90ffb"), "ar", new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "حساب هذا الشريك في التطبيق. وما يراه أي شخص من رأس المال يُحدَّد في الصلاحيات." },
                    { new Guid("1be91f58-09d1-680a-2bcf-434059164ea3"), "ar", new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "داكن" },
                    { new Guid("1c0564a0-4f59-daac-217c-202925930cf3"), "ar", new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("1c11b7d7-e069-0b2b-2ef4-a67430835f22"), "en", new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "New user" },
                    { new Guid("1c245e64-6423-331c-ed4b-b7de9730980a"), "ar", new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "سبب الرفض" },
                    { new Guid("1c709a19-039b-058f-2c81-4ed5138bcb90"), "en", new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "Outside the contract: it counts in the item's total, and does not come off the remaining." },
                    { new Guid("1c714eed-e194-96c3-172c-b29084aeb4d5"), "ar", new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "لهذا المشروع بند رأس المال بالفعل." },
                    { new Guid("1c80da2b-03c3-c8c8-8abb-7274da8f1a9c"), "ar", new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "تعذر فتح الملف." },
                    { new Guid("1c9de493-b47c-ff5c-5bde-caaba82c15d0"), "en", new Guid("25ec3ba2-371a-b6e4-8fb1-49d2cba0e327"), "Could not read the history. Check the network and try again." },
                    { new Guid("1cad9dfd-1dff-8acf-d4b7-af03de63d8ba"), "en", new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "Try again" },
                    { new Guid("1d0c0b18-decd-79e5-9acb-c571abe35e8e"), "ar", new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "الحسابات البنكية" },
                    { new Guid("1d2217b8-1b88-c5ea-baed-35fe0e740bb3"), "en", new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "Only a picture or a PDF can be kept." },
                    { new Guid("1d22c626-af58-e2fb-f54c-3f520687f432"), "ar", new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "محاولات كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("1d40d7e9-9b93-793a-26f0-2da01ec0a8ec"), "ar", new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "إضافة مستخدم موجود" },
                    { new Guid("1d9788c3-5124-5869-63ae-8644ef92f74a"), "ar", new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "حذف هذا القيد المعتمد؟" },
                    { new Guid("1d99d4ff-c3a1-f931-b2b5-eb0f28664540"), "en", new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "This person also works in another project, so this cannot be changed here." },
                    { new Guid("1db2a7f4-85c6-31e4-913a-4debaffc8b3e"), "ar", new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "إيرادات أخرى" },
                    { new Guid("1e171aa3-57a0-9bb4-22fd-a9715f281476"), "en", new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "Expenses" },
                    { new Guid("1e3330ab-ea34-a807-bbff-1974fcb9139f"), "en", new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "Issue report number {0}?" },
                    { new Guid("1e4b5bb0-edc6-c704-8c95-42f19d64fcb8"), "en", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "Product" },
                    { new Guid("1e79722a-e349-6e16-bf81-1e6d89752e97"), "en", new Guid("d08b5b27-7569-2575-31ea-7b34b9f9e878"), "Remove it?" },
                    { new Guid("1eea6e9f-ed9f-533d-7926-0c9f1598add0"), "en", new Guid("1aab11c1-eab5-db18-53fa-2364c62c7a33"), "These entries are sent by themselves once the network is back." },
                    { new Guid("1f1084ef-11f8-9df5-e5e3-ce743b8bdc4c"), "ar", new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "يجب أن يكون المستند أصغر من 25 ميغابايت." },
                    { new Guid("1f26116e-ec23-1c04-fe2d-fd8d4157b7a2"), "ar", new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "الحالة" },
                    { new Guid("1f77ea74-bed9-79bf-ff3f-2d1ff54c117b"), "en", new Guid("badfd342-36df-e95e-7240-7bf0cdd61c36"), "No document is kept yet." },
                    { new Guid("1fc9db1e-a140-30ae-94a6-de5301f312a4"), "ar", new Guid("29b6d7c2-7319-b4f6-4dac-3878accf9f24"), "إضافة عمود" },
                    { new Guid("206878f7-b25b-3829-67aa-b5b21bd45fb9"), "en", new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "English" },
                    { new Guid("20787cab-bf3a-2686-0d90-59eb2db4f1c9"), "ar", new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "إضافة الحساب {0}" },
                    { new Guid("208411dd-ce72-67c3-a31d-8d412d16b299"), "en", new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "Categories and items" },
                    { new Guid("209d2417-44f5-d683-e394-752c3d511abe"), "ar", new Guid("6178cdf9-ca43-4fa3-6129-2be822ab2878"), "عقد مفتوح القيمة" },
                    { new Guid("20a25d80-5e0e-ed88-74f3-701c588ef8e2"), "ar", new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("2102fe1f-64c3-cf99-10e9-8c1eccac3852"), "ar", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "رقم الفاتورة" },
                    { new Guid("210ebaf7-0b22-b12b-5ad0-b31c0c2c98c7"), "ar", new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "المبلغ أكبر من رصيد الحساب الآن." },
                    { new Guid("2119540b-c48d-a9b7-334c-ae3970962eef"), "ar", new Guid("4a677513-7f3a-40c5-6238-0710725ca3f4"), "تغيّرت القائمة. حاول مرة أخرى." },
                    { new Guid("2154434c-62c9-9f9c-e09c-aeca936ac2a6"), "ar", new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "جارٍ" },
                    { new Guid("2160835e-82fb-6d29-e008-01dfd0e5d412"), "ar", new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "حفظ في التنزيلات" },
                    { new Guid("219c0ede-cfac-b34f-2a5b-2dfbc66e31ca"), "ar", new Guid("85f17e6f-428a-bc84-cce6-e9dd93bcf644"), "توجد وحدة بهذا الاسم في هذا المشروع." },
                    { new Guid("22146e94-fe67-f051-9da8-2dd86d39af15"), "ar", new Guid("a182c392-1ffb-c5f0-ce52-81fd52979006"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("22391e2d-43d6-2517-ddca-a5241abfc811"), "en", new Guid("51c49503-a627-70d1-dfb4-89178f387607"), "Sign in" },
                    { new Guid("225734b9-d03f-0039-6ab9-f13027c19c5d"), "en", new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "Save" },
                    { new Guid("22755869-6663-f268-d51f-497c0788873e"), "ar", new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "الملاحظات 1000 حرف على الأكثر." },
                    { new Guid("22cef7e2-36dd-770d-ced0-8d5b085caed8"), "ar", new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "اذكر سبب الرفض." },
                    { new Guid("2317fb4a-267e-1a66-0e25-6ad9a7679716"), "en", new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "Enter a name of 1 to 100 characters." },
                    { new Guid("23308247-efaf-66c5-c5f5-d175f94a1c7c"), "ar", new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "تصوير الفاتورة" },
                    { new Guid("2334d59c-fb2b-0cb5-082a-042802e4cc7f"), "en", new Guid("5b4575cc-d63c-892c-ae74-34fcdeb6c3d2"), "No connection to the server. Try again." },
                    { new Guid("23375d30-be30-7728-7613-49a2ef55fc47"), "en", new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "Could not read the reconciliation. Check the connection and try again." },
                    { new Guid("235b8609-5286-fb5e-fe0d-937707400e17"), "ar", new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "حفظ" },
                    { new Guid("235d063c-bf70-6448-b2ec-fa9ad672b541"), "ar", new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "نوع الحساب" },
                    { new Guid("23723111-6d1f-43ef-6e30-3d194bd9b0eb"), "en", new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "Enter a contract value above zero, with at most two decimals." },
                    { new Guid("239cf61f-56bf-d755-6952-72a2077a6580"), "en", new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "Manage the project" },
                    { new Guid("23ad0c7a-5843-e59f-7422-d8df769f75c0"), "en", new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "Name" },
                    { new Guid("23f84925-6f6c-6530-6aab-1923098f37b0"), "ar", new Guid("443baa91-634b-1c28-3bcd-a0af3d91f1eb"), "قيد ينتظر قرارك" },
                    { new Guid("2430dbfc-5f1b-45ef-8811-612833d7ecd5"), "ar", new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "أُرسل للاعتماد." },
                    { new Guid("244047f7-5b2e-d51e-70eb-9e428069bf9b"), "en", new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "Partners are this item's tables, so its type cannot change." },
                    { new Guid("24623385-75dc-1b1d-f09d-4275a629af8f"), "en", new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "Enter a name of 1 to 100 characters." },
                    { new Guid("24787303-6dcc-7ffa-2fab-61938696a525"), "en", new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "The invoice was attached." },
                    { new Guid("24fc50d0-951c-b5c1-b71c-743af5799106"), "ar", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "الحساب" },
                    { new Guid("2507f7f2-32de-9ba5-9c66-b7a7219dd9e2"), "en", new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "No connection to the server. Try again." },
                    { new Guid("25198551-2c29-9579-5a95-a9f6ac99c0a9"), "ar", new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "طرق الدفع" },
                    { new Guid("252dd856-9447-ec03-bcaa-cb6bbfcfa9f9"), "en", new Guid("6e6b33e7-a92e-ec64-29d9-bf2395e6ad33"), "Add contract" },
                    { new Guid("254a86f6-7f8c-3a8b-0cb7-2bcac036d61f"), "ar", new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "تعديل" },
                    { new Guid("25958a45-636a-ade3-55e1-23a2f6d2f54e"), "ar", new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "أي ملف أصغر من 25 ميغابايت." },
                    { new Guid("25ad6845-2996-1d4b-049a-adec75ec8f75"), "ar", new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "لا مطابقة بعد." },
                    { new Guid("25d53029-97f7-fc40-34c8-aab53e019a6a"), "en", new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "Show everything waiting" },
                    { new Guid("267604cc-9d02-d756-d785-7fd2e3b2d03a"), "ar", new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "اعتماد: {0}" },
                    { new Guid("2679e75f-a430-fa83-92a9-3762735e6c2c"), "en", new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "This could not be done." },
                    { new Guid("267bc664-e781-2e21-8b08-3c4274e4220c"), "en", new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "Holds now: {0}" },
                    { new Guid("26df8882-7ec8-aaf4-881f-fbab3ac7b634"), "en", new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "Approved by {0}" },
                    { new Guid("26fde679-57b8-9897-9447-4325b22141c5"), "ar", new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "مخصص" },
                    { new Guid("272d3ca4-41dd-217c-e709-209d69047098"), "ar", new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "حدد ما يستطيعه هذا الشخص في هذا المشروع. إلغاء «يرى المشروع» يخرجه منه." },
                    { new Guid("27551e78-834a-b2be-5c05-da732d084acd"), "en", new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "Partners and capital" },
                    { new Guid("27589247-c918-bdbf-ba5a-5014b4d3537f"), "ar", new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "ما تستطيعه في هذا المشروع. يغيّره مدير المشروع فقط." },
                    { new Guid("27bfc3bc-4fab-e70c-06e0-7c119ec9eb04"), "en", new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "The default template" },
                    { new Guid("27f4c817-48d8-bfb4-a1ba-8a7c4cb8eca6"), "en", new Guid("b196425c-3768-95bb-5c37-17a7807feb19"), "Category name" },
                    { new Guid("281181ed-562a-0723-ed54-132a566f1fee"), "ar", new Guid("1ca0ec62-9701-e642-fa33-5a96ff5f1c9e"), "الخيارات، كل خيار في سطر" },
                    { new Guid("282e2102-a165-f0e1-7b59-00b3394a0c10"), "en", new Guid("dedfe861-7fcd-cf05-2929-9bb938f970c7"), "That account is not in this project." },
                    { new Guid("28469952-9edd-366a-04dc-327698d9bda4"), "en", new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "No connection to the server. Try again." },
                    { new Guid("288b3ed2-44de-4224-4401-a4fd33dd2249"), "ar", new Guid("6eb92667-c6c9-f895-793a-7b69eb0669ab"), "تسجيل مشروع جديد" },
                    { new Guid("28971838-a830-84b5-20ad-3b5074952704"), "en", new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "Enter a name of 1 to 100 characters." },
                    { new Guid("28b8f94f-21b6-a554-a8da-6a5a742f7e6a"), "en", new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "Total" },
                    { new Guid("28c68a9b-b43c-e11b-a3db-4747ce7b04a3"), "en", new Guid("e167ab2b-b928-ab95-5783-acfe384a0f2e"), "Account" },
                    { new Guid("28e7c9e3-024d-b988-d0c8-26f5783ed79b"), "ar", new Guid("726d5c9b-bcc3-4192-453c-3eca88284762"), "تعديل {0}: {1}" },
                    { new Guid("28f60970-e91b-5d53-df85-bc71ff454cb5"), "ar", new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "أكمل ما ينقص هذا القيد قبل الإرسال." },
                    { new Guid("29050156-82f7-1eae-7a5c-2cbe0b75973e"), "en", new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "You have no project to enter expenses in." },
                    { new Guid("2924c1e9-3455-6329-eefc-c20015df9d83"), "ar", new Guid("19c6ced2-fb75-55c7-df46-902db5a79c09"), "الوصف" },
                    { new Guid("29815d63-2083-1ef2-b576-afd54f6bffab"), "ar", new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "إعادة الإرسال" },
                    { new Guid("29901006-c674-5027-89b3-3434b3969906"), "ar", new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "رُفض هذا التغيير." },
                    { new Guid("2a6ffd43-a6d2-9a19-2806-2d3ba4f09be0"), "ar", new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "أدخله" },
                    { new Guid("2ab4d5c5-614e-362d-d207-fbb7c5a34bdc"), "ar", new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "في هذا المشروع قيود بانتظار الاعتماد. اعتمدها أو ارفضها أولاً." },
                    { new Guid("2ac24559-5a33-259a-1a18-275eeef1fad6"), "ar", new Guid("75732723-aba7-1685-bd11-04205305acb8"), "حفظ" },
                    { new Guid("2b366df5-55ed-956b-e04c-3a5b372f8804"), "en", new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "Enter at least one choice, of up to 100 characters each." },
                    { new Guid("2b67fe74-5528-3992-bcd0-3e7a1021f46b"), "ar", new Guid("531379ae-8cde-2467-8484-33a498b15916"), "كلمة المرور الجديدة" },
                    { new Guid("2b94ed2d-2fe6-83a6-c2ab-5457442fd805"), "ar", new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "صورة الفاتورة" },
                    { new Guid("2bd0e7d9-7feb-df5a-7bf0-b2dc0647b86d"), "ar", new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "اكتب اسم مستخدم من 1 إلى 50 حرفاً." },
                    { new Guid("2bd3db63-bb80-8b0c-0632-7c6948235300"), "en", new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "Custom period" },
                    { new Guid("2bf004c2-bc55-cbed-49ee-42f3fce6a5d8"), "en", new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "Clear filters" },
                    { new Guid("2c5baad7-2442-ae4f-712b-bb71b86a5407"), "en", new Guid("5a51514e-0576-d48c-90bd-ef8839d2b98a"), "Bank accounts" },
                    { new Guid("2c84e7a6-efe3-7964-f07a-b0fa11204277"), "en", new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "This permission cannot be given in a project." },
                    { new Guid("2c93f1b1-55c0-5b36-a327-093f24ae119d"), "en", new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "Without it, only from the accounts in their custody." },
                    { new Guid("2c9fd444-231d-e12c-678e-12af42f77887"), "ar", new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "رفض" },
                    { new Guid("2cb2b3b6-de2c-891d-1a09-8f21e8a6e737"), "ar", new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "لا يوجد جدول مطابق." },
                    { new Guid("2cb3a343-530f-50f8-72f8-7a83cc651996"), "ar", new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "تعذّر فتح الكاميرا. اختر الفاتورة من ملفاتك." },
                    { new Guid("2cba0c38-b315-245e-dc47-2d09d0065507"), "ar", new Guid("d32a2de0-209f-c23e-6578-86f483e06f7b"), "يجب أن تكون الصورة أقل من 1 ميجابايت." },
                    { new Guid("2cd5b879-4ca7-cc70-c26a-75721504586e"), "en", new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "Enter a name of 1 to 100 characters." },
                    { new Guid("2da36dce-3c83-bbd3-2753-bbea5efbe22a"), "ar", new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "تاريخ" },
                    { new Guid("2dba8ecd-54f7-38e6-7c05-351a3812354a"), "en", new Guid("b402a5a1-c456-729a-0d64-18cf0c439e73"), "Take a picture" },
                    { new Guid("2de39c18-51d9-1ba3-6999-8b678d3a0f49"), "ar", new Guid("3cb6f531-aeb0-f3a0-c22f-7cf5ec486084"), "بند الإيرادات لا يطلب أعمدة." },
                    { new Guid("2e095970-ebf6-f721-2dd4-83933473db8e"), "en", new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "Role name" },
                    { new Guid("2e4e68bb-e544-1020-5b36-75b89607dba9"), "en", new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "Entered by" },
                    { new Guid("2e8f0b1c-564a-71e9-0c87-eddeb3dbedb7"), "en", new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "This project is closed." },
                    { new Guid("2f269167-9dbe-50fa-a95b-d4048a566e2a"), "ar", new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "رأس المال" },
                    { new Guid("2f368baa-4303-b9e6-0b4b-952fbd670f75"), "ar", new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "فرق المطابقة مع الأرصدة" },
                    { new Guid("2f48c14b-20cc-5567-9008-15088029f85d"), "ar", new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "المبلغ لا يطابق الفاتورة" },
                    { new Guid("2f4ae0b3-2478-0c99-50c8-a6afa3e85cda"), "en", new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "No contracts." },
                    { new Guid("2f55b30b-3fa7-74ca-b2bf-7906150e78d8"), "en", new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "Change type" },
                    { new Guid("2f76b791-3886-e852-4cf5-312a600d7dd9"), "ar", new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("2fa225cc-a6be-2ea6-0785-529de756469c"), "ar", new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "إضافة واختيار" },
                    { new Guid("2fc0c662-b7cd-be4b-b658-ad5be4f14aec"), "ar", new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "لا جديد." },
                    { new Guid("2fe21422-63b6-1782-b29a-934f085e976a"), "ar", new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "الحركات" },
                    { new Guid("301f3e9d-68bd-7ad1-ba03-9e08433295f9"), "en", new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "Surplus (what the accounts should hold)" },
                    { new Guid("3042eff0-ba9f-2cb5-8bde-09ff6a02d391"), "ar", new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "إلغاء" },
                    { new Guid("30aafa0c-fe08-5359-fc11-2a58f5724dd1"), "en", new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "No login" },
                    { new Guid("30ad34e1-f173-0b01-e898-449911fbf599"), "ar", new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "اعتمد المالي هذا القيد قبل وصول تعديلك، فلم يُطبَّق تعديلك." },
                    { new Guid("30f76099-5b8a-18f1-9531-e3453a7dfb6f"), "ar", new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "حفظ" },
                    { new Guid("3122d1d1-1453-7af9-d90a-3e3337be3f89"), "en", new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "This item only" },
                    { new Guid("318b2e5d-a71b-bd65-783a-6a65f36a2080"), "en", new Guid("380e642c-9c07-1a59-fa8e-00ac845ab893"), "Sent for approval." },
                    { new Guid("3194494e-f789-a642-1cae-3623afddcb1e"), "ar", new Guid("30a6a806-0d33-a469-51d4-aafbe7e65f4d"), "ينتظر هذا القيد قرار المالي، فلا يتغيّر الآن." },
                    { new Guid("31c261e4-1282-737d-25f2-357e045933d6"), "ar", new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "بانتظار الاعتماد" },
                    { new Guid("31f22691-54a1-6fc9-5e37-2892bd568a5b"), "en", new Guid("b102c8cb-efc2-f411-0219-59b1e745861b"), "Fill what this entry still lacks before sending." },
                    { new Guid("32077232-7d2c-03e6-7827-370f417338c8"), "en", new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "Someone already decided this entry." },
                    { new Guid("32179368-0d1d-84b6-fcdd-d899caf3d8e4"), "en", new Guid("ab7ceffa-b317-2236-1ef2-6c6987be7af5"), "Accounts and reconciliation" },
                    { new Guid("32296e23-e84b-e1d7-56f5-5d374ed44256"), "en", new Guid("2c47eb7b-ab0d-272b-3b21-e0985b156e95"), "Switch project" },
                    { new Guid("322f0e08-e8a6-23cc-5ffd-8c980e0c320a"), "en", new Guid("d67edc90-0193-bfbb-457a-8a87e27709ca"), "Finance approved this before your change arrived, so your change was not applied." },
                    { new Guid("325f1b4c-18a8-84bc-db9d-e816d18f4534"), "en", new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "after approval" },
                    { new Guid("32755b57-4823-0526-ba64-bb34b002aa09"), "en", new Guid("46821251-c294-7141-3e30-25a81e924b85"), "No connection to the server. Try again." },
                    { new Guid("328a90cd-7775-101d-2339-ecebd31005b2"), "en", new Guid("d1ea0ec5-55a3-250e-99b2-13dd222079ac"), "Your session ended. Sign in again." },
                    { new Guid("32ba74b5-198d-4e98-1601-ea8a617ebcfc"), "ar", new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "أدخله {0}" },
                    { new Guid("3308bc99-1fe4-57fd-32d3-4d34abb4ce29"), "en", new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "Could not read the payment methods. Check the network and try again." },
                    { new Guid("333035c5-99f6-df96-d24d-5caa61992920"), "ar", new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "تعذّر التغيير. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("33616a11-2b8d-d277-7512-8785320e996d"), "ar", new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "تعذّر حفظ المصروف." },
                    { new Guid("33cec58f-f17f-ec8b-196f-200d1e78db83"), "en", new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "Disable" },
                    { new Guid("3444d843-0b82-f08c-f421-f027bf0cf64c"), "ar", new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "سند الاستلام أو إيصال التحويل" },
                    { new Guid("3466bc9b-1e3e-fe1c-1b56-02a0189e625d"), "ar", new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "اختر حساباً يحق لك الدفع منه." },
                    { new Guid("347526c5-7a89-31e1-0ca3-fdc0b77a6961"), "en", new Guid("f6cceea5-ccf2-c1a2-e144-8a19c88d05b7"), "Choose an account you may pay from." },
                    { new Guid("347be3b6-2650-f9c5-c8c1-1cedfb05e279"), "en", new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "This project is finished, so nothing in it changes." },
                    { new Guid("348910dc-dafd-320d-f96e-17a6b3661905"), "ar", new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "رصيد عهدتي" },
                    { new Guid("34891a4c-1dd4-73ee-27c7-30ae57f1f9f2"), "en", new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "Mine" },
                    { new Guid("34cb8af5-110f-9768-fcd3-fccba97ad946"), "en", new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "Reports" },
                    { new Guid("3521bdfb-9682-8d84-ff2c-617976ec9c64"), "ar", new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "تم حفظ المستند." },
                    { new Guid("35742bd6-0307-d220-896c-2547f138bff5"), "ar", new Guid("32492e3e-b952-a698-a19c-971373c185e4"), "رُفض هذا التغيير." },
                    { new Guid("35903837-025e-bc6c-7e73-7c042765c7e9"), "en", new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "Add a document" },
                    { new Guid("35b99f1b-b2e3-3691-a79c-b5cd8f230bab"), "en", new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "Disabled" },
                    { new Guid("35d9c4dd-ace8-15c7-6b9e-9eadb39adf1d"), "en", new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "Description saved." },
                    { new Guid("36044e90-1f94-6a8f-1be4-c5bd3e2a1de2"), "en", new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "Payment method name" },
                    { new Guid("3654c337-1a43-1e43-80b7-617e072188ae"), "ar", new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "تجاوز العقد" },
                    { new Guid("36661599-e814-80b8-2bb6-dc1b66ee8caa"), "en", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "Movement type" },
                    { new Guid("3669e773-c686-f408-6244-03e64f12f3f1"), "en", new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "Rejected" },
                    { new Guid("3682633d-086d-2cf3-2391-b09233d41b8b"), "ar", new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("36b06183-7d8d-53f1-84db-bf1bb1ad8127"), "ar", new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "اكتب اسم المستند في 200 حرف على الأكثر." },
                    { new Guid("36d8c20b-c689-f2dd-ab27-3f928773bb2f"), "ar", new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +." },
                    { new Guid("36ec9ba4-e1bb-1782-3014-5944541d9262"), "ar", new Guid("4c868806-5279-b16b-7f09-ed6cd7e8b8f6"), "عرض كل ما ينتظر" },
                    { new Guid("372a4c05-537b-1f7a-a6cb-2c304c7c8002"), "en", new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "Items" },
                    { new Guid("37987c1b-10fb-1f47-e59c-a1d31626a59c"), "en", new Guid("53945eef-ca15-df6a-f8d8-901efa608b7c"), "Download" },
                    { new Guid("37bc8571-e4db-517a-46f8-63d7277fae6b"), "ar", new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "{0} أصبح مع {1}" },
                    { new Guid("37d92a93-331b-1055-425f-dcb139479d91"), "en", new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "No connection to the server. Try again." },
                    { new Guid("38060b25-7a69-e435-2f93-c4e0dcb4a2b1"), "en", new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "You will need your username and password to sign in again." },
                    { new Guid("380f2856-0edb-7026-540f-ac27975e4e54"), "ar", new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "القيود" },
                    { new Guid("38646627-4699-0b85-957e-8138cab4482a"), "en", new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "History" },
                    { new Guid("387d53d4-e312-064b-8ff4-8a4b3eaaedce"), "en", new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "Item total" },
                    { new Guid("38a21b97-90ba-d6bd-7aa2-f2439749b1b4"), "ar", new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "هذا الملف مرفق بمصروف آخر." },
                    { new Guid("39132977-0184-7595-23c6-0eceed12cd78"), "en", new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "This could not be done." },
                    { new Guid("39290c37-9535-99cc-8a6b-35e76c645283"), "ar", new Guid("5d44071c-4a2d-884f-12ea-8a088050a468"), "تعذّر تنفيذ ذلك." },
                    { new Guid("39873145-6091-6ad4-ebeb-049e648bd62e"), "en", new Guid("d32e4883-f6ce-3aee-0a72-b6a985054b28"), "Reject" },
                    { new Guid("39882be5-77c6-ee30-84b4-0396ba31a973"), "ar", new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "{0} هو مشروعك الافتراضي الآن، ويُفتح عند الدخول." },
                    { new Guid("39b8e0c1-f63c-2507-adcd-402f32d51626"), "ar", new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "ر.س" },
                    { new Guid("3a0588a7-d759-de4a-7071-f47e2f087a93"), "en", new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "Account name" },
                    { new Guid("3a09b055-42a9-3a82-585d-6bfcace2eca4"), "ar", new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "المتبقي بعد الدفعة" },
                    { new Guid("3a6ee065-0acb-c978-ceb4-9f363647374a"), "ar", new Guid("f3cb7312-d757-9241-9294-fb9f3991e990"), "يمكن حفظ صورة أو ملف PDF فقط." },
                    { new Guid("3ab7caf6-b51d-590f-a6d9-b4f5dc29b4ad"), "ar", new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "صورة أو PDF" },
                    { new Guid("3ac93d3f-8a62-cde3-9bf2-803c92011b99"), "en", new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "A contract names this item, so its type cannot change." },
                    { new Guid("3ae42790-423a-73e8-a74a-bf251b414c75"), "ar", new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "لا يرسل هذا القيد إلا من أدخله أو المالي." },
                    { new Guid("3b1e4a56-a7bf-5801-7c87-d3d47f5705f9"), "ar", new Guid("6fc4c8a4-668e-fe16-d5d8-b81aebd1606c"), "رأس مال كل الشركاء" },
                    { new Guid("3b6258bb-7b88-0f8a-c2b1-9a920d4a4c4a"), "en", new Guid("53a15eb4-21d2-2df7-92c6-6ff00ad5f433"), "Purchases + contractor payments" },
                    { new Guid("3b8d0e13-2dee-42dd-9262-626de90992ec"), "ar", new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "إرفاق بالمصروف المحدد" },
                    { new Guid("3b92cf4f-4399-afdb-e1c1-fd65582728b7"), "ar", new Guid("f765a18e-4438-22a7-7c83-34c6c52556aa"), "تعذّرت قراءة العقود. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("3b9fe6f5-dcd2-ca08-5dca-8c3e5edea319"), "ar", new Guid("49918492-7c51-8293-228b-6515961524d9"), "اذكر السبب." },
                    { new Guid("3be681f9-b871-25f4-e0d4-f47a864b86e0"), "en", new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "Columns" },
                    { new Guid("3c319db8-027f-e7a4-2605-8f25f8c22b5a"), "ar", new Guid("666082ac-4221-7539-3047-ab60621f529c"), "المزيد" },
                    { new Guid("3c3356dc-3043-1024-6602-f7c34a0928a4"), "en", new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "Save" },
                    { new Guid("3c8b6ed6-25aa-82c8-16e8-16f7693254c7"), "ar", new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("3cb92c85-c471-1548-2b32-cd2e9e319124"), "en", new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "The default template decides what new items of this type start with. Existing items change only if you apply this to them." },
                    { new Guid("3cbf9aa9-b363-af0d-1a0d-75315fd6558d"), "ar", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "الإيرادات" },
                    { new Guid("3cdc625a-c1a7-ea31-6477-fed56a1b1fd7"), "ar", new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "هذا الشريك ليس في هذا المشروع." },
                    { new Guid("3cef95e3-7edd-406a-526d-393293701900"), "ar", new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "الفعلي في آخر مطابقة: {0}" },
                    { new Guid("3d2b74b3-6e67-f200-669d-3c233f673ffb"), "en", new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "Could not read the accounts. Check the network and try again." },
                    { new Guid("3d3020e9-002e-e254-0252-b2ce4977a76a"), "en", new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "That person is not in this project." },
                    { new Guid("3d99b2a2-3292-4ead-2398-247109cce373"), "ar", new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "حذف القيود المعتمدة" },
                    { new Guid("3dc04493-e929-0bb4-1223-bc4346454f03"), "ar", new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "لا توجد تصنيفات بعد. أضف أولها." },
                    { new Guid("3df3c585-78d9-6f9d-c618-5e51af2f3075"), "en", new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "An account is no longer in the project." },
                    { new Guid("3e53413a-f45c-c5a7-f646-c88e19bf6d4f"), "en", new Guid("4827533f-5397-6f3f-72bc-b000f2264a9f"), "{0} is now your default project. It opens when you sign in." },
                    { new Guid("3e554d08-bf9d-2b80-22c7-f429c30841f4"), "ar", new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "إدارة المستخدمين" },
                    { new Guid("3eb23b5b-fd16-1f57-0f5f-334051a6e206"), "en", new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "Item, supplier, who entered it, number or amount" },
                    { new Guid("3ef5499c-aee8-ca07-94dc-a6285b6a58cd"), "ar", new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "تفعيل" },
                    { new Guid("3f1f6a1f-06c3-9d56-47e4-724c1b5644e1"), "en", new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "My permissions" },
                    { new Guid("3f2be9a4-93e6-2c34-ede6-6e5391f9fd1c"), "ar", new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "هذا الملف ليس في هذا المشروع." },
                    { new Guid("3f56e07d-596d-ef3f-827a-f27a040975cf"), "ar", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "لا مصروف بلا فاتورة تحت هذا البند." },
                    { new Guid("3f7584f0-ded4-b107-9da9-4cb667009ae6"), "ar", new Guid("27d5e8ae-ac71-1977-0de4-6b4fee3d166e"), "REDIX" },
                    { new Guid("3f90be0e-f505-e27b-d744-336d177d557f"), "ar", new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "حذف" },
                    { new Guid("40161a06-bbb8-c042-0e96-45bd4735dcb9"), "ar", new Guid("0a72ae0c-0fa9-2c23-bbab-f9afc100b04d"), "إجمالي البند" },
                    { new Guid("401c8cca-a079-3e8e-456c-d8c40d4aa32d"), "en", new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "Purchases" },
                    { new Guid("40334390-4fa3-9236-1fa2-098fbe4d0e41"), "en", new Guid("d6dd0fc7-b558-91bd-9b9f-bf48c8e2cde9"), "Cancel" },
                    { new Guid("40948408-1d44-86d2-8337-d200bf82d4c0"), "ar", new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "النوع" },
                    { new Guid("40a32818-66b6-8438-813f-65754333fd20"), "ar", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "اختر البند لتظهر مصروفاته." },
                    { new Guid("40babdd9-1424-a07f-7a62-73d37a70eaa1"), "ar", new Guid("6189d8ce-f0d4-bff7-cf18-8c219ceaf917"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("40eff482-ba9e-dfc4-5d89-2e2c87694911"), "ar", new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "إضافة المستخدم" },
                    { new Guid("41405262-df7c-3c8b-fdfb-e8e631c21de3"), "ar", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "اسم الصنف طويل جداً." },
                    { new Guid("41572b00-4ec1-ef5e-5aa2-92ccb7c06d66"), "en", new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "Drag to reorder. This is the order of the periodic report." },
                    { new Guid("41862f4c-661b-5d95-c629-8f00a0937b4f"), "en", new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "A draft by {0} names them." },
                    { new Guid("41cf1f59-890c-c866-c203-4da297d3830c"), "en", new Guid("e113ddf6-29dc-cbcc-8d08-6e6b221f4428"), "Capital" },
                    { new Guid("4278aa6c-1d4e-bef1-c377-5695d3ef30b9"), "en", new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "Your drafts naming them are deleted with them:" },
                    { new Guid("42c3e0ae-58be-2da9-d2f4-6bac2dde9c1e"), "en", new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "Number" },
                    { new Guid("43095f56-19bc-18a8-547a-e9c76293573d"), "ar", new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "هذا المستند ليس في هذا المشروع." },
                    { new Guid("431730d8-ae62-73dd-e18d-0a0c6ab23f4c"), "ar", new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "تعطيل" },
                    { new Guid("434311bd-4a54-392b-6bcf-e6e48504337e"), "en", new Guid("40750c8a-e1e9-ca92-7809-36cb4a631cb0"), "No reconciliation yet." },
                    { new Guid("435fbe16-ecb6-5264-b242-245654ea7164"), "ar", new Guid("1a68b1f2-5740-d5e6-fda7-7854e1428c8b"), "هيكل المصروفات" },
                    { new Guid("43658527-3e52-7bb6-c8f9-7b6fa0f71bb6"), "ar", new Guid("5e6d14f6-9dfe-d059-f357-5038bdc193d1"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("443a77c0-8ef8-fa94-ade1-6086bf39ab58"), "ar", new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "وصف المشروع (اختياري)" },
                    { new Guid("443b2d31-c9c4-7214-cbde-71b4a0e0f983"), "en", new Guid("27a7104a-afd2-424f-1a1d-33600d7f049d"), "Entered by" },
                    { new Guid("44519fdc-d77f-895e-ee9f-ab385469bd1d"), "en", new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "Enter a name of 1 to 100 characters." },
                    { new Guid("446c4ca9-bcb0-419f-9fc4-36c73099562e"), "en", new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "Move money" },
                    { new Guid("449fcb9f-8f5a-3d13-f1c5-54e9ffb328be"), "ar", new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "إرسال للاعتماد" },
                    { new Guid("44b7f447-f095-ece2-d777-1535fd54a35e"), "en", new Guid("e29ac4a4-aa9c-5e60-66bc-bb3cc77defd2"), "Reason (required)" },
                    { new Guid("44c40e8f-3807-b8d4-f80c-f31bc7159333"), "en", new Guid("73edffd2-c6ee-2428-cd05-2ac078532a09"), "This could not be done." },
                    { new Guid("44e6dde1-cc1d-635a-b8b7-4ed2c1f66101"), "ar", new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "المشروع" },
                    { new Guid("4526860c-d2fe-f2ea-1eac-2ae72eecea42"), "en", new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "Could not read the suppliers. Check the network and try again." },
                    { new Guid("4538616f-59a3-cf02-5dc7-44e579d9c65c"), "en", new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "A file must be smaller than 25 MB." },
                    { new Guid("459b6e4d-681a-e768-1672-3a84823203b6"), "ar", new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "{0} {1} إلى {2}" },
                    { new Guid("45cd37a8-56af-fe3a-4d76-36ddece53523"), "ar", new Guid("d9ed0b5b-f493-d1b0-1b89-07fcafa70e01"), "فترة مخصصة" },
                    { new Guid("45df6f33-3423-3174-92a3-8f0b8badad1f"), "en", new Guid("7c25b041-baaf-90c2-86f7-5f747f1cd0d7"), "Account" },
                    { new Guid("46253d83-6248-a436-b5a1-8961fd9bde78"), "en", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "Description" },
                    { new Guid("463b4b29-7cc8-0c26-5dfa-c77bc476263f"), "ar", new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "تعذّرت قراءة الملف. اختر ملفاً آخر." },
                    { new Guid("465e2404-09f6-8303-578a-c669cf18aa29"), "en", new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "Enter a username of 1 to 50 characters." },
                    { new Guid("466ab419-06f6-009d-1104-0bb20f4db6f0"), "en", new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "Apply to all items" },
                    { new Guid("467aa725-2bac-e2f2-44d0-10e6dfdb3734"), "en", new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "Disabled" },
                    { new Guid("46a2ed3a-2dc9-27fc-b5a2-4da491272e38"), "ar", new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "الحساب" },
                    { new Guid("46a579cf-a469-0b9b-068b-fb03907c3bf7"), "ar", new Guid("4d3f6a98-3b04-5930-e54b-b0b5386bffb9"), "اكتب اسماً من 1 إلى 100 حرف." },
                    { new Guid("46b64c20-2b81-7a57-d946-fd080564a7a7"), "en", new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "Existing supplier:" },
                    { new Guid("46d429e9-91ff-ab3f-a5c3-6cc7e61e32d6"), "en", new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "Remove" },
                    { new Guid("46d98895-fe5b-ddb2-e53f-43a93274f540"), "ar", new Guid("ecd04b2a-a691-f32a-2b0b-973f9c008b89"), "الجوال (اختياري)" },
                    { new Guid("46f448aa-8ba6-ee59-f620-17e6b19a2ed0"), "ar", new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "حذف الشريك؟" },
                    { new Guid("46fca222-75ab-9a64-f590-5d22e9323f40"), "en", new Guid("bd6c01ee-b87b-def0-a719-6f05a583e17d"), "Remaining after this payment" },
                    { new Guid("4715a33f-814c-637f-2761-52c6980886ac"), "ar", new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "البند الافتراضي (اختياري)" },
                    { new Guid("4758ca2d-8d17-6839-fe36-a59e11b66946"), "en", new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "No accounts." },
                    { new Guid("47a0d312-b1df-5b6b-a174-b23da1dbe394"), "en", new Guid("184f2cd2-3e04-c024-473c-e267e5b10d11"), "No categories yet. Add the first one." },
                    { new Guid("47b7d4c5-b8b2-0602-891c-e8747f5e994e"), "ar", new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "هذا الشريك ليس في هذا المشروع." },
                    { new Guid("480cde1f-ec1c-df55-65ad-d0857a68ec85"), "en", new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "Added the partner {0}" },
                    { new Guid("48457951-e286-f89f-0ab8-aee07a66da24"), "ar", new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "التصنيفات والبنود والجداول والأعمدة." },
                    { new Guid("486f5007-354e-5cde-fed4-214f32923336"), "en", new Guid("a7873a2f-8e1f-0d31-e518-f5c404bf5741"), "Give the document a name, in 200 letters at most." },
                    { new Guid("48ecb1c6-2d74-1a48-87a5-f5a39260e539"), "ar", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "تحويل بين الحسابات" },
                    { new Guid("48ede494-1658-ee76-7203-b3ab21167e7f"), "en", new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "Could not turn it on. Set up a fingerprint or face on this phone, then try again." },
                    { new Guid("4910a692-db3a-6a64-8b1e-285a54fda81d"), "en", new Guid("db2e5cba-5b40-d921-38c4-47e4fde8b5db"), "Add and choose" },
                    { new Guid("49573c45-24a9-23e1-82ac-ff89d8cedea7"), "en", new Guid("baad61b8-835d-de2c-6889-754d605598ca"), "Pay from any account" },
                    { new Guid("49ab3c64-49aa-ec85-00f4-e054f0505e80"), "ar", new Guid("48b55fa7-53ef-a899-30c3-cb14c2213ede"), "تعديل الحساب" },
                    { new Guid("49accb3e-48f9-31a0-f342-911c8d471fdf"), "ar", new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "لا يوجد مورد مطابق." },
                    { new Guid("49b79303-ffb5-f8a2-aa1a-fed2994c4c57"), "ar", new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "تعذّر العثور على هذا القيد: ربما حُذف، أو هو في مشروع لست فيه." },
                    { new Guid("49b88906-65b7-e97a-03a7-b77839b70e35"), "ar", new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "بانتظار الاعتماد" },
                    { new Guid("4a060192-53ee-f888-519a-df94387b5b47"), "ar", new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "هذا القيد مُرسل مسبقاً." },
                    { new Guid("4a30c706-b1fe-298a-3ac6-5c0b52fc3ef3"), "ar", new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "العهد النقدية" },
                    { new Guid("4a48352c-d1ce-e8bc-5fe2-a97dc6759744"), "ar", new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "الشريك" },
                    { new Guid("4a4a6268-c323-067a-3622-36dfdab57fc7"), "ar", new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "عدد القيود المعروضة" },
                    { new Guid("4aa27900-4bf0-8c10-8c8b-86195647bff3"), "ar", new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "العمود الأساسي لا يُحذف، ولا يتغير منه إلا اسمه." },
                    { new Guid("4aa98f96-75ac-2ae6-cec9-f1709d7133dc"), "en", new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "Type the balance of every account." },
                    { new Guid("4b018095-5cb7-213b-98f3-92879e976176"), "ar", new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "اختياري" },
                    { new Guid("4b06f579-83c0-eb52-d25f-2f106c47ca1d"), "en", new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "No user has this username." },
                    { new Guid("4b527f46-f4f9-6b26-9be2-0aaf5707e9bc"), "en", new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "Accounts" },
                    { new Guid("4b7095cb-e444-982d-4e80-d75142520183"), "en", new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "Open REDIX" },
                    { new Guid("4b716d34-3ce8-0d24-9bb5-04681af2ab53"), "en", new Guid("7cfd9b95-5a06-e0e4-6662-505c88b4302c"), "Enter a name of 1 to 100 characters." },
                    { new Guid("4ba46a6e-b644-a163-270c-1e7188679b11"), "en", new Guid("fbe18f17-7aca-e801-0031-793076552587"), "Quarter" },
                    { new Guid("4bc91b90-0791-5b58-c656-fb2f12c46a47"), "ar", new Guid("9acc9a21-9abe-5e0b-3c47-5a58043dd712"), "لهذا العقد دفعات، فلا يتغير مقاوله ولا بنده." },
                    { new Guid("4bf7c0c2-aeb6-3336-fb8b-00349b3b63c4"), "en", new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "A VAT number is a number of at most 20 digits." },
                    { new Guid("4c362fb6-4bd8-ffa0-3fd5-629ea30055b3"), "en", new Guid("25f79ab6-6305-7112-554a-c09ef0e99eee"), "Send for approval" },
                    { new Guid("4c616d31-bed1-d391-9d2b-a1ae970fc863"), "en", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "No." },
                    { new Guid("4c6d93f7-809c-8ad8-3629-b050816d64bb"), "ar", new Guid("df089073-6e5f-4d7c-9838-55d5df0fd63d"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("4c79b3f8-f4eb-4a61-43c0-62adf90e2c96"), "en", new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "The item's general group cannot be renamed or removed." },
                    { new Guid("4c7d3176-5677-9880-e2ef-17f1db36bb93"), "en", new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "Opening the app asks for it, and you stay signed in until you sign out." },
                    { new Guid("4cab34f1-8cee-3437-bb76-36922a8569a6"), "en", new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "Change your password" },
                    { new Guid("4cac6532-1c33-9bcd-0505-068fbc63c665"), "en", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "Outside the contract" },
                    { new Guid("4cd7e391-c11a-644f-23f1-581e915808bf"), "ar", new Guid("64c47648-79d3-d784-3faa-dcec68821de9"), "تعذرت قراءة المطابقة. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("4cd9a712-c7ad-99f7-7061-b957811b6793"), "ar", new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "مجموع الأرصدة الفعلية" },
                    { new Guid("4ce2ac01-8adc-17dc-f3c8-9648a2a17dda"), "en", new Guid("702656c0-d01e-e4f4-aeef-9e4564c3ddaa"), "Account type" },
                    { new Guid("4ce36ed0-86e8-249f-0599-583bad82b688"), "en", new Guid("42aba7c0-aa6b-a637-c1a9-628144f86b3e"), "A document must be smaller than 25 MB." },
                    { new Guid("4d0ec420-b829-c811-b576-920c93d178f8"), "ar", new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "اسحب لتغيير الترتيب. الأعمدة المقفلة أساسية للحسابات والكشف الضريبي: تُعاد تسميتها ولا تُحذف." },
                    { new Guid("4d3851dd-c698-e1cf-46c8-8e457e1a9c34"), "ar", new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "اذكر السبب." },
                    { new Guid("4d5fff76-1477-81dc-d9f5-5b5565a11806"), "ar", new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "مدير المشروع" },
                    { new Guid("4d64a021-be54-2a4b-95e2-de8b150566bb"), "ar", new Guid("1fed799b-9e9d-89b1-e8e3-be7608c77b0b"), "صلاحياتي" },
                    { new Guid("4dc21e06-42a9-6f17-d96f-ed405c48837b"), "ar", new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "حفظ الوصف" },
                    { new Guid("4de5fa8c-00e2-64ad-1ba3-99f49ec65a14"), "en", new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "That login is already another partner's in this project." },
                    { new Guid("4decc0a4-2985-7259-41a7-955a02ace1b5"), "ar", new Guid("39457e5d-d993-e945-7e48-548736d938d2"), "يعمل هذا الشخص في مشروع آخر أيضاً، لذا لا يمكن تغيير ذلك من هنا." },
                    { new Guid("4df36ad6-8f02-af10-ef8c-69d7e61a70ea"), "en", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "An account is no longer in the project." },
                    { new Guid("4e4da4bd-b738-b38d-65d4-a8485cb49214"), "en", new Guid("688f8817-21bc-4d73-fdf1-d7b7b77975dc"), "With a bill" },
                    { new Guid("4e98f9ce-ded1-b9ea-e26f-843d8ef1ff9e"), "en", new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "Could not read the notifications. Check the connection and try again." },
                    { new Guid("4e9aeff7-9a8d-2e0b-c1b0-b0dd1ec83844"), "en", new Guid("59926246-1594-60fb-c542-7c8b332640fd"), "Waiting for approval" },
                    { new Guid("4eb452f8-1e7e-9efe-1db5-d355f4b1546e"), "ar", new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "رُفض هذا التغيير." },
                    { new Guid("4ee59b92-f034-5383-e583-43a359df6b79"), "en", new Guid("14591419-de9f-6241-35d5-f33de2c90d77"), "No supplier matches." },
                    { new Guid("4f1afe05-9708-61a5-d3f7-3d010bc2c796"), "ar", new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "الجدول الفرعي للمشتريات، فلا يكون إلا لبند يأخذ مشتريات." },
                    { new Guid("4f2eb84e-eaaf-aebd-a72e-d21a44f6fee0"), "en", new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "Revenue" },
                    { new Guid("4f3c3c31-8d21-d6dd-34a8-de6f99ccb374"), "ar", new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "أدخل الكمية بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("4f6d5a3b-31b2-cf76-afe7-f874c1ca805e"), "en", new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "From" },
                    { new Guid("4faf3b4a-e2c7-30ab-f1e0-5347398e6127"), "en", new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "Removed." },
                    { new Guid("4fe0d45b-bada-bc47-9016-a1bd40f6d23f"), "en", new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "Item" },
                    { new Guid("4ffb7347-a7b7-b809-201f-6493d715e4b0"), "en", new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "Total of all" },
                    { new Guid("5002d330-f762-a54f-f666-d4af30207d5a"), "ar", new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("5007f4c9-6b81-dd0d-1204-2aaba8bced79"), "en", new Guid("c2c86997-0b2d-9b63-1f90-8a58ab3343d7"), "Saved, with the contract overrun." },
                    { new Guid("500e9ea9-5c04-5553-4a7e-f79933275c60"), "ar", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "يرتبط هذا بقيد في مشروع لا يمكنك تغييره." },
                    { new Guid("50256a83-ee73-f9bd-8abc-1426bdcfc3a8"), "ar", new Guid("84411198-0da1-63ee-0dff-15f9ebe1da55"), "اكتب اسم مستخدم من 1 إلى 50 حرفاً." },
                    { new Guid("50338268-3dfb-dd9a-3c78-9c0aea2a098c"), "en", new Guid("6ed616ac-0b1a-ba48-6b32-53ec1f53a1bf"), "Add a supplier" },
                    { new Guid("509149e7-bae6-f85b-cfab-ce48ad7efd41"), "ar", new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "رفض الخادم هذا التغيير." },
                    { new Guid("50a061a0-b5b3-fb25-e183-e6311159a40b"), "ar", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "إلى" },
                    { new Guid("50aa967c-017d-15cf-cb7f-91fc7f4c8bd5"), "en", new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "The documents could not be read." },
                    { new Guid("50c76735-8650-fb66-8df0-19d905c97439"), "ar", new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "يعدّل ويرسل ويحذف قيداً أرسله غيره، أو قيداً ينتظر، قبل اعتماده." },
                    { new Guid("50d20cae-4408-fdd2-39a8-47d8014b6fd8"), "en", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "Amount" },
                    { new Guid("50e3db7b-4c86-f7df-3d11-affe944993af"), "en", new Guid("110f45a5-bf83-028e-eb55-10d45e66b158"), "Save" },
                    { new Guid("50e985a8-3305-7a16-931c-9c25ff2c7fa0"), "en", new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "New password again" },
                    { new Guid("5127f1d4-aaf0-0c02-5306-6efa98c1f692"), "ar", new Guid("3fcd9339-243f-3cbe-fd7b-5b5ecafffb95"), "أدخل قيمة عقد أكبر من صفر، بخانتين عشريتين على الأكثر." },
                    { new Guid("51428f67-c525-7ce4-c883-d52a40432206"), "en", new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "They are signed out at their next action, and cannot sign in until enabled." },
                    { new Guid("5155c37f-54a1-18cc-606f-89fcbfd53f03"), "ar", new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "اختر كلمة مرور جديدة من 8 أحرف على الأقل قبل أن تبدأ." },
                    { new Guid("51d0682e-0e80-81a6-206a-aa1dbcd1768c"), "en", new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "All" },
                    { new Guid("520015e9-be6b-69ab-16b6-f810d9fbe647"), "ar", new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "الأول" },
                    { new Guid("52177e46-4d0b-4890-8b73-665ac066089f"), "ar", new Guid("1334383d-e3fa-4e9e-79fc-375601f92344"), "اختر طريقة الدفع." },
                    { new Guid("5232747e-71ea-3536-984e-a2d41608aca4"), "en", new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "Rename project" },
                    { new Guid("5282be38-4c0a-dca4-1d1d-0a8c014e4091"), "en", new Guid("d789c746-8ff3-9ef6-b5b5-b737ad7747eb"), "New category" },
                    { new Guid("5284c6cd-ad42-2fe2-758b-ab26b6604296"), "en", new Guid("e15cd528-79c3-d92c-d7c2-7146df9ad77d"), "Description (optional)" },
                    { new Guid("528d2dd3-7f1a-20fe-8a40-55ac99701dd0"), "en", new Guid("8337a03e-1096-507f-c92d-0e6cde9f22f2"), "Enter a name of 1 to 100 characters." },
                    { new Guid("52a794de-c72a-091a-14e7-c5bf6eab62d4"), "ar", new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "يضيف الأشخاص إلى المشروع ويحدد صلاحياتهم." },
                    { new Guid("52bc5dd2-764d-ebd8-dbee-37a32bd0a6e2"), "en", new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "VAT number (optional)" },
                    { new Guid("52d36e53-7194-c20f-2584-4f0fba64d6b3"), "en", new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "Appearance" },
                    { new Guid("53792625-fd5b-2d21-30ef-290f67988a2a"), "ar", new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "تم حذف الشريك." },
                    { new Guid("538c09d8-5500-f6bc-777c-307b54eb3edc"), "ar", new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "لا يقبل هذا المصروف فاتورة." },
                    { new Guid("53ec6b65-33b3-1612-9b7a-60fa12921bda"), "ar", new Guid("46821251-c294-7141-3e30-25a81e924b85"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("540d4f62-b9f6-1734-fab6-c4d279d128c7"), "en", new Guid("3d391363-227c-a3d1-1197-343359825878"), "Enable" },
                    { new Guid("5413b105-ff81-dda9-eab6-5ff0e881a869"), "en", new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "Settings" },
                    { new Guid("541b0f09-c1b1-dcfd-db58-224a26d77bf0"), "en", new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "Save the balances" },
                    { new Guid("542f775e-6703-e590-e114-497279f476fe"), "ar", new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "تعطيل" },
                    { new Guid("54376eb9-a582-9b74-e237-923b178fe856"), "en", new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "A cash custody needs someone who holds it." },
                    { new Guid("548bc422-73b8-dab0-577a-47f7c8601ba6"), "en", new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "Keep" },
                    { new Guid("549b8139-4dbe-0355-7f02-9561f6f4d77e"), "en", new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "The server could not change your password. Try again." },
                    { new Guid("54b7566e-df19-911a-587d-3fec3321a9f1"), "en", new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "Rejected: {0}, because {1}" },
                    { new Guid("54bdde86-da79-445c-ac1c-9ad69e73be17"), "ar", new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "اسم المستخدم هذا مستخدم. إن كان لك، سجّل الدخول وسجّل المشروع من مشاريعك." },
                    { new Guid("54beb956-04c1-7183-a384-1f4f43661ae7"), "ar", new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "اكتب السبب، في 500 حرف على الأكثر." },
                    { new Guid("54c9ba2e-2749-c51e-50c0-ccd1c56f7ef0"), "en", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "Type an amount above zero." },
                    { new Guid("54e3ab54-6a2f-2561-bd73-5bc4d81c01c7"), "en", new Guid("501441ed-9b5d-2cd1-5b0b-f9429cc42810"), "Capital" },
                    { new Guid("551a5636-c500-9bee-823f-1e76d3761df4"), "ar", new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "اعتماد" },
                    { new Guid("5548523f-fa04-ec88-a798-66102b09d8fe"), "ar", new Guid("96118bfd-4008-580d-d590-9a2bf23e8ea6"), "الأعمدة" },
                    { new Guid("557e9a49-f05b-f047-e5c3-394a09b66b44"), "en", new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "Report issued." },
                    { new Guid("55a55129-cf99-6c5e-9c96-0492a445c900"), "en", new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "Calculate a percentage for each table" },
                    { new Guid("55b02ed6-bb61-9ff8-add0-9d282000d545"), "en", new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "Cash custody" },
                    { new Guid("55b45fdc-f947-833e-1fb3-957cd14ea4fc"), "ar", new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "الشريك" },
                    { new Guid("55f26691-8729-b53a-ed7f-1c7a5ad7f141"), "en", new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "Disabled" },
                    { new Guid("55f602c4-4b1c-3db4-f7f9-f9f18bb77996"), "en", new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "This cannot be undone." },
                    { new Guid("562b0891-0800-e684-ec3b-0f528948a91e"), "ar", new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "حذف الشريك {0}" },
                    { new Guid("5652f47e-a4ff-58bf-1bb7-0b5f1d251317"), "ar", new Guid("dfb2bc2a-67b4-74e9-ac8f-11055235e2eb"), "تعديل الشريك {0}: {1}" },
                    { new Guid("569045a4-b515-3955-379c-4e55af8f5fac"), "en", new Guid("ff64cabb-c86b-492a-c8b8-d821442416da"), "Could not change this. Check the network and try again." },
                    { new Guid("56961017-ea49-5af0-a1c2-6260eee19d0e"), "en", new Guid("bbd4f711-998d-29d0-6006-7b82407fbc0c"), "Status" },
                    { new Guid("56c052ca-bee3-7631-652a-52c9e77f9042"), "en", new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "The reason reaches who entered it, and the entry returns to them to fix and send again." },
                    { new Guid("56e1972c-425a-0b33-f0a7-6e289ce698dd"), "en", new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "Could not read the partners. Check the network and try again." },
                    { new Guid("574ed884-311c-943e-4016-03d72adff5db"), "en", new Guid("7b0cb500-d2d4-c60d-a601-83f7b6210711"), "Added the account {0}" },
                    { new Guid("58003d22-9775-6a8b-bbd1-480fdaacb2f6"), "en", new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "Nobody" },
                    { new Guid("580da880-4154-0edc-b4c0-2915a596e72b"), "en", new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "This project is finished, so nothing in it changes." },
                    { new Guid("58102dec-646d-a5fa-8874-5fa6a4f7f70c"), "en", new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "The project opens once the admin approves it. You will be its project manager." },
                    { new Guid("581f18a7-e7b3-01dd-6d56-85814037152d"), "ar", new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "رُفض قيدك" },
                    { new Guid("5834fc2a-cf04-d2c6-2af3-290eb5a4883a"), "en", new Guid("6cd6e6cb-0b49-b6d9-14a7-cba7d152ac37"), "In the books" },
                    { new Guid("587e484b-e94d-2e55-53f6-285520e88efe"), "ar", new Guid("ecb0ee7d-db94-4df3-afda-8ac1a7cef0ac"), "اختر المورد" },
                    { new Guid("588b37f2-3f61-b827-9c14-5c3273a9a0e3"), "ar", new Guid("f190c394-52dc-db41-6381-e5b58e874199"), "الحسابات" },
                    { new Guid("589761c4-52c2-a344-d24f-82285a13b785"), "en", new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "Previous report's expenses" },
                    { new Guid("58ce52c3-cee6-964d-4d1f-94c1f0f198ac"), "ar", new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "وصف المشروع" },
                    { new Guid("591e93bf-4d7d-4960-163d-c20d7eb72311"), "en", new Guid("658b933b-4e23-f89c-a3f2-f3854b064465"), "Approved: {0}" },
                    { new Guid("59304d44-5c0d-b30f-d917-51ae73d98459"), "en", new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "Permissions" },
                    { new Guid("59543b95-d796-b41a-fbe1-d2085e9d87af"), "ar", new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "الجوال" },
                    { new Guid("59c429f7-b163-b2bc-6eb1-7ae5ab5d9edd"), "en", new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "Attach an invoice" },
                    { new Guid("59d60906-33ff-eafd-e3b2-0a6c4e871b4f"), "ar", new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "حساب بنكي" },
                    { new Guid("5a0f7ab0-36c6-9c34-44b5-ec5cf2524ab6"), "en", new Guid("0e648c6e-93d6-6eef-aba3-a712af269713"), "Enter a name of 1 to 100 characters." },
                    { new Guid("5a746b04-23f3-04dd-fe7e-c8dc90f18f07"), "en", new Guid("a7e8287c-4b4b-99cd-f7d9-a654e461a49e"), "Side" },
                    { new Guid("5a8e491f-ac5f-d72d-7306-ebbc65ea4fb4"), "ar", new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "القيمة" },
                    { new Guid("5a97c78e-f491-6297-cb10-c5289e0bc149"), "ar", new Guid("9c8312c3-f598-8eb6-6361-5600dbbf8781"), "اللغة" },
                    { new Guid("5aad714e-d090-9cd2-4484-cd6faa8d9403"), "ar", new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "اختر أحد الأنواع المعروضة." },
                    { new Guid("5ab5e673-8ab1-52ac-c703-155216c8cc0d"), "en", new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "Payment method" },
                    { new Guid("5adf10df-28fe-3666-c786-7971235bfff9"), "en", new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "Write at most 2,000 characters." },
                    { new Guid("5ae095d9-2383-e5e9-6307-ddd0d545cba7"), "en", new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "Revenue" },
                    { new Guid("5b38f0e3-42cb-c02e-3dff-d310dd14ccf9"), "en", new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "Save draft" },
                    { new Guid("5b4be8c6-59f1-7da9-4a93-8eb7f224c147"), "ar", new Guid("db920181-5bba-2173-698f-310a22fe706d"), "تعذر تنفيذ ذلك." },
                    { new Guid("5ba06fc6-a02a-810b-bb75-852e3e2b6bb6"), "en", new Guid("a04a00e5-b6ca-496f-9dc4-cd4a480a07cb"), "Deleted the partner {0}" },
                    { new Guid("5bc47b9c-6b4e-b110-7c46-44c7b4491220"), "en", new Guid("ad754a8d-9ee5-26fa-dd40-b6caa50d56a3"), "Remove this approved entry?" },
                    { new Guid("5beb3b76-4b3e-0558-bc95-90d8a35362eb"), "en", new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "or" },
                    { new Guid("5ceaf5e5-6bd3-8f12-1ae3-553a30d1c1e2"), "ar", new Guid("3e235119-ab31-5245-6304-aefa87cb07f1"), "بعد الاعتماد" },
                    { new Guid("5cebf35d-a35b-4eca-e25e-da3715e5bf45"), "en", new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "Add payment method" },
                    { new Guid("5d383655-2a22-cacb-7882-c6a6d557a4d4"), "en", new Guid("118f0575-b4a4-103c-ec45-82886336345a"), "This change was refused." },
                    { new Guid("5d4419e7-9875-0537-dd7a-9010446a6834"), "en", new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "VAT refund" },
                    { new Guid("5d45640a-3d73-e747-8693-b34a9d3d76c6"), "ar", new Guid("5b673e1a-ed7e-4212-3b9d-772705106db7"), "مستخدم في قيود أو مطابقات أو تحويلات، فيمكن تعطيله فقط." },
                    { new Guid("5d4802f1-1f09-666c-fa63-0f7834e92d9e"), "en", new Guid("af3ab20a-00f8-d723-0dc2-152eabe91921"), "Bank account" },
                    { new Guid("5d4c3a67-6fe3-707c-c495-92181accdc47"), "en", new Guid("6a6e1c85-5b12-7a2b-4187-66ac5ca9048f"), "First half" },
                    { new Guid("5d5d2855-6b1e-47ed-76e3-2a7f0007d6e4"), "en", new Guid("7be7f869-b472-557b-1492-eccce29349c1"), "Enter a quantity in numbers, with at most two decimals." },
                    { new Guid("5d76814e-eef5-9a3b-5973-050a2ef7b76f"), "en", new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "Inside the contract" },
                    { new Guid("5d8b19f0-31ee-0468-e90c-0c17a6cae0f0"), "ar", new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "تُغيَّر عند أول دخول" },
                    { new Guid("5db81562-0e7e-9605-0453-6db25836ace8"), "ar", new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "في الفاتورة {0}، وفي القيد {1}." },
                    { new Guid("5dc3589c-a39c-5097-5ff2-a7991c592b6f"), "en", new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "Saved in the phone's Download folder." },
                    { new Guid("5de87bc7-f42c-c035-6afa-f960a91c28d8"), "en", new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "This change was refused." },
                    { new Guid("5e73fbae-4e0c-47d7-7488-38202dc6bf36"), "en", new Guid("eaefdf42-2db1-261d-f1a4-8cc650443fe3"), "Project manager" },
                    { new Guid("5e8b416f-733e-2c61-df31-17fe5ac5e604"), "ar", new Guid("d18a15d7-3399-d293-0533-5e2fadf35dba"), "مدفوع أكثر من العقد" },
                    { new Guid("5eaf9d86-54a9-f83a-77e8-069c3663096a"), "ar", new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "المبلغ" },
                    { new Guid("5eca2ccd-c0ce-08aa-2f05-ef3535940ec1"), "en", new Guid("7eaac843-f0f3-1f45-b9dc-5fc32b5934b0"), "{0} {1} into {2}" },
                    { new Guid("5ee3f7c1-1ef6-aac4-40ca-a6b730919937"), "ar", new Guid("0e766766-fe39-e5a9-283e-84266293cb52"), "معطّل" },
                    { new Guid("5f61238f-f858-0bbb-d858-f34a1ddd6f74"), "en", new Guid("4bbf48cb-3405-2e57-5302-e8b53a619dd3"), "Too many tries. Try again later." },
                    { new Guid("5fb7173c-f1be-c1e4-e8d2-df3e2ebf7a94"), "ar", new Guid("a2d5cbb9-4863-2ef9-3685-ccbcbd18f8c0"), "حفظ" },
                    { new Guid("601a51fe-2550-0959-c7b8-8fa8a54362d1"), "ar", new Guid("abc9c7bb-c4a3-da7c-aacc-36d7adc86285"), "تفعيل" },
                    { new Guid("601b7b56-a12b-d87b-23a5-f42b65290567"), "en", new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "{0} holds {1}; {2} waits on it, {3} short." },
                    { new Guid("603072cd-6548-61dc-14b3-cd1da13e7e1a"), "ar", new Guid("5b494c8c-bbc0-5021-25cb-ad60253b794c"), "طريقة الدفع" },
                    { new Guid("6083b683-5093-cb17-369d-1ff78c50d106"), "ar", new Guid("b77d7114-1f38-c54d-15d1-1765cd1c57de"), "الإيرادات" },
                    { new Guid("60a70f8c-d012-a361-030e-cbb86aaa43b1"), "en", new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "Item" },
                    { new Guid("60b7af07-64c6-8e07-8e6d-6a00230dd157"), "en", new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "{0} {1} out of {2}" },
                    { new Guid("60e46676-b9a3-ffb7-61e6-e8855338bea0"), "ar", new Guid("37bce61a-7169-feb4-7b00-807d656de57c"), "خارج العقد: تدخل إجمالي البند ولا تُخصم من المتبقي." },
                    { new Guid("60f953f1-891d-e1c6-c956-8d24450e1d04"), "ar", new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "مفتوح" },
                    { new Guid("60f9c445-c74f-c2dd-b1cc-164d6312b080"), "en", new Guid("f8cad813-0c34-ea12-02f5-6fc9a034c153"), "A picture or PDF" },
                    { new Guid("61069e80-7cef-b368-0966-1530951e816c"), "en", new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "Column name" },
                    { new Guid("6165aa82-b26e-9df9-c816-a3d605685987"), "en", new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "Paid" },
                    { new Guid("61aeab36-2741-caf4-712f-b9ee7ced5772"), "en", new Guid("13989443-8496-3325-85a0-79bccd168cfa"), "Password" },
                    { new Guid("61bf3f95-c591-a317-7f75-ba3ec8c90a02"), "ar", new Guid("ffef1403-38d0-d9c2-03fe-b57a9d5d3a9b"), "استرجاع ضريبة" },
                    { new Guid("61e0d155-53d8-c8bf-5289-99820f68b691"), "ar", new Guid("8c1e6e3e-e5cb-80d9-99a3-23e442c0db26"), "متجاوز" },
                    { new Guid("62004d03-a8ee-83c5-3037-6918571c762a"), "en", new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "The item's sheet prints its tables in this order. Drag a table to move it." },
                    { new Guid("62289094-f771-e3c7-d0b6-d12a800243df"), "ar", new Guid("6d4ee85a-889f-6d62-e29d-6b232481a7e1"), "إرفاق فاتورة" },
                    { new Guid("6251cae4-8958-0c60-a083-f58fd5709b15"), "en", new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "Enter a unit price in numbers, with at most two decimals." },
                    { new Guid("62a48d5e-9f21-2f32-bed4-30bbca2d8e16"), "en", new Guid("5e37fcb2-fe5d-a524-e249-ca1b8bd74518"), "Receipt voucher or transfer receipt" },
                    { new Guid("62b48a3f-caf9-2d3a-690b-df740cb0ca33"), "ar", new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "اكتب التصنيف في 100 حرف على الأكثر." },
                    { new Guid("62de20e4-bb93-0d2d-774d-1c6a445d63df"), "en", new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "Tick the project that opens when you sign in. Untick it to see the list instead." },
                    { new Guid("62f4cc21-86b1-0826-244e-85914e951e0a"), "en", new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "No connection to the server. Try again." },
                    { new Guid("6314437b-e392-fd3e-b551-ff02bfa40154"), "en", new Guid("c0967f38-4061-7441-a2f5-8319e20cf0c1"), "Delete the account?" },
                    { new Guid("6316b8e7-2f05-0e2b-341e-98ae3e9e2a5b"), "en", new Guid("54aa5f3c-f52e-21c8-3edd-77b7485d1b95"), "Enter a mobile in digits, up to 20, with a + at the start if it has one." },
                    { new Guid("6365be7d-cd57-7759-7547-5c4268748814"), "en", new Guid("96d1c05b-bef4-4ba4-c982-f13d5138e384"), "My custody" },
                    { new Guid("63767ad6-b82c-0813-b699-b3f70dedb365"), "ar", new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "أدخل غيرك هذا القيد." },
                    { new Guid("63cf4e6e-f5b2-ef02-c4a8-706e6c9d8e27"), "ar", new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "المسمى الوظيفي 50 حرفاً على الأكثر." },
                    { new Guid("63d12455-aeed-2107-bf58-856365e611fd"), "ar", new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "إعادة تعيين كلمة المرور" },
                    { new Guid("63dc14e6-9f5c-1095-d06f-6cfcb7ba755f"), "ar", new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "لا يوجد شركاء." },
                    { new Guid("63ed8453-6f35-eee2-3d9d-22ce3bdb9a02"), "ar", new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "اعتمد المالي هذا القيد قبل وصول تعديلك، فلم يُطبَّق تعديلك." },
                    { new Guid("63f8f177-9942-10b6-6829-d8051cabd3f6"), "en", new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "No connection to the server. Try again." },
                    { new Guid("644b6cb5-ef60-82a9-dce7-c84a3a6d72b1"), "ar", new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("6451fd66-1c98-369d-6587-36f245056e8f"), "ar", new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "تعطيل" },
                    { new Guid("647864bf-e1c4-0e4f-b8cd-9c257b7456f9"), "ar", new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "معطّل" },
                    { new Guid("6531a1ef-2aa5-7feb-68c6-10fe52e4fb7d"), "en", new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "Second half" },
                    { new Guid("6531f12b-15dc-fb39-d3a8-777e72fb20ef"), "en", new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "This expense is someone else's." },
                    { new Guid("653f1c12-8398-3dc7-d855-c2d16b3f9f45"), "ar", new Guid("5ccfb2fd-cf81-3f4a-5156-a391715acd5a"), "يُسجَّل صفاً مستقلاً بمبلغ سالب" },
                    { new Guid("65618f07-639c-16da-2294-965a110f51c8"), "en", new Guid("871c72e3-b132-a835-90d7-5b4dc360dbe3"), "Send again" },
                    { new Guid("65eeaa02-dda1-cfd6-7afe-cb61a7a16e76"), "ar", new Guid("d01e3388-93a7-f994-4751-dd16fdceef4f"), "إضافة طريقة دفع" },
                    { new Guid("6612c2a8-4db5-ff66-b871-060ac4ca758d"), "en", new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "History" },
                    { new Guid("66227c72-1351-8dfc-69e2-97d3f4fb7271"), "ar", new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "اسم المورد" },
                    { new Guid("663136a2-da1f-da9f-4642-d3c32e267abf"), "ar", new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "هذا البند ليس في هذا المشروع." },
                    { new Guid("66382a84-cc98-5860-4f0b-11163479a10f"), "en", new Guid("886b87e1-42e3-e4fa-09da-8de435b16b0b"), "Edit partner" },
                    { new Guid("666670a1-0c62-3c57-6caf-029c8395b848"), "ar", new Guid("b9b1e037-874f-9086-e211-5b66759e9303"), "المظهر" },
                    { new Guid("666a4015-5ba1-6afb-363b-3c9d6ad3648f"), "en", new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "This project is finished. Nothing in it can change." },
                    { new Guid("669900fc-707d-4455-33c3-9dce46e0c16b"), "ar", new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "استخدم 8 أحرف على الأقل فيها حرف ورقم، ليست اسم المستخدم، ومختلفة عن كلمة المرور الحالية." },
                    { new Guid("66abbd85-3ef4-f41c-b0b6-50d3118dff0c"), "en", new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "Fill the required field." },
                    { new Guid("6709191c-fcba-8e91-2a28-d2b4ac929653"), "ar", new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "استرداد ضريبة القيمة المضافة" },
                    { new Guid("6740acb9-caf0-ce05-919f-a656798e7dba"), "en", new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "Total capital" },
                    { new Guid("675e3729-3695-4e2f-cc2d-70263a08c6e6"), "en", new Guid("99815dd7-8976-d1e9-8ca1-f695088eeb34"), "No connection to the server. Try again." },
                    { new Guid("677a2272-33e1-01de-c567-fe1f1ca1eabf"), "en", new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "Photographs or chooses the file of an invoice, a receipt or a contract." },
                    { new Guid("67f430e2-0127-d902-8316-332c98f880bf"), "ar", new Guid("49b771c9-e288-eb67-996b-2d28a1d369ea"), "يرى رأس مال الشركاء المؤشَّرين تحته فقط." },
                    { new Guid("67feda3a-4efa-983d-c8d9-1c0eaf5a01b6"), "en", new Guid("9afa03fa-30e8-5df8-3ea5-4c4b41859e50"), "Notes" },
                    { new Guid("68047c66-4617-69f6-81e8-7edf53c7a81b"), "ar", new Guid("f8f711e3-1529-2a76-bef6-df429f27b36c"), "البند أو المورد أو من أدخله أو الرقم أو المبلغ" },
                    { new Guid("6813967c-e70e-9293-8c2e-4bcc0e109817"), "en", new Guid("0b1b9439-643f-fd46-ae16-7c6f329dc016"), "Waiting for approval" },
                    { new Guid("683dc7c7-c7a9-cc35-2bd5-bd74cb20762a"), "ar", new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "بند رأس المال في هذا المشروع معطّل أو غير موجود، فلا يُضاف شريك." },
                    { new Guid("684108bb-42ce-dcff-5934-43e67dfaa0c2"), "ar", new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "رفض الخادم هذا التغيير." },
                    { new Guid("6843f540-24f0-3f19-4b3c-73fca46ea856"), "ar", new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "تسجيل الخروج" },
                    { new Guid("68d3dfdf-8118-7a8c-49b9-fcb433f4533a"), "en", new Guid("a3b243e1-d152-3d66-0d16-029a53f33aae"), "Choose the item to see its expenses." },
                    { new Guid("68ea08a0-2f03-84b6-83dc-c12f10efe48f"), "en", new Guid("79fc88ca-db19-92d5-c185-41bb606735c5"), "Entry" },
                    { new Guid("693d629a-fe16-83d6-8e03-19cfbb917165"), "ar", new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "الكل" },
                    { new Guid("6960ad34-64e9-5004-bbb8-1c929d4d6fd8"), "en", new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "Paid" },
                    { new Guid("6a3837c1-fc33-e41f-c8ad-86cac17ed328"), "en", new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "Username" },
                    { new Guid("6a726cfd-e081-1803-61b4-784a9d2629a9"), "ar", new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "سبق البت في هذا المشروع." },
                    { new Guid("6b2b13eb-57e7-b453-f82c-6b4e1b7cc388"), "en", new Guid("2e97e755-063c-c0fd-64fb-bc562ca179b7"), "Capital" },
                    { new Guid("6b407479-99dd-22f6-527e-f9e4de15c2fb"), "ar", new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "السبب" },
                    { new Guid("6b42f598-8569-1fdf-eb47-7abda83e7ff6"), "en", new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "Send for approval" },
                    { new Guid("6b561eae-1f5b-b5d4-609c-5f2136247149"), "en", new Guid("39054aee-63ce-ce01-530d-aa84a950cf1d"), "Contract overrun" },
                    { new Guid("6bacabe1-5d51-8314-c738-7229c9956a02"), "en", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "Matching" },
                    { new Guid("6bb36050-f13f-084c-04de-c5e950eaf0f8"), "ar", new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "المبلغ" },
                    { new Guid("6bbb42c7-a370-3160-55f0-0f3f44885030"), "ar", new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "دفعات مقاولين" },
                    { new Guid("6bdf5f67-e9dc-22bc-1da6-8b7ae4f6ec0b"), "en", new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "Light" },
                    { new Guid("6c2ca628-066c-4303-9242-78e65f36e8e3"), "ar", new Guid("a4c5a6c5-2bf8-9771-ccad-f574e9524b1d"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("6c78f16a-120e-7f9c-9061-c96cb5af2029"), "en", new Guid("59a3a15a-b128-8e44-708b-809d6a215e8b"), "Account" },
                    { new Guid("6cc8e010-24d1-9c49-cd1a-0e8ef2f6ae2c"), "ar", new Guid("7a08dde9-457f-cbfc-46ad-5cafa6ca4e1f"), "فاتح" },
                    { new Guid("6d786445-66af-9d78-71f8-89ac8954bc53"), "ar", new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("6d79cb8d-b572-f652-f60d-9655ba9cc976"), "en", new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "Saved as a draft." },
                    { new Guid("6ddf9830-c72a-17ae-6149-02b6b5e4a47a"), "en", new Guid("6cf5e100-bffd-f471-d6c8-ad244a4b6944"), "A project needs at least one person who manages its users." },
                    { new Guid("6df2c889-7f00-be9c-4a2d-eb07d1e61e7d"), "en", new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "No connection to the server. Try again." },
                    { new Guid("6e1e3fca-cb3a-8803-c3f4-0ac65c4e9b5d"), "ar", new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "إضافة وحدة" },
                    { new Guid("6e5a5f70-08f1-5fa0-7090-1e66d1afdcf4"), "ar", new Guid("61cc9c1c-a89b-e4c7-e155-ba31c84eb42f"), "تعذّر على الخادم تغيير كلمة المرور. حاول مرة أخرى." },
                    { new Guid("6e82f6c8-d992-41a4-0633-34ad24225097"), "ar", new Guid("78c8f6e2-468d-81ad-0c0c-7c46de910feb"), "وتُحذف معه مسوداتك التي تذكره:" },
                    { new Guid("6e8565c5-7d06-7747-3f2f-f31b32de6009"), "ar", new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "خيارات جديدة، كل خيار في سطر. وتبقى الخيارات أعلاه لأن القيود تحملها." },
                    { new Guid("6eec3541-5454-0d53-fe0e-6948aba80b43"), "ar", new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("6f346cf2-9d16-9a0a-26a7-7ecc2eff58c8"), "ar", new Guid("9cc0e03d-6ab9-f6cb-77f3-01dd5db78588"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("6fda4d74-06d1-bc92-5d0f-b1630324fcfb"), "ar", new Guid("f6b2b75c-1d98-8d57-4464-a4438d2892c7"), "يُفتح المشروع بعد اعتماد مسؤول النظام له، وستكون مديره." },
                    { new Guid("6fdd527a-b27f-dd2c-6d2e-5881c25c0545"), "en", new Guid("75732723-aba7-1685-bd11-04205305acb8"), "Save" },
                    { new Guid("6fe229df-5870-3913-b2a4-0ad2dcb32b88"), "en", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "From" },
                    { new Guid("7089d880-d7c3-8133-bf4b-7c51ff67b491"), "en", new Guid("09fe5c23-d25c-7eec-931b-9828d1532aa7"), "Drag to reorder. Locked columns are what the calculations and the tax statement rely on: they can be renamed, not deleted." },
                    { new Guid("70d7bb76-225d-0a11-df22-f67cc426495b"), "ar", new Guid("4528a1fd-bbfc-a759-0960-9099879b7ead"), "تم الحفظ في مجلد التنزيلات في الهاتف." },
                    { new Guid("70e7a079-1e69-1216-7a77-5dda1d81908a"), "en", new Guid("3d2d51a2-54a6-d0d2-6dee-c1f414566543"), "That file is not in this project." },
                    { new Guid("711df39f-5dbc-5c25-7d1b-e77a43111bb5"), "en", new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "Search by item or contractor" },
                    { new Guid("71291288-a49c-e478-8098-df8320952222"), "en", new Guid("82816946-030f-2d29-a52b-2bb2c4b1020d"), "Save description" },
                    { new Guid("71329c37-e1c2-c006-923b-458fb2aa7639"), "en", new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "Reject" },
                    { new Guid("713aef6d-b027-7b8d-478a-4924cb333e42"), "ar", new Guid("592be19e-5277-8b9b-3aa1-dcc113fceba2"), "من" },
                    { new Guid("71627720-2929-5eda-a1b8-1bf9760a8303"), "ar", new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "تعذّر التحقق منك. حاول مرة أخرى، أو استخدم كلمة المرور." },
                    { new Guid("71632ad2-1d92-9230-0086-1a3ab8ae3500"), "ar", new Guid("0c1a7e24-0bfc-afc4-589a-a190830c78fa"), "إصدار التقرير رقم {0}؟" },
                    { new Guid("7183af42-9714-2db6-7275-93695c65dfaa"), "en", new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "This item no longer asks for that field. Open the expense again." },
                    { new Guid("719faf28-945b-6645-53bb-1160f38e092d"), "en", new Guid("ae277cab-a814-238a-ccce-45a1d5483326"), "A difference means an expense not recorded, or bank fees. Check the accounts' movements since the last reconciliation." },
                    { new Guid("71de789d-4059-45ea-db83-2c5cc9128034"), "ar", new Guid("c7bf799e-0114-c25a-46db-b0ff31655ea2"), "لا توجد عقود." },
                    { new Guid("71e4cc35-36cc-62ee-f963-39d00a94175d"), "ar", new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "لا يوجد مستخدم مطابق." },
                    { new Guid("71e5ae9f-3dfd-8342-d089-89a008936b52"), "en", new Guid("ab1f493e-24de-51d9-c2c0-6d65e8a8d07e"), "At least 8 characters, with a letter and a digit, not the user name." },
                    { new Guid("726046d8-680b-7fad-96fc-7266129b4766"), "en", new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "Print" },
                    { new Guid("7277f98f-f4ab-08c2-7cdd-ed34a4cc3b8c"), "en", new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "Export project settings" },
                    { new Guid("72a747e5-aa1e-46a0-df21-2ce3cb338e9b"), "en", new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "Contribution" },
                    { new Guid("72abe461-ae43-cd25-dad1-4b187f9a3c03"), "ar", new Guid("7efe0a23-fbf6-156e-7523-a100ecd6139f"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("72dd481f-5b91-301c-1d5a-63a693734e0b"), "ar", new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "استخدم كلمة المرور لتسجيل الدخول." },
                    { new Guid("730eac13-98bc-14e8-0019-c7e8c9f8d16c"), "en", new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "Shown in printed reports" },
                    { new Guid("730f2470-b846-db63-6151-f0ce073b316c"), "ar", new Guid("0f9f6d5c-956a-540c-cfdc-782f680e6b4e"), "الفائض (المبلغ المفروض بالحساب)" },
                    { new Guid("7312ee8f-623d-19de-7965-4304250d4330"), "ar", new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "تعذرت قراءة التقرير. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("73300cc3-2450-fd26-b078-e38691f5ca9d"), "ar", new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "مرفوض" },
                    { new Guid("734a8a46-3da8-7ad3-56bd-1f917da195a9"), "en", new Guid("00b1deda-67b4-bc25-b5f7-1c1532bcbccc"), "Categories, items, subgroups and columns." },
                    { new Guid("734da565-e045-324a-1b27-e6dded36c8ba"), "en", new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "Completion" },
                    { new Guid("73774f15-36cb-daba-df7e-e30ff27ce97c"), "ar", new Guid("e7265e1b-72cf-1669-57a7-ee469f63cc8a"), "مطابق" },
                    { new Guid("73a2d377-b9bd-d063-79a8-301f64586de4"), "en", new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "Disabled" },
                    { new Guid("73b9b7d9-252a-12b3-0f7a-a20582ea149e"), "ar", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "الإجمالي" },
                    { new Guid("73ca43f1-8e42-9306-ef47-be4891a43920"), "en", new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "No suppliers." },
                    { new Guid("73ff9d1f-755d-294f-e431-ba0032867da0"), "en", new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "Choose the document's file first." },
                    { new Guid("7403d59f-215f-18b4-1dba-71c8f1aa4c7b"), "en", new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "Account" },
                    { new Guid("7430aaa3-a28b-adbc-b8d4-80b5ed10dd27"), "ar", new Guid("cd2f79cb-88f8-1027-d65f-85e16b1d0818"), "مرفوض" },
                    { new Guid("7433a2e0-7a84-7689-9953-0d337a0118d2"), "en", new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "Approve selected" },
                    { new Guid("7438bcb4-a721-9b62-e298-638f67ed13a8"), "en", new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "Send for approval" },
                    { new Guid("749a7613-2e98-7878-e198-2e17e5f384f1"), "en", new Guid("2b5d0eb9-77c7-aba7-830b-68d1cd699637"), "Partner" },
                    { new Guid("74acd799-0bf7-d406-faf6-8c8cf270d806"), "ar", new Guid("762e094c-1186-fe92-0635-7c8febfdaa03"), "أدخل خياراً واحداً على الأقل، لا يزيد كل خيار على 100 حرف." },
                    { new Guid("75032098-1f4b-e633-77fa-5227d31d59e7"), "en", new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "Excel" },
                    { new Guid("751e7cc9-ecbb-9284-bce2-a3fa0af82a05"), "ar", new Guid("18ee31d1-4060-e5d1-a3cc-6f6d8fcd21ec"), "حذف" },
                    { new Guid("752af740-649a-e8af-ce73-3d0e9c1d0b16"), "ar", new Guid("5178081f-f07c-5e94-89b6-62af2e8decb9"), "تعذّرت قراءة الموردين. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("752f40d1-0f70-4b91-7dc3-3fdbf30b0821"), "ar", new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "إعادة تسمية" },
                    { new Guid("753fa9ff-b2c7-9e47-86e8-87c8c3e57748"), "en", new Guid("a90cb511-0d28-774c-503e-c6136e67fa4b"), "Edit" },
                    { new Guid("7554ce76-1b7b-2135-e342-bd788a577769"), "en", new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "Notifications" },
                    { new Guid("75610cda-762f-7651-81a7-aad2c40e9b7c"), "ar", new Guid("32604101-1bb2-8f2a-15b4-1a7df4de510b"), "تعذر تنفيذ ذلك." },
                    { new Guid("7578fbbd-7e55-11e4-5b50-403bed9e3cc2"), "en", new Guid("a44971ad-e0b8-c94d-db39-ec5f7ef189c7"), "Contract payments" },
                    { new Guid("759d5049-bb47-77d4-c446-9278ce4f8123"), "ar", new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "المقاول" },
                    { new Guid("75f18932-a0ef-fc8b-fb07-47939cf3f08c"), "en", new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "You hold no account to pay from. Ask Finance to hand you one." },
                    { new Guid("76258564-fab3-3334-1530-5f4e83393ad2"), "ar", new Guid("1e6d1ddc-7644-bd59-baaa-42aa4d9fa697"), "عهدة نقدية" },
                    { new Guid("763474b4-80bd-24cd-b736-1e61d551dd3c"), "ar", new Guid("f58e0ce6-f8d0-df50-fbd6-c38656f3d36a"), "الاسم" },
                    { new Guid("766077bb-e32a-60a9-c67e-776ab9f647f7"), "ar", new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "لا قيود بعد." },
                    { new Guid("7670ffa3-16f0-0c95-dfa1-1bae4b6c1213"), "ar", new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "حفظ" },
                    { new Guid("767f61e8-ecd7-f948-dd14-0a5aeaa9368b"), "ar", new Guid("d4c24f38-a771-b643-5e84-e06c66dc48bd"), "الصلاحيات" },
                    { new Guid("76acd042-4716-fdee-de80-d419292e9604"), "ar", new Guid("3480e79d-9b47-988e-38f4-103a9e372d40"), "تعطيل" },
                    { new Guid("76e19e53-274c-bdd5-57cf-73e421bcc386"), "ar", new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "تفعيل" },
                    { new Guid("7726ace2-b636-acb7-1c29-43fc1a702dec"), "en", new Guid("f1614e9a-a1f6-f4cd-579f-80327444df72"), "Counted the actual balances: {0}" },
                    { new Guid("7755f159-824e-6727-668c-be21e71aa518"), "en", new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "Supplier" },
                    { new Guid("77a8d9ee-a77a-84fb-74be-c5c1446b005b"), "en", new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "Keep the settings" },
                    { new Guid("77d396c5-4df5-b053-ca34-0cc68a360bc1"), "en", new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "Purchases" },
                    { new Guid("78216797-7f73-756c-b9d5-1d9957a172b0"), "en", new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "This project is finished. Nothing in it can change." },
                    { new Guid("782d21e6-d160-be0b-2191-7d9906073b55"), "en", new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "Could not read the categories. Check the network and try again." },
                    { new Guid("7898a82b-821a-556b-1853-a79e5c7944b2"), "ar", new Guid("47245a21-fa0d-0f99-b5fe-c01ba2f395f1"), "مستخدم جديد" },
                    { new Guid("78a0e7e2-d5a5-a1d1-44e7-baba704c660a"), "ar", new Guid("650c3f6d-1a58-ea52-771e-62a588c05d40"), "حُفظ كمسودة." },
                    { new Guid("78b2e4f0-8a07-7c6c-ee1c-b2ba51436fe4"), "en", new Guid("2dcce97a-2ada-8d02-afb4-dd8a9cef0a7a"), "PDF" },
                    { new Guid("793dd720-f479-ed71-932f-0c1c98df1087"), "en", new Guid("4c99caf5-3f60-f788-46d0-a6f2050c86a9"), "The amount does not match the invoice" },
                    { new Guid("7970121f-bb93-19c7-7b59-17644b42fb4d"), "en", new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "Default project" },
                    { new Guid("797a2634-a804-c5e8-d043-001d135ddebf"), "en", new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "A partner is deleted only at zero capital with nothing waiting." },
                    { new Guid("79f6892b-0096-8d06-5c60-82dbc99147a5"), "en", new Guid("7dcfe54e-0249-e447-0ac2-52fb545df407"), "No connection to the server. Try again." },
                    { new Guid("7a116032-67e6-9680-1cb9-7d52e88ab8c9"), "en", new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "Expenses" },
                    { new Guid("7a2f08fa-fe87-fe62-b1b1-c028ad316f93"), "en", new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "Only whoever entered it, or a manager, changes it." },
                    { new Guid("7a5676a9-acbc-ae3c-e4da-02d9382b7a9a"), "en", new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "Enters purchases and contractor payments." },
                    { new Guid("7a888b79-0b27-2870-0879-9a584a4670d8"), "ar", new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم." },
                    { new Guid("7b181123-b765-2047-838f-02a6733014de"), "ar", new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "تراجع" },
                    { new Guid("7c235ba1-1c26-19c7-2e4f-8810321b77b7"), "en", new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "Reconciliation difference" },
                    { new Guid("7c83c5b7-e684-a93a-c6f0-b8e0bb0f8e15"), "en", new Guid("6ba82d29-a6be-19e3-260b-3062002d29f3"), "Reason" },
                    { new Guid("7cc07d3c-fde6-76b8-3c7a-de0c36cbd338"), "en", new Guid("cce4eecf-9d81-da0a-b60a-e5c2c5854cae"), "Chosen partners' capital" },
                    { new Guid("7cdd9127-daab-476d-3ddd-b00205d8c298"), "ar", new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "لا توجد بنود بعد." },
                    { new Guid("7cec74d2-98ca-19ea-2405-4cd1058e2bd0"), "en", new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "Deletes a bill or a payment not yet approved: their own, or one someone else sent, with \"Handle others' entries\"." },
                    { new Guid("7cfff014-ce15-fd80-fa6a-1cc505275f3f"), "en", new Guid("23f2ed67-dfc9-a31d-6835-23026e65948d"), "No connection to the server. Try again." },
                    { new Guid("7d1b4f10-f58e-d845-d981-2a2a8ec9cfd0"), "en", new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "An approved entry does not change." },
                    { new Guid("7d30e2ff-930c-063b-52cf-8bef179bc274"), "ar", new Guid("c0673ea9-19aa-de67-77df-005a90751bb4"), "اسم المستخدم أو كلمة المرور غير صحيحة." },
                    { new Guid("7d7142f1-4e13-9a45-ec84-f4faeccd11bb"), "en", new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "Disable" },
                    { new Guid("7d891732-73d3-f482-711d-c81e23098136"), "en", new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "Deleted {0}" },
                    { new Guid("7daefe63-0ef4-9919-195b-061255c4093c"), "ar", new Guid("bd47da61-ded7-c7d8-447a-ea0c154a1bc7"), "الإجمالي" },
                    { new Guid("7dc0dba1-da42-fba6-46a2-adcf62ec1493"), "en", new Guid("a60f5052-361e-e04f-d287-c7a05df71feb"), "Mobile" },
                    { new Guid("7dd4e58a-92f6-31cb-2d37-122bff506f60"), "ar", new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "الشركاء" },
                    { new Guid("7dfc43b1-3ff5-6f98-03c9-ed46275b8798"), "en", new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "Approve" },
                    { new Guid("7e4e2ae5-06ef-6405-4006-e9372858893b"), "en", new Guid("3e1c052e-8583-4a49-c195-97db89e57466"), "To" },
                    { new Guid("7f0f7f9d-5122-a2a0-84ed-d9b1df17f168"), "ar", new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "إضافة قيد" },
                    { new Guid("7f1c7913-3212-6c3d-8bc6-938f059a033e"), "en", new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "Use at least 8 characters with a letter and a digit, not the user name." },
                    { new Guid("7f6b4c73-3edc-416f-1703-8c3ed226d550"), "ar", new Guid("7bb25355-b5d1-2942-c2f6-c5021ec53e8d"), "الثاني" },
                    { new Guid("7fa24744-2a43-b4f2-3e6a-76c638faa527"), "en", new Guid("dd8bc922-ee46-5b59-8f0d-a0194bf291fa"), "Revenue" },
                    { new Guid("7fefdbce-3238-63eb-de35-6573500c2c6b"), "ar", new Guid("75246717-bf39-3cfa-5de7-7f8a65488ec7"), "التاريخ" },
                    { new Guid("802fa457-18b6-ef8f-835a-4dc814b65498"), "en", new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "Withdrew {0} for {1} from {2}" },
                    { new Guid("804a5fd6-855f-8499-9de1-3c6790f551c8"), "ar", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "المصروف" },
                    { new Guid("80569a26-c2bb-f965-5922-9e8596342460"), "en", new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "Permissions: {0}" },
                    { new Guid("80635ccc-ce5f-79aa-2cf4-b3973026ea7d"), "en", new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "Required at entry" },
                    { new Guid("807b69e5-8dc5-d43c-9c86-20c3623e9c93"), "ar", new Guid("887ed9de-786e-0196-93ae-39846a2947f1"), "بدونها يصرف من الحسابات التي في عهدته فقط." },
                    { new Guid("80d2c9f2-d915-2c8f-8466-16d22da29919"), "ar", new Guid("fe97eb8a-3e48-8aa5-c0a2-e05c4a486950"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("80e4223f-d7d0-074d-d05c-6bab80ead9ba"), "en", new Guid("a12c2484-9af8-73db-511a-e98d0d2108ad"), "Period" },
                    { new Guid("8105e699-5616-ec64-5169-6206f7046852"), "ar", new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "اسم المشروع" },
                    { new Guid("8118e655-cf60-98c7-9308-bc6939298674"), "en", new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "Unit price (optional)" },
                    { new Guid("815055e7-277d-551f-4ac2-04c96bf001fd"), "en", new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "Expenses waiting for approval" },
                    { new Guid("8167b690-1b8a-2eba-bd1f-d6fe763feb0c"), "en", new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "Default project" },
                    { new Guid("8190fb7b-9169-86a9-a6c6-50131f977630"), "ar", new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "صورة أو PDF للإيصال" },
                    { new Guid("81c2003a-b0b1-a3cb-e346-4adb4f598f3f"), "ar", new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "تطبيق على كل البنود؟" },
                    { new Guid("81cb2052-68e8-057b-f249-12566f3c415c"), "ar", new Guid("1a488f05-814f-6d54-6801-ea79e6ab5c27"), "اسم الحساب" },
                    { new Guid("81d93df7-0d57-1298-acd9-2532ff271c6d"), "en", new Guid("03e55979-5b42-cb95-2cd4-fe90242f98e9"), "Add partner" },
                    { new Guid("8232e654-0733-83e4-4675-ee1fcbd48707"), "ar", new Guid("41b8bec4-2f08-0ebf-c459-f13df0e88abf"), "أدخل سعر الوحدة بالأرقام، بخانتين عشريتين على الأكثر." },
                    { new Guid("824ddda2-086c-0620-f7f9-1d01831345e8"), "en", new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "Users" },
                    { new Guid("826a7f52-5662-75b4-3b2d-7777fb431589"), "en", new Guid("fc677b8e-5cd9-210c-f9f2-c85630db6d17"), "Movements" },
                    { new Guid("82c826a3-aa0c-ee8f-fb01-6b4bd7e2b926"), "ar", new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "نصف سنة" },
                    { new Guid("82d0d5ad-1b42-5a41-6628-6d2ec02a262f"), "ar", new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "الملاحظات والتعديلات" },
                    { new Guid("82df2b30-b051-34b8-4054-c03c225c78b3"), "en", new Guid("1aed4663-400d-2540-4649-dc62be40737c"), "That document is not in this project." },
                    { new Guid("82e5e61e-97de-faaf-5e8a-a0639a099841"), "en", new Guid("1fca0161-6d26-0ed8-dc56-db06559748de"), "No connection to the server. Try again." },
                    { new Guid("83787c0e-8f10-309b-63e0-40b3cd31c972"), "en", new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "Unit name" },
                    { new Guid("839b885e-723e-6b9b-9ee1-40cac7accf67"), "ar", new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "مشتريات" },
                    { new Guid("83c0fdf7-9e65-1478-9d0c-f25c7db06694"), "ar", new Guid("2a797bf4-71b0-94fd-cc76-459353a103d9"), "مسح التصفية" },
                    { new Guid("83fc62c8-1559-2c4c-2e5c-7ea95ff738e1"), "ar", new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "تفعيل" },
                    { new Guid("844f5420-59c6-f292-73c8-444e7caf41dd"), "en", new Guid("9459e6ec-8ea8-9189-851a-1927ebfa815d"), "Search entries" },
                    { new Guid("847c3a84-117e-ad30-29c4-1258d291ce1d"), "ar", new Guid("12a28e1f-f7ff-815e-f6a1-23c0e1513b58"), "المورد الموجود:" },
                    { new Guid("84aa7fdb-4037-21e6-cd46-7de194092a45"), "en", new Guid("b88fb47f-187a-56bd-dd60-23f3358ec98a"), "Enable" },
                    { new Guid("84b082ac-44dd-dd3e-1bda-41c632d65cf4"), "ar", new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "الملف فارغ. اختر ملفاً آخر." },
                    { new Guid("84c15413-b773-41ee-90ec-58fe36b8ab4d"), "en", new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "Choose the file" },
                    { new Guid("84d6d2ba-ce62-325e-310c-430c9a86e30e"), "ar", new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("851d14ad-683a-b566-849f-1c0bb3495a6e"), "en", new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "Receipt attached" },
                    { new Guid("8551d4c0-bb19-f50a-7f57-b3c01804cdc3"), "ar", new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "في التقارير" },
                    { new Guid("855942fd-638f-0214-64e2-939ead8a7376"), "ar", new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "رفض وإرسال السبب" },
                    { new Guid("85783e6a-f3b4-0258-5d18-fa09d7058fa5"), "ar", new Guid("343d054d-7afb-5693-0444-a7954e933b1e"), "حذف {0}" },
                    { new Guid("85a40146-829e-b0af-aa57-c9a06b7e7506"), "en", new Guid("47bb9716-3f1b-be53-c0a9-d283b6aa66a8"), "This change was refused." },
                    { new Guid("85d83d9a-c79b-105e-575e-1c2bce433b68"), "ar", new Guid("3bd6e700-bb66-a2da-2be6-a49db7bf01be"), "ليس لديك مشروع تُدخل فيه مصروفات." },
                    { new Guid("85d8e46b-f92d-477e-f068-b357c4577eaa"), "ar", new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "كلمة المرور الحالية غير صحيحة." },
                    { new Guid("85fb396c-3eff-6813-7c0a-c5ea328b8978"), "ar", new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "معطّل" },
                    { new Guid("86257ea3-ea66-634f-49d3-70becf1f1bb3"), "en", new Guid("1ade3417-952a-8fa8-121a-71a15fd1046e"), "Type" },
                    { new Guid("862e9633-f73e-a600-dffb-d5c8ff123092"), "en", new Guid("59d9682c-dba3-c807-f61a-b91d6ffbceb8"), "Date" },
                    { new Guid("86440aa5-6daf-329e-6911-46d350bc2278"), "ar", new Guid("eff3f9a8-f4aa-bb09-f4a5-c7b1e423ee6a"), "السجل" },
                    { new Guid("864be42c-280f-d6f1-123c-a95722be794f"), "ar", new Guid("da28b3b3-d768-fb85-9015-fd87d2d9caed"), "سعر الوحدة" },
                    { new Guid("8672a050-5585-4022-cb0e-bd2bf3dc8182"), "en", new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "That person is not in this project." },
                    { new Guid("868ad98d-7996-0d9a-0dd6-55d62e1cc411"), "ar", new Guid("3e3ffa86-e670-d194-d664-808dc91ce2bc"), "Excel" },
                    { new Guid("86930c14-e8d1-8a00-7d35-1db59f2d309a"), "ar", new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "صورة الفاتورة غير واضحة" },
                    { new Guid("8694c1f7-f024-e5be-9eb4-18435cdbae24"), "en", new Guid("2cfd816e-0a15-bbdb-8db6-14e58a17a45d"), "A sub-table holds purchases, so only an item that takes purchases has one." },
                    { new Guid("8698db59-8216-c3b5-2004-ecdcf4c879af"), "en", new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "Enable" },
                    { new Guid("86a308aa-bd62-a572-52c0-513e73e9b87e"), "en", new Guid("b5b5508b-37fd-41f3-b79c-9d7d3a186860"), "The server refused this change." },
                    { new Guid("86d887f0-969a-12cf-fd9a-3e3e1807118f"), "ar", new Guid("a5fc4e4e-6b11-94e8-7857-1d4cccf3065c"), "يحذف فاتورة أو دفعة لم تُعتمد: قيده، أو قيداً أرسله غيره مع «إدارة قيود الآخرين»." },
                    { new Guid("8728059e-aa91-1f07-1cc4-550ffdcc7787"), "en", new Guid("4d59cc79-fba1-0d84-cbce-f1075f701ce9"), "Add user" },
                    { new Guid("873a71aa-5b82-2a81-2cad-9844aa232bfa"), "ar", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "اختر حسابا آخر للتحويل إليه." },
                    { new Guid("873d2519-e1c3-6937-8789-61be872b97ec"), "en", new Guid("86ce51ef-5c17-fb50-de5a-c03244dc1f33"), "To" },
                    { new Guid("8759e2d3-541a-56b4-0e4d-3818a7d4f732"), "ar", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "مرتجع (إشعار دائن)" },
                    { new Guid("8762d56e-cd0b-b2a5-5b61-402481900344"), "en", new Guid("aacd9d13-e9db-dd34-0f7b-2a366f311c99"), "No partners." },
                    { new Guid("87773873-a24c-66c4-8303-47c93e0d89ad"), "ar", new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "كلمة المرور" },
                    { new Guid("87880210-6ae9-b2c1-98d3-5e1f97635647"), "en", new Guid("ee0cfa52-4f14-c773-752e-f15f5d8e7386"), "First" },
                    { new Guid("87895462-7ad3-1510-af16-0ef1cb9dd232"), "ar", new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "مشتريات" },
                    { new Guid("878e14cc-7138-534e-699c-4edb6817f77c"), "ar", new Guid("3c4e52be-f5bd-5cad-2b66-9b14153822e1"), "بدون حساب دخول" },
                    { new Guid("879ba48c-03e4-2bb8-59a5-1c18d126ce1b"), "ar", new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "حذف" },
                    { new Guid("87e50819-ceb5-d8e5-fa5c-46048a550f84"), "en", new Guid("24cb39ad-03f5-00c5-f8b1-9dc2cfe8ee6f"), "Notes may have at most 1000 characters." },
                    { new Guid("87e5906d-abb8-d36f-3ed2-b81c5eea3202"), "ar", new Guid("a3f2bd02-c7c8-7e4e-79bb-1c5c9b06d494"), "لا تملك حساباً تدفع منه. اطلب من المالي تسليمك حساباً." },
                    { new Guid("8806813a-6bd7-0912-fc60-2a0fc87bffb6"), "en", new Guid("531379ae-8cde-2467-8484-33a498b15916"), "New password" },
                    { new Guid("881d39af-4af9-584b-f8fe-5bed589c9968"), "en", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "Enter a quantity of zero or more, with at most two decimals." },
                    { new Guid("88338f31-66dd-4b57-3553-2a3f42403585"), "ar", new Guid("09429ebe-a93e-aac0-7ce5-3a3a4ee70309"), "هذا المشروع منتهٍ، ولا يمكن تغيير شيء فيه." },
                    { new Guid("88453c8f-99fc-01bd-f0c5-2ac1e5d4f4dc"), "en", new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "the name was {0}" },
                    { new Guid("885236d2-1ea5-dd20-cb66-d1efc37f2970"), "ar", new Guid("bcbf7000-c209-ad82-7ae1-64f73a1a8699"), "إيرادات" },
                    { new Guid("88604375-6641-00e0-c3a3-001883a46a1f"), "en", new Guid("f850bed2-134d-1998-8332-e2fa4bfa7c0e"), "Save to Download" },
                    { new Guid("88a95bdc-3789-74b3-3d09-39aa55ad829b"), "ar", new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "تاريخ البدء" },
                    { new Guid("88b34e8c-459b-f0cc-58ea-0b17c7efe3d0"), "en", new Guid("349c6178-e979-bb67-feaf-ae7e84931477"), "Only who entered it, or Finance, can send this entry." },
                    { new Guid("88e57667-defe-76b6-5c1e-e2ab43ad1350"), "en", new Guid("5d539b4d-2590-21e4-8daa-7cb2498a77ed"), "Rejected" },
                    { new Guid("890aae98-e1fa-05b6-c938-b7f516ec44f8"), "en", new Guid("c36975f1-9c37-7149-dc27-cb52654d11ac"), "Capital {0} from {1} into {2}" },
                    { new Guid("8934e942-a3d8-1826-db65-6e31da247756"), "en", new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "File" },
                    { new Guid("895ee1c1-759c-d715-05f7-f271ab4bae15"), "en", new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "This VAT number already belongs to another supplier." },
                    { new Guid("8974ef2b-80b2-37ab-7465-c3a72c4d72ef"), "ar", new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "اختر صورة أو ملف PDF." },
                    { new Guid("89cf7cc3-dd8e-19b2-b9dd-eaccbf453565"), "ar", new Guid("40f283f1-6f76-952a-01a6-fce856633438"), "الرقم الضريبي (اختياري)" },
                    { new Guid("89ebe5ba-1a6e-f3df-6e4c-403d2fed261d"), "en", new Guid("89edd0b5-9abb-04dc-ac6f-37cc08ee3fb8"), "This entry could not be found: it may have been deleted, or it is in a project you are not in." },
                    { new Guid("89edeeb2-a4dc-d538-6e16-1f03541fac4f"), "ar", new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "تظهر المشاريع المصرّح لك بها فقط" },
                    { new Guid("8a0a9852-488d-9072-e30c-7c6f8b3dd59b"), "ar", new Guid("c93ba15e-0cbd-57ca-e19a-83b07442967c"), "المدفوع" },
                    { new Guid("8a24b383-6e39-03d6-94eb-d0c37f7f167d"), "ar", new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "تعذّرت قراءة رمز QR من هذه الفاتورة. عبّئ الحقول يدوياً." },
                    { new Guid("8a6d4c58-ac90-1435-4cb6-fa22329dfbe0"), "en", new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "The VAT number on the invoice is not this supplier's." },
                    { new Guid("8aa3d54a-3e63-2088-7d1a-36e5165f67b1"), "ar", new Guid("92bde5e4-5b89-c5a2-88fe-bc3ff7d4e5f3"), "اكتب رصيد كل حساب." },
                    { new Guid("8ac02635-5ac2-7569-39b0-3438445f8278"), "en", new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "Text" },
                    { new Guid("8ac7932b-a2dd-3832-c878-036a22095b91"), "en", new Guid("90dba418-4743-2a94-2b40-ff1f227a1ac5"), "Enter a name of 1 to 100 characters." },
                    { new Guid("8adb7508-af93-ac7b-ec3f-c2eba709908f"), "en", new Guid("297ec172-80d8-98a7-f35a-62fb4c7e30fb"), "Use password" },
                    { new Guid("8adcd323-1366-adc4-cd92-5bc04d3aa8f2"), "ar", new Guid("a4113921-35de-ca8a-a341-140a9fb13728"), "المستخدمون" },
                    { new Guid("8af62ca9-469a-42cc-dcf6-fb697f8d22eb"), "ar", new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "رُفض هذا التغيير." },
                    { new Guid("8baa24c5-a100-2237-c76f-1ad966f9f9b4"), "en", new Guid("0b1a31ce-eb65-a16c-5c69-50e215cbaa39"), "Edit" },
                    { new Guid("8bb75904-7efa-680a-8051-903dfbf18b1d"), "en", new Guid("7fa6fda1-bb3e-ab2f-b414-976a58fb7b94"), "Contractor" },
                    { new Guid("8bc23a05-b085-6ee1-1e87-6a3fa3ce1988"), "ar", new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "الدخول بالبصمة أو الوجه" },
                    { new Guid("8cc9437d-8965-496d-3359-273f355fc542"), "en", new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "The partner {0}'s account: {1}" },
                    { new Guid("8d166f86-6476-b963-2ada-3bb6c4bba42a"), "ar", new Guid("af34908b-a227-3b20-f64c-5789425cc04e"), "السجل" },
                    { new Guid("8d623765-3340-4981-e339-e7484d9f17bb"), "en", new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "No connection to the server. Try again." },
                    { new Guid("8dce01ff-e163-0770-3cef-03294fe56646"), "en", new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "Notes" },
                    { new Guid("8e4491d6-a31e-58a8-908c-f19b774e3c68"), "ar", new Guid("0e880823-7ee4-ffaa-6062-6185b524a293"), "هذا البند فقط" },
                    { new Guid("8e45b5bb-7f01-5d36-875a-1d3ab5deef47"), "en", new Guid("08357229-1958-81f5-3bac-8309b59996a4"), "Delete" },
                    { new Guid("8e817f1d-9085-f6aa-c189-cad8f32fe7b5"), "ar", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "المبلغ السالب خصم: اذكر سببه في الملاحظات." },
                    { new Guid("8e9bb63d-f963-eac9-97a7-eb08beb1c27c"), "ar", new Guid("da7a2b71-6655-c965-411b-18f830383110"), "حفظ" },
                    { new Guid("8ea23b2c-fee7-13c6-fe53-ded4229378bb"), "ar", new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "المشاريع المسجّلة" },
                    { new Guid("8eab892f-9a86-e0df-d4a0-95fdff3b909a"), "ar", new Guid("a8a282dd-76f5-b557-a0d1-b8916c10db8c"), "الشركاء ورأس المال" },
                    { new Guid("8ebf05b7-1607-da43-d933-797d6592d256"), "en", new Guid("f5d2b92f-25d4-26fb-aa40-308720f1635c"), "On the invoice {0}, on the entry {1}." },
                    { new Guid("8ec27bfb-8b44-3252-6a5b-fc8e2758df54"), "ar", new Guid("772e071b-75e1-28ee-8740-e783aaa76b57"), "اعتماد المحدد" },
                    { new Guid("8ef0379e-38ee-015d-7acb-862575173b37"), "ar", new Guid("e731e945-d131-6d29-8c16-9323b51e3c8f"), "حُذف." },
                    { new Guid("8f73d678-3680-27ec-09ad-9157ead98e74"), "en", new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "Handle others' entries" },
                    { new Guid("8f90319f-8ea2-22e1-c4cf-ebda60acc97c"), "ar", new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "لم يُعثر على الملف. شاركه مجدداً." },
                    { new Guid("8f9cb151-d853-c355-7287-045a2fce25b3"), "ar", new Guid("85937fc3-912f-a21d-5e8b-465780da5c7d"), "المصروفات" },
                    { new Guid("8fe7cd36-03ff-f7db-ac7a-b3c8fd0f3e03"), "en", new Guid("acae32c6-973e-6d26-4d54-3e792ce3c18c"), "What you may do in this project. Only the project manager changes it." },
                    { new Guid("9011182f-5ced-e079-387c-6366fcea47dd"), "ar", new Guid("4e776b54-347c-16c2-41ee-e94dc0277a18"), "أدخل عدداً صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("901d38cd-65bb-a8ed-3402-46399c5d98a5"), "en", new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "Remaining" },
                    { new Guid("9057b474-7571-8890-57a4-5013e0327e68"), "en", new Guid("864fa9d1-6598-ddae-41ff-c8c448ff16eb"), "Delete the partner?" },
                    { new Guid("905893a9-de3c-66e5-f0e7-d7e758856e4e"), "en", new Guid("6e50a558-a6d3-3256-093c-09fe2224bf91"), "The server refused this change." },
                    { new Guid("9094a335-5dba-db86-a44d-a3a9472516b9"), "en", new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "The list has changed. Try again." },
                    { new Guid("9096a3b0-ca65-08d0-32b3-e9e28031682c"), "en", new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "Pending approval" },
                    { new Guid("90a05b67-d0ec-636b-c805-679cbd892cab"), "ar", new Guid("118c1718-9436-8431-a45c-33cc878270cd"), "اختر مقاولاً من الموردين." },
                    { new Guid("90c5a3e6-4ad5-691e-d348-74da5e463ad6"), "ar", new Guid("0f496a72-6aa9-9845-a2bd-7b9df5c35b0d"), "يصل السبب لمن أدخله، ويعود القيد له للتعديل وإعادة الإرسال." },
                    { new Guid("90d5a9ae-4874-5667-a181-8b26575f1585"), "ar", new Guid("35c3f97b-cd53-5a4e-966e-4fee912427cc"), "لا يوجد موردون." },
                    { new Guid("90dbd797-b11c-75d6-96ed-00f46da23f80"), "ar", new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "عدد القيود في الرئيسية، وعدد ما يضيفه كل «عرض المزيد»." },
                    { new Guid("90edf415-ebb0-6eb2-34c0-da83a9a2427f"), "en", new Guid("4989a434-0337-c6d1-2409-7fdbbf9376ff"), "Partner deleted." },
                    { new Guid("914fe1a3-aa21-52c0-13fa-61a8254e8f71"), "ar", new Guid("d51695c1-a3c1-639c-878d-e5ecfa29ccda"), "إدارة الإعدادات" },
                    { new Guid("91752244-d7e2-8f15-3f95-790ba9c1143d"), "en", new Guid("f037bf7e-2dd1-0211-2d8b-1d6dcfd0b372"), "No items yet." },
                    { new Guid("91e412fa-0eab-013c-4d7b-b8a582d7ba0e"), "en", new Guid("442db054-1bbe-da4f-fcce-ea086b6a4eca"), "VAT refund" },
                    { new Guid("92175432-7671-3975-1c42-4aeb395b7604"), "ar", new Guid("514da573-6ea1-e3b6-02f5-aa5e98c20a19"), "التقرير الدوري" },
                    { new Guid("9265d48b-0aee-184a-2357-5c80fc637298"), "en", new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "Could not read the entries. Check the connection and try again." },
                    { new Guid("928c8257-9d8d-84f6-e981-a7b5142b80ce"), "en", new Guid("e01ff550-bb9d-0b5f-2781-0f1467855439"), "Why it is rejected" },
                    { new Guid("92db7af7-3317-c382-5f7d-45edc73e851f"), "ar", new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "اكتب كل رصيد رقماً بخانتين عشريتين كحد أقصى." },
                    { new Guid("9331a3a3-33e8-6f93-aabb-6e1f1ea8c92d"), "en", new Guid("0d438e76-7e09-e6c2-9d8c-23f2d91dee1c"), "This project already has its capital item." },
                    { new Guid("933d15b2-8b99-72b0-077b-358732d39b51"), "ar", new Guid("2b172b5d-394e-1ae8-51c2-c06c285eda7b"), "البند" },
                    { new Guid("935e5aea-3540-b23a-6eb8-338e5bb7b8a6"), "ar", new Guid("fde9f3e7-7e59-d850-cd11-3318164c64b2"), "م" },
                    { new Guid("93e20883-0a9f-f218-9a4c-4980016e9729"), "ar", new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "لا يوجد وصف بعد." },
                    { new Guid("93edac82-6c8c-8291-b19d-64da58f9be72"), "ar", new Guid("d61d2a95-2885-25fd-f791-9eb38c86025e"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("93f949e4-ae8a-dc67-08b1-ec6d7c87d1fa"), "ar", new Guid("79d57a45-ccf5-596d-1e55-f250deb73c52"), "مصروفات التقرير السابق" },
                    { new Guid("9420a811-b80d-abdb-aaef-f1016d3e4036"), "en", new Guid("a600acec-e37d-ba1c-1ec4-322b87ac9712"), "Remove" },
                    { new Guid("945e5f2d-6620-e680-e0ac-3471da6f7b4a"), "en", new Guid("bb728c26-370d-6f3e-7dbc-14e5e224578a"), "Amount" },
                    { new Guid("946b9583-a5c5-e30f-b864-6fc57763e4ce"), "ar", new Guid("b05829c4-d861-f7a2-defc-9f5e307dbe67"), "فتح REDIX" },
                    { new Guid("946bb534-8991-a648-dd54-288248c4eeff"), "ar", new Guid("350662dd-a114-636f-e070-06a358c487b9"), "جداول البند" },
                    { new Guid("94e1bf81-f4a3-2918-c488-0d4268a32f18"), "en", new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "This change was refused." },
                    { new Guid("94e33314-5b6a-29aa-4f31-c965847747e8"), "en", new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "Contractor payments" },
                    { new Guid("94ed2e62-29b3-a221-6dcd-b2f8b5afe7a4"), "ar", new Guid("16e0fe60-45c7-4fda-b4d8-14c611962895"), "عبّئ الحقل الإلزامي." },
                    { new Guid("94ed53b4-1874-de23-89c5-fd33eb1a3c38"), "en", new Guid("b025a21a-81a7-10b8-602d-817a66f2158d"), "Edits, sends and deletes an entry someone else sent, or one waiting, before approval." },
                    { new Guid("951a7c24-cdd1-025b-d06c-182c2035dd5a"), "ar", new Guid("758286d7-52bb-c6f4-6a80-0c1aadcdf2c4"), "تسجيل الخروج من هذا الجوال؟" },
                    { new Guid("9599a4cf-e2cc-f5ff-9adc-c329b1b4ee65"), "ar", new Guid("8a0d2cee-6618-8920-0e3b-b7756035d8a1"), "تعذر تنفيذ ذلك." },
                    { new Guid("95ccb243-4dfa-ffef-1f95-e0798ee51e92"), "en", new Guid("a1b9d41a-10d5-3f31-b67f-358cc0a46ddc"), "New supplier" },
                    { new Guid("95dbf3fb-a165-cec6-1a24-2c2fbd1d4232"), "ar", new Guid("fb02bd6e-5d70-1516-9b9e-cc0dc9de6e24"), "رُفض هذا التغيير." },
                    { new Guid("95e26c3a-3ccc-aec4-d36c-f245c0887c56"), "ar", new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "البند غير صحيح" },
                    { new Guid("95ea06a1-8525-89cd-c56d-6c69f02bc1b6"), "en", new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "Contract value" },
                    { new Guid("960a2bad-7be7-a210-a515-4267ad9cc0a6"), "ar", new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "لم يُعتمد مشروعك" },
                    { new Guid("9626b2aa-641e-c783-229a-02a934de527e"), "en", new Guid("b5834e1b-bf74-70a7-283f-a8491b5cc932"), "Value" },
                    { new Guid("9628e6ed-ded8-d1df-83b2-b3f252b6d9ea"), "en", new Guid("9e1d3b05-fe40-a90f-e8af-bb2db13ad54c"), "Most used" },
                    { new Guid("963654cb-6b5c-7c54-c49d-f5f9c10e4b81"), "en", new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "This could not be done." },
                    { new Guid("9653b522-3d12-8d64-9642-86c6e6e4cb2b"), "ar", new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "القالب فقط" },
                    { new Guid("9670223c-b75d-cd3d-733e-8f7ed2affae2"), "ar", new Guid("889422c2-d45e-5ab0-4486-d40f6d2d6342"), "اكتب 2000 حرف على الأكثر." },
                    { new Guid("96715ee9-f05a-0a87-509e-fae8ab7a766b"), "ar", new Guid("858460ee-2646-7a25-aafd-17e36724328b"), "اسحب لتغيير الترتيب، وهو ترتيب التقرير الدوري." },
                    { new Guid("96803f30-457b-d836-4a29-eebe739af674"), "ar", new Guid("f593f3b0-6790-8886-e822-f6b5a951f42e"), "إضافة الشريك {0}" },
                    { new Guid("96ec8e27-85a4-43c1-2ff0-cafb4a3cb2de"), "en", new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "Opens the project and reads everything in it." },
                    { new Guid("973d2b22-768d-85e8-4ff3-24e43cce082f"), "en", new Guid("cd486259-ff5a-d472-f61b-2169e0f04c02"), "Transfer between accounts" },
                    { new Guid("97523ac0-00fb-3869-9de8-2b4bb8ac0c55"), "ar", new Guid("7dcd7eb5-e6f8-611b-d54e-f013824c52ed"), "البند" },
                    { new Guid("9794af56-343f-ddd7-463d-8091f5855175"), "en", new Guid("fd4532ab-a578-4527-9d3f-f322bc18fbd5"), "Write why, in up to 500 characters." },
                    { new Guid("97abc77c-ac8c-2c9b-9ef6-a800eaa81918"), "ar", new Guid("0905885c-e1c1-32f3-d9d0-ae824677258d"), "إدارة قيود الآخرين" },
                    { new Guid("97d500dd-a11d-076d-f290-06b7dd34604b"), "ar", new Guid("52ce80ec-4e4f-307c-9bae-d5d35a50efaa"), "سجّل الدخول لمتابعة مصروفات مشاريعك" },
                    { new Guid("97d70963-922a-314a-df3f-4b06c18577a5"), "ar", new Guid("7af715d8-ec95-8907-97e9-8b70888b6057"), "تعديل العقد" },
                    { new Guid("97d9b24d-2d25-2676-84ab-6d91f4feabc8"), "en", new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "Waiting to sync" },
                    { new Guid("97e1b99f-765a-724b-d8cf-c9ace29c9513"), "en", new Guid("963ecbda-35ee-c555-fabb-5c377d3b50e0"), "Delete" },
                    { new Guid("97e8294e-1f74-1317-ecd2-67725ab8a2e1"), "ar", new Guid("3a2883ea-82c1-43d9-5f73-1478242e6dab"), "القالب الافتراضي يحدد ما تبدأ به البنود الجديدة من هذا النوع. ولا تتغير البنود الموجودة إلا إذا طبقت هذا عليها." },
                    { new Guid("97f4c6f5-779f-a628-7939-cbc37c60496d"), "ar", new Guid("4ec39f26-d680-d5cd-2a47-762ae686e253"), "التصنيف (اختياري)" },
                    { new Guid("9851423d-3fcb-3bbd-f6bc-567c5d7330b0"), "en", new Guid("7a8f8a7c-07f0-936b-7188-523ca3c5838e"), "The document is kept." },
                    { new Guid("9866dab7-5227-ade8-af4e-f2e51ebc52c6"), "ar", new Guid("6717342a-504e-3687-226a-0638210dd5f9"), "أدخله" },
                    { new Guid("9867ceec-41c9-cf30-3f34-6610ddf4add3"), "en", new Guid("026b4332-42b3-4456-7dba-9bcccc4b0c5d"), "Other revenue" },
                    { new Guid("98964e9f-9ceb-029d-034e-2c64d373e010"), "en", new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "You cannot copy from that project." },
                    { new Guid("98ad406e-6ed3-0b3e-08eb-9c54d2b906da"), "en", new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "Choose a supplier from the register." },
                    { new Guid("98ad487b-af55-b8eb-5d3e-e4724c6aaf3e"), "ar", new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "رفض الخادم هذا التغيير." },
                    { new Guid("98f62942-8ec3-1862-f134-6cf46723655b"), "ar", new Guid("2a2f83a3-3bbe-2f97-b9a2-75f818045624"), "الوصف" },
                    { new Guid("9920bf18-86ad-e7a5-dc7c-7a3dccf87bea"), "ar", new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "إضافة حساب" },
                    { new Guid("9924ff9a-241a-bef7-bdd6-052d9b7b5cb9"), "ar", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "المبلغ" },
                    { new Guid("992bba4c-016d-41f8-567d-0fb671857be4"), "en", new Guid("d27f69c2-2341-a5fd-169a-0fe4fcb28231"), "No connection to the server. Try again." },
                    { new Guid("99a0b490-267d-c832-264e-98e50f051126"), "ar", new Guid("d2f1dab2-bee4-08f9-8ce4-075fdc665f0d"), "إجمالي رأس المال" },
                    { new Guid("99d1d398-6591-bfc7-d441-4a9accd86709"), "ar", new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "التاريخ" },
                    { new Guid("9a0c3da3-69d6-dfeb-d337-0933ec362005"), "ar", new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "إضافة هذا المورد" },
                    { new Guid("9a112cb9-5f3e-498f-bf95-e13593dc6336"), "en", new Guid("adef17b4-d853-0bd6-6df4-733bd6dc75a8"), "Whose account this partner is in the app. What anyone sees of the capital is ticked in Permissions." },
                    { new Guid("9a13e3bc-22f3-786b-a963-311d54740fd4"), "ar", new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "بدون بند افتراضي" },
                    { new Guid("9a336727-b6fe-d5b0-991a-145fadb70768"), "en", new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "Could not read the capital. Check the connection and try again." },
                    { new Guid("9a3c784d-2c8b-7ba4-ce2e-f1628ae9d6e6"), "en", new Guid("350662dd-a114-636f-e070-06a358c487b9"), "Tables" },
                    { new Guid("9a577061-dc73-d48f-f237-47fa260c1483"), "ar", new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "فاتورة مكررة" },
                    { new Guid("9a7c5537-28f1-40ef-99dc-364374c6ad50"), "en", new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "Read-only" },
                    { new Guid("9af83acd-dd3b-1a26-b4a3-c342a99bd1da"), "en", new Guid("e2a52a90-73ad-0e59-a413-69652745704f"), "A role name is at most 50 characters." },
                    { new Guid("9b49bb69-d420-4b2c-2bc1-faf08cb86074"), "en", new Guid("d36a6909-2fd9-433d-9065-f68a372b6c9e"), "Disabled" },
                    { new Guid("9baea722-2831-1387-5e7d-fdf507b22847"), "en", new Guid("a26bc0f3-c210-7458-5766-d6352b305d8e"), "Use at least 8 characters with a letter and a digit, not the user name." },
                    { new Guid("9bc1cbe4-3f74-a139-cbde-1ea35a8b1bbb"), "en", new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "Capital movements, revenue, reconciling and the periodic report." },
                    { new Guid("9bdd52bf-2f1a-7874-5aef-342c747881b6"), "en", new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "Subgroup name" },
                    { new Guid("9c0826bb-a284-807a-ab76-29915cc13f90"), "ar", new Guid("cb1a06dc-b074-b6b1-821e-2eaebcb11f5d"), "معلومات المشروع" },
                    { new Guid("9c3a7252-4527-c446-ab4f-ef96b47b9bc6"), "en", new Guid("18868db1-2bd7-13d2-3ec2-89c757f56775"), "They are signed out, and must choose a new password at their next sign-in." },
                    { new Guid("9c62c0bb-c98d-6df4-8cb1-35d85bec3e34"), "en", new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "Nothing yet." },
                    { new Guid("9c745c7a-7cc4-98c8-10d9-bf768249bbbe"), "ar", new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "تعطيل {0}" },
                    { new Guid("9c7e6d82-bae9-ec48-9970-49b1322fdf63"), "ar", new Guid("99bb5246-dc7c-0c02-3e28-8264e981b90f"), "اختر مورداً من القائمة." },
                    { new Guid("9d3a6b16-ec79-bde0-dedf-f0f8e1eccc28"), "ar", new Guid("ab14abf3-e4d5-eb48-b92f-5224e3a439bc"), "تعذّر تحميل المشاريع. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("9d53dc65-7117-df0f-ba60-09d4c1f66aba"), "ar", new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "بانتظار الاعتماد" },
                    { new Guid("9d7feb18-5b2e-8d12-1792-05188d3f865d"), "ar", new Guid("a303f630-0e5c-ceb7-348c-4724d9f37752"), "رفض: {0}، والسبب: {1}" },
                    { new Guid("9d85559e-6be3-186a-1200-acaa8a1d6b94"), "en", new Guid("28dbe9c8-0563-d9c3-859b-0a54e80ba746"), "Approve" },
                    { new Guid("9d99d97b-ed59-b769-49e7-841d8a3ba7f4"), "ar", new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "تعذّر حفظ المصروف." },
                    { new Guid("9dc2e435-783d-e7c0-b7e2-4495e0daf5f7"), "ar", new Guid("ef633ad4-8106-d17f-4929-6a2ab9430967"), "لا يوجد اتصال. حاول مرة أخرى عند الاتصال." },
                    { new Guid("9e2d6a22-9713-50b7-a89b-4fa23c3cf61c"), "en", new Guid("80b6142e-d1ce-c722-a458-21b86c12b803"), "Notes and amendments" },
                    { new Guid("9e51c001-0df9-7e10-a3e3-e301d893295a"), "ar", new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "الثالث" },
                    { new Guid("9e981665-3ac8-1a4e-7838-c3566161ffd4"), "ar", new Guid("bfbe6b50-e331-e9d6-398a-e19e73524e7c"), "النسبة %" },
                    { new Guid("9ea691bc-4176-625f-6d08-f3a9ad22a4f5"), "ar", new Guid("f0e2c210-2c77-ee5d-54d1-611ab13f2097"), "استيراد إعدادات المشروع" },
                    { new Guid("9ec7c2f3-b222-77e1-c31f-3974db486e7d"), "ar", new Guid("d34475ac-83df-c60e-5175-4d0fc61569f5"), "يطلبها التطبيق عند فتحه، وتبقى مسجّلاً حتى تخرج." },
                    { new Guid("9f49461f-69c8-9e89-bf30-f308d8b56552"), "ar", new Guid("f2b8ab51-9615-7946-1f12-5565bc6e282c"), "لا توجد حسابات." },
                    { new Guid("9f49ce8e-c825-8608-5c4f-812f4c92fe07"), "ar", new Guid("e93a435d-cfeb-0c29-a2fb-3ccab5b307ba"), "ما قمت به" },
                    { new Guid("9f56048b-7076-42e9-b45e-773e748ed574"), "en", new Guid("e30fe533-e540-06cc-f84f-ba661fe21db9"), "Choose another account to transfer to." },
                    { new Guid("9f5f8bc4-7278-37af-9089-0e7355c3a0d9"), "en", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "Quantity" },
                    { new Guid("9f6c3b52-21fc-6462-aaff-8753a461ac25"), "ar", new Guid("0f5601eb-70a7-0f06-2124-b42f05805d3e"), "حفظ مسودة" },
                    { new Guid("9f77f9c2-a131-c951-3994-74d0d68009e6"), "ar", new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "مسودة" },
                    { new Guid("9f846c51-e181-2eeb-aaea-fbdd2d6fb7ae"), "ar", new Guid("368581a4-041a-47f0-93d2-7c7f22ba8054"), "هذا المصروف لغيرك." },
                    { new Guid("9f8f0047-8a7c-db10-e0e5-ebf700fa47e5"), "ar", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "العدد" },
                    { new Guid("9fb4f1ac-eba2-e933-f119-714caaf01e96"), "en", new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "Enabled {0}" },
                    { new Guid("9feb97fb-ca3c-c4ef-a9f1-c8e152bff433"), "en", new Guid("16b7ad86-a2eb-26d5-44a6-4689b0ae9b4c"), "This entry was already sent." },
                    { new Guid("9ff9754f-f4e1-b3fe-f887-e2ae30fa7693"), "ar", new Guid("931cdb8d-0d2f-5f7b-3bce-87dafd6fdb4b"), "تعذرت قراءة رأس المال. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("a024db6b-8b4b-a0bc-5103-ce2e0fd895ef"), "ar", new Guid("9f0299ce-f903-f128-6e8b-d427b45762aa"), "معتمد" },
                    { new Guid("a058e5f4-fd5f-ed10-d0b3-7853475b6212"), "en", new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "Reject the entry" },
                    { new Guid("a0cd70d9-d4a3-c98a-eb0e-1ea41f8086ca"), "en", new Guid("c7dce5a3-7e8d-86fa-8fd9-7ad75dc54007"), "History" },
                    { new Guid("a0d95ad8-fbd9-e3c8-f8d9-a7ffe20e8b65"), "ar", new Guid("0da1a854-f40d-8414-1a65-283ef6a8978c"), "طباعة" },
                    { new Guid("a10cf312-27dc-b82d-7a2b-83c8bb01496d"), "ar", new Guid("21aa3960-77ad-2f82-26e2-4eb72a5fb9fb"), "لا تُمنح هذه الصلاحية داخل مشروع." },
                    { new Guid("a149ee7e-8928-d208-5427-9bb3c7ac87fa"), "en", new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "This project is finished, so nothing in it changes." },
                    { new Guid("a1b38c34-d9f0-8715-1447-e4a4af33a402"), "en", new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "Could not read the revenue. Check the connection and try again." },
                    { new Guid("a1e04957-1404-e807-dd72-5678e5bfdc5b"), "en", new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "Take again" },
                    { new Guid("a234616e-c390-b179-ca76-e267ec03f326"), "ar", new Guid("523c58cf-8a55-99a1-d45a-b56f130afad6"), "معطّلة" },
                    { new Guid("a24348de-03a7-5ff6-b5b8-94f8a049aa1c"), "en", new Guid("4b62fddb-55f7-7d0e-fad6-616eb0ef753b"), "Units" },
                    { new Guid("a2720be4-dcfc-f46f-58bc-b9567fed8010"), "ar", new Guid("f99cc9c5-5c4f-c58e-36c4-749ea41980a1"), "التصنيفات والبنود" },
                    { new Guid("a2a581fb-9169-4524-ef5f-375da2594e38"), "en", new Guid("edb8d6b3-2216-2603-4f82-fb0ebbab701e"), "Choose one of the types offered." },
                    { new Guid("a2dffc10-7bb3-8b28-3ca8-fd28c16f4796"), "en", new Guid("1f1eaf69-1625-34b5-d702-d6b656564302"), "Enter a username of 1 to 50 characters." },
                    { new Guid("a2ffb9aa-64df-334d-989d-5dbb53b48a4d"), "en", new Guid("75c17f5e-cb87-87cc-3022-bcf9fa626525"), "Some entries still wait for approval. Approve or reject them first." },
                    { new Guid("a313c2bf-1e85-873a-4b19-41aad6bbb75f"), "en", new Guid("e058cc8c-8a0f-8dd3-9667-cd4d56902b4a"), "No expense without an invoice under this item." },
                    { new Guid("a32a43bd-b85b-c6e3-4c54-f75b314bfe3b"), "ar", new Guid("5d71b29b-07f6-26ba-a47d-d6a100c2d513"), "المورد" },
                    { new Guid("a39f0718-fbab-9144-900c-5272495dc673"), "en", new Guid("c33d1bdd-122b-d5b7-5927-b9b1d6e949f0"), "Entries in this project are waiting for approval. Approve or reject them first." },
                    { new Guid("a40204fc-d2db-3ae7-d9d8-908a4ab4765b"), "en", new Guid("554cb4c3-3ff5-2643-ec96-eba12e29997f"), "That file could not be read. Pick another." },
                    { new Guid("a43294f3-1f35-071b-20cd-887c0d87ef3e"), "en", new Guid("5a3a8175-fa63-86fe-2e32-5fee67641042"), "This change was refused." },
                    { new Guid("a449c304-b61a-bae9-2eab-5af525df991b"), "ar", new Guid("af432105-c4bf-b4e6-ff54-3f378b79eb51"), "ملف" },
                    { new Guid("a4fbb7ad-15f5-6f16-b848-c5986c260232"), "en", new Guid("9e1b31b8-41db-595f-6003-71a6d6da807d"), "Switching the language needs a network." },
                    { new Guid("a5381c88-a2ca-e791-31dd-92b6a440e978"), "ar", new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "تظهر مستندات التصنيف الواحد معاً تحت اسمه." },
                    { new Guid("a55a378b-9ecb-380f-5c8f-9188ea84affe"), "en", new Guid("5e7d1bc6-ced1-d4b3-ab2b-8e082f3acd7d"), "Add an existing user" },
                    { new Guid("a576bf4f-43cb-3ea8-fffb-e0e9613e5817"), "en", new Guid("38f1cad3-3916-86fa-2ff4-76e2ff662848"), "Waiting for approval" },
                    { new Guid("a66ae456-9a16-df2c-3895-350064537fa8"), "en", new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "Quantity (optional)" },
                    { new Guid("a6796ce7-0546-4c15-1f2e-c0053daa2fa0"), "ar", new Guid("f7037acc-0425-5b6e-b289-86cade476b41"), "مشتريات" },
                    { new Guid("a68616c7-7ae7-0795-859d-532446851903"), "en", new Guid("abc0d3cf-b75e-4ef5-0c08-a56480100b42"), "Entries shown" },
                    { new Guid("a68e5578-e7f1-59ca-104b-a58c554d208c"), "en", new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "Disable this user?" },
                    { new Guid("a6902aee-08d2-3320-b4e2-03e40172436b"), "en", new Guid("db434b0e-fca1-d5bd-e5e6-b7e11f512e63"), "Purchases" },
                    { new Guid("a69839b2-8b2d-b69c-9d4e-6f96224a1dcc"), "ar", new Guid("0a3800a9-da57-40e3-ac6a-6606971c85d2"), "هذا المشروع مغلق." },
                    { new Guid("a72b8281-60b9-f850-ca0a-8aecd004292a"), "ar", new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "دُفع بهذه الطريقة مصروف، فلا تُحذف. عطّلها بدلاً من ذلك." },
                    { new Guid("a7369150-03f1-bc5e-2543-eb77771c1621"), "en", new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "This file is already kept as a document." },
                    { new Guid("a772dd27-8908-b6ee-b335-5eb9f457a10c"), "en", new Guid("76d4cd24-6534-d7be-07ab-9ee6d13f873a"), "The partner takes this amount out of their capital" },
                    { new Guid("a77e7b43-a910-603b-c0b9-fcf5735c0f84"), "en", new Guid("e1c72b1e-831a-6c6e-aab3-b646bf173c80"), "Undo" },
                    { new Guid("a79038d5-81be-3a89-e011-75b4fa2e6e9e"), "ar", new Guid("3ef9fec2-4eac-7979-e614-e6fd0ed5ff44"), "اعتمده {0}" },
                    { new Guid("a791b7b6-01c0-611a-fa72-142e915ae419"), "ar", new Guid("b00962cf-aa2b-7a27-ff57-2a5eebbcf125"), "حاول مرة أخرى" },
                    { new Guid("a7d434ab-ab55-f24f-9f24-295044b0b2f3"), "ar", new Guid("d8112671-3edd-9801-007c-1bee4c7d98d1"), "أو" },
                    { new Guid("a7fb47fb-313d-542c-4658-5ad4dd23dbfd"), "ar", new Guid("b6da7377-1356-0a6c-9720-3af2ce4723fc"), "يُدخل المشتريات ودفعات المقاولين." },
                    { new Guid("a7fdd2d9-fd87-5970-f912-314438cd8ae9"), "ar", new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "قيمة العقد" },
                    { new Guid("a831aa9c-0c16-8050-a4cd-ffcf2f830f0b"), "en", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "Item" },
                    { new Guid("a85c3058-afec-c7fc-4c92-4db6589fd659"), "ar", new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "حفظ" },
                    { new Guid("a88558ba-bb33-e988-ed57-87f27785bff9"), "ar", new Guid("efe2f215-2679-6afa-f013-5f2831a3ad0d"), "دفعات مقاولين" },
                    { new Guid("a899736c-4db2-7a95-9ee7-48468b544825"), "en", new Guid("55f2a6f4-17e3-49ac-d2a4-e12bbcf1b93b"), "Nothing is waiting." },
                    { new Guid("a8a9e2fa-1432-ae6c-4a58-d26c39d08af4"), "en", new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "Edit supplier" },
                    { new Guid("a8c8a8ac-b5c5-7612-edec-21fd5c8477b6"), "en", new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "Contractor" },
                    { new Guid("a8e5cd8b-4ec2-0c35-933c-4dcfe0aa0dfb"), "ar", new Guid("be9ac772-02a4-1393-7189-495e3103dfad"), "عقد يذكر هذا البند، فلا يتغيّر نوعه." },
                    { new Guid("a8fd20c5-eedd-da6a-7a8a-dd5e35e7c419"), "ar", new Guid("1a09023f-e56b-1467-f632-099aef723ce3"), "استخدم 8 أحرف على الأقل فيها حرف ورقم، وليست اسم المستخدم." },
                    { new Guid("a904eb69-401c-27a9-c536-528d86269314"), "ar", new Guid("84edc898-3c27-78b5-2adf-ff16a9c9537c"), "العربية" },
                    { new Guid("a947585d-dd93-22bb-e2d2-137655018a8e"), "en", new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "Sees every partner's capital, and takes the report files." },
                    { new Guid("a956fb7c-c040-bacf-adf3-4973c480a159"), "en", new Guid("be76222d-384e-52df-0251-68ae7904f8d9"), "Issue number {0}" },
                    { new Guid("a960ef51-2a68-48c4-95b6-13f35684e777"), "ar", new Guid("c98fbaa2-010d-7a4f-9d25-8930b53b7205"), "الكمية (اختياري)" },
                    { new Guid("a99bb7fd-bafd-8d55-18a8-f71b540e2c23"), "ar", new Guid("eb815a2c-5253-c40e-bce3-3ad40ae4384a"), "العهدة النقدية تحتاج إلى صاحب عهدة." },
                    { new Guid("a9cf36d2-cbee-1a9b-8907-2edab4f83431"), "en", new Guid("10db82b0-b441-b891-b34f-2c0877e937a5"), "The server refused this change." },
                    { new Guid("a9f655de-d43a-88d3-af1d-57abc0a81622"), "ar", new Guid("b4f08861-5c94-9864-f5ae-b014536a1942"), "النصف الثاني" },
                    { new Guid("aa2989ed-a339-0d4b-e9bd-5f39bfd1307d"), "en", new Guid("ff4f7497-9898-b6a7-20ee-02b823af236e"), "No movements yet." },
                    { new Guid("aa67a7e4-b40e-9580-2506-09f7c3284e90"), "en", new Guid("c9968ba7-3308-7944-1a89-f4abb7abecc5"), "No units yet." },
                    { new Guid("aa6d1b53-6702-9735-845b-c7279b35a913"), "en", new Guid("f5cf763d-3c5a-ecec-7da8-900d83593bc0"), "Remove approved entries" },
                    { new Guid("aa9bbd9b-89ab-252f-cfb9-203794c8f144"), "ar", new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "تعذّر حفظ الملف." },
                    { new Guid("aaaef9fb-7bc9-1a27-fe73-cffa18868e1d"), "en", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "Balance" },
                    { new Guid("ab1b4a87-d462-984b-3577-8dc13f929aba"), "ar", new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "الفائض" },
                    { new Guid("ab1c5d98-e73b-e783-0037-c747996e34a0"), "en", new Guid("700e94b7-2fe5-d557-54da-b692a2f39e3d"), "Template only" },
                    { new Guid("ab39459a-caa2-fbf3-5060-86259feda8a0"), "en", new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "Your drafts name them, and deleting those needs Delete entries." },
                    { new Guid("ab67a1c3-cee3-5c60-5257-9ec07bab9e7c"), "ar", new Guid("133520ba-c89a-4c91-3190-0369147b3f45"), "بحث بالبند أو المقاول" },
                    { new Guid("aba727bb-c6fd-dedd-f8b8-0356a52a044c"), "en", new Guid("6c698dc5-6913-7700-ae5d-3936e93fc45f"), "Finance approved this before your change arrived, so your change was not applied." },
                    { new Guid("abb1ad24-6a3f-8d72-1c07-ad9ce7148799"), "ar", new Guid("d5743a65-2a5b-54e3-51ff-e497503ca7c2"), "ملاحظات" },
                    { new Guid("abb8b122-d8ac-fb98-a28a-69b5e98870a3"), "en", new Guid("912681b5-ddca-815f-37dc-ec1db8fe715b"), "This username is taken. If it is yours, sign in and register from your projects." },
                    { new Guid("abc1ee24-d071-836d-7f75-8ecdf9e633de"), "ar", new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "اختر بنداً يقبل دفعات المقاولين." },
                    { new Guid("abcb61ac-ca6e-fd7d-c4a0-e2faacf69ca7"), "en", new Guid("8ad9f47a-26e3-5ae0-8798-f029dd50344b"), "Contractor payments" },
                    { new Guid("abd7b6ae-81cc-5edb-6b57-a33d2ab65144"), "ar", new Guid("7ce49ef4-21c4-c799-066f-a0e2b9432a7e"), "لا شيء بعد." },
                    { new Guid("ac0aa3f0-4aae-d942-c831-1bf8a847ed88"), "en", new Guid("db920181-5bba-2173-698f-310a22fe706d"), "This could not be done." },
                    { new Guid("ac10a866-e15d-3df6-71fd-267c394ad399"), "ar", new Guid("ec6a123c-4253-8938-4dbe-8ec5a3f73729"), "قيودي" },
                    { new Guid("ac308006-c3ec-d1ee-34a7-ee41e5367cb0"), "en", new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "Last updated" },
                    { new Guid("ac662de6-7f16-d09a-4d2a-6527c2c0a9b2"), "ar", new Guid("30a04719-0aab-9967-2f78-c57274e87419"), "هذا الشخص ليس في هذا المشروع." },
                    { new Guid("ac802dc0-5481-f4c4-a770-1a12f7da8cb4"), "en", new Guid("2a624064-b0f2-ff4e-bbd6-fd1a2a44d647"), "A negative amount is a deduction: say why in the notes." },
                    { new Guid("acb692b0-9dbe-3ff4-501e-370319a4f298"), "en", new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "Revenue and capital waiting for approval" },
                    { new Guid("ad298718-9489-9b96-325a-194b05d40b86"), "ar", new Guid("5d7d57cb-582d-ae44-3442-32006955b507"), "اختر جدولاً متاحاً لك." },
                    { new Guid("ad3e5092-c017-23ce-0de8-58beca6d2ac0"), "ar", new Guid("3eb99653-ceb0-f8bf-c7e0-20eb0a3d5b57"), "تعذرت قراءة الإيرادات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("adb5f6b8-03d3-7006-8d6e-de86af2c4463"), "en", new Guid("2925e425-6bd7-7e87-1885-4a1921999f73"), "Partners" },
                    { new Guid("adfacaa2-2fa0-7ed2-7bae-8aeda504f4c8"), "ar", new Guid("5b245aa4-efba-010e-497b-1716e298f0d5"), "تعديل المورد" },
                    { new Guid("ae1abfb5-d65e-ac30-5ded-2c07ddcc4733"), "ar", new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "حفظ" },
                    { new Guid("ae2b97fa-fbea-6d7b-25dc-14a69959b463"), "ar", new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "تسجيل مشروع جديد" },
                    { new Guid("ae2cb54d-7049-b5e0-ccae-a31896f5c137"), "ar", new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "اعتُمد قيدك" },
                    { new Guid("ae52cbd8-f8c4-48f3-ece8-9a4bc6b23fa8"), "en", new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "This item has entries, so its type cannot change." },
                    { new Guid("ae5fa845-65f5-62a7-2053-5597d359d418"), "ar", new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "نسخ التصنيفات والبنود من" },
                    { new Guid("aea28216-c2bb-678d-5a3a-932ed1169692"), "ar", new Guid("56ae38d2-084a-3fb5-1a13-772cc15213ae"), "إضافة بند" },
                    { new Guid("af3bbf36-2c99-551f-16b7-e385f0b53fe7"), "en", new Guid("47f27971-14f6-b7cb-a730-f265b420dc16"), "Report number" },
                    { new Guid("af4f8727-67f9-44b2-06fa-515f686c3108"), "en", new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "Sent for approval. Sign in to follow it." },
                    { new Guid("af50ad36-e9a3-998a-e935-02dc53d0d6ca"), "en", new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "Suppliers" },
                    { new Guid("af514d7d-67e2-587c-83b8-202cc2636710"), "en", new Guid("5362c8a3-f990-2309-67ec-d817a0810062"), "Default template" },
                    { new Guid("afd60553-d9e0-d075-a45d-bc2746653e11"), "en", new Guid("60f4a34e-1e29-66ff-56c3-02c72aea7686"), "No connection to the server. Try again." },
                    { new Guid("afe970f2-f153-8dc1-9469-6dce6a9350ef"), "en", new Guid("fe25fe7c-def5-fe97-4264-5d5ace06b24f"), "This project was already decided." },
                    { new Guid("afe9a25c-46ab-4369-d0e6-c992d6c2e5d4"), "ar", new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "ينتظر هذا القيد قرار المالي، فلا يتغيّر الآن." },
                    { new Guid("aff7468c-16c7-951f-64d5-f5334986c495"), "en", new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "Add supplier" },
                    { new Guid("b00a0fed-698c-9c72-9181-486aca8bc155"), "en", new Guid("602b884f-6c0e-9f7a-48b0-29e1ec3db7ec"), "A picture or PDF of the receipt" },
                    { new Guid("b015f457-0983-afca-0f2b-bc7d6609589d"), "en", new Guid("4fedd4a6-50e3-7063-5d5a-4fee6cf75f12"), "Date" },
                    { new Guid("b03d52e8-83d6-b8a1-86c2-fac93863fd9d"), "ar", new Guid("38b5c983-1c64-1df1-b077-be4a99515f9f"), "قائمة اختيار" },
                    { new Guid("b03e0175-efcc-ad93-7cb5-395ae811fd64"), "en", new Guid("0099032f-ca81-4339-2d36-56fdb23ad6c0"), "Add unit" },
                    { new Guid("b048caaf-7304-becd-6344-4443e8ee4021"), "ar", new Guid("a600acec-e37d-ba1c-1ec4-322b87ac9712"), "حذف" },
                    { new Guid("b0695acc-5528-7f25-ae5c-150a4d6020cb"), "en", new Guid("68500f85-3483-5951-a047-618c73bc9477"), "Documents" },
                    { new Guid("b0af1a92-9ebc-9d56-9d0f-20647e0c25b9"), "en", new Guid("e3004fbe-96e6-1618-a77a-bd12eb7f6e3b"), "The item is wrong" },
                    { new Guid("b0f17453-e313-2166-78b6-a5a32e5e3b00"), "en", new Guid("d9d33794-9a38-c6e2-00d3-e4abf7c6af32"), "Saved on this phone. It will be sent when the network returns." },
                    { new Guid("b0f7b8d5-978c-bc00-135f-c8e18d3c6d5d"), "en", new Guid("9a500d31-c9d4-87a3-0f52-be7ba38c43b4"), "This goes with an entry in a project you cannot change." },
                    { new Guid("b10d821b-3ca9-8e02-f22c-0bfc2a145db7"), "en", new Guid("828e8604-d963-be86-cb1f-4506027d2ad6"), "Attach to the chosen expense" },
                    { new Guid("b11e1995-d39e-e358-f91e-bedc7c1a2511"), "ar", new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "تعذرت قراءة المصروفات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("b1214456-32e9-8dc0-3d56-d7c1d8275c2e"), "en", new Guid("de7278c2-c6a5-218f-bbd6-55f908aee2c3"), "Its name, units and documents, and closing it." },
                    { new Guid("b165a593-83fe-23e6-f8d9-c1118168f082"), "ar", new Guid("e1e53e75-2a15-bb0b-aeab-b30e98e9b060"), "إعادة التصوير" },
                    { new Guid("b1792b8a-a1e3-89c2-f37c-f259114c181f"), "ar", new Guid("d3393a2e-74f3-b29e-c4cf-3a5c6ab27c56"), "هذا الملف محفوظ كمستند من قبل." },
                    { new Guid("b1854c01-8eff-bb0e-f8eb-2c31556ef60f"), "ar", new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "سحب" },
                    { new Guid("b1a8fd40-fb65-e3ee-3821-eb0ea0cad85a"), "ar", new Guid("9ad90439-bc2c-6788-3e50-09c4c447588c"), "فتح الملف" },
                    { new Guid("b1ad78b1-29af-25bb-25f7-5bf7b9a19b12"), "ar", new Guid("c639f7f4-f682-98b6-9edb-5ddebe6e8eab"), "الإعدادات" },
                    { new Guid("b1cdc609-141d-be64-30cb-05a2c5b15e2d"), "ar", new Guid("4d93403b-9585-4da1-bb82-0fb56ca0bdd4"), "التقارير" },
                    { new Guid("b1ddb234-4c7a-8d36-3327-ae1ff4088e2e"), "en", new Guid("31356397-adb5-a92e-fab9-edbdae1ca503"), "SAR" },
                    { new Guid("b1e8bccf-9c1a-cc4a-2ef0-f1d8ce0d1665"), "ar", new Guid("f23354f3-a95d-0349-e199-002851a61c15"), "رصيده الآن: {0}" },
                    { new Guid("b1ff9c33-75f4-5ed0-52f5-feb521768b24"), "en", new Guid("481f73bf-97b3-c99c-7512-dc7d94c12b26"), "The file was not found. Share it again." },
                    { new Guid("b2098f9f-8854-55e4-20c6-dad34dbf4cb5"), "en", new Guid("101214e9-dee5-e49c-cab1-38503d4450b4"), "Someone else entered this." },
                    { new Guid("b217d252-53b9-2e21-82d3-7624fbd1ad9a"), "en", new Guid("f204e3a5-e455-2eb0-564f-a8707664748a"), "Only the projects you have access to show." },
                    { new Guid("b21d00f0-e6ac-0cc1-cc81-7eedd631dcda"), "en", new Guid("39ad4b4a-7f47-a6bf-fbb8-205a73713e64"), "Invoice no." },
                    { new Guid("b24dadbc-2c8f-402f-129f-a954e9631890"), "ar", new Guid("ba3e3ad0-efd3-3322-fdfb-3cdbbe155bc4"), "تعذّرت قراءة طرق الدفع. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("b2903c28-0c9f-6203-6f3b-d759fc2a3e8f"), "en", new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "Documents" },
                    { new Guid("b2b57447-a5d1-43e8-02f3-b5abc254eff1"), "ar", new Guid("9d1f88af-c113-c76e-5788-70b467c68c5f"), "تعذّرت قراءة الشركاء. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("b3066af4-7e85-5095-a251-39f799f99fb4"), "ar", new Guid("f86c64d9-5ef7-0b62-ea66-57ad18d77c5f"), "الموردون" },
                    { new Guid("b32662e1-ac20-3ef9-0d7e-5d6dd85158d3"), "en", new Guid("97a8b48e-51a3-e1e5-7229-da3211d1ec1a"), "All" },
                    { new Guid("b32b53f8-6ae7-5b8a-4d87-e1924588dfb5"), "en", new Guid("1a928387-fb91-5a82-e042-9b39ad419467"), "Changed at first sign-in" },
                    { new Guid("b332f743-46cc-e453-3d01-e294eb643509"), "en", new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "This expense is already saved." },
                    { new Guid("b34ac086-f169-703d-ad3d-48bb542f5be4"), "ar", new Guid("02b967e1-ed23-df48-6e82-1ee35eace98f"), "اسم المستخدم" },
                    { new Guid("b36bbb66-29c0-88e0-83bd-0f3d35e06844"), "ar", new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "التاريخ" },
                    { new Guid("b38ae88e-cbfb-aae2-39c4-c4179c696ea6"), "ar", new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "يعتمد ما ينتظر أو يرفضه." },
                    { new Guid("b3a6474e-638a-a21a-f02a-9ca69cb912aa"), "en", new Guid("ccb69031-c62d-3916-a8ec-7b221acadc46"), "Name the group in 100 letters at most." },
                    { new Guid("b3b28dfb-1d6d-fb8d-60cc-2bdd682b8a12"), "ar", new Guid("b9082fd6-b5a6-158d-5eef-2b32e34be47f"), "اعتماد" },
                    { new Guid("b3d2cce1-0cb6-36ce-a025-1d132c6893cf"), "en", new Guid("3a4c7bbb-f2bc-f1d2-3bb7-32056745510f"), "Total" },
                    { new Guid("b4501234-fb15-2c4a-a472-b95b1eba84ea"), "en", new Guid("289fc2c6-a1cc-a7ea-98f9-46b5325c6cb4"), "Revenue" },
                    { new Guid("b488c2f2-6fc7-daf2-ab23-0fe7690d6e05"), "en", new Guid("a7afa38f-9ce1-cabd-b048-f71c4e96b033"), "Amount" },
                    { new Guid("b49b5d53-aac8-c57c-5092-36c83230e76d"), "en", new Guid("e75adf00-c3b4-ff87-6747-4821be7d85ea"), "Approves or rejects what waits." },
                    { new Guid("b4d51a4e-0907-9ec9-2c38-015a9d6a7217"), "en", new Guid("533f51e7-d239-a203-6b25-506321cf9034"), "Draft" },
                    { new Guid("b4e40a20-3f0c-5bed-56f7-421af4002991"), "ar", new Guid("501441ed-9b5d-2cd1-5b0b-f9429cc42810"), "رأس المال" },
                    { new Guid("b4e4d543-fa12-5ad4-302a-cb1db1520ac1"), "en", new Guid("6e6f2043-3714-fbfe-aaa6-8274f5ff1098"), "Use at least 8 characters with a letter and a digit, not your user name, and different from the current password." },
                    { new Guid("b50efa0c-f73b-3a6d-1e99-8b79c73ff267"), "en", new Guid("bd77c634-cf0c-8392-135f-a3d7e0c232aa"), "That partner is not in this project." },
                    { new Guid("b537132f-7209-b701-ca9a-68e307f410e5"), "ar", new Guid("2b323d65-b935-9024-19ff-f7d92e0304aa"), "إضافة مورد" },
                    { new Guid("b546f5c9-a722-8229-a43b-d4730a92982f"), "ar", new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("b557a25f-694a-e762-3979-a4a9ee538a31"), "ar", new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "تحويل {0} من {1} إلى {2}" },
                    { new Guid("b5634a95-8e1e-48dc-2586-fd20e5230c7e"), "en", new Guid("2031c582-a52b-baca-6cab-db6b6d21938c"), "Contracts" },
                    { new Guid("b6059236-48f8-38c1-ee34-96f03c35bee8"), "en", new Guid("57a1c46c-ea7a-0a58-0bce-f596e68b0b6c"), "No payment methods." },
                    { new Guid("b62d510d-1bc8-8dc8-8ca5-d19c3296fd86"), "ar", new Guid("9e3c91a6-2783-fa52-203e-fbcd93848e1d"), "أُرفقت الفاتورة." },
                    { new Guid("b641739c-7ebd-1d23-8003-c810aabd7db7"), "en", new Guid("999558f1-820f-444d-8020-cf4d0aa34358"), "Disabled" },
                    { new Guid("b65204a1-3ffe-2b75-5944-24abbcd9f56f"), "ar", new Guid("1548e8d5-514e-fdc6-580f-5ec78ed8dc29"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("b66f44e7-feb6-d98e-0458-d4ea0162537c"), "en", new Guid("e1c44e00-aaa9-49f2-fddf-82ee3a2897bc"), "Date" },
                    { new Guid("b6952d60-4568-045f-be8c-67d1baae49d8"), "en", new Guid("3ed0cf5a-537b-b891-2b5b-05d214530e71"), "This change was refused." },
                    { new Guid("b6c86106-2a29-f9b1-0270-cbf71ffa6ed7"), "en", new Guid("1d54d895-bc6f-b0bb-95b3-04b0adfb2786"), "Attach files" },
                    { new Guid("b6ff2b8f-e772-604e-3c3c-87062810560d"), "en", new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "No connection to the server. Try again." },
                    { new Guid("b745a96c-1c16-9063-e1a4-0b9ba5460f77"), "ar", new Guid("4dceab17-bb05-a54a-9b67-78115174e661"), "اسم الوحدة" },
                    { new Guid("b74efbbc-e2b5-96e9-28f9-654a876eac0b"), "en", new Guid("c72b099e-a1ed-4e24-d546-4cd1f82dcb4c"), "No entries yet." },
                    { new Guid("b77e71a1-02d2-c068-6796-b8be0b29316a"), "en", new Guid("848ee936-39bc-0bdf-e45b-3ef21de0b402"), "This file is already on an expense." },
                    { new Guid("b784bf3d-ca05-0a9f-f3d4-d5ed4dd8023b"), "en", new Guid("9a204ace-ddf2-e175-0369-fc78227a5d0c"), "Expense" },
                    { new Guid("b7e01401-517e-e321-19ce-aec59ac3c1c3"), "ar", new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "الأرصدة الفعلية" },
                    { new Guid("b809ee8f-3e36-fa4d-5711-e8ca9399c91e"), "ar", new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "تصفية" },
                    { new Guid("b82762a3-5c4f-7acf-8286-1cac80a17553"), "en", new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "Current password" },
                    { new Guid("b8395f34-5f8a-c127-d6ec-9feca2e315b6"), "ar", new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "نشط" },
                    { new Guid("b85b59c8-a2cb-53a1-0a6e-4543545e9872"), "ar", new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "تعذّر تحميل المستخدمين. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("b85f36f4-b9e6-2619-25c8-b21f8407bf21"), "en", new Guid("da7a2b71-6655-c965-411b-18f830383110"), "Save" },
                    { new Guid("b8b9d691-a292-2c39-cc4e-06abc62eca20"), "en", new Guid("0235f6de-0058-db2f-73d1-1a2f72aca48e"), "Your project was not approved" },
                    { new Guid("b8e00284-3351-ecbf-a316-01198df27aee"), "ar", new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "الصورة كبيرة جداً؛ يجب أن تكون أصغر من 1 ميغابايت." },
                    { new Guid("b8f89f3c-cc3f-066c-eb00-2997b444efaf"), "ar", new Guid("61e8b084-fc5d-d129-4eab-77ac71e6cee7"), "الرقم الضريبي رقم من 20 خانة على الأكثر." },
                    { new Guid("b908cf51-8972-34ea-abb4-58f7bfd7488f"), "en", new Guid("ebfe1024-9d74-67cc-c710-eff7ab5b337a"), "Say why." },
                    { new Guid("b917dc50-1777-e7e0-56c3-84a3a693e355"), "ar", new Guid("cef9c77e-4b66-df0c-bdfb-aee431fbc39c"), "مشاركة" },
                    { new Guid("b921f07d-8c0d-ae7a-a19d-38e9e38e6200"), "ar", new Guid("e8475111-cb44-b492-0147-e7f68f661565"), "إضافة مستند" },
                    { new Guid("b996a55c-99c7-a85e-7feb-6daec1d8a748"), "en", new Guid("d062adfa-7534-9d79-232f-8d7ebb5375d0"), "Disabled {0}" },
                    { new Guid("b9c4ad48-52e9-cdc2-0e51-076189c925f0"), "en", new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "New entry with this file" },
                    { new Guid("ba0c67b4-6d66-92e9-4c57-f84b89225a7a"), "en", new Guid("a8b28b8d-6a5f-d29e-716d-eb779b35f193"), "Copy categories and items from" },
                    { new Guid("ba4d6398-6834-732c-e75a-760262abe5ea"), "ar", new Guid("b73ed300-8a4a-3b72-380e-fa4652da0ce8"), "هذا المصروف محفوظ مسبقاً." },
                    { new Guid("ba526b82-2fa1-8b38-b8db-5158c720e391"), "en", new Guid("95e136cd-e108-c6dc-13ed-b987ae036063"), "The file could not be opened." },
                    { new Guid("ba95605f-ef31-ca7b-f253-710466eafe8f"), "ar", new Guid("2edf46b0-1df2-fb8f-c27b-ca3f9a3e7a7a"), "صدر التقرير." },
                    { new Guid("ba97a245-f1fa-b26e-5326-f194dd39bc50"), "ar", new Guid("d156a14c-9a0d-5f58-6249-dda176bf7b92"), "مصروفات بانتظار الاعتماد" },
                    { new Guid("ba99d72f-8250-4216-d6f6-5f3fdc6b4e4b"), "en", new Guid("d7acf450-4902-a00a-a930-0e1512044dfa"), "Forgot your password? Contact your project manager." },
                    { new Guid("bb265a13-bdb8-5b09-13ba-b0c1d1cb66aa"), "ar", new Guid("376d61bf-013e-c7a2-cf50-ec5d6b2cfe11"), "البنود" },
                    { new Guid("bb55ce0c-b381-050a-d78f-e1d8b7f9f452"), "ar", new Guid("b1903dbd-8660-ac0d-d56d-26e69f4bc1ca"), "لا أحد" },
                    { new Guid("bb67ef87-cdbe-de54-d31f-2d269f8a6576"), "ar", new Guid("6961eeab-eced-6391-4369-fa1d7dea2807"), "الحساب" },
                    { new Guid("bba8a844-a32c-636f-5cc5-668db02c3193"), "en", new Guid("d24b9a39-e4da-0e0c-ac58-9249561ac870"), "Default item (optional)" },
                    { new Guid("bbdee45f-ebf9-9cc2-954b-dd9cbbbff299"), "ar", new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "تحديث" },
                    { new Guid("bc0681ee-d10f-180f-f3e1-e69128b2d8c0"), "en", new Guid("e9035ab3-1df8-5100-e5ed-49c69a7f3604"), "Add entry" },
                    { new Guid("bc3e2fa3-9a60-26bc-52db-dc18efd88e3d"), "ar", new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "استخدام هذا المورد" },
                    { new Guid("bc4dd455-c1c0-c109-0c10-cb19970e85c8"), "ar", new Guid("ffb71c12-ea7d-ca20-dec5-a15d0b18e64b"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("bc717084-a565-0d2c-ce9c-3409534ba099"), "ar", new Guid("ea46842b-3d2a-8204-111e-18e19350629a"), "هذا الشخص ليس في هذا المشروع." },
                    { new Guid("bc73d6f9-aa26-0808-a87b-6ae8fcdcf2a6"), "ar", new Guid("1fe446b5-0aae-679c-1a2b-e689d8fc61ff"), "تعطيل هذا المستخدم؟" },
                    { new Guid("bc790e39-1fe5-7142-d642-a78d632d8aa5"), "en", new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "Could not load the registered projects. Check the network and try again." },
                    { new Guid("bc859c76-e6b0-a1c0-840c-a360e6953c7a"), "ar", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "ملاحظات" },
                    { new Guid("bc903481-d646-1e34-15d8-2250873d7d9a"), "ar", new Guid("86bc252f-a959-a7af-d3e5-d44e30e51763"), "اسم العمود" },
                    { new Guid("bc93c7e7-2261-ebd4-1dac-fbf16d28a8bd"), "ar", new Guid("e0f28ed6-aea3-7a16-7d90-1b2f8796700f"), "إدارة المشروع" },
                    { new Guid("bc9a7f02-eedd-add1-96e6-cc8b0fb2a04c"), "ar", new Guid("2bdeab56-6aba-a65f-aa3a-a6741d6fd983"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("bcfbb0d1-d1a2-b1af-916f-ca151fb92825"), "en", new Guid("a2b9b6a0-e2a6-76f9-6a69-d9b7848f6606"), "The general cash supplier stays on the register, because expenses with no supplier need it. You can rename it." },
                    { new Guid("bcff7fa0-bddf-3307-61de-2134c2caacbd"), "ar", new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "اختيار من الملفات" },
                    { new Guid("bd07f295-e1fe-244d-c14c-8db6968c80ab"), "ar", new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "الاسم كما يظهر في القيود" },
                    { new Guid("bd451b52-7f45-f998-5f7d-02061883875a"), "ar", new Guid("e84af6cd-4410-441d-7fa4-f1d800eb903e"), "تغيير النوع" },
                    { new Guid("bd567479-be43-96a7-14ff-495c1c53b137"), "ar", new Guid("e713af45-a01a-1dd8-c532-07221dd9d129"), "يُسجَّل خروجه عند أول إجراء تالٍ، ولا يستطيع الدخول حتى يُفعَّل." },
                    { new Guid("bd7127e5-b543-02fb-a27f-f22c19745cc5"), "ar", new Guid("3fbc510f-e893-ce53-2316-1964829c99fb"), "تطبيق على كل البنود" },
                    { new Guid("bdb6b091-da55-9df8-aec1-87f72da3beb1"), "ar", new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "هذه الدفعة تتجاوز قيمة العقد. يمكنك الحفظ رغم ذلك." },
                    { new Guid("bdf1673c-d93e-d96a-0a56-5cc98a228ecd"), "en", new Guid("a13edf44-3a0e-efac-4e44-4ec55dbda22d"), "Nothing new." },
                    { new Guid("be1ead27-a400-a768-2fcf-bbcd40acc118"), "en", new Guid("29ec94e3-2193-25b2-910d-684e12785662"), "Choose an item that takes contractor payments." },
                    { new Guid("be76414a-bec5-9328-1aad-667a4376acee"), "en", new Guid("d2626f2e-4e55-cb7b-1207-2925df60e749"), "{0} is now held by {1}" },
                    { new Guid("be95c09e-c195-465e-d12a-042ab44453f5"), "ar", new Guid("99616917-0265-ac67-cc5f-f394388643ae"), "داخل العقد" },
                    { new Guid("beefe2bf-5d6c-1d75-ffff-880119124f02"), "ar", new Guid("87e50af1-6ea7-1755-99f0-e54063095107"), "كلمة المرور الحالية" },
                    { new Guid("bf4b3ecf-eddc-3231-37fc-0aa8b7cea2ac"), "ar", new Guid("21722db6-09db-e46d-376b-05fe398dc252"), "الرقم الضريبي في الفاتورة ليس لهذا المورد." },
                    { new Guid("bf5db913-233c-6507-301a-a8f3c720810e"), "ar", new Guid("b3fbe8b5-d532-2a6b-77a3-46d077e8b7b7"), "أُرسل للاعتماد. سجّل الدخول لمتابعته." },
                    { new Guid("bf72b835-ae88-6b56-b8fb-18a69ef55559"), "ar", new Guid("2ada6c71-bf65-bb92-803e-0668858d52c9"), "الملاحظات 1000 حرف على الأكثر." },
                    { new Guid("bfaea1c6-4c42-8aa1-abf4-e15d28ef1535"), "en", new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "Payment method" },
                    { new Guid("bfaf5ea8-514f-0617-4494-226d64ecebb4"), "en", new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "Close project" },
                    { new Guid("bffad5a3-ae8b-237c-d9c2-102afc62a0c0"), "en", new Guid("6e8d932d-ba5b-7b8f-09da-d44b93b14d31"), "Quantity" },
                    { new Guid("bfffb476-e097-b544-4e6b-cbe22bbfd7d4"), "en", new Guid("7a0595cc-1f5f-dc55-9a0f-c83aa3c37510"), "Send now" },
                    { new Guid("c02775dc-0135-6f76-5c2f-e6ffab2bd0a0"), "en", new Guid("56188aab-17e8-90db-33b8-57ef5761ad3c"), "Sign in with fingerprint or face" },
                    { new Guid("c027e59b-44bd-490e-557f-e1beae6980a3"), "ar", new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "المبلغ" },
                    { new Guid("c047d356-3223-83cd-6643-144f0f6788df"), "ar", new Guid("33309176-122e-b83f-89ad-7b666e92093f"), "بدون البيع" },
                    { new Guid("c0552221-6525-e831-8a63-ba0c017f4490"), "ar", new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "لا توجد أعمدة." },
                    { new Guid("c0b7c618-9586-c57d-b1e5-c456603972ca"), "en", new Guid("786f5578-b9e9-21c1-0653-95497fc2c4bc"), "The server refused this change." },
                    { new Guid("c0dac685-2657-4194-7881-9d6e1a67aa95"), "ar", new Guid("2da0e6fa-d310-9d09-498d-8987765ec375"), "تُطبع جداول البند في ورقته بهذا الترتيب. اسحب الجدول لتغيير مكانه." },
                    { new Guid("c0e152d4-1ecf-107f-aac7-e708dad6c1f8"), "ar", new Guid("b431f747-6e0e-242d-4abb-474876efeeb4"), "تعديل اسم المشروع" },
                    { new Guid("c0e46a69-e649-a9c3-b5fb-5a76924abebd"), "ar", new Guid("5284c9c9-e2eb-3439-6370-d3e769bf4a6d"), "اضغط للمراجعة مع صور الفواتير" },
                    { new Guid("c13317b9-4c83-6689-87a5-b3482bcd1fe0"), "en", new Guid("1dd0b347-df2a-efd1-f980-3882b9f22cc7"), "Type each balance as a number with at most two decimals." },
                    { new Guid("c148e72c-fe84-d06f-6bed-1e2a96249cf1"), "ar", new Guid("bb889bd6-05ff-215e-a98b-114840aa2db2"), "إيصال مرفق" },
                    { new Guid("c14c3c92-b93e-3325-805e-3b69ea979ca4"), "en", new Guid("3f15fb81-ad8a-daec-0fc2-831ac08ff2a9"), "Notes" },
                    { new Guid("c1507bb1-74bd-4279-ebf2-90f4220fd276"), "en", new Guid("0cc52ca2-6354-2dca-d283-5d68fd64b6fd"), "Sign out" },
                    { new Guid("c1b7c721-22fc-4abb-7772-59d5345fcec8"), "ar", new Guid("75c17f5e-cb87-87cc-3022-bcf9fa626525"), "توجد قيود بانتظار الاعتماد. اعتمدها أو ارفضها أولاً." },
                    { new Guid("c1e4bcdf-26c8-401a-d875-07309a8e32a4"), "ar", new Guid("3d700b75-43c6-6223-d8ab-58e512f360fd"), "إرسال للاعتماد" },
                    { new Guid("c2073572-67a8-6929-8913-d87672d3b3ba"), "en", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "Unit price" },
                    { new Guid("c2ce3aec-ea61-478f-c365-054426c5c54b"), "ar", new Guid("db99bbe9-66be-5278-b5f0-232b520547a5"), "بانتظار الاعتماد" },
                    { new Guid("c2d75122-0484-0453-3314-154cc0b08f1f"), "ar", new Guid("f175fa8d-7f19-c536-797c-2e39f6ebaeff"), "الرصيد" },
                    { new Guid("c2fa55bc-06f0-5e62-285a-85e5c807e92f"), "en", new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "Approve entries" },
                    { new Guid("c34eaa54-94a9-7c67-77c9-b947237ec412"), "en", new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "What the project is, for everyone in it. Up to 2,000 characters." },
                    { new Guid("c382ae11-8729-e60f-48e0-99b0a70a17fc"), "ar", new Guid("0456debb-3ef9-ec1e-89c2-81f61e247353"), "الشركاء جداول هذا البند، فلا يتغيّر نوعه." },
                    { new Guid("c3a105d6-6e87-5737-dfa3-93a88690d257"), "en", new Guid("f8571475-b130-7a9c-37fb-305e561984e7"), "This project is finished, so nothing in it changes." },
                    { new Guid("c3af4e7e-e7c1-612a-1b41-dc81586582d8"), "ar", new Guid("8a4f1058-dc1f-d01b-b157-30e09c26a7ff"), "تعذّر تحميل المشاريع المسجّلة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("c3df0f18-b200-87a1-b710-ccecac22ab91"), "en", new Guid("5ca80f8e-9ed5-b201-6bc2-d408d2f1546d"), "All" },
                    { new Guid("c3fb2f69-2c21-ee75-975e-113586377cb3"), "ar", new Guid("689a982e-37ec-94f4-2980-b26a0563e156"), "يحذف القيد المعتمد: يخرج من كل القوائم والأرقام، ويبقى سجله." },
                    { new Guid("c4182e16-8b04-06eb-d26c-a55114c0c2b4"), "ar", new Guid("adac30c0-6957-9b47-7e6c-6eab5993367b"), "حساب الشريك {0}: {1}" },
                    { new Guid("c41f921e-119e-040a-9441-d51a2f13b60b"), "en", new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "Record expenses" },
                    { new Guid("c43b3cc7-cb30-0b9c-3d7f-4d2e5444ce4f"), "en", new Guid("cc91f0f8-c9dc-c060-380b-888212c6cbd8"), "Disable" },
                    { new Guid("c44b7041-88bb-9204-1c8e-ef6c96c080d7"), "ar", new Guid("e9160ba5-1b69-99b2-12f3-5206b9c2f576"), "المدفوع" },
                    { new Guid("c485cdcb-a72f-e65e-dcf0-081649bfa1ab"), "en", new Guid("cbb53a90-092a-78a5-cf23-5c33bec8b0f7"), "Could not read the report. Check the connection and try again." },
                    { new Guid("c4ba7de7-217d-f868-d3cc-259546894792"), "ar", new Guid("950d578b-60be-173b-32f6-2f91ed0565c5"), "حركات رأس المال والإيرادات والمطابقة والتقرير الدوري." },
                    { new Guid("c4f98685-f8b8-3f24-613b-cee24f2327f6"), "ar", new Guid("be2b4779-1157-7818-3138-61de471fdb61"), "حساب النسبة لكل جدول" },
                    { new Guid("c51f677b-d4a4-1c2a-8348-c71be175079d"), "ar", new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "حذف القيود" },
                    { new Guid("c52a61be-a397-1527-3563-a20bbbb342d5"), "ar", new Guid("d8653553-e111-a503-ccc9-af6dbc2704c3"), "حفظ" },
                    { new Guid("c595b861-7d34-9c84-97f6-41948f51bd1f"), "en", new Guid("b2ee8abf-87ec-8fe6-8bc2-90497031b108"), "QR code read and fields filled" },
                    { new Guid("c612bdfb-b377-78c1-3b1f-987856a54580"), "en", new Guid("dc6108e5-d00e-7366-079d-c085383fd361"), "Reject and send the reason" },
                    { new Guid("c61ae08f-d679-9668-1872-76d337dbdcd6"), "en", new Guid("d857b9df-cd11-7058-44ed-149d57ae316e"), "Documents of one group show together under its name." },
                    { new Guid("c63a9606-d26a-1d6c-2098-3506b39467a4"), "en", new Guid("88cd465e-6d2e-c0e5-7eed-02679ff5107f"), "Expenses by category" },
                    { new Guid("c65b0944-c196-84c2-18e8-614d03c94fc6"), "en", new Guid("49c3c74e-801c-8788-8358-3d5146a78313"), "No connection to the server. Try again." },
                    { new Guid("c65ff10d-e85f-0923-a7c8-2f7ae4855757"), "en", new Guid("5e14778e-c9c9-847a-d97a-3b0471c0cacd"), "Credit note" },
                    { new Guid("c692285c-b140-52e3-3ef7-240e8ba9fa6f"), "en", new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "Hide details" },
                    { new Guid("c6b0a403-2128-32cd-ce1a-8f6af5c8f074"), "en", new Guid("58bcb203-6d38-d376-4cb1-1db18f028703"), "Active" },
                    { new Guid("c6ec0d92-3b21-7092-6f78-42e9d80b9aa1"), "en", new Guid("4a978372-0bb8-8f15-cc36-afff5db0c982"), "Start date" },
                    { new Guid("c70e93d6-7e24-5ec2-a36a-6e21d3803a2e"), "ar", new Guid("67e33263-15d6-e8f1-7603-7f9f41359510"), "المشروع الافتراضي" },
                    { new Guid("c71f9322-5af5-1974-ed24-53969b26e4d8"), "ar", new Guid("7e82184e-8326-ee14-8c5a-3e0882567df9"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("c74f6343-0490-cf1a-d3d9-7684f4e06813"), "ar", new Guid("c399ddb9-dd32-a5fd-6d88-bf5e50bf12ba"), "إضافة مبلغ" },
                    { new Guid("c7e5ba40-7c21-9b54-4352-81aadbfeb5f2"), "en", new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "There is no partner to share it among." },
                    { new Guid("c7f17b65-f7f6-0634-2b1a-12e4372565a7"), "en", new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "Core column: it cannot be deleted" },
                    { new Guid("c8226c38-6ed8-3a89-e2a8-96da616eab76"), "en", new Guid("7722eb1b-97ae-02db-f23a-fc2064eba698"), "Counted at the last check: {0}" },
                    { new Guid("c84862fd-64a9-df39-5f68-c36a13b3a42f"), "ar", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "لا اتصال بالخادم. حاول مجدداً." },
                    { new Guid("c886e719-2e70-cc39-2946-485d7987ede4"), "ar", new Guid("e03bb912-f276-4631-ae48-d0ff54c96a26"), "تعذّر التفعيل. أضف بصمة أو وجهاً في إعدادات الجوال، ثم حاول مرة أخرى." },
                    { new Guid("c8c1fdac-7014-338a-4fae-9feecf7d82d7"), "en", new Guid("f848d1b4-8f77-2fb9-acc7-2bd95c196747"), "Description" },
                    { new Guid("c8c85611-21fd-2ba7-ceb1-e823a91c8b08"), "ar", new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "اسم الشريك" },
                    { new Guid("c8daed7b-bb4e-322d-fa46-536456195c23"), "ar", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "تعذّرت قراءة الفواتير." },
                    { new Guid("c8e7b8ce-e873-6ba2-cf12-331a066b55df"), "en", new Guid("cb895b73-22f7-75b6-8314-30d4da3f876b"), "Search by name or VAT number" },
                    { new Guid("c8ecc720-3aaf-cad3-ed60-ea5a5bb6c190"), "ar", new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "سحب مبلغ" },
                    { new Guid("c9014313-f285-f4e0-fa68-eb7d04eab008"), "en", new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "Could not read the figures. Check the connection and try again." },
                    { new Guid("c91218ab-8fe6-4e4f-19f3-41ee196eecae"), "ar", new Guid("5e05bf11-75f0-72c2-a100-c3f1f4569848"), "كلمة مرور مؤقتة" },
                    { new Guid("c9628631-0fec-75a3-8e06-74305a66a871"), "ar", new Guid("627e20df-8451-b393-4dca-65e11f806396"), "البند" },
                    { new Guid("c96dd33e-153d-d72e-fa54-c3d5d4cdab01"), "en", new Guid("f9008dc5-8ea2-6d76-47bf-d2c890d6acea"), "Your entry was rejected" },
                    { new Guid("c97c5262-719b-0e92-7a7e-a0bc7063aae1"), "ar", new Guid("18651164-b6f9-a403-1f85-a016015fc38d"), "تعذّرت قراءة المستندات." },
                    { new Guid("c9a115a5-383e-1b62-7340-dbca17bf1ad6"), "ar", new Guid("91ce291c-b4fb-0b1c-0b20-82fd13f712ab"), "العدد" },
                    { new Guid("c9aa1bbc-849a-9597-1ed4-fd2c2a12f1eb"), "en", new Guid("2ea2c1b4-6439-1f24-0708-076cae0d7be1"), "Any file under 25 MB." },
                    { new Guid("ca063417-6d1b-586c-ba51-935dc5891835"), "ar", new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "بيع" },
                    { new Guid("ca24b425-e2db-c35d-3376-c1e637bc52c1"), "en", new Guid("53912ea8-cc64-aab2-4551-ddd6603e25e1"), "Apply to all items?" },
                    { new Guid("ca4b8363-fa30-36db-66c9-2c2f91126eb3"), "ar", new Guid("7a8e5906-932f-70eb-2be8-fe5c2bd446d5"), "تحويل" },
                    { new Guid("ca53b9e7-fec4-993d-1fb6-a1c8b2b7d39c"), "ar", new Guid("7313aa92-61f1-1081-7685-b3f184740dda"), "حفظ" },
                    { new Guid("ca78df77-b12f-3d6a-9afd-29c2dbf86a5b"), "en", new Guid("7d9f7e11-de57-6746-3aaa-da22be2674c6"), "Project name" },
                    { new Guid("ca8a90fa-e652-27cf-eaf8-92f776185a07"), "ar", new Guid("9a388654-9894-e595-d200-a6fc18d83f3a"), "تعذر تنفيذ ذلك." },
                    { new Guid("cab370f6-b4cc-4094-42cb-9196f7e26070"), "en", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "Enter a unit price of zero or more, with at most two decimals." },
                    { new Guid("caeac573-0954-ef80-378f-ea758267a0af"), "ar", new Guid("9406c4c3-3fec-17cc-14ce-b9a6eb97dc2d"), "إرسال للاعتماد" },
                    { new Guid("caebcc3a-a820-07c2-d094-1425014d9551"), "en", new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "This project is finished, so nothing in it changes." },
                    { new Guid("cb2158d5-9575-9aa9-a683-498ebf016a2b"), "ar", new Guid("a6778df3-f6ae-496b-670e-e27f109f9a11"), "قيمة العقد" },
                    { new Guid("cc05b2f4-4642-dacd-5d54-6dce22b3999f"), "ar", new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "إضافة مستند" },
                    { new Guid("cc1e4606-7762-e2bc-66f5-0224b550e1d0"), "en", new Guid("a845c7b5-7c6a-5394-5e49-b2b92dfe32c0"), "New choices, one on each line. Those above stay, because entries hold them." },
                    { new Guid("cc3870ac-104c-49e4-e40d-ef8c85f38af0"), "en", new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "Open value" },
                    { new Guid("cca85eda-03bd-e0c3-b4b6-cf0f8333f803"), "en", new Guid("d0b175f8-91b1-2b54-f9be-fefa663a6bde"), "No description yet." },
                    { new Guid("ccebc95d-a5f9-0ea8-36ca-a53657efc666"), "ar", new Guid("e7af2e6a-4c0c-8029-5b2a-710ccfaec2f2"), "اعتُمد مشروعك" },
                    { new Guid("cd21c1fa-d9c1-2d32-9a24-8f480a5fa245"), "en", new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "This report's expenses" },
                    { new Guid("cd51533c-c98b-9db6-0722-0a42d0dbe6a6"), "en", new Guid("21575f31-6470-40a7-1f01-5c5e4b271c8f"), "Save" },
                    { new Guid("cd61fbfa-ad7e-a23b-5b5c-e781be3f0056"), "en", new Guid("49918492-7c51-8293-228b-6515961524d9"), "Say why." },
                    { new Guid("cd927d0b-dd88-d853-edef-9ddae47fb67d"), "en", new Guid("bf0fe6f4-08d8-4408-2871-16b53a5a85aa"), "Edit column" },
                    { new Guid("cdc61298-806d-6f34-b7c8-7a8fc2a6a17b"), "en", new Guid("21d95887-8bcc-8670-37f5-426441d6e91a"), "The date must be within the project's period and not in the future." },
                    { new Guid("ce50e0fd-6902-50f2-4525-5059edf59f3e"), "ar", new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "يرى المشروع" },
                    { new Guid("ce98fab6-382b-23aa-8678-1c0d30ec55bf"), "ar", new Guid("3d5f7057-0bdd-4ae2-8c10-1cfcaefe82bd"), "التاريخ" },
                    { new Guid("ce993095-af66-6b79-ddee-0bc79c52bfa1"), "ar", new Guid("0e7114d2-377a-4b29-b959-11a28a1e0cf2"), "حفظ" },
                    { new Guid("ce9e01fe-93ce-8325-0b26-25fdf38f6bb4"), "ar", new Guid("cef9eb7b-5faf-e8d4-a27a-de09b2f28e4a"), "بحث" },
                    { new Guid("cea439cd-63b8-f5e2-a1a1-4f3ef4a6accd"), "ar", new Guid("7528ef5c-ce52-b260-55f8-2bec69450d5d"), "يفتح المشروع ويقرأ كل ما فيه." },
                    { new Guid("cea63ab5-8b1b-16d1-b785-82bb19ffa618"), "en", new Guid("a2a7be53-b697-e4cf-4ba3-1201ad92bd81"), "A core column cannot be deleted, and only its name changes." },
                    { new Guid("cee4d6a4-0475-784e-a0c2-d65b8dd5b565"), "ar", new Guid("83e68eb5-2e9f-3110-3bb7-8b16b5248213"), "معطّل" },
                    { new Guid("cf56166f-ccdd-60b1-38b8-a78c5fbbfb52"), "ar", new Guid("0258dbfc-ff28-d70b-adb9-50e9eb52a057"), "إخفاء التفاصيل" },
                    { new Guid("cf97457f-9011-3ee6-e375-8314bbc0be29"), "en", new Guid("cd4a60f1-e5bf-1b94-d586-f8cffe541755"), "Register new project" },
                    { new Guid("cfaf598e-7e06-247e-fd2f-782f2cad5df6"), "en", new Guid("ec97e060-595b-d96f-9e99-bd7dc225278d"), "Registered projects" },
                    { new Guid("cfb7da61-781a-08e3-db09-46e586a455c0"), "ar", new Guid("f7aa01bb-a08c-fd84-a26e-0f4a02368f21"), "هذا البند عليه قيود، فلا يتغير نوعه." },
                    { new Guid("cfddfb4c-09a6-4628-c7c8-4461c40df153"), "ar", new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "تعديل اسم الوحدة" },
                    { new Guid("cfe22ac3-c042-7163-7235-2d1d4f9704f1"), "ar", new Guid("5dfaaef0-358a-0acd-dcaa-acc4d6d96f66"), "حساب الدخول هذا لشريك آخر في هذا المشروع." },
                    { new Guid("d00a0545-3aa7-4db1-bfc5-9418a5a548f0"), "ar", new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "المورد" },
                    { new Guid("d0137f61-0897-c280-669f-6bec27a159fd"), "ar", new Guid("7fb34e88-539f-648b-5087-8c28abb6f5c9"), "من" },
                    { new Guid("d04ce8da-1296-b46c-3ef8-bab65fd39e5c"), "en", new Guid("02819354-7adf-4d7c-0f6e-10d4840479a7"), "A repeated invoice" },
                    { new Guid("d06a7fa9-124b-a4a5-a47a-e70eb74eeab8"), "ar", new Guid("03dbb97e-c1fa-7d1b-b9d8-92d58a5c1f9c"), "قيمة مفتوحة" },
                    { new Guid("d079cf4f-4f08-d1f5-9750-45543cd0a5d8"), "en", new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "Custodian" },
                    { new Guid("d07c2efb-9b03-7d3d-adba-f66426f4c0c7"), "ar", new Guid("b795472d-3899-d5fd-cba4-5392d3b56864"), "المتبقي" },
                    { new Guid("d0885057-f7c0-0d35-9645-8bcb5079de25"), "ar", new Guid("92c24db1-d4f0-7ab5-7a5c-cf5daa120472"), "يرى رأس مال كل الشركاء، ويأخذ ملفات التقارير." },
                    { new Guid("d093b1e5-a8b6-e763-d9a4-c05c6508600d"), "ar", new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "بانتظار اعتمادك: {0}" },
                    { new Guid("d0b17e79-fb7c-c582-6fa4-bae0246f619c"), "ar", new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "لم يُصدر تقرير بعد." },
                    { new Guid("d0bd8ac0-7611-d6ea-4225-70813262f85d"), "en", new Guid("671d1d80-10e0-c2be-de3d-3a3361cf496b"), "Enable" },
                    { new Guid("d0c4b90d-4596-ead2-f8b9-3cbd3b220ef2"), "en", new Guid("71587926-7cf3-5cd6-d3a0-8142b33ee5a4"), "Close this project?" },
                    { new Guid("d0caac3a-dd13-a692-ddd3-5672d6cb6eee"), "en", new Guid("aa0b31a4-08c7-6eb4-9d08-822660b39582"), "This project is finished, so nothing in it changes." },
                    { new Guid("d0f3663b-c7de-8580-ab10-8f6a0201da8e"), "en", new Guid("15e6c2aa-44a1-0d26-a855-0066f951d5b9"), "Item" },
                    { new Guid("d1113ba1-239d-cf5f-8ab2-aef570600dc4"), "ar", new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "تم حذف الحساب." },
                    { new Guid("d123a465-22e3-4308-5306-4c179554a338"), "ar", new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "طرق الدفع ونسبة الضريبة والحسابات والشركاء والموردون والعقود." },
                    { new Guid("d153a608-8621-9b9c-b829-d9b4f844a1b6"), "ar", new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "عرض المزيد" },
                    { new Guid("d1885949-a744-d536-a983-56f47fbd09e1"), "ar", new Guid("26eff82d-3c22-ffbf-bb13-ff0182121378"), "المشروع الافتراضي" },
                    { new Guid("d1970d56-03c6-46ec-aad0-fb7034b4c13e"), "en", new Guid("d4480892-540b-1557-b068-b1fa0157b882"), "Contract value" },
                    { new Guid("d22e65e2-1391-5cab-1550-854a410fb3df"), "ar", new Guid("c3a34ea4-46fc-ebe4-efc5-8ae0325a4fb2"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("d24f07e9-da5b-373f-d492-871258154c78"), "en", new Guid("7c5178da-e72c-99ca-6caf-953d8a2a626b"), "Enter a mobile in digits, up to 20, with a + at the start if it has one." },
                    { new Guid("d24f10d0-0ca1-1835-8d7b-a85d68f5dd46"), "en", new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "While entering; without \"Keep the settings\" the supplier waits for approval." },
                    { new Guid("d266db2b-6ac5-0e45-62a9-27104b44cbfc"), "ar", new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "كلمتا المرور الجديدتان غير متطابقتين." },
                    { new Guid("d268dba9-834d-f034-f2e4-cf910a79ee59"), "en", new Guid("7bbb8fdb-be82-0c5b-4117-031834f49b7f"), "Kind" },
                    { new Guid("d2712a09-f97d-c1e1-9bd3-93c462deaead"), "ar", new Guid("838b078f-0a43-2ff6-73da-b3b4491ac5ea"), "إعادة تسمية" },
                    { new Guid("d29f07c7-34c0-cd1f-ea79-61cde3f238f3"), "en", new Guid("14a2c20d-3711-2ed5-51e0-6114c23a9958"), "Withdrawal" },
                    { new Guid("d2b0830e-d2ed-9e32-ec2c-d81942647d73"), "en", new Guid("f00a8d9c-5e55-2519-8f9b-79ad64bf0351"), "Date" },
                    { new Guid("d2f2d153-21a7-f53b-6c5e-ba95e8edb447"), "en", new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "Computed surplus (what the accounts should hold)" },
                    { new Guid("d31d47f2-f844-03a9-734f-3d521cd67db5"), "ar", new Guid("d681fd4a-60ac-accd-8bc0-132875a52b89"), "اكتب مبلغا أكبر من صفر." },
                    { new Guid("d34b8d94-c323-e735-45e5-b4840f194257"), "en", new Guid("a7f9b2fb-31d2-5a13-54bd-14811b2392a8"), "No projects yet." },
                    { new Guid("d34f2b37-d8e2-af17-a407-cfb54eee8f9c"), "en", new Guid("8a75bd17-549c-a12f-2cbf-bae1881dfcaf"), "Could not load the users. Check the network and try again." },
                    { new Guid("d371833c-264b-9a90-6e92-8146b718c4d3"), "en", new Guid("31a5e59d-76db-28dd-5f8a-5c4916a114e1"), "Could not read the expenses. Check the connection and try again." },
                    { new Guid("d3a8c4a6-c289-7be1-fac0-d10b7ca5612a"), "en", new Guid("5f1eaeef-20ad-4a12-71dd-275139c4b2f1"), "No connection to the server. Try again." },
                    { new Guid("d3fa33a9-cbd0-39f6-88d5-d5d6f3af2d51"), "en", new Guid("77c18bfe-8b4b-02bd-6b55-4e3906106c6e"), "Enter an amount above zero with at most two decimals, or a unit price and a quantity." },
                    { new Guid("d441dd0e-330b-feaf-153f-099d902bfa6c"), "en", new Guid("cc458256-b2d6-4007-2ddf-97a062a4d95d"), "Waiting for your approval: {0}" },
                    { new Guid("d493ade0-b55e-bceb-90d3-2150965e9689"), "ar", new Guid("b849bf92-456d-e8ee-7467-0189899fbec2"), "تعطيل" },
                    { new Guid("d4b31d3d-581e-ada5-8dbd-4b13375d4d7d"), "en", new Guid("de911436-2d9c-22a8-8bab-2a6ed5ba2820"), "That partner is not in this project." },
                    { new Guid("d4e97a31-d9fa-2649-5699-a6e3baff2fb9"), "ar", new Guid("517b7f24-f28a-ea6d-9337-f254bb9c2fe0"), "نص" },
                    { new Guid("d4fb6ff0-b19d-d02e-7359-104e3308068d"), "ar", new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "سبب سريع" },
                    { new Guid("d4fc8ea8-0edd-9c7c-6013-587236e7e06b"), "en", new Guid("4e93ddfc-eee1-14fb-09b7-68e3140e7816"), "This expense cannot take an invoice." },
                    { new Guid("d5050b2f-a083-f3c0-e96e-d1ac8142bbbf"), "en", new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "Remaining" },
                    { new Guid("d5323b40-30fd-f6e5-aac2-6a6eafe2c2cd"), "en", new Guid("8f6a9ff1-d7db-fc89-d679-f6379ed7d682"), "It holds what is approved now and never changes. Entries waiting for approval ({0}) show apart: expenses on one line, revenue and capital on another." },
                    { new Guid("d55fb92a-b4cc-448d-ce63-1dfa92003294"), "en", new Guid("9876a2f0-34b3-61e5-bce4-8373a59aa30d"), "Rename unit" },
                    { new Guid("d5615cd2-56f0-9ba8-49f3-c895c4999b67"), "en", new Guid("242892cf-3a73-8d62-4b58-0d740b6a3f01"), "Enable" },
                    { new Guid("d5ca4622-0846-9b0f-90b6-005f52f01989"), "ar", new Guid("302fd9fd-584a-c19d-e9fb-0437027fbcb5"), "الصنف" },
                    { new Guid("d5d0dd62-a7ae-8094-514b-86c95f46999b"), "ar", new Guid("e91317c6-a78a-90bd-982d-86fcd70c2e6d"), "القيد المعتمد لا يتغيّر." },
                    { new Guid("d5d3a3a6-b28d-7c2a-01a7-35f65fdb2cff"), "en", new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "Contractor payments" },
                    { new Guid("d5e21278-1717-4036-641e-8b0d7797277d"), "en", new Guid("819f545f-f676-205c-39b4-9f252160b826"), "This could not be done." },
                    { new Guid("d6198828-358d-c4b2-aec0-70281e50b55e"), "en", new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "Could not read the columns. Check the network and try again." },
                    { new Guid("d62b6c6b-cd23-ba5c-ad5f-9bc340f92f6c"), "en", new Guid("3ea858bc-cee7-2197-bdda-95ca1fc21d16"), "Transferred {0} from {1} to {2}" },
                    { new Guid("d65613b2-a7af-4f13-ffc8-9e6d0edd23fa"), "en", new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "A disabled account takes no transfer." },
                    { new Guid("d693debd-5ae5-74dd-7e16-bab96575f9a4"), "en", new Guid("95797639-57bc-97c8-840f-ce7fdc3feba9"), "Optional" },
                    { new Guid("d6b3e79a-de10-b5a3-d80c-aab6df19ca35"), "en", new Guid("a0e03f3d-852e-5069-9503-0fcb0dc63427"), "Dark" },
                    { new Guid("d6be266c-60a2-6060-8a47-bd1a0cfc9d24"), "en", new Guid("b379c1f7-e5e2-9b3c-947b-0d4d2ca6f100"), "In progress" },
                    { new Guid("d6d04633-6379-18fa-64c5-8d0a0fe4ce91"), "ar", new Guid("b6696a41-d69d-a941-ef7e-200e6cc08802"), "آخر تحديث" },
                    { new Guid("d6d2eee3-e183-5d03-14b3-591ffce846e3"), "ar", new Guid("c53eb634-036f-cbfe-2c8d-ad1130753b3b"), "المتبقي" },
                    { new Guid("d6e673fc-07a2-8a1b-09f7-6b3fab96c693"), "en", new Guid("1390dd4a-57ba-f706-667a-3a11503b3611"), "Could not confirm it is you. Try again, or use your password." },
                    { new Guid("d6f5189d-13fa-0e1d-e17b-0fc419621101"), "ar", new Guid("cb16e480-14f3-4451-bcfc-d698b272ec80"), "عمود أساسي لا يُحذف" },
                    { new Guid("d7794096-77d7-6539-5d2f-14ec8917f148"), "ar", new Guid("696720c5-0c62-2420-70d4-04467ae4060c"), "سحب {0} لـ{1} من {2}" },
                    { new Guid("d7796efd-3ec8-2eaa-9984-fa9f5818e73f"), "en", new Guid("51f00fba-db5c-2714-8182-e554dc6a0bb6"), "Project" },
                    { new Guid("d783a220-203d-52d7-daeb-bfe3e3437318"), "en", new Guid("9d2895f5-b4b6-b6fe-2e61-3da054109597"), "No user matches." },
                    { new Guid("d78a7206-209e-3a8e-a670-5541f32e20f9"), "ar", new Guid("b868c743-57b3-966c-bf96-67292b30293c"), "اختر ملف المستند أولاً." },
                    { new Guid("d82b471d-24c9-f838-c1f3-ff3a97cc2189"), "ar", new Guid("3331cac9-1828-89fe-b4f4-fcc7ca394d54"), "إظهار التفاصيل" },
                    { new Guid("d850eb67-7433-6417-be41-71062508507e"), "en", new Guid("6badde79-3784-70f8-5315-9ed699c74888"), "No connection to the server. Try again." },
                    { new Guid("d864ad82-617c-4f49-776f-9bece77202ee"), "en", new Guid("85f57724-f694-714f-0253-f870bfdb25a2"), "By {0}" },
                    { new Guid("d86deaf5-bfa1-6019-dfae-bd572c4844a9"), "en", new Guid("cefbd261-af15-a8f0-1690-5d7d93609f53"), "The expense could not be saved." },
                    { new Guid("d892dc31-36a4-2be8-747b-cb26dba2bffd"), "en", new Guid("204b1daa-eaae-c1eb-c979-646389e17604"), "The new passwords do not match." },
                    { new Guid("d894983e-cd89-4065-ffa2-24e8be2274e3"), "ar", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "التاريخ" },
                    { new Guid("d8bbe973-07e0-f170-47ea-6f188018793a"), "ar", new Guid("6ff27195-8804-5460-9908-204f4bf75070"), "{0} {1} من {2}" },
                    { new Guid("d8d3f074-8584-cc73-8c87-a238712d42ab"), "ar", new Guid("84c516d0-6dc5-5a84-9df7-f00d9f3eb773"), "الحساب المعطل لا يقبل التحويل." },
                    { new Guid("d90f5917-14c9-b5ec-bbb4-daf7ee1bc8b7"), "en", new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "Username" },
                    { new Guid("d9244a6a-d6a1-eb5e-3350-9d37c89cf15e"), "ar", new Guid("aa61ccff-4503-4423-7299-9244e9807aba"), "ستحتاج إلى اسم المستخدم وكلمة المرور للدخول مرة أخرى." },
                    { new Guid("d9349297-5a8a-54ff-af18-aa2487e8e9cb"), "en", new Guid("031eedd8-ca40-9972-3c5e-62436b4fcde2"), "That item is not in this project." },
                    { new Guid("d94d2fd7-57a3-2f53-4977-0dc14eec5101"), "ar", new Guid("423d6f2d-e430-8ac9-48fc-ae8bfa25cb6d"), "رفض" },
                    { new Guid("d958d35e-dec3-8479-9d91-34ff80e10e1b"), "ar", new Guid("3d391363-227c-a3d1-1197-343359825878"), "تفعيل" },
                    { new Guid("d9605ec2-dce2-3c02-f021-931b25374d79"), "ar", new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "لا مشاريع بانتظار الاعتماد." },
                    { new Guid("d96f8ded-1abb-c561-23c3-f6ae7a577b0f"), "en", new Guid("f16951fa-955f-b5ce-3d07-6959b3c1f936"), "The product name is too long." },
                    { new Guid("d9b4342e-66ce-7d2b-f139-fd9b65ad80d4"), "ar", new Guid("13d5a6d3-2192-2762-27cf-53e5cb92e9d8"), "الفائض المحسوب (المفروض بالحساب)" },
                    { new Guid("daaf9e11-a6e3-f149-7cd5-91049a23c2e2"), "en", new Guid("1b046c9f-0281-026d-d058-b2d9f6f0b69b"), "How many entries the home shows, and how many each Show more adds." },
                    { new Guid("dae75be4-a8c8-f10d-0bb6-5049c866c748"), "en", new Guid("e890cfcf-1cc2-5d22-91f3-9f7e866dc452"), "Account deleted." },
                    { new Guid("db0d1ce8-769e-0f7e-e39e-0b1e8305bb9f"), "ar", new Guid("25567d30-09c9-af1e-dc76-6ecc83c5bc0e"), "تفعيل {0}" },
                    { new Guid("db223d83-c6f8-65a2-2f8b-00a82101b117"), "en", new Guid("a9e61c2c-ca64-b70b-195b-fd96d498488d"), "Difference" },
                    { new Guid("db328cd6-399b-da9d-2bde-b64c7cbf076f"), "ar", new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "تغيير كلمة المرور" },
                    { new Guid("db4a6104-5e3b-5410-c4ed-29390ae2aabb"), "ar", new Guid("a66af0e7-4d06-079a-9ad0-8ac2ac2d93a0"), "سعر الحبة" },
                    { new Guid("db5dcb9f-e254-f1c9-c2f0-f539c6203ba9"), "en", new Guid("a22e9a4f-6864-e64e-de6f-911aef07c007"), "That value does not fit this field." },
                    { new Guid("dbb25a89-ddf0-72f1-c985-e645573ae3b5"), "ar", new Guid("d4d54668-94a8-24a2-a92e-711ee4ce86c5"), "هذا الرقم الضريبي لمورد آخر." },
                    { new Guid("dbca40e4-b309-7d4b-bfc1-ba2cc32c009a"), "en", new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "Write at most 2,000 characters." },
                    { new Guid("dc07afa9-eda1-de49-25de-a215d4f96d02"), "en", new Guid("eb50f295-45c5-9ef0-5656-66f32cc12c21"), "Too many wrong tries. Try again later." },
                    { new Guid("dc0ccf4e-494e-e2d0-8594-d68f23e3fb31"), "ar", new Guid("28ab262b-9f6e-2f1e-19e0-9798b7289063"), "للقراءة فقط" },
                    { new Guid("dc3cdff0-1164-a105-a310-b60e54f4e8a4"), "ar", new Guid("c441eaa7-e92f-ec27-e0bc-e60d9f0d9510"), "تحويل" },
                    { new Guid("dc7066b8-452a-d89d-5dd4-8a06c9e4c22d"), "ar", new Guid("0769b8e3-4101-6c97-8929-de379be41ff6"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("dc89417e-d584-78fa-f8ad-ae03f0d34cbd"), "ar", new Guid("bdbf42d2-e8d6-59d4-43ae-ef73d5633cdf"), "اسم طريقة الدفع" },
                    { new Guid("dc93e950-2e06-1e46-381c-3782e8f8084c"), "en", new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "Enter a mobile in digits, up to 20, with a + at the start if it has one." },
                    { new Guid("dcd0b709-c7c4-92bc-a383-0184ef386d3b"), "ar", new Guid("7aa4cb9c-361f-c384-b305-c794d843c08b"), "غيّر كلمة المرور" },
                    { new Guid("dcd2ad27-d516-05c6-dcb9-b813fe8ba62d"), "en", new Guid("36ef27c5-fe22-8894-89bd-30a10c24cf7b"), "The expense could not be saved." },
                    { new Guid("dcec7463-fb3d-212e-5bd4-05380592cb6b"), "ar", new Guid("8fa855b8-82dd-277c-08c9-23f47523cf46"), "لا يوجد شريك لتوزيعه عليه." },
                    { new Guid("dcfd8610-399a-ac1b-e836-557c7ced95bd"), "en", new Guid("81e98bb6-41e3-c651-2b2c-499fddd37f98"), "See the project" },
                    { new Guid("dd05bec7-4fcd-6d78-58e4-259c17bc0018"), "ar", new Guid("03d517df-f8c9-f45a-bd70-8ffd68bb9741"), "تعذر تنفيذ ذلك." },
                    { new Guid("dd73dad7-06cd-1f93-6496-ec5376c11b58"), "en", new Guid("218c9be0-b314-2316-5af3-bebb4c2e08fd"), "Partner" },
                    { new Guid("dda76acf-8e8a-0661-d7e1-c84317b3d395"), "en", new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "Two units cannot have the same name." },
                    { new Guid("dda850e5-37a4-874e-2ee0-0fc01febaf4e"), "ar", new Guid("53e4fc4e-1871-f976-c534-cede7db19503"), "الصلاحيات: {0}" },
                    { new Guid("ddbdacd8-3021-3287-d720-ad056334d754"), "en", new Guid("c5514c54-33b7-fc1d-25c8-13ba413cd0e1"), "The picture is too large; it must be under 1 MB." },
                    { new Guid("ddc67816-317d-043b-e2a6-7b8fe596ea8b"), "ar", new Guid("ccb47e87-a076-9863-36ac-daeed28b8011"), "تسجيل المصروفات" },
                    { new Guid("ddc946d7-c8dd-f2fd-49b3-d9da769ad66c"), "en", new Guid("8715efff-ca9b-9091-09d3-f091e5f9b292"), "No projects are waiting." },
                    { new Guid("de8446f0-0df6-3c4a-2cce-f280a721c2b3"), "en", new Guid("75cf2362-a9c4-5e88-4164-a49b15ce6d27"), "Half year" },
                    { new Guid("de955506-0678-30c5-aba0-c74616789580"), "en", new Guid("928081e2-36dc-555b-7453-9f98e48d99c3"), "Open" },
                    { new Guid("deba4109-3767-e9f1-1013-381fc48fb0af"), "en", new Guid("03475c48-e1b4-9066-cafb-7f976c461e98"), "Disable" },
                    { new Guid("dec2ca4f-b171-73d9-0664-232f6dd31143"), "en", new Guid("cf58aac4-dc75-18a4-4d4e-e57d32f8bfda"), "In reports" },
                    { new Guid("dec6a131-a057-7d43-38b3-ec5d4403a656"), "ar", new Guid("7c550e36-e436-b664-54e5-4c1036f1c673"), "تعديل وإعادة إرسال" },
                    { new Guid("decb554a-544e-f514-ad1b-d6fec5bed241"), "ar", new Guid("418bd004-f9b1-3ac8-ff74-3e22943f5476"), "إضافة مجموعة فرعية" },
                    { new Guid("dee8c0c9-4367-ec4b-ca1e-4a18947057b2"), "ar", new Guid("c623b333-e28e-2731-732c-449230d2c0f9"), "مصروفات" },
                    { new Guid("deec1ebb-84a0-6c1e-4e9c-7bf6a361097a"), "ar", new Guid("4ab76c09-7697-3a77-2698-4fb7b30d48e4"), "لم يعد أحد الحسابات في المشروع." },
                    { new Guid("df20d423-6470-a0ec-1caf-cb2aa3d86f60"), "en", new Guid("9f70a91f-7c59-9706-ab0e-ff8f610345e8"), "No table matches." },
                    { new Guid("df49099d-4369-3ac2-87af-0ac47c1b4b8f"), "ar", new Guid("a1ab47cf-7bdf-64e1-e5dc-88e8b861dab7"), "أدخل سعر وحدة صفراً أو أكثر، بخانتين عشريتين كحد أقصى." },
                    { new Guid("df4bea51-871a-3c27-66cf-faf13c74541d"), "en", new Guid("959d271a-89f7-17a4-e7d4-6f503de4cc75"), "Purchases" },
                    { new Guid("df521f26-396a-8be5-8eee-ad658f66c2af"), "ar", new Guid("ed206502-29cb-47eb-d3da-50081a18dc80"), "مشتريات" },
                    { new Guid("df992583-74f1-ed92-3cf7-1add9451b823"), "en", new Guid("91c17e1e-fd83-0186-3d9d-56349e3235c7"), "Pick from files" },
                    { new Guid("dfac1ddd-60c1-8e56-04e8-b9515077db46"), "ar", new Guid("a30144a0-067f-9738-be76-58ce171612a5"), "لا يمكن التراجع عن الحذف." },
                    { new Guid("e09c9942-0742-1b5d-1385-68735c76100c"), "ar", new Guid("b2c007d5-af70-bb8e-ea39-a7599e19bb85"), "تصدير إعدادات المشروع" },
                    { new Guid("e0c35421-b088-3b0e-75e6-57634ec027b8"), "ar", new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "لم يبدأ" },
                    { new Guid("e0f53c3f-798a-5275-358b-7c227333fb0a"), "ar", new Guid("7ebeed80-b35a-7f5b-4f47-96ed95f3c879"), "تم حفظ الوصف." },
                    { new Guid("e13605a7-1868-edf4-99ad-ca08023ecda9"), "en", new Guid("fb13c972-25d4-62c5-d9e3-032537936bc6"), "Tick what this person may do in this project. Unticking \"See the project\" takes them out of it." },
                    { new Guid("e182625f-6fe9-334e-6634-9375d0227bc1"), "en", new Guid("4d0ddf42-3e31-c2be-bbd1-e046a203bb54"), "No report issued yet." },
                    { new Guid("e1e22577-9284-d54e-26c9-72202ddf4fa0"), "ar", new Guid("a99cbe37-22ed-be76-50eb-887d569ce687"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("e1f480fb-09d9-7e1b-a1d5-7c4b8c163e33"), "ar", new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("e1f5a845-9682-8efa-0269-5929fc8776cc"), "en", new Guid("f199e153-a803-98f3-82ad-fa9cbca08c22"), "No item matches the search." },
                    { new Guid("e1f94ecb-f861-48f4-2103-dbea7cc2dd65"), "ar", new Guid("f8d624df-18d9-793f-5c54-5c2d689e0c7a"), "رصيد {0} {1}، وينتظر عليه {2}، فينقص {3}" },
                    { new Guid("e200befd-e428-3154-ce16-c39684370923"), "ar", new Guid("420c7e21-3622-70d2-9e25-606eafec4461"), "اسم المجموعة الفرعية" },
                    { new Guid("e20ceb0f-32db-12d6-b76f-38021ed02301"), "ar", new Guid("9466d9b1-4985-04e7-43be-3661d88e826a"), "إغلاق المشروع" },
                    { new Guid("e291a276-e0d0-09ac-3edb-7ce78116064e"), "ar", new Guid("c578af1a-a6a5-c34a-80cd-77ac151c3526"), "المسمى الوظيفي" },
                    { new Guid("e2b5213e-d00c-171a-f71e-edfdef4804fe"), "en", new Guid("4fa3122e-1ca9-a581-ef8f-d3bbf6108d1e"), "Supplier" },
                    { new Guid("e3398233-b521-bba9-a5a2-05fd743d420c"), "ar", new Guid("9204e426-af38-ad74-31fe-b90cdd796f65"), "ابحث في {0}" },
                    { new Guid("e34d1ae3-d6c8-96c7-2629-cc1d38e22b70"), "en", new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "This username is taken. Choose another." },
                    { new Guid("e354a690-42e6-edd7-f26d-bd7d9de2d4a6"), "en", new Guid("a6b357e6-3a37-7f35-fe4e-fe2dad16a421"), "Add account" },
                    { new Guid("e38ba2fd-e815-7c4b-134e-9702bece859b"), "en", new Guid("79c8bd2c-4f2f-eff3-df8c-826c8386cfd1"), "Partner name" },
                    { new Guid("e3929ee3-ac15-caab-1154-3dcfef5b0fdb"), "ar", new Guid("ac416bb0-0b76-e45d-5c5b-e8dd34b4bd29"), "تعطيل" },
                    { new Guid("e4060706-fac1-de53-48f3-3c3c72d10922"), "ar", new Guid("3548fc89-0bf0-833b-a7d3-41a32211c447"), "اختر المشروع الذي يُفتح عند الدخول، أو ألغِ اختياره لتظهر القائمة." },
                    { new Guid("e461b3d6-12af-fcae-11cc-2783ac594cae"), "en", new Guid("7e9ea7f0-6d30-5238-6983-354604749559"), "Choose a picture or a PDF." },
                    { new Guid("e46aa106-94d9-1082-7dd1-b7cd04c860ba"), "ar", new Guid("4c26822e-e06b-e9e2-f35f-c777dbe621d8"), "يجب أن يكون الملف أقل من 25 ميجابايت." },
                    { new Guid("e4b27f47-6391-5082-1c40-9983b685b345"), "en", new Guid("c1987e37-ee52-46cc-d514-2fda4be0b52a"), "This project is finished, so nothing in it changes." },
                    { new Guid("e4f1ff32-b9ad-4487-35af-8e5389d38792"), "ar", new Guid("f99247a0-15ac-6f60-2448-20f9343b51da"), "لم يعد هذا البند يطلب هذا الحقل. افتح المصروف من جديد." },
                    { new Guid("e51d665f-3d79-94f2-18d1-7ae7c443ecbc"), "en", new Guid("2d50413b-5f55-8ea2-5176-0289873a10cd"), "Total of actual balances" },
                    { new Guid("e5691cf3-c78a-28e8-dd12-e34c8fdda7fc"), "en", new Guid("db4f52fd-08a3-49ea-7979-2b9632296230"), "Outside the contract" },
                    { new Guid("e57b4282-78c8-f2e6-571b-c47126609ec1"), "ar", new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "اسم البند" },
                    { new Guid("e5d69077-a483-8894-8bb1-38f7d026719a"), "ar", new Guid("a623fef9-9131-2343-7812-dfd0db58a88b"), "القالب الافتراضي" },
                    { new Guid("e5e5629f-598f-db97-8e0d-aa2760cddd95"), "ar", new Guid("af4e7146-0390-4ae9-51ac-5d24f7a48b0f"), "بانتظار المزامنة" },
                    { new Guid("e64dbe64-5c66-07e5-b91f-6434c70108b5"), "ar", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "الشريك" },
                    { new Guid("e6652c43-48e0-9aa1-c34c-a6b26b1951a5"), "ar", new Guid("7c46044b-11e2-c91d-47d8-c6e2ac25ff42"), "الكل" },
                    { new Guid("e6db5caa-81d1-c6fa-1e33-3d8850e1ff59"), "ar", new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "تعذر جلب الملف. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("e6ebdc9a-0eef-7821-1fcb-24851593c2fd"), "ar", new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "حساب الشريك" },
                    { new Guid("e71eea87-53cd-9cfd-ad88-009cd0a7a45a"), "ar", new Guid("fbf5c1c6-0711-e783-e60d-f7b4b5909fa7"), "لم يعد أحد الحسابين في المشروع." },
                    { new Guid("e773f6e7-f71c-3206-a1c7-a10243befe56"), "en", new Guid("fea6811e-cbc3-d4a9-30db-48746629ea6e"), "Filter" },
                    { new Guid("e8040fc1-e214-a8c5-dd64-7c22c78e8c09"), "ar", new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "اسم المستند" },
                    { new Guid("e8299e2f-fcbd-ac3e-feee-9f81c3ea1eb4"), "ar", new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "هذا القيد لم يعد هنا." },
                    { new Guid("e83097b1-5f6a-96e5-d929-147658347f21"), "ar", new Guid("d7ac423a-16a1-47b1-d833-12ce010a99b0"), "لا يصح أن تحمل وحدتان الاسم نفسه." },
                    { new Guid("e893aaaf-469c-f4ca-539c-e6aef0e71e61"), "ar", new Guid("ce47b02e-e9c6-9218-1e86-cbd3ff295615"), "سعر الوحدة (اختياري)" },
                    { new Guid("e89b9bc6-73cc-48d5-fbbc-79baa7352dae"), "en", new Guid("77ac899a-2c86-99d5-b7fd-2e6ee1cc1710"), "Amount" },
                    { new Guid("e964c709-b218-8312-9cc8-5492b306673d"), "ar", new Guid("fbe18f17-7aca-e801-0031-793076552587"), "ربع" },
                    { new Guid("e9901f44-61d1-3c11-b67c-9d13e8436cc5"), "en", new Guid("78ac582e-f81e-4937-c647-9b32d73b8886"), "System" },
                    { new Guid("e9b39b25-e0b4-5c6a-b958-a9e297e20c09"), "ar", new Guid("1ad78c31-0366-8535-b690-d668cfead1df"), "المبلغ" },
                    { new Guid("e9f4f448-07d6-a616-b57d-ce81a31bd3f0"), "en", new Guid("7ca538d7-3f4d-53fb-1935-3db639949765"), "Amount" },
                    { new Guid("e9fd8ffe-e017-a51d-a59a-365eed5432b2"), "en", new Guid("ded470ed-f243-7198-f2d2-b8bd5ea2fff0"), "Invoice picture" },
                    { new Guid("ea0ba365-45bc-a997-1790-4678e3a7ea68"), "ar", new Guid("cb3e1b79-14ee-f733-c669-85ffa3099d90"), "تعذرت قراءة الأرقام. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("ea134a38-26b0-5b1f-83a7-aa03b4db147b"), "ar", new Guid("8f49d7b4-08aa-6c94-9aad-334dac55b2da"), "مسودة لـ{0} تذكره." },
                    { new Guid("ea197b2d-f1ec-396b-c690-1cf5abc648fe"), "en", new Guid("2b347c6e-15bb-bf2c-9812-fc7c8e36fe1e"), "That file is empty. Pick another." },
                    { new Guid("ea2b89c2-6ba0-2cfa-8597-955a77052eaf"), "ar", new Guid("99630674-1fe3-fb14-81dd-1a39c1af1703"), "المجموعة العامة للبند لا يُعدَّل اسمها ولا تُحذف." },
                    { new Guid("ea37b653-cff1-3ffc-cb79-b3f78e05d996"), "ar", new Guid("d58964e4-f346-6c5a-ba20-56a307f76f20"), "حفظ الأرصدة" },
                    { new Guid("ea3c1ee1-238d-8acc-a4c4-45749a6a5f30"), "en", new Guid("bed09e4d-ae1e-c18c-21b2-8e0bf3e99e23"), "Actual balances" },
                    { new Guid("eac29f40-7500-4caf-55cf-9c3f53a0686b"), "ar", new Guid("ffe71948-11fe-fc36-0edd-9991f74d9188"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("eb12f83c-0111-7126-047d-d7f7dbe69efd"), "en", new Guid("b8fccba4-17f6-71b3-026f-8b4b6df011e3"), "Supplier name" },
                    { new Guid("eb241407-0e68-8e0f-9524-ec9ffb88bc8a"), "ar", new Guid("79dfe678-4a14-27a6-a438-2b159ea9f4dc"), "أثناء الإدخال، وبدون «إدارة الإعدادات» ينتظر المورد الاعتماد." },
                    { new Guid("eb310cf0-0aeb-4a41-afa5-9e6ef8401eee"), "en", new Guid("8bb3b13e-37cd-23f0-c804-e753030b9a71"), "This project's capital item is disabled or missing, so a partner cannot be added." },
                    { new Guid("eb34bc4f-46f7-e1f3-76c0-ec50de20a9f9"), "ar", new Guid("3436f16b-5a5b-3d5e-1597-8c4f5592fa43"), "تعذّر على الخادم تسجيل دخولك. حاول مرة أخرى." },
                    { new Guid("eb40b015-7cd0-8bbb-2498-79d928b5bd53"), "ar", new Guid("6f157cb0-f378-1140-7760-ab27a2a8b5c1"), "اختر الملف" },
                    { new Guid("eb521ae0-de5c-7bc9-87e2-080781de3586"), "ar", new Guid("0510a2b0-1a69-9340-a9ff-336ab0317b22"), "محاولات خاطئة كثيرة. حاول مرة أخرى لاحقاً." },
                    { new Guid("eb88ea9a-4dc7-5cc7-a84c-8facbb35040c"), "en", new Guid("5a6d64e4-bbf8-955e-45af-d8cdba56450d"), "Show more" },
                    { new Guid("ebe3e3ae-c8f1-bdfb-8e81-5a070fbb108a"), "ar", new Guid("a3570f57-d336-573f-3eef-dfdfbc93440d"), "الإشعارات" },
                    { new Guid("ec4581f7-676d-06c7-9300-88ca040dcb92"), "ar", new Guid("34f597ac-32db-4988-0a50-6644b4f6ca18"), "تفعيل" },
                    { new Guid("ec5c3b41-0426-b222-161a-6980f742ff2f"), "ar", new Guid("3bfbeb3b-8ce1-9e8b-844b-07b2542b8117"), "يظهر في التقارير المطبوعة" },
                    { new Guid("ec72daf3-bb87-dda8-8795-6258d81cbb08"), "en", new Guid("5ac218a8-9c91-94cf-3e4b-2e5888b3e41c"), "Custom" },
                    { new Guid("ec804f07-bd20-8d37-f843-e20c7eb2d63a"), "ar", new Guid("36cd458b-f7df-d4d1-233c-4aa193b2f9ab"), "لا يوجد في القائمة مورد بهذا الرقم الضريبي." },
                    { new Guid("ecb26879-92f7-96d4-dfb1-981c14462a18"), "en", new Guid("b5126880-99f8-1137-ad27-d3c4362c1cbf"), "The notes are too long." },
                    { new Guid("ecb7fbf3-ee85-8368-44c7-7e094c1cb98f"), "ar", new Guid("bd8e32e1-ea45-cc38-b532-6d9b116ce501"), "اسم المستخدم هذا مستخدم. اختر غيره." },
                    { new Guid("ecd1c0b8-0061-64c3-8c02-f81e7a5a3f38"), "ar", new Guid("902fc6b5-bbfd-c0c2-1454-9734955dfb31"), "تعذّرت قراءة الحسابات. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("ed09774d-483c-6953-ee99-c08ad6da3024"), "ar", new Guid("abda6c4e-9ecd-7db1-5a4e-4e275466da2d"), "English" },
                    { new Guid("ed84fea8-e269-239d-d688-0f9a9d87e678"), "en", new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "Status" },
                    { new Guid("ede1dd51-994b-e938-966e-25fb3bf3d09e"), "ar", new Guid("212bb87c-af38-a4dc-f492-d4d9337e483f"), "نسبة الإنجاز" },
                    { new Guid("edf1e0e9-0853-1142-1929-a6ae84d8a00f"), "en", new Guid("5d7d57cb-582d-ae44-3442-32006955b507"), "Choose a table offered to you." },
                    { new Guid("ee1ca999-68a1-5ab9-e3ca-20b296d00172"), "ar", new Guid("b3300888-6503-ff69-d52e-dd2f55114352"), "الحالة" },
                    { new Guid("ee253398-1340-6bd6-c74c-055af47c8aa2"), "en", new Guid("1468ab7b-9abc-30b7-2414-7358c4ee6567"), "The name as it shows on entries" },
                    { new Guid("ee2f60c8-35d5-2f41-9b87-89aa5d128677"), "ar", new Guid("d94488a2-70c8-d228-301e-539d46c5c0a4"), "تعذّرت قراءة الأعمدة. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("ee66f33a-25fc-f1a1-591a-e601c2910591"), "ar", new Guid("18bd135b-d31c-b651-a588-29f349aeef05"), "خارج العقد" },
                    { new Guid("ee6ad155-fce7-080f-7d48-9427942009c1"), "ar", new Guid("4f42daeb-c624-5557-7811-db10c51c86af"), "يصوّر أو يختار ملف الفاتورة أو الإيصال أو العقد." },
                    { new Guid("ee7c7d45-5dbd-0a6a-190f-b662d47e0868"), "en", new Guid("dc487e2b-badf-0cb8-e0fe-25023c5569ea"), "Disable" },
                    { new Guid("eec075fa-f5f6-87b0-d5de-603142937bfa"), "en", new Guid("2a9f8f05-90b0-cdc5-679a-77feb4fad691"), "Use your password to sign in." },
                    { new Guid("eec1dec6-5897-4be1-fc18-966c245f7fbf"), "ar", new Guid("0ff9fad8-f47e-3d63-6319-59eac7685f33"), "الحركات المالية" },
                    { new Guid("eee66bc5-8de0-c896-d146-3329eba9c822"), "en", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "Save the movement" },
                    { new Guid("eeeed37e-ca44-83a6-0d16-1cf0c9cc94e3"), "en", new Guid("ab4c2d48-928b-1109-1660-5c47d7108d91"), "Partner's account" },
                    { new Guid("ef0227cb-8383-d67f-53d5-426a964faf93"), "ar", new Guid("e7ec9288-6a5a-d9a5-5200-873ad377cc13"), "صاحب العهدة" },
                    { new Guid("ef4a5786-3d73-ef98-1744-e6b6c6e4637c"), "en", new Guid("a3c7960c-1745-a4db-449f-935a870adbc3"), "Payment methods, VAT rate, accounts, partners, suppliers and contracts." },
                    { new Guid("ef7d5929-ecd8-e00d-4e5b-aa1964e2f9d0"), "en", new Guid("dddacedd-b79c-aad1-66ba-682a14134923"), "Payment type" },
                    { new Guid("ef876afe-f68b-15ee-9de0-5ae887176e8b"), "en", new Guid("78ee8b96-9c2c-2d3d-6ea1-51cb310d4c74"), "Adds people to the project and ticks their permissions." },
                    { new Guid("ef94a4f6-c87b-0c79-2d6e-e227016332d4"), "en", new Guid("15cc3735-49e9-7dec-4543-8f0e82cfaa0f"), "Use this supplier" },
                    { new Guid("efa82456-f11c-30e9-f1b9-40dfe26a452b"), "en", new Guid("9cd2fa12-f014-2e3f-62db-19295e9680ff"), "This payment goes beyond the contract's value. You can still save it." },
                    { new Guid("efbd0bc2-fa6c-4e87-6cfe-8878bc5284f3"), "ar", new Guid("47056225-e027-6aca-b0b6-2e8fa5e643f1"), "لا يوجد مستخدم بهذا الاسم." },
                    { new Guid("efc1753c-bf8a-5afa-acdb-e934fac17f9a"), "en", new Guid("a26612ce-85ea-9d8a-90c7-f499edd46573"), "Type" },
                    { new Guid("efc5d5da-391a-a123-fc6b-eeed7bc13c5e"), "ar", new Guid("f59f9d15-b1c8-d7e4-68b6-8ea7e5e2d198"), "رفض القيد" },
                    { new Guid("efd91d5a-c430-77c3-2092-fd37d7e34618"), "ar", new Guid("baa327d4-e13f-b01c-96b9-865b9e648a55"), "لا يوجد اتصال بالخادم. حاول مرة أخرى." },
                    { new Guid("efedce5b-f492-e2b1-7347-701aa375e793"), "en", new Guid("7d030930-f4ab-c522-fff4-96e5714f6b58"), "Amount" },
                    { new Guid("eff07d2b-a70d-ca41-d55f-c958e3438efa"), "en", new Guid("d456f83d-2a7c-2428-8b6e-ac98a52a64cb"), "Change password" },
                    { new Guid("f0027403-0a50-4a40-94c4-365c9024e797"), "ar", new Guid("819f545f-f676-205c-39b4-9f252160b826"), "تعذر تنفيذ ذلك." },
                    { new Guid("f031e97b-9ca7-bdbc-1037-a2330976af4f"), "ar", new Guid("15b6755e-2c36-9883-10d0-e1fee19e122e"), "هذا المشروع منتهٍ، فلا يتغير فيه شيء." },
                    { new Guid("f04af644-87a0-d63b-7d77-e34d0a797e5d"), "ar", new Guid("644677bf-1c82-8d84-5b33-44c6fd46816b"), "قيد جديد بهذا الملف" },
                    { new Guid("f07e3552-07f3-0f88-e991-31daabc44809"), "ar", new Guid("f9c223e7-01ed-0cf2-576e-688be1721c5c"), "اكتب رقم الجوال بالأرقام، حتى 20 رقماً، وقد يبدأ بعلامة +." },
                    { new Guid("f0927f44-a92f-8efd-8beb-77c7cf7b66ee"), "en", new Guid("2d69bcda-f315-408b-f10b-afcc14a1cdb7"), "No QR code could be read from this invoice. Fill the fields yourself." },
                    { new Guid("f09e94cd-299b-32b9-e872-f062bff0c962"), "ar", new Guid("da549b6c-99ef-56a4-0cae-1691856311f8"), "اسم المستخدم" },
                    { new Guid("f0add891-c0c5-230c-e66b-54b4ef4141fb"), "en", new Guid("7c9ece14-849a-bfa6-4cb9-f78e8bd3b113"), "Third" },
                    { new Guid("f20e7f66-2dc8-3714-1ca6-f8f5a058fb59"), "ar", new Guid("bd6513e2-844c-3edc-0c48-a774569236ea"), "تعذّرت قراءة التصنيفات. تحقق من الشبكة وحاول مرة أخرى." },
                    { new Guid("f2897876-7a7e-6d7c-37eb-ff9c14a73c31"), "ar", new Guid("de94612c-7a2b-ecec-f3db-2f5c5668a6fd"), "معطّل" },
                    { new Guid("f28a3021-8b54-711a-7701-de758bf8865f"), "ar", new Guid("9f06078f-e74f-a396-e2e7-0ba93ec7efb5"), "المقاول" },
                    { new Guid("f2ac9cf2-6b26-e5ee-fcf6-ee7754119317"), "en", new Guid("8cdea733-7cea-adc9-f0e4-da02f0e09246"), "Manage users" },
                    { new Guid("f2d919b0-d8d3-435b-810f-9ccf54e57363"), "ar", new Guid("077a7edf-b7d3-f51f-2d8f-06b19ca15ae0"), "لا يمكنك النسخ من ذلك المشروع." },
                    { new Guid("f2da35e5-3b03-99a2-c729-2d87804062db"), "en", new Guid("338c0fed-ebff-67bc-e0fa-5bd1e3b24e0c"), "Waiting for approval" },
                    { new Guid("f31b62b4-8c82-04e8-6785-1e45af6dceff"), "ar", new Guid("7b9780dd-416b-92de-d22c-a4ffe2898e0d"), "إلزامي عند الإدخال" },
                    { new Guid("f380c4e6-8af7-55c3-1b9e-9e667ea8b473"), "ar", new Guid("a2078bfe-1381-d297-c4a1-5766ddcf0629"), "هذه المجموعة الفرعية عليها قيود، فلا تُحذف." },
                    { new Guid("f3c40fde-2ce5-4b76-37d6-2d7e01d6249f"), "en", new Guid("e5f8719d-c49c-7a47-c852-a9a2e578f177"), "Sale" },
                    { new Guid("f3cf1efb-219e-5c7e-574c-528917603c06"), "ar", new Guid("3eb3a54d-a33e-4256-1fd3-19e28b6753f5"), "رقم" },
                    { new Guid("f401ad31-bcc4-056e-48a2-85618c6cc068"), "ar", new Guid("711490e3-5ccb-59ec-030e-26b08865b0a0"), "لا شيء بانتظار الاعتماد." },
                    { new Guid("f42cf6f5-f78b-73c9-2157-38d242581fd8"), "ar", new Guid("3d860c53-1e24-9e8d-7a73-d4c581ce7e16"), "تعذرت قراءة الإشعارات. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f46516ad-db2a-ed9c-eb91-a6bc9f81838f"), "en", new Guid("3ce67a2d-e572-f1d6-965d-195e1bf271fc"), "That is more than the account holds now." },
                    { new Guid("f4b91e3a-cc3d-e2a1-7873-47a7091658bc"), "en", new Guid("a1157602-7ea9-5700-6aa7-0366db512410"), "Save" },
                    { new Guid("f4f50a8e-8aa5-73fd-0c43-00772c96172c"), "en", new Guid("0dc9579b-f7dd-d811-e38d-4839b21a14a0"), "The invoices could not be read." },
                    { new Guid("f4fb997a-ef7b-2a82-55fb-a3989ba202d4"), "en", new Guid("3c09dceb-ebff-699f-f371-c74219300a75"), "Share" },
                    { new Guid("f52868e6-a9ec-1988-10ae-c30eb66d8392"), "ar", new Guid("bb817023-ef08-1dd2-4025-1f63199a3b89"), "هذا المشروع منتهٍ، ولا يمكن تغيير شيء فيه." },
                    { new Guid("f54525a2-3ac5-203a-e96f-df5ad53f8a99"), "ar", new Guid("9373c407-fce7-6561-e99a-48237e06394f"), "تعذرت قراءة القيود. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f5607183-c3cd-db73-1da7-240d88805f86"), "en", new Guid("bceb373f-bc7f-b5b5-1758-1b259c6e9e17"), "Not started" },
                    { new Guid("f56cf8ff-87f7-d8a4-a5f6-3ca02ec209fc"), "en", new Guid("972d0715-2cc6-7472-abcd-adb29f039c1e"), "Reset password" },
                    { new Guid("f59132b7-d118-05d5-e3fc-823af0df4506"), "en", new Guid("15b622c9-20e3-855c-21c4-24453272e534"), "An expense was paid with this method, so it is not deleted. Disable it instead." },
                    { new Guid("f5afafaa-3467-ef54-360e-a79c4e83155c"), "ar", new Guid("f70404cd-3040-ad17-8cc8-d1e06ac3acef"), "يصبح للقراءة فقط: لا إدخال ولا اعتماد، وتبقى تقاريره متاحة. لا يمكن التراجع عن ذلك." },
                    { new Guid("f613c221-b425-f4df-fda9-3ce24271f3e4"), "en", new Guid("f85c9b7f-5aab-9ad0-3214-cb379cf2ca5e"), "Item name" },
                    { new Guid("f632250b-caa1-c286-6db0-532735828b18"), "en", new Guid("9c653f8d-24cb-bfdb-e2c8-ea65f37ea8dd"), "The invoice's picture is not clear" },
                    { new Guid("f662c899-4abb-c968-a0b8-d4a1bf4254c2"), "en", new Guid("c8675013-b514-82b4-c81b-2a801ab38051"), "This project is finished, so it cannot change." },
                    { new Guid("f6983ba5-47fd-5dfc-eaad-bb8f51b06be7"), "en", new Guid("75a3a9fb-3e2f-2886-82f8-f321ab100de4"), "Save" },
                    { new Guid("f6a9e449-4bfe-3b41-11e9-82896d45db61"), "ar", new Guid("8a1b4169-beca-c13f-9dd5-fb20b67be600"), "اكتب 2000 حرف على الأكثر." },
                    { new Guid("f6bef1aa-f1f7-fc19-c85f-8ecb287c9215"), "en", new Guid("1d57001f-4aaf-4128-21b3-31912acb68eb"), "Update" },
                    { new Guid("f6d020f1-7439-a0f5-c5c3-2134260f96e1"), "en", new Guid("1e2aa6c1-4b5a-cad5-eaec-4d372a0931bf"), "The current password is wrong." },
                    { new Guid("f6f0000d-cdaf-e0c3-ecb0-1fcca7ef1aff"), "en", new Guid("286ab6fb-1ba7-10e5-ff3d-51ee5270f71a"), "Choose a new password of at least 8 characters before you start." },
                    { new Guid("f70934cc-417e-17e4-ddac-84742eb16a2d"), "ar", new Guid("3a34eb7f-69cb-b5d4-8a31-7e5677ef96f0"), "أدخل اسماً من 1 إلى 100 حرف." },
                    { new Guid("f7269c82-69c8-c0d5-a7d9-4b53d9147aab"), "en", new Guid("8b35c979-312d-449d-541a-49971dd1d280"), "Partner" },
                    { new Guid("f7295045-a04d-27e3-f29f-d4d4d2b6de83"), "en", new Guid("89c0a3f9-aed9-9c32-6a3f-b49d12b1f0b8"), "Date" },
                    { new Guid("f73af20e-80ba-4eca-7421-89eeb1918f6d"), "en", new Guid("37d49fba-1eda-de57-c176-21f08074b25e"), "Add a document" },
                    { new Guid("f794e624-9cf8-c21a-b718-7aac4f727c68"), "ar", new Guid("775c8654-e05d-95d2-4473-3d29e9f0c8cb"), "أعد كتابة كلمة المرور الجديدة" },
                    { new Guid("f7b7479b-e8b5-c733-2a06-1b4a1c8250b9"), "en", new Guid("51ff4955-892c-26c8-462a-c9c821be7286"), "Payment methods" },
                    { new Guid("f7dae556-d44e-0abe-6a1e-27968a00713d"), "en", new Guid("892cc3c4-3e05-39a4-a409-1eb3d58f3aad"), "This waits for Finance's decision, so it cannot change now." },
                    { new Guid("f7e5f447-88c0-ee39-482d-6a12bd87e639"), "en", new Guid("09141e0c-bf8e-99ff-b321-85b1838a6c59"), "Finished" },
                    { new Guid("f8288b56-1c40-60c8-d18e-b0fdc84783b4"), "ar", new Guid("2cd9691e-fe19-6384-2745-9b1c0f782d71"), "تعذرت قراءة ما ينتظر. تحقق من الاتصال وحاول مجدداً." },
                    { new Guid("f8382ac4-1a11-7d3e-04e5-9ce9f7fb051d"), "en", new Guid("b8975c30-11f0-1191-b8d4-b9af47e27481"), "Entries" },
                    { new Guid("f845e027-dfd4-22b0-b7c6-d43587f7d7e4"), "en", new Guid("a123bfd7-b572-5e1e-b32a-7bff2dc87be8"), "They leave Partners and Add entry; their movements stay in the books and reports." },
                    { new Guid("f88687a8-b936-fede-3159-9db15e0eeabe"), "en", new Guid("bfdad1c5-52c9-a786-2af4-af7f283bcde7"), "This entry is no longer here." },
                    { new Guid("f8b063d7-3f00-126f-7aef-cfc899cc7dc6"), "ar", new Guid("923e65bb-3973-1536-0da6-6fed77ed393f"), "قرر أحدهم هذا القيد من قبل." },
                    { new Guid("f8b97354-eefe-e959-bfc6-f51027311cbc"), "en", new Guid("6f01fbfa-c3b7-1958-9efc-bfa66134d00d"), "No columns." },
                    { new Guid("f8ba640a-51f5-1d85-a4ac-5a2f52d2573c"), "en", new Guid("00c3c7b8-a2dc-1c17-99a1-42de7ca3b988"), "Could not get the file. Check the connection and try again." },
                    { new Guid("f8bb37f4-3968-5312-e1aa-58d9f1a9e1e1"), "ar", new Guid("7b2a2fb8-d655-5111-c5a4-6f541ea5182d"), "إيرادات ورأس مال بانتظار الاعتماد" },
                    { new Guid("f8c98edc-6929-0fee-95b7-20cd2b6150cd"), "en", new Guid("f405963d-ceb0-a769-42e6-e2ee6231ffa7"), "Withdrawal" },
                    { new Guid("f8eb3403-bc93-f6ba-efe3-9c482b3f8710"), "en", new Guid("e289acbb-0b00-2518-4dc5-67b315187f91"), "Cash custodies" },
                    { new Guid("f95be2b8-d4f7-93db-30bd-00dd37dd4fd5"), "ar", new Guid("0d1b5c7b-a32c-f224-592c-a1079f821392"), "دفعات مقاولين" },
                    { new Guid("f9c7207b-d1a7-015e-f3b5-ba217e465eb3"), "ar", new Guid("766b12c7-1e9f-841b-9c80-bdc5136e83be"), "نوع الحركة" },
                    { new Guid("f9e5116e-d37e-cff4-e334-dc93bf7e9214"), "en", new Guid("ecbc1890-c919-c9c3-c4ec-2b02e170d000"), "Add this supplier" },
                    { new Guid("f9f2a38b-5773-971a-573f-976c841d794d"), "ar", new Guid("f569d1f9-3602-9e2c-c559-97d7b73686f9"), "رُفض هذا التغيير." },
                    { new Guid("f9fc0c3d-5718-1b19-c2f8-95f42a77bf6e"), "ar", new Guid("e3c05b9b-d3be-b086-7c74-842e7831c547"), "المستندات" },
                    { new Guid("f9feee8a-40ca-c720-467e-dc4895a3610a"), "en", new Guid("d3248620-efc3-08c6-4a0d-759eb701b08b"), "The camera could not open. Pick the invoice from your files." },
                    { new Guid("fa040b2d-e38d-f90e-557c-9d4311521cd5"), "ar", new Guid("95e494bc-2749-eeea-deb4-b63fbe408416"), "طريقة الدفع" },
                    { new Guid("fa0c92da-a754-b892-d9fe-3c2942d9f0c2"), "en", new Guid("e1863268-b732-f67b-b3a2-0e162b2e08a3"), "Its name" },
                    { new Guid("fa50bd92-9f48-51ed-2903-db0574483b1a"), "ar", new Guid("c3ac744a-7b2e-b85f-c633-dbd564b74fe0"), "يغيّره من أدخله أو المدير فقط." },
                    { new Guid("fa6b554a-91eb-31c5-6c93-9d6f1b9d273c"), "ar", new Guid("a7863f42-dee1-5ffc-ca2c-d1704003e515"), "لا يُحذف الشريك إلا ورأس ماله صفر ولا ينتظر له قيد." },
                    { new Guid("faae8e7c-b2bd-8cc7-487b-9060311107eb"), "ar", new Guid("444dd357-7cc1-e78f-a448-0c16a2617477"), "مصروفات التقرير الحالي" },
                    { new Guid("fac0b0e6-4155-8db6-6c9e-05de1e5a335f"), "ar", new Guid("089564f3-2b91-b254-3745-8116aeef59dc"), "اجمالي الكل" },
                    { new Guid("fae7bcdb-f6d4-c5da-8b34-12361e353a10"), "ar", new Guid("aa3cad16-a1ce-3a2d-3390-e51b9947de81"), "مسوداتك تذكره، وحذفها يحتاج صلاحية حذف القيود." },
                    { new Guid("faf4fd23-3926-23c0-26a5-878a4a23517d"), "ar", new Guid("94259fa2-84ec-4d8b-52b7-b833834562b5"), "فرق المطابقة" },
                    { new Guid("fafe640b-8a07-33d9-6e53-65ef4636e24f"), "ar", new Guid("11e87fa3-420a-9c71-d439-1bed904ec773"), "تغيّرت القائمة. حاول مرة أخرى." },
                    { new Guid("fb08cc33-c75d-3522-1170-450e5c0f761b"), "ar", new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "مفتوح القيمة" },
                    { new Guid("fb393a9c-4886-a9f9-0e2b-69034ea2fe20"), "ar", new Guid("b4cad059-85c6-0d7f-2358-96816b55f8bf"), "عام" },
                    { new Guid("fb3a7b61-8742-26eb-f8da-436cb72b4de6"), "en", new Guid("0f5fb0bc-426d-b1d1-e736-373393389502"), "Surplus" },
                    { new Guid("fb3f132b-2ca9-03de-c872-bc5acf3909da"), "ar", new Guid("68500f85-3483-5951-a047-618c73bc9477"), "المستندات" },
                    { new Guid("fb73e8c3-b65d-7031-bf76-909bd16e8128"), "en", new Guid("49a703b2-e054-2289-29a7-12dffaa4ad19"), "Say why it is rejected." },
                    { new Guid("fb9de578-4f34-3b76-9072-6d50ea72aff1"), "en", new Guid("ed0aa89b-85e9-5685-1d92-b4b3ca316ffa"), "No default item" },
                    { new Guid("fbb8f8aa-105f-b9ef-68ea-2a3ae2eceb64"), "en", new Guid("7460cc1f-4e0d-4708-0957-713ff98ffd93"), "Delete entries" },
                    { new Guid("fbdb6b97-7753-071c-aa8c-13a058488d70"), "ar", new Guid("06effe02-bdca-7e81-7f0d-7b4aa5ea5636"), "ما هو المشروع، لكل من فيه. حتى 2000 حرف." },
                    { new Guid("fc777906-eb20-1309-796f-44d54fe66d48"), "ar", new Guid("53945eef-ca15-df6a-f8d8-901efa608b7c"), "تنزيل" },
                    { new Guid("fca2ccc2-20cd-c741-f93b-a15a1b4d3494"), "en", new Guid("12930e47-bc50-4e4f-477b-67b863e6317c"), "The file could not be kept." },
                    { new Guid("fccd9a35-73ad-f8e1-3f0a-d8f82b98dc77"), "ar", new Guid("82cacafc-af76-9c7d-1135-b4758edfb673"), "اعتماد القيود" },
                    { new Guid("fcf4ee5c-1885-92d8-9b2e-b9a19486d0ef"), "en", new Guid("f06db356-d48c-9833-6ce4-84fd6216c6f4"), "Save" },
                    { new Guid("fd15300e-1d04-c7dc-6b3b-d47608e75824"), "en", new Guid("79ff7e2f-527b-779c-ce47-95604cfa8be5"), "Open value" },
                    { new Guid("fd5de387-cdb7-ed3c-8e81-910210d4c221"), "en", new Guid("4ef536a5-a7d5-313a-1c15-1bf7d854c58b"), "A new supplier is approved with its first expense." },
                    { new Guid("fd72c743-bed0-ba53-dea8-fd727c7907c0"), "en", new Guid("33d64ff3-e7e3-f272-c07d-a0b66e6a99f3"), "Rename" },
                    { new Guid("fd846041-794a-7018-17db-e9e25479bb90"), "ar", new Guid("c4bb1636-edb6-9df1-c124-c12bd6930eec"), "حفظ الحركة" },
                    { new Guid("fe14f56a-6cba-8192-5772-1db987c46607"), "ar", new Guid("49a34959-b87d-1860-f555-25771b6897e8"), "الاسم كان {0}" },
                    { new Guid("fe638376-2f4f-1dbe-b53a-7cca9772baef"), "ar", new Guid("a57b43a9-63f7-b93a-6e84-5c0b6bd470a4"), "اختر المشروع" },
                    { new Guid("ff92b910-f0e2-53b5-4f8e-04afde6cb610"), "en", new Guid("ba32de37-bebe-08b9-be33-35e0aa46fedd"), "Reconciliation difference" },
                    { new Guid("ffb372f1-15eb-cc43-6e5d-4b9eb0400e8f"), "en", new Guid("1f837e7c-3a33-e378-f943-d6478ef27825"), "Your entry was approved" },
                    { new Guid("ffd38969-1890-8b22-fd24-8a302ffdc79d"), "en", new Guid("3a54e458-808b-2f23-1ddc-03727b3ef47d"), "Quick reason" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "الإشراف", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 4 },
                    { new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"), true, "إيرادات أخرى", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 7 },
                    { new Guid("7d56b915-89c1-0e21-4392-a31e37428c69"), true, "رأس المال", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 5 },
                    { new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "التشطيب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 3 },
                    { new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "الرسوم والفواتير", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 1 },
                    { new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "العظم", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 2 },
                    { new Guid("c4aece2a-cb9d-fb1d-ae30-dce8baba5959"), true, "تكلفة الأراضي", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 0 },
                    { new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"), true, "استرجاع ضرائب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 6 },
                    { new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"), true, "فلوس البيع المودعة بالحساب", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 8 }
                });

            migrationBuilder.InsertData(
                table: "DefaultColumns",
                columns: new[] { "Id", "DataType", "IsCore", "IsRequired", "ItemType", "Key", "Name", "Options", "ProjectId", "ShowInReports", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("014f3b89-4ee5-664d-a59d-57f5b98c0c44"), (byte)3, true, true, (byte)0, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("01785f24-f721-2d01-7d39-2ffaf783f7d6"), (byte)4, false, false, (byte)2, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 6 },
                    { new Guid("0a297057-af7f-843e-b01a-4ed653b4ad06"), (byte)1, false, false, (byte)0, "quantity", "العدد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 4 },
                    { new Guid("0c55e229-1af8-f7ce-cbcc-f4802fa64c24"), (byte)4, false, false, (byte)0, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 6 },
                    { new Guid("10ce12a7-5e1a-20d4-27ba-5240c2058d59"), (byte)4, true, false, (byte)1, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("15c86a05-a062-0cea-7b42-d0634822d6a8"), (byte)2, true, true, (byte)0, "total", "الإجمالي", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 5 },
                    { new Guid("2ca7654d-4371-6db6-14ff-a95d9b0aa972"), (byte)2, false, false, (byte)2, "unitPrice", "سعر الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("36ccb44c-d5fd-8b0a-7579-d111f91f3d58"), (byte)2, true, false, (byte)2, "amount", "المبلغ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 11 },
                    { new Guid("36f2f763-7e4b-fb79-7667-75297e859eed"), (byte)4, true, false, (byte)1, "outsideContract", "داخل أو خارج العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 5 },
                    { new Guid("3a69be0b-a697-4a48-2dc0-c70a043a7389"), (byte)0, false, false, (byte)0, "product", "الصنف", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("45cb94d7-73dd-02ac-f457-b5cedc3a5767"), (byte)1, false, false, (byte)2, "quantity", "العدد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 4 },
                    { new Guid("47874291-b290-c9e7-623d-b0d47f539b63"), (byte)0, false, false, (byte)1, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 6 },
                    { new Guid("4c9d7893-60b7-58aa-d675-486e13466312"), (byte)0, false, false, (byte)2, "product", "الصنف", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("53a2404d-b058-aba6-c6a2-0a33b171f266"), (byte)4, true, false, (byte)2, "outsideContract", "داخل أو خارج العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 12 },
                    { new Guid("5c13ae77-437a-7db1-6d68-cce6207da136"), (byte)4, true, true, (byte)0, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("5df1c111-116a-349c-277d-1a5adab41cc2"), (byte)4, true, true, (byte)1, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 4 },
                    { new Guid("6745ee14-a1b1-7ed0-65f8-52d895ed5246"), (byte)5, true, false, (byte)0, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("6b16b02b-d1e5-2bc0-3ea7-fc18fa2125d4"), (byte)4, true, true, (byte)1, "contract", "العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("843d08f0-0f41-4c0b-4c93-179c0bfae455"), (byte)4, true, true, (byte)2, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 },
                    { new Guid("8480dce1-394d-8cd2-8263-851dd298f4cc"), (byte)3, true, true, (byte)2, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("888dd438-d07c-b425-2ff4-f9cbaa0087f1"), (byte)0, false, false, (byte)2, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("8ce8e464-22b7-f5e1-bbe3-1758e4d3fffb"), (byte)5, true, false, (byte)1, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("9ff01dd6-d582-ca82-a2fa-72d632064391"), (byte)4, true, true, (byte)2, "account", "الحساب", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 7 },
                    { new Guid("a6b762c9-8621-22ef-b836-7070fccc1896"), (byte)2, true, true, (byte)1, "amount", "المبلغ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 2 },
                    { new Guid("b07fdd34-db99-3860-7f42-6a006a167443"), (byte)5, true, false, (byte)2, "attachment", "المرفق", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 8 },
                    { new Guid("dbe3eb09-3c58-cd1d-77ac-2f781d7e20d2"), (byte)4, false, false, (byte)1, "paymentMethod", "طريقة الدفع", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 3 },
                    { new Guid("eb622f9e-4def-d5db-934f-3150a3949aab"), (byte)2, true, true, (byte)2, "total", "الإجمالي", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 5 },
                    { new Guid("f01dd745-c15b-5292-bc4b-fa3500db903c"), (byte)0, false, false, (byte)0, "notes", "ملاحظات", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 9 },
                    { new Guid("f8e8fd13-8804-05b5-08ce-8e6e114e7fcf"), (byte)3, true, true, (byte)1, "date", "التاريخ", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 0 },
                    { new Guid("f9801152-23d0-a6f6-5569-b0fc4529dbc3"), (byte)4, true, false, (byte)2, "contract", "العقد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 10 },
                    { new Guid("faf254c8-b5ff-65a6-3b15-631369bb7c8a"), (byte)2, false, false, (byte)0, "unitPrice", "سعر الوحدة", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), true, 3 },
                    { new Guid("fe89eb78-ba76-bb13-6610-da78585c3f72"), (byte)4, true, true, (byte)0, "supplier", "المورد", "[]", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), false, 1 }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "IsActive", "Name", "ProjectId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("9f6a8063-8e6d-345c-f226-9f353471286b"), true, "شبكة", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 0 },
                    { new Guid("f67ccb16-dde0-e041-046c-bf08645cb38a"), true, "كاش", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 2 },
                    { new Guid("f9f1ae53-17fc-c6ec-f24e-d4b87c1f4db7"), true, "تحويل", new Guid("f11ea9ed-0623-ebf7-abff-3f4e8714248a"), 1 }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "ShowsTablePercentages", "SortOrder", "Type" },
                values: new object[,]
                {
                    { new Guid("04af0eb6-f27d-1f55-1999-a027e57d9ccb"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "ردود الماء", false, 7, (byte)2 },
                    { new Guid("04c58bf8-0504-85a6-4245-d2d42aff4322"), new Guid("46fec1e9-1837-3227-7ce4-a9a8b537e535"), true, "إيرادات أخرى", false, 0, (byte)5 },
                    { new Guid("04f4b5bd-08cb-fe77-fb77-b0dc67a41e94"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الحفر", false, 0, (byte)2 },
                    { new Guid("0cf9ec40-d505-1642-076c-34362aacc748"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "درابزين", false, 8, (byte)2 },
                    { new Guid("12e6ecda-c096-25f0-782e-4dbc5b1b4de3"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الحديد", false, 1, (byte)0 },
                    { new Guid("180b728a-47d2-5142-5427-5033a619ea95"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "التأمين", false, 3, (byte)2 },
                    { new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"), new Guid("c4aece2a-cb9d-fb1d-ae30-dce8baba5959"), true, "تكلفة الأرض", false, 0, (byte)2 },
                    { new Guid("21280dd7-483e-17c2-640f-03bc3cdb8988"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "حجر الواجهة", false, 9, (byte)2 },
                    { new Guid("2614447c-52a2-553a-15d8-0011ddfafd1c"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "أبواب خارجية", false, 2, (byte)2 },
                    { new Guid("286990e1-6c68-33cf-b0b3-78059b23b697"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الكهرباء", false, 5, (byte)2 },
                    { new Guid("2ac089f0-6d06-7441-4b4e-874fe1543f43"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "لياسة", false, 3, (byte)2 },
                    { new Guid("2e248f9f-9064-45a0-334f-c53e38d678fb"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "معدات تشطيب", false, 12, (byte)2 },
                    { new Guid("314438d3-36aa-8eaa-328a-cc9beb5767b6"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "دفعات العظم", false, 4, (byte)1 },
                    { new Guid("6657f024-a507-a98a-da2d-7a4f4c083f38"), new Guid("fbb8a0e1-8927-75c6-abb2-23125da9d4d4"), true, "البيع", false, 0, (byte)3 },
                    { new Guid("673de086-8cee-5596-1b93-acb0acfef09f"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "بويه", false, 6, (byte)2 },
                    { new Guid("6ad84f04-d71e-3e7f-8479-cedd8b6f49d9"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "البناء", false, 3, (byte)2 },
                    { new Guid("6da2f643-7b61-6067-d461-5397f6c43410"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المكتب المشرف", false, 2, (byte)2 },
                    { new Guid("6e86db2d-d5ec-b6fa-53cc-7cdde0c14b44"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "العوازل", false, 11, (byte)2 },
                    { new Guid("6f303ab6-00bc-ae69-5f2a-8331b61bc6f0"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المحاسب", false, 4, (byte)2 },
                    { new Guid("748258b8-99e2-f7a3-a9b2-06052a9b95f7"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "أخرى", false, 5, (byte)2 },
                    { new Guid("7b8220e1-c3e9-06c2-3615-53b8032e7262"), new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "إشراف ميداني", false, 1, (byte)2 },
                    { new Guid("8435b564-68b8-4b1e-5176-750bb0b72b3d"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "السباكة", false, 0, (byte)2 },
                    { new Guid("84820fed-9506-6e9d-9308-53c9841a97e9"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "جبس", false, 4, (byte)2 },
                    { new Guid("9c89740e-854b-393e-45d5-e649ad5de00c"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "رسوم الأمانة", false, 1, (byte)2 },
                    { new Guid("9fca4a36-6136-161d-07bb-cea835e1d6f7"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "أغراض الموقع", false, 8, (byte)2 },
                    { new Guid("ae991b59-acb1-7420-d09b-e6895207aac0"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "أبواب داخلية", false, 10, (byte)2 },
                    { new Guid("aebccc22-25aa-9838-9062-e867eccb8b8e"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "الكهرباء", false, 1, (byte)2 },
                    { new Guid("c6b680bb-5976-3505-4910-573ac8ec190c"), new Guid("f70f19e6-8d6d-4965-8b00-3e6552bb95a9"), true, "استرجاع الضرائب", false, 0, (byte)4 },
                    { new Guid("cf854e63-cad5-1c83-90a2-c6164c86b6c4"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "ألمنيوم", false, 7, (byte)2 },
                    { new Guid("db415f43-3b91-e7b2-4041-3330e8c49b76"), new Guid("b0d24870-bb67-9c79-b0bd-ec7fef769cde"), true, "المكتب الهندسي", false, 0, (byte)2 },
                    { new Guid("dcfd5629-5844-9427-bb79-53273463077a"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "السباكة", false, 6, (byte)2 },
                    { new Guid("eb1a681e-7168-48f9-5fc5-c1d1351d58db"), new Guid("122410be-d98a-5b8c-6cac-4843b313db40"), true, "إشراف مالي", false, 0, (byte)2 },
                    { new Guid("f0085a42-9276-ee42-6602-5b1e32f49e10"), new Guid("b66bf15d-0e06-7bae-0877-bdf852e93dad"), true, "الخرسانة", false, 2, (byte)0 },
                    { new Guid("f2d90dd8-4d88-c252-27ca-0160324d3cd4"), new Guid("7d56b915-89c1-0e21-4392-a31e37428c69"), true, "رأس المال", true, 0, (byte)6 },
                    { new Guid("f55f1253-ff90-54ed-3d8a-833d0f5a06e1"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "رواتب الحارس", false, 13, (byte)2 },
                    { new Guid("f64be08a-09b1-c732-261f-c4bb8f2d37f5"), new Guid("a3ccf054-ca23-f401-5b42-d8522b8cef2d"), true, "بلاط", false, 5, (byte)2 }
                });

            migrationBuilder.InsertData(
                table: "Subgroups",
                columns: new[] { "Id", "ItemId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("000586db-98e1-becf-6b50-01778c91e7b3"), new Guid("aebccc22-25aa-9838-9062-e867eccb8b8e"), "", 0 },
                    { new Guid("03a246b8-d8fe-78f2-4617-4eaa516e05c7"), new Guid("c6b680bb-5976-3505-4910-573ac8ec190c"), "", 0 },
                    { new Guid("2c0e710e-bd86-15fd-1ad8-0573a254ca24"), new Guid("dcfd5629-5844-9427-bb79-53273463077a"), "", 0 },
                    { new Guid("303c5901-677c-31f2-da36-31fe95e53b63"), new Guid("f55f1253-ff90-54ed-3d8a-833d0f5a06e1"), "", 0 },
                    { new Guid("324cafe0-f959-ca2f-3b19-c167197ebad6"), new Guid("84820fed-9506-6e9d-9308-53c9841a97e9"), "", 0 },
                    { new Guid("32e345ff-943f-de0d-89b5-cc978b79b33f"), new Guid("180b728a-47d2-5142-5427-5033a619ea95"), "", 0 },
                    { new Guid("40e0cf07-7729-cd68-160f-a568dd140b39"), new Guid("6e86db2d-d5ec-b6fa-53cc-7cdde0c14b44"), "", 0 },
                    { new Guid("4f8f065e-ed62-4984-2820-ab3cca39c455"), new Guid("04af0eb6-f27d-1f55-1999-a027e57d9ccb"), "", 0 },
                    { new Guid("53a7bf92-62ce-d80a-b5fb-abcbffa9b60c"), new Guid("314438d3-36aa-8eaa-328a-cc9beb5767b6"), "", 0 },
                    { new Guid("5d5d6e60-1047-0933-b1f2-3633529247ca"), new Guid("6ad84f04-d71e-3e7f-8479-cedd8b6f49d9"), "", 0 },
                    { new Guid("5e79f6ac-ca2f-dc3c-ccb6-8a27761bf6f4"), new Guid("6f303ab6-00bc-ae69-5f2a-8331b61bc6f0"), "", 0 },
                    { new Guid("61c56bb2-138f-5f41-9383-448b817bf4e5"), new Guid("21280dd7-483e-17c2-640f-03bc3cdb8988"), "", 0 },
                    { new Guid("70863205-936d-e8bd-e9ae-4379f5816f36"), new Guid("6da2f643-7b61-6067-d461-5397f6c43410"), "", 0 },
                    { new Guid("75d39cb5-7c79-4382-963b-8d59e93672c4"), new Guid("12e6ecda-c096-25f0-782e-4dbc5b1b4de3"), "", 0 },
                    { new Guid("7860a42d-3edf-8147-730c-265d34ccc642"), new Guid("db415f43-3b91-e7b2-4041-3330e8c49b76"), "", 0 },
                    { new Guid("7f1e80a0-63f0-0351-5262-2a5a02392d3a"), new Guid("f2d90dd8-4d88-c252-27ca-0160324d3cd4"), "", 0 },
                    { new Guid("897a23be-99c3-c327-63f8-b9811ac20ce4"), new Guid("eb1a681e-7168-48f9-5fc5-c1d1351d58db"), "", 0 },
                    { new Guid("8a97233b-3293-cc71-872d-7c709e093b93"), new Guid("0cf9ec40-d505-1642-076c-34362aacc748"), "", 0 },
                    { new Guid("91632167-ef32-8dff-d059-6eaac4554047"), new Guid("748258b8-99e2-f7a3-a9b2-06052a9b95f7"), "", 0 },
                    { new Guid("aec8376e-4f0b-3a1e-ebfa-01c56fbc6676"), new Guid("6657f024-a507-a98a-da2d-7a4f4c083f38"), "", 0 },
                    { new Guid("b2837e7b-6a2e-7e75-9d10-b27c6a1bc4ba"), new Guid("9fca4a36-6136-161d-07bb-cea835e1d6f7"), "", 0 },
                    { new Guid("b48fe98b-9432-34ab-eab0-45773afbae03"), new Guid("286990e1-6c68-33cf-b0b3-78059b23b697"), "", 0 },
                    { new Guid("b54806d8-db61-13c8-e192-2ac11678df46"), new Guid("673de086-8cee-5596-1b93-acb0acfef09f"), "", 0 },
                    { new Guid("b753077c-a5b1-ec2d-a6ec-0a71153ab451"), new Guid("f0085a42-9276-ee42-6602-5b1e32f49e10"), "", 0 },
                    { new Guid("baa2771a-7861-a2ed-dc09-02e30607f220"), new Guid("04c58bf8-0504-85a6-4245-d2d42aff4322"), "", 0 },
                    { new Guid("bcf76c0a-8f53-704f-b0b1-b222df5dcfa7"), new Guid("f64be08a-09b1-c732-261f-c4bb8f2d37f5"), "", 0 },
                    { new Guid("c2db4d48-a4a5-e23f-d04c-09564fe6b494"), new Guid("1b5d6c03-3b9b-a505-ec3e-e164c29fb9cc"), "", 0 },
                    { new Guid("cabb8175-0a11-9c2d-be34-ff846246e9f5"), new Guid("2ac089f0-6d06-7441-4b4e-874fe1543f43"), "", 0 },
                    { new Guid("d9f40360-24f3-9803-6883-fa7c04572073"), new Guid("ae991b59-acb1-7420-d09b-e6895207aac0"), "", 0 },
                    { new Guid("ddbd3ea0-92f3-ead5-0baa-4720989b526d"), new Guid("2e248f9f-9064-45a0-334f-c53e38d678fb"), "", 0 },
                    { new Guid("e646dc59-f645-5878-3949-e406f4d0e555"), new Guid("cf854e63-cad5-1c83-90a2-c6164c86b6c4"), "", 0 },
                    { new Guid("e897f26e-7fd9-5b34-30a9-8ca97fa411a9"), new Guid("8435b564-68b8-4b1e-5176-750bb0b72b3d"), "", 0 },
                    { new Guid("f228ea29-3d05-6e30-f10e-92c94cf4dec4"), new Guid("9c89740e-854b-393e-45d5-e649ad5de00c"), "", 0 },
                    { new Guid("f5d0be5f-23b0-db35-9da9-7a158a70a153"), new Guid("7b8220e1-c3e9-06c2-3615-53b8032e7262"), "", 0 },
                    { new Guid("f6a3834a-8a64-0781-c69b-13d67503c10e"), new Guid("04f4b5bd-08cb-fe77-fb77-b0dc67a41e94"), "", 0 },
                    { new Guid("fc247740-3368-0e14-c6ff-255d25cd1b01"), new Guid("2614447c-52a2-553a-15d8-0011ddfafd1c"), "", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountCustodians_UserId",
                table: "AccountCustodians",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProjectId",
                table: "Accounts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type",
                table: "Accounts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ProjectId_Sha256",
                table: "Attachments",
                columns: new[] { "ProjectId", "Sha256" });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedByUserId",
                table: "Attachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CapitalMovementDetails_PartnerId",
                table: "CapitalMovementDetails",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CapitalMovementDetails_Type",
                table: "CapitalMovementDetails",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ProjectId",
                table: "Categories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_PartnerId",
                table: "ChosenPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_ProjectId_UserId_PartnerId",
                table: "ChosenPartners",
                columns: new[] { "ProjectId", "UserId", "PartnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChosenPartners_UserId",
                table: "ChosenPartners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigValues_Key",
                table: "ConfigValues",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_AttachmentId",
                table: "ContractAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPaymentDetails_ContractId",
                table: "ContractorPaymentDetails",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ItemId",
                table: "Contracts",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId",
                table: "Contracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SupplierId",
                table: "Contracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_DataType",
                table: "DefaultColumns",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_ItemType",
                table: "DefaultColumns",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_ProjectId_ItemType_Key",
                table: "DefaultColumns",
                columns: new[] { "ProjectId", "ItemType", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_RefreshTokenHash",
                table: "Devices",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AttachmentId",
                table: "Documents",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProjectId_AttachmentId",
                table: "Documents",
                columns: new[] { "ProjectId", "AttachmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_AccountId",
                table: "Entries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CreatedByUserId",
                table: "Entries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Kind",
                table: "Entries",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ProjectId_Status",
                table: "Entries",
                columns: new[] { "ProjectId", "Status" })
                .Annotation("SqlServer:Include", new[] { "Kind", "Amount", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Status",
                table: "Entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttachments_AttachmentId",
                table: "EntryAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttachments_Role",
                table: "EntryAttachments",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_Action",
                table: "EntryDecisions",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_EntryId",
                table: "EntryDecisions",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_UserId",
                table: "EntryDecisions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_ItemId",
                table: "ExpenseDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_PaymentMethodId",
                table: "ExpenseDetails",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_SubgroupId",
                table: "ExpenseDetails",
                column: "SubgroupId");

            migrationBuilder.CreateIndex(
                name: "IX_History_Action",
                table: "History",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_History_ProjectId_Id",
                table: "History",
                columns: new[] { "ProjectId", "Id" })
                .Annotation("SqlServer:Include", new[] { "At", "UserId", "Action", "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_History_UserId",
                table: "History",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_ItemColumns_DataType",
                table: "ItemColumns",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_ItemColumns_ItemId_Key",
                table: "ItemColumns",
                columns: new[] { "ItemId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CategoryId",
                table: "Items",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Kind",
                table: "Notifications",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_At",
                table: "Notifications",
                columns: new[] { "UserId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ProjectId_UserId",
                table: "Partners",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] <> '00000000-0000-0000-0000-000000000000' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ProjectId",
                table: "PaymentMethods",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicReports_IssuedByUserId",
                table: "PeriodicReports",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicReports_ProjectId_Number",
                table: "PeriodicReports",
                columns: new[] { "ProjectId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissions_ProjectId_UserId_Permission",
                table: "ProjectPermissions",
                columns: new[] { "ProjectId", "UserId", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissions_UserId",
                table: "ProjectPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoleNames_ProjectId_UserId",
                table: "ProjectRoleNames",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoleNames_UserId",
                table: "ProjectRoleNames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_SupplierId",
                table: "PurchaseDetails",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationBalance_AccountId",
                table: "ReconciliationBalance",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_ProjectId_At",
                table: "Reconciliations",
                columns: new[] { "ProjectId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Reconciliations_UserId",
                table: "Reconciliations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDetails_ItemId",
                table: "RevenueDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDetails_SubgroupId",
                table: "RevenueDetails",
                column: "SubgroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Subgroups_ItemId",
                table: "Subgroups",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDefaultItems_ItemId",
                table: "SupplierDefaultItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedByUserId",
                table: "Suppliers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ProjectId_Name",
                table: "Suppliers",
                columns: new[] { "ProjectId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ProjectId_VatNumber",
                table: "Suppliers",
                columns: new[] { "ProjectId", "VatNumber" },
                unique: true,
                filter: "[VatNumber] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Status",
                table: "Suppliers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_UserId_Permission",
                table: "SystemPermissions",
                columns: new[] { "UserId", "Permission" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_TranslationKeys_Key",
                table: "TranslationKeys",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TranslationKeyId_Language",
                table: "Translations",
                columns: new[] { "TranslationKeyId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_ProjectId",
                table: "Units",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultProjects_ProjectId",
                table: "UserDefaultProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDefaultProjects_UserId",
                table: "UserDefaultProjects",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.Sql("""
                CREATE TRIGGER HistoryIsAppendOnly ON History INSTEAD OF UPDATE, DELETE AS
                THROW 50021, 'The History table is append-only: its rows are never changed or deleted (AC-21.1).', 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER HistoryIsAppendOnly;");

            migrationBuilder.DropTable(
                name: "AccountCustodians");

            migrationBuilder.DropTable(
                name: "CapitalMovementDetails");

            migrationBuilder.DropTable(
                name: "ChosenPartners");

            migrationBuilder.DropTable(
                name: "ConfigValues");

            migrationBuilder.DropTable(
                name: "ContractAttachments");

            migrationBuilder.DropTable(
                name: "ContractorPaymentDetails");

            migrationBuilder.DropTable(
                name: "DefaultColumns");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "EntryAttachments");

            migrationBuilder.DropTable(
                name: "EntryDecisions");

            migrationBuilder.DropTable(
                name: "ExpenseDetails");

            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ItemColumns");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PeriodicReports");

            migrationBuilder.DropTable(
                name: "ProjectPermissions");

            migrationBuilder.DropTable(
                name: "ProjectRoleNames");

            migrationBuilder.DropTable(
                name: "PurchaseDetails");

            migrationBuilder.DropTable(
                name: "ReconciliationBalance");

            migrationBuilder.DropTable(
                name: "RevenueDetails");

            migrationBuilder.DropTable(
                name: "SupplierDefaultItems");

            migrationBuilder.DropTable(
                name: "SystemPermissions");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "UserDefaultProjects");

            migrationBuilder.DropTable(
                name: "LK_CapitalMovementType");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "LK_EntryAttachmentRole");

            migrationBuilder.DropTable(
                name: "LK_EntryDecisionAction");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "LK_HistoryAction");

            migrationBuilder.DropTable(
                name: "LK_ColumnDataType");

            migrationBuilder.DropTable(
                name: "LK_NotificationKind");

            migrationBuilder.DropTable(
                name: "Reconciliations");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "Subgroups");

            migrationBuilder.DropTable(
                name: "TranslationKeys");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "LK_EntryKind");

            migrationBuilder.DropTable(
                name: "LK_EntryStatus");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "LK_SupplierStatus");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LK_AccountType");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "LK_ItemType");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "LK_ProjectStatus");
        }
    }
}
