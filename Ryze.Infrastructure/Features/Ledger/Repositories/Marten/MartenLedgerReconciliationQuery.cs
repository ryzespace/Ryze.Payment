using Marten;
using Marten.Linq;
using Ryze.Domain.Features.Ledger.DTO;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Repositories;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// Reconciliation and aggregate query operations for ledger entries.
/// </summary>
/// <remarks>
/// Uses server side aggregations to calculate debit, credit, and entry count totals
/// for individual accounts, the entire ledger, and individual currencies.
/// </remarks>
public sealed class MartenLedgerReconciliationQuery(IDocumentSession session)
    : ILedgerReconciliationQuery
{
    /// <summary>
    /// Returns aggregated debit, credit, and entry totals for the specified ledger account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="periodEnd">Optional point in time up to which journal entries are included. </param>
    /// <param name="status">Optional entry status used to filter the results. Defaults to cleared entries. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns> Aggregated debit, credit, and entry count totals for the specified account. </returns>
    public async Task<PeriodTotals> GetPeriodTotalsAsync(
        string accountId,
        DateTimeOffset? periodEnd = null,
        EntryStatus? status = EntryStatus.Cleared,
        CancellationToken ct = default)
    {
        IQueryable<JournalEntry> query = session.Query<JournalEntry>()
            .Where(x => x.Account.AccountId == accountId);

        if (periodEnd.HasValue)
        {
            query = query.Where(x => x.Timestamp <= periodEnd.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Posting.Status == status.Value);
        }

        var debits = await ((IMartenQueryable<JournalEntry>)query)
            .Where(x => x.Posting.Type == EntryType.Debit)
            .SumAsync(x => x.Posting.Amount, ct);

        var credits = await ((IMartenQueryable<JournalEntry>)query)
            .Where(x => x.Posting.Type == EntryType.Credit)
            .SumAsync(x => x.Posting.Amount, ct);

        var count = await ((IMartenQueryable<JournalEntry>)query)
            .CountAsync(ct);

        return new PeriodTotals(debits, credits, count);
    }

    /// <summary>
    /// Returns aggregated debit, credit, and entry totals for the entire ledger.
    /// </summary>
    /// <param name="periodEnd">Optional point in time up to which journal entries are included. </param>
    /// <param name="status">Optional entry status used to filter the results. Defaults to cleared entries.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>Aggregated debit, credit, and entry count totals for the entire ledger. </returns>
    public async Task<PeriodTotals> GetGlobalPeriodTotalsAsync(
        DateTimeOffset? periodEnd = null,
        EntryStatus? status = EntryStatus.Cleared,
        CancellationToken ct = default)
    {
        IQueryable<JournalEntry> query = session.Query<JournalEntry>();

        if (periodEnd.HasValue)
        {
            query = query.Where(x => x.Timestamp <= periodEnd.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Posting.Status == status.Value);
        }

        var debits = await ((IMartenQueryable<JournalEntry>)query)
            .Where(x => x.Posting.Type == EntryType.Debit)
            .SumAsync(x => x.Posting.Amount, ct);

        var credits = await ((IMartenQueryable<JournalEntry>)query)
            .Where(x => x.Posting.Type == EntryType.Credit)
            .SumAsync(x => x.Posting.Amount, ct);

        var count = await ((IMartenQueryable<JournalEntry>)query)
            .CountAsync(ct);

        return new PeriodTotals(debits, credits, count);
    }

    /// <summary>
    /// Returns aggregated debit, credit, and entry totals for each currency.
    /// </summary>
    /// <remarks>
    /// Each currency is aggregated separately using server-side queries because
    /// Marten does not support the required grouping projection for this query.
    /// </remarks>
    /// <param name="periodEnd"> Optional point in time up to which journal entries are included. </param>
    /// <param name="status"> Optional entry status used to filter the results. Defaults to cleared entries. </param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns> Collection containing aggregated totals for currencies with recorded entries. </returns>
    public async Task<IReadOnlyList<CurrencyTotals>> GetGlobalTotalsPerCurrencyAsync(
        DateTimeOffset? periodEnd = null,
        EntryStatus? status = EntryStatus.Cleared,
        CancellationToken ct = default)
    {
        IQueryable<JournalEntry> baseQuery = session.Query<JournalEntry>();

        if (periodEnd.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Timestamp <= periodEnd.Value);
        }

        if (status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Posting.Status == status.Value);
        }

        var currencies = Enum.GetValues<Currency>()
            .Where(c => c != Currency.Unspecified)
            .ToList();

        var results = new List<CurrencyTotals>();

        foreach (var currency in currencies)
        {
            var currencyQuery = baseQuery
                .Where(x => x.Posting.Currency == currency);

            var debits = await ((IMartenQueryable<JournalEntry>)currencyQuery)
                .Where(x => x.Posting.Type == EntryType.Debit)
                .SumAsync(x => x.Posting.Amount, ct);

            var credits = await ((IMartenQueryable<JournalEntry>)currencyQuery)
                .Where(x => x.Posting.Type == EntryType.Credit)
                .SumAsync(x => x.Posting.Amount, ct);

            var count = await ((IMartenQueryable<JournalEntry>)currencyQuery)
                .CountAsync(ct);

            if (count > 0)
            {
                results.Add(new CurrencyTotals(currency, debits, credits, count));
            }
        }

        return results;
    }
}