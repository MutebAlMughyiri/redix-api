using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Redix.Api.Attachments;
using Redix.Api.History;
using Redix.Api.Contracts;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(ThirdDatabaseCollection.Name)]
public class AttachmentsTests(MigratedApiFactory factory)
{
    private const string Password = "Attachments-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 1, 2, 3];
    private static readonly byte[] Pdf = [0x25, 0x50, 0x44, 0x46, 1, 2, 3];

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6", "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (client, project);
    }

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient client, Guid projectId, byte[] content, string fileName = "invoice.jpg", string contentType = "image/jpeg")
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return client.PostAsync($"/projects/{projectId}/attachments", new MultipartFormDataContent { { file, "file", fileName } });
    }

    private static async Task<AttachmentView> UploadedAsync(HttpClient client, Guid projectId, byte[] content)
    {
        var response = await UploadAsync(client, projectId, content);
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!;
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private async Task<Attachment> StoredAsync(Guid attachmentId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<RedixDbContext>().Attachments.SingleAsync(row => row.Id == attachmentId);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("application/pdf")]
    public async Task APictureOrAPdfIsKeptWithItsNameTypeSizeAndHash(string contentType)
    {
        var (client, project) = await FinanceAsync();
        var content = contentType switch { "image/jpeg" => Jpeg, "image/png" => Png, _ => Pdf };

        var response = await UploadAsync(client, project.Id, content, "فاتورة 2617", contentType);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var view = (await response.Content.ReadFromJsonAsync<AttachmentView>(Json))!;
        Assert.Equal("فاتورة 2617", view.FileName);
        Assert.Equal(contentType, view.ContentType);
        Assert.Equal(content.Length, view.SizeBytes);
        var stored = await StoredAsync(view.Id);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(content)), stored.Sha256);
        Assert.Equal(project.Id, stored.ProjectId);
    }

    [Fact]
    public async Task TheSameFileSentAgainIsKeptOnce()
    {
        var (client, project) = await FinanceAsync();

        var first = await (await UploadAsync(client, project.Id, Png)).Content.ReadFromJsonAsync<AttachmentView>(Json);
        var again = await UploadAsync(client, project.Id, Png);

        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(first!.Id, (await again.Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id);
    }

    [Fact]
    public async Task WhatAFileIsComesFromItsBytesNotFromWhatTheSenderSays()
    {
        var (client, project) = await FinanceAsync();

        var refused = await UploadAsync(client, project.Id, "<script>"u8.ToArray(), "invoice.pdf", "application/pdf");
        var kept = await UploadAsync(client, project.Id, Pdf, "invoice.png", "image/png");

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidFileType", await CodeOf(refused));
        Assert.Equal("application/pdf", (await kept.Content.ReadFromJsonAsync<AttachmentView>(Json))!.ContentType);
    }

    [Fact]
    public async Task AFileThatIsNotAPictureOrAPdfIsRefused()
    {
        var (client, project) = await FinanceAsync();

        var response = await UploadAsync(client, project.Id, "PK zip"u8.ToArray(), "invoice.zip", "application/zip");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalidFileType", await CodeOf(response));
    }

    [Fact]
    public async Task Row_138_ADocumentsFileMayBeAnyFileThoughAnEntrysMayNot()
    {
        var (client, project) = await FinanceAsync();
        var file = new ByteArrayContent("PK zip"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        var kept = await client.PostAsync($"/projects/{project.Id}/attachments/document", new MultipartFormDataContent { { file, "file", "quantities.zip" } });
        var refused = await UploadAsync(client, project.Id, "PK zip"u8.ToArray(), "quantities.zip", "application/zip");

        Assert.Equal(HttpStatusCode.Created, kept.StatusCode);
        Assert.Equal("quantities.zip", (await kept.Content.ReadFromJsonAsync<AttachmentView>(Json))!.FileName);
        Assert.Equal("invalidFileType", await CodeOf(refused));
    }

    [Fact]
    public async Task APictureOf1MbOrMoreIsRefusedAndOneJustBelowIsKept()
    {
        var (client, project) = await FinanceAsync();
        var large = new byte[Attachment.ImageLimitBytes];
        Jpeg.CopyTo(large, 0);

        var refused = await UploadAsync(client, project.Id, large);
        var kept = await UploadAsync(client, project.Id, large[..^1]);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("imageTooLarge", await CodeOf(refused));
        Assert.Equal(HttpStatusCode.Created, kept.StatusCode);
    }

    [Fact]
    public async Task APdfOf1MbOrMoreIsKept()
    {
        var (client, project) = await FinanceAsync();
        var large = new byte[Attachment.ImageLimitBytes * 2];
        Pdf.CopyTo(large, 0);

        Assert.Equal(HttpStatusCode.Created, (await UploadAsync(client, project.Id, large, "contract.pdf", "application/pdf")).StatusCode);
    }

    [Fact]
    public async Task TheFileIsKeptOutsideTheDatabaseAndServedBackUnchanged()
    {
        var (client, project) = await FinanceAsync();
        var view = await UploadedAsync(client, project.Id, Jpeg);

        var served = await client.GetAsync($"/projects/{project.Id}/attachments/{view.Id}");

        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal("image/jpeg", served.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Jpeg, await served.Content.ReadAsByteArrayAsync());
        var options = factory.Services.GetRequiredService<IOptions<AttachmentOptions>>().Value;
        var stored = await StoredAsync(view.Id);
        Assert.Equal(Jpeg, await File.ReadAllBytesAsync(options.PathOf(stored)));
        Assert.Equal($"{project.Id}/{view.Id}.jpg", stored.StoragePath);
    }

    [Fact]
    public async Task ANameTheSenderChoseNeverReachesThePathOfTheFile()
    {
        var (client, project) = await FinanceAsync();

        var response = await UploadAsync(client, project.Id, Jpeg, "../../outside.jpg");

        var view = (await response.Content.ReadFromJsonAsync<AttachmentView>(Json))!;
        Assert.Equal("outside.jpg", view.FileName);
        Assert.Equal($"{project.Id}/{view.Id}.jpg", (await StoredAsync(view.Id)).StoragePath);
    }

    [Theory]
    [InlineData(Field)]
    [InlineData(Finance)]
    public async Task FieldAndFinanceKeepFiles(string role)
    {
        var (client, project) = await FinanceAsync();
        var member = await MemberAsync(project, role);

        Assert.Equal(HttpStatusCode.Created, (await UploadAsync(member, project.Id, Jpeg)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/projects/{project.Id}/attachments/{(await UploadedAsync(member, project.Id, Jpeg)).Id}")).StatusCode);
    }

    [Theory]
    [InlineData(Reader)]
    [InlineData(Manager)]
    public async Task WhoHoldsNoAttachFilesCannotKeepAFileButReadsIt(string role)
    {
        var (client, project) = await FinanceAsync();
        var member = await MemberAsync(project, role);
        var view = await UploadedAsync(client, project.Id, Jpeg);

        Assert.Equal(HttpStatusCode.Forbidden, (await UploadAsync(member, project.Id, Jpeg)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await member.GetAsync($"/projects/{project.Id}/attachments/{view.Id}")).StatusCode);
    }

    [Fact]
    public async Task SomeoneWithNoAccessToTheProjectCannotReadOrKeepAFile()
    {
        var (client, project) = await FinanceAsync();
        var view = await UploadedAsync(client, project.Id, Jpeg);
        var outsider = await factory.AddUserAsync(Password);
        var outsiderClient = await factory.ClientSignedInAsAsync(outsider, Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await outsiderClient.GetAsync($"/projects/{project.Id}/attachments/{view.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await UploadAsync(outsiderClient, project.Id, Jpeg)).StatusCode);
    }

    [Fact]
    public async Task AFileOfAnotherProjectIsNotFoundEvenForSomeoneInBoth()
    {
        var (client, project) = await FinanceAsync();
        var view = await UploadedAsync(client, project.Id, Jpeg);
        var other = await factory.ApprovedProjectAsync(client, "غصون 7", "الفلة الشرقية");

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/projects/{other.Id}/attachments/{view.Id}")).StatusCode);
    }

    [Fact]
    public async Task WithoutASessionNoFileIsServed()
    {
        var (client, project) = await FinanceAsync();
        var view = await UploadedAsync(client, project.Id, Jpeg);

        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync($"/projects/{project.Id}/attachments/{view.Id}")).StatusCode);
    }

    [Fact]
    public async Task AFinishedProjectKeepsNoNewFileAndStillServesItsOwn()
    {
        var (client, project) = await FinanceAsync();
        var view = await UploadedAsync(client, project.Id, Jpeg);
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        var refused = await UploadAsync(client, project.Id, Jpeg);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("projectFinished", await CodeOf(refused));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/projects/{project.Id}/attachments/{view.Id}")).StatusCode);
    }

    [Fact]
    public async Task KeepingAFileIsInTheHistory()
    {
        var (client, project) = await FinanceAsync();

        var view = await UploadedAsync(client, project.Id, Jpeg);

        var record = Assert.Single(await factory.HistoryOfAsync(view.Id));
        Assert.Equal(HistoryAction.Attach, record.Action);
        Assert.Equal("Attachments", record.TableName);
    }

    private static async Task<AddContractRequest> ContractRequestFor(HttpClient client, ProjectView project)
    {
        var contractor = (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{project.Id}/suppliers", Json))!.Single(supplier => supplier.Name == Supplier.GeneralCashSupplierName);
        var item = (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories/offered?itemType={ItemType.ContractorPayments}", Json))!.SelectMany(category => category.Items).First();
        return new AddContractRequest(contractor.Id, item.Id, 34000m, false);
    }

    private static async Task<List<ContractView>> ContractsOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<ContractView>>($"/projects/{projectId}/contracts", Json))!;

    [Fact]
    public async Task AContractCarriesItsDocumentFromWhenItIsAdded()
    {
        var (client, project) = await FinanceAsync();
        var document = await UploadedAsync(client, project.Id, Pdf);
        var request = await ContractRequestFor(client, project) with { AttachmentIds = [document.Id] };

        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts", request, Json)).EnsureSuccessStatusCode();

        Assert.Equal(document.Id, Assert.Single(Assert.Single(await ContractsOf(client, project.Id)).Documents).Id);
    }

    [Fact]
    public async Task AnAmendmentAddsADocumentAndKeepsThoseAlreadyCarried()
    {
        var (client, project) = await FinanceAsync();
        var first = await UploadedAsync(client, project.Id, Pdf);
        var second = await UploadedAsync(client, project.Id, Jpeg);
        var request = await ContractRequestFor(client, project) with { AttachmentIds = [first.Id] };
        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts", request, Json)).EnsureSuccessStatusCode();
        var contract = Assert.Single(await ContractsOf(client, project.Id));

        (await client.PutAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(34000m, false) { AttachmentIds = [second.Id, first.Id] }, Json)).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync($"/projects/{project.Id}/contracts/{contract.Id}", new ContractTerms(35000m, false), Json)).EnsureSuccessStatusCode();

        Assert.Equal([first.Id, second.Id], Assert.Single(await ContractsOf(client, project.Id)).Documents.Select(document => document.Id));
    }

    [Fact]
    public async Task ADocumentOfAnotherProjectIsRefusedOnAContract()
    {
        var (client, project) = await FinanceAsync();
        var other = await factory.ApprovedProjectAsync(client, "غصون 7", "الفلة الشرقية");
        await factory.WithDatabaseAsync(async db =>
        {
            var userId = await db.ProjectPermissions.Where(held => held.ProjectId == project.Id).Select(held => held.UserId).FirstAsync();
            await factory.GiveAsync(userId, other.Id, Holds.Of(Finance));
        });
        var document = await UploadedAsync(client, other.Id, Pdf);
        var request = await ContractRequestFor(client, project) with { AttachmentIds = [document.Id] };

        var response = await client.PostAsJsonAsync($"/projects/{project.Id}/contracts", request, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("unknownAttachment", await CodeOf(response));
    }

    [Fact]
    public async Task LinkingADocumentToAContractIsInTheHistoryWithWhichOne()
    {
        var (client, project) = await FinanceAsync();
        var document = await UploadedAsync(client, project.Id, Pdf);
        var request = await ContractRequestFor(client, project) with { AttachmentIds = [document.Id] };

        (await client.PostAsJsonAsync($"/projects/{project.Id}/contracts", request, Json)).EnsureSuccessStatusCode();

        var contract = Assert.Single(await ContractsOf(client, project.Id));
        var link = Assert.Single(await factory.HistoryOfAsync(contract.Id), record => record.Action == HistoryAction.Attach);
        Assert.Equal("ContractAttachments", link.TableName);
        Assert.Contains(document.Id.ToString(), link.After);
    }
}
