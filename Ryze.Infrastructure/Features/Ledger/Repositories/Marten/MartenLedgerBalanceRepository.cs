using Marten;
using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.ReadModels;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// Provides Marten backed persistence and query operations for ledger account balances.
/// </summary>
/// <remarks>
/// Uses materialized <see cref="LedgerAccountBalance"/> read models for current and
/// aggregate balance queries, while historical balance queries are calculated from
/// cleared journal entries up to the requested point in time.
/// </remarks>
public sealed class MartenLedgerBalanceRepository(IDocumentSession session, IBalanceCalculator balanceCalc)
    : ILedgerBalanceRepository
{
    /// <summary>
    /// Returns the current calculated balance of the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The current account balance, or zero when no balance projection exists.</returns>
    public async Task<decimal> GetBalanceAsync(string accountId, CancellationToken ct = default)
    {
        var readModel = await session.LoadAsync<LedgerAccountBalance>(accountId, ct);
        return readModel?.CurrentBalance ?? 0m;
    }

    /// <summary>
    /// Returns the materialized balance projection for the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The account balance projection, or null when no projection exists.</returns>
    public async Task<LedgerAccountBalance?> GetAccountBalanceAsync(string accountId, CancellationToken ct = default) =>
        await session.LoadAsync<LedgerAccountBalance>(accountId, ct);

    /// <summary>
    /// Returns all materialized ledger account balance projections.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Collection containing all available account balance projections.</returns>
    public async Task<IReadOnlyList<LedgerAccountBalance>> GetAllBalancesAsync(CancellationToken ct = default) =>
        await session.Query<LedgerAccountBalance>().ToListAsync(ct);

    /// <summary>
    /// Calculates the balance of ledger account at specific point in time.
    /// </summary>
    /// <remarks>
    /// Only journal entries with <see cref="EntryStatus.Cleared"/> status and timestamp
    /// less than or equal to the requested timestamp are included in the calculation.
    /// The resulting debit and credit totals are interpreted according to the account's
    /// inferred normal balance category.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="timestamp">The point in time at which the historical balance is calculated.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<(decimal Balance, int EntryCount)> GetBalanceAtAsync(string accountId, DateTimeOffset timestamp, CancellationToken ct = default)
    {
        var entries = await session.Query<JournalEntry>()
            .Where(x => x.Account.AccountId == accountId
                && x.Timestamp <= timestamp
                && x.Posting.Status == EntryStatus.Cleared)
            .ToListAsync(ct);

        var totalDebits = entries
            .Where(e => e.Posting.Type == EntryType.Debit)
            .Sum(e => e.Posting.Amount);

        var totalCredits = entries
            .Where(e => e.Posting.Type == EntryType.Credit)
            .Sum(e => e.Posting.Amount);

        var category = balanceCalc.InferCategory(accountId);
        var balance = balanceCalc.CalculateBalance(totalDebits, totalCredits, category);

        return (balance, entries.Count);
    }
}