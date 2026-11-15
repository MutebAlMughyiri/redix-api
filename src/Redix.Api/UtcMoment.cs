using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Redix.Api;

/// <summary>
/// Reads every moment the database keeps as UTC, which is what REDIX writes (<see cref="TimeProvider.GetUtcNow"/>). SQL Server's datetime2 keeps no zone, so without it a moment
/// read back is of no kind, its JSON names no zone, and a phone shows UTC as its own time: a history line three hours early in Riyadh, a file's date a day off near midnight.
/// </summary>
public sealed class UtcMoment() : ValueConverter<DateTime, DateTime>(moment => moment, kept => DateTime.SpecifyKind(kept, DateTimeKind.Utc));
