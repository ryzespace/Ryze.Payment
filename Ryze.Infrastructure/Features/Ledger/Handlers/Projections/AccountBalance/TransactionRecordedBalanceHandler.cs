using Marten;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Events.Transaction;

namespace Ryze.Infrastructure.Features.Ledger.Handlers.Projections.AccountBalance;

/// <summary>
/// Handles ledger transaction recorded events by applying their balance impact
/// to the corresponding account balance projections.
/// </summary>
/// <remarks>
/// Loads or creates each affected account balance projection and applies the
/// journal entry impact according to its current entry status.
/// </remarks>
public static class TransactionRecordedBalanceHandler
{
    /// <summary>
    /// Handles ledger transaction recorded event.
    /// </summary>
    /// <param name="event">The event containing the recorded journal entries.</param>
    /// <param name="session">The Marten document session used to load and persist account balances.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    public static async Task Handle(
        LedgerTransactionRecorded @event,
        IDocumentSession session)
    {
        foreach (var entry in @event.Entries)
        {
            var balanceDoc = await LedgerAccountBalanceHelper.LoadOrCreate(
                session,
                entry.Account.AccountId,
                entry.Posting.Currency,
                @event.Timestamp);

            switch (entry.Posting.Status)
            {
                case EntryStatus.Cleared:
                    LedgerAccountBalanceHelper.ApplyClearedEntry(balanceDoc, entry);
                    break;
                case EntryStatus.Pending:
                    LedgerAccountBalanceHelper.ApplyPendingDebit(balanceDoc, entry);
                    break;
            }

            balanceDoc.LastUpdatedAt = @event.Timestamp;
            session.Store(balanceDoc);
        }

        await session.SaveChangesAsync();
    }
}