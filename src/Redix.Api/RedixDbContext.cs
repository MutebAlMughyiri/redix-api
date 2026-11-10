using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Documents;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;
using Redix.Api.Translations;

namespace Redix.Api;

/// <summary>
/// The REDIX database. It owns the rules every table shares: money as <c>decimal(19,4)</c> (BR-13), a required rowversion on every table, and
/// GUID ids made in code (<c>Guid.CreateVersion7</c>), never by the database, so a new record added through a navigation is inserted.
/// </summary>
public class RedixDbContext(DbContextOptions<RedixDbContext> options) : DbContext(options)
{
    /// <summary>Every word the app shows, with its value in each language.</summary>
    public DbSet<TranslationKey> TranslationKeys => Set<TranslationKey>();

    /// <summary>Everyone who signs in to REDIX.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>The phones signed in, one per sign-in, each with its refresh token's hash.</summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>Who did what to which record, when, and in which project (FR-21, redix-app #89); append-only, guarded by the database's <c>HistoryIsAppendOnly</c> trigger (AC-21.1).</summary>
    public DbSet<HistoryRecord> History => Set<HistoryRecord>();

    /// <summary>The real-estate development projects, each with its units.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Who may do what in which project: one row per permission ticked (FR-36).</summary>
    public DbSet<ProjectPermission> ProjectPermissions => Set<ProjectPermission>();

    /// <summary>The project each user chose to land on (FR-37).</summary>
    public DbSet<UserDefaultProject> UserDefaultProjects => Set<UserDefaultProject>();

    /// <summary>What each person is called in each project (Muteb, 9 Nov 2026).</summary>
    public DbSet<ProjectRoleName> ProjectRoleNames => Set<ProjectRoleName>();

    /// <summary>Each project's categories of expenses, in the default template too (FR-39).</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>The items under the categories (FR-05, FR-38).</summary>
    public DbSet<Item> Items => Set<Item>();

    /// <summary>The groups inside the items, a general one in each (FR-05).</summary>
    public DbSet<Subgroup> Subgroups => Set<Subgroup>();

    /// <summary>What a new item of each type starts with, per project (FR-38, FR-41).</summary>
    public DbSet<DefaultColumn> DefaultColumns => Set<DefaultColumn>();

    /// <summary>What each item asks for at entry (FR-41).</summary>
    public DbSet<ItemColumn> ItemColumns => Set<ItemColumn>();

    /// <summary>The ways each project pays (FR-40).</summary>
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    /// <summary>Where each project's money sits (FR-22).</summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>Money moved between a project's accounts (FR-23, redix-app #89): counted in both balances at once, never in the surplus.</summary>
    public DbSet<Transfer> Transfers => Set<Transfer>();

    /// <summary>Who holds which account.</summary>
    public DbSet<AccountCustodian> AccountCustodians => Set<AccountCustodian>();

    /// <summary>The partners of each project (UC-016).</summary>
    public DbSet<Partner> Partners => Set<Partner>();

    /// <summary>The supplier register of each project (FR-06).</summary>
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>The item each supplier's expenses usually go to.</summary>
    public DbSet<SupplierDefaultItem> SupplierDefaultItems => Set<SupplierDefaultItem>();

    /// <summary>What each project's contractors were promised (FR-07).</summary>
    public DbSet<Contract> Contracts => Set<Contract>();

    /// <summary>The pictures and PDFs each project keeps (FR-09).</summary>
    public DbSet<Attachment> Attachments => Set<Attachment>();

    /// <summary>Documents, each a file of the attachment store kept for a project under a name (UC-064).</summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>The ledger: every movement of money, of any kind.</summary>
    public DbSet<Entry> Entries => Set<Entry>();

    /// <summary>What every expense has.</summary>
    public DbSet<ExpenseDetails> ExpenseDetails => Set<ExpenseDetails>();

