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
    public static void MapContractorPayments(this WebApplication app)
    {
        var changes = app.MapProjectChanges(Permission.RecordEntries);
        changes.MapPost("/contractor-payments", Record);
        changes.MapPost("/contractor-payments/{entryId:guid}", Edit);
    }

    /// <summary>
    /// Records a payment as a draft, or sent for approval, and answers what the contract stands at with it, an overrun included: going beyond the value is saved, with an alert (AC-07.1). A contract not active in the project answers <c>unknownContract</c>; its item no longer taking contractor payments, <c>itemNotOffered</c> (AC-38.1).
    /// What it names and what was typed answer as a purchase's do, and an amount of zero <c>invalidAmount</c>. A negative amount is a deduction (FR-14), which says why in its notes or answers <c>deductionNeedsNotes</c> (D14). Sending needs the required columns; the receipt is optional (D16).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What was typed and chosen.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static Task<IResult> Record(Guid projectId, ContractorPaymentRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        EntryRecording.RecordAsync(projectId, request, principal, db, () => SaveAsync(projectId, request, principal, db, time, null));

    /// <summary>Changes a contractor payment before it is approved (UC-033), through every check a new one passes, and with the refusals of <see cref="EntryChanges.RefusalOf"/>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">What it holds now.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static Task<IResult> Edit(Guid projectId, Guid entryId, ContractorPaymentRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        EntryRecording.ChangeAsync(projectId, entryId, EntryKind.ContractorPayment, principal, db, entry => SaveAsync(projectId, request with { Id = entryId }, principal, db, time, entry));

    private static async Task<IResult> SaveAsync(Guid projectId, ContractorPaymentRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, Entry? existing)
    {
        var editing = existing?.Id ?? Guid.Empty;
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
        var refusal = await EntryRecording.RefusalOfAsync(projectId, request, item, userId, (await UserAccess.OfAsync(userId, db)).In(projectId), editing, db, time);
        if (refusal.Code.Length > 0)
        {
            return Refusal.Of(refusal.Status, refusal.Code, "columnKey", refusal.ColumnKey);
        }

        var money = (await ContractMoney.OfProjectAsync(projectId, db)).GetValueOrDefault(contract.Id);
        var at = time.GetUtcNow().UtcDateTime;
        var entry = await EntryRecording.KeepAsync(EntryRecording.EntryOf(request, projectId, EntryKind.ContractorPayment, request.Amount, userId, at), DetailsOf(request, contract, item), existing, userId, db);
        var counted = money.Paid + money.Pending + (request.IsOutsideContract ? 0 : request.Amount);
        return Results.Created($"/projects/{projectId}/entries/{entry.Id}", new ContractorPaymentView(entry.Id, entry.Status, entry.Amount, money.Paid, money.Pending, contract.RemainingAfter(counted), contract.IsOverrunBy(counted)));
    }

    /// <summary>What the payment holds besides its entry: under its item's general subgroup, its contract being its table (R11), with its receipt when there is one.</summary>
    private static List<object> DetailsOf(ContractorPaymentRequest request, Contract contract, Item item)
    {
        List<object> details =
        [
            EntryRecording.ExpenseDetailsOf(request, contract.ItemId, item.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id),
            new ContractorPaymentDetails { EntryId = request.Id, ContractId = contract.Id, IsOutsideContract = request.IsOutsideContract },
        ];
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
/// <param name="Paid">What was approved inside the contract before it.</param>
/// <param name="Pending">What waited for approval inside the contract before it.</param>
/// <param name="RemainingAfter">The value less everything paid or waiting, with this payment if inside the contract; 0 for an open-value contract.</param>
/// <param name="IsOverrun">Whether that goes beyond the value, which shows an alert.</param>
public record ContractorPaymentView(Guid Id, EntryStatus Status, decimal Amount, decimal Paid, decimal Pending, decimal RemainingAfter, bool IsOverrun);
