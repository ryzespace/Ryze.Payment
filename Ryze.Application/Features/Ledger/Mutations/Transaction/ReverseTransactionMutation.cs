using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Transfer;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Exceptions;

namespace Ryze.Application.Features.Ledger.Mutations.Transaction;

/// <summary>
/// Mutation responsible for reversing an existing ledger transaction by posting
/// an opposite transaction instead of modifying or deleting historical entries.
/// </summary>
/// <remarks>
/// Creates new reversal transaction that negates the debit and credit postings
/// of the original transaction while preserving the original ledger history.
/// </remarks>
/// <param name="ctx">The reversal context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
public sealed class ReverseTransactionMutation(ReverseTransactionContext ctx, MutationContext mutationContext) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.reverse",
        category: "domain.ledger",
        description: "Reverse an existing ledger transaction by posting an inverse entry",
        riskLevel: MutationRiskLevel.High,
        isReversible: false,
        tags: new HashSet<string>
        {
            "ledger",
            "transaction",
            "reversal"
        }),
    mutationContext)
{
    /// <summary>
    /// Applies the reversal mutation and creates an inverse ledger transaction.
    /// </summary>
    /// <param name="state">
    /// The current ledger transaction state. Must be <see langword="null"/> because
    /// reversal creates new transaction instead of modifying the original.
    /// </param>
    /// <returns>successful mutation result containing the newly created reversal transaction. </returns>
    /// <exception cref="LedgerValidationException"> Thrown when an existing transaction state is provided. </exception>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        if (state != null)
        {
            throw new LedgerValidationException(
                "Reversal creates a new state block, cannot mutate existing state.");
        }

        var original = ctx.OriginalTransaction;

        var builder = LedgerTransactionBuilder
            .Create(TransactionType.Refund, ctx.InitiatedBy)
            .WithIdempotencyKey(ctx.ReversalIdempotencyKey)
            .WithReason(ctx.Reason);

        foreach (var entry in original.Entries)
        {
            var metadata = new Dictionary<string, string>(
                entry.Metadata ?? new Dictionary<string, string>())
            {
                ["reversal_for"] = original.Id.ToString()
            };

            if (entry.Posting.Type == EntryType.Debit)
            {
                builder.AddCredit(
                    entry.Account.AccountId,
                    entry.Posting.Amount,
                    entry.Posting.Currency,
                    $"Reversal of {entry.Id}",
                    entry.Account.AccountType,
                    metadata);
            }
            else
            {
                builder.AddDebit(
                    entry.Account.AccountId,
                    entry.Posting.Amount,
                    entry.Posting.Currency,
                    $"Reversal of {entry.Id}",
                    entry.Account.AccountType,
                    metadata);
            }
        }

        var reversalTx = builder.Build();
        return Success(reversalTx, StateChange.Added("reversal_transaction", reversalTx));
    }

    /// <summary>
    /// Validates the reversal mutation before execution.
    /// </summary>
    /// <returns>
    /// Validation result containing errors when the original transaction
    /// cannot be reversed.
    /// </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (ctx.OriginalTransaction.Entries.Count == 0)
        {
            result.AddError(
                "original_transaction",
                "Missing entries in original transaction to reverse.");
        }

        return result;
    }
}