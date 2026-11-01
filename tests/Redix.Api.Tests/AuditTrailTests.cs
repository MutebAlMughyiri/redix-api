using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Audit;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class AuditTrailTests(MigratedApiFactory factory)
{
    [Fact]
    public async Task NoRecordOfTheTrailCanBeChangedOrDeletedByAnyone()
    {
        var user = await factory.AddUserAsync("Any-password-1");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RedixDbContext>();
        db.Attach(user);
        db.Audit(user, AuditAction.SignIn, user.Id, factory.Time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        var record = Assert.Single(await factory.AuditTrailOfAsync(user.Id));
        var sameRecord = db.AuditTrail.Where(stored => stored.Id == record.Id);

        await Assert.ThrowsAsync<SqlException>(() => sameRecord.ExecuteUpdateAsync(stored => stored.SetProperty(audit => audit.At, DateTime.UnixEpoch)));
        await Assert.ThrowsAsync<SqlException>(() => sameRecord.ExecuteDeleteAsync());
        db.AuditTrail.Remove(await sameRecord.SingleAsync());
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        var kept = Assert.Single(await factory.AuditTrailOfAsync(user.Id));
        Assert.Equal((record.UserId, record.At, record.Action), (kept.UserId, kept.At, kept.Action));
    }
}
