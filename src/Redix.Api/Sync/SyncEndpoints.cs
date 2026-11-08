using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Sync;

/// <summary>What a phone keeps so that it can work without a network (UC-030, FR-16).</summary>
public static class SyncEndpoints
{
    /// <summary>Maps the route, for anyone who reads the project.</summary>
    /// <param name="app">The app.</param>
    public static void MapSyncEndpoints(this WebApplication app) =>
        app.MapGet("/projects/{projectId:guid}/sync", Changes).RequireAuthorization(nameof(Permission.ReadProject));

    /// <summary>
    /// Every list a person may see and needs to enter an entry, by the address the app reads it at, so that a kept copy answers that same read. The stamp says what they hold: <paramref name="since"/> being
    /// the current stamp answers no lists, and anything else, a first sync included, answers them all. One stamp covers all of them, because a row that left a list has nothing left to compare.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the lists live in.</param>
    /// <param name="json">How the API writes JSON, so the stamp follows what is sent.</param>
    /// <param name="since">The stamp of the last sync; empty for the first.</param>
    public static async Task<SyncView> Changes(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, IOptions<JsonOptions> json, string since = "")
    {
        var lists = await ListsAsync(projectId, principal, db);
        var stamp = Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(lists, json.Value.SerializerOptions)));
        return new SyncView(stamp, since == stamp ? [] : lists);
    }

    private static async Task<Dictionary<string, object>> ListsAsync(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var project = $"/projects/{projectId}";
        var purchases = await CategoriesEndpoints.Offered(projectId, new ItemTypeInUrl(ItemType.Purchases), db);
        var payments = await CategoriesEndpoints.Offered(projectId, new ItemTypeInUrl(ItemType.ContractorPayments), db);
        var lists = new Dictionary<string, object>
        {
            [$"{project}/categories/offered?itemType=purchases"] = purchases,
            [$"{project}/categories/offered?itemType=contractorPayments"] = payments,
            [$"{project}/entry-tables"] = await CategoriesEndpoints.EntryTables(projectId, principal, db),
            [$"{project}/suppliers"] = await SuppliersEndpoints.List(projectId, db),
            [$"{project}/suppliers/search?text="] = await SuppliersEndpoints.Search(projectId, principal, db),
            [$"{project}/contracts"] = await ContractsEndpoints.List(projectId, db),
            [$"{project}/accounts"] = await AccountsEndpoints.List(projectId, principal, db),
            [$"{project}/accounts/offered"] = await AccountsEndpoints.Offered(projectId, principal, db),
            [$"{project}/payment-methods"] = await PaymentMethodsEndpoints.List(projectId, db),
            [$"{project}/payment-methods/offered"] = await PaymentMethodsEndpoints.Offered(projectId, db),
            [$"{project}/entries/mine"] = await EntriesEndpoints.Mine(projectId, principal, db),
        };
        foreach (var itemId in purchases.Concat(payments).SelectMany(category => category.Items).Select(item => item.Id).Distinct())
        {
            lists[$"{project}/items/{itemId}/columns"] = await ColumnsEndpoints.ViewsOfItemAsync(itemId, db);
        }
        return lists;
    }
}

/// <summary>What changed since the last sync.</summary>
/// <param name="Stamp">What the lists hold now, to send back at the next sync.</param>
/// <param name="Lists">The lists, by the address the app reads each at; none when nothing changed.</param>
public record SyncView(string Stamp, Dictionary<string, object> Lists);
