namespace Ryze.Domain.Features.Ledger.Enum;

/// <summary>
/// Defines the lifecycle state of ledger entry.
/// </summary>
/// <remarks>
/// Controls how an individual journal entry affects account balances.
/// Pending entries represent reserved funds that are not yet finalized,
/// while cleared entries represent confirmed ledger movements.
/// </remarks>
public enum EntryStatus
{
    /// <summary>
    /// Indicates that the ledger entry has been finalized.
    /// </summary>
    /// <remarks>
    /// Cleared entries are included in committed balances
    /// and represent completed financial movements.
    /// </remarks>
    Cleared = 0,

    /// <summary>
    /// Indicates that the ledger entry represents a pending operation.
    /// </summary>
    /// <remarks>
    /// Pending entries affect reserved or available balance calculations
    /// but have not yet been fully finalized.
    ///
    /// Common examples include payment authorization holds
    /// and reserved funds.
    /// </remarks>
    Pending = 1,

    /// <summary>
    /// Indicates that a previously pending ledger entry was canceled.
    /// </summary>
    /// <remarks>
    /// Voided entries remain part of the audit history
    /// but no longer contribute to active balance calculations.
    /// </remarks>
    Voided = 2
}