using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Revenue;

/// <summary>Money that comes in other than capital (UC-047, FR-28), sales invoices (UC-048, FR-42), and the VAT statement (UC-056, FR-32). Finance enters these, and they count at once (BR-30).</summary>
public static class RevenueEndpoints
{
    /// <summary>Maps the routes that read, for anyone who reads the project, and those that enter, for whoever holds <see cref="Permission.MoveMoney"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapRevenue(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/revenue", List).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapGet("/projects/{projectId:guid}/vat-statement", Statement).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.MoveMoney);
        changes.MapPost("/revenue", Record);
        changes.MapPost("/sales-invoices", RecordSale);
        changes.MapPost("/entries/{entryId:guid}/vat-period", DeclareLater);
    }

    /// <summary>The VAT statement from <paramref name="from"/> to <paramref name="to"/> (AC-32.2). A last day before the first answers <c>invalidPeriod</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<IResult> Statement(Guid projectId, DateOnly from, DateOnly to, RedixDbContext db) =>
        to < from ? Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPeriod") : Results.Ok(await VatStatement.OfAsync(projectId, from, to, db));

    /// <summary>Everything that came in other than capital, newest first. A VAT refund also says what its quarter's statement expected, and the difference (AC-28.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database the ledger lives in.</param>
    public static async Task<List<RevenueRowView>> List(Guid projectId, RedixDbContext db)
    {
        var rows = await (
            from details in db.Set<RevenueDetails>()
            join entry in db.Entries on details.EntryId equals entry.Id
            join item in db.Items on details.ItemId equals item.Id
            where entry.ProjectId == projectId && entry.Status == EntryStatus.Approved
            orderby entry.EntryDate descending, entry.Id descending
            select new { entry, details, item }).ToListAsync();
        var entries = rows.Select(row => row.entry).ToList();
        var ids = entries.Select(entry => entry.Id).ToList();
        var facts = await EntryFacts.OfAsync(projectId, entries, db);
        var invoices = await db.Set<SalesInvoice>().Where(sale => ids.Contains(sale.EntryId)).ToDictionaryAsync(sale => sale.EntryId);
        var reversed = (await db.EntryLinks.Where(link => link.Type == EntryLinkType.Reversal && (ids.Contains(link.EntryId) || ids.Contains(link.LinkedEntryId))).ToListAsync()).SelectMany(link => new[] { link.EntryId, link.LinkedEntryId }).ToHashSet();
        var views = new List<RevenueRowView>();
        foreach (var row in rows)
        {
            var isRefund = row.item.Type == ItemType.VatRefund;
            var expected = isRefund ? -await VatStatement.NetDueOfAsync(projectId, row.details.PeriodStart, db) : 0;
            var invoice = invoices.GetValueOrDefault(row.entry.Id);
            views.Add(new RevenueRowView(row.entry.Id, row.item.Type, row.item.Id, row.item.Name, row.entry.EntryDate, row.entry.Amount, row.details.PeriodStart, row.details.PeriodEnd, facts.AccountNameOf(row.entry), row.entry.Notes, facts.AttachmentIdOf(row.entry), facts.ContentTypeOf(row.entry), reversed.Contains(row.entry.Id), expected, isRefund ? row.entry.Amount - expected : 0, invoice?.BuyerName ?? "", invoice?.Number ?? ""));
        }
        return views;
    }

    /// <summary>
    /// Records revenue on one of the project's revenue items, approved at once; the item's type says what it is (FR-28, FR-38). Anything but an active revenue item of the project, or a
    /// subgroup not its own, answers <c>unknownItem</c>. A VAT refund is for a quarter: a start that is no quarter's first day answers <c>invalidPeriod</c>. Anything else is for its own day
    /// unless a period is given, and a period ending before it starts also answers <c>invalidPeriod</c>. Otherwise it answers as <see cref="DirectEntries.RefusalOfAsync"/> does.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The revenue.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, RevenueRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var item = await RevenueItemOfAsync(projectId, request.ItemId, db);
        var subgroupId = SubgroupOf(item, request.SubgroupId);
        if (subgroupId == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownItem");
        }
        var start = request.PeriodStart == default ? request.Date : request.PeriodStart;
        var isRefund = item.Type == ItemType.VatRefund;
        var end = isRefund ? VatStatement.NextQuarter(start).AddDays(-1) : request.PeriodEnd == default ? start : request.PeriodEnd;
        if ((isRefund && start != Invoice.QuarterStartOf(start)) || end < start)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPeriod");
        }
        var refusal = await DirectEntries.RefusalOfAsync(projectId, request.Date, request.Amount, request.Description, [request.AccountId], request.AttachmentId, time, db);
        if (refusal.Length > 0)
        {
            return Refusal.Of(refusal == "InvoiceAlreadyAttached" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var entry = DirectEntries.New(projectId, EntryKind.Revenue, request.Date, request.Amount, request.AccountId, request.Description.Trim(), userId, at);
        DirectEntries.Add(entry, DetailsOf(entry.Id, item.Id, subgroupId, start, end, request.AttachmentId), userId, at, db);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entry.Id, entry.Status, ""));
    }

    /// <summary>
    /// Records a sales invoice, which reaches the sales section of its quarter (AC-42.1). Naming a sale already recorded as revenue puts the invoice on it, so its amount counts once; otherwise it records the sale as well, for what the buyer paid, on the sale item named or else the project's first, and a project without one, or a name that is no sale item of it, answers <c>unknownItem</c>. A sale not found, or one with an invoice, answers <c>unknownSale</c>.
    /// What was typed answers <c>invalidBuyerName</c>, <c>invalidVatNumber</c>, <c>invalidInvoiceNumber</c>, <c>invalidInvoiceDate</c>, <c>invalidAmount</c> or <c>invalidDiscount</c>; a number the project used <c>duplicateInvoice</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The invoice.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> RecordSale(Guid projectId, SalesInvoiceRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        var refusal = RefusalOfTyped(request, project.StartDate, Entry.Today(time));
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        var number = request.Number.Trim();
        var used = await (from sale in db.Set<SalesInvoice>() join entry in db.Entries on sale.EntryId equals entry.Id where entry.ProjectId == projectId && sale.Number == number select sale.EntryId).AnyAsync();
        if (used)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "DuplicateInvoice");
        }
        var rate = request.TaxType == SaleTaxType.Vat ? project.VatRate : SalesInvoice.RealEstateTransactionRate;
        var figures = InvoiceFigures.Of(request.Amount, false, request.Discount, rate);
        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var entryId = request.SaleEntryId;
        if (entryId != Guid.Empty)
        {
            var isOpen = await db.Set<RevenueDetails>().AnyAsync(details => details.EntryId == entryId && db.Items.Any(item => item.Id == details.ItemId && item.Type == ItemType.Sale) && db.Entries.Any(entry => entry.Id == entryId && entry.ProjectId == projectId) && !db.Set<SalesInvoice>().Any(sale => sale.EntryId == entryId));
            if (!isOpen)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSale");
            }
        }
        else
        {
            var item = await (
                from candidate in db.Items
                join category in db.Categories on candidate.CategoryId equals category.Id
                where category.ProjectId == projectId && candidate.IsActive && candidate.Type == ItemType.Sale && (request.ItemId == Guid.Empty || candidate.Id == request.ItemId)
                orderby category.SortOrder, candidate.SortOrder
                select candidate).Include(candidate => candidate.Subgroups).FirstOrDefaultAsync() ?? Item.None;
            if (item.Id == Guid.Empty)
            {
                return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownItem");
            }
            var received = request.TaxType == SaleTaxType.Vat ? figures.Total : figures.AmountAfterDiscount;
            refusal = await DirectEntries.RefusalOfAsync(projectId, request.InvoiceDate, received, request.Description, [request.AccountId], request.AttachmentId, time, db);
            if (refusal.Length > 0)
            {
                return Refusal.Of(refusal == "InvoiceAlreadyAttached" ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest, refusal);
            }
            var entry = DirectEntries.New(projectId, EntryKind.Revenue, request.InvoiceDate, received, request.AccountId, request.Description.Trim(), userId, at);
            DirectEntries.Add(entry, DetailsOf(entry.Id, item.Id, SubgroupOf(item, Guid.Empty), request.InvoiceDate, request.InvoiceDate, request.AttachmentId), userId, at, db);
            entryId = entry.Id;
        }
        var invoice = new SalesInvoice
        {
            EntryId = entryId,
            BuyerName = request.BuyerName.Trim(),
            BuyerVatNumber = request.BuyerVatNumber.Trim(),
            Number = number,
            InvoiceDate = request.InvoiceDate,
            AmountBeforeDiscount = figures.AmountBeforeDiscount,
            Discount = figures.Discount,
            AmountAfterDiscount = figures.AmountAfterDiscount,
            Tax = figures.Vat,
            Total = figures.Total,
            TaxType = request.TaxType,
            VatPeriodStart = Invoice.QuarterStartOf(request.InvoiceDate),
        };
        db.Add(invoice);
        db.Audit(invoice, AuditAction.Create, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok(new EntryStatusView(entryId, EntryStatus.Approved, ""));
    }

    /// <summary>Declares an invoice in a later quarter, as Finance may decide (BR-17); the statement then lists it as an exception. An entry with no invoice is not found; a start that is no quarter's first day, or that comes before the invoice's own quarter, answers <c>invalidPeriod</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">The quarter.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> DeclareLater(Guid projectId, Guid entryId, VatPeriodRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var invoice = await db.Invoices.SingleOrDefaultAsync(row => row.EntryId == entryId && db.Entries.Any(entry => entry.Id == entryId && entry.ProjectId == projectId));
        if (invoice is null)
        {
            return Results.NotFound();
        }
        if (request.PeriodStart != Invoice.QuarterStartOf(request.PeriodStart) || request.PeriodStart < Invoice.QuarterStartOf(invoice.InvoiceDate))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidPeriod");
        }
        invoice.VatPeriodStart = request.PeriodStart;
        db.Audit(invoice, AuditAction.Update, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>The active revenue item <paramref name="itemId"/> of the project, with its subgroups; <see cref="Item.None"/> when there is no such item.</summary>
    private static async Task<Item> RevenueItemOfAsync(Guid projectId, Guid itemId, RedixDbContext db) =>
        await db.Items.Include(item => item.Subgroups).SingleOrDefaultAsync(item => item.Id == itemId && item.IsActive && item.Type >= ItemType.Sale && db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId)) ?? Item.None;

    /// <summary>The subgroup <paramref name="subgroupId"/> of <paramref name="item"/>, or its general one when none is named; empty when the item has no such subgroup.</summary>
    private static Guid SubgroupOf(Item item, Guid subgroupId) =>
        item.Subgroups.Where(subgroup => subgroupId == Guid.Empty ? subgroup.Name == Subgroup.GeneralName : subgroup.Id == subgroupId).Select(subgroup => subgroup.Id).FirstOrDefault();

    private static List<object> DetailsOf(Guid entryId, Guid itemId, Guid subgroupId, DateOnly start, DateOnly end, Guid attachmentId)
    {
        List<object> details = [new RevenueDetails { EntryId = entryId, ItemId = itemId, SubgroupId = subgroupId, PeriodStart = start, PeriodEnd = end }];
        if (attachmentId != Guid.Empty)
        {
            details.Add(new EntryAttachment { EntryId = entryId, AttachmentId = attachmentId, Role = EntryAttachmentRole.Other });
        }
        return details;
    }

    private static string RefusalOfTyped(SalesInvoiceRequest request, DateOnly projectStart, DateOnly today)
    {
        if (request.BuyerName.Trim().Length is 0 or > SalesInvoice.BuyerNameMaxLength)
        {
            return "InvalidBuyerName";
        }
        var vatNumber = request.BuyerVatNumber.Trim();
        if (vatNumber.Length > 0 && !Supplier.IsValidVatNumber(vatNumber))
        {
            return "InvalidVatNumber";
        }
        if (request.Number.Trim().Length is 0 or > Invoice.NumberMaxLength)
        {
            return "InvalidInvoiceNumber";
        }
        if (!Entry.IsAcceptableDate(request.InvoiceDate, projectStart, today))
        {
            return "InvalidInvoiceDate";
        }
        if (request.Amount <= 0 || !Money.IsAcceptable(request.Amount))
        {
            return "InvalidAmount";
        }
        return request.Discount < 0 || request.Discount > request.Amount || !Money.IsAcceptable(request.Discount) ? "InvalidDiscount" : "";
    }
}

