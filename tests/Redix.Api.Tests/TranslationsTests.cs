using System.Net;
using System.Net.Http.Json;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class TranslationsTests(MigratedApiFactory factory)
{
    [Theory]
    [InlineData("ar", "اختر المشروع")]
    [InlineData("en", "Choose a project")]
    public async Task BundleHoldsEveryWordOfTheLanguage(string language, string choose)
    {
        var words = await factory.CreateClient().GetFromJsonAsync<Dictionary<string, string>>($"/translations/{language}");

        Assert.Equal(choose, words!["projects.choose"]);
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
