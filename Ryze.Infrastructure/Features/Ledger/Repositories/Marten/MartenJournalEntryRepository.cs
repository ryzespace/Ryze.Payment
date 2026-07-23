using Marten;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// Provides Marten backed persistence operations for ledger journal entries.
/// </summary>
/// <remarks>
/// Implements journal entry retrieval using Marten document queries.
/// Supports lookups by entry identifier, account identifier, transaction identifier,
/// and idempotency key.
/// </remarks>
public sealed class MartenJournalEntryRepository(IDocumentSession session) : IJournalEntryRepository
{
    /// <summary>
    /// Returns journal entry by its unique identifier.
    /// </summary>
    /// <param name="entryId">The unique identifier of the journal entry.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<JournalEntry?> GetEntryByIdAsync(Guid entryId, CancellationToken ct = default) =>
        await session.LoadAsync<JournalEntry>(entryId, ct);

    /// <summary>
    /// Returns all journal entries associated with the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Collection containing all matching journal entries.</returns>
    public async Task<IReadOnlyList<JournalEntry>> GetEntriesAsync(string accountId, CancellationToken ct = default)
    {
        return await session.Query<JournalEntry>()
            .Where(x => x.Account.AccountId == accountId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns the journal entry associated with the specified idempotency key.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key used to identify the original operation.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<JournalEntry?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await session.Query<JournalEntry>()
            .Where(x => x.Audit.IdempotencyKey == idempotencyKey)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Returns all journal entries belonging to the specified ledger transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the ledger transaction.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Collection containing all journal entries belonging to the transaction.</returns>
    public async Task<IReadOnlyList<JournalEntry>> GetTransactionEntriesAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await session.Query<JournalEntry>()
            .Where(x => x.TransactionId == transactionId)
            .ToListAsync(ct);
    }
}