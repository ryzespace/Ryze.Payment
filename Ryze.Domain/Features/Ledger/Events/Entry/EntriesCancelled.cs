namespace Ryze.Domain.Features.Ledger.Events.Entry;

/// <summary>
/// Represents domain event raised when one or more ledger journal entries
/// have been canceled.
/// </summary>
/// <param name="EntryIds">The unique identifiers of the journal entries that were canceled. </param>
/// <param name="Timestamp">The UTC timestamp when the entries were canceled. </param>
public sealed record EntriesCancelled(
    IReadOnlyList<Guid> EntryIds,
    DateTimeOffset Timestamp
);