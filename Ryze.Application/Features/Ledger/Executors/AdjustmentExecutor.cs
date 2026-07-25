using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Adjustment;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes ledger adjustment operations through the mutation pipeline
/// and commits the resulting ledger transaction.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="LedgerAdjustmentContext"/>, creates a system
/// mutation context in commit mode, executes the adjustment mutation, and passes
/// the resulting transaction state to the ledger transaction executor for persistence.
/// </remarks>
public sealed class AdjustmentExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<LedgerAdjustmentContext> contextAccessor) : IAdjustmentExecutor
{
    /// <summary>
    /// Executes the active ledger adjustment operation and commits the resulting transaction.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The committed ledger transaction created by the adjustment operation.</returns>
    public async Task<LedgerTransaction> AdjustmentAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current ??
            throw new InvalidOperationException("LedgerAdjustmentContext is not active.");

        var mutationContext = MutationContext.System(
            context.InitiatedBy,
            context.Reason ?? "Adjustment") with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new AdjustmentMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Adjustment failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}