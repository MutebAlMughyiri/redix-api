namespace Redix.Api.Contracts;

/// <summary>A document a contract carries (UC-020 step 3). It stays once attached.</summary>
public class ContractAttachment
{
    /// <summary>The contract.</summary>
    public Guid ContractId { get; init; }

    /// <summary>The document.</summary>
    public Guid AttachmentId { get; init; }
}
