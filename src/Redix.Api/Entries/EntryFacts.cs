using Microsoft.EntityFrameworkCore;
using Redix.Api.Projects;
using Redix.Api.Suppliers;

namespace Redix.Api.Entries;

/// <summary>What a list of a project's entries shows of each, read at once: the one owner of joining an entry to its names and its invoice.</summary>
public sealed class EntryFacts
{
    private Dictionary<Guid, ExpenseDetails> _expenses = [];
    private Dictionary<Guid, PurchaseDetails> _purchases = [];
    private Dictionary<Guid, ContractorPaymentDetails> _payments = [];
    private Dictionary<Guid, Invoice> _invoices = [];
    private ILookup<Guid, EntryAttachment> _files = Array.Empty<EntryAttachment>().ToLookup(row => row.EntryId);
    private Dictionary<Guid, string> _contentTypes = [];
    private Dictionary<Guid, string> _items = [];
    private Dictionary<Guid, string> _accounts = [];
    private Dictionary<Guid, Supplier> _suppliers = [];
    private Dictionary<Guid, Guid> _contractors = [];
    private Dictionary<Guid, string> _authors = [];

    /// <summary>The facts of <paramref name="entries"/> of the project <paramref name="projectId"/>.</summary>
    public static async Task<EntryFacts> OfAsync(Guid projectId, List<Entry> entries, RedixDbContext db)
    {
        var ids = entries.Select(entry => entry.Id).ToList();
        var authorIds = entries.Select(entry => entry.CreatedByUserId).Distinct().ToList();
        return new EntryFacts
        {
            _expenses = await db.ExpenseDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _purchases = await db.PurchaseDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _payments = await db.ContractorPaymentDetails.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _invoices = await db.Invoices.Where(row => ids.Contains(row.EntryId)).ToDictionaryAsync(row => row.EntryId),
            _files = (await db.EntryAttachments.Where(row => ids.Contains(row.EntryId)).ToListAsync()).ToLookup(row => row.EntryId),
            _contentTypes = await db.Attachments.Where(row => row.ProjectId == projectId).ToDictionaryAsync(row => row.Id, row => row.ContentType),
            _items = await db.Items.Where(item => db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId)).ToDictionaryAsync(item => item.Id, item => item.Name),
            _accounts = await db.Accounts.Where(account => account.ProjectId == projectId).ToDictionaryAsync(account => account.Id, account => account.Name),
            _suppliers = await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).ToDictionaryAsync(supplier => supplier.Id),
            _contractors = await db.Contracts.Where(contract => contract.ProjectId == projectId).ToDictionaryAsync(contract => contract.Id, contract => contract.SupplierId),
            _authors = await db.Users.Where(user => authorIds.Contains(user.Id)).ToDictionaryAsync(user => user.Id, user => user.DisplayName),
        };
    }

    /// <summary>The expense's own fields; empty ones when the entry has none.</summary>
    public ExpenseDetails ExpenseOf(Entry entry) => _expenses.GetValueOrDefault(entry.Id) ?? new ExpenseDetails();

    /// <summary>The entry's invoice; an empty one when it has none.</summary>
    public Invoice InvoiceOf(Entry entry) => _invoices.GetValueOrDefault(entry.Id) ?? new Invoice();

    /// <summary>The entry's item.</summary>
    public string ItemNameOf(Entry entry) => _items.GetValueOrDefault(ExpenseOf(entry).ItemId, "");

    /// <summary>The account that paid it.</summary>
    public string AccountNameOf(Entry entry) => _accounts.GetValueOrDefault(entry.AccountId, "");

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
    public string ContentTypeOf(Entry entry) => _files[entry.Id].Select(file => _contentTypes.GetValueOrDefault(file.AttachmentId, "")).FirstOrDefault("");
}
