namespace Redix.Api;

/// <summary>
/// One member of an enum the database stores, as a row of the enum's lookup table <c>LK_{enum}</c> (redix-api #87): the table names what every stored value means, and a column holding
/// the enum points at it, so the database refuses a value the code does not know. <see cref="RedixDbContext"/> seeds it from the enum's members, so it cannot drift from the enum.
/// </summary>
/// <typeparam name="TEnum">The enum.</typeparam>
public class Lookup<TEnum> where TEnum : struct, Enum
{
    /// <summary>The member as its number: the key of the table, which a column storing the enum as a number points at.</summary>
    public TEnum Id { get; init; }

    /// <summary>The member as its name, unique: what a column storing the enum by name points at.</summary>
    public TEnum Name { get; init; }
}
