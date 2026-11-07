using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class DuplicateInvoiceTests(MigratedApiFactory factory)
{
    private const string Password = "Duplicates-pass-1";
    private const string Finance = "Finance";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private sealed record Setup(HttpClient Client, ProjectView Project, Guid ItemId, Guid MethodId, Guid AccountId, Guid SupplierId);

    private async Task<Setup> InProjectAsync(HttpClient client, Redix.Api.Auth.User user, string name, string vatNumber)
    {
        var project = await factory.ApprovedProjectAsync(client, name, "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        var supplier = await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers", new SupplierRequest("شركة النخيل") { VatNumber = vatNumber }, Json);
        var item = (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories/offered?itemType={ItemType.Purchases}", Json))!.SelectMany(category => category.Items).First();
        var method = (await client.GetFromJsonAsync<List<PaymentMethodView>>($"/projects/{project.Id}/payment-methods", Json))![0];
        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حساب", AccountType.Bank, 0), Json)).EnsureSuccessStatusCode();
        var account = (await client.GetFromJsonAsync<List<AccountView>>($"/projects/{project.Id}/accounts", Json))!.Single();
        return new Setup(client, project, item.Id, method.Id, account.Id, (await supplier.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!.Id);
    }

    private async Task<Setup> FinanceAsync(string name = "غصون 6", string vatNumber = "")
    {
        var user = await factory.AddUserAsync(Password);
        return await InProjectAsync(await factory.ClientSignedInAsAsync(user, Password), user, name, vatNumber.Length > 0 ? vatNumber : MigratedApiFactory.NewVatNumber());
    }

    private ExpenseRequest Expense(Setup setup, string number = "F4858") => new(Guid.CreateVersion7())
    {
        ItemId = setup.ItemId,
        UnitId = setup.Project.Units[0].Id,
        SupplierId = setup.SupplierId,
        InvoiceNumber = number,
        InvoiceDate = Entry.Today(factory.Time),
        Amount = 1150m,
        PaymentMethodId = setup.MethodId,
        AccountId = setup.AccountId,
    };

    private static Task<HttpResponseMessage> PostAsync(Setup setup, ExpenseRequest request) =>
        setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/expenses", request, Json);

    private static async Task<Guid> ImageAsync(Setup setup, byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await setup.Client.PostAsync($"/projects/{setup.Project.Id}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id;
    }

    private static byte[] AnInvoiceFile() => [0xFF, 0xD8, 0xFF, 0xE0, .. Guid.NewGuid().ToByteArray()];

    private async Task<int> EntriesNamedAsync(Guid projectId)
    {
        var count = 0;
        await factory.WithDatabaseAsync(async db => count = await db.Entries.CountAsync(entry => entry.ProjectId == projectId));
        return count;
    }

    [Fact]
    public async Task AC_17_1_TheSameSupplierAndNumberAnswerAWarningWithTheEarlierEntryAndSaveNothing()
    {
        var setup = await FinanceAsync();
        (await PostAsync(setup, Expense(setup))).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Expense(setup));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var answer = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("duplicateInvoice", answer.GetProperty("code").GetString());
        var earlier = answer.GetProperty("earlier").Deserialize<EarlierEntryView>(Json)!;
        Assert.Equal("غصون 6", earlier.ProjectName);
        Assert.Equal(1150m, earlier.Total);
        Assert.Equal(Entry.Today(factory.Time), earlier.InvoiceDate);
        Assert.Equal(1, await EntriesNamedAsync(setup.Project.Id));
    }

    [Fact]
    public async Task AnInvoiceWithNoNumberIsKeptAndNeverWarnsOfAnother()
    {
        var setup = await FinanceAsync();
        (await PostAsync(setup, Expense(setup, ""))).EnsureSuccessStatusCode();

        (await PostAsync(setup, Expense(setup, ""))).EnsureSuccessStatusCode();

        Assert.Equal(2, await EntriesNamedAsync(setup.Project.Id));
    }

    [Fact]
    public async Task AC_17_1_ConfirmingTheWarningSavesIt()
    {
        var setup = await FinanceAsync();
        (await PostAsync(setup, Expense(setup))).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Expense(setup) with { ConfirmDuplicate = true });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(2, await EntriesNamedAsync(setup.Project.Id));
    }

    [Fact]
    public async Task TheNumberIsComparedTrimmed()
    {
        var setup = await FinanceAsync();
        (await PostAsync(setup, Expense(setup))).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Conflict, (await PostAsync(setup, Expense(setup, " F4858 "))).StatusCode);
    }

    [Fact]
    public async Task AnotherNumberFromTheSameSupplierNeedsNoConfirmation()
    {
        var setup = await FinanceAsync();
        (await PostAsync(setup, Expense(setup))).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup, Expense(setup, "F4859"))).StatusCode);
    }

    [Fact]
    public async Task TheSameNumberFromAnotherSupplierNeedsNoConfirmation()
    {
        var setup = await FinanceAsync();
        var other = await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/suppliers", new SupplierRequest("مؤسسة الواحة") { VatNumber = MigratedApiFactory.NewVatNumber() }, Json);
        (await PostAsync(setup, Expense(setup))).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Expense(setup) with { SupplierId = (await other.Content.ReadFromJsonAsync<SupplierView>(Json))!.Id });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task FR_17_TheSameVatNumberInAnotherProjectOfTheCallerIsTheSameSupplierAndIsNamed()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var vatNumber = MigratedApiFactory.NewVatNumber();
        var first = await InProjectAsync(client, user, "غصون 6", vatNumber);
        var second = await InProjectAsync(client, user, "غصون 7", vatNumber);
        (await PostAsync(first, Expense(first))).EnsureSuccessStatusCode();

        var response = await PostAsync(second, Expense(second));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var earlier = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("earlier").Deserialize<EarlierEntryView>(Json)!;
        Assert.Equal("غصون 6", earlier.ProjectName);
    }

    [Fact]
    public async Task AC_17_1_AnEarlierEntryInAProjectTheCallerCannotReadIsNeverMentioned()
    {
        var vatNumber = MigratedApiFactory.NewVatNumber();
        var first = await FinanceAsync("غصون 6", vatNumber);
        var second = await FinanceAsync("غصون 7", vatNumber);
        (await PostAsync(first, Expense(first))).EnsureSuccessStatusCode();

        var response = await PostAsync(second, Expense(second));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("غصون 6", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AC_17_1_OfSeveralEarlierEntriesTheWarningNamesOneTheCallerCanRead()
    {
        var vatNumber = MigratedApiFactory.NewVatNumber();
        var hidden = await FinanceAsync("غصون 5", vatNumber);
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var readable = await InProjectAsync(client, user, "غصون 6", vatNumber);
        var current = await InProjectAsync(client, user, "غصون 7", vatNumber);
        (await PostAsync(hidden, Expense(hidden))).EnsureSuccessStatusCode();
        (await PostAsync(readable, Expense(readable))).EnsureSuccessStatusCode();

        var response = await PostAsync(current, Expense(current));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var earlier = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("earlier").Deserialize<EarlierEntryView>(Json)!;
        Assert.Equal("غصون 6", earlier.ProjectName);
    }

    [Fact]
    public async Task ASupplierWithNoVatNumberIsComparedInItsOwnProjectOnly()
    {
        var first = await FinanceAsync("غصون 6");
        var second = await FinanceAsync("غصون 7");
        Task<Guid> CashSupplierOf(Setup setup) => setup.Client.GetFromJsonAsync<List<SupplierView>>($"/projects/{setup.Project.Id}/suppliers", Json).ContinueWith(task => task.Result!.Single(row => row.Name == Supplier.GeneralCashSupplierName).Id);
        (await PostAsync(first, Expense(first) with { SupplierId = await CashSupplierOf(first) })).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(second, Expense(second) with { SupplierId = await CashSupplierOf(second) })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await PostAsync(first, Expense(first) with { SupplierId = await CashSupplierOf(first) })).StatusCode);
    }

    [Fact]
    public async Task BR_22_TheSameFileOnASecondExpenseIsRefusedEvenWhenConfirmed()
    {
        var setup = await FinanceAsync();
        var attachmentId = await ImageAsync(setup, AnInvoiceFile());
        (await PostAsync(setup, Expense(setup) with { AttachmentId = attachmentId })).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Expense(setup, "F4859") with { AttachmentId = attachmentId, ConfirmDuplicate = true });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("invoiceAlreadyAttached", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task BR_22_TheSameFileSentAgainIsTheSameInvoiceAndIsRefused()
    {
        var setup = await FinanceAsync();
        var file = AnInvoiceFile();
        (await PostAsync(setup, Expense(setup) with { AttachmentId = await ImageAsync(setup, file) })).EnsureSuccessStatusCode();

        var response = await PostAsync(setup, Expense(setup, "F4859") with { AttachmentId = await ImageAsync(setup, file) });

        Assert.Equal("invoiceAlreadyAttached", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task AFileKeptAndOnNoEntryIsFreeToUse()
    {
        var setup = await FinanceAsync();
        await ImageAsync(setup, AnInvoiceFile());

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup, Expense(setup) with { AttachmentId = await ImageAsync(setup, AnInvoiceFile()) })).StatusCode);
    }
}
