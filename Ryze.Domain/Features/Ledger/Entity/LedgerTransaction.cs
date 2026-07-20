using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Exceptions;

namespace Ryze.Domain.Features.Ledger.Entity;

/// <summary>
/// Represents complete immutable double entry ledger transaction.
/// </summary>
/// <remarks>
/// A ledger transaction groups one or more journal entries that together
/// describe single business operation.
///
/// Every transaction must satisfy fundamental accounting rules:
///
/// Transactions are immutable after creation and uniquely identified by an
/// idempotency key to prevent duplicate processing.
/// </remarks>
public sealed record LedgerTransaction
{
    /// <summary>
    /// Unique identifier of the transaction.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Timestamp when the transaction was created.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Collection of journal entries composing this transaction.
    /// </summary>
    public required IReadOnlyList<JournalEntry> Entries { get; init; }

    /// <summary>
    /// High-level business classification of the transaction.
    /// </summary>
    public required TransactionType Type { get; init; }

    /// <summary>
    /// Unique idempotency key preventing duplicate transaction processing.
    /// </summary>
    public required string IdempotencyKey { get; init; }

    /// <summary>
    /// Identifier of the user or system that initiated the transaction.
    /// </summary>
    public required string InitiatedBy { get; init; }

    /// <summary>
    /// Optional business reason describing why the transaction exists.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Validates the structural integrity of the transaction.
    /// </summary>
    /// <remarks>
    /// Validation ensures that the transaction satisfies the core
    /// invariants required by the ledger before it can be persisted.
    /// </remarks>
    /// <exception cref="LedgerValidationException">
    /// Thrown when one or more accounting rules are violated.
    /// </exception>
    /// <exception cref="LedgerTransactionImbalancedException">
    /// Thrown when debit and credit totals do not balance.
    /// </exception>
    public void Validate()
    {
        ValidateMinimumEntries();
        ValidateEntryDirections();
        ValidateAmounts();
        ValidateCurrencyBalance();
    }

    /// <summary>
    /// Ensures the transaction contains at least two journal entries.
    /// </summary>
    private void ValidateMinimumEntries()
    {
        if (Entries.Count < 2)
        {
            throw new LedgerValidationException(
                "Transaction must contain at least two journal entries.");
        }
    }

    /// <summary>
    /// Ensures that both debit and credit postings are present.
    /// </summary>
    private void ValidateEntryDirections()
    {
        if (Entries.All(e => e.Posting.Type != EntryType.Debit))
        {
            throw new LedgerValidationException(
                "Transaction must contain at least one debit entry.");
        }

        if (Entries.All(e => e.Posting.Type != EntryType.Credit))
        {
            throw new LedgerValidationException(
                "Transaction must contain at least one credit entry.");
        }
    }

    /// <summary>
    /// Ensures that every posting amount is greater than zero.
    /// </summary>
    private void ValidateAmounts()
    {
        if (Entries.Any(e => e.Posting.Amount <= 0))
        {
            throw new LedgerValidationException(
                "Transaction entries must have a positive amount.");
        }
    }

    /// <summary>
    /// Ensures that debit and credit totals balance for every currency.
    /// </summary>
    /// <remarks>
    /// Multi currency transactions are permitted only when the
    /// Currency Exchange control account participates in the transaction.
    /// </remarks>
    private void ValidateCurrencyBalance()
    {
        var currencyGroups = Entries
            .GroupBy(e => e.Posting.Currency)
            .ToList();

        var isMultiCurrency = currencyGroups.Count > 1;

        var hasFxAccount = Entries.Any(e =>
            e.Account.AccountId ==
            ChartsAccounts.ChartOfAccounts.CurrencyExchangeGainLoss.Id);

        foreach (var group in currencyGroups)
        {
            var debitTotal = group
                .Where(e => e.Posting.Type == EntryType.Debit)
                .Sum(e => e.Posting.Amount);

            var creditTotal = group
                .Where(e => e.Posting.Type == EntryType.Credit)
                .Sum(e => e.Posting.Amount);

            if (debitTotal == creditTotal)
            {
                continue;
            }

            if (isMultiCurrency && hasFxAccount)
            {
                continue;
            }

            throw new LedgerTransactionImbalancedException(
                group.Key,
                debitTotal,
                creditTotal);
        }

        if (isMultiCurrency && !hasFxAccount)
        {
            throw new LedgerValidationException(
                "Multi-currency transaction must involve the Currency Exchange account.");
        }
    }
}
