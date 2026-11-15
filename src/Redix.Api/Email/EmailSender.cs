using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Redix.Api.Email;

/// <summary>
/// The one sender of every email REDIX sends, which redix-api #124 reuses (redix-api #127, D2): its subject and body are words of the translations, in the language the phone asked
/// in (R8, AC-02.10). How an email leaves is the kind of sender: Azure Communication Services in qa and production, a folder in development, memory in the tests (D7).
/// </summary>
public abstract class EmailSender
{
    /// <summary>
    /// Sends <paramref name="to"/> the words <paramref name="subjectKey"/> and <paramref name="bodyKey"/>, the body's <c>{0}</c> being <paramref name="code"/>, in the language this request
    /// asked in through <c>Accept-Language</c>, Arabic when it named neither Arabic nor English.
    /// </summary>
    public async Task SendAsync(string to, string subjectKey, string bodyKey, string code, RedixDbContext db)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var words = await db.TranslationKeys.Where(key => key.Key == subjectKey || key.Key == bodyKey)
            .SelectMany(key => key.Translations.Where(translation => translation.Language == language), (key, translation) => new { key.Key, translation.Value })
            .ToDictionaryAsync(word => word.Key, word => word.Value);
        await DeliverAsync(new SentEmail(to, words[subjectKey], string.Format(CultureInfo.InvariantCulture, words[bodyKey], code)));
    }

    /// <summary>Hands <paramref name="email"/> to whatever carries it.</summary>
    protected abstract Task DeliverAsync(SentEmail email);
}
