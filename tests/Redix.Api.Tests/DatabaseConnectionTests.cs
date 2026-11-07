using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Redix.Api.Tests;

/// <summary>Where the database is, from appsettings.json, and who signs in to it, from the secrets.</summary>
public class DatabaseConnectionTests
{
    private const string Address = "Server=10.0.0.5,1433;Database=Redix;Integrated Security=True;TrustServerCertificate=True";

    private static SqlConnectionStringBuilder ConnectionOf(Dictionary<string, string?> values) =>
        new(DatabaseConnection.Of(new ConfigurationBuilder().AddInMemoryCollection(values).Build()));

    [Fact]
    public void WithNoLoginItSignsInAsWindowsAtTheAddressTheSettingsGive()
    {
        var connection = ConnectionOf(new() { ["ConnectionStrings:Redix"] = Address });

        Assert.Equal(("10.0.0.5,1433", "Redix", true, ""), (connection.DataSource, connection.InitialCatalog, connection.IntegratedSecurity, connection.UserID));
    }

    [Fact]
    public void ALoginFromTheSecretsReplacesWindowsSignIn()
    {
        var connection = ConnectionOf(new() { ["ConnectionStrings:Redix"] = Address, ["Database:User"] = "redix", ["Database:Password"] = "Secret-1" });

        Assert.Equal(("10.0.0.5,1433", false, "redix", "Secret-1"), (connection.DataSource, connection.IntegratedSecurity, connection.UserID, connection.Password));
    }
}
