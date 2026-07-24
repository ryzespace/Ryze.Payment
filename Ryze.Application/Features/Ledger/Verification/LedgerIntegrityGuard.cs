using Microsoft.Extensions.Logging;
using Ryze.Domain.Features.Ledger.Repositories;

namespace Ryze.Application.Features.Ledger.Verification;

/// <summary>
/// Provides comprehensive ledger integrity verification across financial aggregates
/// and account balance chains.
/// </summary>
/// <remarks>
/// Combines server side currency reconciliation with per account running balance
/// chain verification to detect imbalances, projection inconsistencies, and
/// corrupted ledger state.
/// </remarks>
public sealed class LedgerIntegrityGuard(
    ILedgerReconciliationQuery reconciliationQuery,
    ChainVerification chainVerifier,
    ILogger<LedgerIntegrityGuard> logger)
{
    /// <summary> Performs full integrity verification of the ledger. </summary>
    /// <remarks>
    /// Verifies that debit and credit totals are balanced for each currency
    /// and validates the running balance chain for every ledger account.
    /// Any detected inconsistencies are collected into the resulting report.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An integrity report containing the overall health status, detected issues,
    /// and the timestamp at which the verification completed.
    /// </returns>
    public async Task<IntegrityReport> VerifyFullIntegrityAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting full ledger integrity check using server-side aggregations...");

        var issues = new List<string>();

        var currencyTotals = await reconciliationQuery.GetGlobalTotalsPerCurrencyAsync(ct: ct);

        foreach (var total in currencyTotals)
        {
            if (Math.Abs(total.TotalDebits - total.TotalCredits) > 0.0000000001m)
            {
                issues.Add(
                    $"Currency {total.Currency} is imbalanced. " +
                    $"Debits: {total.TotalDebits}, " +
                    $"Credits: {total.TotalCredits}, " +
                    $"Diff: {total.TotalDebits - total.TotalCredits}");
            }

            logger.LogDebug(
                "Currency {Currency} totals: D={Debits}, C={Credits}, Count={Count}",
                total.Currency,
                total.TotalDebits,
                total.TotalCredits,
                total.EntryCount);
        }

        logger.LogInformation("Running balance chain verification for all accounts...");

        var chainReports = await chainVerifier.VerifyAllChainsAsync(includeEntries: false, ct);
        var totalEntries = 0;

        foreach (var report in chainReports)
        {
            totalEntries += report.TotalEntries;

            if (!report.IsChainValid)
            {
                issues.Add(
                    $"Account {report.AccountId}: chain invalid — " +
                    $"{report.MismatchedEntries} mismatched entries, " +
                    $"balance computed={report.ComputedBalance} " +
                    $"vs projected={report.ProjectedBalance}");
            }

            if (report.Issues is null) continue;

            foreach (var issue in report.Issues)
            {
                issues.Add(
                    $"[{issue.IssueType}] " +
                    $"{issue.AccountId}: " +
                    $"{issue.Description}");
            }
        }

        logger.LogInformation(
            "Ledger integrity check completed with {IssueCount} issues. Verified {AccountCount} accounts and {EntryCount} entries.",
            issues.Count,
            chainReports.Count,
            totalEntries);

        return new IntegrityReport
        {
            IsHealthy = issues.Count == 0,
            Issues = issues,
            AccountsChecked = chainReports.Count,
            TotalEntriesVerified = totalEntries,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Represents the result of ledger integrity verification.
/// </summary>
/// <remarks>
/// Contains the overall health status of the ledger, collection of detected
/// integrity issues, and the timestamp at which the verification was completed.
/// </remarks>
public sealed class IntegrityReport
{
    /// <summary>Gets value indicating whether the ledger passed all integrity checks. </summary>
    public bool IsHealthy { get; init; }

    /// <summary>Gets the collection of issues detected during the integrity verification.</summary>
    public List<string> Issues { get; init; } = [];

    /// <summary>Gets the number of accounts verified during the integrity check. </summary>
    public int AccountsChecked { get; init; }

    /// <summary>Gets the total number of entries verified during the integrity check. </summary>
    public int TotalEntriesVerified { get; init; }

    /// <summary>Gets the timestamp at which the integrity verification was completed. </summary>
    public DateTimeOffset CheckedAt { get; init; }
}