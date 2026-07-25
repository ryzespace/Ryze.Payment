using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Deposit;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes ledger deposit operations through the mutation pipeline
/// and commits the resulting ledger transaction.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="LedgerDepositContext"/>, creates a system
/// mutation context in commit mode, executes the deposit mutation, and passes
/// the resulting transaction state to the ledger transaction executor for persistence.
/// </remarks>
public sealed class DepositExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<LedgerDepositContext> contextAccessor) : IDepositExecutor
{
    /// <summary>
    /// Executes the active ledger deposit operation and commits the resulting transaction.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The committed ledger transaction created by the deposit operation.</returns>
    public async Task<LedgerTransaction> DepositAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current ??
            throw new InvalidOperationException("LedgerDepositContext is not active.");

        var mutationContext = MutationContext.System(
            context.InitiatedBy,
            context.Reason ?? "Deposit") with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new DepositMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Deposit failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}