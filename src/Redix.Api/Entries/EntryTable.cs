using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Structure;

namespace Redix.Api.Entries;

/// <summary>
/// A table an entry goes into, and what its kind adds to every entry (redix-api #110): its own checks, its own rows, what the entry is, what its file is, and what recording it
/// answers. Everything every entry shares is asked by <see cref="EntryRecording"/>; nothing outside a kind's own class asks which kind it is.
/// </summary>
/// <param name="Category">The category its item is in.</param>
/// <param name="Item">The item it belongs to.</param>
/// <param name="Id">Its id: a group's, a contract's or a partner's.</param>
/// <param name="Name">Its name in the picker.</param>
/// <param name="Kind">What it is in the item's sheet.</param>
/// <param name="SortOrder">Its place in the item's sheet (AC-46.8).</param>
public abstract record EntryTable(Category Category, Item Item, Guid Id, string Name, ItemTableKind Kind, int SortOrder)
{
    /// <summary>True on the revenue side (FR-38): entered by whoever holds <see cref="Permission.MoveMoney"/> into any active account, with no payment method and no deduction.</summary>
    public bool IsRevenue => Item.Type.IsRevenue();

    /// <summary>What an entry of <paramref name="request"/> in this table is.</summary>
    public abstract EntryKind EntryKindOf(EntryRequest request);

    /// <summary>What the entry's file is.</summary>
    public abstract EntryAttachmentRole FileRole { get; }

    /// <summary>The total <paramref name="request"/> comes to, as it was typed.</summary>
    public virtual decimal TotalOf(EntryRequest request) => request.Amount;

    /// <summary>The amount the ledger keeps for <paramref name="request"/>.</summary>
    public virtual decimal AmountOf(EntryRequest request) => TotalOf(request);

    /// <summary>Why what was typed for this kind of table cannot be kept; empty when it can.</summary>
    public virtual string RefusalOfTyped(EntryRequest request) => "";

    /// <summary>Why <paramref name="request"/> cannot go into this table, by what the project holds; empty when it can. <paramref name="editing"/> is the entry being changed.</summary>
    public virtual Task<string> RefusalOfAsync(Guid projectId, EntryRequest request, Guid userId, List<Permission> held, Guid editing, RedixDbContext db) => Task.FromResult("");

    /// <summary>The rows this kind of table adds to the entry of <paramref name="request"/>, its file aside.</summary>
    public abstract List<object> DetailsOf(EntryRequest request);

    /// <summary>What recording <paramref name="entry"/> from <paramref name="request"/> answers.</summary>
    public virtual Task<object> AnswerOfAsync(Entry entry, EntryRequest request, RedixDbContext db) => Task.FromResult<object>(new RecordedEntryView(entry.Id, entry.Status, entry.Amount));

    /// <summary>
    /// Every table of the project <paramref name="projectId"/> whoever holds <paramref name="held"/> may enter into, in the order of the report: each active category's items, each
    /// with its tables in the order its sheet prints them. An expense item's groups and active contracts are offered to whoever holds <see cref="Permission.RecordEntries"/>; a
    /// revenue item's groups and a capital item's partners to whoever holds <see cref="Permission.MoveMoney"/> (BR-30, redix-api #109). The picker lists these, and an entry goes
    /// into nothing else.
    /// </summary>
    public static async Task<List<EntryTable>> OfferedAsync(Guid projectId, List<Permission> held, RedixDbContext db)
    {
        var categories = await db.Categories.Where(category => category.ProjectId == projectId && category.IsActive).OrderBy(category => category.SortOrder)
            .Include(category => category.Items.OrderBy(item => item.SortOrder)).ThenInclude(item => item.Subgroups)
            .Include(category => category.Items).ThenInclude(item => item.Columns)
            .AsSplitQuery().ToListAsync();
        var contracts = await (
            from contract in db.Contracts
            join supplier in db.Suppliers on contract.SupplierId equals supplier.Id
            where contract.ProjectId == projectId && contract.IsActive
            select new { Contract = contract, Contractor = supplier.Name }).ToListAsync();
        var partners = await db.Partners.Where(Partners.Partner.Kept).Where(partner => partner.ProjectId == projectId).ToListAsync();
        var recordsEntries = held.Contains(Permission.RecordEntries);
        var movesMoney = held.Contains(Permission.MoveMoney);
        return categories.SelectMany(category => category.Items.SelectMany(item =>
        {
            List<EntryTable> tables =
            [
                .. recordsEntries && item.IsOfferedFor(ItemType.Purchases) ? item.Subgroups.Select(subgroup => new PurchaseTable(category, item, subgroup)) : [],
                .. recordsEntries && item.IsOfferedFor(ItemType.ContractorPayments) ? contracts.Where(row => row.Contract.ItemId == item.Id).Select(row => new ContractTable(category, item, row.Contract, row.Contractor)) : [],
                .. movesMoney && item.IsOfferedFor(ItemType.OtherRevenue) ? item.Subgroups.Select(subgroup => new RevenueTable(category, item, subgroup)) : [],
                .. movesMoney && item.IsOfferedFor(ItemType.Capital) ? partners.Where(partner => partner.ItemId == item.Id).Select(partner => new PartnerTable(category, item, partner)) : [],
            ];
            return tables.OrderBy(table => table.SortOrder);
        })).ToList();
    }

    /// <summary>The name of <paramref name="item"/>'s group <paramref name="subgroup"/> in the picker: its general group goes by the item's name.</summary>
    protected static string NameOf(Item item, Subgroup subgroup) => subgroup.IsGeneral ? item.Name : subgroup.Name;

    /// <summary>What <paramref name="subgroup"/> is in its item's sheet.</summary>
    protected static ItemTableKind KindOf(Subgroup subgroup) => subgroup.IsGeneral ? ItemTableKind.General : ItemTableKind.Subgroup;

    /// <summary>What every expense of <paramref name="request"/> holds, in its item's group <paramref name="subgroupId"/>: how it was paid and its custom values.</summary>
    protected ExpenseDetails ExpenseDetailsOf(EntryRequest request, Guid subgroupId) => new()
    {
        EntryId = request.Id,
        ItemId = Item.Id,
        SubgroupId = subgroupId,
        PaymentMethodId = request.PaymentMethodId,
        CustomValues = System.Text.Json.JsonSerializer.Serialize(request.CustomValuesKept()),
    };
}

/// <summary>A recorded entry, as the API kept it.</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands.</param>
/// <param name="Amount">Its amount: negative for a credit note or a deduction.</param>
public record RecordedEntryView(Guid Id, EntryStatus Status, decimal Amount);
