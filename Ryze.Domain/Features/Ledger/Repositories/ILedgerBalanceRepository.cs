using Ryze.Domain.Features.Ledger.ReadModels;

namespace Ryze.Domain.Features.Ledger.Repositories;

/// <summary>
/// Defines read only persistence operations for ledger account balances.
/// </summary>
/// <remarks>
/// Provides access to current materialized balances, account balance projections,
/// historical balances, and balance information for all ledger accounts.
/// </remarks>
public interface ILedgerBalanceRepository
{
    /// <summary>
    /// Retrieves the current calculated balance of a ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose balance is requested.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The current balance of the specified ledger account.</returns>
    Task<decimal> GetBalanceAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the materialized balance projection for a ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose balance projection is requested.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The materialized account balance projection. </returns>
    Task<LedgerAccountBalance?> GetAccountBalanceAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the materialized balance projections for all ledger accounts.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>collection containing the balance projection of each ledger account.</returns>
    Task<IReadOnlyList<LedgerAccountBalance>> GetAllBalancesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves the calculated balance of an account at a specific point in time.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account whose historical balance is requested.</param>
    /// <param name="timestamp">The point in time at which the account balance should be calculated.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the calculated account balance and the number of
    /// journal entries contributing to that balance up to the specified timestamp.
    /// </returns>
    Task<(decimal Balance, int EntryCount)> GetBalanceAtAsync(string accountId, DateTimeOffset timestamp, CancellationToken ct = default);
}