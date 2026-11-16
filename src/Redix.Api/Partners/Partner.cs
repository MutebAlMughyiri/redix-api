using System.Linq.Expressions;
using Redix.Api.Structure;

namespace Redix.Api.Partners;

/// <summary>
/// A partner in one project (FR-25), whose capital is reckoned from their movements (BR-05). A partner is a table of a capital item, as a contract is of a contractor's item
/// (redix-api #109): their movements are entered by picking that table. Someone who withdrew stays, with a zero balance and their history.
/// </summary>
public class Partner : IOrdered, IProjectRow
{
    /// <summary>No partner: what a new one is added over, whose id and item are empty.</summary>
    public static Partner None { get; } = new() { Id = Guid.Empty, Name = "" };

    /// <summary>The partner's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project they are a partner in (BR-33).</summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// The partner's own login, one row at most (findings row 214): whose account this partner is, shown beside their name. It gives no sight of capital: what anyone sees is ticked on
    /// their permissions (redix-api #106, Muteb 11 Nov 2026), so an accountant who sees the figures is never taken for a partner.
    /// </summary>
    public List<PartnerUser> Users { get; init; } = [];

    /// <summary>The partner's own login; <see cref="Guid.Empty"/> for a partner with no login.</summary>
    public Guid UserId => Users.Select(login => login.UserId).FirstOrDefault();

    /// <summary>The partner's name, as it was typed, which names their table.</summary>
    public required string Name { get; set; }

    /// <summary>Their table's place among the item's tables (AC-46.8).</summary>
    public int SortOrder { get; set; }

    /// <summary>True once deleted, softly (redix-api #100): the partner leaves every place a new movement starts, and their movements stay in the books.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Makes <paramref name="userId"/> the partner's own login; <see cref="Guid.Empty"/> leaves them with none.</summary>
    public void LinkTo(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            Users.Clear();
        }
        else if (Users.Count == 0)
        {
            Users.Add(new PartnerUser { PartnerId = Id, ProjectId = ProjectId, UserId = userId });
        }
        else
        {
            Users[0].UserId = userId;
        }
    }

    /// <summary>The partners still in their project: the one owner of leaving a deleted partner out (redix-api #100).</summary>
    public static Expression<Func<Partner, bool>> Kept { get; } = partner => !partner.IsDeleted;
}
