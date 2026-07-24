using Ryze.Application.Features.Ledger.DTO.Balance;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Application.Shared.Locking;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.ReadModels;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Queries;

/// <summary>
/// Provides ledger account reporting and summary query operations.
/// </summary>
/// <remarks>
/// Retrieves account-level financial summaries based on materialized ledger balances,
/// journal entry counts, and current account lock state.
/// Supports both individual account reporting and aggregated reporting across
/// all ledger accounts.
/// </remarks>
public sealed class ReportingQuery(
    ILedgerBalanceRepository balanceRepo,
    IJournalEntryRepository entryRepo,
    ILockProvider lockProvider) : IReportingQuery
{
    /// <summary>
    /// Returns summaries for all ledger accounts.
    /// </summary>
    /// <remarks>
    /// Account summaries are ordered by account identifier using ordinal string comparison.
    /// Account lock states are resolved concurrently to reduce the overall latency of the operation.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection containing the current summary information for all ledger accounts.
    /// </returns>
    public async Task<IReadOnlyList<LedgerAccountSummaryDto>> GetAllAccountSummariesAsync(CancellationToken ct = default)
    {
        var balances = await balanceRepo.GetAllBalancesAsync(ct);
        var lockStates = await Task.WhenAll(
            balances.Select(async balance => new
            {
                balance.Id,
                IsLocked = await lockProvider.IsLockedAsync($"ledger:account:{balance.Id}")
            }));

        var lockMap = lockStates.ToDictionary(x => x.Id, x => x.IsLocked, StringComparer.Ordinal);

        return [.. balances
            .OrderBy(b => b.Id, StringComparer.Ordinal)
            .Select(balance => ToSummary(balance, lockMap.GetValueOrDefault(balance.Id)))];
    }

    /// <summary>
    /// Returns a summary for the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The account summary when the account exists; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<LedgerAccountSummaryDto?> GetAccountSummaryAsync(string accountId, CancellationToken ct = default)
    {
        var balance = await balanceRepo.GetAccountBalanceAsync(accountId, ct);
        if (balance is null)
            return null;

        var entryCount = (await entryRepo.GetEntriesAsync(accountId, ct)).Count;
        var normalBalance = GetNormalBalanceForAccount(accountId);

        return new LedgerAccountSummaryDto
        {
            AccountId = balance.Id,
            Currency = balance.Currency,
            CurrentBalance = balance.CurrentBalance,
            AvailableBalance = balance.GetAvailableBalance(normalBalance),
            TotalDebits = balance.TotalDebits,
            TotalCredits = balance.TotalCredits,
            PendingDebits = balance.PendingDebits,
            PendingCredits = balance.PendingCredits,
            EntryCount = entryCount,
            IsLocked = await lockProvider.IsLockedAsync($"ledger:account:{accountId}"),
            LastUpdatedAt = balance.LastUpdatedAt
        };
    }

    /// <summary>
    /// Creates an account summary from materialized balance projection.
    /// </summary>
    /// <param name="balance">The materialized ledger account balance used to build the summary.</param>
    /// <param name="isLocked">Indicates whether the account is currently locked for ledger operations.</param>
    /// <returns>A ledger account summary containing the current balance and reporting information.</returns>
    private static LedgerAccountSummaryDto ToSummary(LedgerAccountBalance balance, bool isLocked)
    {
        var normalBalance = GetNormalBalanceForAccount(balance.Id);

        return new LedgerAccountSummaryDto
        {
            AccountId = balance.Id,
            Currency = balance.Currency,
            CurrentBalance = balance.CurrentBalance,
            AvailableBalance = balance.GetAvailableBalance(normalBalance),
            TotalDebits = balance.TotalDebits,
            TotalCredits = balance.TotalCredits,
            PendingDebits = balance.PendingDebits,
            PendingCredits = balance.PendingCredits,
            EntryCount = 0,
            IsLocked = isLocked,
            LastUpdatedAt = balance.LastUpdatedAt
        };
    }

    /// <summary>
    /// Determines the normal balance direction for the specified ledger account.
    /// </summary>
    /// <remarks>
    /// Asset and expense accounts normally carry debit balances.
    /// Other account categories are treated as having credit normal balances.
    /// </remarks>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <returns>The normal balance direction used when calculating the available balance.</returns>
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