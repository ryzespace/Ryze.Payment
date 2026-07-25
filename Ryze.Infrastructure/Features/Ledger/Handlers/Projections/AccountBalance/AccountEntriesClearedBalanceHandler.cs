using Marten;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Events.Account;
using Ryze.Domain.Features.Ledger.ReadModels;

namespace Ryze.Infrastructure.Features.Ledger.Handlers.Projections.AccountBalance;

/// <summary>
/// Handles account entry cleared events by applying their cleared balance impact.
/// </summary>
/// <remarks>
/// Loads each affected journal entry and its corresponding account balance projection,
/// applies the cleared balance contribution, updates the projection timestamp, and
/// persists the changes through the Marten document session.
/// </remarks>
public static class AccountEntriesClearedBalanceHandler
{
    /// <summary>
    /// Handles an account entries cleared event.
    /// </summary>
    /// <param name="event">The event containing the journal entries to apply.</param>
    /// <param name="session">The Marten document session used to load and persist account balances.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    public static async Task Handle(
        AccountEntriesCleared @event,
        IDocumentSession session)
    {
        foreach (var entryId in @event.EntryIds)
        {
            var entry = await session.LoadAsync<JournalEntry>(entryId);
            if (entry is null) continue;

            var balanceDoc = await session.LoadAsync<LedgerAccountBalance>(
                entry.Account.AccountId);
            if (balanceDoc is null) continue;

            LedgerAccountBalanceHelper.ApplyClearedEntry(balanceDoc, entry);
            balanceDoc.LastUpdatedAt = @event.Timestamp;

            session.Store(balanceDoc);
        }

        await session.SaveChangesAsync();
    }
}