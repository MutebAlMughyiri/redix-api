namespace Redix.Api.Entries;

/// <summary>Where an entry stands: what answers an upload sent again (UC-029), and what a phone keeps of its user's entries (UC-030).</summary>
/// <param name="Id">The entry's id.</param>
/// <param name="Status">Where it stands now.</param>
/// <param name="RejectionReason">Why it was rejected, while it stands rejected; empty otherwise.</param>
public record EntryStatusView(Guid Id, EntryStatus Status, string RejectionReason);
