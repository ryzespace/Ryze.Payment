using System.Runtime.CompilerServices;
using Marten;
using Marten.Linq;
using Ryze.Application.Features.Ledger.Helpers;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// query operations for ledger journal entries.
/// </summary>
/// <remarks>
/// Supports filtering by account, date range, amount, status, and posting direction.
/// Provides both offset based pagination for conventional page navigation and
/// cursor based pagination for efficient traversal of large or frequently changing
/// journal entry datasets.
/// </remarks>
public sealed class MartenLedgerEntryQuery(IDocumentSession session) : ILedgerEntryQuery
{
    /// <summary>
    /// Queries journal entries for an account using cursor based pagination.
    /// </summary>
    /// <remarks>
    /// Uses an opaque continuation token to identify the position of the last
    /// returned entry. Fetches one additional record to determine whether another
    /// page is available without performing separate count query.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="dateFrom">Optional inclusive lower bound for the journal entry timestamp.</param>
    /// <param name="dateTo">Optional inclusive upper bound for the journal entry timestamp.</param>
    /// <param name="amountMin">Optional minimum journal entry amount.</param>
    /// <param name="amountMax">Optional maximum journal entry amount.</param>
    /// <param name="status">Optional entry status used to filter the results.</param>
    /// <param name="direction">Optional entry direction used to filter debit or credit postings.</param>
    /// <param name="sortBy">The property used to sort the results. Defaults to <c>Timestamp</c>.</param>
    /// <param name="sortDirection">The sort direction, either ascending or descending.</param>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <param name="continuationToken">An opaque cursor identifying the position from which the next page should be retrieved.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A tuple containing the requested journal entries and a value indicating whether more entries are available.</returns>
    public async Task<(IReadOnlyList<JournalEntry> Items, bool HasMore)> QueryEntriesWithCursorAsync(
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
        CancellationToken ct = default)
    {
        IQueryable<JournalEntry> query = BuildFilteredQuery(
            accountId, dateFrom, dateTo, amountMin, amountMax, status, direction);

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            var cursor = PageToken.Decode(continuationToken);

            if (cursor is not null)
            {
                var isDesc = cursor.SortDirection.Equals("Desc", StringComparison.OrdinalIgnoreCase);

                if (cursor.SortBy == "Amount" && decimal.TryParse(cursor.LastValue, out var lastAmount))
                {
                    query = isDesc
                        ? query.Where(x => x.Posting.Amount < lastAmount ||
                                           (x.Posting.Amount == lastAmount &&
                                            x.Id.CompareTo(cursor.LastId) < 0))
                        : query.Where(x => x.Posting.Amount > lastAmount ||
                                           (x.Posting.Amount == lastAmount &&
                                            x.Id.CompareTo(cursor.LastId) > 0));
                }
                else if (DateTimeOffset.TryParse(cursor.LastValue, out var lastTimestamp))
                {
                    query = isDesc
                        ? query.Where(x => x.Timestamp < lastTimestamp ||
                                           (x.Timestamp == lastTimestamp &&
                                            x.Id.CompareTo(cursor.LastId) < 0))
                        : query.Where(x => x.Timestamp > lastTimestamp ||
                                           (x.Timestamp == lastTimestamp &&
                                            x.Id.CompareTo(cursor.LastId) > 0));
                }
            }
        }

        query = ApplySort(query, sortBy, sortDirection, tieBreakOnId: true);

        var fetched = await query
            .Take(limit + 1)
            .ToListAsync(ct);

        var hasMore = fetched.Count > limit;
        var items = hasMore
            ? fetched.Take(limit).ToList()
            : fetched;

