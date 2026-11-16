namespace Redix.Api.Structure;

/// <summary>
/// The keys of the columns REDIX itself seeds (FR-41, redix-api #129), which its code reads by key: what an entry keeps in its own fields, the amount, and the lists whose choices
/// carry a sign. The one owner of these words; a column a project adds has a key of its own.
/// </summary>
public static class ColumnKeys
{
    /// <summary>«الوصف», kept in the entry's own description.</summary>
    public const string Description = "description";

    /// <summary>«التاريخ», the entry's date.</summary>
    public const string Date = "date";

    /// <summary>«المورد», a purchase's supplier.</summary>
    public const string Supplier = "supplier";

    /// <summary>«الاتفاق», a contractor payment's contract.</summary>
    public const string Contract = "contract";

    /// <summary>A purchase's amount, «التكلفة».</summary>
    public const string Total = "total";

    /// <summary>Any other entry's amount.</summary>
    public const string Amount = "amount";

    /// <summary>«الحساب», the account it was paid from or into.</summary>
    public const string Account = "account";

    /// <summary>«طريقة الدفع».</summary>
    public const string PaymentMethod = "paymentMethod";

    /// <summary>«المرفق», the entry's own file.</summary>
    public const string Attachment = "attachment";

    /// <summary>Whether a contractor payment is inside or outside its contract (BR-04).</summary>
    public const string OutsideContract = "outsideContract";

    /// <summary>«سعر الحبة».</summary>
    public const string UnitPrice = "unitPrice";

    /// <summary>«العدد».</summary>
    public const string Quantity = "quantity";

    /// <summary>«نوع القيد», a purchase's list whose choice says plus or minus.</summary>
    public const string EntryType = "entryType";

    /// <summary>«نوع الحركة», a capital movement's list whose choice says in or out.</summary>
    public const string MovementType = "movementType";
}
