using System.Linq.Expressions;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Suppliers;

/// <summary>A supplier in one project's register (FR-06, BR-33), which a purchase has to name (BR-19). It is disabled rather than deleted (BR-15), and it owns the rule of a VAT number (BR-16).</summary>
public class Supplier
{
    /// <summary>The name of the supplier every approved project starts with, for expenses with no supplier (UC-017).</summary>
    public const string GeneralCashSupplierName = "مورد نقدي عام";

    /// <summary>How many digits a VAT number has (BR-16).</summary>
    public const int VatNumberLength = 15;

    /// <summary>How many letters a search needs before it narrows anything down (AC-43.1).</summary>
    public const int SearchMinLength = 3;

    /// <summary>The most suppliers one search answers.</summary>
    public const int SearchLimit = 20;

    /// <summary>The supplier's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project whose register holds it.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The supplier's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>Its VAT number; empty for a supplier not VAT-registered, whose invoices stay out of the VAT statement (AC-06.2).</summary>
    public string VatNumber { get; set; } = "";

    /// <summary>Its mobile; empty when none.</summary>
    public string Mobile { get; set; } = "";

    /// <summary>Where it stands.</summary>
    public SupplierStatus Status { get; init; } = SupplierStatus.Approved;

    /// <summary>False once disabled: it disappears from entry and stays in the reports (BR-15).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The user who added it.</summary>
    public Guid CreatedByUserId { get; init; }

    /// <summary>The item its expenses usually go to: one row at most.</summary>
    public List<SupplierDefaultItem> DefaultItems { get; init; } = [];

    /// <summary>That item's id; <see cref="Guid.Empty"/> when it has none.</summary>
    public Guid DefaultItemId => DefaultItems.Select(row => row.ItemId).FirstOrDefault();

    /// <summary>The supplier <paramref name="projectId"/> starts with, for expenses with no supplier: approved at once, and not VAT-registered, so nothing on it reaches the VAT statement.</summary>
    public static Supplier GeneralCashSupplier(Guid projectId, Guid createdByUserId) => new() { ProjectId = projectId, Name = GeneralCashSupplierName, CreatedByUserId = createdByUserId };

    /// <summary>What <paramref name="userId"/> may pick: active suppliers that are approved or that they added themselves (UC-019 step 3), and any pending one if they <paramref name="seePending"/>.</summary>
    public static Expression<Func<Supplier, bool>> OfferedTo(Guid userId, bool seePending) =>
        supplier => supplier.IsActive && (supplier.Status == SupplierStatus.Approved || supplier.CreatedByUserId == userId || seePending);

    /// <summary>Whether <paramref name="vatNumber"/> is 15 digits, starting and ending with 3 (BR-16).</summary>
    public static bool IsValidVatNumber(string vatNumber) => vatNumber.Length == VatNumberLength && vatNumber.All(char.IsAsciiDigit) && vatNumber[0] == '3' && vatNumber[^1] == '3';

    /// <summary>Why a supplier with these values, already trimmed, cannot be kept: a name outside its limits, a VAT number breaking BR-16 unless <paramref name="isVatRegistered"/> is false, or a mobile <see cref="User.IsAcceptableMobile"/> refuses. Empty when it can.</summary>
    public static string RefusalOf(string name, string vatNumber, bool isVatRegistered, string mobile)
    {
        if (!Project.IsAcceptableName(name))
        {
            return "InvalidName";
        }
        if (isVatRegistered && !IsValidVatNumber(vatNumber))
        {
            return "InvalidVatNumber";
        }
        return User.IsAcceptableMobile(mobile) ? "" : "InvalidMobile";
    }

    /// <summary>Makes <paramref name="itemId"/> the item its expenses usually go to; <see cref="Guid.Empty"/> leaves it with none.</summary>
    public void DefaultTo(Guid itemId)
    {
        if (itemId == Guid.Empty)
        {
            DefaultItems.Clear();
        }
        else if (DefaultItems.Count == 0)
        {
            DefaultItems.Add(new SupplierDefaultItem { SupplierId = Id, ItemId = itemId });
        }
        else
        {
            DefaultItems[0].ItemId = itemId;
        }
    }
}
