using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Redix.Api.Tests;

public class SwaggerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [InlineData("Development", HttpStatusCode.OK)]
    [InlineData("Staging", HttpStatusCode.OK)]
    [InlineData("Production", HttpStatusCode.NotFound)]
    public async Task SwaggerIsServedOutsideProductionOnly(string environment, HttpStatusCode expected)
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment)).CreateClient();

        var swaggerUi = await client.GetAsync("/swagger/index.html");
        var openApiDocument = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(expected, swaggerUi.StatusCode);
        Assert.Equal(expected, openApiDocument.StatusCode);
    }
}
