using Microsoft.Extensions.Options;

namespace Redix.Api.Email;

/// <summary>Keeps each email as a text file in <see cref="EmailOptions.Folder"/> instead of sending it, in development (redix-api #127, D7), so a code is read without Azure.</summary>
public class FolderEmailSender(IOptions<EmailOptions> options) : EmailSender
{
    /// <inheritdoc />
    protected override async Task DeliverAsync(SentEmail email)
    {
        Directory.CreateDirectory(options.Value.Folder);
        await File.WriteAllTextAsync(Path.Combine(options.Value.Folder, $"{Guid.CreateVersion7():N}.txt"), $"To: {email.To}\nSubject: {email.Subject}\n\n{email.Body}\n");
    }
}
