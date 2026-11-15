using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Redix.Api.Email;

namespace Redix.Api.Tests;

/// <summary>The sender of the tests: keeps every email REDIX sends, in memory, instead of sending it (redix-api #127, D7).</summary>
public class KeptEmails : EmailSender
{
    private readonly ConcurrentQueue<SentEmail> _sent = new();

    /// <summary>Every email sent to <paramref name="address"/>, oldest first.</summary>
    public List<SentEmail> To(string address) => _sent.Where(email => email.To == address).ToList();

    /// <summary>The six-digit code of the last email sent to <paramref name="address"/>.</summary>
    public string LastCodeTo(string address) => Regex.Match(To(address).Last().Body, @"\d{6}").Value;

    /// <inheritdoc />
    protected override Task DeliverAsync(SentEmail email)
    {
        _sent.Enqueue(email);
        return Task.CompletedTask;
    }
}
