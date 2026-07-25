using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Fee;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for recording platform fee against ledger account.
/// </summary>
/// <remarks>
/// Creates fee transaction that records platform fee charged against
/// the specified source account.
/// </remarks>
/// <param name="ctx">The fee context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
public sealed class FeeMutation(LedgerFeeContext ctx, MutationContext mutationContext) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.fee",
        category: "domain.ledger",
        description: "Record a platform fee against a source account",
        riskLevel: MutationRiskLevel.High,
        isReversible: true,
        tags: new HashSet<string>
        {
            "ledger",
            "fee"
        }),
        mutationContext)
{
    public LedgerFeeContext Input { get; } = ctx;

    /// <summary>
    /// Applies the fee mutation and creates the resulting ledger transaction.
    /// </summary>
    /// <returns>Successful mutation result containing the newly created fee transaction. </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var transaction = LedgerOperationBuilder
            .Start(TransactionType.Fee, Input.InitiatedBy)
            .WithIdempotencyKey(Input.Id)
            .WithReason(Input.Reason ?? $"Fee: {Input.FeeType}")
            .Fee(
                Input.FromAccountId,
                Input.FeeType,
                Input.Amount,
                Enum.Parse<Currency>(Input.Currency, true),
                Input.Description)
            .Build();

        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the fee mutation before execution.
    /// </summary>
    /// <returns>Validation result containing errors when the fee input is invalid. </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (Input.Amount <= 0)
        {
            result.AddError(
                "ledger.fee",
                "Amount must be positive",
                "INVALID_AMOUNT");
        }

        if (string.IsNullOrWhiteSpace(Input.FromAccountId))
        {
            result.AddError(
                "ledger.fee",
                "Source account is required",
                "INVALID_ACCOUNT");
        }

        if (string.IsNullOrWhiteSpace(Input.FeeType))
        {
            result.AddError(
                "ledger.fee",
                "Fee type is required",
                "INVALID_FEE_TYPE");
        }

        return result;
    }
}