namespace Redix.Api.Email;

/// <summary>An email as REDIX sends it: the address it goes to, and its subject and body already in the reader's language.</summary>
/// <param name="To">The address it goes to.</param>
/// <param name="Subject">Its subject.</param>
/// <param name="Body">Its body, as plain text.</param>
public record SentEmail(string To, string Subject, string Body);
