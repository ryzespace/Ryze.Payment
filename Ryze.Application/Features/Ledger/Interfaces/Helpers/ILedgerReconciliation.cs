namespace Ryze.Application.Features.Ledger.Interfaces.Helpers;

/// <summary>
/// Defines operations for validating ledger integrity and performing reconciliation checks.
/// </summary>
/// <remarks>
/// Provides application layer reconciliation operations for verifying
/// double entry accounting consistency, recalculating account balances,
/// and validating ledger state during accounting period closure.
/// </remarks>
public interface ILedgerReconciliation
{
    /// <summary>
    /// Reconciles an individual ledger account by recalculating its balance
    /// from the underlying transaction history.
    /// </summary>
    /// <param name="accountId">Ledger account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// True when the calculated account balance matches the recorded ledger state;
    /// otherwise false.
    /// </returns>
    Task<bool> ReconcileAccountAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Performs reconciliation checks required for closing an accounting period.
    /// </summary>
    /// <param name="periodEnd">End timestamp of the accounting period.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// True when all ledger accounts are balanced and the period is ready to close;
    /// otherwise false.
    /// </returns>
    Task<bool> ClosePeriodAsync(
        DateTimeOffset periodEnd,
        CancellationToken ct = default);
}
