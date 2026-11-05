using Redix.Api.Projects;

namespace Redix.Api.Accounts;

/// <summary>Where a project's money sits: a bank account or a cash custody (FR-22). It belongs to one project (BR-33), and is disabled rather than deleted (BR-15). It owns the rules of who may spend from it.</summary>
public class Account
{
    /// <summary>The account's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project it belongs to.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>What kind of account it is; set when it is added, and never changed.</summary>
    public AccountType Type { get; init; }

    /// <summary>The account's name, as it was typed.</summary>
    public required string Name { get; set; }

    /// <summary>What it held before its first entry; negative for an overdrawn account.</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>False once disabled: it disappears from entry and stays in the reports (BR-15).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Who holds it: one row at most.</summary>
    public List<AccountCustodian> Custodians { get; init; } = [];

    /// <summary>The user who holds it; <see cref="Guid.Empty"/> when nobody does.</summary>
    public Guid CustodianId => Custodians.Select(custodian => custodian.UserId).FirstOrDefault();

    /// <summary>Whether <paramref name="userId"/> holds it.</summary>
    public bool IsHeldBy(Guid userId) => Custodians.Any(custodian => custodian.UserId == userId);

    /// <summary>Whether <paramref name="userId"/> may spend from it: only while it is active, and only if they hold it unless <paramref name="chargesAny"/> (BRS open question 4). Anyone may say a partner paid (BR-25).</summary>
    public bool IsOfferedTo(Guid userId, bool chargesAny) => IsActive && (chargesAny || IsHeldBy(userId) || Type == AccountType.PartnerPocket);

    /// <summary>Why an account with these values cannot be kept: a name outside its limits, an opening balance beyond its limit or with more than two decimals, or a cash custody with nobody holding it. Empty when it can.</summary>
    public static string RefusalOf(string name, AccountType type, decimal openingBalance, Guid custodianId)
    {
        if (type == AccountType.PartnerPocket)
        {
            return "InvalidType";
        }
        if (!Project.IsAcceptableName(name))
        {
            return "InvalidName";
        }
        if (!Money.IsAcceptable(openingBalance))
        {
            return "InvalidOpeningBalance";
        }
        return type == AccountType.CashCustody && custodianId == Guid.Empty ? "CustodianRequired" : "";
    }

    /// <summary>Makes <paramref name="userId"/> the one who holds it; <see cref="Guid.Empty"/> leaves it with nobody.</summary>
    public void HandTo(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            Custodians.Clear();
        }
        else if (Custodians.Count == 0)
        {
            Custodians.Add(new AccountCustodian { AccountId = Id, UserId = userId });
        }
        else
        {
            Custodians[0].UserId = userId;
        }
    }
}
