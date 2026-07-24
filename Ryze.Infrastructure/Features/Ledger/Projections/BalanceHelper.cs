using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Infrastructure.Features.Ledger.Projections;

/// <summary>
/// Provides balance calculation helpers for ledger projections.
/// </summary>
/// <remarks>
/// Uses configured <see cref="IBalanceCalculator"/> instance to determine
/// the accounting category and balance impact of ledger entries.
/// The calculator must be initialized before invoking projection calculations.
/// </remarks>
public static class BalanceHelper
{
    private static IBalanceCalculator? _calculator;
    
    public static void Initialize(IBalanceCalculator calculator)
    {
        _calculator = calculator;
    }

    /// <summary>
    /// Calculates the balance impact of ledger entry for the specified account.
    /// </summary>
    /// <param name="entryType">The debit or credit type of the ledger entry.</param>
    /// <param name="amount">The monetary amount of the ledger entry.</param>
    /// <param name="accountId">The identifier of the account affected by the entry.</param>
    /// <returns>The signed balance impact of the entry according to the account's normal balance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the balance calculator has not been initialized.
    /// </exception>
    public static decimal EntryImpact(EntryType entryType, decimal amount, string accountId)
    {
        var calc = _calculator ??
                   throw new InvalidOperationException(
                       "BalanceHelper not initialized. Call Initialize() first.");

        var category = calc.InferCategory(accountId);
        return calc.CalculateEntryImpact(entryType, amount, category);
    }
}