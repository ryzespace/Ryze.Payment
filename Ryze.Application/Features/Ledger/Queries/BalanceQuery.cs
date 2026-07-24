using Ryze.Application.Features.Ledger.DTO.Balance;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Application.Shared.Locking;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Queries;

/// <summary>
/// Provides balance query operations for ledger accounts.
/// </summary>
/// <remarks>
/// Retrieves current, available, pending, historical, and dual balance
/// information from the ledger balance repository.
/// </remarks>
public sealed class BalanceQuery(
    ILedgerBalanceRepository balanceRepo,
    ILockProvider lockProvider) : IBalanceQuery
{
    /// <summary>
    /// Returns the current balance snapshot for the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<BalanceSnapshotDto?> GetBalanceAsync(string accountId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId);

        var balance = await balanceRepo.GetAccountBalanceAsync(accountId, ct);
        if (balance is null)
            return null;

        var normalBalance = GetNormalBalanceForAccount(accountId);

        return new BalanceSnapshotDto(
            balance.Id,
            balance.Currency,
            balance.CurrentBalance,
            balance.GetAvailableBalance(normalBalance),
            balance.PendingBalance,
            balance.LastUpdatedAt
        );
    }

    /// <summary>
    /// Returns current balance snapshots for all ledger accounts.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<IReadOnlyList<BalanceSnapshotDto>> GetAllBalancesAsync(CancellationToken ct = default)
    {
        var balances = await balanceRepo.GetAllBalancesAsync(ct);

        return [.. balances
            .OrderBy(b => b.Id, StringComparer.Ordinal)
            .Select(balance =>
            {
                var normalBalance = GetNormalBalanceForAccount(balance.Id);
                return new BalanceSnapshotDto(
                    balance.Id,
                    balance.Currency,
                    balance.CurrentBalance,
                    balance.GetAvailableBalance(normalBalance),
                    balance.PendingBalance,
                    balance.LastUpdatedAt
                );
            })];
    }

    /// <summary>
    /// Returns the calculated balance of an account at specific point in time.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="timestamp">The point in time at which the historical balance is calculated.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The historical balance snapshot, or null when the account does not have materialized balance. </returns>
    public async Task<BalanceAtDto?> GetBalanceAtAsync(string accountId, DateTimeOffset timestamp, CancellationToken ct = default)
    {
        var balance = await balanceRepo.GetAccountBalanceAsync(accountId, ct);
        if (balance is null)
            return null;

        var (computedBalance, entryCount) = await balanceRepo.GetBalanceAtAsync(accountId, timestamp, ct);

        return new BalanceAtDto(
            accountId,
            balance.Currency,
            computedBalance,
            entryCount,
            timestamp
        );
    }

    /// <summary>
    /// Returns the current and pending balance information for an account,
    /// separating pending incoming and outgoing amounts according to the
    /// account's normal balance direction.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<DualBalanceDto?> GetDualBalanceAsync(string accountId, CancellationToken ct = default)
    {
        var balance = await balanceRepo.GetAccountBalanceAsync(accountId, ct);
        if (balance is null)
            return null;

        var normalBalance = GetNormalBalanceForAccount(accountId);
        var pendingIncoming = normalBalance == EntryType.Debit
            ? balance.PendingDebits
            : balance.PendingCredits;
        var pendingOutgoing = normalBalance == EntryType.Debit
            ? balance.PendingCredits
            : balance.PendingDebits;

        return new DualBalanceDto(
            balance.Id,
            balance.Currency,
            balance.CurrentBalance,
            balance.PendingBalance,
            balance.GetAvailableBalance(normalBalance),
            pendingIncoming,
            pendingOutgoing,
            balance.LastUpdatedAt
        );
    }

    /// <summary>
    /// Determines whether the specified ledger account is currently locked.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<bool> IsAccountLockedAsync(string accountId, CancellationToken ct = default)
    {
        var lockKey = $"ledger:account:{accountId}";
        return await lockProvider.IsLockedAsync(lockKey);
    }

    /// <summary>
    /// Determines the normal balance direction for ledger account
    /// based on its account identifier.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    private static EntryType GetNormalBalanceForAccount(string accountId)
    {
        if (accountId.StartsWith("asset:", StringComparison.OrdinalIgnoreCase) ||
            accountId.StartsWith("expense:", StringComparison.OrdinalIgnoreCase))
        {
            return EntryType.Debit;
        }

        return EntryType.Credit;
    }
}