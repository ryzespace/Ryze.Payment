using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Interfaces.Executors;

/// <summary>
/// Defines execution operations for ledger transactions.
/// </summary>
/// <remarks>
/// Provides application layer infrastructure coordination for executing
/// ledger transaction workflows while handling technical concerns such as
/// distributed locking, idempotency validation, audit-compliant sequence
/// numbering, and transaction lifecycle management.
///
/// Implementations are expected to preserve ledger consistency and ensure
/// that transaction execution remains safe under concurrent processing.
/// </remarks>
public interface ILedgerTransactionExecutor
{
    /// <summary>
    /// Executes the complete lifecycle of recording a ledger transaction.
    /// </summary>
    /// <param name="transaction">Ledger transaction to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted ledger transaction.</returns>
    Task<LedgerTransaction> ExecuteAsync(
        LedgerTransaction transaction,
        CancellationToken ct = default);

    /// <summary>
    /// Appends a clearing event to a previously recorded transaction.
    /// </summary>
    /// <param name="transactionId">Identifier of the transaction to clear.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync(
        Guid transactionId,
        CancellationToken ct = default);
}