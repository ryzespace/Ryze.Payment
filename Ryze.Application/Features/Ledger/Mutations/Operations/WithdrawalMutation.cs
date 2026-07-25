using Ryze.Application.Features.Ledger.Contexts;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Withdrawal;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for withdrawing funds from ledger account to an external destination.
/// </summary>
/// <remarks>
/// Creates withdrawal transaction that moves funds from the specified ledger
/// account to an external destination.
/// </remarks>
/// <param name="ctx">The withdrawal context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
public sealed class WithdrawalMutation(LedgerWithdrawalContext ctx, MutationContext mutationContext) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.withdrawal",
        category: "domain.ledger",
        description: "Withdraw funds from ledger account to external destination",
        riskLevel: MutationRiskLevel.High,
        isReversible: true, tags: new HashSet<string>
        {
            "ledger", "withdrawal"
        }),
        mutationContext)
{
    private LedgerWithdrawalContext Input { get; } = ctx;
    
    /// <summary>
    /// Applies the withdrawal mutation and creates the resulting ledger transaction.
    /// </summary>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A successful mutation result containing the newly created withdrawal transaction.
    /// </returns>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var transaction = LedgerOperationBuilder.Start(TransactionType.Withdrawal, Input.InitiatedBy)
            .WithIdempotencyKey(Input.Id)
            .WithReason(Input.Reason ?? "Withdrawal")
            .Withdrawal(Input.AccountId, Input.Destination, Input.Amount, Enum.Parse<Currency>(Input.Currency, true), Input.Description)
            .Build();

        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the withdrawal mutation before execution.
    /// </summary>
    /// <returns>Validation result containing errors when the withdrawal input is invalid. </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (Input.Amount <= 0)
        {
            result.AddError(
                "ledger.withdrawal",
                "Amount must be positive",
                "INVALID_AMOUNT");
        }

        return result;
    }
}
