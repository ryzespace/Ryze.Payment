using ModularityKit.Mutator.Abstractions.Effects;

namespace Ryze.Application.Features.Ledger.Mutations.SideEffects;

/// <summary>
/// Represents the data contract for ledger transaction cleared side effect.
/// </summary>
/// <remarks>
/// Contains the transaction and entry identifiers associated with the cleared
/// ledger entries, together with the timestamp at which the clearing occurred.
/// </remarks>
[SideEffectDataContract("ryze.ledger.transaction-cleared")]
public sealed record LedgerTransactionClearedSideEffectData
{
    /// <summary>
    /// Gets the ID of the cleared ledger transaction.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the IDs of the ledger entries that were cleared.
    /// </summary>
    public required IReadOnlyList<Guid> EntryIds { get; init; }

    /// <summary>
    /// Gets the date and time when the ledger entries were cleared.
    /// </summary>
    public required DateTimeOffset ClearedAt { get; init; }
}