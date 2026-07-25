using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Withdrawal;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes ledger withdrawal operations through the ledger transaction execution pipeline.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="LedgerWithdrawalContext"/>, builds a withdrawal transaction
/// using <see cref="LedgerOperationBuilder"/>, and delegates the resulting transaction to
/// the ledger transaction executor for persistence and event processing.
/// </remarks>
public sealed class WithdrawalExecutor(
    ILedgerTransactionExecutor executor,
    IContextAccessor<LedgerWithdrawalContext> contextAccessor) : IWithdrawalExecutor
{
    /// <summary>
    /// Records withdrawal from ledger account to an external destination.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The resulting ledger withdrawal transaction.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active <see cref="LedgerWithdrawalContext"/> is available. </exception>
    public async Task<LedgerTransaction> WithdrawalAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current
            ?? throw new InvalidOperationException(
                "LedgerWithdrawalContext is not active.");

        var tx = LedgerOperationBuilder
            .Start(TransactionType.Withdrawal, context.InitiatedBy)
            .WithIdempotencyKey(context.Id)
            .WithReason(context.Reason ?? "Withdrawal")
            .Withdrawal(
                context.AccountId,
                context.Destination,
                context.Amount,
                Enum.Parse<Currency>(context.Currency, true),
                context.Description)
            .Build();

        return await executor.ExecuteAsync(tx, ct);
    }
}