using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Tests;

/// <summary>redix-api #120: a person's access is read at most once and kept between requests, and every change to it counts on their next request (AC-32.3, AC-32.4).</summary>
[Collection(FourthDatabaseCollection.Name)]
public class AccessTests(MigratedApiFactory factory)
{
    private const string Password = "Access-kept-1";

    private static UserAccess Nothing() => new([], Array.Empty<(Guid, Permission)>().ToLookup(row => row.Item1, row => row.Item2), Array.Empty<(Guid, Guid)>().ToLookup(row => row.Item1, row => row.Item2));

    /// <summary>A reader that counts how often it reads.</summary>
    private sealed class CountedReads
    {
        public int Count { get; private set; }

        public Task<UserAccess> ReadAsync()
        {
            Count++;
            return Task.FromResult(Nothing());
        }
    }

    [Fact]
    public async Task AC_32_3_ASecondAskReadsNothingWhileNothingChanged()
    {
        using var cache = new AccessCache();
        var reads = new CountedReads();
        var userId = Guid.CreateVersion7();

        var first = await cache.OfAsync(userId, reads.ReadAsync);
        var second = await cache.OfAsync(userId, reads.ReadAsync);

        Assert.Equal(1, reads.Count);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task AC_32_3_AForgottenPersonIsReadAgainAndNoOneElse()
    {
        using var cache = new AccessCache();
        var reads = new CountedReads();
        var forgotten = Guid.CreateVersion7();
        var kept = Guid.CreateVersion7();
        await cache.OfAsync(forgotten, reads.ReadAsync);
        await cache.OfAsync(kept, reads.ReadAsync);

        cache.Forget([forgotten]);
        await cache.OfAsync(forgotten, reads.ReadAsync);
        await cache.OfAsync(kept, reads.ReadAsync);

        Assert.Equal(3, reads.Count);
    }

    [Fact]
    public async Task AC_32_3_AReadThatRacedAChangeIsAnsweredButNotKept()
    {
        using var cache = new AccessCache();
        var userId = Guid.CreateVersion7();
        var reads = 0;
        Task<UserAccess> ReadWhileAChangeLands()
        {
            reads++;
            cache.Forget([userId]);
            return Task.FromResult(Nothing());
        }

        await cache.OfAsync(userId, ReadWhileAChangeLands);
        await cache.OfAsync(userId, ReadWhileAChangeLands);

        Assert.Equal(2, reads);
    }

    [Fact]
    public async Task AC_32_3_ForgettingEveryoneReadsEveryoneAgain()
    {
        using var cache = new AccessCache();
        var reads = new CountedReads();
        Guid[] people = [Guid.CreateVersion7(), Guid.CreateVersion7()];
        foreach (var userId in people)
        {
            await cache.OfAsync(userId, reads.ReadAsync);
        }

        cache.ForgetEveryone();
        foreach (var userId in people)
        {
            await cache.OfAsync(userId, reads.ReadAsync);
        }

        Assert.Equal(4, reads.Count);
    }

    [Fact]
    public async Task AC_32_4_AnUntickedPermissionIsRefusedOnTheNextRequestAndATickedOneAllowed()
    {
        var manager = await factory.AddUserAsync(Password);
        var managerClient = await factory.ClientSignedInAsAsync(manager, Password);
        var projectId = (await factory.ApprovedProjectAsync(managerClient, $"project-{Guid.CreateVersion7()}")).Id;
        var member = await factory.AddUserAsync(Password);
        await factory.GiveAsync(member.Id, projectId, Holds.Field);
        var memberClient = await factory.ClientSignedInAsAsync(member, Password);
        var reports = $"/projects/{projectId}/periodic-reports";
        var seeReports = $"/projects/{projectId}/users/{member.Id}/permissions/seeReports";
        Assert.Equal(HttpStatusCode.OK, (await memberClient.GetAsync(reports)).StatusCode);

        (await managerClient.DeleteAsync(seeReports)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Forbidden, (await memberClient.GetAsync(reports)).StatusCode);

        (await managerClient.PostAsync(seeReports, null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, (await memberClient.GetAsync(reports)).StatusCode);
    }

    [Fact]
    public void R120_D3_RowsAnAccessIsReadFromAreChangedOnlyThroughSaveChanges()
    {
        var sets = typeof(RedixDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Where(property => property.PropertyType.GetGenericArguments()[0] is var entity && (typeof(IAccessRow).IsAssignableFrom(entity) || entity == typeof(Project)))
            .Select(property => property.Name).ToList();
        var bulk = new Regex($@"\.({string.Join('|', sets)})\b[^;]*?\.Execute(?:Update|Delete)Async", RegexOptions.Singleline);
        var source = Path.Combine(RepositoryRoot(), "src", "Redix.Api");

        var offenders = Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}") && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(path => bulk.IsMatch(File.ReadAllText(path))).ToList();

        Assert.Equal(5, sets.Count);
        Assert.Empty(offenders);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(directory.FullName, "src", "Redix.Api")))
        {
            directory = directory.Parent ?? throw new DirectoryNotFoundException("The repository's src folder");
        }
        return directory.FullName;
    }
}
