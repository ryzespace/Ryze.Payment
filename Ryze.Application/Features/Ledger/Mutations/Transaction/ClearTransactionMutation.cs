using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Mutations.SideEffects;

namespace Ryze.Application.Features.Ledger.Mutations.Transaction;

/// <summary>
/// Mutation responsible for clearing pending entries of a ledger transaction.
/// </summary>
/// <remarks>
/// Creates side effect that notifies the system that the specified ledger
/// transaction entries have been cleared.
/// </remarks>
/// <param name="transactionId">The ID of the transaction to clear.</param>
/// <param name="entryIds">The IDs of the entries being cleared.</param>
/// <param name="context">The mutation execution context.</param>
public sealed class ClearTransactionMutation(
    Guid transactionId,
    IReadOnlyList<Guid> entryIds,
    MutationContext context)
    : MutationBase<Guid>(
        CreateIntent(
            operationName: "ledger.clear",
            category: "domain.ledger",
            description: "Clear pending entries of a ledger transaction",
            riskLevel: MutationRiskLevel.Medium,
            isReversible: false,
            tags: new HashSet<string>
            {
                "ledger",
                "clear",
                "transaction"
            }),
        context)
{
    /// <summary>
    /// Gets the ID of the transaction being cleared.
    /// </summary>
    public Guid TransactionId { get; } = transactionId;

    /// <summary>
    /// Gets the IDs of the entries being cleared.
    /// </summary>
    public IReadOnlyList<Guid> EntryIds { get; } = entryIds;

    /// <summary>
    /// Applies the clear transaction mutation and creates the corresponding side effect.
    /// </summary>
    /// <returns>
    /// Successful mutation result containing the cleared transaction ID,
    /// state change, and clear transaction side effect.
    /// </returns>
    /// <param name="state">The current transaction state.</param>
    public override MutationResult<Guid> Apply(Guid state)
    {
        var sideEffect = SideEffect.Create(
            type: "LedgerTransactionCleared",
            description: $"Ledger transaction {TransactionId} was cleared.",
            data: new LedgerTransactionClearedSideEffectData
            {
                TransactionId = TransactionId,
                EntryIds = EntryIds,
                ClearedAt = DateTimeOffset.UtcNow
            },
            severity: SideEffectSeverity.Info);

        return Success(TransactionId, StateChange.Modified("transaction", state, TransactionId), [sideEffect]);
    }
}