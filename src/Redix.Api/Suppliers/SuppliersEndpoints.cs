using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Suppliers;

/// <summary>A project's supplier register (UC-017, FR-06), the search of it (UC-018, FR-43), and a supplier added from the field (UC-019, FR-44). There is no delete: what is no longer wanted is disabled (BR-15).</summary>
public static class SuppliersEndpoints
{
    /// <summary>Maps the routes that read a project's register and search it, for anyone who reads the project, the one that adds, for whoever holds <see cref="Permission.AddSupplier"/>, and those that change it, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapSuppliers(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/suppliers").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/search", Search);

        app.MapProjectChanges(Permission.AddSupplier).MapPost("/suppliers", Add);

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/suppliers/{supplierId:guid}", Edit);
        changes.MapPost("/suppliers/{supplierId:guid}/disable", Disable);
        changes.MapPost("/suppliers/{supplierId:guid}/enable", Enable);
    }

    /// <summary>Every supplier of the project, disabled ones included, by name.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<SupplierView>> List(Guid projectId, RedixDbContext db) =>
        await ViewsOfAsync(await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).Include(supplier => supplier.DefaultItems).OrderBy(supplier => supplier.Name).ToListAsync(), db);

    /// <summary>
    /// What the caller's entry may pick: the suppliers on offer to them (<see cref="Supplier.OfferedTo"/>) whose name or VAT number holds <paramref name="text"/>, those on most of the caller's purchases first and then by name, at most <see cref="Supplier.SearchLimit"/> of them (AC-43.1). Fewer than
    /// <see cref="Supplier.SearchMinLength"/> letters narrow nothing down. An exact VAT number answers its one supplier, and one in no supplier's hands answers an empty list, for the app to offer a quick add (FR-43).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="text">What was typed, or read from a QR code.</param>
    public static async Task<List<SupplierView>> Search(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, string text = "")
    {
        var userId = AccessTokens.UserIdOf(principal);
        var seesPending = await KeepsSettingsAsync(projectId, userId, db);
        var typed = text.Trim();
        var narrowsNothing = typed.Length < Supplier.SearchMinLength;
        var callersPurchases = db.PurchaseDetails.Where(purchase => db.Entries.Any(entry => entry.Id == purchase.EntryId && entry.CreatedByUserId == userId));
        var found = await db.Suppliers.Where(Supplier.OfferedTo(userId, seesPending))
            .Where(supplier => supplier.ProjectId == projectId && (narrowsNothing || supplier.Name.Contains(typed) || supplier.VatNumber.Contains(typed)))
            .Include(supplier => supplier.DefaultItems)
            .OrderByDescending(supplier => callersPurchases.Count(purchase => purchase.SupplierId == supplier.Id)).ThenBy(supplier => supplier.Name).Take(Supplier.SearchLimit).ToListAsync();
        var foundIds = found.Select(supplier => supplier.Id).ToList();
        var uses = await callersPurchases.Where(purchase => foundIds.Contains(purchase.SupplierId)).GroupBy(purchase => purchase.SupplierId)
            .Select(group => new { SupplierId = group.Key, Uses = group.Count() }).ToDictionaryAsync(row => row.SupplierId, row => row.Uses);
        return (await ViewsOfAsync(found, db)).Select(view => view with { TimesUsed = uses.GetValueOrDefault(view.Id) }).ToList();
    }

    /// <summary>
    /// Adds a supplier, and answers it: approved for whoever holds <see cref="Permission.ManageSettings"/>, otherwise waiting for approval (FR-44). An unacceptable name answers <c>invalidName</c>; a VAT number breaking BR-16, <c>invalidVatNumber</c>; a mobile too long, <c>invalidMobile</c>; a default item outside the project,
    /// <c>unknownItem</c>; a VAT number another supplier of the project holds, <c>vatNumberTaken</c>, with that supplier as <c>existing</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The supplier's name, VAT number or that it has none, mobile and default item.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<IResult> Add(Guid projectId, SupplierRequest request, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var status = await KeepsSettingsAsync(projectId, userId, db) ? SupplierStatus.Approved : SupplierStatus.PendingApproval;
        return await IfAcceptableAsync(projectId, Guid.Empty, request, db, async () =>
        {
            var supplier = new Supplier { ProjectId = projectId, Name = request.Name.Trim(), VatNumber = request.VatNumberKept(), Mobile = request.Mobile.Trim(), Status = status, CreatedByUserId = userId };
            supplier.DefaultTo(request.DefaultItemId);
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
            return Results.Created($"/projects/{projectId}/suppliers/{supplier.Id}", (await ViewsOfAsync([supplier], db)).Single());
        });
    }

    /// <summary>Changes a supplier's name, VAT number, mobile and default item. It answers as <see cref="Add"/> does, leaving a supplier its own number.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="request">The supplier's name, VAT number or that it has none, mobile and default item.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<IResult> Edit(Guid projectId, Guid supplierId, SupplierRequest request, RedixDbContext db)
    {
        var supplier = await db.Suppliers.Include(row => row.DefaultItems).SingleOrDefaultAsync(row => row.Id == supplierId && row.ProjectId == projectId);
        if (supplier is null)
        {
            return Results.NotFound();
        }
        return await IfAcceptableAsync(projectId, supplierId, request, db, async () =>
        {
            supplier.Name = request.Name.Trim();
            supplier.VatNumber = request.VatNumberKept();
            supplier.Mobile = request.Mobile.Trim();
            supplier.DefaultTo(request.DefaultItemId);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }

    /// <summary>Disables a supplier: it disappears from entry and stays in the reports (BR-15).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Disable(Guid projectId, Guid supplierId, RedixDbContext db) => SetActiveAsync(projectId, supplierId, false, db);

    /// <summary>Brings a disabled supplier back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Enable(Guid projectId, Guid supplierId, RedixDbContext db) => SetActiveAsync(projectId, supplierId, true, db);

    private static async Task<bool> KeepsSettingsAsync(Guid projectId, Guid userId, RedixDbContext db) => (await UserAccess.OfAsync(userId, db)).In(projectId).Contains(Permission.ManageSettings);

    private static async Task<IResult> IfAcceptableAsync(Guid projectId, Guid supplierId, SupplierRequest request, RedixDbContext db, Func<Task<IResult>> change)
    {
        var vatNumber = request.VatNumberKept();
        var refusal = Supplier.RefusalOf(request.Name.Trim(), vatNumber, !request.NotVatRegistered, request.Mobile.Trim());
        if (refusal.Length == 0 && request.DefaultItemId != Guid.Empty && await ProjectStructure.ItemOfAsync(projectId, request.DefaultItemId, db) is null)
        {
            refusal = "UnknownItem";
        }
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var holder = await db.Suppliers.Include(row => row.DefaultItems).SingleOrDefaultAsync(row => row.ProjectId == projectId && row.VatNumber != "" && row.VatNumber == vatNumber && row.Id != supplierId);
        return holder is null ? await change() : Refusal.Of(StatusCodes.Status409Conflict, "VatNumberTaken", "existing", (await ViewsOfAsync([holder], db)).Single());
    }

    private static async Task<List<SupplierView>> ViewsOfAsync(List<Supplier> suppliers, RedixDbContext db)
    {
        var itemIds = suppliers.Select(supplier => supplier.DefaultItemId).ToList();
        var itemNames = await db.Items.Where(item => itemIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name);
        return suppliers.Select(supplier => SupplierView.Of(supplier, itemNames.GetValueOrDefault(supplier.DefaultItemId, ""))).ToList();
    }

    private static async Task<IResult> SetActiveAsync(Guid projectId, Guid supplierId, bool isActive, RedixDbContext db)
    {
        var supplier = await db.Suppliers.SingleOrDefaultAsync(row => row.Id == supplierId && row.ProjectId == projectId);
        if (supplier is null)
        {
            return Results.NotFound();
        }
        supplier.IsActive = isActive;
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>What the app sends to add or change a supplier.</summary>
/// <param name="Name">The supplier's name.</param>
public record SupplierRequest(string Name)
{
    /// <summary>Its VAT number; ignored when <see cref="NotVatRegistered"/>.</summary>
    public string VatNumber { get; init; } = "";

    /// <summary>True for a supplier with no VAT number (AC-06.2).</summary>
    public bool NotVatRegistered { get; init; }

    /// <summary>Its mobile; left out when none.</summary>
    public string Mobile { get; init; } = "";

    /// <summary>The item its expenses usually go to; left out when none.</summary>
    public Guid DefaultItemId { get; init; }

    /// <summary>The VAT number to keep: what was sent, trimmed, and empty for a supplier not VAT-registered.</summary>
    public string VatNumberKept() => NotVatRegistered ? "" : VatNumber.Trim();
}

/// <summary>A supplier as the app shows it.</summary>
/// <param name="Id">The supplier's id.</param>
/// <param name="Name">The supplier's name.</param>
/// <param name="VatNumber">Its VAT number; empty for a supplier not VAT-registered.</param>
/// <param name="Mobile">Its mobile; empty when none.</param>
/// <param name="DefaultItemId">The item its expenses usually go to; empty when none.</param>
/// <param name="DefaultItemName">That item's name; empty when none.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="IsActive">False once disabled.</param>
public record SupplierView(Guid Id, string Name, string VatNumber, string Mobile, string DefaultItemId, string DefaultItemName, SupplierStatus Status, bool IsActive)
{
    /// <summary>How many of the caller's purchases name it, in a search; 0 otherwise.</summary>
    public int TimesUsed { get; init; }

    /// <summary>The view of <paramref name="supplier"/>, whose default item is named <paramref name="defaultItemName"/>.</summary>
    public static SupplierView Of(Supplier supplier, string defaultItemName) =>
        new(supplier.Id, supplier.Name, supplier.VatNumber, supplier.Mobile, supplier.DefaultItemId == Guid.Empty ? "" : supplier.DefaultItemId.ToString(), defaultItemName, supplier.Status, supplier.IsActive);
}
