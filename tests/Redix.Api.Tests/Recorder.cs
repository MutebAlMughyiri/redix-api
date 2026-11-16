using System.Data.Common;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Redix.Api.Email;
using Xunit.Sdk;

namespace Redix.Api.Tests;

/// <summary>
/// Writes, when <c>REDIX_RECORD</c> names a folder, every step one collection's tests take against the API, in order, for the Go replays of redix-api #140 (G12): each
/// request as the API saw it, at the fake clock's time, with its answer; each seed a factory helper puts straight into the database; and any other write straight into
/// the database, which marks the collection as one whose rules are ported by hand. With no folder it records nothing, and the tests run as they always did.
/// </summary>
public sealed class Recorder
{
    private static readonly AsyncLocal<bool> Inside = new();

    /// <summary>The test running, which every step names, so a direct write marks only its own test as one to port by hand (redix-api #140, G17).</summary>
    private static readonly AsyncLocal<string> CurrentTest = new();

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly string _file;
    private readonly Lock _writing = new();
    private int _hosts;

    private Recorder(string file) => _file = file;

    /// <summary>The recorder of the collection whose database is <paramref name="database"/>, writing into <c>REDIX_RECORD</c>'s folder, or one that records nothing.</summary>
    public static Recorder For(string database)
    {
        var folder = Environment.GetEnvironmentVariable("REDIX_RECORD") ?? "";
        if (folder.Length == 0)
        {
            return new Recorder("");
        }
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, $"{database}.jsonl");
        File.Delete(file);
        return new Recorder(file);
    }

    /// <summary>Records <paramref name="kind"/> of seed with what it was given, and keeps the database writes it makes from counting as direct ones.</summary>
    public async Task SeedAsync(string kind, object given, Func<Task> seed)
    {
        Write(new { step = "seed", kind, given });
        await InsideAsync(seed);
    }

    /// <summary>Records an email the API sent, so a replay can match the code a test typed from it to the one the Go API sends.</summary>
    public void Sent(SentEmail email) => Write(new { step = "email", to = email.To, subject = email.Subject, body = email.Body });

    /// <summary>
    /// Records a request and its answer through the API's own pipeline, at <paramref name="time"/>'s moment, naming the host that answered it: a test that builds a host
    /// of its own, with a setting of its own, gets a replay router of its own, so the host's limits are recorded once, as it starts.
    /// </summary>
    public void Use(IApplicationBuilder app, FakeTimeProvider time)
    {
        var host = Interlocked.Increment(ref _hosts);
        var settings = app.ApplicationServices.GetRequiredService<IConfiguration>();
        Write(new
        {
            step = "host", host, emailCodesPerHour = settings.GetValue("EmailCodes:PermitsPerHour", 5), registrationsPerHour = settings.GetValue("Registration:PermitsPerHour", 5),
        });
        UseFor(app, time, host);
    }

    private void UseFor(IApplicationBuilder app, FakeTimeProvider time, int host) => app.Use(async (context, next) =>
    {
        if (_file.Length == 0)
        {
            await next();
            return;
        }
        var at = time.GetUtcNow().UtcDateTime;
        context.Request.EnableBuffering();
        var sent = await BodyOf(context.Request.Body, context.Request.ContentType ?? "");
        context.Request.Body.Position = 0;
        var answer = context.Response.Body;
        using var kept = new MemoryStream();
        context.Response.Body = kept;
        await InsideAsync(() => next());
        kept.Position = 0;
        var answered = await BodyOf(kept, context.Response.ContentType ?? "");
        kept.Position = 0;
        await kept.CopyToAsync(answer);
        context.Response.Body = answer;
        Write(new
        {
            step = "request", host, at, method = context.Request.Method, path = context.Request.Path.Value, query = context.Request.QueryString.Value,
            headers = Headers(context.Request.Headers, "Authorization", "Accept-Language", "If-None-Match"), contentType = context.Request.ContentType ?? "", body = sent,
            status = context.Response.StatusCode, answerType = context.Response.ContentType ?? "", answerHeaders = Headers(context.Response.Headers, "ETag", "Content-Disposition", "WWW-Authenticate", "Allow", "Retry-After", "Location"),
            answer = answered,
        });
    });

    /// <summary>The interceptors that record a database write made outside a request and a seed: through a save, or a bulk update or delete that skips it.</summary>
    public IInterceptor[] DirectWrites => [new DirectWriteInterceptor(this), new BulkWriteInterceptor(this)];

    private static async Task InsideAsync(Func<Task> work)
    {
        var outer = Inside.Value;
        Inside.Value = true;
        try
        {
            await work();
        }
        finally
        {
            Inside.Value = outer;
        }
    }

    private static Dictionary<string, string> Headers(IHeaderDictionary headers, params string[] names) =>
        names.Where(headers.ContainsKey).ToDictionary(name => name, name => headers[name].ToString());

    /// <summary>Text as it is; a file as its length and SHA-256, since a Go file is not .NET's byte for byte.</summary>
    private static async Task<object> BodyOf(Stream body, string contentType)
    {
        using var copy = new MemoryStream();
        await body.CopyToAsync(copy);
        var bytes = copy.ToArray();
        var isText = contentType.Length == 0 || contentType.Contains("json") || contentType.StartsWith("text/") || contentType.Contains("x-www-form-urlencoded");
        if (isText)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        return contentType.StartsWith("multipart/")
            ? new { base64 = Convert.ToBase64String(bytes) }
            : new { length = bytes.Length, sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)) };
    }

    private void Write(object step)
    {
        if (_file.Length == 0)
        {
            return;
        }
        var line = JsonSerializer.SerializeToNode(step, Json)!.AsObject();
        line["test"] = CurrentTest.Value ?? "";
        lock (_writing)
        {
            File.AppendAllText(_file, line.ToJsonString(Json) + "\n");
        }
    }

    private sealed class DirectWriteInterceptor(Recorder recorder) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!Inside.Value)
            {
                var tables = eventData.Context!.ChangeTracker.Entries().Where(entry => entry.State is not EntityState.Unchanged)
                    .Select(entry => entry.Metadata.GetTableName()).Distinct().Order().ToList();
                recorder.Write(new { step = "direct", tables });
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    /// <summary>Records a bulk update, delete or insert made outside a request and a seed, as ExecuteUpdateAsync makes one without a save.</summary>
    private sealed class BulkWriteInterceptor(Recorder recorder) : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var verb = command.CommandText.TrimStart().Split(' ', 2)[0].ToUpperInvariant();
            if (!Inside.Value && verb is "UPDATE" or "DELETE" or "INSERT" or "MERGE")
            {
                recorder.Write(new { step = "direct", tables = new[] { verb } });
            }
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    /// <summary>Names the test that runs, on a collection's definition, so each step the recorder writes says which test took it.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestNamedAttribute : BeforeAfterTestAttribute
    {
        /// <inheritdoc/>
        public override void Before(MethodInfo methodUnderTest) => CurrentTest.Value = $"{methodUnderTest.DeclaringType!.Name}.{methodUnderTest.Name}";

        /// <inheritdoc/>
        public override void After(MethodInfo methodUnderTest) => CurrentTest.Value = "";
    }

    /// <summary>Puts the recorder first in the API's pipeline, so it sees each request before anything else answers it.</summary>
    public sealed class Startup(Recorder recorder, FakeTimeProvider time) : IStartupFilter
    {
        /// <inheritdoc/>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            recorder.Use(app, time);
            next(app);
        };
    }
}
