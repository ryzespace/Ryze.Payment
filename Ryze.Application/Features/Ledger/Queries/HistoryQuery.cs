using Ryze.Application.Features.Ledger.DTO.Entry;
using Ryze.Application.Features.Ledger.Helpers;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Queries;

/// <summary>
/// Provides journal entry history query operations.
/// </summary>
/// <remarks>
/// Retrieves ledger entry history using configurable filtering, sorting,
/// The query implementation maps domain journal entries into application
/// layer DTOs and generates opaque continuation tokens for efficient
/// traversal of large entry histories.
/// </remarks>
public sealed class HistoryQuery(
    ILedgerEntryQuery entryQuery) : IHistoryQuery
{
    /// <summary>
    /// Queries journal entry history using offset based pagination.
    /// </summary>
    /// <param name="filter">The filter and pagination criteria used to query journal entries.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<EntryHistoryResultDto> QueryEntryHistoryAsync(EntryHistoryFilter filter, CancellationToken ct = default)
    {
        var (items, totalCount) = await entryQuery.QueryEntriesAsync(
            filter.AccountId,
            filter.DateFrom,
            filter.DateTo,
            filter.AmountMin,
            filter.AmountMax,
            filter.Status,
            filter.Direction,
            filter.SortBy,
            filter.SortDirection,
            filter.Skip,
            filter.Take,
            ct);

        var entryItems = items.Select(e => new EntryItemDto(
            e.Id,
            e.TransactionId,
            e.Timestamp,
            e.Account.AccountId,
            e.Posting.Type.ToString(),
            e.Posting.Amount,
            e.Posting.Currency.ToString(),
            e.Posting.Status.ToString(),
            e.Audit.TransactionType.ToString(),
            e.Posting.Description,
            e.Audit.InitiatedBy
        )).ToList();

        return new EntryHistoryResultDto(entryItems, totalCount, filter.Skip, filter.Take);
    }

    /// <summary>
    /// Queries journal entry history using cursor based pagination.
    /// </summary>
    /// <remarks>
    /// The requested page size is capped at 1000 entries. When additional
    /// entries are available, an opaque continuation token is generated from
    /// the last returned entry and can be supplied to retrieve the next page.
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
    /// <param name="limit">The maximum number of entries to return. Values above 1000 are capped.</param>
    /// <param name="continuationToken">An opaque cursor identifying the position from which the next page should be retrieved.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<EntryHistoryTokenResultDto> QueryEntryHistoryWithTokenAsync(
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
        var actualLimit = Math.Min(limit, 1000);

        var (items, _) = await entryQuery.QueryEntriesWithCursorAsync(
            accountId,
            dateFrom,
            dateTo,
            amountMin,
            amountMax,
            status,
            direction,
            sortBy,
            sortDirection,
            actualLimit,
            continuationToken,
            ct);

        var hasMore = items.Count == actualLimit;
        string? nextToken = null;

        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextToken = PageToken.FromEntry(last, sortBy, sortDirection);
        }

        var entryItems = items.Select(e => new EntryItemDto(
            e.Id,
            e.TransactionId,
            e.Timestamp,
            e.Account.AccountId,
            e.Posting.Type.ToString(),
            e.Posting.Amount,
            e.Posting.Currency.ToString(),
            e.Posting.Status.ToString(),
            e.Audit.TransactionType.ToString(),
            e.Posting.Description,
            e.Audit.InitiatedBy
        )).ToList();

        return new EntryHistoryTokenResultDto(entryItems, nextToken, hasMore);
    }
}