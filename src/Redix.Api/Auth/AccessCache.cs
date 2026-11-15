using Microsoft.Extensions.Caching.Memory;

namespace Redix.Api.Auth;

/// <summary>
/// What each person may do, kept between requests so a request reads it at most once (redix-api #120 D1, D2). <see cref="AccessChanges"/> forgets a person whenever a row
/// their access is read from is saved, and everyone when a project's status changes. An access read while anything was forgotten is answered but not kept, so a read that
/// raced a change never outlives it. Each kept access also lives at most <see cref="Lifetime"/>, the last guard against a change made outside REDIX (D7).
/// </summary>
public sealed class AccessCache : IDisposable
{
    /// <summary>How long a kept access lives at most.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private readonly MemoryCache _kept = new(new MemoryCacheOptions());
    private long _forgettings;

    /// <summary>The access of <paramref name="userId"/>: the kept one, or what <paramref name="read"/> answers, kept unless something was forgotten while it read.</summary>
    /// <param name="userId">The person.</param>
    /// <param name="read">Reads the person's access from the database.</param>
    public async Task<UserAccess> OfAsync(Guid userId, Func<Task<UserAccess>> read)
    {
        if (_kept.Get<UserAccess>(userId) is { } kept)
        {
            return kept;
        }
        var forgettings = Interlocked.Read(ref _forgettings);
        var access = await read();
        if (Interlocked.Read(ref _forgettings) == forgettings)
        {
            _kept.Set(userId, access, Lifetime);
        }
        return access;
    }

    /// <summary>Forgets the kept access of each of <paramref name="userIds"/>.</summary>
    /// <param name="userIds">The people whose access changed.</param>
    public void Forget(IEnumerable<Guid> userIds)
    {
        Interlocked.Increment(ref _forgettings);
        foreach (var userId in userIds)
        {
            _kept.Remove(userId);
        }
    }

    /// <summary>Forgets every kept access, as when a project's status changes for all its members.</summary>
    public void ForgetEveryone()
    {
        Interlocked.Increment(ref _forgettings);
        _kept.Clear();
    }

    /// <summary>Ends the memory the accesses are kept in.</summary>
    public void Dispose() => _kept.Dispose();
}
