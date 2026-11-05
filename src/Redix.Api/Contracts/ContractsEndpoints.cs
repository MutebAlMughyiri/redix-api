using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Contracts;

/// <summary>A project's contracts (UC-020, FR-07). There is no delete, and every change is in the audit trail, because the remaining of a contract is money.</summary>
public static class ContractsEndpoints
{
    /// <summary>Maps the route that reads a project's contracts, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapContracts(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/contracts", List).RequireAuthorization(nameof(Permission.ReadProject));

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/contracts", Add);
        changes.MapPost("/contracts/{contractId:guid}", Amend);
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
            join unit in db.Set<Unit>() on contract.UnitId equals unit.Id
            where contract.ProjectId == projectId
            select new { Contract = contract, SupplierName = supplier.Name, ItemName = item.Name, UnitName = unit.Name }).ToListAsync();
        return rows.OrderBy(row => row.Contract.Id).Select(row => ContractView.Of(row.Contract, row.Contract.Paid, row.SupplierName, row.ItemName, row.UnitName)).ToList();
    }

    /// <summary>
    /// Adds a contract. A contractor who is not an active supplier of the project answers <c>unknownSupplier</c>; an item that takes no contractor payments, <c>itemNotOffered</c>; a unit outside the project, <c>unknownUnit</c>;
    /// a value not above zero, <c>invalidValue</c>; a unit price or a quantity not acceptable, <c>invalidUnitPrice</c> or <c>invalidQuantity</c>; notes too long, <c>invalidNotes</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The contractor, item and unit, and the terms.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Add(Guid projectId, AddContractRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var refusal = await RefusalOfPartiesAsync(projectId, request, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTerms();
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var contract = new Contract { ProjectId = projectId, ItemId = request.ItemId, UnitId = request.UnitId, SupplierId = request.SupplierId };
        request.ApplyTo(contract);
        db.Contracts.Add(contract);
        db.Audit(contract, AuditAction.Create, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Amends a contract's terms; its contractor, item and unit never change. It answers as <see cref="Add"/> does for the terms.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="terms">The new terms.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static async Task<IResult> Amend(Guid projectId, Guid contractId, ContractTerms terms, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var refusal = terms.RefusalOfTerms();
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        return await ChangeAsync(projectId, contractId, principal, db, time, AuditAction.Update, terms.ApplyTo);
    }

    /// <summary>Disables a contract: no new payment can name it, and it stays in the reports.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Disable(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, contractId, principal, db, time, AuditAction.Disable, contract => contract.IsActive = false);

    /// <summary>Brings a disabled contract back.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="contractId">The contract.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time, for the audit record.</param>
    public static Task<IResult> Enable(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, contractId, principal, db, time, AuditAction.Enable, contract => contract.IsActive = true);

    private static async Task<string> RefusalOfPartiesAsync(Guid projectId, AddContractRequest request, RedixDbContext db)
    {
        if (!await db.Suppliers.AnyAsync(supplier => supplier.Id == request.SupplierId && supplier.ProjectId == projectId && supplier.IsActive))
        {
            return "UnknownSupplier";
        }
        var category = await db.Categories.Include(row => row.Items).SingleOrDefaultAsync(row => row.ProjectId == projectId && row.Items.Any(item => item.Id == request.ItemId));
        if (category is null || !category.ItemsOfferedFor(ItemType.ContractorPayments).Any(item => item.Id == request.ItemId))
        {
            return "ItemNotOffered";
        }
        return await db.Set<Unit>().AnyAsync(unit => unit.Id == request.UnitId && unit.ProjectId == projectId) ? "" : "UnknownUnit";
    }

    private static async Task<IResult> ChangeAsync(Guid projectId, Guid contractId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, AuditAction action, Action<Contract> change)
    {
        var contract = await db.Contracts.SingleOrDefaultAsync(row => row.Id == contractId && row.ProjectId == projectId);
        if (contract is null)
        {
            return Results.NotFound();
        }
        change(contract);
        db.Audit(contract, action, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
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

    /// <summary>Why a contract cannot have these terms; empty when it can.</summary>
    public string RefusalOfTerms() => Contract.RefusalOf(Value, IsOpenValue, UnitPrice, Quantity, Notes.Trim());

    /// <summary>Makes these the terms of <paramref name="contract"/>.</summary>
    public void ApplyTo(Contract contract) => contract.SetTerms(Value, IsOpenValue, UnitPrice, Quantity, Notes.Trim());
}

/// <summary>What the app sends to add a contract.</summary>
/// <param name="SupplierId">The contractor, a supplier of the project.</param>
/// <param name="ItemId">The item it is for.</param>
/// <param name="UnitId">The unit it is for.</param>
/// <param name="Value">What was promised; ignored when <paramref name="IsOpenValue"/>.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
public record AddContractRequest(Guid SupplierId, Guid ItemId, Guid UnitId, decimal Value, bool IsOpenValue) : ContractTerms(Value, IsOpenValue);

/// <summary>A contract as the app shows it.</summary>
/// <param name="Id">The contract's id.</param>
/// <param name="SupplierName">The contractor's name.</param>
/// <param name="ItemName">The item's name.</param>
/// <param name="UnitName">The unit's name.</param>
/// <param name="Value">What was promised; 0 for an open-value contract.</param>
/// <param name="IsOpenValue">True for a contract whose payments only add up.</param>
/// <param name="UnitPrice">The price of one unit of work; 0 when not used.</param>
/// <param name="Quantity">How many units of work; 0 when not used.</param>
/// <param name="Notes">Notes on the contract and its amendments; empty when none.</param>
/// <param name="Paid">What has been paid inside the contract.</param>
/// <param name="Remaining">The value less what was paid, below zero once overrun; 0 for an open-value contract.</param>
/// <param name="IsOverrun">Whether what was paid goes beyond the value, which shows an alert.</param>
/// <param name="IsActive">False once disabled.</param>
public record ContractView(Guid Id, string SupplierName, string ItemName, string UnitName, decimal Value, bool IsOpenValue, decimal UnitPrice, decimal Quantity, string Notes, decimal Paid, decimal Remaining, bool IsOverrun, bool IsActive)
{
    /// <summary>The view of <paramref name="contract"/> with <paramref name="paid"/> paid inside it, between the parties named.</summary>
    public static ContractView Of(Contract contract, decimal paid, string supplierName, string itemName, string unitName) =>
        new(contract.Id, supplierName, itemName, unitName, contract.Value, contract.IsOpenValue, contract.UnitPrice, contract.Quantity, contract.Notes, paid, contract.RemainingAfter(paid), contract.IsOverrunBy(paid), contract.IsActive);
}
