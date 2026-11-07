using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>Records a purchase expense (UC-022, FR-08): a total the project spent, on an item, from a supplier, on a day, paid one way from one account, with its file when there is one (redix-api #96).</summary>
public static class ExpensesEndpoints
{
    /// <summary>Maps the routes that record and change an expense, for whoever holds <see cref="Permission.RecordEntries"/>, and the one that reads what a credit note can return, for anyone who reads the project.</summary>
    public static void MapExpenses(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/expenses/creditable", Creditable).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.RecordEntries);
        changes.MapPost("/expenses", Record);
        changes.MapPost("/expenses/{entryId:guid}", Edit);
    }

    /// <summary>The bills of <paramref name="supplierId"/> that a credit note can return (D13): sent or approved, and neither a credit note nor a deduction themselves, each with what is left of its total after the credit notes that already name it. One rejected no longer takes anything.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<CreditableInvoiceView>> Creditable(Guid projectId, Guid supplierId, RedixDbContext db) => CreditableExceptAsync(projectId, supplierId, db, Guid.Empty);

    private static async Task<List<CreditableInvoiceView>> CreditableExceptAsync(Guid projectId, Guid supplierId, RedixDbContext db, Guid editing)
    {
        var bills = await (
            from purchase in db.PurchaseDetails
            join entry in db.Entries on purchase.EntryId equals entry.Id
            where entry.ProjectId == projectId && purchase.SupplierId == supplierId && entry.Amount > 0 && (entry.Status == EntryStatus.PendingApproval || entry.Status == EntryStatus.Approved)
            select new { entry.Id, entry.EntryDate, entry.Amount, Number = db.Invoices.Where(invoice => invoice.EntryId == entry.Id).Select(invoice => invoice.Number).FirstOrDefault() ?? "" }).ToListAsync();
        var returned = await (
            from link in db.EntryLinks
            join note in db.Entries on link.EntryId equals note.Id
            where link.Type == EntryLinkType.CreditNoteOf && note.ProjectId == projectId && note.Status != EntryStatus.Rejected && note.Id != editing
            group note.Amount by link.LinkedEntryId into notes
            select new { EntryId = notes.Key, Total = notes.Sum() }).ToDictionaryAsync(row => row.EntryId, row => row.Total);
        return bills.OrderBy(bill => bill.Id).Select(bill => new CreditableInvoiceView(bill.Id, bill.Number, bill.EntryDate, bill.Amount + returned.GetValueOrDefault(bill.Id))).ToList();
    }

    /// <summary>
    /// Records a purchase as a draft, or sent for approval, and answers its total. The same id from whoever recorded it answers that entry, so a retried upload lands once (AC-16.3); from anyone else <c>entryExists</c>, and an empty one <c>invalidEntryId</c>. What it names has to be on offer to the caller: otherwise <c>itemNotOffered</c>, <c>unknownSubgroup</c>,
    /// <c>unknownSupplier</c> (BR-19), <c>unknownPaymentMethod</c> or <c>accountNotOffered</c> (BRS open question 4). What was typed answers <c>invalidProduct</c>, <c>invalidNotes</c>, <c>invalidUnitPrice</c>, <c>invalidQuantity</c> or <c>invalidAmount</c>, and a date outside the project's period or in the future <c>invalidPaymentDate</c> (BR-12).
    /// A file of no such attachment answers <c>unknownAttachment</c>, and one already on another entry <c>invoiceAlreadyAttached</c> (BR-22). A custom value answers <c>unknownColumn</c> or <c>invalidColumnValue</c>, naming the column as <c>columnKey</c>; sending also needs the required columns, <c>requiredColumnMissing</c> (FR-41). The file is optional (D3).
    /// A negative total is a deduction (FR-14), which says why in its notes or answers <c>deductionNeedsNotes</c> (D14). A credit note needs the bill it returns, <c>creditNoteNeedsOriginal</c>; one naming what it cannot return answers <c>unknownOriginalInvoice</c>, and one above what is left of that bill <c>creditNoteAboveOriginal</c> (D13).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What was typed and chosen.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, ExpenseRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var repeated = await EntryRecording.RepeatOfAsync(request.Id, projectId, AccessTokens.UserIdOf(principal), db);
        if (repeated is not null)
        {
            return Results.Ok(repeated);
        }

        var (status, idRefusal) = await EntryRecording.RefusalOfIdAsync(request.Id, db);
        return idRefusal.Length > 0 ? Refusal.Of(status, idRefusal) : await SaveAsync(projectId, request, principal, db, time, null);
    }

    /// <summary>Changes a purchase before it is approved (UC-033), through every check a new one passes. What is approved answers <c>entryApproved</c> (BR-11, AC-16.2); someone else's <c>notYourEntry</c>; one waiting for a decision, to whoever may not change it then, <c>entryWaitingForDecision</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">What it holds now.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Edit(Guid projectId, Guid entryId, ExpenseRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await db.Entries.SingleOrDefaultAsync(row => row.Id == entryId && row.ProjectId == projectId && row.Kind == EntryKind.Purchase);
        if (entry is null)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var (status, refusal) = EntryChanges.RefusalOf(entry, userId, (await UserAccess.OfAsync(userId, db)).In(projectId));
        return refusal.Length > 0 ? Refusal.Of(status, refusal) : await SaveAsync(projectId, request with { Id = entryId }, principal, db, time, entry);
    }

    private static async Task<IResult> SaveAsync(Guid projectId, ExpenseRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, Entry? existing)
    {
        var editing = existing?.Id ?? Guid.Empty;
        var userId = AccessTokens.UserIdOf(principal);
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var item = await ProjectStructure.OfferedItemAsync(projectId, request.ItemId, ItemType.Purchases, db);
        if (item is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "ItemNotOffered");
        }
        var subgroup = item.Subgroups.SingleOrDefault(row => request.SubgroupId == Guid.Empty ? row.Name == Subgroup.GeneralName : row.Id == request.SubgroupId);
        if (subgroup is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSubgroup");
        }
        if (!await db.Suppliers.Where(Supplier.OfferedTo(userId, held.Contains(Permission.ManageSettings))).AnyAsync(row => row.Id == request.SupplierId && row.ProjectId == projectId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSupplier");
        }

        var refusal = await EntryRecording.RefusalOfPaymentAsync(projectId, request, userId, held, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTyped();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 || Entry.IsAcceptableDate(request.PaymentDate, project.StartDate, Entry.Today(time)) ? refusal : "InvalidPaymentDate";
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if (request.IsCreditNote)
        {
            var original = (await CreditableExceptAsync(projectId, request.SupplierId, db, editing)).SingleOrDefault(row => row.EntryId == request.OriginalEntryId);
            if (original is null)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownOriginalInvoice");
            }
            if (request.Total() > original.Returnable)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "CreditNoteAboveOriginal");
            }
        }

        var (reason, columnKey) = EntryRecording.RefusalOfColumns(request, item.Columns.Where(column => !column.IsHidden).ToList());
        if (reason.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, reason, "columnKey", columnKey);
        }
        if (request.AttachmentId != Guid.Empty && await EntryRecording.IsFileOnAnEntryAsync(projectId, request.AttachmentId, db, editing))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }

        var at = time.GetUtcNow().UtcDateTime;
        var (entry, details) = Build(request, projectId, subgroup.Id, userId, at);
        if (existing is not null)
        {
            await EntryChanges.ReplaceAsync(existing, entry, details, userId, at, db);
            return Results.Ok(new ExpenseView(existing.Id, existing.Status, existing.Amount));
        }
        await EntryRecording.SaveAsync(entry, details, userId, db);
        return Results.Created($"/projects/{projectId}/expenses/{entry.Id}", new ExpenseView(entry.Id, entry.Status, entry.Amount));
    }

    private static (Entry Entry, List<object> Details) Build(ExpenseRequest request, Guid projectId, Guid subgroupId, Guid userId, DateTime at)
    {
        var entry = EntryRecording.EntryOf(request, projectId, EntryKind.Purchase, request.PaymentDate, request.KeptTotal(), userId, at);
        List<object> details =
        [
            EntryRecording.ExpenseDetailsOf(request, request.ItemId, subgroupId),
            new PurchaseDetails { EntryId = entry.Id, SupplierId = request.SupplierId, Product = request.Product.Trim(), UnitPrice = request.UnitPrice, Quantity = request.Quantity },
        ];
        if (request.IsCreditNote)
        {
            details.Add(new EntryLink { EntryId = entry.Id, Type = EntryLinkType.CreditNoteOf, LinkedEntryId = request.OriginalEntryId });
        }
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entry.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.Invoice });
        }
        return (entry, details);
    }
}

/// <summary>A bill a credit note can return (D13).</summary>
/// <param name="EntryId">The bill's entry.</param>
/// <param name="Number">The number of an older bill that had one; empty for the rest.</param>
/// <param name="Date">The bill's date.</param>
/// <param name="Returnable">What is left of its total after the credit notes that already name it.</param>
public record CreditableInvoiceView(Guid EntryId, string Number, DateOnly Date, decimal Returnable);

/// <summary>A recorded expense, as the API kept it.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Amount">Its total: negative for a credit note or a deduction.</param>
public record ExpenseView(Guid Id, EntryStatus Status, decimal Amount);
