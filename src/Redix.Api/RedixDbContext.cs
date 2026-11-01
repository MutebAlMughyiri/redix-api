using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Translations;

namespace Redix.Api;

/// <summary>The REDIX database. It owns the rules every table shares: money as <c>decimal(19,4)</c> (BR-13) and a required rowversion on every table.</summary>
public class RedixDbContext(DbContextOptions<RedixDbContext> options) : DbContext(options)
{
    /// <summary>Every word the app shows, with its value in each language.</summary>
    public DbSet<TranslationKey> TranslationKeys => Set<TranslationKey>();

    /// <summary>Everyone who signs in to REDIX.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>The phones signed in, one per sign-in, each with its refresh token's hash.</summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>Who did what to which record, and when (FR-21); append-only, guarded by the database's <c>AuditTrailIsAppendOnly</c> trigger (AC-21.1).</summary>
    public DbSet<AuditRecord> AuditTrail => Set<AuditRecord>();

    /// <summary>
    /// Records <paramref name="action"/>, done by <paramref name="userId"/> at <paramref name="at"/>, to the tracked <paramref name="record"/>. Call it after the change
    /// and before <c>SaveChanges</c>, which saves the record and the change together.
    /// </summary>
    public void Audit(object record, AuditAction action, Guid userId, DateTime at) => AuditTrail.Add(AuditRecord.Of(Entry(record), action, userId, at));

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TranslationKey>(key =>
        {
            key.Property(k => k.Key).HasMaxLength(100).IsUnicode(false);
            key.Property(k => k.Description).HasMaxLength(500);
            key.HasIndex(k => k.Key).IsUnique();
            key.HasMany(k => k.Translations).WithOne().HasForeignKey(t => t.TranslationKeyId);
            key.HasData(TranslationSeed.Keys);
        });
        modelBuilder.Entity<Translation>(translation =>
        {
            translation.ToTable("Translations");
            translation.Property(t => t.Language).HasMaxLength(2).IsUnicode(false).IsFixedLength();
            translation.Property(t => t.Value).HasMaxLength(1000);
            translation.HasIndex(t => new { t.TranslationKeyId, t.Language }).IsUnique();
            translation.HasData(TranslationSeed.Values);
        });

        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Username).HasMaxLength(50);
            user.HasIndex(u => u.Username).IsUnique();
            user.Property(u => u.PasswordHash).HasMaxLength(200).IsUnicode(false);
        });
        modelBuilder.Entity<Device>(device =>
        {
            device.Property(d => d.RefreshTokenHash).HasMaxLength(64).IsUnicode(false).IsFixedLength();
            device.HasIndex(d => d.RefreshTokenHash).IsUnique();
            device.HasOne<User>().WithMany().HasForeignKey(d => d.UserId);
        });
        modelBuilder.Entity<AuditRecord>(audit =>
        {
            audit.ToTable(nameof(AuditTrail), table => table.HasTrigger("AuditTrailIsAppendOnly"));
            audit.Property(a => a.Action).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
            audit.Property(a => a.TableName).HasMaxLength(128).IsUnicode(false);
            audit.Property(a => a.Before).HasColumnType("json");
            audit.Property(a => a.After).HasColumnType("json");
            audit.HasOne<User>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.ClrType).Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
        }
    }
}
