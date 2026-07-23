using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Interfaces.Helpers;

/// <summary>
/// Defines operations for calculating ledger balances and entry effects.
/// </summary>
/// <remarks>
/// Provides application-layer helper methods for determining account
/// balances, evaluating the impact of ledger entries, and resolving
/// account metadata required for accounting calculations.
/// </remarks>
public interface IBalanceCalculator
{
    /// <summary>
    /// Calculates the current balance of a ledger account.
    /// </summary>
    /// <param name="totalDebits">Total debit amount recorded for the account.</param>
    /// <param name="totalCredits">Total credit amount recorded for the account.</param>
    /// <param name="category">Account category that determines the normal balance.</param>
    /// <returns>The calculated account balance.</returns>
    decimal CalculateBalance(
        decimal totalDebits,
        decimal totalCredits,
        AccountCategory category);

    /// <summary>
    /// Calculates the balance impact of a ledger entry.
    /// </summary>
    /// <param name="entryType">Type of ledger entry.</param>
    /// <param name="amount">Entry amount.</param>
    /// <param name="category">Account category that determines the normal balance.</param>
    /// <returns>The calculated balance impact.</returns>
    decimal CalculateEntryImpact(
        EntryType entryType,
        decimal amount,
        AccountCategory category);

    /// <summary>
    /// Determines the normal balance type for an account category.
    /// </summary>
    /// <param name="category">Account category.</param>
    /// <returns>The normal balance entry type.</returns>
    EntryType GetNormalBalance(
        AccountCategory category);

    /// <summary>
    /// Infers the account category from a ledger account identifier.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <returns>The inferred account category.</returns>
    AccountCategory InferCategory(
        string accountId);
}
