using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Repositories;

/// <summary>
/// Defines read only query operations for ledger journal entries.
/// </summary>
/// <remarks>
/// Provides flexible filtering, sorting, and pagination capabilities for
/// retrieving journal entries associated with ledger accounts.
///
/// Supports both offset based pagination for conventional page navigation
/// and cursor based pagination for efficient traversal of large datasets.
/// </remarks>
public interface ILedgerEntryQuery
{
    /// <summary>
    /// Queries journal entries for an account using offset based pagination.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="dateFrom">Optional inclusive lower bound for the journal entry timestamp.</param>
    /// <param name="dateTo">Optional inclusive upper bound for the journal entry timestamp.</param>
    /// <param name="amountMin">Optional minimum journal entry amount.</param>
    /// <param name="amountMax">Optional maximum journal entry amount.</param>
    /// <param name="status">Optional entry status used to filter the results.</param>
    /// <param name="direction">Optional entry direction used to filter debit or credit postings.</param>
    /// <param name="sortBy">The property used to sort the results. Defaults to <c>Timestamp</c>.</param>
    /// <param name="sortDirection">The sort direction, either ascending or descending. Defaults to <c>Desc</c>.</param>
    /// <param name="skip">The number of matching entries to skip before returning results.</param>
    /// <param name="take">The maximum number of entries to return.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the requested journal entries and the total number
    /// of entries matching the supplied filters.
    /// </returns>
    Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> QueryEntriesAsync(
        string accountId,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        decimal? amountMin = null,
        decimal? amountMax = null,
        EntryStatus? status = null,
        string? direction = null,
        string sortBy = "Timestamp",
        string sortDirection = "Desc",
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Queries journal entries for an account using cursor based pagination.
    /// </summary>
    /// <remarks>
    /// Cursor based pagination is suitable for traversing large or frequently
    /// changing datasets while avoiding the performance characteristics of
    /// large offset values.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="dateFrom">Optional inclusive lower bound for the journal entry timestamp.</param>
    /// <param name="dateTo">Optional inclusive upper bound for the journal entry timestamp.</param>
    /// <param name="amountMin">Optional minimum journal entry amount.</param>
    /// <param name="amountMax">Optional maximum journal entry amount.</param>
    /// <param name="status">Optional entry status used to filter the results.</param>
    /// <param name="direction">Optional entry direction used to filter debit or credit postings.</param>
    /// <param name="sortBy">The property used to sort the results. Defaults to <c>Timestamp</c>.</param>
    /// <param name="sortDirection">The sort direction, either ascending or descending. Defaults to <c>Desc</c>.</param>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <param name="continuationToken">An opaque cursor identifying the position from which the next page of results should be retrieved.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the requested journal entries and a value indicating
    /// whether additional entries are available.
    /// </returns>
    Task<(IReadOnlyList<JournalEntry> Items, bool HasMore)> QueryEntriesWithCursorAsync(
        string accountId,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        decimal? amountMin = null,
        decimal? amountMax = null,
        EntryStatus? status = null,
        string? direction = null,
        string sortBy = "Timestamp",
        string sortDirection = "Desc",
        int limit = 50,
        string? continuationToken = null,
        CancellationToken ct = default);

    /// <summary>
    /// Streams journal entries for an account using keyset pagination.
    /// </summary>
    /// <remarks>
    /// Optimized for large scale audit and chain verification.
    /// Uses Timestamp ASC, Id ASC ordering for consistent reconstruction.
    /// </remarks>
    IAsyncEnumerable<JournalEntry> StreamEntriesAsync(
        string accountId,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        EntryStatus? status = null,
        CancellationToken ct = default);
}