namespace Ryze.Domain.Features.Ledger.Events.Transaction;

/// <summary>
/// Represents domain event raised when previously pending ledger transaction
/// has been fully settled and cleared.
/// </summary>
/// <param name="TransactionId">The unique identifier of the ledger transaction that was cleared. </param>
/// <param name="EntryIds">The unique identifiers of the journal entries that were cleared as part of this transaction. </param>
/// <param name="Timestamp">The UTC timestamp when the transaction was fully settled and cleared. </param>
public sealed record LedgerTransactionCleared(
    Guid TransactionId,
    IReadOnlyList<Guid> EntryIds,
    DateTimeOffset Timestamp
);
