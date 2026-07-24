using Ryze.Application.Features.Ledger.Interfaces.Helpers;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Helpers;

/// <summary>
/// Provides accounting calculations for ledger balances and journal entry impacts.
/// </summary>
/// <remarks>
/// Calculates account balances according to the normal balance of each account
/// category and determines the balance impact of individual debit and credit
/// journal entries.
/// </remarks>
public sealed class BalanceCalculator : IBalanceCalculator
{
    /// <summary>
    /// Calculates the current balance from total debit and credit amounts.
    /// </summary>
    /// <param name="totalDebits">The total debit amount recorded for the account.</param>
    /// <param name="totalCredits">The total credit amount recorded for the account.</param>
    /// <param name="category">The accounting category of the account.</param>
    /// <returns>The calculated account balance based on its normal balance direction.</returns>
    public decimal CalculateBalance(decimal totalDebits, decimal totalCredits, AccountCategory category)
    {
        return GetNormalBalance(category) == EntryType.Debit
            ? totalDebits - totalCredits
            : totalCredits - totalDebits;
    }

    /// <summary>
    /// Calculates the balance impact of single journal entry.
    /// </summary>
    /// <param name="entryType">The debit or credit type of the journal entry.</param>
    /// <param name="amount">The monetary amount of the journal entry.</param>
    /// <param name="category">The accounting category of the affected account.</param>
    /// <returns>The signed balance impact of the entry.</returns>
    public decimal CalculateEntryImpact(EntryType entryType, decimal amount, AccountCategory category)
    {
        var normalBalance = GetNormalBalance(category);
        return entryType == EntryType.Debit
            ? normalBalance == EntryType.Debit ? amount : -amount
            : normalBalance == EntryType.Credit ? amount : -amount;
    }

    /// <summary>
    /// Returns the normal balance direction for an accounting category.
    /// </summary>
    /// <param name="category">The accounting category for which the normal balance is requested.</param>
    /// <returns><see cref="EntryType.Debit"/> for debit normal accounts; otherwise, <see cref="EntryType.Credit"/>.</returns>
    public EntryType GetNormalBalance(AccountCategory category) => category switch
    {
        AccountCategory.Asset or AccountCategory.Expense => EntryType.Debit,
        _ => EntryType.Credit
    };

    /// <summary>
    /// Infers the accounting category from the ledger account identifier.
    /// </summary>
    /// <param name="accountId">The unique ledger account identifier containing the category prefix.</param>
    /// <returns>The inferred <see cref="AccountCategory"/> for the account.</returns>
    public AccountCategory InferCategory(string accountId)
    {
        if (accountId.StartsWith("asset:", StringComparison.OrdinalIgnoreCase)) return AccountCategory.Asset;
        if (accountId.StartsWith("liability:", StringComparison.OrdinalIgnoreCase)) return AccountCategory.Liability;
        if (accountId.StartsWith("equity:", StringComparison.OrdinalIgnoreCase)) return AccountCategory.Equity;
        if (accountId.StartsWith("revenue:", StringComparison.OrdinalIgnoreCase)) return AccountCategory.Revenue;
        if (accountId.StartsWith("expense:", StringComparison.OrdinalIgnoreCase)) return AccountCategory.Expense;
        return AccountCategory.Liability;
    }
}