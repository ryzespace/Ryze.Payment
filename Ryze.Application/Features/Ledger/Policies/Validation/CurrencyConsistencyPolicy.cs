using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;
using ChartOfAccounts = Ryze.Domain.Features.Ledger.Entity.ChartsAccounts.ChartOfAccounts;

namespace Ryze.Application.Features.Ledger.Policies.Validation;

/// <summary>
/// Ensures currency consistency within recorded ledger transactions.
/// </summary>
/// <remarks>
/// Allows transactions containing single currency without additional restrictions.
/// Multi currency transactions must include the designated currency exchange account
/// to explicitly represent the foreign exchange component of the transaction.
/// </remarks>
public sealed class CurrencyConsistencyPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.validation.currency";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 130;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Validates currency consistency in simple ledger transactions";

    /// <summary>
    /// Evaluates the specified mutation against currency consistency rules.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies a multi-currency transaction when it does not
    /// contain the designated currency exchange account; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(
        IMutation<LedgerTransaction> mutation,
        LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record)
        {
            return PolicyDecision.Allow(Name);
        }

        var transaction = record.Transaction;

        var isMultiCurrency = transaction.Entries
            .Select(entry => entry.Posting.Currency)
            .Distinct()
            .Skip(1)
            .Any();

        if (!isMultiCurrency)
            return PolicyDecision.Allow(Name);

        var hasFxAccount = transaction.Entries.Any(entry =>
            entry.Account.AccountId ==
            ChartOfAccounts.CurrencyExchangeGainLoss.Id);

        return hasFxAccount
            ? PolicyDecision.Allow(
                Name,
                reason: "Multi currency transaction includes the Currency Exchange account.")
            : PolicyDecision.Deny(
                "Multi currency transaction must involve the Currency Exchange account.",
                Name);
    }
}
