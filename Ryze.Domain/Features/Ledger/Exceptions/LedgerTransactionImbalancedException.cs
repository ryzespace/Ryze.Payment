using Ryze.Domain.Shared.Enum;

namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Represents ledger transaction that violates double entry accounting balance rules.
/// </summary>
/// <remarks>
/// This exception is thrown when the total debit amount does not match the total
/// credit amount for given currency.
///
/// Double-entry accounting requires every transaction to remain balanced within
/// the same currency unless explicitly handled by supported multi-currency
/// mechanisms such as foreign exchange accounts.
/// </remarks>
public sealed class LedgerTransactionImbalancedException(
    Currency currency,
    decimal debits,
    decimal credits)
    : LedgerException(
        $"Transaction for currency {currency} is imbalanced. Debits: {debits}, Credits: {credits}",
        "LEDGER_IMBALANCED")
{
    /// <summary>
    /// Gets the total debit amount recorded for the affected currency.
    /// </summary>
    /// <remarks>
    /// Represents the sum of all debit postings that participated in the
    /// failed ledger validation.
    /// </remarks>
    public decimal TotalDebits { get; } = debits;

    /// <summary>
    /// Gets the total credit amount recorded for the affected currency.
    /// </summary>
    /// <remarks>
    /// Represents the sum of all credit postings that participated in the
    /// failed ledger validation.
    /// </remarks>
    public decimal TotalCredits { get; } = credits;

    /// <summary>
    /// Gets the currency in which the imbalance was detected.
    /// </summary>
    /// <remarks>
    /// Ledger validation is performed per currency to prevent invalid
    /// cross currency balancing.
    /// </remarks>
    public Currency Currency { get; } = currency;
}