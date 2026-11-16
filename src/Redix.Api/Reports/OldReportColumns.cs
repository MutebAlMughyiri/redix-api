using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Structure;

namespace Redix.Api.Reports;

/// <summary>
/// Writes, once, the table columns into the periodic reports issued before redix-api #129, which kept none, so their files print each table's columns as a new report does (Muteb,
/// 16 Nov 2026): each table takes its item's columns as they print today, and each row its entry's values and number. Every row and amount stays as issued (BR-26). EF Core's seeding
/// runs it after every migration; a report whose tables carry their amount's name was issued after #129 and is left alone, so a second run changes nothing.
/// </summary>
public static class OldReportColumns
{
    /// <summary>Fills the old reports' columns in <paramref name="db"/>, and saves; nothing while a migration is still to apply, when the tables it reads may not be there yet.</summary>
    public static async Task FillAsync(RedixDbContext db)
    {
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            return;
        }
        foreach (var report in await db.PeriodicReports.ToListAsync())
        {
            var snapshot = report.Read();
            if (!snapshot.Items.Any(IsOld))
            {
                continue;
            }
            var items = new List<ItemReportView>();
            foreach (var item in snapshot.Items)
            {
                items.Add(IsOld(item) ? await FilledAsync(report.ProjectId, item, db) : item);
            }
            db.Entry(report).Property(row => row.Snapshot).CurrentValue = PeriodicReport.Write(snapshot with { Items = items });
        }
        await db.SaveChangesAsync();
    }

    private static bool IsOld(ItemReportView item) => item.Subgroups.Any(table => table.AmountName.Length == 0) || item.Contracts.Any(table => table.AmountName.Length == 0);

    /// <summary><paramref name="item"/> with every table in the columns it prints today; as issued when the item is no longer in the project.</summary>
    private static async Task<ItemReportView> FilledAsync(Guid projectId, ItemReportView item, RedixDbContext db)
    {
        var structure = await ProjectStructure.ItemWithTablesAsync(projectId, item.ItemId, db);
        if (structure == Item.None)
        {
            return item;
        }
        var ids = item.EntryIds().ToList();
        var entries = await db.Entries.Where(entry => ids.Contains(entry.Id)).ToDictionaryAsync(entry => entry.Id);
        var facts = await EntryFacts.OfAsync(projectId, [.. entries.Values], db);
        var registers = await ReportColumns.OfProjectAsync(projectId, db);
        var tableKind = structure.Type == ItemType.Capital ? EntryKind.CapitalIn : structure.Type.IsRevenue() ? EntryKind.Revenue : EntryKind.Purchase;
        var tableColumns = ReportColumns.PrintedOf(structure, tableKind);
        var paymentColumns = ReportColumns.PrintedOf(structure, EntryKind.ContractorPayment);

        List<ItemEntryRow> RowsIn(List<ItemColumn> columns, List<ItemEntryRow> rows) => [.. rows.Select(row => entries.TryGetValue(row.EntryId, out var entry)
            ? row with { Values = ReportColumns.ValuesOf(columns, entry, facts, row.IsOutsideContract), Number = entry.Number }
            : row with { Values = [.. columns.Select(_ => "")] })];

        return item with
        {
            Subgroups = [.. item.Subgroups.Select(table => table with
            {
                Rows = RowsIn(tableColumns, table.Rows),
                Columns = registers.ViewsOf(tableColumns),
                AmountName = ReportColumns.AmountNameOf(structure, tableKind),
            })],
            Contracts = [.. item.Contracts.Select(table => table with
            {
                Payments = RowsIn(paymentColumns, table.Payments),
                Columns = registers.ViewsOf(paymentColumns),
                AmountName = ReportColumns.AmountNameOf(structure, EntryKind.ContractorPayment),
            })],
        };
    }
}
