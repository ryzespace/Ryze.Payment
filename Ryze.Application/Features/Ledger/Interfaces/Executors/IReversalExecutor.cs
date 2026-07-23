using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for reversing ledger transactions.
/// </summary>
/// <remarks>
/// Provides application layer commands for creating
/// compensating transactions that reverse the financial
/// effect of previously recorded ledger transactions
/// without modifying immutable ledger history.
/// </remarks>
public interface IReversalExecutor
{
    /// <summary>
    /// Records a reversal transaction.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded reversal transaction.</returns>
    Task<LedgerTransaction> ReverseTransactionAsync(
        CancellationToken ct = default);
}