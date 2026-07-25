using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Adjustment;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for recording manual or system adjustment to ledger account.
/// </summary>
/// <remarks>
/// Creates correction transaction representing an adjustment to ledger account.
/// The adjustment may represent either debit or credit operation depending on
/// the specified <see cref="LedgerAdjustmentContext.AdjustmentType"/> and may optionally
/// target an encumbrance.
/// </remarks>
/// <param name="ctx">The context containing the adjustment operation input and metadata.</param>
/// <param name="mutationContext">
/// The mutation execution context that defines the identity, execution mode,
/// and other metadata associated with the mutation.
/// </param>
public sealed class AdjustmentMutation(LedgerAdjustmentContext ctx, MutationContext mutationContext ) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.adjustment",
        category: "domain.ledger",
        description: "Record manual or system adjustment to ledger account",
        riskLevel: MutationRiskLevel.Critical,
        isReversible: false,
        tags: new HashSet<string>
        {
            "ledger",
            "adjustment",
            "correction"
        }),
        mutationContext)
{
    public LedgerAdjustmentContext Input { get; } = ctx;

    /// <summary>
    /// Applies the adjustment mutation to the current ledger state.
    /// </summary>
    /// <remarks>
    /// Builds new <see cref="LedgerTransaction"/> using the configured adjustment
    /// parameters, including the target account, adjustment type, amount, currency,
    /// and encumbrance state.
    ///
    /// The transaction uses the input identifier as its idempotency key and records
    /// the supplied reason, or generates a default reason when none is provided.
    /// </remarks>
    /// <param name="state">The current ledger transaction state to which the mutation is applied.</param>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var reason = Input.Reason ?? $"Adjustment: {Input.AdjustmentType}";

        var transaction = LedgerOperationBuilder.Start(TransactionType.Correction, Input.InitiatedBy)
            .WithIdempotencyKey(Input.Id)
            .WithReason(reason)
            .Adjustment(
                Input.AccountId,
                Input.AdjustmentType,
                Input.Amount,
                Enum.Parse<Currency>(Input.Currency, true),
                Input.IsEncumbrance,
                reason)
            .Build();

        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the adjustment mutation input before the adjustment is applied.
    /// </summary>
    /// <remarks>
    /// Validates that the adjustment amount is positive, a target account has been
    /// provided, and an adjustment type has been specified.
    ///
    /// Validation errors are returned using stable ledger adjustment error codes
    /// so that callers can distinguish between invalid amount, account, and
    /// adjustment type failures.
    /// </remarks>
    /// <param name="state">The current ledger transaction state used as the validation context. </param>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        if (Input.Amount <= 0)
            result.AddError(
                "ledger.adjustment",
                "Amount must be positive",
                "INVALID_AMOUNT");

        if (string.IsNullOrWhiteSpace(Input.AccountId))
            result.AddError(
                "ledger.adjustment",
                "Account is required",
                "INVALID_ACCOUNT");

        if (string.IsNullOrWhiteSpace(Input.AdjustmentType))
            result.AddError(
                "ledger.adjustment",
                "Adjustment type is required",
                "INVALID_ADJUSTMENT_TYPE");

        return result;
    }
}