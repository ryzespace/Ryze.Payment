using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Transfer;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes internal ledger account transfer operations through the mutation and transaction execution pipeline.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="LedgerInternalTransferContext"/>, creates a system mutation context
/// in commit mode, executes the transfer mutation, and delegates the resulting transaction
/// to the ledger transaction executor for persistence and event processing.
/// </remarks>
public sealed class TransferExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<LedgerInternalTransferContext> contextAccessor) : ITransferExecutor
{
    /// <summary>
    /// Executes an internal transfer between ledger accounts using the currently active transfer context.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The resulting ledger transfer transaction.</returns> 
    public async Task<LedgerTransaction> TransferAsync(
        CancellationToken ct = default)
    {
        var context = contextAccessor.Current ?? throw new InvalidOperationException(
            "LedgerInternalTransferContext is not active.");

        var mutationContext = MutationContext.System(context.InitiatedBy, context.Reason ?? "Transfer") with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new TransferMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Transfer failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}