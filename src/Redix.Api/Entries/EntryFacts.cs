using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Projects;
using Redix.Api.Revenue;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>What a list of a project's entries shows of each, read at once: the one owner of joining an entry to its names and its invoice.</summary>
public sealed class EntryFacts
{
    private Dictionary<Guid, ExpenseDetails> _expenses = [];
    private Dictionary<Guid, Guid> _revenueItems = [];
    private Dictionary<Guid, Guid> _capitalPartners = [];
    private Dictionary<Guid, string> _partners = [];
    private Dictionary<Guid, PurchaseDetails> _purchases = [];
    private Dictionary<Guid, ContractorPaymentDetails> _payments = [];
    private Dictionary<Guid, Invoice> _invoices = [];
    private ILookup<Guid, EntryAttachment> _files = Array.Empty<EntryAttachment>().ToLookup(row => row.EntryId);
    private Dictionary<Guid, (string ContentType, string FileName)> _attachments = [];
    private Dictionary<Guid, string> _items = [];
    private Dictionary<Guid, string> _accounts = [];
    private Dictionary<Guid, decimal> _balances = [];
    private Dictionary<Guid, decimal> _waitingOut = [];
    private Dictionary<Guid, string> _paymentMethods = [];
    private Dictionary<Guid, Supplier> _suppliers = [];
    private Dictionary<Guid, Guid> _contractors = [];
    private Dictionary<Guid, string> _authors = [];

