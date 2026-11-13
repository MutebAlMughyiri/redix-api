using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.History;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(SecondDatabaseCollection.Name)]
public class ExpensesTests(MigratedApiFactory factory)
{
    private const string Password = "Expenses-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private sealed record Setup(HttpClient Client, ProjectView Project, Guid ItemId, Guid TableId, Guid SupplierId, Guid MethodId, Guid AccountId);

    private async Task<Setup> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name, "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        var supplier = await SupplierAsync(client, project.Id, "شركة النخيل", MigratedApiFactory.NewVatNumber());
        var methods = (await client.GetFromJsonAsync<List<PaymentMethodView>>($"/projects/{project.Id}/payment-methods", Json))!;
        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حساب المؤسسة", AccountType.Bank), Json)).EnsureSuccessStatusCode();
        var account = (await client.GetFromJsonAsync<List<AccountView>>($"/projects/{project.Id}/accounts", Json))!.Single();
        var item = await PurchaseItemAsync(client, project.Id);
        return new Setup(client, project, item.Id, item.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, supplier.Id, methods[0].Id, account.Id);
    }

    private static async Task<SupplierView> SupplierAsync(HttpClient client, Guid projectId, string name, string vatNumber)
    {
        var response = await client.PostAsJsonAsync($"/projects/{projectId}/suppliers", new SupplierRequest(name) { VatNumber = vatNumber }, Json);
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!;
    }

    private static async Task<ItemView> PurchaseItemAsync(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<CategoryView>>($"/projects/{projectId}/categories/offered?itemType={ItemType.Purchases}", Json))!.SelectMany(category => category.Items).First();

    private async Task<(HttpClient Client, Guid UserId)> MemberAsync(Setup setup, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, setup.Project.Id, Holds.Of(role));
        return (await factory.ClientSignedInAsAsync(user, Password), user.Id);
    }

    private EntryRequest Expense(Setup setup, decimal amount = 6000m) => new(Guid.CreateVersion7())
    {
        ItemId = setup.ItemId,
        TableId = setup.TableId,
        SupplierId = setup.SupplierId,
        Date = Entry.Today(factory.Time),
        Amount = amount,
        PaymentMethodId = setup.MethodId,
        AccountId = setup.AccountId,
    };

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid projectId, EntryRequest request) =>
        client.PostAsJsonAsync($"/projects/{projectId}/entries", request, Json);

    private static async Task<RecordedEntryView> RecordAsync(HttpClient client, Guid projectId, EntryRequest request) =>
        (await (await PostAsync(client, projectId, request)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<RecordedEntryView>(Json))!;

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static async Task AssertRefusedAsync(Setup setup, EntryRequest request, string code)
    {
        var response = await PostAsync(setup.Client, setup.Project.Id, request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await CodeOf(response));
    }

    private async Task<T> ReadAsync<T>(Func<RedixDbContext, Task<T>> read)
    {
        var result = default(T)!;
        await factory.WithDatabaseAsync(async db => result = await read(db));
        return result;
    }

    private Task<Entry> EntryOfAsync(Guid entryId) => ReadAsync(db => db.Entries.SingleAsync(row => row.Id == entryId));

    private static async Task<Guid> ImageAsync(HttpClient client, Guid projectId)
    {
        var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256)]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await client.PostAsync($"/projects/{projectId}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id;
    }

    private static async Task<string> CustomColumnAsync(HttpClient client, Guid projectId, Guid itemId, ColumnDataType dataType, bool isRequired, params List<string> choices)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/columns", new AddColumnRequest("المساحة م²", dataType, isRequired, false, [.. choices.Select(name => new ChoiceRequest(name))]), Json)).EnsureSuccessStatusCode();
        return (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{projectId}/items/{itemId}/columns", Json))!.Last().Key;
    }

    [Fact]
    public async Task BR_01_LeftWithoutAnAmountTheTotalIsThePriceTimesTheQuantity()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 0m) with { UnitPrice = 230m, Quantity = 5m });

        Assert.Equal(1150m, view.Amount);
    }

    [Fact]
    public async Task AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 1150m) with { UnitPrice = 999m, Quantity = 2m, Product = " مواد لياسة " });

        Assert.Equal(1150m, view.Amount);
        var purchase = await ReadAsync(db => db.PurchaseDetails.SingleAsync(row => row.EntryId == view.Id));
        Assert.Equal(999m, purchase.UnitPrice);
        Assert.Equal(2m, purchase.Quantity);
        Assert.Equal("مواد لياسة", purchase.Product);
    }

    [Fact]
    public async Task D10_AnExpenseIsItsTotalOnItsOneDate()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup);

        var entry = await EntryOfAsync((await RecordAsync(setup.Client, setup.Project.Id, request)).Id);

        Assert.Equal(request.Date, entry.EntryDate);
        Assert.Equal(6000m, entry.Amount);
    }

    [Fact]
    public async Task AnExpenseIsARowInTheLedgerWithItsDetailsAndTheIdThePhoneMade()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup) with { Notes = " دفعة أولى " };

        await RecordAsync(setup.Client, setup.Project.Id, request);

        var entry = await EntryOfAsync(request.Id);
        Assert.Equal(EntryKind.Purchase, entry.Kind);
        Assert.Equal(setup.Project.Id, entry.ProjectId);
        Assert.Equal(setup.AccountId, entry.AccountId);
        Assert.Equal("دفعة أولى", entry.Notes);
        Assert.Equal(Entry.NotApproved, entry.ApprovedAt);
        var expense = await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == request.Id));
        Assert.Equal(setup.ItemId, expense.ItemId);
        Assert.Equal(setup.SupplierId, (await ReadAsync(db => db.PurchaseDetails.SingleAsync(row => row.EntryId == request.Id))).SupplierId);
        Assert.False(await ReadAsync(db => db.Invoices.AnyAsync(row => row.EntryId == request.Id)));
    }

    [Fact]
    public async Task UC_022_AnExpenseGoesToTheItemsGeneralSubgroupUnlessAnotherIsChosen()
    {
        var setup = await FinanceAsync();
        (await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/subgroups", new NameRequest("مواد"), Json)).EnsureSuccessStatusCode();
        var subgroups = (await PurchaseItemAsync(setup.Client, setup.Project.Id)).Subgroups;

        var general = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));
        var named = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { TableId = subgroups.Single(row => row.Name == "مواد").Id });

        Assert.Equal(subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, (await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == general.Id))).SubgroupId);
        Assert.Equal(subgroups.Single(row => row.Name == "مواد").Id, (await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == named.Id))).SubgroupId);
    }

    [Fact]
    public async Task ASubgroupOfAnotherItemIsRefused()
    {
        var setup = await FinanceAsync();
        await AssertRefusedAsync(setup, Expense(setup) with { TableId = Guid.CreateVersion7() }, "unknownTable");
    }

    [Fact]
    public async Task UC_022_ADraftWaitsAndASentOneIsPendingApproval()
    {
        var setup = await FinanceAsync();

        var draft = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));
        var sent = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = await ImageAsync(setup.Client, setup.Project.Id) });

        Assert.Equal(EntryStatus.Draft, draft.Status);
        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
        Assert.Equal(EntryStatus.PendingApproval, (await EntryOfAsync(sent.Id)).Status);
    }

    [Fact]
    public async Task TheFileOfAnInvoiceIsLinkedToItsEntryAsItsInvoice()
    {
        var setup = await FinanceAsync();
        var attachmentId = await ImageAsync(setup.Client, setup.Project.Id);

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { AttachmentId = attachmentId });

        var link = await ReadAsync(db => db.EntryAttachments.SingleAsync(row => row.EntryId == view.Id));
        Assert.Equal(attachmentId, link.AttachmentId);
        Assert.Equal(EntryAttachmentRole.Invoice, link.Role);
    }

    [Fact]
    public async Task BR_33_AFileOfAnotherProjectIsRefusedOnAnEntry()
    {
        var setup = await FinanceAsync();
        var other = await FinanceAsync("غصون 7");
        await AssertRefusedAsync(setup, Expense(setup) with { AttachmentId = await ImageAsync(other.Client, other.Project.Id) }, "unknownAttachment");
    }

    [Fact]
    public async Task BR_19_ASupplierNotInTheRegisterIsRefused()
    {
        var setup = await FinanceAsync();
        var other = await FinanceAsync("غصون 7");

        await AssertRefusedAsync(setup, Expense(setup) with { SupplierId = Guid.Empty }, "unknownSupplier");
        await AssertRefusedAsync(setup, Expense(setup) with { SupplierId = other.SupplierId }, "unknownSupplier");
    }

    [Fact]
    public async Task ADisabledSupplierIsRefused()
    {
        var setup = await FinanceAsync();
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/suppliers/{setup.SupplierId}/disable", null)).EnsureSuccessStatusCode();

        await AssertRefusedAsync(setup, Expense(setup), "unknownSupplier");
    }

    [Fact]
    public async Task FR_44_FieldUsesTheirOwnPendingSupplierAndNotAnothers()
    {
        var setup = await FinanceAsync();
        var (nawaf, nawafId) = await MemberAsync(setup, Field);
        var (salem, _) = await MemberAsync(setup, Field);
        var pending = await SupplierAsync(nawaf, setup.Project.Id, "مؤسسة الواحة", MigratedApiFactory.NewVatNumber());
        var custody = await CustodyOfAsync(setup, nawafId);
        var request = Expense(setup) with { SupplierId = pending.Id, AccountId = custody };

        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(salem, setup.Project.Id, request)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(nawaf, setup.Project.Id, request)).StatusCode);
    }

    private static async Task<Guid> CustodyOfAsync(Setup setup, Guid userId, string name = "عهدة")
    {
        (await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/accounts", new AddAccountRequest(name, AccountType.CashCustody) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        return (await setup.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{setup.Project.Id}/accounts", Json))!.Single(account => account.Name == name).Id;
    }

    [Fact]
    public async Task AC_38_1_AnItemThatTakesNoPurchasesOrIsDisabledOrOfAnotherProjectIsRefused()
    {
        var setup = await FinanceAsync();
        var other = await FinanceAsync("غصون 7");
        var contractorOnly = (await setup.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{setup.Project.Id}/categories", Json))!.SelectMany(category => category.Items).First(item => item.Type == ItemType.ContractorPayments);

        await AssertRefusedAsync(setup, Expense(setup) with { ItemId = contractorOnly.Id, TableId = contractorOnly.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id }, "unknownTable");
        await AssertRefusedAsync(setup, Expense(setup) with { ItemId = other.ItemId, TableId = other.TableId }, "unknownTable");
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/disable", null)).EnsureSuccessStatusCode();
        await AssertRefusedAsync(setup, Expense(setup), "unknownTable");
    }

    [Fact]
    public async Task AC_40_1_ADisabledPaymentMethodIsRefused()
    {
        var setup = await FinanceAsync();
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/payment-methods/{setup.MethodId}/disable", null)).EnsureSuccessStatusCode();

        await AssertRefusedAsync(setup, Expense(setup), "unknownPaymentMethod");
    }

    [Fact]
    public async Task OpenQuestion4_FieldChargesAnAccountTheyAreACustodianOf()
    {
        var setup = await FinanceAsync();
        var (field, fieldId) = await MemberAsync(setup, Field);
        var custody = await CustodyOfAsync(setup, fieldId);

        var view = await RecordAsync(field, setup.Project.Id, Expense(setup) with { AccountId = custody });

        Assert.Equal(custody, (await EntryOfAsync(view.Id)).AccountId);
    }

    [Fact]
    public async Task OpenQuestion4_FieldCannotChargeAnAccountTheyAreNotACustodianOf()
    {
        var setup = await FinanceAsync();
        var (field, _) = await MemberAsync(setup, Field);
        var (other, otherId) = await MemberAsync(setup, Field);
        var hisCustody = await CustodyOfAsync(setup, otherId);

        foreach (var accountId in new[] { setup.AccountId, hisCustody })
        {
            var response = await PostAsync(field, setup.Project.Id, Expense(setup) with { AccountId = accountId });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("accountNotOffered", await CodeOf(response));
        }
        Assert.NotNull(other);
    }

    [Fact]
    public async Task ChargeAnyAccountLetsFinanceChargeAnyActiveAccountAndNotADisabledOne()
    {
        var setup = await FinanceAsync();
        var (_, fieldId) = await MemberAsync(setup, Field);
        var custody = await CustodyOfAsync(setup, fieldId);

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { AccountId = custody })).StatusCode);
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/accounts/{custody}/disable", null)).EnsureSuccessStatusCode();
        await AssertRefusedAsync(setup, Expense(setup) with { AccountId = custody }, "accountNotOffered");
    }

    [Fact]
    public async Task BR_33_AnAccountOfAnotherProjectIsRefused()
    {
        var setup = await FinanceAsync();
        var other = await FinanceAsync("غصون 7");

        await AssertRefusedAsync(setup, Expense(setup) with { AccountId = other.AccountId }, "accountNotOffered");
    }

    [Fact]
    public async Task BR_12_ADateOutsideTheProjectsPeriodOrInTheFutureIsRefused()
    {
        var setup = await FinanceAsync();
        var today = Entry.Today(factory.Time);

        await AssertRefusedAsync(setup, Expense(setup) with { Date = today.AddDays(1) }, "invalidDate");
        await AssertRefusedAsync(setup, Expense(setup) with { Date = setup.Project.StartDate.AddDays(-1) }, "invalidDate");
        await AssertRefusedAsync(setup, Expense(setup) with { Date = default }, "invalidDate");
    }

    [Fact]
    public async Task BR_12_TheProjectsFirstDayAndTodayAreAccepted()
    {
        var setup = await FinanceAsync();

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { Date = setup.Project.StartDate })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { Date = Entry.Today(factory.Time) })).StatusCode);
    }

    [Fact]
    public void BR_12_TodayIsTheDateInSaudiArabiaNotOnTheServer()
    {
        Assert.Equal(new DateOnly(2026, 11, 6), Entry.Today(new FakeTimeProvider(new DateTimeOffset(2026, 11, 5, 21, 30, 0, TimeSpan.Zero))));
        Assert.Equal(new DateOnly(2026, 11, 5), Entry.Today(new FakeTimeProvider(new DateTimeOffset(2026, 11, 5, 20, 59, 0, TimeSpan.Zero))));
    }

    [Fact]
    public async Task WhatWasTypedIsRefusedWhenItCannotBeKept()
    {
        var setup = await FinanceAsync();

        await AssertRefusedAsync(setup, Expense(setup) with { Product = new string('م', PurchaseDetails.ProductMaxLength + 1) }, "invalidProduct");
        await AssertRefusedAsync(setup, Expense(setup) with { Notes = new string('م', Entry.NotesMaxLength + 1) }, "invalidNotes");
        await AssertRefusedAsync(setup, Expense(setup) with { UnitPrice = -1m }, "invalidUnitPrice");
        await AssertRefusedAsync(setup, Expense(setup) with { Quantity = 1.234m }, "invalidQuantity");
        await AssertRefusedAsync(setup, Expense(setup, 0m), "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, -5m), "deductionNeedsNotes");
        await AssertRefusedAsync(setup, Expense(setup, 1.234m), "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, 0m) with { UnitPrice = Money.Largest, Quantity = Money.Largest }, "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, Money.Largest + 1m), "invalidAmount");
    }

    [Fact]
    public async Task AC_41_1_ARequiredCustomColumnIsAskedOnlyOfItsItemWhenSending()
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, ColumnDataType.Number, isRequired: true);
        var attachmentId = await ImageAsync(setup.Client, setup.Project.Id);
        var otherItem = (await setup.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{setup.Project.Id}/categories/offered?itemType={ItemType.Purchases}", Json))!.SelectMany(category => category.Items).First(item => item.Id != setup.ItemId);

        var refused = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = attachmentId });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        var answer = await refused.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("requiredColumnMissing", answer.GetProperty("code").GetString());
        Assert.Equal(key, answer.GetProperty("columnKey").GetString());
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { ItemId = otherItem.Id, TableId = otherItem.Subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, Submit = true, AttachmentId = attachmentId })).StatusCode);
    }

    [Fact]
    public async Task ADraftCanWaitForARequiredColumn()
    {
        var setup = await FinanceAsync();
        await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, ColumnDataType.Number, isRequired: true);

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup))).StatusCode);
    }

    [Fact]
    public async Task FR_41_CustomValuesAreStoredWithTheEntryAndAnEmptyOneIsNoValue()
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, ColumnDataType.Number, isRequired: true);
        var attachmentId = await ImageAsync(setup.Client, setup.Project.Id);

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = attachmentId, CustomValues = new Dictionary<string, string> { [key] = " 125.5 " } });
        await AssertRefusedAsync(setup, Expense(setup) with { Submit = true, AttachmentId = attachmentId, CustomValues = new Dictionary<string, string> { [key] = "  " } }, "requiredColumnMissing");

        var stored = JsonSerializer.Deserialize<Dictionary<string, string>>((await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == view.Id))).CustomValues)!;
        Assert.Equal("125.5", stored[key]);
    }

    [Theory]
    [InlineData(ColumnDataType.Number, "كثير")]
    [InlineData(ColumnDataType.Amount, "1.234")]
    [InlineData(ColumnDataType.Date, "05.11.2026")]
    [InlineData(ColumnDataType.List, "أخضر")]
    public async Task FR_41_AValueThatDoesNotFitItsColumnIsRefusedNamingIt(ColumnDataType dataType, string value)
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, dataType, false, "أحمر");

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new Dictionary<string, string> { [key] = value } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var answer = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("invalidColumnValue", answer.GetProperty("code").GetString());
        Assert.Equal(key, answer.GetProperty("columnKey").GetString());
    }

    [Theory]
    [InlineData(ColumnDataType.Number, "125.5")]
    [InlineData(ColumnDataType.Amount, "1234.56")]
    [InlineData(ColumnDataType.Date, "2026-11-05")]
    [InlineData(ColumnDataType.List, "أحمر")]
    [InlineData(ColumnDataType.Text, "نص حر")]
    public async Task FR_41_AValueThatFitsItsColumnIsKept(ColumnDataType dataType, string value)
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, dataType, false, "أحمر");
        var columns = (await setup.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{setup.Project.Id}/items/{setup.ItemId}/columns", Json))!;
        var typed = dataType == ColumnDataType.List ? columns.Single(column => column.Key == key).Choices.Single(choice => choice.Name == value).Id.ToString() : value;

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new Dictionary<string, string> { [key] = typed } })).StatusCode);
    }

    [Fact]
    public async Task AValueForAColumnTheItemDoesNotAskIsRefused()
    {
        var setup = await FinanceAsync();

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new Dictionary<string, string> { ["custom.nothing"] = "1" } });

        Assert.Equal("unknownColumn", await CodeOf(response));
    }

    [Fact]
    public async Task BR_21_ADeletedColumnIsNoLongerAsked()
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, ColumnDataType.Number, isRequired: true);
        var column = (await setup.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{setup.Project.Id}/items/{setup.ItemId}/columns", Json))!.Single(row => row.Key == key);
        (await setup.Client.DeleteAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/columns/{column.Id}")).EnsureSuccessStatusCode();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = await ImageAsync(setup.Client, setup.Project.Id) });

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
        Assert.Equal("unknownColumn", await CodeOf(await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new Dictionary<string, string> { [key] = "1" } })));
    }

    [Theory]
    [InlineData("product")]
    [InlineData("unitPrice")]
    [InlineData("quantity")]
    [InlineData("notes")]
    public async Task FR_41_ABuiltInColumnMadeRequiredIsAskedWhenSending(string key)
    {
        var setup = await FinanceAsync();
        await factory.WithDatabaseAsync(async db =>
        {
            (await db.ItemColumns.SingleAsync(row => row.ItemId == setup.ItemId && row.Key == key)).IsRequired = true;
            await db.SaveChangesAsync();
        });

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = await ImageAsync(setup.Client, setup.Project.Id) });

        var answer = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("requiredColumnMissing", answer.GetProperty("code").GetString());
        Assert.Equal(key, answer.GetProperty("columnKey").GetString());
    }

    [Fact]
    public async Task AC_16_3_AnUploadSentAgainLandsOnceAndAnswersTheEntry()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup);
        await RecordAsync(setup.Client, setup.Project.Id, request);

        var again = await PostAsync(setup.Client, setup.Project.Id, request);

        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(request.Id, (await again.Content.ReadFromJsonAsync<EntryStatusView>(Json))!.Id);
        Assert.Equal(1, await ReadAsync(db => db.Entries.CountAsync(row => row.Id == request.Id)));
        await AssertRefusedAsync(setup, Expense(setup) with { Id = Guid.Empty }, "invalidEntryId");
    }

    [Fact]
    public async Task AnIdSomeoneElseUsedIsRefusedAndTellsThemNothingOfIt()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup);
        await RecordAsync(setup.Client, setup.Project.Id, request);
        var other = await factory.AddUserAsync(Password);
        await factory.GiveAsync(other.Id, setup.Project.Id, Holds.Of(Finance));
        var otherClient = await factory.ClientSignedInAsAsync(other, Password);

        var refused = await PostAsync(otherClient, setup.Project.Id, request);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("entryExists", await CodeOf(refused));
    }

    [Theory]
    [InlineData(Reader)]
    [InlineData(Manager)]
    public async Task WhoHoldsNoRecordEntriesCannotRecordAnExpense(string role)
    {
        var setup = await FinanceAsync();
        var (member, _) = await MemberAsync(setup, role);

        var refused = await PostAsync(member, setup.Project.Id, Expense(setup));

        Assert.Equal((HttpStatusCode.BadRequest, "unknownTable"), (refused.StatusCode, await CodeOf(refused)));
    }

    [Fact]
    public async Task BR_33_SomeoneOutsideTheProjectCannotRecordInIt()
    {
        var setup = await FinanceAsync();
        var outsider = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password), Password);

        Assert.Equal(HttpStatusCode.Forbidden, (await PostAsync(outsider, setup.Project.Id, Expense(setup))).StatusCode);
    }

    [Fact]
    public async Task AC_04_1_AFinishedProjectRecordsNoExpense()
    {
        var setup = await FinanceAsync();
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/close", null)).EnsureSuccessStatusCode();

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("projectFinished", await CodeOf(response));
    }

    [Fact]
    public async Task FR_21_AnExpenseIsInTheHistoryWithWhoAndItsTotal()
    {
        var setup = await FinanceAsync();
        var attachmentId = await ImageAsync(setup.Client, setup.Project.Id);

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = attachmentId });

        var trail = await factory.HistoryOfAsync(view.Id);
        Assert.Equal(["Entries", "ExpenseDetails", "PurchaseDetails", "EntryAttachments"], trail.Select(record => record.TableName));
        Assert.Equal(HistoryAction.Submit, trail[0].Action);
        Assert.Equal(6000m, JsonDocument.Parse(trail[0].After).RootElement.GetProperty("Amount").GetDecimal());
        Assert.Contains(attachmentId.ToString(), trail.Single(record => record.TableName == "EntryAttachments").After);
    }

    [Fact]
    public async Task ADraftIsCreatedInTheHistoryRatherThanSubmitted()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));

        Assert.Equal(HistoryAction.Create, (await factory.HistoryOfAsync(view.Id))[0].Action);
    }

    [Fact]
    public async Task AC_43_1_TheSearchPutsWhatTheCallerUsedMostFirstAndThenByName()
    {
        var setup = await FinanceAsync();
        var other = await SupplierAsync(setup.Client, setup.Project.Id, "مؤسسة الواحة", MigratedApiFactory.NewVatNumber());
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = other.Id });
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = other.Id });
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));

        var found = (await setup.Client.GetFromJsonAsync<List<SupplierView>>($"/projects/{setup.Project.Id}/suppliers/search", Json))!;

        Assert.Equal(["مؤسسة الواحة", "شركة النخيل", Supplier.GeneralCashSupplierName], found.Select(supplier => supplier.Name));
        Assert.Equal([2, 1, 0], found.Select(supplier => supplier.TimesUsed));
    }

    [Fact]
    public async Task AC_43_1_MostUsedIsTheCallersOwnNotSomeoneElses()
    {
        var setup = await FinanceAsync();
        var other = await SupplierAsync(setup.Client, setup.Project.Id, "مؤسسة الواحة", MigratedApiFactory.NewVatNumber());
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = other.Id });
        var (member, _) = await MemberAsync(setup, Field);

        var found = (await member.GetFromJsonAsync<List<SupplierView>>($"/projects/{setup.Project.Id}/suppliers/search", Json))!;

        Assert.All(found, supplier => Assert.Equal(0, supplier.TimesUsed));
        Assert.Equal(["شركة النخيل", "مؤسسة الواحة", Supplier.GeneralCashSupplierName], found.Select(supplier => supplier.Name));
    }
}
