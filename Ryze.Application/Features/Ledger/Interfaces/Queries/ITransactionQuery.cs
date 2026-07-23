using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Queries;

/// <summary>
/// Defines read only operations for retrieving ledger transactions and entries.
/// </summary>
/// <remarks>
/// Implementations are expected to provide consistent historical views
/// of ledger transactions and their associated journal entries.
/// </remarks>
public interface ITransactionQuery
{
    /// <summary>
    /// Retrieves complete ledger transaction by its identifier.
    /// </summary>
    /// <remarks>
    /// The returned transaction includes all associated journal entries
    /// required to reconstruct the transaction details.
    /// </remarks>
    /// <param name="transactionId">Ledger transaction identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LedgerTransaction?> GetTransactionAsync(
        Guid transactionId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all journal entries associated with a transaction.
    /// </summary>
    /// <param name="transactionId">Ledger transaction identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<JournalEntry>> GetTransactionEntriesAsync(
        Guid transactionId,
        CancellationToken ct = default);

    /// <summary>
    /// Finds a journal entry using its idempotency key.
    /// </summary>
    /// <remarks>
    /// Used to detect previously processed operations and prevent
    /// duplicate ledger mutations.
    /// </remarks>
    /// <param name="idempotencyKey">Unique operation idempotency identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<JournalEntry?> FindByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves historical journal entries affecting a ledger account.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<JournalEntry>> GetEntriesAsync(
        string accountId,
        CancellationToken ct = default);
}
