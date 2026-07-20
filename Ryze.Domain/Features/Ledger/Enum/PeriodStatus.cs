namespace Ryze.Domain.Features.Ledger.Enum;

/// <summary>
/// Represents the lifecycle state of an accounting period.
/// </summary>
/// <remarks>
/// Defines the allowed progression of a ledger accounting period:
/// <c>Open → Closing → Closed → Locked</c>.
///
/// The period status controls whether new journal entries can be recorded
/// and what level of authorization is required for financial adjustments.
/// </remarks>
public enum PeriodStatus
{
    /// <summary>
    /// Indicates that the accounting period is active and accepts new entries.
    /// </summary>
    /// <remarks>
    /// Normal operating state where transactions can be posted,
    /// balances can be updated, and regular ledger operations are allowed.
    /// </remarks>
    Open = 0,

    /// <summary>
    /// Indicates that the accounting period is undergoing reconciliation.
    /// </summary>
    /// <remarks>
    /// New entries are restricted and typically require explicit approval.
    /// This phase is used to verify balances, resolve discrepancies,
    /// and prepare the period for final closure.
    /// </remarks>
    Closing = 1,

    /// <summary>
    /// Indicates that the accounting period has been finalized.
    /// </summary>
    /// <remarks>
    /// The period has completed reconciliation and no longer accepts
    /// regular journal entries.
    ///
    /// Additional postings require a controlled override workflow.
    /// </remarks>
    Closed = 2,

    /// <summary>
    /// Indicates that the accounting period is permanently sealed.
    /// </summary>
    /// <remarks>
    /// Locked periods cannot receive any new ledger entries or modifications.
    ///
    /// This state is typically applied after audit completion
    /// or regulatory financial approval.
    /// </remarks>
    Locked = 3
}