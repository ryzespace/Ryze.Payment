using Marten;
using Ryze.Domain.Features.Ledger.Events.Transaction;

namespace Ryze.Infrastructure.Features.Ledger.Handlers.Projections.JournalEntries;

/// <summary>
/// Handles ledger transaction recorded events by storing the associated journal entries.
/// </summary>
/// <remarks>
/// Persists each journal entry contained in the recorded transaction event
/// through the Marten document session.
/// </remarks>
public static class TransactionRecordedJournalEntryHandler
{
    /// <summary>
    /// Handles ledger transaction recorded event.
    /// </summary>
    /// <param name="event">The event containing the journal entries to persist.</param>
    /// <param name="session">The Marten document session used to store journal entries.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    public static async Task Handle(LedgerTransactionRecorded @event, IDocumentSession session)
    {
        foreach (var entry in @event.Entries)
        {
            session.Store(entry);
        }
        await session.SaveChangesAsync();
    }
}
