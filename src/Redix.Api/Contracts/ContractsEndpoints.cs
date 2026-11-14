using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Attachments;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Contracts;

/// <summary>A project's contracts (UC-020, FR-07). There is no delete, and every change is in the History table, because the remaining of a contract is money.</summary>
public static class ContractsEndpoints
{
    /// <summary>Maps the route that reads a project's contracts, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapContracts(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/contracts", List).RequireAuthorization(nameof(Permission.ReadProject));

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/contracts", Add);
        changes.MapPut("/contracts/{contractId:guid}", Amend);
        changes.MapPost("/contracts/{contractId:guid}/disable", Disable);
        changes.MapPost("/contracts/{contractId:guid}/enable", Enable);
    }

    /// <summary>Every contract of the project, disabled ones included, in the order they were added, each with its value, paid and remaining (BR-04).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<ContractView>> List(Guid projectId, RedixDbContext db)
    {
        var rows = await (
            from contract in db.Contracts
            join supplier in db.Suppliers on contract.SupplierId equals supplier.Id
            join item in db.Items on contract.ItemId equals item.Id
            where contract.ProjectId == projectId
            select new { Contract = contract, SupplierName = supplier.Name, ItemName = item.Name }).ToListAsync();
        var documents = (await (
            from link in db.Set<ContractAttachment>()
            join attachment in db.Attachments on link.AttachmentId equals attachment.Id
            where attachment.ProjectId == projectId
            select new { link.ContractId, Attachment = attachment }).ToListAsync()).OrderBy(row => row.Attachment.Id).ToLookup(row => row.ContractId, row => AttachmentView.Of(row.Attachment));
        var money = await ContractMoney.OfProjectAsync(projectId, db);
        var withPayments = await db.ContractorPaymentDetails.Where(payment => db.Contracts.Any(contract => contract.Id == payment.ContractId && contract.ProjectId == projectId)).Select(payment => payment.ContractId).Distinct().ToListAsync();
        return rows.OrderBy(row => row.Contract.Id)
            .Select(row => ContractView.Of(row.Contract, money.GetValueOrDefault(row.Contract.Id), row.SupplierName, row.ItemName) with { HasPayments = withPayments.Contains(row.Contract.Id), Documents = [.. documents[row.Contract.Id]] }).ToList();
    }

    /// <summary>
    /// Adds a contract. A contractor who is not an active supplier of the project answers <c>unknownSupplier</c>; an item that takes no contractor payments, <c>itemNotOffered</c>;
    /// a value not above zero, <c>invalidValue</c>; a unit price or a quantity not acceptable, <c>invalidUnitPrice</c> or <c>invalidQuantity</c>; notes too long, <c>invalidNotes</c>; a document that is no file of the project, <c>unknownAttachment</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The contractor and item, and the terms.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Add(Guid projectId, AddContractRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var refusal = await AddAsync(projectId, request, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime, db);
        return refusal.Length > 0 ? Refusal.Of(StatusCodes.Status400BadRequest, refusal) : Results.Ok();
    }

    /// <summary>
    /// Adds the contract <paramref name="request"/> by <paramref name="userId"/> at <paramref name="at"/>, last among its item's tables, in the History table: the one way a contract is
    /// added, by hand and from a project file's load alike (redix-api #132 D6). It answers the refusals of <see cref="Add"/>; empty once added.
    /// </summary>
    public static async Task<string> AddAsync(Guid projectId, AddContractRequest request, Guid userId, DateTime at, RedixDbContext db)
    {
        var refusal = await RefusalOfPartiesAsync(projectId, request.SupplierId, request.ItemId, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTerms();
        refusal = refusal.Length > 0 ? refusal : await RefusalOfDocumentsAsync(projectId, request.AttachmentIds, db);
        if (refusal.Length > 0)
        {
            return refusal;
        }

        var contract = new Contract { ProjectId = projectId, ItemId = request.ItemId, SupplierId = request.SupplierId, SortOrder = await ProjectStructure.NextTablePlaceAsync(request.ItemId, db) };
        request.ApplyTo(contract);
        db.Contracts.Add(contract);
        db.AddHistory(contract, HistoryAction.Create, userId, at);
        await AttachAsync(contract.Id, request.AttachmentIds, userId, at, db);
        await db.SaveChangesAsync();
        return "";
    }

    /// <summary>
    /// Amends a contract's terms and adds to its documents, a document staying once attached, and changes its contractor or item while no payment names it, or answers <c>contractHasPayments</c> and changes nothing.
    /// It answers as <see cref="Add"/> does for all of them, and all of it is in the History table.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="request">The new terms, and any new parties.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Amend(Guid projectId, Guid contractId, AmendContractRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var refusal = request.RefusalOfTerms();
        refusal = refusal.Length > 0 ? refusal : await RefusalOfDocumentsAsync(projectId, request.AttachmentIds, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var contract = await db.Contracts.SingleOrDefaultAsync(row => row.Id == contractId && row.ProjectId == projectId);
        if (contract is null)
        {
            return Results.NotFound();
        }
        if (request.ChangesPartiesOf(contract))
        {
            if (await db.ContractorPaymentDetails.AnyAsync(payment => payment.ContractId == contractId))
            {
                return Refusal.Of(StatusCodes.Status409Conflict, "ContractHasPayments");
            }
            refusal = await RefusalOfPartiesAsync(projectId, request.SupplierOf(contract), request.ItemOf(contract), db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
            }
        }
        var place = request.ItemOf(contract) == contract.ItemId ? contract.SortOrder : await ProjectStructure.NextTablePlaceAsync(request.ItemOf(contract), db);
        return await ChangeAsync(projectId, contractId, principal, db, time, HistoryAction.Update, changed =>
        {
            request.ApplyAllTo(changed);
            changed.SortOrder = place;
        }, request.AttachmentIds);
    }

    /// <summary>Disables a contract: no new payment can name it, and it stays in the reports.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Disable(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, contractId, principal, db, time, HistoryAction.Disable, contract => contract.IsActive = false);

    /// <summary>Brings a disabled contract back.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Enable(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, contractId, principal, db, time, HistoryAction.Enable, contract => contract.IsActive = true);

    private static async Task<string> RefusalOfPartiesAsync(Guid projectId, Guid supplierId, Guid itemId, RedixDbContext db)
    {
        if (!await db.Suppliers.AnyAsync(supplier => supplier.Id == supplierId && supplier.ProjectId == projectId && supplier.IsActive))
        {
            return "UnknownSupplier";
        }
        return await ProjectStructure.OfferedItemAsync(projectId, itemId, ItemType.ContractorPayments, db) == Item.None ? "ItemNotOffered" : "";
    }

    private static async Task<string> RefusalOfDocumentsAsync(Guid projectId, List<Guid> attachmentIds, RedixDbContext db)
    {
        var kept = await db.Attachments.CountAsync(attachment => attachment.ProjectId == projectId && attachmentIds.Contains(attachment.Id));
        return kept == attachmentIds.Distinct().Count() ? "" : "UnknownAttachment";
    }

    private static async Task AttachAsync(Guid contractId, List<Guid> attachmentIds, Guid userId, DateTime at, RedixDbContext db)
    {
        var held = await db.Set<ContractAttachment>().Where(link => link.ContractId == contractId).Select(link => link.AttachmentId).ToListAsync();
        foreach (var attachmentId in attachmentIds.Except(held))
        {
            var link = new ContractAttachment { ContractId = contractId, AttachmentId = attachmentId };
            db.Add(link);
            db.AddHistory(link, HistoryAction.Attach, userId, at);
        }
    }

    private static async Task<IResult> ChangeAsync(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, HistoryAction action, Action<Contract> change, params List<Guid> attachmentIds)
    {
        var contract = await db.Contracts.SingleOrDefaultAsync(row => row.Id == contractId && row.ProjectId == projectId);
        if (contract is null)
        {
            return Results.NotFound();
        }
        change(contract);
        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        db.AddHistory(contract, action, userId, at);
        await AttachAsync(contractId, attachmentIds, userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What can change on a contract, which is what the app sends to amend it.</summary>
/// <param name="Value">What was promised; ignored when <paramref name="IsOpenValue"/>.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
public record ContractTerms(decimal Value, bool IsOpenValue)
{
    /// <summary>The price of one unit of work; left out when not used.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>How many units of work; left out when not used.</summary>
    public decimal Quantity { get; init; }

    /// <summary>Notes on the contract and its amendments; left out when none.</summary>
    public string Notes { get; init; } = "";

    /// <summary>Files of the project this contract is to carry, besides those it already does; left out when none.</summary>
    public List<Guid> AttachmentIds { get; init; } = [];

    /// <summary>Why a contract cannot have these terms; empty when it can.</summary>
    public string RefusalOfTerms() => Contract.RefusalOf(Value, IsOpenValue, UnitPrice, Quantity, Notes.Trim());

    /// <summary>Makes these the terms of <paramref name="contract"/>.</summary>
    public void ApplyTo(Contract contract) => contract.SetTerms(Value, IsOpenValue, UnitPrice, Quantity, Notes.Trim());
}

/// <summary>What the app sends to amend a contract: its terms, and any contractor or item that is to change.</summary>
/// <param name="Value">What was promised; ignored when <paramref name="IsOpenValue"/>.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
public record AmendContractRequest(decimal Value, bool IsOpenValue) : ContractTerms(Value, IsOpenValue)
{
    /// <summary>The contractor it is to have; left out when it stays.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>The item it is to be for; left out when it stays.</summary>
    public Guid ItemId { get; init; }

    /// <summary>The contractor <paramref name="contract"/> has after this.</summary>
    public Guid SupplierOf(Contract contract) => SupplierId == Guid.Empty ? contract.SupplierId : SupplierId;

    /// <summary>The item <paramref name="contract"/> is for after this.</summary>
    public Guid ItemOf(Contract contract) => ItemId == Guid.Empty ? contract.ItemId : ItemId;

    /// <summary>Whether this changes the contractor or item of <paramref name="contract"/>.</summary>
    public bool ChangesPartiesOf(Contract contract) => SupplierOf(contract) != contract.SupplierId || ItemOf(contract) != contract.ItemId;

    /// <summary>Makes these the terms and the parties of <paramref name="contract"/>.</summary>
    public void ApplyAllTo(Contract contract)
    {
        ApplyTo(contract);
        (contract.SupplierId, contract.ItemId) = (SupplierOf(contract), ItemOf(contract));
    }
}

/// <summary>What the app sends to add a contract.</summary>
/// <param name="SupplierId">The contractor, a supplier of the project.</param>
/// <param name="ItemId">The item it is for.</param>
/// <param name="Value">What was promised; ignored when <paramref name="IsOpenValue"/>.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
public record AddContractRequest(Guid SupplierId, Guid ItemId, decimal Value, bool IsOpenValue) : ContractTerms(Value, IsOpenValue);

/// <summary>A contract as the app shows it.</summary>
/// <param name="Id">The contract's id.</param>
/// <param name="ItemId">The item it is for, whose columns a payment asks.</param>
/// <param name="SupplierName">The contractor's name.</param>
/// <param name="ItemName">The item's name.</param>
/// <param name="Value">What was promised; 0 for an open-value contract.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
/// <param name="UnitPrice">The price of one unit of work; 0 when not used.</param>
/// <param name="Quantity">How many units of work; 0 when not used.</param>
/// <param name="Notes">Notes on the contract and its amendments; empty when none.</param>
/// <param name="Paid">What has been paid inside the contract, which counts only once approved (BR-10).</param>
/// <param name="Pending">What waits for approval inside the contract, shown beside what was paid.</param>
/// <param name="Remaining">The value less what was paid, below zero once overrun; 0 for an open-value contract.</param>
/// <param name="IsOverrun">Whether what was paid goes beyond the value, which shows an alert.</param>
/// <param name="IsActive">False once disabled.</param>
public record ContractView(Guid Id, Guid ItemId, string SupplierName, string ItemName, decimal Value, bool IsOpenValue, decimal UnitPrice, decimal Quantity, string Notes, decimal Paid, decimal Pending, decimal Remaining, bool IsOverrun, bool IsActive)
{
    /// <summary>The contractor.</summary>
    public Guid SupplierId { get; init; }

    /// <summary>Whether any payment names it, whatever became of that payment: its contractor and item then stay as they are.</summary>
    public bool HasPayments { get; init; }

    /// <summary>The documents it carries, in the order they were kept.</summary>
    public IReadOnlyList<AttachmentView> Documents { get; init; } = [];

    /// <summary>The view of <paramref name="contract"/> with <paramref name="money"/> inside it, between the parties named.</summary>
    public static ContractView Of(Contract contract, ContractMoney money, string supplierName, string itemName) =>
        new(contract.Id, contract.ItemId, supplierName, itemName, contract.Value, contract.IsOpenValue, contract.UnitPrice, contract.Quantity, contract.Notes, money.Paid, money.Pending, contract.RemainingAfter(money.Paid), contract.IsOverrunBy(money.Paid), contract.IsActive)
        {
            SupplierId = contract.SupplierId,
        };
}
