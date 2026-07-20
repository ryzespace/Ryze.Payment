namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Represents conflict caused by reusing an idempotency key for different ledger operation.
/// </summary>
/// <remarks>
/// This exception is thrown when transaction request contains an idempotency key
/// that has already been used by previously recorded transaction.
///
/// Idempotency protection ensures that repeated requests do not create duplicate
/// financial postings or inconsistent ledger states.
/// </remarks>
public sealed class LedgerIdempotencyException(string key, Guid originalId, string message)
    : LedgerException(message, "LEDGER_IDEMPOTENCY_CONFLICT")
{
    /// <summary>
    /// Gets the idempotency key that caused the conflict.
    /// </summary>
    /// <remarks>
    /// This key identifies the origina operation that has already
    /// been processed.
    /// </remarks>
    public string IdempotencyKey { get; } = key;

    /// <summary>
    /// Gets the identifier of the transaction that already exists for this key.
    /// </summary>
    /// <remarks>
    /// Allows callers to locate the original transaction and safely return
    /// an existing result instead of creating a duplicate ledger entry.
    /// </remarks>
    public Guid OriginalTransactionId { get; } = originalId;
}