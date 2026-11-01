using System.Net;
using System.Net.Http.Json;

namespace Redix.Api.Tests;

public class TranslationsTests(MigratedApiFactory factory) : IClassFixture<MigratedApiFactory>
{
    [Theory]
    [InlineData("ar", "مصروفات المشاريع")]
    [InlineData("en", "Project expenses")]
    public async Task BundleHoldsEveryWordOfTheLanguage(string language, string subtitle)
    {
        var words = await factory.CreateClient().GetFromJsonAsync<Dictionary<string, string>>($"/translations/{language}");

        Assert.Equal(subtitle, words!["app.subtitle"]);
        Assert.Equal("REDIX", words["app.title"]);
    }

    [Fact]
    public async Task UnchangedBundleAnswersNotModified()
    {
        var client = factory.CreateClient();
        var first = await client.GetAsync("/translations/ar");

        var request = new HttpRequestMessage(HttpMethod.Get, "/translations/ar");
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        var second = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task UnknownLanguageIsNotFound()
    {
        var response = await factory.CreateClient().GetAsync("/translations/fr");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
