namespace Ryze.Application.Features.Ledger.DTO.Balance;

/// <summary>
/// Represents dual balance snapshot for ledger account.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of an account balance
/// separated into finalized and pending financial states.
///
/// Contains committed funds, pending transaction impact, available balance,
/// and directional pending amounts required for projected balance analysis.
/// </remarks>
/// <param name="AccountId">Ledger account identifier.</param>
/// <param name="Currency">Currency code associated with the balance snapshot.</param>
/// <param name="CommittedBalance">Finalized balance based on cleared ledger entries.</param>
/// <param name="PendingBalance">Net balance impact from pending ledger entries.</param>
/// <param name="AvailableBalance">Balance currently available for account operations.</param>
/// <param name="PendingIncoming">Total amount of pending incoming ledger entries.</param>
/// <param name="PendingOutgoing">Total amount of pending outgoing ledger entries.</param>
/// <param name="LastUpdatedAt">Timestamp when the balance snapshot was last updated.</param>
public sealed record DualBalanceDto(
    string AccountId,
    string Currency,
    decimal CommittedBalance,
    decimal PendingBalance,
    decimal AvailableBalance,
    decimal PendingIncoming,
    decimal PendingOutgoing,
    DateTimeOffset LastUpdatedAt
);