using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Attachments;
using Redix.Api.Documents;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-064: a project's documents, each any file under a name, in a group when one is given.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class DocumentsTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static byte[] WordFile() => [0x50, 0x4B, 0x03, 0x04, .. Guid.NewGuid().ToByteArray()];

    private static Task<HttpResponseMessage> AddAsync(HttpClient client, Guid projectId, string name, string group, byte[] content, string fileName) =>
        client.PostAsync($"/projects/{projectId}/documents", new MultipartFormDataContent
        {
            { new ByteArrayContent(content), "file", fileName },
            { new StringContent(name), "name" },
            { new StringContent(group), "group" },
        });

    private static Task<HttpResponseMessage> AddAsync(Ledger ledger, string name, string group = "") => AddAsync(ledger.Client, ledger.ProjectId, name, group, WordFile(), "contract.docx");

    private static async Task<List<DocumentView>> ListAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<List<DocumentView>>($"/projects/{projectId}/documents", Json))!;

    [Fact]
    public async Task UC_064_TheManagerKeepsDocumentsAndEveryoneReadsThemUngroupedFirstThenByGroup()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        foreach (var (name, group) in new[] { ("المخطط المعماري", "مخططات"), ("رخصة البناء", " تراخيص "), ("  عقد المقاول الرئيسي ", ""), ("مخطط الموقع العام", "مخططات") })
        {
            (await AddAsync(ledger, name, group)).EnsureSuccessStatusCode();
            factory.Time.Advance(TimeSpan.FromMinutes(1));
        }
        var (reader, _) = await ledger.MemberAsync("Reader");

        var seen = await ListAsync(reader, ledger.ProjectId);

        Assert.Equal(
            [("عقد المقاول الرئيسي", ""), ("رخصة البناء", "تراخيص"), ("مخطط الموقع العام", "مخططات"), ("المخطط المعماري", "مخططات")],
            seen.Select(document => (document.Name, document.Group)));
        Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync($"/projects/{ledger.ProjectId}/attachments/{seen[0].AttachmentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await AddAsync(reader, ledger.ProjectId, "مستند", "", WordFile(), "a.docx")).StatusCode);
    }

    [Fact]
    public async Task ADocumentIsAnyFileNamedByItsExtensionWhileAnInvoiceStaysAPictureOrAPdf()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await AddAsync(ledger, "جدول الكميات")).EnsureSuccessStatusCode();
        (await AddAsync(ledger.Client, ledger.ProjectId, "مخطط أوتوكاد", "", [0x41, 0x43, 0x31, 0x30, .. Guid.NewGuid().ToByteArray()], "site.dwg")).EnsureSuccessStatusCode();

        var seen = await ListAsync(ledger.Client, ledger.ProjectId);

        Assert.Equal("application/octet-stream", seen.Single(document => document.FileName == "site.dwg").ContentType);
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", seen.Single(document => document.FileName == "contract.docx").ContentType);
        var invoice = await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/attachments", new MultipartFormDataContent { { new ByteArrayContent(WordFile()), "file", "invoice.docx" } });
        Assert.Equal("invalidFileType", await Ledger.CodeOf(invoice));
    }

    [Fact]
    public async Task ADocumentNeedsANameAGroupOf100LettersAtMostAFileOf25MbAtMostAndIsKeptOnce()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var file = WordFile();

        Assert.Equal("invalidName", await Ledger.CodeOf(await AddAsync(ledger, " ")));
        Assert.Equal("invalidName", await Ledger.CodeOf(await AddAsync(ledger, new string('م', Document.NameMaxLength + 1))));
        Assert.Equal("invalidGroup", await Ledger.CodeOf(await AddAsync(ledger, "مستند", new string('م', Document.GroupMaxLength + 1))));
        Assert.Equal("fileTooLarge", await Ledger.CodeOf(await AddAsync(ledger.Client, ledger.ProjectId, "مستند", "", new byte[Attachment.AnyFileLimitBytes + 1], "big.zip")));
        (await AddAsync(ledger.Client, ledger.ProjectId, "مستند", "", file, "a.docx")).EnsureSuccessStatusCode();
        Assert.Equal("documentAlreadyKept", await Ledger.CodeOf(await AddAsync(ledger.Client, ledger.ProjectId, "مستند آخر", "", file, "a.docx")));
    }

    [Fact]
    public void Findings137_ADocumentsRequestMayHoldAsMuchAsAFormSoAFileOver25MbIsAnsweredRatherThanCutOff()
    {
        var add = factory.Services.GetRequiredService<IEnumerable<EndpointDataSource>>().SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .Single(route => route.RoutePattern.RawText!.EndsWith("/documents") && route.Metadata.GetOrderedMetadata<IHttpMethodMetadata>().Any(metadata => metadata.HttpMethods.Contains("POST")));

        Assert.Equal(FormOptions.DefaultMultipartBodyLengthLimit, add.Metadata.GetMetadata<IRequestSizeLimitMetadata>()!.MaxRequestBodySize);
    }

    [Fact]
    public async Task ARemovedDocumentLeavesTheListButItsFileStays()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await AddAsync(ledger, "مخطط الكهرباء", "مخططات")).EnsureSuccessStatusCode();
        var kept = Assert.Single(await ListAsync(ledger.Client, ledger.ProjectId));

        (await ledger.Client.DeleteAsync($"/projects/{ledger.ProjectId}/documents/{kept.Id}")).EnsureSuccessStatusCode();

        Assert.Empty(await ListAsync(ledger.Client, ledger.ProjectId));
        Assert.Equal(HttpStatusCode.OK, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/attachments/{kept.AttachmentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/blueprints")).StatusCode);
    }
}
