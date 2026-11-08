using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Redix.Api.Tests;

/// <summary>
/// A list of ids reaches SQL Server as one JSON parameter, so a query keeps one plan however many ids it carries. As one parameter per id, the entries list of
/// RedixReal's first project (1,038 entries on 8 Nov 2026) took 6 to 9 seconds to compile whenever the plan cache had emptied, past the app's 10-second limit once
/// the answer crossed to the phone, and the home said it had no entries.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ParameterListTests(MigratedApiFactory factory)
{
    [Fact]
    public void AListOfIdsReachesSqlServerAsOneParameter()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var ids = Enumerable.Range(0, 1038).Select(_ => Guid.NewGuid()).ToList();

        var sql = db.Entries.Where(row => ids.Contains(row.Id)).ToQueryString();

        Assert.Contains("OPENJSON(@ids)", sql);
        Assert.DoesNotContain("@ids1", sql);
    }

    [Fact]
    public async Task AnEntryIsFoundAmongManyIdsAndNoneAmongIdsThatNameNone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var entry = await ledger.RecordAsync(ledger.Purchase());
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        var strangers = Enumerable.Range(0, 2500).Select(_ => Guid.NewGuid()).ToList();
        List<Guid> withTheEntry = [.. strangers, entry.Id];

        Assert.Equal([entry.Id], await db.Entries.Where(row => withTheEntry.Contains(row.Id)).Select(row => row.Id).ToListAsync());
        Assert.Equal(0, await db.Entries.CountAsync(row => strangers.Contains(row.Id)));
    }
}
