using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Queries;

/// <summary>
/// Provides ledger transaction and journal entry query operations.
/// </summary>
/// <remarks>
/// Retrieves complete ledger transactions reconstructed from their journal entries,
/// as well as individual journal entries associated with transactions or ledger accounts.
/// Supports idempotency key lookups for identifying previously recorded operations.
/// </remarks>
public sealed class TransactionQuery(
    IJournalEntryRepository entryRepo) : ITransactionQuery
{
    /// <summary>
    /// Retrieves ledger transaction by its identifier.
    /// </summary>
    /// <remarks>
    /// Reconstructs the transaction from its associated journal entries and uses
    /// the first entry to populate transaction-level metadata.
    /// </remarks>
    /// <param name="transactionId">The unique identifier of the ledger transaction.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<LedgerTransaction?> GetTransactionAsync(Guid transactionId, CancellationToken ct = default)
    {
        var entries = await entryRepo.GetTransactionEntriesAsync(transactionId, ct);
        if (entries.Count == 0)
            return null;

        var first = entries[0];
        return new LedgerTransaction
        {
            Id = transactionId,
            Timestamp = first.Timestamp,
            Entries = [.. entries],
            Type = first.Audit.TransactionType,
            IdempotencyKey = first.Audit.IdempotencyKey ?? transactionId.ToString("N"),
            InitiatedBy = first.Audit.InitiatedBy,
            Reason = first.Audit.Reason
        };
    }

    /// <summary>
    /// Retrieves all journal entries associated with ledger transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the ledger transaction.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<IReadOnlyList<JournalEntry>> GetTransactionEntriesAsync(Guid transactionId, CancellationToken ct = default) =>
        await entryRepo.GetTransactionEntriesAsync(transactionId, ct);

    /// <summary>
    /// Finds a journal entry associated with the specified idempotency key.
    /// </summary>
    /// <remarks>
    /// Idempotency key lookups can be used to determine whether a previously
    /// submitted ledger operation has already been recorded.
    /// </remarks>
    /// <param name="idempotencyKey">The idempotency key associated with the ledger operation.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<JournalEntry?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default) =>
        await entryRepo.GetByIdempotencyKeyAsync(idempotencyKey, ct);

    /// <summary>
    /// Retrieves all journal entries associated with ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<IReadOnlyList<JournalEntry>> GetEntriesAsync(string accountId, CancellationToken ct = default) =>
        await entryRepo.GetEntriesAsync(accountId, ct);
}