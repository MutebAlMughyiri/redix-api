using Microsoft.Data.SqlClient;

namespace Redix.Api;

/// <summary>
/// The database connection: its server, port and database from <c>ConnectionStrings:Redix</c> in appsettings.json, and, where the server signs in with a SQL login rather
/// than as Windows, that login from the secrets <c>REDIX_Database__User</c> and <c>REDIX_Database__Password</c>, which never sit in a file.
/// </summary>
public static class DatabaseConnection
{
    /// <summary>The connection string <paramref name="configuration"/> describes.</summary>
    public static string Of(IConfiguration configuration)
    {
        var connection = new SqlConnectionStringBuilder(configuration.GetConnectionString("Redix"));
        var user = configuration["Database:User"] ?? "";
        if (user.Length > 0)
        {
            connection.IntegratedSecurity = false;
            connection.UserID = user;
            connection.Password = configuration["Database:Password"] ?? "";
        }
        return connection.ConnectionString;
    }
}
