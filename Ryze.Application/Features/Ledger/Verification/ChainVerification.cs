using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Ledger.DTO.Chain;
using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Application.Features.Ledger.Interfaces.Queries;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;
using Ryze.Domain.Features.Ledger.ValueObject;

namespace Ryze.Application.Features.Ledger.Verification;

/// <summary>
/// Verifies the integrity and consistency of account balance chains
/// reconstructed from cleared ledger journal entries.
/// </summary>
/// <remarks>
/// The verification process reconstructs an account's running balance by
/// processing cleared journal entries in chronological order and comparing
/// the calculated values with the running balances persisted on each entry.
/// </remarks>
public sealed class ChainVerification(
    ILedgerEntryQuery entryQuery,
    ILedgerBalanceRepository balanceRepo,
    IReportingQuery reportingQuery,
    IBalanceCalculator balanceCalc,
    IServiceScopeFactory scopeFactory,
    ILogger<ChainVerification> logger)
{
    /// <summary>
    /// Verifies the balance chain of a single ledger account.
    /// </summary>
    /// <remarks>
    /// Only cleared journal entries are considered when reconstructing
    /// the account's running balance. Entries are processed chronologically
    /// using keyset streaming and their calculated impact is compared 
    /// against the persisted <see cref="LedgerPosting.RunningBalance"/> value.
    /// </remarks>
    /// <param name="accountId">
    /// The unique identifier of the ledger account whose balance chain
    /// should be verified.
    /// </param>
    /// <param name="includeEntries">
    /// Indicates whether the resulting report should include detailed
    /// information for each verified journal entry.
    /// </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<BalanceChainReportDto> VerifyAccountChainAsync(
        string accountId, bool includeEntries = false, CancellationToken ct = default)
    {
        var category = balanceCalc.InferCategory(accountId);

        var issues = new List<ChainIssue>();
        var chainEntries = includeEntries ? new List<ChainEntryItem>() : null;
        var matchedCount = 0;
        var mismatchedCount = 0;
        var runningBalance = 0m;
        var totalCount = 0;

        Ryze.Domain.Shared.Enum.Currency? firstCurrency = null;
        bool multiCurrencyDetected = false;

        JournalEntry? prev = null;

        await foreach (var entry in entryQuery.StreamEntriesAsync(accountId, status: EntryStatus.Cleared, ct: ct))
        {
            totalCount++;
            
            if (firstCurrency == null)
                firstCurrency = entry.Posting.Currency;
            else if (entry.Posting.Currency != firstCurrency)
                multiCurrencyDetected = true;

            runningBalance += balanceCalc.CalculateEntryImpact(entry.Posting.Type, entry.Posting.Amount, category);

            var isMatch = entry.Posting.RunningBalance == runningBalance;

            if (includeEntries)
                chainEntries!.Add(new ChainEntryItem(
                    entry.Id, entry.TransactionId, entry.Timestamp,
                    entry.Posting.Type.ToString(), entry.Posting.Amount,
                    runningBalance, entry.Posting.RunningBalance, isMatch));

            switch (isMatch)
            {
                case false when entry.Posting.RunningBalance != 0:
                    mismatchedCount++;
                    issues.Add(new ChainIssue(accountId, "RunningBalanceMismatch",
                        $"Entry {entry.Id} at {entry.Timestamp:O}: expected running balance {runningBalance}, stored {entry.Posting.RunningBalance}",
                        entry.Id));
                    break;
                case false when entry.Posting.RunningBalance == 0:
                    break;
                default:
                    matchedCount++;
                    break;
            }

            if (prev != null)
            {
                var gap = entry.Timestamp - prev.Timestamp;
                if (gap > TimeSpan.FromDays(1) && prev.Timestamp.Date != entry.Timestamp.Date)
                {
                    logger.LogInformation("Time gap of {GapDays:F1} days between entry {PrevId} and {EntryId} for account {Account}",
                        gap.TotalDays, prev.Id, entry.Id, accountId);
                }
            }

            prev = entry;
        }

        var balance = await balanceRepo.GetAccountBalanceAsync(accountId, ct);
        var projectedBalance = balance?.CurrentBalance ?? 0m;
        var balanceMatches = Math.Abs(runningBalance - projectedBalance) < 0.0001m;

        if (!balanceMatches)
        {
            issues.Add(new ChainIssue(accountId, "ProjectionMismatch",
                $"Computed balance {runningBalance} does not match projected balance {projectedBalance}"));
        }

        if (multiCurrencyDetected)
        {
            issues.Add(new ChainIssue(accountId, "MultiCurrency",
                "CRITICAL: Account has entries in multiple currencies."));
            logger.LogCritical("Account {Account} has entries in multiple currencies.", accountId);
        }

        var isChainValid = mismatchedCount == 0 && balanceMatches && !multiCurrencyDetected;

        return new BalanceChainReportDto(
            accountId,
            balance?.Currency ?? "unknown",
            totalCount,
            matchedCount,
            mismatchedCount,
            isChainValid,
            runningBalance,
            projectedBalance,
            balanceMatches,
            DateTimeOffset.UtcNow,
            chainEntries?.AsReadOnly(),
            issues.AsReadOnly()
        );
    }

    /// <summary>
    /// Verifies the balance chains of all accounts available through the
    /// ledger reporting query.
    /// </summary>
    /// <remarks>
    /// Accounts are processed in parallel with a bounded concurrency level.
    /// Each worker uses its own <see cref="IServiceScope"/> to ensure
    /// thread safety of database sessions.
    /// </remarks>
    public async Task<IReadOnlyList<BalanceChainReportDto>> VerifyAllChainsAsync(
        bool includeEntries = false, CancellationToken ct = default)
    {
        var summaries = await reportingQuery.GetAllAccountSummariesAsync(ct);
        var reports = new System.Collections.Concurrent.ConcurrentBag<BalanceChainReportDto>();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(summaries, options, async (summary, token) =>
        {
            using var scope = scopeFactory.CreateScope();
            var scopedVerification = new ChainVerification(
                scope.ServiceProvider.GetRequiredService<ILedgerEntryQuery>(),
                scope.ServiceProvider.GetRequiredService<ILedgerBalanceRepository>(),
                scope.ServiceProvider.GetRequiredService<IReportingQuery>(),
                scope.ServiceProvider.GetRequiredService<IBalanceCalculator>(),
                scopeFactory,
                scope.ServiceProvider.GetRequiredService<ILogger<ChainVerification>>());

            try
            {
                var report = await scopedVerification.VerifyAccountChainAsync(summary.AccountId, includeEntries, token);
                reports.Add(report);

                if (!report.IsChainValid)
                {
                    logger.LogWarning("Chain verification failed for account {Account}: {Issues}",
                        summary.AccountId,
                        string.Join("; ", report.Issues?.Select(i => i.Description) ?? []));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Chain verification error for account {Account}", summary.AccountId);
            }
        });

        return reports.ToList().AsReadOnly();
    }
}
