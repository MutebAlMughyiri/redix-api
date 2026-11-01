using System.Net;

namespace Redix.Api.Tests;

public class HealthTests(MigratedApiFactory factory) : IClassFixture<MigratedApiFactory>
{
    [Fact]
    public async Task HealthAnswersAgainstMigratedDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
