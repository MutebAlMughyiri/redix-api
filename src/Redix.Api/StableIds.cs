using System.Security.Cryptography;
using System.Text;

namespace Redix.Api;

/// <summary>Ids for seeded rows, derived from a name so every migration and database agree on them.</summary>
public static class StableIds
{
    /// <summary>The id derived from <paramref name="name"/>.</summary>
    public static Guid Of(string name) => new(SHA256.HashData(Encoding.UTF8.GetBytes(name)).AsSpan(0, 16));
}
