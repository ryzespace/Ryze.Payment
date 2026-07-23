using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Interfaces.Helpers;

/// <summary>
/// Defines operations for managing the lifecycle of accounting periods.
/// </summary>
/// <remarks>
/// Provides application layer operations for validating accounting
/// periods, transitioning their lifecycle state, and retrieving
/// period information used by ledger transactions.
/// </remarks>
public interface ILedgerPeriod
{
    /// <summary>
    /// Ensures that the specified transaction timestamp belongs to an open accounting period.
    /// </summary>
    /// <param name="transactionTimestamp">Timestamp of the transaction to validate.</param>
    /// <param name="hasOverride">Indicates whether period restrictions may be bypassed by an authorized override. </param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Ryze.Domain.Features.Ledger.Exceptions.LedgerPeriodClosedException">
    /// Thrown when the corresponding accounting period is closed or locked and no override is permitted.
    /// </exception>
    Task EnsurePeriodOpenAsync(
        DateTimeOffset transactionTimestamp,
        bool hasOverride = false,
        CancellationToken ct = default);

    /// <summary>
    /// Transitions an accounting period to the specified status.
    /// </summary>
    /// <param name="periodId">Accounting period identifier.</param>
    /// <param name="newStatus">Target period status.</param>
    /// <param name="initiatedBy">Identifier of the actor performing the transition.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LedgerPeriod> TransitionPeriodAsync(
        string periodId,
        PeriodStatus newStatus,
        string initiatedBy,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the accounting period for the specified timestamp.
    /// </summary>
    /// <param name="timestamp">Timestamp used to resolve the accounting period.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<LedgerPeriod> GetOrCreatePeriodAsync(
        DateTimeOffset timestamp,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves all accounting periods.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<LedgerPeriod>> GetAllPeriodsAsync(
        CancellationToken ct = default);
}
