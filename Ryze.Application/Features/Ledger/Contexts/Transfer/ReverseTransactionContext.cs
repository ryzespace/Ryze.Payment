using ModularityKit.Context.Abstractions;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Contexts.Transfer;

/// <summary>
/// Write context for ledger transaction reversal operations.
/// </summary>
/// <remarks>
/// Represents immutable command intent describing the reversal of an
/// existing double-entry ledger transaction.
///
/// The context identifies the original transaction to reverse together with
/// audit information and an idempotency key, ensuring the reversal can be
/// executed safely without creating duplicate compensating entries.
/// </remarks>
public sealed class ReverseTransactionContext : IContext
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Identifier of the actor initiating the transaction reversal.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    ///  Reason explaining why the transaction is being reversed.
    /// </summary>
    public string? Reason { get; init; }
    
    /// <summary>
    /// Arbitrary metadata associated with the reversal request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = [];

    /// <summary>
    /// Original ledger transaction that will be reversed.
    /// </summary>
    /// <remarks>
    /// The reversal operation creates compensating entries that negate the
    /// financial effects of this transaction while preserving the audit trail.
    /// </remarks>
    public required LedgerTransaction OriginalTransaction { get; init; }

    /// <summary>
    /// Idempotency key uniquely identifying the reversal operation.
    /// </summary>
    /// <remarks>
    /// Prevents duplicate reversal requests from creating multiple
    /// compensating transactions.
    /// </remarks>
    public required string ReversalIdempotencyKey { get; init; }
}
