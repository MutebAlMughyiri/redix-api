namespace Redix.Api.Structure;

/// <summary>
/// One choice of a list column, kept in the column's definition (redix-api #129): its id, which is what an entry keeps, its name, and the sign it gives the amount, if any. A sign
/// decides the direction of the amount of an entry that chooses it, as «مرتجع» takes away and «سحب مبلغ» takes out.
/// </summary>
public class ColumnChoice
{
    /// <summary>No choice: what a value naming none of a list's choices finds, so no reader holds a null. Its empty id matches nothing, and it gives no sign.</summary>
    public static ColumnChoice None { get; } = new() { Id = Guid.Empty, Name = "" };

    /// <summary>The choice's id: what an entry's value holds once it is chosen.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The choice's name, as it was typed.</summary>
    public required string Name { get; init; }

    /// <summary>The sign it gives the amount; <see cref="ChoiceSign.None"/> for a choice that says nothing of it.</summary>
    public ChoiceSign Sign { get; init; }

    /// <summary>The same choice, with the same id, for another column: a copy of a template's column keeps the template's choices.</summary>
    public ColumnChoice Copy() => new() { Id = Id, Name = Name, Sign = Sign };
}

/// <summary>The sign a list's choice gives the amount of the entry that chooses it (redix-api #129).</summary>
public enum ChoiceSign : byte
{
    /// <summary>It says nothing of the amount.</summary>
    None,

    /// <summary>The amount counts as it is: «شراء», «إضافة مبلغ».</summary>
    Plus,

    /// <summary>The amount counts the other way: «مرتجع» takes away from the item, «سحب مبلغ» takes out of the capital.</summary>
    Minus,
}

/// <summary>A choice as the app sends it, for a new list or one gaining choices.</summary>
/// <param name="Name">Its name.</param>
/// <param name="Sign">The sign it gives the amount; left out for none.</param>
public record ChoiceRequest(string Name, ChoiceSign Sign = ChoiceSign.None);
