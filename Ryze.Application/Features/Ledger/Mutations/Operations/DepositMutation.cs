using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Deposit;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for depositing funds from an external source into ledger account.
/// </summary>
/// <remarks>
/// Creates deposit transaction that records funds received from an external
/// funding source and credits the specified ledger account.
/// </remarks>
/// <param name="ctx">The deposit context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
public sealed class DepositMutation(LedgerDepositContext ctx, MutationContext mutationContext ) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.deposit",
        category: "domain.ledger",
        description: "Deposit funds from external gateway to ledger account",
        riskLevel: MutationRiskLevel.High,
        isReversible: true,
        tags: new HashSet<string>
        {
            "ledger", "deposit"
        }),
        mutationContext)
{
    public LedgerDepositContext Input { get; } = ctx;

    /// <summary>
    /// Applies the deposit mutation and creates the resulting ledger transaction.
    /// </summary>
    /// <returns>Successful mutation result containing the newly created deposit transaction.</returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var transaction = LedgerOperationBuilder
            .Start(TransactionType.Deposit, Input.InitiatedBy)
            .WithIdempotencyKey(Input.Id)
            .WithReason(Input.Reason ?? "Deposit")
            .Deposit(
                Input.AccountId,
                Input.FundingSource,
                Input.Amount,
                Enum.Parse<Currency>(Input.Currency, true),
                Input.Description)
            .Build();

        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the deposit mutation before execution.
    /// </summary>
    /// <returns>Validation result containing errors when the deposit input is invalid. </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (Input.Amount <= 0)
        {
            result.AddError(
                "ledger.deposit",
                "Amount must be positive",
                "INVALID_AMOUNT");
        }

        return result;
    }
}