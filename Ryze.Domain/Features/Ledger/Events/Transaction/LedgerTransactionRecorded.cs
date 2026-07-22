using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Domain.Features.Ledger.Events.Transaction;

/// <summary>
/// Represents domain event raised when a valid and balanced double-entry
/// ledger transaction has been successfully recorded.
/// </summary>
/// <param name="TransactionId">The unique identifier of the recorded ledger transaction. </param>
/// <param name="Timestamp">The UTC timestamp when the transaction was recorded. </param>
/// <param name="Entries">The journal entries that compose the double-entry transaction. </param>
/// <param name="TransactionType">The classification of the recorded transaction.</param>
/// <param name="IdempotencyKey">The unique idempotency key used to prevent duplicate transaction processing. </param>
/// <param name="InitiatedBy">The identity of the actor, service, or system that initiated the transaction.</param>
/// <param name="Reason">An optional business reason or explanation associated with the transaction. </param>
/// <param name="EntryNumber">An optional sequential accounting entry number assigned to the transaction. </param>
public sealed record LedgerTransactionRecorded(
    Guid TransactionId,
    DateTimeOffset Timestamp,
    IReadOnlyList<JournalEntry> Entries,
    string TransactionType,
    string IdempotencyKey,
    string InitiatedBy,
    string? Reason,
    string? EntryNumber = null
);