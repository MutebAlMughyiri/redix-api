using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class PermissionsTests(MigratedApiFactory factory)
{
    private const string Password = "Permissions-pass-1";

    private async Task<(User User, HttpClient Client, Guid ProjectId)> ManagerAsync()
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        return (user, client, (await factory.ApprovedProjectAsync(client, $"project-{Guid.CreateVersion7()}")).Id);
    }

    [Fact]
    public async Task TheAdminHoldsApprovingProjectsFromTheFirstStart()
    {
        await factory.WithDatabaseAsync(async db =>
        {
            var admin = await db.Users.SingleAsync(user => user.Username == MigratedApiFactory.AdminUsername);
            var held = await db.SystemPermissions.Where(row => row.UserId == admin.Id).Select(row => row.Permission).ToListAsync();
            Assert.Equal([Permission.ApproveProjects], held);
        });
    }

    [Fact]
    public async Task ANewRowGivesAPermissionAtTheNextRequest()
    {
        var (_, _, projectId) = await ManagerAsync();
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, projectId, Holds.Reader);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var addUnit = () => client.PostAsJsonAsync($"/projects/{projectId}/units", new AddUnitRequest("الفلة الغربية"));
        Assert.Equal(HttpStatusCode.Forbidden, (await addUnit()).StatusCode);

        await factory.GiveAsync(user.Id, projectId, [Permission.ManageProject]);
        Assert.Equal(HttpStatusCode.OK, (await addUnit()).StatusCode);

        await factory.TakeAsync(db => db.ProjectPermissions.Where(row => row.UserId == user.Id && row.Permission == Permission.ManageProject));
        Assert.Equal(HttpStatusCode.Forbidden, (await addUnit()).StatusCode);
    }

    [Fact]
    public async Task APermissionHeldEverywhereReachesEveryProjectAndAnUnknownOneIsNotFound()
    {
        var (_, _, projectId) = await ManagerAsync();
        var admin = await factory.AddUserAsync(Password, isAdmin: true);
        var client = await factory.ClientSignedInAsAsync(admin, Password);
        await factory.WithDatabaseAsync(db =>
        {
            db.SystemPermissions.Add(new SystemPermission { UserId = admin.Id, Permission = Permission.ManageProject });
            return db.SaveChangesAsync();
        });

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/projects/{projectId}/close", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{Guid.CreateVersion7()}/close", null)).StatusCode);
    }

    [Fact]
    public async Task TheDatabaseRefusesAPermissionTheCodeDoesNotKnowAndTheAdminsInAProject()
    {
        var (manager, _, projectId) = await ManagerAsync();
        await factory.WithDatabaseAsync(async db =>
        {
            const string sql = "INSERT INTO ProjectPermissions (Id, ProjectId, UserId, Permission) VALUES (NEWID(), {0}, {1}, {2})";
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(sql, projectId, manager.Id, "NotAPermission"));
            await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(sql, projectId, manager.Id, nameof(Permission.ApproveProjects)));
        });
    }

    [Fact]
    public void EveryChangingRouteNamesAPolicy()
    {
        var routes = Endpoints().Where(route => route.Metadata.GetOrderedMetadata<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods).Any(method => method != "GET" && method != "HEAD")).ToList();
        Assert.NotEmpty(routes);

        var open = routes.Where(route =>
            route.Metadata.GetMetadata<IAllowAnonymous>() is null
            && route.Metadata.GetMetadata<AuthorizationPolicy>() is null
            && !route.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(data => data.Policy is not null))
            .Select(route => route.RoutePattern.RawText);

        Assert.Empty(open);
    }

    [Fact]
    public void EveryRouteOfAProjectNamesAPermission()
    {
        var unguarded = Endpoints().Where(route => route.RoutePattern.GetParameter("projectId") is not null && PermissionsOf(route).Count == 0)
            .Select(route => route.DisplayName);

        Assert.Empty(unguarded);
    }

    [Fact]
    public async Task EveryRouteRefusesWhoeverLacksItsPermissionAndOpensOnceItIsTicked()
    {
        var routes = Endpoints().SelectMany(route => route.Metadata.GetOrderedMetadata<IHttpMethodMetadata>().SelectMany(metadata => metadata.HttpMethods).Distinct()
            .Select(method => new GatedRoute(method, route.RoutePattern, PermissionsOf(route)))).Where(route => route.Permissions.Count > 0).ToList();
        var user = await factory.AddUserAsync(Password);
        var wrong = new List<string>();
        await factory.WithDatabaseAsync(db =>
        {
            db.SystemPermissions.AddRange(Enum.GetValues<Permission>().Select(permission => new SystemPermission { UserId = user.Id, Permission = permission }));
            return db.SaveChangesAsync();
        });
        var client = await factory.ClientSignedInAsAsync(user, Password);

        foreach (var permission in routes.SelectMany(route => route.Permissions).Distinct())
        {
            await factory.TakeAsync(db => db.SystemPermissions.Where(row => row.UserId == user.Id && row.Permission == permission));
            foreach (var route in routes.Where(route => route.Permissions.Contains(permission)))
            {
                var status = (await client.SendAsync(route.Request())).StatusCode;
                wrong.AddRange(status == HttpStatusCode.Forbidden ? [] : [$"{route} without {permission} answered {status}"]);
            }

            await factory.WithDatabaseAsync(db =>
            {
                db.SystemPermissions.Add(new SystemPermission { UserId = user.Id, Permission = permission });
                return db.SaveChangesAsync();
            });
        }

        foreach (var route in routes)
        {
            var status = (await client.SendAsync(route.Request())).StatusCode;
            wrong.AddRange(status == HttpStatusCode.Forbidden ? [$"{route} with every permission answered {status}"] : []);
        }

        Assert.NotEmpty(routes);
        Assert.Empty(wrong);
    }

    private IEnumerable<RouteEndpoint> Endpoints() =>
        factory.Services.GetRequiredService<IEnumerable<EndpointDataSource>>().SelectMany(source => source.Endpoints).OfType<RouteEndpoint>();

    private static List<Permission> PermissionsOf(RouteEndpoint route)
    {
        var policies = route.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(data => data.Policy).ToList();
        return Enum.GetValues<Permission>().Where(permission => policies.Contains(permission.ToString())).ToList();
    }

    /// <summary>One method of a route that names a permission, called with made-up ids and no body: the permission is checked before either is read.</summary>
    private record GatedRoute(string Method, RoutePattern Pattern, List<Permission> Permissions)
    {
        public HttpRequestMessage Request() => new(new HttpMethod(Method), "/" + string.Join("/", Pattern.PathSegments.Select(segment => string.Concat(segment.Parts.Select(part => part switch
        {
            RoutePatternLiteralPart literal => literal.Content,
            RoutePatternSeparatorPart separator => separator.Content,
            RoutePatternParameterPart { ParameterPolicies: [{ Content: "guid" }] } => Guid.CreateVersion7().ToString(),
            _ => "1",
        })))));

        public override string ToString() => $"{Method} {Pattern.RawText}";
    }
}
