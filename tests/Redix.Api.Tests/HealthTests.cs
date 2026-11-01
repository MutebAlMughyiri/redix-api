using System.Net;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class HealthTests(MigratedApiFactory factory)
{
    [Fact]
    public async Task HealthAnswersAgainstMigratedDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
