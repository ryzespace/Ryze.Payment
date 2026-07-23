using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for recording ledger withdrawals.
/// </summary>
/// <remarks>
/// Provides application layer commands for recording
/// withdrawals from ledger accounts to external destinations.
/// </remarks>
public interface IWithdrawalExecutor
{
    /// <summary>
    /// Records a withdrawal from a ledger account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> WithdrawalAsync(
        CancellationToken ct = default);
}