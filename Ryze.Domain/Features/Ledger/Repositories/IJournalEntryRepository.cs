using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Domain.Features.Ledger.Repositories;

/// <summary>
/// Defines read only persistence operations for ledger journal entries.
/// </summary>
/// <remarks>
/// Provides access to individual journal entries, account-specific entry
/// history, transaction entries, and entries associated with idempotency keys.
/// </remarks>
public interface IJournalEntryRepository
{
    /// <summary>
    /// Retrieves a journal entry by its unique identifier.
    /// </summary>
    /// <param name="entryId">The unique identifier of the journal entry to retrieve. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<JournalEntry?> GetEntryByIdAsync(
        Guid entryId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all journal entries associated with a ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are requested. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<IReadOnlyList<JournalEntry>> GetEntriesAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a journal entry associated with the specified idempotency key.
    /// </summary>
    /// <remarks>
    /// This operation can be used to detect previously processed requests
    /// and prevent duplicate ledger operations.
    /// </remarks>
    /// <param name="idempotencyKey">The idempotency key used to identify the original ledger operation. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<JournalEntry?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all journal entries belonging to ledger transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the ledger transaction whose entries are requested.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation. </param>
    Task<IReadOnlyList<JournalEntry>> GetTransactionEntriesAsync(
        Guid transactionId,
        CancellationToken ct = default);
}