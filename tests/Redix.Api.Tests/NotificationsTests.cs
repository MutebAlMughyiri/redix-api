using System.Net;
using Redix.Api.Projects;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Notifications;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-036: who is told what about an entry.</summary>
[Collection(DatabaseCollection.Name)]
public class NotificationsTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private const string ManagerPassword = "Registration-1";

    private async Task<(HttpClient Client, Guid ProjectId)> RegisteredAsync()
    {
        var username = $"user-{Guid.CreateVersion7():N}";
        var client = factory.CreateClient();
        var registered = await client.PostAsJsonAsync("/projects/register", new RegisterProjectRequest("غصون 8", new DateOnly(2026, 11, 5), [], "ريم", username, "0500000000", ManagerPassword));
        var project = await registered.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(MigratedApiFactory.Json);
        var signIn = await client.PostAsJsonAsync("/auth/sign-in", new SignInRequest(username, ManagerPassword));
        var session = await signIn.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Session>(MigratedApiFactory.Json);
        client.DefaultRequestHeaders.Authorization = new("Bearer", session!.AccessToken);
        return (client, project!.Id);
    }

    private static Task<List<NotificationView>?> MineAsync(HttpClient client) => client.GetFromJsonAsync<List<NotificationView>>("/notifications", Json);

    private static async Task<(HttpClient Client, ExpenseRequest Request)> FieldSendsAsync(Ledger ledger)
    {
        var (field, userId) = await ledger.MemberAsync("Field");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("عهدة نواف", AccountType.CashCustody, 0) { CustodianId = userId }, Json)).EnsureSuccessStatusCode();
        var account = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", Json))!.Single(row => row.Name == "عهدة نواف");
        var request = ledger.Purchase(200m) with { AccountId = account.Id, Submit = true };
        (await field.PostAsJsonAsync($"/projects/{ledger.ProjectId}/expenses", request, Json)).EnsureSuccessStatusCode();
        return (field, request);
    }

    [Fact]
    public async Task AC_20_1_SendingAnEntryTellsWhoeverDecidesAndNotWhoSentIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (reader, _) = await ledger.MemberAsync("Reader");
        var (field, request) = await FieldSendsAsync(ledger);

        var told = Assert.Single((await MineAsync(ledger.Client))!);

        Assert.Equal((NotificationKind.EntrySubmitted, request.Id, ledger.ProjectId, false), (told.Kind, told.EntryId, told.ProjectId, told.IsRead));
        Assert.Equal("غصون 6", told.ProjectName);
        Assert.Empty((await MineAsync(field))!);
        Assert.Empty((await MineAsync(reader))!);
    }

    [Fact]
    public async Task SavingADraftTellsNobody()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        await ledger.MemberAsync("Finance");
        await ledger.RecordAsync(ledger.Purchase(200m));

        Assert.Empty(await ledger.ReadAsync(db => db.Notifications.Where(row => row.ProjectId == ledger.ProjectId).ToListAsync()));
    }

    [Fact]
    public async Task SendingADraftLaterTellsWhoeverDecides()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (other, _) = await ledger.MemberAsync("Finance");
        var draft = ledger.Purchase(200m);
        await ledger.RecordAsync(draft);

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{draft.Id}/submit", null)).EnsureSuccessStatusCode();

        Assert.Equal(NotificationKind.EntrySubmitted, Assert.Single((await MineAsync(other))!).Kind);
    }

    [Fact]
    public async Task AC_20_1_ApprovingTellsItsAuthor()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request) = await FieldSendsAsync(ledger);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([request.Id]), Json)).EnsureSuccessStatusCode();

        var told = Assert.Single((await MineAsync(field))!);
        Assert.Equal((NotificationKind.EntryApproved, request.Id), (told.Kind, told.EntryId));
    }

    [Fact]
    public async Task AC_20_1_RejectingTellsItsAuthor()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, request) = await FieldSendsAsync(ledger);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/reject", new RejectEntryRequest("الصورة غير واضحة"), Json)).EnsureSuccessStatusCode();

        Assert.Equal(NotificationKind.EntryRejected, Assert.Single((await MineAsync(field))!).Kind);
    }

    [Fact]
    public async Task NobodyIsToldOfTheirOwnDecision()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);

        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/approve", new ApproveEntriesRequest([request.Id]), Json)).EnsureSuccessStatusCode();

        Assert.Empty((await MineAsync(ledger.Client))!);
    }

    [Fact]
    public async Task MarkingOneReadChangesOnlyYourOwn()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (field, _) = await FieldSendsAsync(ledger);
        var told = Assert.Single((await MineAsync(ledger.Client))!);

        Assert.Equal(HttpStatusCode.NotFound, (await field.PostAsync($"/notifications/{told.Id}/read", null)).StatusCode);
        Assert.False(Assert.Single((await MineAsync(ledger.Client))!).IsRead);

        (await ledger.Client.PostAsync($"/notifications/{told.Id}/read", null)).EnsureSuccessStatusCode();

        Assert.True(Assert.Single((await MineAsync(ledger.Client))!).IsRead);
    }

    [Fact]
    public async Task DeletingADraftTakesWhatWasToldAboutIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (other, _) = await ledger.MemberAsync("Finance");
        var request = ledger.Purchase(200m) with { Submit = true };
        await ledger.RecordAsync(request);
        Assert.NotEmpty((await MineAsync(other))!);
        (await other.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/reject", new RejectEntryRequest("الصورة غير واضحة"), Json)).EnsureSuccessStatusCode();

        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/entries/{request.Id}/delete", null)).EnsureSuccessStatusCode();

        Assert.Empty((await MineAsync(other))!);
        Assert.Empty((await MineAsync(ledger.Client))!);
    }

    [Fact]
    public async Task TheAdminsDecisionTellsTheManagerOfTheProject()
    {
        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(ManagerPassword, isAdmin: true), ManagerPassword);
        var approved = await RegisteredAsync();
        var rejected = await RegisteredAsync();

        (await admin.PostAsync($"/projects/{approved.ProjectId}/approve", null)).EnsureSuccessStatusCode();
        (await admin.PostAsJsonAsync($"/projects/{rejected.ProjectId}/reject", new RejectProjectRequest("الاسم غير واضح"), Json)).EnsureSuccessStatusCode();

        var toldApproved = Assert.Single((await MineAsync(approved.Client))!);
        Assert.Equal((NotificationKind.ProjectApproved, approved.ProjectId, Guid.Empty), (toldApproved.Kind, toldApproved.ProjectId, toldApproved.EntryId));
        var toldRejected = Assert.Single((await MineAsync(rejected.Client))!);
        Assert.Equal((NotificationKind.ProjectRejected, rejected.ProjectId), (toldRejected.Kind, toldRejected.ProjectId));
    }

    [Fact]
    public async Task NobodyReadsThemWithoutSigningIn()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/notifications")).StatusCode);
    }

    [Fact]
    public async Task SomeoneWhoLeftAProjectIsNoLongerToldOfIt()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (other, otherId) = await ledger.MemberAsync("Finance");
        await FieldSendsAsync(ledger);
        Assert.NotEmpty((await MineAsync(other))!);

        await factory.WithDatabaseAsync(db => db.ProjectPermissions.Where(row => row.UserId == otherId).ExecuteDeleteAsync());

        Assert.Empty((await MineAsync(other))!);
    }}
