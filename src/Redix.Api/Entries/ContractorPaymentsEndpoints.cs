using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>Records a payment against a contract (UC-027, FR-12): the same ledger as a purchase, what is paid from checked by the same rules, and the contract's remaining answered with it.</summary>
public static class ContractorPaymentsEndpoints
{
    /// <summary>Maps the route that records a contractor payment, for whoever holds <see cref="Permission.RecordEntries"/>.</summary>
    public static void MapContractorPayments(this WebApplication app) => app.MapProjectChanges(Permission.RecordEntries).MapPost("/contractor-payments", Record);

    /// <summary>
    /// Records a payment as a draft, or sent for approval, and answers what the contract stands at with it, an overrun included: going beyond the value is saved, with an alert (AC-07.1). A contract not active in the project answers <c>unknownContract</c>; its item no longer taking contractor payments, <c>itemNotOffered</c> (AC-38.1).
    /// What it names and what was typed answer as a purchase's do, and an amount of zero <c>invalidAmount</c>. A negative amount is a deduction (FR-14), which has neither receipt nor tax invoice and has to say why: otherwise <c>deductionNeedsReason</c>, and a reason on anything else <c>invalidWithoutInvoiceReason</c>. Sending needs the receipt, <c>receiptRequired</c> (AC-12.1), and the required columns.
    /// A tax invoice is optional, and answers <c>invalidInvoiceNumber</c>, <c>invalidInvoiceDate</c>, <c>invalidInvoiceAmount</c> and <c>duplicateInvoice</c> as a purchase's does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What was typed and chosen.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, ContractorPaymentRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var (status, idRefusal) = await EntryRecording.RefusalOfIdAsync(request.Id, db);
        if (idRefusal.Length > 0)
        {
            return Refusal.Of(status, idRefusal);
        }
        var contract = await db.Contracts.SingleOrDefaultAsync(row => row.Id == request.ContractId && row.ProjectId == projectId && row.IsActive);
        if (contract is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownContract");
        }
        var item = await ProjectStructure.OfferedItemAsync(projectId, contract.ItemId, ItemType.ContractorPayments, db);
        if (item is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ItemNotOffered");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var access = await UserAccess.OfAsync(userId, db);
        var refusal = await EntryRecording.RefusalOfPaymentAsync(projectId, request, userId, access.In(projectId), db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTyped();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfDates(project.StartDate, Entry.Today(time));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var (reason, columnKey) = EntryRecording.RefusalOfColumns(request, item.Columns.Where(column => !column.IsHidden).ToList());
        if (reason.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, reason, "columnKey", columnKey);
        }
        if (request.Submit && request.AttachmentId == Guid.Empty && !request.IsWithoutInvoice())
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ReceiptRequired");
        }
        if (request.AttachmentId != Guid.Empty && await EntryRecording.IsFileOnAnEntryAsync(projectId, request.AttachmentId, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }

        var contractor = await db.Suppliers.SingleAsync(row => row.Id == contract.SupplierId);
        var figures = default(InvoiceFigures);
        if (request.HasInvoice())
        {
            figures = InvoiceFigures.Of(request.InvoiceAmountBeforeVat, includesVat: false, discount: 0, contractor.VatNumber.Length == 0 ? 0 : project.VatRate);
            if (figures.Total > Money.Largest)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidInvoiceAmount");
            }
            var earlier = request.ConfirmDuplicate ? null : await EntryRecording.EarlierEntryAsync(contractor, request.InvoiceNumber.Trim(), access, db);
            if (earlier is not null)
            {
                return Refusal.Of(StatusCodes.Status409Conflict, "DuplicateInvoice", "earlier", earlier);
            }
        }

        var money = (await ContractMoney.OfProjectAsync(projectId, db)).GetValueOrDefault(contract.Id);
        var entry = EntryRecording.EntryOf(request, projectId, EntryKind.ContractorPayment, request.PaymentDate, request.Amount, userId, time.GetUtcNow().UtcDateTime);
        await EntryRecording.SaveAsync(entry, DetailsOf(request, contract, item.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, figures), userId, db);
        var counted = money.Paid + money.Pending + (request.IsOutsideContract ? 0 : request.Amount);
        return Results.Created($"/projects/{projectId}/entries/{entry.Id}", new ContractorPaymentView(entry.Id, entry.Status, entry.Amount, figures.Vat, money.Paid, money.Pending, contract.RemainingAfter(counted), contract.IsOverrunBy(counted)));
    }

    private static List<object> DetailsOf(ContractorPaymentRequest request, Contract contract, Guid subgroupId, InvoiceFigures figures)
    {
        List<object> details =
        [
            EntryRecording.ExpenseDetailsOf(request, contract.ItemId, subgroupId, contract.UnitId),
            new ContractorPaymentDetails { EntryId = request.Id, ContractId = contract.Id, IsOutsideContract = request.IsOutsideContract },
        ];
        if (request.HasInvoice())
        {
            details.Add(EntryRecording.InvoiceOf(request.Id, request.InvoiceNumber.Trim(), request.InvoiceDate, figures, isCreditNote: false));
        }
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = request.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.Receipt });
        }
        return details;
    }
}

/// <summary>A recorded contractor payment, and what its contract stands at with it.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="Vat">The VAT of its tax invoice; 0 with none.</param>
/// <param name="Paid">What was approved inside the contract before it.</param>
/// <param name="Pending">What waited for approval inside the contract before it.</param>
/// <param name="RemainingAfter">The value less everything paid or waiting, with this payment if inside the contract; 0 for an open-value contract.</param>
/// <param name="IsOverrun">Whether that goes beyond the value, which shows an alert.</param>
public record ContractorPaymentView(Guid Id, EntryStatus Status, decimal Amount, decimal Vat, decimal Paid, decimal Pending, decimal RemainingAfter, bool IsOverrun);
