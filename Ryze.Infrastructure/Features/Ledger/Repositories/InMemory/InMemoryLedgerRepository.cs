using System.Collections.Concurrent;
using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.ReadModels;
using Ryze.Domain.Features.Ledger.Repositories;
using Ryze.Domain.Shared;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.InMemory;

/// <summary>
/// Provides an in-memory implementation of ledger persistence, query,
/// balance calculation, entry number generation, and event publishing contracts.
/// </summary>
/// <remarks>
/// Stores journal entries in thread safe concurrent dictionary and provides
/// lightweight implementations of ledger repository abstractions for development,
/// testing, and single process execution.
/// </remarks>
/// <param name="balanceCalc">The service used to calculate account balances and infer account categories.</param>
public sealed class InMemoryLedgerRepository(IBalanceCalculator balanceCalc) : IJournalEntryRepository,
    ILedgerEntryQuery, ILedgerBalanceRepository, ILedgerEntryNumberGenerator, IEventPublisher
{
    private readonly ConcurrentDictionary<Guid, JournalEntry> Entries = new();
    private long _entrySequence;

    /// <summary>
    /// Retrieves a journal entry by its unique identifier.
    /// </summary>
    /// <param name="entryId">The unique identifier of the journal entry.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching journal entry, or <see langword="null"/> if no entry exists
    /// with the specified identifier.
    /// </returns>
    public Task<JournalEntry?> GetEntryByIdAsync(Guid entryId, CancellationToken ct = default)
    {
        Entries.TryGetValue(entryId, out var entry);
        return Task.FromResult(entry);
    }

    /// <summary>
    /// Retrieves all journal entries associated with a ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection containing all journal entries associated
    /// with the specified account.
    /// </returns>
    public Task<IReadOnlyList<JournalEntry>> GetEntriesAsync(string accountId, CancellationToken ct = default)
    {
        var result = Entries.Values.Where(e => e.Account.AccountId == accountId).ToList();
        return Task.FromResult<IReadOnlyList<JournalEntry>>(result);
    }

    /// <summary>
    /// Calculates the current balance of a ledger account from stored journal entries.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The calculated balance of the specified ledger account.</returns>
    public Task<decimal> GetBalanceAsync(string accountId, CancellationToken ct = default)
    {
        var entries = Entries.Values.Where(e => e.Account.AccountId == accountId).ToList();
        var debits = entries.Where(e => e.Posting.Type == EntryType.Debit).Sum(e => e.Posting.Amount);
        var credits = entries.Where(e => e.Posting.Type == EntryType.Credit).Sum(e => e.Posting.Amount);
        var category = balanceCalc.InferCategory(accountId);

        return Task.FromResult(balanceCalc.CalculateBalance(debits, credits, category));
    }

    /// <summary>
    /// Retrieves a journal entry associated with the specified idempotency key.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key identifying the ledger operation.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching journal entry, or <see langword="null"/> if no entry
    /// is associated with the specified idempotency key.
    /// </returns>
    public Task<JournalEntry?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var entry = Entries.Values.FirstOrDefault(e => e.Audit.IdempotencyKey == idempotencyKey);
        return Task.FromResult(entry);
    }

    /// <summary>
    /// Retrieves all journal entries belonging to a ledger transaction.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the ledger transaction.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection containing all journal entries associated
    /// with the specified transaction.
    /// </returns>
    public Task<IReadOnlyList<JournalEntry>> GetTransactionEntriesAsync(Guid transactionId, CancellationToken ct = default)
    {
        var result = Entries.Values.Where(e => e.TransactionId == transactionId).ToList();
        return Task.FromResult<IReadOnlyList<JournalEntry>>(result);
    }

    /// <summary>
    /// Retrieves the materialized balance projection for a ledger account.
    /// </summary>
    /// <remarks>
    /// This in-memory implementation does not maintain materialized account balance
    /// projections and therefore always returns <see langword="null"/>.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Always returns <see langword="null"/>.</returns>
    public Task<LedgerAccountBalance?> GetAccountBalanceAsync(string accountId, CancellationToken ct = default) =>
        Task.FromResult<LedgerAccountBalance?>(null);

    /// <summary>
    /// Retrieves all materialized ledger account balance projections.
    /// </summary>
    /// <remarks>
    /// This in-memory implementation does not maintain materialized balance
    /// projections and therefore returns an empty collection.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An empty read-only collection.</returns>
    public Task<IReadOnlyList<LedgerAccountBalance>> GetAllBalancesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<LedgerAccountBalance>>(Array.Empty<LedgerAccountBalance>());

    /// <summary>
    /// Queries journal entries for an account using cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Supports filtering by timestamp, amount, status, and entry direction,
    /// as well as sorting by supported journal entry properties.
    /// The continuation token identifies the position from which the next
    /// result page should be retrieved.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
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
    /// <returns>
    /// A tuple containing the requested journal entries and a value indicating
    /// whether additional entries are available.
    /// </returns>
    public Task<(IReadOnlyList<JournalEntry> Items, bool HasMore)> QueryEntriesWithCursorAsync(
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
        var query = Entries.Values.Where(x => x.Account.AccountId == accountId).AsEnumerable();

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

        var list = query.ToList();

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            var cursor = Application.Features.Ledger.Helpers.PageToken.Decode(continuationToken);

            if (cursor is not null)
            {
                var isDesc = cursor.SortDirection.Equals("Desc", StringComparison.OrdinalIgnoreCase);

                if (cursor.SortBy == "Amount")
                {
                    if (decimal.TryParse(cursor.LastValue, out var lastAmount))
                    {
                        list = isDesc
                            ? list.Where(x => x.Posting.Amount < lastAmount || (x.Posting.Amount == lastAmount && x.Id.CompareTo(cursor.LastId) < 0)).ToList()
                            : list.Where(x => x.Posting.Amount > lastAmount || (x.Posting.Amount == lastAmount && x.Id.CompareTo(cursor.LastId) > 0)).ToList();
                    }
                }
                else if (DateTimeOffset.TryParse(cursor.LastValue, out var lastTs))
                {
                    list = isDesc
                        ? list.Where(x => x.Timestamp < lastTs || (x.Timestamp == lastTs && x.Id.CompareTo(cursor.LastId) < 0)).ToList()
                        : list.Where(x => x.Timestamp > lastTs || (x.Timestamp == lastTs && x.Id.CompareTo(cursor.LastId) > 0)).ToList();
                }
            }
        }

        list = (sortBy, sortDirection) switch
        {
            ("Amount", "Asc") => [.. list.OrderBy(x => x.Posting.Amount).ThenBy(x => x.Id)],
            ("Amount", "Desc") => [.. list.OrderByDescending(x => x.Posting.Amount).ThenByDescending(x => x.Id)],
            ("Status", "Asc") => [.. list.OrderBy(x => x.Posting.Status).ThenBy(x => x.Id)],
            ("Status", "Desc") => [.. list.OrderByDescending(x => x.Posting.Status).ThenByDescending(x => x.Id)],
            ("Type", "Asc") => [.. list.OrderBy(x => x.Posting.Type).ThenBy(x => x.Id)],
            ("Type", "Desc") => [.. list.OrderByDescending(x => x.Posting.Type).ThenByDescending(x => x.Id)],
            ("Timestamp", "Asc") => [.. list.OrderBy(x => x.Timestamp).ThenBy(x => x.Id)],
            _ => [.. list.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Id)]
        };

        var hasMore = list.Count > limit;
        var items = hasMore ? list.Take(limit).ToList() : list;

        return Task.FromResult<(IReadOnlyList<JournalEntry>, bool)>((items, hasMore));
    }

    /// <summary>
    /// Queries journal entries for an account using offset based pagination.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
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
    /// <returns>
    /// A tuple containing the requested journal entries and the total number
    /// of entries matching the supplied filters.
    /// </returns>
    public Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> QueryEntriesAsync(
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
        var query = Entries.Values.Where(x => x.Account.AccountId == accountId).AsEnumerable();

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

        var list = query.ToList();
        var totalCount = list.Count;

        list = (sortBy, sortDirection) switch
        {
            ("Amount", "Asc") => [.. list.OrderBy(x => x.Posting.Amount)],
            ("Amount", "Desc") => [.. list.OrderByDescending(x => x.Posting.Amount)],
            ("Status", "Asc") => [.. list.OrderBy(x => x.Posting.Status)],
            ("Status", "Desc") => [.. list.OrderByDescending(x => x.Posting.Status)],
            ("Type", "Asc") => [.. list.OrderBy(x => x.Posting.Type)],
            ("Type", "Desc") => [.. list.OrderByDescending(x => x.Posting.Type)],
            ("Timestamp", "Asc") => [.. list.OrderBy(x => x.Timestamp)],
            _ => [.. list.OrderByDescending(x => x.Timestamp)]
        };

        var items = list.Skip(skip).Take(take).ToList();

        return Task.FromResult<(IReadOnlyList<JournalEntry>, int)>((items, totalCount));
    }

    /// <summary>
    /// Calculates the account balance at a specific point in time.
    /// </summary>
    /// <remarks>
    /// Only cleared journal entries posted at or before the specified timestamp
    /// are included in the calculation.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="timestamp">The point in time at which the balance is calculated.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the calculated balance and the number of cleared
    /// journal entries contributing to that balance.
    /// </returns>
    public Task<(decimal Balance, int EntryCount)> GetBalanceAtAsync(string accountId, DateTimeOffset timestamp, CancellationToken ct = default)
    {
        var entries = Entries.Values.Where(x => x.Account.AccountId == accountId
            && x.Timestamp <= timestamp
            && x.Posting.Status == EntryStatus.Cleared).ToList();

        var totalDebits = entries.Where(e => e.Posting.Type == EntryType.Debit).Sum(e => e.Posting.Amount);
        var totalCredits = entries.Where(e => e.Posting.Type == EntryType.Credit).Sum(e => e.Posting.Amount);
        var category = balanceCalc.InferCategory(accountId);
        var balance = balanceCalc.CalculateBalance(totalDebits, totalCredits, category);

        return Task.FromResult<(decimal, int)>((balance, entries.Count));
    }

    /// <summary>
    /// Generates the next sequential journal entry number.
    /// </summary>
    /// <remarks>
    /// Uses an atomic in-memory sequence and formats the generated number
    /// using the current UTC year in the form <c>JE-yyyy-######</c>.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The next sequential journal entry number.</returns>
    public Task<string> GetNextEntryNumberAsync(CancellationToken ct = default)
    {
        var seq = Interlocked.Increment(ref _entrySequence);
        var year = DateTimeOffset.UtcNow.Year;

        return Task.FromResult($"JE-{year}-{seq:D6}");
    }

    /// <summary>
    /// Publishes an event to the in memory event publisher.
    /// </summary>
    /// <remarks>
    /// This implementation intentionally performs no operation. It exists to satisfy
    /// the event publisher contract in development and testing environments where
    /// durable event storage is not required.
    /// </remarks>
    /// <typeparam name="T">The type of the event being published.</typeparam>
    /// <param name="streamId">The unique identifier of the event stream.</param>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A completed task representing the publish operation.</returns>
    public Task PublishAsync<T>(Guid streamId, T @event, CancellationToken ct = default) where T : class
    {
        return Task.CompletedTask;
    }
}