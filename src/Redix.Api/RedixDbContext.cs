using Microsoft.EntityFrameworkCore;
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

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.ClrType).Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
        }
    }
}
