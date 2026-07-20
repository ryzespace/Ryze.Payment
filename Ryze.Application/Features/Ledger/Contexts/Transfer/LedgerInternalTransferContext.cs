using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Ledger.Contexts.Transfer;

/// <summary>
/// Write context for internal ledger transfer operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing a transfer of funds
/// between two ledger accounts managed within the same ledger.
///
/// The context specifies the source and destination accounts, transfer
/// amount, currency, and audit metadata required to execute the operation
/// through the application layer.
/// </remarks>
public sealed class LedgerInternalTransferContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the actor initiating the transfer.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    ///  Reason explaining why the transfer is performed.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the transfer request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Identifier of the ledger account from which funds are transferred.
    /// </summary>
    public required string FromAccountId { get; init; }

    /// <summary>
    /// Identifier of the ledger account receiving the transferred funds.
    /// </summary>
    public required string ToAccountId { get; init; }

    /// <summary>
    /// Amount to transfer between the accounts.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency in which the transfer is performed.
    /// </summary>
    public required string Currency { get; init; }
    
    public string Description { get; init; } = "Internal Transfer";
}