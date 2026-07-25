using ModularityKit.Mutator.Abstractions.Effects;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Mutations.SideEffects;

/// <summary>
/// Represents the data contract for ledger transaction recorded side effect.
/// </summary>
/// <remarks>
/// Contains the transaction metadata and journal entries associated with a
/// newly recorded ledger transaction.
/// </remarks>
[SideEffectDataContract("ryze.ledger.transaction-recorded")]
public sealed record LedgerTransactionRecordedSideEffectData
{
    /// <summary>
    /// Gets the ID of the recorded ledger transaction.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the date and time when the transaction was recorded.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the journal entries associated with the recorded transaction.
    /// </summary>
    public required IReadOnlyList<JournalEntry> Entries { get; init; }

    /// <summary>
    /// Gets the type of the recorded ledger transaction.
    /// </summary>
    public required string TransactionType { get; init; }

    /// <summary>
    /// Gets the idempotency key associated with the transaction, if available.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Gets the identifier of the actor that initiated the transaction.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Gets the reason associated with the transaction, if provided.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets the sequential entry number assigned to the transaction, if available.
    /// </summary>
    public required string? EntryNumber { get; init; }
}