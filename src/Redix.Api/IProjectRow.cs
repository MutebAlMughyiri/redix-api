namespace Redix.Api;

/// <summary>
/// Marks a table whose rows belong to one project, by a <c>ProjectId</c> of its own or through the row it hangs from: <see cref="RedixDbContext"/> keeps every such table in the schema
/// <see cref="Schema"/>, and every other table, REDIX's core, in <c>dbo</c> (redix-api #123, D4, D5). The one place a table says its side.
/// </summary>
public interface IProjectRow
{
    /// <summary>The schema of the tables whose rows belong to one project (redix-api #123, D4).</summary>
    const string Schema = "project";
}
