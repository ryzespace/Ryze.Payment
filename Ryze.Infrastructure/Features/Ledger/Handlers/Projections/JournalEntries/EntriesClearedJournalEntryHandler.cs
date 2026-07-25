using Marten;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Events.Entry;

namespace Ryze.Infrastructure.Features.Ledger.Handlers.Projections.JournalEntries;

/// <summary>
/// Handles entry cleared events by marking affected journal entries as cleared.
/// </summary>
/// <remarks>
/// Loads each affected journal entry, updates its posting status to
/// <see cref="EntryStatus.Cleared"/>, and persists the updated entries
/// through the Marten document session.
/// </remarks>
public static class EntriesClearedJournalEntryHandler
{
    /// <summary>
    /// Handles an entries cleared event.
    /// </summary>
    /// <param name="event">The event containing the journal entries to mark as cleared.</param>
    /// <param name="session">The Marten document session used to load and persist journal entries.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    public static async Task Handle(EntriesCleared @event, IDocumentSession session)
    {
        foreach (var entryId in @event.EntryIds)
        {
            var entry = await session.LoadAsync<JournalEntry>(entryId);
            if (entry is null) continue;

            session.Store(
                entry with
                {
                    Posting = entry.Posting with
                    {
                        Status = EntryStatus.Cleared
                    }
                });
        }
        await session.SaveChangesAsync();
    }
}
