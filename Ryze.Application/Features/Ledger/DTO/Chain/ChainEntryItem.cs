namespace Ryze.Application.Features.Ledger.DTO.Chain;

/// <summary>
/// Represents single ledger chain verification item.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of ledger entry
/// during chain validation.
///
/// Contains both recalculated and persisted running balances to verify
/// ledger sequence integrity and detect inconsistencies.
/// </remarks>
/// <param name="EntryId">Unique identifier of the ledger entry.</param>
/// <param name="TransactionId">Identifier of the transaction containing the entry.</param>
/// <param name="Timestamp">Timestamp when the ledger entry was recorded.</param>
/// <param name="Direction">Ledger entry direction, such as debit or credit.</param>
/// <param name="Amount">Monetary amount represented by the ledger entry.</param>
/// <param name="ComputedRunningBalance">
/// Running balance recalculated from the ledger history.
/// </param>
/// <param name="CurrentRunningBalance">
/// Running balance currently stored with the ledger entry.
/// </param>
/// <param name="IsMatch">
/// Indicates whether the computed balance matches the stored balance.
/// </param>
public sealed record ChainEntryItem(
    Guid EntryId,
    Guid TransactionId,
    DateTimeOffset Timestamp,
    string Direction,
    decimal Amount,
    decimal ComputedRunningBalance,
    decimal CurrentRunningBalance,
    bool IsMatch
);