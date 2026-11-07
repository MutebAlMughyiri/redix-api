namespace Redix.Api.Structure;

/// <summary>What an item is for (FR-38), which also says its side: an expense or revenue. It is offered only when entering what its type allows (AC-38.1).</summary>
public enum ItemType : byte
{
    /// <summary>Purchases only.</summary>
    Purchases,

    /// <summary>Contractor payments only.</summary>
    ContractorPayments,

    /// <summary>Both purchases and contractor payments.</summary>
    Both,

    /// <summary>Revenue: a unit sold, which carries its sales invoice (FR-42).</summary>
    Sale,

    /// <summary>Revenue: VAT refunded, recorded like any other revenue (redix-api #96, D23).</summary>
    VatRefund,

    /// <summary>Revenue: anything else that comes in.</summary>
    OtherRevenue,
}

/// <summary>What an item's type says beyond itself.</summary>
public static class ItemTypes
{
    /// <summary>True for a revenue type: its entries are money coming in, on the revenue side of the summary (FR-38).</summary>
    public static bool IsRevenue(this ItemType type) => type >= ItemType.Sale;
}

/// <summary>An <see cref="ItemType"/> as a URL names it: camelCase, like every enum that travels. Anything else answers a bad request.</summary>
/// <param name="Value">The type named.</param>
public readonly record struct ItemTypeInUrl(ItemType Value)
{
    /// <summary>Reads <paramref name="text"/> from a URL, as ASP.NET Core asks of a type it binds.</summary>
    public static bool TryParse(string text, out ItemTypeInUrl itemType)
    {
        var known = Enum.TryParse<ItemType>(text, ignoreCase: true, out var value) && Enum.IsDefined(value);
        itemType = new ItemTypeInUrl(value);
        return known;
    }
}
