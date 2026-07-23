using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines operations for recording ledger fees.
/// </summary>
/// <remarks>
/// Provides application layer commands for recording fee
/// charges against ledger accounts while maintaining
/// accounting consistency.
/// </remarks>
public interface IFeeExecutor
{
    /// <summary>
    /// Records a fee charge on a ledger account.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recorded ledger transaction.</returns>
    Task<LedgerTransaction> FeeAsync(
        CancellationToken ct = default);
}