using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Redix.Api.Auth;

/// <summary>A phone signed in as a user. It keeps only the hash of its refresh token, so the database alone opens no session.</summary>
public class Device
{
    /// <summary>A refresh token unused this long expires, and the user signs in again (NFR-09).</summary>
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromHours(12);

    /// <summary>The device's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The user signed in on this device.</summary>
    public Guid UserId { get; init; }

    /// <summary>The SHA-256 of the device's current refresh token, in hex.</summary>
    public string RefreshTokenHash { get; private set; } = "";

    /// <summary>When the refresh token expires unless it is used first.</summary>
    public DateTime RefreshExpiresAt { get; private set; }

    /// <summary>True once the device may no longer refresh its session.</summary>
    public bool IsRevoked { get; set; }

    /// <summary>True when this phone asks for a fingerprint or face on opening, so its session never times out (UC-011).</summary>
    public bool UsesBiometrics { get; set; }

    /// <summary>When the device last opened or renewed its session.</summary>
    public DateTime LastSeenAt { get; private set; }

    /// <summary>Gives the device a new refresh token, valid for <see cref="IdleTimeout"/> from <paramref name="now"/>, and returns it; only its hash is kept.</summary>
    public string RenewRefreshToken(DateTime now)
    {
        var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        RefreshTokenHash = HashOf(refreshToken);
        RefreshExpiresAt = now + IdleTimeout;
        LastSeenAt = now;
        return refreshToken;
    }

    /// <summary>Whether the device's refresh token still renews a session at <paramref name="now"/>. A phone that uses biometrics never times out; it can only be revoked.</summary>
    public bool CanRefresh(DateTime now) => !IsRevoked && (UsesBiometrics || RefreshExpiresAt > now);

    /// <summary>The stored form of a refresh token: its SHA-256, in hex.</summary>
    public static string HashOf(string refreshToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
