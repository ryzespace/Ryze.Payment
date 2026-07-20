using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Represents an attempt to create ledger posting in an unavailable accounting period.
/// </summary>
/// <remarks>
/// This exception is raised when journal entry is submitted to period that
/// does not allow normal postings, such as closed or permanently locked period.
///
/// Closed periods preserve accounting integrity by preventing historical data
/// modification without following an explicit override or adjustment process.
/// </remarks>
public sealed class LedgerPeriodClosedException(string periodId, PeriodStatus status)
    : LedgerException(
        $"Cannot post entry to period '{periodId}' — period is {status}. " +
        "Entries to closed periods require a special override procedure.",
        "LEDGER_PERIOD_CLOSED")
{
    /// <summary>
    /// Gets the identifier of the accounting period that rejected the posting.
    /// </summary>
    /// <remarks>
    /// Typically represents period key such as <c>2026-01</c>.
    /// </remarks>
    public string PeriodId { get; } = periodId;

    /// <summary>
    /// Gets the lifecycle status of the period at the time of rejection.
    /// </summary>
    /// <remarks>
    /// Determines why the period could not accept new ledger entries.
    /// </remarks>
    public PeriodStatus PeriodStatus { get; } = status;
}