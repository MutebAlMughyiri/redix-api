using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
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
        app.MapProjectChanges(Permission.RecordEntries).MapPost("/expenses", Record);
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

    /// <summary>
    /// Records a purchase as a draft, or sent for approval, and answers its figures. An id already used answers <c>entryExists</c>, and an empty one <c>invalidEntryId</c>. What it names has to be on offer to the caller: otherwise <c>itemNotOffered</c>, <c>unknownUnit</c>, <c>unknownSubgroup</c>,
    /// <c>unknownSupplier</c> (BR-19), <c>unknownPaymentMethod</c> or <c>accountNotOffered</c> (BRS open question 4). What was typed answers <c>invalidInvoiceNumber</c>, <c>invalidProduct</c>, <c>invalidNotes</c>, <c>invalidUnitPrice</c>, <c>invalidQuantity</c>, <c>invalidAmount</c>, <c>invalidDiscount</c>,
    /// <c>invalidPaymentAmount</c> or <c>invalidBankFees</c>. A date outside the project's period or in the future answers <c>invalidInvoiceDate</c> or <c>invalidPaymentDate</c> (BR-12). A file of no such attachment answers <c>unknownAttachment</c>. A custom value answers <c>unknownColumn</c> or <c>invalidColumnValue</c>, naming the column as <c>columnKey</c>.
    /// Sending also needs the required columns, <c>requiredColumnMissing</c> with its <c>columnKey</c> (FR-41), and the invoice's file or why there is none, <c>invoiceRequired</c> (FR-09); a draft can wait for them. A reason too long answers <c>invalidWithoutInvoiceReason</c>, and a reason beside a file <c>invoiceAndReason</c>.
    /// A file already on another entry answers <c>invoiceAlreadyAttached</c> (BR-22). The same supplier's invoice number in any project answers <c>duplicateInvoice</c>, with the earlier entry as <c>earlier</c>, until the request says <c>confirmDuplicate</c> (UC-023, FR-17).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What was typed and chosen.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, ExpenseRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        if (request.Id == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidEntryId");
        }
        if (await db.Entries.AnyAsync(entry => entry.Id == request.Id))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "EntryExists");
        }

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

        var refusal = await RefusalOfWhatItNamesAsync(projectId, request, userId, held, db);
        refusal = refusal.Length > 0 ? refusal : request.RefusalOfTyped();
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 ? refusal : RefusalOfDates(request, project.StartDate, Entry.Today(time));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var vatRate = request.IsNotTaxable || request.IsWithoutInvoice() || supplier.VatNumber.Length == 0 ? 0 : project.VatRate;
        var figures = InvoiceFigures.Of(request.EnteredAmount(), request.EnteredAmountIncludesVat(), request.Discount, vatRate);
        refusal = ExpenseRequest.RefusalOfFigures(figures);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var (reason, columnKey) = RefusalOfColumns(request, item.Columns.Where(column => !column.IsHidden).ToList());
        if (reason.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, reason, "columnKey", columnKey);
        }
        if (request.Submit && request.AttachmentId == Guid.Empty && !request.IsWithoutInvoice())
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvoiceRequired");
        }

        if (request.AttachmentId != Guid.Empty && await IsFileOnAnEntryAsync(projectId, request.AttachmentId, db))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "InvoiceAlreadyAttached");
        }
        if (!request.ConfirmDuplicate && !request.IsWithoutInvoice())
        {
            var earlier = await EarlierEntryAsync(supplier, request.InvoiceNumber.Trim(), access, db);
            if (earlier is not null)
            {
                return Refusal.Of(StatusCodes.Status409Conflict, "DuplicateInvoice", "earlier", earlier);
            }
        }

        var (entry, details) = Build(request, projectId, subgroup.Id, figures, userId, time.GetUtcNow().UtcDateTime);
        db.Add(entry);
        details.ForEach(detail => db.Add(detail));
        db.Audit(entry, request.Submit ? AuditAction.Submit : AuditAction.Create, userId, entry.CreatedAt);
        details.ForEach(detail => db.Audit(detail, AuditAction.Create, userId, entry.CreatedAt));
        await db.SaveChangesAsync();
        return Results.Created($"/projects/{projectId}/expenses/{entry.Id}", new ExpenseView(entry.Id, entry.Status, figures.AmountBeforeDiscount, figures.Discount, figures.AmountAfterDiscount, figures.Vat, figures.Total, entry.Amount));
    }

    private static async Task<string> RefusalOfWhatItNamesAsync(Guid projectId, ExpenseRequest request, Guid userId, List<Permission> held, RedixDbContext db)
    {
        if (!await db.Set<Unit>().AnyAsync(unit => unit.Id == request.UnitId && unit.ProjectId == projectId))
        {
            return "UnknownUnit";
        }
        if (!await db.PaymentMethods.AnyAsync(method => method.Id == request.PaymentMethodId && method.ProjectId == projectId && method.IsActive))
        {
            return "UnknownPaymentMethod";
        }
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == request.AccountId && row.ProjectId == projectId);
        if (account is null || !account.IsOfferedTo(userId, held.Contains(Permission.ChargeAnyAccount)))
        {
            return "AccountNotOffered";
        }
        return request.AttachmentId == Guid.Empty || await db.Attachments.AnyAsync(attachment => attachment.Id == request.AttachmentId && attachment.ProjectId == projectId) ? "" : "UnknownAttachment";
    }

    private static async Task<bool> IsFileOnAnEntryAsync(Guid projectId, Guid attachmentId, RedixDbContext db)
    {
        var hash = await db.Attachments.Where(attachment => attachment.Id == attachmentId).Select(attachment => attachment.Sha256).SingleAsync();
        return await db.EntryAttachments.AnyAsync(link => db.Attachments.Any(attachment => attachment.Id == link.AttachmentId && attachment.ProjectId == projectId && attachment.Sha256 == hash));
    }

    /// <summary>The first entry with <paramref name="number"/> from <paramref name="supplier"/> or from whoever holds its VAT number in another project, one the caller can read if there is any.</summary>
    private static async Task<EarlierEntryView?> EarlierEntryAsync(Supplier supplier, string number, UserAccess access, RedixDbContext db)
    {
        var rows = await (
            from invoice in db.Invoices
            join purchase in db.PurchaseDetails on invoice.EntryId equals purchase.EntryId
            join named in db.Suppliers on purchase.SupplierId equals named.Id
            join project in db.Projects on named.ProjectId equals project.Id
            where invoice.Number == number && (named.Id == supplier.Id || (supplier.VatNumber != "" && named.VatNumber == supplier.VatNumber))
            select new { ProjectId = project.Id, ProjectName = project.Name, invoice.EntryId, invoice.InvoiceDate, invoice.Total }).ToListAsync();
        if (rows.Count == 0)
        {
            return null;
        }
        var visible = rows.OrderBy(row => row.EntryId).FirstOrDefault(row => access.In(row.ProjectId).Contains(Permission.ReadProject));
        return visible is null ? new EarlierEntryView(false, "", default, 0) : new EarlierEntryView(true, visible.ProjectName, visible.InvoiceDate, visible.Total);
    }

    private static string RefusalOfDates(ExpenseRequest request, DateOnly projectStart, DateOnly today)
    {
        if (!request.IsWithoutInvoice() && !Entry.IsAcceptableDate(request.InvoiceDate, projectStart, today))
        {
            return "InvalidInvoiceDate";
        }
        return (request.PaymentDate == default && !request.IsWithoutInvoice()) || Entry.IsAcceptableDate(request.PaymentDate, projectStart, today) ? "" : "InvalidPaymentDate";
    }

    private static (string Reason, string ColumnKey) RefusalOfColumns(ExpenseRequest request, List<ItemColumn> columns)
    {
        foreach (var (key, value) in request.CustomValuesKept())
        {
            var column = columns.SingleOrDefault(row => row.Key == key && row.IsCustom);
            if (column is null)
            {
                return ("UnknownColumn", key);
            }
            if (!column.Accepts(value))
            {
                return ("InvalidColumnValue", key);
            }
        }
        var missing = request.Submit ? columns.FirstOrDefault(column => column.IsRequired && !request.Answers(column)) : null;
        return missing is null ? ("", "") : ("RequiredColumnMissing", missing.Key);
    }

    private static (Entry Entry, List<object> Details) Build(ExpenseRequest request, Guid projectId, Guid subgroupId, InvoiceFigures figures, Guid userId, DateTime at)
    {
        var entry = new Entry
        {
            Id = request.Id,
            ProjectId = projectId,
            Kind = EntryKind.Purchase,
            Status = request.Submit ? EntryStatus.PendingApproval : EntryStatus.Draft,
            EntryDate = request.PaymentDate == default ? request.InvoiceDate : request.PaymentDate,
            Amount = request.PaymentAmount > 0 ? request.PaymentAmount : figures.Total,
            AccountId = request.AccountId,
            Notes = request.Notes.Trim(),
            CreatedByUserId = userId,
            CreatedAt = at,
        };
        List<object> details =
        [
            new ExpenseDetails
            {
                EntryId = entry.Id,
                ItemId = request.ItemId,
                SubgroupId = subgroupId,
                UnitId = request.UnitId,
                PaymentMethodId = request.PaymentMethodId,
                BankFees = request.BankFees,
                WithoutInvoiceReason = request.WithoutInvoiceReason.Trim(),
                CustomValues = JsonSerializer.Serialize(request.CustomValuesKept()),
            },
            new PurchaseDetails { EntryId = entry.Id, SupplierId = request.SupplierId, Product = request.Product.Trim(), UnitPrice = request.UnitPrice, Quantity = request.Quantity },
        ];
        if (!request.IsWithoutInvoice())
        {
            details.Add(new Invoice
            {
                EntryId = entry.Id,
                Number = request.InvoiceNumber.Trim(),
                InvoiceDate = request.InvoiceDate,
                AmountBeforeDiscount = figures.AmountBeforeDiscount,
                Discount = figures.Discount,
                AmountAfterDiscount = figures.AmountAfterDiscount,
                Vat = figures.Vat,
                Total = figures.Total,
                VatPeriodStart = Invoice.QuarterStartOf(request.InvoiceDate),
            });
        }
        if (request.AttachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entry.Id, AttachmentId = request.AttachmentId, Role = EntryAttachmentRole.Invoice });
        }
        return (entry, details);
    }
}

/// <summary>The entry an invoice number repeats (FR-17). One in a project the caller cannot read says only that it exists, because projects are separate (BR-33).</summary>
/// <param name="IsVisible">Whether the caller can read its project, and so whether the rest says anything.</param>
/// <param name="ProjectName">The project it is in.</param>
/// <param name="InvoiceDate">The invoice's date.</param>
/// <param name="Total">The invoice's total.</param>
public record EarlierEntryView(bool IsVisible, string ProjectName, DateOnly InvoiceDate, decimal Total);

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
