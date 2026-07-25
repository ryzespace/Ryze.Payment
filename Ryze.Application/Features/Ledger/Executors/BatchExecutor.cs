using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Batch;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes batch ledger operations through the mutation pipeline
/// and commits the resulting ledger transaction.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="BatchLedgerContext"/>, creates a system
/// mutation context in commit mode, executes the batch mutation, and passes
/// the resulting transaction state to the ledger transaction executor for persistence.
/// </remarks>
public sealed class BatchExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<BatchLedgerContext> contextAccessor) : IBatchExecutor
{
    /// <summary>
    /// Executes the active batch ledger operation and commits the resulting transaction.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The committed ledger transaction created by the batch operation.</returns>
    public async Task<LedgerTransaction> BatchAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current ??
            throw new InvalidOperationException("BatchLedgerContext is not active.");

        var mutationContext = MutationContext.System(
            context.InitiatedBy,
            context.Description ?? "Batch") with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new BatchMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Batch failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}