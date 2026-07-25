using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Mutations.SideEffects;
using Ryze.Domain.Features.Ledger.Entity;

namespace Ryze.Application.Features.Ledger.Mutations.Transaction;

/// <summary>
/// Mutation responsible for recording new ledger transaction.
/// </summary>
/// <remarks>
/// Validates the provided ledger transaction, creates corresponding side effect,
/// and returns the transaction as newly added state.
/// </remarks>
/// <param name="transaction">The transaction to record.</param>
/// <param name="context">The mutation execution context.</param>
public sealed class RecordTransactionMutation(
    LedgerTransaction transaction,
    MutationContext context)
    : MutationBase<LedgerTransaction>(
        CreateIntent(
            operationName: "ledger.record",
            category: "domain.ledger",
            description: "Record a double entry ledger transaction",
            riskLevel: MutationRiskLevel.High,
            isReversible: false,
            tags: new HashSet<string>
            {
                "ledger",
                "transaction",
                "double-entry"
            }),
        context)
{
    /// <summary>
    /// Gets the ledger transaction being recorded by this mutation.
    /// </summary>
    public LedgerTransaction Transaction => transaction;
    
    /// <summary>
    /// Applies the mutation and records the ledger transaction.
    /// </summary>
    /// <param name="state">
    /// The current ledger transaction state. Must be <see langword="null"/> because
    /// the transaction is created as a new state.
    /// </param>
    /// <returns>
    /// Successful mutation result containing the recorded transaction and
    /// the corresponding transaction recorded side effect.
    /// </returns>
    public override MutationResult<LedgerTransaction> Apply(
        LedgerTransaction state)
    {
        if (state is not null)
        {
            throw new InvalidOperationException(
                "Transaction state must be built from scratch, not mutated.");
        }

        var sideEffect = SideEffect.Create(
            type: "LedgerTransactionRecorded",
            description:
                $"Ledger transaction {transaction.Id} was recorded.",
            data: new LedgerTransactionRecordedSideEffectData
            {
                TransactionId = transaction.Id,
                Timestamp = transaction.Timestamp,
                Entries = transaction.Entries,
                TransactionType = transaction.Type.ToString(),
                IdempotencyKey = transaction.IdempotencyKey,
                InitiatedBy = transaction.InitiatedBy,
                Reason = transaction.Reason,
                EntryNumber = transaction.Entries
                    .Select(entry => entry.EntryNumber)
                    .FirstOrDefault()
            },
            severity: SideEffectSeverity.Info);

        return Success(
            transaction,
            StateChange.Added("transaction", transaction),
            [
                sideEffect
            ]);
    }

    /// <summary>
    /// Validates the ledger transaction before it is recorded.
    /// </summary>
    /// <returns>Validation result containing any errors raised during transaction validation. </returns>
    /// <param name="state">The current ledger transaction state. </param>
    public override ValidationResult Validate(
        LedgerTransaction state)
    {
        var result = new ValidationResult();

        try
        {
            transaction.Validate();
        }
        catch (Exception ex)
        {
            result.AddError(
                "ledger",
                ex.Message,
                "VALIDATION_FAILED");
        }

        return result;
    }
}