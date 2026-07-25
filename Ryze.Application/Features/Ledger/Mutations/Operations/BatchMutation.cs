using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using Ryze.Application.Features.Ledger.Builders;
using Ryze.Application.Features.Ledger.Contexts.Batch;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Shared.Enum;
using ChartOfAccounts = Ryze.Domain.Features.Ledger.Entity.ChartsAccounts.ChartOfAccounts;

namespace Ryze.Application.Features.Ledger.Mutations.Operations;

/// <summary>
/// Mutation responsible for executing batch of ledger operations as single transaction.
/// </summary>
/// <param name="ctx">The batch ledger context.</param>
/// <param name="mutationContext">The mutation execution context.</param>
/// <remarks>
/// Creates correction transaction containing the debit and credit operations
/// defined by the batch context and offsets currency differences against the
/// platform treasury account.
/// </remarks>
public sealed class BatchMutation(BatchLedgerContext ctx, MutationContext mutationContext) : MutationBase<LedgerTransaction>(
    CreateIntent(
        operationName: "ledger.batch",
        category: "domain.ledger",
        description: "Execute a batch of ledger operations",
        riskLevel: MutationRiskLevel.Critical,
        isReversible: false,
        tags: new HashSet<string>
        {
            "ledger",
            "batch"
        }),
        mutationContext)
{
    private BatchLedgerContext Input { get; } = ctx;

    /// <summary>
    /// Applies the batch mutation and creates the resulting ledger transaction.
    /// </summary>
    /// <remarks>
    /// Adds all batch debit and credit operations to the transaction and offsets
    /// any currency imbalance against the platform treasury account.
    /// </remarks>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>Successful mutation result containing the newly created ledger transaction. </returns>
    public override MutationResult<LedgerTransaction> Apply(LedgerTransaction state)
    {
        var idempotencyKey = Input.IdempotencyKeyOverride ?? $"batch:{Input.Id}";
        var description = Input.Description ?? $"Batch of {Input.Items.Count} operations";

        var builder = LedgerTransactionBuilder.Create(
                TransactionType.Correction,
                Input.InitiatedBy)
            .WithIdempotencyKey(idempotencyKey)
            .WithReason(description);

        var treasuryNet = Input.Items
            .GroupBy(i => i.Currency)
            .ToDictionary(g => g.Key, g =>
            {
                var debits = g.Where(i => i.Direction == "Debit").Sum(i => i.Amount);
                var credits = g.Where(i => i.Direction == "Credit").Sum(i => i.Amount);

                return credits - debits;
            });

        foreach (var item in Input.Items)
        {
            var ccyEnum = Enum.Parse<Currency>(item.Currency, true);

            switch (item.Direction)
            {
                case "Debit":
                    builder.AddDebit(
                        ChartOfAccounts.UserWallet(item.WalletId, item.Currency),
                        item.Amount,
                        ccyEnum,
                        $"Batch debit: {description}");
                    break;

                case "Credit":
                    builder.AddCredit(
                        ChartOfAccounts.UserWallet(item.WalletId, item.Currency),
                        item.Amount,
                        ccyEnum,
                        $"Batch credit: {description}");
                    break;
            }
        }

        foreach (var (ccy, net) in treasuryNet)
        {
            var ccyEnum = Enum.Parse<Currency>(ccy, true);

            switch (net)
            {
                case > 0.001m:
                    builder.AddDebit(
                        ChartOfAccounts.PlatformTreasury,
                        net,
                        ccyEnum,
                        $"Batch offset: total credit > total debit by {net} {ccy}");
                    break;

                case < -0.001m:
                    builder.AddCredit(
                        ChartOfAccounts.PlatformTreasury,
                        Math.Abs(net),
                        ccyEnum,
                        $"Batch offset: total debit > total credit by {Math.Abs(net)} {ccy}");
                    break;
            }
        }

        var transaction = builder.Build();
        return Success(transaction, StateChange.Added("transaction", transaction));
    }

    /// <summary>
    /// Validates the batch mutation before execution.
    /// </summary>
    /// <remarks>
    /// Verifies that debit and credit totals for each currency are balanced
    /// within the allowed tolerance.
    /// </remarks>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>Validation result containing errors when any currency group is unbalanced. </returns>
    public override ValidationResult Validate(LedgerTransaction state)
    {
        var result = ValidationResult.Success();

        var grouped = Input.Items
            .GroupBy(i => i.Currency)
            .ToList();

        foreach (var group in grouped)
        {
            var debitTotal = group
                .Where(i => i.Direction == "Debit")
                .Sum(i => i.Amount);

            var creditTotal = group
                .Where(i => i.Direction == "Credit")
                .Sum(i => i.Amount);

            if (Math.Abs(debitTotal - creditTotal) > 0.001m)
            {
                result.AddError(
                    "ledger.batch",
                    $"Batch currency {group.Key} is unbalanced",
                    "UNBALANCED_BATCH");
            }
        }

        return result;
    }
}