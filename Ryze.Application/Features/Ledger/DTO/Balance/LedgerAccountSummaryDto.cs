namespace Ryze.Application.Features.Ledger.DTO.Balance;

/// <summary>
/// Represents read side snapshot of ledger account.
/// </summary>
/// <remarks>
/// Contains projected balances, debit and credit totals, pending impacts,
/// entry statistics, and account locking information used by reporting
/// and query operations.
/// </remarks>
public sealed class LedgerAccountSummaryDto
{
    /// <summary>
    /// Ledger account identifier.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Currency code associated with the ledger account.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Current projected balance of the ledger account.
    /// </summary>
    public decimal CurrentBalance { get; init; }

    /// <summary>
    /// Balance available after applying pending ledger impacts.
    /// </summary>
    public decimal AvailableBalance { get; init; }

    /// <summary>
    /// Total debit amount recorded for the account.
    /// </summary>
    public decimal TotalDebits { get; init; }

    /// <summary>
    /// Total credit amount recorded for the account.
    /// </summary>
    public decimal TotalCredits { get; init; }

    /// <summary>
    /// Total debit amount from pending entries not yet cleared.
    /// </summary>
    public decimal PendingDebits { get; init; }

    /// <summary>
    /// Total credit amount from pending entries not yet cleared.
    /// </summary>
    public decimal PendingCredits { get; init; }

    /// <summary>
    /// Number of ledger entries associated with the account.
    /// </summary>
    public int EntryCount { get; init; }

    /// <summary>
    /// Indicates whether the account is currently locked for write operations.
    /// </summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// Timestamp of the latest projected balance update.
    /// </summary>
    public DateTimeOffset? LastUpdatedAt { get; init; }
}