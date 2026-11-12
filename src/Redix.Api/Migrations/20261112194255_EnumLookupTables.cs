using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class EnumLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Status",
                table: "Suppliers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Kind",
                table: "Notifications",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ItemColumns_DataType",
                table: "ItemColumns",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_History_Action",
                table: "History",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDecisions_Action",
                table: "EntryDecisions",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_EntryAttachments_Role",
                table: "EntryAttachments",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Kind",
                table: "Entries",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_Status",
                table: "Entries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_DataType",
                table: "DefaultColumns",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_DefaultColumns_ItemType",
                table: "DefaultColumns",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_CapitalMovementDetails_Type",
                table: "CapitalMovementDetails",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type",
                table: "Accounts",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_LK_AccountType_Type",
                table: "Accounts",
                column: "Type",
                principalTable: "LK_AccountType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CapitalMovementDetails_LK_CapitalMovementType_Type",
                table: "CapitalMovementDetails",
                column: "Type",
                principalTable: "LK_CapitalMovementType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DefaultColumns_LK_ColumnDataType_DataType",
                table: "DefaultColumns",
                column: "DataType",
                principalTable: "LK_ColumnDataType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DefaultColumns_LK_ItemType_ItemType",
                table: "DefaultColumns",
                column: "ItemType",
                principalTable: "LK_ItemType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_LK_EntryKind_Kind",
                table: "Entries",
                column: "Kind",
                principalTable: "LK_EntryKind",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_LK_EntryStatus_Status",
                table: "Entries",
                column: "Status",
                principalTable: "LK_EntryStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryAttachments_LK_EntryAttachmentRole_Role",
                table: "EntryAttachments",
                column: "Role",
                principalTable: "LK_EntryAttachmentRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntryDecisions_LK_EntryDecisionAction_Action",
                table: "EntryDecisions",
                column: "Action",
                principalTable: "LK_EntryDecisionAction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_History_LK_HistoryAction_Action",
                table: "History",
                column: "Action",
                principalTable: "LK_HistoryAction",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemColumns_LK_ColumnDataType_DataType",
                table: "ItemColumns",
                column: "DataType",
                principalTable: "LK_ColumnDataType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LK_ItemType_Type",
                table: "Items",
                column: "Type",
                principalTable: "LK_ItemType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_LK_NotificationKind_Kind",
                table: "Notifications",
                column: "Kind",
                principalTable: "LK_NotificationKind",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_LK_ProjectStatus_Status",
                table: "Projects",
                column: "Status",
                principalTable: "LK_ProjectStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_LK_SupplierStatus_Status",
                table: "Suppliers",
                column: "Status",
                principalTable: "LK_SupplierStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_LK_AccountType_Type",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_CapitalMovementDetails_LK_CapitalMovementType_Type",
                table: "CapitalMovementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DefaultColumns_LK_ColumnDataType_DataType",
                table: "DefaultColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_DefaultColumns_LK_ItemType_ItemType",
                table: "DefaultColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_Entries_LK_EntryKind_Kind",
                table: "Entries");

            migrationBuilder.DropForeignKey(
                name: "FK_Entries_LK_EntryStatus_Status",
                table: "Entries");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryAttachments_LK_EntryAttachmentRole_Role",
                table: "EntryAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryDecisions_LK_EntryDecisionAction_Action",
                table: "EntryDecisions");

            migrationBuilder.DropForeignKey(
                name: "FK_History_LK_HistoryAction_Action",
                table: "History");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemColumns_LK_ColumnDataType_DataType",
                table: "ItemColumns");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LK_ItemType_Type",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_LK_NotificationKind_Kind",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_LK_ProjectStatus_Status",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_LK_SupplierStatus_Status",
                table: "Suppliers");

            migrationBuilder.DropTable(
                name: "LK_AccountType");

            migrationBuilder.DropTable(
                name: "LK_CapitalMovementType");

            migrationBuilder.DropTable(
                name: "LK_ColumnDataType");

            migrationBuilder.DropTable(
                name: "LK_EntryAttachmentRole");

            migrationBuilder.DropTable(
                name: "LK_EntryDecisionAction");

            migrationBuilder.DropTable(
                name: "LK_EntryKind");

            migrationBuilder.DropTable(
                name: "LK_EntryStatus");

            migrationBuilder.DropTable(
                name: "LK_HistoryAction");

            migrationBuilder.DropTable(
                name: "LK_ItemType");

            migrationBuilder.DropTable(
                name: "LK_NotificationKind");

            migrationBuilder.DropTable(
                name: "LK_ProjectStatus");

            migrationBuilder.DropTable(
                name: "LK_SupplierStatus");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Status",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Kind",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Items_Type",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_ItemColumns_DataType",
                table: "ItemColumns");

            migrationBuilder.DropIndex(
                name: "IX_History_Action",
                table: "History");

            migrationBuilder.DropIndex(
                name: "IX_EntryDecisions_Action",
                table: "EntryDecisions");

            migrationBuilder.DropIndex(
                name: "IX_EntryAttachments_Role",
                table: "EntryAttachments");

            migrationBuilder.DropIndex(
                name: "IX_Entries_Kind",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_Status",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_DefaultColumns_DataType",
                table: "DefaultColumns");

            migrationBuilder.DropIndex(
                name: "IX_DefaultColumns_ItemType",
                table: "DefaultColumns");

            migrationBuilder.DropIndex(
                name: "IX_CapitalMovementDetails_Type",
                table: "CapitalMovementDetails");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Type",
                table: "Accounts");
        }
    }
}
