using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Redix.Api.Projects;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>UC-064: engineering blueprints for a project or a unit.</summary>
[Collection(DatabaseCollection.Name)]
public class BlueprintsTests(MigratedApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = Ledger.Json;

    private static Task<HttpResponseMessage> AddAsync(Ledger ledger, BlueprintRequest request) => ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/blueprints", request, Json);

    private static async Task<List<BlueprintView>> ListAsync(HttpClient client, Guid projectId) => (await client.GetFromJsonAsync<List<BlueprintView>>($"/projects/{projectId}/blueprints", Json))!;

    [Fact]
    public async Task UC_064_TheManagerKeepsBlueprintsAndEveryoneReadsThemTheProjectsFirst()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var unit = ledger.Project.Units[0];
        (await AddAsync(ledger, new BlueprintRequest(await ledger.FileAsync(), "المخطط المعماري") { UnitId = unit.Id })).EnsureSuccessStatusCode();
        (await AddAsync(ledger, new BlueprintRequest(await ledger.FileAsync(), "  مخطط الموقع العام "))).EnsureSuccessStatusCode();
        var (reader, _) = await ledger.MemberAsync("users.roleReader");

        var seen = await ListAsync(reader, ledger.ProjectId);

        Assert.Equal([("مخطط الموقع العام", ""), ("المخطط المعماري", unit.Name)], seen.Select(blueprint => (blueprint.Title, blueprint.UnitName)));
        Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync($"/projects/{ledger.ProjectId}/attachments/{seen[0].AttachmentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await reader.PostAsJsonAsync($"/projects/{ledger.ProjectId}/blueprints", new BlueprintRequest(seen[0].AttachmentId, "مخطط"), Json)).StatusCode);
    }

    [Fact]
    public async Task ABlueprintNeedsATitleAFileOfTheProjectAndOneOfItsOwnUnits()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var file = await ledger.FileAsync();
        var shared = ledger.Project.EntryUnits.Single(unit => ledger.Project.Units.All(own => own.Id != unit.Id)).Id;

        Assert.Equal("invalidTitle", await Ledger.CodeOf(await AddAsync(ledger, new BlueprintRequest(file, " "))));
        Assert.Equal("invalidTitle", await Ledger.CodeOf(await AddAsync(ledger, new BlueprintRequest(file, new string('م', Blueprint.TitleMaxLength + 1)))));
        Assert.Equal("unknownAttachment", await Ledger.CodeOf(await AddAsync(ledger, new BlueprintRequest(Guid.NewGuid(), "مخطط"))));
        Assert.Equal("unknownUnit", await Ledger.CodeOf(await AddAsync(ledger, new BlueprintRequest(file, "مخطط") { UnitId = shared })));
        (await AddAsync(ledger, new BlueprintRequest(file, "مخطط"))).EnsureSuccessStatusCode();
        Assert.Equal("blueprintAlreadyKept", await Ledger.CodeOf(await AddAsync(ledger, new BlueprintRequest(file, "مخطط آخر"))));
    }

    [Fact]
    public async Task ARemovedBlueprintLeavesTheListButItsFileStaysAndItsUnitCannotBeDeleted()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var unit = ledger.Project.Units[0].Id;
        (await AddAsync(ledger, new BlueprintRequest(await ledger.FileAsync(), "مخطط الكهرباء") { UnitId = unit })).EnsureSuccessStatusCode();
        var kept = Assert.Single(await ListAsync(ledger.Client, ledger.ProjectId));

        Assert.Equal("unitInUse", await Ledger.CodeOf(await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/units/{unit}/delete", null)));
        (await ledger.Client.PostAsync($"/projects/{ledger.ProjectId}/blueprints/{kept.Id}/delete", null)).EnsureSuccessStatusCode();

        Assert.Empty(await ListAsync(ledger.Client, ledger.ProjectId));
        Assert.Equal(HttpStatusCode.OK, (await ledger.Client.GetAsync($"/projects/{ledger.ProjectId}/attachments/{kept.AttachmentId}")).StatusCode);
    }
}
