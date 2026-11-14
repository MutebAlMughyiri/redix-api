using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Notifications;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>
/// Recording any entry, whatever its table (redix-api #110): one route records it and one changes it before approval. It is recorded once per id, goes only into a table the
/// caller is offered, passes the checks every entry shares by its item's side, then its table's own, and reaches the ledger with its history row, its decision and the deciders told.
/// </summary>
public static class EntryRecording
{
    /// <summary>Maps the route that records an entry and the one that changes it. Who may enter which table is the table's to say (<see cref="EntryTable.OfferedAsync"/>), so the routes ask only that the caller reads the project.</summary>
    public static void MapEntryRecording(this WebApplication app)
    {
        var changes = app.MapProjectChanges(Permission.ReadProject);
        changes.MapPost("/entries", Record);
        changes.MapPut("/entries/{entryId:guid}", Change);
    }

    /// <summary>
    /// Records <paramref name="request"/> as a draft, or sent for approval. The same id from whoever recorded it answers that entry, so a retried upload lands once (AC-16.3, UC-029);
    /// from anyone else <c>entryExists</c>, and an empty one <c>invalidEntryId</c>. Otherwise it answers as <see cref="SaveAsync"/>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">What was typed and chosen.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Record(Guid projectId, EntryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var repeated = await db.Entries.Where(entry => entry.Id == request.Id && entry.ProjectId == projectId && entry.CreatedByUserId == userId)
            .Select(entry => new EntryStatusView(entry.Id, entry.Number, entry.Status, "")).ToListAsync();
        if (repeated.Count > 0)
        {
            return Results.Ok(repeated[0]);
        }
        if (request.Id == Guid.Empty)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidEntryId");
        }
        return await db.Entries.AnyAsync(entry => entry.Id == request.Id) ? Refusal.Of(StatusCodes.Status409Conflict, "EntryExists") : await SaveAsync(projectId, request, userId, db, time, Entry.None);
    }

    /// <summary>
    /// Changes the entry <paramref name="entryId"/> before it is approved (UC-033), through every check a new one passes; it may move to another table, of any kind. One not in the
    /// project is not found; otherwise it answers the refusals of <see cref="EntryChanges.RefusalOf"/>, then as <see cref="SaveAsync"/>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="entryId">The entry.</param>
    /// <param name="request">What it holds now.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the ledger lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Change(Guid projectId, Guid entryId, EntryRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var entry = await Entry.OfProjectAsync(projectId, entryId, db);
        if (entry == Entry.None)
        {
            return Results.NotFound();
        }
        var userId = AccessTokens.UserIdOf(principal);
        var (status, refusal) = EntryChanges.RefusalOf(entry, userId, (await UserAccess.OfAsync(userId, db)).In(projectId));
        return refusal.Length > 0 ? Refusal.Of(status, refusal) : await SaveAsync(projectId, request with { Id = entryId }, userId, db, time, entry);
    }

    /// <summary>
    /// Keeps <paramref name="request"/>, as a new entry or in place of <paramref name="existing"/>. A table the caller is not offered answers <c>unknownTable</c>; otherwise it answers
    /// the refusals of <see cref="RecordAsync"/>, naming the column as <c>columnKey</c>. A new entry answers 201 with what was kept, a changed one 200.
    /// </summary>
    private static async Task<IResult> SaveAsync(Guid projectId, EntryRequest request, Guid userId, RedixDbContext db, TimeProvider time, Entry existing)
    {
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var table = (await EntryTable.OfferedAsync(projectId, held, db)).SingleOrDefault(row => row.Item.Id == request.ItemId && row.Id == request.TableId);
        if (table is null)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownTable");
        }
        var (status, code, columnKey, entry) = await RecordAsync(projectId, request, table, userId, held, existing, tellDeciders: true, db, time);
        if (code.Length > 0)
        {
            return Refusal.Of(status, code, "columnKey", columnKey);
        }
        var answer = await table.AnswerOfAsync(entry, request, db);
        return existing == Entry.None ? Results.Created($"/projects/{projectId}/entries/{entry.Id}", answer) : Results.Ok(answer);
    }

    /// <summary>
    /// Records <paramref name="request"/> into <paramref name="table"/>, one the caller holding <paramref name="held"/> is offered, as a new entry or in place of
    /// <paramref name="existing"/>: the one way into the ledger, for Add entry and a project file's load alike (BR-43). An expense needs a payment method, <c>unknownPaymentMethod</c>;
    /// the account is one the caller may use, <c>accountNotOffered</c> (BRS open question 4); a file of no such attachment answers <c>unknownAttachment</c>. A description too long
    /// answers <c>invalidDescription</c>; a negative total is a deduction, only on an expense, which says why in its description, <c>deductionNeedsDescription</c> (FR-14, D14); then
    /// the table's own typing; an amount of zero, with more than two decimals, beyond <see cref="Money.Largest"/> or below zero on the revenue side answers <c>invalidAmount</c>, and
    /// a date outside the project's period or in the future <c>invalidDate</c> (BR-12). Then the table's own checks; a column's value answers <c>unknownColumn</c> or
    /// <c>invalidColumnValue</c>, naming the column, and sending needs the required columns, <c>requiredColumnMissing</c> (FR-41); a file already on another entry answers
    /// <c>invoiceAlreadyAttached</c> (BR-22). It answers the refusal's status, code and column, or the entry kept; a new one sent for approval tells the deciders when
    /// <paramref name="tellDeciders"/>, which a project file's load does not (redix-api #132 D7).
    /// </summary>
    public static async Task<(int Status, string Code, string ColumnKey, Entry Entry)> RecordAsync(Guid projectId, EntryRequest request, EntryTable table, Guid userId, List<Permission> held, Entry existing, bool tellDeciders, RedixDbContext db, TimeProvider time)
    {
        var (status, code, columnKey) = await RefusalOfAsync(projectId, request, table, userId, held, existing.Id, db, time);
        if (code.Length > 0)
        {
            return (status, code, columnKey, Entry.None);
        }
        var at = time.GetUtcNow().UtcDateTime;
        return (0, "", "", await KeepAsync(EntryOf(request, projectId, table, userId, at), [.. table.DetailsOf(request), .. FileOf(request, table.FileRole)], existing, userId, tellDeciders, db));
    }

    private static async Task<(int Status, string Code, string ColumnKey)> RefusalOfAsync(Guid projectId, EntryRequest request, EntryTable table, Guid userId, List<Permission> held, Guid editing, RedixDbContext db, TimeProvider time)
    {
        var refusal = await RefusalOfPaymentAsync(projectId, request, table, userId, held, db);
        refusal = refusal.Length > 0 ? refusal : RefusalOfTyped(request, table);
        var project = await db.Projects.SingleAsync(row => row.Id == projectId);
        refusal = refusal.Length > 0 || Entry.IsAcceptableDate(request.Date, project.StartDate, Entry.Today(time)) ? refusal : "InvalidDate";
        refusal = refusal.Length > 0 ? refusal : await table.RefusalOfAsync(projectId, request, userId, held, editing, db);
        if (refusal.Length > 0)
        {
            return (StatusCodes.Status400BadRequest, refusal, "");
        }
        var (reason, columnKey) = RefusalOfColumns(request, table.Columns);
        if (reason.Length > 0)
        {
            return (StatusCodes.Status400BadRequest, reason, columnKey);
        }
        return request.AttachmentId != Guid.Empty && await IsFileOnAnEntryAsync(projectId, request.AttachmentId, db, editing) ? (StatusCodes.Status409Conflict, "InvoiceAlreadyAttached", "") : (0, "", "");
    }

    private static async Task<string> RefusalOfPaymentAsync(Guid projectId, EntryRequest request, EntryTable table, Guid userId, List<Permission> held, RedixDbContext db)
    {
        if (!table.IsRevenue && !await db.PaymentMethods.AnyAsync(method => method.Id == request.PaymentMethodId && method.ProjectId == projectId && method.IsActive))
        {
            return "UnknownPaymentMethod";
        }
        var account = await db.Accounts.Include(row => row.Custodians).SingleOrDefaultAsync(row => row.Id == request.AccountId && row.ProjectId == projectId);
        if (account is null || !account.IsOfferedTo(userId, table.IsRevenue || held.Contains(Permission.ChargeAnyAccount)))
        {
            return "AccountNotOffered";
        }
        return request.AttachmentId == Guid.Empty || await db.Attachments.AnyAsync(attachment => attachment.Id == request.AttachmentId && attachment.ProjectId == projectId) ? "" : "UnknownAttachment";
    }

    private static string RefusalOfTyped(EntryRequest request, EntryTable table)
    {
        var description = request.Description.Trim();
        if (description.Length > Entry.DescriptionMaxLength)
        {
            return "InvalidDescription";
        }
        var total = table.TotalOf(request);
        if (!table.IsRevenue && total < 0 && description.Length == 0)
        {
            return "DeductionNeedsDescription";
        }
        var typed = table.RefusalOfTyped(request);
        if (typed.Length > 0)
        {
            return typed;
        }
        return !Money.IsAcceptable(request.Amount) || total == 0 || (table.IsRevenue && total < 0) ? "InvalidAmount" : "";
    }

    /// <summary>Whether the file <paramref name="attachmentId"/> names, or the same bytes, are on an entry of the project other than <paramref name="editing"/> already (BR-22).</summary>
    public static async Task<bool> IsFileOnAnEntryAsync(Guid projectId, Guid attachmentId, RedixDbContext db, Guid editing = default)
    {
        var hash = await db.Attachments.Where(attachment => attachment.Id == attachmentId).Select(attachment => attachment.Sha256).SingleAsync();
        return await db.EntryAttachments.AnyAsync(link => link.EntryId != editing && db.Attachments.Any(attachment => attachment.Id == link.AttachmentId && attachment.ProjectId == projectId && attachment.Sha256 == hash));
    }

    /// <summary>
    /// Why the column values of <paramref name="request"/> do not fit <paramref name="columns"/>, those its table asks, or, when sending, a required one is empty (FR-41), and the column's
    /// key. Every column whose value the entry keeps is checked the same way, whatever it is (redix-api #129, R5). An empty reason when they fit.
    /// </summary>
    private static (string Reason, string ColumnKey) RefusalOfColumns(EntryRequest request, List<ItemColumn> columns)
    {
        foreach (var (key, value) in request.ColumnValuesKept())
        {
            var column = columns.SingleOrDefault(row => row.Key == key && !row.IsEntryField);
            if (column is null)
            {
                return ("UnknownColumn", key);
            }
            if (!column.Accepts(value))
            {
                return ("InvalidColumnValue", key);
            }
        }
        var missing = request.Submit ? request.Answers().MissingOf(columns) : [];
        return missing.Count == 0 ? ("", "") : ("RequiredColumnMissing", missing[0]);
    }

    /// <summary>A new entry from <paramref name="request"/> in <paramref name="table"/>, on its one date (D10): sent for approval, or else a draft.</summary>
    private static Entry EntryOf(EntryRequest request, Guid projectId, EntryTable table, Guid userId, DateTime at) => new()
    {
        Id = request.Id,
        ProjectId = projectId,
        Kind = table.EntryKindOf(request),
        Status = request.Submit ? EntryStatus.PendingApproval : EntryStatus.Draft,
        EntryDate = request.Date,
        Amount = table.AmountOf(request),
        AccountId = request.AccountId,
        Description = request.Description.Trim(),
        ColumnValues = JsonSerializer.Serialize(request.ColumnValuesKept()),
        CreatedByUserId = userId,
        CreatedAt = at,
    };

    /// <summary>The file of <paramref name="request"/>, as <paramref name="role"/>; none when it has none.</summary>
    private static List<object> FileOf(EntryRequest request, EntryAttachmentRole role) =>
        request.AttachmentId == Guid.Empty ? [] : [new EntryAttachment { EntryId = request.Id, AttachmentId = request.AttachmentId, Role = role }];

    /// <summary>Keeps <paramref name="entry"/> with its <paramref name="details"/>: as a new entry, or in place of <paramref name="existing"/> when one is being changed. Answers the entry kept.</summary>
    private static async Task<Entry> KeepAsync(Entry entry, List<object> details, Entry existing, Guid userId, bool tellDeciders, RedixDbContext db)
    {
        if (existing == Entry.None)
        {
            await AddAsync(entry, details, userId, tellDeciders, db);
            await db.SaveChangesAsync();
            return entry;
        }
        await EntryChanges.ReplaceAsync(existing, entry, details, userId, entry.CreatedAt, db);
        return existing;
    }

    /// <summary>
    /// Adds <paramref name="entry"/> with its <paramref name="details"/> and the history row of each (FR-21), saved with the change, under its project's next «رقم القيد» (FR-54): the
    /// one place an entry is created, so the route, a phone's upload and a project file's load all number. One sent for approval is a decision, and tells the deciders when
    /// <paramref name="tellDeciders"/>; a draft is neither.
    /// </summary>
    public static async Task AddAsync(Entry entry, List<object> details, Guid userId, bool tellDeciders, RedixDbContext db)
    {
        entry.Number = await NextNumberAsync(entry.ProjectId, db);
        db.Add(entry);
        var sent = entry.Status == EntryStatus.PendingApproval;
        db.AddHistory(entry, sent ? HistoryAction.Submit : HistoryAction.Create, userId, entry.CreatedAt);
        if (sent)
        {
            db.EntryDecisions.Add(new EntryDecision { EntryId = entry.Id, Action = EntryDecisionAction.Submit, UserId = userId, At = entry.CreatedAt });
        }
        if (sent && tellDeciders)
        {
            await NotificationsEndpoints.TellDecidersAsync(entry, userId, entry.CreatedAt, db);
        }
        foreach (var detail in details)
        {
            db.Add(detail);
            db.AddHistory(detail, HistoryAction.Create, userId, entry.CreatedAt);
        }
    }

    /// <summary>
    /// The next «رقم القيد» of <paramref name="projectId"/> (FR-54, D3): its counter moves and answers in one statement, which takes the project's row lock, so two entries never share
    /// a number, and a number taken is never given again, even by an entry never kept (G2). It runs as written: composing it would wrap the <c>UPDATE</c> in a query.
    /// </summary>
    private static async Task<int> NextNumberAsync(Guid projectId, RedixDbContext db) =>
        (await db.Database.SqlQuery<int>($"UPDATE Projects SET LastEntryNumber = LastEntryNumber + 1 OUTPUT inserted.LastEntryNumber AS Value WHERE Id = {projectId}").ToListAsync())[0];
}
