using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for recording ledger balance adjustments.
/// </summary>
/// <remarks>
/// Provides application layer commands for creating adjustment
/// transactions used to reconcile or correct ledger balances.
/// </remarks>
public interface IAdjustmentExecutor
{
    /// <summary>
    /// Records a balance adjustment for a ledger account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> AdjustmentAsync(
        CancellationToken ct = default);
}