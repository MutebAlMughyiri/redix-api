using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    /// <summary>
    /// Text as it is; a workbook as its sheets' cells, which the Go replay compares cell by cell; any other file as its length and SHA-256, since a Go file is not .NET's byte
    /// for byte.
    /// </summary>
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
        if (contentType.Contains("spreadsheetml"))
        {
            return new { workbook = CellsOf(bytes) };
        }
        return contentType.StartsWith("multipart/")
            ? new { base64 = Convert.ToBase64String(bytes) }
            : new { length = bytes.Length, sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)) };
    }

    /// <summary>Each sheet of the workbook <paramref name="bytes"/>, in order, with every cell that holds something: its formula, or its value as Excel keeps it - a day as its serial number.</summary>
    private static object CellsOf(byte[] bytes)
    {
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        return workbook.Worksheets.Select(sheet => new
        {
            name = sheet.Name,
            cells = sheet.CellsUsed(XLCellsUsedOptions.Contents).Select(cell => new
            {
                at = cell.Address.ToStringRelative(),
                formula = cell.HasFormula ? cell.FormulaA1 : "",
                value = cell.HasFormula ? "" : ValueOf(cell.Value),
            }).ToList(),
        }).ToList();
    }

    private static string ValueOf(XLCellValue value) => value.Type switch
    {
        XLDataType.Number => value.GetNumber().ToString("R", CultureInfo.InvariantCulture),
        XLDataType.DateTime => value.GetDateTime().ToOADate().ToString("R", CultureInfo.InvariantCulture),
        XLDataType.Boolean => value.GetBoolean() ? "1" : "0",
        _ => value.ToString(CultureInfo.InvariantCulture),
    };

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

    /// <summary>Records a save made outside a request and a seed once it is kept: the rows it added, changed or removed; nothing for one the database refused.</summary>
    private sealed class DirectWriteInterceptor(Recorder recorder) : SaveChangesInterceptor
    {
        private readonly ConditionalWeakTable<DbContext, List<DirectRow>> saving = [];

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!Inside.Value && CurrentTest.Value is { Length: > 0 })
            {
                saving.AddOrUpdate(eventData.Context!, [.. eventData.Context!.ChangeTracker.Entries().Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).Select(RowOf)]);
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (saving.TryGetValue(eventData.Context!, out var rows))
            {
                saving.Remove(eventData.Context!);
                recorder.Write(new { step = "direct", rows });
            }
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            saving.Remove(eventData.Context!);
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        /// <summary>An entity about to be saved, as the row the Go replay writes: its table, what happens to it, and each column's value as SQLite keeps it.</summary>
        private static DirectRow RowOf(EntityEntry entry) => new(
            $"{entry.Metadata.GetSchema() ?? "dbo"}.{entry.Metadata.GetTableName()}",
            entry.State.ToString().ToLowerInvariant(),
            entry.Properties.Where(property => property.Metadata.GetColumnName() is { Length: > 0 }).ToDictionary(
                property => property.Metadata.GetColumnName(),
                property => Stored(property.Metadata.GetTypeMapping().Converter is { } converter && property.CurrentValue is not null
                    ? converter.ConvertToProvider(property.CurrentValue)
                    : property.CurrentValue)));
    }

    /// <summary>
    /// Records a bulk update, delete or insert made outside a request and a seed, as ExecuteUpdateAsync makes one without a save: the rows of its table it added, changed or removed,
    /// read before and after it runs; only the verb when its table cannot be read, as a migration's statement.
    /// </summary>
    private sealed class BulkWriteInterceptor(Recorder recorder) : DbCommandInterceptor
    {
        private static readonly Regex TableOf = new(@"(?:FROM|INTO|MERGE)\s+\[(\w+)\]\.\[(\w+)\]");
        private readonly ConditionalWeakTable<DbCommand, Snapshot> before = [];

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var verb = command.CommandText.TrimStart().Split(' ', 2)[0].ToUpperInvariant();
            if (!Inside.Value && CurrentTest.Value is { Length: > 0 } && verb is "UPDATE" or "DELETE" or "INSERT" or "MERGE")
            {
                var named = TableOf.Match(command.CommandText);
                var key = named.Success ? await KeyOfAsync(command, named.Groups[1].Value, named.Groups[2].Value) : [];
                if (key.Count == 0)
                {
                    recorder.Write(new { step = "direct", tables = new[] { verb } });
                }
                else
                {
                    var table = $"{named.Groups[1].Value}.{named.Groups[2].Value}";
                    before.AddOrUpdate(command, new Snapshot(table, key, await RowsOfAsync(command, table, key)));
                }
            }
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (before.TryGetValue(command, out var snapshot))
            {
                before.Remove(command);
                var (table, key, earlier) = snapshot;
                var later = await RowsOfAsync(command, table, key);
                List<DirectRow> rows =
                [
                    .. later.Where(row => !earlier.TryGetValue(row.Key, out var was) || !Same(was, row.Value)).Select(row => new DirectRow(table, earlier.ContainsKey(row.Key) ? "modified" : "added", row.Value)),
                    .. earlier.Where(row => !later.ContainsKey(row.Key)).Select(row => new DirectRow(table, "deleted", row.Value)),
                ];
                recorder.Write(new { step = "direct", rows });
            }
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            before.Remove(command);
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        /// <summary>A table's rows by key, read before a bulk write runs.</summary>
        private sealed record Snapshot(string Table, List<string> Key, Dictionary<string, Dictionary<string, object?>> Rows);

        private static bool Same(Dictionary<string, object?> one, Dictionary<string, object?> other) => one.All(pair => Equals(pair.Value, other.GetValueOrDefault(pair.Key)));

        private static async Task<List<string>> KeyOfAsync(DbCommand command, string schema, string table)
        {
            await using var read = command.Connection!.CreateCommand();
            read.Transaction = command.Transaction;
            read.CommandText = @"SELECT column_usage.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS constraints
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS column_usage ON column_usage.CONSTRAINT_NAME = constraints.CONSTRAINT_NAME AND column_usage.TABLE_SCHEMA = constraints.TABLE_SCHEMA
                WHERE constraints.CONSTRAINT_TYPE = 'PRIMARY KEY' AND constraints.TABLE_SCHEMA = @schema AND constraints.TABLE_NAME = @table ORDER BY column_usage.ORDINAL_POSITION";
            AddParameter(read, "@schema", schema);
            AddParameter(read, "@table", table);
            var key = new List<string>();
            await using var reader = await read.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                key.Add(reader.GetString(0));
            }
            return key;
        }

        private static async Task<Dictionary<string, Dictionary<string, object?>>> RowsOfAsync(DbCommand command, string table, List<string> key)
        {
            await using var read = command.Connection!.CreateCommand();
            read.Transaction = command.Transaction;
            var (schema, name) = (table.Split('.')[0], table.Split('.')[1]);
            read.CommandText = $"SELECT * FROM [{schema}].[{name}]";
            var rows = new Dictionary<string, Dictionary<string, object?>>();
            await using var reader = await read.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (var column = 0; column < reader.FieldCount; column++)
                {
                    var type = reader.GetDataTypeName(column);
                    if (type is not ("timestamp" or "rowversion"))
                    {
                        row[reader.GetName(column)] = Stored(reader.IsDBNull(column) ? null : type == "date" ? DateOnly.FromDateTime(reader.GetDateTime(column)) : reader.GetValue(column));
                    }
                }
                rows[string.Join("|", key.Select(column => Convert.ToString(row[column], CultureInfo.InvariantCulture)))] = row;
            }
            return rows;
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }

    /// <summary>
    /// A value as the SQLite schema keeps it (redix-api #140 G10, N6): a GUID as lower-case text, money as ten-thousandths, a moment as UTC text with seven decimals, a day as
    /// yyyy-MM-dd, a bit as 0 or 1, an enum as its number; anything else as it is.
    /// </summary>
    private static object? Stored(object? value) => value switch
    {
        null or DBNull => null,
        Guid id => id.ToString(),
        decimal amount => (long)(amount * 10000),
        DateTime moment => moment.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "Z",
        DateTimeOffset moment => moment.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "Z",
        DateOnly day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        bool flag => flag ? 1 : 0,
        Enum kind => Convert.ToInt64(kind, CultureInfo.InvariantCulture),
        byte[] bytes => Convert.ToBase64String(bytes),
        _ => value,
    };

    /// <summary>A row a direct write added, changed or removed: its table as schema.Name, what happened to it, and each column's value.</summary>
    private sealed record DirectRow(string Table, string State, Dictionary<string, object?> Values);

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
