using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Redix.Api.Auth;

/// <summary>Issues the short-lived access tokens an app sends with each request once signed in.</summary>
public class AccessTokens(IOptions<JwtOptions> options, TimeProvider time)
{
    /// <summary>How long an access token is valid; the device's refresh token renews it.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    /// <summary>The issuer and audience of every REDIX token.</summary>
    public const string Issuer = "redix";

    private readonly SigningCredentials signingCredentials = new(new SymmetricSecurityKey(Convert.FromBase64String(options.Value.SigningKey)), SecurityAlgorithms.HmacSha256);

    /// <summary>A signed access token for <paramref name="user"/>, carrying whether the password must be changed before anything else.</summary>
    public string Create(User user)
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
                ["mustChangePassword"] = user.MustChangePassword,
            },
            SigningCredentials = signingCredentials,
        });
    }
}

/// <summary>The access tokens' settings, bound from the <c>Jwt</c> section; the key comes from the <c>REDIX_Jwt__SigningKey</c> environment variable.</summary>
public class JwtOptions
{
    /// <summary>The base64 key that signs access tokens, at least 256 bits.</summary>
    public string SigningKey { get; set; } = "";
}
