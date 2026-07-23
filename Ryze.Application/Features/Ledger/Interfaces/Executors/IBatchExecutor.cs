using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for recording batch ledger transactions.
/// </summary>
/// <remarks>
/// Provides application layer commands for processing multiple
/// credits and debits atomically as a single balanced
/// ledger transaction.
/// </remarks>
public interface IBatchExecutor
{
    /// <summary>
    /// Records a balanced batch ledger transaction.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> BatchAsync(
        CancellationToken ct = default);
}