    /// <summary>What only a purchase has.</summary>
    public DbSet<PurchaseDetails> PurchaseDetails => Set<PurchaseDetails>();

    /// <summary>What only a contractor payment has.</summary>
    public DbSet<ContractorPaymentDetails> ContractorPaymentDetails => Set<ContractorPaymentDetails>();

    /// <summary>The bill numbers of expenses entered before REDIX stopped asking for one (redix-api #96, D2).</summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>What ties one entry to another.</summary>
    public DbSet<EntryLink> EntryLinks => Set<EntryLink>();

    /// <summary>What only a capital movement has.</summary>
    public DbSet<Redix.Api.Partners.CapitalMovementDetails> CapitalMovementDetails => Set<Redix.Api.Partners.CapitalMovementDetails>();

    /// <summary>Each periodic report as it was issued.</summary>
    public DbSet<Redix.Api.Reports.PeriodicReport> PeriodicReports => Set<Redix.Api.Reports.PeriodicReport>();

    /// <summary>Each check of the accounts against the books.</summary>
    public DbSet<Reconciliation> Reconciliations => Set<Reconciliation>();

    /// <summary>What each person was told about an entry.</summary>
    public DbSet<Redix.Api.Notifications.Notification> Notifications => Set<Redix.Api.Notifications.Notification>();

    /// <summary>Each entry's steps through approval.</summary>
    public DbSet<EntryDecision> EntryDecisions => Set<EntryDecision>();

    /// <summary>The files each entry carries.</summary>
    public DbSet<EntryAttachment> EntryAttachments => Set<EntryAttachment>();

    /// <summary>What users may do across the system, such as the admin's approving projects.</summary>
    public DbSet<SystemPermission> SystemPermissions => Set<SystemPermission>();

    /// <summary>The configurable values, such as how long the app waits for an answer.</summary>
    public DbSet<Config.ConfigValue> ConfigValues => Set<Config.ConfigValue>();

    /// <summary>
    /// Records <paramref name="action"/>, done by <paramref name="userId"/> at <paramref name="at"/>, to the tracked <paramref name="record"/>. Call it after the change
    /// and before <c>SaveChanges</c>, which saves the record and the change together.
    /// </summary>
    public void AddHistory(object record, HistoryAction action, Guid userId, DateTime at) => History.Add(HistoryRecord.Of(Entry(record), action, userId, at));

