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

/// <summary>What the ledger's test classes share: a running project with Finance signed in, and the way to enter into it and read it back.</summary>
public sealed record Ledger(MigratedApiFactory Factory, HttpClient Client, ProjectView Project, Guid ItemId, Guid SupplierId, Guid MethodId, Guid AccountId)
{
    public const string Password = "Ledger-pass-1";

    public static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public Guid ProjectId => Project.Id;

    public static async Task<Ledger> WithFinanceAsync(MigratedApiFactory factory)
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, "غصون 6", "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of("Finance"));
        var supplier = await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers", new SupplierRequest("شركة النخيل") { VatNumber = MigratedApiFactory.NewVatNumber() }, Json);
        var supplierId = (await supplier.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!.Id;
        var item = (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{project.Id}/categories/offered?itemType={ItemType.Purchases}", Json))!.SelectMany(category => category.Items).First();
        var method = (await client.GetFromJsonAsync<List<PaymentMethodView>>($"/projects/{project.Id}/payment-methods", Json))![0];
        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حساب المؤسسة", AccountType.Bank, 0), Json)).EnsureSuccessStatusCode();
        var account = (await client.GetFromJsonAsync<List<AccountView>>($"/projects/{project.Id}/accounts", Json))!.Single();
        return new Ledger(factory, client, project, item.Id, supplierId, method.Id, account.Id);
    }

    public DateOnly Today => Entry.Today(Factory.Time);

    public async Task<(HttpClient Client, Guid UserId)> MemberAsync(string holds)
    {
        var user = await Factory.AddUserAsync(Password);
        await Factory.GiveAsync(user.Id, ProjectId, Holds.Of(holds));
        return (await Factory.ClientSignedInAsAsync(user, Password), user.Id);
    }

    /// <summary>The project's revenue item of <paramref name="type"/>: the template gives every project one of each.</summary>
    public async Task<Guid> RevenueItemAsync(ItemType type) =>
        (await Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ProjectId}/categories", Json))!.SelectMany(category => category.Items).First(item => item.Type == type).Id;

    public ExpenseRequest Purchase(decimal amount = 1150m, string number = "") => new(Guid.CreateVersion7())
    {
        ItemId = ItemId,
        SupplierId = SupplierId,
        InvoiceNumber = number.Length > 0 ? number : Guid.NewGuid().ToString("N")[..12],
        InvoiceDate = Today,
        Amount = amount,
        PaymentMethodId = MethodId,
        AccountId = AccountId,
    };

    public ExpenseRequest WithoutInvoice(decimal amount = 200m, string reason = "شراء نقدي من السوق") => new(Guid.CreateVersion7())
    {
        ItemId = ItemId,
        SupplierId = SupplierId,
        WithoutInvoiceReason = reason,
        PaymentDate = Today,
        Amount = amount,
        PaymentMethodId = MethodId,
        AccountId = AccountId,
    };

    public Task<HttpResponseMessage> PostAsync(ExpenseRequest request) => Client.PostAsJsonAsync($"/projects/{ProjectId}/expenses", request, Json);

    public async Task<ExpenseView> RecordAsync(ExpenseRequest request) => (await (await PostAsync(request)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ExpenseView>(Json))!;

    public static async Task<string> CodeOf(HttpResponseMessage response) => (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    public async Task AssertRefusedAsync(ExpenseRequest request, string code)
    {
        var response = await PostAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await CodeOf(response));
    }

    public async Task<T> ReadAsync<T>(Func<RedixDbContext, Task<T>> read)
    {
        var result = default(T)!;
        await Factory.WithDatabaseAsync(async db => result = await read(db));
        return result;
    }

    public Task SetStatusAsync(Guid entryId, EntryStatus status) =>
        Factory.WithDatabaseAsync(db => db.Entries.Where(entry => entry.Id == entryId).ExecuteUpdateAsync(entry => entry.SetProperty(row => row.Status, status)));

    public async Task<Guid> FileAsync()
    {
        var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, .. Guid.NewGuid().ToByteArray()]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await Client.PostAsync($"/projects/{ProjectId}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id;
    }
}
