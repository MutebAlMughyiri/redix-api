using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>Records a purchase expense (UC-022, FR-08): the first time money enters the ledger. The figures are computed here from what was typed, once, and kept (BR-13).</summary>
public static class ExpensesEndpoints
{
    /// <summary>Maps the route that records an expense, for whoever holds <see cref="Permission.RecordEntries"/>, and the one that reads the exceptions list, for anyone who reads the project.</summary>
    public static void MapExpenses(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/expenses/exceptions", Exceptions).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/expenses/creditable", Creditable).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.RecordEntries);
        changes.MapPost("/expenses", Record);
        changes.MapPost("/expenses/{entryId:guid}", Edit);
    }

    /// <summary>The exceptions list (AC-09.1): the expenses sent or approved with no invoice, each saying why, in the order they were entered. UC-056 "Build the VAT statement for a period" adds what else it holds.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<ExceptionView>> Exceptions(Guid projectId, RedixDbContext db) => (
        from details in db.ExpenseDetails
        join entry in db.Entries on details.EntryId equals entry.Id
        join item in db.Items on details.ItemId equals item.Id
        where entry.ProjectId == projectId && details.WithoutInvoiceReason != "" && (entry.Status == EntryStatus.PendingApproval || entry.Status == EntryStatus.Approved)
        orderby entry.Id
        select new ExceptionView(entry.Id, entry.Status, entry.EntryDate, entry.Amount, item.Name, details.WithoutInvoiceReason)).ToListAsync();

    /// <summary>The invoices of <paramref name="supplierId"/> that a credit note can return (BR-18): sent or approved, and no credit note themselves, each with what is left of its value after the credit notes that already name it. One rejected no longer takes anything.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="supplierId">The supplier.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static Task<List<CreditableInvoiceView>> Creditable(Guid projectId, Guid supplierId, RedixDbContext db) => CreditableExceptAsync(projectId, supplierId, db, Guid.Empty);

    private static async Task<List<CreditableInvoiceView>> CreditableExceptAsync(Guid projectId, Guid supplierId, RedixDbContext db, Guid editing)
    {
        var invoices = await (
            from invoice in db.Invoices
            join purchase in db.PurchaseDetails on invoice.EntryId equals purchase.EntryId
            join entry in db.Entries on invoice.EntryId equals entry.Id
            where entry.ProjectId == projectId && purchase.SupplierId == supplierId && !invoice.IsCreditNote && (entry.Status == EntryStatus.PendingApproval || entry.Status == EntryStatus.Approved)
            select invoice).ToListAsync();
        var returned = await (
            from link in db.EntryLinks
            join note in db.Invoices on link.EntryId equals note.EntryId
            join entry in db.Entries on link.EntryId equals entry.Id
            where link.Type == EntryLinkType.CreditNoteOf && entry.ProjectId == projectId && entry.Status != EntryStatus.Rejected && entry.Id != editing
            group note.Total by link.LinkedEntryId into notes
            select new { EntryId = notes.Key, Total = notes.Sum() }).ToDictionaryAsync(row => row.EntryId, row => row.Total);
        return invoices.OrderBy(invoice => invoice.EntryId).Select(invoice => new CreditableInvoiceView(invoice.EntryId, invoice.Number, invoice.InvoiceDate, invoice.Total + returned.GetValueOrDefault(invoice.EntryId), InvoiceFigures.RateOf(invoice.AmountAfterDiscount, invoice.Vat))).ToList();
    }

    /// <summary>
    /// Records a purchase as a draft, or sent for approval, and answers its figures. The same id from whoever recorded it answers that entry, so a retried upload lands once (AC-16.3); from anyone else <c>entryExists</c>, and an empty one <c>invalidEntryId</c>. What it names has to be on offer to the caller: otherwise <c>itemNotOffered</c>, <c>unknownUnit</c>, <c>unknownSubgroup</c>,
    /// <c>unknownSupplier</c> (BR-19), <c>unknownPaymentMethod</c> or <c>accountNotOffered</c> (BRS open question 4). What was typed answers <c>invalidInvoiceNumber</c>, <c>invalidProduct</c>, <c>invalidNotes</c>, <c>invalidUnitPrice</c>, <c>invalidQuantity</c>, <c>invalidAmount</c>, <c>invalidDiscount</c>,
    /// <c>invalidPaymentAmount</c> or <c>invalidBankFees</c>. A split answers as <see cref="EntryRecording.RefusalOfSplitAsync"/> does. A date outside the project's period or in the future answers <c>invalidInvoiceDate</c> or <c>invalidPaymentDate</c> (BR-12). A file of no such attachment answers <c>unknownAttachment</c>. A custom value answers <c>unknownColumn</c> or <c>invalidColumnValue</c>, naming the column as <c>columnKey</c>.
    /// Sending also needs the required columns, <c>requiredColumnMissing</c> with its <c>columnKey</c> (FR-41), and the invoice's file or why there is none, <c>invoiceRequired</c> (FR-09); a draft can wait for them. A reason too long answers <c>invalidWithoutInvoiceReason</c>, and a reason beside a file <c>invoiceAndReason</c>.
    /// A negative amount is a deduction (FR-14): it has no invoice, so with no reason it answers <c>deductionNeedsReason</c>. A credit note (BR-18) with a reason answers <c>creditNoteNeedsInvoice</c>; one naming what it cannot return <c>unknownOriginalInvoice</c>; and one above what is left of that invoice <c>creditNoteAboveOriginal</c>.
    /// A file already on another entry answers <c>invoiceAlreadyAttached</c> (BR-22). The same supplier's invoice number in this project or another the caller can read answers <c>duplicateInvoice</c>, with the earlier entry as <c>earlier</c>, until the request says <c>confirmDuplicate</c> (UC-023, FR-17).
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
        var access = await UserAccess.OfAsync(userId, db);
        var held = access.In(projectId);
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
        var supplier = await db.Suppliers.Where(Supplier.OfferedTo(userId, held.Contains(Permission.ManageSettings))).SingleOrDefaultAsync(row => row.Id == request.SupplierId && row.ProjectId == projectId);
        if (supplier is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSupplier");
        }

        var refusal = await db.Set<Unit>().AnyAsync(unit => unit.Id == request.UnitId && unit.ProjectId == projectId) ? "" : "UnknownUnit";
        refusal = refusal.Length > 0 ? refusal : await EntryRecording.RefusalOfSplitAsync(projectId, request.UnitId, request.UnitShares, db);
        refusal = refusal.Length > 0 ? refusal : await EntryRecording.RefusalOfPaymentAsync(projectId, request, userId, held, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTyped();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 ? refusal : RefusalOfDates(request, project.StartDate, Entry.Today(time));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var vatRate = request.IsNotTaxable || request.IsWithoutInvoice() || supplier.VatNumber.Length == 0 ? 0 : project.VatRate;
        var original = request.OriginalEntryId == Guid.Empty ? CreditableInvoiceView.None(vatRate) : (await CreditableExceptAsync(projectId, request.SupplierId, db, editing)).SingleOrDefault(row => row.EntryId == request.OriginalEntryId);
        if (original is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownOriginalInvoice");
        }
        var figures = InvoiceFigures.Of(request.EnteredAmount(), request.EnteredAmountIncludesVat(), request.Discount, original.VatRate);
        refusal = request.RefusalOfFigures(figures);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if (figures.Total > original.Returnable)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "CreditNoteAboveOriginal");
        }

        var (reason, columnKey) = EntryRecording.RefusalOfColumns(request, item.Columns.Where(column => !column.IsHidden).ToList());
        if (reason.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, reason, "columnKey", columnKey);
        }
        if (request.Submit && request.AttachmentId == Guid.Empty && !request.IsWithoutInvoice())
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvoiceRequired");
        }

        if (request.AttachmentId != Guid.Empty && await EntryRecording.IsFileOnAnEntryAsync(projectId, request.AttachmentId, db, editing))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }
        if (!request.ConfirmDuplicate && !request.IsWithoutInvoice())
        {
            var earlier = await EntryRecording.EarlierEntryAsync(supplier, request.InvoiceNumber.Trim(), access, db, editing);
            if (earlier is not null)
            {
                return Refusal.Of(StatusCodes.Status409Conflict, "DuplicateInvoice", "earlier", earlier);
            }
        }

        var kept = request.IsCreditNote ? figures.Negated() : figures;
        var at = time.GetUtcNow().UtcDateTime;
        var (entry, details) = Build(request, projectId, subgroup.Id, kept, userId, at);
        if (existing is not null)
        {
            await EntryChanges.ReplaceAsync(existing, entry, details, userId, at, db);
            return Results.Ok(new ExpenseView(existing.Id, existing.Status, kept.AmountBeforeDiscount, kept.Discount, kept.AmountAfterDiscount, kept.Vat, kept.Total, existing.Amount));
        }
        await EntryRecording.SaveAsync(entry, details, userId, db);
        return Results.Created($"/projects/{projectId}/expenses/{entry.Id}", new ExpenseView(entry.Id, entry.Status, kept.AmountBeforeDiscount, kept.Discount, kept.AmountAfterDiscount, kept.Vat, kept.Total, entry.Amount));
    }

    private static string RefusalOfDates(ExpenseRequest request, DateOnly projectStart, DateOnly today)
    {
        if (!request.IsWithoutInvoice() && !Entry.IsAcceptableDate(request.InvoiceDate, projectStart, today))
        {
            return "InvalidInvoiceDate";
        }
        return (request.PaymentDate == default && !request.IsWithoutInvoice()) || Entry.IsAcceptableDate(request.PaymentDate, projectStart, today) ? "" : "InvalidPaymentDate";
    }

    private static (Entry Entry, List<object> Details) Build(ExpenseRequest request, Guid projectId, Guid subgroupId, InvoiceFigures kept, Guid userId, DateTime at)
    {
        var entryDate = request.PaymentDate == default ? request.InvoiceDate : request.PaymentDate;
        var entry = EntryRecording.EntryOf(request, projectId, EntryKind.Purchase, entryDate, request.PaidOf(kept), userId, at);
        List<object> details =
        [
            EntryRecording.ExpenseDetailsOf(request, request.ItemId, subgroupId, request.UnitId),
            new PurchaseDetails { EntryId = entry.Id, SupplierId = request.SupplierId, Product = request.Product.Trim(), UnitPrice = request.UnitPrice, Quantity = request.Quantity },
            .. request.UnitShares.Select(share => new UnitShare { EntryId = entry.Id, UnitId = share.UnitId, Percent = share.Percent }),
        ];
        if (!request.IsWithoutInvoice())
        {
            details.Add(EntryRecording.InvoiceOf(entry.Id, request.InvoiceNumber.Trim(), request.InvoiceDate, kept, request.IsCreditNote));
        }
        if (request.OriginalEntryId != Guid.Empty)
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

/// <summary>The entry an invoice number repeats, in a project the caller can read (AC-17.1).</summary>
/// <param name="ProjectName">The project it is in.</param>
/// <param name="InvoiceDate">The invoice's date.</param>
/// <param name="Total">The invoice's total.</param>
public record EarlierEntryView(string ProjectName, DateOnly InvoiceDate, decimal Total);

/// <summary>An invoice a credit note can return (BR-18), or none.</summary>
/// <param name="EntryId">The invoice's entry; empty for none.</param>
/// <param name="Number">The invoice's number.</param>
/// <param name="InvoiceDate">The invoice's date.</param>
/// <param name="Returnable">What is left of its total, with VAT, after the credit notes that already name it.</param>
/// <param name="VatRate">The rate it was computed at, which its credit note follows, as a fraction.</param>
public record CreditableInvoiceView(Guid EntryId, string Number, DateOnly InvoiceDate, decimal Returnable, decimal VatRate)
{
    /// <summary>No invoice: nothing limits what an expense not tied to one holds, computed at <paramref name="vatRate"/>.</summary>
    public static CreditableInvoiceView None(decimal vatRate) => new(Guid.Empty, "", default, decimal.MaxValue, vatRate);
}

/// <summary>One row of the exceptions list (AC-09.1).</summary>
/// <param name="EntryId">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="EntryDate">The day the money moved.</param>
/// <param name="Amount">What was paid.</param>
/// <param name="ItemName">The item it is recorded against.</param>
/// <param name="Reason">Why there is no invoice.</param>
public record ExceptionView(Guid EntryId, EntryStatus Status, DateOnly EntryDate, decimal Amount, string ItemName, string Reason);

/// <summary>A recorded expense's figures, as the API computed them.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="AmountBeforeDiscount">The amount before VAT and before the discount.</param>
/// <param name="Discount">The discount, before VAT.</param>
/// <param name="AmountAfterDiscount">The amount before VAT, after the discount.</param>
/// <param name="Vat">The VAT of this row.</param>
/// <param name="Total">The invoice's total.</param>
/// <param name="Amount">What was paid.</param>
public record ExpenseView(Guid Id, EntryStatus Status, decimal AmountBeforeDiscount, decimal Discount, decimal AmountAfterDiscount, decimal Vat, decimal Total, decimal Amount);
