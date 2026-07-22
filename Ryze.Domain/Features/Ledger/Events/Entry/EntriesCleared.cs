namespace Ryze.Domain.Features.Ledger.Events.Entry;

/// <summary>
/// Represents domain event raised when one or more pending ledger journal entries
/// have been successfully cleared and finalized.
/// </summary>
/// <param name="EntryIds">The unique identifiers of the journal entries that were cleared. </param>
/// <param name="Timestamp">The UTC timestamp when the entries were cleared. </param>
public sealed record EntriesCleared(
    IReadOnlyList<Guid> EntryIds,
    DateTimeOffset Timestamp
);