using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Entries;

namespace Redix.Api.Notifications;

/// <summary>Who is told what, and what they were told (UC-036, FR-20).</summary>
public static class NotificationsEndpoints
{
    /// <summary>How many a person sees: the latest.</summary>
    public const int Latest = 100;

    /// <summary>Maps the routes, each for the signed-in person alone.</summary>
    /// <param name="app">The app.</param>
    public static void MapNotifications(this WebApplication app)
    {
        var mine = app.MapGroup("/notifications").RequireAuthorization(SessionPolicies.Working);
        mine.MapGet("/", List);
        mine.MapPost("/{id:guid}/read", MarkRead);
    }

    /// <summary>What the caller was told, newest first, only about the projects they still have.</summary>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<NotificationView>> List(ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        return await db.Notifications
            .Where(row => row.UserId == userId && db.ProjectPermissions.Any(held => held.UserId == userId && held.ProjectId == row.ProjectId))
            .OrderByDescending(row => row.At).ThenByDescending(row => row.Id).Take(Latest)
            .Join(db.Projects, row => row.ProjectId, project => project.Id,
                (row, project) => new NotificationView(row.Id, row.Kind, row.ProjectId, project.Name, row.Entries.Select(link => link.EntryId).FirstOrDefault(), row.At, row.IsRead))
            .ToListAsync();
    }

    /// <summary>Marks one of the caller's as seen; someone else's is not found.</summary>
    /// <param name="id">The notification.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<IResult> MarkRead(Guid id, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var row = await db.Notifications.SingleOrDefaultAsync(notification => notification.Id == id && notification.UserId == userId);
        if (row is null)
        {
            return Results.NotFound();
        }
        row.IsRead = true;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    /// <summary>Tells whoever decides entries in the project that <paramref name="entry"/> waits, other than <paramref name="actorId"/>; saved with the change.</summary>
    public static async Task TellDecidersAsync(Entry entry, Guid actorId, DateTime at, RedixDbContext db)
    {
        var deciders = await HoldersOfAsync(Permission.ApproveEntries, entry.ProjectId, db);
        db.Notifications.AddRange(deciders.Where(userId => userId != actorId).Select(userId => About(entry, userId, NotificationKind.EntrySubmitted, at)));
    }

    /// <summary>Tells who entered <paramref name="entry"/> that it was decided, unless they decided it themselves; saved with the change.</summary>
    public static void TellAuthor(Entry entry, NotificationKind kind, Guid actorId, DateTime at, RedixDbContext db)
    {
        if (entry.CreatedByUserId != actorId)
        {
            db.Notifications.Add(About(entry, entry.CreatedByUserId, kind, at));
        }
    }

    /// <summary>What <paramref name="userId"/> is told about <paramref name="entry"/>, which tapping it opens (findings row 214).</summary>
    private static Notification About(Entry entry, Guid userId, NotificationKind kind, DateTime at)
    {
        var told = new Notification { UserId = userId, ProjectId = entry.ProjectId, Kind = kind, At = at };
        told.Entries.Add(new NotificationEntry { NotificationId = told.Id, EntryId = entry.Id });
        return told;
    }

    /// <summary>Tells the managers of <paramref name="projectId"/> what the admin decided; saved with the change.</summary>
    public static async Task TellManagersAsync(Guid projectId, NotificationKind kind, DateTime at, RedixDbContext db)
    {
        var managers = await HoldersOfAsync(Permission.ManageUsers, projectId, db);
        db.Notifications.AddRange(managers.Select(userId => new Notification { UserId = userId, ProjectId = projectId, Kind = kind, At = at }));
    }

    private static Task<List<Guid>> HoldersOfAsync(Permission permission, Guid projectId, RedixDbContext db) =>
        db.ProjectPermissions.Where(held => held.ProjectId == projectId && held.Permission == permission).Select(held => held.UserId).ToListAsync();
}

/// <summary>Something a person was told.</summary>
/// <param name="Id">The notification's id.</param>
/// <param name="Kind">What happened.</param>
/// <param name="ProjectId">The project it is about.</param>
/// <param name="ProjectName">Its name.</param>
/// <param name="EntryId">The entry tapping it opens; empty when it is about the project itself.</param>
/// <param name="At">When.</param>
/// <param name="IsRead">True once seen.</param>
public record NotificationView(Guid Id, NotificationKind Kind, Guid ProjectId, string ProjectName, Guid EntryId, DateTime At, bool IsRead);
