using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class SwaggerTests(MigratedApiFactory factory)
{
    [Theory]
    [InlineData("Development", true)]
    [InlineData("Staging", true)]
    [InlineData("Production", false)]
    public async Task SwaggerIsServedOutsideProductionOnly(string environment, bool served)
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment)).CreateClient();

        var swaggerUi = await client.GetAsync("/swagger/index.html");
        var openApiDocument = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(served, swaggerUi.StatusCode == HttpStatusCode.OK);
        Assert.Equal(served, openApiDocument.StatusCode == HttpStatusCode.OK);
    }
}
