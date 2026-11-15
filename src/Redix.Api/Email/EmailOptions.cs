namespace Redix.Api.Email;

/// <summary>How REDIX sends email (redix-api #127, D2, D7), bound from the <c>Email</c> section of configuration.</summary>
public class EmailOptions
{
    /// <summary>Azure Communication Services' connection string, from the secret <c>REDIX_Email__ConnectionString</c>, never a file; empty until it is set.</summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>The address every email is sent from, <c>Email:From</c>: one Azure Communication Services holds for REDIX; empty until it is set.</summary>
    public string From { get; set; } = "";

    /// <summary>Where development keeps each email instead of sending it; by default a folder in the local application data of the user running the API.</summary>
    public string Folder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Redix", "Emails");
}
