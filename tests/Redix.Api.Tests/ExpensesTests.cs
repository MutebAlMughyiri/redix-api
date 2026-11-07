using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.Audit;
using Redix.Api.Entries;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class ExpensesTests(MigratedApiFactory factory)
{
    private const string Password = "Expenses-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private sealed record Setup(HttpClient Client, ProjectView Project, Guid ItemId, Guid SupplierId, Guid CashSupplierId, Guid MethodId, Guid AccountId);

    private async Task<Setup> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name, "الفلة الغربية");
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        var supplier = await SupplierAsync(client, project.Id, "شركة النخيل", MigratedApiFactory.NewVatNumber());
        var suppliers = (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{project.Id}/suppliers", Json))!;
        var methods = (await client.GetFromJsonAsync<List<PaymentMethodView>>($"/projects/{project.Id}/payment-methods", Json))!;
        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حساب المؤسسة", AccountType.Bank, 0), Json)).EnsureSuccessStatusCode();
        var account = (await client.GetFromJsonAsync<List<AccountView>>($"/projects/{project.Id}/accounts", Json))!.Single();
        return new Setup(client, project, (await PurchaseItemAsync(client, project.Id)).Id, supplier.Id, suppliers.Single(row => row.Name == Supplier.GeneralCashSupplierName).Id, methods[0].Id, account.Id);
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

    private ExpenseRequest Expense(Setup setup, decimal amount = 6000m) => new(Guid.CreateVersion7())
    {
        ItemId = setup.ItemId,
        SupplierId = setup.SupplierId,
        InvoiceNumber = "2617",
        InvoiceDate = Entry.Today(factory.Time),
        Amount = amount,
        PaymentMethodId = setup.MethodId,
        AccountId = setup.AccountId,
    };

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid projectId, ExpenseRequest request) =>
        client.PostAsJsonAsync($"/projects/{projectId}/expenses", request, Json);

    private static async Task<ExpenseView> RecordAsync(HttpClient client, Guid projectId, ExpenseRequest request) =>
        (await (await PostAsync(client, projectId, request)).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ExpenseView>(Json))!;

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private static async Task AssertRefusedAsync(Setup setup, ExpenseRequest request, string code)
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

    private Task<Invoice> InvoiceOfAsync(Guid entryId) => ReadAsync(db => db.Invoices.SingleAsync(row => row.EntryId == entryId));

    private Task<Entry> EntryOfAsync(Guid entryId) => ReadAsync(db => db.Entries.SingleAsync(row => row.Id == entryId));

    private static async Task<Guid> ImageAsync(HttpClient client, Guid projectId)
    {
        var file = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xE0, (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256)]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var response = await client.PostAsync($"/projects/{projectId}/attachments", new MultipartFormDataContent { { file, "file", "invoice.jpg" } });
        return (await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<AttachmentView>(Json))!.Id;
    }

    private static async Task<string> CustomColumnAsync(HttpClient client, Guid projectId, Guid itemId, ColumnDataType dataType, bool isRequired, params List<string> options)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/items/{itemId}/columns", new AddColumnRequest("المساحة م²", dataType, isRequired, false, options), Json)).EnsureSuccessStatusCode();
        return (await client.GetFromJsonAsync<List<ColumnView>>($"/projects/{projectId}/items/{itemId}/columns", Json))!.Last().Key;
    }

    [Fact]
    public void AC_08_1_AnAmountIncludingVatKeepsFourDecimalsAndAddsUpToWhatWasTyped()
    {
        var figures = InvoiceFigures.Of(6000m, includesVat: true, discount: 0m, vatRate: 0.15m);

        Assert.Equal(5217.3913m, figures.AmountBeforeDiscount);
        Assert.Equal(5217.3913m, figures.AmountAfterDiscount);
        Assert.Equal(782.6087m, figures.Vat);
        Assert.Equal(6000.0000m, figures.Total);
    }

    [Fact]
    public void AC_08_2_VatOnAnAmountBeforeVatIsNotRounded()
    {
        var figures = InvoiceFigures.Of(2569.90m, includesVat: false, discount: 0m, vatRate: 0.15m);

        Assert.Equal(385.485m, figures.Vat);
        Assert.Equal(2955.385m, figures.Total);
    }

    [Fact]
    public void AC_08_3_TheDiscountComesOffBeforeVat()
    {
        var figures = InvoiceFigures.Of(1000m, includesVat: false, discount: 100m, vatRate: 0.15m);

        Assert.Equal(900m, figures.AmountAfterDiscount);
        Assert.Equal(135m, figures.Vat);
        Assert.Equal(1035m, figures.Total);
    }

    [Fact]
    public void ADiscountOnAnAmountIncludingVatComesOffTheAmountBeforeVat()
    {
        var figures = InvoiceFigures.Of(1150m, includesVat: true, discount: 100m, vatRate: 0.15m);

        Assert.Equal(1000m, figures.AmountBeforeDiscount);
        Assert.Equal(900m, figures.AmountAfterDiscount);
        Assert.Equal(135m, figures.Vat);
        Assert.Equal(1035m, figures.Total);
    }

    [Theory]
    [InlineData(1000, false, 0.05, 1000, 50, 1050)]
    [InlineData(1000, false, 0.10, 1000, 100, 1100)]
    [InlineData(1000, false, 0.125, 1000, 125, 1125)]
    [InlineData(1050, true, 0.05, 1000, 50, 1050)]
    [InlineData(1125, true, 0.125, 1000, 125, 1125)]
    [InlineData(1234.57, false, 0.05, 1234.57, 61.7285, 1296.2985)]
    [InlineData(1234.57, false, 0.125, 1234.57, 154.3213, 1388.8913)]
    [InlineData(500, true, 0, 500, 0, 500)]
    [InlineData(500, false, 0, 500, 0, 500)]
    public void TheVatFollowsTheRateOfTheProject(double amount, bool includesVat, double rate, double before, double vat, double total)
    {
        var figures = InvoiceFigures.Of((decimal)amount, includesVat, 0m, (decimal)rate);

        Assert.Equal((decimal)before, figures.AmountBeforeDiscount);
        Assert.Equal((decimal)vat, figures.Vat);
        Assert.Equal((decimal)total, figures.Total);
    }

    [Fact]
    public void AWholeInvoiceWithNoCreditNoteAddsUpFromItsTypedAmountsToItsTotals()
    {
        InvoiceFigures[] rows =
        [
            InvoiceFigures.Of(6000m, true, 0m, 0.15m),
            InvoiceFigures.Of(2569.90m, false, 0m, 0.15m),
            InvoiceFigures.Of(1000m, false, 100m, 0.15m),
        ];

        Assert.Equal(1303.0937m, rows.Sum(row => row.Vat));
        Assert.Equal(9990.385m, rows.Sum(row => row.Total));
        Assert.Equal(8687.2913m, rows.Sum(row => row.AmountAfterDiscount));
    }

    [Theory]
    [InlineData(0.05)]
    [InlineData(0.10)]
    [InlineData(0.125)]
    [InlineData(0.15)]
    public void AtEveryRateTheStoredFiguresAddUpAndAnAmountIncludingVatStaysExactly(double rate)
    {
        for (var halalas = 1; halalas < 2_000_000; halalas += 977)
        {
            var amount = halalas / 100m;

            var figures = InvoiceFigures.Of(amount, true, 0m, (decimal)rate);

            Assert.Equal(amount, figures.Total);
            Assert.Equal(figures.Total, figures.AmountAfterDiscount + figures.Vat);
        }
    }

    [Fact]
    public async Task AC_08_1_TheFiguresReachTheDatabaseWithTheirFourDecimals()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));

        var invoice = await InvoiceOfAsync(view.Id);
        Assert.Equal(5217.3913m, invoice.AmountBeforeDiscount);
        Assert.Equal(5217.3913m, invoice.AmountAfterDiscount);
        Assert.Equal(782.6087m, invoice.Vat);
        Assert.Equal(6000m, invoice.Total);
        Assert.Equal(6000m, (await EntryOfAsync(view.Id)).Amount);
    }

    [Fact]
    public async Task AFigureIsNeverRoundedToTwoDecimalsBeforeItIsStored()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 2569.90m) with { AmountIncludesVat = false });

        var invoice = await InvoiceOfAsync(view.Id);
        Assert.Equal(385.4850m, invoice.Vat);
        Assert.Equal(2955.3850m, invoice.Total);
        Assert.Equal(2955.3850m, (await EntryOfAsync(view.Id)).Amount);
        Assert.Equal(385.485m, view.Vat);
        Assert.Equal(2955.385m, view.Total);
    }

    [Fact]
    public async Task BR_13_VatStaysUnroundedOnEachRowSoATotalRoundsOnlyOnce()
    {
        var setup = await FinanceAsync();
        List<Guid> ids = [];
        for (var row = 0; row < 3; row++)
        {
            ids.Add((await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 10.03m) with { AmountIncludesVat = false, InvoiceNumber = $"F{row}" })).Id);
        }

        var vat = await ReadAsync(db => db.Invoices.Where(invoice => ids.Contains(invoice.EntryId)).SumAsync(invoice => invoice.Vat));

        Assert.Equal(4.5135m, vat);
        Assert.Equal(4.51m, decimal.Round(vat, 2));
        Assert.NotEqual(decimal.Round(1.5045m, 2) * 3, decimal.Round(vat, 2));
    }

    [Fact]
    public async Task BR_14_TheRateIsTheProjectsAndAnEarlierEntryKeepsTheVatItWasSavedWith()
    {
        var setup = await FinanceAsync();
        var earlier = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 1000m) with { AmountIncludesVat = false });

        (await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/vat-rate", new SetVatRateRequest(5m), Json)).EnsureSuccessStatusCode();
        var later = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 1000m) with { AmountIncludesVat = false, InvoiceNumber = "2618" });

        Assert.Equal(150m, (await InvoiceOfAsync(earlier.Id)).Vat);
        Assert.Equal(50m, (await InvoiceOfAsync(later.Id)).Vat);
    }

    [Fact]
    public async Task BR_14_ASupplierNotVatRegisteredHasNoVatAndTheAmountStaysWhole()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = setup.CashSupplierId });

        Assert.Equal(6000m, view.AmountBeforeDiscount);
        Assert.Equal(0m, view.Vat);
        Assert.Equal(6000m, view.Total);
    }

    [Fact]
    public async Task BR_14_AnInvoiceNotTaxableHasNoVat()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { IsNotTaxable = true });

        Assert.Equal(0m, view.Vat);
        Assert.Equal(6000m, view.Total);
    }

    [Fact]
    public async Task BR_01_LeftWithoutAnAmountThePriceTimesTheQuantityIncludesVat()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 0m) with { UnitPrice = 230m, Quantity = 5m, AmountIncludesVat = false });

        Assert.Equal(1000m, view.AmountBeforeDiscount);
        Assert.Equal(150m, view.Vat);
        Assert.Equal(1150m, view.Total);
    }

    [Fact]
    public async Task AnAmountTypedWinsOverThePriceAndQuantityWhichStayAsTyped()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup, 1150m) with { UnitPrice = 999m, Quantity = 2m, Product = " مواد لياسة " });

        Assert.Equal(1150m, view.Total);
        var purchase = await ReadAsync(db => db.PurchaseDetails.SingleAsync(row => row.EntryId == view.Id));
        Assert.Equal(999m, purchase.UnitPrice);
        Assert.Equal(2m, purchase.Quantity);
        Assert.Equal("مواد لياسة", purchase.Product);
    }

    [Fact]
    public async Task BR_24_APaymentLeftOutIsTheInvoicesDateAndTotal()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup);

        var entry = await EntryOfAsync((await RecordAsync(setup.Client, setup.Project.Id, request)).Id);

        Assert.Equal(request.InvoiceDate, entry.EntryDate);
        Assert.Equal(6000m, entry.Amount);
    }

    [Fact]
    public async Task BR_24_APaymentThatDiffersKeepsItsOwnDateAndAmountBesideTheInvoicesAndItsFees()
    {
        var setup = await FinanceAsync();
        var request = Expense(setup) with { PaymentDate = setup.Project.StartDate, PaymentAmount = 6001.50m, BankFees = 1.50m };

        var view = await RecordAsync(setup.Client, setup.Project.Id, request);

        var entry = await EntryOfAsync(view.Id);
        var invoice = await InvoiceOfAsync(view.Id);
        Assert.Equal(setup.Project.StartDate, entry.EntryDate);
        Assert.Equal(6001.50m, entry.Amount);
        Assert.Equal(request.InvoiceDate, invoice.InvoiceDate);
        Assert.Equal(6000m, invoice.Total);
        Assert.Equal(1.50m, (await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == view.Id))).BankFees);
    }

    [Fact]
    public async Task BR_17_TheVatPeriodIsTheQuarterOfTheInvoicesDate()
    {
        Assert.Equal(new DateOnly(2026, 10, 1), Invoice.QuarterStartOf(new DateOnly(2026, 11, 5)));
        Assert.Equal(new DateOnly(2026, 1, 1), Invoice.QuarterStartOf(new DateOnly(2026, 3, 31)));
        Assert.Equal(new DateOnly(2026, 4, 1), Invoice.QuarterStartOf(new DateOnly(2026, 4, 1)));

        var setup = await FinanceAsync();
        var request = Expense(setup);
        var view = await RecordAsync(setup.Client, setup.Project.Id, request);
        Assert.Equal(Invoice.QuarterStartOf(request.InvoiceDate), (await InvoiceOfAsync(view.Id)).VatPeriodStart);
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
        Assert.Equal("2617", (await InvoiceOfAsync(request.Id)).Number);
    }

    [Fact]
    public async Task UC_022_AnExpenseGoesToTheItemsGeneralSubgroupUnlessAnotherIsChosen()
    {
        var setup = await FinanceAsync();
        (await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/subgroups", new NameRequest("مواد"), Json)).EnsureSuccessStatusCode();
        var subgroups = (await PurchaseItemAsync(setup.Client, setup.Project.Id)).Subgroups;

        var general = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));
        var named = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceNumber = "2618", SubgroupId = subgroups.Single(row => row.Name == "مواد").Id });

        Assert.Equal(subgroups.Single(row => row.Name == Subgroup.GeneralName).Id, (await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == general.Id))).SubgroupId);
        Assert.Equal(subgroups.Single(row => row.Name == "مواد").Id, (await ReadAsync(db => db.ExpenseDetails.SingleAsync(row => row.EntryId == named.Id))).SubgroupId);
    }

    [Fact]
    public async Task ASubgroupOfAnotherItemIsRefused()
    {
        var setup = await FinanceAsync();
        await AssertRefusedAsync(setup, Expense(setup) with { SubgroupId = Guid.CreateVersion7() }, "unknownSubgroup");
    }

    [Fact]
    public async Task UC_022_ADraftWaitsAndASentOneIsPendingApproval()
    {
        var setup = await FinanceAsync();

        var draft = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));
        var sent = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceNumber = "2618", Submit = true, AttachmentId = await ImageAsync(setup.Client, setup.Project.Id) });

        Assert.Equal(EntryStatus.Draft, draft.Status);
        Assert.Equal(EntryStatus.PendingApproval, sent.Status);
        Assert.Equal(EntryStatus.PendingApproval, (await EntryOfAsync(sent.Id)).Status);
    }

    [Fact]
    public async Task FR_09_SendingNeedsTheInvoicesFileAndADraftCanWaitForIt()
    {
        var setup = await FinanceAsync();

        await AssertRefusedAsync(setup, Expense(setup) with { Submit = true }, "invoiceRequired");

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup))).StatusCode);
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
        (await setup.Client.PostAsJsonAsync($"/projects/{setup.Project.Id}/accounts", new AddAccountRequest(name, AccountType.CashCustody, 0) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        return (await setup.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{setup.Project.Id}/accounts", Json))!.Single(account => account.Name == name).Id;
    }

    [Fact]
    public async Task AC_38_1_AnItemThatTakesNoPurchasesOrIsDisabledOrOfAnotherProjectIsRefused()
    {
        var setup = await FinanceAsync();
        var other = await FinanceAsync("غصون 7");
        var contractorOnly = (await setup.Client.GetFromJsonAsync<List<CategoryView>>($"/projects/{setup.Project.Id}/categories", Json))!.SelectMany(category => category.Items).First(item => item.Type == ItemType.ContractorPayments);

        await AssertRefusedAsync(setup, Expense(setup) with { ItemId = contractorOnly.Id }, "itemNotOffered");
        await AssertRefusedAsync(setup, Expense(setup) with { ItemId = other.ItemId }, "itemNotOffered");
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/disable", null)).EnsureSuccessStatusCode();
        await AssertRefusedAsync(setup, Expense(setup), "itemNotOffered");
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
        await AssertRefusedAsync(setup, Expense(setup) with { AccountId = custody, InvoiceNumber = "2618" }, "accountNotOffered");
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

        await AssertRefusedAsync(setup, Expense(setup) with { InvoiceDate = today.AddDays(1) }, "invalidInvoiceDate");
        await AssertRefusedAsync(setup, Expense(setup) with { InvoiceDate = setup.Project.StartDate.AddDays(-1) }, "invalidInvoiceDate");
        await AssertRefusedAsync(setup, Expense(setup) with { InvoiceDate = default }, "invalidInvoiceDate");
        await AssertRefusedAsync(setup, Expense(setup) with { PaymentDate = today.AddDays(1) }, "invalidPaymentDate");
        await AssertRefusedAsync(setup, Expense(setup) with { PaymentDate = setup.Project.StartDate.AddDays(-1) }, "invalidPaymentDate");
    }

    [Fact]
    public async Task BR_12_TheProjectsFirstDayAndTodayAreAccepted()
    {
        var setup = await FinanceAsync();

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceDate = setup.Project.StartDate })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceNumber = "2618", InvoiceDate = Entry.Today(factory.Time) })).StatusCode);
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

        await AssertRefusedAsync(setup, Expense(setup) with { InvoiceNumber = new string('1', Invoice.NumberMaxLength + 1) }, "invalidInvoiceNumber");
        await AssertRefusedAsync(setup, Expense(setup) with { Product = new string('م', PurchaseDetails.ProductMaxLength + 1) }, "invalidProduct");
        await AssertRefusedAsync(setup, Expense(setup) with { Notes = new string('م', Entry.NotesMaxLength + 1) }, "invalidNotes");
        await AssertRefusedAsync(setup, Expense(setup) with { UnitPrice = -1m }, "invalidUnitPrice");
        await AssertRefusedAsync(setup, Expense(setup) with { Quantity = 1.234m }, "invalidQuantity");
        await AssertRefusedAsync(setup, Expense(setup, 0m), "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, -5m), "deductionNeedsReason");
        await AssertRefusedAsync(setup, Expense(setup, 1.234m), "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, 0m) with { UnitPrice = Money.Largest, Quantity = Money.Largest }, "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup, Money.Largest) with { AmountIncludesVat = false }, "invalidAmount");
        await AssertRefusedAsync(setup, Expense(setup) with { Discount = -1m }, "invalidDiscount");
        await AssertRefusedAsync(setup, Expense(setup, 100m) with { Discount = 100.01m, AmountIncludesVat = false }, "invalidDiscount");
        await AssertRefusedAsync(setup, Expense(setup) with { PaymentAmount = -1m }, "invalidPaymentAmount");
        await AssertRefusedAsync(setup, Expense(setup) with { BankFees = 0.123m }, "invalidBankFees");
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
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { ItemId = otherItem.Id, Submit = true, AttachmentId = attachmentId })).StatusCode);
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

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = attachmentId, CustomValues = new() { [key] = " 125.5 " } });
        await AssertRefusedAsync(setup, Expense(setup) with { InvoiceNumber = "2618", Submit = true, AttachmentId = attachmentId, CustomValues = new() { [key] = "  " } }, "requiredColumnMissing");

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

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new() { [key] = value } });

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

        Assert.Equal(HttpStatusCode.Created, (await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new() { [key] = value } })).StatusCode);
    }

    [Fact]
    public async Task AValueForAColumnTheItemDoesNotAskIsRefused()
    {
        var setup = await FinanceAsync();

        var response = await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { CustomValues = new() { ["custom.nothing"] = "1" } });

        Assert.Equal("unknownColumn", await CodeOf(response));
    }

    [Fact]
    public async Task BR_21_ADeletedColumnIsNoLongerAsked()
    {
        var setup = await FinanceAsync();
        var key = await CustomColumnAsync(setup.Client, setup.Project.Id, setup.ItemId, ColumnDataType.Number, isRequired: true);
        var column = (await setup.Client.GetFromJsonAsync<List<ColumnView>>($"/projects/{setup.Project.Id}/items/{setup.ItemId}/columns", Json))!.Single(row => row.Key == key);
        (await setup.Client.PostAsync($"/projects/{setup.Project.Id}/items/{setup.ItemId}/columns/{column.Id}/delete", null)).EnsureSuccessStatusCode();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = await ImageAsync(setup.Client, setup.Project.Id) });

        Assert.Equal(EntryStatus.PendingApproval, view.Status);
        Assert.Equal("unknownColumn", await CodeOf(await PostAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceNumber = "2618", CustomValues = new() { [key] = "1" } })));
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

        Assert.Equal(HttpStatusCode.Forbidden, (await PostAsync(member, setup.Project.Id, Expense(setup))).StatusCode);
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
    public async Task FR_21_AnExpenseIsInTheAuditTrailWithWhoAndItsFigures()
    {
        var setup = await FinanceAsync();
        var attachmentId = await ImageAsync(setup.Client, setup.Project.Id);

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { Submit = true, AttachmentId = attachmentId });

        var trail = await factory.AuditTrailOfAsync(view.Id);
        Assert.Equal(["Entries", "ExpenseDetails", "PurchaseDetails", "Invoices", "EntryAttachments"], trail.Select(record => record.TableName));
        Assert.Equal(AuditAction.Submit, trail[0].Action);
        Assert.Equal(782.6087m, JsonDocument.Parse(trail.Single(record => record.TableName == "Invoices").After).RootElement.GetProperty("Vat").GetDecimal());
        Assert.Contains(attachmentId.ToString(), trail.Single(record => record.TableName == "EntryAttachments").After);
    }

    [Fact]
    public async Task ADraftIsCreatedInTheAuditTrailRatherThanSubmitted()
    {
        var setup = await FinanceAsync();

        var view = await RecordAsync(setup.Client, setup.Project.Id, Expense(setup));

        Assert.Equal(AuditAction.Create, (await factory.AuditTrailOfAsync(view.Id))[0].Action);
    }

    [Fact]
    public async Task AC_43_1_TheSearchPutsWhatTheCallerUsedMostFirstAndThenByName()
    {
        var setup = await FinanceAsync();
        var other = await SupplierAsync(setup.Client, setup.Project.Id, "مؤسسة الواحة", MigratedApiFactory.NewVatNumber());
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = other.Id });
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { SupplierId = other.Id, InvoiceNumber = "2618" });
        await RecordAsync(setup.Client, setup.Project.Id, Expense(setup) with { InvoiceNumber = "2619" });

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