/// <summary>Revenue to record.</summary>
/// <param name="ItemId">The revenue item it came in on, whose type says what it is.</param>
/// <param name="Date">The day it came in.</param>
/// <param name="Amount">How much, above zero.</param>
/// <param name="AccountId">The account it reached.</param>
/// <param name="Description">What was said about it; may be empty.</param>
public record RevenueRequest(Guid ItemId, DateOnly Date, decimal Amount, Guid AccountId, string Description)
{
    /// <summary>The item's subgroup; left out, its general one.</summary>
    public Guid SubgroupId { get; init; }

    /// <summary>The first day of its period: a quarter's for a VAT refund; left out, its own day.</summary>
    public DateOnly PeriodStart { get; init; }

    /// <summary>The last day of its period; left out, computed.</summary>
    public DateOnly PeriodEnd { get; init; }

    /// <summary>A file showing it; left out when none.</summary>
    public Guid AttachmentId { get; init; }
}

/// <summary>A sales invoice to record.</summary>
/// <param name="BuyerName">The buyer.</param>
/// <param name="BuyerVatNumber">The buyer's VAT number; may be empty.</param>
/// <param name="Number">Its number.</param>
/// <param name="InvoiceDate">Its date.</param>
/// <param name="Amount">The amount before tax and before the discount.</param>
/// <param name="Discount">The discount; may be zero.</param>
/// <param name="TaxType">Which tax it carries.</param>
/// <param name="AccountId">The account the money reached; unused when it names a sale.</param>
public record SalesInvoiceRequest(string BuyerName, string BuyerVatNumber, string Number, DateOnly InvoiceDate, decimal Amount, decimal Discount, SaleTaxType TaxType, Guid AccountId)
{
    /// <summary>A sale already recorded as revenue; left out to record it now.</summary>
    public Guid SaleEntryId { get; init; }

