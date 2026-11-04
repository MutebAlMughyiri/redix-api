using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Redix.Api.Auth;

/// <summary>Issues the short-lived access tokens an app sends with each request once signed in, and says how they are validated.</summary>
public class AccessTokens(IOptions<JwtOptions> options, TimeProvider time)
{
    /// <summary>How long an access token is valid; the device's refresh token renews it.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    /// <summary>The issuer and audience of every REDIX token.</summary>
    public const string Issuer = "redix";

    /// <summary>The claim that names the device the session is open on.</summary>
    public const string DeviceClaim = "sid";

    /// <summary>The claim that says the password is temporary and must be changed before anything else.</summary>
    public const string MustChangePasswordClaim = "mustChangePassword";


    private readonly SigningCredentials signingCredentials = new(KeyOf(options.Value), SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// A signed access token for <paramref name="user"/> on the device <paramref name="deviceId"/>, carrying whether the password must be changed first.
    /// Nothing about roles is in it: they are read on each request, so a change counts at once.
    /// </summary>
    public string Create(User user, Guid deviceId)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Issuer,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + Lifetime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Name] = user.Username,
                [DeviceClaim] = deviceId.ToString(),
                [MustChangePasswordClaim] = user.MustChangePassword,
            },
            SigningCredentials = signingCredentials,
        });
    }

    /// <summary>How the API checks an access token: the same key, issuer and audience it was signed with, and the same clock.</summary>
    public static TokenValidationParameters ValidationParameters(JwtOptions options, TimeProvider time) => new()
    {
        ValidIssuer = Issuer,
        ValidAudience = Issuer,
        IssuerSigningKey = KeyOf(options),
        LifetimeValidator = (notBefore, expires, _, _) => notBefore <= time.GetUtcNow().UtcDateTime && time.GetUtcNow().UtcDateTime < expires,
    };

    /// <summary>The user a validated access token belongs to.</summary>
    public static Guid UserIdOf(ClaimsPrincipal principal) => ClaimOf(principal, JwtRegisteredClaimNames.Sub);

    /// <summary>The device a validated access token was issued to.</summary>
    public static Guid DeviceIdOf(ClaimsPrincipal principal) => ClaimOf(principal, DeviceClaim);

    private static Guid ClaimOf(ClaimsPrincipal principal, string claim) =>
        Guid.Parse(principal.FindFirstValue(claim) ?? throw new InvalidOperationException($"The access token has no '{claim}' claim."));

    private static SymmetricSecurityKey KeyOf(JwtOptions options) => new(Convert.FromBase64String(options.SigningKey));
}

/// <summary>The access tokens' settings, bound from the <c>Jwt</c> section; the key comes from the <c>REDIX_Jwt__SigningKey</c> environment variable.</summary>
public class JwtOptions
{
    /// <summary>The base64 key that signs access tokens, at least 256 bits.</summary>
    public string SigningKey { get; set; } = "";
}
