using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Accounts;

namespace Redix.Api.Tests;

/// <summary>redix-api #98: a plain edit is PUT and a delete is DELETE, on the route of the thing itself; every other change stays POST.</summary>
[Collection(DatabaseCollection.Name)]
public class VerbsTests(MigratedApiFactory factory)
{
    [Fact]
    public void NoRouteSaysDeleteOrRemoveInItsPath()
    {
        var named = factory.Services.GetRequiredService<IEnumerable<EndpointDataSource>>().SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .Select(route => route.RoutePattern.RawText!).Where(path => path.EndsWith("/delete") || path.EndsWith("/remove"));

        Assert.Empty(named);
    }

    [Fact]
    public async Task AnEditIsPutAndAPostToItIsRefused()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        var path = $"/projects/{ledger.ProjectId}/accounts/{ledger.AccountId}";

        var posted = await ledger.Client.PostAsJsonAsync(path, new EditAccountRequest("الحساب الجديد"), MigratedApiFactory.Json);
        var put = await ledger.Client.PutAsJsonAsync(path, new EditAccountRequest("الحساب الجديد"), MigratedApiFactory.Json);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, posted.StatusCode);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
    }

    [Fact]
    public async Task ADeleteIsDeleteAndTheOldDeleteRouteIsGone()
    {
        var ledger = await Ledger.WithFinanceAsync(factory);
        (await ledger.Client.PostAsJsonAsync($"/projects/{ledger.ProjectId}/accounts", new AddAccountRequest("احتياطي", AccountType.Bank), MigratedApiFactory.Json)).EnsureSuccessStatusCode();
        var spare = (await ledger.Client.GetFromJsonAsync<List<AccountView>>($"/projects/{ledger.ProjectId}/accounts", MigratedApiFactory.Json))!.Single(row => row.Name == "احتياطي").Id;
        var path = $"/projects/{ledger.ProjectId}/accounts/{spare}";

        var oldRoute = await ledger.Client.PostAsync($"{path}/delete", null);
        var deleted = await ledger.Client.DeleteAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, oldRoute.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
    }
}
