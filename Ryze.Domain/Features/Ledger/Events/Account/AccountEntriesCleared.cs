namespace Ryze.Domain.Features.Ledger.Events.Account;

/// <summary>
/// Raised when all pending journal entries associated with ledger account
/// have been successfully cleared and settled.
/// </summary>
/// <param name="AccountId">The unique identifier of the ledger account whose entries were cleared. </param>
/// <param name="EntryIds">The unique identifiers of the journal entries that were cleared. </param>
/// <param name="Timestamp">The UTC timestamp at which the clearing operation was completed. </param>
public sealed record AccountEntriesCleared(
    string AccountId,
    IReadOnlyList<Guid> EntryIds,
    DateTimeOffset Timestamp
);
