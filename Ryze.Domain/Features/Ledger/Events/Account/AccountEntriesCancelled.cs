namespace Ryze.Domain.Features.Ledger.Events.Account;

/// <summary>
/// Raised when all pending or cancellable journal entries associated with
/// a ledger account have been canceled.
/// </summary>
/// <param name="AccountId">The unique identifier of the ledger account whose entries were canceled. </param>
/// <param name="EntryIds">The unique identifiers of the journal entries that were canceled. </param>
/// <param name="Timestamp">The UTC timestamp at which the cancellation operation was completed. </param>
public sealed record AccountEntriesCancelled(
    string AccountId,
    IReadOnlyList<Guid> EntryIds,
    DateTimeOffset Timestamp
);
