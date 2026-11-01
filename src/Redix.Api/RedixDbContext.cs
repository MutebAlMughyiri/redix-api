using Microsoft.EntityFrameworkCore;
using Redix.Api.Translations;

namespace Redix.Api;

/// <summary>The REDIX database. It owns the rules every table shares: money as <c>decimal(19,4)</c> (BR-13) and a required rowversion on every table.</summary>
public class RedixDbContext(DbContextOptions<RedixDbContext> options) : DbContext(options)
{
    /// <summary>Every word the app shows, with its value in each language.</summary>
    public DbSet<TranslationKey> TranslationKeys => Set<TranslationKey>();

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

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.ClrType).Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
        }
    }
}
