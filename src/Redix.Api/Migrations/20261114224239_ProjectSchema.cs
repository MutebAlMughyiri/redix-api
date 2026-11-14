using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redix.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "project");

            migrationBuilder.RenameTable(
                name: "Units",
                newName: "Units",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Transfers",
                newName: "Transfers",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Suppliers",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "SupplierDefaultItems",
                newName: "SupplierDefaultItems",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Subgroups",
                newName: "Subgroups",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "RevenueDetails",
                newName: "RevenueDetails",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Reconciliations",
                newName: "Reconciliations",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ReconciliationBalances",
                newName: "ReconciliationBalances",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "PurchaseDetails",
                newName: "PurchaseDetails",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "PeriodicReports",
                newName: "PeriodicReports",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "PaymentMethods",
                newName: "PaymentMethods",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "PartnerUsers",
                newName: "PartnerUsers",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Partners",
                newName: "Partners",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Items",
                newName: "Items",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ItemColumns",
                newName: "ItemColumns",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Invoices",
                newName: "Invoices",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ExpenseDetails",
                newName: "ExpenseDetails",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "EntryDecisions",
                newName: "EntryDecisions",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "EntryAttachments",
                newName: "EntryAttachments",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Entries",
                newName: "Entries",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Documents",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "DefaultColumns",
                newName: "DefaultColumns",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Contracts",
                newName: "Contracts",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ContractorPaymentDetails",
                newName: "ContractorPaymentDetails",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ContractAttachments",
                newName: "ContractAttachments",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "ChosenPartners",
                newName: "ChosenPartners",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Categories",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "CapitalMovementDetails",
                newName: "CapitalMovementDetails",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachments",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Accounts",
                newSchema: "project");

            migrationBuilder.RenameTable(
                name: "AccountCustodians",
                newName: "AccountCustodians",
                newSchema: "project");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Units",
                schema: "project",
                newName: "Units");

            migrationBuilder.RenameTable(
                name: "Transfers",
                schema: "project",
                newName: "Transfers");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                schema: "project",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "SupplierDefaultItems",
                schema: "project",
                newName: "SupplierDefaultItems");

            migrationBuilder.RenameTable(
                name: "Subgroups",
                schema: "project",
                newName: "Subgroups");

            migrationBuilder.RenameTable(
                name: "RevenueDetails",
                schema: "project",
                newName: "RevenueDetails");

            migrationBuilder.RenameTable(
                name: "Reconciliations",
                schema: "project",
                newName: "Reconciliations");

            migrationBuilder.RenameTable(
                name: "ReconciliationBalances",
                schema: "project",
                newName: "ReconciliationBalances");

            migrationBuilder.RenameTable(
                name: "PurchaseDetails",
                schema: "project",
                newName: "PurchaseDetails");

            migrationBuilder.RenameTable(
                name: "PeriodicReports",
                schema: "project",
                newName: "PeriodicReports");

            migrationBuilder.RenameTable(
                name: "PaymentMethods",
                schema: "project",
                newName: "PaymentMethods");

            migrationBuilder.RenameTable(
                name: "PartnerUsers",
                schema: "project",
                newName: "PartnerUsers");

            migrationBuilder.RenameTable(
                name: "Partners",
                schema: "project",
                newName: "Partners");

            migrationBuilder.RenameTable(
                name: "Items",
                schema: "project",
                newName: "Items");

            migrationBuilder.RenameTable(
                name: "ItemColumns",
                schema: "project",
                newName: "ItemColumns");

            migrationBuilder.RenameTable(
                name: "Invoices",
                schema: "project",
                newName: "Invoices");

            migrationBuilder.RenameTable(
                name: "ExpenseDetails",
                schema: "project",
                newName: "ExpenseDetails");

            migrationBuilder.RenameTable(
                name: "EntryDecisions",
                schema: "project",
                newName: "EntryDecisions");

            migrationBuilder.RenameTable(
                name: "EntryAttachments",
                schema: "project",
                newName: "EntryAttachments");

            migrationBuilder.RenameTable(
                name: "Entries",
                schema: "project",
                newName: "Entries");

            migrationBuilder.RenameTable(
                name: "Documents",
                schema: "project",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "DefaultColumns",
                schema: "project",
                newName: "DefaultColumns");

            migrationBuilder.RenameTable(
                name: "Contracts",
                schema: "project",
                newName: "Contracts");

            migrationBuilder.RenameTable(
                name: "ContractorPaymentDetails",
                schema: "project",
                newName: "ContractorPaymentDetails");

            migrationBuilder.RenameTable(
                name: "ContractAttachments",
                schema: "project",
                newName: "ContractAttachments");

            migrationBuilder.RenameTable(
                name: "ChosenPartners",
                schema: "project",
                newName: "ChosenPartners");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "project",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "CapitalMovementDetails",
                schema: "project",
                newName: "CapitalMovementDetails");

            migrationBuilder.RenameTable(
                name: "Attachments",
                schema: "project",
                newName: "Attachments");

            migrationBuilder.RenameTable(
                name: "Accounts",
                schema: "project",
                newName: "Accounts");

            migrationBuilder.RenameTable(
                name: "AccountCustodians",
                schema: "project",
                newName: "AccountCustodians");
        }
    }
}
