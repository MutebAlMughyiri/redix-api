using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.History;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class HistoryTests(MigratedApiFactory factory)
{
    [Fact]
    public async Task NoRecordOfTheTrailCanBeChangedOrDeletedByAnyone()
    {
        var user = await factory.AddUserAsync("Any-password-1");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        db.Attach(user);
        db.AddHistory(user, HistoryAction.SignIn, user.Id, factory.Time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        var record = Assert.Single(await factory.HistoryOfAsync(user.Id));
        var sameRecord = db.History.Where(stored => stored.Id == record.Id);

        await Assert.ThrowsAsync<SqlException>(() => sameRecord.ExecuteUpdateAsync(stored => stored.SetProperty(history => history.At, DateTime.UnixEpoch)));
        await Assert.ThrowsAsync<SqlException>(() => sameRecord.ExecuteDeleteAsync());
        db.History.Remove(await sameRecord.SingleAsync());
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        var kept = Assert.Single(await factory.HistoryOfAsync(user.Id));
        Assert.Equal((record.UserId, record.At, record.Action), (kept.UserId, kept.At, kept.Action));
    }
}
