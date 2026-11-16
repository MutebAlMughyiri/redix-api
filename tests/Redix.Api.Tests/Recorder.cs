using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;

namespace Redix.Api.Tests;

/// <summary>
/// Writes, when <c>REDIX_RECORD</c> names a folder, every step one collection's tests take against the API, in order, for the Go replays of redix-api #140 (G12): each
/// request as the API saw it, at the fake clock's time, with its answer; each seed a factory helper puts straight into the database; and any other write straight into
/// the database, which marks the collection as one whose rules are ported by hand. With no folder it records nothing, and the tests run as they always did.
/// </summary>
public sealed class Recorder
{
    private static readonly AsyncLocal<bool> Inside = new();
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly string _file;
    private readonly Lock _writing = new();

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

    /// <summary>Records a request and its answer through the API's own pipeline, at <paramref name="time"/>'s moment.</summary>
    public void Use(IApplicationBuilder app, FakeTimeProvider time) => app.Use(async (context, next) =>
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
            step = "request", at, method = context.Request.Method, path = context.Request.Path.Value, query = context.Request.QueryString.Value,
            headers = Headers(context.Request.Headers, "Authorization", "Accept-Language", "If-None-Match"), contentType = context.Request.ContentType ?? "", body = sent,
            status = context.Response.StatusCode, answerType = context.Response.ContentType ?? "", answerHeaders = Headers(context.Response.Headers, "ETag", "Content-Disposition", "WWW-Authenticate", "Allow", "Retry-After", "Location"),
            answer = answered,
        });
    });

    /// <summary>An interceptor that records a database write made outside a request and a seed.</summary>
    public IInterceptor DirectWrites => new DirectWriteInterceptor(this);

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
        lock (_writing)
        {
            File.AppendAllText(_file, JsonSerializer.Serialize(step, Json) + "\n");
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
