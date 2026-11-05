using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Translations;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class RolesTests(MigratedApiFactory factory)
{
    private const string Password = "Roles-pass-1";

    private async Task<(User User, HttpClient Client, Guid ProjectId)> ManagerAsync()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        return (user, client, (await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}")).Id);
    }

    public static TheoryData<string, RoleScope, Permission[]> SectionTwo => new()
    {
        { "users.roleAdmin", RoleScope.System, [Permission.ApproveProjects] },
        { "users.roleManager", RoleScope.Project, [Permission.ManageUsers, Permission.ReadProject, Permission.SeeAllCapital] },
        { "users.roleFinance", RoleScope.Project, [Permission.ReadProject, Permission.SeeAllCapital, Permission.ManageProject, Permission.ManageStructure, Permission.ManageSettings, Permission.ChargeAnyAccount, Permission.AddSupplier, Permission.RecordEntries, Permission.AttachFiles] },
        { "users.roleField", RoleScope.Project, [Permission.ReadProject, Permission.AddSupplier, Permission.RecordEntries, Permission.AttachFiles] },
        { "users.roleReader", RoleScope.Project, [Permission.ReadProject] },
    };

    [Theory]
    [MemberData(nameof(SectionTwo))]
    public async Task TheRolesHoldWhatBrsSectionTwoSays(string word, RoleScope scope, Permission[] permissions)
    {
        await factory.WithDatabaseAsync(async db =>
        {
            var role = await db.Roles.SingleAsync(role => role.WordKey == word);
            Assert.Equal(scope, role.Scope);
            var held = await db.RolePermissions.Where(held => held.RoleId == role.Id).Select(held => held.Permission).ToListAsync();
            Assert.Equal(permissions.Order(), held.Order());
        });
    }

    [Fact]
    public async Task RolesAreOfferedInOrderAndTheAdminsIsNot()
    {
        var (_, client, projectId) = await ManagerAsync();

        var roles = (await client.GetFromJsonAsync<List<RoleView>>($"/projects/{projectId}/roles", MigratedApiFactory.Json))!;

        Assert.Equal(["users.roleManager", "users.roleFinance", "users.roleField", "users.roleReader"], roles.Select(role => role.WordKey));
    }

    [Fact]
    public async Task ANewRowGivesARoleAPermissionAtTheNextRequest()
    {
        var (_, starter, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, "users.roleReader");
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var addUnit = () => client.PostAsJsonAsync($"/projects/{projectId}/units", new AddUnitRequest("الفلة الغربية"));
        Assert.Equal(HttpStatusCode.Forbidden, (await addUnit()).StatusCode);
        var held = new RolePermission { RoleId = RoleSeed.IdOf("users.roleReader"), Permission = Permission.ManageProject };

        try
        {
            await factory.WithDatabaseAsync(db =>
            {
                db.RolePermissions.Add(held);
                return db.SaveChangesAsync();
            });

            Assert.Equal(HttpStatusCode.OK, (await addUnit()).StatusCode);
        }
        finally
        {
            await factory.WithDatabaseAsync(db => db.RolePermissions.Where(row => row.Id == held.Id).ExecuteDeleteAsync());
        }

        Assert.Equal(HttpStatusCode.Forbidden, (await addUnit()).StatusCode);
    }

    [Fact]
    public async Task ANewRoleInTheDatabaseIsOfferedAndWorks()
    {
        const string word = "users.roleAuditor";
        var key = new TranslationKey { Key = word };
        var role = new Role { WordKey = word, Scope = RoleScope.Project, SortOrder = 2 };
        var (manager, client, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, projectId, "users.roleReader");

        try
        {
            await factory.WithDatabaseAsync(db =>
            {
                db.TranslationKeys.Add(key);
                db.Roles.Add(role);
                db.RolePermissions.Add(new RolePermission { RoleId = role.Id, Permission = Permission.ManageProject });
                return db.SaveChangesAsync();
            });

            var offered = (await client.GetFromJsonAsync<List<RoleView>>($"/projects/{projectId}/roles", MigratedApiFactory.Json))!;
            Assert.Contains(offered, listed => listed.Id == role.Id && listed.WordKey == word);
            Assert.True((await client.PostAsync($"/projects/{projectId}/users/{user.Id}/roles/{role.Id}", null)).IsSuccessStatusCode);
            var theirs = await factory.ClientSignedInAsAsync(user, Password);
            Assert.Equal(HttpStatusCode.OK, (await theirs.PostAsync($"/projects/{projectId}/close", null)).StatusCode);
        }
        finally
        {
            await factory.WithDatabaseAsync(async db =>
            {
                await db.ProjectMembers.Where(row => row.RoleId == role.Id).ExecuteDeleteAsync();
                await db.RolePermissions.Where(row => row.RoleId == role.Id).ExecuteDeleteAsync();
                await db.Roles.Where(row => row.Id == role.Id).ExecuteDeleteAsync();
                await db.TranslationKeys.Where(row => row.Id == key.Id).ExecuteDeleteAsync();
            });
        }
    }

    [Fact]
    public async Task ARoleHoldingAPermissionEverywhereReachesEveryProjectAndAnUnknownOneIsNotFound()
    {
        var (_, starter, projectId) = await ManagerAsync();
        var client = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        var held = new RolePermission { RoleId = RoleSeed.AdminId, Permission = Permission.ManageProject };

        try
        {
            await factory.WithDatabaseAsync(db =>
            {
                db.RolePermissions.Add(held);
                return db.SaveChangesAsync();
            });

            Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/projects/{projectId}/close", null)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{Guid.CreateVersion7()}/close", null)).StatusCode);
        }
        finally
        {
            await factory.WithDatabaseAsync(db => db.RolePermissions.Where(row => row.Id == held.Id).ExecuteDeleteAsync());
        }
    }

    [Fact]
    public async Task TheDatabaseRefusesAPermissionTheCodeDoesNotKnow()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            var sql = "INSERT INTO RolePermissions (Id, RoleId, Permission) VALUES (NEWID(), {0}, 'NotAPermission')";
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(sql, RoleSeed.IdOf("users.roleReader")));
        });
    }

    [Fact]
    public async Task TheDatabaseRefusesARoleWithNoWord()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            db.Roles.Add(new Role { WordKey = "users.noSuchWord", Scope = RoleScope.Project, SortOrder = 9 });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        });
    }

    [Fact]
    public async Task TheDatabaseRefusesDeletingARoleInUse()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            await Assert.ThrowsAnyAsync<Exception>(() => db.Roles.Where(role => role.WordKey == "users.roleManager").ExecuteDeleteAsync());
        });
    }

    [Fact]
    public void EveryChangingRouteNamesAPolicy()
    {
        var sources = factory.Services.GetRequiredService<IEnumerable<EndpointDataSource>>();
        var routes = sources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .Where(route => route.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Any(method => method != "GET" && method != "HEAD") == true)
            .ToList();
        Assert.NotEmpty(routes);

        var open = routes.Where(route =>
            route.Metadata.GetMetadata<IAllowAnonymous>() is null
            && route.Metadata.GetMetadata<AuthorizationPolicy>() is null
            && !route.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(data => data.Policy is not null))
            .Select(route => route.RoutePattern.RawText);

        Assert.Empty(open);
    }
}
