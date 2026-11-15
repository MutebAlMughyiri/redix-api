using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace Redix.Api.Email;

/// <summary>
/// Sends through Azure Communication Services with its own .NET client (redix-api #127, D2), from <see cref="EmailOptions.From"/> with <see cref="EmailOptions.ConnectionString"/>: qa and
/// production send for real (D7). An email that cannot leave, Azure not set up included, is logged and the answer stays as it was, so it never tells whether an account exists (R5).
/// </summary>
public class AzureEmailSender(IOptions<EmailOptions> options, ILogger<AzureEmailSender> logger) : EmailSender
{
    /// <inheritdoc />
    protected override async Task DeliverAsync(SentEmail email)
    {
        try
        {
            var client = new EmailClient(options.Value.ConnectionString);
            await client.SendAsync(WaitUntil.Started, new EmailMessage(options.Value.From, email.To, new EmailContent(email.Subject) { PlainText = email.Body }));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An email was not sent: Azure Communication Services refused it or is not set up (Email:ConnectionString, Email:From).");
        }
    }
}
