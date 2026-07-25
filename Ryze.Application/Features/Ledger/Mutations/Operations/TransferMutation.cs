using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Transfer;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for an internal funds transfer between two ledger accounts.
/// </summary>
/// <remarks>
/// Creates transfer transaction that moves funds from the specified source
/// account to the destination account.
/// </remarks>
/// <param name="ctx">The transfer context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
public sealed class TransferMutation(LedgerInternalTransferContext ctx, MutationContext mutationContext) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.transfer",
        category: "domain.ledger",
        description: "Transfer funds between two ledger accounts",
        riskLevel: MutationRiskLevel.High,
        isReversible: true,
        tags: new HashSet<string>
        {
            "ledger",
            "transfer"
        }),
        mutationContext)
{
    public LedgerInternalTransferContext Input { get; } = ctx;

    /// <summary>
    /// Applies the transfer mutation and creates the resulting ledger transaction.
    /// </summary>
    /// <returns>Successful mutation result containing the newly created transfer transaction.</returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var transaction = LedgerOperationBuilder
            .Start(TransactionType.Transfer, Input.InitiatedBy)
            .WithIdempotencyKey(Input.Id)
            .WithReason(Input.Reason ?? "Internal Transfer")
            .Transfer(
                Input.FromAccountId,
                Input.ToAccountId,
                Input.Amount,
                Enum.Parse<Currency>(Input.Currency, true),
                Input.Description)
            .Build();

        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the transfer mutation before execution.
    /// </summary>
    /// <returns>Validation result containing errors when the transfer input is invalid. </returns>
    /// <param name="state">The current ledger transaction state.</param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (Input.Amount <= 0)
        {
            result.AddError(
                "ledger.transfer",
                "Amount must be positive",
                "INVALID_AMOUNT");
        }

        if (Input.FromAccountId == Input.ToAccountId)
        {
            result.AddError(
                "ledger.transfer",
                "Source and destination accounts must be different",
                "SAME_ACCOUNT");
        }

        return result;
    }
}