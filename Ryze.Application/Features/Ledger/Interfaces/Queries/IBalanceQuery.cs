using Ryze.Application.Features.Ledger.DTO.Balance;

namespace Ryze.Application.Features.Ledger.Interfaces.Queries;

/// <summary>
/// Defines read only operations for retrieving ledger account balances.
/// </summary>
/// <remarks>
/// Provides application layer queries for getting current balances,
/// historical balance snapshots, projected balances, and account state
/// information without mutating ledger data.
/// </remarks>
public interface IBalanceQuery
{
    /// <summary>
    /// Returns a lightweight balance snapshot for the given account.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Balance snapshot, or null when the account has no balance data.</returns>
    Task<BalanceSnapshotDto?> GetBalanceAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns balance snapshots for all accounts, ordered by account ID.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ordered list of all account balance snapshots.</returns>
    Task<IReadOnlyList<BalanceSnapshotDto>> GetAllBalancesAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the account balance at a specific point in ledger history.
    /// </summary>
    /// <remarks>
    /// Only cleared ledger entries created on or before the specified
    /// timestamp are included in the calculation.
    /// </remarks>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="timestamp">Historical timestamp used for balance calculation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Historical balance snapshot, or null when no balance data exists. </returns>
    Task<BalanceAtDto?> GetBalanceAtAsync(
        string accountId,
        DateTimeOffset timestamp,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves committed and projected balance information for an account.
    /// </summary>
    /// <remarks>
    /// The committed balance represents finalized cleared entries,
    /// while the projected balance includes pending ledger activity.
    /// </remarks>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dual balance snapshot, or null when the account has no balance data.</returns>
    Task<DualBalanceDto?> GetDualBalanceAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Determines whether the specified ledger account is currently locked.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsAccountLockedAsync(
        string accountId,
        CancellationToken ct = default);
}