    /// <summary>Records <paramref name="action"/> against each record the coming save changes, so one reached through a navigation is recorded like any other. Call it after the change, before any other history row.</summary>
    public void AddHistoryOfChanges(HistoryAction action, Guid userId, DateTime at)
    {
        ChangeTracker.DetectChanges();
        foreach (var changed in ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).ToList())
        {
            AddHistory(changed.Entity, action, userId, at);
        }
    }

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
            user.Property(u => u.Username).HasMaxLength(User.UsernameMaxLength);
            user.Property(u => u.DisplayName).HasMaxLength(User.NameMaxLength);
            user.Property(u => u.Mobile).HasMaxLength(User.MobileMaxLength).IsUnicode(false);
            user.HasIndex(u => u.Username).IsUnique();
            user.Property(u => u.PasswordHash).HasMaxLength(200).IsUnicode(false);
        });
        modelBuilder.Entity<Device>(device =>
        {
            device.Property(d => d.RefreshTokenHash).HasMaxLength(64).IsUnicode(false).IsFixedLength();
            device.HasIndex(d => d.RefreshTokenHash).IsUnique();
            device.HasOne<User>().WithMany().HasForeignKey(d => d.UserId);
        });
        modelBuilder.Entity<Project>(project =>
        {
            project.Property(p => p.Name).HasMaxLength(Project.NameMaxLength);
            project.Property(p => p.RejectionReason).HasMaxLength(Project.ReasonMaxLength);
            project.Property(p => p.Description).HasMaxLength(Project.DescriptionMaxLength);
            project.HasMany(p => p.Units).WithOne().HasForeignKey(u => u.ProjectId);
            project.HasData(StructureSeed.Template);
        });
        modelBuilder.Entity<Unit>(unit =>
        {
            unit.ToTable("Units");
            unit.Property(u => u.Name).HasMaxLength(Project.NameMaxLength);
        });
        modelBuilder.Entity<Category>(category =>
        {
            category.Property(c => c.Name).HasMaxLength(Project.NameMaxLength);
            category.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Restrict);
            category.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Restrict);
            category.HasData(StructureSeed.Categories);
        });
        modelBuilder.Entity<Item>(item =>
        {
            item.Property(i => i.Name).HasMaxLength(Project.NameMaxLength);
            item.HasMany(i => i.Subgroups).WithOne().HasForeignKey(s => s.ItemId).OnDelete(DeleteBehavior.Restrict);
            item.HasMany(i => i.Columns).WithOne().HasForeignKey(c => c.ItemId).OnDelete(DeleteBehavior.Restrict);
            item.HasData(StructureSeed.Items);
        });
        modelBuilder.Entity<Subgroup>(subgroup =>
        {
            subgroup.Property(s => s.Name).HasMaxLength(Project.NameMaxLength);
            subgroup.HasData(StructureSeed.Subgroups);
        });
        modelBuilder.Entity<DefaultColumn>(column =>
        {
            column.Property(c => c.Key).HasMaxLength(Column.KeyMaxLength).IsUnicode(false);
            column.Property(c => c.Name).HasMaxLength(Project.NameMaxLength);
            column.Property(c => c.Options).HasColumnType("json");
            column.HasIndex(c => new { c.ProjectId, c.ItemType, c.Key }).IsUnique();
            column.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Restrict);
            column.HasData(StructureSeed.DefaultColumns);
        });
        modelBuilder.Entity<ItemColumn>(column =>
        {
            column.Property(c => c.Key).HasMaxLength(Column.KeyMaxLength).IsUnicode(false);
            column.Property(c => c.Name).HasMaxLength(Project.NameMaxLength);
            column.Property(c => c.Options).HasColumnType("json");
            column.HasIndex(c => new { c.ItemId, c.Key }).IsUnique();
        });
        modelBuilder.Entity<PaymentMethod>(method =>
        {
            method.Property(m => m.Name).HasMaxLength(Project.NameMaxLength);
            method.HasOne<Project>().WithMany().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Restrict);
            method.HasData(PaymentMethodSeed.PaymentMethods);
        });
        modelBuilder.Entity<Account>(account =>
        {
            account.Property(a => a.Name).HasMaxLength(Project.NameMaxLength);
            account.HasOne<Project>().WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Restrict);
            account.HasMany(a => a.Custodians).WithOne().HasForeignKey(c => c.AccountId);
        });
        modelBuilder.Entity<Transfer>(transfer =>
        {
            transfer.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Transfers_Amount", "[Amount] > 0");
                table.HasCheckConstraint("CK_Transfers_TwoAccounts", "[FromAccountId] <> [ToAccountId]");
            });
            transfer.Property(t => t.Notes).HasMaxLength(Transfer.NotesMaxLength);
            transfer.HasOne<Project>().WithMany().HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Restrict);
            transfer.HasOne<Account>().WithMany().HasForeignKey(t => t.FromAccountId).OnDelete(DeleteBehavior.Restrict);
            transfer.HasOne<Account>().WithMany().HasForeignKey(t => t.ToAccountId).OnDelete(DeleteBehavior.Restrict);
            transfer.HasOne<User>().WithMany().HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AccountCustodian>(custodian =>
        {
            custodian.HasKey(c => c.AccountId);
            custodian.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Partner>(partner =>
        {
            partner.Property(p => p.Name).HasMaxLength(Project.NameMaxLength);
            partner.HasIndex(p => new { p.ProjectId, p.UserId }).IsUnique().HasFilter($"[UserId] <> '{Guid.Empty}'");
            partner.HasOne<Project>().WithMany().HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Supplier>(supplier =>
        {
            supplier.Property(s => s.Name).HasMaxLength(Project.NameMaxLength);
            supplier.Property(s => s.NormalizedName).HasMaxLength(Project.NameMaxLength);
            supplier.Property(s => s.VatNumber).HasMaxLength(Supplier.VatNumberMaxLength).IsUnicode(false);
            supplier.Property(s => s.Mobile).HasMaxLength(User.MobileMaxLength).IsUnicode(false);
            supplier.HasIndex(s => new { s.ProjectId, s.VatNumber }).IsUnique().HasFilter("[VatNumber] <> ''");
            supplier.HasIndex(s => new { s.ProjectId, s.Name });
            supplier.HasOne<Project>().WithMany().HasForeignKey(s => s.ProjectId).OnDelete(DeleteBehavior.Restrict);
            supplier.HasOne<User>().WithMany().HasForeignKey(s => s.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            supplier.HasMany(s => s.DefaultItems).WithOne().HasForeignKey(row => row.SupplierId);
        });
        modelBuilder.Entity<SupplierDefaultItem>(row =>
        {
            row.HasKey(r => r.SupplierId);
            row.HasOne<Item>().WithMany().HasForeignKey(r => r.ItemId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Contract>(contract =>
        {
            contract.Property(c => c.Notes).HasMaxLength(Contract.NotesMaxLength);
            contract.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Restrict);
            contract.HasOne<Item>().WithMany().HasForeignKey(c => c.ItemId).OnDelete(DeleteBehavior.Restrict);
            contract.HasOne<Supplier>().WithMany().HasForeignKey(c => c.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Attachment>(attachment =>
        {
            attachment.Property(a => a.StoragePath).HasMaxLength(200).IsUnicode(false);
            attachment.Property(a => a.FileName).HasMaxLength(Attachment.FileNameMaxLength);
            attachment.Property(a => a.ContentType).HasMaxLength(Attachment.ContentTypeMaxLength).IsUnicode(false);
            attachment.Property(a => a.Sha256).HasMaxLength(64).IsUnicode(false).IsFixedLength();
            attachment.HasIndex(a => new { a.ProjectId, a.Sha256 });
            attachment.HasOne<Project>().WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Restrict);
            attachment.HasOne<User>().WithMany().HasForeignKey(a => a.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Document>(document =>
        {
            document.Property(d => d.Name).HasMaxLength(Document.NameMaxLength);
            document.Property(d => d.Group).HasMaxLength(Document.GroupMaxLength);
            document.HasIndex(d => new { d.ProjectId, d.AttachmentId }).IsUnique();
            document.HasOne<Project>().WithMany().HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Restrict);
            document.HasOne<Attachment>().WithMany().HasForeignKey(d => d.AttachmentId).OnDelete(DeleteBehavior.Restrict);
            document.HasOne<User>().WithMany().HasForeignKey(d => d.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContractAttachment>(link =>
        {
            link.ToTable("ContractAttachments");
            link.HasKey(l => new { l.ContractId, l.AttachmentId });
            link.HasOne<Contract>().WithMany().HasForeignKey(l => l.ContractId).OnDelete(DeleteBehavior.Restrict);
            link.HasOne<Attachment>().WithMany().HasForeignKey(l => l.AttachmentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Entry>(entry =>
        {
            entry.Property(e => e.Notes).HasMaxLength(global::Redix.Api.Entries.Entry.NotesMaxLength);
            // Measured on RedixReal, 8 Nov 2026: the summary, the figures and what waits all filter by project and status and sum amounts by kind and account (redix-api #87).
            entry.HasIndex(e => new { e.ProjectId, e.Status }).IncludeProperties(e => new { e.Kind, e.Amount, e.AccountId });
            entry.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entry.HasOne<Account>().WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entry.HasOne<User>().WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ExpenseDetails>(details =>
        {
            details.HasKey(d => d.EntryId);
            details.Property(d => d.CustomValues).HasColumnType("json");
            details.HasOne<Entry>().WithOne().HasForeignKey<ExpenseDetails>(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Item>().WithMany().HasForeignKey(d => d.ItemId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Subgroup>().WithMany().HasForeignKey(d => d.SubgroupId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<PaymentMethod>().WithMany().HasForeignKey(d => d.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PurchaseDetails>(details =>
        {
            details.HasKey(d => d.EntryId);
            details.Property(d => d.Product).HasMaxLength(global::Redix.Api.Entries.PurchaseDetails.ProductMaxLength);
            details.HasOne<Entry>().WithOne().HasForeignKey<PurchaseDetails>(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Supplier>().WithMany().HasForeignKey(d => d.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ContractorPaymentDetails>(details =>
        {
            details.HasKey(d => d.EntryId);
            details.HasIndex(d => d.ContractId);
            details.HasOne<Entry>().WithOne().HasForeignKey<ContractorPaymentDetails>(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Contract>().WithMany().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Invoice>(invoice =>
        {
            invoice.HasKey(i => i.EntryId);
            invoice.Property(i => i.Number).HasMaxLength(Invoice.NumberMaxLength);
            invoice.HasIndex(i => i.Number);
            invoice.HasOne<Entry>().WithOne().HasForeignKey<Invoice>(i => i.EntryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Redix.Api.Revenue.RevenueDetails>(details =>
        {
            details.HasKey(d => d.EntryId);
            details.HasOne<Entry>().WithOne().HasForeignKey<Redix.Api.Revenue.RevenueDetails>(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Item>().WithMany().HasForeignKey(d => d.ItemId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Subgroup>().WithMany().HasForeignKey(d => d.SubgroupId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Redix.Api.Partners.CapitalMovementDetails>(details =>
        {
            details.HasKey(d => d.EntryId);
            details.HasIndex(d => d.PartnerId);
            details.HasOne<Entry>().WithOne().HasForeignKey<Redix.Api.Partners.CapitalMovementDetails>(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            details.HasOne<Partner>().WithMany().HasForeignKey(d => d.PartnerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Redix.Api.Reports.PeriodicReport>(report =>
        {
            report.HasIndex(r => new { r.ProjectId, r.Number }).IsUnique();
            report.Property(r => r.Snapshot).HasColumnType("json");
            report.HasOne<Project>().WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Restrict);
            report.HasOne<User>().WithMany().HasForeignKey(r => r.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Reconciliation>(reconciliation =>
        {
            reconciliation.HasIndex(r => new { r.ProjectId, r.At });
            reconciliation.HasOne<Project>().WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Restrict);
            reconciliation.HasOne<User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            reconciliation.HasMany(r => r.Balances).WithOne().HasForeignKey(b => b.ReconciliationId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ReconciliationBalance>(balance =>
        {
            balance.HasKey(b => new { b.ReconciliationId, b.AccountId });
            balance.HasOne<Account>().WithMany().HasForeignKey(b => b.AccountId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Redix.Api.Notifications.Notification>(notification =>
        {
            notification.HasIndex(n => new { n.UserId, n.At });
            notification.HasOne<User>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EntryDecision>(decision =>
        {
            decision.Property(d => d.Reason).HasMaxLength(ApprovalsEndpoints.ReasonMaxLength);
            decision.HasIndex(d => d.EntryId);
            decision.HasOne<Entry>().WithMany().HasForeignKey(d => d.EntryId).OnDelete(DeleteBehavior.Restrict);
            decision.HasOne<User>().WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EntryLink>(link =>
        {
            link.HasKey(l => new { l.EntryId, l.Type });
            link.Property(l => l.Reason).HasMaxLength(ApprovalsEndpoints.ReasonMaxLength);
            link.HasIndex(l => l.LinkedEntryId);
            link.HasIndex(l => l.LinkedEntryId, "IX_EntryLinks_OneReversal").IsUnique().HasFilter($"[Type] = {(byte)EntryLinkType.Reversal}");
            link.HasOne<Entry>().WithMany().HasForeignKey(l => l.EntryId).OnDelete(DeleteBehavior.Restrict);
            link.HasOne<Entry>().WithMany().HasForeignKey(l => l.LinkedEntryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EntryAttachment>(link =>
        {
            link.HasKey(l => new { l.EntryId, l.AttachmentId });
            link.HasOne<Entry>().WithMany().HasForeignKey(l => l.EntryId).OnDelete(DeleteBehavior.Restrict);
            link.HasOne<Attachment>().WithMany().HasForeignKey(l => l.AttachmentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProjectPermission>(held =>
        {
            held.ToTable(table => table.HasCheckConstraint("CK_ProjectPermissions_Permission", $"[Permission] IN ({NamesOf<Permission>()}) AND [Permission] <> '{Permission.ApproveProjects}'"));
            held.Property(h => h.Permission).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
            held.HasIndex(h => new { h.ProjectId, h.UserId, h.Permission }).IsUnique();
            held.HasOne<Project>().WithMany().HasForeignKey(h => h.ProjectId).OnDelete(DeleteBehavior.Restrict);
            held.HasOne<User>().WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProjectRoleName>(named =>
        {
            named.Property(row => row.RoleName).HasMaxLength(ProjectRoleName.MaxLength);
            named.HasIndex(row => new { row.ProjectId, row.UserId }).IsUnique();
            named.HasOne<Project>().WithMany().HasForeignKey(row => row.ProjectId).OnDelete(DeleteBehavior.Restrict);
            named.HasOne<User>().WithMany().HasForeignKey(row => row.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<UserDefaultProject>(chosen =>
        {
            chosen.HasIndex(row => row.UserId).IsUnique();
            chosen.HasOne<User>().WithMany().HasForeignKey(row => row.UserId).OnDelete(DeleteBehavior.Restrict);
            chosen.HasOne<Project>().WithMany().HasForeignKey(row => row.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Config.ConfigValue>(value =>
        {
            value.Property(v => v.Key).HasMaxLength(Config.ConfigValue.KeyMaxLength).IsUnicode(false);
            value.Property(v => v.Value).HasMaxLength(Config.ConfigValue.ValueMaxLength);
            value.HasIndex(v => v.Key).IsUnique();
            value.HasData(Config.ConfigValue.Seeded);
        });
        modelBuilder.Entity<SystemPermission>(held =>
        {
            held.ToTable(table => table.HasCheckConstraint("CK_SystemPermissions_Permission", $"[Permission] IN ({NamesOf<Permission>()})"));
            held.Property(h => h.Permission).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
            held.HasIndex(h => new { h.UserId, h.Permission }).IsUnique();
            held.HasOne<User>().WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<HistoryRecord>(history =>
        {
            history.ToTable(nameof(History), table => table.HasTrigger("HistoryIsAppendOnly"));
            history.Property(a => a.Action).HasConversion<string>().HasMaxLength(30).IsUnicode(false);
            history.Property(a => a.TableName).HasMaxLength(128).IsUnicode(false);
            history.Property(a => a.Before).HasColumnType("json");
            history.Property(a => a.After).HasColumnType("json");
            history.HasOne<User>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
            history.HasIndex(a => new { a.ProjectId, a.Id }).IncludeProperties(a => new { a.At, a.UserId, a.Action, a.TableName, a.RecordId });
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
            if (entityType.FindPrimaryKey()?.Properties is [{ ClrType: var keyType, Name: var keyName }] && keyType == typeof(Guid))
            {
                entity.Property(keyName).ValueGeneratedNever();
            }
        }
    }

    private static string NamesOf<TEnum>() where TEnum : struct, Enum => string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"));
}
