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

/// <summary>What the ledger's test classes share: an active project with Finance signed in, and the way to enter into it and read it back.</summary>
public sealed record Ledger(MigratedApiFactory Factory, HttpClient Client, ProjectView Project, Guid ItemId, Guid TableId, Guid SupplierId, Guid MethodId, Guid AccountId)
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
        return new Ledger(factory, client, project, item.Id, item.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, supplierId, method.Id, account.Id);
    }

    public DateOnly Today => Entry.Today(Factory.Time);

    public async Task<(HttpClient Client, Guid UserId)> MemberAsync(string holds)
    {
        var user = await Factory.AddUserAsync(Password);
        await Factory.GiveAsync(user.Id, ProjectId, Holds.Of(holds));
        return (await Factory.ClientSignedInAsAsync(user, Password), user.Id);
    }

    /// <summary>The project's revenue item of <paramref name="type"/>: the template gives every project one of each.</summary>
    public Task<Guid> RevenueItemAsync(ItemType type) => ItemOfTypeAsync(Client, ProjectId, type);

    /// <summary>The first item of <paramref name="type"/> in the project <paramref name="projectId"/>, as <paramref name="client"/> reads it.</summary>
    public static async Task<Guid> ItemOfTypeAsync(HttpClient client, Guid projectId, ItemType type) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories", Json))!.SelectMany(category => category.Items).First(item => item.Type == type).Id;

    /// <summary>The project's رأس المال, whose tables its partners are (redix-api #109): the template gives every project one.</summary>
    public static Task<Guid> CapitalItemOfAsync(HttpClient client, Guid projectId) => ItemOfTypeAsync(client, projectId, ItemType.Capital);

    /// <summary>A purchase of <paramref name="amount"/> today, in the general table of the ledger's item, from its supplier, by its method and account (redix-api #96).</summary>
    public EntryRequest Purchase(decimal amount = 1150m) => new(Guid.CreateVersion7())
    {
        ItemId = ItemId,
        TableId = TableId,
        SupplierId = SupplierId,
        Date = Today,
        Amount = amount,
        PaymentMethodId = MethodId,
        AccountId = AccountId,
    };

    /// <summary>A deduction of <paramref name="amount"/>, below zero, saying why in its notes (D14).</summary>
    public EntryRequest Deduction(decimal amount, string notes = "خصم من المورد") => Purchase(amount) with { Notes = notes };

    /// <summary>A revenue entry of <paramref name="amount"/> today, sent for approval, in the general table of the project's revenue item of <paramref name="type"/>.</summary>
    public async Task<EntryRequest> RevenueAsync(ItemType type, decimal amount, string notes = "") => await RevenueOnAsync(await RevenueItemAsync(type), amount, notes);

    /// <summary>A revenue entry of <paramref name="amount"/> today, sent for approval, in the general table of the item <paramref name="itemId"/>, into the ledger's account.</summary>
    public async Task<EntryRequest> RevenueOnAsync(Guid itemId, decimal amount, string notes = "") => new(Guid.CreateVersion7())
    {
        ItemId = itemId,
        TableId = (await Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{ProjectId}/categories", Json))!.SelectMany(category => category.Items).Single(item => item.Id == itemId).Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id,
        Date = Today,
        Amount = amount,
        AccountId = AccountId,
        Notes = notes,
        Submit = true,
    };

    /// <summary>What the partner <paramref name="partnerId"/> puts in today, or takes out as a withdrawal, sent for approval through the ledger's account (redix-api #109, #110).</summary>
    public async Task<EntryRequest> PartnerEntryAsync(Guid partnerId, decimal amount, bool isWithdrawal = false, string notes = "") => new(Guid.CreateVersion7())
    {
        ItemId = await ReadAsync(db => db.Partners.Where(partner => partner.Id == partnerId).Select(partner => partner.ItemId).SingleAsync()),
        TableId = partnerId,
        Date = Today,
        Amount = amount,
        AccountId = AccountId,
        Notes = notes,
        IsWithdrawal = isWithdrawal,
        Submit = true,
    };

    /// <summary>Posts <paramref name="request"/> to the one route every entry is recorded through (redix-api #110), as <paramref name="client"/> or else Finance.</summary>
    public Task<HttpResponseMessage> PostAsync(EntryRequest request, HttpClient? client = null) => (client ?? Client).PostAsJsonAsync($"/projects/{ProjectId}/entries", request, Json);

    /// <summary>Posts <paramref name="request"/> as a change of its entry (UC-033), as <paramref name="client"/> or else Finance.</summary>
    public Task<HttpResponseMessage> ChangeAsync(EntryRequest request, HttpClient? client = null) => (client ?? Client).PostAsJsonAsync($"/projects/{ProjectId}/entries/{request.Id}", request, Json);

    public async Task<RecordedEntryView> RecordAsync(EntryRequest request) => (await (await PostAsync(request)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<RecordedEntryView>(Json))!;

    public static async Task<string> CodeOf(HttpResponseMessage response) => (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    public async Task AssertRefusedAsync(EntryRequest request, string code)
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

    /// <summary>Approves the entry <paramref name="sent"/> answered, as Finance does with what waits (BR-30, redix-api #101), and answers its id.</summary>
    public async Task<Guid> ApproveAsync(HttpResponseMessage sent) => await ApproveAsync((await sent.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id);

    /// <summary>Approves the entry <paramref name="id"/>, as Finance does with what waits, and answers its id.</summary>
    public async Task<Guid> ApproveAsync(Guid id)
    {
        (await Client.PostAsJsonAsync($"/projects/{ProjectId}/entries/approve", new ApproveEntriesRequest([id]), Json)).EnsureSuccessStatusCode();
        return id;
    }

    public async Task<Guid> FileAsync()
    {
        var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, .. Guid.NewGuid().ToByteArray()]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await Client.PostAsync($"/projects/{ProjectId}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id;
    }
}
