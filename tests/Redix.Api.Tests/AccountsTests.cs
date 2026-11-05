using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class AccountsTests(MigratedApiFactory factory)
{
    private const string Password = "Accounts-pass-1";
    private const string Finance = "users.roleFinance";
    private const string Field = "users.roleField";
    private const string Reader = "users.roleReader";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    public static TheoryData<string> UnacceptableNames => ["   ", new string('x', Project.NameMaxLength + 1)];

    public static TheoryData<decimal> UnacceptableBalances => [1.234m, Money.Largest + 1, -Money.Largest - 1];

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);
        return (client, project);
    }

    private async Task<(User User, HttpClient Client)> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, project.Id, role);
        return (user, await factory.ClientSignedInAsAsync(user, Password));
    }

    private static async Task<List<AccountView>> AccountsOf(HttpClient client, Guid projectId, string path = "") =>
        (await client.GetFromJsonAsync<List<AccountView>>($"/projects/{projectId}/accounts{path}", Json))!;

    private static async Task<AccountView> AddAsync(HttpClient client, Guid projectId, string name, AccountType type = AccountType.Bank, decimal openingBalance = 0, Guid custodianId = default)
    {
        (await client.PostAsJsonAsync($"/projects/{projectId}/accounts", new AddAccountRequest(name, type, openingBalance) { CustodianId = custodianId }, Json)).EnsureSuccessStatusCode();
        return (await AccountsOf(client, projectId)).Single(account => account.Name == name);
    }

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    [Fact]
    public async Task FinanceAddsABankAccountWithItsOpeningBalance()
    {
        var (client, project) = await FinanceAsync();

        var account = await AddAsync(client, project.Id, "حساب المؤسسة", openingBalance: 4210.35m);

        Assert.Equal(AccountType.Bank, account.Type);
        Assert.Equal(4210.35m, account.OpeningBalance);
        Assert.Equal(4210.35m, account.Balance);
        Assert.Equal("", account.CustodianId);
        Assert.Equal("", account.CustodianName);
        Assert.True(account.IsActive);
    }

    [Fact]
    public async Task FinanceAddsACashCustodyHeldBySomeoneInTheProject()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);

        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 2750, nawaf.Id);

        Assert.Equal(AccountType.CashCustody, custody.Type);
        Assert.Equal(nawaf.Id.ToString(), custody.CustodianId);
        Assert.Equal(nawaf.DisplayName, custody.CustodianName);
    }

    [Fact]
    public async Task ABankAccountMayHaveACustodian()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);

        var account = await AddAsync(client, project.Id, "البطاقة مع نواف", custodianId: nawaf.Id);

        Assert.Equal(nawaf.Id.ToString(), account.CustodianId);
    }

    [Fact]
    public async Task AnOverdrawnAccountMayOpenBelowZero()
    {
        var (client, project) = await FinanceAsync();

        Assert.Equal(-120.40m, (await AddAsync(client, project.Id, "مكشوف", openingBalance: -120.40m)).OpeningBalance);
    }

    [Fact]
    public async Task ACashCustodyNeedsACustodian()
    {
        var (client, project) = await FinanceAsync();

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("كاش", AccountType.CashCustody, 0), Json);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("custodianRequired", await CodeOf(refused));
        Assert.Empty(await AccountsOf(client, project.Id));
    }

    [Fact]
    public async Task AnEditCannotLeaveACashCustodyWithNobody()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 0, nawaf.Id);

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{custody.Id}", new EditAccountRequest("كاش مع نواف", 0), Json);

        Assert.Equal("custodianRequired", await CodeOf(refused));
    }

    [Fact]
    public async Task ACustodianMustBeInTheProject()
    {
        var (client, project) = await FinanceAsync();
        var stranger = await factory.AddUserAsync(Password);
        var account = await AddAsync(client, project.Id, "حساب");

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("كاش", AccountType.CashCustody, 0) { CustodianId = stranger.Id }, Json),
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{account.Id}", new EditAccountRequest("حساب", 0) { CustodianId = stranger.Id }, Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("unknownCustodian", await CodeOf(refused));
        }
    }

    [Theory]
    [MemberData(nameof(UnacceptableNames))]
    public async Task AnAccountNeedsANameOfAtMost100Characters(string name)
    {
        var (client, project) = await FinanceAsync();
        var account = await AddAsync(client, project.Id, "حساب");

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest(name, AccountType.Bank, 0), Json),
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{account.Id}", new EditAccountRequest(name, 0), Json),
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
            Assert.Equal("invalidName", await CodeOf(refused));
        }
    }

    [Theory]
    [MemberData(nameof(UnacceptableBalances))]
    public async Task AnOpeningBalanceHasTwoDecimalsAndALimit(decimal openingBalance)
    {
        var (client, project) = await FinanceAsync();

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حساب", AccountType.Bank, openingBalance), Json);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidOpeningBalance", await CodeOf(refused));
    }

    [Fact]
    public async Task AnEditChangesTheNameTheOpeningBalanceAndTheCustodian()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var (fawaz, _) = await MemberAsync(project, Field);
        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 100, nawaf.Id);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{custody.Id}", new EditAccountRequest("كاش مع فواز", 250.5m) { CustodianId = fawaz.Id }, Json)).EnsureSuccessStatusCode();

        var edited = (await AccountsOf(client, project.Id)).Single();
        Assert.Equal("كاش مع فواز", edited.Name);
        Assert.Equal(250.5m, edited.OpeningBalance);
        Assert.Equal(fawaz.Id.ToString(), edited.CustodianId);
        Assert.Equal(AccountType.CashCustody, edited.Type);
    }

    [Fact]
    public async Task AnEditCanLeaveABankAccountWithNobody()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        var account = await AddAsync(client, project.Id, "البطاقة", custodianId: nawaf.Id);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{account.Id}", new EditAccountRequest("البطاقة", 0), Json)).EnsureSuccessStatusCode();

        Assert.Equal("", (await AccountsOf(client, project.Id)).Single().CustodianId);
        await factory.WithDatabaseAsync(async db => Assert.False(await db.AccountCustodians.AnyAsync(row => row.AccountId == account.Id)));
    }

    [Fact]
    public async Task ADisabledAccountLeavesEntryAndStaysOnTheList()
    {
        var (client, project) = await FinanceAsync();
        var account = await AddAsync(client, project.Id, "حساب");

        (await client.PostAsync($"/projects/{project.Id}/accounts/{account.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.Empty(await AccountsOf(client, project.Id, "/offered"));
        Assert.False((await AccountsOf(client, project.Id)).Single().IsActive);
    }

    [Fact]
    public async Task EnablingBringsAnAccountBackIntoEntry()
    {
        var (client, project) = await FinanceAsync();
        var account = await AddAsync(client, project.Id, "حساب");
        (await client.PostAsync($"/projects/{project.Id}/accounts/{account.Id}/disable", null)).EnsureSuccessStatusCode();

        (await client.PostAsync($"/projects/{project.Id}/accounts/{account.Id}/enable", null)).EnsureSuccessStatusCode();

        Assert.Single(await AccountsOf(client, project.Id, "/offered"));
    }

    [Fact]
    public async Task BanksComeFirstThenInTheOrderTheyWereAdded()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 0, nawaf.Id);
        await AddAsync(client, project.Id, "الأول");
        await AddAsync(client, project.Id, "الثاني");

        Assert.Equal(["الأول", "الثاني", "كاش مع نواف"], (await AccountsOf(client, project.Id)).Select(account => account.Name));
    }

    [Fact]
    public async Task ACustodianSeesTheirCustodyAndEveryOtherAccountWithoutChangingThem()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, nawafClient) = await MemberAsync(project, Field);
        var bank = await AddAsync(client, project.Id, "حساب المؤسسة", openingBalance: 64250.75m);
        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 2750, nawaf.Id);

        var seen = await AccountsOf(nawafClient, project.Id);

        Assert.Equal(2750m, seen.Single(account => account.IsMine).Balance);
        Assert.Equal(custody.Id, seen.Single(account => account.IsMine).Id);
        Assert.Equal(64250.75m, seen.Single(account => account.Id == bank.Id).Balance);
        foreach (var refused in new[]
        {
            await nawafClient.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("حسابي", AccountType.Bank, 0), Json),
            await nawafClient.PostAsJsonAsync($"/projects/{project.Id}/accounts/{bank.Id}", new EditAccountRequest("حسابي", 0), Json),
            await nawafClient.PostAsJsonAsync($"/projects/{project.Id}/accounts/{custody.Id}", new EditAccountRequest("حسابي", 0) { CustodianId = nawaf.Id }, Json),
            await nawafClient.PostAsync($"/projects/{project.Id}/accounts/{custody.Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        }
    }

    [Fact]
    public async Task FieldIsOfferedOnlyTheAccountsTheyHold()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, nawafClient) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "حساب المؤسسة");
        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 0, nawaf.Id);

        Assert.Equal([custody.Id], (await AccountsOf(nawafClient, project.Id, "/offered")).Select(account => account.Id));
    }

    [Fact]
    public async Task FieldHoldingNothingIsOfferedNothing()
    {
        var (client, project) = await FinanceAsync();
        var (_, fieldClient) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "حساب المؤسسة");

        Assert.Empty(await AccountsOf(fieldClient, project.Id, "/offered"));
    }

    [Fact]
    public async Task FieldIsNotOfferedADisabledAccountTheyHold()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, nawafClient) = await MemberAsync(project, Field);
        var custody = await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 0, nawaf.Id);

        (await client.PostAsync($"/projects/{project.Id}/accounts/{custody.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.Empty(await AccountsOf(nawafClient, project.Id, "/offered"));
    }

    [Fact]
    public async Task FinanceIsOfferedEveryActiveAccount()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        await AddAsync(client, project.Id, "حساب المؤسسة");
        await AddAsync(client, project.Id, "كاش مع نواف", AccountType.CashCustody, 0, nawaf.Id);

        Assert.Equal(2, (await AccountsOf(client, project.Id, "/offered")).Count);
    }

    [Fact]
    public async Task OneProjectsAccountsAreNeverAnothers()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var theirs = await AddAsync(otherClient, other.Id, "حسابهم");

        Assert.Empty(await AccountsOf(client, project.Id));
        foreach (var missed in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{theirs.Id}", new EditAccountRequest("سرقة", 0), Json),
            await client.PostAsync($"/projects/{project.Id}/accounts/{theirs.Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.NotFound, missed.StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/accounts")).StatusCode);
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (client, project) = await FinanceAsync();
        var account = await AddAsync(client, project.Id, "حساب");
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("آخر", AccountType.Bank, 0), Json),
            await client.PostAsJsonAsync($"/projects/{project.Id}/accounts/{account.Id}", new EditAccountRequest("آخر", 0), Json),
            await client.PostAsync($"/projects/{project.Id}/accounts/{account.Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Single(await AccountsOf(client, project.Id));
    }

    [Theory]
    [InlineData(Reader)]
    [InlineData("users.roleManager")]
    public async Task EveryoneWithAccessReadsAccountsAndOnlyFinanceChangesThem(string role)
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "حساب");
        var (_, memberClient) = await MemberAsync(project, role);

        Assert.Single(await AccountsOf(memberClient, project.Id));
        Assert.Equal(HttpStatusCode.Forbidden, (await memberClient.PostAsJsonAsync($"/projects/{project.Id}/accounts", new AddAccountRequest("آخر", AccountType.Bank, 0), Json)).StatusCode);
    }

    [Fact]
    public async Task OnlyFinanceChargesAnyAccount()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            var holders = await db.RolePermissions.Where(held => held.Permission == Permission.ChargeAnyAccount).Select(held => held.RoleId).ToListAsync();
            Assert.Equal([RoleSeed.IdOf(Finance)], holders);
        });
    }

    [Fact]
    public async Task TheMembersListNamesEachActivePersonOnceWithNothingElse()
    {
        var (client, project) = await FinanceAsync();
        var (nawaf, _) = await MemberAsync(project, Field);
        await factory.GiveRoleAsync(nawaf.Id, project.Id, Reader);
        var (gone, _) = await MemberAsync(project, Field);
        await factory.DisableAsync(gone);
        var stranger = await factory.AddUserAsync(Password);

        var members = await client.GetFromJsonAsync<List<JsonElement>>($"/projects/{project.Id}/members", Json);

        Assert.Single(members!, member => member.GetProperty("id").GetGuid() == nawaf.Id);
        Assert.DoesNotContain(members!, member => member.GetProperty("id").GetGuid() == gone.Id || member.GetProperty("id").GetGuid() == stranger.Id);
        Assert.All(members!, member => Assert.Equal(["id", "displayName"], member.EnumerateObject().Select(property => property.Name)));
    }

    [Fact]
    public async Task EveryoneWithAccessReadsTheMembersAndNobodyElse()
    {
        var (_, project) = await FinanceAsync();
        var (_, reader) = await MemberAsync(project, Reader);
        var (outsider, _) = await FinanceAsync("غصون 5");

        Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync($"/projects/{project.Id}/members")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.GetAsync($"/projects/{project.Id}/members")).StatusCode);
    }
}
