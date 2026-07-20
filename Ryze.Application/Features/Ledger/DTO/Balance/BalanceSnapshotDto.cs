namespace Ryze.Application.Features.Ledger.DTO.Balance;

/// <summary>
/// Represents balance snapshot for ledger account.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of the current
/// financial state of a ledger account.
///
/// Contains the total account balance, the amount available for use,
/// pending transaction impact, and the timestamp of the latest update.
/// </remarks>
/// <param name="AccountId">Ledger account identifier.</param>
/// <param name="Currency">Currency code associated with the balance snapshot.</param>
/// <param name="CurrentBalance">Current finalized balance of the ledger account.</param>
/// <param name="AvailableBalance">Balance currently available for account operations.</param>
/// <param name="PendingBalance">Balance impact from pending ledger entries.</param>
/// <param name="LastUpdatedAt">Timestamp when the balance snapshot was last updated.</param>
public sealed record BalanceSnapshotDto(
    string AccountId,
    string Currency,
    decimal CurrentBalance,
    decimal AvailableBalance,
    decimal PendingBalance,
    DateTimeOffset LastUpdatedAt
);