using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Interfaces;
using Ryze.Domain.Features.Ledger.ValueObject;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Domain.Features.Ledger.Helpers;

/// <summary>
/// Validates the economic consistency of multi-currency ledger transactions.
/// </summary>
/// <remarks>
/// This validator complements <see cref="LedgerTransaction.Validate"/>.
///
/// While <see cref="LedgerTransaction.Validate"/> verifies structural
/// accounting rules such as balanced postings, debit and credit presence,
/// and required ledger accounts, this validator ensures that foreign
/// exchange calculations are economically correct.
///
/// Exchange rates are represented as directed graph, allowing indirect
/// conversions through intermediate currencies when no direct exchange
/// rate exists. All journal entries are converted into a common base
/// currency before debit and credit totals are compared.
/// </remarks>
public sealed class ExchangeRateValidator : IForeignExchangeValidator
{
    /// <summary>
    /// Validates the economic consistency of a multi-currency ledger transaction.
    /// </summary>
    /// <remarks>
    /// The validator converts all journal entries into a common base currency
    /// using the supplied exchange rates and verifies that debit and credit
    /// totals remain balanced within the configured tolerance.
    /// </remarks>
    /// <param name="transaction">Ledger transaction to validate.</param>
    /// <param name="exchangeRates">Exchange rates available for the currencies involved in the transaction. </param>
    /// <returns>
    /// Validation result containing the overall status and any detected
    /// foreign exchange validation errors.
    /// </returns>
    public ForeignExchangeValidationResult Validate(
        LedgerTransaction transaction,
        IReadOnlyList<ExchangeRate> exchangeRates)
    {
        if (exchangeRates.Count == 0)
        {
            return new ForeignExchangeValidationResult
            {
                IsValid = true,
                Errors = []
            };
        }

        foreach (var rate in exchangeRates)
        {
            rate.Validate();
        }

        var graph = BuildRateGraph(exchangeRates);

        var baseCurrency = exchangeRates.First().BaseCurrency;

        var errors = ValidateDebitCreditBalance(
            transaction,
            baseCurrency,
            graph,
            exchangeRates);

        return new ForeignExchangeValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// Builds bidirectional exchange rate graph.
    /// </summary>
    /// <remarks>
    /// Every exchange rate creates two directed edges:
    /// The resulting graph enables indirect currency conversion through
    /// intermediate currencies when a direct conversion is unavailable.
    /// </remarks>
    /// <param name="rates">Exchange rates used to construct the graph. </param>
    /// <returns>
    /// A directed graph containing conversion rates between currencies.
    /// </returns>
    private static Dictionary<Currency, Dictionary<Currency, decimal>> BuildRateGraph(
        IReadOnlyList<ExchangeRate> rates)
    {
        var graph = new Dictionary<Currency, Dictionary<Currency, decimal>>();

        foreach (var rate in rates)
        {
            if (!graph.ContainsKey(rate.BaseCurrency))
                graph[rate.BaseCurrency] = new Dictionary<Currency, decimal>();

            if (!graph.ContainsKey(rate.QuoteCurrency))
                graph[rate.QuoteCurrency] = new Dictionary<Currency, decimal>();

            graph[rate.BaseCurrency][rate.QuoteCurrency] = rate.Rate;
            graph[rate.QuoteCurrency][rate.BaseCurrency] = 1m / rate.Rate;
        }

        return graph;
    }

    /// <summary>
    /// Attempts to resolve conversion rate between two currencies.
    /// </summary>
    /// <remarks>
    /// Uses breadth first search over the exchange rate graph to
    /// locate the shortest available conversion path.
    ///
    /// The effective conversion rate is calculated by multiplying the
    /// exchange rates along the discovered path.
    /// </remarks>
    /// <param name="from">Source currency. </param>
    /// <param name="to">Destination currency. </param>
    /// <param name="graph">Exchangerate graph. </param>
    /// <param name="conversionRate">Effective conversion rate from the source currency to the destination currency when a path exists.</param>
    /// <returns>
    /// <see langword="true"/> if a conversion path exists; otherwise
    /// <see langword="false"/>.
    /// </returns>
    private static bool TryGetConversionRate(
        Currency from,
        Currency to,
        Dictionary<Currency, Dictionary<Currency, decimal>> graph,
        out decimal conversionRate)
    {
        conversionRate = 0m;

        if (from == to)
        {
            conversionRate = 1m;
            return true;
        }

        if (!graph.ContainsKey(from))
        {
            return false;
        }

        var visited = new HashSet<Currency> { from };
        var queue = new Queue<(Currency Currency, decimal Rate)>();

        queue.Enqueue((from, 1m));

        while (queue.Count > 0)
        {
            var (current, currentRate) = queue.Dequeue();

            if (!graph.TryGetValue(current, out var neighbors))
            {
                continue;
            }

            foreach (var (neighbor, edgeRate) in neighbors)
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }

                var newRate = currentRate * edgeRate;

                if (neighbor == to)
                {
                    conversionRate = newRate;
                    return true;
                }

                visited.Add(neighbor);
                queue.Enqueue((neighbor, newRate));
            }
        }

        return false;
    }

    /// <summary>
    /// Validates debit and credit totals after currency conversion.
    /// </summary>
    /// <param name="transaction">Ledger transaction being validated.</param>
    /// <param name="baseCurrency">Currency used as the common comparison unit.</param>
    /// <param name="graph">Exchange rate graph.</param>
    /// <param name="exchangeRates">Exchange rates used to determine acceptable tolerance.</param>
    /// <returns>
    /// Collection of detected foreign exchange validation errors.
    /// An empty collection indicates successful validation.
    /// </returns>
    private static List<ForeignExchangeError> ValidateDebitCreditBalance(
        LedgerTransaction transaction,
        Currency baseCurrency,
        Dictionary<Currency, Dictionary<Currency, decimal>> graph,
        IReadOnlyList<ExchangeRate> exchangeRates)
    {
        var errors = new List<ForeignExchangeError>();

        decimal debitTotal = 0m;
        decimal creditTotal = 0m;

        foreach (var entry in transaction.Entries)
        {
            if (!TryGetConversionRate(
                entry.Posting.Currency,
                baseCurrency,
                graph,
                out var rate))
            {
                errors.Add(new ForeignExchangeError
                {
                    Currency = entry.Posting.Currency.ToString(),
                    Message =
                        $"No conversion path from {entry.Posting.Currency} to base currency {baseCurrency}."
                });

                continue;
            }

            var amountInBase = entry.Posting.Amount * rate;

            if (entry.Posting.Type == EntryType.Debit)
            {
                debitTotal += amountInBase;
            }
            else
            {
                creditTotal += amountInBase;
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        var tolerancePercent = exchangeRates
            .FirstOrDefault(r =>
                r.BaseCurrency == baseCurrency ||
                r.QuoteCurrency == baseCurrency)
            ?.TolerancePercent
             ?? 1m;

        var difference = Math.Abs(debitTotal - creditTotal);

        if (debitTotal > 0)
        {
            var deviationPercent = difference / debitTotal * 100m;

            if (deviationPercent > tolerancePercent)
            {
                errors.Add(new ForeignExchangeError
                {
                    Currency = baseCurrency.ToString(),
                    Message =
                        $"Debit ({debitTotal:F2} {baseCurrency}) does not equal Credit ({creditTotal:F2} {baseCurrency}). " +
                        $"Deviation {deviationPercent:F2}% exceeds tolerance {tolerancePercent:F2}%."
                });
            }
        }

        return errors;
    }
}
