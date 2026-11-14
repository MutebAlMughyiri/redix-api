using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Redix.Api.Email;
using Redix.Api.Translations;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class EmailSenderTests(MigratedApiFactory factory)
{
    /// <summary>Sends with <paramref name="sender"/> in <paramref name="language"/>, as a request asking in it does.</summary>
    private Task SendInAsync(string language, EmailSender sender, string to)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        return factory.WithDatabaseAsync(db => sender.SendAsync(to, "email.resetSubject", "email.resetBody", "482913", db));
    }

    [Theory]
    [InlineData("ar")]
    [InlineData("en")]
    public async Task R8_AC_02_10_AnEmailsSubjectAndBodyAreTheTranslationsWordsInTheLanguageAskedWithItsCode(string language)
    {
        var to = MigratedApiFactory.NewEmail();

        await SendInAsync(language, factory.Emails, to);

        var sent = factory.Emails.To(to).Single();
        var arabic = language == "ar";
        Assert.Equal(arabic ? TranslationSeed.Arabic("email.resetSubject") : "Your password reset code", sent.Subject);
        Assert.Equal(string.Format(CultureInfo.InvariantCulture, arabic ? TranslationSeed.Arabic("email.resetBody") : "Your REDIX code: {0}. Valid for 15 minutes, once. If you did not ask for it, ignore this email.", "482913"), sent.Body);
    }

    [Fact]
    public async Task R127_D7_DevelopmentKeepsEachEmailAsAFileInItsFolderAndSendsNothing()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"RedixTestsEmails-{Guid.CreateVersion7():N}");
        var to = MigratedApiFactory.NewEmail();

        await SendInAsync("ar", new FolderEmailSender(Options.Create(new EmailOptions { Folder = folder })), to);

        var kept = File.ReadAllText(Directory.GetFiles(folder).Single());
        Assert.StartsWith($"To: {to}\nSubject: {TranslationSeed.Arabic("email.resetSubject")}\n", kept);
        Assert.Contains("482913", kept);
        Assert.Empty(factory.Emails.To(to));
        Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public async Task R5_D2_AnAzureSenderNotSetUpThrowsNothingSoTheAnswerStaysAsItWas()
    {
        var notSetUp = new AzureEmailSender(Options.Create(new EmailOptions()), NullLogger<AzureEmailSender>.Instance);

        var sending = await Record.ExceptionAsync(() => SendInAsync("ar", notSetUp, MigratedApiFactory.NewEmail()));

        Assert.Null(sending);
    }
}
