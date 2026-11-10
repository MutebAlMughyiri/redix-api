using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>redix-app #101: the partners' history, the accounts' history for partners, seen through BR-31.</summary>
[Collection(SecondDatabaseCollection.Name)]
public class PartnerHistoryTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static async Task<List<PartnerHistoryLine>> HistoryAsync(HttpClient client, Ledger ledger, string query = "") =>
        (await client.GetFromJsonAsync<List<PartnerHistoryLine>>($"/projects/{ledger.ProjectId}/partners/history{query}", Json))!;

    private static async Task<Guid> PartnerAsync(Ledger ledger, string name, Guid userId = default)
    {
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners", new PartnerRequest(name) { UserId = userId }, Json)).EnsureSuccessStatusCode();
        return (await ledger.Client.GetFromJsonAsync<List<PartnerView>>($"/projects/{ledger.ProjectId}/partners", Json))!.Single(row => row.Name == name).Id;
    }

    private static async Task<Guid> MoveAsync(Ledger ledger, Guid partnerId, decimal amount, bool isWithdrawal = false) =>
        await ledger.ApproveAsync(await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries", await ledger.PartnerEntryAsync(partnerId, amount, isWithdrawal), Json));

    [Fact]
    public async Task UC_101_EachPartnerActionAndApprovedOrReversedCapitalMovementIsALineNewestFirst()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (_, memberId) = await ledger.MemberAsync("Reader");
        await factory.WithDatabaseAsync(db => db.Users.Where(user => user.Id == memberId).ExecuteUpdateAsync(user => user.SetProperty(row => row.DisplayName, "نواف")));
        var khaled = await PartnerAsync(ledger, "خالد م");
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{khaled}", new PartnerRequest("خالد"), Json)).EnsureSuccessStatusCode();
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/partners/{khaled}", new PartnerRequest("خالد") { UserId = memberId }, Json)).EnsureSuccessStatusCode();
        var cameIn = await MoveAsync(ledger, khaled, 1000);
        await MoveAsync(ledger, khaled, 200, isWithdrawal: true);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/entries/{cameIn}/reverse", new RejectEntryRequest("إدخال خاطئ"), Json)).EnsureSuccessStatusCode();
        var nour = await PartnerAsync(ledger, "نور");
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/partners/{nour}/delete", null)).EnsureSuccessStatusCode();

        var lines = await HistoryAsync(ledger.Client, ledger);

        Assert.Equal(
            [PartnerHistoryKind.PartnerDeleted, PartnerHistoryKind.PartnerCreated, PartnerHistoryKind.Reversed, PartnerHistoryKind.CapitalOut, PartnerHistoryKind.CapitalIn, PartnerHistoryKind.PartnerAccount, PartnerHistoryKind.PartnerChanged, PartnerHistoryKind.PartnerCreated],
            lines.Select(line => line.Kind));
        Assert.Equal(("نور", "خالد"), (lines[0].Partner, lines[^1].Partner));
        Assert.Equal("خالد م", lines.Single(line => line.Kind == PartnerHistoryKind.PartnerChanged).NameWas);
        Assert.Equal("نواف", lines.Single(line => line.Kind == PartnerHistoryKind.PartnerAccount).Login);
        var capitalIn = lines.Single(line => line.Kind == PartnerHistoryKind.CapitalIn);
        Assert.Equal((1000m, "خالد"), (capitalIn.Amount, capitalIn.Partner));
        Assert.NotEmpty(capitalIn.Account);
    }

    [Fact]
    public async Task UC_101_OnePartnersLinesAndWhatICanBeKeptAlone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var khaled = await PartnerAsync(ledger, "خالد");
        var saad = await PartnerAsync(ledger, "سعد");
        await MoveAsync(ledger, saad, 500);

        var khaledsLines = await HistoryAsync(ledger.Client, ledger, $"?partner={khaled}");
        var mine = await HistoryAsync(ledger.Client, ledger, "?mine=true");

        Assert.Equal(["خالد"], khaledsLines.Select(line => line.Partner).Distinct());
        Assert.Equal(3, mine.Count);
    }

    [Fact]
    public async Task UC_101_BR_31_SomeoneWithChosenPartnersSeesOnlyTheirLinesAndSomeoneWithNeitherSeesNone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var (chosenClient, chosenId) = await ledger.MemberAsync("Reader");
        var (otherClient, _) = await ledger.MemberAsync("Reader");
        var khaled = await PartnerAsync(ledger, "خالد");
        var saad = await PartnerAsync(ledger, "سعد");
        await MoveAsync(ledger, saad, 500);
        await ledger.ChooseAsync(chosenId, khaled);

        var seenByChosen = await HistoryAsync(chosenClient, ledger);
        var unseen = await chosenClient.GetAsync($"/projects/{ledger.ProjectId}/partners/history?partner={saad}");

        Assert.Equal(["خالد"], seenByChosen.Select(line => line.Partner).Distinct());
        Assert.Empty(await HistoryAsync(otherClient, ledger));
        Assert.Equal((HttpStatusCode.BadRequest, "unknownPartner"), (unseen.StatusCode, await Ledger.CodeOf(unseen)));
    }
}