        return (items, hasMore);
    }

    /// <summary>
    /// Queries journal entries for an account using offset-based pagination.
    /// </summary>
    /// <remarks>
    /// Uses Marten query statistics to retrieve the total number of matching
    /// entries together with the requested page, avoiding a separate count query.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="dateFrom">Optional inclusive lower bound for the journal entry timestamp.</param>
    /// <param name="dateTo">Optional inclusive upper bound for the journal entry timestamp.</param>
    /// <param name="amountMin">Optional minimum journal entry amount.</param>
    /// <param name="amountMax">Optional maximum journal entry amount.</param>
    /// <param name="status">Optional entry status used to filter the results.</param>
    /// <param name="direction">Optional entry direction used to filter debit or credit postings.</param>
    /// <param name="sortBy">The property used to sort the results. Defaults to <c>Timestamp</c>.</param>
    /// <param name="sortDirection">The sort direction, either ascending or descending.</param>
    /// <param name="skip">The number of matching entries to skip before returning results.</param>
    /// <param name="take">The maximum number of entries to return.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A tuple containing the requested journal entries and the total number of matching entries.</returns>
    public async Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> QueryEntriesAsync(
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
        CancellationToken ct = default)
    {
        IQueryable<JournalEntry> query = BuildFilteredQuery(
            accountId, dateFrom, dateTo, amountMin, amountMax, status, direction);

        query = ApplySort(query, sortBy, sortDirection, tieBreakOnId: false);

        var martenQuery = (IMartenQueryable<JournalEntry>)query;

        var items = await martenQuery
            .Stats(out var stats)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, (int)stats.TotalResults);
    }

    /// <summary>
    /// Builds filtered journal entry query for the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose entries are queried.</param>
    /// <param name="dateFrom">Optional inclusive lower bound for the journal entry timestamp.</param>
    /// <param name="dateTo">Optional inclusive upper bound for the journal entry timestamp.</param>
    /// <param name="amountMin">Optional minimum journal entry amount.</param>
    /// <param name="amountMax">Optional maximum journal entry amount.</param>
    /// <param name="status">Optional entry status used to filter the results.</param>
    /// <param name="direction">Optional entry direction used to filter debit or credit postings.</param>
    /// <returns>An <see cref="IQueryable{T}"/> containing the supplied journal entry filters.</returns>
    private IQueryable<JournalEntry> BuildFilteredQuery(
        string accountId,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        decimal? amountMin,
        decimal? amountMax,
        EntryStatus? status,
        string? direction)
    {
        var query = session.Query<JournalEntry>()
            .Where(x => x.Account.AccountId == accountId);

        if (dateFrom.HasValue)
            query = query.Where(x => x.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.Timestamp <= dateTo.Value);

        if (amountMin.HasValue)
            query = query.Where(x => x.Posting.Amount >= amountMin.Value);

        if (amountMax.HasValue)
            query = query.Where(x => x.Posting.Amount <= amountMax.Value);

        if (status.HasValue)
            query = query.Where(x => x.Posting.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(direction))
        {
            var entryType = direction.Equals("Debit", StringComparison.OrdinalIgnoreCase)
                ? EntryType.Debit
                : EntryType.Credit;

            query = query.Where(x => x.Posting.Type == entryType);
        }

        return query;
    }

    /// <summary>
    /// Applies sorting to journal entry query.
    /// </summary>
    /// <remarks>
    /// Cursor based pagination uses the journal entry identifier as deterministic
    /// tie breaker to ensure stable ordering when multiple entries have the same
    /// primary sort value.
    /// </remarks>
    /// <param name="query">The journal entry query to sort.</param>
    /// <param name="sortBy">The property used to sort the results.</param>
    /// <param name="sortDirection">The sort direction, either ascending or descending.</param>
    /// <param name="tieBreakOnId">Indicates whether the journal entry identifier should be used as a secondary sort key.</param>
    /// <returns>The sorted journal entry query.</returns>
    private static IQueryable<JournalEntry> ApplySort(
        IQueryable<JournalEntry> query,
        string sortBy,
        string sortDirection,
        bool tieBreakOnId)
    {
        if (!tieBreakOnId)
        {
            return (sortBy, sortDirection) switch
            {
                ("Amount", "Asc") => query.OrderBy(x => x.Posting.Amount),
                ("Amount", "Desc") => query.OrderByDescending(x => x.Posting.Amount),
                ("Status", "Asc") => query.OrderBy(x => x.Posting.Status),
                ("Status", "Desc") => query.OrderByDescending(x => x.Posting.Status),
                ("Type", "Asc") => query.OrderBy(x => x.Posting.Type),
                ("Type", "Desc") => query.OrderByDescending(x => x.Posting.Type),
                ("Timestamp", "Asc") => query.OrderBy(x => x.Timestamp),
                _ => query.OrderByDescending(x => x.Timestamp)
            };
        }

        return (sortBy, sortDirection) switch
        {
            ("Amount", "Asc") => query.OrderBy(x => x.Posting.Amount).ThenBy(x => x.Id),
            ("Amount", "Desc") => query.OrderByDescending(x => x.Posting.Amount).ThenByDescending(x => x.Id),
            ("Status", "Asc") => query.OrderBy(x => x.Posting.Status).ThenBy(x => x.Id),
            ("Status", "Desc") => query.OrderByDescending(x => x.Posting.Status).ThenByDescending(x => x.Id),
            ("Type", "Asc") => query.OrderBy(x => x.Posting.Type).ThenBy(x => x.Id),
            ("Type", "Desc") => query.OrderByDescending(x => x.Posting.Type).ThenByDescending(x => x.Id),
            ("Timestamp", "Asc") => query.OrderBy(x => x.Timestamp).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Id)
        };
    }

    /// <summary>
    /// Streams journal entries for the specified ledger account using the requested filters.
    /// </summary>
    /// <remarks>
    /// Entries are ordered by timestamp ascending and then by identifier ascending
    /// to provide deterministic ordering required for ledger chain verification.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="dateFrom">Optional start timestamp used to filter journal entries.</param>
    /// <param name="dateTo">Optional end timestamp used to filter journal entries.</param>
    /// <param name="status">Optional entry status used to filter journal entries.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous enumeration.</param>
    /// <returns>
    /// An asynchronous stream of journal entries matching the specified filters,
    /// ordered by timestamp and entry identifier.
    /// </returns>
    public async IAsyncEnumerable<JournalEntry> StreamEntriesAsync(
        string accountId,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        EntryStatus? status = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        IQueryable<JournalEntry> query = BuildFilteredQuery(
            accountId, dateFrom, dateTo, null, null, status, null);

        // Chain verification requires specific ordering: Timestamp ASC, Id ASC
        query = query.OrderBy(x => x.Timestamp).ThenBy(x => x.Id);

        var asyncEnumerable = query.ToAsyncEnumerable(ct);
        await foreach (var entry in asyncEnumerable)
        {
            yield return entry;
        }
    }
}