    /// <summary>The facts of <paramref name="entries"/> of the project <paramref name="projectId"/>.</summary>
    public static async Task<EntryFacts> OfAsync(Guid projectId, List<Entry> entries, RedixDbContext db)
    {
        var ids = entries.Select(entry => entry.Id).ToList();
        var files = await db.EntryAttachments.Where(row => ids.Contains(row.EntryId)).ToListAsync();
        var fileIds = files.Select(row => row.AttachmentId).Distinct().ToList();
        var authorIds = entries.Select(entry => entry.CreatedByUserId).Distinct().ToList();
        return new EntryFacts
        {
            _expenses = await db.ExpenseDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _revenueItems = await db.Set<RevenueDetails>().Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId, row => row.ItemId),
            _capitalPartners = await db.CapitalMovementDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId, row => row.PartnerId),
            _partners = await db.Partners.Where(partner => partner.ProjectId == projectId).ToDictionaryAsync(partner => partner.Id, partner => partner.Name),
            _purchases = await db.PurchaseDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _payments = await db.ContractorPaymentDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _invoices = await db.Invoices.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _files = files.ToLookup(row => row.EntryId),
            _attachments = await db.Attachments.Where(row => fileIds.Contains(row.Id)).ToDictionaryAsync(row => row.Id, row => (row.ContentType, row.FileName)),
            _items = await ProjectStructure.ItemsOf(projectId, db).ToDictionaryAsync(item => item.Id, item => item.Name),
            _accounts = await db.Accounts.Where(account => account.ProjectId == projectId).ToDictionaryAsync(account => account.Id, account => account.Name),
            _balances = await AccountMoney.MovementsOfProjectAsync(projectId, db),
            _waitingOut = (await db.Entries.Where(row => row.ProjectId == projectId && row.Status == EntryStatus.PendingApproval).Select(row => new { row.AccountId, row.Kind, row.Amount }).ToListAsync())
                .Where(row => row.Kind.Signed(row.Amount) < 0).GroupBy(row => row.AccountId).ToDictionary(group => group.Key, group => -group.Sum(row => row.Kind.Signed(row.Amount))),
            _paymentMethods = await db.PaymentMethods.Where(method => method.ProjectId == projectId).ToDictionaryAsync(method => method.Id, method => method.Name),
            _suppliers = await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).ToDictionaryAsync(supplier => supplier.Id),
            _contractors = await db.Contracts.Where(contract => contract.ProjectId == projectId).ToDictionaryAsync(contract => contract.Id, contract => contract.SupplierId),
            _authors = await db.Users.Where(user => authorIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName),
        };
    }

    /// <summary>The expense's own fields; empty ones when the entry has none.</summary>
    public ExpenseDetails ExpenseOf(Entry entry) => _expenses.GetValueOrDefault(entry.Id) ?? new ExpenseDetails();

    /// <summary>The entry's invoice; an empty one when it has none.</summary>
    public Invoice InvoiceOf(Entry entry) => _invoices.GetValueOrDefault(entry.Id) ?? new Invoice();

    /// <summary>The entry's item: an expense's, or a revenue entry's.</summary>
    public string ItemNameOf(Entry entry) => _items.GetValueOrDefault(_revenueItems.TryGetValue(entry.Id, out var itemId) ? itemId : ExpenseOf(entry).ItemId, "");

    /// <summary>A capital movement's partner; empty for any other entry.</summary>
    public string PartnerNameOf(Entry entry) => _partners.GetValueOrDefault(_capitalPartners.GetValueOrDefault(entry.Id), "");

    /// <summary>The account that paid it.</summary>
    public string AccountNameOf(Entry entry) => _accounts.GetValueOrDefault(entry.AccountId, "");

    /// <summary>What the entry's account holds now: its approved entries and transfers (redix-app #89).</summary>
    public decimal AccountBalanceOf(Entry entry) => _balances.GetValueOrDefault(entry.AccountId);

    /// <summary>
    /// What waits to leave the entry's account: every entry taking money out of it that waits for approval, and the entry itself while it is a draft (Muteb, 10 Nov 2026:
    /// the total, so two that each fit but together do not are both noted).
    /// </summary>
    public decimal AccountWaitingOf(Entry entry) =>
        _waitingOut.GetValueOrDefault(entry.AccountId) + (entry.Status == EntryStatus.Draft && entry.Kind.Signed(entry.Amount) < 0 ? entry.Amount : 0);

    /// <summary>
    /// How much more what waits to leave the entry's account takes than the account holds now, for an entry taking money out that is not decided yet; nothing blocks it,
    /// but the inbox and the shared entry say it (R5, R6, D4, D6). Zero for any other entry, and for one its account covers with all that waits on it.
    /// </summary>
    public decimal AccountShortByOf(Entry entry) =>
        entry.Kind.Signed(entry.Amount) < 0 && entry.Status is EntryStatus.Draft or EntryStatus.PendingApproval ? Math.Max(0, AccountWaitingOf(entry) - AccountBalanceOf(entry)) : 0;

    /// <summary>How an expense was paid; empty when nobody said, and for any other entry.</summary>
    public string PaymentMethodNameOf(Entry entry) => _paymentMethods.GetValueOrDefault(ExpenseOf(entry).PaymentMethodId, "");

    /// <summary>Who entered it.</summary>
    public string AuthorNameOf(Entry entry) => _authors.GetValueOrDefault(entry.CreatedByUserId, "");

    /// <summary>Its supplier, or its contract's contractor; an empty one when it has none.</summary>
    public Supplier SupplierOf(Entry entry)
    {
        var supplierId = _purchases.TryGetValue(entry.Id, out var purchase) ? purchase.SupplierId : _payments.TryGetValue(entry.Id, out var payment) ? _contractors.GetValueOrDefault(payment.ContractId) : Guid.Empty;
        return _suppliers.GetValueOrDefault(supplierId) ?? new Supplier { Name = "" };
    }

    /// <summary>The name of its supplier.</summary>
    public string SupplierNameOf(Entry entry) => SupplierOf(entry).Name;

    /// <summary>The VAT number of its supplier.</summary>
    public string SupplierVatNumberOf(Entry entry) => SupplierOf(entry).VatNumber;

    /// <summary>What was bought; empty when not said.</summary>
    public string ProductOf(Entry entry) => _purchases.TryGetValue(entry.Id, out var purchase) ? purchase.Product : "";

    /// <summary>The id of its file; empty when it has none.</summary>
    public string AttachmentIdOf(Entry entry) => _files[entry.Id].Select(file => file.AttachmentId.ToString()).FirstOrDefault("");

    /// <summary>What its file is; empty when it has none.</summary>
    public string ContentTypeOf(Entry entry) => FileOf(entry).ContentType;

    /// <summary>Its file's name, so the phone hands it over by that name (findings row 158); empty when it has none.</summary>
    public string FileNameOf(Entry entry) => FileOf(entry).FileName;

    private (string ContentType, string FileName) FileOf(Entry entry) => _files[entry.Id].Select(file => _attachments.GetValueOrDefault(file.AttachmentId, ("", ""))).FirstOrDefault(("", ""));
}
