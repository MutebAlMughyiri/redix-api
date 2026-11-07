using System.Net;

namespace Redix.Api.Tests;

[Collection(ThirdDatabaseCollection.Name)]
public class HealthTests(MigratedApiFactory factory)
{
    [Fact]
    public async Task HealthAnswersAgainstMigratedDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
