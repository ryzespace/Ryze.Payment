using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Transfer;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Transaction;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes ledger transaction reversal operations through the mutation and transaction execution pipeline.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="ReverseTransactionContext"/>, creates system mutation context
/// in commit mode, executes the reversal mutation, and delegates the resulting transaction
/// to ledger transaction executor for persistence and event processing.
/// </remarks>
public sealed class ReversalExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<ReverseTransactionContext> contextAccessor) : IReversalExecutor
{
    /// <summary>
    /// Executes an automated reversal of ledger transaction using the currently active reversal context.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The resulting reversal ledger transaction.</returns>
    public async Task<LedgerTransaction> ReverseTransactionAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current ?? throw new InvalidOperationException(
            "ReverseTransactionContext is not active.");

        var mutationContext = MutationContext.System(context.InitiatedBy, context.Reason) with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new ReverseTransactionMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Reversal failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}