    /// <summary>The sale item a sale recorded now comes in on; left out, the project's first sale item; unused when it names a sale.</summary>
    public Guid ItemId { get; init; }

    /// <summary>A file showing it; left out when none.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>What was said about it.</summary>
    public string Description { get; init; } = "";
}

/// <summary>A later quarter to declare an invoice in.</summary>
/// <param name="PeriodStart">The quarter's first day.</param>
public record VatPeriodRequest(DateOnly PeriodStart);

/// <summary>Revenue as the app shows it.</summary>
/// <param name="Id">The entry.</param>
/// <param name="Type">What it is: its item's type.</param>
/// <param name="ItemId">The revenue item it came in on.</param>
/// <param name="ItemName">That item's name.</param>
/// <param name="Date">The day it came in.</param>
/// <param name="Amount">How much.</param>
/// <param name="PeriodStart">The first day of its period.</param>
/// <param name="PeriodEnd">The last day of its period.</param>
/// <param name="AccountName">The account it reached.</param>
/// <param name="Description">What was said about it.</param>
/// <param name="AttachmentId">Its file; empty when none.</param>
/// <param name="ContentType">What the file is; empty when none.</param>
/// <param name="IsReversed">True when it was reversed, or is itself a reversal.</param>
/// <param name="StatementRefund">What the statement of a VAT refund's quarter expected back; zero for anything else.</param>
/// <param name="RefundDifference">What came back, less that; zero for anything else.</param>
/// <param name="BuyerName">The buyer on its sales invoice; empty when it has none.</param>
/// <param name="InvoiceNumber">The number of its sales invoice; empty when it has none.</param>
public record RevenueRowView(Guid Id, ItemType Type, Guid ItemId, string ItemName, DateOnly Date, decimal Amount, DateOnly PeriodStart, DateOnly PeriodEnd, string AccountName, string Description, string AttachmentId, string ContentType, bool IsReversed, decimal StatementRefund, decimal RefundDifference, string BuyerName, string InvoiceNumber);
