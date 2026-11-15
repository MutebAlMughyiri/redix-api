using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Redix.Api.Projects;

namespace Redix.Api.Auth;

/// <summary>
/// Keeps <see cref="AccessCache"/> true (redix-api #120 D3). Every save that adds, changes or removes an <see cref="IAccessRow"/> forgets that person, and one that changes
/// a project's status forgets everyone, since its members gain or lose all of it at once (BR-34). It forgets when the save ends, and again when the transaction around it
/// commits, so no request keeps what it read before the change could be seen.
/// </summary>
/// <param name="access">The kept accesses.</param>
public sealed class AccessChanges(AccessCache access) : SaveChangesInterceptor, IDbTransactionInterceptor
{
    private readonly ConditionalWeakTable<DbContext, Changed> _pending = [];

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Note(eventData.Context);
        return result;
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Note(eventData.Context);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc/>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Forget(eventData.Context, committed: false);
        return result;
    }

    /// <inheritdoc/>
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Forget(eventData.Context, committed: false);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc/>
    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) => Forget(eventData.Context, committed: true);

    /// <inheritdoc/>
    public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Forget(eventData.Context, committed: true);
        return Task.CompletedTask;
    }

    private void Note(DbContext? context)
    {
        if (context is null)
        {
            return;
        }
        var saved = context.ChangeTracker.Entries().Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        var people = saved.Select(entry => entry.Entity).OfType<IAccessRow>().Select(row => row.UserId).ToList();
        var everyone = saved.Any(entry => entry.Entity is Project && (entry.State != EntityState.Modified || entry.Property(nameof(Project.Status)).IsModified));
        if (people.Count == 0 && !everyone)
        {
            return;
        }
        var changed = _pending.GetOrCreateValue(context);
        changed.People.UnionWith(people);
        changed.Everyone |= everyone;
    }

    private void Forget(DbContext? context, bool committed)
    {
        if (context is null || !_pending.TryGetValue(context, out var changed))
        {
            return;
        }
        if (changed.Everyone)
        {
            access.ForgetEveryone();
        }
        else if (changed.People.Count > 0)
        {
            access.Forget(changed.People);
        }
        if (committed || context.Database.CurrentTransaction is null)
        {
            _pending.Remove(context);
        }
    }

    /// <summary>What one context's saves changed since its last commit.</summary>
    private sealed class Changed
    {
        /// <summary>The people whose access rows changed.</summary>
        public HashSet<Guid> People { get; } = [];

        /// <summary>True when a project's status changed, which changes every member's access.</summary>
        public bool Everyone { get; set; }
    }
}
