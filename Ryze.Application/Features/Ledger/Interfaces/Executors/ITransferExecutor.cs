using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for performing internal ledger transfers.
/// </summary>
/// <remarks>
/// Provides application-layer commands for transferring funds
/// between ledger accounts while preserving double-entry
/// accounting consistency.
/// </remarks>
public interface ITransferExecutor
{
    /// <summary>
    /// Performs an internal transfer between ledger accounts.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> TransferAsync(
        CancellationToken ct = default);
}