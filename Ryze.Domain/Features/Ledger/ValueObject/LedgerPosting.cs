using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Domain.Features.Ledger.ValueObject;

/// <summary>
/// Represents the financial posting information of a journal entry.
/// </summary>
/// <remarks>
/// Defines the accounting direction, monetary value, currency,
/// current balance snapshot, lifecycle state, and human-readable description.
///
/// A posting is immutable and forms one side of double entry transaction.
/// </remarks>
public sealed record LedgerPosting
{
    /// <summary>
    /// Defines whether the posting is a debit or credit entry.
    /// </summary>
    public required EntryType Type { get; init; }

    /// <summary>
    /// Monetary amount of the posting.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency of the posted amount.
    /// </summary>
    public required Currency Currency { get; init; }

    /// <summary>
    /// Account balance after applying this posting.
    /// </summary>
    /// <remarks>
    /// Used as historical snapshot for auditing and reconciliation.
    /// </remarks>
    public required decimal RunningBalance { get; init; }

    /// <summary>
    /// Current lifecycle state of the entry.
    /// </summary>
    public required EntryStatus Status { get; init; }

    /// <summary>
    /// Explanation of the posting.
    /// </summary>
    public required string Description { get; init; }
}