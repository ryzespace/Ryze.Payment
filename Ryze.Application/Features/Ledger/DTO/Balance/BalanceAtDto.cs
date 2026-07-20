namespace Ryze.Application.Features.Ledger.DTO.Balance;

/// <summary>
/// Represents historical ledger balance snapshot for an account.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of an account balance
/// calculated at a specific point in time.
///
/// The snapshot includes the account identity, currency context, calculated
/// balance value, number of contributing ledger entries, and the timestamp
/// used for historical reconstruction.
/// </remarks>
/// <param name="AccountId">Ledger account identifier.</param>
/// <param name="Currency">Currency code associated with the balance.</param>
/// <param name="Balance">Calculated account balance at the specified point in time.</param>
/// <param name="EntryCount">Number of ledger entries included in the balance calculation.</param>
/// <param name="AsOf">Timestamp representing the point in ledger history for the snapshot.</param>
public sealed record BalanceAtDto(
    string AccountId,
    string Currency,
    decimal Balance,
    int EntryCount,
    DateTimeOffset AsOf
);