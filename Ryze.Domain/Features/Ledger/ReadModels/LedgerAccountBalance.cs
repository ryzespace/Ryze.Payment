using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.ReadModels;

/// <summary>
/// Represents read optimized projection of ledger transactions
/// used to retrieve the current account balance state.
/// </summary>
/// <remarks>
/// This model is not the source of truth for accounting data.
/// It is a calculated projection built from journal entries
/// and maintained separately for fast balance queries.
///
/// Balance calculations respect the account normal accounting balance
/// direction (Debit normal or Credit normal).
/// </remarks>
public sealed class LedgerAccountBalance
{
    /// <summary>
    /// Unique identifier of the ledger account.
    /// </summary>
    /// <example>wallet:1234:USD</example>
    /// <example>liability:platform:aggregates</example>
    public required string Id { get; init; }

    /// <summary>
    /// Current settled balance calculated from cleared journal entries.
    /// </summary>
    /// <remarks>
    /// Only entries with <see cref="EntryStatus.Cleared"/> status
    /// contribute to this value.
    /// </remarks>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Total pending debit amount currently reserved on the account.
    /// </summary>
    /// <remarks>
    /// Pending debit entries represent expected decreases
    /// that have not yet been finalized.
    /// </remarks>
    public decimal PendingDebits { get; set; }

    /// <summary>
    /// Total pending credit amount currently reserved on the account.
    /// </summary>
    /// <remarks>
    /// Pending credit entries represent expected increases
    /// that have not yet been finalized.
    /// </remarks>
    public decimal PendingCredits { get; set; }

    /// <summary>
    /// Calculates the projected pending balance impact.
    /// </summary>
    public decimal PendingBalance => PendingCredits - PendingDebits;

    /// <summary>
    /// Calculates the available account balance including pending entries.
    /// </summary>
    /// <param name="normalBalance">
    /// Defines the natural balance direction of the account.
    /// </param>
    /// <returns>
    /// The available balance adjusted according to accounting rules.
    /// </returns>
    /// <remarks>
    /// Asset and expense accounts are normally debit-balanced.
    /// Liability, equity, and revenue accounts are normally credit-balanced.
    /// </remarks>
    public decimal GetAvailableBalance(EntryType normalBalance)
        => normalBalance == EntryType.Debit
            ? CurrentBalance + PendingDebits - PendingCredits
            : CurrentBalance - PendingDebits + PendingCredits;

    /// <summary>
    /// Total debit amount posted to this account over its lifetime.
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Total credit amount posted to this account over its lifetime.
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Currency associated with this account balance projection.
    /// </summary>
    /// <example>USD</example>
    public required string Currency { get; init; }

    /// <summary>
    /// Timestamp of the last projection update.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}