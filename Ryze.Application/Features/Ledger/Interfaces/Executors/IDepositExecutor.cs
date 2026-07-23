using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for recording ledger deposits.
/// </summary>
/// <remarks>
/// Provides application layer commands for recording deposits
/// into ledger accounts as balanced ledger transactions.
/// </remarks>
public interface IDepositExecutor
{
    /// <summary>
    /// Records a deposit into a ledger account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> DepositAsync(
        CancellationToken ct = default);
}