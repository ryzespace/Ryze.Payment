using ModularityKit.Context.Abstractions;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using Ryze.Application.Features.Ledger.Contexts.Fee;
using Ryze.Application.Features.Ledger.Interfaces.Executors;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Executes ledger fee operations through the mutation and transaction execution pipeline.
/// </summary>
/// <remarks>
/// Resolves the active <see cref="LedgerFeeContext"/>, creates system mutation context
/// in commit mode, executes the fee mutation, and delegates the resulting transaction
/// to the ledger transaction executor for persistence and event processing.
/// </remarks>
public sealed class FeeExecutor(
    IMutationEngine engine,
    ILedgerTransactionExecutor executor,
    IContextAccessor<LedgerFeeContext> contextAccessor) : IFeeExecutor
{
    /// <summary>
    /// Executes the fee operation using the currently active ledger fee context.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The resulting ledger transaction.</returns>
    public async Task<LedgerTransaction> FeeAsync(CancellationToken ct = default)
    {
        var context = contextAccessor.Current
            ?? throw new InvalidOperationException("LedgerFeeContext is not active.");

        var mutationContext = MutationContext.System(
            context.InitiatedBy,
            context.Reason ?? "Fee") with
        {
            Mode = MutationMode.Commit
        };

        var mutation = new FeeMutation(context, mutationContext);
        var result = await engine.ExecuteAsync(mutation, null!, ct);

        if (!result.IsSuccess || result.NewState is null)
            throw new InvalidOperationException("Fee failed");

        return await executor.ExecuteAsync(result.NewState, ct);
    }
}