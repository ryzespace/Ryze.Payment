using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Verification;

/// <summary>
/// Provides ledger reconciliation operations for account balances and accounting periods.
/// </summary>
/// <remarks>
/// Reconciles materialized account balances against aggregated cleared journal entries
/// and validates that the ledger remains balanced before an accounting period is closed.
/// </remarks>
public sealed class LedgerReconciliation(
    IReportingQuery reportingQuery,
    ILedgerReconciliationQuery reconciliationQuery,
    IBalanceCalculator balanceCalc,
    ILogger<LedgerReconciliation> logger) : ILedgerReconciliation
{
    /// <summary>
    /// Reconciles the projected balance of an account against its underlying cleared ledger entries.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account to reconcile.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<bool> ReconcileAccountAsync(string accountId, CancellationToken ct = default)
    {
        logger.LogInformation("Reconciling account {AccountId} from raw ledger entries using DB aggregation...", accountId);

        var summary = await reportingQuery.GetAccountSummaryAsync(accountId, ct);
        var totals = await reconciliationQuery.GetPeriodTotalsAsync(accountId, status: EntryStatus.Cleared, ct: ct);

        if (summary == null)
        {
            return totals.EntryCount == 0;
        }

        var category = balanceCalc.InferCategory(accountId);
        var computedBalance = balanceCalc.CalculateBalance(totals.TotalDebits, totals.TotalCredits, category);

        bool isReconciled =
            summary.TotalDebits == totals.TotalDebits &&
            summary.TotalCredits == totals.TotalCredits &&
            summary.CurrentBalance == computedBalance;

        if (!isReconciled)
        {
            logger.LogError("Reconciliation Failed for Account {AccountId}. " +
                            "Projected [D: {PDebits}, C: {PCredits}, B: {PBalance}]. " +
                            "Computed [D: {CDebits}, C: {CCredits}, B: {CBalance}].",
                accountId,
                summary.TotalDebits, summary.TotalCredits, summary.CurrentBalance,
                totals.TotalDebits, totals.TotalCredits, computedBalance);
        }

        return isReconciled;
    }

    /// <summary>
    /// Reconciles the ledger before closing an accounting period.
    /// </summary>
    /// <param name="periodEnd">The timestamp representing the end of the accounting period to reconcile.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<bool> ClosePeriodAsync(DateTimeOffset periodEnd, CancellationToken ct = default)
    {
        logger.LogInformation("Starting period closing reconciliation for period ending {PeriodEnd} using global aggregation...", periodEnd);

        var totals = await reconciliationQuery.GetGlobalPeriodTotalsAsync(periodEnd, status: EntryStatus.Cleared, ct: ct);

        if (totals.TotalDebits != totals.TotalCredits)
        {
            logger.LogError("PERIOD CLOSING FAILED: Ledger is out of balance as of {PeriodEnd}. Total Debits: {TotalDebits}, Total Credits: {TotalCredits}, Difference: {Diff}",
                periodEnd, totals.TotalDebits, totals.TotalCredits, totals.TotalDebits - totals.TotalCredits);
            return false;
        }

        logger.LogInformation("Period closing reconciliation successful for period ending {PeriodEnd}. Ledger is balanced. Total Entries: {Count}",
            periodEnd, totals.EntryCount);

        return true;
    }
}