using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Tests;

[Collection(ThirdDatabaseCollection.Name)]
public class ConfigTests(MigratedApiFactory factory)
{
    [Fact]
    public async Task UC_070_TheAppReadsHowLongToWaitBeforeAnyoneSignsIn()
    {
        var values = (await factory.CreateClient().GetFromJsonAsync<Dictionary<string, string>>("/config"))!;

        Assert.Equal("10", values["requestTimeoutSeconds"]);
        Assert.Equal("120", values["fileTimeoutSeconds"]);
    }

    [Fact]
    public async Task UC_070_AValueChangedInTheDatabaseIsReadAtTheNextRequest()
    {
        var client = factory.CreateClient();
        try
        {
            await factory.WithDatabaseAsync(db => db.ConfigValues.Where(row => row.Key == "requestTimeoutSeconds").ExecuteUpdateAsync(set => set.SetProperty(row => row.Value, "15")));

            Assert.Equal("15", (await client.GetFromJsonAsync<Dictionary<string, string>>("/config"))!["requestTimeoutSeconds"]);
        }
        finally
        {
            await factory.WithDatabaseAsync(db => db.ConfigValues.Where(row => row.Key == "requestTimeoutSeconds").ExecuteUpdateAsync(set => set.SetProperty(row => row.Value, "10")));
        }
    }
}
