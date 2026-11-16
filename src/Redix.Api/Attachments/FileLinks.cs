using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;

namespace Redix.Api.Attachments;

/// <summary>
/// A link that opens one kept file with no sign-in until it expires, <see cref="Lifetime"/> after it is made: what a workbook's «المرفق» cell opens, since Excel cannot sign in
/// (Muteb, 16 Nov 2026). The file's id and the moment it expires are signed by ASP.NET Core's Data Protection, so a link cannot be made up or kept alive; the moment is checked on
/// <see cref="TimeProvider"/>, the API's one clock. The one owner of making and reading such links.
/// </summary>
public sealed class FileLinks(IDataProtectionProvider protection, TimeProvider time)
{
    /// <summary>How long a link opens its file.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private readonly IDataProtector protector = protection.CreateProtector(nameof(FileLinks));

    /// <summary>
    /// The link to the own file of each of <paramref name="entryIds"/> that has one, its oldest (redix-app #115), by entry, on the address <paramref name="request"/> reached the API at,
    /// expiring <see cref="Lifetime"/> from now.
    /// </summary>
    public async Task<Dictionary<Guid, string>> OfEntriesAsync(IEnumerable<Guid> entryIds, HttpRequest request, RedixDbContext db)
    {
        var ids = entryIds.ToList();
        var files = await EntryAttachment.OldestFirst(db.EntryAttachments.Where(file => ids.Contains(file.EntryId)), db).ToListAsync();
        var expiresAt = (time.GetUtcNow() + Lifetime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        return files.GroupBy(file => file.EntryId).ToDictionary(group => group.Key,
            group => $"{request.Scheme}://{request.Host}{request.PathBase}/files/{protector.Protect($"{group.First().AttachmentId}|{expiresAt}")}");
    }

    /// <summary>The file <paramref name="link"/> opens; <see cref="Guid.Empty"/> for one expired, changed or never made.</summary>
    public Guid AttachmentOf(string link)
    {
        try
        {
            var signed = protector.Unprotect(link).Split('|');
            return time.GetUtcNow().ToUnixTimeSeconds() < long.Parse(signed[1], CultureInfo.InvariantCulture) ? Guid.Parse(signed[0]) : Guid.Empty;
        }
        catch (CryptographicException)
        {
            return Guid.Empty;
        }
    }
}
