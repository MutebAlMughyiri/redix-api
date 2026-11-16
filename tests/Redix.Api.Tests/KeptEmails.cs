using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Redix.Api.Email;

namespace Redix.Api.Tests;

/// <summary>The sender of the tests: keeps every email REDIX sends, in memory, instead of sending it (redix-api #127, D7), and gives it to the recorder.</summary>
public class KeptEmails(Recorder recorder) : EmailSender
{
    private static readonly TimeSpan LongestWait = TimeSpan.FromSeconds(30);

    private readonly ConcurrentQueue<SentEmail> _sent = new();
    private TaskCompletionSource _delivered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Task _released = Task.CompletedTask;

    /// <summary>Every email sent to <paramref name="address"/> so far, oldest first.</summary>
    public List<SentEmail> To(string address) => _sent.Where(email => email.To == address).ToList();

    /// <summary>
    /// Every email sent to <paramref name="address"/> once there are <paramref name="count"/> of them, oldest first: a reset code leaves after the answer (redix-api #127, R5), so a
    /// test waits for it, 30 seconds at most.
    /// </summary>
    public async Task<List<SentEmail>> ToAsync(string address, int count = 1)
    {
        while (true)
        {
            var delivered = _delivered.Task;
            var sent = To(address);
            if (sent.Count >= count)
            {
                return sent;
            }
            await delivered.WaitAsync(LongestWait);
        }
    }

    /// <summary>The six-digit code of the last email sent to <paramref name="address"/>, once <paramref name="count"/> have been sent to it.</summary>
    public async Task<string> LastCodeToAsync(string address, int count = 1) => Regex.Match((await ToAsync(address, count)).Last().Body, @"\d{6}").Value;

    /// <summary>Holds every email until the hold is disposed, so a test sees what the API answered before an email could leave (R5).</summary>
    public IDisposable Hold()
    {
        var hold = new EmailHold();
        _released = hold.Released;
        return hold;
    }

    /// <inheritdoc />
    protected override async Task DeliverAsync(SentEmail email)
    {
        await _released;
        recorder.Sent(email);
        _sent.Enqueue(email);
        Interlocked.Exchange(ref _delivered, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();
    }

    /// <summary>Emails held until it is disposed.</summary>
    private sealed class EmailHold : IDisposable
    {
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Done once the hold ends.</summary>
        public Task Released => _released.Task;

        /// <inheritdoc />
        public void Dispose() => _released.TrySetResult();
    }
}
