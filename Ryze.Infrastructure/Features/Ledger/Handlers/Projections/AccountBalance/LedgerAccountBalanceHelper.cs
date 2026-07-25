using Marten;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.ReadModels;
using Ryze.Domain.Shared.Enum;
using Ryze.Infrastructure.Features.Ledger.Projections;

namespace Ryze.Infrastructure.Features.Ledger.Handlers.Projections.AccountBalance;

/// <summary>
/// Provides helper operations for loading, creating, and updating ledger account balance projections.
/// </summary>
/// <remarks>
/// Centralizes balance projection updates for cleared and pending journal entries,
/// including applying and reverting pending debit and credit amounts.
/// </remarks>
public static class LedgerAccountBalanceHelper
{
    /// <summary>
    /// Loads the balance projection for the specified account or creates a new projection when none exists.
    /// </summary>
    /// <param name="session">The Marten document session used to load the balance projection.</param>
    /// <param name="accountId">The unique identifier of the ledger account.</param>
    /// <param name="currency">The currency associated with the account balance.</param>
    /// <param name="timestamp">The timestamp assigned to a newly created balance projection.</param>
    /// <returns>
    /// The existing account balance projection, or new projection initialized with zero balances.
    /// </returns>
    public static async Task<LedgerAccountBalance> LoadOrCreate(
        IDocumentSession session,
        string accountId,
        Currency currency,
        DateTimeOffset timestamp)
    {
        var doc = await session.LoadAsync<LedgerAccountBalance>(accountId);

        if (doc is not null)
        {
            return doc;
        }

        doc = new LedgerAccountBalance
        {
            Id = accountId,
            Currency = currency.ToString(),
            CurrentBalance = 0,
            TotalCredits = 0,
            TotalDebits = 0,
            LastUpdatedAt = timestamp
        };

        return doc;
    }

    /// <summary>
    /// Applies the cleared journal entry to the account balance projection.
    /// </summary>
    /// <param name="doc">The account balance projection to update.</param>
    /// <param name="entry">The cleared journal entry to apply.</param>
    public static void ApplyClearedEntry(LedgerAccountBalance doc, JournalEntry entry)
    {
        UpdateTotal(doc, entry.Posting.Type, entry.Posting.Amount);
        doc.CurrentBalance += BalanceHelper.EntryImpact(
            entry.Posting.Type,
            entry.Posting.Amount,
            entry.Account.AccountId);
    }

    /// <summary>
    /// Applies the pending balance impact of a journal entry.
    /// </summary>
    /// <param name="doc">The account balance projection to update.</param>
    /// <param name="entry">The pending journal entry to apply.</param>
    public static void ApplyPendingDebit(LedgerAccountBalance doc, JournalEntry entry) =>
        UpdatePending(doc, entry.Posting.Type, entry.Posting.Amount);

    /// <summary>
    /// Reverts the pending balance impact of journal entry.
    /// </summary>
    /// <param name="doc">The account balance projection to update.</param>
    /// <param name="entry">The pending journal entry to revert.</param>
    public static void RevertPending(LedgerAccountBalance doc, JournalEntry entry) =>
        UpdatePending(doc, entry.Posting.Type, -entry.Posting.Amount);

    /// <summary>
    /// Updates the total debits or credits based on the entry type.
    /// </summary>
    /// <param name="doc">The account balance projection to update.</param>
    /// <param name="type">The type of the entry (Debit or Credit).</param>
    /// <param name="amount">The amount to add (positive) or subtract (negative).</param>
    private static void UpdateTotal(LedgerAccountBalance doc, EntryType type, decimal amount)
    {
        _ = type switch
        {
            EntryType.Debit => doc.TotalDebits += amount,
            EntryType.Credit => doc.TotalCredits += amount,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported entry type.")
        };
    }

    /// <summary>
    /// Updates the pending debits or credits based on the entry type.
    /// </summary>
    /// <param name="doc">The account balance projection to update.</param>
    /// <param name="type">The type of the entry (Debit or Credit).</param>
    /// <param name="amount">The amount to add (positive) or subtract (negative).</param>
    private static void UpdatePending(LedgerAccountBalance doc, EntryType type, decimal amount)
    {
        _ = type switch
        {
            EntryType.Debit => doc.PendingDebits += amount,
            EntryType.Credit => doc.PendingCredits += amount,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported entry type.")
        };
    }
}